<div align="center">

# 💰 FinTrack

### Personal Finance Management REST API

A backend-focused personal finance platform built with **ASP.NET Core, Entity Framework Core, MySQL, JWT Authentication and Docker**.

Designed to demonstrate real-world backend development: authentication, financial business logic, relational data modeling, background processing, testing and containerization.

</div>

---

## ✨ Features

- 🔐 JWT authentication and authorization
- 🔑 Secure password hashing with BCrypt
- 👤 User registration and login
- 💳 Multiple financial accounts
- 💵 Income and expense tracking
- 🏷️ Transaction categories
- 🔄 Account-to-account transfers
- 📊 Monthly budget tracking
- 📈 Financial dashboard summary
- ⏰ Recurring transactions
- ⚙️ Background transaction processing
- 🗄️ Automatic EF Core database migrations
- 📖 Swagger / OpenAPI documentation
- 🐳 Dockerized API and MySQL database
- 🧪 Automated tests with xUnit

---

## 🛠️ Tech Stack

| Technology | Usage |
|---|---|
| **C# / .NET 10** | Main backend platform |
| **ASP.NET Core Web API** | REST API |
| **Entity Framework Core 10** | ORM and database access |
| **MySQL 8** | Relational database |
| **JWT Bearer** | Authentication and authorization |
| **BCrypt** | Password hashing |
| **Swagger / OpenAPI** | API documentation |
| **Docker** | Containerization |
| **Docker Compose** | API + database orchestration |
| **xUnit** | Automated testing |
| **EF Core InMemory** | Test database |

---

## 🏗️ Architecture

The solution is separated into multiple projects to keep responsibilities isolated:

```text
FinTrack/
│
├── backend/
│   ├── FinTrack.Api/
│   │   └── Controllers/
│   │
│   ├── FinTrack.Application/
│   │   ├── Accounts/
│   │   ├── Auth/
│   │   ├── Budgets/
│   │   ├── Categories/
│   │   ├── Dashboard/
│   │   ├── RecurringTransactions/
│   │   └── Transactions/
│   │
│   ├── FinTrack.Domain/
│   │   ├── Entities/
│   │   └── Enums/
│   │
│   ├── FinTrack.Infrastructure/
│   │   ├── BackgroundServices/
│   │   ├── Persistence/
│   │   └── Services/
│   │
│   └── FinTrack.Tests/
│       └── Transactions/
│
├── Dockerfile
├── docker-compose.yml
├── .dockerignore
├── .gitignore
└── FinTrack.slnx
```

### `FinTrack.Api`

Presentation layer responsible for:

- REST controllers
- JWT configuration
- Swagger configuration
- dependency injection
- application startup
- automatic database migration execution

### `FinTrack.Application`

Contains request/response models, application contracts and DTOs for:

- Authentication
- Accounts
- Categories
- Transactions
- Transfers
- Budgets
- Dashboard
- Recurring transactions

### `FinTrack.Domain`

Contains the core domain entities and enums.

Main entities:

- `User`
- `Account`
- `Transaction`
- `Category`
- `Budget`
- `RecurringTransaction`

### `FinTrack.Infrastructure`

Handles infrastructure concerns:

- Entity Framework Core
- MySQL persistence
- EF Core migrations
- Authentication service
- JWT generation
- BCrypt password verification
- Recurring transaction background worker

### `FinTrack.Tests`

Contains automated tests for core financial transaction logic using **xUnit** and **EF Core InMemory**.

---

## 🔌 API Endpoints

### Authentication

| Method | Endpoint | Description |
|---|---|---|
| `POST` | `/api/auth/register` | Register a new user |
| `POST` | `/api/auth/login` | Login and receive JWT |

### Accounts

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/accounts` | Get user accounts |
| `GET` | `/api/accounts/{id}` | Get account by ID |
| `POST` | `/api/accounts` | Create account |
| `PUT` | `/api/accounts/{id}` | Update account |
| `DELETE` | `/api/accounts/{id}` | Delete account |

### Categories

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/categories` | Get categories |
| `POST` | `/api/categories` | Create category |
| `DELETE` | `/api/categories/{id}` | Delete category |

### Transactions

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/transactions` | Get transaction history |
| `POST` | `/api/transactions` | Create income or expense |
| `POST` | `/api/transactions/transfer` | Transfer money between accounts |

### Budgets

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/budgets` | Get budgets |
| `GET` | `/api/budgets/{id}` | Get budget by ID |
| `POST` | `/api/budgets` | Create budget |
| `PUT` | `/api/budgets/{id}` | Update budget |
| `DELETE` | `/api/budgets/{id}` | Delete budget |

### Recurring Transactions

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/recurring-transactions` | Get recurring transactions |
| `GET` | `/api/recurring-transactions/{id}` | Get recurring transaction |
| `POST` | `/api/recurring-transactions` | Create recurring transaction |
| `PUT` | `/api/recurring-transactions/{id}` | Update recurring transaction |
| `DELETE` | `/api/recurring-transactions/{id}` | Delete recurring transaction |

### Dashboard

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/dashboard/summary` | Get financial summary |

---

## 💸 Financial Business Logic

FinTrack implements financial logic beyond standard CRUD operations.

### Income

Income transactions automatically increase the selected account balance.

### Expenses

Expense transactions decrease the account balance.

Transactions are rejected when the account does not contain sufficient funds.

### Account Transfers

Transfers are processed atomically between two accounts.

Before completing a transfer, FinTrack:

1. Verifies ownership of both accounts.
2. Validates the transfer amount.
3. Checks the source account balance.
4. Ensures both accounts use the same currency.
5. Decreases the source account balance.
6. Increases the destination account balance.
7. Creates transaction records representing both sides of the transfer.

This ensures the total balance remains consistent during internal transfers.

---

## 📊 Budget Tracking

Users can define monthly spending limits for expense categories.

For every budget, FinTrack dynamically calculates:

```text
Limit Amount
Spent Amount
Remaining Amount
Percentage Used
```

Spending is calculated from actual expense transactions instead of being manually stored.

---

## ⏰ Recurring Transactions

FinTrack supports automatic recurring income and expense transactions.

Supported recurrence intervals:

- Daily
- Weekly
- Monthly
- Yearly

A hosted `.NET BackgroundService` periodically checks for transactions that are ready to execute.

When a recurring transaction becomes due, the worker:

```text
Recurring transaction becomes due
              │
              ▼
     Validate account
              │
              ▼
       Update balance
              │
              ▼
     Create transaction
              │
              ▼
Calculate next execution date
```

This allows operations such as salaries, subscriptions and recurring expenses to be processed automatically.

---

## 🔐 Authentication & Security

FinTrack uses **JWT Bearer Authentication**.

After successful login, the API returns a JWT:

```http
Authorization: Bearer <token>
```

Protected resources are scoped to the authenticated user.

Security measures include:

- BCrypt password hashing
- JWT authentication
- user-scoped accounts
- account ownership validation
- category ownership validation
- insufficient-funds validation
- secrets separated from source-controlled configuration

Passwords are never stored in plain text.

---

## 🐳 Running with Docker

### Requirements

You only need:

- Docker
- Docker Compose

Clone the repository:

```bash
git clone https://github.com/LazyOroz/FinTrack.git
cd FinTrack
```

Start the complete application:

```bash
docker compose up --build
```

Docker Compose starts:

```text
┌─────────────────────┐
│    FinTrack API     │
│      :8081          │
└──────────┬──────────┘
           │
           │ EF Core
           ▼
┌─────────────────────┐
│      MySQL 8        │
│      :3308          │
└─────────────────────┘
```

Database migrations are automatically applied when the API starts.

### Swagger

After startup, open:

```text
http://localhost:8081/swagger
```

### Stop containers

```bash
docker compose down
```

To also delete the database volume:

```bash
docker compose down -v
```

> [!WARNING]
> `docker compose down -v` permanently removes the Docker database volume and its stored data.

---

## 💻 Running Locally

### Requirements

- .NET 10 SDK
- MySQL 8

For local development, sensitive configuration should be stored with **.NET User Secrets**.

### Connection String

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "server=localhost;port=3306;database=fintrack_db;user=root;password=YOUR_PASSWORD" --project backend/FinTrack.Api
```

### JWT Secret

```bash
dotnet user-secrets set "Jwt:Key" "YOUR_SECURE_JWT_KEY_AT_LEAST_32_CHARACTERS_LONG" --project backend/FinTrack.Api
```

Run the API:

```bash
dotnet run --project backend/FinTrack.Api
```

---

## 🗄️ Database

FinTrack uses **MySQL 8** with **Entity Framework Core**.

Main tables:

```text
Users
Accounts
Categories
Transactions
Budgets
RecurringTransactions
```

### Create a Migration

```bash
dotnet ef migrations add MigrationName --project backend/FinTrack.Infrastructure --startup-project backend/FinTrack.Api --output-dir Persistence/Migrations
```

### Apply Migrations

```bash
dotnet ef database update --project backend/FinTrack.Infrastructure --startup-project backend/FinTrack.Api
```

When using Docker, existing migrations are automatically applied during API startup.

---

## 🧪 Tests

FinTrack includes automated tests for core financial logic.

Run:

```bash
dotnet test
```

Current test scenarios:

| Test | Purpose |
|---|---|
| Income | Verifies that income increases account balance |
| Expense | Verifies that expenses decrease account balance |
| Insufficient funds | Prevents invalid expense processing |
| Transfer | Verifies money movement between accounts |

Current test suite:

```text
Total tests: 4
Passed:      4
Failed:      0
```

---

## 📖 Swagger

Swagger / OpenAPI is included for exploring and testing the API.

The complete authentication flow can be tested directly from Swagger:

```text
Register
   ↓
Login
   ↓
Receive JWT
   ↓
Authorize
   ↓
Access protected endpoints
```

When running with Docker:

```text
http://localhost:8081/swagger
```

---

## 🚀 Future Improvements

Planned and possible improvements:

- React frontend
- Refresh tokens
- Integration tests
- Transaction filtering
- Pagination
- Multi-currency conversion
- Financial reports and charts
- CI/CD with GitHub Actions
- Cloud deployment

---

## 👨‍💻 Author

**Orozobek Israilov**

Backend / Full-Stack Developer

GitHub: [@LazyOroz](https://github.com/LazyOroz)

---

<div align="center">

Built with **C# · ASP.NET Core · MySQL · Docker**

⭐ If you find this project useful, consider giving it a star.

</div>