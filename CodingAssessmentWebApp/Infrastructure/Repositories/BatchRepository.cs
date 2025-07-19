using System.Linq.Expressions;
using Application.Dtos;
using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class BatchRepository : BaseRepository<Batch>, IBatchRepository
    {
        public BatchRepository(ClhAssessmentAppDpContext context) : base(context)
        {
        }

        public async Task DeleteBatchAsync(Guid id)
        {
            var batch = await _context.Set<Batch>().FindAsync(id);
            if (batch != null)
            {
                _context.Set<Batch>().Remove(batch);
            }
        }

        public async Task<ICollection<Batch>> GetAllAsync(Expression<Func<Batch, bool>> exp)
        {
            if (exp == null)
            {
                throw new ArgumentNullException(nameof(exp));
            }

            var result =  _context.Set<Batch>()
                .Include(x => x.Students)
                .Include(x => x.AssessmentAssignments)
                .ThenInclude(x => x.Assessment)
                .ThenInclude(x =>x.Submissions)
                .Where(exp);
            return await result.ToListAsync();
        }

        public async Task<IEnumerable<Batch>> GetAllBatchesAsync()
        {
           return await _context.Set<Batch>()
               .Include(x => x.Students)
               .ThenInclude(x => x.Submissions).ToListAsync();
        }
        public async Task<PaginationDto<Batch>> GetAllBatchesAsync(PaginationRequest request)
        {
            var query = _context.Set<Batch>()
                .Include(x => x.Students)
                .ThenInclude(x => x.Submissions)
                .ThenInclude(x => x.Assessment);


            var totalRecord = query.Count();
            var totalPages = (int)Math.Ceiling((double)totalRecord / request.PageSize);
            var result = await query.Skip((request.CurrentPage - 1) * request.PageSize).Take(request.PageSize)
               .ToListAsync();
            return new PaginationDto<Batch>
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

        public async Task<Batch> GetBatchByIdAsync(Guid id)
        {
            var batch = await _context.Set<Batch>()
                .Include(x => x.Students)
                .Include(x => x.AssessmentAssignments)
                .ThenInclude(x => x.Assessment)
                .FirstOrDefaultAsync(x => x.Id == id);
               
            return batch; 
        }
        public async Task<Batch?> GetBatchIdWithRelationship(Guid id)
        {
            var batch = await _context.Set<Batch>()
                .Include(x => x.Students)
                .ThenInclude(x => x.Submissions)
                .ThenInclude(x => x.Assessment)
                .FirstOrDefaultAsync(x => x.Id == id);

            return batch;
        }
        public async Task<Batch> GetByIdWithRelationshipAsync(Guid id)
        {
            var batch = await _context.Set<Batch>()
                .Include(x => x.Students)
                .Include(x => x.AssessmentAssignments)
                .FirstOrDefaultAsync(x => x.Id == id);
               
            return batch; 
        }
        

        public async Task<PaginationDto<Batch>> GetPagedAsync(PaginationRequest request)
        {
            var query = _context.Set<Batch>().Include(x => x.Students);

            var totalRecord = query.Count();
            var totalPages = (int)Math.Ceiling((double)totalRecord / request.PageSize);
            var result = await query.Skip((request.CurrentPage - 1) * request.PageSize).Take(request.PageSize)
               .ToListAsync();
            return new PaginationDto<Batch>
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

        public async Task<ICollection<Batch>> GetSelectedIds(ICollection<Guid> ids)
        {
            var batches = await _context.Set<Batch>()
                .Where(batch => ids.Contains(batch.Id))
                .ToListAsync();

            return batches;
        }

        public async Task UpdateAsync(Batch batch)
        {
            if (batch == null)
            {
                throw new ArgumentNullException(nameof(batch));
            }

            var existingBatch = await _context.Set<Batch>().FindAsync(batch.Id);
            if (existingBatch != null)
            {
                existingBatch.Update();
                _context.Set<Batch>().Update(existingBatch);
                await _context.SaveChangesAsync();
            }
        }

        public Task<Batch> UpdateBatchAsync(Batch batch)
        {
            throw new NotImplementedException();
        }
    }
}
