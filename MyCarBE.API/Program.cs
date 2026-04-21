using MyCarBE.Data.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Data Layer (PostgreSQL + Identity + FluentValidation)
builder.Services.AddDataLayer(builder.Configuration);

// Add services to the container.
builder.Services.AddControllers();

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "MyCarBE API",
        Version = "v1",
        Description = "Backend API for MyCarApp"
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "MyCarBE API v1");
        options.RoutePrefix = string.Empty; // Swagger en la raíz: http://localhost:PORT/
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
