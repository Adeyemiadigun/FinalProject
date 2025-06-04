using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entitties;
using Domain.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class QuestionConfigurations: IEntityTypeConfiguration<Question>
    {
        public void Configure(EntityTypeBuilder<Question> builder)
        {
            builder.ToTable("Question");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.AssessmentId).IsRequired();
            builder.Property(x => x.QuestionText).IsRequired();
            builder.Property(x => x.QuestionType).IsRequired();
            builder.Property(x => x.Marks).IsRequired();
            builder.Property(x => x.Order).IsRequired();
            builder.HasMany(x => x.Options)
                .WithOne(o => o.Question)
                .HasForeignKey(o => o.QuestionId);
            builder.HasMany(x => x.Tests)
                .WithOne(t => t.Question)
                .HasForeignKey(t => t.QuestionId);
            builder.HasMany(x => x.AnswerSubmissions)
                .WithOne(a => a.Question)
                .HasForeignKey(a => a.QuestionId);
            builder.HasOne(x => x.Answer)
                .WithOne(a => a.Question)
                .HasForeignKey<Answer>(x => x.QuestionId);
        }
    }
}
