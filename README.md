# LedgerFlow — Production-Grade Fintech Wallet API

LedgerFlow is a production-oriented wallet backend designed to model how real fintech/payment systems are built: **clean architecture, secure auth, resilient distributed components, and transaction-safe money movement**.

---

## Table of Contents
1. [Vision](#vision)
2. [Core Engineering Philosophy](#core-engineering-philosophy)
3. [Architecture](#architecture)
4. [Tech Stack](#tech-stack)
5. [Current Project Structure](#current-project-structure)
6. [Domain Model](#domain-model)
7. [Implemented Features (Current)](#implemented-features-current)
8. [Planned Features (Roadmap)](#planned-features-roadmap)
9. [Money Transfer Consistency Design](#money-transfer-consistency-design)
10. [Security Design](#security-design)
11. [Caching, Locking, and Idempotency Strategy](#caching-locking-and-idempotency-strategy)
12. [Resiliency and Observability](#resiliency-and-observability)
13. [API Endpoints](#api-endpoints)
14. [Configuration](#configuration)
15. [Run with Docker](#run-with-docker)
16. [Run Locally (without Docker)](#run-locally-without-docker)
17. [Testing](#testing)
18. [Production Hardening Checklist](#production-hardening-checklist)
19. [Notes for Reviewers / Interviewers](#notes-for-reviewers--interviewers)
20. [License](#license)

---

## Vision

This project is intended to be portfolio- and interview-grade, not a beginner CRUD sample. It emphasizes:

- wallet/account consistency under concurrency
- secure authentication and authorization boundaries
- maintainable layered architecture
- hybrid data access patterns used in real systems
- operational readiness via logs, retries, and containerization

---

## Core Engineering Philosophy

LedgerFlow intentionally uses a **hybrid persistence strategy**:

- **EF Core** for productivity, readability, and maintainability in standard operations.
- **Raw SQL in explicit DB transactions** for critical money movement logic where atomicity and consistency guarantees are paramount.

> “Use EF Core for productivity and maintainability, while leveraging raw SQL transactions for critical financial consistency operations.”

---

## Architecture

Clean Architecture with clear layer separation:

- **LedgerFlow.Domain**
  - pure business entities/enums/interfaces
  - no EF Core and no infrastructure concerns

- **LedgerFlow.Application**
  - use cases (CQRS), validators, DTOs, application abstractions
  - orchestration-facing business logic contracts

- **LedgerFlow.Infrastructure**
  - JWT token generation
  - resiliency helpers (Polly pipeline)
  - infrastructure DI composition

- **LedgerFlow.Persistence**
  - EF Core DbContext + entity mapping
  - PostgreSQL integrations
  - raw SQL transfer consistency service

- **LedgerFlow.API**
  - HTTP entrypoint
  - authentication/authorization middleware
  - endpoint mapping
  - Swagger/OpenAPI exposure

---

## Tech Stack

### Backend
- ASP.NET Core Web API (.NET 10 preview target)

### Data
- PostgreSQL
- EF Core
- Dapper (for raw SQL execution)

### Distributed/Performance
- Redis (compose-ready)

### Security
- JWT bearer auth (access-token path scaffolded)

### Quality/Architecture
- MediatR
- FluentValidation
- Polly
- Serilog

### Packaging
- Docker + Docker Compose

### Testing
- xUnit

---

## Current Project Structure

```text
.
├── LedgerFlow.sln
├── Directory.Build.props
├── Dockerfile
├── docker-compose.yml
├── src
│   ├── LedgerFlow.API
│   ├── LedgerFlow.Application
│   ├── LedgerFlow.Domain
│   ├── LedgerFlow.Infrastructure
│   └── LedgerFlow.Persistence
└── tests
    └── LedgerFlow.Tests
```

---

## Domain Model

### Wallet
- `Id`
- `UserId`
- `Currency`
- `Balance`
- `Status` (`Active`, `Frozen`, `Closed`)
- `CreatedAtUtc`

### Transaction
- `Id`
- `SourceWalletId`
- `DestinationWalletId`
- `Amount`
- `Currency`
- `Type`
- `Status`
- `CreatedAtUtc`

These entities are intentionally in Domain to keep business structures free from infrastructure concerns.

---

## Implemented Features (Current)

### API and Pipeline
- Serilog host wiring
- Swagger/OpenAPI setup
- JWT bearer authentication + authorization middleware
- health endpoint: `GET /health`
- secured transfer endpoint: `POST /api/transactions/transfer`

### Application Layer
- transfer request/response DTOs
- transfer command + handler (MediatR)
- transfer validator (FluentValidation)
- transfer consistency abstraction contract

### Persistence Layer
- EF Core `LedgerFlowDbContext` for wallet/transaction mappings
- raw SQL transactional transfer service with rollback on consistency failures

### DevOps
- dockerized API container
- `docker-compose` stack for API + PostgreSQL + Redis

### Tests
- initial validation test for transfer request rules

---

## Planned Features (Roadmap)

1. Authentication module:
   - register/login/refresh endpoints
   - refresh token rotation and revocation
2. Wallet module:
   - create wallet, get wallet, freeze wallet, status transitions
3. Transactions module expansion:
   - deposit/withdraw/history endpoints
4. Idempotency:
   - persistence-backed idempotency keys and replay-safe responses
5. Distributed locking:
   - Redis lock per wallet pair for transfer contention control
6. Audit logging:
   - immutable audit trail for sensitive operations
7. Rate limiting:
   - transfer throttling strategy
8. Outbox/Background processing:
   - reliable event publication and retry workers

---

## Money Transfer Consistency Design

Current transfer consistency path uses:

1. open PostgreSQL connection
2. begin transaction
3. execute raw SQL statements atomically:
   - debit sender with balance guard
   - credit receiver
   - insert transaction record
4. verify all expected modifications occurred
5. rollback on failure, commit on success

This prevents partially-applied transfer state and demonstrates production-style consistency controls.

---

## Security Design

Current scaffold includes:

- JWT bearer middleware
- issuer/audience/signature/lifetime checks
- authorization requirement on transfer endpoint

Planned hardening:

- refresh token rotation
- stronger key management via secret stores
- key rollover support
- role/permission policies
- endpoint-level least privilege

---

## Caching, Locking, and Idempotency Strategy

Redis is provisioned in compose and reserved for:

- wallet balance cache keys (e.g., `wallet:balance:{walletId}`)
- idempotency keys (e.g., `idempotency:{requestId}`)
- distributed transfer locks
- transfer endpoint rate limiting windows

Current repository includes the architectural foundation; full Redis-backed logic is a roadmap item.

---

## Resiliency and Observability

- **Polly** retry pipeline scaffolding is registered in Infrastructure.
- **Serilog** is integrated at host startup and configured via appsettings.
- health endpoint allows baseline probe integration.

Planned:
- structured domain event logging
- correlation-id propagation
- metrics/tracing (OpenTelemetry)
- failure dashboards and alerting hooks

---

## API Endpoints

### Implemented now
- `GET /health`
- `POST /api/transactions/transfer` (authorized)

### Target endpoints
- `POST /api/auth/register`
- `POST /api/auth/login`
- `POST /api/auth/refresh`
- `POST /api/wallets`
- `GET /api/wallets/{id}`
- `POST /api/wallets/{id}/freeze`
- `POST /api/transactions/deposit`
- `POST /api/transactions/withdraw`
- `GET /api/transactions/history`

---

## Configuration

`src/LedgerFlow.API/appsettings.json` currently includes:

- `ConnectionStrings:Postgres`
- `Jwt:Key`
- `Jwt:Issuer`
- `Jwt:Audience`
- Serilog minimum settings

For real production usage, move secrets to managed secret stores and rotate credentials regularly.

---

## Run with Docker

```bash
docker compose up --build
```

Services:
- API on `http://localhost:8080`
- PostgreSQL on `localhost:5432`
- Redis on `localhost:6379`

---

## Run Locally (without Docker)

Prerequisites:
- .NET SDK compatible with target framework
- PostgreSQL
- Redis

Steps:

```bash
dotnet restore LedgerFlow.sln
dotnet build LedgerFlow.sln
dotnet run --project src/LedgerFlow.API/LedgerFlow.API.csproj
```

---

## Testing

```bash
dotnet test LedgerFlow.sln
```

Current automated coverage is intentionally small and focused on early validator behavior; test surface should grow with feature depth (transfer consistency integration tests, auth tests, concurrency tests, and idempotency tests).

---

## Production Hardening Checklist

- [ ] Replace dev JWT key with secret-managed key material
- [ ] Add refresh token persistence and rotation
- [ ] Add transfer idempotency persistence and replay response path
- [ ] Add Redis distributed locking around transfer critical section
- [ ] Add audit log entity + write path
- [ ] Add DB migrations and schema evolution flow
- [ ] Add CI/CD (build/test/lint/security scan)
- [ ] Add API versioning strategy
- [ ] Add observability: metrics + traces + alerting
- [ ] Add performance/load tests for transfer throughput and contention

---

## Notes for Reviewers / Interviewers

When discussing design, highlight:

- Why hybrid EF Core + raw SQL is used.
- How transaction boundaries prevent partial financial updates.
- How planned idempotency + locking will prevent duplicate or concurrent double-spend scenarios.
- How the layer boundaries support long-term maintainability in larger teams.

---

## License

This project is currently unlicensed. Add an explicit LICENSE file before public distribution.
