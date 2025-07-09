using Application.Interfaces.Services;
using Hangfire;

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

            // Add more jobs here as needed
        }
    }

}
