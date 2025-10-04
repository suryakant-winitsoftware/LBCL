-- =====================================================
-- Add Warehouses submodule under Warehouse Management
-- =====================================================

DECLARE @WarehouseManagementUID NVARCHAR(50);
DECLARE @WarehousesUID NVARCHAR(50);

-- Get Warehouse Management parent UID
SELECT @WarehouseManagementUID = UID
FROM Modules
WHERE ModuleName = 'Warehouse Management'
  AND ParentUID IN (SELECT UID FROM Modules WHERE ModuleName = 'Administration');

IF @WarehouseManagementUID IS NULL
BEGIN
    PRINT '❌ Warehouse Management module not found';
    RETURN;
END

PRINT '✓ Found Warehouse Management: ' + CAST(@WarehouseManagementUID AS NVARCHAR(50));

-- Check if Warehouses already exists
IF EXISTS (SELECT 1 FROM Modules WHERE ModuleName = 'Warehouses' AND ParentUID = @WarehouseManagementUID)
BEGIN
    PRINT 'ℹ Warehouses submodule already exists';
    SELECT @WarehousesUID = UID FROM Modules WHERE ModuleName = 'Warehouses' AND ParentUID = @WarehouseManagementUID;
END
ELSE
BEGIN
    -- Add Warehouses submodule
    SET @WarehousesUID = NEWID();

    INSERT INTO Modules (UID, ModuleName, ParentUID, NavigationRoute, OrderNumber, CreatedBy, CreatedTime, ModifiedBy, ModifiedTime)
    VALUES (
        @WarehousesUID,
        'Warehouses',
        @WarehouseManagementUID,
        '/administration/warehouse-management/warehouses',
        1,  -- First in the list
        'SYSTEM',
        GETDATE(),
        'SYSTEM',
        GETDATE()
    );

    PRINT '✓ Warehouses submodule created: ' + CAST(@WarehousesUID AS NVARCHAR(50));
END

-- =====================================================
-- Add Permissions for Warehouses
-- =====================================================

PRINT '';
PRINT '📋 Adding Permissions for Warehouses...';

-- View Permission
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Name = 'View Warehouses' AND ModuleUID = @WarehousesUID)
BEGIN
    INSERT INTO Permissions (UID, Name, Description, ModuleUID, CreatedBy, CreatedTime, ModifiedBy, ModifiedTime)
    VALUES (
        NEWID(),
        'View Warehouses',
        'Permission to view warehouses',
        @WarehousesUID,
        'SYSTEM',
        GETDATE(),
        'SYSTEM',
        GETDATE()
    );
    PRINT '  ✓ Added: View Warehouses';
END
ELSE
BEGIN
    PRINT '  ℹ View Warehouses already exists';
END

-- Create Permission
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Name = 'Create Warehouses' AND ModuleUID = @WarehousesUID)
BEGIN
    INSERT INTO Permissions (UID, Name, Description, ModuleUID, CreatedBy, CreatedTime, ModifiedBy, ModifiedTime)
    VALUES (
        NEWID(),
        'Create Warehouses',
        'Permission to create warehouses',
        @WarehousesUID,
        'SYSTEM',
        GETDATE(),
        'SYSTEM',
        GETDATE()
    );
    PRINT '  ✓ Added: Create Warehouses';
END
ELSE
BEGIN
    PRINT '  ℹ Create Warehouses already exists';
END

-- Update Permission
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Name = 'Update Warehouses' AND ModuleUID = @WarehousesUID)
BEGIN
    INSERT INTO Permissions (UID, Name, Description, ModuleUID, CreatedBy, CreatedTime, ModifiedBy, ModifiedTime)
    VALUES (
        NEWID(),
        'Update Warehouses',
        'Permission to update warehouses',
        @WarehousesUID,
        'SYSTEM',
        GETDATE(),
        'SYSTEM',
        GETDATE()
    );
    PRINT '  ✓ Added: Update Warehouses';
END
ELSE
BEGIN
    PRINT '  ℹ Update Warehouses already exists';
END

-- Delete Permission
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Name = 'Delete Warehouses' AND ModuleUID = @WarehousesUID)
BEGIN
    INSERT INTO Permissions (UID, Name, Description, ModuleUID, CreatedBy, CreatedTime, ModifiedBy, ModifiedTime)
    VALUES (
        NEWID(),
        'Delete Warehouses',
        'Permission to delete warehouses',
        @WarehousesUID,
        'SYSTEM',
        GETDATE(),
        'SYSTEM',
        GETDATE()
    );
    PRINT '  ✓ Added: Delete Warehouses';
END
ELSE
BEGIN
    PRINT '  ℹ Delete Warehouses already exists';
END

-- Export Permission
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Name = 'Export Warehouses' AND ModuleUID = @WarehousesUID)
BEGIN
    INSERT INTO Permissions (UID, Name, Description, ModuleUID, CreatedBy, CreatedTime, ModifiedBy, ModifiedTime)
    VALUES (
        NEWID(),
        'Export Warehouses',
        'Permission to export warehouses',
        @WarehousesUID,
        'SYSTEM',
        GETDATE(),
        'SYSTEM',
        GETDATE()
    );
    PRINT '  ✓ Added: Export Warehouses';
END
ELSE
BEGIN
    PRINT '  ℹ Export Warehouses already exists';
END

-- =====================================================
-- Show all Warehouse Management submodules
-- =====================================================
PRINT '';
PRINT '📊 All Warehouse Management Submodules:';
PRINT '========================================';

SELECT
    m.OrderNumber AS [Order],
    m.ModuleName AS [Module Name],
    m.NavigationRoute AS [Route],
    COUNT(p.UID) AS [Permissions]
FROM Modules m
LEFT JOIN Permissions p ON p.ModuleUID = m.UID
WHERE m.ParentUID = @WarehouseManagementUID
GROUP BY m.OrderNumber, m.ModuleName, m.NavigationRoute
ORDER BY m.OrderNumber;

PRINT '';
PRINT '✅ Warehouses submodule setup complete!';
