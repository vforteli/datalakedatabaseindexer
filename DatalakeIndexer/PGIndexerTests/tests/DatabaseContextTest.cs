using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PGDatabaseContext.GeneratedDatabase;

namespace PGIndexerTests.Tests;

public class DatabaseContextTest : DatabaseTest
{
    [Test]
    public async Task TestSomething()
    {
        var context = SetupServiceProvider.GetRequiredService<PgIndexerContext>();
        var path = await context.paths.SingleOrDefaultAsync(o => o.path == "blaa");
    }
}