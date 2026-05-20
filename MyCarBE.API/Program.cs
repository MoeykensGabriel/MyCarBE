using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MyCarBE.API.Middleware;
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

// File storage — swap this for Azure Blob / S3 at deploy time
builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();

// Email — swap this for SendGrid / SES at deploy time
builder.Services.AddScoped<IEmailService, SmtpEmailService>();

// PDF generation — swap for another provider if needed
builder.Services.AddScoped<IPdfService, QuotePdfService>();

// Approval link builder — change ApprovalBaseUrl in appsettings for production/frontend
builder.Services.AddScoped<IApprovalLinkBuilder, ApprovalLinkBuilder>();

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

// CORS — permite requests desde el frontend Next.js
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy
            .WithOrigins("http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Global exception handler → ProblemDetails (RFC 7807)
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Controllers + Swagger con soporte JWT
builder.Services.AddControllers();
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

app.UseHttpsRedirection();
app.UseStaticFiles();     // sirve wwwroot/uploads/...
app.UseCors("FrontendPolicy");  // ← antes de Auth
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Auto-migración SOLO en Development:
// - Si la DB no existe, EF la crea.
// - Si hay migraciones pendientes, las aplica.
// - Si está al día, no hace nada (idempotente).
// En producción NO se ejecuta — las migraciones deben aplicarse manualmente
// como paso explícito del deploy para evitar cambios accidentales en la DB.
if (app.Environment.IsDevelopment())
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
