using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Domain.Entities;

namespace Infrastructure.Persistence.Configurations
{
    public class BatchAssessmentConfiguration : IEntityTypeConfiguration<BatchAssessment>
    {
        public void Configure(EntityTypeBuilder<BatchAssessment> builder)
        {
            builder.ToTable("BatchAssessment");
            builder.HasKey(x => x.Id);

            builder.HasOne(x => x.Assessment)
                   .WithMany(a => a.BatchAssessment)
                   .HasForeignKey(x => x.AssessmentId);

            builder.HasOne(x => x.Batch)
                   .WithMany(b => b.AssessmentAssignments)
                   .HasForeignKey(x => x.BatchId);
        }
    }

}
