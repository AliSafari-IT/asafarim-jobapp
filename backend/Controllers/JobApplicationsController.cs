using System.Security.Claims;
using System.Text.Json;
using backend.Data;
using backend.DTOs.JobApplication;
using backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class JobApplicationsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<JobApplicationsController> _logger;

    public JobApplicationsController(
        ApplicationDbContext context,
        ILogger<JobApplicationsController> logger
    )
    {
        _context = context;
        _logger = logger;
    }

    private string GetCurrentUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException();
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<JobApplicationDto>>> GetJobApplications(
        [FromQuery] ApplicationStatus? status = null,
        [FromQuery] string? search = null,
        [FromQuery] string? tags = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20
    )
    {
        try
        {
            var userId = GetCurrentUserId();
            var query = _context
                .JobApplications.Include(ja => ja.Company)
                .Include(ja => ja.Resume)
                .Where(ja => ja.UserId == userId);

            // Apply filters
            if (status.HasValue)
            {
                query = query.Where(ja => ja.Status == status.Value);
            }

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(ja =>
                    ja.JobTitle.Contains(search)
                    || ja.Company.Name.Contains(search)
                    || (ja.Notes != null && ja.Notes.Contains(search))
                );
            }

            if (!string.IsNullOrEmpty(tags))
            {
                var tagList = tags.Split(',').Select(t => t.Trim()).ToList();
                query = query.Where(ja =>
                    ja.Tags != null && tagList.Any(tag => ja.Tags.Contains(tag))
                );
            }

            // Apply pagination
            var totalCount = await query.CountAsync();
            var applications = await query
                .OrderByDescending(ja => ja.DateApplied)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = applications
                .Select(ja => new JobApplicationDto
                {
                    Id = ja.Id,
                    JobTitle = ja.JobTitle,
                    CompanyId = ja.CompanyId,
                    CompanyName = ja.Company.Name,
                    Location = ja.Location,
                    JobUrl = ja.JobUrl,
                    Status = ja.Status,
                    DateApplied = ja.DateApplied,
                    Source = ja.Source,
                    Tags = string.IsNullOrEmpty(ja.Tags)
                        ? new List<string>()
                        : JsonSerializer.Deserialize<List<string>>(ja.Tags) ?? new List<string>(),
                    ContactPersonName = ja.ContactPersonName,
                    ContactPersonEmail = ja.ContactPersonEmail,
                    ContactPersonPhone = ja.ContactPersonPhone,
                    Notes = ja.Notes,
                    ResumeId = ja.ResumeId,
                    ResumeTitle = ja.Resume?.Title,
                    AttachmentPaths = string.IsNullOrEmpty(ja.AttachmentPaths)
                        ? new List<string>()
                        : JsonSerializer.Deserialize<List<string>>(ja.AttachmentPaths)
                            ?? new List<string>(),
                    CreatedAt = ja.CreatedAt,
                    UpdatedAt = ja.UpdatedAt,
                })
                .ToList();

            Response.Headers["X-Total-Count"] = totalCount.ToString();
            Response.Headers["X-Page"] = page.ToString();
            Response.Headers["X-Page-Size"] = pageSize.ToString();

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving job applications");
            return StatusCode(
                500,
                new { message = "An error occurred while retrieving job applications" }
            );
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<JobApplicationDto>> GetJobApplication(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var jobApplication = await _context
                .JobApplications.Include(ja => ja.Company)
                .Include(ja => ja.Resume)
                .Include(ja => ja.Feedbacks)
                .FirstOrDefaultAsync(ja => ja.Id == id && ja.UserId == userId);

            if (jobApplication == null)
            {
                return NotFound(new { message = "Job application not found" });
            }

            var result = new JobApplicationDto
            {
                Id = jobApplication.Id,
                JobTitle = jobApplication.JobTitle,
                CompanyId = jobApplication.CompanyId,
                CompanyName = jobApplication.Company.Name,
                Location = jobApplication.Location,
                JobUrl = jobApplication.JobUrl,
                Status = jobApplication.Status,
                DateApplied = jobApplication.DateApplied,
                Source = jobApplication.Source,
                Tags = string.IsNullOrEmpty(jobApplication.Tags)
                    ? new List<string>()
                    : JsonSerializer.Deserialize<List<string>>(jobApplication.Tags)
                        ?? new List<string>(),
                ContactPersonName = jobApplication.ContactPersonName,
                ContactPersonEmail = jobApplication.ContactPersonEmail,
                ContactPersonPhone = jobApplication.ContactPersonPhone,
                Notes = jobApplication.Notes,
                ResumeId = jobApplication.ResumeId,
                ResumeTitle = jobApplication.Resume?.Title,
                AttachmentPaths = string.IsNullOrEmpty(jobApplication.AttachmentPaths)
                    ? new List<string>()
                    : JsonSerializer.Deserialize<List<string>>(jobApplication.AttachmentPaths)
                        ?? new List<string>(),
                CreatedAt = jobApplication.CreatedAt,
                UpdatedAt = jobApplication.UpdatedAt,
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving job application {Id}", id);
            return StatusCode(
                500,
                new { message = "An error occurred while retrieving the job application" }
            );
        }
    }

    [HttpPost]
    public async Task<ActionResult<JobApplicationDto>> CreateJobApplication(
        CreateJobApplicationDto createDto
    )
    {
        try
        {
            var userId = GetCurrentUserId();

            // Validate company exists and belongs to user
            var company = await _context.Companies.FirstOrDefaultAsync(c =>
                c.Id == createDto.CompanyId && c.UserId == userId
            );
            if (company == null)
            {
                return BadRequest(new { message = "Company not found or access denied" });
            }

            // Validate resume exists and belongs to user (if provided)
            if (createDto.ResumeId.HasValue)
            {
                var resume = await _context.Resumes.FirstOrDefaultAsync(r =>
                    r.Id == createDto.ResumeId.Value && r.UserId == userId
                );
                if (resume == null)
                {
                    return BadRequest(new { message = "Resume not found or access denied" });
                }
            }

            var jobApplication = new JobApplication
            {
                JobTitle = createDto.JobTitle,
                CompanyId = createDto.CompanyId,
                Location = createDto.Location,
                JobUrl = createDto.JobUrl,
                Status = createDto.Status,
                DateApplied = createDto.DateApplied,
                Source = createDto.Source,
                Tags = createDto.Tags.Any() ? JsonSerializer.Serialize(createDto.Tags) : null,
                ContactPersonName = createDto.ContactPersonName,
                ContactPersonEmail = createDto.ContactPersonEmail,
                ContactPersonPhone = createDto.ContactPersonPhone,
                Notes = createDto.Notes,
                ResumeId = createDto.ResumeId,
                AttachmentPaths = createDto.AttachmentPaths.Any()
                    ? JsonSerializer.Serialize(createDto.AttachmentPaths)
                    : null,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            _context.JobApplications.Add(jobApplication);
            await _context.SaveChangesAsync();

            // Create audit log
            await CreateAuditLog(
                "JobApplication",
                jobApplication.Id,
                "Create",
                null,
                null,
                null,
                userId
            );

            // Reload with includes for response
            await _context.Entry(jobApplication).Reference(ja => ja.Company).LoadAsync();

            if (jobApplication.ResumeId.HasValue)
            {
                await _context.Entry(jobApplication).Reference(ja => ja.Resume).LoadAsync();
            }

            var result = new JobApplicationDto
            {
                Id = jobApplication.Id,
                JobTitle = jobApplication.JobTitle,
                CompanyId = jobApplication.CompanyId,
                CompanyName = jobApplication.Company.Name,
                Location = jobApplication.Location,
                JobUrl = jobApplication.JobUrl,
                Status = jobApplication.Status,
                DateApplied = jobApplication.DateApplied,
                Source = jobApplication.Source,
                Tags = createDto.Tags,
                ContactPersonName = jobApplication.ContactPersonName,
                ContactPersonEmail = jobApplication.ContactPersonEmail,
                ContactPersonPhone = jobApplication.ContactPersonPhone,
                Notes = jobApplication.Notes,
                ResumeId = jobApplication.ResumeId,
                ResumeTitle = jobApplication.Resume?.Title,
                AttachmentPaths = createDto.AttachmentPaths,
                CreatedAt = jobApplication.CreatedAt,
                UpdatedAt = jobApplication.UpdatedAt,
            };

            return CreatedAtAction(
                nameof(GetJobApplication),
                new { id = jobApplication.Id },
                result
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating job application");
            return StatusCode(
                500,
                new { message = "An error occurred while creating the job application" }
            );
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateJobApplication(int id, UpdateJobApplicationDto updateDto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var jobApplication = await _context.JobApplications.FirstOrDefaultAsync(ja =>
                ja.Id == id && ja.UserId == userId
            );

            if (jobApplication == null)
            {
                return NotFound(new { message = "Job application not found" });
            }

            // Store old values for audit log
            var oldValues = new Dictionary<string, object?>
            {
                ["JobTitle"] = jobApplication.JobTitle,
                ["Status"] = jobApplication.Status,
                ["Location"] = jobApplication.Location,
                ["Notes"] = jobApplication.Notes,
            };

            // Update fields if provided
            if (!string.IsNullOrEmpty(updateDto.JobTitle))
                jobApplication.JobTitle = updateDto.JobTitle;

            if (updateDto.CompanyId.HasValue)
            {
                // Validate company exists and belongs to user
                var company = await _context.Companies.FirstOrDefaultAsync(c =>
                    c.Id == updateDto.CompanyId.Value && c.UserId == userId
                );
                if (company == null)
                {
                    return BadRequest(new { message = "Company not found or access denied" });
                }
                jobApplication.CompanyId = updateDto.CompanyId.Value;
            }

            if (updateDto.Location != null)
                jobApplication.Location = updateDto.Location;

            if (updateDto.JobUrl != null)
                jobApplication.JobUrl = updateDto.JobUrl;

            if (updateDto.Status.HasValue)
                jobApplication.Status = updateDto.Status.Value;

            if (updateDto.DateApplied.HasValue)
                jobApplication.DateApplied = updateDto.DateApplied.Value;

            if (updateDto.Source != null)
                jobApplication.Source = updateDto.Source;

            if (updateDto.Tags != null)
                jobApplication.Tags = updateDto.Tags.Any()
                    ? JsonSerializer.Serialize(updateDto.Tags)
                    : null;

            if (updateDto.ContactPersonName != null)
                jobApplication.ContactPersonName = updateDto.ContactPersonName;

            if (updateDto.ContactPersonEmail != null)
                jobApplication.ContactPersonEmail = updateDto.ContactPersonEmail;

            if (updateDto.ContactPersonPhone != null)
                jobApplication.ContactPersonPhone = updateDto.ContactPersonPhone;

            if (updateDto.Notes != null)
                jobApplication.Notes = updateDto.Notes;

            if (updateDto.ResumeId.HasValue)
            {
                if (updateDto.ResumeId.Value > 0)
                {
                    // Validate resume exists and belongs to user
                    var resume = await _context.Resumes.FirstOrDefaultAsync(r =>
                        r.Id == updateDto.ResumeId.Value && r.UserId == userId
                    );
                    if (resume == null)
                    {
                        return BadRequest(new { message = "Resume not found or access denied" });
                    }
                }
                jobApplication.ResumeId =
                    updateDto.ResumeId.Value == 0 ? null : updateDto.ResumeId.Value;
            }

            if (updateDto.AttachmentPaths != null)
                jobApplication.AttachmentPaths = updateDto.AttachmentPaths.Any()
                    ? JsonSerializer.Serialize(updateDto.AttachmentPaths)
                    : null;

            jobApplication.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Create audit logs for changed fields
            var newValues = new Dictionary<string, object?>
            {
                ["JobTitle"] = jobApplication.JobTitle,
                ["Status"] = jobApplication.Status,
                ["Location"] = jobApplication.Location,
                ["Notes"] = jobApplication.Notes,
            };

            foreach (var kvp in oldValues)
            {
                if (!Equals(kvp.Value, newValues[kvp.Key]))
                {
                    await CreateAuditLog(
                        "JobApplication",
                        jobApplication.Id,
                        "Update",
                        kvp.Key,
                        kvp.Value?.ToString(),
                        newValues[kvp.Key]?.ToString(),
                        userId
                    );
                }
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating job application {Id}", id);
            return StatusCode(
                500,
                new { message = "An error occurred while updating the job application" }
            );
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteJobApplication(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var jobApplication = await _context.JobApplications.FirstOrDefaultAsync(ja =>
                ja.Id == id && ja.UserId == userId
            );

            if (jobApplication == null)
            {
                return NotFound(new { message = "Job application not found" });
            }

            _context.JobApplications.Remove(jobApplication);
            await _context.SaveChangesAsync();

            // Create audit log
            await CreateAuditLog(
                "JobApplication",
                id,
                "Delete",
                null,
                jobApplication.JobTitle,
                null,
                userId
            );

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting job application {Id}", id);
            return StatusCode(
                500,
                new { message = "An error occurred while deleting the job application" }
            );
        }
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<object>> GetDashboard()
    {
        try
        {
            var userId = GetCurrentUserId();
            var applications = await _context
                .JobApplications.Where(ja => ja.UserId == userId)
                .ToListAsync();

            var statusBreakdown = applications
                .GroupBy(ja => ja.Status)
                .ToDictionary(g => g.Key.ToString(), g => g.Count());

            var recentApplications = applications
                .OrderByDescending(ja => ja.DateApplied)
                .Take(5)
                .Select(ja => new
                {
                    ja.Id,
                    ja.JobTitle,
                    CompanyName = _context.Companies.First(c => c.Id == ja.CompanyId).Name,
                    ja.Status,
                    ja.DateApplied,
                })
                .ToList();

            var result = new
            {
                TotalApplications = applications.Count,
                StatusBreakdown = statusBreakdown,
                RecentApplications = recentApplications,
                ApplicationsThisMonth = applications.Count(ja =>
                    ja.DateApplied.Month == DateTime.UtcNow.Month
                    && ja.DateApplied.Year == DateTime.UtcNow.Year
                ),
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving dashboard data");
            return StatusCode(
                500,
                new { message = "An error occurred while retrieving dashboard data" }
            );
        }
    }

    private async Task CreateAuditLog(
        string entityType,
        int entityId,
        string action,
        string? propertyName,
        string? oldValue,
        string? newValue,
        string userId
    )
    {
        var auditLog = new AuditLog
        {
            EntityType = entityType,
            EntityId = entityId,
            Action = action,
            PropertyName = propertyName,
            OldValue = oldValue,
            NewValue = newValue,
            UserId = userId,
            JobApplicationId = entityType == "JobApplication" ? entityId : null,
            CreatedAt = DateTime.UtcNow,
        };

        _context.AuditLogs.Add(auditLog);
        await _context.SaveChangesAsync();
    }
}
