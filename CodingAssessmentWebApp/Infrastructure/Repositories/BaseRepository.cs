using System.Linq.Expressions;
using Application.Interfaces.Repositories;
using Domain.Entitties;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        protected ClhAssessmentAppDpContext _context;
        public BaseRepository(ClhAssessmentAppDpContext context)
        {
            _context = context;
        }
        public async Task<bool> CheckAsync(Expression<Func<T, bool>> exp)
        {
            return await _context.Set<T>().AnyAsync(exp);
        }
        public async Task CreateAsync(T entity)
        {
            await _context.Set<T>().AddAsync(entity);
        }

        public async Task CreateAsync(ICollection<T> entity)
        {
            await _context.Set<T>().AddRangeAsync(entity);
        }

        public void Update(T entity)
        {
            _context.Set<T>().Update(entity);
        }
        public async Task<bool> CheckAllIds(ICollection<Guid> ids, Expression<Func<T, bool>> exp)
        {
            var existingIdsCount = await _context.Set<T>()
                                                 .CountAsync(exp);
            return existingIdsCount == ids.Count;
        }

    }
}
