using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Application.Dtos;
using Domain.Entities;
using Domain.Entitties;

namespace Application.Interfaces.Repositories
{
    public interface IBatchRepository : IBaseRepository<Batch>
    {
        Task<Batch> GetBatchByIdAsync(Guid id);
        Task<Batch> GetByIdWithRelationshipAsync(Guid id);
        Task<ICollection<Batch>> GetAllAsync(Expression<Func<Batch,bool>> exp);
        Task<IEnumerable<Batch>> GetAllBatchesAsync();
        Task<PaginationDto<Batch>> GetPagedAsync(PaginationRequest request);

        Task<ICollection<Batch>> GetSelectedIds(ICollection<Guid> ids);
        Task<Batch> UpdateBatchAsync(Batch batch);
        Task DeleteBatchAsync(Guid id);
        Task UpdateAsync(Batch batch);
    }
}
