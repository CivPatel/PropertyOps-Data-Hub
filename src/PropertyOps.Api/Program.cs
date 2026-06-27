using Microsoft.EntityFrameworkCore;
using PropertyOps.Api.Data;
using Scalar.AspNetCore;
using PropertyOps.Api.Services;

var builder = WebApplication.CreateBuilder(args);
const string DashboardCorsPolicy = "DashboardClient";

builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy(DashboardCorsPolicy, policy => //add react 
    {
        policy.WithOrigins(
                "http://localhost:5173",
                "http://127.0.0.1:5173"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
builder.Services.AddControllers();

builder.Services.AddOpenApi();

builder.Services.AddDbContext<PropertyOpsDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("PropertyOpsDatabase")
        ?? throw new InvalidOperationException(
            "Connection string 'PropertyOpsDatabase' was not found."
        ),
        sqlOptions => sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 8,
            maxRetryDelay: TimeSpan.FromSeconds(15),
            errorNumbersToAdd: null
        )
    )
);

builder.Services.AddScoped<LeasingIngestionService>();  // why????????

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseCors(DashboardCorsPolicy); //react

app.MapGet("/health", async (
    PropertyOpsDbContext db,
    CancellationToken cancellationToken) =>
{
    var databaseConnected = await db.Database.CanConnectAsync(cancellationToken);

    return databaseConnected
        ? Results.Ok(new
        {
            status = "Healthy",
            database = "Connected",
            timestampUtc = DateTime.UtcNow
        })
        : Results.Json(
            new
            {
                status = "Unhealthy",
                database = "Disconnected",
                timestampUtc = DateTime.UtcNow
            },
            statusCode: StatusCodes.Status503ServiceUnavailable
        );
});

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PropertyOpsDbContext>();

    var retryStrategy = db.Database.CreateExecutionStrategy();

    await retryStrategy.ExecuteAsync(async () =>
    {
        await db.Database.MigrateAsync();
        await DbInitializer.SeedAsync(db);
    });
}

app.Run();