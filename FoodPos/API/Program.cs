using API.Dtos.Stripe;
using API.Extensions;
using API.Helpers.Errors;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Microsoft.OpenApi.Models; // Para OpenApiSecurityScheme, etc.
using System.Reflection; // Necesario para la documentación XML

var builder = WebApplication.CreateBuilder(args);

//AutoMapper
builder.Services.AddAutoMapper(Assembly.GetEntryAssembly());

//Stripe config
builder.Services.Configure<StripeOptions>(builder.Configuration.GetSection("StripeOptions"));
// Get Stripe SecretKey to configure Stripe globally
var stripeOptions = builder.Configuration.GetSection("StripeOptions").Get<StripeOptions>();
StripeConfiguration.ApiKey = stripeOptions.SecretKey;

//StripeConfiguration.ApiKey = stripeSettings.SecretKey;

//builder.Services.ConfigureRateLimiting();

// Add services to the container.
builder.Services.ConfigureCors(builder.Configuration);
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

builder.Services.AddSwaggerGen(options =>
{
    // 1. Configuración de seguridad JWT (ya la tenías, ahora con el método de Swashbuckle)
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please insert JWT with Bearer into field",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

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

//app.UseIpRateLimiting();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Tu API v1");
    });
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
