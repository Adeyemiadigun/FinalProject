using Application.Interfaces.ExternalServices.AIProviderStrategy;
using Application.Interfaces.ExternalServices;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services.GradingStrategyInterfaces.Interfaces;
using Application.Interfaces.Services;
using Application.Services.GradingStrategy.Implementation;
using Application.Services;
using Infrastructure.ExternalServices.AIProviderStrategy;
using Infrastructure.ExternalServices;
using Infrastructure.Repositories;
using Application.Interfaces.Services.AuthService;
using Application.Services.AuthService;
using Application.Validation;
using FluentValidation;
using FluentValidation.AspNetCore;
using Infrastructure.Persistence;

namespace Host.Configurations
{
    public static class ServicesConfig
    {
        public static IServiceCollection AddServices(this IServiceCollection builder)
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
            builder.AddScoped<IBatchService, BatchService>();
            builder.AddScoped<IReminderService, ReminderService>();
            builder.AddScoped<IStudentProgressService, StudentProgressService>();
            builder.AddHttpClient();
            builder.AddHttpContextAccessor();
            builder.AddSingleton<IRefreshTokenStore, InMemoryRefreshTokenStore>();

            //FluentValidation
            builder.AddFluentValidationAutoValidation();
            builder.AddValidatorsFromAssemblyContaining<RegisterUserRequestModelValidator>();
            // Grading Strategy
            builder.AddScoped<IGradingStrategyFactory, GradingStrategyFactory>();
            builder.AddScoped<IGradingStrategy, McqGradingStrategy>();
            builder.AddScoped<IGradingStrategy, ObjectiveGradingStrategy>();
            builder.AddScoped<IGradingStrategy, CodeChallengeGradingStrategy>();
            // Repositories
            builder.AddScoped<IAssessmentRepository, AssessmentRepository>();
            builder.AddScoped<IQuestionRepository, QuestionRepository>();
            builder.AddScoped<ISubmissionRepository, SubmissionRepository>();
            builder.AddScoped<IUserRepository, UserRepository>();
            builder.AddScoped<IBatchRepository, BatchRepository>();
            builder.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.AddScoped<IStudentProgressRepository, StudentProgressRepository>();

            // External Services
            builder.AddScoped<IEmailService, EmailService>();
            builder.AddScoped<IBackgroundService, BackgroundJobService>();
            builder.AddScoped<ICodeExcution, Judge0CodeExecution>();
            builder.AddScoped<IJudge0LanguageService, Judge0LanguageService>();
            builder.AddSingleton<IJudge0LanguageStore, Judge0LanguageStore>();
            builder.AddScoped<IAIProviderGateway, AIStrategyGateway>();
            builder.AddScoped<IPayloadBuider, PayloadBuilder>();

            //Cors Policy

            builder.AddCors(o => o.AddPolicy("CLH_App", builder =>
            {
                builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
            }));
            return builder;
        }
    }
}
