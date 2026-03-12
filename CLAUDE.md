# CLAUDE.md — AppPickleball BackEnd

Tài liệu hướng dẫn cho Claude Code khi làm việc với BackEnd của **AppPickleball** — hệ thống quản lý giải đấu Pickleball.

> **Tài liệu chi tiết**: xem `../Documents/` (MAINFLOWS, BA-SPEC, API-CONTRACT, CONVENTIONS, DATABASE)

## Build & Run

```bash
dotnet restore
dotnet build AppPickleball.slnx
dotnet run --project AppPickleball.Api

# Docker
docker-compose up -d
```

**Solution file:** `AppPickleball.slnx` (.NET 10)
**Database:** PostgreSQL 16 — Dùng **EF Core Migrations** (`Persistence/Migrations/`). Auto-apply khi startup.

**Tạo migration mới:**
```bash
dotnet ef migrations add <TenMigration> --project AppPickleball.Infrastructure --startup-project AppPickleball.Api --output-dir Persistence/Migrations
dotnet ef database update --project AppPickleball.Infrastructure --startup-project AppPickleball.Api
```

## Architecture

```
AppPickleball/
├── AppPickleball.Api/            # Presentation — Controllers, Middleware, Configurations
├── AppPickleball.Application/    # Business Logic — CQRS Commands/Queries/Handlers (MediatR)
├── AppPickleball.Domain/         # Domain — Entities, Enums, BaseEntity
├── AppPickleball.Infrastructure/ # Data Access — EF Core, Repositories, External Services
├── Shared.Kernel/                # Shared — ApiResponse wrapper, Localization, CorrelationId
└── AppPickleball.Tests/          # Tests — xUnit, FluentAssertions, Moq
```

### Layer Dependencies
- `Api` → `Application`, `Infrastructure`, `Shared.Kernel`
- `Application` → `Domain`, `Shared.Kernel`
- `Infrastructure` → `Application`, `Domain`
- `Domain` — không phụ thuộc gì (pure domain)

## Rules Bắt Buộc

### DateTime
- Luôn dùng `DateTime.UtcNow` — KHÔNG dùng `DateTime.Now`
- EF Config default: `HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'")`
- PostgreSQL column type: `TIMESTAMPTZ`

### Mapping
- **KHÔNG dùng AutoMapper** — manual mapping trong Handler

### Repository (ISP)
- `IRepository<T>` — CRUD cho mọi entity
- `ISoftDeletableRepository<T>` — chỉ cho `BaseEntity` (SoftDelete, Restore)
- Entity kế thừa `BaseEntity` → repository implement cả hai interface
- Entity kế thừa `BaseCreatedEntity` → chỉ implement `IRepository<T>`
- KHÔNG gọi `SaveChanges` trong repository — luôn qua `IUnitOfWork`

### API Response
- Luôn wrap trong `ApiResponse<T>` — không trả raw object
- Messages dùng `IStringLocalizer<SharedResource>` — không hardcode string

### Code Style
- 1 class = 1 file
- Controllers thin — delegate logic cho MediatR Handler
- Vietnamese comments OK

## Key Patterns

### CQRS với MediatR
Files trong `AppPickleball.Application/Features/{Domain}/{Commands|Queries}/{Name}/`:
```
{Name}Command.cs          — record IRequest<ApiResponse<T>>
{Name}CommandHandler.cs   — IRequestHandler<,>
{Name}CommandValidator.cs — AbstractValidator<> (FluentValidation)
```

Pipeline: `ValidationBehaviour` → `LoggingBehaviour` → Handler

### Repository + UnitOfWork
```csharp
// BaseEntity repos
public interface IUserRepository : IRepository<User>, ISoftDeletableRepository<User> {}

// BaseCreatedEntity repos (junction/token tables)
public interface IRefreshTokenRepository : IRepository<RefreshToken> {}
```

### EF Core Conventions
- PostgreSQL snake_case: `HasColumnName("full_name")`
- Default Id: `HasDefaultValueSql("gen_random_uuid()")`
- Global query filter: `x.DeletedAt == null` cho mọi `BaseEntity`
- Enum: `HasConversion<string>()`

### ApiResponse Pattern
```csharp
ApiResponse<T>.SuccessResponse(data, message)
ApiResponse<T>.FailureResponse(message, statusCode, errorCodes)
```

### Exception Handling
| Exception | HTTP | Error Code |
|-----------|------|------------|
| `ValidationException` | 400 | VALIDATION_ERROR |
| `NotFoundException` | 404 | NOT_FOUND |
| `DomainException` | 400 | DOMAIN_ERROR |
| Unhandled | 500 | INTERNAL_SERVER_ERROR |

### JWT & Auth
- Access token: 15 phút
- Refresh token: 7 ngày (stored in DB table `refresh_tokens`)
- SignalR: token từ query string `?access_token=...`
- `ICurrentUserService.UserId` — từ JWT claim `NameIdentifier`

## Domain Model (Pickleball)

| Entity | Base | Mô tả |
|--------|------|-------|
| `User` | BaseEntity | Tài khoản người dùng |
| `RefreshToken` | BaseCreatedEntity | Refresh token rotation |
| `Tournament` | BaseEntity | Giải đấu |
| `TournamentParticipant` | BaseCreatedEntity | Người tham gia giải |
| `Match` | BaseEntity | Trận đấu trong giải |
| `MatchScore` | BaseCreatedEntity | Điểm số từng set |
| `CommunityGame` | BaseEntity | Game cộng đồng |
| `Notification` | BaseCreatedEntity | Thông báo in-app |
| `ChatMessage` | BaseCreatedEntity | Tin nhắn chat |

## Thêm Feature Mới

1. **Domain** — entity trong `AppPickleball.Domain/Entities/`
2. **EF Config** — `IEntityTypeConfiguration<T>` trong `Infrastructure/Persistence/Configurations/`
3. **DbContext** — thêm `DbSet<T>` vào `AppPickleballDbContext`
4. **Repository interface** — trong `Application/Common/Interfaces/`
5. **Repository impl** — trong `Infrastructure/Persistence/Repositories/`
6. **DI** — đăng ký trong `Infrastructure/DependencyInjection.cs`
7. **Command/Query + Handler + Validator** — trong `Application/Features/{Domain}/`
8. **Controller** — kế thừa `BaseApiController`, dùng `_mediator.Send()`

## Đọc Thêm
- Business rules: `../Documents/BA-SPEC/`
- API contracts: `../Documents/API-CONTRACT/`
- Flows chi tiết: `../Documents/MAINFLOWS/`
- Database schema: `../Documents/DATABASE/`
- SignalR hubs: `../Documents/REALTIME/SignalR_Contracts.md`

## Agent Skills

| Skill | Dùng khi |
|-------|---------|
| `/build-api` | Tạo API endpoint mới |
| `/api-test` | Test live HTTP endpoints |
| `/api-fix` | Fix API errors tự động |
| `/create-entity` | Tạo entity + EF Config |
