# Project & Resource Management (PRM) Tool — Project Context

> **Purpose of this document:** Single source of truth for architecture, conventions, and task workflow.
> Reference this file (`@PROJECT_CONTEXT.md`) in every request to avoid re-explaining context.

---

## 1. Project Summary

A **client-server console application** for managing employees, projects, resource allocations, timesheets, and AI-powered skill matching / risk analysis in an IT services company.

| Aspect            | Detail                                                              |
| ----------------- | ------------------------------------------------------------------- |
| **Client**        | .NET Console Application (interactive menus)                        |
| **Server**        | ASP.NET Core Web API (REST)                                        |
| **Database**      | Microsoft SQL Server (MSSQL)                                       |
| **AI Integration**| LLM via REST (Google Gemini / Groq) — configurable at runtime      |
| **Auth**          | JWT-based authentication (force password change on first login)     |
| **Scheduler**     | Background hosted service (utilisation computation, health flags)   |
| **Target**        | .NET 8 (LTS)                                                       |

### User Roles

| Role       | Key Capabilities                                                                |
| ---------- | ------------------------------------------------------------------------------- |
| **Admin**  | Manage users, employees, projects, milestones, skills, allocations view, config |
| **Manager**| Resource dashboard, allocate/deallocate, my projects, timesheets (read), AI     |
| **Employee**| Submit/view timesheets, view own allocations                                   |

### Key Business Rules (Quick Reference)

- Total utilisation across overlapping allocations **cannot exceed 100%**
- Timesheet hours per project ≤ `allocation% × max_weekly_hours`
- Total weekly hours ≤ `max_weekly_hours` (default 40, configurable)
- No duplicate timesheets for same employee + week
- No future-week timesheet submission
- Deactivation preserves all historical data; ends active allocations immediately
- First admin bootstrapped via DB seed script (default: `admin` / `Admin@1234`)
- `force_password_change` flag enforced before any menu access

---

## 2. Tech Stack

| Layer              | Technology                          | Version  |
| ------------------ | ----------------------------------- | -------- |
| Runtime            | .NET                                | 8.0 LTS  |
| Backend API        | ASP.NET Core Web API                | 8.x      |
| Console Client     | .NET Console App (`System.Console`) | 8.x      |
| ORM                | Entity Framework Core               | 8.x      |
| Database           | Microsoft SQL Server                | 2019+    |
| Authentication     | JWT Bearer Tokens                   | —        |
| HTTP Client        | `HttpClient` / `IHttpClientFactory` | Built-in |
| LLM Integration    | REST HTTP calls (Gemini / Groq)     | —        |
| Background Jobs    | `IHostedService` / `BackgroundService` | Built-in |
| Logging            | `Microsoft.Extensions.Logging` + Serilog | —    |
| Testing            | xUnit + Moq + FluentAssertions      | —        |
| Migrations         | EF Core Migrations                  | —        |

---

## 3. Solution Structure

```
ProjectResourceManager/
│
├── ProjectResourceManager.sln
│
├── src/
│   ├── PRM.API/                          # ASP.NET Core Web API (Server)
│   │   ├── Controllers/                  # API Controllers (thin — delegate to services)
│   │   ├── Middleware/                    # Auth middleware, exception handler, logging
│   │   ├── Filters/                      # Action/exception filters
│   │   ├── Program.cs                    # Host builder, DI registration, pipeline
│   │   └── appsettings.json              # Connection strings, JWT config, LLM config
│   │
│   ├── PRM.Console/                      # .NET Console Application (Client)
│   │   ├── Screens/                      # One class per screen/menu (Admin, Manager, Employee)
│   │   │   ├── LoginScreen.cs
│   │   │   ├── Admin/
│   │   │   ├── Manager/
│   │   │   └── Employee/
│   │   ├── Services/                     # ApiClient wrapper, input helpers
│   │   ├── Models/                       # DTOs for API communication
│   │   ├── Helpers/                      # Console UI utilities (borders, tables, colors)
│   │   └── Program.cs                    # Entry point, DI setup
│   │
│   ├── PRM.Core/                         # Domain Layer (shared)
│   │   ├── Entities/                     # Domain models (Employee, Project, Allocation, etc.)
│   │   ├── Enums/                        # Role, Status, HealthStatus, SkillCategory, etc.
│   │   ├── Interfaces/                   # Service & Repository contracts
│   │   ├── Exceptions/                   # Custom domain exceptions
│   │   └── Constants/                    # App-wide constants (no magic numbers)
│   │
│   ├── PRM.Application/                  # Application / Business Logic Layer
│   │   ├── Services/                     # Business logic implementations
│   │   ├── DTOs/                         # Request/Response DTOs
│   │   ├── Validators/                   # Input validation (FluentValidation or manual)
│   │   ├── Mappers/                      # Entity ↔ DTO mapping
│   │   └── AI/                           # LLM prompt builders, response parsers
│   │
│   ├── PRM.Infrastructure/              # Infrastructure Layer
│   │   ├── Data/                         # EF DbContext, configurations, migrations
│   │   │   ├── PrmDbContext.cs
│   │   │   ├── Configurations/           # IEntityTypeConfiguration<T> per entity
│   │   │   ├── Migrations/
│   │   │   └── Seed/                     # Admin bootstrap seed script
│   │   ├── Repositories/                 # Repository implementations
│   │   ├── ExternalServices/             # LLM API client adapters
│   │   └── BackgroundJobs/              # Hosted services (scheduler)
│   │
│   └── PRM.Shared/                       # Cross-cutting utilities (optional)
│       ├── Extensions/                   # Extension methods
│       └── Helpers/                      # Shared helpers (date utils, password hashing)
│
├── tests/
│   ├── PRM.UnitTests/                    # Unit tests (services, validators, mappers)
│   └── PRM.IntegrationTests/            # Integration tests (API endpoints, DB)
│
├── docs/                                 # Detailed documentation
│   ├── database-schema.md               # Full DB schema design
│   ├── api-contracts.md                  # REST API endpoints, DTOs, status codes
│   ├── ai-integration.md               # LLM prompts, parsing, provider abstraction
│   ├── scheduler-design.md             # Background job logic
│   ├── auth-flow.md                     # JWT auth, force password change flow
│   ├── screen-mapping.md               # Console screen → API mapping
│   ├── business-rules.md               # All validation & business rules consolidated
│   └── design-patterns.md              # SOLID, patterns, principles documentation
│
├── PROJECT_CONTEXT.md                    # ← This file
├── PRM_BRD_V4.md                         # Business Requirements Document
└── README.md                             # Setup, build, run instructions
```

### Layer Dependency Rules

```
PRM.Console ──→ PRM.Application (DTOs only) ──→ PRM.Core
                                                    ↑
PRM.API ──→ PRM.Application ──→ PRM.Core            │
                                                    │
PRM.Infrastructure ──→ PRM.Core ◄───────────────────┘
```

- **PRM.Core** depends on nothing. Pure domain.
- **PRM.Application** depends on PRM.Core only.
- **PRM.Infrastructure** depends on PRM.Core (implements interfaces).
- **PRM.API** depends on PRM.Application and registers PRM.Infrastructure via DI.
- **PRM.Console** depends on PRM.Application DTOs only (communicates via HTTP).

---

## 4. Architecture Decisions

> These are final decisions — not discussions. Each entry records the choice and the rationale.

### AD-01: Clean Architecture (Onion/Hexagonal)

**Decision:** Use Clean Architecture with four layers: Core → Application → Infrastructure → Presentation (API + Console).
**Rationale:** BRD requires demonstrable SOLID principles and Separation of Concerns. Clean Architecture enforces dependency inversion naturally. The domain layer has zero framework dependencies.

### AD-02: Separate Console Client from API Server

**Decision:** Console app is a standalone project that communicates with the API server over HTTP only. No direct DB access from Console.
**Rationale:** BRD specifies "client-server" architecture. Decoupling allows independent deployment and potential future UI (web/desktop) without server changes.
**Addendum (AD-02):** Server and Console live in the same git repository under `server/` and `client/` folders respectively, each with their own `.sln` file. The Console has zero compile-time project references to any server project. It owns local mirror models for API contracts.

### AD-03: Repository Pattern

**Decision:** Use the Repository pattern with interfaces in PRM.Core and implementations in PRM.Infrastructure.
**Rationale:** BRD requires at least one design pattern to be documented. Repository abstracts data access, enables unit testing with mocks, and satisfies DIP (Dependency Inversion Principle).

### AD-04: Strategy Pattern for LLM Providers

**Decision:** Define an `ILlmProvider` interface with `GeminiProvider` and `GroqProvider` implementations. Provider is selected at runtime via configuration.
**Rationale:** BRD requires configurable LLM provider (Gemini/Groq). Strategy pattern allows runtime switching without code changes. Satisfies OCP (Open-Closed Principle).

### AD-05: JWT Authentication

**Decision:** Server issues JWT tokens on login. Console stores token in memory for session. Token includes role claim for authorization.
**Rationale:** Stateless auth fits REST API design. Role-based authorization maps directly to Admin/Manager/Employee menus.

### AD-06: EF Core with Code-First Migrations

**Decision:** Use Entity Framework Core with code-first approach and migrations.
**Rationale:** Strongly typed, LINQ-based queries. Migrations provide versioned, repeatable schema evolution. Widely supported with MSSQL.

### AD-07: Background Scheduler as Hosted Service

**Decision:** Use `BackgroundService` (IHostedService) within the API project for periodic tasks (utilisation computation, project health flagging, missed timesheet detection).
**Rationale:** No need for external job scheduler. Built into ASP.NET Core. Interval is configurable via System Configuration.

### AD-08: Centralized Error Handling

**Decision:** Global exception handling middleware in API. Custom domain exceptions thrown from Application layer. Console displays user-friendly error messages from API error responses.
**Rationale:** Consistent error format. Prevents leaking stack traces. Follows Fail-Fast principle.

### AD-09: Database Seed for First Admin

**Decision:** Include a data seed in EF Core migrations that inserts the first Admin user with default credentials (`admin` / `Admin@1234`) and `force_password_change = true`.
**Rationale:** BRD explicitly requires bootstrap admin via DB seed script.

### AD-10: Single Solution, Multiple Projects

**Decision:** One `.sln` file containing all projects (API, Console, Core, Application, Infrastructure, Tests).
**Rationale:** Simplifies build, IDE navigation, and CI/CD. All code in one repository.

---

## 5. Coding Conventions

### 5.1 Naming

| Element             | Convention              | Example                           |
| ------------------- | ----------------------- | --------------------------------- |
| Solution/Projects   | PascalCase              | `PRM.API`, `PRM.Core`             |
| Namespaces          | PascalCase              | `PRM.Core.Entities`               |
| Classes / Records   | PascalCase              | `EmployeeService`, `CreateUserDto`|
| Interfaces          | `I` + PascalCase        | `IEmployeeRepository`             |
| Methods             | PascalCase              | `GetAvailableEmployeesAsync()`    |
| Properties          | PascalCase              | `FullName`, `IsActive`            |
| Private fields      | `_camelCase`            | `_employeeRepository`             |
| Local variables     | camelCase               | `weekStart`, `totalHours`         |
| Constants           | PascalCase              | `MaxWeeklyHours`, `DefaultRole`   |
| Enums               | PascalCase (singular)   | `EmployeeStatus.Allocated`        |
| Async methods       | Suffix `Async`          | `CreateUserAsync()`               |
| DTOs                | Suffix `Dto`/`Request`/`Response` | `CreateProjectRequest`  |
| DB columns          | snake_case              | `employee_id`, `is_active`        |

### 5.2 Clean Code Rules

1. **Meaningful names** — No abbreviations unless universally understood (ID, API, DTO, LLM). Variable name reveals intent.
2. **Small focused functions** — Each method does ONE thing. Max ~30 lines as a guideline.
3. **No magic numbers** — All constants defined in `PRM.Core.Constants` or as `const`/`enum`.
4. **No dead code** — No commented-out code committed. Use Git history.
5. **Single Responsibility** — Each class has one reason to change. Controllers are thin.
6. **Dependency Injection** — All dependencies injected via constructor. No `new` for services.
7. **Fail Fast** — Validate inputs at the boundary (API entry / Console input). Throw early with descriptive exceptions.
8. **Guard Clauses** — Check preconditions at top of methods. Reduce nesting.
9. **Async all the way** — All I/O operations (DB, HTTP, LLM) use `async/await`. No `.Result` or `.Wait()`.
10. **Immutable DTOs** — Use `record` types for DTOs where possible.
11. **No logic in Controllers** — Controllers validate request format, call service, return response. Period.
12. **Consistent error responses** — Standard `ApiErrorResponse` DTO with `StatusCode`, `Message`, `Errors[]`.

### 5.3 Required SOLID Demonstrations

| Principle | Where Applied |
| --------- | ------------- |
| **SRP**   | Controllers (HTTP concerns only), Services (business logic only), Repositories (data access only) |
| **OCP**   | `ILlmProvider` → add new providers without modifying existing code |
| **LSP**   | All repository implementations are interchangeable via `IRepository<T>` |
| **ISP**   | Separate `IEmployeeReadRepository` / `IEmployeeWriteRepository` if needed, role-specific service interfaces |
| **DIP**   | Core defines interfaces; Infrastructure provides implementations; wired via DI |

### 5.4 Required Design Patterns

| Pattern      | Usage                                                        |
| ------------ | ------------------------------------------------------------ |
| **Repository**| Data access abstraction for all entities                    |
| **Strategy** | LLM provider selection (Gemini/Groq) at runtime             |
| **Factory**  | Screen creation in Console based on user role (optional)     |

### 5.5 Required Design Principles

| Principle               | Application                                              |
| ----------------------- | -------------------------------------------------------- |
| **DRY**                 | Shared validation logic, common DTOs, extension methods  |
| **Separation of Concerns** | Clean Architecture layers, screen vs. API vs. data  |
| **YAGNI**               | Build only what BRD specifies — no speculative features  |
| **Fail Fast**           | Input validation at boundaries, early exceptions         |

### 5.6 File & Code Organization

- **One class per file** (except nested private classes).
- **File name = Class name** (e.g., `EmployeeService.cs` contains `EmployeeService`).
- **Regions are banned** — use separate files instead.
- **Usings** — `global using` in a `GlobalUsings.cs` per project for common namespaces.
- **XML doc comments** on all public APIs (interfaces, service methods, controller actions).

---

## 6. Document Index

> Detailed docs live in `/docs/`. This section is a **pointer index** — not the docs themselves.
> Create each doc when implementation reaches that area.

| # | Document                      | Path                      | Covers                                                           | Status  |
|---|-------------------------------|---------------------------|------------------------------------------------------------------|---------|
| 1 | Database Schema               | `docs/database-schema.md` | All tables, columns, types, keys, indexes, relationships         | Pending |
| 2 | API Contracts                 | `docs/api-contracts.md`   | All endpoints, HTTP methods, request/response DTOs, status codes | Pending |
| 3 | AI Integration                | `docs/ai-integration.md`  | LLM prompt templates, provider abstraction, response parsing     | Pending |
| 4 | Scheduler Design              | `docs/scheduler-design.md`| Background job logic, interval config, computed fields            | Pending |
| 5 | Auth Flow                     | `docs/auth-flow.md`       | JWT auth, force password change, role-based access          | Done    |
| 6 | Screen → API Mapping          | `docs/screen-mapping.md`  | Each console screen mapped to API endpoints it calls              | Pending |
| 7 | Business Rules                | `docs/business-rules.md`  | All validation rules, allocation constraints, timesheet rules     | Pending |
| 8 | Design Patterns & Principles  | `docs/design-patterns.md` | SOLID examples, pattern usage, principle demonstrations           | Pending |
| 9 | BRD (Requirements)            | `PRM_BRD_V4.md`           | Full business requirements document (source of truth for *what*)  | Done    |
| 10| Project Context (This File)   | `PROJECT_CONTEXT.md`      | Tech stack, conventions, architecture, task guidelines            | Done    |

---

## 7. Task Guidelines

> **These rules govern every implementation request.** Reference this section in all prompts.

### Guideline 1: Always Plan Before Implementing

- **Before any implementation**, create a `plan.md` file that describes:
  - What will be built/changed
  - Which files will be created/modified
  - Database changes (if any)
  - API endpoints affected
  - Dependencies on other components
  - Acceptance criteria
- **Do NOT start writing code** until I explicitly approve the plan.
- If the plan needs revisions, update `plan.md` and re-request approval.

### Guideline 2: Clarify Before Assuming

- If the context is **not 100% clear**, ask as many questions as needed.
- **Never assume** business logic, data types, validation rules, or UI behavior.
- Refer back to `PRM_BRD_V4.md` for business requirements.
- Refer back to this file (`PROJECT_CONTEXT.md`) for technical decisions.
- Keep asking until every ambiguity is resolved.

### Guideline 3: Minimal Blast Radius

- Only modify files and areas **directly related** to the requested task.
- If a change **requires touching** another area (e.g., shared interface, database migration, existing service):
  - **Stop and ask first** before making changes to unrelated areas.
  - Explain why the adjacent change is necessary.
  - Get explicit approval before proceeding.
- Never refactor, rename, or restructure code outside the task scope.

---

## 8. Quick Reference — Entity Summary

> High-level entity list derived from BRD. Full schema goes in `docs/database-schema.md`.

| Entity          | Key Fields                                                             |
| --------------- | ---------------------------------------------------------------------- |
| **User**        | Id, Username, Email, PasswordHash, Role, IsActive, ForcePasswordChange |
| **Employee**    | Id, UserId (FK), FullName, Department, Status, ManagerId, IsActive     |
| **Skill**       | Id, Name, Category                                                     |
| **EmployeeSkill**| EmployeeId, SkillId, ProficiencyLevel                                |
| **Project**     | Id, Name, Description, StartDate, EndDate, Status, ManagerId, TotalStoryPoints |
| **Milestone**   | Id, ProjectId, Title, DueDate, StoryPoints, Status                     |
| **Allocation**  | Id, EmployeeId, ProjectId, UtilisationPercent, FromDate, ToDate        |
| **Timesheet**   | Id, EmployeeId, WeekStartDate, Status                                  |
| **TimesheetEntry**| Id, TimesheetId, ProjectId, HoursWorked, ActivityTags                |
| **SystemConfig**| Key, Value (LLM provider, API key, scheduler interval, max hours)      |

---

## 9. How to Use This Document

### When starting a new task:
```
@PROJECT_CONTEXT.md  
Task: [describe what you need]
```

### When referencing specific BRD screens:
```
@PROJECT_CONTEXT.md @PRM_BRD_V4.md  
Task: Implement Screen 4.2 — Allocate Resource
```

### Minimum context for any request:
- Reference `@PROJECT_CONTEXT.md` (this file)
- State the task clearly
- Mention which BRD screen(s) are relevant (if applicable)

This avoids re-explaining the tech stack, architecture, and conventions every time.

---

*Last updated: 2026-06-10*
