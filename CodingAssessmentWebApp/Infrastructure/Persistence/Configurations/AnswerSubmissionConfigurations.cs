using Domain.Entitties;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class AnswerSubmissionConfigurations : IEntityTypeConfiguration<AnswerSubmission>
    {
        public void Configure(EntityTypeBuilder<AnswerSubmission> builder)
        {
            builder.ToTable("AnswerSubmission");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.SubmissionId)
                .IsRequired();
            builder.Property(x => x.QuestionId)
                .IsRequired();
            builder.Property(x => x.SubmittedAnswer)
                .HasMaxLength(200)
                .IsRequired();
            builder.Property(x => x.Score)
                .IsRequired();
            builder.Property(x => x.IsCorrect)
                .IsRequired();
            builder.Property(x => x.SelectedOptionIds)
    .HasColumnType("uuid[]");
            builder.HasOne(x => x.Submission)
                .WithMany(s => s.AnswerSubmissions)
                .HasForeignKey(x => x.SubmissionId);
            builder.HasOne(x => x.Question)
                .WithMany(q => q.AnswerSubmissions)
                .HasForeignKey(x => x.QuestionId);
        }
    }
}


