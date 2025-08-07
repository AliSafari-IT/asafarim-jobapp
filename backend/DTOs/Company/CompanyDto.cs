namespace backend.DTOs.Company;

public class CompanyDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string? Website { get; set; }
    public string? Industry { get; set; }
    public string? Size { get; set; }
    public string? Description { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int JobApplicationsCount { get; set; }
}

public class CreateCompanyDto
{
    public string Name { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string? Website { get; set; }
    public string? Industry { get; set; }
    public string? Size { get; set; }
    public string? Description { get; set; }
    public string? Notes { get; set; }
}

public class UpdateCompanyDto
{
    public string? Name { get; set; }
    public string? Location { get; set; }
    public string? Website { get; set; }
    public string? Industry { get; set; }
    public string? Size { get; set; }
    public string? Description { get; set; }
    public string? Notes { get; set; }
}

public class CompanyContactDto
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Position { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? LinkedIn { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateCompanyContactDto
{
    public string Name { get; set; } = string.Empty;
    public string? Position { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? LinkedIn { get; set; }
    public string? Notes { get; set; }
}
