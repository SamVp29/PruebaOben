using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using PruebaOben.Api.Configuration;
using PruebaOben.Application.Interfaces;
using PruebaOben.Application.Services;
using PruebaOben.Domain.Interfaces;
using PruebaOben.Infrastructure.Data;
using PruebaOben.Infrastructure.Repositories;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException(
            "No se encontró la cadena de conexión 'DefaultConnection'. ");
Console.WriteLine($"CONNECTION STRING: [{connectionString}]");

var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException(
        "No está configurada la variable de entorno Jwt__Key."
    );

builder.Services.AddSingleton<IJwtSettings>(
    new JwtSettings
    {
        Key = jwtKey
    }
);

builder.Services.AddSingleton(new SqlConnectionFactory(connectionString));
// Registramos la implementación concreta del repositorio.
// La aplicación trabajará con IUserRepository,
// sin depender directamente de UserRepository.
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// ============================================================
// AUTENTICACIÓN JWT
// ============================================================

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            // Validar que el token haya sido firmado con nuestra clave.
            ValidateIssuerSigningKey = true,

            // Clave utilizada para validar la firma del JWT.
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey)
            ),

            // En este proyecto no estamos utilizando Issuer.
            ValidateIssuer = false,

            // En este proyecto no estamos utilizando Audience.
            ValidateAudience = false,

            // Validar que el token no esté expirado.
            ValidateLifetime = true,

            // Evita aceptar tokens con una diferencia de tiempo
            // adicional entre el servidor y el token.
            ClockSkew = TimeSpan.Zero
        };
    });

// ============================================================
// AUTORIZACIÓN
// ============================================================

builder.Services.AddAuthorization();


builder.Services.AddControllers();
// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Ingresa el JWT obtenido del endpoint /api/auth/login."
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("bearer", document)] = []
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseSwagger();
app.UseSwaggerUI();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// Authentication debe ejecutarse antes que Authorization.
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
