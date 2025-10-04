-- Task Management Module - Database Schema
-- PostgreSQL compatible schema for Task Management functionality

-- Create Task Type table
CREATE TABLE IF NOT EXISTS task_type (
    id SERIAL PRIMARY KEY,
    uid VARCHAR(50) UNIQUE NOT NULL,
    code VARCHAR(20) UNIQUE NOT NULL,
    name VARCHAR(100) NOT NULL,
    description TEXT,
    is_active BOOLEAN DEFAULT true,
    sort_order INTEGER DEFAULT 0,
    ss INTEGER,
    created_by VARCHAR(50),
    created_time TIMESTAMP,
    modified_by VARCHAR(50),
    modified_time TIMESTAMP,
    server_add_time TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    server_modified_time TIMESTAMP
);

-- Create Task Sub Type table
CREATE TABLE IF NOT EXISTS task_sub_type (
    id SERIAL PRIMARY KEY,
    uid VARCHAR(50) UNIQUE NOT NULL,
    code VARCHAR(20) UNIQUE NOT NULL,
    name VARCHAR(100) NOT NULL,
    description TEXT,
    task_type_id INTEGER NOT NULL REFERENCES task_type(id) ON DELETE CASCADE,
    is_active BOOLEAN DEFAULT true,
    sort_order INTEGER DEFAULT 0,
    ss INTEGER,
    created_by VARCHAR(50),
    created_time TIMESTAMP,
    modified_by VARCHAR(50),
    modified_time TIMESTAMP,
    server_add_time TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    server_modified_time TIMESTAMP
);

-- Create Task table
CREATE TABLE IF NOT EXISTS task (
    id SERIAL PRIMARY KEY,
    uid VARCHAR(50) UNIQUE NOT NULL,
    code VARCHAR(50) UNIQUE NOT NULL,
    title VARCHAR(200) NOT NULL,
    description TEXT,
    task_type_id INTEGER NOT NULL REFERENCES task_type(id),
    task_sub_type_id INTEGER REFERENCES task_sub_type(id),
    sales_org_id INTEGER NOT NULL,
    start_date DATE NOT NULL,
    end_date DATE NOT NULL,
    is_active BOOLEAN DEFAULT true,
    priority VARCHAR(20) DEFAULT 'Medium',
    status VARCHAR(20) DEFAULT 'Draft',
    task_data TEXT, -- JSON data for custom task-specific information
    ss INTEGER,
    created_by VARCHAR(50),
    created_time TIMESTAMP,
    modified_by VARCHAR(50),
    modified_time TIMESTAMP,
    server_add_time TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    server_modified_time TIMESTAMP,
    
    CONSTRAINT chk_task_dates CHECK (end_date > start_date),
    CONSTRAINT chk_task_priority CHECK (priority IN ('Low', 'Medium', 'High', 'Critical')),
    CONSTRAINT chk_task_status CHECK (status IN ('Draft', 'Active', 'Completed', 'Cancelled', 'OnHold'))
);

-- Create Task Assignment table
CREATE TABLE IF NOT EXISTS task_assignment (
    id SERIAL PRIMARY KEY,
    uid VARCHAR(50) UNIQUE NOT NULL,
    task_id INTEGER NOT NULL REFERENCES task(id) ON DELETE CASCADE,
    assigned_to_type VARCHAR(20) NOT NULL, -- 'User' or 'UserGroup'
    user_id INTEGER, -- Reference to user table (when assigned_to_type = 'User')
    user_group_id INTEGER, -- Reference to user group table (when assigned_to_type = 'UserGroup')
    status VARCHAR(20) DEFAULT 'Assigned',
    assigned_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    started_date TIMESTAMP,
    completed_date TIMESTAMP,
    notes TEXT,
    progress INTEGER DEFAULT 0, -- 0-100 percentage
    ss INTEGER,
    created_by VARCHAR(50),
    created_time TIMESTAMP,
    modified_by VARCHAR(50),
    modified_time TIMESTAMP,
    server_add_time TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    server_modified_time TIMESTAMP,
    
    CONSTRAINT chk_assignment_type CHECK (assigned_to_type IN ('User', 'UserGroup')),
    CONSTRAINT chk_assignment_status CHECK (status IN ('Pending', 'Assigned', 'InProgress', 'Completed', 'Cancelled', 'Overdue')),
    CONSTRAINT chk_assignment_progress CHECK (progress >= 0 AND progress <= 100),
    CONSTRAINT chk_assignment_user CHECK (
        (assigned_to_type = 'User' AND user_id IS NOT NULL AND user_group_id IS NULL) OR
        (assigned_to_type = 'UserGroup' AND user_group_id IS NOT NULL AND user_id IS NULL)
    )
);

-- Create indexes for better performance
CREATE INDEX IF NOT EXISTS idx_task_type_active ON task_type(is_active);
CREATE INDEX IF NOT EXISTS idx_task_type_sort ON task_type(sort_order);

CREATE INDEX IF NOT EXISTS idx_task_sub_type_task_type ON task_sub_type(task_type_id);
CREATE INDEX IF NOT EXISTS idx_task_sub_type_active ON task_sub_type(is_active);

CREATE INDEX IF NOT EXISTS idx_task_type ON task(task_type_id);
CREATE INDEX IF NOT EXISTS idx_task_sub_type ON task(task_sub_type_id);
CREATE INDEX IF NOT EXISTS idx_task_sales_org ON task(sales_org_id);
CREATE INDEX IF NOT EXISTS idx_task_dates ON task(start_date, end_date);
CREATE INDEX IF NOT EXISTS idx_task_status ON task(status);
CREATE INDEX IF NOT EXISTS idx_task_active ON task(is_active);
CREATE INDEX IF NOT EXISTS idx_task_created_time ON task(created_time);

CREATE INDEX IF NOT EXISTS idx_task_assignment_task ON task_assignment(task_id);
CREATE INDEX IF NOT EXISTS idx_task_assignment_user ON task_assignment(user_id);
CREATE INDEX IF NOT EXISTS idx_task_assignment_user_group ON task_assignment(user_group_id);
CREATE INDEX IF NOT EXISTS idx_task_assignment_status ON task_assignment(status);
CREATE INDEX IF NOT EXISTS idx_task_assignment_assigned_date ON task_assignment(assigned_date);

-- Insert default Task Types
INSERT INTO task_type (uid, code, name, description, is_active, sort_order, created_by, created_time) 
VALUES 
    (gen_random_uuid()::text, 'SURVEY', 'Survey', 'Survey related tasks', true, 1, 'SYSTEM', CURRENT_TIMESTAMP),
    (gen_random_uuid()::text, 'AUDIT', 'Audit', 'Audit and compliance tasks', true, 2, 'SYSTEM', CURRENT_TIMESTAMP),
    (gen_random_uuid()::text, 'TRAINING', 'Training', 'Training and development tasks', true, 3, 'SYSTEM', CURRENT_TIMESTAMP),
    (gen_random_uuid()::text, 'SALES', 'Sales', 'Sales related tasks', true, 4, 'SYSTEM', CURRENT_TIMESTAMP),
    (gen_random_uuid()::text, 'MARKETING', 'Marketing', 'Marketing and promotion tasks', true, 5, 'SYSTEM', CURRENT_TIMESTAMP),
    (gen_random_uuid()::text, 'MAINTENANCE', 'Maintenance', 'Store maintenance tasks', true, 6, 'SYSTEM', CURRENT_TIMESTAMP),
    (gen_random_uuid()::text, 'INVENTORY', 'Inventory', 'Inventory management tasks', true, 7, 'SYSTEM', CURRENT_TIMESTAMP),
    (gen_random_uuid()::text, 'REPORTING', 'Reporting', 'Reporting and analytics tasks', true, 8, 'SYSTEM', CURRENT_TIMESTAMP)
ON CONFLICT (code) DO NOTHING;

-- Insert default Task Sub Types
INSERT INTO task_sub_type (uid, code, name, description, task_type_id, is_active, sort_order, created_by, created_time)
SELECT 
    gen_random_uuid()::text, 
    'SURVEY_STORE', 
    'Store Survey', 
    'Store condition and compliance survey', 
    tt.id, 
    true, 
    1, 
    'SYSTEM', 
    CURRENT_TIMESTAMP
FROM task_type tt WHERE tt.code = 'SURVEY'
ON CONFLICT (code) DO NOTHING;

INSERT INTO task_sub_type (uid, code, name, description, task_type_id, is_active, sort_order, created_by, created_time)
SELECT 
    gen_random_uuid()::text, 
    'SURVEY_CUSTOMER', 
    'Customer Survey', 
    'Customer satisfaction survey', 
    tt.id, 
    true, 
    2, 
    'SYSTEM', 
    CURRENT_TIMESTAMP
FROM task_type tt WHERE tt.code = 'SURVEY'
ON CONFLICT (code) DO NOTHING;

INSERT INTO task_sub_type (uid, code, name, description, task_type_id, is_active, sort_order, created_by, created_time)
SELECT 
    gen_random_uuid()::text, 
    'AUDIT_FINANCIAL', 
    'Financial Audit', 
    'Financial records and transactions audit', 
    tt.id, 
    true, 
    1, 
    'SYSTEM', 
    CURRENT_TIMESTAMP
FROM task_type tt WHERE tt.code = 'AUDIT'
ON CONFLICT (code) DO NOTHING;

INSERT INTO task_sub_type (uid, code, name, description, task_type_id, is_active, sort_order, created_by, created_time)
SELECT 
    gen_random_uuid()::text, 
    'AUDIT_INVENTORY', 
    'Inventory Audit', 
    'Physical inventory count and verification', 
    tt.id, 
    true, 
    2, 
    'SYSTEM', 
    CURRENT_TIMESTAMP
FROM task_type tt WHERE tt.code = 'AUDIT'
ON CONFLICT (code) DO NOTHING;

-- Add comments for documentation
COMMENT ON TABLE task_type IS 'Master table for task categories';
COMMENT ON TABLE task_sub_type IS 'Sub-categories under each task type';
COMMENT ON TABLE task IS 'Main task entities with assignments and tracking';
COMMENT ON TABLE task_assignment IS 'Assignment of tasks to users or user groups';

COMMENT ON COLUMN task.task_data IS 'JSON field for storing task-specific custom data';
COMMENT ON COLUMN task_assignment.progress IS 'Completion percentage from 0 to 100';
COMMENT ON COLUMN task_assignment.assigned_to_type IS 'Indicates if assigned to User or UserGroup';