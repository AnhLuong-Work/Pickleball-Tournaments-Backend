# ─── Stage 1: Base runtime image ───────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# ─── Stage 2: Build ─────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy csproj và restore (tận dụng Docker layer cache)
COPY ["AppPickleball.Api/AppPickleball.Api.csproj", "AppPickleball.Api/"]
COPY ["AppPickleball.Application/AppPickleball.Application.csproj", "AppPickleball.Application/"]
COPY ["AppPickleball.Domain/AppPickleball.Domain.csproj", "AppPickleball.Domain/"]
COPY ["AppPickleball.Infrastructure/AppPickleball.Infrastructure.csproj", "AppPickleball.Infrastructure/"]
COPY ["Shared.Kernel/Shared.Kernel.csproj", "Shared.Kernel/"]

RUN dotnet restore "AppPickleball.Api/AppPickleball.Api.csproj"

# Copy toàn bộ source
COPY . .

WORKDIR "/src/AppPickleball.Api"
RUN dotnet build "AppPickleball.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# ─── Stage 3: Publish ────────────────────────────────────────────────────────────
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "AppPickleball.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# ─── Stage 4: Final ──────────────────────────────────────────────────────────────
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AppPickleball.Api.dll"]
