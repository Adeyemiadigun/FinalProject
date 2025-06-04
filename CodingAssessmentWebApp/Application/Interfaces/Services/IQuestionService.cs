using Application.Dtos;

namespace Application.Interfaces.Services
{
    public interface IQuestionService
    {
        Task<bool> CreateQuestionsAsync(List<CreateQuestionRequestModel> model, Guid assessmentId);
        //Task<BaseResponse<QuestionDto>> GetQuestionAsync(Guid id);
        //Task<BaseResponse<ICollection<QuestionDto>>> GetAllQuestionsAsync();
        //Task<BaseResponse<ICollection<QuestionDto>>> GetAllQuestionsByAssessmentIdAsync(Guid assessmentId);
        //Task<BaseResponse<ICollection<QuestionDto>>> GetAllQuestionsByInstructorIdAsync(Guid instructorId);
        //Task<BaseResponse<ICollection<QuestionDto>>> GetAllQuestionsByCourseIdAsync(Guid courseId);
    }
}
