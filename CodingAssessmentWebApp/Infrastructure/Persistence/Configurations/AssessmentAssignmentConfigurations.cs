using Domain.Entitties;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class AssessmentAssignmentConfigurations : IEntityTypeConfiguration<AssessmentAssignment>
    {
        public void Configure(EntityTypeBuilder<AssessmentAssignment> builder)
        {
            builder.ToTable("AssessmentAssignment");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.AssignedAt)
                .IsRequired();

            builder.Property(x => x.EmailSent)
                .IsRequired();
            builder.Property(x => x.StudentId)
                .IsRequired();
            builder.Property(x => x.AssessmentId)
                .IsRequired();

            builder.HasOne(x => x.Assessment)
                .WithMany(a => a.AssessmentAssignments)
                .HasForeignKey(x => x.AssessmentId);

            builder.HasOne(x => x.Student)
                .WithMany(u => u.AssessmentAssignments)
                .HasForeignKey(x => x.StudentId);
        }
    }
}