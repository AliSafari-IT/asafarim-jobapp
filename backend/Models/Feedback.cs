using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models;

public class Feedback
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int JobApplicationId { get; set; }

    [ForeignKey("JobApplicationId")]
    public virtual JobApplication JobApplication { get; set; } = null!;

    [Required]
    public FeedbackType Type { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Content { get; set; } = string.Empty;

    public DateTime? ScheduledFollowUpDate { get; set; }

    public bool IsFollowUpCompleted { get; set; } = false;

    [MaxLength(100)]
    public string? InterviewerName { get; set; }

    [MaxLength(100)]
    public string? InterviewType { get; set; } // Phone, Video, In-person, etc.

    public int? Rating { get; set; } // 1-5 rating for interview performance

    public string? AttachmentPaths { get; set; } // JSON array of file paths

    [Required]
    public string UserId { get; set; } = string.Empty;

    [ForeignKey("UserId")]
    public virtual ApplicationUser User { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public enum FeedbackType
{
    General,
    Interview,
    PhoneScreen,
    TechnicalInterview,
    OnSite,
    Rejection,
    Offer,
    FollowUp,
    Reference
}
