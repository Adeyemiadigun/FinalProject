using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Dtos;
using Domain.Entitties;

namespace Application.Interfaces.Services
{
    public interface IGradingService
    {
        Task GradeSubmissionAndNotifyAsync(Guid submissionEntityId,Guid  studentId);
    }
}
