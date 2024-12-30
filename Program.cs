using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddOpenTelemetry()
    .WithTracing(tracerProviderBuilder =>
    {
        tracerProviderBuilder
            .SetResourceBuilder(ResourceBuilder.CreateDefault()
                .AddService("MyDotNetApp", serviceVersion: "1.0.0")
                .AddAttributes(new[] {
                    new KeyValuePair<string, object>("deployment.environment", "production")
                }))
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddOtlpExporter(o =>
            {
                o.Endpoint = new Uri("https://7ebab4f8cf494df4bdb5774a8c33d356.apm.us-central1.gcp.cloud.es.io:443");
                o.Headers = "Authorization=Bearer HOck0wh231OOCa9MjU";
            });
    })
    .WithMetrics(metricsProviderBuilder =>
    {
        metricsProviderBuilder
            .SetResourceBuilder(ResourceBuilder.CreateDefault()
                .AddService("MyDotNetApp", serviceVersion: "1.0.0"))
            .AddAspNetCoreInstrumentation()
            .AddOtlpExporter(o =>
            {
                o.Endpoint = new Uri("https://7ebab4f8cf494df4bdb5774a8c33d356.apm.us-central1.gcp.cloud.es.io:443");
                o.Headers = "Authorization=Bearer HOck0wh231OOCa9MjU";
            });
    });

builder.Logging.AddOpenTelemetry(loggingBuilder =>
{
    loggingBuilder.IncludeFormattedMessage = true;
    loggingBuilder.ParseStateValues = true;
    loggingBuilder.AddOtlpExporter(o =>
    {
        o.Endpoint = new Uri("https://7ebab4f8cf494df4bdb5774a8c33d356.apm.us-central1.gcp.cloud.es.io:443");
        o.Headers = "Authorization=Bearer HOck0wh231OOCa9MjU";
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}



app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
