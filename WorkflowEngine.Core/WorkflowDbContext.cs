using Microsoft.EntityFrameworkCore;
using WorkflowEngine.Core.Entities;

namespace WorkflowEngine.Core.Data;

public class WorkflowDbContext : DbContext
{
    public WorkflowDbContext(DbContextOptions<WorkflowDbContext> options) : base(options)
    {
    }

    public DbSet<WorkflowDefinition> WorkflowDefinitions { get; set; }
    public DbSet<StepDefinition> StepDefinitions { get; set; }
    public DbSet<WorkflowStep> WorkflowSteps { get; set; }
    public DbSet<WorkflowExecution> WorkflowExecutions { get; set; }
    public DbSet<StepExecution> StepExecutions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure PostgreSQL-specific settings
        modelBuilder.HasDefaultSchema("workflow");

        // WorkflowDefinition configuration
        modelBuilder.Entity<WorkflowDefinition>(entity =>
        {
            entity.ToTable("workflow_definitions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.Name).HasColumnName("name").IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasColumnName("description").HasMaxLength(1000);
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            
            entity.HasIndex(e => e.Name).IsUnique().HasDatabaseName("ix_workflow_definitions_name");
            entity.HasIndex(e => e.IsActive).HasDatabaseName("ix_workflow_definitions_is_active");
            entity.HasIndex(e => e.CreatedAt).HasDatabaseName("ix_workflow_definitions_created_at");
        });

        // StepDefinition configuration
        modelBuilder.Entity<StepDefinition>(entity =>
        {
            entity.ToTable("step_definitions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.Name).HasColumnName("name").IsRequired().HasMaxLength(200);
            entity.Property(e => e.StepType).HasColumnName("step_type").IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasColumnName("description").HasMaxLength(1000);
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            
            entity.HasIndex(e => new { e.Name, e.StepType }).IsUnique().HasDatabaseName("ix_step_definitions_name_type");
            entity.HasIndex(e => e.StepType).HasDatabaseName("ix_step_definitions_step_type");
        });

        // WorkflowStep configuration
        modelBuilder.Entity<WorkflowStep>(entity =>
        {
            entity.ToTable("workflow_steps");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.WorkflowDefinitionId).HasColumnName("workflow_definition_id");
            entity.Property(e => e.StepDefinitionId).HasColumnName("step_definition_id");
            entity.Property(e => e.Order).HasColumnName("order");
            entity.Property(e => e.ConditionRules).HasColumnName("condition_rules").HasColumnType("jsonb");
            entity.Property(e => e.StepConfiguration).HasColumnName("step_configuration").HasColumnType("jsonb");
            entity.Property(e => e.DelayMinutes).HasColumnName("delay_minutes").HasDefaultValue(0);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            
            entity.HasOne(e => e.WorkflowDefinition)
                  .WithMany(e => e.Steps)
                  .HasForeignKey(e => e.WorkflowDefinitionId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.StepDefinition)
                  .WithMany(e => e.WorkflowSteps)
                  .HasForeignKey(e => e.StepDefinitionId)
                  .OnDelete(DeleteBehavior.Restrict);
                  
            entity.HasIndex(e => new { e.WorkflowDefinitionId, e.Order })
                  .IsUnique().HasDatabaseName("ix_workflow_steps_workflow_order");
            entity.HasIndex(e => e.WorkflowDefinitionId).HasDatabaseName("ix_workflow_steps_workflow_id");
        });

        // WorkflowExecution configuration
        modelBuilder.Entity<WorkflowExecution>(entity =>
        {
            entity.ToTable("workflow_executions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.WorkflowDefinitionId).HasColumnName("workflow_definition_id");
            entity.Property(e => e.Status).HasColumnName("status").HasConversion<int>();
            entity.Property(e => e.InputData).HasColumnName("input_data").HasColumnType("jsonb");
            entity.Property(e => e.OutputData).HasColumnName("output_data").HasColumnType("jsonb");
            entity.Property(e => e.ScheduledTime).HasColumnName("scheduled_time");
            entity.Property(e => e.StartedTime).HasColumnName("started_time");
            entity.Property(e => e.CompletedTime).HasColumnName("completed_time");
            entity.Property(e => e.ErrorMessage).HasColumnName("error_message").HasMaxLength(2000);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            
            entity.HasOne(e => e.WorkflowDefinition)
                  .WithMany(e => e.Executions)
                  .HasForeignKey(e => e.WorkflowDefinitionId)
                  .OnDelete(DeleteBehavior.Restrict);
                  
            entity.HasIndex(e => e.Status).HasDatabaseName("ix_workflow_executions_status");
            entity.HasIndex(e => e.WorkflowDefinitionId).HasDatabaseName("ix_workflow_executions_workflow_id");
            entity.HasIndex(e => e.ScheduledTime).HasDatabaseName("ix_workflow_executions_scheduled_time");
            entity.HasIndex(e => e.CreatedAt).HasDatabaseName("ix_workflow_executions_created_at");
        });

        // StepExecution configuration
        modelBuilder.Entity<StepExecution>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.WorkflowExecution)
                  .WithMany(e => e.StepExecutions)
                  .HasForeignKey(e => e.WorkflowExecutionId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.WorkflowStep)
                  .WithMany(e => e.Executions)
                  .HasForeignKey(e => e.WorkflowStepId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.Property(e => e.Status).HasConversion<int>();
        });
    }
}

