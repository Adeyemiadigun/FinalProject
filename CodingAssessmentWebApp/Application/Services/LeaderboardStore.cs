using Application.Dtos;
using Application.Interfaces.Services;
using Microsoft.Extensions.Caching.Memory;

namespace Application.Services
{
    public class LeaderboardStore : ILeaderboardStore
    {
        private readonly IMemoryCache _cache;
        private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(5);

        public LeaderboardStore(IMemoryCache cache)
        {
            _cache = cache;
        }

        private string GetBatchKey(Guid batchId) => $"leaderboard_batch_{batchId}";

        public Task<List<LeaderboardDto>> GetLeaderBoardByBatchId(Guid batchId)
        {
            var key = GetBatchKey(batchId);
            _cache.TryGetValue(key, out List<LeaderboardDto> leaderboard);
            return Task.FromResult(leaderboard ?? new List<LeaderboardDto>());
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
