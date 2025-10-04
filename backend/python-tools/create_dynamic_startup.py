"""
This script creates a modified Startup.cs that dynamically switches between
PostgreSQL and MSSQL based on appsettings.json configuration.
"""

import re

STARTUP_FILE = '/Users/suryakantkumar/Desktop/Multiplex/backend/WINITAPI/Startup.cs'
OUTPUT_FILE = '/Users/suryakantkumar/Desktop/Multiplex/backend/WINITAPI/Startup_Dynamic.cs'

def create_dynamic_startup():
    print("="*80)
    print("CREATING DYNAMIC DATABASE PROVIDER STARTUP.CS")
    print("="*80)

    with open(STARTUP_FILE, 'r', encoding='utf-8') as f:
        content = f.read()

    # Step 1: Add DatabaseProvider property in Startup class
    # Find the Configuration property and add DatabaseProvider after it
    config_pattern = r'(public IConfiguration Configuration \{ get; \})'

    if re.search(config_pattern, content):
        content = re.sub(
            config_pattern,
            r'\1\n    private string DatabaseProvider => Configuration["DatabaseProvider"] ?? "PostgreSQL";',
            content
        )
        print("✓ Added DatabaseProvider property")

    # Step 2: Create helper method to register services based on provider
    # Add this method before ConfigureServices
    helper_method = '''
    private void RegisterDatabaseProvider<TInterface, TPgClass, TMsClass>(IServiceCollection services)
        where TInterface : class
        where TPgClass : class, TInterface
        where TMsClass : class, TInterface
    {
        if (DatabaseProvider.Equals("MSSQL", StringComparison.OrdinalIgnoreCase))
        {
            services.AddTransient<TInterface, TMsClass>();
        }
        else
        {
            services.AddTransient<TInterface, TPgClass>();
        }
    }
'''

    # Insert before ConfigureServices method
    configure_services_pattern = r'(public void ConfigureServices\(IServiceCollection services\))'
    if re.search(configure_services_pattern, content):
        content = re.sub(
            configure_services_pattern,
            helper_method + '\n    \\1',
            content
        )
        print("✓ Added RegisterDatabaseProvider helper method")

    # Step 3: Find all PGSQL/MSSQL registrations and convert them
    print("\nConverting service registrations...")

    # Pattern to match AddTransient with PGSQL classes
    # Example: .AddTransient<Interface, PGSQLClass>()
    pattern = r'(services\.AddTransient|_ = services\.AddTransient|services\s*\.\s*AddTransient)<([^,]+),\s*([^>]+\.)(PGSQL)(\w+)>\(\);?'

    def replacement(match):
        prefix = match.group(1)
        interface = match.group(2).strip()
        namespace = match.group(3)
        db_type = match.group(4)  # PGSQL
        class_name = match.group(5)  # Rest of class name

        # Check if MSSQL version likely exists
        pg_class = f"{namespace}PGSQL{class_name}"
        ms_class = f"{namespace}MSSQL{class_name}"

        # Create dynamic registration
        return f'''if (DatabaseProvider.Equals("MSSQL", StringComparison.OrdinalIgnoreCase))
            {{
                {prefix}<{interface}, {ms_class}>();
            }}
            else
            {{
                {prefix}<{interface}, {pg_class}>();
            }}'''

    # Apply the replacement
    modified_content, count = re.subn(pattern, replacement, content)

    if count > 0:
        print(f"✓ Converted {count} service registrations to dynamic")
        content = modified_content

    # Step 4: Handle PostgreSQL repositories (different naming convention)
    pattern2 = r'(services\.AddTransient|_ = services\.AddTransient)<([^,]+),\s*([^>]+\.)(PostgreSQL)(\w+Repository)>\(\);?'

    def replacement2(match):
        prefix = match.group(1)
        interface = match.group(2).strip()
        namespace = match.group(3)
        db_type = match.group(4)  # PostgreSQL
        class_name = match.group(5)  # Repository name

        pg_class = f"{namespace}PostgreSQL{class_name}"
        ms_class = f"{namespace}SQLServer{class_name}"

        return f'''if (DatabaseProvider.Equals("MSSQL", StringComparison.OrdinalIgnoreCase))
            {{
                {prefix}<{interface}, {ms_class}>();
            }}
            else
            {{
                {prefix}<{interface}, {pg_class}>();
            }}'''

    modified_content, count2 = re.subn(pattern2, replacement2, content)

    if count2 > 0:
        print(f"✓ Converted {count2} repository registrations to dynamic")
        content = modified_content

    # Step 5: Add using statement for StringComparison if not present
    if 'using System;' in content and 'StringComparison' not in content[:1000]:
        content = content.replace('using System;', 'using System;\nusing System.Linq;', 1)
        print("✓ Added necessary using statements")

    # Write the output
    with open(OUTPUT_FILE, 'w', encoding='utf-8') as f:
        f.write(content)

    print(f"\n✓ Created: {OUTPUT_FILE}")
    print("\n" + "="*80)
    print("SUMMARY")
    print("="*80)
    print(f"Total conversions: {count + count2}")
    print("\nTo use:")
    print("1. Review the generated Startup_Dynamic.cs")
    print("2. Backup your current Startup.cs")
    print("3. Replace Startup.cs with Startup_Dynamic.cs")
    print("4. Change 'DatabaseProvider' in appsettings.json:")
    print('   - "PostgreSQL" for PostgreSQL')
    print('   - "MSSQL" for SQL Server')
    print("="*80)

if __name__ == "__main__":
    create_dynamic_startup()
