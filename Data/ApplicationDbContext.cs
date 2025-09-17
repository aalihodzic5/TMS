using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TMS.Models;

namespace TMS.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        //public DbSet<User> Users { get; set; }
        public DbSet<Driver> Driver { get; set; }
        public DbSet<DriverLicence> DriverLicences { get; set; }
        public DbSet<Notification> Notification { get; set; }
        public DbSet<Offer> Offer { get; set; }
        public DbSet<OfferUser> OfferUsers { get; set; }
        public DbSet<Payment> Payment { get; set; }
        public DbSet<SavedJob> SavedJob { get; set; }
        public DbSet<Trailer> Trailer { get; set; }
        public DbSet<Subscription> Subscription { get; set; }
        public DbSet<Job> Job { get; set; }
        public DbSet<Truck> Truck { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Mapiranje tabela
            modelBuilder.Entity<Driver>().ToTable("Driver");
            modelBuilder.Entity<DriverLicence>().ToTable("DriverLicence");
            modelBuilder.Entity<Notification>().ToTable("Notification");
            modelBuilder.Entity<Offer>().ToTable("Offer");
            modelBuilder.Entity<OfferUser>().ToTable("OfferUser");
            modelBuilder.Entity<Payment>().ToTable("Payment");
            modelBuilder.Entity<SavedJob>().ToTable("SavedJob");
            modelBuilder.Entity<Trailer>().ToTable("Trailer");
            modelBuilder.Entity<Subscription>().ToTable("Subscription");
            modelBuilder.Entity<Job>().ToTable("Job");
            modelBuilder.Entity<Truck>().ToTable("Truck");

            // --- SPRIJEČI multiple cascade paths ---


            modelBuilder.Entity<SavedJob>()
        .HasIndex(x => new { x.UserId, x.JobId })
        .IsUnique(); // jedan user može sačuvati dati job samo jednom

            modelBuilder.Entity<SavedJob>()
                .HasOne(s => s.User).WithMany() // ili .WithMany(u => u.SavedJobs) ako dodaš kolekciju u User
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SavedJob>()
                .HasOne(s => s.Job).WithMany() // ili .WithMany(j => j.SavedBy) ako dodaš kolekciju u Job
                .HasForeignKey(s => s.JobId)
                .OnDelete(DeleteBehavior.Cascade);

            // (preporuka) Trailer -> Truck: spriječi nenamjerni kaskadni delete
            modelBuilder.Entity<Trailer>()
                .HasOne(t => t.Truck).WithMany()
                .HasForeignKey(t => t.TruckId)
                .OnDelete(DeleteBehavior.Restrict);


            // OfferUser -> Offer
            modelBuilder.Entity<OfferUser>()
                .HasOne(ou => ou.Offer)
                .WithMany()
                .HasForeignKey(ou => ou.OfferId)
                .OnDelete(DeleteBehavior.Restrict); // ili NoAction

            // Payment -> Offer
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Offer)
                .WithMany()
                .HasForeignKey(p => p.OfferId)
                .OnDelete(DeleteBehavior.Restrict);

            // Payment -> User
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade); // ovo je OK

            // Subscription -> Payment
            modelBuilder.Entity<Subscription>()
                .HasOne(s => s.Payment)
                .WithMany()
                .HasForeignKey(s => s.PaymentId)
                .OnDelete(DeleteBehavior.Restrict); // KLJUČNO

            // Subscription -> User
            modelBuilder.Entity<Subscription>()
                .HasOne(s => s.User)
                .WithMany()
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade); // ovo je OK
        }
    }
}
