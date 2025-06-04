using Hangfire;
using Hangfire.PostgreSql;
using Host.Configurations;
using Host.Middlewares;
using Infrastructure.Configurations;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.  

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi  
builder.Services.AddOpenApi();
// Register All the services needed  
builder.Services.AddServices(builder.Configuration);

builder.Services.AddHangfire(config =>
  config.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UsePostgreSqlStorage(builder.Configuration.GetConnectionString("HangfireConnection"))
);
builder.Services.Configure<HuggingFaceSettings>(
   builder.Configuration.GetSection("HuggingFace"));

// This lets you use BackgroundJob.Enqueue and dashboard  
builder.Services.AddHangfireServer(); // No changes needed here if the Hangfire package is correctly referenced  

var app = builder.Build();

// Configure the HTTP request pipeline.  
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.UseMiddleware<ExceptionMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
