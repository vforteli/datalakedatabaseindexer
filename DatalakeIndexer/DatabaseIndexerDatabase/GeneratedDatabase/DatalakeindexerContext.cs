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

    public virtual DbSet<Paths> Paths { get; set; }

    public virtual DbSet<PathsMetadata> PathsMetadata { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Paths>(entity =>
        {
            entity.HasKey(e => e.PathKey).IsClustered(false);

            entity.HasIndex(e => new { e.FilesystemName, e.Path_reversed }, "filesystem_path_reversed");

            entity.HasIndex(e => new { e.FilesystemName, e.Path }, "path_filesystem_unique").IsClustered();

            entity.HasIndex(e => e.PathKey, "path_key").IsUnique();

            entity.Property(e => e.PathKey)
                .HasMaxLength(32)
                .IsFixedLength();
            entity.Property(e => e.ETag).HasMaxLength(20);
            entity.Property(e => e.FilesystemName).HasMaxLength(255);
            entity.Property(e => e.Path).HasMaxLength(1024);
            entity.Property(e => e.Path_reversed)
                .HasMaxLength(1024)
                .HasComputedColumnSql("(reverse([Path]))", false);
        });

        modelBuilder.Entity<PathsMetadata>(entity =>
        {
            entity.HasKey(e => e.PathKey);

            entity.Property(e => e.PathKey)
                .HasMaxLength(32)
                .IsFixedLength();
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
