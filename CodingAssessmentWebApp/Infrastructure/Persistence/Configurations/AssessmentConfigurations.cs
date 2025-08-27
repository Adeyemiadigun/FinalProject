using Domain.Entitties;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class AssessmentConfigurations : IEntityTypeConfiguration<Assessment>
    {
        public void Configure(EntityTypeBuilder<Assessment> builder)
        {
            builder.ToTable("Assessment");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Title).IsRequired().HasMaxLength(200);
            builder.Property(x => x.Description).IsRequired()
                .HasMaxLength(500);
            builder.Property(a => a.TechnologyStack)
                .IsRequired()
                .HasConversion<string>();
            builder.Property(x => x.DurationInMinutes).IsRequired();
            builder.Property(x => x.StartDate).IsRequired()
                    .HasConversion(
                    v => v.ToUniversalTime(),
                    v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

            builder.Property(x => x.EndDate)
                .IsRequired()
                .HasConversion(
                        v => v.ToUniversalTime(),
                        v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
            builder.Property(x => x.Status).IsRequired();
            builder.Property(x => x.CreatedAt);
            builder.Property(x => x.PassingPercentage).IsRequired();
            builder.Property(x => x.InstructorId).IsRequired();
            builder.HasOne(a => a.Instructor)
                .WithMany(u => u.Assessments)
                .HasForeignKey(a => a.InstructorId);        
            builder.HasMany(a => a.Questions)
                .WithOne(a => a.Assessment)
                .HasForeignKey(x => x.AssessmentId);
            builder.HasMany(a => a.Submissions)
                .WithOne()
                .HasForeignKey(x => x.AssessmentId);

            builder.HasMany(x => x.AssessmentAssignments)
                .WithOne( x => x.Assessment)
                .HasForeignKey(x => x.AssessmentId);
            builder.HasMany(x => x.BatchAssessment)
                .WithOne(a => a.Assessment)
                .HasForeignKey(a => a.AssessmentId);
        }
    }
}
