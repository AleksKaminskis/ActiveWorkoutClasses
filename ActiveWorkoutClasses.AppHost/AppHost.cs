var builder = DistributedApplication.CreateBuilder(args);

// ApiService now hosts both the REST API and the Blazor WASM frontend (hosted model)
builder.AddProject<Projects.ActiveWorkoutClasses_ApiService>("apiservice")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health");

builder.Build().Run();
