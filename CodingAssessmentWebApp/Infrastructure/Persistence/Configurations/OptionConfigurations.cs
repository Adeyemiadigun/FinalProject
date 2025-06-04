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
    public class OptionConfigurations : IEntityTypeConfiguration<Option>
    {
        public void Configure(EntityTypeBuilder<Option> builder)
        {
            builder.ToTable("Option");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.QuestionId).IsRequired();
            builder.Property(x => x.OptionText).IsRequired();
            builder.Property(x => x.IsCorrect).IsRequired();
            builder.HasOne(x => x.Question)
                .WithMany(x => x.Options)
                .HasForeignKey(x => x.QuestionId);
        }
    }
}