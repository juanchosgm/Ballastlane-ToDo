# Ballastlane To-Do

A full-stack **To-Do** application built as a technical assessment. It demonstrates a
CRUD workflow across a **.NET 10 Minimal API** (Clean Architecture + TDD) and an
**Angular 21** single-page app (Angular Material + Reactive Forms), backed by an
**in-memory database** for zero-config setup.

---

## Table of contents

1. [The original prompt](#1-the-original-prompt)
2. [Tech stack](#2-tech-stack)
3. [Architecture](#3-architecture)
4. [Functionality](#4-functionality)
5. [Getting started](#5-getting-started)
6. [Testing](#6-testing)
7. [API reference](#7-api-reference)
8. [Project structure](#8-project-structure)

---

## 1. The original prompt

> This project was generated from the following initial prompt (reproduced verbatim, as
> requested in the requirements):

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

---

## 2. Tech stack

### Backend

| Concern              | Choice                                                        |
| -------------------- | ------------------------------------------------------------- |
| Runtime / language   | **.NET 10**, C# 14 (`net10.0`, nullable + implicit usings on) |
| API style            | **Minimal API** with grouped, named endpoints                 |
| Validation           | **FluentValidation** (via a reusable endpoint filter)         |
| Error handling       | **Global** `IExceptionHandler` → RFC 7807 `ProblemDetails`    |
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
┌─────────────────────────────────────────────────────────────┐
│  ToDo.Api  (Minimal API, Swagger, CORS, Global Error Handler) │
│    └── depends on ▼                                           │
│  ToDo.Infrastructure  (EF Core In-Memory, Repository impl)    │
│    └── depends on ▼                                           │
│  ToDo.Application  (Use cases / Services, DTOs, Validators,    │
│                     interfaces — ITodoRepository)             │
│    └── depends on ▼                                           │
│  ToDo.Domain  (TodoItem entity, business invariants)          │
└─────────────────────────────────────────────────────────────┘
```

- **Domain** — the `TodoItem` entity with encapsulated behaviour (no public setters; state
  changes go through `Update` / `MarkCompleted`). Zero external dependencies.
- **Application** — orchestrates use cases (`TodoService`), defines DTOs, FluentValidation
  validators, and the `ITodoRepository` abstraction (Dependency Inversion). The list DTO
  deliberately omits `Description`; only the detail DTO exposes it.
- **Infrastructure** — EF Core `DbContext`, entity configuration, the concrete
  `TodoRepository`, and database seeding.
- **Api** — thin HTTP surface: endpoint definitions, the validation endpoint filter, the
  global exception handler, CORS for the Angular client, and composition of the layers.

### Cross-cutting concerns

- **Global error handling** — a single `GlobalExceptionHandler` converts every exception
  into a consistent `ProblemDetails` payload: `NotFoundException` → **404**, validation
  failures → **400** (with a per-field `errors` map), anything else → **500**.
- **Validation** — a generic `ValidationFilter<T>` runs the registered FluentValidation
  validator before each write handler and returns `400` with field errors.

### Request flow (create a To-Do)

```
Angular Reactive Form
   → POST /api/todos
      → ValidationFilter<CreateTodoRequest>   (FluentValidation)
         → TodoService.CreateAsync            (use case)
            → TodoItem(...)                   (domain invariants)
               → ITodoRepository.AddAsync     (EF Core In-Memory)
      ← 201 Created + TodoDetailDto
```

---

## 4. Functionality

| Capability                | Where                                                            |
| ------------------------- | --------------------------------------------------------------- |
| Create a To-Do            | "New task" → Material dialog with a Reactive Form                |
| See all To-Dos            | List view with live counters (total / pending / completed)      |
| Toggle completed          | Checkbox on each card, or from the detail view                  |
| Modify a To-Do            | Edit dialog (same Reactive Form, pre-filled)                    |
| Delete a To-Do            | Confirmation dialog                                             |
| **See details**           | Dedicated route `/todos/:id` — **the only view showing the description** |

The list endpoint returns a summary projection **without** the description, satisfying the
requirement that the description is shown only on the details view.

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
- The in-memory database is seeded with a few sample tasks on startup.

### 2) Run the frontend (in a second terminal)

```bash
cd frontend/todo-app
npm install        # first time only
npm start          # → ng serve
```

- App: **http://localhost:4200**

The Angular app is pre-configured (`src/environments/environment.ts`) to call the API at
`http://localhost:5064/api`, and the API enables CORS for `http://localhost:4200`. Start the
API first, then the frontend.

> **Ports:** if you change the API port, update `frontend/todo-app/src/environments/environment.ts`
> and the CORS origin in `backend/src/ToDo.Api/Program.cs`.

---

## 6. Testing

TDD was used to drive the backend. The suite covers the domain/use cases (unit),
the repository (against a real in-memory context), the validators, and the full HTTP
surface (integration via `WebApplicationFactory`).

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

Base path: `/api/todos`

| Method   | Route         | Body                                             | Success        | Notes                                   |
| -------- | ------------- | ------------------------------------------------ | -------------- | --------------------------------------- |
| `GET`    | `/`           | —                                                | `200` list     | Summary (no description)                |
| `GET`    | `/{id}`       | —                                                | `200` detail   | Includes description; `404` if missing  |
| `POST`   | `/`           | `{ title, description? }`                         | `201` detail   | `400` on validation error               |
| `PUT`    | `/{id}`       | `{ title, description?, isCompleted }`            | `200` detail   | `404` if missing; `400` on validation   |
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
│   │   ├── ToDo.Domain/          # Entities + invariants
│   │   ├── ToDo.Application/     # Services, DTOs, Validators, interfaces
│   │   ├── ToDo.Infrastructure/  # EF Core In-Memory, repository, seed
│   │   └── ToDo.Api/             # Minimal API, filters, global error handler
│   └── tests/
│       ├── ToDo.Application.Tests/  # Unit + repository + validator tests
│       └── ToDo.Api.Tests/          # HTTP integration tests
└── frontend/
    └── todo-app/
        └── src/app/
            ├── core/            # models + TodoService (HttpClient)
            ├── features/todos/  # list, detail, form dialog
            └── shared/          # confirm dialog
```
