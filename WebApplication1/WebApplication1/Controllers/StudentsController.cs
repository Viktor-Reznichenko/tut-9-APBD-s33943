using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.DTOs;

namespace WebApplication1.Controllers;

[ApiController]
[Route("api/students")]
public class StudentsController : ControllerBase
{
    private readonly UniversityTasksDbContext _db;

    public StudentsController(UniversityTasksDbContext db)
    {
        _db = db;
    }

    [HttpGet("{idStudent}/dashboard")]
    public async Task<ActionResult<StudentDashboardDto>> GetDashboard(int idStudent)
    {
        var student = await _db.Students
            .Where(s => s.StudentId == idStudent)
            .Select(s => new StudentDashboardDto
            {
                StudentId = s.StudentId,
                IndexNumber = s.IndexNumber,
                FullName = s.FullName,
                IsActive = s.IsActive,

                Enrollments = s.Enrollments.Select(e => new EnrollmentDto
                {
                    EnrollmentId = e.EnrollmentId,
                    CourseId = e.CourseId,
                    CourseCode = e.Course.Code,
                    CourseName = e.Course.Name,
                    EnrolledAt = e.EnrolledAt,
                    Status = e.Status
                }).ToList(),

                Submissions = s.Submissions.Select(sub => new SubmissionDto
                {
                    SubmissionId = sub.SubmissionId,
                    RepositoryUrl = sub.RepositoryUrl,
                    Status = sub.Status,
                    Score = sub.Score,
                    Feedback = sub.Feedback,

                    Student = new SimpleStudentDto
                    {
                        StudentId = s.StudentId,
                        FullName = s.FullName
                    },

                    Assignment = new SimpleAssignmentDto
                    {
                        AssignmentId = sub.AssignmentId,
                        Title = sub.Assignment.Title
                    }
                }).ToList()
            })
            .FirstOrDefaultAsync();

        if (student == null)
            return NotFound();

        return Ok(student);
    }
}
