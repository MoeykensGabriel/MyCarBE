using MyCarBE.API.Middleware;
using MyCarBE.Application.Extensions;
using MyCarBE.Data.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Data Layer (PostgreSQL + Identity + FluentValidation)
builder.Services.AddDataLayer(builder.Configuration);

// Application Layer (MediatR + Validators + Mapster)
builder.Services.AddApplicationLayer();

// Global exception handler → ProblemDetails (RFC 7807)
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Controllers + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title       = "MyCarBE API",
        Version     = "v1",
        Description = "Backend API for MyCarApp"
    });
});

var app = builder.Build();

// Middleware pipeline
app.UseExceptionHandler();

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
app.UseAuthorization();
app.MapControllers();

app.Run();
