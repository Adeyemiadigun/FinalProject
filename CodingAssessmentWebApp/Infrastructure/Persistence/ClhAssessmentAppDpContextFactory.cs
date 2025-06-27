using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Persistence
{
    public class ClhAssessmentAppDpContextFactory : IDesignTimeDbContextFactory<ClhAssessmentAppDpContext>
    {
        public ClhAssessmentAppDpContext CreateDbContext(string[] args)
        {
            var basePath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "../Host"));
            Console.WriteLine($"EF Design-time base path resolved to: {basePath}");

            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
            var devSettingsPath = Path.Combine(basePath, "appsettings.Development.json");

            Console.WriteLine(File.Exists(devSettingsPath)
                ? "✔ appsettings.Development.json FOUND"
                : "❌ appsettings.Development.json MISSING");

            var config = new ConfigurationBuilder()
                .SetBasePath(basePath) // ✅ This is the fix
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile($"appsettings.{environment}.json", optional: true)
                .Build();

            var connectionString = config.GetConnectionString("DefaultConnection");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("❌ Could not find 'DefaultConnection' in configuration.");
            }

            var optionsBuilder = new DbContextOptionsBuilder<ClhAssessmentAppDpContext>();
            optionsBuilder.UseNpgsql(connectionString);

            return new ClhAssessmentAppDpContext(optionsBuilder.Options);
        }
    }

}
