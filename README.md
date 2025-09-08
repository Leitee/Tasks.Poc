# Tasks.Poc - Todo Management System

A modern todo management system built with .NET 9.0, demonstrating Clean Architecture principles with Domain-Driven Design patterns, powered by .NET Aspire for cloud-native development.

## Features

### Core Functionality
- **User Management** - Create and manage user accounts with email addresses
- **Todo Lists** - Organize tasks into categorized lists with titles and descriptions
- **Todo Items** - Manage individual tasks with priorities, due dates, and completion tracking
- **Real-time Updates** - Blazor Server frontend with immediate UI feedback

### Technical Highlights
- **Clean Architecture** with Domain-Driven Design patterns
- **CQRS** implementation using MediatR for command/query separation
- **Comprehensive Logging** with Serilog + OpenTelemetry for full observability
- **Cloud-Native** development with .NET Aspire orchestration
- **PostgreSQL** database with Entity Framework Core
- **Two-Phase Logging** architecture for startup and runtime correlation
- **Performance Monitoring** with automatic slow operation detection

## Quick Start

### Prerequisites
- .NET 9.0 SDK
- Docker (for PostgreSQL via Aspire)
- Visual Studio 2022 or VS Code

### Running the Application

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd Tasks.Poc
   ```

2. **Build the solution**
   ```bash
   dotnet build Tasks.Poc.sln
   ```

3. **Run with Aspire (Recommended)**
   ```bash
   dotnet run --project src/aspire/Tasks.Poc.AppHost
   ```
   This starts:
   - PostgreSQL database with pgWeb management UI
   - API service on configured port
   - Blazor web frontend
   - Service discovery and health checks

4. **Access the application**
   - **Web Frontend**: Check Aspire dashboard for the web application URL
   - **API Endpoints**: Check Aspire dashboard for the API service URL
   - **Aspire Dashboard**: Usually at `https://localhost:15888`

### Development Workflow

1. **Database Migrations**
   ```bash
   # Create new migration
   dotnet ef migrations add MigrationName --project src/services/apicore/Tasks.Poc.Infrastructure --startup-project src/services/apicore/Tasks.Poc.ApiCore --context TodoDbContext --output-dir Persistence/Migrations

   # Apply migrations (development only - production uses SQL scripts)
   dotnet ef database update --project src/services/apicore/Tasks.Poc.Infrastructure --startup-project src/services/apicore/Tasks.Poc.ApiCore --context TodoDbContext
   ```

2. **Running Tests**
   ```bash
   # Run all tests
   dotnet test

   # Run specific test projects
   dotnet test src/tests/Tasks.Poc.E2eTests
   dotnet test src/tests/Tasks.Poc.ArchTests
   ```

3. **Development Servers**
   ```bash
   # Watch mode for API service
   dotnet watch --project src/services/apicore/Tasks.Poc.ApiCore

   # Watch mode for web frontend
   dotnet watch --project src/clients/web/Tasks.Poc.Web
   ```

## API Endpoints

The API service provides RESTful endpoints for todo management:

### User Management
- `POST /api/users` - Create a new user
- `GET /api/users` - Get all users
- `GET /api/users/{id}` - Get user by ID

### Todo Lists
- `POST /api/users/{userId}/todos` - Create a todo list for user
- `GET /api/users/{userId}/todos` - Get user's todo lists
- `GET /api/todos/{todoListId}` - Get todo list with items

### Todo Items
- `POST /api/todos/{todoListId}/items` - Add item to todo list
- `PUT /api/todos/{todoListId}/items/{itemId}/complete` - Mark item as complete

## Domain Model

The application manages a hierarchical todo system:

### Entities
- **Users** - Have email addresses, names, creation timestamps, and login tracking
- **TodoLists** - Owned by users with titles, descriptions, and timestamps
- **TodoItems** - Belong to lists with titles, descriptions, priorities, due dates, and completion status

### Value Objects
All domain entities use value objects with implicit operators for seamless conversions:
- `EntityId` for strongly-typed entity identifiers
- `UserName`, `Email` for user properties
- `TodoTitle`, `TodoDescription` for todo content
- Domain events for business logic notifications

## Database Schema

### Core Tables
- **Users** - Id (uuid), Name (varchar), Email (varchar), CreatedAt, LastLoginAt
  - Unique index on Email, performance index on CreatedAt
- **TodoLists** - Id (uuid), Title (varchar), Description (varchar), OwnerId (FK), CreatedAt, UpdatedAt
- **TodoItems** - Id (uuid), Title (varchar), Description (varchar), IsCompleted, Priority (enum), CreatedAt, CompletedAt, DueDate, TodoListId (FK)

### Migration Strategy
- **Development**: Automatic database creation with rich fake data using Bogus library (10-15 users with realistic todo content)
- **Production/Staging**: Blue-green deployment ready with SQL scripts in `Persistence/Scripts/`
- **Custom History**: Migrations tracked in `system.TwarzPocMigrationsHistory` table

## Observability & Monitoring

### Logging Architecture
The application implements comprehensive observability with Serilog and OpenTelemetry:

- **Structured Logging** - JSON formatted logs with contextual properties (UserId, CorrelationId, etc.)
- **Request Correlation** - End-to-end tracing via `X-Correlation-ID` headers
- **Performance Monitoring** - Automatic detection of slow requests (>1s warning, >5s alert)
- **Security** - Automatic redaction of sensitive headers and data
- **Early Logging** - Bootstrap logger captures startup events before dependency injection

### Integration Ready
- **Jaeger/Zipkin** - Distributed tracing via OTLP export
- **Prometheus/Grafana** - Metrics collection and visualization  
- **ELK Stack/Splunk** - Log aggregation and analysis
- **Application Insights** - Azure cloud monitoring

### Log Locations
- **Development**: Console output + `logs/tasks-poc-dev-.json` (7-day retention)
- **Production**: JSON structured logs in `logs/tasks-poc-.json` (30-day retention)

## Testing Strategy

### Test Projects
- **Tasks.Poc.E2eTests** - End-to-end tests using Aspire.Hosting.Testing framework
- **Tasks.Poc.ArchTests** - Architecture tests enforcing design principles and coding standards

### Test Features
- Service communication validation
- Health check integration
- Separate test database (InMemory provider)
- Aspire service orchestration testing

## Deployment

### Development Environment
- Automatic database seeding with realistic fake data
- Rich console logging with colors and formatting
- Hot-reload support for both API and web projects
- In-memory database options for testing

### Production Environment
- SQL script-based deployment for blue-green strategies
- Structured JSON logging optimized for aggregation
- Health checks at `/health` endpoints
- OTLP export configuration for observability platforms
- No automatic seeding (clean production databases)

## Architecture Highlights

### Clean Architecture Layers
- **Domain** - Core entities, value objects, and business rules
- **Application** - CQRS handlers, MediatR behaviors, and interfaces  
- **Infrastructure** - Data persistence, external service integrations
- **Presentation** - Web API and Blazor frontend

### Cross-Cutting Concerns
- **Logging** - Comprehensive observability with correlation tracking
- **Validation** - Domain rule enforcement and input validation
- **Error Handling** - Structured exception handling with detailed logging
- **Performance** - Automatic monitoring and alerting for slow operations

## Contributing

### Code Style
- File-scoped namespaces
- Usings inside namespace declarations
- Nullable reference types enabled
- StyleCop analyzers for consistent formatting

### Development Principles
- Domain-Driven Design with explicit domain models
- CQRS pattern with clear command/query separation
- Comprehensive logging for observability
- Security-first approach with automatic sensitive data redaction

---

*This project demonstrates modern .NET development practices with Clean Architecture, comprehensive observability, and cloud-native deployment strategies.*