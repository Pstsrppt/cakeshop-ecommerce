using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace Project_CakeShop_66095681.Models.Db;

public partial class Csi402dbContext : DbContext
{
    public Csi402dbContext()
    {
    }

    public Csi402dbContext(DbContextOptions<Csi402dbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<StockLog> StockLogs { get; set; }
    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<Orderdetail> Orderdetails { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<Promotion> Promotions { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseMySql("server=localhost;user=root;password=YOUR_PASSWORD_HERE;database=csi402db;AllowPublicKeyRetrieval=True", Microsoft.EntityFrameworkCore.ServerVersion.Parse("9.6.0-mysql"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_general_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PRIMARY");

            entity
                .ToTable("orders")
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => e.UserId, "FK_Orders_Users");

            entity.Property(e => e.OrderDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.OrderStatus)
                .HasMaxLength(20)
                .HasDefaultValueSql("'Pending'");
            entity.Property(e => e.ShippingAddress)
                .HasMaxLength(500);
            entity.Property(e => e.PaymentStatus)
                .HasMaxLength(20)
                .HasDefaultValueSql("'Unpaid'");

            entity.HasOne(d => d.User).WithMany(p => p.Orders)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_Orders_Users");
        });

        modelBuilder.Entity<Orderdetail>(entity =>
        {
            entity.HasKey(e => e.DetailId).HasName("PRIMARY");

            entity
                .ToTable("orderdetails")
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => e.OrderId, "FK_Details_Orders");

            entity.HasIndex(e => e.ProductId, "FK_Details_Products");

            entity.HasOne(d => d.Order).WithMany(p => p.Orderdetails)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Details_Orders");

            entity.HasOne(d => d.Product).WithMany(p => p.Orderdetails)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK_Details_Products");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.ProductId).HasName("PRIMARY");

            entity
                .ToTable("products")
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.Property(e => e.CakeName).HasMaxLength(100);
            entity.Property(e => e.DiscountAmount).HasDefaultValueSql("'0'");
            entity.Property(e => e.ProductImage).HasMaxLength(255);
        });

        modelBuilder.Entity<Promotion>(entity =>
        {
            entity.HasKey(e => e.PromoCode).HasName("PRIMARY");

            entity
                .ToTable("promotions")
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.Property(e => e.PromoCode).HasMaxLength(20);
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.DiscountPercent).HasDefaultValueSql("'0'");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PRIMARY");

            entity
                .ToTable("users")
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => e.Username, "Username").IsUnique();

            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.Password).HasMaxLength(255);
            entity.Property(e => e.UserRole)
                .HasMaxLength(20)
                .HasDefaultValueSql("'Customer'");
            entity.Property(e => e.Username).HasMaxLength(50);
        });
        modelBuilder.Entity<StockLog>(entity =>
        {
            entity.HasKey(e => e.LogId); // กำหนด Key ในระดับ Fluent API
            entity.ToTable("stocklog"); // ชื่อตารางใน MySQL (ระวังเรื่องตัวเล็กตัวใหญ่ให้ตรงกับใน DB นะครับ)

            entity.Property(e => e.LogId).HasColumnName("LogId");
        });
        OnModelCreatingPartial(modelBuilder);

    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
