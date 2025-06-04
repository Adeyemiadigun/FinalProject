using Domain.Entitties;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class SubmissionConfigurations : IEntityTypeConfiguration<Submission>
    {
        public void Configure(EntityTypeBuilder<Submission> builder)
        {
            builder.ToTable("Submission");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.AssessmentId).IsRequired();
            builder.Property(x => x.StudentId).IsRequired();
            builder.Property(x => x.SubmittedAt).IsRequired();
            builder.Property(x => x.TotalScore).IsRequired();
            builder.Property(x => x.FeedBack);
            builder.HasOne(x =>x.Assessment)
                .WithMany(a => a.Submissions)
                .HasForeignKey(x => x.AssessmentId);
            builder.HasOne(x => x.Student)
                .WithMany(u => u.Submissions)
                .HasForeignKey(x => x.StudentId);
            builder.HasMany(x => x.AnswerSubmissions)
                .WithOne(a => a.Submission)
                .HasForeignKey(x => x.SubmissionId);
        }
    }
}