using System;
using Microsoft.EntityFrameworkCore;
using SDS.Models;

namespace SDS.Data;

public class SdsDbContext : DbContext
{
    public SdsDbContext(DbContextOptions<SdsDbContext> options) : base(options)
    {

    }

    public DbSet<SDSContent> SDSContents { get; set; } // SDSContent table
    public DbSet<Product> Products { get; set; }
    public DbSet<Headers> Headers { get; set; }
    public DbSet<HeadersH> HeadersH { get; set; }
    public DbSet<HeadersHD> HeadersHD { get; set; }
    public DbSet<HeadersData> HeadersData { get; set; }
    public DbSet<HeaderHImage> HeaderHImages { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<HeadersH>()
            .HasOne(h => h.Headers)
            .WithMany()
            .HasForeignKey(h => h.HeaderId)
            .OnDelete(DeleteBehavior.Cascade); // Keep cascade here

        modelBuilder.Entity<HeadersHD>()
            .HasOne(hd => hd.HeadersH)
            .WithMany()
            .HasForeignKey(hd => hd.HeadersHId)
            .OnDelete(DeleteBehavior.Cascade); // Keep cascade here

        modelBuilder.Entity<HeadersData>()
            .HasOne(hd => hd.HeadersH)
            .WithMany()
            .HasForeignKey(hd => hd.HeadersHId)
            .OnDelete(DeleteBehavior.Restrict); // Change to Restrict or NoAction

        modelBuilder.Entity<HeadersData>()
            .HasOne(hd => hd.HeadersHD)
            .WithMany()
            .HasForeignKey(hd => hd.HeadersHDId)
            .OnDelete(DeleteBehavior.Restrict); // Change to Restrict or NoAction

        modelBuilder.Entity<HeadersData>()
            .HasOne(hd => hd.Product)
            .WithMany()
            .HasForeignKey(hd => hd.ProductId)
            .OnDelete(DeleteBehavior.Cascade); // Keep cascade here
    }
}
