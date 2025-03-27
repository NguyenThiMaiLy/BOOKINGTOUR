using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace BookingTour.Models;

public partial class BookingtourContext : DbContext
{
    public BookingtourContext()
    {
    }

    public BookingtourContext(DbContextOptions<BookingtourContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AspNetUser> AspNetUsers { get; set; }

    public virtual DbSet<Promotion> Promotions { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=LAPTOP-7R00JHKC;Database=BOOKINGTOUR;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AspNetUser>(entity =>
        {
            entity.HasIndex(e => e.NormalizedEmail, "EmailIndex");

            entity.HasIndex(e => e.NormalizedUserName, "UserNameIndex")
                .IsUnique()
                .HasFilter("([NormalizedUserName] IS NOT NULL)");

            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.fullname)
                .HasDefaultValue("")
                .HasColumnName("fullname");
            entity.Property(e => e.NormalizedEmail).HasMaxLength(256);
            entity.Property(e => e.NormalizedUserName).HasMaxLength(256);
            entity.Property(e => e.UserName).HasMaxLength(256);
        });

        modelBuilder.Entity<Promotion>(entity =>
        {
            entity.HasKey(e => e.IdPmt).HasName("pk_PROMOTION");

            entity.ToTable("PROMOTION");

            entity.Property(e => e.IdPmt)
                .ValueGeneratedNever()
                .HasColumnName("ID_PMT");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("DESCRIPTION");
            entity.Property(e => e.DiscountAmount)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("DISCOUNT_AMOUNT");
            entity.Property(e => e.EligibilityCriteria)
                .HasMaxLength(100)
                .HasColumnName("ELIGIBILITY_CRITERIA");
            entity.Property(e => e.EndDate)
                .HasColumnType("datetime")
                .HasColumnName("END_DATE");
            entity.Property(e => e.IdHotel).HasColumnName("ID_HOTEL");
            entity.Property(e => e.IdTour).HasColumnName("ID_TOUR");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("NAME");
            entity.Property(e => e.StartDate)
                .HasColumnType("datetime")
                .HasColumnName("START_DATE");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("STATUS");

            entity.HasMany(d => d.Ids).WithMany(p => p.IdPmts)
                .UsingEntity<Dictionary<string, object>>(
                    "PromotionUser",
                    r => r.HasOne<AspNetUser>().WithMany()
                        .HasForeignKey("Id")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__PROMOTION_US__Id__1D314762"),
                    l => l.HasOne<Promotion>().WithMany()
                        .HasForeignKey("IdPmt")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__PROMOTION__ID_PM__1E256B9B"),
                    j =>
                    {
                        j.HasKey("IdPmt", "Id").HasName("pk_PROMOTION_USER");
                        j.ToTable("PROMOTION_USER");
                        j.IndexerProperty<long>("IdPmt").HasColumnName("ID_PMT");
                    });
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
