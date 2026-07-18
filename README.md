# Ballastlane To-Do

A full-stack **To-Do** application built as a technical assessment. It demonstrates an
**authenticated, per-user CRUD workflow** across a **.NET 10 Minimal API** (Clean
Architecture + TDD, ASP.NET Core Identity with bearer tokens) and an **Angular 21**
single-page app (Angular Material + Reactive Forms, login + interceptor + route guard),
backed by an **in-memory database** for zero-config setup.

---

## Table of contents

1. [Generative AI workflow](#1-generative-ai-workflow)
2. [Tech stack](#2-tech-stack)
3. [Architecture](#3-architecture)
4. [Functionality](#4-functionality)
5. [Getting started](#5-getting-started)
6. [Testing](#6-testing)
7. [API reference](#7-api-reference)
8. [Project structure](#8-project-structure)

---

## 1. Generative AI workflow

This project was built with **Claude Code** as the GenAI pair-programmer. This section
documents the prompts used, how the AI output was validated, what was corrected or
improved along the way, and how edge cases / authentication / validation were handled —
as requested by the exercise.

### 1.1 Prompt #1 — project scaffold (reproduced verbatim)

```text
I need to create a ToDo application additional to that I need that you documented the first promp (this one) in the ReadMe.md file

# Requeriments
- We need a ToDO application to allow us apply a CRUD operations
- Create an interactive UI design, I'm presenting a test for a position. Let's imprisve them
- Application need to be implemented with .Net 10 and Angular (last version)
- We are going to use InMemory databse to make the development easy

# Architecture
- We need to apply Clean Architecture and TDD as core
- We are goign to implement this with Minimal API, FluentValidation and Global Handling Error
- Try to use the last version of C# where we create a file(s) as scripts
- Make a correct layer separation
- Angular will use ReactiveForms and Angular Material to present and validate the form

## Functionallity
- User can create a To-Do
- User will see all To-Dos
- User can delete, modify and see details of  the To-Do
- Details view is the only one that display de description

## Documentation
- The initially prompt
- Document the stack and architecture used for this aplication
- Document how to start the application
```

This produced the full first iteration: the four Clean Architecture projects, the
Minimal API CRUD endpoints, FluentValidation + global error handling, the test suite,
and the Angular app.

### 1.2 Prompt #2 — authentication & user ownership

After comparing the first iteration against the exercise requirements, a gap analysis
(also run with the AI) showed the **user / login requirement** was missing. That drove
the second prompt:

```text
I need to implement the whole login and authentication process in this project.

- For that we must create a user that will be in the project seed, with username (email) and password
- The To-Dos must be related to the user
```

This produced: ASP.NET Core **Identity** with token-based auth (`/api/auth/register`,
`/api/auth/login`, …), a **seeded demo user** (`demo@ballastlane.com` / `Passw0rd!`,
overridable via `SeedUser:*` configuration), the `UserId` ownership field on `TodoItem`,
per-user filtering in the repository, and the Angular login page + HTTP interceptor +
route guard.

### 1.3 Prompt #3 — task statuses & due date

The exercise's GenAI scenario describes tasks with a `status` and a `due_date`, while
the first iteration only had a boolean `IsCompleted`. This prompt aligned the model
with that richer shape:

```text
Help me improve my application regarding task statuses. For that we need to do the following:
  1. Statuses must be an enumeration: Pending / InProgress / Done
  2. For those statuses to make sense, tasks must have a DueDate field
```

This produced: a `TodoStatus` enum (`Pending` / `InProgress` / `Done`) in the Domain
layer replacing the boolean flag, an optional `DueDate` on `TodoItem`, updated DTOs /
validators / repository / tests across every layer, and the Angular side (status select
in the form, status chips + due date in the list and detail views).

### 1.4 How the AI's suggestions were validated

- **Manual, line-by-line approval.** Auto-accept was never used: every diff the AI
  proposed was reviewed and approved change by change, so I know exactly which lines
  entered the codebase and why.
- **Tests as a safety net.** The backend suite (unit + integration through
  `WebApplicationFactory`) runs after each change; a generated change that breaks a
  test does not get in.
- **Manual smoke testing.** Endpoints are exercised through Swagger UI (including the
  401 → login → 200 flow) and the UI flows through the Angular app.
- **Requirements cross-check.** The exercise statement was replayed against the codebase
  (gap analysis) to catch missing requirements — that is exactly how the missing
  auth/user story was detected and became prompt #2.

### 1.5 What was corrected or improved over the raw output

- **"C# as scripts" vs. Clean Architecture.** Prompt #1 asked for file-based C# scripts;
  the AI flagged the conflict with a proper layer separation, and the decision (mine)
  was to prioritise Clean Architecture — see the note in [Tech stack](#2-tech-stack).
- **Missing auth requirement.** The first iteration had no users at all; the gap
  analysis surfaced it and prompt #2 closed it — including relating every To-Do to its
  owner, which the exercise implies but does not spell out.
- **Seed strategy.** Task seeding was replaced by *user* seeding: tasks now start empty
  and belong to whoever creates them, while the demo credentials are seeded so the app
  is usable out of the box (a submission requirement).
- **Boolean flag → status enum.** The first iteration modelled completion as
  `IsCompleted`; cross-checking against the exercise's task model (`status`,
  `due_date`) motivated prompt #3, which replaced it with the `TodoStatus` enum plus an
  optional `DueDate` — a refactor that touched every layer (entity, DTOs, validators,
  repository, tests, Angular UI) and was reviewed diff by diff.

### 1.6 Edge cases, authentication and validation handling

- **Authorized vs. non-authorized endpoints.** The whole `/api/todos` group requires a
  bearer token (`.RequireAuthorization()`); the `/api/auth` endpoints are anonymous.
  Requests without a valid token get `401`.

  ```csharp
  var group = app.MapGroup("/api/todos")
      .WithTags("Todos")
      .RequireAuthorization();
  ```

- **Ownership as a domain invariant.** A `TodoItem` cannot exist without an owner —
  the constructor rejects an empty `UserId` — and the Application layer resolves the
  caller through an `ICurrentUser` abstraction, so it stays independent of
  `HttpContext` (Clean Architecture preserved).
- **Input validation.** FluentValidation validators run in a reusable endpoint filter
  before any write handler; failures return `400` with a per-field `errors` map. The
  status field is guarded with `IsInEnum()`, so an out-of-range value is rejected as a
  `400` instead of silently persisting an invalid state.
- **Readable enum contract.** `JsonStringEnumConverter` serialises `TodoStatus` by name
  (`"Pending"`, `"InProgress"`, `"Done"`), and the Angular model mirrors it as a string
  union type — no magic numbers crossing the API boundary.
- **Consistent errors.** A single `GlobalExceptionHandler` maps `NotFoundException` →
  `404`, validation failures → `400`, anything else → `500`, always as RFC 7807
  `ProblemDetails`.
- **Seeding safety.** The identity seeder is idempotent (skips if the user exists) and
  fails fast with the concrete Identity errors if user creation is rejected.
- **Session survival on the frontend.** The access token is kept in `localStorage` and
  the auth signals are re-hydrated on startup, so a page refresh keeps the session; the
  route guard redirects anonymous visitors to `/login`.

### 1.7 Prompt retrospective — what I would improve

Looking back at the three prompts with the final application in hand, a single richer
initial prompt would have landed much closer to the end state in one iteration. The
gaps that forced prompts #2 and #3 were all *specification* gaps, not AI failures:

- **The data model was underspecified.** Prompt #1 never described the task fields, so
  the AI reasonably invented a minimal shape (`IsCompleted` boolean, no due date).
  Migrating to `TodoStatus` + `DueDate` later touched every layer — entity, DTOs,
  validators, repository, tests and UI. Stating the full model upfront would have
  avoided the most expensive refactor of the project.
- **Authentication was missing entirely.** Users, login and per-user ownership are
  cross-cutting; bolting them on afterwards forced changes to the repository contract,
  the service layer and every test. Auth is the kind of requirement that should always
  be in the first prompt.
- **Acceptance criteria beat feature lists.** The items that were phrased as concrete,
  verifiable behaviours ("Details view is the only one that displays the description")
  survived intact from the very first output. Vaguer items ("interactive UI design")
  required follow-up iterations.

An improved version of the initial prompt, incorporating those lessons:

```text
Build a To-Do application with .NET 10 (Minimal API) and the latest Angular.

# Data model
- Task: id, title (required, max 200), optional description (max 2000),
  status (enum: Pending / InProgress / Done, default Pending), optional dueDate,
  createdAt/updatedAt timestamps, and an owner (userId).
- User: email + password, managed by ASP.NET Core Identity.

# Auth (from day one)
- Token-based login/register endpoints; all task endpoints require a bearer token
  and only ever return the caller's own tasks (401 without a token).
- Seed one demo user (email + password) so the app works out of the box; do not
  seed tasks.

# Architecture
- Clean Architecture (Domain / Application / Infrastructure / Api), TDD,
  FluentValidation via endpoint filters, global error handling with RFC 7807
  ProblemDetails, EF Core In-Memory.
- The Application layer must not depend on HttpContext — abstract the current
  user behind an interface.

# Acceptance criteria
- The list endpoint/view never exposes the description; only the detail view does.
- Enum values travel by name over the API ("Pending"), not as numbers.
- Unauthenticated visitors are redirected to /login by a route guard; the token
  survives a page refresh.
- Integration tests cover the 401 → login → 200 flow.

# Documentation
- README with this prompt, the stack/architecture, setup steps and demo credentials.
```

Two caveats worth keeping in mind. First, some of that precision only exists *because*
of the iterations — you rarely know the right acceptance criteria before seeing a first
version, so short prompt cycles with a careful review of each diff remain a healthy way
to work. Second, a mega-prompt raises the blast radius of a misunderstanding: the
line-by-line review described in [§1.4](#14-how-the-ais-suggestions-were-validated)
matters even more as prompts get bigger.

---

## 2. Tech stack

### Backend

| Concern              | Choice                                                        |
| -------------------- | ------------------------------------------------------------- |
| Runtime / language   | **.NET 10**, C# 14 (`net10.0`, nullable + implicit usings on) |
| API style            | **Minimal API** with grouped, named endpoints                 |
| Validation           | **FluentValidation** (via a reusable endpoint filter)         |
| Error handling       | **Global** `IExceptionHandler` → RFC 7807 `ProblemDetails`    |
| Authentication       | **ASP.NET Core Identity** token endpoints (`MapIdentityApi`)  |
| Persistence          | **EF Core 10 – In-Memory** provider                           |
| API docs             | Swagger / OpenAPI (Swashbuckle)                               |
| Testing              | xUnit, Shouldly, NSubstitute, `WebApplicationFactory`         |

### Frontend

| Concern            | Choice                                                     |
| ------------------ | --------------------------------------------------------- |
| Framework          | **Angular 21** (standalone, zoneless, signals)            |
| UI components      | **Angular Material 3** (azure theme) + Material Icons      |
| Forms & validation | **Reactive Forms** with Material `mat-error` messages      |
| HTTP               | `HttpClient` (fetch backend), typed models                |
| Auth               | Login page + functional **interceptor** (bearer) + **route guard** |
| Routing            | Lazy-loaded standalone routes                              |

> **A note on "C# as scripts".** .NET 10 introduces file-based programs
> (`dotnet run app.cs`). Because the assessment also requires **Clean Architecture with a
> correct layer separation**, the solution is organised as separate projects rather than a
> single script file — the two goals pull in opposite directions, and layer separation was
> treated as the higher priority. Modern C# idioms requested by the prompt (top-level
> statements in `Program.cs`, file-scoped namespaces, records, primary constructors) are
> used throughout.

---

## 3. Architecture

### Clean Architecture layers

The backend follows Clean Architecture. Dependencies point **inwards only** — outer layers
know about inner layers, never the reverse.

```
┌───────────────────────────────────────────────────────────────┐
│  ToDo.Api  (Minimal API, Identity auth endpoints, Swagger,     │
│             CORS, Global Error Handler, CurrentUser adapter)   │
│    └── depends on ▼                                            │
│  ToDo.Infrastructure  (EF Core In-Memory, Identity stores,     │
│                        Repository impl, user seeding)          │
│    └── depends on ▼                                            │
│  ToDo.Application  (Use cases / Services, DTOs, Validators,    │
│                     interfaces — ITodoRepository, ICurrentUser)│
│    └── depends on ▼                                            │
│  ToDo.Domain  (TodoItem entity, TodoStatus enum, invariants)   │
└───────────────────────────────────────────────────────────────┘
```

- **Domain** — the `TodoItem` entity with encapsulated behaviour (no public setters; state
  changes go through `Update` / `MarkCompleted`), the `TodoStatus` lifecycle enum
  (`Pending` / `InProgress` / `Done`), and the ownership invariant (a task cannot exist
  without a `UserId`). Zero external dependencies.
- **Application** — orchestrates use cases (`TodoService`), defines DTOs, FluentValidation
  validators, and two abstractions (Dependency Inversion): `ITodoRepository` for
  persistence and `ICurrentUser` for the caller's identity — so this layer never touches
  `HttpContext`. The list DTO deliberately omits `Description`; only the detail DTO
  exposes it.
- **Infrastructure** — EF Core `DbContext` (todo table + Identity user tables), entity
  configuration, the concrete `TodoRepository` (always filtered by owner), and the demo
  user seeding.
- **Api** — thin HTTP surface: endpoint definitions, the Identity auth endpoints
  (`/api/auth/*`), the `CurrentUser` adapter (claims → `ICurrentUser`), the validation
  endpoint filter, the global exception handler, CORS for the Angular client, and
  composition of the layers.

### Cross-cutting concerns

- **Authentication / authorization** — ASP.NET Core Identity with bearer tokens. The
  `/api/auth` group (register, login, refresh, …) is anonymous; the entire `/api/todos`
  group requires a valid token and every query is scoped to the authenticated owner.
- **Global error handling** — a single `GlobalExceptionHandler` converts every exception
  into a consistent `ProblemDetails` payload: `NotFoundException` → **404**, validation
  failures → **400** (with a per-field `errors` map), anything else → **500**.
- **Validation** — a generic `ValidationFilter<T>` runs the registered FluentValidation
  validator before each write handler and returns `400` with field errors.

### Request flow (create a To-Do)

```
Angular Reactive Form  (auth.interceptor attaches the bearer token)
   → POST /api/todos
      → Authentication middleware              (401 if token missing/invalid)
         → ValidationFilter<CreateTodoRequest> (FluentValidation)
            → TodoService.CreateAsync          (use case, owner via ICurrentUser)
               → TodoItem(...)                 (domain invariants)
                  → ITodoRepository.AddAsync   (EF Core In-Memory)
      ← 201 Created + TodoDetailDto
```

---

## 4. Functionality

### User story

> **As a** registered user, **I want to** sign in and manage my own list of tasks —
> creating them with a title, description and due date, and moving them through
> *Pending → In progress → Done* — **so that** I can keep track of my pending work
> without other users seeing or touching my tasks.

### Capabilities

| Capability                | Where                                                            |
| ------------------------- | --------------------------------------------------------------- |
| Sign in / sign out        | `/login` page (demo credentials pre-seeded); logout from the toolbar |
| Create a To-Do            | "New task" → Material dialog with a Reactive Form (title, description, due date) |
| See all **my** To-Dos     | List view with live counters — every query is scoped to the signed-in user |
| Change status             | `Pending` / `InProgress` / `Done` via the edit dialog; status chips in the list |
| Due date                  | Optional date shown in the list and detail views                |
| Modify a To-Do            | Edit dialog (same Reactive Form, pre-filled)                    |
| Delete a To-Do            | Confirmation dialog                                             |
| **See details**           | Dedicated route `/todos/:id` — **the only view showing the description** |

The list endpoint returns a summary projection **without** the description, satisfying the
requirement that the description is shown only on the details view. Unauthenticated
visitors are redirected to `/login` by the route guard, and API calls without a token
get `401`.

---

## 5. Getting started

### Prerequisites

- [.NET SDK 10](https://dotnet.microsoft.com/download) (`dotnet --version` → `10.x`)
- [Node.js 20+](https://nodejs.org) and npm
- Angular CLI 21 (`npm i -g @angular/cli`) — optional; `npm start` works without a global CLI

### 1) Run the backend API

```bash
cd backend
dotnet run --project src/ToDo.Api --launch-profile http
```

- API base URL: **http://localhost:5064**
- Swagger UI: **http://localhost:5064/swagger**
- On startup the in-memory database is seeded with the demo user:
  **`demo@ballastlane.com` / `Passw0rd!`** (tasks start empty — log in and create your own).

### 2) Run the frontend (in a second terminal)

```bash
cd frontend/todo-app
npm install        # first time only
npm start          # → ng serve
```

- App: **http://localhost:4200**
- Sign in with the demo credentials: **`demo@ballastlane.com` / `Passw0rd!`**

The Angular app is pre-configured (`src/environments/environment.ts`) to call the API at
`http://localhost:5064/api`, and the API enables CORS for `http://localhost:4200`. Start the
API first, then the frontend.

> **Ports:** if you change the API port, update `frontend/todo-app/src/environments/environment.ts`
> and the CORS origin in `backend/src/ToDo.Api/Program.cs`.

---

## 6. Testing

TDD was used to drive the backend. The suite covers the domain/use cases (unit),
the repository (against a real in-memory context), the validators, and the full HTTP
surface (integration via `WebApplicationFactory`) — including the auth flow: requests
without a token get `401`, and the happy path logs in with the seeded user and calls
the protected endpoints with the returned bearer token.

```bash
cd backend
dotnet test
```

Frontend build / unit test:

```bash
cd frontend/todo-app
npm run build      # AOT production build
npm test           # Angular unit tests (requires a browser)
```

---

## 7. API reference

### Auth — `/api/auth` (anonymous)

ASP.NET Core Identity's built-in endpoints, mapped under `/api/auth`:

| Method | Route       | Body                                | Success                                            |
| ------ | ----------- | ----------------------------------- | -------------------------------------------------- |
| `POST` | `/register` | `{ email, password }`               | `200` — user created                               |
| `POST` | `/login`    | `{ email, password }`               | `200` `{ tokenType, accessToken, expiresIn, refreshToken }` |
| `POST` | `/refresh`  | `{ refreshToken }`                  | `200` — new token pair                             |

### To-Dos — `/api/todos` (requires `Authorization: Bearer <accessToken>`)

All endpoints return `401` without a valid token, and only operate on the caller's own
tasks. `status` is serialised by name: `"Pending"`, `"InProgress"` or `"Done"`.

| Method   | Route         | Body                                             | Success        | Notes                                   |
| -------- | ------------- | ------------------------------------------------ | -------------- | --------------------------------------- |
| `GET`    | `/`           | —                                                | `200` list     | Summary (no description)                |
| `GET`    | `/{id}`       | —                                                | `200` detail   | Includes description; `404` if missing  |
| `POST`   | `/`           | `{ title, description?, dueDate? }`               | `201` detail   | `400` on validation error               |
| `PUT`    | `/{id}`       | `{ title, description?, status, dueDate? }`       | `200` detail   | `404` if missing; `400` on validation   |
| `DELETE` | `/{id}`       | —                                                | `204`          | `404` if missing                        |

Error responses use `application/problem+json` (RFC 7807). Example `404`:

```json
{
  "title": "Resource not found",
  "status": 404,
  "detail": "\"TodoItem\" (…) was not found.",
  "instance": "/api/todos/…"
}
```

---

## 8. Project structure

```
Ballastlane-ToDo/
├── README.md
├── backend/
│   ├── ToDo.slnx
│   ├── src/
│   │   ├── ToDo.Domain/          # TodoItem entity, TodoStatus enum, invariants
│   │   ├── ToDo.Application/     # Services, DTOs, Validators, interfaces
│   │   │                         #   (ITodoRepository, ICurrentUser)
│   │   ├── ToDo.Infrastructure/  # EF Core In-Memory, Identity (AppUser),
│   │   │                         #   repository, demo-user seeder
│   │   └── ToDo.Api/             # Minimal API, auth endpoints, CurrentUser,
│   │                             #   filters, global error handler
│   └── tests/
│       ├── ToDo.Application.Tests/  # Unit + repository + validator tests
│       └── ToDo.Api.Tests/          # HTTP integration tests (incl. auth flow)
└── frontend/
    └── todo-app/
        └── src/app/
            ├── core/            # models, TodoService, AuthService,
            │                    #   auth interceptor + route guard
            ├── features/
            │   ├── auth/        # login page
            │   └── todos/       # list, detail, form dialog
            └── shared/          # confirm dialog
```
