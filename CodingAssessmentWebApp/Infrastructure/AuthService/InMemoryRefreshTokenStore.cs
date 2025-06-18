using System.Collections.Concurrent;
using Application.Exceptions;
using Application.Interfaces.Services;
using Application.Interfaces.Services.AuthService;
using Domain.ValueObjects;

namespace Application.Services.AuthService
{
    public class InMemoryRefreshTokenStore(ICurrentUser _currentUser) : IRefreshTokenStore
    {
        private readonly ConcurrentDictionary<string, RefreshTokenEntry> _store = new();
        public Task SaveTokenAsync(Guid userId, string token)
        {
            if (userId == Guid.Empty)
            {
                throw new ApiException("User ID cannot be empty", 400, "INVALID_USER_ID", null);
            }
            _store[token] = new RefreshTokenEntry { UserId = userId, ExpiresAt = DateTime.UtcNow.AddDays(1) };
            return Task.CompletedTask;
        }
        public Task<(bool Isvalid, Guid UserId)> IsTokenValidAsync(string token)
        {
            var currentUserId = _currentUser.GetCurrentUserId();
            if (_store.TryGetValue(token, out var entry))
            {
                if (entry.UserId != currentUserId)
                {
                    throw new ApiException("Token does not belong to the current user", 403, "INVALID_TOKEN_USER", null);
                }
                if (entry.ExpiresAt < DateTime.UtcNow)
                {
                    _store.TryRemove(token, out _);
                    throw new ApiException("Token has expired", 401, "TOKEN_EXPIRED", null);
                }
                return Task.FromResult((_store.Keys.Any(x => x.Equals(token)), entry.UserId));
            }

            throw new ApiException("Token not found", 404, "TOKEN_NOT_FOUND", null);
        }

        public Task RemoveExpiredTokensAsync()
        {
            var now = DateTime.UtcNow;
            foreach (var kvp in _store)
            {
                if (kvp.Value.ExpiresAt < now)
                {
                    _store.TryRemove(kvp.Key, out _);
                }
            }
            return Task.CompletedTask;
        }
    }

}
