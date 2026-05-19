# AGENTS.md — DevBoard Project

> This file is the single source of truth for any AI agent, coding assistant, or automated tool working on this codebase.
> Read this entire file before making any changes.

---

## Project Overview

**Name:** DevBoard  
**Type:** Full-stack web application  
**Purpose:** A developer-focused task management platform with Kanban boards, real-time event messaging, and AI-powered task summaries.  
**Status:** In active development (learning project / portfolio)

---

## Tech Stack

### Backend
| Concern | Technology |
|---|---|
| Runtime | .NET 9 / C# 13 |
| Web framework | ASP.NET Core 9 (minimal APIs) |
| ORM | Entity Framework Core 9 (code-first) |
| Database | SQL Server 2022 |
| Auth | JWT Bearer tokens (ASP.NET Core Identity) |
| Messaging | Apache Kafka (Confluent.Kafka .NET client) |
| Validation | FluentValidation |
| Mediator | MediatR (CQRS pattern) |
| API docs | Scalar (replaces Swagger UI) |
| Mapping | AutoMapper |

### Frontend
| Concern | Technology |
|---|---|
| Framework | React 18 + TypeScript (Vite) |
| Styling | Tailwind CSS v3 |
| Server state | React Query (TanStack Query v5) |
| Client state | Zustand |
| Routing | React Router v6 |
| Drag & drop | dnd-kit |
| HTTP client | Axios (typed with TS interfaces) |

### Infrastructure
| Concern | Technology |
|---|---|
| Containerization | Docker + Docker Compose |
| CI/CD | GitHub Actions |
| Container registry | GitHub Container Registry (GHCR) |
| Integration tests | xUnit + TestContainers |
| Unit tests | xUnit + Moq |
| AI integration | Anthropic Claude API (claude-sonnet-4-20250514) |

---

## Solution Structure

```
DevBoard/
├── src/
│   ├── DevBoard.Domain/          # Entities, value objects, domain events. NO external dependencies.
│   ├── DevBoard.Application/     # CQRS commands/queries (MediatR), interfaces, DTOs, validators.
│   ├── DevBoard.Infrastructure/  # EF Core DbContext, repos, Kafka producer/consumer, external services.
│   └── DevBoard.API/             # ASP.NET Core minimal API endpoints, DI registration, middleware.
├── tests/
│   ├── DevBoard.UnitTests/       # Domain + Application layer tests. Mock all external deps.
│   └── DevBoard.IntegrationTests/# Full API tests using TestContainers (real SQL Server + Kafka).
├── frontend/                     # React + TypeScript (Vite) app.
├── docker-compose.yml            # Local dev: API + SQL Server + Kafka + Zookeeper.
├── docker-compose.test.yml       # CI test environment.
├── .github/workflows/            # GitHub Actions pipelines.
└── AGENTS.md                     # This file.
```

---

## Architecture Rules

These are hard rules. Do not violate them.

1. **Dependency direction:** `API → Application → Domain`. Infrastructure implements Application interfaces. Domain has zero external package references.
2. **No EF Core in Domain or Application.** DbContext and EF-specific types live only in Infrastructure.
3. **No business logic in API layer.** Endpoints dispatch MediatR commands/queries only.
4. **CQRS via MediatR.** Every operation is a `IRequest<T>` handler. No service classes that mix reads and writes.
5. **Repository pattern.** All data access goes through interfaces defined in Application, implemented in Infrastructure.
6. **Kafka events are side effects.** Publish domain events after a successful write — never inside the domain entity itself.
7. **All endpoints require auth** unless explicitly decorated with `[AllowAnonymous]`.

---

## Domain Model

### Core Entities

```csharp
// DevBoard.Domain/Entities/

User         { Id, Email, PasswordHash, DisplayName, CreatedAt }
Board        { Id, Name, Description, OwnerId, CreatedAt, Tasks }
TaskItem     { Id, BoardId, Title, Description, Status, Priority, AssigneeId, CreatedAt, UpdatedAt }
Notification { Id, UserId, Message, IsRead, CreatedAt }
```

### Enums

```csharp
TaskStatus   : Todo | InProgress | InReview | Done
TaskPriority : Low | Medium | High | Critical
```

### Domain Events (published to Kafka)

```
task.status.changed   → { TaskId, BoardId, OldStatus, NewStatus, ChangedBy, Timestamp }
task.assigned         → { TaskId, AssigneeId, AssignedBy, Timestamp }
board.created         → { BoardId, OwnerId, Timestamp }
```

---

## API Endpoints

Base path: `/api/v1`  
Auth: All routes require `Authorization: Bearer <token>` unless noted.

### Auth (no auth required)
```
POST /auth/register   → { email, password, displayName } → { token, userId }
POST /auth/login      → { email, password }              → { token, userId }
```

### Boards
```
GET    /boards            → List<BoardSummaryDto>
POST   /boards            → CreateBoardCommand → BoardDto
GET    /boards/{id}       → BoardDto (with tasks)
PUT    /boards/{id}       → UpdateBoardCommand → BoardDto
DELETE /boards/{id}       → 204
```

### Tasks
```
GET    /boards/{boardId}/tasks         → List<TaskDto>
POST   /boards/{boardId}/tasks         → CreateTaskCommand → TaskDto
GET    /boards/{boardId}/tasks/{id}    → TaskDto
PUT    /boards/{boardId}/tasks/{id}    → UpdateTaskCommand → TaskDto
PATCH  /boards/{boardId}/tasks/{id}/status → { status } → TaskDto
DELETE /boards/{boardId}/tasks/{id}    → 204
```

### Notifications
```
GET   /notifications        → List<NotificationDto>
PATCH /notifications/{id}/read → 204
```

### AI
```
POST /ai/boards/{boardId}/summarize   → streams text/event-stream (SSE)
POST /ai/boards/{boardId}/suggest     → List<TaskSuggestionDto>
```

---

## Database

- **Provider:** SQL Server 2022
- **Migrations:** EF Core code-first. Run `dotnet ef migrations add <Name>` from `DevBoard.Infrastructure`.
- **Connection string env var:** `ConnectionStrings__DefaultConnection`
- **Naming conventions:** Tables = PascalCase, columns = PascalCase. No snake_case in SQL.
- **Soft deletes:** Not implemented. Hard delete only for now.
- **Audit fields:** `CreatedAt` (UTC) on all entities. `UpdatedAt` (UTC) on `TaskItem`.

### Running migrations
```bash
dotnet ef migrations add <MigrationName> --project src/DevBoard.Infrastructure --startup-project src/DevBoard.API
dotnet ef database update --project src/DevBoard.Infrastructure --startup-project src/DevBoard.API
```

---

## Kafka

- **Broker:** `localhost:9092` (local), env var `Kafka__BootstrapServers` in production.
- **Topics:** Defined as constants in `DevBoard.Infrastructure/Messaging/KafkaTopics.cs`.
- **Producer:** Singleton registered in DI. Fire-and-forget after successful DB write.
- **Consumer:** Hosted service (`IHostedService`) in the API project. Processes notification events and writes to the `Notifications` table.
- **Message format:** JSON (System.Text.Json). No Avro/Schema Registry yet.
- **Consumer group:** `devboard-notification-consumer`

---

## Environment Variables

The app reads from environment or `appsettings.{Environment}.json`.  
Never commit secrets. Use `.env` locally (gitignored) and GitHub Secrets in CI.

```
# Database
ConnectionStrings__DefaultConnection=Server=localhost;Database=DevBoard;...

# Auth
Jwt__Secret=<min 32 char secret>
Jwt__Issuer=devboard-api
Jwt__Audience=devboard-client
Jwt__ExpiryHours=24

# Kafka
Kafka__BootstrapServers=localhost:9092

# AI
AI__Provider=Anthropic               # or OpenAI
AI__ApiKey=<your key>
AI__Model=claude-sonnet-4-20250514
```

---

## Running Locally

### Prerequisites
- .NET 9 SDK
- Node.js 20+
- Docker Desktop

### Start everything
```bash
# Start infrastructure (SQL Server + Kafka + Zookeeper)
docker compose up -d

# Apply migrations
dotnet ef database update --project src/DevBoard.Infrastructure --startup-project src/DevBoard.API

# Run API
cd src/DevBoard.API
dotnet run

# Run frontend (separate terminal)
cd frontend
npm install
npm run dev
```

API runs on `https://localhost:5001`  
Scalar docs: `https://localhost:5001/scalar/v1`  
Frontend: `http://localhost:5173`

---

## Testing

### Unit tests
```bash
dotnet test tests/DevBoard.UnitTests
```
- Test Application layer handlers with mocked repositories (Moq).
- Test Domain entity logic directly.
- No I/O, no network, no DB.

### Integration tests
```bash
dotnet test tests/DevBoard.IntegrationTests
```
- Uses TestContainers — spins up real SQL Server and Kafka containers automatically.
- Tests full HTTP request → DB flow via `WebApplicationFactory`.
- Requires Docker running locally.

### Coverage expectation
- Domain + Application: aim for >80% line coverage.
- Infrastructure: covered by integration tests, not unit tests.

---

## CI/CD Pipeline

File: `.github/workflows/ci.yml`

```
Trigger: push to main, pull_request to main

Jobs:
  1. build-and-test
     - dotnet build (Release)
     - dotnet test (UnitTests)
     - dotnet test (IntegrationTests) — needs Docker
  2. docker-build (on main only, after tests pass)
     - Build API Docker image
     - Push to ghcr.io/<owner>/devboard-api:latest
  3. deploy (on main only, manual approval gate)
     - Deploy to target environment
```

---

## Code Conventions

### C# / Backend
- Use `record` types for DTOs and CQRS commands/queries.
- Use `Result<T>` pattern (or `OneOf`) for application layer return types — no throwing exceptions for business rule failures.
- Async all the way down. Every I/O method is `async Task<T>`. No `.Result` or `.Wait()`.
- Use `CancellationToken` on every async public method.
- Minimal API endpoints registered in extension methods grouped by feature (not one giant `Program.cs`).
- No `var` for non-obvious types. Explicit types preferred.
- XML doc comments on public interfaces in Application layer only.

### TypeScript / Frontend
- Strict mode enabled (`"strict": true` in tsconfig).
- All API response types defined as TS interfaces in `frontend/src/types/api.ts`.
- No `any`. Use `unknown` and narrow if needed.
- React Query handles all server state. Zustand for UI-only state (modal open/close, drag state).
- Components are functional only. No class components.
- File naming: `PascalCase.tsx` for components, `camelCase.ts` for utilities and hooks.

---

## What AI Agents Should NOT Do

- Do not change the Clean Architecture layer boundaries.
- Do not add EF Core references to Domain or Application projects.
- Do not add business logic directly in API endpoint handlers.
- Do not use `Thread.Sleep` or synchronous blocking calls.
- Do not commit `.env` files, API keys, or connection strings.
- Do not create new npm packages without updating this file.
- Do not rename Kafka topic constants without updating all consumers and producers.
- Do not modify migration files that have already been applied — create a new migration instead.

---

## Useful Commands Reference

```bash
# Add a new EF migration
dotnet ef migrations add <Name> --project src/DevBoard.Infrastructure --startup-project src/DevBoard.API

# Scaffold a new minimal API endpoint group
# → Create src/DevBoard.API/Endpoints/<Feature>Endpoints.cs
# → Register in Program.cs via app.MapGroup("/api/v1").MapFeatureEndpoints()

# Run only a specific test class
dotnet test --filter "FullyQualifiedName~CreateTaskCommandHandlerTests"

# Rebuild Docker images after code change
docker compose up --build -d

# View Kafka messages (from inside the kafka container)
docker exec -it devboard-kafka kafka-console-consumer \
  --bootstrap-server localhost:9092 \
  --topic task.status.changed \
  --from-beginning
```

---

*Last updated: Week 1 of development. Update this file whenever the stack, schema, or architecture decisions change.*
