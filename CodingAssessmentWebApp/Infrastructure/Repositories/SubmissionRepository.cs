using System.Linq.Expressions;
using Application.Dtos;
using Application.Interfaces.Repositories;
using Domain.Entitties;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class SubmissionRepository(ClhAssessmentAppDpContext context) : BaseRepository<Submission>(context), ISubmissionRepository
    {
        public async Task<PaginationDto<Submission>> GetAllAsync(Guid assessmentId, PaginationRequest request)
        {
            var query = _context.Set<Submission>()
                .Include(x => x.AnswerSubmissions)
                .ThenInclude(x => x.Question)
                .Where(x => x.AssessmentId == assessmentId);
            var totalRecord = query.Count();
            var totalPages = (int)Math.Ceiling((double)totalRecord / request.PageSize);
            var result = await query.Skip((request.CurrentPage - 1) * request.PageSize).Take(request.PageSize)
                .ToListAsync();
            return new PaginationDto<Submission>
            {
                TotalItems = totalRecord,
                TotalPages = totalPages,
                Items = result,
                HasNextPage = totalPages / request.CurrentPage == 1 ? false : true,
                HasPreviousPage = request.CurrentPage - 1 == 0 ? false : true,
                CurrentPage = request.CurrentPage,
            };
        }

        public async Task<ICollection<Submission?>> GetAllAsync(Expression<Func<Submission, bool>> exp)
        {

            var result = await _context.Set<Submission>()
                .Include(x => x.AnswerSubmissions)
                .ThenInclude(x => x.Question)
                .Where(exp)
                .ToListAsync();

            return result;
        }

        public async Task<Submission?> GetAsync(Guid id)
        {
            return await _context.Set<Submission>()
                .Include(x => x.AnswerSubmissions)
                .ThenInclude(x => x.Question)
                .FirstOrDefaultAsync(x => x.Id == id);
        }
        public async Task<Submission?> GetFullSubmissionWithRelationsAsync(Guid submissionId)
        {
            return await _context.Submissions
                .Include(x => x.Assessment)
                .Include(x => x.Student)
                .Include(x => x.AnswerSubmissions)
                    .ThenInclude(x => x.Question)
                        .ThenInclude(q => q.Options)
                .Include(x => x.AnswerSubmissions)
                    .ThenInclude(x => x.Question)
                        .ThenInclude(q => q.Tests)
                .Include(x => x.AnswerSubmissions)
                    .ThenInclude(x => x.Question)
                        .ThenInclude(q => q.Answer)
                .FirstOrDefaultAsync(x => x.Id == submissionId);
        }

        public async Task<Submission?> GetAsync(Expression<Func<Submission, bool>> exp)
        {
            return await _context.Set<Submission>()
               .Include(x => x.AnswerSubmissions)
               .ThenInclude(x => x.Question)
                .Include(x => x.Assessment)
               .FirstOrDefaultAsync(exp);
        }

        public async Task<ICollection<Submission>> GetSelectedIds(ICollection<Guid> ids)
        {
            var res = _context.Set<Submission>()
                .Include(x => x.AnswerSubmissions)
                .ThenInclude(x => x.Question)
                .ThenInclude(x => x.Options)
                .Where(x => ids.Contains(x.Id));
            return await res.ToListAsync();
        }
        public async Task<PaginationDto<Submission>> GetStudentSubmissionsAsync(Guid studentId, PaginationRequest request)
        {
            var query = _context.Set<Submission>()
                .Include(x => x.AnswerSubmissions)
                .ThenInclude(x => x.Question)
                .Where(x => x.StudentId == studentId);
            var totalRecord = query.Count();
            var totalPages = (int)Math.Ceiling((double)totalRecord / request.PageSize);
            var result = await query.Skip((request.CurrentPage - 1) * request.PageSize).Take(request.PageSize)
                .ToListAsync();
            return new PaginationDto<Submission>
            {
                TotalItems = totalRecord,
                TotalPages = totalPages,
                Items = result,
                HasNextPage = totalPages / request.CurrentPage == 1 ? false : true,
                HasPreviousPage = request.CurrentPage - 1 == 0 ? false : true,
                CurrentPage = request.CurrentPage,
            };
        }
    }
}

