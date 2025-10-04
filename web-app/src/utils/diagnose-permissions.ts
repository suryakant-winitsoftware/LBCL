/**
 * Diagnostic utility to identify permission issues
 */

import { roleService } from "@/services/admin/role.service";
import { permissionService } from "@/services/admin/permission.service";

export async function diagnosePermissionIssue(roleIdentifier: string) {
  console.log("🔍 Diagnosing permission issue for role:", roleIdentifier);
  
  try {
    // 1. Get the role details
    console.log("\n1. Fetching role details...");
    const role = await roleService.getRoleByUID(roleIdentifier);
    
    if (!role) {
      console.error("❌ Role not found with UID:", roleIdentifier);
      
      // Try to find role by name
      console.log("Searching for role by name...");
      const pagingRequest = roleService.buildRolePagingRequest(1, 1000);
      const rolesResponse = await roleService.getRoles(pagingRequest);
      const matchingRole = rolesResponse.pagedData.find(
        r => r.Name?.toLowerCase() === roleIdentifier.toLowerCase() ||
             r.Code?.toLowerCase() === roleIdentifier.toLowerCase()
      );
      
      if (matchingRole) {
        console.log("✅ Found role by name/code:");
        console.log("  - Actual UID:", matchingRole.UID);
        console.log("  - Name:", matchingRole.Name);
        console.log("  - Code:", matchingRole.Code);
        console.log("  - IsPrincipalRole:", matchingRole.IsPrincipalRole);
        console.log("\n⚠️ Use this UID instead:", matchingRole.UID);
        return matchingRole.UID;
      } else {
        console.error("❌ No role found matching:", roleIdentifier);
        console.log("\nAvailable roles:");
        rolesResponse.pagedData.forEach(r => {
          console.log(`  - ${r.Name} (UID: ${r.UID}, Code: ${r.Code})`);
        });
      }
      return null;
    }
    
    console.log("✅ Role found:");
    console.log("  - UID:", role.UID);
    console.log("  - Name:", role.Name);
    console.log("  - Code:", role.Code);
    console.log("  - IsPrincipalRole:", role.IsPrincipalRole);
    
    // 2. Check permissions
    console.log("\n2. Checking permissions...");
    try {
      const permissions = await permissionService.getPermissionsByRole(
        role.UID,
        "Web",
        role.IsPrincipalRole
      );
      
      console.log(`  - Found ${permissions.length} Web permissions`);
      
      // Check for role UID mismatches
      const mismatchedPermissions = permissions.filter(p => p.roleUID !== role.UID);
      if (mismatchedPermissions.length > 0) {
        console.warn(`⚠️ Found ${mismatchedPermissions.length} permissions with mismatched role UIDs`);
        console.log("Sample mismatched permission:", mismatchedPermissions[0]);
      }
      
    } catch (error) {
      console.error("❌ Error fetching permissions:", error);
    }
    
    return role.UID;
    
  } catch (error) {
    console.error("❌ Diagnostic failed:", error);
    return null;
  }
}

/**
 * Fix role UID case sensitivity issues
 */
export async function fixRoleUIDCase() {
  console.log("🔧 Checking for role UID case issues...");
  
  try {
    // Get all roles
    const pagingRequest = roleService.buildRolePagingRequest(1, 1000);
    const rolesResponse = await roleService.getRoles(pagingRequest);
    
    console.log(`Found ${rolesResponse.pagedData.length} roles`);
    
    // Check for common problematic UIDs
    const problematicUIDs = ["Admin", "admin", "ADMIN"];
    
    rolesResponse.pagedData.forEach(role => {
      if (problematicUIDs.includes(role.UID) || problematicUIDs.includes(role.Code)) {
        console.log(`\n⚠️ Potentially problematic role found:`);
        console.log(`  - UID: "${role.UID}"`);
        console.log(`  - Code: "${role.Code}"`);
        console.log(`  - Name: "${role.Name}"`);
        console.log(`  - DB might expect different case`);
      }
    });
    
    // Find the actual Admin role
    const adminRole = rolesResponse.pagedData.find(
      r => r.Name?.toLowerCase() === "admin" || 
           r.Code?.toLowerCase() === "admin" ||
           r.UID?.toLowerCase() === "admin"
    );
    
    if (adminRole) {
      console.log("\n✅ Admin role found:");
      console.log(`  - Use this UID: "${adminRole.UID}"`);
      console.log(`  - Not: "Admin" or "admin"`);
      return adminRole.UID;
    }
    
  } catch (error) {
    console.error("❌ Error checking role UIDs:", error);
  }
  
  return null;
}