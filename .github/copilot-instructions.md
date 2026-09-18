# GitHub Copilot Instructions for Open-X-Gest

## 1. Architectural Philosophy & Layer Boundaries
Open-X-Gest is an enterprise-grade, portable mini ERP / business management system built on .NET 11 and Clean Architecture. Strict unidirectional dependencies MUST always be respected:
- `OpenX.Gest.Domain`: Pure enterprise domain model. No external infrastructure dependencies. Encapsulated entities, private setters, invariant enforcement.
- `OpenX.Gest.Application`: Core business use cases following CQRS via MediatR. Validations through FluentValidation pipeline behaviors. Defines repository and service abstractions (`IApplicationDbContext`, `ICurrentUserService`, `IIdentityService`).
- `OpenX.Gest.Infrastructure`: EF Core with SQLite, persistence, and external service integrations.
- `OpenX.Gest.Api`: ASP.NET Core Web API presentation layer with endpoints protected by authorization.

## 2. CQRS & MediatR In-Process Dispatching Rules
- Every feature must be segregated into `Commands/` and `Queries/` with dedicated request records, FluentValidation validators, and request handlers.
- All commands that mutate state must validate preconditions and uniqueness constraints through FluentValidation.
- The `ValidationBehavior<TRequest, TResponse>` pipeline behavior automatically intercepts requests and throws a domain `ValidationException` when validation fails.
- Handlers should return strongly typed DTOs (e.g., `ArticleDto`, `CustomerDto`) or identifiers, never exposing raw EF Core tracked entities outside Application.

## 3. Database & SQLite Guidelines
- Always use the dynamic path: `Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "openx_gest.db")`.
- Ensure directory creation prior to establishing connections.
- Ensure SQLite WAL mode is consistently enforced on connections via `SqliteWalConnectionInterceptor` (`PRAGMA journal_mode = 'wal'; PRAGMA synchronous = NORMAL;`).

## 4. MudBlazor & UI Guidelines
- Always use asynchronous MudBlazor APIs (e.g., `_form.ValidateAsync()`, `DialogService.ShowMessageBoxAsync(...)`).
- Use `MudThemeProvider`, `MudDialogProvider`, `MudSnackbarProvider`, and `MudPopoverProvider`.
- Protect all sensitive Blazor pages with `@attribute [Authorize]`.

## 5. Coding & Documentation Standards
- Official language: English across all code, comments, exception messages, and UI strings.
- Complete XML documentation (`/// <summary>`, `<param>`, `<returns>`, `<exception>`) is mandatory on all public and internal interfaces, classes, records, and methods.
- Strict nullability enabled (`<Nullable>enable</Nullable>`).
- Zero build warnings or errors under `TreatWarningsAsErrors=true`.
