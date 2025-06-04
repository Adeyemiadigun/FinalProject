using Domain.Entitties;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence
{
    public class ClhAssessmentAppDpContext(DbContextOptions<ClhAssessmentAppDpContext> options) : DbContext(options)
    {
       public DbSet<Answer> Answers => Set<Answer>();
        public DbSet<AnswerSubmission> AnswerSubmissions => Set<AnswerSubmission>();
        public DbSet<Assessment> Assessments => Set<Assessment>();
        public DbSet<AssessmentAssignment> AssessmentAssignments => Set<AssessmentAssignment>();
        public DbSet<Option> Options => Set<Option>();
        public DbSet<Question> Questions => Set<Question>();
        public DbSet<Submission> Submissions => Set<Submission>();
        public DbSet<TestCase> TestCases => Set<TestCase>();
        public DbSet<User> Users => Set<User>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("app");
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ClhAssessmentAppDpContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }
    }
}
