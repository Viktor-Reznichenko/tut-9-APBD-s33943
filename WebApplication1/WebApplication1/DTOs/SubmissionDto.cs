namespace WebApplication1.DTOs;

public class SubmissionDto
{
    public int SubmissionId { get; set; }

    public SimpleStudentDto Student { get; set; } = null!;
    public SimpleAssignmentDto Assignment { get; set; } = null!;

    public string RepositoryUrl { get; set; } = null!;
    public string Status { get; set; } = null!;
    public int? Score { get; set; }
    public string? Feedback { get; set; }
}