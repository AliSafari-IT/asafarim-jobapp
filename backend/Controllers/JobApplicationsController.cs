using System.Globalization;
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

    private string GetCurrentUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException();

    private List<string> DeserializeList(string? json) =>
        string.IsNullOrWhiteSpace(json)
            ? new()
            : JsonSerializer.Deserialize<List<string>>(json) ?? new();

    #region CRUD

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

            if (status.HasValue)
                query = query.Where(ja => ja.Status == status.Value);

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(ja =>
                    ja.JobTitle.Contains(search)
                    || ja.Company.Name.Contains(search)
                    || (ja.Notes != null && ja.Notes.Contains(search))
                );

            if (!string.IsNullOrWhiteSpace(tags))
            {
                var tagList = tags.Split(',').Select(t => t.Trim()).ToList();
                query = query.Where(ja =>
                    ja.Tags != null && tagList.Any(tag => ja.Tags.Contains(tag))
                );
            }

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
                    Tags = DeserializeList(ja.Tags),
                    ContactPersonName = ja.ContactPersonName,
                    ContactPersonEmail = ja.ContactPersonEmail,
                    ContactPersonPhone = ja.ContactPersonPhone,
                    Notes = ja.Notes,
                    ResumeId = ja.ResumeId,
                    ResumeTitle = ja.Resume?.Title,
                    AttachmentPaths = DeserializeList(ja.AttachmentPaths),
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
            var ja = await _context
                .JobApplications.Include(j => j.Company)
                .Include(j => j.Resume)
                .FirstOrDefaultAsync(j => j.Id == id && j.UserId == userId);

            if (ja == null)
                return NotFound(new { message = "Job application not found" });

            var result = new JobApplicationDto
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
                Tags = DeserializeList(ja.Tags),
                ContactPersonName = ja.ContactPersonName,
                ContactPersonEmail = ja.ContactPersonEmail,
                ContactPersonPhone = ja.ContactPersonPhone,
                Notes = ja.Notes,
                ResumeId = ja.ResumeId,
                ResumeTitle = ja.Resume?.Title,
                AttachmentPaths = DeserializeList(ja.AttachmentPaths),
                CreatedAt = ja.CreatedAt,
                UpdatedAt = ja.UpdatedAt,
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

    [HttpPost("simple")]
    public async Task<ActionResult<JobApplicationDto>> CreateJobApplicationSimple([FromBody] JsonElement data)
    {
        try
        {
            var userId = GetCurrentUserId();
            
            // Extract properties from JsonElement
            string jobTitle = "";
            int companyId = 0;
            string location = "";
            string notes = "";
            DateTime dateApplied = DateTime.UtcNow;
            
            if (data.TryGetProperty("jobTitle", out var jobTitleElement))
            {
                jobTitle = jobTitleElement.GetString() ?? "";
            }
            
            if (data.TryGetProperty("companyId", out var companyIdElement))
            {
                companyId = companyIdElement.GetInt32();
            }
            
            if (data.TryGetProperty("location", out var locationElement))
            {
                location = locationElement.GetString() ?? "";
            }
            
            if (data.TryGetProperty("notes", out var notesElement))
            {
                notes = notesElement.GetString() ?? "";
            }
            
            if (data.TryGetProperty("dateApplied", out var dateElement))
            {
                DateTime.TryParse(dateElement.GetString(), out dateApplied);
            }
            
            // Simple validation
            if (string.IsNullOrWhiteSpace(jobTitle))
            {
                return BadRequest(new { message = "Job title is required" });
            }

            if (companyId <= 0)
            {
                return BadRequest(new { message = "Valid company ID is required" });
            }

            // Check for duplicate job application
            var existingApplication = await _context.JobApplications
                .FirstOrDefaultAsync(ja => 
                    ja.UserId == userId && 
                    ja.JobTitle.ToLower() == jobTitle.ToLower() && 
                    ja.CompanyId == companyId);

            if (existingApplication != null)
            {
                return BadRequest(new { 
                    message = $"You already have a job application for '{jobTitle}' at this company. Please use a different job title or update the existing application." 
                });
            }

            // Create job application
            var ja = new JobApplication
            {
                JobTitle = jobTitle,
                CompanyId = companyId,
                Location = location,
                JobUrl = null,
                Status = ApplicationStatus.Applied,
                DateApplied = dateApplied,
                Source = null,
                Tags = "[]",
                ContactPersonName = null,
                ContactPersonEmail = null,
                ContactPersonPhone = null,
                Notes = notes,
                ResumeId = null,
                AttachmentPaths = "[]",
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            _context.JobApplications.Add(ja);
            await _context.SaveChangesAsync();

            // Return success
            return Ok(new { 
                message = "Job application created successfully",
                id = ja.Id 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating job application");
            return BadRequest(new { message = "Failed to create job application: " + ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<JobApplicationDto>> CreateJobApplication(
        CreateJobApplicationDto dto
    )
    {
        _logger.LogInformation("Received DTO: {@Dto}", dto);

        try
        {
            // Manual validation to provide better error messages
            if (string.IsNullOrWhiteSpace(dto.JobTitle))
            {
                return BadRequest(new { message = "Job title is required" });
            }

            if (dto.CompanyId <= 0)
            {
                return BadRequest(new { message = "Valid company ID is required" });
            }

            var userId = GetCurrentUserId();

            // Check for duplicate job application
            var existingApplication = await _context.JobApplications
                .FirstOrDefaultAsync(ja => 
                    ja.UserId == userId && 
                    ja.JobTitle.ToLower() == dto.JobTitle.ToLower() && 
                    ja.CompanyId == dto.CompanyId);

            if (existingApplication != null)
            {
                return BadRequest(new { 
                    message = $"You already have a job application for '{dto.JobTitle}' at this company. Please use a different job title or update the existing application." 
                });
            }

            // Parse date string to DateTime
            DateTime parsedDate;
            if (string.IsNullOrWhiteSpace(dto.DateApplied))
            {
                parsedDate = DateTime.UtcNow;
            }
            else
            {
                if (!DateTime.TryParseExact(dto.DateApplied, "yyyy-MM-dd", null, DateTimeStyles.None, out parsedDate))
                {
                    return BadRequest(new { message = "Invalid date format. Use YYYY-MM-DD format." });
                }
            }

            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState)
                {
                    foreach (var subError in error.Value.Errors)
                    {
                        _logger.LogError("Model error on '{Field}': {Error}", error.Key, subError.ErrorMessage);
                    }
                }
                return BadRequest(new { message = "Validation failed", errors = ModelState });
            }

            var company = await _context.Companies.FirstOrDefaultAsync(c =>
                c.Id == dto.CompanyId && c.UserId == userId
            );
            if (company == null)
                return BadRequest(new { message = "Company not found or access denied" });

            if (dto.ResumeId.HasValue)
            {
                var resume = await _context.Resumes.FirstOrDefaultAsync(r =>
                    r.Id == dto.ResumeId && r.UserId == userId
                );
                if (resume == null)
                    return BadRequest(new { message = "Resume not found or access denied" });
            }

            var ja = new JobApplication
            {
                JobTitle = dto.JobTitle,
                CompanyId = dto.CompanyId,
                Location = dto.Location,
                JobUrl = dto.JobUrl,
                Status = dto.Status,
                DateApplied = parsedDate,
                Source = dto.Source,
                Tags = dto.Tags.Count != 0 ? JsonSerializer.Serialize(dto.Tags) : null,
                ContactPersonName = dto.ContactPersonName,
                ContactPersonEmail = dto.ContactPersonEmail,
                ContactPersonPhone = dto.ContactPersonPhone,
                Notes = dto.Notes,
                ResumeId = dto.ResumeId,
                AttachmentPaths =
                    dto.AttachmentPaths.Count != 0
                        ? JsonSerializer.Serialize(dto.AttachmentPaths)
                        : null,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            _context.JobApplications.Add(ja);
            await _context.SaveChangesAsync();

            await CreateAuditLog("JobApplication", ja.Id, "Create", null, null, null, userId);

            await _context.Entry(ja).Reference(j => j.Company).LoadAsync();
            if (ja.ResumeId.HasValue)
                await _context.Entry(ja).Reference(j => j.Resume).LoadAsync();

            var result = new JobApplicationDto
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
                Tags = dto.Tags,
                ContactPersonName = ja.ContactPersonName,
                ContactPersonEmail = ja.ContactPersonEmail,
                ContactPersonPhone = ja.ContactPersonPhone,
                Notes = ja.Notes,
                ResumeId = ja.ResumeId,
                ResumeTitle = ja.Resume?.Title,
                AttachmentPaths = dto.AttachmentPaths,
                CreatedAt = ja.CreatedAt,
                UpdatedAt = ja.UpdatedAt,
            };

            return CreatedAtAction(nameof(GetJobApplication), new { id = ja.Id }, result);
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
    public async Task<IActionResult> UpdateJobApplication(int id, UpdateJobApplicationDto dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var ja = await _context.JobApplications.FirstOrDefaultAsync(j =>
                j.Id == id && j.UserId == userId
            );

            if (ja == null)
                return NotFound(new { message = "Job application not found" });

            var oldValues = new Dictionary<string, object?>
            {
                ["JobTitle"] = ja.JobTitle,
                ["Status"] = ja.Status,
                ["Location"] = ja.Location,
                ["Notes"] = ja.Notes,
            };

            if (!string.IsNullOrWhiteSpace(dto.JobTitle))
                ja.JobTitle = dto.JobTitle;
            if (dto.Location != null)
                ja.Location = dto.Location;
            if (dto.JobUrl != null)
                ja.JobUrl = dto.JobUrl;
            if (dto.Status.HasValue)
                ja.Status = dto.Status.Value;
            if (dto.DateApplied.HasValue)
                ja.DateApplied = dto.DateApplied.Value;
            if (dto.Source != null)
                ja.Source = dto.Source;
            if (dto.Tags != null)
                ja.Tags = dto.Tags.Any() ? JsonSerializer.Serialize(dto.Tags) : null;
            if (dto.ContactPersonName != null)
                ja.ContactPersonName = dto.ContactPersonName;
            if (dto.ContactPersonEmail != null)
                ja.ContactPersonEmail = dto.ContactPersonEmail;
            if (dto.ContactPersonPhone != null)
                ja.ContactPersonPhone = dto.ContactPersonPhone;
            if (dto.Notes != null)
                ja.Notes = dto.Notes;
            if (dto.AttachmentPaths != null)
                ja.AttachmentPaths = dto.AttachmentPaths.Any()
                    ? JsonSerializer.Serialize(dto.AttachmentPaths)
                    : null;

            if (dto.CompanyId.HasValue)
            {
                var company = await _context.Companies.FirstOrDefaultAsync(c =>
                    c.Id == dto.CompanyId && c.UserId == userId
                );
                if (company == null)
                    return BadRequest(new { message = "Company not found or access denied" });

                ja.CompanyId = dto.CompanyId.Value;
            }

            if (dto.ResumeId.HasValue)
            {
                if (dto.ResumeId.Value > 0)
                {
                    var resume = await _context.Resumes.FirstOrDefaultAsync(r =>
                        r.Id == dto.ResumeId && r.UserId == userId
                    );
                    if (resume == null)
                        return BadRequest(new { message = "Resume not found or access denied" });
                }
                ja.ResumeId = dto.ResumeId == 0 ? null : dto.ResumeId;
            }

            ja.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            foreach (var key in oldValues.Keys)
            {
                if (!Equals(oldValues[key], typeof(JobApplication).GetProperty(key)?.GetValue(ja)))
                {
                    await CreateAuditLog(
                        "JobApplication",
                        ja.Id,
                        "Update",
                        key,
                        oldValues[key]?.ToString(),
                        typeof(JobApplication).GetProperty(key)?.GetValue(ja)?.ToString(),
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
            var ja = await _context.JobApplications.FirstOrDefaultAsync(j =>
                j.Id == id && j.UserId == userId
            );

            if (ja == null)
                return NotFound(new { message = "Job application not found" });

            _context.JobApplications.Remove(ja);
            await _context.SaveChangesAsync();

            await CreateAuditLog("JobApplication", id, "Delete", null, ja.JobTitle, null, userId);

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

    #endregion

    #region Dashboard

    [HttpGet("dashboard")]
    public async Task<ActionResult<object>> GetDashboard()
    {
        try
        {
            var userId = GetCurrentUserId();

            var applications = await _context
                .JobApplications.Where(j => j.UserId == userId)
                .Include(j => j.Company)
                .ToListAsync();

            var statusBreakdown = applications
                .GroupBy(j => j.Status)
                .ToDictionary(g => g.Key.ToString(), g => g.Count());

            var recent = applications
                .OrderByDescending(j => j.DateApplied)
                .Take(5)
                .Select(j => new
                {
                    j.Id,
                    j.JobTitle,
                    CompanyName = j.Company.Name,
                    j.Status,
                    j.DateApplied,
                })
                .ToList();

            var result = new
            {
                TotalApplications = applications.Count,
                StatusBreakdown = statusBreakdown,
                RecentApplications = recent,
                ApplicationsThisMonth = applications.Count(j =>
                    j.DateApplied.Month == DateTime.UtcNow.Month
                    && j.DateApplied.Year == DateTime.UtcNow.Year
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

    #endregion

    private async Task CreateAuditLog(
        string entityType,
        int entityId,
        string action,
        string? property,
        string? oldValue,
        string? newValue,
        string userId
    )
    {
        _context.AuditLogs.Add(
            new AuditLog
            {
                EntityType = entityType,
                EntityId = entityId,
                Action = action,
                PropertyName = property,
                OldValue = oldValue,
                NewValue = newValue,
                UserId = userId,
                JobApplicationId = entityType == "JobApplication" ? entityId : null,
                CreatedAt = DateTime.UtcNow,
            }
        );

        await _context.SaveChangesAsync();
    }
}
