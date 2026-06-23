using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SolarPath.Web.Models;

namespace SolarPath.Web.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<Category>   Categories { get; set; }
    public DbSet<Models.Route> Routes   { get; set; }
    public DbSet<RoutePoint> RoutePoints { get; set; }
    public DbSet<Booking>    Bookings   { get; set; }
    public DbSet<Payment>    Payments   { get; set; }
    public DbSet<Review>     Reviews    { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Models.Route>(e =>
        {
            e.Property(r => r.PricePerPerson).HasColumnType("decimal(18,2)");
            e.HasOne(r => r.Category).WithMany(c => c.Routes).HasForeignKey(r => r.CategoryId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(r => r.Guide).WithMany().HasForeignKey(r => r.GuideId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<RoutePoint>(e =>
        {
            e.HasOne(p => p.Route).WithMany(r => r.Points).HasForeignKey(p => p.RouteId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Booking>(e =>
        {
            e.Property(b => b.TotalPrice).HasColumnType("decimal(18,2)");
            e.HasOne(b => b.Tourist).WithMany().HasForeignKey(b => b.TouristId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(b => b.Route).WithMany(r => r.Bookings).HasForeignKey(b => b.RouteId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Payment>(e =>
        {
            e.Property(p => p.Amount).HasColumnType("decimal(18,2)");
            e.HasOne(p => p.Booking).WithOne(b => b.Payment).HasForeignKey<Payment>(p => p.BookingId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Review>(e =>
        {
            e.HasOne(r => r.Tourist).WithMany().HasForeignKey(r => r.TouristId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(r => r.Route).WithMany(r => r.Reviews).HasForeignKey(r => r.RouteId).OnDelete(DeleteBehavior.Cascade);
        });

        // Індекси для оптимізації вибірок
        builder.Entity<Models.Route>()
            .HasIndex(r => r.RouteStatus)
            .HasDatabaseName("IX_Routes_RouteStatus");
        builder.Entity<Models.Route>()
            .HasIndex(r => r.CategoryId)
            .HasDatabaseName("IX_Routes_CategoryId");
        builder.Entity<Booking>()
            .HasIndex(b => b.TouristId)
            .HasDatabaseName("IX_Bookings_TouristId");
        builder.Entity<Booking>()
            .HasIndex(b => b.BookingStatus)
            .HasDatabaseName("IX_Bookings_BookingStatus");
        builder.Entity<Review>()
            .HasIndex(r => r.RouteId)
            .HasDatabaseName("IX_Reviews_RouteId");

        // Seed categories
        builder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Пішохідні",   IconUrl = "bi-person-walking" },
            new Category { Id = 2, Name = "Велосипедні", IconUrl = "bi-bicycle" },
            new Category { Id = 3, Name = "Культурні",   IconUrl = "bi-building-fill" },
            new Category { Id = 4, Name = "Водні",       IconUrl = "bi-water" },
            new Category { Id = 5, Name = "Гірські",     IconUrl = "bi-mountain" }
        );
    }
}
