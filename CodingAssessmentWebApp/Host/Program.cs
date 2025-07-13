using System.Text;
using Hangfire;
using Hangfire.PostgreSql;
using Host.Configurations;
using Host.Middlewares;
using Infrastructure.Configurations;
using Infrastructure.ExternalServices;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.  

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi  
builder.Services.AddOpenApi();
// Register All the services needed  
builder.Services.AddServices();
builder.Services.AddDbContext<ClhAssessmentAppDpContext>(config => config.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
//Api versioning

    builder.Services.AddApiVersioning(options =>
    {
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.ReportApiVersions = true;

        
        options.ApiVersionReader = ApiVersionReader.Combine(
            new QueryStringApiVersionReader("api-version"),
            new HeaderApiVersionReader("X-Version"),
            new UrlSegmentApiVersionReader()
        );
    });
    builder.Services.AddVersionedApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV"; // Use v1, v1.0, etc.
        options.SubstituteApiVersionInUrl = true; // 🔥 This is the fix you need
    });
if (!builder.Environment.IsEnvironment("DesignTime"))
{
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddHttpClient();
    // Add services that require those above...
}


builder.Services.AddHangfire(config =>
  config.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UsePostgreSqlStorage(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// Register the AI settings configuration
builder.Services.Configure<AISettings>(
   builder.Configuration.GetSection("AISettings"));
builder.Services.Configure<PayloadTemplateSettings>(
   builder.Configuration.GetSection("PayloadTemplate"));

builder.Services.Configure<JwtSettings>(
   builder.Configuration.GetSection("JwtSettings"));

// This lets you use BackgroundJob.Enqueue and dashboard  
builder.Services.AddHangfireServer(); 
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });

    // ✅ Add JWT Bearer Auth
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme (Example: 'Bearer 12345abcdef')",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            new string[] {}
        }
    });
});
builder.Services.AddAuthentication(config =>
{
    config.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    config.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}
).AddJwtBearer(config =>
{
    config.SaveToken = true;
    config.Authority = "https://your-auth-server.com";
    config.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        ValidateIssuer = true,
        ValidateAudience = true,
        ClockSkew = TimeSpan.Zero,
        ValidIssuer = builder.Configuration.GetSection("JwtSettings")["issuer"],
        ValidAudience = builder.Configuration.GetSection("JwtSettings")["audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetSection("JwtSettings")["key"])),

    };
});


var app = builder.Build();
app.UseHangfireServer();
app.UseHangfireDashboard();
HangfireJobScheduler.RegisterRecurringJobs();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(); 
}

// Configure the HTTP request pipeline.  
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.UseCors("CLH_App");

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseMiddleware<ExceptionMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.Run();
