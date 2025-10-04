@echo off
echo SQLite Database Schema Verification
echo ===================================
echo.

set DB_PATH=Data\Sqlite\Base\WINITSQLite.db

if not exist "%DB_PATH%" (
    echo ERROR: Database not found at %DB_PATH%
    pause
    exit /b 1
)

echo Database: %DB_PATH%
echo.

echo Checking required tables...
echo.

REM Check for required tables
set MISSING_TABLES=

for %%t in (store_group_data org_hierarchy list_item org_currency tax_group_taxes) do (
    sqlite3 "%DB_PATH%" "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='%%t';" | findstr /C:"0" >nul
    if !errorlevel! equ 0 (
        echo [MISSING] %%t
        set MISSING_TABLES=!MISSING_TABLES! %%t
    ) else (
        echo [OK] %%t
    )
)

echo.
echo Checking required columns...
echo.

REM Check for required columns
set MISSING_COLUMNS=

REM Check tax.created_by
sqlite3 "%DB_PATH%" "SELECT COUNT(*) FROM pragma_table_info('tax') WHERE name='created_by';" | findstr /C:"0" >nul
if !errorlevel! equ 0 (
    echo [MISSING] tax.created_by
    set MISSING_COLUMNS=!MISSING_COLUMNS! tax.created_by
) else (
    echo [OK] tax.created_by
)

REM Check sku.created_by
sqlite3 "%DB_PATH%" "SELECT COUNT(*) FROM pragma_table_info('sku') WHERE name='created_by';" | findstr /C:"0" >nul
if !errorlevel! equ 0 (
    echo [MISSING] sku.created_by
    set MISSING_COLUMNS=!MISSING_COLUMNS! sku.created_by
) else (
    echo [OK] sku.created_by
)

REM Check vehicle.created_by
sqlite3 "%DB_PATH%" "SELECT COUNT(*) FROM pragma_table_info('vehicle') WHERE name='created_by';" | findstr /C:"0" >nul
if !errorlevel! equ 0 (
    echo [MISSING] vehicle.created_by
    set MISSING_COLUMNS=!MISSING_COLUMNS! vehicle.created_by
) else (
    echo [OK] vehicle.created_by
)

REM Check currency.code
sqlite3 "%DB_PATH%" "SELECT COUNT(*) FROM pragma_table_info('currency') WHERE name='code';" | findstr /C:"0" >nul
if !errorlevel! equ 0 (
    echo [MISSING] currency.code
    set MISSING_COLUMNS=!MISSING_COLUMNS! currency.code
) else (
    echo [OK] currency.code
)

echo.
echo ===================================
echo VERIFICATION SUMMARY
echo ===================================

if defined MISSING_TABLES (
    echo MISSING TABLES: !MISSING_TABLES!
    echo.
    echo Please run the fix_sqlite_schema.sql script to create missing tables.
) else (
    echo All required tables exist.
)

if defined MISSING_COLUMNS (
    echo MISSING COLUMNS: !MISSING_COLUMNS!
    echo.
    echo Please run the fix_sqlite_schema.sql script to add missing columns.
) else (
    echo All required columns exist.
)

if not defined MISSING_TABLES if not defined MISSING_COLUMNS (
    echo.
    echo SUCCESS: Database schema is valid and ready for mobile synchronization!
    echo.
)

pause 