using DatabaseIndexerDatabase.GeneratedDatabase;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DatabaseIndexerTests.Tests;

public class DatabaseContextTest : DatabaseTest
{
    [Test]
    public async Task TestSomething()
    {
        var context = SetupServiceProvider.GetRequiredService<DatalakeindexerContext>();
        var path = await context.Paths.SingleOrDefaultAsync(o => o.Path == "blaa");
    }
}
