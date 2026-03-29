using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Food_Ordering.Entities;

public partial class FoodOrderingDbContext : DbContext
{
    public FoodOrderingDbContext()
    {
    }

    public FoodOrderingDbContext(DbContextOptions<FoodOrderingDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Cart> Carts { get; set; }

    public virtual DbSet<CartItem> CartItems { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<FoodItem> FoodItems { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderItem> OrderItems { get; set; }

    public virtual DbSet<Restaurant> Restaurants { get; set; }

    public virtual DbSet<RestaurantApplication> RestaurantApplications { get; set; }

    public virtual DbSet<RestaurantOwner> RestaurantOwners { get; set; }

    public virtual DbSet<Shipper> Shippers { get; set; }

    public virtual DbSet<ShipperApplication> ShipperApplications { get; set; }

    public virtual DbSet<User> Users { get; set; }

    //    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    //#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
    //        => optionsBuilder.UseSqlServer("Server=(local);uid=sa;pwd=12345;database=FoodOrderingDB;TrustServerCertificate=True;");

    private string GetConnectionString()
    {
        IConfiguration config = new ConfigurationBuilder()
             .SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile("appsettings.json", true, true)
                    .Build();
        var strConn = config["ConnectionStrings:DefaultConnection"];

        return strConn;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(GetConnectionString());
    }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cart>(entity =>
        {
            entity.HasKey(e => e.CartId).HasName("PK__Carts__51BCD7B76EEEC128");

            entity.HasIndex(e => e.CustomerId, "UQ__Carts__A4AE64D9CF539425").IsUnique();

            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Customer).WithOne(p => p.Cart)
                .HasForeignKey<Cart>(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Carts__CustomerI__4E88ABD4");

            entity.HasOne(d => d.Restaurant).WithMany(p => p.Carts)
                .HasForeignKey(d => d.RestaurantId)
                .HasConstraintName("FK__Carts__Restauran__4F7CD00D");
        });

        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.HasKey(e => e.CartItemId).HasName("PK__CartItem__488B0B0AD44BBE26");

            entity.Property(e => e.Quantity).HasDefaultValue(1);

            entity.HasOne(d => d.Cart).WithMany(p => p.CartItems)
                .HasForeignKey(d => d.CartId)
                .HasConstraintName("FK__CartItems__CartI__534D60F1");

            entity.HasOne(d => d.FoodItem).WithMany(p => p.CartItems)
                .HasForeignKey(d => d.FoodItemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CartItems__FoodI__5441852A");
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.CustomerId).HasName("PK__Customer__A4AE64D837C5BD81");

            entity.Property(e => e.CustomerId).ValueGeneratedNever();
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.Latitude).HasColumnType("decimal(9, 6)");
            entity.Property(e => e.Longitude).HasColumnType("decimal(9, 6)");

            entity.HasOne(d => d.CustomerNavigation).WithOne(p => p.Customer)
                .HasForeignKey<Customer>(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Customers__Custo__2B3F6F97");
        });

        modelBuilder.Entity<FoodItem>(entity =>
        {
            entity.HasKey(e => e.FoodItemId).HasName("PK__FoodItem__464DC812D5C5131A");

            entity.HasIndex(e => e.RestaurantId, "IX_FoodItems_RestaurantId");

            entity.Property(e => e.Category).HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.ImageUrl).HasMaxLength(500);
            entity.Property(e => e.IsAvailable).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Restaurant).WithMany(p => p.FoodItems)
                .HasForeignKey(d => d.RestaurantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__FoodItems__Resta__48CFD27E");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PK__Orders__C3905BCF30F450BD");

            entity.HasIndex(e => e.CustomerId, "IX_Orders_CustomerId");

            entity.HasIndex(e => e.RestaurantId, "IX_Orders_RestaurantId");

            entity.HasIndex(e => e.ShipperId, "IX_Orders_ShipperId");

            entity.HasIndex(e => e.Status, "IX_Orders_Status");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.DeliveryAddress).HasMaxLength(500);
            entity.Property(e => e.Note).HasMaxLength(500);
            entity.Property(e => e.PaymentMethod)
                .HasMaxLength(50)
                .HasDefaultValue("VNPay");
            entity.Property(e => e.PaymentTransactionId).HasMaxLength(255);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("PendingPayment");
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Customer).WithMany(p => p.Orders)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Orders__Customer__59063A47");

            entity.HasOne(d => d.Restaurant).WithMany(p => p.Orders)
                .HasForeignKey(d => d.RestaurantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Orders__Restaura__59FA5E80");

            entity.HasOne(d => d.Shipper).WithMany(p => p.Orders)
                .HasForeignKey(d => d.ShipperId)
                .HasConstraintName("FK__Orders__ShipperI__5AEE82B9");
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.OrderItemId).HasName("PK__OrderIte__57ED06816A22914E");

            entity.Property(e => e.FoodName).HasMaxLength(255);
            entity.Property(e => e.FoodPrice).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.FoodItem).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.FoodItemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__OrderItem__FoodI__6383C8BA");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("FK__OrderItem__Order__628FA481");
        });

        modelBuilder.Entity<Restaurant>(entity =>
        {
            entity.HasKey(e => e.RestaurantId).HasName("PK__Restaura__87454C957C8B55AF");

            entity.HasIndex(e => e.OwnerId, "UQ__Restaura__819385B9C93869D3").IsUnique();

            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.AverageRating).HasColumnType("decimal(3, 2)");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.IsOpen).HasDefaultValue(true);
            entity.Property(e => e.Latitude).HasColumnType("decimal(9, 6)");
            entity.Property(e => e.LogoUrl).HasMaxLength(500);
            entity.Property(e => e.Longitude).HasColumnType("decimal(9, 6)");
            entity.Property(e => e.Name).HasMaxLength(255);

            entity.HasOne(d => d.Owner).WithOne(p => p.Restaurant)
                .HasForeignKey<Restaurant>(d => d.OwnerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Restauran__Owner__4222D4EF");
        });

        modelBuilder.Entity<RestaurantApplication>(entity =>
        {
            entity.HasKey(e => e.ApplicationId).HasName("PK__Restaura__C93A4C9920B7D007");

            entity.HasIndex(e => e.UserId, "IX_RestApp_UserId");

            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.AdminNote).HasMaxLength(1000);
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.Latitude).HasColumnType("decimal(9, 6)");
            entity.Property(e => e.LogoUrl).HasMaxLength(500);
            entity.Property(e => e.Longitude).HasColumnType("decimal(9, 6)");
            entity.Property(e => e.RestaurantName).HasMaxLength(255);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Pending");
            entity.Property(e => e.SubmittedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.User).WithMany(p => p.RestaurantApplications)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Restauran__UserI__3B75D760");
        });

        modelBuilder.Entity<RestaurantOwner>(entity =>
        {
            entity.HasKey(e => e.OwnerId).HasName("PK__Restaura__819385B8E155590B");

            entity.Property(e => e.OwnerId).ValueGeneratedNever();

            entity.HasOne(d => d.Owner).WithOne(p => p.RestaurantOwner)
                .HasForeignKey<RestaurantOwner>(d => d.OwnerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Restauran__Owner__38996AB5");
        });

        modelBuilder.Entity<Shipper>(entity =>
        {
            entity.HasKey(e => e.ShipperId).HasName("PK__Shippers__1F8AFE59931270FB");

            entity.Property(e => e.ShipperId).ValueGeneratedNever();
            entity.Property(e => e.IsAvailable).HasDefaultValue(true);
            entity.Property(e => e.LicensePlate).HasMaxLength(20);
            entity.Property(e => e.VehicleType).HasMaxLength(50);

            entity.HasOne(d => d.ShipperNavigation).WithOne(p => p.Shipper)
                .HasForeignKey<Shipper>(d => d.ShipperId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Shippers__Shippe__2E1BDC42");
        });

        modelBuilder.Entity<ShipperApplication>(entity =>
        {
            entity.HasKey(e => e.ApplicationId).HasName("PK__ShipperA__C93A4C997D073FF9");

            entity.HasIndex(e => e.UserId, "IX_ShipperApp_UserId");

            entity.Property(e => e.AdminNote).HasMaxLength(1000);
            entity.Property(e => e.IdentityCard).HasMaxLength(20);
            entity.Property(e => e.IdentityCardBackUrl).HasMaxLength(500);
            entity.Property(e => e.IdentityCardFrontUrl).HasMaxLength(500);
            entity.Property(e => e.LicensePlate).HasMaxLength(20);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Pending");
            entity.Property(e => e.SubmittedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.VehicleType).HasMaxLength(50);

            entity.HasOne(d => d.User).WithMany(p => p.ShipperApplications)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ShipperAp__UserI__32E0915F");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4C0D997DC2");

            entity.HasIndex(e => e.Email, "UQ__Users__A9D105346F709867").IsUnique();

            entity.Property(e => e.AvatarUrl).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.FullName).HasMaxLength(255);
            entity.Property(e => e.PasswordHash).HasMaxLength(500);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.Role).HasMaxLength(50);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Active");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
