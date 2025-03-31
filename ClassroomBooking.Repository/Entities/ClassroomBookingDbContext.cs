using ClassroomBooking.Repository.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClassroomBooking.Repository
{
    public partial class ClassroomBookingDbContext : DbContext
    {
        public ClassroomBookingDbContext(DbContextOptions<ClassroomBookingDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Booking> Bookings { get; set; } = null!;
        public virtual DbSet<Campus> Campuses { get; set; } = null!;
        public virtual DbSet<Department> Departments { get; set; } = null!;
        public virtual DbSet<Room> Rooms { get; set; } = null!;
        public virtual DbSet<RoomSlot> RoomSlots { get; set; } = null!;
        public virtual DbSet<Users> Users { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Booking>(entity =>
            {
                entity.HasKey(e => e.BookingId);
                entity.Property(e => e.BookingStatus).HasMaxLength(50);
                entity.Property(e => e.Purpose).IsRequired();
                entity.HasOne(e => e.User)
                      .WithMany(u => u.Bookings)
                      .HasForeignKey(e => e.StudentId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Campus>(entity =>
            {
                entity.HasKey(e => e.CampusId);
                entity.Property(e => e.CampusName).HasMaxLength(100);
            });

            modelBuilder.Entity<Department>(entity =>
            {
                entity.HasKey(e => e.DepartmentId);
                entity.Property(e => e.DepartmentName).HasMaxLength(100);
            });

            modelBuilder.Entity<Room>(entity =>
            {
                entity.HasKey(e => e.RoomId);
                entity.Property(e => e.RoomName).HasMaxLength(100);
                entity.Property(e => e.Capacity).HasDefaultValue(1);
                entity.Property(e => e.Status).HasMaxLength(50);
                entity.Property(e => e.Description).HasMaxLength(255);
                entity.HasOne(e => e.Campus)
                      .WithMany(c => c.Rooms)
                      .HasForeignKey(e => e.CampusId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<RoomSlot>(entity =>
            {
                entity.HasKey(e => e.RoomSlotId);
                entity.HasOne(e => e.Booking)
                      .WithMany(b => b.RoomSlots)
                      .HasForeignKey(e => e.BookingId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Room)
                      .WithMany(r => r.RoomSlots)
                      .HasForeignKey(e => e.RoomId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Users>(entity =>
            {
                entity.HasKey(e => e.USerId);
                entity.Property(e => e.UserCode).HasMaxLength(20);
                entity.Property(e => e.FullName).HasMaxLength(100);
                entity.Property(e => e.Email).HasMaxLength(100);
                entity.Property(e => e.Password).HasMaxLength(100);
                entity.Property(e => e.Role).HasMaxLength(20);
                entity.HasOne(e => e.Department)
                      .WithMany(d => d.Users)
                      .HasForeignKey(e => e.DepartmentId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

        }
    }
}