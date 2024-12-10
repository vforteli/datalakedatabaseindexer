using Microsoft.EntityFrameworkCore;

namespace PGDatabaseContext.GeneratedDatabase;

// this is here so we can use --no-onconfiguring to avoid having the connection string included in code every time...
public partial class PgIndexerContext : DbContext
{
    public PgIndexerContext()
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseNpgsql();
}
