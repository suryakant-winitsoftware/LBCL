import re
import shutil
from datetime import datetime

# File paths
ORIGINAL_STARTUP = '/Users/suryakantkumar/Desktop/Multiplex/backend/WINITAPI/Startup.cs'
BACKUP_STARTUP = '/Users/suryakantkumar/Desktop/Multiplex/backend/WINITAPI/Startup.cs.backup'
MSSQL_STARTUP = '/Users/suryakantkumar/Desktop/Multiplex/backend/WINITAPI/Startup_MSSQL.cs'

def backup_original():
    """Create backup of original Startup.cs"""
    timestamp = datetime.now().strftime('%Y%m%d_%H%M%S')
    backup_path = f"{BACKUP_STARTUP}.{timestamp}"
    shutil.copy2(ORIGINAL_STARTUP, backup_path)
    print(f"✓ Backup created: {backup_path}")
    return backup_path

def convert_pgsql_to_mssql(content):
    """Replace all PGSQL references with MSSQL"""

    # Track changes
    changes = []

    # Pattern 1: PGSQL class names (e.g., PGSQLAuthDL -> MSSQLAuthDL)
    pattern1 = r'\bPGSQL(\w+)'
    matches1 = re.findall(pattern1, content)
    if matches1:
        changes.append(f"Found {len(matches1)} PGSQL class references")
    content = re.sub(pattern1, r'MSSQL\1', content)

    # Pattern 2: PostgreSQL in repository names (e.g., PostgreSQLRuleEngineRepository -> SQLServerRuleEngineRepository)
    pattern2 = r'\bPostgreSQL(\w+Repository)'
    matches2 = re.findall(pattern2, content)
    if matches2:
        changes.append(f"Found {len(matches2)} PostgreSQL repository references")
    content = re.sub(pattern2, r'SQLServer\1', content)

    # Pattern 3: PGSQT (typo in code - PGSQTaxMasterDL)
    pattern3 = r'\bPGSQ(\w+)'
    matches3 = re.findall(pattern3, content)
    if matches3:
        changes.append(f"Found {len(matches3)} PGSQ (typo) references")
    content = re.sub(pattern3, r'MSSQL\1', content)

    return content, changes

def analyze_file():
    """Analyze what will be changed"""
    print("\n" + "="*80)
    print("ANALYZING STARTUP.CS")
    print("="*80 + "\n")

    with open(ORIGINAL_STARTUP, 'r', encoding='utf-8') as f:
        content = f.read()

    # Find all PGSQL references
    pgsql_classes = set(re.findall(r'\bPGSQL\w+', content))
    postgres_repos = set(re.findall(r'\bPostgreSQL\w+Repository', content))
    pgsq_typos = set(re.findall(r'\bPGSQ\w+', content))

    print(f"📊 Found references to convert:")
    print(f"   - PGSQL classes: {len(pgsql_classes)}")
    print(f"   - PostgreSQL repositories: {len(postgres_repos)}")
    print(f"   - PGSQ typos: {len(pgsq_typos)}")
    print(f"   - Total: {len(pgsql_classes) + len(postgres_repos) + len(pgsq_typos)}")

    if len(pgsql_classes) > 0:
        print(f"\n📝 Sample PGSQL classes (first 10):")
        for i, cls in enumerate(sorted(list(pgsql_classes))[:10], 1):
            mssql_name = cls.replace('PGSQL', 'MSSQL').replace('PGSQ', 'MSSQL')
            print(f"   {i}. {cls} → {mssql_name}")

    if len(postgres_repos) > 0:
        print(f"\n📝 PostgreSQL repositories:")
        for i, repo in enumerate(sorted(list(postgres_repos)), 1):
            mssql_name = repo.replace('PostgreSQL', 'SQLServer')
            print(f"   {i}. {repo} → {mssql_name}")

    print("\n" + "="*80)

def create_mssql_version():
    """Create MSSQL version of Startup.cs"""
    print("\n" + "="*80)
    print("CREATING MSSQL VERSION")
    print("="*80 + "\n")

    # Read original
    with open(ORIGINAL_STARTUP, 'r', encoding='utf-8') as f:
        content = f.read()

    # Convert
    mssql_content, changes = convert_pgsql_to_mssql(content)

    # Write MSSQL version
    with open(MSSQL_STARTUP, 'w', encoding='utf-8') as f:
        f.write(mssql_content)

    print(f"✓ Created MSSQL version: {MSSQL_STARTUP}")

    for change in changes:
        print(f"  - {change}")

    return len(changes) > 0

def apply_to_original():
    """Apply MSSQL changes to original Startup.cs"""
    print("\n" + "="*80)
    print("APPLYING CHANGES TO ORIGINAL STARTUP.CS")
    print("="*80 + "\n")

    # Backup first
    backup_path = backup_original()

    # Read original
    with open(ORIGINAL_STARTUP, 'r', encoding='utf-8') as f:
        content = f.read()

    # Convert
    mssql_content, changes = convert_pgsql_to_mssql(content)

    # Write back to original
    with open(ORIGINAL_STARTUP, 'w', encoding='utf-8') as f:
        f.write(mssql_content)

    print(f"✓ Updated original Startup.cs")
    print(f"✓ Backup saved: {backup_path}")

    for change in changes:
        print(f"  - {change}")

def main():
    print("="*80)
    print("STARTUP.CS POSTGRESQL → MSSQL CONVERTER")
    print("="*80)

    # Step 1: Analyze
    analyze_file()

    # Step 2: Create MSSQL version (for comparison)
    if create_mssql_version():
        print("\n✓ MSSQL version created successfully!")

    # Step 3: Ask user if they want to apply to original
    print("\n" + "="*80)
    print("NEXT STEPS")
    print("="*80)
    print("\n1. Review the generated Startup_MSSQL.cs")
    print("2. To apply changes to the original Startup.cs, run:")
    print("   python convert_startup_to_mssql.py --apply")
    print("\n⚠️  A backup will be created before applying changes")
    print("="*80)

if __name__ == "__main__":
    import sys

    if len(sys.argv) > 1 and sys.argv[1] == '--apply':
        # Apply mode
        print("\n⚠️  APPLY MODE - Will modify original Startup.cs\n")
        response = input("Continue? (yes/no): ")
        if response.lower() == 'yes':
            apply_to_original()
            print("\n✅ DONE! Rebuild your project and restart the API.")
        else:
            print("❌ Cancelled")
    else:
        # Analysis mode (default)
        main()
