# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["./ExpenseTracker.Domain/ExpenseTracker.Domain.csproj", "ExpenseTracker.Domain/"]
COPY ["./ExpenseTracker.Application/ExpenseTracker.Application.csproj", "ExpenseTracker.Application/"]
COPY ["./ExpenseTracker.Infrastructure/ExpenseTracker.Infrastructure.csproj", "ExpenseTracker.Infrastructure/"]
COPY ["./ExpenseTracker.Api/ExpenseTracker.Api.csproj", "ExpenseTracker.Api/"]

RUN dotnet restore "ExpenseTracker.Api/ExpenseTracker.Api.csproj"

COPY . .
RUN dotnet build "ExpenseTracker.Api/ExpenseTracker.Api.csproj" -c Debug -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "ExpenseTracker.Api/ExpenseTracker.Api.csproj" -c Debug -o /app/publish /p:UseAppHost=false

# Runtime stage (using SDK for debugger support)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS runtime
WORKDIR /app
COPY --from=publish /app/publish .

# Install vsdbg debugger
RUN apt-get update && apt-get install -y curl unzip && \
    curl -sSL https://aka.ms/getvsdbgsh | /bin/sh /dev/stdin -v latest -l /vsdbg && \
    apt-get clean && rm -rf /var/lib/apt/lists/*

# Expose application ports and debugger port
EXPOSE 8080 8443 5005

# Environment variables for debugging
ENV ASPNETCORE_ENVIRONMENT=Development
ENV DOTNET_USE_POLLING_FILE_WATCHER=1
ENV DOTNET_WATCH_SUPPRESS_LAUNCH_BROWSER=1

ENTRYPOINT ["dotnet", "ExpenseTracker.Api.dll"]
