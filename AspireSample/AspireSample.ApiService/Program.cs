using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

var logger = app.Services.GetRequiredService<ILogger<Program>>();

string[] summaries = ["Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"];

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
    
    if (logger.IsEnabled(LogLevel.Debug))
    {
        logger.LogDebug("Forecast obtained: {forecast} ({requestId})", JsonSerializer.Serialize(forecast),requestId);
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
