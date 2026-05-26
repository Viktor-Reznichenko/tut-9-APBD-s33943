using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.DTOs;
using WebApplication1.Services;

namespace WebApplication1.Controllers;

[ApiController]
[Route("api/submissions")]
public class SubmissionsController : ControllerBase
{
    private readonly UniversityTasksDbContext _db;
    private readonly ISubmissionService _service;

    public SubmissionsController(UniversityTasksDbContext db, ISubmissionService service)
    {
        _db = db;
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> CreateSubmission(CreateSubmissionDto dto)
    {
        try
        {
            var submission = await _service.CreateSubmissionAsync(dto);
            return CreatedAtAction(nameof(GetSubmission), new { idSubmission = submission.SubmissionId }, null);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{idSubmission}")]
    public async Task<ActionResult<SubmissionDto>> GetSubmission(int idSubmission)
    {
        var sub = await _db.Submissions
            .Where(s => s.SubmissionId == idSubmission)
            .Select(s => new SubmissionDto
            {
                SubmissionId = s.SubmissionId,
                RepositoryUrl = s.RepositoryUrl,
                Status = s.Status,
                Score = s.Score,
                Feedback = s.Feedback,
                Student = new SimpleStudentDto
                {
                    StudentId = s.StudentId,
                    FullName = s.Student.FullName
                },
                Assignment = new SimpleAssignmentDto
                {
                    AssignmentId = s.AssignmentId,
                    Title = s.Assignment.Title
                }
            })
            .FirstOrDefaultAsync();

        if (sub == null)
            return NotFound();

        return Ok(sub);
    }

    [HttpPut("{idSubmission}/grade")]
    public async Task<IActionResult> GradeSubmission(int idSubmission, GradeSubmissionDto dto)
    {
        try
        {
            await _service.GradeSubmissionAsync(idSubmission, dto);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{idSubmission}")]
    public async Task<IActionResult> DeleteSubmission(int idSubmission)
    {
        try
        {
            await _service.DeleteSubmissionAsync(idSubmission);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
