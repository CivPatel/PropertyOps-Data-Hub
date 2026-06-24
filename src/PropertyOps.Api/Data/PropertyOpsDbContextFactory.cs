
//This file is Entity Framework migration issue: EF can build the app, but at design time it cannot construct PropertyOpsDbContext through dependency injection.
//Add a small design-time factory. EF will use it only for migrations. Microsoft specifically supports IDesignTimeDbContextFactory<TContext> for this situation.
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace PropertyOps.Api.Data;

public class PropertyOpsDbContextFactory
    : IDesignTimeDbContextFactory<PropertyOpsDbContext>
{
    public PropertyOpsDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<PropertyOpsDbContextFactory>(optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString =
            configuration.GetConnectionString("PropertyOpsDatabase")
            ?? throw new InvalidOperationException(
                "Connection string 'PropertyOpsDatabase' was not found."
            );

        var optionsBuilder = new DbContextOptionsBuilder<PropertyOpsDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new PropertyOpsDbContext(optionsBuilder.Options);
    }
}