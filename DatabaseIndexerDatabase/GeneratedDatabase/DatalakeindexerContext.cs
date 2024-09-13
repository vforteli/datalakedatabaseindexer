using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace DatabaseIndexerDatabase.GeneratedDatabase;

public partial class DatalakeindexerContext : DbContext
{
    public DatalakeindexerContext(DbContextOptions<DatalakeindexerContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Path> Paths { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Path>(entity =>
        {
            entity.HasIndex(e => new { e.FilesystemName, e.PathReversed }, "filesystem_path_reversed");

            entity.HasIndex(e => new { e.FilesystemName, e.Path1 }, "path_filesystem_unique").IsUnique();

            entity.Property(e => e.FilesystemName).HasMaxLength(255);
            entity.Property(e => e.Path1)
                .HasMaxLength(1024)
                .HasColumnName("Path");
            entity.Property(e => e.PathReversed)
                .HasMaxLength(1024)
                .HasComputedColumnSql("(reverse([Path]))", false)
                .HasColumnName("Path_reversed");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
