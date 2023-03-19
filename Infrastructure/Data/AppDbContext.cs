using System;
using System.Collections.Generic;
using Infrastructure.Entity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AspNetUser> AspNetUsers { get; set; }

    public virtual DbSet<SalesOrderDt> SalesOrderDts { get; set; }

    public virtual DbSet<SalesOrderHd> SalesOrderHds { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AspNetUser>(entity =>
        {
            entity.HasKey(e => e.UserId);

            entity.ToTable("AspNetUser");

            entity.HasIndex(e => e.Email, "IX_AspNetUser").IsUnique();

            entity.Property(e => e.UserId).HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.CreatedBy).HasMaxLength(50);
            entity.Property(e => e.Email).HasMaxLength(50);
            entity.Property(e => e.FirstName).HasMaxLength(50);
            entity.Property(e => e.LastChanged).HasColumnType("datetime");
            entity.Property(e => e.LastName).HasMaxLength(50);
            entity.Property(e => e.ModifiedAt).HasColumnType("datetime");
            entity.Property(e => e.ModifiedBy).HasMaxLength(50);
            entity.Property(e => e.StatusCode).HasMaxLength(100);
            entity.Property(e => e.Version)
                .IsRowVersion()
                .IsConcurrencyToken();
        });

        modelBuilder.Entity<SalesOrderDt>(entity =>
        {
            entity.ToTable("SalesOrderDt");

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.CreatedBy).HasMaxLength(50);
            entity.Property(e => e.Item).HasMaxLength(50);
            entity.Property(e => e.ModifiedAt).HasColumnType("datetime");
            entity.Property(e => e.ModifiedBy).HasMaxLength(50);
            entity.Property(e => e.Price).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.Qty).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.SalesOrderCode).HasMaxLength(50);
            entity.Property(e => e.Version)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasOne(d => d.SalesOrderCodeNavigation).WithMany(p => p.SalesOrderDts)
                .HasForeignKey(d => d.SalesOrderCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SalesOrderDt_SalesOrderHd");
        });

        modelBuilder.Entity<SalesOrderHd>(entity =>
        {
            entity.HasKey(e => e.SalesOrderCode);

            entity.ToTable("SalesOrderHd");

            entity.Property(e => e.SalesOrderCode).HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.CreatedBy).HasMaxLength(50);
            entity.Property(e => e.ModifiedAt).HasColumnType("datetime");
            entity.Property(e => e.ModifiedBy).HasMaxLength(50);
            entity.Property(e => e.SalesOrderDate).HasColumnType("datetime");
            entity.Property(e => e.StatusCode).HasMaxLength(100);
            entity.Property(e => e.UserId).HasMaxLength(50);
            entity.Property(e => e.Version)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasOne(d => d.User).WithMany(p => p.SalesOrderHds)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SalesOrderHd_AspNetUser");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
