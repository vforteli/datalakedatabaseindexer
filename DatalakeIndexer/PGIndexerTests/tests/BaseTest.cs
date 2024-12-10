#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using PGDatabaseContext.GeneratedDatabase;
using PGDatabaseIndexer;

namespace PGIndexerTests.Tests;

public abstract class BaseTest
{
    /// <summary>
    /// Services related to setting up and migrating the database for integration tests
    /// Should generally not be used anywhere else or fiddled with
    /// </summary>
    internal ServiceProvider SetupServiceProvider;

    /// <summary>
    /// Services for testing
    /// </summary>
    internal ServiceProvider TestServiceProvider;

    internal static string ConnectionString { get; private set; } = "";

    [OneTimeSetUp]
    public virtual async Task OneTimeSetup()
    {
        ConnectionString =
            $"User ID=myuser;Password=mypassword;Host=127.0.0.1;Port=5432;Database=integrationtest-{Guid.NewGuid().ToString()[25..]}";
        var services = new ServiceCollection();

        services.AddDbContext<PgIndexerContext>(o => o.UseNpgsql(ConnectionString));

        services.AddSingleton(o => new NpgsqlDataSourceBuilder(ConnectionString).Build());

        SetupServiceProvider = services.BuildServiceProvider();
        TestServiceProvider = CreateTestServiceCollection().BuildServiceProvider();
    }


    [OneTimeTearDown]
    public virtual async Task OneTimeTearDown()
    {
        SetupServiceProvider.Dispose();
        TestServiceProvider.Dispose();
    }

    /// <summary>
    /// Add the default services for testing, eg all needed for running the application
    /// </summary>
    internal ServiceCollection CreateTestServiceCollection()
    {
        var services = new ServiceCollection();

        services.AddDbContext<PgIndexerContext>(o => o.UseNpgsql(ConnectionString),
            ServiceLifetime.Transient); // ensure a transient scope so each run gets its own context
        services.AddSingleton(o => new NpgsqlDataSourceBuilder(ConnectionString).Build());
        services.AddLogging();

        services.AddTransient<PGServerIndexer>();

        return services;
    }
}