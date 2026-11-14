# CLAUDE.md

This file provides guidance to Claude Code when working with code in this repository.

## Overview

.NET 10 API on a Clean Architecture template. The project's purpose is not defined yet. The only feature is the `Users` module (register, log in, get current user). New features copy its shape.

## Stack

- C# / .NET 10, ASP.NET Core Minimal APIs. No controllers.
- PostgreSQL 17 via EF Core 10 (Npgsql). Docker Compose for local dev.
- CQRS via MediatR 12. Each command or query lives in the same file as its handler.
- Auth: self-issued HS256 JWTs with `sub = AppUser.Id`. Passwords hashed with PBKDF2-HMAC-SHA256. No refresh tokens, no ASP.NET Core Identity.

```
Endure.Api -> Endure.Infrastructure -> Endure.Application -> Endure.Domain
Endure.Api -> Endure.Application
```

- `Endure.Domain`: entities and repository interfaces. Zero references.
- `Endure.Application`: commands, queries, handlers, DTOs, contracts, exceptions, `ICurrentUser`, `JwtOptions`.
- `Endure.Infrastructure`: `ApplicationDbContext`, EF configurations, repositories, migrations, password hashing, token issuing.
- `Endure.Api`: `Program.cs`, endpoints, JWT validation, Swagger, `ExceptionHandler`.

## Commands

```bash
docker compose up -d                              # postgres:17 as endure-db
dotnet build                                      # zero warnings
dotnet run --project src/Endure.Api               # http://localhost:5170/swagger
dotnet test                                       # tests/Endure.Tests (xUnit)
dotnet ef migrations add <Name> --project src/Endure.Infrastructure --startup-project src/Endure.Api --output-dir Persistence/Migrations
dotnet ef database update --project src/Endure.Infrastructure --startup-project src/Endure.Api
```

The API does not start without `Jwt:Key` (32+ bytes) in user-secrets.

## Conventions

- `Common/` folders group by technical kind (`Entities/`, `Contracts/`, `Services/`, `Schemas/`, `Configuration/`).
- `Modules/<Module>/` folders group by business feature. Use the same module name in Domain, Application and `Endpoints/<Module>/` in Api.
- Infrastructure groups by technical concern: `Persistence/Configuration/<Module>/`, `Repositories/<Module>/`, `Authentication/`.
- Every entity inherits `BaseEntity`. `ApplicationDbContext` fills `CreatedAtUtc` and `UpdatedAtUtc`.
- Persistence rules live in `IEntityTypeConfiguration<T>`, not on the entity.
- Code must be easy for a junior developer to read. Write every method, property getter and constructor in full, with curly braces and an explicit `return`.
- The arrow `=>` is only allowed in three places:
  - Inside a LINQ query, for example `.Where(u => u.Email == email)`.
  - In an EF Core property selector, for example `builder.Property(u => u.Email)`.
  - As the endpoint handler passed to `app.MapGet` / `MapPost` / `MapPut` / `MapDelete`. The handler always has a `{ }` block body.
- Do not use `=>` anywhere else:
  - No expression-bodied members. Write `public string Hash(string password) { return CreateHash(password); }`, not `public string Hash(string password) => CreateHash(password);`.
  - No inline lambdas for framework callbacks. Write a named `private static` method and pass it by name, for example `services.AddOpenApi(ConfigureOpenApi);`.
  - No switch expressions. Use a `switch` statement with `case ...: return ...;`.
- No ternary operator (`condition ? a : b`). Use an `if` statement with curly braces and an early `return`, for example `if (appUser is null) { return TypedResults.NotFound(); } return TypedResults.Ok(appUser);`.
- EF-generated migration files are exempt. Do not hand-edit them.
- Requests are `sealed` classes with constructor values and get-only properties. Handlers are `internal sealed` and live in the same file.
- DTOs are bare records in `Modules/<Module>/Dtos/` with `required` `{ get; init; }` properties. No mapping methods on the DTO (no `FromEntity`).
- The handler builds the DTO itself with an object initializer, assigns it to a variable, and returns that variable, for example `var appUserDto = new AppUserDto { Id = appUser.Id, ... }; return appUserDto;`.
- One class per endpoint, `internal sealed`, implementing `IEndpoint`, named `<Verb><Noun>Endpoint.cs`. A class without `IEndpoint` is skipped silently. A class with `IEndpoint` whose `MapEndpoint` is not `public static` fails at startup.
- Routes are constants in `Endpoints/ApiEndpoints.cs`. No raw route strings.
- Endpoints only map the request, send it with `ISender`, and return the result.
- All endpoint logic lives inline in the `MapX` handler lambda inside `MapEndpoint`. Do not move the handler body into a separate `HandleAsync` method. A small `private static` helper in the same endpoint class is fine when the handler needs a decision, for example picking which command to send.
- The handler lambda declares its success return type, for example `async Task<Created<AppUserDto>> (...) => { ... }`, and returns `TypedResults.Created(...)` or `TypedResults.Ok(...)`. The typed result puts the success response in the OpenAPI document.
- Each endpoint calls `RequireAuthorization()` or `AllowAnonymous()` explicitly.
- Anonymous endpoints that hash a password (login, register) call `.RequireRateLimiting(RateLimitPolicies.Auth)` and list `.ProducesProblem(StatusCodes.Status429TooManyRequests)`.
- Each endpoint lists every error status it can return with `.ProducesProblem(StatusCodes.StatusXxx)`, so Swagger shows it.
- Each layer registers its services in its own `Common/DependencyInjection/DependencyInjection.cs`: a static class `DependencyInjection` with one public `Add<Layer>Dependencies(this IHostApplicationBuilder builder)` method. That method only calls private `Add<Area>(IServiceCollection services)` helpers, one per area (for example `AddDatabase`, `AddRepositories`). `Program.cs` calls each layer's method once, before `builder.Build()`.
- `Endure.Domain` has no `DependencyInjection` class until it has something to register.
- Handlers are registered automatically by MediatR assembly scanning. Endpoints are mapped automatically by `MapApiEndpoints`. Everything else is registered by hand in the matching layer's helper.

## Errors

- Handlers return the plain DTO, for example `IRequest<AppUserDto>`. On failure they throw. Do not return `null` and do not wrap results.
- Throw only the exceptions in `Endure.Application/Common/Exceptions/`:
  - `ValidationException` gives 400. Handlers validate their own input and throw it.
  - `NotFoundException` gives 404.
  - `ConflictException` gives 409.
  - `UnauthorizedException` gives 401.
- Write the exception message for the API client, for example `throw new NotFoundException("The user does not exist.");`. The message is sent as the `detail` of the response, so never put ids of other users, secrets or database details in it.
- `ExceptionHandler` (`Endure.Api/Common/Exceptions/ExceptionHandler.cs`) is the only place that turns an exception into an HTTP status code. It writes an RFC 7807 ProblemDetails response (`application/problem+json`, with `traceId`). When you add an exception type, add an `if` for it in `GetStatusCode` and `GetDetail`.
- Any other exception becomes a 500 with no `detail`, and is logged. Never send `exception.Message` of an unknown exception to the client.
- Endpoints do not catch exceptions and do not check for errors. They send the request and return the success result.

## Auth rules

- Owned entities carry an `AppUserId`. Handlers take `appUserId` and filter on it. The endpoint gets it from `ICurrentUser`, never from the route or body.
- A read of another user's record returns 404, not 403.
- Login returns the same 401 for an unknown email and a wrong password, and always runs the hash.
- Emails are trimmed and lowercased before storage and lookup.
- Never put secrets or personal data in the JWT.

## Adding a module

1. Domain: `Modules/<Module>/Entities/` and `Modules/<Module>/Repositories/I<Entity>Repository.cs`.
2. Application: `Modules/<Module>/Dtos/`, `Commands/`, `Queries/`.
3. Infrastructure: `Persistence/Configuration/<Module>/`, `Repositories/<Module>/`, then register the repository in `AddRepositories` in `Common/DependencyInjection/DependencyInjection.cs`.
4. Api: route constants in `ApiEndpoints`, one `IEndpoint` class per route.
5. Build, then add a migration.

## Tests

- Tests live in `tests/Endure.Tests` (xUnit, hand-written fakes, no mocking library).
- Only write tests for behavior where a silent break is a security hole or locks users out: password hashing, login, registration validation, and later anything that guards another user's data.
- Do not add tests for plumbing, mapping, endpoints or anything a manual run would catch at once. Fewer, meaningful tests beat coverage.

## Guardrails

- Do not run `git commit`, `git push`, or anything that writes git history. The user commits.
- Do not write comments in code.
- Do not add any reference to `Endure.Domain`.
- Do not reference `Endure.Infrastructure` from anywhere except `Endure.Api`.
- Do not reference ASP.NET Core from `Endure.Application`.
- Do not edit a migration after it is pushed. Add a new migration instead.
- Do not put real secrets or connection strings with real passwords in `appsettings.json`. Use user-secrets.
