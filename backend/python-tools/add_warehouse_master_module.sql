-- Add Warehouses submodule under Warehouse Management
-- First, get the Warehouse Management parent UID

DECLARE @WarehouseManagementUID NVARCHAR(50);
DECLARE @AdministrationUID NVARCHAR(50);

-- Get Administration module UID
SELECT @AdministrationUID = UID FROM Modules WHERE ModuleName = 'Administration';

-- Get or verify Warehouse Management module exists
SELECT @WarehouseManagementUID = UID FROM Modules WHERE ModuleName = 'Warehouse Management' AND ParentUID = @AdministrationUID;

IF @WarehouseManagementUID IS NULL
BEGIN
    PRINT 'Warehouse Management module not found. Please ensure it exists first.';
END
ELSE
BEGIN
    -- Add Warehouses submodule
    IF NOT EXISTS (SELECT 1 FROM Modules WHERE ModuleName = 'Warehouses' AND ParentUID = @WarehouseManagementUID)
    BEGIN
        INSERT INTO Modules (UID, ModuleName, ParentUID, NavigationRoute, OrderNumber, CreatedBy, CreatedTime, ModifiedBy, ModifiedTime)
        VALUES (
            NEWID(),
            'Warehouses',
            @WarehouseManagementUID,
            '/administration/warehouse-management/warehouses',
            1,
            'SYSTEM',
            GETDATE(),
            'SYSTEM',
            GETDATE()
        );
        PRINT 'Warehouses submodule added successfully';
    END
    ELSE
    BEGIN
        PRINT 'Warehouses submodule already exists';
    END

    -- Verify and display the hierarchy
    SELECT
        m1.ModuleName AS 'Administration',
        m2.ModuleName AS 'Warehouse Management',
        m3.ModuleName AS 'Submodule',
        m3.NavigationRoute AS 'Route',
        m3.OrderNumber AS 'Order'
    FROM Modules m3
    JOIN Modules m2 ON m3.ParentUID = m2.UID
    JOIN Modules m1 ON m2.ParentUID = m1.UID
    WHERE m1.ModuleName = 'Administration'
    AND m2.ModuleName = 'Warehouse Management'
    ORDER BY m3.OrderNumber;
END
