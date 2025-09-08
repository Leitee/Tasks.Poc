using MediatR;
using Tasks.Poc.Application.TodoLists.Commands;
using Tasks.Poc.Application.TodoLists.Queries;
using Tasks.Poc.Application.Users.Commands;
using Tasks.Poc.Application.Users.Queries;
using Tasks.Poc.Domain.Enums;
using Tasks.Poc.Infrastructure;
using Tasks.Poc.ApiCore.Logging;
using Tasks.Poc.ApiCore.Behaviors;
using Serilog;
using System.Diagnostics;

var startupStopwatch = Stopwatch.StartNew();

BootstrapLogger.Initialize();

try
{
    Log.Information("=== Starting Tasks.Poc.ApiCore application ===");
    Log.Information("Environment: {Environment}", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"));
    Log.Information("Process ID: {ProcessId}", Environment.ProcessId);

    var builder = WebApplication.CreateBuilder(args);
    
    BootstrapLogger.LogConfigurationLoaded(builder.Configuration);
    
    // Phase 1: Bootstrap logging complete, Phase 2: Configure Serilog with OpenTelemetry
    builder.AddSerilogWithOpenTelemetry();

    // Add service defaults & Aspire client integrations.
    builder.AddServiceDefaults();

    // Add services to the container.
    builder.Services.AddProblemDetails();
    builder.Services.AddHttpContextAccessor();

    // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
    builder.Services.AddOpenApi();

    // Add Clean Architecture layers with MediatR logging behavior
    builder.Services.AddMediatR(cfg => 
    {
        cfg.RegisterServicesFromAssemblyContaining<CreateUserCommand>();
        cfg.AddBehavior<LoggingBehavior<CreateUserCommand, Guid>>();
        cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
    });
    
    var infrastructureStopwatch = Stopwatch.StartNew();
    builder.AddInfrastructure();
    infrastructureStopwatch.Stop();
    
    BootstrapLogger.LogStartupMetrics(infrastructureStopwatch.Elapsed, "Infrastructure Configuration");

    Log.Information("Building application...");
    var app = builder.Build();

    startupStopwatch.Stop();
    BootstrapLogger.LogStartupMetrics(startupStopwatch.Elapsed, "Application Startup");
    
    // Configure the HTTP request pipeline.
    app.UseExceptionHandler();

    // Add enhanced request logging
    app.UseEnhancedRequestLogging();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    // User endpoints
    app.MapPost("/api/users", async (CreateUserRequest request, IMediator mediator) =>
    {
        var command = new CreateUserCommand(request.Name, request.Email);
        var userId = await mediator.Send(command);
        return Results.Created($"/api/users/{userId}", userId);
    })
    .WithName("CreateUser");

    app.MapGet("/api/users", async (IMediator mediator) =>
    {
        var query = new GetAllUsersQuery();
        var users = await mediator.Send(query);
        return Results.Ok(users);
    })
    .WithName("GetAllUsers");

    app.MapGet("/api/users/{userId:guid}", async (Guid userId, IMediator mediator) =>
    {
        var query = new GetUserByIdQuery(userId);
        var user = await mediator.Send(query);
        return user == null ? Results.NotFound() : Results.Ok(user);
    })
    .WithName("GetUserById");

    // TodoList endpoints
    app.MapPost("/api/users/{userId:guid}/todos", async (Guid userId, CreateTodoListRequest request, IMediator mediator) =>
    {
        var command = new CreateTodoListCommand(userId, request.Title, request.Description);
        var todoListId = await mediator.Send(command);
        return Results.Created($"/api/todos/{todoListId}", todoListId);
    })
    .WithName("CreateTodoList");

    app.MapGet("/api/users/{userId:guid}/todos", async (Guid userId, IMediator mediator) =>
    {
        var query = new GetUserTodoListsQuery(userId);
        var todoLists = await mediator.Send(query);
        return Results.Ok(todoLists);
    })
    .WithName("GetUserTodoLists");

    app.MapGet("/api/todos/{todoListId:guid}", async (Guid todoListId, IMediator mediator) =>
    {
        var query = new GetTodoListWithItemsQuery(todoListId);
        var todoList = await mediator.Send(query);
        return todoList == null ? Results.NotFound() : Results.Ok(todoList);
    })
    .WithName("GetTodoListWithItems");

    // TodoItem endpoints
    app.MapPost("/api/todos/{todoListId:guid}/items", async (Guid todoListId, AddTodoItemRequest request, IMediator mediator) =>
    {
        var command = new AddTodoItemCommand(todoListId, request.Title, request.Description, request.Priority, request.DueDate);
        var itemId = await mediator.Send(command);
        return Results.Created($"/api/todos/{todoListId}/items/{itemId}", itemId);
    })
    .WithName("AddTodoItem");

    app.MapPut("/api/todos/{todoListId:guid}/items/{itemId:guid}/complete", async (Guid todoListId, Guid itemId, IMediator mediator) =>
    {
        var command = new CompleteTodoItemCommand(todoListId, itemId);
        await mediator.Send(command);
        return Results.NoContent();
    })
    .WithName("CompleteTodoItem");

    app.MapDefaultEndpoints();

    Log.Information("=== Tasks.Poc.ApiCore application configured successfully ===");
    Log.Information("Total startup time: {TotalStartupTime}ms", startupStopwatch.Elapsed.TotalMilliseconds);

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "=== Application startup failed ===");
    throw;
}
finally
{
    Log.Information("=== Application shutting down ===");
    BootstrapLogger.Flush();
}

// Request DTOs
public record CreateUserRequest(string Name, string Email);
public record CreateTodoListRequest(string Title, string? Description);
public record AddTodoItemRequest(string Title, string? Description, Priority Priority, DateTime? DueDate);
