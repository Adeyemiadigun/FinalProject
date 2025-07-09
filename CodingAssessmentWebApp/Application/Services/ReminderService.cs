using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Interfaces.ExternalServices;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entitties;

namespace Application.Services
{
    public class ReminderService : IReminderService
    {
        private readonly IAssessmentRepository _assessmentRepo;
        private readonly IEmailService _emailService;

        public ReminderService(IAssessmentRepository assessmentRepo, IEmailService emailService)
        {
            _assessmentRepo = assessmentRepo;
            _emailService = emailService;
        }

        public async Task CheckAssessmentsWithoutQuestions()
        {
            var upcomingAssessments = await _assessmentRepo.GetAllAsync(a =>
                a.StartDate <= DateTime.UtcNow.AddHours(1) &&  // Adjust time window as needed
                !a.Questions.Any());

            foreach (var assessment in upcomingAssessments)
            {
                var template = EmptyAsseementTemplate(assessment);
                await _emailService.SendEmailAsync(assessment.Instructor, "Assessment Without Questions ALert",template);
            }
        }
        public string EmptyAsseementTemplate(Assessment assessment )
        {

            string template = $@"
                <html>
                  <body style='font-family: Arial, sans-serif; color: #333;'>
                    <h2>🚨 Assessment Missing Questions</h2>
                    <p>Hi <strong>{assessment.Instructor.FullName}</strong>,</p>
                    <p>
                      The assessment <strong>{assessment.Title}</strong> on <strong>{assessment.TechnologyStack}</strong> is scheduled to start soon,
                      but currently has <strong>no questions assigned</strong>.
                    </p>
                    <h4>Assessment Details:</h4>
                    <ul>
                      <li><strong>Start Date:</strong> {assessment.StartDate}</li>
                      <li><strong>End Date:</strong> {assessment.EndDate}</li>
                      <li><strong>Duration:</strong> {assessment.DurationInMinutes} minutes</li>
                      <li><strong>Time Remaining:</strong> {(assessment.StartDate - DateTime.UtcNow).Hours} hr left</li>
                    </ul>
                    <p style='color: #D9534F; font-weight: bold;'>
                      Please add questions to this assessment immediately to ensure it's ready for participants.
                    </p>
                    <p>
                      <a href='' style='background-color: #D9534F; color: #fff; padding: 10px 20px; text-decoration: none; border-radius: 4px;'>Manage Assessment</a>
                    </p>
                    <br/>
                    <p>If this was intentional, you may safely ignore this message.</p>
                    <p>Thanks,</p>
                    <p>The Assessment Platform</p>
                    <hr style='margin-top:30px;' />
                    <small style='color: gray;'>This is an automated system alert. Please take necessary action.</small>
                  </body>
                </html>";
            return template;
        }
    }

}
