-- Create development database
CREATE DATABASE "WorkflowEngineDb_Dev" WITH OWNER = postgres;

-- Create workflow schema in main database
\c WorkflowEngineDb;
CREATE SCHEMA IF NOT EXISTS workflow;
ALTER SCHEMA workflow OWNER TO postgres;

-- Create workflow schema in development database
\c WorkflowEngineDb_Dev;
CREATE SCHEMA IF NOT EXISTS workflow;
ALTER SCHEMA workflow OWNER TO postgres;

-- Grant permissions
GRANT ALL PRIVILEGES ON SCHEMA workflow TO postgres;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA workflow TO postgres;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA workflow TO postgres;

-- Switch back to main database
\c WorkflowEngineDb;
GRANT ALL PRIVILEGES ON SCHEMA workflow TO postgres;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA workflow TO postgres;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA workflow TO postgres;

