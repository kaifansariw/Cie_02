# ==============================================================================
# Dockerfile
# Online Shopping Product Management System (ASP.NET Core MVC - .NET 10)
# Multi-stage build for lightweight, production-ready container deployment
# ==============================================================================

# Stage 1: Build & Publish
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

# Copy project file and restore dependencies
COPY ["OnlineShoppingSystem.csproj", "./"]
RUN dotnet restore "OnlineShoppingSystem.csproj"

# Copy the rest of the source code
COPY . .

# Build and publish release binaries
RUN dotnet publish "OnlineShoppingSystem.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS final
WORKDIR /app
COPY --from=build /app/publish .

# Configure ASP.NET Core environment and listening port
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "OnlineShoppingSystem.dll"]
