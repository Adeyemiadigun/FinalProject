using System.Linq.Expressions;
using Application.Dtos;
using Domain.Entitties;

namespace Application.Interfaces.Repositories
{
    public interface IAssessmentRepository : IBaseRepository<Assessment>
    {
        Task<Assessment?> GetAsync(Guid id);
        Task<Assessment?> GetForBatchPerformanceAsync(Guid id);
        Task<Assessment?> GetForSubmissionAsync(Guid id);
        Task<Assessment?> GetAsync(Expression<Func<Assessment, bool>> exp);
        Task<PaginationDto<Assessment>> GetAllAsync(PaginationRequest request);
        Task<PaginationDto<Assessment>> GetAllAsync(Expression<Func<Assessment, bool>> exp, PaginationRequest request);
        Task<PaginationDto<Assessment>> GetAllAsync(Guid userId,PaginationRequest request);
        Task<ICollection<Assessment>> GetAllAsync(Expression<Func<Assessment, bool>> exp);
        Task<ICollection<Assessment>> GetSelectedIds(ICollection<Guid> ids);
        Task<Assessment?> GetWithQuestionsAndOptionsAsync(Guid id);
    }
}
