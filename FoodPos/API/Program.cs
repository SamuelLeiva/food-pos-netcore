using API;
using API.Extensions;
using API.Helpers.Errors;
using AspNetCoreRateLimit;
using AutoMapper;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

var logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

//builder.Logging.ClearProviders(); // limpia los mensajes por defecto en desarrollo, para solo usar nuestro logger
builder.Logging.AddSerilog(logger);

//AutoMapper
builder.Services.AddAutoMapper(Assembly.GetEntryAssembly());

builder.Services.ConfigureRateLimiting();

// Add services to the container.
builder.Services.ConfigureCors();
builder.Services.AddApplicationServices();
builder.Services.ConfigureApiVersioning();
builder.Services.AddJwt(builder.Configuration);

//permitimos soporte del formato xml ("Accept": "application/xml")
builder.Services.AddControllers(options =>
{
    options.RespectBrowserAcceptHeader = true;
    // envía un error en el caso en el que el cliente solicite un formato no permitido
    options.ReturnHttpNotAcceptable = true;
}).AddXmlSerializerFormatters();
// solo si serializamos a xml, se hacen los cambios de constructores vacíos, setters, etc

// extension para errores de validación
builder.Services.AddValidationErrors();

builder.Services.AddDbContext<PosContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// middleware para el manejo de excepciones globalmente
app.UseMiddleware<ExceptionMiddleware>();

app.UseStatusCodePagesWithReExecute("/errors/{0}");

app.UseIpRateLimiting();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// En el futuro configurar el seed (consultar curso)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var loggerFactory = services.GetRequiredService<ILoggerFactory>();
    try
    {
        var context = services.GetRequiredService<PosContext>();
        await context.Database.MigrateAsync();
        await PosContextSeed.SeedAsync(context, loggerFactory);
        await PosContextSeed.SeedRolesAsync(context, loggerFactory);
    }
    catch (Exception ex)
    {
        var _logger = loggerFactory.CreateLogger<Program>();
        _logger.LogError(ex, "An error ocurred during the migration.");
    }
}

app.UseCors("CorsPolicy");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
