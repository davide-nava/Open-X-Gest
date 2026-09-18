# AGENTS.md - Operational Governance & Architecture Manifesto for Autonomous Agents

This document defines the strict operational boundaries, engineering standards, and architectural invariants governing any AI coding assistant or autonomous agent working across the **Open-X-Gest** repository.

---

## 1. Monorepo Anatomy & Solution Structure

Open-X-Gest is structured as a multi-tier solution ecosystem containing backend, frontend, and mobile sub-projects:

```text
Open-X-Gest/
├── Open-X-Gest.slnx                # Visual Studio XML Solution orchestrating backend projects
├── Directory.Build.props           # Global MSBuild rules, Roslynator, StyleCop, SonarAnalyzer
├── .editorconfig                   # Centralized Roslyn and code styling rules
├── nuget.config                    # Package restore & source mappings
├── sonar-project.properties        # SonarQube quality gate configuration
├── aspire.config.json              # Aspire configuration
│
├── dockers/
│   └── seq/
│       └── docker-compose.yml      # Local Seq structured logging container
│
├── src/
│   ├── OpenX.Gest.Domain/          # Pure domain models, entities, business logic & invariants
│   ├── OpenX.Gest.Application/     # CQRS use cases (MediatR), FluentValidation, DTOs & interfaces
│   ├── OpenX.Gest.Infrastructure/  # EF Core, dynamic SQLite (WAL mode), DbInitializer & identity
│   └── OpenX.Gest.Api/             # ASP.NET Core Web API presentation layer
│
├── frontend/                       # Angular 21 Enterprise Web Client
│   ├── src/                        # Standalone components, signals, services & NG-ZORRO UI
│   ├── package.json                # NPM scripts and dependencies
│   ├── AGENTS.md                   # Specialized Frontend Agent Guidelines
│   ├── ARCHITECTURE.md             # Frontend Architectural Deep-Dive
│   └── TESTING.md                  # Vitest Unit Testing Standards
│
├── mobile/                         # Flutter Multiplatform Client (Android, iOS, Desktop, Web)
│   ├── pubspec.yaml                # Flutter dependencies
│   ├── AGENTS.md                   # Specialized Mobile Agent Guidelines
│   ├── ARCHITECTURE.md             # Mobile Offline-First Documentation
│   └── TESTING.md                  # Flutter & BLoC Testing Standards
│
└── tests/
    └── OpenX.Gest.Application.UnitTests/ # Command/Query handlers, pipeline behaviors & validator tests
```

---

## 2. Fundamental Architectural Rules by Stack

### 2.1 Backend (.NET 11 & .NET Aspire)

1. **Clean Architecture Dependency Inversion**:
   - `Domain` depends on NOTHING external.
   - `Application` depends ONLY on `Domain`.
   - `Infrastructure` depends on `Domain` and `Application`.
   - `Api` depends on `Application` and `Infrastructure`.
   - **FORBIDDEN**: Never reference `Infrastructure` or `Api` from `Application` or `Domain`.

2. **SOLID & Clean Code Principles**:
   - **Single Responsibility (SRP)**: Each command, query, validator, and handler must reside in its own dedicated, isolated file.
   - **Strict Encapsulation**: Domain models must feature `private set` properties and dedicated domain mutating methods (`ClockIn`, `ClockOut`, `SetComplianceMetrics`, etc.) that enforce business invariants.
   - **Immutability**: DTOs, MediatR Requests, and Responses must be declared as immutable `record` types.
   - **No Magic Strings**: Use strongly-typed constants or enums where appropriate.

3. **CQRS & MediatR Pipeline Pattern**:
   - Commands mutate state and return `Guid` or `Result` / `Unit`.
   - Queries perform read-only projection with `AsNoTracking()` and return DTO records.
   - Every Command MUST have an accompanying FluentValidation `AbstractValidator<TCommand>`.
   - The MediatR `ValidationBehavior<TRequest, TResponse>` pipeline automatically intercepts all requests and throws a domain `ValidationException` on failure.

4. **Database & Portable SQLite Architecture**:
   - Database paths must always resolve dynamically via `DatabasePathHelper` or connection string relative to `AppDomain.CurrentDomain.BaseDirectory/data/openx_gest.db`.
   - Write-Ahead Logging (WAL) mode and `synchronous = NORMAL` must be enforced on every SQLite connection via `SqliteWalConnectionInterceptor`.
   - Database initialization and seed logic (`DbInitializer`) must be idempotent and non-destructive.

5. **Language & Documentation Standards**:
   - **Language**: English is mandatory for ALL identifiers, comments, logs, exception messages, and documentation.
   - **XML Documentation**: Every public and internal type, interface, property, and method MUST include comprehensive XML documentation comments (`GenerateDocumentationFile=true` is strictly enforced).

---

### 2.2 Frontend (Angular 21 Standalone & Signals)

Agents working inside `frontend` must adhere to [frontend/AGENTS.md](frontend/AGENTS.md):
- **Standalone Components Exclusively**: Do not specify `standalone: true` (default in modern Angular).
- **Angular Signals**: Use `signal()`, `computed()`, `input()`, and `output()`. Avoid deprecated decorators (`@Input`, `@Output`, `@HostBinding`, `@HostListener`).
- **OnPush Change Detection**: Every component must declare `changeDetection: ChangeDetectionStrategy.OnPush`.
- **Functional Injection**: Always use `inject()` rather than constructor parameter injection.
- **Native Control Flow**: Use `@if`, `@for`, `@switch`, `@empty` (avoid `*ngIf`, `*ngFor`).
- **Strict Typing**: No `any` types; prefer typed reactive forms (`FormGroup`, `FormControl`).
- **UI Consistency**: Leverage NG-ZORRO (`ng-zorro-antd`) components and WCAG AA accessibility rules.

---

### 2.3 Mobile (Flutter Multiplatform & Offline-First)

Agents working inside `mobile` must adhere to [mobile/AGENTS.md](mobile/AGENTS.md):
- **Feature-First Clean Architecture**: Structure code into `lib/features/<feature>/{domain, data, presentation}`.
- **State Management**: Use `flutter_bloc` (BLoC / Cubit) with immutable events and states (`equatable`).
- **Functional Error Handling**: All Use Cases and Repositories must return `Future<Either<Failure, T>>` via `dartz`.
- **Offline-First Synchronization**: Mutate and cache punches through local SQLite (`sqflite`) before syncing remotely.
- **Dependency Injection**: Register all singletons and dependencies in `lib/core/di/injection.dart` using `get_it`.
- **Analyzer Integrity**: Zero analyzer warnings (`flutter analyze` must pass with 0 errors/warnings).

---

## 3. Testing & Verification Protocols

Prior to completing any turn or task, agents must execute the appropriate verification steps based on modified workspaces:

### Backend Verification:
```bash
# Solution build with zero errors and zero warnings
dotnet build Open-X-Gest.slnx

# Run all backend unit test suites
dotnet test Open-X-Gest.slnx
```
- Test cases must follow the naming convention: `MethodName_Condition_ExpectedBehavior`.
- Assertions must use `FluentAssertions`.

### Frontend Verification:
```bash
cd frontend

# Single-run Vitest test suite
npx ng test --watch=false

# Production build check
npm run build
```

### Mobile Verification:
```bash
cd mobile

# Static analysis
flutter analyze

# Run unit and widget tests
flutter test
```
