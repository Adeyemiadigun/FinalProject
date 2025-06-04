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
        Task<bool> SendEmailAsync(string to, string subject, string body);

        // Fix for CS0246 and CS0080: Make the interface generic to allow the use of T with constraints
        Task<bool> SendResultEmailAsync(Submission submission, User user);

        Task<bool> SendBulkEmailAsync(ICollection<User> to, string subject, AssessmentDto assessment);
    }
}
