var builder = DistributedApplication.CreateBuilder(args);



// Add Redis
var redis = builder.AddRedis("cache");

// Add RabbitMQ 
var username = builder.AddParameter("username", secret: true);
var password = builder.AddParameter("password", secret: true);

var rabbitmq = builder.AddRabbitMQ("rabbitmq", username, password)    
    .WithDataVolume()
    .WithManagementPlugin();


// Add Api
var apiService = 
    builder.AddProject<Projects.AspireSample_ApiService>("apiservice")      
        .WithReference(rabbitmq)
        .WaitFor(rabbitmq);

// Add Web
builder.AddProject<Projects.AspireSample_Web>("webfrontend")
    .WithExternalHttpEndpoints()    
    .WithReference(redis) 
    .WaitFor(redis)
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
