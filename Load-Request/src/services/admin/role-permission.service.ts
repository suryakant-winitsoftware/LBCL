import { roleService } from "./role.service"
import { permissionService } from "./permission.service"
import { Role, Permission, PermissionMaster, ModuleHierarchy } from "@/types/admin.types"

class RolePermissionService {
  /**
   * Creates a role with module permissions
   */
  async createRoleWithPermissions(
    roleData: Partial<Role>,
    selectedWebModules: string[],
    selectedMobileModules: string[],
    webModuleHierarchy: ModuleHierarchy[],
    mobileModuleHierarchy: ModuleHierarchy[]
  ): Promise<Role> {
    try {
      // Step 1: Create the role first
      const createdRole = await roleService.createRole(roleData)
      
      // Step 2: Create permissions for web modules
      if (roleData.isWebUser && selectedWebModules.length > 0) {
        await this.saveModulePermissions(
          createdRole.UID,
          "Web",
          selectedWebModules,
          webModuleHierarchy,
          roleData.isPrincipalRole || false
        )
      }
      
      // Step 3: Create permissions for mobile modules
      if (roleData.isAppUser && selectedMobileModules.length > 0) {
        await this.saveModulePermissions(
          createdRole.UID,
          "Mobile",
          selectedMobileModules,
          mobileModuleHierarchy,
          roleData.isPrincipalRole || false
        )
      }
      
      return createdRole
    } catch (error) {
      console.error("Failed to create role with permissions:", error)
      throw error
    }
  }
  
  /**
   * Updates a role with module permissions
   */
  async updateRoleWithPermissions(
    roleData: Role,
    selectedWebModules: string[],
    selectedMobileModules: string[],
    webModuleHierarchy: ModuleHierarchy[],
    mobileModuleHierarchy: ModuleHierarchy[]
  ): Promise<Role> {
    try {
      // Step 1: Update the role
      const updatedRole = await roleService.updateRole(roleData)
      
      // Step 2: Update permissions for web modules
      if (roleData.IsWebUser && selectedWebModules.length > 0) {
        await this.saveModulePermissions(
          roleData.UID,
          "Web",
          selectedWebModules,
          webModuleHierarchy,
          roleData.IsPrincipalRole || false
        )
      }
      
      // Step 3: Update permissions for mobile modules
      if (roleData.IsAppUser && selectedMobileModules.length > 0) {
        await this.saveModulePermissions(
          roleData.UID,
          "Mobile",
          selectedMobileModules,
          mobileModuleHierarchy,
          roleData.IsPrincipalRole || false
        )
      }
      
      return updatedRole
    } catch (error) {
      console.error("Failed to update role with permissions:", error)
      throw error
    }
  }
  
  /**
   * Saves module permissions for a role
   */
  private async saveModulePermissions(
    roleUID: string,
    platform: "Web" | "Mobile",
    selectedModuleUIDs: string[],
    moduleHierarchy: ModuleHierarchy[],
    isPrincipalRole: boolean
  ): Promise<boolean> {
    try {
      // Build permissions array from selected modules
      // IMPORTANT: Permissions can only be created at the page (SubSubModule) level
      const permissions: Permission[] = []
      
      // Iterate through the module hierarchy to expand selections to page level
      moduleHierarchy.forEach(module => {
        const isModuleSelected = selectedModuleUIDs.includes(module.UID)
        
        // Check sub-modules
        if (module.children) {
          module.children.forEach(subModule => {
            const isSubModuleSelected = selectedModuleUIDs.includes(subModule.UID)
            
            // Check sub-sub-modules (pages) - This is where we create permissions
            if (subModule.children) {
              subModule.children.forEach(page => {
                // Create permission if:
                // 1. The page itself is selected, OR
                // 2. The parent sub-module is selected, OR  
                // 3. The grandparent module is selected
                if (selectedModuleUIDs.includes(page.UID) || isSubModuleSelected || isModuleSelected) {
                  permissions.push(this.createFullAccessPermission(roleUID, page.UID, platform))
                }
              })
            }
          })
        }
      })
      
      // If we have permissions to save, save them
      if (permissions.length > 0) {
        const permissionMaster: PermissionMaster = {
          roleUID,
          platform,
          isPrincipalRole,
          permissions
        }
        
        return await permissionService.updatePermissions(permissionMaster)
      }
      
      return true
    } catch (error) {
      console.error(`Failed to save ${platform} permissions:`, error)
      throw error
    }
  }
  
  /**
   * Creates a full access permission object
   */
  private createFullAccessPermission(
    roleUID: string,
    subSubModuleUID: string,
    platform: "Web" | "Mobile"
  ): Permission {
    return {
      uid: `${roleUID}_${subSubModuleUID}_${Date.now()}`, // Generate unique UID
      roleUID,
      subSubModuleUID,
      platform,
      fullAccess: true,
      viewAccess: true,
      addAccess: true,
      editAccess: true,
      deleteAccess: true,
      downloadAccess: true,
      approvalAccess: true,
      isModified: true
    }
  }
  
  /**
   * Loads existing permissions for a role
   */
  async loadRolePermissions(
    roleUID: string,
    platform: "Web" | "Mobile",
    isPrincipalRole: boolean
  ): Promise<string[]> {
    try {
      // Get permissions from the permission service
      const permissions = await permissionService.getPermissionsByRole(
        roleUID,
        platform,
        isPrincipalRole
      )
      
      // Extract the module UIDs that have permissions
      const moduleUIDs = permissions
        .filter(p => p.viewAccess || p.fullAccess)
        .map(p => p.subSubModuleUID)
      
      return moduleUIDs
    } catch (error) {
      console.error(`Failed to load ${platform} permissions:`, error)
      return []
    }
  }
}

export const rolePermissionService = new RolePermissionService()