using System.Linq.Expressions;
using Application.Dtos;
using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    public interface IBatchRepository : IBaseRepository<Batch>
    {
        Task<Batch> GetBatchByIdAsync(Guid id);
        Task<Batch> GetByIdWithRelationshipAsync(Guid id);
        Task<ICollection<Batch>> GetAllAsync(Expression<Func<Batch,bool>> exp);
        Task<IEnumerable<Batch>> GetAllBatchesAsync();
        Task<PaginationDto<Batch>> GetAllBatchesAsync(PaginationRequest request);
        Task<PaginationDto<Batch>> GetPagedAsync(PaginationRequest request);

        Task<ICollection<Batch>> GetSelectedIds(ICollection<Guid> ids);
        Task<Batch> UpdateBatchAsync(Batch batch);
        Task DeleteBatchAsync(Guid id);
        Task UpdateAsync(Batch batch);
        Task<Batch?> GetBatchIdWithRelationship(Guid id);
    }
}
