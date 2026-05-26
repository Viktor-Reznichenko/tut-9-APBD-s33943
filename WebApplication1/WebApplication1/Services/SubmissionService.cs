using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.DTOs;
using WebApplication1.Models;

namespace WebApplication1.Services;

public class SubmissionService : ISubmissionService
{
    private readonly UniversityTasksDbContext _db;

    public SubmissionService(UniversityTasksDbContext db)
    {
        _db = db;
    }

    public async Task<Submission> CreateSubmissionAsync(CreateSubmissionDto dto)
    {
        var student = await _db.Students
            .FirstOrDefaultAsync(s => s.StudentId == dto.StudentId);

        if (student == null)
            throw new InvalidOperationException("Student does not exist.");

        if (!student.IsActive)
            throw new InvalidOperationException("Student is not active.");

        var assignment = await _db.Assignments
            .Include(a => a.Course)
            .FirstOrDefaultAsync(a => a.AssignmentId == dto.AssignmentId);

        if (assignment == null)
            throw new InvalidOperationException("Assignment does not exist.");

        if (!assignment.IsPublished)
            throw new InvalidOperationException("Assignment is not published.");

        var enrolled = await _db.Enrollments.AnyAsync(e =>
            e.StudentId == dto.StudentId &&
            e.CourseId == assignment.CourseId &&
            (e.Status == "Active" || e.Status == "Completed"));

        if (!enrolled)
            throw new InvalidOperationException("Student is not enrolled in this course.");

        var exists = await _db.Submissions.AnyAsync(s =>
            s.StudentId == dto.StudentId &&
            s.AssignmentId == dto.AssignmentId);

        if (exists)
            throw new InvalidOperationException("Submission already exists.");

        if (string.IsNullOrWhiteSpace(dto.RepositoryUrl) ||
            !dto.RepositoryUrl.StartsWith("https://"))
            throw new InvalidOperationException("RepositoryUrl must start with https://");

        var status = DateTime.UtcNow > assignment.DueDate
            ? "Late"
            : "Submitted";

        var submission = new Submission
        {
            AssignmentId = dto.AssignmentId,
            StudentId = dto.StudentId,
            RepositoryUrl = dto.RepositoryUrl,
            SubmittedAt = DateTime.UtcNow,
            Status = status
        };

        _db.Submissions.Add(submission);
        await _db.SaveChangesAsync();

        return submission;
    }

    public async Task GradeSubmissionAsync(int submissionId, GradeSubmissionDto dto)
    {
        var submission = await _db.Submissions
            .Include(s => s.Assignment)
            .FirstOrDefaultAsync(s => s.SubmissionId == submissionId);

        if (submission == null)
            throw new InvalidOperationException("Submission does not exist.");

        if (dto.Score < 0)
            throw new InvalidOperationException("Score cannot be negative.");

        if (dto.Score > submission.Assignment.MaxPoints)
            throw new InvalidOperationException("Score exceeds maximum points.");

        submission.Score = dto.Score;
        submission.Feedback = dto.Feedback;
        submission.Status = "Graded";

        await _db.SaveChangesAsync();
    }

    public async Task DeleteSubmissionAsync(int submissionId)
    {
        var submission = await _db.Submissions
            .FirstOrDefaultAsync(s => s.SubmissionId == submissionId);

        if (submission == null)
            throw new InvalidOperationException("Submission does not exist.");

        if (submission.Status == "Graded")
            throw new InvalidOperationException("Cannot delete a graded submission.");

        _db.Submissions.Remove(submission);
        await _db.SaveChangesAsync();
    }
}
