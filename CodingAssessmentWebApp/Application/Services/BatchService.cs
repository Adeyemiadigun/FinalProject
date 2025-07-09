using System.Globalization;
using System.Linq.Expressions;
using Application.Dtos;
using Application.Exceptions;
using Application.Interfaces.ExternalServices;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Entitties;

namespace Application.Services
{
    public class BatchService(IBatchRepository batchRepository, IUnitOfWork unitOfwork, IAssessmentRepository assessmentRepository, IBackgroundService _backgroundService) : IBatchService
    {
        public async Task<BaseResponse<string>> AssignAssessmentToBatchAsync(Guid batchId, Guid assessmentId)
        {
            // Validate input parameters  
            if (batchId == Guid.Empty || assessmentId == Guid.Empty)
            {
                throw new ApiException("Invalid batch or assessment ID.", 400, "InvalidInput", null);
            }
            var assessment = await assessmentRepository.GetAsync(assessmentId);
            if (assessment == null)
            {
                throw new ApiException("Assessment not found.", 404, "AssessmentNotFound", null);
            }

            // Retrieve the batch entity  
            var batch = await batchRepository.GetBatchByIdAsync(batchId);
            if (batch == null)
            {
                throw new ApiException("Batch not found.", 404, "BatchNotFound", null);
            }
            if (batch.AssessmentAssignments.Any())
                throw new ApiException("Batch already has an assessment assigned.", 400, "BatchAlreadyHasAssessment", null);
            if (batch == null)
            {
                throw new ApiException("Batch not found.", 404, "BatchNotFound", null);
            }
            var batchAssessment = new BatchAssessment()
            {
                BatchId = batchId,
                AssessmentId = assessmentId
            };
            var studentAssessments = batch.Students.Select(x => new AssessmentAssignment()
            {
                StudentId = x.Id,
                Student = x,
                AssessmentId = assessment.Id,
                Assessment = assessment
            }
            );
            var validStudent = batch.Students;
            _backgroundService.Enqueue<IEmailService>(emailService => emailService.SendBulkEmailAsync(validStudent, "New Assessment", new AssessmentDto()
            {
                Title = assessment.Title,
                Description = assessment.Description,
                TechnologyStack = assessment.TechnologyStack,
                DurationInMinutes = assessment.DurationInMinutes,
                StartDate = assessment.StartDate,
                EndDate = assessment.EndDate,
                PassingScore = assessment.PassingScore
            }));
            // Update the batch in the repository  
            await batchRepository.UpdateAsync(batch);
            await unitOfwork.SaveChangesAsync();

            // Return success response  
            return new BaseResponse<string>
            {
                Status = true,
                Message = "Assessment assigned to batch successfully.",
                Data = "Success"
            };
        }

        public async Task<BaseResponse<List<BatchAnalytics>>> BatchAnalytics()
        {
            var batches = await batchRepository.GetAllBatchesAsync();
            if (batches is null || !batches.Any())
            {
                throw new ApiException("No batches found.", 404, "NoBatchesFound", null);
            }

            var batchAnalytics = batches.Select(x => new BatchAnalytics()
            {
                Id = x.Id,
                Name = x.Name + x.BatchNumber,
                StudentCount = x.Students.Count,
                AverageScore = x.Students
                                 .Where(student => student.Submissions.Any())
                                 .Select(student => student.Submissions.Sum(sub => sub.TotalScore))
                                 .DefaultIfEmpty(0)
                                 .Average(),
                PassRate = x.Students
                             .Where(student => student.Submissions.Any())
                             .Count(student => student.Submissions.Any(sub => sub.TotalScore >= sub.Assessment.PassingScore))
                             / (double)x.Students.Count * 100
            }).ToList();

            return new BaseResponse<List<BatchAnalytics>>()
            {
                Data = batchAnalytics,
                Status = true,
                Message = "Batch Analytics"
            };
        }

        public async Task<BaseResponse<BatchDto>> CreateBatchAsync(CreateBatchRequestModel model)
        {
            if (model == null)
            {
                throw new ApiException("Model cannot be null.", 400, "InvalidModel", null);
            }

            // Simulate batch creation logic  
            var createdBatch = new Batch()
            {
                Name = model.Name,
                BatchNumber = (short)model.BatchNumber
            };
            await batchRepository.CreateAsync(createdBatch);
            await unitOfwork.SaveChangesAsync();
            return new BaseResponse<BatchDto>
            {
                Status = true,
                Message = "Batch created successfully.",
                Data = new BatchDto()
                {
                    Id = createdBatch.Id,
                    Name = model.Name,
                    BatchNumber = (short)model.BatchNumber

                }
            };
        }

        public Task<BaseResponse<BatchDto>> DeleteBatchAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public async Task<BaseResponse<PaginationDto<BatchDto>>> GetAllBatchesAsync(PaginationRequest request)
        {
            if (request == null)
            {
                throw new ApiException("Pagination request cannot be null.", 400, "InvalidPaginationRequest", null);
            }

            var batches = await batchRepository.GetPagedAsync(request);
            if (batches == null || !batches.Items.Any())
            {
                throw new ApiException("No batches found.", 404, "NoBatchesFound", null);
            }

            var batchDtos = batches.Items.Select(batch => new BatchDto
            {
                Id = batch.Id,
                Name = batch.Name,
                BatchNumber = batch.BatchNumber
            }).ToList();

            var paginationDto = new PaginationDto<BatchDto>
            {
                Items = batchDtos,
                TotalItems = batches.TotalItems,
                TotalPages = batches.TotalPages,
                CurrentPage = batches.CurrentPage,
                PageSize = batches.PageSize,
                HasNextPage = batches.HasNextPage,
                HasPreviousPage = batches.HasPreviousPage
            };

            return new BaseResponse<PaginationDto<BatchDto>>
            {
                Status = true,
                Message = "Batches retrieved successfully.",
                Data = paginationDto
            };
        }

        public async Task<BaseResponse<List<BatchDto>>> GetAllBatchesAsync()
        {
            var batchees = await batchRepository.GetAllBatchesAsync();
            if (batchees == null || !batchees.Any())
            {
                throw new ApiException("No batches found.", 404, "NoBatchesFound", null);
            }

            var batchesDto = batchees.Select(x => new BatchDto()
            {
                Id = x.Id,
                Name = x.Name,
                BatchNumber = x.BatchNumber
            }).ToList();

            return new BaseResponse<List<BatchDto>>
            {
                Message = "All Batches Retrieved",
                Status = true,
                Data = batchesDto
            };
        }

        public Task<BaseResponse<BatchDto>> GetBatchByIdAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<BaseResponse<BatchDto>> UpdateBatchAsync(Guid id, UpdateBatchRequestModel model)
        {
            throw new NotImplementedException();
        }
        public async Task<BaseResponse<BatchAnalyticsMetricsDto>> GetBatchAnalyticsMetrics(Guid? batchId)
        {
            Expression<Func<Batch, bool>> predicate = b => true;
            if (batchId.HasValue && batchId.Value != Guid.Empty)
                predicate = b => b.Id == batchId.Value;

            var batches = await batchRepository.GetAllAsync(predicate);

            if (!batches.Any())
                throw new ApiException("No batches found.", 404, "NoBatchesFound", null);

            var assessments = batches
                .SelectMany(b => b.AssessmentAssignments)
                .Select(ba => ba.Assessment)
                .Distinct()
                .ToList();

            if (!assessments.Any())
            {
                return new BaseResponse<BatchAnalyticsMetricsDto>
                {
                    Status = false,
                    Message = "No assessments found for the batch.",
                    Data = null
                };
            }

            var submissions = assessments
                .SelectMany(a => a.Submissions)
                .ToList();

            if (!submissions.Any())
            {
                throw new ApiException("No submissions found.", 404, "NoSubmissionsFound", null);
            }

            var totalSubmissions = submissions.Count;
            var passedSubmissions = submissions.Count(s => s.TotalScore >= s.Assessment.PassingScore);
            var averageScore = submissions.Average(s => s.TotalScore);

            var assignedStudentsCount = assessments
                .SelectMany(a => a.AssessmentAssignments)
                .Select(x => x.StudentId)
                .Distinct()
                .Count();

            var completionRate = assignedStudentsCount > 0
                ? (totalSubmissions * 100.0 / assignedStudentsCount)
                : 0;
            var avgTimeInSeconds = submissions
                .Where(s => s.SubmittedAt != default && s.Assessment.StartDate != default)
                .Average(s => (s.SubmittedAt - s.Assessment.StartDate).TotalSeconds);

            var avgTimeFormatted = TimeSpan.FromSeconds(avgTimeInSeconds).ToString(@"hh\:mm\:ss");

            return new BaseResponse<BatchAnalyticsMetricsDto>
            {
                Status = true,
                Message = "Batch analytics metrics retrieved successfully.",
                Data = new BatchAnalyticsMetricsDto
                {
                    PassRate = Math.Round(passedSubmissions * 100.0 / totalSubmissions, 2),
                    AverageScore = Math.Round(averageScore, 2),
                    CompletionRate = Math.Round(completionRate, 2),
                    AverageCompletionTime = avgTimeFormatted
                }
            };
        }

        public async Task<BaseResponse<BatchPerformanceTrendDto>> GetBatchPerformanceTrend(Guid? batchId)
        {
            // 1. Build predicate to apply filtering at DB-level
            Expression<Func<Assessment, bool>> predicate = a => true;

            if (batchId.HasValue && batchId.Value != Guid.Empty)
            {
                predicate = a => a.BatchAssessment.Any(ba => ba.BatchId == batchId.Value);
            }

            // 2. Query database with includes and filter
            var assessments = await assessmentRepository.GetAllAsync(predicate);

            if (!assessments.Any())
            {
                throw new ApiException("No assessments found.", 404, "NoAssessmentsFound", null);
            }

            var submissions = assessments
                .SelectMany(a => a.Submissions)
                .Where(s => s.SubmittedAt != default(DateTime))
                .ToList();

            if (!submissions.Any())
            {
                return new BaseResponse<BatchPerformanceTrendDto>
                {
                    Status = false,
                    Message = "No submissions found to compute trend.",
                    Data = null
                };
            }

            var grouped = submissions
                .GroupBy(s => $"{s.SubmittedAt.Year}-W{CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(
                    s.SubmittedAt,
                    CalendarWeekRule.FirstFourDayWeek,
                    DayOfWeek.Monday)}")
                .OrderBy(g => g.Key)
                .ToList();

            // 5. Create response DTO
            var dto = new BatchPerformanceTrendDto
            {
                Labels = grouped.Select(g => g.Key).ToList(),
                Scores = grouped.Select(g => Math.Round(g.Average(s => s.TotalScore), 2)).ToList()
            };

            return new BaseResponse<BatchPerformanceTrendDto>
            {
                Status = true,
                Message = "Batch performance trend data retrieved successfully.",
                Data = dto
            };
        }

        public async Task<BaseResponse<List<BatchSummaryDto>>> GetBatchSummariesAsync()
        {
            var batches = await batchRepository.GetAllBatchesAsync();

            if (!batches.Any())
            {
                throw new ApiException("No batches found.", 404, "NoBatchesFound", null);
            }

            var result = batches.Select(batch =>
            {
                var assessments = batch.AssessmentAssignments;
                var allSubmissions = assessments.SelectMany(a => a.Assessment.Submissions).ToList();

                double passRate = 0;
                if (allSubmissions.Any())
                {
                    var passed = allSubmissions.Count(s => s.TotalScore >= s.Assessment.PassingScore);
                    passRate = passed * 100.0 / allSubmissions.Count;
                }

                return new BatchSummaryDto
                {
                    BatchName = $"{batch.Name} {batch.BatchNumber}",
                    StudentCount = batch.Students.Count,
                    AssessmentCount = assessments.Count,
                    PassRate = Math.Round(passRate, 2)
                };
            }).ToList();

            return new BaseResponse<List<BatchSummaryDto>>
            {
                Status = true,
                Message = "Batch summaries retrieved successfully.",
                Data = result
            };
        }
        public async Task<BaseResponse<List<BatchStudentCountDto>>> GetBatchStudentCountsAsync()
        {
            var batches = await batchRepository.GetAllBatchesAsync();
            if (batches == null || !batches.Any())
            {
                throw new ApiException("No batches found.", 404, "NoBatchesFound", null);
            }
            var batchStudentCounts = batches.Select(batch => new BatchStudentCountDto
            {
                BatchId = batch.Id,
                BatchName = $"{batch.Name} {batch.BatchNumber}",
                StudentCount = batch.Students.Count
            }).ToList();
            return new BaseResponse<List<BatchStudentCountDto>>
            {
                Status = true,
                Message = "Batch student counts retrieved successfully.",
                Data = batchStudentCounts
            };
        }
    }
}
