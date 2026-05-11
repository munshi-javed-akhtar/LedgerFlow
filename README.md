# LedgerFlow

Production-grade fintech wallet API using Clean Architecture with a hybrid consistency strategy:

- EF Core for general data access and maintainability.
- Raw SQL transaction script for critical money transfer consistency.

## Stack
- .NET 10 Web API
- PostgreSQL + EF Core + Dapper
- Redis
- JWT auth + refresh-token friendly architecture
- FluentValidation + MediatR + Polly + Serilog
- Docker / docker-compose
- xUnit

## Run
```bash
docker compose up --build
```
