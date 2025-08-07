-- Initialize the database with proper permissions
-- This script runs when the PostgreSQL container starts for the first time

-- Grant all privileges to the jobappuser
GRANT ALL PRIVILEGES ON DATABASE jobappdb TO jobappuser;

-- Create extensions if needed
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Set up proper search path
ALTER DATABASE jobappdb SET search_path TO public;
