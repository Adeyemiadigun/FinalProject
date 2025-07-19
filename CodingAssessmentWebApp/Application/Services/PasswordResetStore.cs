using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Interfaces.Services;
using Microsoft.Extensions.Caching.Memory;

namespace Application.Services
{
    public class PasswordResetService : IPasswordResetService
    {
        private readonly IMemoryCache _cache;
        private readonly TimeSpan _tokenExpiry = TimeSpan.FromMinutes(15);

        public PasswordResetService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public string GenerateResetToken(string email)
        {
            var token = Guid.NewGuid().ToString("N");

            _cache.Set($"reset-token:{email}", token, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = _tokenExpiry
            });

            return token;
        }

        public bool ValidateToken(string email, string submittedToken)
        {
            var key = $"reset-token:{email}";
            if (_cache.TryGetValue(key, out string storedToken))
            {
                if (storedToken == submittedToken)
                {
                    _cache.Remove(key); // Optional: one-time use
                    return true;
                }
            }
            return false;
        }
    }

}
