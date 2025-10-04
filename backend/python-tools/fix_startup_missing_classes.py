"""
Fix Startup.cs by reverting services where MSSQL class doesn't exist
"""

import re
import os
from pathlib import Path

STARTUP_FILE = '/Users/suryakantkumar/Desktop/Multiplex/backend/WINITAPI/Startup.cs'
MODULES_PATH = '/Users/suryakantkumar/Desktop/Multiplex/backend/Modules'

# Classes that don't have MSSQL versions (based on error messages)
MISSING_MSSQL_CLASSES = [
    'MSSQLTargetDL',
    'MSSQLStoreCheckDL',
    'MSSQLSurveyDL',
    'MSSQLSurveyResponseDL',
    'MSSQLActivityModuleDL',
    'MSSQLStoreandUserReportsDL',
    'MSSQLTaskDL',
    'MSSQLStoreCheckReportDL',
]

def check_if_mssql_exists(namespace, classname):
    """Check if MSSQL class file exists"""
    # Extract module name from namespace
    # Example: Winit.Modules.Target.DL.Classes -> Target
    parts = namespace.split('.')
    if len(parts) >= 3 and parts[0] == 'Winit' and parts[1] == 'Modules':
        module_name = parts[2]

        # Build expected path
        # Example: Modules/Target/Winit.Modules.Target.DL/Classes/MSSQLTargetDL.cs
        expected_path = os.path.join(
            MODULES_PATH,
            module_name,
            f'Winit.Modules.{module_name}.DL',
            'Classes',
            f'{classname}.cs'
        )

        exists = os.path.exists(expected_path)
        return exists, expected_path

    return False, None

def fix_startup():
    print("="*80)
    print("FIXING STARTUP.CS - REVERTING MISSING MSSQL CLASSES")
    print("="*80 + "\n")

    with open(STARTUP_FILE, 'r', encoding='utf-8') as f:
        content = f.read()

    original_content = content
    reverted = []

    # Pattern to find AddDatabaseProvider calls
    pattern = r'((?:_ = )?services)\.AddDatabaseProvider<\s*([^,]+?)\s*,\s*([^\s,]+\.Classes\.)PGSQL(\w+)\s*,\s*([^\s,]+\.Classes\.)MSSQL(\w+)\s*>\(Configuration\);'

    def check_and_revert(match):
        prefix = match.group(1)
        interface = match.group(2).strip()
        pg_namespace = match.group(3)
        pg_classname = match.group(4)
        ms_namespace = match.group(5)
        ms_classname = match.group(6)

        # Check if MSSQL class exists
        exists, path = check_if_mssql_exists(ms_namespace, f'MSSQL{ms_classname}')

        if not exists or f'MSSQL{ms_classname}' in MISSING_MSSQL_CLASSES:
            # Revert to PGSQL only
            reverted.append(f'MSSQL{ms_classname} (not found)')
            return f'{prefix}.AddTransient<{interface}, {pg_namespace}PGSQL{pg_classname}>();'
        else:
            # Keep the dynamic version
            return match.group(0)

    content = re.sub(pattern, check_and_revert, content)

    # Also fix the broken AddTransient line that lost the underscore
    content = re.sub(
        r'services\.AddTransient<',
        r'_ = services.AddTransient<',
        content
    )

    # Write back
    with open(STARTUP_FILE, 'w', encoding='utf-8') as f:
        f.write(content)

    print(f"✓ Fixed Startup.cs")
    print(f"\nReverted to PGSQL-only for {len(reverted)} services:")
    for item in reverted:
        print(f"  - {item}")

    print("\n" + "="*80)
    print("NEXT: Rebuild the project")
    print("="*80)

if __name__ == "__main__":
    fix_startup()
