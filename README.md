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


## End-to-End (E2E) Testing Guide

This section explains how to validate LedgerFlow as a running system (API + PostgreSQL + Redis), not just unit tests.

### 1) Prerequisites
- Docker Desktop / Docker Engine + Compose v2
- `curl` (or Postman/Insomnia)
- Optional: `jq` for easier JSON parsing

### 2) Start the full stack
```bash
docker compose up --build -d
```

Check containers:
```bash
docker compose ps
```

Expected:
- `api` is running on `http://localhost:8080`
- `postgres` on `localhost:5432`
- `redis` on `localhost:6379`

### 3) Health-check the API
```bash
curl -s http://localhost:8080/health
```

Expected: JSON similar to
```json
{"status":"Healthy","utc":"..."}
```

### 4) Open Swagger for manual E2E calls
- URL: `http://localhost:8080/swagger`
- Use this for request/response inspection and quick endpoint exploration.

### 5) Obtain a JWT token for secured endpoints
Current scaffold does not yet include `/api/auth/login`, so for E2E testing `POST /api/transactions/transfer` you have two practical options:

#### Option A (recommended for current scaffold): temporary dev bypass
Temporarily remove `.RequireAuthorization()` from transfer endpoint in `Program.cs`, rebuild, and run E2E transfer payload tests.

#### Option B: generate a dev JWT matching appsettings
Generate a signed token using the configured `Jwt:Key`, `Jwt:Issuer`, and `Jwt:Audience`, then call the transfer endpoint with:
```http
Authorization: Bearer <token>
```

### 6) Prepare database state for transfer E2E
The transfer endpoint expects sender/receiver wallets to exist in PostgreSQL.

Connect to postgres container:
```bash
docker exec -it $(docker compose ps -q postgres) psql -U postgres -d ledgerflow
```

Create minimal tables and sample wallets (if migrations are not yet added):
```sql
CREATE TABLE IF NOT EXISTS wallets (
  id uuid PRIMARY KEY,
  userid uuid NOT NULL,
  currency varchar(3) NOT NULL,
  balance numeric(18,2) NOT NULL,
  status int NOT NULL,
  createdatutc timestamptz NOT NULL DEFAULT now()
);

CREATE TABLE IF NOT EXISTS transactions (
  id uuid PRIMARY KEY,
  sourcewalletid uuid NOT NULL,
  destinationwalletid uuid NOT NULL,
  amount numeric(18,2) NOT NULL,
  currency varchar(3) NOT NULL,
  type text NOT NULL,
  status text NOT NULL,
  createdatutc timestamptz NOT NULL DEFAULT now()
);

INSERT INTO wallets (id, userid, currency, balance, status)
VALUES
('11111111-1111-1111-1111-111111111111', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 'USD', 1000, 1),
('22222222-2222-2222-2222-222222222222', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', 'USD', 100, 1)
ON CONFLICT (id) DO NOTHING;
```

### 7) Execute transfer request (E2E)
```bash
curl -i -X POST http://localhost:8080/api/transactions/transfer   -H "Content-Type: application/json"   -H "Authorization: Bearer <token-or-use-dev-bypass>"   -d '{
    "sourceWalletId":"11111111-1111-1111-1111-111111111111",
    "destinationWalletId":"22222222-2222-2222-2222-222222222222",
    "amount":150,
    "currency":"USD",
    "idempotencyKey":"e2e-test-001",
    "userId":"aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"
  }'
```

Expected:
- HTTP `200 OK`
- response includes transaction id and status `Completed`

### 8) Verify DB side effects (critical E2E assertion)
Inside `psql`:
```sql
SELECT id, balance FROM wallets
WHERE id IN (
  '11111111-1111-1111-1111-111111111111',
  '22222222-2222-2222-2222-222222222222'
)
ORDER BY id;

SELECT id, sourcewalletid, destinationwalletid, amount, status
FROM transactions
ORDER BY createdatutc DESC
LIMIT 5;
```

Expected:
- sender wallet balance decreases by amount
- receiver wallet balance increases by amount
- a transaction row is inserted once per successful call

### 9) Negative-path E2E checks
Run these to validate consistency behavior:

1. **Insufficient balance**
   - Send amount higher than sender balance.
   - Expected: failure (non-200) and no partial balance update.

2. **Frozen wallet**
   - `UPDATE wallets SET status = 2 WHERE id = '<sender-or-receiver>';`
   - Expected: transfer blocked.

3. **Invalid payload**
   - amount <= 0 or same source/destination wallet.
   - Expected: validation problem response.

### 10) End the test session
```bash
docker compose down
```

To remove volumes and reset data:
```bash
docker compose down -v
```

### E2E Automation Next Step (recommended)
As the next maturity step, add an automated E2E test project that:
- boots dependencies via Testcontainers
- seeds wallets
- calls API via `HttpClient`
- asserts DB balances and transaction rows after each scenario



## Frontend Testing Guide (LedgerFlow Dashboard)

Use this checklist to test only the React frontend.

### 1) Prerequisites
- Node.js 20+ (22 recommended)
- npm
- LedgerFlow backend running at `http://localhost:8080` (or update `VITE_API_URL`)

### 2) Install dependencies
```bash
cd frontend
npm install
```

### 3) Run frontend locally
```bash
VITE_API_URL=http://localhost:8080 npm run dev
```

Open:
- `http://localhost:5173`

### 4) Manual smoke test flow
1. Open `/login`.
2. Validate form rules:
   - invalid email should show validation error
   - password under 8 chars should fail validation
3. Try login request:
   - with invalid credentials -> expect error toast
   - with valid backend response -> expect redirect to `/dashboard`
4. Validate route protection:
   - without token, opening `/dashboard` should redirect to `/login`
5. After login, verify pages render:
   - `/dashboard` (KPI cards + chart)
   - `/wallets`
   - `/transactions`

### 5) Production build test
```bash
npm run build
npm run preview
```
Open preview URL printed by Vite and verify routes/UI load.

### 6) Optional Docker test for frontend only
```bash
docker build -t ledgerflow-dashboard ./frontend
docker run --rm -p 3000:80 ledgerflow-dashboard
```
Open:
- `http://localhost:3000`

### 7) Quick troubleshooting
- If API calls fail, confirm backend is running and `VITE_API_URL` is correct.
- If blank page appears, check browser console for runtime errors.
- If dependencies fail to install, remove `node_modules` and lockfile, then reinstall.


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
