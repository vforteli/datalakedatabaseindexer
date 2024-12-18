using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using PGDatabaseContext.GeneratedDatabase;

namespace PGIndexerTests.Tests;

public abstract class DatabaseTest : BaseTest
{
    [OneTimeSetUp]
    public override async Task OneTimeSetup()
    {
        await base.OneTimeSetup();

        await SetupServiceProvider.GetRequiredService<PgIndexerContext>().Database.MigrateAsync();
    }


    [OneTimeTearDown]
    public override async Task OneTimeTearDown()
    {
        await SetupServiceProvider.GetRequiredService<PgIndexerContext>().Database.EnsureDeletedAsync();

        await base.OneTimeTearDown();
    }

    [SetUp]
    public async Task Setup()
    {
        using var connection = SetupServiceProvider.GetRequiredService<NpgsqlDataSource>().CreateConnection();

        await connection.ExecuteScalarAsync(
            """
            TRUNCATE TABLE paths;
            """);
    }
}