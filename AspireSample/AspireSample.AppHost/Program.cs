var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.AspireSample_ApiService>("apiservice");

// Add Redis
var redis = builder.AddRedis("cache");

builder.AddProject<Projects.AspireSample_Web>("webfrontend")
    .WithExternalHttpEndpoints()    
    .WithReference(redis)
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
