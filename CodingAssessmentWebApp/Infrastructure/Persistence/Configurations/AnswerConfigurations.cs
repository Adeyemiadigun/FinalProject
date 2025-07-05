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
            builder.Property(x => x.AnswerText).IsRequired().HasMaxLength(300);
            builder.Property(x => x.QuestionId).IsRequired();
            builder.HasOne(x => x.Question)
                .WithOne( q => q.Answer)
                .HasForeignKey<Answer>(x => x.QuestionId);

        }
    }
}
