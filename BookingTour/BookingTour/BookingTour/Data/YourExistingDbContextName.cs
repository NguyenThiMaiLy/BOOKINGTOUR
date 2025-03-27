using System;
using System.Collections.Generic;
using BookingTour.Models;
using Microsoft.EntityFrameworkCore;

namespace BookingTour.Data;

public partial class YourExistingDbContextName : DbContext
{
    public YourExistingDbContextName()
    {
    }

    public YourExistingDbContextName(DbContextOptions<YourExistingDbContextName> options)
        : base(options)
    {
    }
	public DbSet<ChatMessage> ChatMessages { get; set; }
	public virtual DbSet<HostRegistration> HostRegistrations { get; set; }
    public virtual DbSet<AspNetRole> AspNetRoles { get; set; }

    public virtual DbSet<AspNetRoleClaim> AspNetRoleClaims { get; set; }

    public virtual DbSet<AspNetUser> AspNetUsers { get; set; }

    public virtual DbSet<AspNetUserClaim> AspNetUserClaims { get; set; }

    public virtual DbSet<AspNetUserLogin> AspNetUserLogins { get; set; }

    public virtual DbSet<AspNetUserToken> AspNetUserTokens { get; set; }
  
    public virtual DbSet<Booking> Bookings { get; set; }

    public virtual DbSet<BookingStatus> BookingStatuses { get; set; }

    public virtual DbSet<Chat> Chats { get; set; }

    public virtual DbSet<Comment> Comments { get; set; }

    public virtual DbSet<Destination> Destinations { get; set; }

    public virtual DbSet<Facility> Facilities { get; set; }

    public virtual DbSet<Hotel> Hotels { get; set; }

    public virtual DbSet<Invoice> Invoices { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<PaymentInvoice> PaymentInvoices { get; set; }

    public virtual DbSet<Promotion> Promotions { get; set; }

    public virtual DbSet<Report> Reports { get; set; }

    public virtual DbSet<Room> Rooms { get; set; }

    public virtual DbSet<RoomDetail> RoomDetails { get; set; }

    public virtual DbSet<Service> Services { get; set; }

    public virtual DbSet<Tour> Tours { get; set; }

    public virtual DbSet<TourDetail> TourDetails { get; set; }

    public virtual DbSet<Transportation> Transportations { get; set; }

    public virtual DbSet<TypeOfFacility> TypeOfFacilities { get; set; }

    public virtual DbSet<TypeOfRoom> TypeOfRooms { get; set; }

    public virtual DbSet<TypeOfService> TypeOfServices { get; set; }

    public virtual DbSet<TypeOfTour> TypeOfTours { get; set; }

    public virtual DbSet<UsersFavoriteTour> UsersFavoriteTours { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=LAPTOP-7R00JHKC;Database=BOOKINGTOUR;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AspNetRole>(entity =>
        {
            entity.HasIndex(e => e.NormalizedName, "RoleNameIndex")
                .IsUnique()
                .HasFilter("([NormalizedName] IS NOT NULL)");

            entity.Property(e => e.Name).HasMaxLength(256);
            entity.Property(e => e.NormalizedName).HasMaxLength(256);
        });

        modelBuilder.Entity<AspNetRoleClaim>(entity =>
        {
            entity.HasIndex(e => e.RoleId, "IX_AspNetRoleClaims_RoleId");

            entity.HasOne(d => d.Role).WithMany(p => p.AspNetRoleClaims).HasForeignKey(d => d.RoleId);
        });

        modelBuilder.Entity<AspNetUser>(entity =>
        {
            entity.HasIndex(e => e.NormalizedEmail, "EmailIndex");

            entity.HasIndex(e => e.NormalizedUserName, "UserNameIndex")
                .IsUnique()
                .HasFilter("([NormalizedUserName] IS NOT NULL)");

            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.NormalizedEmail).HasMaxLength(256);
            entity.Property(e => e.NormalizedUserName).HasMaxLength(256);
            entity.Property(e => e.UserName).HasMaxLength(256);

            entity.HasMany(d => d.Roles).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "AspNetUserRole",
                    r => r.HasOne<AspNetRole>().WithMany().HasForeignKey("RoleId"),
                    l => l.HasOne<AspNetUser>().WithMany().HasForeignKey("UserId"),
                    j =>
                    {
                        j.HasKey("UserId", "RoleId");
                        j.ToTable("AspNetUserRoles");
                        j.HasIndex(new[] { "RoleId" }, "IX_AspNetUserRoles_RoleId");
                    });
        });

        modelBuilder.Entity<AspNetUserClaim>(entity =>
        {
            entity.HasIndex(e => e.UserId, "IX_AspNetUserClaims_UserId");

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserClaims).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<AspNetUserLogin>(entity =>
        {
            entity.HasKey(e => new { e.LoginProvider, e.ProviderKey });

            entity.HasIndex(e => e.UserId, "IX_AspNetUserLogins_UserId");

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserLogins).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<AspNetUserToken>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.LoginProvider, e.Name });

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserTokens).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(e => e.IdBooking).HasName("pk_BOOKING");

            entity.ToTable("BOOKING");

            entity.Property(e => e.IdBooking)
                .ValueGeneratedNever()
                .HasColumnName("ID_BOOKING");
            entity.Property(e => e.BookingTime)
                .HasColumnType("datetime")
                .HasColumnName("BOOKING_TIME");
            entity.Property(e => e.CheckInDate)
                .HasColumnType("datetime")
                .HasColumnName("CHECK_IN_DATE");
            entity.Property(e => e.CheckOutDate)
                .HasColumnType("datetime")
                .HasColumnName("CHECK_OUT_DATE");
            entity.Property(e => e.Id).HasMaxLength(450);
            entity.Property(e => e.IdHotel).HasColumnName("ID_HOTEL");
            entity.Property(e => e.IdStatus).HasColumnName("ID_STATUS");
            entity.Property(e => e.IdTour).HasColumnName("ID_TOUR");

            entity.HasOne(d => d.IdNavigation).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.Id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__BOOKING__Id__1C3D2329");

            entity.HasOne(d => d.IdHotelNavigation).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.IdHotel)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__BOOKING__ID_HOTE__22EA20B8");

            entity.HasOne(d => d.IdStatusNavigation).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.IdStatus)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__BOOKING__ID_STAT__28A2FA0E");

            entity.HasOne(d => d.IdTourNavigation).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.IdTour)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__BOOKING__ID_TOUR__1590259A");
        });

        modelBuilder.Entity<BookingStatus>(entity =>
        {
            entity.HasKey(e => e.IdStatus).HasName("pk_BOOKING_STATUS");

            entity.ToTable("BOOKING_STATUS");

            entity.Property(e => e.IdStatus)
                .ValueGeneratedNever()
                .HasColumnName("ID_STATUS");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("CREATED_AT");
            entity.Property(e => e.Description)
                .HasMaxLength(100)
                .HasColumnName("DESCRIPTION");
            entity.Property(e => e.StatusName)
                .HasMaxLength(50)
                .HasColumnName("STATUS_NAME");
        });
        modelBuilder.Entity<HostRegistration>()
            .HasOne(h => h.User) // Mỗi HostRegistration thuộc về một User
            .WithMany() // Một User có thể có nhiều HostRegistration
            .HasForeignKey(h => h.UserId) // Khóa ngoại
            .OnDelete(DeleteBehavior.Cascade); // Hành động khi xóa
        modelBuilder.Entity<Chat>(entity =>
        {
            entity.HasKey(e => e.IdChat).HasName("pk_CHAT");

            entity.ToTable("CHAT");

            entity.Property(e => e.IdChat)
                .ValueGeneratedNever()
                .HasColumnName("ID_CHAT");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("CREATED_AT");
            entity.Property(e => e.Id).HasMaxLength(450);
            entity.Property(e => e.Message)
                .HasMaxLength(100)
                .HasColumnName("MESSAGE");

            entity.HasOne(d => d.IdNavigation).WithMany(p => p.Chats)
                .HasForeignKey(d => d.Id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CHAT__Id__1A54DAB7");
        });

        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(e => new { e.IdTour, e.Id }).HasName("pk_COMMENT");

            entity.ToTable("COMMENT");

            entity.Property(e => e.IdTour).HasColumnName("ID_TOUR");
            entity.Property(e => e.CommentDate)
                .HasColumnType("datetime")
                .HasColumnName("COMMENT_DATE");
            entity.Property(e => e.Content)
                .HasMaxLength(100)
                .HasColumnName("CONTENT");
            entity.Property(e => e.Reply)
                .HasMaxLength(100)
                .HasColumnName("REPLY");

            entity.HasOne(d => d.IdNavigation).WithMany(p => p.Comments)
                .HasForeignKey(d => d.Id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__COMMENT__Id__17786E0C");

            entity.HasOne(d => d.IdTourNavigation).WithMany(p => p.Comments)
                .HasForeignKey(d => d.IdTour)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__COMMENT__ID_TOUR__10CB707D");
        });

        modelBuilder.Entity<Destination>(entity =>
        {
            entity.HasKey(e => e.IdDes).HasName("pk_DESTINATION");

            entity.ToTable("DESTINATION");

            entity.Property(e => e.IdDes)
                .ValueGeneratedNever()
                .HasColumnName("ID_DES");
            entity.Property(e => e.Address)
                .HasMaxLength(255)
                .HasColumnName("ADDRESS");
            entity.Property(e => e.Description)
                .HasMaxLength(200)
                .HasColumnName("DESCRIPTION");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("NAME");
        });

        modelBuilder.Entity<Facility>(entity =>
        {
            entity.HasKey(e => e.IdFacility).HasName("pk_FACILITY");

            entity.ToTable("FACILITY");

            entity.Property(e => e.IdFacility)
                .ValueGeneratedNever()
                .HasColumnName("ID_FACILITY");
            entity.Property(e => e.Description)
                .HasMaxLength(100)
                .HasColumnName("DESCRIPTION");
            entity.Property(e => e.IdType).HasColumnName("ID_TYPE");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("NAME");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasColumnName("STATUS");

            entity.HasOne(d => d.IdTypeNavigation).WithMany(p => p.Facilities)
                .HasForeignKey(d => d.IdType)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__FACILITY__ID_TYP__2D67AF2B");
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

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(e => e.IdInvoice).HasName("pk_INVOICE");

            entity.ToTable("INVOICE");

            entity.Property(e => e.IdInvoice)
                .ValueGeneratedNever()
                .HasColumnName("ID_INVOICE")
                .ValueGeneratedOnAdd();
            entity.Property(e => e.BillingDate)
                .HasColumnType("datetime")
                .HasColumnName("BILLING_DATE");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("CREATED_AT");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("DESCRIPTION");
            entity.Property(e => e.TotalAmount)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("TOTAL_AMOUNT");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.IdPayment).HasName("pk_PAYMENT");

            entity.ToTable("PAYMENT");

            entity.Property(e => e.IdPayment)
                .ValueGeneratedNever()
                .HasColumnName("ID_PAYMENT");
            entity.Property(e => e.Amount)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("AMOUNT");
            entity.Property(e => e.Date)
                .HasColumnType("datetime")
                .HasColumnName("DATE");
            entity.Property(e => e.Id).HasMaxLength(450);
            entity.Property(e => e.IsRefunded)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasColumnName("IS_REFUNDED");
            entity.Property(e => e.Method)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("METHOD");
            entity.Property(e => e.RefundAmount)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("REFUND_AMOUNT");
            entity.Property(e => e.RefundDate)
                .HasColumnType("datetime")
                .HasColumnName("REFUND_DATE");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("STATUS");

            entity.HasOne(d => d.IdNavigation).WithMany(p => p.Payments)
                .HasForeignKey(d => d.Id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PAYMENT__Id__186C9245");
        });

        modelBuilder.Entity<PaymentInvoice>(entity =>
        {
            entity.HasKey(e => new { e.IdPayment, e.IdInvoice }).HasName("pk_PAYMENT_INVOICE");

            entity.ToTable("PAYMENT_INVOICE");

            entity.Property(e => e.IdPayment).HasColumnName("ID_PAYMENT");
            entity.Property(e => e.IdInvoice).HasColumnName("ID_INVOICE");
            entity.Property(e => e.PaidAmount)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("PAID_AMOUNT");
            entity.Property(e => e.PaymentDate)
                .HasColumnType("datetime")
                .HasColumnName("PAYMENT_DATE");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("STATUS");

            entity.HasOne(d => d.IdInvoiceNavigation).WithMany(p => p.PaymentInvoices)
                .HasForeignKey(d => d.IdInvoice)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PAYMENT_I__ID_IN__27AED5D5");

            entity.HasOne(d => d.IdPaymentNavigation).WithMany(p => p.PaymentInvoices)
                .HasForeignKey(d => d.IdPayment)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PAYMENT_I__ID_PA__24D2692A");
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

            entity.HasOne(d => d.IdHotelNavigation).WithMany(p => p.Promotions)
                .HasForeignKey(d => d.IdHotel)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PROMOTION__ID_HO__2101D846");

            entity.HasOne(d => d.IdTourNavigation).WithMany(p => p.Promotions)
                .HasForeignKey(d => d.IdTour)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PROMOTION__ID_TO__13A7DD28");

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

        modelBuilder.Entity<Report>(entity =>
        {
            entity.HasKey(e => e.IdReport).HasName("pk_REPORT");

            entity.ToTable("REPORT");

            entity.Property(e => e.IdReport)
                .ValueGeneratedNever()
                .HasColumnName("ID_REPORT");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("CREATED_AT");
            entity.Property(e => e.Id).HasMaxLength(450);
            entity.Property(e => e.IdTour).HasColumnName("ID_TOUR");
            entity.Property(e => e.Rating)
                .HasMaxLength(100)
                .HasColumnName("RATING");

            entity.HasOne(d => d.IdNavigation).WithMany(p => p.Reports)
                .HasForeignKey(d => d.Id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__REPORT__Id__1B48FEF0");

            entity.HasOne(d => d.IdTourNavigation).WithMany(p => p.Reports)
                .HasForeignKey(d => d.IdTour)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__REPORT__ID_TOUR__149C0161");
        });

        modelBuilder.Entity<Room>(entity =>
        {
            entity.HasKey(e => e.IdRoom).HasName("pk_ROOM");

            entity.ToTable("ROOM");

            entity.Property(e => e.IdRoom)
                .ValueGeneratedNever()
                .HasColumnName("ID_ROOM");
            entity.Property(e => e.IdHotel).HasColumnName("ID_HOTEL");
            entity.Property(e => e.IdType).HasColumnName("ID_TYPE");
            entity.Property(e => e.Price)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("PRICE");
            entity.Property(e => e.RoomNumber)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("ROOM_NUMBER");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("STATUS");

            entity.HasOne(d => d.IdHotelNavigation).WithMany(p => p.Rooms)
                .HasForeignKey(d => d.IdHotel)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ROOM__ID_HOTEL__23DE44F1");

            entity.HasOne(d => d.IdTypeNavigation).WithMany(p => p.Rooms)
                .HasForeignKey(d => d.IdType)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ROOM__ID_TYPE__2A8B4280");
        });

        modelBuilder.Entity<RoomDetail>(entity =>
        {
            entity.HasKey(e => new { e.IdRoom, e.IdFacility }).HasName("pk_ROOM_DETAIL");

            entity.ToTable("ROOM_DETAIL");

            entity.Property(e => e.IdRoom).HasColumnName("ID_ROOM");
            entity.Property(e => e.IdFacility).HasColumnName("ID_FACILITY");
            entity.Property(e => e.Available)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("AVAILABLE");
            entity.Property(e => e.Condition)
                .HasMaxLength(50)
                .HasColumnName("CONDITION");
            entity.Property(e => e.Quanlity).HasColumnName("QUANLITY");

            entity.HasOne(d => d.IdFacilityNavigation).WithMany(p => p.RoomDetails)
                .HasForeignKey(d => d.IdFacility)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ROOM_DETA__ID_FA__2C738AF2");

            entity.HasOne(d => d.IdRoomNavigation).WithMany(p => p.RoomDetails)
                .HasForeignKey(d => d.IdRoom)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ROOM_DETA__ID_RO__29971E47");
        });

        modelBuilder.Entity<Service>(entity =>
        {
            entity.HasKey(e => e.IdService).HasName("pk_SERVICE");

            entity.ToTable("SERVICE");

            entity.Property(e => e.IdService)
                .ValueGeneratedNever()
                .HasColumnName("ID_SERVICE");
            entity.Property(e => e.Available)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("AVAILABLE");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("CREATED_AT");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("DESCRIPTION");
            entity.Property(e => e.IdHotel).HasColumnName("ID_HOTEL");
            entity.Property(e => e.IdTour).HasColumnName("ID_TOUR");
            entity.Property(e => e.IdType).HasColumnName("ID_TYPE");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("NAME");
            entity.Property(e => e.Price)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("PRICE");

            entity.HasOne(d => d.IdHotelNavigation).WithMany(p => p.Services)
                .HasForeignKey(d => d.IdHotel)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__SERVICE__ID_HOTE__21F5FC7F");

            entity.HasOne(d => d.IdTourNavigation).WithMany(p => p.Services)
                .HasForeignKey(d => d.IdTour)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("");

            entity.HasOne(d => d.IdTypeNavigation).WithMany(p => p.Services)
                .HasForeignKey(d => d.IdType)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__SERVICE__ID_TYPE__2B7F66B9");
        });

        modelBuilder.Entity<Tour>(entity =>
        {
            entity.HasKey(e => e.IdTour).HasName("pk_TOUR");

            entity.ToTable("TOUR");

            entity.Property(e => e.IdTour)
                .ValueGeneratedNever()
                .HasColumnName("ID_TOUR");
            entity.Property(e => e.Description)
                .HasMaxLength(1000)
                .HasColumnName("DESCRIPTION");
            entity.Property(e => e.EndDate)
                .HasColumnType("datetime")
                .HasColumnName("END_DATE");
            entity.Property(e => e.IdHotel).HasColumnName("ID_HOTEL");
            entity.Property(e => e.IdTrans).HasColumnName("ID_TRANS");
            entity.Property(e => e.IdType).HasColumnName("ID_TYPE");
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
        });

        modelBuilder.Entity<TourDetail>(entity =>
        {
            entity.HasKey(e => new { e.IdTour, e.IdDes }).HasName("pk_TOUR_DETAILS");

            entity.ToTable("TOUR_DETAILS");

            entity.Property(e => e.IdTour).HasColumnName("ID_TOUR");
            entity.Property(e => e.IdDes).HasColumnName("ID_DES");
            entity.Property(e => e.Itinerary)
                .HasMaxLength(2000)
                .HasColumnName("ITINERARY");

            entity.HasOne(d => d.IdDesNavigation).WithMany(p => p.TourDetails)
                .HasForeignKey(d => d.IdDes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TOUR_DETA__ID_DE__25C68D63");

            entity.HasOne(d => d.IdTourNavigation).WithMany(p => p.TourDetails)
                .HasForeignKey(d => d.IdTour)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TOUR_DETA__ID_TO__11BF94B6");
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

        modelBuilder.Entity<TypeOfFacility>(entity =>
        {
            entity.HasKey(e => e.IdType).HasName("pk_TYPE_OF_FACILITY");

            entity.ToTable("TYPE_OF_FACILITY");

            entity.Property(e => e.IdType)
                .ValueGeneratedNever()
                .HasColumnName("ID_TYPE");
            entity.Property(e => e.Description)
                .HasMaxLength(100)
                .HasColumnName("DESCRIPTION");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("NAME");
        });

        modelBuilder.Entity<TypeOfRoom>(entity =>
        {
            entity.HasKey(e => e.IdType).HasName("pk_TYPE_OF_ROOM");

            entity.ToTable("TYPE_OF_ROOM");

            entity.Property(e => e.IdType)
                .ValueGeneratedNever()
                .HasColumnName("ID_TYPE");
            entity.Property(e => e.Description)
                .HasMaxLength(100)
                .HasColumnName("DESCRIPTION");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("NAME");
        });

        modelBuilder.Entity<TypeOfService>(entity =>
        {
            entity.HasKey(e => e.IdType).HasName("pk_TYPE_OF_SERVICE");

            entity.ToTable("TYPE_OF_SERVICE");

            entity.Property(e => e.IdType)
                .ValueGeneratedNever()
                .HasColumnName("ID_TYPE");
            entity.Property(e => e.Description)
                .HasMaxLength(100)
                .HasColumnName("DESCRIPTION");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("NAME");
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

        modelBuilder.Entity<UsersFavoriteTour>(entity =>
        {
            entity.HasKey(e => new { e.Id, e.IdTour }).HasName("pk_USERS_FAVORITE_TOUR");

            entity.ToTable("USERS_FAVORITE_TOUR");

            entity.Property(e => e.IdTour).HasColumnName("ID_TOUR");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("CREATED_AT");

            entity.HasOne(d => d.IdNavigation).WithMany(p => p.UsersFavoriteTours)
                .HasForeignKey(d => d.Id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__USERS_FAVORI__Id__1960B67E");

            entity.HasOne(d => d.IdTourNavigation).WithMany(p => p.UsersFavoriteTours)
                .HasForeignKey(d => d.IdTour)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__USERS_FAV__ID_TO__12B3B8EF");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

public DbSet<BookingTour.Models.Blog> Blog { get; set; } = default!;

}
