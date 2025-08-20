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
                throw new ApiException("User ID is empty.", 400, "INVALID_USER", null);

            if (saveProgressDto == null || saveProgressDto.AssessmentId == Guid.Empty)
                throw new ApiException("Invalid request payload.", 400, "INVALID_INPUT", null);

            // Check student and assessment
            var studentExists = await _studentRepository.CheckAsync(x => x.Id == userId);
            var assessment = await _assessmentRepository.GetWithQuestionsAndOptionsAsync(saveProgressDto.AssessmentId);

            if (!studentExists || assessment == null)
                throw new ApiException("Invalid student or assessment.", 400, "INVALID_INPUT", null);

            // Build valid question/option sets
            var questionDict = assessment.Questions.ToDictionary(q => q.Id);
            var validOptionIds = assessment.Questions.SelectMany(q => q.Options).Select(o => o.Id).ToHashSet();

            // Global duplicate check
            var allSelectedOptionIds = saveProgressDto.Answers.SelectMany(a => a.SelectedOptionIds ?? new List<Guid>());
            if (allSelectedOptionIds.Count() != allSelectedOptionIds.Distinct().Count())
                throw new ApiException("Duplicate selected options in answers.", 400, "INVALID_INPUT", allSelectedOptionIds);

            // Per-answer validation
            foreach (var answer in saveProgressDto.Answers)
            {
                if (!questionDict.ContainsKey(answer.QuestionId))
                    throw new ApiException($"Invalid question ID: {answer.QuestionId}", 400, "INVALID_QUESTION", null);

                var selectedIds = answer.SelectedOptionIds ?? new List<Guid>();
                if (selectedIds.Count != selectedIds.Distinct().Count())
                    throw new ApiException($"Duplicate selected options in question {answer.QuestionId}.", 400, "INVALID_INPUT", selectedIds);

                foreach (var optionId in selectedIds)
                {
                    if (!validOptionIds.Contains(optionId))
                        throw new ApiException($"Invalid option ID: {optionId}", 400, "INVALID_OPTION", null);
                }
            }

            // Load existing progress with tracking
            var progress = await _progressRepository
                .GetByStudentAndAssessmentAsync(userId, saveProgressDto.AssessmentId); 

            if (progress == null)
            {
                // First save
                var newProgress = new StudentAssessmentProgress
                {
                    StudentId = userId,
                    AssessmentId = saveProgressDto.AssessmentId,
                    StartedAt = DateTime.UtcNow,
                    CurrentSessionStart = saveProgressDto.CurrentSessionStart ?? DateTime.UtcNow,
                    LastSavedAt = DateTime.UtcNow,
                    ElapsedTime = saveProgressDto.ElapsedTime ?? TimeSpan.Zero,
                    Answers = saveProgressDto.Answers.Select(a => new InProgressAnswer
                    {
                        QuestionId = a.QuestionId,
                        AnswerText = a.AnswerText,
                        SelectedOptions = (a.SelectedOptionIds ?? new List<Guid>())
                            .Select(id => new InProgressSelectedOption { OptionId = id }).ToList()
                    }).ToList()
                };

                await _progressRepository.CreateAsync(newProgress);
            }
            else
            {
                // Update timing
                if (saveProgressDto.ElapsedTime.HasValue)
                {
                    progress.ElapsedTime += saveProgressDto.ElapsedTime.Value;
                }

                progress.LastSavedAt = DateTime.UtcNow;
                progress.CurrentSessionStart = null;


                progress.CurrentSessionStart = null;
                progress.LastSavedAt = DateTime.UtcNow;

                // remove existing answers & options 
                await _progressRepository.RemoveAnswersAndOptionsAsync(progress);

                // Replace with new answers
                foreach (var answer in saveProgressDto.Answers)
                {
                    var selectedOptions = (answer.SelectedOptionIds ?? new List<Guid>())
                        .Select(id => new InProgressSelectedOption { OptionId = id }).ToList();

                    var newAnswer = new InProgressAnswer
                    {
                        QuestionId = answer.QuestionId,
                        AnswerText = answer.AnswerText,
                        SelectedOptions = selectedOptions
                    };

                    progress.Answers.Add(newAnswer);
                }

                // No need to call Update, EF is tracking it
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
