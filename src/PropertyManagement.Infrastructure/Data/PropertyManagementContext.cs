using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using PropertyManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropertyManagement.Infrastructure.Data;

public class PropertyManagementContext : DbContext
{
    public PropertyManagementContext(DbContextOptions<PropertyManagementContext> options)
        : base(options)
    {
    }

    public DbSet<Owner> Owners { get; set; }
    public DbSet<Property> Properties { get; set; }
    public DbSet<PropertyImage> PropertyImages { get; set; }
    public DbSet<PropertyTrace> PropertyTraces { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Owner configuration
        modelBuilder.Entity<Owner>(entity =>
        {
            entity.HasKey(e => e.IdOwner);
            entity.Property(e => e.IdOwner).UseIdentityColumn();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Address).HasMaxLength(255);
            entity.Property(e => e.Birthday).HasColumnType("date");
        });

        // Property configuration
        modelBuilder.Entity<Property>(entity =>
        {
            entity.HasKey(e => e.IdProperty);
            entity.Property(e => e.IdProperty).UseIdentityColumn();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Address).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
            entity.Property(e => e.CodeInternal).HasMaxLength(50);
            entity.Property(e => e.Enabled).HasDefaultValue(true);

            entity.HasOne(d => d.Owner)
                  .WithMany(p => p.Properties)
                  .HasForeignKey(d => d.IdOwner)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.IdOwner).HasDatabaseName("IX_Property_IdOwner");
        });

        // PropertyImage configuration
        modelBuilder.Entity<PropertyImage>(entity =>
        {
            entity.HasKey(e => e.IdPropertyImage);
            entity.Property(e => e.IdPropertyImage).UseIdentityColumn();
            entity.Property(e => e.Enabled).HasDefaultValue(true);

            entity.HasOne(d => d.Property)
                  .WithMany(p => p.PropertyImages)
                  .HasForeignKey(d => d.IdProperty)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.IdProperty).HasDatabaseName("IX_PropertyImage_IdProperty");
        });

        // PropertyTrace configuration
        modelBuilder.Entity<PropertyTrace>(entity =>
        {
            entity.HasKey(e => e.IdPropertyTrace);
            entity.Property(e => e.IdPropertyTrace).UseIdentityColumn();
            entity.Property(e => e.DateSale).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Value).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Tax).HasColumnType("decimal(18,2)");

            entity.HasOne(d => d.Property)
                  .WithMany(p => p.PropertyTraces)
                  .HasForeignKey(d => d.IdProperty)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.IdProperty).HasDatabaseName("IX_PropertyTrace_IdProperty");
            entity.HasIndex(e => e.DateSale).HasDatabaseName("IX_PropertyTrace_DateSale");
        });

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.IdUser);
            entity.Property(e => e.IdUser).UseIdentityColumn();
            entity.Property(e => e.Username).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Role).HasMaxLength(50).HasDefaultValue("User");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasDefaultValue(DateTime.UtcNow);

            // Unique constraints
            entity.HasIndex(e => e.Username).IsUnique().HasDatabaseName("IX_User_Username");
            entity.HasIndex(e => e.Email).IsUnique().HasDatabaseName("IX_User_Email");
        });

        // RefreshToken configuration
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).UseIdentityColumn();
            entity.Property(e => e.Token).IsRequired();
            entity.Property(e => e.ExpiresAt).IsRequired();
            entity.Property(e => e.IsRevoked).HasDefaultValue(false);
            entity.Property(e => e.CreatedAt).HasDefaultValue(DateTime.UtcNow);

            entity.HasOne(d => d.User)
                  .WithMany(p => p.RefreshTokens)
                  .HasForeignKey(d => d.IdUser)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.Token).IsUnique().HasDatabaseName("IX_RefreshToken_Token");
            entity.HasIndex(e => e.IdUser).HasDatabaseName("IX_RefreshToken_IdUser");
            entity.HasIndex(e => e.ExpiresAt).HasDatabaseName("IX_RefreshToken_ExpiresAt");
        });

        // Seed data
        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        // Seed Users (Admin and Test User)
        modelBuilder.Entity<User>().HasData(
            new User
            {
                IdUser = 1,
                Username = "admin",
                Email = "admin@propertymanagement.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"), // Default admin password
                FirstName = "System",
                LastName = "Administrator",
                Role = "Admin",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new User
            {
                IdUser = 2,
                Username = "testuser",
                Email = "test@propertymanagement.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test123!"), // Default test password
                FirstName = "Test",
                LastName = "User",
                Role = "User",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        );

        // Seed Owners
        modelBuilder.Entity<Owner>().HasData(
            new Owner
            {
                IdOwner = 1,
                Name = "John Doe",
                Address = "123 Main St, New York, NY",
                Birthday = new DateTime(1980, 1, 1)
            },
            new Owner
            {
                IdOwner = 2,
                Name = "Jane Smith",
                Address = "456 Oak Ave, Los Angeles, CA",
                Birthday = new DateTime(1975, 5, 15)
            }
        );

        // Seed Properties
        modelBuilder.Entity<Property>().HasData(
            new Property
            {
                IdProperty = 1,
                Name = "Luxury Apartment",
                Address = "789 Park Ave, New York, NY",
                Price = 1500000.00m,
                CodeInternal = "NYC001",
                Year = 2020,
                IdOwner = 1,
                Enabled = true
            },
            new Property
            {
                IdProperty = 2,
                Name = "Beach House",
                Address = "321 Ocean Dr, Los Angeles, CA",
                Price = 2200000.00m,
                CodeInternal = "LA001",
                Year = 2018,
                IdOwner = 2,
                Enabled = true
            }
        );
    }
}
