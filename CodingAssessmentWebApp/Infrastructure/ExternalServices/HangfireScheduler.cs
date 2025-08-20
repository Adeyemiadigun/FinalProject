using Application.Interfaces.Services;
using Application.Services;
using Hangfire;
using Hangfire.PostgreSql.Properties;

namespace Infrastructure.ExternalServices
{
    public static class HangfireJobScheduler
    {
        public static void RegisterRecurringJobs()
        {
            RecurringJob.AddOrUpdate<IReminderService>(
                "check-assessments-without-questions",
                service => service.CheckAssessmentsWithoutQuestions(),
                Cron.Daily()
            );
          
            //RecurringJob.AddOrUpdate<IMissedSubmissionScoringService>(
            // "score-missed-submissions",
            // service => service.RunAsync(),
            //Cron.Hourly); // Adjust as needed


            // Add more jobs here as needed
        }
    }

}
