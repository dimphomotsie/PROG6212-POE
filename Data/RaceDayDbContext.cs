using Microsoft.EntityFrameworkCore;
using RaceDay_API.Models.Entities;


namespace RaceDay_API.Data
{
    public class RaceDayDbContext : DbContext
    {
        public RaceDayDbContext(DbContextOptions<RaceDayDbContext> options)
        : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Organiser> Organisers { get; set; }
        public DbSet<Participant> Participants { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Enrolment> Enrolments { get; set; }
        public DbSet<Result> Results { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Explicit configuration of primary keys.
            modelBuilder.Entity<User>().HasKey(u => u.UserId);
            modelBuilder.Entity<Organiser>().HasKey(o => o.OrganiserId);
            modelBuilder.Entity<Participant>().HasKey(p => p.ParticipantId);
            modelBuilder.Entity<Event>().HasKey(e => e.EventId);
            modelBuilder.Entity<Category>().HasKey(c => c.CategoryId);
            modelBuilder.Entity<Enrolment>().HasKey(e => e.EnrolmentId);
            modelBuilder.Entity<Result>().HasKey(r => r.ResultId);

            // User: unique email
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // User -> Organiser (1..1)
            modelBuilder.Entity<Organiser>()
                .HasOne(o => o.User)
                .WithOne(u => u.Organiser)
                .HasForeignKey<Organiser>(o => o.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // User -> Participant (1..1)
            modelBuilder.Entity<Participant>()
                .HasOne(p => p.User)
                .WithOne(u => u.Participant)
                .HasForeignKey<Participant>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            // Organiser -> Events (1..*)
            modelBuilder.Entity<Event>()
                .HasOne(e => e.Organiser)
                .WithMany(o => o.Events)
                .HasForeignKey(e => e.OrganiserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Event -> Categories (1.*)
            modelBuilder.Entity<Category>()
                .HasOne(c => c.Event)
                .WithMany(e => e.Categories)
                .HasForeignKey(c => c.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            // Enrolment: unique (ParticipantId, EventId)
            modelBuilder.Entity<Enrolment>()
                .HasIndex(e => new { e.ParticipantId, e.EventId })
                .IsUnique();

            // Enrolment -> Participant
            modelBuilder.Entity<Enrolment>()
                .HasOne(e => e.Participant)
                .WithMany(p => p.Enrolments)
                .HasForeignKey(e => e.ParticipantId)
                .OnDelete(DeleteBehavior.Cascade);

            // Enrolment -> Event
            modelBuilder.Entity<Enrolment>()
                .HasOne(e => e.Event)
                .WithMany(ev => ev.Enrolments)
                .HasForeignKey(e => e.EventId)
                .OnDelete(DeleteBehavior.NoAction);

            // Enrolment -> Category
            modelBuilder.Entity<Enrolment>()
                .HasOne(e => e.Category)
                .WithMany(c => c.Enrolments)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.NoAction);

            // Enrolment -> Result (0..1)
            modelBuilder.Entity<Result>()
                .HasOne(r => r.Enrolment)
                .WithOne(e => e.Result)
                .HasForeignKey<Result>(r => r.EnrolmentId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

/*
1. DeleteBehavior enum (Cascade, NoAction).
Author: Microsoft
Link: <https://learn.microsoft.com/en-us/ef/core/saving/cascade-delete>
Used in: Specifying cascade behavior for dependent entities (e.g., deleting a User removes their Organiser/Participant record).
Date Accessed: 18 October 2026

2. EF Core DbContext and DbSet<T> (Microsoft.EntityFrameworkCore 10.0.13).
Author: Microsoft
Link: <https://learn.microsoft.com/en-us/ef/core/dbcontext-configuration/>
Used in: Defining RaceDayDbContext and exposing each entity as a DbSet.
Date Accessed: 18 October 2026

3. IsUnique and HasIndex (Unique Constraints).
Author: Microsoft
Link: <https://learn.microsoft.com/en-us/ef/core/modeling/indexes>
Used in: Enforcing unique Email on User and unique (ParticipantId, EventId) on Enrolment.
Date Accessed: 18 October 2026

4. OnModelCreating and Fluent API (HasOne, WithOne, WithMany, HasForeignKey, HasIndex, OnDelete).
Author: Microsoft
Link: <https://learn.microsoft.com/en-us/ef/core/modeling/relationships>
Used in: Configuring one-to-one, one-to-many, and many-to-many relationships, unique constraints, and cascade delete rules that match the Part 1 ERD.
Date Accessed: 18 October 2026
*/