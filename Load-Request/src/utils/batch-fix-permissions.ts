/**
 * Batch fix permissions in the database
 * This utility can be run to fix all permissions for all roles
 */

import { permissionService } from "@/services/admin/permission.service";
import { roleService } from "@/services/admin/role.service";

export async function batchFixAllPermissions() {
  console.log("🔧 Starting batch permission fix...");
  
  try {
    // Get all roles
    const pagingRequest = roleService.buildRolePagingRequest(1, 1000);
    const rolesResponse = await roleService.getRoles(pagingRequest);
    const roles = rolesResponse.pagedData.filter(role => role.IsActive);
    
    console.log(`Found ${roles.length} active roles to process`);
    
    let totalFixed = 0;
    
    for (const role of roles) {
      console.log(`\nProcessing role: ${role.Name} (${role.UID})`);
      
      // Process both Web and Mobile permissions
      for (const platform of ["Web", "Mobile"] as const) {
        try {
          console.log(`  - Fetching ${platform} permissions...`);
          
          // Get current permissions
          const permissions = await permissionService.getPermissionsByRole(
            role.UID,
            platform,
            role.IsPrincipalRole
          );
          
          if (permissions.length === 0) {
            console.log(`    No ${platform} permissions found`);
            continue;
          }
          
          console.log(`    Found ${permissions.length} ${platform} permissions`);
          
          // Check how many need fixing
          const needsFix = permissions.filter(p => 
            p.fullAccess && (!p.viewAccess || !p.addAccess || !p.editAccess || !p.deleteAccess)
          );
          
          if (needsFix.length === 0) {
            console.log(`    All ${platform} permissions are already consistent`);
            continue;
          }
          
          console.log(`    ${needsFix.length} permissions need fixing`);
          
          // Fix permissions
          const fixedPermissions = permissions.map(permission => {
            if (permission.fullAccess) {
              return {
                ...permission,
                viewAccess: true,
                addAccess: true,
                editAccess: true,
                deleteAccess: true,
                downloadAccess: true,
                approvalAccess: true,
                showInMenu: true
              };
            }
            return permission;
          });
          
          // Update permissions
          const permissionMaster = {
            roleUID: role.UID,
            platform,
            isPrincipalRole: role.IsPrincipalRole,
            permissions: fixedPermissions
          };
          
          console.log(`    Updating ${platform} permissions...`);
          const success = await permissionService.updatePermissions(permissionMaster);
          
          if (success) {
            console.log(`    ✅ Successfully fixed ${needsFix.length} ${platform} permissions`);
            totalFixed += needsFix.length;
          } else {
            console.error(`    ❌ Failed to update ${platform} permissions`);
          }
          
        } catch (error) {
          console.error(`    ❌ Error processing ${platform} permissions:`, error);
        }
      }
    }
    
    console.log(`\n✅ Batch fix complete! Fixed ${totalFixed} permissions total`);
    return totalFixed;
    
  } catch (error) {
    console.error("❌ Batch fix failed:", error);
    throw error;
  }
}

/**
 * Fix permissions for a specific role
 */
export async function fixRolePermissions(roleUID: string) {
  console.log(`🔧 Fixing permissions for role: ${roleUID}`);
  
  try {
    // Get role details
    const role = await roleService.getRoleByUID(roleUID);
    if (!role) {
      throw new Error(`Role ${roleUID} not found`);
    }
    
    let totalFixed = 0;
    
    // Process both platforms
    for (const platform of ["Web", "Mobile"] as const) {
      console.log(`  Processing ${platform} permissions...`);
      
      const permissions = await permissionService.getPermissionsByRole(
        roleUID,
        platform,
        role.IsPrincipalRole
      );
      
      // Check and fix
      const needsFix = permissions.filter(p => 
        p.fullAccess && (!p.viewAccess || !p.addAccess || !p.editAccess || !p.deleteAccess)
      );
      
      if (needsFix.length > 0) {
        console.log(`    Found ${needsFix.length} permissions needing fix`);
        
        const fixedPermissions = permissions.map(permission => {
          if (permission.fullAccess) {
            return {
              ...permission,
              viewAccess: true,
              addAccess: true,
              editAccess: true,
              deleteAccess: true,
              downloadAccess: true,
              approvalAccess: true,
              showInMenu: true
            };
          }
          return permission;
        });
        
        const permissionMaster = {
          roleUID,
          platform,
          isPrincipalRole: role.IsPrincipalRole,
          permissions: fixedPermissions
        };
        
        const success = await permissionService.updatePermissions(permissionMaster);
        if (success) {
          console.log(`    ✅ Fixed ${needsFix.length} ${platform} permissions`);
          totalFixed += needsFix.length;
        }
      } else {
        console.log(`    All ${platform} permissions are consistent`);
      }
    }
    
    console.log(`✅ Fixed ${totalFixed} permissions for role ${roleUID}`);
    return totalFixed;
    
  } catch (error) {
    console.error(`❌ Failed to fix permissions for role ${roleUID}:`, error);
    throw error;
  }
}