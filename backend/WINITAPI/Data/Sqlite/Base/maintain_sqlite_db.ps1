# SQLite Database Maintenance Script
# This script helps maintain and validate the SQLite database schema

param(
    [string]$DatabasePath = "Data\Sqlite\Base\WINITSQLite.db",
    [switch]$VerifySchema,
    [switch]$Backup,
    [switch]$ShowTables,
    [switch]$ShowTableInfo,
    [string]$TableName
)

# Function to check if SQLite3 is available
function Test-Sqlite3 {
    try {
        $null = sqlite3 --version
        return $true
    }
    catch {
        Write-Error "SQLite3 is not installed or not in PATH. Please install SQLite3."
        return $false
    }
}

# Function to backup database
function Backup-Database {
    param([string]$SourcePath, [string]$BackupPath)
    
    if (Test-Path $SourcePath) {
        $timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
        $backupFile = $BackupPath.Replace(".db", "_backup_$timestamp.db")
        
        Copy-Item $SourcePath $backupFile
        Write-Host "Database backed up to: $backupFile" -ForegroundColor Green
        return $backupFile
    }
    else {
        Write-Error "Source database not found: $SourcePath"
    }
}

# Function to verify schema
function Verify-Schema {
    param([string]$DbPath)
    
    Write-Host "Verifying database schema..." -ForegroundColor Yellow
    
    # Check required tables
    $requiredTables = @(
        "store_group_data",
        "org_hierarchy", 
        "list_item",
        "org_currency",
        "tax_group_taxes"
    )
    
    $missingTables = @()
    
    foreach ($table in $requiredTables) {
        $result = sqlite3 $DbPath "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='$table';"
        if ($result -eq "0") {
            $missingTables += $table
        }
    }
    
    if ($missingTables.Count -gt 0) {
        Write-Host "Missing tables: $($missingTables -join ', ')" -ForegroundColor Red
        return $false
    }
    else {
        Write-Host "All required tables exist" -ForegroundColor Green
    }
    
    # Check required columns
    $requiredColumns = @(
        @{Table="tax"; Column="created_by"},
        @{Table="sku"; Column="created_by"},
        @{Table="vehicle"; Column="created_by"},
        @{Table="currency"; Column="code"}
    )
    
    $missingColumns = @()
    
    foreach ($col in $requiredColumns) {
        $result = sqlite3 $DbPath "SELECT COUNT(*) FROM pragma_table_info('$($col.Table)') WHERE name='$($col.Column)';"
        if ($result -eq "0") {
            $missingColumns += "$($col.Table).$($col.Column)"
        }
    }
    
    if ($missingColumns.Count -gt 0) {
        Write-Host "Missing columns: $($missingColumns -join ', ')" -ForegroundColor Red
        return $false
    }
    else {
        Write-Host "All required columns exist" -ForegroundColor Green
    }
    
    return $true
}

# Function to show all tables
function Show-Tables {
    param([string]$DbPath)
    
    Write-Host "Available tables in database:" -ForegroundColor Yellow
    sqlite3 $DbPath ".tables"
}

# Function to show table info
function Show-TableInfo {
    param([string]$DbPath, [string]$Table)
    
    if ([string]::IsNullOrEmpty($Table)) {
        Write-Error "Table name is required. Use -TableName parameter."
        return
    }
    
    Write-Host "Table structure for '$Table':" -ForegroundColor Yellow
    sqlite3 $DbPath "PRAGMA table_info($Table);"
}

# Main execution
if (-not (Test-Sqlite3)) {
    exit 1
}

# Resolve database path
$fullDbPath = Join-Path (Get-Location) $DatabasePath

if (-not (Test-Path $fullDbPath)) {
    Write-Error "Database not found: $fullDbPath"
    exit 1
}

Write-Host "SQLite Database Maintenance Tool" -ForegroundColor Cyan
Write-Host "Database: $fullDbPath" -ForegroundColor Gray
Write-Host ""

# Execute requested operations
if ($Backup) {
    $backupDir = Split-Path $fullDbPath -Parent
    Backup-Database -SourcePath $fullDbPath -BackupPath $fullDbPath
}

if ($ShowTables) {
    Show-Tables -DbPath $fullDbPath
}

if ($ShowTableInfo -and $TableName) {
    Show-TableInfo -DbPath $fullDbPath -Table $TableName
}

if ($VerifySchema) {
    $isValid = Verify-Schema -DbPath $fullDbPath
    if ($isValid) {
        Write-Host "Schema verification completed successfully!" -ForegroundColor Green
    }
    else {
        Write-Host "Schema verification failed. Please run the fix script." -ForegroundColor Red
        exit 1
    }
}

# If no specific operation requested, show help
if (-not ($VerifySchema -or $Backup -or $ShowTables -or $ShowTableInfo)) {
    Write-Host "Usage examples:" -ForegroundColor Yellow
    Write-Host "  .\maintain_sqlite_db.ps1 -VerifySchema" -ForegroundColor White
    Write-Host "  .\maintain_sqlite_db.ps1 -Backup" -ForegroundColor White
    Write-Host "  .\maintain_sqlite_db.ps1 -ShowTables" -ForegroundColor White
    Write-Host "  .\maintain_sqlite_db.ps1 -ShowTableInfo -TableName tax" -ForegroundColor White
    Write-Host ""
    Write-Host "Parameters:" -ForegroundColor Yellow
    Write-Host "  -VerifySchema    Verify that all required tables and columns exist" -ForegroundColor White
    Write-Host "  -Backup          Create a timestamped backup of the database" -ForegroundColor White
    Write-Host "  -ShowTables      Display all tables in the database" -ForegroundColor White
    Write-Host "  -ShowTableInfo   Show structure of a specific table (requires -TableName)" -ForegroundColor White
    Write-Host "  -TableName       Specify table name for -ShowTableInfo operation" -ForegroundColor White
    Write-Host "  -DatabasePath    Custom path to the SQLite database (default: Data\Sqlite\Base\WINITSQLite.db)" -ForegroundColor White
} 