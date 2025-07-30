using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Dtos;
using Application.Interfaces.Services;
using Domain.Entitties;

namespace Application.Services
{
    public class TemplateService : ITemplateService
    {
        public string NewAssessmentTemplate(UserDto user, AssessmentDto assessment)
        {
            string template = $@"
                <html>
                  <body style='font-family: Arial, sans-serif; color: #333;'>
                    <h2>You're Invited to an Assessment</h2>
                    <p>Hi <strong>{user.FullName}</strong>,</p>
                    <p>
                      You have been invited to participate in the assessment <strong>{assessment.Title}</strong> on <strong>{assessment.TechnologyStack}</strong>.
                    </p>
                    <h4>Assessment Details:</h4>
                    <ul>
                      <li><strong>Start Date:</strong> {assessment.StartDate}</li>
                      <li><strong>End Date:</strong> {assessment.EndDate}</li>
                      <li><strong>Duration:</strong> {assessment.DurationInMinutes} minutes</li>
                    </ul>
                    <p>
                      Please click the link below to access the assessment:
                    </p>
                    <p>
                      <a href='assessmentLink' style='background-color: #4A90E2; color: #fff; padding: 10px 20px; text-decoration: none; border-radius: 4px;'>Start Assessment</a>
                    </p>
                    <br/>
                    <p>If you have any questions, feel free to reply to this email.</p>
                    <p>Good luck!</p>
                    <p>The Assessment Team</p>
                    <hr style='margin-top:30px;' />
                    <small style='color: gray;'>This is an automated message. Please do not reply directly to this email.</small>
                  </body>
                </html>";
            return template;
        }
        public string ResultTemplate(UserDto user, Submission submission)
        {
            string template = $@"
                <html>
                  <body style='font-family: Arial, sans-serif; color: #333;'>
                    <h2>Assessment Result</h2>
                    <p>Hi <strong>{user.FullName}</strong>,</p>
                    <p>
                      Your assessment submission has been evaluated. Here are your results:
                    </p>
                      <p>
                          Thank you for completing the assessment <strong>{submission.Assessment.Title}</strong> on <strong>{submission.Assessment.TechnologyStack}</strong>.
                       </p>
                    <h4>Submission Details:</h4>
                    <ul>
                      <li><strong>Submitted At:</strong> {submission.SubmittedAt}</li>
                      <li><strong>Total Score:</strong> {submission.TotalScore}</li>
                      <li><strong>Feedback:</strong> {submission.FeedBack}</li>
                    </ul>
                    <p>If you have any questions or need further clarification, feel free to reach out.</p>
                    <p>Best regards,</p>
                    <p>The Assessment Team</p>
                    <hr style='margin-top:30px;' />
                    <small style='color: gray;'>This is an automated message. Please do not reply directly to this email.</small>
                  </body>
                </html>";
            return template;
        }
        public string GenerateAssessmentReminderTemplate(AssessmentDto assessment)
        {
            string template = $@"
                <html>
                  <body style='font-family: Arial, sans-serif; color: #333; padding: 20px;'>
                    <h2 style='color: #4A90E2;'>📢 Assessment Reminder</h2>
                    <p>Hi ,</p>

                    <p>
                      This is a reminder that you have an upcoming assessment titled
                      <strong>{assessment.Title}</strong> in <strong>{assessment.TechnologyStack}</strong>.
                    </p>

                    <h4>🗓 Assessment Details:</h4>
                    <ul>
                      <li><strong>Start Date:</strong> {assessment.StartDate:dddd, MMMM d, yyyy – h:mm tt}</li>
                      <li><strong>End Date:</strong> {assessment.EndDate:dddd, MMMM d, yyyy – h:mm tt}</li>
                      <li><strong>Duration:</strong> {assessment.DurationInMinutes} minutes</li>
                    </ul>

                    <p>
                      Click the button below to begin your assessment when the time starts:
                    </p>


                    <p>If you have any questions, feel free to reach out to support.</p>

                    <p>Good luck!<br/><strong>– The Assessment Team</strong></p>

                    <hr style='margin-top: 30px;' />
                    <p style='font-size: 12px; color: gray;'>This is an automated message. Please do not reply directly to this email.</p>
                  </body>
                </html>";

            return template;
        }

    }
}
