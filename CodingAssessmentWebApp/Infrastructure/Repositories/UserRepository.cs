using System.Linq.Expressions;
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
            return await _context.Set<User>().ToListAsync();

        }
    }
}
