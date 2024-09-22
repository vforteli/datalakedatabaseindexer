using Dapper;
using DatabaseIndexer;
using DatabaseIndexerDatabase.GeneratedDatabase;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DatabaseIndexerTests.Tests;

public abstract class DatabaseTest : BaseTest
{
    [OneTimeSetUp]
    public override async Task OneTimeSetup()
    {
        await base.OneTimeSetup();

        await SetupServiceProvider.GetRequiredService<DatalakeindexerContext>().Database.MigrateAsync();
    }


    [OneTimeTearDown]
    public override async Task OneTimeTearDown()
    {
        await SetupServiceProvider.GetRequiredService<DatalakeindexerContext>().Database.EnsureDeletedAsync();

        await base.OneTimeTearDown();
    }

    [SetUp]
    public async Task Setup()
    {
        using var connection = SetupServiceProvider.GetRequiredService<ISqlConnectionFactory>().Create();

        await connection.ExecuteScalarAsync("""
            TRUNCATE TABLE Paths
            TRUNCATE TABLE PathsMetadata
            """);
    }
}

