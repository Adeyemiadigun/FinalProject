using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Services.AuthService
{
    public interface IRefreshTokenStore
    {
        Task SaveTokenAsync(Guid userId, string token);
        Task<(bool Isvalid, Guid UserId)> IsTokenValidAsync( string token);
        Task RemoveExpiredTokensAsync();
    }
}
