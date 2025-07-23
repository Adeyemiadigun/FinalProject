using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Dtos;
using Domain.Entitties;

namespace Application.Interfaces.Services
{
    public interface ITemplateService
    {
        string NewAssessmentTemplate(UserDto user, AssessmentDto assessment);
        string ResultTemplate(UserDto user, Submission submission);
        string GenerateAssessmentReminderTemplate(AssessmentDto assessment);
    }
}
