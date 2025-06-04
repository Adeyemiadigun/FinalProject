using Domain.Entitties;
using Domain.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class UserConfigurations : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("User");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.FullName).IsRequired();
            builder.Property(x => x.Email).IsRequired();
            builder.Property(x => x.PasswordHash).IsRequired();
            builder.Property(x => x.Role).IsRequired();
            builder.Property(x => x.CreatedAt).IsRequired();
            builder.Property(x => x.IsActive).IsRequired();

            builder.HasMany(x => x.Assessments)
                .WithOne()
                .HasForeignKey("InstructorId");
            builder.HasMany(x => x.Submissions)
                .WithOne()
                .HasForeignKey("StudentId");
            builder.HasMany(x => x.AssessmentAssignments)
                .WithOne(a => a.Student)
                .HasForeignKey(a => a.StudentId);

            builder.HasIndex(x => x.Email);

        }
    }
}