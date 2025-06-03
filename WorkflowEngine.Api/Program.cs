using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using WorkflowEngine.Core.Interfaces;
using WorkflowEngine.Core.Services;
using WorkflowEngine.Core.Steps;
using WorkflowEngine.Core.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add CORS support for frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", builder =>
    {
        builder
            .WithOrigins("http://localhost:3000") // React dev server
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// Add Entity Framework with PostgreSQL
builder.Services.AddDbContext<WorkflowDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseNpgsql(connectionString);
    
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

// Add Hangfire with PostgreSQL
builder.Services.AddHangfire(configuration =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    configuration
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UsePostgreSqlStorage(c => c.UseNpgsqlConnection(connectionString), new PostgreSqlStorageOptions
        {
            QueuePollInterval = TimeSpan.FromSeconds(10),
            JobExpirationCheckInterval = TimeSpan.FromHours(1),
            CountersAggregateInterval = TimeSpan.FromMinutes(5),
            PrepareSchemaIfNecessary = true,
            TransactionSynchronisationTimeout = TimeSpan.FromMinutes(5)
        });
});

builder.Services.AddHangfireServer();

// Add Workflow Engine services
builder.Services.AddScoped<IWorkflowEngine, WorkflowEngine.Core.Services.WorkflowEngine>();

// Register workflow steps
builder.Services.AddScoped<EmailStep>();
builder.Services.AddScoped<LogStep>();
builder.Services.AddScoped<DelayStep>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Initialize database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<WorkflowDbContext>();
    context.Database.EnsureCreated();
}

app.UseHttpsRedirection();

// Use CORS
app.UseCors("AllowFrontend");

app.UseAuthorization();

// Add Hangfire Dashboard
app.UseHangfireDashboard("/hangfire");

app.MapControllers();

app.Run();
