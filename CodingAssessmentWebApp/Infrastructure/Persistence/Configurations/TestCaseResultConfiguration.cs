using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class TestCaseResultConfiguration : IEntityTypeConfiguration<TestCaseResult>
{
    public void Configure(EntityTypeBuilder<TestCaseResult> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Input)
            .IsRequired()
            .HasMaxLength(5000); 

        builder.Property(x => x.ExpectedOutput)
            .IsRequired()
            .HasMaxLength(5000);

        builder.Property(x => x.ActualOutput)
            .HasMaxLength(5000);

        builder.Property(x => x.Passed)
            .IsRequired();

        builder.Property(x => x.EarnedWeight)
            .IsRequired()
            .HasPrecision(5, 2); 

        builder.HasOne(x => x.AnswerSubmission)
            .WithMany(x => x.TestCaseResults)
            .HasForeignKey(x => x.AnswerSubmissionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.ToTable("TestCaseResults");
    }
}

