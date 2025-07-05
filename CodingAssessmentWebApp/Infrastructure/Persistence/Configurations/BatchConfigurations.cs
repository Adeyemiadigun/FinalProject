using Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Configurations
{
    public class BatchConfiguration : IEntityTypeConfiguration<Batch>
    {
        public void Configure(EntityTypeBuilder<Batch> builder)
        {
            builder.ToTable("Batch");
            builder.HasKey(b => b.Id);

            builder.Property(b => b.Name)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(b => b.BatchNumber)
                   .IsRequired();
            builder.Property(b => b.StartDate)
                     .IsRequired();
            builder.Property(b => b.EndDate)
                     .IsRequired(false); // Nullable for EndDate

            builder.Property(b => b.CreatedAt)
                   .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(b => b.UpdatedAt)
                   .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.HasMany(b => b.Students)
                   .WithOne(a => a.Batch) 
                   .HasForeignKey(a => a.BatchId)
                   .OnDelete(DeleteBehavior.Cascade);
            builder.HasMany(b => b.AssessmentAssignments)
                   .WithOne(a => a.Batch) 
                   .HasForeignKey(a => a.BatchId) 
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }

}
