using backend.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace backend.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    // Domain entities
    public DbSet<Company> Companies { get; set; }
    public DbSet<CompanyContact> CompanyContacts { get; set; }
    public DbSet<JobApplication> JobApplications { get; set; }
    public DbSet<Resume> Resumes { get; set; }
    public DbSet<ResumeVersion> ResumeVersions { get; set; }
    public DbSet<Feedback> Feedbacks { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure JobApplication relationships
        builder
            .Entity<JobApplication>()
            .HasOne(ja => ja.Company)
            .WithMany(c => c.JobApplications)
            .HasForeignKey(ja => ja.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .Entity<JobApplication>()
            .HasOne(ja => ja.Resume)
            .WithMany(r => r.JobApplications)
            .HasForeignKey(ja => ja.ResumeId)
            .OnDelete(DeleteBehavior.SetNull);

        builder
            .Entity<JobApplication>()
            .HasOne(ja => ja.User)
            .WithMany()
            .HasForeignKey(ja => ja.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure Company relationships
        builder
            .Entity<Company>()
            .HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure CompanyContact relationships
        builder
            .Entity<CompanyContact>()
            .HasOne(cc => cc.Company)
            .WithMany(c => c.Contacts)
            .HasForeignKey(cc => cc.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure Resume relationships
        builder
            .Entity<Resume>()
            .HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure ResumeVersion relationships
        builder
            .Entity<ResumeVersion>()
            .HasOne(rv => rv.Resume)
            .WithMany(r => r.Versions)
            .HasForeignKey(rv => rv.ResumeId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure Feedback relationships
        builder
            .Entity<Feedback>()
            .HasOne(f => f.JobApplication)
            .WithMany(ja => ja.Feedbacks)
            .HasForeignKey(f => f.JobApplicationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .Entity<Feedback>()
            .HasOne(f => f.User)
            .WithMany()
            .HasForeignKey(f => f.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure AuditLog relationships
        builder
            .Entity<AuditLog>()
            .HasOne(al => al.User)
            .WithMany()
            .HasForeignKey(al => al.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .Entity<AuditLog>()
            .HasOne(al => al.JobApplication)
            .WithMany(ja => ja.AuditLogs)
            .HasForeignKey(al => al.JobApplicationId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure indexes for better performance
        builder.Entity<JobApplication>().HasIndex(ja => ja.UserId);

        builder.Entity<JobApplication>().HasIndex(ja => ja.Status);

        builder.Entity<JobApplication>().HasIndex(ja => ja.DateApplied);

        builder.Entity<Company>().HasIndex(c => c.UserId);

        builder.Entity<Resume>().HasIndex(r => r.UserId);

        builder.Entity<Feedback>().HasIndex(f => f.JobApplicationId);

        builder.Entity<AuditLog>().HasIndex(al => new { al.EntityType, al.EntityId });
    }
}
