using System.Security.Claims;
using System.Text.Json;
using backend.Data;
using backend.DTOs.Resume;
using backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ResumesController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ResumesController> _logger;
    private readonly IWebHostEnvironment _environment;

    public ResumesController(
        ApplicationDbContext context,
        ILogger<ResumesController> logger,
        IWebHostEnvironment environment
    )
    {
        _context = context;
        _logger = logger;
        _environment = environment;
    }

    private string GetCurrentUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException();
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ResumeDto>>> GetResumes(
        [FromQuery] string? search = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20
    )
    {
        try
        {
            var userId = GetCurrentUserId();
            var query = _context
                .Resumes.Include(r => r.JobApplications)
                .Where(r => r.UserId == userId);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(r =>
                    r.Title.Contains(search)
                    || (r.Description != null && r.Description.Contains(search))
                    || (r.Tags != null && r.Tags.Contains(search))
                );
            }

            var totalCount = await query.CountAsync();
            var resumes = await query
                .OrderByDescending(r => r.IsDefault)
                .ThenByDescending(r => r.UpdatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = resumes
                .Select(r => new ResumeDto
                {
                    Id = r.Id,
                    Title = r.Title,
                    Description = r.Description,
                    FilePath = r.FilePath,
                    FileType = r.FileType,
                    FileSizeBytes = r.FileSizeBytes,
                    Tags = string.IsNullOrEmpty(r.Tags)
                        ? new List<string>()
                        : JsonSerializer.Deserialize<List<string>>(r.Tags) ?? new List<string>(),
                    IsDefault = r.IsDefault,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt,
                    UsageCount = r.JobApplications.Count,
                })
                .ToList();

            Response.Headers["X-Total-Count"] = totalCount.ToString();
            Response.Headers["X-Page"] = page.ToString();
            Response.Headers["X-Page-Size"] = pageSize.ToString();

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving resumes");
            return StatusCode(500, new { message = "An error occurred while retrieving resumes" });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ResumeDto>> GetResume(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var resume = await _context
                .Resumes.Include(r => r.JobApplications)
                .Include(r => r.Versions)
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

            if (resume == null)
            {
                return NotFound(new { message = "Resume not found" });
            }

            var result = new ResumeDto
            {
                Id = resume.Id,
                Title = resume.Title,
                Description = resume.Description,
                FilePath = resume.FilePath,
                FileType = resume.FileType,
                FileSizeBytes = resume.FileSizeBytes,
                Tags = string.IsNullOrEmpty(resume.Tags)
                    ? new List<string>()
                    : JsonSerializer.Deserialize<List<string>>(resume.Tags) ?? new List<string>(),
                IsDefault = resume.IsDefault,
                CreatedAt = resume.CreatedAt,
                UpdatedAt = resume.UpdatedAt,
                UsageCount = resume.JobApplications.Count,
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving resume {Id}", id);
            return StatusCode(
                500,
                new { message = "An error occurred while retrieving the resume" }
            );
        }
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ResumeDto>> CreateResume(
        [FromForm] CreateResumeDto createDto
    )
    {
        try
        {
            var userId = GetCurrentUserId();

            if (createDto.File == null || createDto.File.Length == 0)
            {
                return BadRequest(new { message = "File is required" });
            }

            // Validate file type
            var allowedExtensions = new[] { ".pdf", ".docx", ".md" };
            var fileExtension = Path.GetExtension(createDto.File.FileName).ToLowerInvariant();

            if (!allowedExtensions.Any(ext => ext == fileExtension))
            {
                return BadRequest(new { message = "Only PDF, DOCX, and MD files are allowed" });
            }

            // Validate file size (max 10MB)
            if (createDto.File.Length > 10 * 1024 * 1024)
            {
                return BadRequest(new { message = "File size cannot exceed 10MB" });
            }

            // Create uploads directory if it doesn't exist
            var uploadsPath = Path.Combine(
                _environment.ContentRootPath,
                "uploads",
                "resumes",
                userId
            );
            Directory.CreateDirectory(uploadsPath);

            // Generate unique filename
            var fileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(uploadsPath, fileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await createDto.File.CopyToAsync(stream);
            }

            // If this is set as default, unset other default resumes
            if (createDto.IsDefault)
            {
                var existingDefaults = await _context
                    .Resumes.Where(r => r.UserId == userId && r.IsDefault)
                    .ToListAsync();

                foreach (var existingDefault in existingDefaults)
                {
                    existingDefault.IsDefault = false;
                }
            }

            var resume = new Resume
            {
                Title = createDto.Title,
                Description = createDto.Description,
                FilePath = filePath,
                FileType = fileExtension.Substring(1).ToUpperInvariant(),
                FileSizeBytes = createDto.File.Length,
                Tags = createDto.Tags.Any() ? JsonSerializer.Serialize(createDto.Tags) : null,
                IsDefault = createDto.IsDefault,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            _context.Resumes.Add(resume);
            await _context.SaveChangesAsync();

            var result = new ResumeDto
            {
                Id = resume.Id,
                Title = resume.Title,
                Description = resume.Description,
                FilePath = resume.FilePath,
                FileType = resume.FileType,
                FileSizeBytes = resume.FileSizeBytes,
                Tags = createDto.Tags,
                IsDefault = resume.IsDefault,
                CreatedAt = resume.CreatedAt,
                UpdatedAt = resume.UpdatedAt,
                UsageCount = 0,
            };

            return CreatedAtAction(nameof(GetResume), new { id = resume.Id }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating resume");
            return StatusCode(500, new { message = "An error occurred while creating the resume" });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateResume(int id, UpdateResumeDto updateDto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var resume = await _context.Resumes.FirstOrDefaultAsync(r =>
                r.Id == id && r.UserId == userId
            );

            if (resume == null)
            {
                return NotFound(new { message = "Resume not found" });
            }

            // If setting as default, unset other default resumes
            if (updateDto.IsDefault == true && !resume.IsDefault)
            {
                var existingDefaults = await _context
                    .Resumes.Where(r => r.UserId == userId && r.IsDefault && r.Id != id)
                    .ToListAsync();

                foreach (var existingDefault in existingDefaults)
                {
                    existingDefault.IsDefault = false;
                }
            }

            if (!string.IsNullOrEmpty(updateDto.Title))
                resume.Title = updateDto.Title;

            if (updateDto.Description != null)
                resume.Description = updateDto.Description;

            if (updateDto.Tags != null)
                resume.Tags = updateDto.Tags.Any()
                    ? JsonSerializer.Serialize(updateDto.Tags)
                    : null;

            if (updateDto.IsDefault.HasValue)
                resume.IsDefault = updateDto.IsDefault.Value;

            resume.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating resume {Id}", id);
            return StatusCode(500, new { message = "An error occurred while updating the resume" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteResume(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var resume = await _context
                .Resumes.Include(r => r.JobApplications)
                .Include(r => r.Versions)
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

            if (resume == null)
            {
                return NotFound(new { message = "Resume not found" });
            }

            // Check if resume is being used by job applications
            if (resume.JobApplications.Any())
            {
                return BadRequest(
                    new { message = "Cannot delete resume that is being used by job applications" }
                );
            }

            // Delete physical file
            if (System.IO.File.Exists(resume.FilePath))
            {
                System.IO.File.Delete(resume.FilePath);
            }

            // Delete version files
            foreach (var version in resume.Versions)
            {
                if (System.IO.File.Exists(version.FilePath))
                {
                    System.IO.File.Delete(version.FilePath);
                }
            }

            _context.Resumes.Remove(resume);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting resume {Id}", id);
            return StatusCode(500, new { message = "An error occurred while deleting the resume" });
        }
    }

    [HttpGet("{id}/download")]
    public async Task<IActionResult> DownloadResume(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var resume = await _context.Resumes.FirstOrDefaultAsync(r =>
                r.Id == id && r.UserId == userId
            );

            if (resume == null)
            {
                return NotFound(new { message = "Resume not found" });
            }

            if (!System.IO.File.Exists(resume.FilePath))
            {
                return NotFound(new { message = "Resume file not found" });
            }

            var fileBytes = await System.IO.File.ReadAllBytesAsync(resume.FilePath);
            var fileName = $"{resume.Title}.{resume.FileType.ToLowerInvariant()}";

            var contentType = resume.FileType.ToLowerInvariant() switch
            {
                "pdf" => "application/pdf",
                "docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                "md" => "text/markdown",
                _ => "application/octet-stream",
            };

            return File(fileBytes, contentType, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading resume {Id}", id);
            return StatusCode(
                500,
                new { message = "An error occurred while downloading the resume" }
            );
        }
    }

    [HttpGet("{id}/versions")]
    public async Task<ActionResult<IEnumerable<ResumeVersionDto>>> GetResumeVersions(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var resume = await _context.Resumes.FirstOrDefaultAsync(r =>
                r.Id == id && r.UserId == userId
            );

            if (resume == null)
            {
                return NotFound(new { message = "Resume not found" });
            }

            var versions = await _context
                .ResumeVersions.Where(rv => rv.ResumeId == id)
                .OrderByDescending(rv => rv.CreatedAt)
                .ToListAsync();

            var result = versions
                .Select(rv => new ResumeVersionDto
                {
                    Id = rv.Id,
                    ResumeId = rv.ResumeId,
                    VersionName = rv.VersionName,
                    FilePath = rv.FilePath,
                    Changes = rv.Changes,
                    JobDescription = rv.JobDescription,
                    AIPrompt = rv.AIPrompt,
                    CreatedAt = rv.CreatedAt,
                })
                .ToList();

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving resume versions for resume {Id}", id);
            return StatusCode(
                500,
                new { message = "An error occurred while retrieving resume versions" }
            );
        }
    }

    [HttpPost("{id}/versions")]
    public async Task<ActionResult<ResumeVersionDto>> CreateResumeVersion(
        int id,
        [FromForm] CreateResumeVersionDto createDto,
        [FromForm] IFormFile file
    )
    {
        try
        {
            var userId = GetCurrentUserId();
            var resume = await _context.Resumes.FirstOrDefaultAsync(r =>
                r.Id == id && r.UserId == userId
            );

            if (resume == null)
            {
                return NotFound(new { message = "Resume not found" });
            }

            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "File is required" });
            }

            // Validate file type matches original resume
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var expectedExtension = $".{resume.FileType.ToLowerInvariant()}";

            if (fileExtension != expectedExtension)
            {
                return BadRequest(
                    new
                    {
                        message = $"File type must match original resume type ({resume.FileType})",
                    }
                );
            }

            // Create versions directory
            var uploadsPath = Path.Combine(
                _environment.ContentRootPath,
                "uploads",
                "resumes",
                userId,
                "versions"
            );
            Directory.CreateDirectory(uploadsPath);

            // Generate unique filename
            var fileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(uploadsPath, fileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var version = new ResumeVersion
            {
                ResumeId = id,
                VersionName = createDto.VersionName,
                FilePath = filePath,
                Changes = createDto.Changes,
                JobDescription = createDto.JobDescription,
                AIPrompt = createDto.AIPrompt,
                CreatedAt = DateTime.UtcNow,
            };

            _context.ResumeVersions.Add(version);
            await _context.SaveChangesAsync();

            var result = new ResumeVersionDto
            {
                Id = version.Id,
                ResumeId = version.ResumeId,
                VersionName = version.VersionName,
                FilePath = version.FilePath,
                Changes = version.Changes,
                JobDescription = version.JobDescription,
                AIPrompt = version.AIPrompt,
                CreatedAt = version.CreatedAt,
            };

            return CreatedAtAction(nameof(GetResumeVersions), new { id = id }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating resume version for resume {Id}", id);
            return StatusCode(
                500,
                new { message = "An error occurred while creating the resume version" }
            );
        }
    }

    [HttpGet("{resumeId}/versions/{versionId}/download")]
    public async Task<IActionResult> DownloadResumeVersion(int resumeId, int versionId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var resume = await _context.Resumes.FirstOrDefaultAsync(r =>
                r.Id == resumeId && r.UserId == userId
            );

            if (resume == null)
            {
                return NotFound(new { message = "Resume not found" });
            }

            var version = await _context.ResumeVersions.FirstOrDefaultAsync(rv =>
                rv.Id == versionId && rv.ResumeId == resumeId
            );

            if (version == null)
            {
                return NotFound(new { message = "Resume version not found" });
            }

            if (!System.IO.File.Exists(version.FilePath))
            {
                return NotFound(new { message = "Resume version file not found" });
            }

            var fileBytes = await System.IO.File.ReadAllBytesAsync(version.FilePath);
            var fileName =
                $"{resume.Title}_{version.VersionName}.{resume.FileType.ToLowerInvariant()}";

            var contentType = resume.FileType.ToLowerInvariant() switch
            {
                "pdf" => "application/pdf",
                "docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                "md" => "text/markdown",
                _ => "application/octet-stream",
            };

            return File(fileBytes, contentType, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error downloading resume version {VersionId} for resume {ResumeId}",
                versionId,
                resumeId
            );
            return StatusCode(
                500,
                new { message = "An error occurred while downloading the resume version" }
            );
        }
    }
}
