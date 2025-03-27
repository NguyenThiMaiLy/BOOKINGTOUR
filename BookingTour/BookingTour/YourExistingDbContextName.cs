using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace BookingTour.Models;

public partial class YourExistingDbContextName : DbContext
{
    public YourExistingDbContextName()
    {
    }

    public YourExistingDbContextName(DbContextOptions<YourExistingDbContextName> options)
        : base(options)
    {
    }

    public virtual DbSet<AspNetUser> AspNetUsers { get; set; }

    public virtual DbSet<Hotel> Hotels { get; set; }

    public virtual DbSet<Tour> Tours { get; set; }

    public virtual DbSet<Transportation> Transportations { get; set; }

    public virtual DbSet<TypeOfTour> TypeOfTours { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Name=DefaultConnection");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AspNetUser>(entity =>
        {
            entity.HasIndex(e => e.NormalizedEmail, "EmailIndex");

            entity.HasIndex(e => e.NormalizedUserName, "UserNameIndex")
                .IsUnique()
                .HasFilter("([NormalizedUserName] IS NOT NULL)");

            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.Fullname)
                .HasDefaultValue("")
                .HasColumnName("fullname");
            entity.Property(e => e.NormalizedEmail).HasMaxLength(256);
            entity.Property(e => e.NormalizedUserName).HasMaxLength(256);
            entity.Property(e => e.UserName).HasMaxLength(256);
        });

        modelBuilder.Entity<Hotel>(entity =>
        {
            entity.HasKey(e => e.IdHotel).HasName("pk_HOTEL");

            entity.ToTable("HOTEL");

            entity.Property(e => e.IdHotel)
                .ValueGeneratedNever()
                .HasColumnName("ID_HOTEL");
            entity.Property(e => e.Available)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("AVAILABLE");
            entity.Property(e => e.Description)
                .HasMaxLength(1000)
                .HasColumnName("DESCRIPTION");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("EMAIL");
            entity.Property(e => e.Image)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("IMAGE");
            entity.Property(e => e.Location)
                .HasMaxLength(200)
                .HasColumnName("LOCATION");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("NAME");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(11)
                .IsUnicode(false)
                .HasColumnName("PHONE_NUMBER");
            entity.Property(e => e.StarRating).HasColumnName("STAR_RATING");
        });

        modelBuilder.Entity<Tour>(entity =>
        {
            entity.HasKey(e => e.IdTour).HasName("pk_TOUR");

            entity.ToTable("TOUR");

            entity.Property(e => e.IdTour)
                .ValueGeneratedNever()
                .HasColumnName("ID_TOUR");
            entity.Property(e => e.ApprovalStatus).HasMaxLength(50);
            entity.Property(e => e.Description)
                .HasMaxLength(1000)
                .HasColumnName("DESCRIPTION");
            entity.Property(e => e.EndDate)
                .HasColumnType("datetime")
                .HasColumnName("END_DATE");
            entity.Property(e => e.IdHotel).HasColumnName("ID_HOTEL");
            entity.Property(e => e.IdTrans).HasColumnName("ID_TRANS");
            entity.Property(e => e.IdType).HasColumnName("ID_TYPE");
            entity.Property(e => e.IdUser).HasMaxLength(450);
            entity.Property(e => e.Image)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("IMAGE");
            entity.Property(e => e.IsDelete)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasColumnName("IS_DELETE");
            entity.Property(e => e.MaxQuantity).HasColumnName("MAX_QUANTITY");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("NAME");
            entity.Property(e => e.Price)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("PRICE");
            entity.Property(e => e.StartDate)
                .HasColumnType("datetime")
                .HasColumnName("START_DATE");

            entity.HasOne(d => d.IdHotelNavigation).WithMany(p => p.Tours)
                .HasForeignKey(d => d.IdHotel)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TOUR__ID_HOTEL__200DB40D");

            entity.HasOne(d => d.IdTransNavigation).WithMany(p => p.Tours)
                .HasForeignKey(d => d.IdTrans)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TOUR__ID_TRANS__26BAB19C");

            entity.HasOne(d => d.IdTypeNavigation).WithMany(p => p.Tours)
                .HasForeignKey(d => d.IdType)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TOUR__ID_TYPE__1F198FD4");

            entity.HasOne(d => d.IdUserNavigation).WithMany(p => p.Tours)
                .HasForeignKey(d => d.IdUser)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Tour_AspNetUsers_IdUser");
        });

        modelBuilder.Entity<Transportation>(entity =>
        {
            entity.HasKey(e => e.IdTrans).HasName("pk_TRANSPORTATION");

            entity.ToTable("TRANSPORTATION");

            entity.Property(e => e.IdTrans)
                .ValueGeneratedNever()
                .HasColumnName("ID_TRANS");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("NAME");
            entity.Property(e => e.SeatAvailability).HasColumnName("SEAT_AVAILABILITY");
            entity.Property(e => e.Type)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("TYPE");
        });

        modelBuilder.Entity<TypeOfTour>(entity =>
        {
            entity.HasKey(e => e.IdType).HasName("pk_TYPE_OF_TOUR");

            entity.ToTable("TYPE_OF_TOUR");

            entity.Property(e => e.IdType)
                .ValueGeneratedNever()
                .HasColumnName("ID_TYPE");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("DESCRIPTION");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("NAME");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
