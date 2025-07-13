using System.Net;
using System.Security.Claims;
using Application.Exceptions;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;

namespace Application.Services
{
    public class CurrentUser : ICurrentUser
    {
        private readonly IHttpContextAccessor _http;

        public CurrentUser(IHttpContextAccessor http)
        {
            _http = http;
        }

        public Guid GetCurrentUserId()
        {
            var id = _http.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(id) || !Guid.TryParse(id, out var parsed) || parsed == Guid.Empty)
            {
                throw new ApiException("User ID is invalid or missing.", (int)HttpStatusCode.BadRequest, "InvalidUserId", null);
            }
            return parsed;
        }
    }
}
