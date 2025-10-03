# Food POS API

A robust and modular RESTful API for a food business POS built with **ASP.NET Core**, implementing essential design patterns (such as the Unit of Work and Services), using **JWT** for authentication and role-based authorization, and handling payments through **Stripe**.  
The API follows a **clean architecture** for managing products, roles, users, and the full order lifecycle.

---

## 🚀 Main Features

- **Authentication & Authorization**: JWT Bearer with Refresh Tokens and role-based access (Admin, Client).
- **Design Patterns**: Implementation of Unit of Work and Generic Repository for database abstraction.
- **Database**: Configured with MySQL and Entity Framework Core.
- **Payment Gateway**: Full integration with Stripe for creating and managing Payment Intents.
- **Error Handling**: Consistent and centralized HTTP responses (400, 401, 404, 409, 500) using the extension `ServiceResult.ToActionResult()`.
- **Documentation**: Interactive Swagger/OpenAPI interface for testing and reference.

---

## 🛠️ Technologies Used

- **Framework**: .NET 8 / ASP.NET Core  
- **Database**: MySQL  
- **ORM**: Entity Framework Core  
- **Authentication**: JWT, Refresh Tokens, Identity (using `IPasswordHasher`)  
- **Mapping**: AutoMapper  
- **Payments**: Stripe .NET SDK  
- **Documentation**: Swashbuckle (OpenAPI)  

---

## ⚙️ Environment Setup

To run the project locally, you will need:

- .NET 8 SDK (or higher)  
- A MySQL server (local or cloud)  
- A Stripe account (to get test keys)  

### 1. Clone the Repository

```bash
git clone https://https://github.com/SamuelLeiva/food-pos-netcore.git
cd food-pos-netcore
```

### 2. Configure appsettings.json

Create an appsettings.Development.json file or edit appsettings.json with your credentials:

```bash
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=your_db;User=your_user;Password=your_password;"
  },
  "JWT": {
    "Key": "A_LONG_SECRET_KEY_WITH_AT_LEAST_32_CHARACTERS",
    "Issuer": "YourApiIssuer",
    "Audience": "YourApiAudience",
    "DurationInMinutes": 60
  },
  "StripeOptions": {
    "PublishableKey": "pk_test_...",
    "SecretKey": "sk_test_..."
  }
  // ... other Serilog settings
}
```

### 3. Apply Migrations

Make sure your database exists and apply migrations:

```bash
dotnet ef database update
```

Program.cs is already configured to run migrations and seeding on startup, but it’s a good practice to apply them manually.

### 4. Run the API

Run the project from the root of the solution:

```bash
dotnet run
```

The API will be available at the URL specified in launchSettings.json (usually: <https://localhost:7001>).

## 📚 Documentation & Endpoints

Swagger interactive documentation will be available at:

👉 <https://localhost:7001/swagger>

## 💡 Architecture & Class Structure

The API follows a three-layer architecture:

- **API**: Controllers, DTOs, Extensions, and presentation/mapping logic.

- **Core**: Entities (User, Product, Order), Repository Interfaces, Service Interfaces, and the ServiceResult class (with StatusCode support).

- **Infrastructure**: Repository implementations, UnitOfWork, DbContext, and seeding logic.

## ⚠️ ServiceResult Usage (Error Handling)

Instead of throwing exceptions, all service methods return a ServiceResult or ServiceResult`<T>`.

```bash
// Inside a Service:
if (product == null)
{
    // Returns the error message along with the 404 status code
    return ServiceResult<ProductDto>.Failure("Product not found.", 404);
}

// Inside the Controller:
// The ToActionResult() method reads the StatusCode (404) and generates the HTTP response
return result.ToActionResult();
```
