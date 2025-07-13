using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Dtos;
using Domain.Entitties;

namespace Application.Interfaces.ExternalServices
{
    public interface IEmailService
    {
        Task SendEmailAsync(UserDto to, string subject, string body);
        Task<bool> SendResultEmailAsync(Submission submission, UserDto user);

        Task<bool> SendBulkEmailAsync(ICollection<UserDto> to, string subject, AssessmentDto assessment);
    }
}
