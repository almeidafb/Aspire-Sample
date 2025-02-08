using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add RabbitMQ 
builder.AddRabbitMQClient("rabbitmq");


var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

var logger = app.Services.GetRequiredService<ILogger<Program>>();
var rabbitConnection = app.Services.GetRequiredService<IConnection>();

string[] summaries =
    ["Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"];

app.MapPost("/sendmessage", async Task<IResult> (IConnection connection, string messageToSend) =>
{
    var channel = connection.CreateModel();
    const string queueName = "weatherForecasts";

    channel.QueueDeclare(
        queue: queueName,
        durable: false,
        exclusive: false,
        autoDelete: false,
        arguments: null
    );

    var body = Encoding.UTF8.GetBytes(messageToSend);

    channel.BasicPublish(
        exchange: string.Empty,
        routingKey: queueName,
        mandatory: false,
        basicProperties: null,
        body: body
    );

    logger.LogInformation("Message sent to RabbitMQ");
    if (logger.IsEnabled(LogLevel.Debug))
    {
        logger.LogDebug("Message: ({messageToSend})", messageToSend);
    }
    return Results.Ok();
});

app.MapGet("/weatherforecast", () =>
    {
        var requestId = Guid.NewGuid();

        // Use the logger to write events
        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug("Begin GetWeatherForecast call from method /weatherforecast ({requestId})", requestId);
        }


        var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
            .ToArray();
        var serializedForecast = JsonSerializer.Serialize(forecast);
        
        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug("Forecast obtained: {forecast} ({requestId})", serializedForecast,
                requestId);
            logger.LogDebug("End GetWeatherForecast call from method /weatherforecast ({requestId})", requestId);
        }

        return forecast;
    })
    .WithName("GetWeatherForecast");

app.MapDefaultEndpoints();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}