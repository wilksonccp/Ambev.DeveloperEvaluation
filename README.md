# Developer Evaluation Project — Sales API

## 📌 Overview
This repository contains the implementation of the **Developer Evaluation Project**, starting from the official template provided.  
The base template already includes a solid foundation with modern .NET 8 architecture, authentication, validations, data access, and testing.  

Our main objective is to **implement a Sales API** that complies with the defined business rules while following clean architecture and best practices.  

---

## ✅ What We Already Have (Template Features)
The provided template is not empty — it comes with a complete working skeleton:

- **.NET 8 Web API** with Swagger/OpenAPI.
- **Layered architecture**:
  - `Domain` (entities and business logic)
  - `Application` (commands, queries, validators, handlers)
  - `Infrastructure` (EF Core, repositories, migrations)
  - `WebApi` (controllers, configuration, IoC)
  - `Common` (security, logging, shared utilities)
- **Authentication**: JWT-based login.
- **Validation**: FluentValidation integrated.
- **ORM**: EF Core with migrations already applied.
- **Docker Compose**: Services for PostgreSQL, MongoDB, and Redis.
- **Testing**:
  - Unit tests (validators, entities, handlers)
  - Integration tests with xUnit and NSubstitute.
- **Frameworks included**:
  - MediatR (CQRS)
  - AutoMapper
  - Rebus (placeholder for messaging)
  - Faker (data generation)

---

## 🎯 Implemented Features
- **Sales flow completed**: create, mutate items, cancel items or entire sale, get by id, and list with filters.
- **Domain aggregate + repository** for Sale and SaleItems.
- **EF Core mappings** with monetary precision (18,2) and indexes.
- **List endpoint** exposed with paging, filtering, and ordering.
- **MediatR + AutoMapper** handlers fully integrated.
- **Swagger configured** with JWT Bearer security.
- **Default admin seeded** for easy login.

---

## 🔧 Technical Details

### Application Handlers
- `CreateSaleHandler`: validates command, builds Sale, persists via repository, maps to result.  
- `AddItemToSaleHandler`: loads sale, adds item, updates repository.  
- `UpdateItemQuantityHandler`: updates item quantity.  
- `RemoveItemFromSaleHandler`: removes item.  
- `CancelItemsHandler`: cancels all active items.  
- `CancelSaleHandler`: cancels the entire sale.  
- `GetSaleHandler`: retrieves sale by Id.  
- `ListSalesHandler`: lists sales with paging/filter/order.  

### Repository
- `ISaleRepository` extended with `ListAsync(...)`.  
- `SaleRepository`: implemented filters (customer, branch, number), simple ordering (`number`, `-number`, `createdAt`, `-createdAt`), and pagination.  

### EF Core Mapping
- **SaleConfiguration**:
  - Sales: unique index on `Number`, precision on totals (18,2), indexes on `CustomerId`, `BranchId`, `CreatedAt`.  
  - Items: mapped as owned collection (`SaleItems`), precision on monetary fields, indexes on `SaleId` and `ProductId`.  
  - `ReadOnlyItems` ignored to prevent EF mapping error.  

### Web API
- `SalesController`: added `[HttpGet]` list endpoint with query params (`page`, `size`, `order`, `customerId`, `branchId`, `number`).  
- Mapped `CreateSaleResult` → `CreateSaleResponse`.  

### Swagger & Auth
- Added JWT Bearer definition.  
- “Authorize” now accepts `Bearer {token}`.  
- On startup: applies migrations and seeds default admin:  
  - **Email**: `admin@local`  
  - **Password**: `Admin@123`  
  - **Role**: `Admin`  

---

## 🧪 Build & Test
- Solution builds clean.  
- **Unit tests**: 107 passed, 0 failed.  
- Integration tests validated.  

---

## 🚀 How to Run

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)  
- [Docker](https://docs.docker.com/get-docker/)  

### Using Docker Compose
```bash
docker compose up -d
```
API available at: [http://localhost:8080/swagger](http://localhost:8080/swagger)

### Local Development
Update `appsettings.json` with local PostgreSQL credentials.  

Apply migrations:
```bash
dotnet ef database update --project src/Ambev.DeveloperEvaluation.ORM
```

Run the API:
```bash
dotnet run --project src/Ambev.DeveloperEvaluation.WebApi
```

---

## 📂 Project Structure
```
root
├── src/
│   ├── Ambev.DeveloperEvaluation.Domain
│   ├── Ambev.DeveloperEvaluation.Application
│   ├── Ambev.DeveloperEvaluation.Infrastructure
│   ├── Ambev.DeveloperEvaluation.Common
│   └── Ambev.DeveloperEvaluation.WebApi
├── tests/
│   └── Ambev.DeveloperEvaluation.Tests
├── docker-compose.yml
└── README.md
```

---

## 🔍 Smoke Test Hints
1. **Auth**  
   - `POST /api/auth` with:  
     ```json
     { "email": "admin@local", "password": "Admin@123" }
     ```  
   - Copy token → “Authorize” in Swagger.  

2. **Sales flow**  
   - `POST /api/sales` → create.  
   - `GET /api/sales/{id}` → retrieve by Id.  
   - `GET /api/sales?page=1&size=10&order=-createdAt` → list.  
   - `POST /api/sales/{id}/items`, `PATCH /api/sales/{id}/items/{productId}`, `DELETE /api/sales/{id}/items/{productId}`.  
   - `POST /api/sales/{id}:cancel-items`, `POST /api/sales/{id}:cancel`.  

---

## 📌 Notes
- PostgreSQL must have **pgcrypto** extension enabled for `gen_random_uuid()`.  
- Commits are atomic and follow **Git Flow** + **Semantic Commit Messages**.  
- Documentation maintained in **English**.  
