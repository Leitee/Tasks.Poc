var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithEndpoint("tcp", e => e.Port = 5432)
    .WithDataVolume()
    .WithPgWeb();

var todosDb = postgres.AddDatabase("todosdb");

var apiService = builder.AddProject<Projects.Tasks_Poc_ApiCore>("apiservice")
    .WithReference(todosDb)
    .WithHttpHealthCheck("/health")
    .WaitFor(todosDb);

builder.AddProject<Projects.Tasks_Poc_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
