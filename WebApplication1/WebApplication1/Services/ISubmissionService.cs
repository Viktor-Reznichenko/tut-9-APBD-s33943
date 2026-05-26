using WebApplication1.DTOs;
using WebApplication1.Models;

namespace WebApplication1.Services;

public interface ISubmissionService
{
    Task<Submission> CreateSubmissionAsync(CreateSubmissionDto dto);
    Task GradeSubmissionAsync(int submissionId, GradeSubmissionDto dto);
    Task DeleteSubmissionAsync(int submissionId);
}