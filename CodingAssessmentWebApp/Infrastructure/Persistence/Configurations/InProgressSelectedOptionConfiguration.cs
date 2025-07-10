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
    public class InProgressSelectedOptionConfiguration : IEntityTypeConfiguration<InProgressSelectedOption>
    {
        public void Configure(EntityTypeBuilder<InProgressSelectedOption> builder)
        {
            builder.HasKey(o => o.Id);
            builder.Property(o => o.OptionId);
            builder.Property(o => o.InProgressAnswerId);

            builder.HasOne(o => o.InProgressAnswer)
                .WithMany(o => o.SelectedOptions)
                .HasForeignKey(o => o.InProgressAnswerId);
        }
    }

}
 