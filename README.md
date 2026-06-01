# Novibet Wallet System

A modular wallet management system built with ASP.NET Core, supporting multi-currency wallets, transactions, background processing, and test coverage.

---


## Solution Structure

Novibet.Api        → REST API (presentation layer)

Novibet.Data       → Database layer (EF Core, entities, DbContext)

Novibet.Domain     → Core domain models, DTOs, enums, mappers

Novibet.Worker     → Background services / scheduled jobs

Novibet.Tests      → Unit & integration tests

Novibet.XML        → XML documentation output (build artifacts)

---

## Running the Solution

This project is fully containerized and can be run using Docker Compose, which orchestrates all services including API, Worker, Database and Cache.

```bash
docker-compose up --build
```

This will start:

Novibet.Api → REST API service

Novibet.Worker → Background processing service

Database service -> Postgres

Cache service -> Redis