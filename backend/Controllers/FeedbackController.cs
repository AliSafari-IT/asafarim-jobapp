using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;
using backend.Data;
using backend.Models;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FeedbackController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<FeedbackController> _logger;

    public FeedbackController(ApplicationDbContext context, ILogger<FeedbackController> logger)
    {
        _context = context;
        _logger = logger;
    }

    private string GetCurrentUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException();
    }

    [HttpGet("application/{jobApplicationId}")]
    public async Task<ActionResult<IEnumerable<object>>> GetFeedbackForApplication(int jobApplicationId)
    {
        try
        {
            var userId = GetCurrentUserId();
            
            // Verify the job application belongs to the user
            var jobApplication = await _context.JobApplications
                .FirstOrDefaultAsync(ja => ja.Id == jobApplicationId && ja.UserId == userId);

            if (jobApplication == null)
            {
                return NotFound(new { message = "Job application not found" });
            }

            var feedbacks = await _context.Feedbacks
                .Where(f => f.JobApplicationId == jobApplicationId)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();

            var result = feedbacks.Select(f => new
            {
                f.Id,
                f.JobApplicationId,
                f.Type,
                f.Title,
                f.Content,
                f.ScheduledFollowUpDate,
                f.IsFollowUpCompleted,
                f.InterviewerName,
                f.InterviewType,
                f.Rating,
                AttachmentPaths = string.IsNullOrEmpty(f.AttachmentPaths) ? new List<string>() : JsonSerializer.Deserialize<List<string>>(f.AttachmentPaths) ?? new List<string>(),
                f.CreatedAt,
                f.UpdatedAt
            }).ToList();

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving feedback for job application {JobApplicationId}", jobApplicationId);
            return StatusCode(500, new { message = "An error occurred while retrieving feedback" });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<object>> GetFeedback(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var feedback = await _context.Feedbacks
                .Include(f => f.JobApplication)
                .FirstOrDefaultAsync(f => f.Id == id && f.UserId == userId);

            if (feedback == null)
            {
                return NotFound(new { message = "Feedback not found" });
            }

            var result = new
            {
                feedback.Id,
                feedback.JobApplicationId,
                JobApplicationTitle = feedback.JobApplication.JobTitle,
                feedback.Type,
                feedback.Title,
                feedback.Content,
                feedback.ScheduledFollowUpDate,
                feedback.IsFollowUpCompleted,
                feedback.InterviewerName,
                feedback.InterviewType,
                feedback.Rating,
                AttachmentPaths = string.IsNullOrEmpty(feedback.AttachmentPaths) ? new List<string>() : JsonSerializer.Deserialize<List<string>>(feedback.AttachmentPaths) ?? new List<string>(),
                feedback.CreatedAt,
                feedback.UpdatedAt
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving feedback {Id}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving the feedback" });
        }
    }

    [HttpPost]
    public async Task<ActionResult<object>> CreateFeedback([FromBody] CreateFeedbackDto createDto)
    {
        try
        {
            var userId = GetCurrentUserId();

            // Verify the job application belongs to the user
            var jobApplication = await _context.JobApplications
                .FirstOrDefaultAsync(ja => ja.Id == createDto.JobApplicationId && ja.UserId == userId);

            if (jobApplication == null)
            {
                return BadRequest(new { message = "Job application not found or access denied" });
            }

            var feedback = new Feedback
            {
                JobApplicationId = createDto.JobApplicationId,
                Type = createDto.Type,
                Title = createDto.Title,
                Content = createDto.Content,
                ScheduledFollowUpDate = createDto.ScheduledFollowUpDate,
                IsFollowUpCompleted = false,
                InterviewerName = createDto.InterviewerName,
                InterviewType = createDto.InterviewType,
                Rating = createDto.Rating,
                AttachmentPaths = createDto.AttachmentPaths?.Any() == true ? JsonSerializer.Serialize(createDto.AttachmentPaths) : null,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Feedbacks.Add(feedback);
            await _context.SaveChangesAsync();

            var result = new
            {
                feedback.Id,
                feedback.JobApplicationId,
                feedback.Type,
                feedback.Title,
                feedback.Content,
                feedback.ScheduledFollowUpDate,
                feedback.IsFollowUpCompleted,
                feedback.InterviewerName,
                feedback.InterviewType,
                feedback.Rating,
                AttachmentPaths = createDto.AttachmentPaths ?? new List<string>(),
                feedback.CreatedAt,
                feedback.UpdatedAt
            };

            return CreatedAtAction(nameof(GetFeedback), new { id = feedback.Id }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating feedback");
            return StatusCode(500, new { message = "An error occurred while creating the feedback" });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateFeedback(int id, [FromBody] UpdateFeedbackDto updateDto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var feedback = await _context.Feedbacks
                .FirstOrDefaultAsync(f => f.Id == id && f.UserId == userId);

            if (feedback == null)
            {
                return NotFound(new { message = "Feedback not found" });
            }

            if (updateDto.Type.HasValue)
                feedback.Type = updateDto.Type.Value;
            
            if (!string.IsNullOrEmpty(updateDto.Title))
                feedback.Title = updateDto.Title;
            
            if (!string.IsNullOrEmpty(updateDto.Content))
                feedback.Content = updateDto.Content;
            
            if (updateDto.ScheduledFollowUpDate.HasValue)
                feedback.ScheduledFollowUpDate = updateDto.ScheduledFollowUpDate.Value;
            
            if (updateDto.IsFollowUpCompleted.HasValue)
                feedback.IsFollowUpCompleted = updateDto.IsFollowUpCompleted.Value;
            
            if (updateDto.InterviewerName != null)
                feedback.InterviewerName = updateDto.InterviewerName;
            
            if (updateDto.InterviewType != null)
                feedback.InterviewType = updateDto.InterviewType;
            
            if (updateDto.Rating.HasValue)
                feedback.Rating = updateDto.Rating.Value;
            
            if (updateDto.AttachmentPaths != null)
                feedback.AttachmentPaths = updateDto.AttachmentPaths.Any() ? JsonSerializer.Serialize(updateDto.AttachmentPaths) : null;

            feedback.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating feedback {Id}", id);
            return StatusCode(500, new { message = "An error occurred while updating the feedback" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteFeedback(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var feedback = await _context.Feedbacks
                .FirstOrDefaultAsync(f => f.Id == id && f.UserId == userId);

            if (feedback == null)
            {
                return NotFound(new { message = "Feedback not found" });
            }

            _context.Feedbacks.Remove(feedback);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting feedback {Id}", id);
            return StatusCode(500, new { message = "An error occurred while deleting the feedback" });
        }
    }

    [HttpGet("follow-ups")]
    public async Task<ActionResult<IEnumerable<object>>> GetPendingFollowUps()
    {
        try
        {
            var userId = GetCurrentUserId();
            var pendingFollowUps = await _context.Feedbacks
                .Include(f => f.JobApplication)
                .ThenInclude(ja => ja.Company)
                .Where(f => f.UserId == userId && 
                           f.ScheduledFollowUpDate.HasValue && 
                           !f.IsFollowUpCompleted &&
                           f.ScheduledFollowUpDate.Value <= DateTime.UtcNow.AddDays(7)) // Next 7 days
                .OrderBy(f => f.ScheduledFollowUpDate)
                .ToListAsync();

            var result = pendingFollowUps.Select(f => new
            {
                f.Id,
                f.Title,
                f.Type,
                f.ScheduledFollowUpDate,
                JobApplication = new
                {
                    f.JobApplication.Id,
                    f.JobApplication.JobTitle,
                    CompanyName = f.JobApplication.Company.Name,
                    f.JobApplication.Status
                }
            }).ToList();

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving pending follow-ups");
            return StatusCode(500, new { message = "An error occurred while retrieving pending follow-ups" });
        }
    }

    [HttpPost("{id}/complete-followup")]
    public async Task<IActionResult> CompleteFollowUp(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var feedback = await _context.Feedbacks
                .FirstOrDefaultAsync(f => f.Id == id && f.UserId == userId);

            if (feedback == null)
            {
                return NotFound(new { message = "Feedback not found" });
            }

            feedback.IsFollowUpCompleted = true;
            feedback.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing follow-up for feedback {Id}", id);
            return StatusCode(500, new { message = "An error occurred while completing the follow-up" });
        }
    }
}

// DTOs for Feedback
public class CreateFeedbackDto
{
    public int JobApplicationId { get; set; }
    public FeedbackType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime? ScheduledFollowUpDate { get; set; }
    public string? InterviewerName { get; set; }
    public string? InterviewType { get; set; }
    public int? Rating { get; set; }
    public List<string>? AttachmentPaths { get; set; }
}

public class UpdateFeedbackDto
{
    public FeedbackType? Type { get; set; }
    public string? Title { get; set; }
    public string? Content { get; set; }
    public DateTime? ScheduledFollowUpDate { get; set; }
    public bool? IsFollowUpCompleted { get; set; }
    public string? InterviewerName { get; set; }
    public string? InterviewType { get; set; }
    public int? Rating { get; set; }
    public List<string>? AttachmentPaths { get; set; }
}
