using Application.Dtos;
using Application.Exceptions;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;

namespace Application.Services
{
    public class StudentProgressService : IStudentProgressService
    {
        private readonly IStudentProgressRepository _progressRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserRepository _studentRepository;
        private readonly IAssessmentRepository _assessmentRepository;
        private readonly ICurrentUser _currentUser;

        public StudentProgressService(IStudentProgressRepository progressRepository, IUnitOfWork unitOfWork, IUserRepository studentRepository, IAssessmentRepository assessmentRepository, ICurrentUser currentUser)
        {
            _progressRepository = progressRepository;
            _unitOfWork = unitOfWork;
            _studentRepository = studentRepository;
            _assessmentRepository = assessmentRepository;
            _currentUser = currentUser;
        }
        public async Task SaveProgressAsync(SaveProgressDto saveProgressDto)
        {
            var userId = _currentUser.GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                throw new ApiException("User ID is empty.", 400, "INVALID_USER", null);
            }
            var selectedOptionCount = saveProgressDto.Answers
                .SelectMany(a => a.SelectedOptionIds);
            if (selectedOptionCount.Distinct().Count() != selectedOptionCount.Count())
                throw new ApiException("Duplicate options selected in answers.", 400, "INVALID_INPUT", selectedOptionCount);
            // Validate assessment and student exist
            var studentExists = await _studentRepository.CheckAsync(x => x.Id == userId);
            var assessmentExists = await _assessmentRepository.CheckAsync(x => x.Id == saveProgressDto.AssessmentId);

            if (!studentExists || !assessmentExists)
                throw new ApiException("Invalid student or assessment.", 400, "INVALID_INPUT", null);

            // Load assessment and questions with options
            var assessment = await _assessmentRepository
                .GetWithQuestionsAndOptionsAsync(saveProgressDto.AssessmentId);

            if (assessment == null)
                throw new ApiException("Assessment not found.", 404, "NOT_FOUND", null);

            var questionDict = assessment.Questions.ToDictionary(q => q.Id, q => q);
            var validOptionIds = assessment.Questions
                .SelectMany(q => q.Options)
                .Select(o => o.Id)
                .ToHashSet();

            // Validate incoming answers
            foreach (var answer in saveProgressDto.Answers)
            {
                if (answer.SelectedOptionIds.Count != answer.SelectedOptionIds.Distinct().Count())
                {
                    throw new ApiException(
                        $"Duplicate selected options in answer for question {answer.QuestionId}.",
                        400,
                        "INVALID_INPUT",
                        answer.SelectedOptionIds
                    );
                }
                if (!questionDict.ContainsKey(answer.QuestionId))
                    throw new ApiException($"Invalid question ID: {answer.QuestionId}", 400, "INVALID_INPUT", null);

                foreach (var optionId in answer.SelectedOptionIds)
                {
                    if (!validOptionIds.Contains(optionId))
                        throw new ApiException($"Invalid option ID: {optionId}", 400, "INVALID_INPUT", null);
                }
            }
            var progress = await _progressRepository.GetByStudentAndAssessmentAsync(userId, saveProgressDto.AssessmentId);

            if (progress == null)
            {
                progress = new StudentAssessmentProgress
                {
                    StudentId = userId,
                    AssessmentId = saveProgressDto.AssessmentId,
                    StartedAt = DateTime.UtcNow,
                    CurrentSessionStart = DateTime.UtcNow,
                    LastSavedAt = DateTime.UtcNow,
                    Answers = saveProgressDto.Answers.Select(a => new InProgressAnswer
                    {
                        QuestionId = a.QuestionId,
                        AnswerText = a.AnswerText,
                        SelectedOptions = a.SelectedOptionIds.Select(id => new InProgressSelectedOption { OptionId = id }).ToList()
                    }).ToList()
                };

                await _progressRepository.CreateAsync(progress);
            }
            else
            {
                if (progress.CurrentSessionStart.HasValue)
                {
                    var sessionTime = DateTime.UtcNow - progress.CurrentSessionStart.Value;
                    progress.ElapsedTime += sessionTime;
                }

                progress.CurrentSessionStart = null;
                progress.LastSavedAt = DateTime.UtcNow;

                progress.Answers.Clear();
                foreach (var answer in saveProgressDto.Answers)
                {
                    if (answer.SelectedOptionIds.Count != answer.SelectedOptionIds.Distinct().Count())
                    {
                        throw new ApiException(
                            $"Duplicate selected options in answer for question {answer.QuestionId}.",
                            400,
                            "INVALID_INPUT",
                            answer.SelectedOptionIds
                        );
                    }

                    var newAnswer = new InProgressAnswer
                    {
                        QuestionId = answer.QuestionId,
                        AnswerText = answer.AnswerText,
                        SelectedOptions = answer.SelectedOptionIds.Select(id => new InProgressSelectedOption { OptionId = id }).ToList()
                    };
                    progress.Answers.Add(newAnswer);
                }

                _progressRepository.Update(progress);
            }

            await _unitOfWork.SaveChangesAsync();
        }
        public async Task<BaseResponse<LoadProgressDto>> GetProgressAsync(Guid assessmentId)
        {
            var userId = _currentUser.GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                throw new ApiException("User ID is empty.", 400, "INVALID_USER", null);
            }
            if (userId == Guid.Empty || assessmentId == Guid.Empty)
                throw new ApiException("Invalid student or assessment.", 400, "INVALID_INPUT", null);

            var assessmetCheck = await _assessmentRepository.CheckAsync(x => x.Id == assessmentId);
            var studentCheck = await _studentRepository.CheckAsync(x => x.Id == userId);
            if (!assessmetCheck || !studentCheck)
                throw new ApiException("Invalid student or assessment.", 400, "INVALID_INPUT", null);

            var progress = await _progressRepository.GetByStudentAndAssessmentAsync(userId, assessmentId); // includes Answers and SelectedOptions

            if (progress == null)
            {
                return new BaseResponse<LoadProgressDto>
                {
                    Status = false,
                    Message = "No progress found.",
                    Data = null
                };
            }

            var dto = new LoadProgressDto
            {
                StudentId = userId,
                AssessmentId = progress.AssessmentId,
                StartedAt = progress.StartedAt,
                LastSavedAt = progress.LastSavedAt,
                ElapsedTime = progress.ElapsedTime,
                Answers = progress.Answers.Select(a => new InProgressAnswerDto
                {
                    QuestionId = a.QuestionId,
                    AnswerText = a.AnswerText,
                    SelectedOptionIds = a.SelectedOptions.Select(o => o.OptionId).ToList()
                }).ToList()
            };

            return new BaseResponse<LoadProgressDto>
            {
                Status = true,
                Message = "Progress loaded.",
                Data = dto
            };
        }

    }
}
