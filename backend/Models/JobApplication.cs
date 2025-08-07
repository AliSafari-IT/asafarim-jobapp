using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models;

public class JobApplication
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string JobTitle { get; set; } = string.Empty;

    [Required]
    public int CompanyId { get; set; }

    [ForeignKey("CompanyId")]
    public virtual Company Company { get; set; } = null!;

    [MaxLength(100)]
    public string? Location { get; set; }

    [Url]
    public string? JobUrl { get; set; }

    [Required]
    public ApplicationStatus Status { get; set; } = ApplicationStatus.Applied;

    [Required]
    public DateTime DateApplied { get; set; } = DateTime.UtcNow;

    [MaxLength(100)]
    public string? Source { get; set; } // LinkedIn, referral, etc.

    public string? Tags { get; set; } // JSON array of tags

    [MaxLength(100)]
    public string? ContactPersonName { get; set; }

    [EmailAddress]
    public string? ContactPersonEmail { get; set; }

    [Phone]
    public string? ContactPersonPhone { get; set; }

    public string? Notes { get; set; }

    public int? ResumeId { get; set; }

    [ForeignKey("ResumeId")]
    public virtual Resume? Resume { get; set; }

    public string? AttachmentPaths { get; set; } // JSON array of file paths

    [Required]
    public string UserId { get; set; } = string.Empty;

    [ForeignKey("UserId")]
    public virtual ApplicationUser User { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();
    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
}

public enum ApplicationStatus
{
    Applied,
    Interviewing,
    Offer,
    Rejected,
    Accepted,
    Withdrawn
}
