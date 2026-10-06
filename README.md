\# FinTrack



FinTrack is a personal finance management REST API built with ASP.NET Core and MySQL.



The project provides secure user authentication, account and transaction management, budgeting, account-to-account transfers, financial summaries, and automatic recurring transaction processing.



It was built as a backend portfolio project with a focus on clean project separation, financial business logic, authentication, persistence, background processing, testing, and containerized deployment.



\## Features



\- JWT authentication and authorization

\- User registration and login

\- BCrypt password hashing

\- Personal financial accounts

\- Income and expense tracking

\- Transaction categories

\- Account-to-account transfers

\- Monthly budgets with automatic spending calculations

\- Dashboard financial summary

\- Recurring income and expense transactions

\- Background processing with `BackgroundService`

\- Automatic EF Core database migrations

\- Swagger / OpenAPI documentation

\- Dockerized API and MySQL database

\- Automated tests with xUnit and EF Core InMemory



\## Tech Stack



\- C#

\- .NET 10

\- ASP.NET Core Web API

\- Entity Framework Core 10

\- MySQL 8

\- JWT Bearer Authentication

\- BCrypt

\- Swagger / OpenAPI

\- Docker

\- Docker Compose

\- xUnit



\## Architecture



FinTrack is divided into separate projects:



```text

FinTrack

|

|-- backend

|   |-- FinTrack.Api

|   |-- FinTrack.Application

|   |-- FinTrack.Domain

|   |-- FinTrack.Infrastructure

|   `-- FinTrack.Tests

|

|-- Dockerfile

|-- docker-compose.yml

|-- .dockerignore

|-- .gitignore

`-- FinTrack.slnx

```



\### FinTrack.Api



The presentation layer of the application.



Contains:



\- REST API controllers

\- JWT authentication configuration

\- Swagger configuration

\- dependency injection

\- application startup

\- automatic database migration execution



\### FinTrack.Application



Contains application contracts and DTOs for:



\- authentication

\- accounts

\- categories

\- transactions

\- transfers

\- budgets

\- dashboard

\- recurring transactions



\### FinTrack.Domain



Contains the core domain models and enums.



Main entities:



\- User

\- Account

\- Transaction

\- Category

\- Budget

\- RecurringTransaction



\### FinTrack.Infrastructure



Handles infrastructure concerns including:



\- Entity Framework Core

\- MySQL persistence

\- database migrations

\- authentication service

\- JWT generation

\- BCrypt password verification

\- recurring transaction background worker



\### FinTrack.Tests



Contains automated tests for financial transaction logic using xUnit and EF Core InMemory.



\## API



\### Authentication



```text

POST /api/auth/register

POST /api/auth/login

```



\### Accounts



```text

GET    /api/accounts

GET    /api/accounts/{id}

POST   /api/accounts

PUT    /api/accounts/{id}

DELETE /api/accounts/{id}

```



\### Categories



```text

GET    /api/categories

POST   /api/categories

DELETE /api/categories/{id}

```



\### Transactions



```text

GET  /api/transactions

POST /api/transactions

POST /api/transactions/transfer

```



\### Budgets



```text

GET    /api/budgets

GET    /api/budgets/{id}

POST   /api/budgets

PUT    /api/budgets/{id}

DELETE /api/budgets/{id}

```



\### Recurring Transactions



```text

GET    /api/recurring-transactions

GET    /api/recurring-transactions/{id}

POST   /api/recurring-transactions

PUT    /api/recurring-transactions/{id}

DELETE /api/recurring-transactions/{id}

```



\### Dashboard



```text

GET /api/dashboard/summary

```



\## Financial Logic



FinTrack contains business logic beyond standard CRUD operations.



\### Income



Income transactions increase the selected account balance.



\### Expenses



Expense transactions decrease the account balance and are rejected when the account has insufficient funds.



\### Transfers



Transfers are performed atomically between two accounts.



A transfer:



1\. validates ownership of both accounts

2\. validates sufficient funds

3\. validates matching currencies

4\. decreases the source account balance

5\. increases the destination account balance

6\. records both sides of the transfer



\### Budgets



Budgets are assigned to expense categories for a specific month and year.



FinTrack dynamically calculates:



\- amount spent

\- remaining budget

\- percentage used



\### Recurring Transactions



Recurring transactions can execute automatically according to a schedule:



\- Daily

\- Weekly

\- Monthly

\- Yearly



A hosted background service periodically checks for due recurring transactions, updates account balances, creates transaction records, and schedules the next execution.



\## Authentication



Protected endpoints use JWT Bearer authentication.



Passwords are never stored directly. Password hashes are generated and verified using BCrypt.



After logging in, use the returned JWT token:



```text

Authorization: Bearer <token>

```



Swagger also provides an \*\*Authorize\*\* button for testing protected endpoints.



\## Running with Docker



\### Requirements



\- Docker

\- Docker Compose



Clone the repository and run:



```bash

docker compose up --build

```



Docker Compose starts:



\- FinTrack API

\- MySQL 8 database



EF Core migrations are automatically applied when the API starts.



Swagger is available at:



```text

http://localhost:8081/swagger

```



The MySQL container is exposed locally on port `3308`.



To stop the application:



```bash

docker compose down

```



To stop the application and remove the database volume:



```bash

docker compose down -v

```



> `docker compose down -v` permanently removes the Docker database volume and its stored data.



\## Running Locally



\### Requirements



\- .NET 10 SDK

\- MySQL 8



Configure the connection string and JWT key using .NET User Secrets:



```bash

dotnet user-secrets set "ConnectionStrings:DefaultConnection" "server=localhost;port=3306;database=fintrack\_db;user=root;password=YOUR\_PASSWORD" --project backend/FinTrack.Api



dotnet user-secrets set "Jwt:Key" "YOUR\_SECURE\_JWT\_KEY\_AT\_LEAST\_32\_CHARACTERS\_LONG" --project backend/FinTrack.Api

```



Then run:



```bash

dotnet run --project backend/FinTrack.Api

```



\## Database Migrations



Create a new migration:



```bash

dotnet ef migrations add MigrationName \\

&#x20; --project backend/FinTrack.Infrastructure \\

&#x20; --startup-project backend/FinTrack.Api \\

&#x20; --output-dir Persistence/Migrations

```



Apply migrations manually:



```bash

dotnet ef database update \\

&#x20; --project backend/FinTrack.Infrastructure \\

&#x20; --startup-project backend/FinTrack.Api

```



When running through Docker, existing migrations are automatically applied during API startup.



\## Tests



Run all tests with:



```bash

dotnet test

```



The current test suite covers core transaction scenarios including:



\- income balance updates

\- expense balance updates

\- insufficient balance protection

\- transfers between accounts



\## Security



FinTrack includes:



\- JWT Bearer authentication

\- BCrypt password hashing

\- user-scoped financial data

\- account ownership validation

\- category ownership validation

\- insufficient-funds validation

\- secrets separated from source-controlled configuration



Local development secrets should be stored using .NET User Secrets or environment variables and should never be committed to the repository.



\## Future Improvements



Possible future improvements include:



\- React frontend

\- refresh tokens

\- integration tests

\- transaction filtering and pagination

\- multi-currency conversion

\- reporting and charts

\- CI/CD pipeline

\- cloud deployment



\## License



This project is intended for educational and portfolio purposes.

