using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Infrastructure.Persistence.Configurations
{
    public class SelectedOptionConfiguration : IEntityTypeConfiguration<SelectedOption>
    {
        public void Configure(EntityTypeBuilder<SelectedOption> builder)
        {
            builder.ToTable("SelectedOptions");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                   .IsRequired();

            builder.Property(x => x.AnswerSubmissionId)
                   .IsRequired();

            builder.Property(x => x.OptionId)
                   .IsRequired();

            builder.HasOne(x => x.AnswerSubmission)
                   .WithMany(a => a.SelectedOptions)
                   .HasForeignKey(x => x.AnswerSubmissionId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Option)
                   .WithMany()
                   .HasForeignKey(x => x.OptionId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
