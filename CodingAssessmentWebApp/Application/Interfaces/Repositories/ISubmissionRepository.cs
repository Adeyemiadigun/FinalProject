using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Application.Dtos;
using Domain.Entitties;

namespace Application.Interfaces.Repositories
{
    public interface ISubmissionRepository : IBaseRepository<Submission>
    {
        Task<Submission> GetAsync(Guid id);
        Task<Submission> GetAsync(Expression<Func<Submission,bool>> exp);
        Task<Submission?> GetFullSubmissionWithRelationsAsync(Guid submissionId);
        Task<PaginationDto<Submission>> GetAllAsync(Guid assessmentId, PaginationRequest request);
        Task<ICollection<Submission?>> GetAllAsync(Expression<Func<Submission, bool>> exp);
        Task<PaginationDto<Submission>> GetStudentSubmissionsAsync(Guid studentId, PaginationRequest request);
        Task<ICollection<Submission>> GetSelectedIds(ICollection<Guid> ids);
        Task<List<Guid>> GetAllIdsAsync(Expression<Func<Submission, bool>> predicate);
    }
}