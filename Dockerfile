# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["./backend/ExpenseTracker.Domain/ExpenseTracker.Domain.csproj", "ExpenseTracker.Domain/"]
COPY ["./backend/ExpenseTracker.Application/ExpenseTracker.Application.csproj", "ExpenseTracker.Application/"]
COPY ["./backend/ExpenseTracker.Infrastructure/ExpenseTracker.Infrastructure.csproj", "ExpenseTracker.Infrastructure/"]
COPY ["./backend/ExpenseTracker.Api/ExpenseTracker.Api.csproj", "ExpenseTracker.Api/"]

RUN dotnet restore "./backend/ExpenseTracker.Api/ExpenseTracker.Api.csproj"

COPY . .
RUN dotnet build "./backend/ExpenseTracker.Api/ExpenseTracker.Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "./backend/ExpenseTracker.Api/ExpenseTracker.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=publish /app/publish .

EXPOSE 8080 8443
ENTRYPOINT ["dotnet", "ExpenseTracker.Api.dll"]
