# Developer Evaluation Project — Sales API

## 📌 Overview
This repository contains the implementation of the **Developer Evaluation Project**, starting from the official template provided.  
The base template already includes a solid foundation with modern .NET 8 architecture, authentication, validations, data access, and testing.  

Our main objective is to **implement a Sales API** that complies with the defined business rules while following clean architecture and best practices.  

---

## ✅ What We Already Have (Template Features)
The provided template is not empty — it comes with a complete working skeleton:

- **.NET 8 Web API** with pre-configured Swagger/OpenAPI.
- **Layered architecture**:
  - `Domain` (entities and business logic)
  - `Application` (commands, queries, validators, handlers)
  - `Infrastructure` (EF Core, repositories, migrations)
  - `WebApi` (controllers, configuration, IoC)
  - `Common` (security, logging, shared utilities)
- **Authentication**: JWT-based login.
- **Validation**: FluentValidation integrated.
- **ORM**: EF Core with existing migrations.
- **Docker Compose**: Services for PostgreSQL, MongoDB, and Redis.
- **Testing**:
  - Unit tests (validators, entities, handlers)
  - Integration tests with xUnit and NSubstitute.
- **Frameworks included**:
  - Mediator (CQRS pattern)
  - AutoMapper
  - Rebus (service bus placeholder)
  - Faker (data generation)

This gives us a robust starting point to focus on the business requirements.

---

## 🎯 What We Intend to Implement
The main task is to implement a **Sales API** that manages sales records.  
This includes CRUD operations and domain rules such as:

- Sales information:
  - Sale number
  - Sale date
  - Customer (external identity)
  - Branch (external identity)
  - Products and quantities
  - Unit prices
  - Discounts
  - Totals
  - Cancelled/Not Cancelled flag
- **Business rules**:
  - 1–3 items → no discount
  - 4–9 items → 10% discount
  - 10–20 items → 20% discount
  - >20 items → not allowed
- **Operations**:
  - Create, update, retrieve, and cancel sales.
  - Add, update, or remove items in an existing sale.
  - Cancel a specific item or the entire sale.
- **Events (bonus points)**:
  - `SaleCreated`
  - `SaleModified`
  - `SaleCancelled`
  - `ItemCancelled`  
  > These will be logged for now, but can be extended to message brokers in the future.

---

## 🔧 Technical Goals
- Maintain consistency with the provided template and project structure.
- Implement **CQRS-style handlers** with MediatR.
- Validate all business rules via FluentValidation.
- Ensure **idempotency and correctness** of discount calculations.
- Provide **unit tests** for discount logic and item rules.
- Provide **integration tests** for the Sales API endpoints.
- Improve **logging and observability** with Serilog.
- Deliver clear **API documentation** via Swagger.

---

## 🛠️ Planned Improvements
If time permits, we will extend the project with:

- **Angular Frontend** (bonus):
  - A simple UI to interact with the Sales API (list, create, cancel sales).
  - Authentication flow using JWT.
  - Dockerized setup to run frontend + backend together.

---

## 🚀 How to Run

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- [Docker](https://docs.docker.com/get-docker/)

### Using Docker Compose
```bash
docker compose up -d
API available at: http://localhost:8080/swagger

Local Development
Update appsettings.json with your local PostgreSQL credentials.

Apply migrations:

bash
Copy code
dotnet ef database update --project src/Ambev.DeveloperEvaluation.ORM
Run the API:

bash
Copy code
dotnet run --project src/Ambev.DeveloperEvaluation.WebApi
📂 Project Structure
css
Copy code
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
🧪 Testing
Unit tests: Validate domain rules (discounts, item limits, totals).

Integration tests: End-to-end API flows (create, update, cancel).

Run all tests:

bash
Copy code
dotnet test

📌 Notes
The repository starts from the official evaluation template.

We will keep commits atomic and descriptive, following Git Flow and Semantic Commit Messages.

Documentation will be maintained in English for clarity and alignment with industry standards.
