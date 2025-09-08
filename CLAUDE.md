# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Architecture Overview

This is a .NET 9.0 solution using .NET Aspire for cloud-native application development. The solution follows Clean Architecture principles with Domain-Driven Design patterns:

### Core Projects
- **Tasks.Poc.Domain** - Core domain entities, value objects, and business rules
- **Tasks.Poc.Application** - Application services, CQRS handlers, and interfaces using MediatR

### Infrastructure Projects  
- **Tasks.Poc.Infrastructure** - Data persistence with Entity Framework Core, PostgreSQL, and repository implementations

### Foundation Projects
- **Tasks.Poc.Contracts** - Shared contracts and DTOs across layers
- **Tasks.Poc.Constants** - Application-wide constants and configuration values
- **Tasks.Poc.SharedKernel** - Common domain primitives and shared abstractions

### Presentation Projects
- **Tasks.Poc.AppHost** - Aspire application host orchestrating services and PostgreSQL
- **Tasks.Poc.ApiCore** - ASP.NET Core Web API with RESTful endpoints for todo management
- **Tasks.Poc.Web** - Blazor Server application serving as the web frontend
- **Tasks.Poc.ServiceDefaults** - Shared service configuration and extensions
- **Tasks.Poc.E2eTests** - End-to-end tests using Aspire.Hosting.Testing
- **Tasks.Poc.ArchTests** - Architecture tests to enforce coding standards and design principles

### Domain Model
The application manages a todo list system with:
- **Users** with email addresses and names
- **TodoLists** owned by users with titles and descriptions
- **TodoItems** within lists with priorities, due dates, and completion status
- **Value Objects** with implicit operators for seamless conversions (EntityId, UserName, Email, TodoTitle, etc.)
- **Domain Events** for business logic notifications

## Common Commands

### Build and Run
```bash
# Build the entire solution
dotnet build Tasks.Poc.sln

# Run the application host (starts PostgreSQL + API + Web services)
dotnet run --project src/aspire/Tasks.Poc.AppHost

# Build specific project
dotnet build src/services/apicore/Tasks.Poc.ApiCore
dotnet build src/clients/web/Tasks.Poc.Web
```

### Database Management
```bash
# Create new migration (stored in Persistence/Migrations)
dotnet ef migrations add MigrationName --project src/services/apicore/Tasks.Poc.Infrastructure --startup-project src/services/apicore/Tasks.Poc.ApiCore --context TodoDbContext --output-dir Persistence/Migrations

# Remove last migration
dotnet ef migrations remove --project src/services/apicore/Tasks.Poc.Infrastructure --startup-project src/services/apicore/Tasks.Poc.ApiCore --context TodoDbContext

# Generate SQL deployment scripts for production
dotnet ef migrations script --project src/services/apicore/Tasks.Poc.Infrastructure --startup-project src/services/apicore/Tasks.Poc.ApiCore --context TodoDbContext --output src/services/apicore/Tasks.Poc.Infrastructure/Persistence/Scripts/full-database-script.sql

# Generate incremental SQL script from specific migration
dotnet ef migrations script InitialCreate --project src/services/apicore/Tasks.Poc.Infrastructure --startup-project src/services/apicore/Tasks.Poc.ApiCore --context TodoDbContext --output src/services/apicore/Tasks.Poc.Infrastructure/Persistence/Scripts/incremental-script.sql

# Update database manually (development only)
dotnet ef database update --project src/services/apicore/Tasks.Poc.Infrastructure --startup-project src/services/apicore/Tasks.Poc.ApiCore --context TodoDbContext
```

### Testing
```bash
# Run all tests
dotnet test

# Run tests for specific project
dotnet test src/tests/Tasks.Poc.E2eTests
dotnet test src/tests/Tasks.Poc.ArchTests

# Run tests with detailed output
dotnet test --verbosity normal
```

### Development
```bash
# Watch mode for API service
dotnet watch --project src/services/apicore/Tasks.Poc.ApiCore

# Watch mode for web frontend
dotnet watch --project src/clients/web/Tasks.Poc.Web
```

## Project Structure

### Service Communication
- The Web frontend (`webfrontend`) communicates with the API service (`apiservice`) through HTTP
- Services are configured in `src/aspire/Tasks.Poc.AppHost/AppHost.cs` with health checks and service references
- PostgreSQL database with persistent data volume and pgWeb management UI
- The API service exposes RESTful endpoints for user and todo management
- Service discovery and dependency management handled automatically by Aspire

### Database Architecture
- **PostgreSQL** as the primary database with Aspire hosting
- **Entity Framework Core** with Code-First migrations stored in `Persistence/Migrations/`
- **Custom migrations history**: `system.TwarzPocMigrationsHistory` table in separate schema
- **Environment-specific approach**: 
  - **Development**: Automatic seeding with rich fake data using Bogus library
  - **Production/Staging**: Manual migration via SQL scripts for blue-green deployments
- **SQL deployment scripts**: Generated in `Persistence/Scripts/` for production use

### Key Files
- `src/aspire/Tasks.Poc.AppHost/AppHost.cs` - Service orchestration with PostgreSQL configuration
- `src/services/apicore/Tasks.Poc.ApiCore/Program.cs` - API service configuration with Clean Architecture setup
- `src/services/apicore/Tasks.Poc.Infrastructure/DependencyInjection.cs` - Database provider configuration
- `src/services/apicore/Tasks.Poc.Infrastructure/Persistence/` - DbContext, configurations, migrations, and seeders
  - `Migrations/` - EF Core migration files with custom directory structure
  - `Scripts/` - SQL deployment scripts for production blue-green deployments
  - `Seeders/` - Development-only data seeding with Bogus library
  - `Configurations/` - Entity Framework entity configurations
- `src/services/apicore/Tasks.Poc.Domain/` - Domain entities and value objects with implicit operators
- `src/services/apicore/Tasks.Poc.Application/` - CQRS handlers using MediatR pattern
- `src/fundations/Tasks.Poc.Contracts/` - Shared contracts and DTOs
- `src/fundations/Tasks.Poc.Constants/` - Application constants and configuration values
- `src/fundations/Tasks.Poc.SharedKernel/` - Common domain primitives and shared abstractions

### API Endpoints
The API service provides RESTful endpoints for:
- **User Management**: `GET/POST /api/users`, `GET /api/users/{id}`
- **Todo Lists**: `GET/POST /api/users/{userId}/todos`, `GET /api/todos/{todoListId}`  
- **Todo Items**: `POST /api/todos/{todoListId}/items`, `PUT /api/todos/{todoListId}/items/{itemId}/complete`

### Testing Strategy
- **End-to-end tests** use `Aspire.Hosting.Testing` framework in `Tasks.Poc.E2eTests`
- **Architecture tests** validate design principles and coding standards in `Tasks.Poc.ArchTests`
- Tests validate end-to-end service communication
- Health checks ensure services are ready before testing
- Separate test database configuration using InMemory provider

## Development Notes

- All projects target .NET 9.0 with nullable reference types enabled
- **Clean Architecture** with Domain-Driven Design principles
- **CQRS pattern** using MediatR for command/query separation
- **Value Objects** with implicit operators for seamless string/GUID conversions
- **Aspire orchestration** for PostgreSQL, API, and Web services
- **Environment-specific database management**:
  - **Development**: Automatic database creation and seeding with fake data
  - **Production**: Manual SQL script deployment for controlled blue-green deployments
- **Custom migrations history table**: `system.TwarzPocMigrationsHistory` in separate schema
- Health checks configured for all services at `/health` endpoints

### Code style
- alwasy use file-scoped namespaces
- usings always go inside namespace

## Database Schema

### Users Table
- Id (uuid), Name (varchar), Email (varchar), CreatedAt, LastLoginAt
- **Indexes**: Unique index on Email, Performance index on CreatedAt

### TodoLists Table  
- Id (uuid), Title (varchar), Description (varchar), OwnerId (FK to Users), CreatedAt, UpdatedAt

### TodoItems Table
- Id (uuid), Title (varchar), Description (varchar), IsCompleted, Priority (enum), CreatedAt, CompletedAt, DueDate, TodoListId (FK to TodoLists)

### System Schema
- **TwarzPocMigrationsHistory** (system schema): Custom migrations history table for tracking database version

## Deployment Strategy

### Development Environment
- Automatic database creation via `EnsureCreatedAsync()`
- Rich fake data seeding using Bogus library (10-15 users, realistic todo lists and items)
- Runs `DevelopmentSeedingService` on application startup

### Production/Staging Environment  
- **Blue-Green Deployment Ready**: Use SQL scripts from `Persistence/Scripts/`
- **Full Deployment**: Execute `full-database-script.sql` for new environments
- **Incremental Updates**: Use migration-specific scripts for existing databases
- **No Automatic Seeding**: Production databases remain clean
- **Custom History Tracking**: Migrations tracked in `system.TwarzPocMigrationsHistory`