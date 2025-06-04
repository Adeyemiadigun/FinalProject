using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entitties;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class AnswerConfigurations : IEntityTypeConfiguration<Answer>
    {
        public void Configure(EntityTypeBuilder<Answer> builder)
        {
            builder.ToTable("Answer");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.AnswerText).IsRequired().HasMaxLength(200);
            builder.Property(x => x.QuestionId).IsRequired();
            builder.HasOne(x => x.Question)
                .WithOne( q => q.Answer)
                .HasForeignKey<Answer>(x => x.QuestionId);

        }
    }
}
