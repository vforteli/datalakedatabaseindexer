using Microsoft.EntityFrameworkCore;

namespace DatabaseIndexerDatabase.GeneratedDatabase;

// this is here so we can use --no-onconfiguring to avoid having the connection string included in code every time...
public partial class DatalakeindexerContext : DbContext
{
    public DatalakeindexerContext()
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseSqlServer();
}