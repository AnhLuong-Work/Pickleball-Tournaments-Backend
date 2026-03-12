# AppPickleball - Operator & Admin Management

> **🎯 Mục đích:** Microservice quản lý Workspace, Subscriptions, Orders và Operator Panel, xây dựng theo Clean Architecture + CQRS pattern.

---

## 📋 Mục Lục

- [Giới Thiệu](#giới-thiệu)
- [Cấu Trúc Project](#cấu-trúc-project)
- [Bắt Đầu Nhanh](#bắt-đầu-nhanh)
- [Công Nghệ Sử Dụng](#công-nghệ-sử-dụng)
- [Tài Liệu](#tài-liệu)

---

## 🎯 Giới Thiệu

**AppPickleball** là microservice quản lý workspace, gói cước, đơn hàng và vận hành hệ thống:

- ✅ Clean Architecture — 5 tầng rõ ràng
- ✅ CQRS Pattern — Tách biệt Command/Query với MediatR
- ✅ FluentValidation — Validation mạnh mẽ + localization
- ✅ Repository + Unit of Work — Data access pattern
- ✅ RabbitMQ (MassTransit) — Message queue
- ✅ PostgreSQL — Database với UUID PKs
- ✅ Multi-language — vi, en (IStringLocalizer)
- ✅ Multi-tenant — Workspace isolation
- ✅ Soft Delete — GlobalQueryFilter

---

## 🏗️ Cấu Trúc Project

```
AppPickleball/
├── AppPickleball.Domain/          # Core — Entities, Enums, BaseEntity (no dependencies)
├── AppPickleball.Application/     # Business logic — CQRS Commands/Queries/Handlers (MediatR)
├── AppPickleball.Infrastructure/  # Data access — EF Core, Repositories, External services
├── AppPickleball.Share/           # Shared — ApiResponse wrapper, Localization resources
├── AppPickleball.Api/             # Presentation — Controllers, Middleware, Configurations
└── Documents/                  # 📚 Tài liệu (Database schema, Coding convention, API docs)
```

### Trách Nhiệm Từng Tầng

| Tầng | Trách Nhiệm | Phụ Thuộc |
|------|-------------|-----------|
| **Domain** | Entities, Enums, Base classes | Không phụ thuộc gì |
| **Application** | Use cases, CQRS, Validation, Interfaces | Domain, Share |
| **Infrastructure** | EF Core, Repositories, Email, Token services | Domain, Application |
| **Share** | ApiResponse wrapper, Localization resources | Không phụ thuộc gì |
| **Api** | Controllers, Middleware, DI Config | Application, Infrastructure |

---

## 🚀 Bắt Đầu Nhanh

### Yêu Cầu

- .NET 10 SDK
- PostgreSQL (hoặc Docker)
- RabbitMQ (tùy chọn)

### Chạy Local

```powershell
# 1. Restore dependencies
dotnet restore

# 2. Cập nhật connection string trong appsettings.json

# 3. Tạo database bằng SQL script
# Chạy Documents/Database_Documents/create_operator_database.sql trên PostgreSQL

# 4. Chạy ứng dụng
dotnet run --project AppPickleball.Api
```

> ⚠️ **Lưu ý:** Project **KHÔNG** sử dụng EF Core Migrations. Schema được quản lý thủ công qua SQL scripts trong `Documents/Database_Documents/`.

### Chạy Với Docker

```powershell
docker-compose up -d
```

---

## 💻 Phát Triển

### Thêm Feature Mới (CQRS Pattern)

#### 1. Tạo Entity (Domain)

```csharp
// AppPickleball.Domain/Entities/Product.cs
public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}
```

#### 2. Tạo Command + Handler (Application)

```csharp
// Command
public record CreateProductCommand(string Name, decimal Price) : IRequest<ApiResponse<Guid>>;

// Handler
public class CreateProductCommandHandler(
    IRepository<Product> repository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateProductCommand, ApiResponse<Guid>>
{
    public async Task<ApiResponse<Guid>> Handle(CreateProductCommand request, CancellationToken ct)
    {
        var product = new Product { Name = request.Name, Price = request.Price };
        await repository.AddAsync(product, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return ApiResponse<Guid>.SuccessResponse(product.Id, "Product created");
    }
}

// Validator
public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator(IStringLocalizer<SharedResource> localizer)
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage(localizer["Validation_Name_Required"]);
        RuleFor(x => x.Price).GreaterThan(0).WithMessage(localizer["Validation_Price_Positive"]);
    }
}
```

#### 3. Tạo Controller (API)

```csharp
[Route("api/products")]
[ApiController]
public class ProductsController : BaseApiController
{
    public ProductsController(IMediator mediator, ILogger<BaseApiController> logger)
        : base(mediator, logger) { }

    [HttpPost]
    [SwaggerOperation(Summary = "Tạo sản phẩm")]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Create([FromBody] CreateProductCommand command)
    {
        var result = await _mediator.Send(command);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
```

---

## 🛠️ Công Nghệ Sử Dụng

| Loại | Công Nghệ | Mục Đích |
|------|-----------|----------|
| **Framework** | .NET 10 | Nền tảng |
| **Architecture** | Clean Architecture | Tách biệt concerns |
| **API** | ASP.NET Core Web API | RESTful API |
| **ORM** | Entity Framework Core 10 | Database access |
| **Database** | PostgreSQL | Primary database |
| **CQRS** | MediatR | Command/Query separation |
| **Validation** | FluentValidation | Input validation |
| **Message Queue** | RabbitMQ + MassTransit | Async messaging |
| **Documentation** | Swagger/OpenAPI | API docs |
| **Container** | Docker | Deployment |
| **Logging** | Serilog | Structured logging |

---

## ⚙️ Cấu Hình

### Database

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=operator_service_db;Username=operator_user;Password=yourpassword"
  }
}
```

### RabbitMQ

```json
{
  "RabbitMQ": {
    "Host": "localhost",
    "Port": 5672,
    "Username": "guest",
    "Password": "guest"
  }
}
```

---

## 📝 Các Lệnh Thường Dùng

```powershell
# Build & Run
dotnet clean
dotnet restore
dotnet build
dotnet run --project AppPickleball.Api
dotnet watch --project AppPickleball.Api  # Auto-reload

# Docker
docker build -f AppPickleball.Api/Dockerfile -t AppPickleball-api .
docker-compose up -d
```

---

## 🎯 Best Practices

### ✅ NÊN

- Tuân thủ Clean Architecture
- Sử dụng CQRS cho features
- Validate input với FluentValidation
- Map DTOs thủ công (không dùng AutoMapper)
- Async/await cho I/O operations
- Dependency injection
- Controllers mỏng (delegate cho MediatR)
- `DateTime.UtcNow` cho tất cả timestamps
- `IStringLocalizer` cho user-facing messages

### ❌ KHÔNG NÊN

- Reference Infrastructure từ Domain
- Business logic trong Controllers
- Sử dụng entities trực tiếp trong responses
- Hardcode configuration
- Bỏ qua validation
- Mix concerns giữa các tầng
- Dùng `DateTime.Now` (luôn dùng `DateTime.UtcNow`)
- Gọi `SaveChanges` trong Repository (dùng UnitOfWork)

---

## 📚 Tài Liệu

| Tài liệu | Đường dẫn | Mô tả |
|-----------|-----------|-------|
| Database Schema | `Documents/Database_Documents/` | ERD, SQL scripts |
| Coding Convention | `Documents/Coding_Convention_DotNet_PostgreSQL.md` | Quy chuẩn code |
| API Functions | `Documents/API_Documents/` | Danh sách API |
| Agent Skills Guide | `Documents/Agent_Skills_and_Team_Guide.md` | Hướng dẫn AI coding |

---

## 🔧 Xử Lý Lỗi

### Lỗi Build

```powershell
# Xóa bin và obj
Get-ChildItem -Include bin,obj -Recurse -Directory | Remove-Item -Recurse -Force

# Restore và rebuild
dotnet restore
dotnet build
```

### Lỗi Database

1. Kiểm tra PostgreSQL đang chạy
2. Xác minh connection string
3. Chạy SQL script trong `Documents/Database_Documents/create_operator_database.sql`

---

**📄 MIT License** — Tự do sử dụng và chỉnh sửa
