using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Configurations
{
    public class InProgressAnswerConfiguration : IEntityTypeConfiguration<InProgressAnswer>
    {
        public void Configure(EntityTypeBuilder<InProgressAnswer> builder)
        {
            builder.HasKey(a => a.Id);

            builder.Property(a => a.QuestionId)
                .IsRequired();

            builder.Property(a => a.AnswerText)
                .HasMaxLength(5000); // adjust as needed
            builder.Property(a => a.StudentAssessmentProgressId)
                .IsRequired();

            builder.HasOne(x => x.StudentAssessmentProgress)
                 .WithMany(X => X.Answers)
                 .HasForeignKey(x => x.StudentAssessmentProgressId);

            

            builder.ToTable("InProgressAnswers");
        }
    }

}
