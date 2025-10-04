-- Sample data insertion script for ROTA activities
-- This script inserts activities for current and next week

-- Helper function to generate UUID (SQLite doesn't have UUID function)
WITH RECURSIVE uuid_generate(uuid, counter) AS (
  SELECT hex(randomblob(4)) || '-' || 
         hex(randomblob(2)) || '-' || 
         '4' || substr(hex(randomblob(2)), 2) || '-' || 
         substr('89AB', abs(random() % 4) + 1, 1) || 
         substr(hex(randomblob(2)), 2) || '-' || 
         hex(randomblob(6))
  , 1
  UNION ALL
  SELECT hex(randomblob(4)) || '-' || 
         hex(randomblob(2)) || '-' || 
         '4' || substr(hex(randomblob(2)), 2) || '-' || 
         substr('89AB', abs(random() % 4) + 1, 1) || 
         substr(hex(randomblob(2)), 2) || '-' || 
         hex(randomblob(6))
  , counter + 1
  FROM uuid_generate
  WHERE counter < 14
)

-- Insert data for current week and next week
INSERT INTO rota_activity (
    uid, 
    ss,
    created_by,
    created_time,
    modified_by,
    modified_time,
    server_add_time,
    server_modified_time,
    job_position_uid,
    rota_date,
    rota_activity_name
)
SELECT 
    uuid as uid,
    1 as ss,
    'SYSTEM' as created_by,
    datetime('now') as created_time,
    'SYSTEM' as modified_by,
    datetime('now') as modified_time,
    datetime('now') as server_add_time,
    datetime('now') as server_modified_time,
    '12345' as job_position_uid, -- Replace with actual job_position_uid
    CASE (counter - 1) / 7
        WHEN 0 THEN -- Current week
            date('now', 'weekday 0', '-' || (strftime('%w', 'now') - 1) || ' days', '+' || ((counter - 1) % 7) || ' days')
        ELSE -- Next week
            date('now', 'weekday 0', '-' || (strftime('%w', 'now') - 1) || ' days', '+' || ((counter - 1) % 7 + 7) || ' days')
    END as rota_date,
    CASE (counter - 1) % 7
        WHEN 0 THEN 'General Work'
        WHEN 1 THEN 'Store Visit'
        WHEN 2 THEN 'Training'
        WHEN 3 THEN 'Week Off'
        WHEN 4 THEN 'Market Survey'
        WHEN 5 THEN 'Team Meeting'
        WHEN 6 THEN 'Report Writing'
    END as rota_activity_name
FROM uuid_generate;

-- Sample query to verify the inserted data:
-- SELECT 
--     rota_date,
--     rota_activity_name,
--     created_time
-- FROM rota_activity 
-- ORDER BY rota_date; 