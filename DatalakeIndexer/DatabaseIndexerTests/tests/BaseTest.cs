#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

using DatabaseIndexer;
using DatabaseIndexerDatabase.GeneratedDatabase;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DatabaseIndexerTests.Tests;

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
        ConnectionString = $"Data Source=localhost,1434;Initial Catalog=integrationtest-{Guid.NewGuid().ToString()[25..]};User ID=sa;Password=Top1Secret!;TrustServerCertificate=true";

        var services = new ServiceCollection();

        services.AddDbContext<DatalakeindexerContext>(o => o.UseSqlServer(ConnectionString, o => o.EnableRetryOnFailure()));
        services.AddSingleton<ISqlConnectionFactory>(o => new SqlConnectionFactory(ConnectionString));

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

        services.AddDbContext<DatalakeindexerContext>(o => o.UseSqlServer(ConnectionString, o => o.EnableRetryOnFailure()), ServiceLifetime.Transient); // ensure a transient scope so each run gets its own context
        services.AddSingleton<ISqlConnectionFactory>(o => new SqlConnectionFactory(ConnectionString));
        services.AddLogging();

        services.AddTransient<DatalakeIndexer>();

        return services;
    }
}

