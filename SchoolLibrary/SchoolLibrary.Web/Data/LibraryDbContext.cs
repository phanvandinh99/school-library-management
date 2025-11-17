using Microsoft.EntityFrameworkCore;
using SchoolLibrary.Web.Models;

namespace SchoolLibrary.Web.Data
{
    public class LibraryDbContext : DbContext
    {
        public LibraryDbContext(DbContextOptions<LibraryDbContext> options)
            : base(options)
        {
        }

        // DbSets for each entity
        public DbSet<Role> Roles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<BookCopy> BookCopies { get; set; }
        public DbSet<BorrowRecord> BorrowRecords { get; set; }
        public DbSet<SystemSettings> SystemSettings { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<BookSuggestion> BookSuggestions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Relationships

            // Users -> Role
            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleID)
                .OnDelete(DeleteBehavior.Restrict);

            // Books -> Category
            modelBuilder.Entity<Book>()
                .HasOne(b => b.Category)
                .WithMany(c => c.Books)
                .HasForeignKey(b => b.CategoryID)
                .OnDelete(DeleteBehavior.SetNull);

            // BookCopies -> Book
            modelBuilder.Entity<BookCopy>()
                .HasOne(bc => bc.Book)
                .WithMany(b => b.BookCopies)
                .HasForeignKey(bc => bc.BookID)
                .OnDelete(DeleteBehavior.Cascade);

            // BorrowRecords -> User
            modelBuilder.Entity<BorrowRecord>()
                .HasOne(br => br.User)
                .WithMany(u => u.BorrowRecords)
                .HasForeignKey(br => br.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            // BorrowRecords -> BookCopy
            modelBuilder.Entity<BorrowRecord>()
                .HasOne(br => br.BookCopy)
                .WithMany(bc => bc.BorrowRecords)
                .HasForeignKey(br => br.CopyID)
                .OnDelete(DeleteBehavior.Restrict);

            // Reservations -> User
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            // Reservations -> Book
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Book)
                .WithMany()
                .HasForeignKey(r => r.BookID)
                .OnDelete(DeleteBehavior.Restrict);

            // BookSuggestions -> User
            modelBuilder.Entity<BookSuggestion>()
                .HasOne(bs => bs.User)
                .WithMany()
                .HasForeignKey(bs => bs.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            // Explicit table name mapping để đảm bảo EF tìm đúng bảng
            modelBuilder.Entity<Reservation>()
                .ToTable("Reservations");
            
            modelBuilder.Entity<BookSuggestion>()
                .ToTable("BookSuggestions");

            // Configure unique constraints
            modelBuilder.Entity<Role>()
                .HasIndex(r => r.RoleName)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<Category>()
                .HasIndex(c => c.CategoryName)
                .IsUnique();

            modelBuilder.Entity<BookCopy>()
                .HasIndex(bc => bc.CopyCode)
                .IsUnique();

            modelBuilder.Entity<SystemSettings>()
                .HasIndex(s => s.SettingKey)
                .IsUnique();

            // Configure default values
            modelBuilder.Entity<User>()
                .Property(u => u.IsActive)
                .HasDefaultValue(true);

            modelBuilder.Entity<BookCopy>()
                .Property(bc => bc.Status)
                .HasDefaultValue("Available");

            modelBuilder.Entity<Book>()
                .Property(b => b.TotalCopies)
                .HasDefaultValue(1);

            modelBuilder.Entity<BorrowRecord>()
                .Property(br => br.IsReturned)
                .HasDefaultValue(false);

            modelBuilder.Entity<BorrowRecord>()
                .Property(br => br.FineAmount)
                .HasDefaultValue(0)
                .HasColumnType("decimal(10,2)");

            modelBuilder.Entity<Reservation>()
                .Property(r => r.Status)
                .HasDefaultValue("Pending");

            modelBuilder.Entity<BookSuggestion>()
                .Property(bs => bs.Status)
                .HasDefaultValue("Pending");
        }
    }
}

