-- Verification script to check database schema fixes
-- This script verifies that all missing tables and columns have been created

-- 1. Check if missing tables exist
SELECT 'Checking missing tables...' as status;

SELECT 
    CASE 
        WHEN COUNT(*) = 5 THEN '✓ All missing tables created successfully'
        ELSE '✗ Some tables are still missing: ' || GROUP_CONCAT(missing_table)
    END as table_status
FROM (
    SELECT 'store_group_data' as missing_table WHERE NOT EXISTS (SELECT 1 FROM sqlite_master WHERE type='table' AND name='store_group_data')
    UNION ALL
    SELECT 'org_hierarchy' WHERE NOT EXISTS (SELECT 1 FROM sqlite_master WHERE type='table' AND name='org_hierarchy')
    UNION ALL
    SELECT 'list_item' WHERE NOT EXISTS (SELECT 1 FROM sqlite_master WHERE type='table' AND name='list_item')
    UNION ALL
    SELECT 'org_currency' WHERE NOT EXISTS (SELECT 1 FROM sqlite_master WHERE type='table' AND name='org_currency')
    UNION ALL
    SELECT 'tax_group_taxes' WHERE NOT EXISTS (SELECT 1 FROM sqlite_master WHERE type='table' AND name='tax_group_taxes')
) missing_tables;

-- 2. Check if missing columns exist in existing tables
SELECT 'Checking missing columns...' as status;

-- Check tax table for created_by column
SELECT 
    CASE 
        WHEN COUNT(*) > 0 THEN '✓ created_by column exists in tax table'
        ELSE '✗ created_by column missing in tax table'
    END as tax_created_by_status
FROM pragma_table_info('tax') 
WHERE name = 'created_by';

-- Check sku table for created_by column (note: it already has CreatedBy)
SELECT 
    CASE 
        WHEN COUNT(*) > 0 THEN '✓ created_by column exists in sku table'
        ELSE '✗ created_by column missing in sku table'
    END as sku_created_by_status
FROM pragma_table_info('sku') 
WHERE name = 'created_by';

-- Check vehicle table for created_by column
SELECT 
    CASE 
        WHEN COUNT(*) > 0 THEN '✓ created_by column exists in vehicle table'
        ELSE '✗ created_by column missing in vehicle table'
    END as vehicle_created_by_status
FROM pragma_table_info('vehicle') 
WHERE name = 'created_by';

-- Check currency table for code column
SELECT 
    CASE 
        WHEN COUNT(*) > 0 THEN '✓ code column exists in currency table'
        ELSE '✗ code column missing in currency table'
    END as currency_code_status
FROM pragma_table_info('currency') 
WHERE name = 'code';

-- 3. Show table counts for verification
SELECT 'Table counts for verification:' as status;

SELECT 'store_group_data' as table_name, COUNT(*) as record_count FROM store_group_data
UNION ALL
SELECT 'org_hierarchy', COUNT(*) FROM org_hierarchy
UNION ALL
SELECT 'list_item', COUNT(*) FROM list_item
UNION ALL
SELECT 'org_currency', COUNT(*) FROM org_currency
UNION ALL
SELECT 'tax_group_taxes', COUNT(*) FROM tax_group_taxes;

-- 4. Final status
SELECT 'Schema verification completed' as final_status; 