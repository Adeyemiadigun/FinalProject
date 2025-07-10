using Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Configurations
{
    public class StudentAssessmentProgressConfiguration : IEntityTypeConfiguration<StudentAssessmentProgress>
    {
        public void Configure(EntityTypeBuilder<StudentAssessmentProgress> builder)
        {
            builder.HasKey(p => p.Id);

            builder.Property(p => p.StudentId)
                .IsRequired();
            builder.Property(p => p.StartedAt)
                .IsRequired();
                builder.Property(builder => builder.ElapsedTime)
                .IsRequired();
            builder.Property(p => p.LastSavedAt)
                .IsRequired();
            builder.Property(p => p.AssessmentId)
                .IsRequired();

            builder.HasIndex(p => new { p.StudentId, p.AssessmentId }).IsUnique();

            builder
                    .HasMany(p => p.Answers)
                    .WithOne(a => a.StudentAssessmentProgress)
                    .HasForeignKey(a => a.StudentAssessmentProgressId)
                    .OnDelete(DeleteBehavior.Cascade);

        builder.ToTable("StudentAssessmentProgresses");
        }
    }

}
