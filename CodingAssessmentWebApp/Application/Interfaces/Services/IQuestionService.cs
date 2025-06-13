using Application.Dtos;

namespace Application.Interfaces.Services
{
    public interface IQuestionService
    {
        Task<bool> CreateQuestionsAsync(List<CreateQuestionRequestModel> model, Guid assessmentId);
        Task<BaseResponse<QuestionDto>> GetQuestionAsync(Guid id);
        Task<BaseResponse<PaginationDto<QuestionDto>>> GetAllQuestionsByAssessmentIdAsync(Guid assessmentId,PaginationRequest request);
        Task<BaseResponse<QuestionDto>> UpdateQuestion(Guid id, UpdateQuestionDto model);
        Task<BaseResponse<QuestionDto>> DeleteQuestionAsync(Guid id); 
        //Task<BaseResponse<ICollection<QuestionDto>>> GetAllQuestionsByInstructorIdAsync(Guid instructorId);
    }
}
