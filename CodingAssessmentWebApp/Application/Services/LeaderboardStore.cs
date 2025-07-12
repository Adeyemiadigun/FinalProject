using Application.Dtos;
using Application.Interfaces.Services;
using Microsoft.Extensions.Caching.Memory;

namespace Application.Services
{
    public class LeaderboardStore : ILeaderboardStore
    {
        private readonly IMemoryCache _cache;
        private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(5);
        private const string AllLeaderboardKey = "leaderboards";

        public LeaderboardStore(IMemoryCache cache)
        {
            _cache = cache;
        }

        public async Task<List<LeaderboardDto>> GetAllLeaderboards()
        {
            _cache.TryGetValue(AllLeaderboardKey, out List<LeaderboardDto> leaderboards);
            return await Task.FromResult(leaderboards ?? new List<LeaderboardDto>());
        }

        public async Task<List<LeaderboardDto>> GetLeaderBoardByBatchId(Guid batchId)
        {
            var all = await GetAllLeaderboards();
            var filtered = all
                .Where(l => l.BatchId == batchId)
                .OrderByDescending(l => l.AvgScore)
                .ThenByDescending(l => l.HighestScore)
                .ToList();

            return filtered;
        }

        public Task<bool> StoreLeaderboard(List<LeaderboardDto> newEntries)
        {
            // Get existing full leaderboard
            _cache.TryGetValue(AllLeaderboardKey, out List<LeaderboardDto> existing);
            existing ??= new List<LeaderboardDto>();

            var newIds = newEntries.Select(e => e.Id).ToHashSet();
            existing.RemoveAll(e => newIds.Contains(e.Id));

            // Add new entries
            existing.AddRange(newEntries);

            _cache.Set(AllLeaderboardKey, existing, _cacheDuration);

            return Task.FromResult(true);
        }

        public void Invalidate()
        {
            _cache.Remove(AllLeaderboardKey);
        }
    }

}
