using Application.Dtos;
using Application.Interfaces.Repositories;
using Domain.Entitties;
using Domain.Enum;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class QuestionRepository(ClhAssessmentAppDpContext context) : BaseRepository<Question>(context), IQuestionRepository
    {
        public async Task<PaginationDto<Question>> GetAllAsync(Guid assessmentId, PaginationRequest request)
        {
            var query = _context.Set<Question>()
                .Include(x => x.Answer)
                .Include(x => x.Options)
                .Include(x => x.Tests)
                .Include(x => x.AnswerSubmissions)
                .Where(x => x.AssessmentId == assessmentId)
                .OrderBy(x => x.Order);
            var totalRecord = query.Count();
            var totalPages = (int)Math.Ceiling((double)totalRecord / request.PageSize);
            var result = await query.Skip((request.CurrentPage - 1) * request.PageSize).Take(request.PageSize)
               .ToListAsync();
            return new PaginationDto<Question>
            {
                TotalPages = totalPages,
                TotalItems = totalRecord,
                Items = result,
                HasNextPage = totalPages / request.CurrentPage == 1 ? false : true,
                HasPreviousPage = request.CurrentPage - 1 == 0 ? false : true,
                PageSize = request.PageSize,
                CurrentPage = request.CurrentPage
            };
        }

        public async Task<PaginationDto<Question>> GetAllAsync(Guid assessmentId, QuestionType questionType, PaginationRequest request)
        {
            var query = _context.Set<Question>()
                .Include(x => x.Answer)
                .Include(x => x.Options)
                .Include(x => x.Tests)
                .Include(x => x.AnswerSubmissions)
                .Where(x => x.AssessmentId == assessmentId && x.QuestionType == questionType);
            var totalRecord = query.Count();
            var totalPages = (int)Math.Ceiling((double)totalRecord / request.PageSize);
            var result = await query.Skip((request.CurrentPage - 1) * request.PageSize).Take(request.PageSize)
               .ToListAsync();
            return new PaginationDto<Question>
            {
                TotalPages = totalPages,
                TotalItems = totalRecord,
                Items = result,
                HasNextPage = totalPages / request.CurrentPage == 1 ? false : true,
                HasPreviousPage = request.CurrentPage - 1 == 0 ? false : true,
                PageSize = request.PageSize,
                CurrentPage = request.CurrentPage
            };
        }

        public async Task<Question?> GetAsync(Guid id)
        {
            return await _context.Set<Question>()
                .Include(x => x.Answer)
                .Include(x => x.Options)
                .Include(x => x.Tests)
                .Include(x => x.AnswerSubmissions)
                .FirstOrDefaultAsync(x => x.Id == id);
        }
        public Task<Question> GetWithOptionsAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public async Task<ICollection<Question>> GetSelectedIds(ICollection<Guid> ids)
        {
            var res = _context.Set<Question>()
                .Include(x => x.Answer)
                .Include(x => x.Tests)
                .Include(x => x.Options)
                .Where(x => ids.Contains(x.Id));
            return await res.ToListAsync();
        }
    }
}

