using ClyvoDayApiWeb.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace ClyvoDayApiWeb.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Tutor> Tutors { get; set; }
        public DbSet<Veterinarian> Veterinarians { get; set; }
        public DbSet<Pet> Pets { get; set; }
        public DbSet<PetMonitoring> PetMonitorings { get; set; }
        public DbSet<CareEvent> CareEvents { get; set; }
        public DbSet<DailyPetLog> DailyPetLogs { get; set; }
        public DbSet<CommunityPost> CommunityPosts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("USERS");
                entity.HasKey(u => u.UserId);
                entity.Property(u => u.UserId).ValueGeneratedOnAdd();
                entity.Property(u => u.FullName).IsRequired().HasMaxLength(150);
                entity.Property(u => u.Email).IsRequired().HasMaxLength(150);
                entity.HasIndex(u => u.Email).IsUnique();
                entity.Property(u => u.PasswordHash).IsRequired().HasMaxLength(255);
                entity.Property(u => u.PhoneNumber).HasMaxLength(20);
                entity.Property(u => u.TypeUser).IsRequired();
                entity.Property(u => u.IsActive).IsRequired();
                entity.Property(u => u.CreatedAt).IsRequired();
            });

            modelBuilder.Entity<Tutor>(entity =>
            {
                entity.ToTable("TUTORS");
                entity.Property(t => t.ScoreEngagement).IsRequired();
                entity.Property(t => t.Achievement).IsRequired();
            });

            modelBuilder.Entity<Veterinarian>(entity =>
            {
                entity.ToTable("VETERINARIANS");
                entity.Property(v => v.Crmv).IsRequired().HasMaxLength(20);
                entity.Property(v => v.State).IsRequired().HasMaxLength(2);
                entity.Property(v => v.Specialty).HasMaxLength(100);
                entity.HasIndex(v => new{v.Crmv,v.State}).IsUnique();
            });

            modelBuilder.Entity<Pet>(entity =>
            {
                entity.ToTable("PETS");
                entity.HasKey(p => p.PetId);
                entity.Property(p => p.PetId).ValueGeneratedOnAdd();
                entity.Property(p => p.Name).IsRequired().HasMaxLength(100);
                entity.Property(p => p.Species).IsRequired().HasMaxLength(50);
                entity.Property(p => p.Breed).HasMaxLength(100);
                entity.Property(p => p.Sex).IsRequired().HasMaxLength(20);
                entity.Property(p => p.BirthDate).IsRequired();
                entity.HasOne(p => p.Tutor).WithMany(t => t.Pets).HasForeignKey(p => p.TutorId).OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(p => p.PetMonitorings).WithOne(pm => pm.Pet).HasForeignKey(pm => pm.PetId).OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(p => p.CareEvents).WithOne(ce => ce.Pet).HasForeignKey(ce => ce.PetId).OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(p => p.DailyPetLogs).WithOne(dpl => dpl.Pet).HasForeignKey(dpl => dpl.PetId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<PetMonitoring>(entity =>
            {
                entity.ToTable("PET_MONITORINGS");
                entity.HasKey(pm => pm.PetMonitoringId);
                entity.Property(pm => pm.PetMonitoringId).ValueGeneratedOnAdd();
                entity.Property(pm => pm.Mood).HasMaxLength(50);
                entity.Property(pm => pm.EnergyLevel).HasMaxLength(50);
                entity.Property(pm => pm.HydrationLevel).HasMaxLength(50);
                entity.Property(pm => pm.Food).HasMaxLength(50);
                entity.Property(pm => pm.SleepQuality).HasMaxLength(50);
                entity.Property(pm => pm.RecentActivities).HasMaxLength(500);
                entity.Property(pm => pm.Sociability).HasMaxLength(50);
                entity.Property(pm => pm.Weight).HasPrecision(5, 2);
                entity.Property(pm => pm.Observations).HasMaxLength(1000);
                entity.Property(pm => pm.RegisteredAt).IsRequired();
                entity.HasOne(pm => pm.Pet).WithMany(p => p.PetMonitorings).HasForeignKey(pm => pm.PetId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<CareEvent>(entity =>
            {
                entity.ToTable("CARE_EVENTS");
                entity.HasKey(ce => ce.CareEventId);
                entity.Property(ce => ce.CareEventId).ValueGeneratedOnAdd();
                entity.Property(ce => ce.TypeEvent).IsRequired();
                entity.Property(ce => ce.Description).IsRequired().HasMaxLength(500);
                entity.Property(ce => ce.EventDate).IsRequired();
                entity.Property(ce => ce.Status).IsRequired();
                entity.Property(ce => ce.Observations).HasMaxLength(1000);
                entity.Property(ce => ce.CreatedAt).IsRequired();
                entity.HasOne(ce => ce.Pet).WithMany(p => p.CareEvents).HasForeignKey(ce => ce.PetId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<DailyPetLog>(entity =>
            {
                entity.ToTable("DAILY_PET_LOGS");
                entity.HasKey(dpl => dpl.DailyPetLogId);
                entity.Property(dpl => dpl.DailyPetLogId).ValueGeneratedOnAdd();
                entity.Property(dpl => dpl.CreatedByUserId).IsRequired();
                entity.Property(dpl => dpl.DailyPetLogType).IsRequired();
                entity.Property(dpl => dpl.Content).IsRequired().HasMaxLength(2000);
                entity.Property(dpl => dpl.Privacy).IsRequired();
                entity.Property(dpl => dpl.RegisteredAt).IsRequired();
                entity.HasOne(dpl => dpl.Pet).WithMany(p => p.DailyPetLogs).HasForeignKey(dpl => dpl.PetId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne<User>().WithMany().HasForeignKey(dpl => dpl.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<CommunityPost>(entity =>
            {
                entity.ToTable("COMMUNITY_POSTS");
                entity.HasKey(cp => cp.CommunityPostId);
                entity.Property(cp => cp.CommunityPostId).ValueGeneratedOnAdd();
                entity.Property(cp => cp.Category).IsRequired().HasMaxLength(100);
                entity.Property(cp => cp.Content).IsRequired().HasMaxLength(2000);
                entity.Property(cp => cp.ImageUrl).HasMaxLength(1000);
                entity.Property(cp => cp.Location).HasMaxLength(200);
                entity.Property(cp => cp.RegisteredAt).IsRequired();
                entity.HasOne(cp => cp.User).WithMany().HasForeignKey(cp => cp.UserId).OnDelete(DeleteBehavior.Cascade);

            });

           
        }

    }
}
