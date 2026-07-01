# FinTrack

A personal finance web application connecting to UK bank accounts via Open Banking (TrueLayer), built with .NET 10, Clean Architecture, and CQRS.

## Tech Stack

**Backend:** .NET 10, ASP.NET Core Web API, MediatR, Entity Framework Core, PostgreSQL, Hangfire, Polly, Serilog  
**Frontend:** React, TypeScript, Recharts, Tailwind CSS  
**Infrastructure:** Docker, Azure, GitHub Actions CI/CD  
**Open Banking:** TrueLayer (Bank of Scotland, Monzo)

## Architecture

Clean Architecture with four layers:
- **Domain** — entities, enums, domain exceptions. Zero external dependencies.
- **Application** — use cases, CQRS commands/queries, MediatR handlers, FluentValidation
- **Infrastructure** — EF Core, TrueLayer client, Hangfire jobs, Polly resilience
- **API** — ASP.NET Core controllers, middleware, JWT auth

## Architecture Decision Records

## Architecture Decision Records

- [ADR-001: Use TrueLayer as Open Banking aggregator](docs/adr/ADR-001-truelayer-open-banking.md)
- [ADR-002: Store TrueLayer tokens encrypted in PostgreSQL](docs/adr/ADR-002-token-storage-strategy.md)

## Local Development

**Prerequisites:** .NET 10 SDK, Docker Desktop

```bash
# Start PostgreSQL and pgAdmin
docker-compose up -d

# Build the solution
dotnet build

# Run the API
dotnet run --project src/FinTrack.API
```

pgAdmin available at http://localhost:5050

## Project Status

🚧 In active development
