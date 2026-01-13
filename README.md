# Inventory Management System Backend

A **full-featured backend API** for managing inventory (Products & Categories) built with **.NET 10**, **Entity Framework Core**, **PostgreSQL**, and following **Clean Architecture & SOLID principles**. This project includes authentication, role-based authorization, CRUD operations, pagination, filtering, caching, logging, and unit testing.

---

## Table of Contents

- [Features](#features)  
- [Tech Stack](#tech-stack)  
- [Architecture & Folder Structure](#architecture--folder-structure)  
- [Setup & Installation](#setup--installation)  
- [Database Setup](#database-setup)  
- [Running the API](#running-the-api)  
- [Authentication & Authorization](#authentication--authorization)  
- [API Endpoints](#api-endpoints)  
- [Caching & Performance](#caching--performance)  
- [Logging & Error Handling](#logging--error-handling)  
- [Testing](#testing)  
- [Clean Code & SOLID](#clean-code--solid)  
- [Contributing](#contributing)  
- [License](#license)  

---

## Features

✅ **Authentication & Authorization**
- JWT-based authentication  
- Role-based access (Admin/User)  
- Secure password hashing  

✅ **CRUD Operations**
- Create, Read, Update, Delete for **Products** & **Categories**  
- Includes relationships (Product → Category)  

✅ **Pagination & Filtering**
- List endpoints support `page`, `pageSize`, and filtering by `price`, `category`, etc.  

✅ **Caching**
- In-memory caching for frequently accessed endpoints  
- Cache invalidation after updates  

✅ **Logging & Error Handling**
- Global exception handling middleware  
- Structured logging for requests and important actions  

✅ **Database**
- PostgreSQL with Entity Framework Core  
- Code-first migrations  
- Eager loading to avoid N+1 queries  

✅ **Unit Tests**
- xUnit + Moq for services  
- FluentAssertions for clean, readable assertions  

✅ **Architecture & Clean Code**
- Layered architecture: API → Application → Domain → Infrastructure  
- SOLID principles applied  
- AutoMapper for DTO-Entity mapping  
- Validators for DTOs  

---

## Tech Stack

- **Backend**: .NET 10, C#  
- **ORM**: EF Core 10  
- **Database**: PostgreSQL  
- **Authentication**: JWT Bearer  
- **Caching**: In-Memory Cache (Microsoft.Extensions.Caching.Memory)  
- **Logging**: Microsoft.Extensions.Logging  
- **Unit Testing**: xUnit, Moq, FluentAssertions  
- **Documentation**: Swagger  

---

## Architecture & Folder Structure

```
InventoryManagementSystem/
├── Inventory.Api/          # Web API project
│   ├── Controllers/        # API endpoints (Products, Categories, Auth)
│   ├── Middleware/         # Global exception handling
│   └── Extensions/         # Service & middleware extensions
├── Inventory.Application/  # Business logic layer
│   ├── DTOs/               # Data Transfer Objects
│   ├── Services/           # ProductService, CategoryService, AuthService
│   ├── Interfaces/         # Service interfaces
│   ├── Validators/         # DTO validators
│   └── Mappings/           # AutoMapper profiles
├── Inventory.Domain/       # Core entities & enums
│   ├── Entities/           # Product, Category, User
│   ├── Enums/              # ProductStatus, UserRole
│   └── Interfaces/         # Repository interfaces
├── Inventory.Infrastructure/  # Data access & infrastructure services
│   ├── Repositories/       # EF Core repositories
│   ├── Services/           # JwtService, MemoryCacheService
│   ├── Logging/            # Custom logging helpers
│   ├── Authentication/     # JWT configuration & helpers
│   └── Data/               # AppDbContext, migrations
├── Inventory.Tests/        # Unit tests
│   ├── Services/           # Service unit tests
│   └── Repositories/       # Repository unit tests
```

---

## Setup & Installation

1. **Clone repository**  
```bash
git clone https://github.com/yourusername/InventoryManagementSystem.git
cd InventoryManagementSystem
```

2. **Install dependencies**  
```bash
dotnet restore
```

3. **Configure environment variables** in `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=InventoryDB;Username=postgres;Password=yourpassword"
  },
  "JwtSettings": {
    "SecretKey": "YOUR_SECRET_KEY_HERE"
  }
}
```

---

## Database Setup

1. **Apply migrations**:
```bash
dotnet ef database update --project Inventory.Infrastructure --startup-project Inventory.Api
```

2. **Seed initial data** (products, categories, admin user) if implemented.

---

## Running the API

```bash
dotnet run --project Inventory.Api
```

- Swagger UI available at: `https://localhost:5001/swagger`  

---

## Authentication & Authorization

- **Login Endpoint**: `POST /api/auth/login`  
- **Register Endpoint**: `POST /api/auth/register` (Admin creates users)  
- **JWT Token**: Include `Authorization: Bearer {token}` in header  
- **Role-based Access**:  
  - Admin: Full access including DELETE  
  - User: Read & Create  

---

## API Endpoints

**Products**
- `GET /api/products` → List products with pagination & filtering  
- `GET /api/products/{id}` → Get single product  
- `POST /api/products` → Create product  
- `PUT /api/products/{id}` → Update product  
- `DELETE /api/products/{id}` → Delete product (Admin only)  

**Categories**
- `GET /api/categories` → List categories  
- `GET /api/categories/{id}` → Get single category  
- `POST /api/categories` → Create category  
- `PUT /api/categories/{id}` → Update category  
- `DELETE /api/categories/{id}` → Delete category (Admin only)  

---

## Caching & Performance

- **Product list caching** using in-memory cache  
- Cache invalidation on create/update/delete actions  
- Reduces DB calls & improves response time  

---

## Logging & Error Handling

- Global exception handling middleware  
- Logs all important actions: create, update, delete  
- Standardized error response format:  
```json
{
  "status": 404,
  "message": "Product not found",
  "timestamp": "2026-01-13T10:00:00Z"
}
```

---

## Testing

- **Unit Tests**: xUnit + Moq + FluentAssertions  
- Coverage includes:
  - ProductService
  - CategoryService
  - AuthService  
- Run tests:  
```bash
dotnet test
```

---

## Clean Code & SOLID

- Each service & repository follows **Single Responsibility Principle**  
- DTOs separate API models from entities  
- Dependency Injection used everywhere  
- AutoMapper for mapping DTOs → Entities  
- Layered architecture ensures **separation of concerns**  

---

## Contributing

1. Fork repository  
2. Create feature branch (`git checkout -b feature/MyFeature`)  
3. Commit changes (`git commit -m "Add feature"`)  
4. Push to branch (`git push origin feature/MyFeature`)  
5. Open a Pull Request  

---

## License

This project is **MIT licensed**.

