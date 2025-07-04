﻿using Application.Dtos;
using Domain.Entitties;
using Domain.Enum;

namespace Application.Interfaces.Repositories
{
    public interface IQuestionRepository : IBaseRepository<Question>
    {
        Task<Question> GetAsync(Guid id);
        Task<Question> GetWithOptionsAsync(Guid id);
        Task<ICollection<Question>> GetSelectedIds(ICollection<Guid> ids);
        Task<PaginationDto<Question>> GetAllAsync(Guid assessmentId, PaginationRequest request);
        Task<PaginationDto<Question>> GetAllAsync(Guid assessmentId, QuestionType questionType, PaginationRequest request);
        Task Delete(Question question);

    }
}