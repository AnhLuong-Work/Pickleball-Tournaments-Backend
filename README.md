# AppPickleball — Backend API

> Hệ thống quản lý giải đấu Pickleball — đăng ký, tham gia giải, xếp bảng, nhập điểm, cộng đồng.

---

## Mục Lục

- [Giới Thiệu](#giới-thiệu)
- [Cấu Trúc Project](#cấu-trúc-project)
- [Bắt Đầu Nhanh](#bắt-đầu-nhanh)
- [Database & Migration](#database--migration)
- [Docker](#docker)
- [Công Nghệ Sử Dụng](#công-nghệ-sử-dụng)
- [Các Lệnh Thường Dùng](#các-lệnh-thường-dùng)
- [Best Practices](#best-practices)
- [Tài Liệu](#tài-liệu)

---

## Giới Thiệu

**AppPickleball Backend** là REST API xây dựng theo **Clean Architecture + CQRS** với .NET 10:

- Clean Architecture — 5 tầng tách biệt rõ ràng
- CQRS Pattern — Command/Query tách biệt qua MediatR
- EF Core Migrations — Quản lý schema database tự động
- JWT Auth — Access token (15 phút) + Refresh token rotation (7 ngày)
- Swagger/OpenAPI — Tài liệu API tự động với annotations
- PostgreSQL — Database chính với UUID primary keys
- FluentValidation — Validation pipeline tự động
- Serilog — Structured logging

---

## Cấu Trúc Project

```
AppPickleball/
├── AppPickleball.Api/            # Presentation — Controllers, Middleware, DI Config
├── AppPickleball.Application/    # Business Logic — CQRS Commands/Queries (MediatR)
├── AppPickleball.Domain/         # Domain — Entities, Enums, BaseEntity
├── AppPickleball.Infrastructure/ # Data Access — EF Core, Repositories, Email, JWT
├── Shared.Kernel/                # Shared — ApiResponse wrapper, Wrappers
└── AppPickleball.Tests/          # Tests — xUnit
```

### Layer Dependencies

| Tầng | Phụ thuộc |
|------|-----------|
| **Domain** | Không phụ thuộc gì |
| **Application** | Domain, Shared.Kernel |
| **Infrastructure** | Application, Domain |
| **Api** | Application, Infrastructure, Shared.Kernel |

---

## Bắt Đầu Nhanh

### Yêu Cầu

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- PostgreSQL 16+ (hoặc Docker)
- dotnet-ef CLI: `dotnet tool install --global dotnet-ef`

### Chạy Local

```bash
# 1. Clone & restore
dotnet restore

# 2. Cấu hình connection string trong appsettings.Development.json
# (xem mục Cấu Hình bên dưới)

# 3. Tạo database và apply migrations
dotnet ef database update --project AppPickleball.Infrastructure --startup-project AppPickleball.Api

# 4. Chạy API
dotnet run --project AppPickleball.Api
```

API sẽ chạy tại:
- `http://localhost:5000`
- Swagger UI: `http://localhost:5000/swagger`

> **Lưu ý:** Khi chạy bình thường (không Docker), migrations được tự động apply khi API khởi động.

---

## Database & Migration

Project sử dụng **EF Core Migrations** để quản lý schema database.

### Cấu Hình Connection String

`AppPickleball.Api/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=apppickleball_db;Username=pickleball_user;Password=pickleball_pass"
  }
}
```

### Các Lệnh Migration

```bash
# Tạo migration mới (sau khi thay đổi Entity/Configuration)
dotnet ef migrations add <TenMigration> \
  --project AppPickleball.Infrastructure \
  --startup-project AppPickleball.Api \
  --output-dir Persistence/Migrations

# Apply migration lên database
dotnet ef database update \
  --project AppPickleball.Infrastructure \
  --startup-project AppPickleball.Api

# Xem danh sách migrations
dotnet ef migrations list \
  --project AppPickleball.Infrastructure \
  --startup-project AppPickleball.Api

# Rollback về migration cụ thể
dotnet ef database update <TenMigration> \
  --project AppPickleball.Infrastructure \
  --startup-project AppPickleball.Api

# Xóa migration cuối (chưa apply)
dotnet ef migrations remove \
  --project AppPickleball.Infrastructure \
  --startup-project AppPickleball.Api
```

### Auto-Migrate khi Startup

API tự động apply pending migrations khi khởi động (cả khi chạy Docker):

```csharp
// Program.cs — tự động khi app start
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppPickleballDbContext>();
    await db.Database.MigrateAsync();
}
```

---

## Docker

### Chạy với Docker Compose (Khuyến nghị cho Dev)

```bash
# Khởi động PostgreSQL + API
docker-compose up -d

# Xem logs
docker-compose logs -f api

# Dừng
docker-compose down

# Dừng và xóa data
docker-compose down -v
```

Services:
| Service | URL | Mô tả |
|---------|-----|-------|
| API | `http://localhost:5000` | REST API |
| Swagger | `http://localhost:5000/swagger` | API Docs |
| PostgreSQL | `localhost:5433` | DB (port 5433 tránh xung đột local) |

DB credentials mặc định (chỉ dùng cho dev):
- Database: `apppickleball_db`
- Username: `pickleball_user`
- Password: `pickleball_pass`

### Build Docker Image riêng

```bash
# Build
docker build -f AppPickleball.Api/Dockerfile -t apppickleball-api:latest .

# Chạy
docker run -p 5000:8080 \
  -e ConnectionStrings__DefaultConnection="Host=localhost;..." \
  apppickleball-api:latest
```

---

## Công Nghệ Sử Dụng

| Loại | Công Nghệ | Version |
|------|-----------|---------|
| Framework | .NET | 10 |
| API | ASP.NET Core Web API | 10 |
| ORM | Entity Framework Core | 10 |
| Database | PostgreSQL | 16 |
| CQRS | MediatR | latest |
| Validation | FluentValidation | latest |
| Documentation | Swashbuckle (Swagger) | 9.x |
| Logging | Serilog | latest |
| Auth | JWT Bearer | latest |
| Container | Docker | — |
| Testing | xUnit + Moq | latest |

---

## Các Lệnh Thường Dùng

```bash
# Build
dotnet clean && dotnet restore && dotnet build AppPickleball.slnx

# Run (auto-migrate on start)
dotnet run --project AppPickleball.Api

# Watch mode
dotnet watch --project AppPickleball.Api

# Tests
dotnet test

# Xóa bin/obj
Get-ChildItem -Include bin,obj -Recurse -Directory | Remove-Item -Recurse -Force
```

---

## Best Practices

### Thêm Feature Mới (CQRS)

1. **Domain** — Thêm/sửa Entity trong `AppPickleball.Domain/Entities/`
2. **EF Config** — `IEntityTypeConfiguration<T>` trong `Infrastructure/Persistence/Configurations/`
3. **Migration** — `dotnet ef migrations add <Name> --project AppPickleball.Infrastructure --startup-project AppPickleball.Api --output-dir Persistence/Migrations`
4. **Repository** — Interface trong `Application/Common/Interfaces/`, impl trong `Infrastructure/Persistence/Repositories/`
5. **Command/Query** — `Application/Features/{Domain}/{Commands|Queries}/{Name}/`
6. **Controller** — Kế thừa `BaseApiController`, thin controller, delegate qua `_mediator.Send()`

### Quy Tắc Bắt Buộc

| Rule | Chi tiết |
|------|---------|
| DateTime | Luôn `DateTime.UtcNow` — KHÔNG `DateTime.Now` |
| Mapping | Map thủ công trong Handler — KHÔNG AutoMapper |
| SaveChanges | Chỉ gọi qua `IUnitOfWork` — KHÔNG trong Repository |
| Response | Luôn wrap `ApiResponse<T>` — KHÔNG trả raw object |
| Controller | Thin — chỉ gọi `_mediator.Send()`, không có logic |

---

## Tài Liệu

| Tài liệu | Đường dẫn |
|-----------|-----------|
| API Contracts | `Documents/API-CONTRACT/` |
| Business Spec | `Documents/BA-SPEC/` |
| Database Schema | `Documents/DATABASE/` |
| Main Flows | `Documents/MAINFLOWS/` |
| API Checker | `Documents/API-DEVELOP-CHECKER.md` |
| Coding Convention | `Documents/Coding_Convention_DotNet_PostgreSQL.md` |

---

**MIT License**
