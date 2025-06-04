using System.Linq.Expressions;

namespace Application.Interfaces.Repositories
{
    public interface IBaseRepository<T>
    {
        Task CreateAsync(T entity);
        Task CreateAsync(ICollection<T> entity);
        void Update(T entity);
        Task<bool> CheckAsync(Expression<Func<T, bool>> exp);
        Task<bool> CheckAllIds(ICollection<Guid> ids, Expression<Func<T, bool>> exp);
    }
}
