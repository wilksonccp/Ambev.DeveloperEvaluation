# Developer Evaluation Project — Sales API

## 📌 Overview

This repository contains the implementation of the Developer Evaluation Project, starting from the official template provided.

The base template already includes a solid foundation with modern .NET 8 architecture, authentication, validations, data access, and testing.

Our main objective is to implement a Sales API that complies with the defined business rules while following clean architecture and best practices.

---

## ✅ What We Already Have (Template Features)

The provided template is not empty — it comes with a complete working skeleton:

- .NET 8 Web API with pre-configured Swagger/OpenAPI.
- Layered architecture:
  - **Domain** (entities and business logic)
  - **Application** (commands, queries, validators, handlers)
  - **Infrastructure** (EF Core, repositories, migrations)
  - **WebApi** (controllers, configuration, IoC)
  - **Common** (security, logging, shared utilities)
- Authentication: JWT-based login.
- Validation: FluentValidation integrated.
- ORM: EF Core with existing migrations.
- Docker Compose: Services for PostgreSQL, MongoDB, and Redis.
- Testing:
  - Unit tests (validators, entities, handlers)
  - Integration tests with xUnit and NSubstitute.
- Frameworks included:
  - Mediator (CQRS pattern)
  - AutoMapper
  - Rebus (service bus placeholder)
  - Faker (data generation)

This gives us a robust starting point to focus on the business requirements.

---

## 🎯 What We Intend to Implement

The main task is to implement a Sales API that manages sales records.

This includes CRUD operations and domain rules such as:

### Sales Information

- Sale number
- Sale date
- Customer (external identity)
- Branch (external identity)
- Products and quantities
- Unit prices
- Discounts
- Totals
- Cancelled/Not Cancelled flag

### Business Rules

- 1–3 items → no discount
- 4–9 items → 10% discount
- 10–20 items → 20% discount
- 20 items → not allowed

### Operations

- Create, update, retrieve, and cancel sales.
- Add, update, or remove items in an existing sale.
- Cancel a specific item or the entire sale.

### Events (bonus points)

- `SaleCreated`
- `SaleModified`
- `SaleCancelled`
- `ItemCancelled`

These will be logged for now, but can be extended to message brokers in the future.

---

## 🔧 Technical Goals

- Maintain consistency with the provided template and project structure.
- Implement CQRS-style handlers with MediatR.
- Validate all business rules via FluentValidation.
- Ensure idempotency and correctness of discount calculations.
- Provide unit tests for discount logic and item rules.
- Provide integration tests for the Sales API endpoints.
- Improve logging and observability with Serilog.
- Deliver clear API documentation via Swagger.

---

## 🛠️ Planned Improvements

If time permits, we will extend the project with:

### Angular Frontend (bonus)

- A simple UI to interact with the Sales API (list, create, cancel sales).
- Authentication flow using JWT.
- Dockerized setup to run frontend + backend together.

---

## 🚀 How to Run

### Prerequisites

- .NET 8 SDK
- Docker

### Using Docker Compose

docker compose up -d
API available at: http://localhost:8080/swagger

### Local Development
Update appsettings.json with your local PostgreSQL credentials.

Apply migrations:
- bash
`dotnet ef database update --project src/Ambev.DeveloperEvaluation.ORM`

- bash
`dotnet run --project src/Ambev.DeveloperEvaluation.WebApi`

## 📂 Project Structure


### Code

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


## 🧪 Testing

Unit tests: Validate domain rules (discounts, item limits, totals).
Integration tests: End-to-end API flows (create, update, cancel).
Run all tests:

- bash
dotnet test

## 📌 Notes
The repository starts from the official evaluation template.
Commits will be atomic and descriptive, following Git Flow and Semantic Commit Messages.
Documentation is maintained in English for clarity and alignment with industry standards.

## 🏷️ Sales Domain
This section documents the design decisions and business rules implemented for the Sales domain. It focuses on entities, discount policies, total calculations, and domain exceptions. Tests and API details are outside this scope.

Entities
Sale
Fields: Id, CreatedAt, UpdatedAt, DeletedAt?, Number, CustomerId, CustomerName, BranchId, BranchName, IsCancelled.

Items: read-only collection (ReadOnlyItems) of SaleItem.

Totals:

TotalAmount: gross sum of items (unit price × quantity).

TotalPayable: net sum of items (after discount, rounded per item).

TotalDiscount: difference between gross and net (TotalAmount - TotalPayable).

Operations:

CreateNew(...)

AddItem(productId, productName, unitPrice, quantity)

UpdateItemQuantity(productId, newQuantity)

RemoveItem(productId)

CancelItems()

CancelSale()

Recalculate()

SaleItem
Fields: ProductId, ProductName, Quantity, UnitPrice, DiscountAmount, LineTotal, IsCancelled.

Operations: SetQuantity, IncreaseQuantity, EnsureSameUnitPrice, Cancel, Recalculate.

## 🎯 Discount Policy
Quantity-based rules (max per item: MaxPerItem = 20):

1 to 3 units: 0%

4 to 9 units: 10%

10 to 20 units: 20%

20 units: invalid (throws exception)

## Implementation
DiscountPolicy.GetRate(quantity)

DiscountPolicy.CalculateDiscount(unitPrice, quantity)

## 💰 Totals and Rounding
Policy uses NodaMoney for monetary calculations and per-item rounding.

Default currency: BRL (Currency.FromCode("BRL")).

Per-item calculation:

gross = unitPrice × quantity

discount = gross × rate

lineTotal = gross - discount (all as Money)

Public API of DiscountPolicy returns decimal (Money.Amount values).

In Sale.Recalculate():

TotalAmount = sum of UnitPrice × Quantity for active items.

TotalPayable = sum of LineTotal for active items.

TotalDiscount = TotalAmount - TotalPayable.

## 🚫 Validations and Domain Exceptions
IDs (Sale, SaleItem): cannot be Guid.Empty.

Strings (Number, CustomerName, BranchName, ProductName): cannot be empty.

Unit price: must be > 0 → UNIT_PRICE_MUST_BE_POSITIVE.

Quantity: must be ≥ 1 → QUANTITY_MUST_BE_POSITIVE.

Max per item: must be ≤ 20 → MAX_PER_ITEM_EXCEEDED.

Same product line: unit price must remain stable → UNIT_PRICE_MISMATCH.

Forbidden operations on cancelled sales/items → (SALE_CANCELLED, ITEM_CANCELLED).

## 💡 Motivation: NodaMoney
Safely represents monetary values with explicit currency.

Handles per-currency rounding rules, avoiding discrepancies in discount operations.

Public interface remains decimal for simplicity and compatibility, while internal calculations use Money.