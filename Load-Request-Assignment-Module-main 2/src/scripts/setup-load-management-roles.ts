/**
 * Script to set up Load Management roles in the system
 * Run this to create LSR Agent and Logistics Approval Agent roles
 */

import { roleService } from "@/services/admin/role.service";
import { LSR_AGENT_ROLE, LOGISTICS_APPROVAL_AGENT_ROLE } from "@/config/load-management-roles";

export async function setupLoadManagementRoles() {
  try {
    console.log("🚀 Setting up Load Management roles...");
    
    // Create LSR Agent Role
    console.log("Creating LSR Agent role...");
    const lsrAgentRole = await roleService.createRole({
      uid: `ROLE_${Date.now()}_LSR`,
      code: LSR_AGENT_ROLE.code,
      roleNameEn: LSR_AGENT_ROLE.roleNameEn,
      roleNameOther: LSR_AGENT_ROLE.roleNameOther,
      isWebUser: LSR_AGENT_ROLE.isWebUser,
      isAppUser: LSR_AGENT_ROLE.isAppUser,
      isActive: LSR_AGENT_ROLE.isActive,
      isAdmin: false,
      isPrincipalRole: false,
      isDistributorRole: false,
      webMenuData: JSON.stringify({
        modules: ["LOAD_MANAGEMENT"],
        subModules: ["LSR"],
        permissions: LSR_AGENT_ROLE.permissions.actions
      }),
      mobileMenuData: ""
    });
    console.log("✅ LSR Agent role created:", lsrAgentRole);
    
    // Create Logistics Approval Agent Role
    console.log("Creating Logistics Approval Agent role...");
    const logisticsApprovalRole = await roleService.createRole({
      uid: `ROLE_${Date.now()}_LOGISTICS`,
      code: LOGISTICS_APPROVAL_AGENT_ROLE.code,
      roleNameEn: LOGISTICS_APPROVAL_AGENT_ROLE.roleNameEn,
      roleNameOther: LOGISTICS_APPROVAL_AGENT_ROLE.roleNameOther,
      isWebUser: LOGISTICS_APPROVAL_AGENT_ROLE.isWebUser,
      isAppUser: LOGISTICS_APPROVAL_AGENT_ROLE.isAppUser,
      isActive: LOGISTICS_APPROVAL_AGENT_ROLE.isActive,
      isAdmin: false,
      isPrincipalRole: false,
      isDistributorRole: false,
      webMenuData: JSON.stringify({
        modules: ["LOAD_MANAGEMENT"],
        subModules: ["LOGISTICS_APPROVAL"],
        permissions: LOGISTICS_APPROVAL_AGENT_ROLE.permissions.actions
      }),
      mobileMenuData: ""
    });
    console.log("✅ Logistics Approval Agent role created:", logisticsApprovalRole);
    
    console.log("🎉 Load Management roles setup complete!");
    
    return {
      lsrAgentRole,
      logisticsApprovalRole
    };
  } catch (error) {
    console.error("❌ Error setting up roles:", error);
    throw error;
  }
}

// Function to assign role to a user
export async function assignLoadManagementRole(
  userEmail: string,
  roleCode: "LSR_AGENT" | "LOGISTICS_APPROVAL"
) {
  try {
    console.log(`Assigning ${roleCode} role to ${userEmail}...`);
    
    // Get the role UID by code
    const roles = await roleService.getRoles({
      pageNumber: 0,
      pageSize: 100,
      filters: { code: roleCode }
    });
    
    const role = roles.pagedData?.find(r => r.code === roleCode);
    if (!role) {
      throw new Error(`Role ${roleCode} not found. Please run setupLoadManagementRoles first.`);
    }
    
    // You would need to implement user update logic here
    // This depends on your user service implementation
    console.log(`TODO: Update user ${userEmail} with role UID: ${role.uid}`);
    
    return role;
  } catch (error) {
    console.error(`Error assigning role to ${userEmail}:`, error);
    throw error;
  }
}

// CLI usage example
if (require.main === module) {
  (async () => {
    try {
      // Setup roles
      await setupLoadManagementRoles();
      
      // Example: Assign LSR Agent role to specific user
      // await assignLoadManagementRole("lsragent1@gmail.com", "LSR_AGENT");
      
    } catch (error) {
      console.error("Setup failed:", error);
      process.exit(1);
    }
  })();
}