using System.Linq.Expressions;
using Application.Dtos;
using Application.Interfaces.Repositories;
using Domain.Entitties;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class AssessmentRepository : BaseRepository<Assessment>, IAssessmentRepository
    {
        public AssessmentRepository(ClhAssessmentAppDpContext context) : base(context)
        {
        }
        public async Task<Assessment?> GetAsync(Guid id)
        {
           return await _context.Set<Assessment>()
                .Include(x => x.AssessmentAssignments)
                .Include(x => x.Submissions)
                .FirstOrDefaultAsync(x => x.Id == id);
        } 
        public async Task<Assessment?> GetForBatchPerformanceAsync(Guid id)
        {
           return await _context.Set<Assessment>()
                .Include(a => a.Submissions)
                .ThenInclude(s => s.Student)
                .ThenInclude(st => st.Batch)
                .Include(x => x.AssessmentAssignments)
                .FirstOrDefaultAsync(x => x.Id == id);
        }    
        public async Task<Assessment?> GetWithQuestionsAndOptionsAsync(Guid id)
        {
            return await _context.Assessments
                .Include(x => x.Questions)
                .ThenInclude(x => x.Options)
                .FirstOrDefaultAsync(x => x.Id == id);

        }
        public Task<Assessment?> GetAsync(string name)
        {
            return _context.Set<Assessment>()
                .Include(x => x.AssessmentAssignments)
                .Include(x => x.Submissions)
                .FirstOrDefaultAsync(x => x.Title == name);
        }
        public async Task<PaginationDto<Assessment>> GetAllAsync(PaginationRequest request)
        {
            var query = _context.Set<Assessment>()
                .Include(x => x.AssessmentAssignments)
                .Include(x => x.Submissions);
            var totalRecord = query.Count();
            var totalPages = (int)Math.Ceiling((double)totalRecord / request.PageSize);
            var result = await query.Skip((request.CurrentPage - 1) * request.PageSize).Take(request.PageSize)
               .ToListAsync();
            return new PaginationDto<Assessment>
            {
                TotalPages = totalPages,
                TotalItems = totalRecord,
                Items = result,
                HasNextPage = request.CurrentPage < totalPages,
                HasPreviousPage = request.CurrentPage > 1,
                PageSize = request.PageSize,
                  CurrentPage = request.CurrentPage

            };
        }
        public async Task<PaginationDto<Assessment>> GetAllAsync(Guid userId,PaginationRequest request)
        {
            var query = _context.Set<Assessment>()
                  .Include(x => x.AssessmentAssignments)
                  .Include(x => x.Submissions);
            var totalRecord = query.Count();
            var totalPages = (int)Math.Ceiling((double)totalRecord / request.PageSize);
            var result = await query.Skip((request.CurrentPage - 1) * request.PageSize).Take(request.PageSize)
               .ToListAsync();
            return new PaginationDto<Assessment>
            {
                TotalPages = totalPages,
                TotalItems = totalRecord,
                Items = result,
                HasNextPage = request.CurrentPage < totalPages,
                HasPreviousPage = request.CurrentPage > 1,
                PageSize = request.PageSize,
                CurrentPage = request.CurrentPage

            };
        }
        public async Task<ICollection<Assessment>> GetSelectedIds(ICollection<Guid> ids)
        {
            var res = _context.Set<Assessment>().Include(x => x.AssessmentAssignments).Include(x => x.Submissions)
                .Where(x => ids.Contains(x.Id));
            return await res.ToListAsync();
        }

        public async Task<PaginationDto<Assessment>> GetAllAsync(Expression<Func<Assessment, bool>> exp, PaginationRequest request)
        {
            var query = _context.Set<Assessment>()
                .Include(x => x.AssessmentAssignments)
                .Include(x => x.Submissions)
                .ThenInclude(x => x.Student)
                .ThenInclude(x => x.Batch)
                .Include(x => x.Instructor)
                .Where(exp);
            var totalRecord = query.Count();
            var totalPages = (int)Math.Ceiling((double)totalRecord / request.PageSize);
            var result = await query.Skip((request.CurrentPage - 1) * request.PageSize).Take(request.PageSize)
               .ToListAsync();
            return new PaginationDto<Assessment>
            {
                TotalPages = totalPages,
                TotalItems = totalRecord,
                Items = result,
                HasNextPage = request.CurrentPage < totalPages,
                HasPreviousPage = request.CurrentPage > 1,
                PageSize = request.PageSize,
                CurrentPage = request.CurrentPage

            };
        }

        public async Task<ICollection<Assessment>> GetAllAsync(Expression<Func<Assessment, bool>> exp)
        {
            var query = _context.Set<Assessment>()
                .Include(x => x.AssessmentAssignments)
                .ThenInclude(x => x.Student)    
                .Include(x => x.Submissions)
                .ThenInclude(x => x.Student)
                .Include(x => x.Questions)
                .Where(exp);
            return await query.ToListAsync();
           
        }

        public async Task<Assessment?> GetForSubmissionAsync(Guid id)
        {
            return await _context.Assessments
                .Include(x => x. Submissions)
                .FirstOrDefaultAsync(x => x.Id == id);
        }
    }
}
