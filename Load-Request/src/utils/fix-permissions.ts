/**
 * Utility to fix permission inconsistencies
 * When fullAccess is true, all other access flags should also be true
 */

import { Permission } from "@/types/admin.types";

export function fixPermissionConsistency(permission: Permission): Permission {
  // If fullAccess is true, enable all other access types
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
  
  // If any access is granted, at least viewAccess should be true
  if (
    permission.addAccess ||
    permission.editAccess ||
    permission.deleteAccess ||
    permission.downloadAccess ||
    permission.approvalAccess
  ) {
    return {
      ...permission,
      viewAccess: true
    };
  }
  
  return permission;
}

export function fixAllPermissions(permissions: Permission[]): Permission[] {
  return permissions.map(fixPermissionConsistency);
}

/**
 * Determines if a permission grants any access at all
 */
export function hasAnyAccess(permission: Permission): boolean {
  return !!(
    permission.fullAccess ||
    permission.viewAccess ||
    permission.addAccess ||
    permission.editAccess ||
    permission.deleteAccess ||
    permission.downloadAccess ||
    permission.approvalAccess
  );
}

/**
 * Determines effective access level based on permission flags
 */
export function getEffectiveAccess(permission: Permission) {
  // fullAccess overrides everything
  if (permission.fullAccess) {
    return {
      view: true,
      add: true,
      edit: true,
      delete: true,
      download: true,
      approval: true
    };
  }
  
  return {
    view: permission.viewAccess,
    add: permission.addAccess,
    edit: permission.editAccess,
    delete: permission.deleteAccess,
    download: permission.downloadAccess,
    approval: permission.approvalAccess
  };
}