# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

AppPickleball is an **Operator/Admin Management** microservice built with **.NET 10**, following **Clean Architecture** principles. It handles workspace management, subscriptions, billing/orders, service plan configuration, and operational auditing for the Super Admin panel.

## Build & Run

```bash
dotnet restore
dotnet build
dotnet run --project AppPickleball.Api

# Docker
docker build -f AppPickleball.Api/Dockerfile -t AppPickleball-api .
docker-compose up -d
```

**Solution file:** `AppPickleball.slnx` (.NET 10 XML solution format)

**No test projects exist yet.** When adding tests, follow standard xUnit conventions.

## Database

**Project KHÔNG sử dụng EF Core Migrations.** Schema được quản lý thủ công qua SQL scripts trong `Documents/Database_Documents/`. Khi thêm entity mới, chỉ cần tạo Entity + EF Configuration — không chạy `dotnet ef migrations add`.

## Architecture

```
AppPickleball/
├── AppPickleball.Api/           # Presentation — Controllers, Middleware, Configurations
├── AppPickleball.Application/   # Business logic — CQRS Commands/Queries/Handlers (MediatR)
├── AppPickleball.Domain/        # Core domain — Entities, Enums, BaseEntity (no dependencies)
├── AppPickleball.Infrastructure/ # Data access — EF Core, Repositories, External services
└── AppPickleball.Share/         # Shared — ApiResponse wrapper, Localization resources
```

### Layer Dependencies
- `Api` → `Application`, `Infrastructure`, `Share`
- `Application` → `Domain`, `Share`
- `Infrastructure` → `Application`, `Domain`
- `Domain` — no dependencies (pure domain model)

### Startup Pipeline (Program.cs)
Layers register via extension methods: `AddApplication()`, `AddInfrastructure(config)`, `AddApiDependencies(config)`, `AddApiControllers()`, `AddSwaggerModule()`, `AddMassTransitConfig(config)`, `AddAppLocalization()`, `AddCorsPolicy()`. Serilog registered via `builder.Host.UseSerilog()`. Middleware order: Swagger (dev) → HTTPS → `CorrelationIdMiddleware` → `ExceptionHandlerMiddleware` → Authentication → Authorization → Controllers.

## Rules (Nguyên tắc bắt buộc)

> **Agent PHẢI tuân thủ các rules này khi tạo code mới hoặc sửa code.**

### DateTime
- **Luôn dùng `DateTime.UtcNow`** — KHÔNG dùng `DateTime.Now`
- EF Config timestamp default: `HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'")`
- PostgreSQL column type: `TIMESTAMPTZ` (không dùng `TIMESTAMP`)

### Mapping
- **KHÔNG dùng AutoMapper** — project dùng **manual mapping** trong Handler
- Mapping từ Entity → DTO trực tiếp trong CommandHandler/QueryHandler

### Repository (ISP - Interface Segregation)
- `IRepository<T>` — base CRUD cho **mọi entity** (Get, Add, Update, Remove)
- `ISoftDeletableRepository<T>` — **chỉ cho BaseEntity** (SoftDelete, Restore, GetIncludingDeleted)
- Entity kế thừa **BaseEntity** → repository interface kế thừa **cả hai**: `IRepository<T>` + `ISoftDeletableRepository<T>`
- Entity kế thừa **BaseCreatedEntity** → repository interface **chỉ** kế thừa `IRepository<T>`

### API Response
- Luôn wrap trong `ApiResponse<T>` — không trả raw object
- Messages dùng `IStringLocalizer<SharedResource>` — không hardcode string

### Code Style
- 1 class/interface = 1 file
- Vietnamese comments OK (mixed Vi/En)
- Controllers phải thin — delegate logic cho MediatR Handler
- Không gọi `SaveChanges` trong repository — luôn qua `IUnitOfWork`

## Key Patterns

### CQRS with MediatR
Commands and queries live under `AppPickleball.Application/Features/{Domain}/{Commands|Queries}/{Name}/`:
- `{Name}Command.cs` — record implementing `IRequest<ApiResponse<T>>`
- `{Name}CommandHandler.cs` — implements `IRequestHandler<,>`
- `{Name}CommandValidator.cs` — FluentValidation `AbstractValidator<>`

MediatR pipeline behaviours (registered automatically):
- `ValidationBehaviour` — runs all FluentValidation validators before the handler; throws `FluentValidation.ValidationException` on failure
- `LoggingBehaviour` — logs request execution

### Repository + Unit of Work (ISP Compliant)
- `IRepository<T>` — generic CRUD for all entities; does **NOT** call `SaveChanges`
- `ISoftDeletableRepository<T> where T : BaseEntity` — soft delete operations (compile-time safe)
- `IUnitOfWork` — commits via `SaveChangesAsync()`
- Domain-specific repositories (e.g., `IUserRepository`) extend appropriate interfaces
- BaseEntity repos: `IUserRepository : IRepository<User>, ISoftDeletableRepository<User>`
- BaseCreatedEntity repos: `IEmailVerificationTokenRepository : IRepository<EmailVerificationToken>`
- Implementation uses **composition**: `SoftDeletableRepository<T>` delegates soft delete logic

### Domain Entities
All entities extend one of two base classes in `AppPickleball.Domain/Common/BaseEntity.cs`:
- `BaseEntity` — full audit fields (`Id`, `CreatedAt`, `UpdatedAt`, `DeletedAt`, `CreatedBy`, `UpdatedBy`, `DeletedBy`; soft-delete via computed `IsDeleted`)
- `BaseCreatedEntity` — lightweight, only `Id` + `CreatedAt` (used for junction/token/log tables)

### EF Core Conventions
Entity configurations in `AppPickleball.Infrastructure/Persistence/Configurations/`:
- PostgreSQL **snake_case** column naming (e.g., `created_at`, `full_name`)
- **Explicit `HasColumnName()`** bắt buộc cho MỌI property
- Default values: `gen_random_uuid()` for Id, `NOW() AT TIME ZONE 'UTC'` for timestamps
- Global query filter `x.DeletedAt == null` on all `BaseEntity`-derived entities
- Entity con có FK tới entity cha có GlobalQueryFilter → phải thêm matching `HasQueryFilter`
- Unique indexes filtered with `deleted_at IS NULL`
- Enum properties dùng `HasConversion<string>()` để lưu dạng text
- `OpeatorDbContext.SaveChangesAsync()` auto-sets `UpdatedAt` on modified `BaseEntity` entries **và normalize tất cả DateTime → UTC**
- `IBaseDbContext` interface dùng cho DI abstraction của DbContext

### API Response Wrapper
Always use `ApiResponse<T>` from `AppPickleball.Share/Wrappers/ApiResponse.cs`:
```csharp
ApiResponse<T>.SuccessResponse(data, message)
ApiResponse<T>.FailureResponse(message, statusCode, errorCodes)
ApiResponse<T>.FailureResponse(message, errorCode, statusCode)  // single error code overload
```
Non-generic `ApiResponse` (without `<T>`) exists for message-only responses. All responses include `MetaResponse Meta` property with `CorrelationId`, `Timestamp`, and optional `Pagination`.

### Correlation ID (Distributed Tracing)
- `CorrelationIdMiddleware` reads `X-Correlation-Id` from request header (gateway) or generates GUID
- Sets `CorrelationIdAccessor.CorrelationId` (AsyncLocal) → `MetaResponse.CorrelationId` reads automatically
- Also pushes to Serilog `LogContext` → appears in log as `[{CorrelationId}]`
- Response header `X-Correlation-Id` set automatically

### Exception Handling
`ExceptionHandlerMiddleware` maps exceptions to `ApiResponse<string>`:

| Exception | HTTP Status | Error Code |
|---|---|---|
| `ValidationException` | 400 | VALIDATION_ERROR |
| `FluentValidation.ValidationException` | 400 | VALIDATION_ERROR |
| `NotFoundException` | 404 | NOT_FOUND |
| `BadHttpRequestException` | 400 | BAD_REQUEST |
| `DomainException` | 400 | DOMAIN_ERROR |
| Unhandled | 500 | INTERNAL_SERVER_ERROR |

Custom exceptions in `AppPickleball.Application/Common/Exceptions/`: `ValidationException`, `NotFoundException`, `UnauthorizedException`, `DomainException`.

### Multi-Tenancy
`ICurrentUserService` resolves identity from JWT claims or fallback headers (gateway gửi về):
- `UserId` — from JWT `NameIdentifier` claim or `X-User-Id` header
- `WorkspaceId` — from JWT `workspace_id` claim or `X-Workspace-Id` header

Swagger requires `X-Workspace-Id` header (configured as ApiKey security definition).

### Localization
All user-facing strings use `IStringLocalizer<SharedResource>`. Resources in `AppPickleball.Share/Resources/`:
- `SharedResource.resx` — default
- `SharedResource.en.resx` — English
- `SharedResource.vi.resx` — Vietnamese

## Adding a New Feature

1. **Domain** — add entity in `AppPickleball.Domain/Entities/`, extend `BaseEntity` or `BaseCreatedEntity`
2. **EF Config** — add `IEntityTypeConfiguration<T>` in `AppPickleball.Infrastructure/Persistence/Configurations/` (snake_case columns, defaults, indexes, query filter)
3. **DbContext** — register `DbSet<T>` in `OpeatorDbContext`
4. **Repository interface** — add in `AppPickleball.Application/Common/Interfaces/` if specialized queries needed; extend `ISoftDeletableRepository<T>` if BaseEntity
5. **Repository impl** — implement in `AppPickleball.Infrastructure/Persistence/Repositories/`; use `SoftDeletableRepository<T>` composition if BaseEntity
6. **DI** — register repository in `AppPickleball.Infrastructure/DependencyInjection.cs`
7. **Command/Query** — create handler under `AppPickleball.Application/Features/{Domain}/{Commands|Queries}/{Name}/`
8. **Validator** — add FluentValidation validator alongside the command (use `IStringLocalizer<SharedResource>` for messages)
9. **DTO** — add in `AppPickleball.Application/Features/{Domain}/Dtos/`; map manually (project không dùng AutoMapper)
10. **Controller** — add endpoint in `AppPickleball.Api/Controllers/`, inherit `BaseApiController` (provides `_mediator`, `_logger`); annotate with `[SwaggerOperation]` and `[ProducesResponseType]`

## Configuration

Key `appsettings.json` sections bound via Options pattern to classes in `AppPickleball.Application/Common/Settings/`:

| Section | Settings Class | Purpose |
|---|---|---|
| `Jwt` | `JwtSettings` | SecretKey, Issuer, Audience, ExpiryInMinutes |
| `EmailSettings` | `EmailSettings` | SMTP config (MailKit) |
| `AuthSettings` | `AuthSettings` | Auth behavior config |
| `RabbitMQ` | `RabbitMQSettings` | MassTransit broker config |
| `GoogleAuth` | `GoogleAuthSettings` | OAuth provider config |
| `SessionCleanup` | — | `RetentionDays` (default 90) |
| `BaseUrlService` | — | `AppPickleball` base URL |

Use `dotnet user-secrets` in development (UserSecretsId configured in `AppPickleball.Api.csproj`).

### Key NuGet Packages
- **Npgsql.EntityFrameworkCore.PostgreSQL** — PostgreSQL EF Core provider
- **MediatR** — CQRS mediator
- **FluentValidation** — Request validation
- **MassTransit.RabbitMQ** — Message bus
- **BCrypt.Net-Next** — Password hashing
- **MailKit** — SMTP email
- **Serilog.AspNetCore** — Structured logging
- **Swashbuckle.AspNetCore.Annotations** — Swagger annotations

## Code Style

- C# 12+ features (records for commands, primary constructors where applicable)
- Nullable reference types enabled — always handle nullability
- Vietnamese comments are acceptable (existing codebase uses mixed Vi/En)
- No direct `SaveChanges` calls in repositories — always go through `IUnitOfWork`
- Controllers should be thin — delegate all logic to MediatR handlers
- All API responses wrapped in `ApiResponse<T>`

## Domain Model Summary

| Entity | Base Class | Purpose |
|---|---|---|
| `User` | BaseEntity | Global identity, independent of workspaces |
| `Workspace` | BaseEntity | Multi-tenant organization unit |
| `WorkspaceUser` | BaseEntity | User membership in a workspace |
| `UserGroup` | BaseEntity | Role groups within workspace |
| `Permission` | BaseEntity | RBAC permissions |
| `WorkspaceCustomDomain` | BaseCreatedEntity | Custom domain & SSL config per workspace |
| `GroupPermission` | BaseCreatedEntity | Group-Permission assignment |
| `WorkspaceUserGroup` | BaseCreatedEntity | User-Group assignment within workspace |
| `UserSession` | BaseCreatedEntity | Active sessions (JWT/refresh tokens) |
| `OAuthAccount` | BaseCreatedEntity | Linked social accounts (Google, Facebook, GitHub) |
| `UserTwoFactorAuth` / `UserBackupCode` | BaseCreatedEntity | 2FA via TOTP, SMS, or Email |
| `PasswordResetToken` / `EmailVerificationToken` | BaseCreatedEntity | Token-based flows |
| `ApiKey` | BaseCreatedEntity | Machine-to-machine API access |
| `Invitation` | BaseCreatedEntity | Workspace invite flow |
| `AuditLog` | BaseCreatedEntity | Operator action audit trail |

## Agent Skills

Các skills có sẵn trong `.claude/skills/`, gọi bằng `/skill-name`:

| Skill | Mô tả |
|-------|--------|
| `/create-feature` | Tạo feature mới theo CQRS pattern (Command/Query + Handler + Validator + Controller + Interface + Repository) |
| `/create-entity` | Tạo entity mới với EF Config và DbSet (KHÔNG tạo repository) |
| `/create-api-endpoint` | Thêm API endpoint mới vào Controller cho feature đã có |
| `/create-dto` | Tạo DTO với manual mapping pattern |
| `/add-resource-keys` | Thêm resource keys song ngữ VI/EN vào 3 file .resx |
| `/test-api` | Tạo xUnit test cho Handler/Controller |
| `/fix-ef-warnings` | Fix EF Core GlobalQueryFilter warnings và column mappings |
| `/debug-runtime` | Debug các runtime errors phổ biến |

## Custom Agents

Các agents pre-configured trong `.claude/agents/`, Claude tự delegate hoặc gọi theo tên:

| Agent | Model | Vai trò | Cách dùng |
|-------|-------|---------|-----------|
| `backend-dev` | Sonnet | Implement features, CQRS handlers, fix bugs | "Use backend-dev to implement feature X" |
| `architect` | Opus | System design, architecture review, refactoring plan | "Use architect to review module Y" |
| `tester` | Haiku | Viết xUnit tests, check coverage | "Use tester to write tests for Z" |
