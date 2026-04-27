using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite;
using StackExchange.Redis;
using System.Reflection;
using Domain.Interfaces;
using Infrastructure.Services;
using Infrastructure.Caching;
using Infrastructure.Events;

var builder = WebApplication.CreateBuilder(args);

// 1. Cargar Variables de Entorno desde .env si existe
var envPath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
if (File.Exists(envPath))
{
    var lines = File.ReadAllLines(envPath);
    foreach (var line in lines)
    {
        if (!string.IsNullOrWhiteSpace(line) && !line.StartsWith("#") && line.Contains("="))
        {
            var parts = line.Split('=', 2);
            if (parts.Length == 2)
            {
                Environment.SetEnvironmentVariable(parts[0].Trim(), parts[1].Trim());
            }
        }
    }
}

// 2. Leer Configuración
var dbHost = Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost";
var dbPort = Environment.GetEnvironmentVariable("DB_PORT") ?? "5432";
var dbName = Environment.GetEnvironmentVariable("DB_NAME") ?? "geo_db";
var dbUser = Environment.GetEnvironmentVariable("DB_USER") ?? "postgres";
var dbPass = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "postgres";

var redisHost = Environment.GetEnvironmentVariable("REDIS_HOST") ?? "localhost";
var redisPort = Environment.GetEnvironmentVariable("REDIS_PORT") ?? "6379";

var connectionString = $"Host={dbHost};Port={dbPort};Database={dbName};Username={dbUser};Password={dbPass};Include Error Detail=true";

// 3. Registrar Servicios
// Redis
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect($"{redisHost}:{redisPort},abortConnect=false"));

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = $"{redisHost}:{redisPort}";
    options.InstanceName = "GeoCache:";
});

// DbContext con PostGIS
builder.Services.AddDbContext<GeoDbContext>(options =>
{
    options.UseNpgsql(connectionString, o => 
    {
        o.UseNetTopologySuite(); // Habilita tipos geoespaciales
        o.MigrationsAssembly(typeof(GeoDbContext).Assembly.FullName);
    });
    #if DEBUG
    options.EnableSensitiveDataLogging();
    options.EnableDetailedErrors();
    #endif
});

// Inyección de Dependencias (Clean Architecture)
builder.Services.AddScoped<ICurrentContextService, CurrentContextService>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Infrastructure.Services.Repository<>));
builder.Services.AddScoped<IEventBus, RedisEventBus>();
builder.Services.AddScoped<ICacheService, RedisCacheService>();

// Repositorios especializados y UnitOfWork
builder.Services.AddScoped<IMarkerRepository, MarkerRepository>();
builder.Services.AddScoped<IPolygonRepository, PolygonRepository>();
builder.Services.AddScoped<IRouteRepository, RouteRepository>();
builder.Services.AddScoped<ITargetRepository, TargetRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Servicios de Dominio
builder.Services.AddScoped<IMarkerService, MarkerService>();
builder.Services.AddScoped<IPolygonService, PolygonService>();
builder.Services.AddScoped<IRouteService, RouteService>();
builder.Services.AddScoped<ITargetService, TargetService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Geo Spatial API .NET 9", Version = "v1" });
});

var app = builder.Build();

// 4. Migraciones Automáticas al Inicio
Console.WriteLine("=== Iniciando Migraciones de Base de Datos ===");
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<GeoDbContext>();
    try
    {
        context.Database.Migrate();
        Console.WriteLine("Migraciones aplicadas correctamente.");
        
        // Asegurar extensión PostGIS
        context.Database.ExecuteSqlRaw("CREATE EXTENSION IF NOT EXISTS postgis;");
        Console.WriteLine("Extensión PostGIS verificada.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"ERROR CRÍTICO al migrar: {ex.Message}");
        Console.WriteLine($"Detalle: {ex.InnerException?.Message}");
        throw; 
    }
}

// 5. Middleware Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Geo API v1"));
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Configurar Puerto
var port = Environment.GetEnvironmentVariable("API_PORT") ?? "5000";
app.Urls.Add($"http://0.0.0.0:{port}");

Console.WriteLine($"========================================");
Console.WriteLine($"API LISTA EN: http://localhost:{port}/swagger");
Console.WriteLine($"Org ID Actual: {Environment.GetEnvironmentVariable("CURRENT_ORG_ID")}");
Console.WriteLine($"Usuario Actual: {Environment.GetEnvironmentVariable("CURRENT_USER_ID")}");
Console.WriteLine($"========================================");

app.Run();
