using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkflowEngine.Core.Migrations
{
    /// <inheritdoc />
    public partial class UpdateStepExecutionsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "workflow");

            migrationBuilder.CreateTable(
                name: "step_definitions",
                schema: "workflow",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    step_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Configuration = table.Column<string>(type: "text", nullable: true),
                    InputSchema = table.Column<string>(type: "text", nullable: true),
                    OutputSchema = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_step_definitions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "workflow_definitions",
                schema: "workflow",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workflow_definitions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "workflow_executions",
                schema: "workflow",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    workflow_definition_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    input_data = table.Column<string>(type: "jsonb", nullable: true),
                    output_data = table.Column<string>(type: "jsonb", nullable: true),
                    scheduled_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    started_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    completed_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    error_message = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workflow_executions", x => x.id);
                    table.ForeignKey(
                        name: "FK_workflow_executions_workflow_definitions_workflow_definitio~",
                        column: x => x.workflow_definition_id,
                        principalSchema: "workflow",
                        principalTable: "workflow_definitions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "workflow_steps",
                schema: "workflow",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    workflow_definition_id = table.Column<Guid>(type: "uuid", nullable: false),
                    step_definition_id = table.Column<Guid>(type: "uuid", nullable: false),
                    order = table.Column<int>(type: "integer", nullable: false),
                    condition_rules = table.Column<string>(type: "jsonb", nullable: true),
                    delay_minutes = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    step_configuration = table.Column<string>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workflow_steps", x => x.id);
                    table.ForeignKey(
                        name: "FK_workflow_steps_step_definitions_step_definition_id",
                        column: x => x.step_definition_id,
                        principalSchema: "workflow",
                        principalTable: "step_definitions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_workflow_steps_workflow_definitions_workflow_definition_id",
                        column: x => x.workflow_definition_id,
                        principalSchema: "workflow",
                        principalTable: "workflow_definitions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "step_executions",
                schema: "workflow",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkflowExecutionId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkflowStepId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    InputData = table.Column<string>(type: "text", nullable: true),
                    OutputData = table.Column<string>(type: "text", nullable: true),
                    ScheduledTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    StartedTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    RetryCount = table.Column<int>(type: "integer", nullable: false),
                    MaxRetries = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_step_executions", x => x.id);
                    table.ForeignKey(
                        name: "FK_step_executions_workflow_executions_WorkflowExecutionId",
                        column: x => x.WorkflowExecutionId,
                        principalSchema: "workflow",
                        principalTable: "workflow_executions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_step_executions_workflow_steps_WorkflowStepId",
                        column: x => x.WorkflowStepId,
                        principalSchema: "workflow",
                        principalTable: "workflow_steps",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_step_definitions_name_type",
                schema: "workflow",
                table: "step_definitions",
                columns: new[] { "name", "step_type" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_step_definitions_step_type",
                schema: "workflow",
                table: "step_definitions",
                column: "step_type");

            migrationBuilder.CreateIndex(
                name: "IX_step_executions_WorkflowExecutionId",
                schema: "workflow",
                table: "step_executions",
                column: "WorkflowExecutionId");

            migrationBuilder.CreateIndex(
                name: "IX_step_executions_WorkflowStepId",
                schema: "workflow",
                table: "step_executions",
                column: "WorkflowStepId");

            migrationBuilder.CreateIndex(
                name: "ix_workflow_definitions_created_at",
                schema: "workflow",
                table: "workflow_definitions",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_workflow_definitions_is_active",
                schema: "workflow",
                table: "workflow_definitions",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_workflow_definitions_name",
                schema: "workflow",
                table: "workflow_definitions",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_workflow_executions_created_at",
                schema: "workflow",
                table: "workflow_executions",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_workflow_executions_scheduled_time",
                schema: "workflow",
                table: "workflow_executions",
                column: "scheduled_time");

            migrationBuilder.CreateIndex(
                name: "ix_workflow_executions_status",
                schema: "workflow",
                table: "workflow_executions",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_workflow_executions_workflow_id",
                schema: "workflow",
                table: "workflow_executions",
                column: "workflow_definition_id");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_steps_step_definition_id",
                schema: "workflow",
                table: "workflow_steps",
                column: "step_definition_id");

            migrationBuilder.CreateIndex(
                name: "ix_workflow_steps_workflow_id",
                schema: "workflow",
                table: "workflow_steps",
                column: "workflow_definition_id");

            migrationBuilder.CreateIndex(
                name: "ix_workflow_steps_workflow_order",
                schema: "workflow",
                table: "workflow_steps",
                columns: new[] { "workflow_definition_id", "order" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "step_executions",
                schema: "workflow");

            migrationBuilder.DropTable(
                name: "workflow_executions",
                schema: "workflow");

            migrationBuilder.DropTable(
                name: "workflow_steps",
                schema: "workflow");

            migrationBuilder.DropTable(
                name: "step_definitions",
                schema: "workflow");

            migrationBuilder.DropTable(
                name: "workflow_definitions",
                schema: "workflow");
        }
    }
}
