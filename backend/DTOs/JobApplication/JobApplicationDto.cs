using backend.Models;

namespace backend.DTOs.JobApplication;

public class JobApplicationDto
{
    public int Id { get; set; }
    public string JobTitle { get; set; } = string.Empty;
    public int CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string? JobUrl { get; set; }
    public ApplicationStatus Status { get; set; }
    public DateTime DateApplied { get; set; }
    public string? Source { get; set; }
    public List<string> Tags { get; set; } = new();
    public string? ContactPersonName { get; set; }
    public string? ContactPersonEmail { get; set; }
    public string? ContactPersonPhone { get; set; }
    public string? Notes { get; set; }
    public int? ResumeId { get; set; }
    public string? ResumeTitle { get; set; }
    public List<string> AttachmentPaths { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateJobApplicationDto
{
    public string JobTitle { get; set; } = string.Empty;
    public int CompanyId { get; set; }
    public string? Location { get; set; }
    public string? JobUrl { get; set; }
    public ApplicationStatus Status { get; set; } = ApplicationStatus.Applied;
    public DateTime DateApplied { get; set; } = DateTime.UtcNow;
    public string? Source { get; set; }
    public List<string> Tags { get; set; } = new();
    public string? ContactPersonName { get; set; }
    public string? ContactPersonEmail { get; set; }
    public string? ContactPersonPhone { get; set; }
    public string? Notes { get; set; }
    public int? ResumeId { get; set; }
    public List<string> AttachmentPaths { get; set; } = new();
}

public class UpdateJobApplicationDto
{
    public string? JobTitle { get; set; }
    public int? CompanyId { get; set; }
    public string? Location { get; set; }
    public string? JobUrl { get; set; }
    public ApplicationStatus? Status { get; set; }
    public DateTime? DateApplied { get; set; }
    public string? Source { get; set; }
    public List<string>? Tags { get; set; }
    public string? ContactPersonName { get; set; }
    public string? ContactPersonEmail { get; set; }
    public string? ContactPersonPhone { get; set; }
    public string? Notes { get; set; }
    public int? ResumeId { get; set; }
    public List<string>? AttachmentPaths { get; set; }
}
