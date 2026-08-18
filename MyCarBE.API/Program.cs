using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MyCarBE.API.Middleware;
using MyCarBE.API.Serialization;
using MyCarBE.API.Services;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Extensions;
using MyCarBE.Data.Context;
using MyCarBE.Data.Extensions;
using MyCarBE.Data.Services;
using Microsoft.OpenApi.Models;
using QuestPDF.Infrastructure;
using Serilog;

// Bootstrap logger — captura errores antes de que el DI container esté listo
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

// Npgsql: treat DateTime as UTC for timestamp with time zone columns
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// QuestPDF Community license (free for open-source / small business)
QuestPDF.Settings.License = LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

// Serilog — reemplaza el logging de .NET con configuración desde appsettings
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "MyCarApp"));

// Data Layer (PostgreSQL + Identity + FluentValidation + Repositories + Auth services)
builder.Services.AddDataLayer(builder.Configuration);

// Application Layer (MediatR + ValidationBehaviour + FluentValidation + Mapster)
builder.Services.AddApplicationLayer();

// Current User (lee claims del JWT en el HttpContext)
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// File storage — Storage:Provider decide la implementación:
//   "R2"    → Cloudflare R2 (producción; el disco de Railway es efímero)
//   "Local" → wwwroot/uploads (default, desarrollo)
if (string.Equals(builder.Configuration["Storage:Provider"], "R2", StringComparison.OrdinalIgnoreCase))
    builder.Services.AddScoped<IFileStorageService, R2FileStorageService>();
else
    builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();

// Email — swap this for SendGrid / SES at deploy time
builder.Services.AddScoped<IEmailService, SmtpEmailService>();

// PDF generation — swap for another provider if needed
builder.Services.AddScoped<IPdfService, QuotePdfService>();
// Informe de cierre: documento distinto del presupuesto, con sus propias reglas.
builder.Services.AddScoped<IOrderClosingPdfService, OrderClosingPdfService>();

// Approval link builder — change ApprovalBaseUrl in appsettings for production/frontend
builder.Services.AddScoped<IApprovalLinkBuilder, ApprovalLinkBuilder>();

// Background: cancela órdenes con presupuesto vencido (corre cada hora).
builder.Services.AddHostedService<QuoteExpirationCleanupService>();

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey   = jwtSettings["SecretKey"]!;

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    // Deshabilitamos el remapeo de claims — los claims conservan sus nombres JWT estándar.
    // Sin esto, "sub" → NameIdentifier y "email" → EmailAddress (nombres largos WS-Federation).
    options.MapInboundClaims = false;

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer           = true,
        ValidateAudience         = true,
        ValidateLifetime         = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer              = jwtSettings["Issuer"],
        ValidAudience            = jwtSettings["Audience"],
        IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew                = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// Rate limiting — anti fuerza-bruta sobre el login (P0, ver SPEC §Seguridad).
// Política "login": ventana fija de 1 min, máx 10 intentos por IP. Combinada con
// el lockout de Identity (5 fallos → cuenta bloqueada 15 min) frena el brute-force.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("login", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window      = TimeSpan.FromMinutes(1),
                QueueLimit  = 0
            }));
});

// CORS — permite requests desde el frontend Next.js.
// Orígenes por env var CORS_ORIGINS (Railway/prod, CSV) o Cors:AllowedOrigins en
// appsettings; fallback a localhost para dev. Mismo patrón que GestionPGB.
var allowedOrigins =
    (Environment.GetEnvironmentVariable("CORS_ORIGINS")
    ?? builder.Configuration["Cors:AllowedOrigins"]
    ?? "http://localhost:3000")
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Global exception handler → ProblemDetails (RFC 7807)
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Controllers + Swagger con soporte JWT
// Las fechas salen SIEMPRE como UTC explicito (con la Z). Sin esto, las columnas
// "timestamp without time zone" viajaban sin marca de zona y el navegador las tomaba
// como hora local: toda la app mostraba 3 horas de mas en Argentina.
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new UtcDateTimeConverter());
    options.JsonSerializerOptions.Converters.Add(new UtcNullableDateTimeConverter());
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title       = "MyCarBE API",
        Version     = "v1",
        Description = "Backend API for MyCarApp"
    });

    // Botón Authorize en Swagger para enviar el JWT
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name         = "Authorization",
        Type         = SecuritySchemeType.Http,
        Scheme       = "Bearer",
        BearerFormat = "JWT",
        In           = ParameterLocation.Header,
        Description  = "Ingresá el token JWT. Ejemplo: Bearer {token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Middleware pipeline (el orden importa)
app.UseExceptionHandler();
app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} → {StatusCode} ({Elapsed:0.0}ms)";
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "MyCarBE API v1");
        options.RoutePrefix = string.Empty;
    });
}

// HTTPS redirect solo en dev: en Railway el proxy termina TLS y nos pasa HTTP —
// redirigir ahí genera loops/warnings. En prod el TLS lo garantiza la plataforma.
if (app.Environment.IsDevelopment())
    app.UseHttpsRedirection();

app.UseStaticFiles();     // sirve wwwroot/uploads/...
app.UseCors("FrontendPolicy");  // ← antes de Auth
app.UseRateLimiter();           // ← anti fuerza-bruta (política "login")
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Auto-migración en Development, o en producción si Database:MigrateOnStartup=true
// (env var Database__MigrateOnStartup en Railway — sin consola interactiva, migrar
// al arrancar es el paso de deploy):
// - Si la DB no existe, EF la crea.
// - Si hay migraciones pendientes, las aplica.
// - Si está al día, no hace nada (idempotente).
var migrateOnStartup = app.Configuration.GetValue<bool>("Database:MigrateOnStartup");
if (app.Environment.IsDevelopment() || migrateOnStartup)
{
    using var scope = app.Services.CreateScope();
    var dbContext   = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger      = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        var pending = await dbContext.Database.GetPendingMigrationsAsync();
        var pendingList = pending.ToList();
        if (pendingList.Count > 0)
        {
            logger.LogInformation("Aplicando {Count} migración(es) pendiente(s)...", pendingList.Count);
            await dbContext.Database.MigrateAsync();
            logger.LogInformation("Migraciones aplicadas correctamente.");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Fallo aplicando migraciones automáticas.");
        throw; // No queremos seguir si la DB no está bien.
    }
}

// Seed del Admin al arrancar (solo si no existe)
await DatabaseSeeder.SeedAdminUserAsync(app.Services);

app.Run();
