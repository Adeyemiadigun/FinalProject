using System.Collections.Concurrent;
using Application.Exceptions;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Interfaces.Services.AuthService;
using Domain.ValueObjects;

namespace Application.Services.AuthService
{
    public class InMemoryRefreshTokenStore : IRefreshTokenStore
    {
        private readonly ConcurrentDictionary<string, RefreshTokenEntry> _store = new();
        public async Task SaveTokenAsync(Guid userId, string token)
        {
            if (userId == Guid.Empty)
            {
                throw new ApiException("User ID cannot be empty", 400, "INVALID_USER_ID", null);
            }
            _store[token] = new RefreshTokenEntry { UserId = userId, ExpiresAt = DateTime.UtcNow.AddDays(1) };
        }
        public async Task<(bool Isvalid, Guid UserId)> IsTokenValidAsync(string token)
        {
            if (_store.TryGetValue(token, out var entry))
            {

               
                if (entry.ExpiresAt < DateTime.UtcNow)
                {
                    _store.TryRemove(token, out _);
                    throw new ApiException("Token has expired", 401, "TOKEN_EXPIRED", null);
                }
                return (true, entry.UserId);
            }

            throw new ApiException("Token not found", 404, "TOKEN_NOT_FOUND", null);
        }

        public async Task RemoveExpiredTokensAsync()
        {
            var now = DateTime.UtcNow;
            foreach (var kvp in _store)
            {
                if (kvp.Value.ExpiresAt < now)
                {
                    _store.TryRemove(kvp.Key, out _);
                }
            }
        }
        public async Task RemoveToken(string token)
        {
            if(!_store.TryRemove(token, out _))
            {
                throw new ApiException("Token not found", 404, "TOKEN_NOT_FOUND", null);
            }
        }
    }

}
