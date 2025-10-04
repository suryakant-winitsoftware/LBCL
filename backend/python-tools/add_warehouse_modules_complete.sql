-- =====================================================
-- Add Warehouse Management Submodules and Permissions
-- =====================================================

DECLARE @AdministrationUID NVARCHAR(50);
DECLARE @WarehouseManagementUID NVARCHAR(50);
DECLARE @WarehousesUID NVARCHAR(50);
DECLARE @StockRequestsUID NVARCHAR(50);
DECLARE @StockSummaryUID NVARCHAR(50);
DECLARE @StockAuditsUID NVARCHAR(50);
DECLARE @StockConversionsUID NVARCHAR(50);

-- Get Administration module UID
SELECT @AdministrationUID = UID FROM Modules WHERE ModuleName = 'Administration';

IF @AdministrationUID IS NULL
BEGIN
    PRINT '❌ Administration module not found. Please ensure it exists first.';
    RETURN;
END

PRINT '✓ Found Administration module: ' + @AdministrationUID;

-- Get or create Warehouse Management module
SELECT @WarehouseManagementUID = UID FROM Modules WHERE ModuleName = 'Warehouse Management' AND ParentUID = @AdministrationUID;

IF @WarehouseManagementUID IS NULL
BEGIN
    SET @WarehouseManagementUID = NEWID();
    INSERT INTO Modules (UID, ModuleName, ParentUID, NavigationRoute, OrderNumber, CreatedBy, CreatedTime, ModifiedBy, ModifiedTime)
    VALUES (
        @WarehouseManagementUID,
        'Warehouse Management',
        @AdministrationUID,
        '/administration/warehouse-management',
        10,
        'SYSTEM',
        GETDATE(),
        'SYSTEM',
        GETDATE()
    );
    PRINT '✓ Warehouse Management module created: ' + CAST(@WarehouseManagementUID AS NVARCHAR(50));
END
ELSE
BEGIN
    PRINT '✓ Found Warehouse Management module: ' + CAST(@WarehouseManagementUID AS NVARCHAR(50));
END

-- =====================================================
-- 1. Add Warehouses submodule
-- =====================================================
SELECT @WarehousesUID = UID FROM Modules WHERE ModuleName = 'Warehouses' AND ParentUID = @WarehouseManagementUID;

IF @WarehousesUID IS NULL
BEGIN
    SET @WarehousesUID = NEWID();
    INSERT INTO Modules (UID, ModuleName, ParentUID, NavigationRoute, OrderNumber, CreatedBy, CreatedTime, ModifiedBy, ModifiedTime)
    VALUES (
        @WarehousesUID,
        'Warehouses',
        @WarehouseManagementUID,
        '/administration/warehouse-management/warehouses',
        1,
        'SYSTEM',
        GETDATE(),
        'SYSTEM',
        GETDATE()
    );
    PRINT '✓ Warehouses submodule created: ' + CAST(@WarehousesUID AS NVARCHAR(50));
END
ELSE
BEGIN
    PRINT '✓ Warehouses submodule already exists: ' + CAST(@WarehousesUID AS NVARCHAR(50));
END

-- =====================================================
-- 2. Add Stock Requests submodule
-- =====================================================
SELECT @StockRequestsUID = UID FROM Modules WHERE ModuleName = 'Stock Requests' AND ParentUID = @WarehouseManagementUID;

IF @StockRequestsUID IS NULL
BEGIN
    SET @StockRequestsUID = NEWID();
    INSERT INTO Modules (UID, ModuleName, ParentUID, NavigationRoute, OrderNumber, CreatedBy, CreatedTime, ModifiedBy, ModifiedTime)
    VALUES (
        @StockRequestsUID,
        'Stock Requests',
        @WarehouseManagementUID,
        '/administration/warehouse-management/stock-requests',
        2,
        'SYSTEM',
        GETDATE(),
        'SYSTEM',
        GETDATE()
    );
    PRINT '✓ Stock Requests submodule created: ' + CAST(@StockRequestsUID AS NVARCHAR(50));
END
ELSE
BEGIN
    PRINT '✓ Stock Requests submodule already exists: ' + CAST(@StockRequestsUID AS NVARCHAR(50));
END

-- =====================================================
-- 3. Add Stock Summary submodule
-- =====================================================
SELECT @StockSummaryUID = UID FROM Modules WHERE ModuleName = 'Stock Summary' AND ParentUID = @WarehouseManagementUID;

IF @StockSummaryUID IS NULL
BEGIN
    SET @StockSummaryUID = NEWID();
    INSERT INTO Modules (UID, ModuleName, ParentUID, NavigationRoute, OrderNumber, CreatedBy, CreatedTime, ModifiedBy, ModifiedTime)
    VALUES (
        @StockSummaryUID,
        'Stock Summary',
        @WarehouseManagementUID,
        '/administration/warehouse-management/stock-summary',
        3,
        'SYSTEM',
        GETDATE(),
        'SYSTEM',
        GETDATE()
    );
    PRINT '✓ Stock Summary submodule created: ' + CAST(@StockSummaryUID AS NVARCHAR(50));
END
ELSE
BEGIN
    PRINT '✓ Stock Summary submodule already exists: ' + CAST(@StockSummaryUID AS NVARCHAR(50));
END

-- =====================================================
-- 4. Add Stock Audits submodule
-- =====================================================
SELECT @StockAuditsUID = UID FROM Modules WHERE ModuleName = 'Stock Audits' AND ParentUID = @WarehouseManagementUID;

IF @StockAuditsUID IS NULL
BEGIN
    SET @StockAuditsUID = NEWID();
    INSERT INTO Modules (UID, ModuleName, ParentUID, NavigationRoute, OrderNumber, CreatedBy, CreatedTime, ModifiedBy, ModifiedTime)
    VALUES (
        @StockAuditsUID,
        'Stock Audits',
        @WarehouseManagementUID,
        '/administration/warehouse-management/stock-audits',
        4,
        'SYSTEM',
        GETDATE(),
        'SYSTEM',
        GETDATE()
    );
    PRINT '✓ Stock Audits submodule created: ' + CAST(@StockAuditsUID AS NVARCHAR(50));
END
ELSE
BEGIN
    PRINT '✓ Stock Audits submodule already exists: ' + CAST(@StockAuditsUID AS NVARCHAR(50));
END

-- =====================================================
-- 5. Add Stock Conversions submodule
-- =====================================================
SELECT @StockConversionsUID = UID FROM Modules WHERE ModuleName = 'Stock Conversions' AND ParentUID = @WarehouseManagementUID;

IF @StockConversionsUID IS NULL
BEGIN
    SET @StockConversionsUID = NEWID();
    INSERT INTO Modules (UID, ModuleName, ParentUID, NavigationRoute, OrderNumber, CreatedBy, CreatedTime, ModifiedBy, ModifiedTime)
    VALUES (
        @StockConversionsUID,
        'Stock Conversions',
        @WarehouseManagementUID,
        '/administration/warehouse-management/stock-conversions',
        5,
        'SYSTEM',
        GETDATE(),
        'SYSTEM',
        GETDATE()
    );
    PRINT '✓ Stock Conversions submodule created: ' + CAST(@StockConversionsUID AS NVARCHAR(50));
END
ELSE
BEGIN
    PRINT '✓ Stock Conversions submodule already exists: ' + CAST(@StockConversionsUID AS NVARCHAR(50));
END

-- =====================================================
-- Add Permissions (Privileges) for all submodules
-- =====================================================

PRINT '';
PRINT '📋 Adding Permissions...';

-- Create a table variable to hold module UIDs
DECLARE @ModuleUIDs TABLE (ModuleUID NVARCHAR(50), ModuleName NVARCHAR(100));

INSERT INTO @ModuleUIDs VALUES (@WarehousesUID, 'Warehouses');
INSERT INTO @ModuleUIDs VALUES (@StockRequestsUID, 'Stock Requests');
INSERT INTO @ModuleUIDs VALUES (@StockSummaryUID, 'Stock Summary');
INSERT INTO @ModuleUIDs VALUES (@StockAuditsUID, 'Stock Audits');
INSERT INTO @ModuleUIDs VALUES (@StockConversionsUID, 'Stock Conversions');

-- For each module, add standard CRUD permissions
DECLARE @CurrentModuleUID NVARCHAR(50);
DECLARE @CurrentModuleName NVARCHAR(100);
DECLARE @PermissionUID NVARCHAR(50);
DECLARE @PermissionName NVARCHAR(100);

DECLARE module_cursor CURSOR FOR
SELECT ModuleUID, ModuleName FROM @ModuleUIDs;

OPEN module_cursor;
FETCH NEXT FROM module_cursor INTO @CurrentModuleUID, @CurrentModuleName;

WHILE @@FETCH_STATUS = 0
BEGIN
    -- View Permission
    SET @PermissionName = 'View ' + @CurrentModuleName;
    IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Name = @PermissionName AND ModuleUID = @CurrentModuleUID)
    BEGIN
        SET @PermissionUID = NEWID();
        INSERT INTO Permissions (UID, Name, Description, ModuleUID, CreatedBy, CreatedTime, ModifiedBy, ModifiedTime)
        VALUES (
            @PermissionUID,
            @PermissionName,
            'Permission to view ' + @CurrentModuleName,
            @CurrentModuleUID,
            'SYSTEM',
            GETDATE(),
            'SYSTEM',
            GETDATE()
        );
        PRINT '  ✓ Added: ' + @PermissionName;
    END

    -- Create Permission
    SET @PermissionName = 'Create ' + @CurrentModuleName;
    IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Name = @PermissionName AND ModuleUID = @CurrentModuleUID)
    BEGIN
        SET @PermissionUID = NEWID();
        INSERT INTO Permissions (UID, Name, Description, ModuleUID, CreatedBy, CreatedTime, ModifiedBy, ModifiedTime)
        VALUES (
            @PermissionUID,
            @PermissionName,
            'Permission to create ' + @CurrentModuleName,
            @CurrentModuleUID,
            'SYSTEM',
            GETDATE(),
            'SYSTEM',
            GETDATE()
        );
        PRINT '  ✓ Added: ' + @PermissionName;
    END

    -- Update Permission
    SET @PermissionName = 'Update ' + @CurrentModuleName;
    IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Name = @PermissionName AND ModuleUID = @CurrentModuleUID)
    BEGIN
        SET @PermissionUID = NEWID();
        INSERT INTO Permissions (UID, Name, Description, ModuleUID, CreatedBy, CreatedTime, ModifiedBy, ModifiedTime)
        VALUES (
            @PermissionUID,
            @PermissionName,
            'Permission to update ' + @CurrentModuleName,
            @CurrentModuleUID,
            'SYSTEM',
            GETDATE(),
            'SYSTEM',
            GETDATE()
        );
        PRINT '  ✓ Added: ' + @PermissionName;
    END

    -- Delete Permission
    SET @PermissionName = 'Delete ' + @CurrentModuleName;
    IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Name = @PermissionName AND ModuleUID = @CurrentModuleUID)
    BEGIN
        SET @PermissionUID = NEWID();
        INSERT INTO Permissions (UID, Name, Description, ModuleUID, CreatedBy, CreatedTime, ModifiedBy, ModifiedTime)
        VALUES (
            @PermissionUID,
            @PermissionName,
            'Permission to delete ' + @CurrentModuleName,
            @CurrentModuleUID,
            'SYSTEM',
            GETDATE(),
            'SYSTEM',
            GETDATE()
        );
        PRINT '  ✓ Added: ' + @PermissionName;
    END

    -- Export Permission
    SET @PermissionName = 'Export ' + @CurrentModuleName;
    IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Name = @PermissionName AND ModuleUID = @CurrentModuleUID)
    BEGIN
        SET @PermissionUID = NEWID();
        INSERT INTO Permissions (UID, Name, Description, ModuleUID, CreatedBy, CreatedTime, ModifiedBy, ModifiedTime)
        VALUES (
            @PermissionUID,
            @PermissionName,
            'Permission to export ' + @CurrentModuleName,
            @CurrentModuleUID,
            'SYSTEM',
            GETDATE(),
            'SYSTEM',
            GETDATE()
        );
        PRINT '  ✓ Added: ' + @PermissionName;
    END

    FETCH NEXT FROM module_cursor INTO @CurrentModuleUID, @CurrentModuleName;
END

CLOSE module_cursor;
DEALLOCATE module_cursor;

-- =====================================================
-- Display final hierarchy
-- =====================================================
PRINT '';
PRINT '📊 Warehouse Management Module Hierarchy:';
PRINT '==========================================';

SELECT
    m3.OrderNumber AS [Order],
    m3.ModuleName AS [Submodule],
    m3.NavigationRoute AS [Route],
    COUNT(p.UID) AS [Permissions]
FROM Modules m3
LEFT JOIN Permissions p ON p.ModuleUID = m3.UID
JOIN Modules m2 ON m3.ParentUID = m2.UID
JOIN Modules m1 ON m2.ParentUID = m1.UID
WHERE m1.ModuleName = 'Administration'
AND m2.ModuleName = 'Warehouse Management'
GROUP BY m3.OrderNumber, m3.ModuleName, m3.NavigationRoute
ORDER BY m3.OrderNumber;

PRINT '';
PRINT '✅ Warehouse Management modules and permissions setup complete!';
