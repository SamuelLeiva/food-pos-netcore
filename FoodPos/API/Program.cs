using API;
using API.Extensions;
using AspNetCoreRateLimit;
using AutoMapper;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

//AutoMapper
builder.Services.AddAutoMapper(Assembly.GetEntryAssembly());

builder.Services.ConfigureRateLimiting();

// Add services to the container.
builder.Services.ConfigureCors();
builder.Services.AddApplicationServices();
builder.Services.ConfigureApiVersioning();

//permitimos soporte del formato xml ("Accept": "application/xml")
builder.Services.AddControllers(options => {
    options.RespectBrowserAcceptHeader = true;
    // envía un error en el caso en el que el cliente solicite un formato no permitido
    options.ReturnHttpNotAcceptable = true; 
    }).AddXmlSerializerFormatters();

builder.Services.AddDbContext<PosContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

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
    }
    catch (Exception ex)
    {
        var logger = loggerFactory.CreateLogger<Program>();
        logger.LogError(ex, "An error ocurred during the migration.");
    }
}

app.UseCors("CorsPolicy");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
