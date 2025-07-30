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
    public class BatchService(IBatchRepository batchRepository, IUnitOfWork unitOfwork, IAssessmentRepository assessmentRepository, IBackgroundService _backgroundService, ILeaderboardStore _leaderboardStore, ITemplateService tempService) : IBatchService
    {
        public async Task<BaseResponse<string>> AssignAssessmentToBatchAsync(Guid batchId, Assessment assessment)
        {
            // Validate input parameters  
            if (batchId == Guid.Empty)
            {
                throw new ApiException("Invalid batch.", 400, "InvalidInput", null);
            }
            // Retrieve the batch entity  
            var batch = await batchRepository.GetBatchByIdAsync(batchId);
            if (batch == null)
            {
                throw new ApiException("Batch not found.", 404, "BatchNotFound", null);
            }
            var newStart = assessment.StartDate;
            var newEnd = assessment.StartDate.AddMinutes(assessment.DurationInMinutes);

            if (batch.AssessmentAssignments.Any(x =>
            {
                var existingStart = x.Assessment.StartDate;
                var existingEnd = x.Assessment.StartDate.AddMinutes(x.Assessment.DurationInMinutes);

                return newStart < existingEnd && newEnd > existingStart;
            }))
            {
                throw new ApiException("Batch already has an assessment assigned in the same time frame.", 200, "BatchAlreadyHasAssessment", batch.Name);
            }
            var studentAssessments = batch.Students.Select(x => new AssessmentAssignment()
            {
                StudentId = x.Id,
                Student = x,
                AssessmentId = assessment.Id,
                Assessment = assessment
            }
            );
            foreach (var assignment in studentAssessments)
            {
                assignment.Student.AssessmentAssignments.Add(assignment);
                assignment.Assessment.AssessmentAssignments.Add(assignment);
            }
            var validStudent = batch.Students.Select(x => new UserDto()
            {
                Id = x.Id,
                Email = x.Email,
                FullName = x.FullName
            }).ToList();
            await unitOfwork.SaveChangesAsync();
            _backgroundService.Enqueue<IEmailService>(emailService => emailService.SendAssessmentEmail(validStudent, "New Assessment", new AssessmentDto()
            {
                Title = assessment.Title,
                Description = assessment.Description,
                TechnologyStack = assessment.TechnologyStack.ToString(),
                DurationInMinutes = assessment.DurationInMinutes,
                StartDate = assessment.StartDate,
                EndDate = assessment.EndDate,
                PassingScore = assessment.PassingScore
            }));
            var reminderTime = assessment.StartDate.AddMinutes(-30);
            var delay = reminderTime - DateTime.UtcNow;
            if (delay > TimeSpan.Zero)
            {
                var template = tempService.GenerateAssessmentReminderTemplate(new AssessmentDto()
                {
                    Title = assessment.Title,
                    Description = assessment.Description,
                    TechnologyStack = assessment.TechnologyStack.ToString(),
                    DurationInMinutes = assessment.DurationInMinutes,
                    StartDate = assessment.StartDate,
                    EndDate = assessment.EndDate,
                    PassingScore = assessment.PassingScore
                });
                _backgroundService.Schedule<IEmailService>((emailService => emailService.SendBulkEmailAsync(validStudent, "Assessment Reminder", template)), reminderTime);
            }
            _leaderboardStore.Invalidate(batchId);

            // Return success response  
            return new BaseResponse<string>
            {
                Status = true,
                Message = "Assessment assigned to batch successfully.",
                Data = "Success"
            };
        } 
        public async Task<BaseResponse<string>> AssignAssessmentToBatchAsync(Guid batchId, Guid assessmentId)
        {
            if (assessmentId == Guid.Empty)
                throw new ApiException("Assessmentid cant be empty", 400, "Empty_Assessment_Id_");
            var assessment = await assessmentRepository.GetAsync(assessmentId);
            // Validate input parameters  
            if (batchId == Guid.Empty)
            {
                throw new ApiException("Invalid batch.", 400, "InvalidInput", null);
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
            var validStudent = batch.Students.Select(x => new UserDto()
            {
                Id = x.Id,
                Email = x.Email,
                FullName = x.FullName
            }).ToList();
            _backgroundService.Enqueue<IEmailService>(emailService => emailService.SendAssessmentEmail(validStudent, "New Assessment", new AssessmentDto()
            {
                Title = assessment.Title,
                Description = assessment.Description,
                TechnologyStack = assessment.TechnologyStack.ToString(),
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

        public async Task<BaseResponse<PaginationDto<BatchAnalytics>>> BatchAnalytics(PaginationRequest request)
        {
            var batches = await batchRepository.GetAllBatchesAsync(request);
            if (batches is null || !batches.Items.Any())
            {
                throw new ApiException("No batches found.", 404, "NoBatchesFound", null);
            }

            var batchAnalytics = batches.Items.Select(x => new BatchAnalytics()
            {
                Id = x.Id,
                Name = x.Name + x.BatchNumber,
                StudentCount = x.Students.Count,
                AverageScore = x.Students
                                 .Where(student => student.Submissions.Any())
                                 .Select(student => student.Submissions.Sum(sub => sub.TotalScore))
                                 .DefaultIfEmpty(0)
                                 .Average(),
                PassRate = x.Students.Count == 0 ? 0
                             : x.Students
                             .Where(student => student.Submissions.Any())
                             .Count(student => student.Submissions.Any(sub => sub.TotalScore >= sub.Assessment.PassingScore))
                         / (double)x.Students.Count * 100
                           }).ToList();

            var paginationDto = new PaginationDto<BatchAnalytics>
            {
                Items = batchAnalytics,
                TotalItems = batches.TotalItems,
                TotalPages = batches.TotalPages,
                CurrentPage = batches.CurrentPage,
                PageSize = request.PageSize,
                HasNextPage = batches.HasNextPage,
                HasPreviousPage = batches.HasPreviousPage
            };

            return new BaseResponse<PaginationDto<BatchAnalytics>>()
            {
                Data = paginationDto,
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
                BatchNumber = (short)model.BatchNumber,
                StartDate = model.StartDate
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
        public async Task<BaseResponse<BatchDetails>> GetBatchDetails(Guid batchId)
        {
            var batch = await batchRepository.GetBatchIdWithRelationship(batchId);
            if (batch is null)
            {
                throw new ApiException("Batch not found.", 404, "BatchNotFound", null);
            }
            var averageScore = batch.Students
                .Where(s => s.Submissions.Any())
                .Select(s => s.Submissions.Average(sub => sub.TotalScore))
                .DefaultIfEmpty(0)
                .Average();
            var passRate = batch.Students.Where(s => s.Submissions.Any())
                .Count(s => s.Submissions.Any(sub => sub.TotalScore >= sub.Assessment.PassingScore))
                / (double)batch.Students.Count * 100;
            var completionRate = batch.Students
                .Where(s => s.Submissions.Any())
                .Count(s => s.Submissions.Any(sub => sub.SubmittedAt != default(DateTime)))
                / (double)batch.Students.Count * 100;

            var avgTimeInSeconds = batch.Students.SelectMany(x => x.Submissions)
                .Where(s => s.SubmittedAt != default && s.Assessment.StartDate != default)
                .Average(s => (s.SubmittedAt - s.Assessment.StartDate).TotalSeconds);

            var avgTimeFormatted = TimeSpan.FromSeconds(avgTimeInSeconds).ToString(@"hh\:mm\:ss");
            var averageCompletionTime = avgTimeFormatted;
            var batchDetails = new BatchDetails
            {
                Id = batch.Id,
                Name = batch.Name + batch.BatchNumber,
                AverageScore = Math.Round(averageScore, 2),
                PassRate = Math.Round(passRate, 2),
                CompletionRate = Math.Round(completionRate, 2),
                AverageCompletionTime = averageCompletionTime,
                TotalStudents = batch.Students.Count,
                TotalAssessments = batch.AssessmentAssignments.Count
            };

            return new BaseResponse<BatchDetails>
            {
                Status = true,
                Message = "Batch details retrieved successfully.",
                Data = batchDetails
            };
        }

        public async Task<BaseResponse<BatchPerformanceTrendDto>> GetBatchPerformanceTrend(Guid? batchId)
        {
            if (batchId.HasValue && batchId.Value != Guid.Empty)
            {
                var batchExists = await batchRepository.CheckAsync(b => b.Id == batchId.Value);
                if (!batchExists)
                {
                    throw new ApiException("Batch not found.", 404, "BatchNotFound", null);
                }
            }

            var assessments = await assessmentRepository.GetAllAsync(
                a => !batchId.HasValue || a.BatchAssessment.Any(ba => ba.BatchId == batchId.Value) 
            );

            if (!assessments.Any())
            {
                throw new ApiException("No assessments found.", 404, "NoAssessmentsFound", null);
            }

            var labels = new List<string>();
            var scores = new List<double>();

            foreach (var assessment in assessments)
            {
                // 2. Correctly filter submissions for THIS assessment AND (if provided) THIS batch.
                var relevantSubmissions = assessment.Submissions
                    .Where(s => !batchId.HasValue || s.Student.BatchId == batchId.Value)
                    .ToList();

                if (relevantSubmissions.Any())
                {
                    labels.Add(assessment.Title);
                    scores.Add(Math.Round(relevantSubmissions.Average(s => s.TotalScore), 2));
                }
            }

            var dto = new BatchPerformanceTrendDto
            {
                Labels = labels,
                Scores = scores
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
        public async Task<BaseResponse<List<SubmissionStatsDto>>> GetBatchSubmissionStats(Guid batchId)
        {
            
            var batchExists = await batchRepository.CheckAsync(b => b.Id == batchId);
            if (!batchExists)
            {
                throw new ApiException("Batch not found.", 404, "BatchNotFound", null);
            }

            
            var assessmentsInBatch = await assessmentRepository.GetAllAsync(
               a => a.BatchAssessment.Any(ba => ba.BatchId == batchId)
            );


            if (!assessmentsInBatch.Any())
            {
                return new BaseResponse<List<SubmissionStatsDto>>
                {
                    Status = true,
                    Message = "No assessments were found for this batch.",
                    Data = new List<SubmissionStatsDto>()
                };
            }

           
            var submissionStats = assessmentsInBatch.Select(assessment =>
            {
                
                var assignedCount = assessment.AssessmentAssignments
                    .Count(aa => aa.Student?.BatchId == batchId);

                var submittedCount = assessment.Submissions
                    .Count(s => s.Student?.BatchId == batchId);

                return new SubmissionStatsDto
                {
                    AssessmentTitle = assessment.Title,
                    TotalAssigned = assignedCount,
                    TotalSubmitted = submittedCount
                };
            }).ToList();

            
            return new BaseResponse<List<SubmissionStatsDto>>
            {
                Status = true,
                Message = "Submission stats retrieved successfully.",
                Data = submissionStats
            };
        }
    }
}
