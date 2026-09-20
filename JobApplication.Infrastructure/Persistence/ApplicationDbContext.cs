using JobApplication.Domain.Entities;
using JobApplication.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace JobApplication.Infrastructure.Persistence
{
    public class ApplicationDbContext : IdentityDbContext<AppUser>
    {
        public DbSet<Job> Jobs { get; set; }
        public DbSet<Candidate> Candidates { get; set; }
        public DbSet<JobCandidateApplication> JobCandidateApplications { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder); // Required for Identity tables

            // Ensure the AppUserId on Candidate is not treated as a FK navigation
            // (it's a plain string referencing IdentityUser.Id)
            builder.Entity<Candidate>()
                .HasIndex(c => c.AppUserId)
                .IsUnique();

            builder.Entity<JobCandidateApplication>()
                .HasOne(a => a.Candidate)
                .WithMany()
                .HasForeignKey(a => a.CandidateId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<JobCandidateApplication>()
                .HasOne(a => a.Job)
                .WithMany()
                .HasForeignKey(a => a.JobId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
