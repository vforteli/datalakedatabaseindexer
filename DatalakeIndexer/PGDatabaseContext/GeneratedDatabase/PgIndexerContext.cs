using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace PGDatabaseContext.GeneratedDatabase;

public partial class PgIndexerContext : DbContext
{
    public PgIndexerContext(DbContextOptions<PgIndexerContext> options)
        : base(options)
    {
    }

    public virtual DbSet<paths> paths { get; set; }

    public virtual DbSet<paths_metadata> paths_metadata { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<paths>(entity =>
        {
            entity.HasKey(e => e.path_key).HasName("paths_pk");

            entity.HasAnnotation("Npgsql:StorageParameter:autovacuum_enabled", "true");

            entity.HasIndex(e => new { e.filesystem_name, e.path }, "filesystem_path_unique").HasAnnotation("Npgsql:StorageParameter:deduplicate_items", "true");

            entity.Property(e => e.etag).HasMaxLength(20);
            entity.Property(e => e.filesystem_name).HasMaxLength(255);
            entity.Property(e => e.path).HasMaxLength(1024);
            entity.Property(e => e.path_reversed)
                .HasMaxLength(1024)
                .HasComputedColumnSql("reverse((path)::text)", true);
        });

        modelBuilder.Entity<paths_metadata>(entity =>
        {
            entity.HasKey(e => e.path_key).HasName("paths_metadata_pk");

            entity.Property(e => e.metadata_json).HasColumnType("jsonb");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
