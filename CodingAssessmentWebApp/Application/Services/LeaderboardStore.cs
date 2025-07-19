using Application.Dtos;
using Application.Exceptions;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Microsoft.Extensions.Caching.Memory;

namespace Application.Services
{
    public class LeaderboardStore : ILeaderboardStore
    {
        private readonly IMemoryCache _cache;
        private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(5);
        private readonly IUserRepository _userRepository;
        private readonly IBatchRepository _batchRepository;
        public LeaderboardStore(IMemoryCache cache, IUserRepository userRepository, IBatchRepository batchRepository)
        {
            _cache = cache;
            _batchRepository = batchRepository;
            _userRepository = userRepository;
        }

        private string GetBatchKey(Guid batchId) => $"leaderboard_batch_{batchId}";

        public async Task<List<LeaderboardDto>> GetLeaderBoardByBatchId(Guid batchId)
        {
            var key = GetBatchKey(batchId);
            _cache.TryGetValue(key, out List<LeaderboardDto> leaderboard);
            if (leaderboard == null || !leaderboard.Any())
            {
                var batch = await _batchRepository.GetBatchByIdAsync(batchId);
                if (batch == null)
                {
                    throw new ApiException("No Batch found", 404, "UserNotFound", null);
                }

                var assignedAssessmentIds = batch.AssessmentAssignments
                    .Select(x => x.AssessmentId)
                .ToList();

                var students = await _userRepository.GetAllAsync(s => s.BatchId == batch.Id);

                leaderboard = students.Select(student =>
                {
                    var submissions = student.Submissions
                        .Where(sub => assignedAssessmentIds.Contains(sub.AssessmentId))
                        .ToList();

                    var avgScore = submissions.Any() ? Math.Round(submissions.Average(s => s.TotalScore), 2) : 0;
                    var highestScore = submissions.Any() ? submissions.Max(s => s.TotalScore) : 0;

                    return new LeaderboardDto
                    {
                        Id = student.Id,
                        Name = student.FullName,
                        BatchId = batchId,
                        AvgScore = avgScore,
                        HighestScore = highestScore,
                        CompletedAssessments = submissions.Count
                    };
                })
                .OrderByDescending(l => l.AvgScore)
                .ThenByDescending(l => l.HighestScore)
                .ToList();
                await StoreLeaderboard(batchId, leaderboard);
            }


            return leaderboard ?? new List<LeaderboardDto>();
        }

        public Task<bool> StoreLeaderboard(Guid batchId, List<LeaderboardDto> leaderboard)
        {
            var key = GetBatchKey(batchId);
            _cache.Set(key, leaderboard, _cacheDuration);
            return Task.FromResult(true);
        }

        public void Invalidate(Guid batchId)
        {
            _cache.Remove(GetBatchKey(batchId));
        }
    }

}
