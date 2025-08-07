using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models;

public class AuditLog
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string EntityType { get; set; } = string.Empty; // JobApplication, Company, Resume, etc.

    [Required]
    public int EntityId { get; set; }

    [Required]
    [MaxLength(50)]
    public string Action { get; set; } = string.Empty; // Create, Update, Delete

    [MaxLength(100)]
    public string? PropertyName { get; set; } // Which property was changed

    public string? OldValue { get; set; }
    public string? NewValue { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [ForeignKey("UserId")]
    public virtual ApplicationUser User { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Optional foreign key relationships for easier querying
    public int? JobApplicationId { get; set; }
    [ForeignKey("JobApplicationId")]
    public virtual JobApplication? JobApplication { get; set; }
}
