using System.Linq.Expressions;
using Application.Dtos;
using Application.Interfaces.Repositories;
using Domain.Entitties;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

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
                .Include(x => x.Batch)
                .ThenInclude(x => x.AssessmentAssignments)
                .FirstOrDefaultAsync(x => x.Id == id);
        }
        public async Task<User?> GetUserAsync(Guid id)
        {
            return await _context.Set<User>()
                .Include(x => x.Batch)
                .ThenInclude(x => x.AssessmentAssignments)
                .Include(x => x.Assessments)
                .Include(x => x.Submissions)
                .Include(x => x.AssessmentAssignments)
                .FirstOrDefaultAsync(x => x.Id == id);
        }
        public async Task<User?> GetAsync(Expression<Func<User, bool>> exp)
        {
            return await _context.Set<User>()
                .Include(x => x.Assessments)
                .Include(x => x.Submissions)
                .ThenInclude(x => x.Assessment)
                .Include(x => x.AssessmentAssignments)
                .FirstOrDefaultAsync(exp);
        }
        public async Task<User?> GetForInstructorAsync(Guid id)
        {
            return await _context.Users
               .Include(x => x.Assessments)
               .ThenInclude(x => x.Submissions)
               .FirstOrDefaultAsync(x => x.Id == id);
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
                 .Include(x => x.Assessments)
                .Include(x => x.Submissions)
                .Include(x => x.AssessmentAssignments)
               .Where(exp);
            return await res.ToListAsync();           

        } 
        public async Task<PaginationDto<User>> GetAllAsync(Expression<Func<User, bool>> exp,PaginationRequest request)
        {
            var query = _context.Set<User>()
                .Where(exp);
            var totalRecord = query.Count();
           var totalPages = (int)Math.Ceiling((double)totalRecord / request.PageSize);
            var result = await query.Skip((request.CurrentPage - 1) * request.PageSize).Take(request.PageSize)
               .ToListAsync();
            return new PaginationDto<User>
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
        public async Task<PaginationDto<User>> GetAllWithRelationshipAsync(Expression<Func<User, bool>> exp, PaginationRequest request)
        {
            var query = _context.Set<User>()
                .Include(x => x.Assessments)
                .Include(x => x.Submissions)
                .Include(x => x.AssessmentAssignments)
                .Where(exp);
            var totalRecord = query.Count();
            var totalPages = (int)Math.Ceiling((double)totalRecord / request.PageSize);
            var result = await query.Skip((request.CurrentPage - 1) * request.PageSize).Take(request.PageSize)
               .ToListAsync();
            return new PaginationDto<User>
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
        public async Task<PaginationDto<User>> GetSelectedIds(ICollection<Guid> ids, PaginationRequest request)
        {
            var res = _context.Set<User>().Include(x => x.AssessmentAssignments).Include(x => x.Submissions)
                .Where(x => ids.Contains(x.Id));
            var totalRecord = res.Count();
            var totalPages = (int)Math.Ceiling((double)totalRecord / request.PageSize);
            var result = await res.Skip((request.CurrentPage - 1) * request.PageSize).Take(request.PageSize)
               .ToListAsync();
            return new PaginationDto<User>
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
        public async Task<List<User>> CheckEmails(ICollection<string> emails)
        {
            var res =  _context.Users.Where(x => emails.Contains(x.Email));
            return await res.ToListAsync();

        }
    }
}
