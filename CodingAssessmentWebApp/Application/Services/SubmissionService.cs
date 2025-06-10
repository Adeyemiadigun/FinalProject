using Application.Dtos;
using Application.Exceptions;
using Application.Interfaces.ExternalServices;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entitties;

namespace Application.Services
{
    public class SubmissionService(IAssessmentRepository _assessmentRepository, ICurrentUser _currentUser, ISubmissionRepository _submissionRepository, IUnitOfWork _unitOfWork, IQuestionRepository _questionRepository, IGradingService _gradingService, IEmailService _emailService, IUserRepository _userRepository, IBackgroundService _backgroundService) : ISubmissionService
    {
        public async Task<BaseResponse<SubmissionDto>> SubmitAssessment(AnswerSubmissionDto submission)
        {
            var studentId = _currentUser.GetCurrentUserId();
            if (studentId == Guid.Empty)
                throw new ApiException("Current user ID is not set or invalid.", 400, "InvalidUserId", null);

            var student = await _userRepository.GetAsync(studentId);
            if (student is null)
                throw new ApiException("Student not found", 404, "StudentNotFound", null);

            var assessment = await _assessmentRepository.GetForSubmissionAsync(submission.AssessmentId);
            if (assessment is null)
                throw new ApiException("Assessment not found", 404, "AssessmentNotFound", null);

            if (assessment.Submissions.Any(s => s.StudentId == studentId))
                throw new ApiException("Duplicate submission detected.", 400, "DuplicateSubmission", null);

            if (DateTime.Now > assessment.EndDate)
                return new BaseResponse<SubmissionDto>
                {
                    Message = "Assessment is closed",
                    Status = false
                };

            if (DateTime.Now < assessment.StartDate)
                return new BaseResponse<SubmissionDto>
                {
                    Message = "Assessment has not started yet",
                    Status = false
                };

            var questionIds = submission.Answers.Select(x => x.QuestionId).ToList();
            var questions = await _questionRepository.GetSelectedIds(questionIds);
            if (questionIds.Count != questions.Count)
            {
                return new BaseResponse<SubmissionDto>
                {
                    Status = false,
                    Message = "Some Question IDs are invalid."
                };
            }

            var submissionEntity = new Submission()
            {
                StudentId = studentId,
                Student = student,
                AssessmentId = submission.AssessmentId,
                SubmittedAt = DateTime.UtcNow,
            };

            var answerSubmissions = submission.Answers.Select(x => new AnswerSubmission()
            {
                SubmissionId = submissionEntity.Id,
                Submission = submissionEntity,
                QuestionId = x.QuestionId,
                Question = questions.FirstOrDefault(q => q.Id == x.QuestionId)!,
                SubmittedAnswer = x.SubmittedAnswer,
                Score = 0,
                IsCorrect = false,
            }).ToList();
            submissionEntity.AnswerSubmissions = answerSubmissions;
            await _submissionRepository.CreateAsync(submissionEntity);
            await _gradingService.GradeSubmissionAsync(submissionEntity);
            submissionEntity.FeedBack = submissionEntity.TotalScore >= assessment.PassingScore ? "You Passed the assessment" : "You failed the assessment";
            _backgroundService.Enqueue<IEmailService>(emailService => emailService.SendResultEmailAsync(submissionEntity, student!));
            await _unitOfWork.SaveChangesAsync();
            return new BaseResponse<SubmissionDto>()
            {
                Message = "Submission created successfully",
                Status = true,
            };
        }

        public Task<bool> CheckSubmissionExistsAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<bool> CheckSubmissionExistsAsync(Guid studentId, Guid assessmentId)
        {
            throw new NotImplementedException();
        }

        public Task<PaginationDto<Submission>> GetAllAsync(Guid assessmentId, PaginationRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<Submission> GetAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<PaginationDto<Submission>> GetStudentSubmissionsAsync(Guid studentId, PaginationRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<Submission> GetWithAnswersAsync(Guid id)
        {
            throw new NotImplementedException();
        }
    }
}
