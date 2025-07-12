using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Dtos;

namespace Application.Interfaces.Services
{
    public interface ILeaderboardStore
    {
        Task<List<LeaderboardDto>> GetLeaderBoardByBatchId(Guid batchId);
        Task<bool> StoreLeaderboard(Guid batchId, List<LeaderboardDto> leaderboard);
        void Invalidate(Guid batchId);
    }

}
