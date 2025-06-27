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
            var id = Guid.Parse(_http.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
            if (id == Guid.Empty)
            {
                throw new ApiException("User ID is invalid.", (int)HttpStatusCode.BadRequest, "InvalidUserId", null);
            }
            return id;
        }
    }
}
