# Food POS API

Una API RESTful de un POS de un negocio de comida robusta y modular construida con **ASP.NET Core** que implementa patrones de diseño esenciales (como el patrón Unit of Work y Servicios), utiliza **JWT** para autenticación y autorización basada en roles, y maneja pagos a través de **Stripe**.  
La API sigue una **arquitectura limpia** para la gestión de productos, roles, usuarios y el ciclo de vida de las órdenes.

---

## 🚀 Características Principales

- **Autenticación y Autorización**: JWT Bearer con Refresh Tokens y autorización basada en roles (Admin, Client).
- **Patrones de Diseño**: Implementación de Unit of Work y Repositorio Genérico para abstracción de la base de datos.
- **Base de Datos**: Configurada con MySQL y Entity Framework Core.
- **Pasarela de Pago**: Integración completa con Stripe para la creación y gestión de Payment Intents.
- **Manejo de Errores**: Respuestas HTTP uniformes y controladas (400, 401, 404, 409, 500) manejadas centralmente con la extensión `ServiceResult.ToActionResult()`.
- **Documentación**: Interfaz interactiva Swagger/OpenAPI para pruebas y referencia.

---

## 🛠️ Tecnologías Utilizadas

- **Framework**: .NET 8 / ASP.NET Core  
- **Base de Datos**: MySQL  
- **ORM**: Entity Framework Core  
- **Autenticación**: JWT, Refresh Tokens, Identity (usando `IPasswordHasher`)  
- **Mapeo**: AutoMapper  
- **Pagos**: Stripe .NET SDK  
- **Documentación**: Swashbuckle (OpenAPI)  

---

## ⚙️ Configuración del Entorno

Para ejecutar el proyecto localmente, necesitas:

- .NET 8 SDK (o superior).  
- Un servidor MySQL (local o en la nube).  
- Una cuenta de Stripe (para obtener claves de prueba).  

### 1. Clonar el Repositorio

```bash
git clone https://github.com/tu-usuario/tu-repo.git
cd tu-repo
```

### 2. Configurar appsettings.json

Crea un archivo appsettings.Development.json o edita appsettings.json con tus credenciales:

```bash
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=tu_db;User=tu_user;Password=tu_password;"
  },
  "JWT": {
    "Key": "UNA_CLAVE_SECRETA_LARGA_DE_AL_MENOS_32_CARACTERES",
    "Issuer": "TuApiIssuer",
    "Audience": "TuApiAudience",
    "DurationInMinutes": 60
  },
  "StripeOptions": {
    "PublishableKey": "pk_test_...",
    "SecretKey": "sk_test_..."
  }
  // ... otras configuraciones de Serilog
}
```

### 3. Ejecutar Migraciones

Asegúrate de que tu base de datos esté creada y aplica las migraciones:

```bash
dotnet ef database update
```

El Program.cs ya está configurado para ejecutar migraciones y seeding al inicio, pero es buena práctica hacerlo manualmente.

### 4. Ejecutar la API

Ejecuta el proyecto desde la raíz de la solución:

```bash
dotnet run
```

La API estará disponible en la dirección especificada en launchSettings.json(usualmente: <https://localhost:7001>).

## 📚 Documentación y Endpoints

La documentación interactiva de Swagger estará disponible en:

👉 <https://localhost:7001/swagger>

## 💡 Arquitectura y Estructura de Clases

La API sigue una arquitectura de tres capas:

- **API**: Controllers, DTOs, Extensiones y la lógica de presentación/mapeo.

- **Core**: Entidades (User, Product, Order), Interfaces de Repositorios, Interfaces de Servicios, y la clase ServiceResult (con soporte de StatusCode).

- **Infrastructure**: Implementaciones de Repositorios, UnitOfWork, DbContext, y lógica de seeding.

## ⚠️ Uso de ServiceResult (Manejo de Errores)

En lugar de lanzar excepciones, todos los métodos de servicio devuelven un ServiceResult o ServiceReult`<T>`.

```bash
// Dentro de un Service:
if (product == null)
{
    // Retorna el mensaje de error junto con el código 404
    return ServiceResult<ProductDto>.Failure("Product not found.", 404);
}

// Dentro del Controller:
// El método ToActionResult() lee el StatusCode (404) y genera la respuesta HTTP
return result.ToActionResult();
```
