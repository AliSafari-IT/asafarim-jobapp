using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models;

public class Resume
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    [MaxLength(500)]
    public string FilePath { get; set; } = string.Empty;

    [Required]
    [MaxLength(10)]
    public string FileType { get; set; } = string.Empty; // PDF, DOCX, MD

    [Required]
    public long FileSizeBytes { get; set; }

    public string? Tags { get; set; } // JSON array of tags

    public bool IsDefault { get; set; } = false;

    [Required]
    public string UserId { get; set; } = string.Empty;

    [ForeignKey("UserId")]
    public virtual ApplicationUser User { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual ICollection<JobApplication> JobApplications { get; set; } = new List<JobApplication>();
    public virtual ICollection<ResumeVersion> Versions { get; set; } = new List<ResumeVersion>();
}

public class ResumeVersion
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int ResumeId { get; set; }

    [ForeignKey("ResumeId")]
    public virtual Resume Resume { get; set; } = null!;

    [Required]
    [MaxLength(100)]
    public string VersionName { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string FilePath { get; set; } = string.Empty;

    public string? Changes { get; set; } // Description of changes made

    public string? JobDescription { get; set; } // Original job description used for customization

    public string? AIPrompt { get; set; } // AI prompt used for generation

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
