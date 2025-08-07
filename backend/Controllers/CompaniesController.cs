using System.Security.Claims;
using backend.Data;
using backend.DTOs.Company;
using backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CompaniesController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CompaniesController> _logger;

    public CompaniesController(ApplicationDbContext context, ILogger<CompaniesController> logger)
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
    public async Task<ActionResult<IEnumerable<CompanyDto>>> GetCompanies(
        [FromQuery] string? search = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20
    )
    {
        try
        {
            var userId = GetCurrentUserId();
            var query = _context
                .Companies.Include(c => c.JobApplications)
                .Where(c => c.UserId == userId);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(c =>
                    c.Name.Contains(search)
                    || (c.Industry != null && c.Industry.Contains(search))
                    || (c.Location != null && c.Location.Contains(search))
                );
            }

            var totalCount = await query.CountAsync();
            var companies = await query
                .OrderBy(c => c.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = companies
                .Select(c => new CompanyDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Location = c.Location,
                    Website = c.Website,
                    Industry = c.Industry,
                    Size = c.Size,
                    Description = c.Description,
                    Notes = c.Notes,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt,
                    JobApplicationsCount = c.JobApplications.Count,
                })
                .ToList();

            Response.Headers["X-Total-Count"] = totalCount.ToString();
            Response.Headers["X-Page"] = page.ToString();
            Response.Headers["X-Page-Size"] = pageSize.ToString();

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving companies");
            return StatusCode(
                500,
                new { message = "An error occurred while retrieving companies" }
            );
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CompanyDto>> GetCompany(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var company = await _context
                .Companies.Include(c => c.JobApplications)
                .Include(c => c.Contacts)
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (company == null)
            {
                return NotFound(new { message = "Company not found" });
            }

            var result = new CompanyDto
            {
                Id = company.Id,
                Name = company.Name,
                Location = company.Location,
                Website = company.Website,
                Industry = company.Industry,
                Size = company.Size,
                Description = company.Description,
                Notes = company.Notes,
                CreatedAt = company.CreatedAt,
                UpdatedAt = company.UpdatedAt,
                JobApplicationsCount = company.JobApplications.Count,
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving company {Id}", id);
            return StatusCode(
                500,
                new { message = "An error occurred while retrieving the company" }
            );
        }
    }

    [HttpPost]
    public async Task<ActionResult<CompanyDto>> CreateCompany(CreateCompanyDto createDto)
    {
        try
        {
            var userId = GetCurrentUserId();

            // Check if company with same name already exists for this user
            var existingCompany = await _context.Companies.FirstOrDefaultAsync(c =>
                c.Name == createDto.Name && c.UserId == userId
            );

            if (existingCompany != null)
            {
                return BadRequest(new { message = "A company with this name already exists" });
            }

            var company = new Company
            {
                Name = createDto.Name,
                Location = createDto.Location,
                Website = createDto.Website,
                Industry = createDto.Industry,
                Size = createDto.Size,
                Description = createDto.Description,
                Notes = createDto.Notes,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            _context.Companies.Add(company);
            await _context.SaveChangesAsync();

            var result = new CompanyDto
            {
                Id = company.Id,
                Name = company.Name,
                Location = company.Location,
                Website = company.Website,
                Industry = company.Industry,
                Size = company.Size,
                Description = company.Description,
                Notes = company.Notes,
                CreatedAt = company.CreatedAt,
                UpdatedAt = company.UpdatedAt,
                JobApplicationsCount = 0,
            };

            return CreatedAtAction(nameof(GetCompany), new { id = company.Id }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating company");
            return StatusCode(
                500,
                new { message = "An error occurred while creating the company" }
            );
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCompany(int id, UpdateCompanyDto updateDto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var company = await _context.Companies.FirstOrDefaultAsync(c =>
                c.Id == id && c.UserId == userId
            );

            if (company == null)
            {
                return NotFound(new { message = "Company not found" });
            }

            // Check if another company with the same name exists (if name is being updated)
            if (!string.IsNullOrEmpty(updateDto.Name) && updateDto.Name != company.Name)
            {
                var existingCompany = await _context.Companies.FirstOrDefaultAsync(c =>
                    c.Name == updateDto.Name && c.UserId == userId && c.Id != id
                );

                if (existingCompany != null)
                {
                    return BadRequest(new { message = "A company with this name already exists" });
                }
                company.Name = updateDto.Name;
            }

            if (updateDto.Location != null)
                company.Location = updateDto.Location;

            if (updateDto.Website != null)
                company.Website = updateDto.Website;

            if (updateDto.Industry != null)
                company.Industry = updateDto.Industry;

            if (updateDto.Size != null)
                company.Size = updateDto.Size;

            if (updateDto.Description != null)
                company.Description = updateDto.Description;

            if (updateDto.Notes != null)
                company.Notes = updateDto.Notes;

            company.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating company {Id}", id);
            return StatusCode(
                500,
                new { message = "An error occurred while updating the company" }
            );
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCompany(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var company = await _context
                .Companies.Include(c => c.JobApplications)
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (company == null)
            {
                return NotFound(new { message = "Company not found" });
            }

            // Check if company has associated job applications
            if (company.JobApplications.Any())
            {
                return BadRequest(
                    new { message = "Cannot delete company with associated job applications" }
                );
            }

            _context.Companies.Remove(company);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting company {Id}", id);
            return StatusCode(
                500,
                new { message = "An error occurred while deleting the company" }
            );
        }
    }

    [HttpGet("{id}/contacts")]
    public async Task<ActionResult<IEnumerable<CompanyContactDto>>> GetCompanyContacts(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var company = await _context.Companies.FirstOrDefaultAsync(c =>
                c.Id == id && c.UserId == userId
            );

            if (company == null)
            {
                return NotFound(new { message = "Company not found" });
            }

            var contacts = await _context
                .CompanyContacts.Where(cc => cc.CompanyId == id)
                .OrderBy(cc => cc.Name)
                .ToListAsync();

            var result = contacts
                .Select(cc => new CompanyContactDto
                {
                    Id = cc.Id,
                    CompanyId = cc.CompanyId,
                    Name = cc.Name,
                    Position = cc.Position,
                    Email = cc.Email,
                    Phone = cc.Phone,
                    LinkedIn = cc.LinkedIn,
                    Notes = cc.Notes,
                    CreatedAt = cc.CreatedAt,
                })
                .ToList();

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving company contacts for company {Id}", id);
            return StatusCode(
                500,
                new { message = "An error occurred while retrieving company contacts" }
            );
        }
    }

    [HttpPost("{id}/contacts")]
    public async Task<ActionResult<CompanyContactDto>> CreateCompanyContact(
        int id,
        CreateCompanyContactDto createDto
    )
    {
        try
        {
            var userId = GetCurrentUserId();
            var company = await _context.Companies.FirstOrDefaultAsync(c =>
                c.Id == id && c.UserId == userId
            );

            if (company == null)
            {
                return NotFound(new { message = "Company not found" });
            }

            var contact = new CompanyContact
            {
                CompanyId = id,
                Name = createDto.Name,
                Position = createDto.Position,
                Email = createDto.Email,
                Phone = createDto.Phone,
                LinkedIn = createDto.LinkedIn,
                Notes = createDto.Notes,
                CreatedAt = DateTime.UtcNow,
            };

            _context.CompanyContacts.Add(contact);
            await _context.SaveChangesAsync();

            var result = new CompanyContactDto
            {
                Id = contact.Id,
                CompanyId = contact.CompanyId,
                Name = contact.Name,
                Position = contact.Position,
                Email = contact.Email,
                Phone = contact.Phone,
                LinkedIn = contact.LinkedIn,
                Notes = contact.Notes,
                CreatedAt = contact.CreatedAt,
            };

            return CreatedAtAction(nameof(GetCompanyContacts), new { id = id }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating company contact for company {Id}", id);
            return StatusCode(
                500,
                new { message = "An error occurred while creating the company contact" }
            );
        }
    }

    [HttpDelete("{companyId}/contacts/{contactId}")]
    public async Task<IActionResult> DeleteCompanyContact(int companyId, int contactId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var company = await _context.Companies.FirstOrDefaultAsync(c =>
                c.Id == companyId && c.UserId == userId
            );

            if (company == null)
            {
                return NotFound(new { message = "Company not found" });
            }

            var contact = await _context.CompanyContacts.FirstOrDefaultAsync(cc =>
                cc.Id == contactId && cc.CompanyId == companyId
            );

            if (contact == null)
            {
                return NotFound(new { message = "Company contact not found" });
            }

            _context.CompanyContacts.Remove(contact);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error deleting company contact {ContactId} for company {CompanyId}",
                contactId,
                companyId
            );
            return StatusCode(
                500,
                new { message = "An error occurred while deleting the company contact" }
            );
        }
    }
}
