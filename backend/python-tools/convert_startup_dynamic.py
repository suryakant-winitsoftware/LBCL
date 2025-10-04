"""
Convert Startup.cs to use DatabaseProviderExtensions for dynamic DB switching
"""

import re
import shutil
from datetime import datetime

STARTUP_FILE = '/Users/suryakantkumar/Desktop/Multiplex/backend/WINITAPI/Startup.cs'
OUTPUT_FILE = '/Users/suryakantkumar/Desktop/Multiplex/backend/WINITAPI/Startup_Dynamic.cs'

def backup_file(filepath):
    timestamp = datetime.now().strftime('%Y%m%d_%H%M%S')
    backup_path = f"{filepath}.backup_{timestamp}"
    shutil.copy2(filepath, backup_path)
    print(f"✓ Backup created: {backup_path}")
    return backup_path

def convert_to_dynamic():
    print("="*80)
    print("CONVERTING STARTUP.CS TO DYNAMIC DATABASE PROVIDER")
    print("="*80 + "\n")

    with open(STARTUP_FILE, 'r', encoding='utf-8') as f:
        content = f.read()

    original_content = content
    conversions = []

    # Step 1: Add using statement for extensions
    if 'using WINITAPI.Extensions;' not in content:
        # Find the last using statement before namespace
        using_pattern = r'(using [^;]+;)\s*namespace'
        content = re.sub(using_pattern, r'\1\nusing WINITAPI.Extensions;\n\nnamespace', content, count=1)
        print("✓ Added using WINITAPI.Extensions;")

    # Step 2: Convert PGSQL/MSSQL service registrations
    # Pattern 1: Simple AddTransient with PGSQL (handles multi-line and indentation)
    pattern1 = r'((?:_ = )?services)\s*\.\s*AddTransient<\s*([^,]+?)\s*,\s*([^\s>]+\.Classes\.)PGSQL(\w+)\s*>\s*\(\s*\)\s*;?'

    def replace_pgsql(match):
        prefix = match.group(1)
        interface = match.group(2).strip()
        namespace = match.group(3)
        classname = match.group(4)

        conversion = f"{interface} :: PGSQL{classname} / MSSQL{classname}"
        conversions.append(conversion)

        return (f'{prefix}.AddDatabaseProvider<{interface}, '
                f'{namespace}PGSQL{classname}, '
                f'{namespace}MSSQL{classname}>(Configuration);')

    content, count1 = re.subn(pattern1, replace_pgsql, content, flags=re.DOTALL)
    print(f"✓ Converted {count1} PGSQL service registrations")

    # Pattern 2: PostgreSQL repositories
    pattern2 = r'((?:_ = )?services)\s*\.\s*AddTransient<\s*([^,]+?)\s*,\s*([^\s>]+\.Classes\.)PostgreSQL(\w+Repository)\s*>\s*\(\s*\)\s*;?'

    def replace_postgres_repo(match):
        prefix = match.group(1)
        interface = match.group(2).strip()
        namespace = match.group(3)
        classname = match.group(4)

        conversion = f"{interface} :: PostgreSQL{classname} / SQLServer{classname}"
        conversions.append(conversion)

        return (f'{prefix}.AddDatabaseProvider<{interface}, '
                f'{namespace}PostgreSQL{classname}, '
                f'{namespace}SQLServer{classname}>(Configuration);')

    content, count2 = re.subn(pattern2, replace_postgres_repo, content, flags=re.DOTALL)
    print(f"✓ Converted {count2} PostgreSQL repository registrations")

    # Write output
    with open(OUTPUT_FILE, 'w', encoding='utf-8') as f:
        f.write(content)

    print(f"\n✓ Created: {OUTPUT_FILE}")

    # Show summary
    print("\n" + "="*80)
    print("CONVERSION SUMMARY")
    print("="*80)
    print(f"Total conversions: {count1 + count2}")

    if conversions[:10]:
        print("\nFirst 10 conversions:")
        for i, conv in enumerate(conversions[:10], 1):
            print(f"  {i}. {conv}")
        if len(conversions) > 10:
            print(f"  ... and {len(conversions) - 10} more")

    print("\n" + "="*80)
    print("HOW TO USE")
    print("="*80)
    print("1. Review Startup_Dynamic.cs")
    print("2. Backup current Startup.cs")
    print("3. Replace Startup.cs with Startup_Dynamic.cs")
    print("4. Rebuild project")
    print("\n5. Switch database by changing appsettings.json:")
    print('   "DatabaseProvider": "PostgreSQL"  <- Use PostgreSQL')
    print('   "DatabaseProvider": "MSSQL"       <- Use SQL Server')
    print("="*80)

    return count1 + count2

if __name__ == "__main__":
    total = convert_to_dynamic()
    print(f"\n✅ Conversion complete! {total} services now support dynamic switching.")
