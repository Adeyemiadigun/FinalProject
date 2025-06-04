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
    public class TestCaseConfigurations : IEntityTypeConfiguration<TestCase>
    {
        public void Configure(EntityTypeBuilder<TestCase> builder)
        {
            builder.ToTable("TestCase");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Input).IsRequired();
            builder.Property(x => x.ExpectedOutput).IsRequired();
            builder.Property(x => x.Weight).IsRequired();
            builder.HasOne(x => x.Question)
                .WithMany(q => q.Tests)
                .HasForeignKey(x => x.QuestionId);
        }
    }
}
