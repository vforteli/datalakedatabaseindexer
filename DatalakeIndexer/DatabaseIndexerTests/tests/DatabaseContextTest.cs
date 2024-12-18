using DatabaseIndexerDatabase.GeneratedDatabase;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DatabaseIndexerTests.Tests;

public class DatabaseContextTest : DatabaseTest
{
    [Test]
    public void TestSomething()
    {
        var context = SetupServiceProvider.GetRequiredService<DatalakeindexerContext>();
        Assert.DoesNotThrowAsync(async () => await context.Paths.SingleOrDefaultAsync(o => o.Path == "blaa"));
    }
}