# Active Workout Classes

A full-stack fitness class management system built with **.NET 10**, **ASP.NET Core**, **Blazor WebAssembly**, and **.NET Aspire**. It supports three distinct roles — **Students**, **Instructors**, and **Admins** — each with a tailored dashboard and feature set.

---

## Features

### Student
- Browse and register for upcoming workout classes
- Self check-in within a 30-minute window via student number
- View personal class history, attendance records, and progress
- Track belt/skill-level progressions and grading results

### Instructor
- Role-specific dashboard with upcoming sessions and student counts
- Create and edit workout classes
- Manage recurring class schedules
- View student attendance per class

### Admin
- Full user management (create, activate/deactivate, view details)
- Location management (soft-delete support)
- Recurring schedule management with automatic class materialisation
- Grading event management with an eligibility engine (attendance thresholds, days-since-registration, etc.)
- Analytics dashboard — class popularity, attendance trends, membership stats

---

## Tech Stack

| Layer | Technology |
|---|---|
| Runtime | .NET 10 |
| API | ASP.NET Core REST + JWT Bearer auth |
| Frontend | Blazor WebAssembly (hosted model) |
| UI library | MudBlazor 9.x |
| ORM | Entity Framework Core 10 |
| Database | In-memory (dev) / SQL Server (prod) |
| Orchestration | .NET Aspire 13 |
| Auth | JWT + Blazored.LocalStorage |
| Password hashing | BCrypt.Net-Next |
| API docs | Swagger / Swashbuckle |

---

## Architecture

The solution follows a clean layered architecture:

```
Domain → Infrastructure → Application → ApiService
                                              ↑
Domain → Shared ──────────────────────────── Web (Blazor WASM)
```

| Project | Role |
|---|---|
| `Domain` | Pure entities, enums. No external dependencies. |
| `Infrastructure` | EF Core `ApplicationDbContext`, migrations, `DbSeeder`. |
| `Shared` | WASM-safe DTOs only — no EF Core, safe to reference from browser. |
| `Application` | Business-logic services (registration, grading, analytics, etc.). |
| `ApiService` | ASP.NET Core REST API. Hosts the Blazor WASM app via `UseBlazorFrameworkFiles`. |
| `Web` | Blazor WASM frontend (MudBlazor, JWT auth state, typed `ApiClient`). |
| `AppHost` | .NET Aspire orchestrator. Runs everything with one command. |
| `ServiceDefaults` | Shared Aspire config: OpenTelemetry, health checks. |

### Domain model highlights

- **User hierarchy (TPH):** `User` → `Student` (auto-generated student number `S-YYYY-XXXXX`) and `Instructor`.
- **WorkoutClass:** supports `RecurringSchedule` FK, computed `DurationMinutes`, and a `CanCheckIn` 30-minute business rule.
- **GradingEvent:** pluggable eligibility rules (min attendance, days since registration, days since last grading, classes of a given type).
- **AuditLog:** every eligibility override is recorded with old/new values and the acting user.

---

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [.NET Aspire workload](https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/setup-tooling)

```bash
dotnet workload install aspire
```

### Run the full application (recommended)

```bash
dotnet run --project ActiveWorkoutClasses.AppHost
```

The .NET Aspire dashboard opens at **https://localhost:17182** — it shows live logs, traces, and resource health.  
The application itself is served at the URL shown in the Aspire dashboard for `apiservice` (typically **https://localhost:7353**).

### Run the API only

```bash
dotnet run --project ActiveWorkoutClasses.ApiService
```

> The Blazor WASM frontend is hosted by `ApiService` (`UseBlazorFrameworkFiles`), so both the API and UI are served from the same process.

### Build

```bash
dotnet build
```

---

## Test Credentials

The application seeds demo data on first startup (in-memory database).

| Role | Email | Password |
|---|---|---|
| Admin | `admin@activeworkout.com` | `Password123!` |
| Instructor | `sarah.kravmaga@activeworkout.com` | `Password123!` |
| Student | `john.student@email.com` | `Password123!` |

---

## API Overview

Swagger UI is available at `/swagger` when the API is running.

| Controller | Base Route | Access |
|---|---|---|
| Auth | `/api/auth` | Login/Register public; rest requires auth |
| Classes | `/api/classes` | All authenticated users |
| Registrations | `/api/registrations` | All authenticated users |
| Attendance | `/api/attendance` | All authenticated; check-in by number is public |
| Users | `/api/users` | Admin only |
| Locations | `/api/locations` | Read: authenticated; Write: Admin only |
| Recurring Schedules | `/api/schedules` | Admin, Instructor |
| Grading Events | `/api/gradingevents` | Role-specific per action |
| Progress | `/api/progress` | Student (own), Admin/Instructor (any) |
| Analytics | `/api/analytics` | Role-specific per action |

---

## Pages & Routes

| Page | Route | Roles |
|---|---|---|
| Login | `/login` | Public |
| Register | `/register` | Public |
| Dashboard | `/` | All authenticated |
| Classes | `/classes` | All authenticated |
| Create Class | `/classes/create` | Admin, Instructor |
| Edit Class | `/classes/edit/{id}` | Admin, Instructor |
| My Classes | `/my-classes` | Student |
| Check In | `/check-in` | Student |
| My Progress | `/my-progress` | Student |
| My Gradings | `/my-gradings` | Student |
| Grading Events | `/grading-events` | Admin, Instructor |
| Users | `/admin/users` | Admin |
| User Detail | `/admin/users/{id}` | Admin |
| Locations | `/admin/locations` | Admin |
| Schedules | `/admin/schedules` | Admin, Instructor |

---

## Database Migrations

Migrations only apply to the SQL Server production target. In development the app uses an in-memory database.

```bash
# Add a migration
dotnet ef migrations add <MigrationName> \
  --project ActiveWorkoutClasses.Infrastructure \
  --startup-project ActiveWorkoutClasses.ApiService

# Apply migrations
dotnet ef database update \
  --project ActiveWorkoutClasses.Infrastructure \
  --startup-project ActiveWorkoutClasses.ApiService
```

---

## Supported Class Types

Krav Maga · Boxing · Brazilian Jiu-Jitsu · Yoga · Pilates · HIIT · CrossFit · Spinning · Zumba

---

## License

See [LICENSE](LICENSE).
