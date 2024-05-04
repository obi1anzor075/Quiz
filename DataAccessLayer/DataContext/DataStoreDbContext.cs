using System;
using System.Collections.Generic;
using DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;

using DataAccessLayer.DataContext;

namespace DataAccessLayer.DataContext;

public partial class DataStoreDbContext : DbContext
{
    public DataStoreDbContext()
    {
    }

    public DataStoreDbContext(DbContextOptions<DataStoreDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Datum> Data { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Datum>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__DATA__3214EC07DCB03DE1");

            entity.ToTable("DATA");

            entity.Property(e => e._1)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("1");
            entity.Property(e => e._2)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("2");
            entity.Property(e => e._3)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("3");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
