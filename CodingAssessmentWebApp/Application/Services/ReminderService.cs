using Application.Dtos;
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

            if (upcomingAssessments is null || !upcomingAssessments.Any())
                return;
            foreach (var assessment in upcomingAssessments)
            {
                var template = EmptyAsseementTemplate(assessment);
                var instructor = assessment.Instructor;
                if (instructor is null)
                    continue;
                var instructorDto = new UserDto()
                {
                    FullName = instructor.FullName,
                    Email = instructor.Email
                };
                await _emailService.SendEmailAsync(instructorDto, "Assessment Without Questions ALert",template);
            }
        }
        public async Task AssessmentReminder(Guid assessmenId)
        {
            var assessment = await _assessmentRepo.GetAsync(assessmenId);

        }
        private string EmptyAsseementTemplate(Assessment assessment)
        {
            var instructorName = assessment.Instructor?.FullName ?? "Instructor";
            var title = assessment.Title ?? "Untitled";
            var tech = assessment.TechnologyStack.ToString() ?? "Unknown";
            var start = assessment.StartDate;
            var end = assessment.EndDate;
            var duration = assessment.DurationInMinutes;

            var timeRemaining = (start - DateTime.UtcNow).TotalMinutes;
            var remainingDisplay = timeRemaining > 60
                ? $"{(int)(timeRemaining / 60)} hr"
                : $"{(int)timeRemaining} min";

            return $@"
        <html>
        <body style='font-family: Arial, sans-serif; color: #333;'>
            <h2>🚨 Assessment Missing Questions</h2>
            <p>Hi <strong>{instructorName}</strong>,</p>
            <p>
              The assessment <strong>{title}</strong> on <strong>{tech}</strong> is scheduled to start soon,
              but currently has <strong>no questions assigned</strong>.
            </p>
            <h4>Assessment Details:</h4>
            <ul>
              <li><strong>Start Date:</strong> {start}</li>
              <li><strong>End Date:</strong> {end}</li>
              <li><strong>Duration:</strong> {duration} minutes</li>
              <li><strong>Time Remaining:</strong> {remainingDisplay} left</li>
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
        }

    }

}
