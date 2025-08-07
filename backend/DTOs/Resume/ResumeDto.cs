namespace backend.DTOs.Resume;

public class ResumeDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public List<string> Tags { get; set; } = new();
    public bool IsDefault { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int UsageCount { get; set; } // How many job applications use this resume
}

public class CreateResumeDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<string> Tags { get; set; } = new();
    public bool IsDefault { get; set; } = false;
    public Microsoft.AspNetCore.Http.IFormFile File { get; set; } = null!;
}

public class UpdateResumeDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public List<string>? Tags { get; set; }
    public bool? IsDefault { get; set; }
}

public class ResumeVersionDto
{
    public int Id { get; set; }
    public int ResumeId { get; set; }
    public string VersionName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string? Changes { get; set; }
    public string? JobDescription { get; set; }
    public string? AIPrompt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateResumeVersionDto
{
    public string VersionName { get; set; } = string.Empty;
    public string? Changes { get; set; }
    public string? JobDescription { get; set; }
    public string? AIPrompt { get; set; }
}
