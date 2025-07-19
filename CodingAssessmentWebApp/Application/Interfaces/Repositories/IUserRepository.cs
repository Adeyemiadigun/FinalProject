using System.Linq.Expressions;
using Application.Dtos;
using Domain.Entitties;

namespace Application.Interfaces.Repositories
{
    public interface IUserRepository : IBaseRepository<User>
    {
        Task<User?> GetAsync(Guid id);
        Task<User?> GetUserAsync(Guid id);
        Task<User?> GetAsync(Expression<Func<User, bool>> exp);
        Task<ICollection<User>> GetAllAsync();
        Task<ICollection<User>> GetAllAsync(Expression<Func<User, bool>> exp);
        Task<PaginationDto<User>> GetAllAsync(Expression<Func<User, bool>> exp, PaginationRequest request);
        Task<PaginationDto<User>> GetAllWithRelationshipAsync(Expression<Func<User, bool>> exp, PaginationRequest request);
        Task<User?> GetForInstructorAsync(Guid id);
        Task<PaginationDto<User>> GetSelectedIds(ICollection<Guid> ids, PaginationRequest request);
        Task<List<User>> CheckEmails(ICollection<string> emails);
    }
}
