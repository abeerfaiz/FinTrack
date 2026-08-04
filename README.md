# FinTrack

A personal finance web application that connects to UK bank accounts via Open Banking (TrueLayer) to automatically track spending, budgets, and account balances.

## Live Demo

- **Frontend:** https://white-pebble-0fb6c5210.7.azurestaticapps.net
- **API (Swagger):** https://fintrack-api.azurewebsites.net/swagger

**Demo credentials:**
- Email: `demo@fintrack.com`
- Password: `FinTrackDemo2026!`

## What It Does

- Connects UK bank accounts through TrueLayer's regulated Open Banking API using a secure OAuth2 flow
- Automatically syncs account balances and transaction history on a schedule, with manual on-demand sync
- Auto-categorises transactions using user-defined keyword rules, with manual override support
- Tracks monthly budgets per category and shows spend vs. budget in real time
- Visualises spending trends, category breakdowns, and top merchants over time
- Keeps bank tokens encrypted at rest and refreshes them proactively before they expire

## Tech Stack

- **Backend:** .NET 10, ASP.NET Core Web API, Clean Architecture, CQRS with MediatR, Entity Framework Core, PostgreSQL, Hangfire, Polly, Serilog, JWT authentication, AES-256 token encryption, FluentValidation
- **Frontend:** React, TypeScript, Vite, Tailwind CSS, Recharts
- **Infrastructure:** Docker, Azure, GitHub Actions CI/CD
- **Open Banking:** TrueLayer

## Infrastructure

- **Azure App Service (B1)** — hosts the ASP.NET Core API
- **Azure Database for PostgreSQL Flexible Server** — primary data store
- **Azure Key Vault** with **Managed Identity** — production secrets (JWT signing key, encryption key, TrueLayer credentials, DB connection string), zero credentials in source control or App Service config
- **Azure Static Web Apps** — hosts the React frontend
- **GitHub Actions** — CI/CD (build, test, security scan, deploy)

## Solution Structure

```
src/
  FinTrack.Domain/              # Entities, enums, domain exceptions — zero external dependencies
    Entities/
    Enums/
    Exceptions/

  FinTrack.Application/         # Use cases: CQRS commands/queries, MediatR handlers, FluentValidation
    Accounts/Queries/
    Auth/Commands/
    BankConnections/Commands/
    Budgets/Commands/, Queries/
    Categories/Commands/, Queries/
    RecurringPayments/Queries/
    Transactions/Commands/, Queries/
    Common/Behaviours/, Interfaces/, Models/

  FinTrack.Infrastructure/      # EF Core, TrueLayer client, Hangfire jobs, Polly resilience, encryption
    BackgroundJobs/
    OpenBanking/, OpenBanking/Mappers/, OpenBanking/Models/
    Persistence/Configurations/, Persistence/Migrations/, Persistence/Repositories/
    Security/

  FinTrack.API/                 # Controllers, middleware, JWT auth, DI composition root
    Controllers/
    Middleware/
    Services/

tests/
  FinTrack.Application.Tests/   # Unit tests — Auth, Budgets, Categories, Transactions
  FinTrack.Infrastructure.Tests/# Integration tests against a real PostgreSQL instance — Repositories

frontend/
  src/                          # React + TypeScript SPA

docs/
  adr/                          # Architecture Decision Records
```

## Key Design Decisions

- **Clean Architecture (4 layers).** Domain has zero dependencies; Application defines use cases against interfaces; Infrastructure implements those interfaces (EF Core, TrueLayer, Hangfire); API composes everything via DI. Business logic never depends on EF Core, HTTP, or any framework concern.
- **Hangfire over `IHostedService`.** Background sync (every 6 hours) and token refresh (every 4 minutes) need persistent scheduling, automatic retry on failure, and visibility into job history — a bare `IHostedService` loop gives you none of that out of the box and dies silently on unhandled exceptions. Hangfire persists job state to PostgreSQL, survives app restarts, and ships a dashboard for free.
- **AES-256 token storage in PostgreSQL, not Key Vault-per-token.** TrueLayer access/refresh tokens are encrypted with AES-256-CBC and stored directly in `bank_connections`, with only the encryption key held in Key Vault/user-secrets. Storing every token as a separate Key Vault secret would mean a network round-trip on every sync job — expensive and slow at scale. See [ADR-002](docs/adr/ADR-002-token-storage-strategy.md).
- **Managed Identity for Key Vault access.** In production, the App Service's system-assigned identity authenticates to Key Vault directly (`DefaultAzureCredential`) — no client secret or connection string for Key Vault itself ever exists anywhere, including in App Service configuration.

## Local Development

**Prerequisites:** .NET 10 SDK, Docker Desktop, Node.js 20+

### 1. Start PostgreSQL

```bash
docker-compose up -d
```

pgAdmin is available at http://localhost:5050 (email `admin@fintrack.com`, password `admin`).

### 2. Configure secrets

From `src/FinTrack.API`, set the required secrets with `dotnet user-secrets`:

```bash
cd src/FinTrack.API

dotnet user-secrets set "Jwt:Key" "a-local-development-key-at-least-32-characters-long"
dotnet user-secrets set "Jwt:Issuer" "FinTrack"
dotnet user-secrets set "Jwt:Audience" "FinTrack"
dotnet user-secrets set "Encryption:Key" "<base64-encoded-32-byte-key>"
dotnet user-secrets set "TrueLayer:ClientId" "<your-sandbox-client-id>"
dotnet user-secrets set "TrueLayer:ClientSecret" "<your-sandbox-client-secret>"
dotnet user-secrets set "TrueLayer:RedirectUri" "http://localhost:5247/api/bank-connections/callback"
```

### 3. Apply migrations

```bash
dotnet ef database update --project src/FinTrack.Infrastructure --startup-project src/FinTrack.API
```

### 4. Run the API

```bash
dotnet run --project src/FinTrack.API
```

Swagger UI is available at `/swagger`, and the Hangfire dashboard at `/hangfire` (development only, unauthenticated).

### 5. Run the frontend

```bash
cd frontend
npm install
npm run dev
```

The frontend runs at http://localhost:5173 and expects the API at http://localhost:5247 (or wherever configured via `VITE_API_URL`).

## TrueLayer Sandbox Setup

1. Register a free account at [console.truelayer.com](https://console.truelayer.com) and create an application in **Sandbox** mode.
2. Copy the sandbox **Client ID** and **Client Secret** into user-secrets as shown above.
3. Add your local callback URL (`http://localhost:5247/api/bank-connections/callback`) as an allowed redirect URI in the TrueLayer console.
4. The app is pre-configured to use TrueLayer's sandbox endpoints (`auth.truelayer-sandbox.com`, `api.truelayer-sandbox.com`) and the `uk-cs-mock` mock bank provider — no real bank account or FCA registration needed for local development.
5. When connecting a bank in the UI, select the **Mock Bank** provider. Use the Mock Bank test credentials: username `john`, password `doe`.

## Architecture Decision Records

- [ADR-001: Use TrueLayer as Open Banking aggregator](docs/adr/ADR-001-truelayer-open-banking.md)
- [ADR-002: Store TrueLayer tokens encrypted in PostgreSQL](docs/adr/ADR-002-token-storage-strategy.md)

## CI/CD Pipeline

Defined in [`.github/workflows/ci.yml`](.github/workflows/ci.yml), runs on every push and on PRs targeting `main`:

1. **Build, Test and Security Scan** — spins up a PostgreSQL 16 service container, restores and builds the solution in Release mode, applies EF Core migrations against it, runs the full test suite (unit + integration), and fails the build if `dotnet list package --vulnerable` finds any vulnerable dependency.
2. **Deploy to Azure App Service** *(main branch only, after tests pass)* — publishes the API, runs migrations against the production database, and deploys via `azure/webapps-deploy` to `fintrack-api`.
3. **Deploy React Frontend** *(main branch only, after tests pass)* — builds the Vite app against the production API URL and deploys to Azure Static Web Apps.

## Tests

56 tests total:
- **50/50 passing** — `FinTrack.Application.Tests` (unit tests, no external dependencies, run in ~1s)
- **6** — `FinTrack.Infrastructure.Tests` (integration tests against a real PostgreSQL instance; require `docker-compose up -d` or the CI Postgres service to run — all pass in CI)

```bash
dotnet test
```

## Future Improvements

- Production-grade Hangfire dashboard authentication (currently development-only)
- Automated refresh-token rotation ahead of TrueLayer's 90-day PSD2 re-authorisation window, with user notification
- Broader bank provider coverage beyond the current sandbox set
- Transaction search and CSV/PDF export
- Multi-currency support beyond GBP
- Containerise the API for consistent local/prod parity (currently Postgres-only via Docker)
- Expand integration test coverage beyond the repository layer
- Swagger UI is exposed unconditionally, including in production — intentional for this portfolio demo so the API surface is easy to explore; would be gated behind `IsDevelopment()` (or require auth) for a real production deployment
- `User.IsRefreshTokenValid` compares the SHA-256 refresh-token hash with `==` rather than a constant-time comparison. Low practical risk since it runs after an indexed DB lookup already narrowed to one row, but should use `CryptographicOperations.FixedTimeEquals` for defense-in-depth, consistent with `OAuthStateSigner.Verify`
- `BudgetsController.DeleteBudget` is a stub that returns `204 NoContent` without any deletion logic or ownership check — needs to be wired up to a real `DeleteBudgetCommand`/handler, or removed until implemented
