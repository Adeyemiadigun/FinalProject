using Application.Interfaces.ExternalServices.AIProviderStrategy;
using Application.Interfaces.ExternalServices;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services.GradingStrategyInterfaces.Interfaces;
using Application.Interfaces.Services;
using Application.Services.GradingStrategy.Implementation;
using Application.Services;
using Infrastructure.Configurations;
using Infrastructure.ExternalServices.AIProviderStrategy;
using Infrastructure.ExternalServices;
using Infrastructure.Repositories;
using Application.Interfaces.Services.AuthService;
using Application.Services.AuthService;

namespace Host.Configurations
{
    public static class ServicesConfig
    {
        public static IServiceCollection AddServices(this IServiceCollection builder, IConfiguration configuration)
        {
            // Application Services
            builder.AddScoped<IAIQuestionService, AIQuestionService>();
            builder.AddScoped<IAssessmentService, AssessmentService>();
            builder.AddScoped<IQuestionService, QuestionService>();
            builder.AddScoped<ISubmissionService, SubmissionService>();
            builder.AddScoped<IDashboardService, DashboardService>();
            builder.AddScoped<IUserService, UserService>();
            builder.AddScoped<IGradingService, GradingService>();
            builder.AddScoped<ICurrentUser, CurrentUser>();
            builder.AddScoped<IAuthService, AuthService>();
            builder.AddScoped<IRefreshTokenStore, InMemoryRefreshTokenStore>();



            // Grading Strategy
            builder.AddScoped<IGradingStrategyFactory, GradingStrategyFactory>();
            builder.AddScoped<IGradingStrategy, McqGradingStrategy>();
            builder.AddScoped<IGradingStrategy, ObjectiveGradingStrategy>();

            // Repositories
            builder.AddScoped<IAssessmentRepository, AssessmentRepository>();
            builder.AddScoped<IQuestionRepository, QuestionRepository>();
            builder.AddScoped<ISubmissionRepository, SubmissionRepository>();
            builder.AddScoped<IUserRepository, UserRepository>();
            builder.AddScoped<IUnitOfWork, UnitOfWork>();

            // External Services
            builder.AddScoped<IEmailService, EmailService>();
            builder.AddScoped<IBackgroundService, BackgroundJobService>();

            builder.AddScoped<IAIProviderGateway, AIStrategyGateway>();
            builder.AddScoped<IPayloadBuider, PayloadBuilder>();
            return builder;
        }
    }
}
