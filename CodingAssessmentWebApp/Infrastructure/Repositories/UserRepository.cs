using System.Linq.Expressions;
using Application.Dtos;
using Application.Interfaces.Repositories;
using Domain.Entitties;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        public UserRepository(ClhAssessmentAppDpContext context) : base(context)
        {
        }
        public async Task<User?> GetAsync(Guid id)
        {
            return await _context.Set<User>()
                .FirstOrDefaultAsync(x => x.Id == id);
        }
        public async Task<User?> GetAsync(Expression<Func<User, bool>> exp)
        {
            return await _context.Set<User>()
                .Include(x => x.Assessments)
                .Include(x => x.Submissions)
                .Include(x => x.AssessmentAssignments)
                .FirstOrDefaultAsync(exp);
        }
        public async Task<ICollection<User>> GetAllAsync()
        {
            return await _context.Set<User>()
                .Include(x => x.Assessments)
                .Include(x => x.Submissions)
                .Include(x => x.AssessmentAssignments)
                .ToListAsync();
        }

        public async Task<ICollection<User>> GetAllAsync(Expression<Func<User, bool>> exp)
        {
            var res = _context.Set<User>()
               .Where(exp);
            return await res.ToListAsync();           

        } 
        public async Task<PaginationDto<User>> GetAllAsync(Expression<Func<User, bool>> exp,PaginationRequest request)
        {
            var query = _context.Set<User>()
                .Where(exp);
            var totalRecord = query.Count();
            var totalPages = totalRecord / request.PageSize;
            var result = await query.Skip((request.CurrentPage - 1) * request.PageSize).Take(request.PageSize)
               .ToListAsync();
            return new PaginationDto<User>
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
    }
}
