using Common.MassTransit;
using Common.MongoDB;
using Inventory.Service.Clients;
using Inventory.Service.Dtos.Entities;
using Polly;
using Polly.Timeout;

void AddCatalogClient(IServiceCollection services)
{
    Random jitterer = new Random();

    services.AddHttpClient<CatalogClient>(client =>
        {
            client.BaseAddress = new Uri("http://localhost:5006/api/");
        })
        .AddTransientHttpErrorPolicy(httpClientBuilder => httpClientBuilder.Or<TimeoutRejectedException>()
            .WaitAndRetryAsync(
                5,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                                + TimeSpan.FromMilliseconds(jitterer.Next(0, 1000)),
                onRetry: (outcome, timespan, retryAttempt) =>
                {
                    var serviceProvider = services.BuildServiceProvider();

                    serviceProvider.GetService<ILogger<CatalogClient>>()?
                        .LogWarning($"Delaying for {timespan.TotalSeconds} seconds,then making retry {retryAttempt}");
                }))
        .AddTransientHttpErrorPolicy(httpClientBuilder => httpClientBuilder.Or<TimeoutRejectedException>()
            .CircuitBreakerAsync(
                3,
                TimeSpan.FromSeconds(15),
                onBreak: (outcome, timespan) =>
                {
                    var serviceProvider = services.BuildServiceProvider();

                    serviceProvider.GetService<ILogger<CatalogClient>>()?
                        .LogWarning($"Opening the circuit for {timespan.TotalSeconds} seconds...");
                },
                onReset: () =>
                {
                    var serviceProvider = services.BuildServiceProvider();
                    serviceProvider.GetService<ILogger<CatalogClient>>()?
                        .LogWarning($"Closing the circuit...");
                }))
        .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(1));
}

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddMongo()
    .AddMongoRepository<InventoryItem>("InventoryItems")
    .AddMongoRepository<CatalogItem>("CatalogItems")
    .AddMassTransitWithRabbitMq();

AddCatalogClient(builder.Services);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
