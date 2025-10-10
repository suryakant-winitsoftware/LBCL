/**
 * Load Management Role Configurations
 * Defines specific roles for LSR and Logistics Approval agents
 */

export interface LoadManagementRole {
  code: string;
  roleNameEn: string;
  roleNameOther: string;
  description: string;
  isWebUser: boolean;
  isAppUser: boolean;
  isActive: boolean;
  permissions: {
    modules: string[];
    subModules: string[];
    actions: string[];
  };
  allowedRoutes: string[];
}

// LSR Agent Role - Can create, view, and manage LSR requests
export const LSR_AGENT_ROLE: LoadManagementRole = {
  code: "LSR_AGENT",
  roleNameEn: "LSR Agent",
  roleNameOther: "Load Service Request Agent",
  description: "Agent responsible for creating and managing Load Service Requests (LSR)",
  isWebUser: true,
  isAppUser: false,
  isActive: true,
  permissions: {
    modules: ["LOAD_MANAGEMENT"],
    subModules: ["LSR"],
    actions: [
      "CREATE_LSR",
      "VIEW_LSR",
      "EDIT_LSR",
      "VIEW_LSR_HISTORY",
      "VIEW_PENDING_LSR",
      "VIEW_APPROVED_LSR"
    ]
  },
  allowedRoutes: [
    "/load-management",
    "/load-management/lsr",
    "/load-management/lsr/create",
    "/load-management/lsr/pending",
    "/load-management/lsr/approved",
    "/load-management/lsr/history",
    "/load-management/lsr/edit",
    "/load-management/lsr/view"
  ]
};

// Logistics Approval Agent Role - Can approve/reject logistics requests
export const LOGISTICS_APPROVAL_AGENT_ROLE: LoadManagementRole = {
  code: "LOGISTICS_APPROVAL",
  roleNameEn: "Logistics Approval Agent",
  roleNameOther: "Logistics Approval Authority",
  description: "Agent responsible for approving or rejecting logistics requests",
  isWebUser: true,
  isAppUser: false,
  isActive: true,
  permissions: {
    modules: ["LOAD_MANAGEMENT"],
    subModules: ["LOGISTICS_APPROVAL"],
    actions: [
      "VIEW_INCOMING_APPROVALS",
      "VIEW_PENDING_APPROVALS",
      "APPROVE_REQUEST",
      "REJECT_REQUEST",
      "VIEW_APPROVED",
      "VIEW_APPROVAL_HISTORY",
      "ADD_COMMENTS",
      "TRACK_APPROVAL"
    ]
  },
  allowedRoutes: [
    "/load-management",
    "/load-management/logistics-approval",
    "/load-management/logistics-approval/incoming",
    "/load-management/logistics-approval/pending",
    "/load-management/logistics-approval/approved",
    "/load-management/logistics-approval/history",
    "/load-management/logistics-approval/track"
  ]
};

// Combined permissions for users who need both roles
export const LOAD_MANAGEMENT_ADMIN_ROLE: LoadManagementRole = {
  code: "LOAD_ADMIN",
  roleNameEn: "Load Management Admin",
  roleNameOther: "Load Management Administrator",
  description: "Full access to all Load Management features",
  isWebUser: true,
  isAppUser: false,
  isActive: true,
  permissions: {
    modules: ["LOAD_MANAGEMENT"],
    subModules: ["LSR", "LOGISTICS_APPROVAL"],
    actions: [
      ...LSR_AGENT_ROLE.permissions.actions,
      ...LOGISTICS_APPROVAL_AGENT_ROLE.permissions.actions
    ]
  },
  allowedRoutes: [
    ...LSR_AGENT_ROLE.allowedRoutes,
    ...LOGISTICS_APPROVAL_AGENT_ROLE.allowedRoutes
  ]
};

// Helper function to check if a user has permission for a route
export function hasLoadManagementPermission(
  userRole: string,
  requestedRoute: string
): boolean {
  let role: LoadManagementRole | null = null;
  
  switch (userRole) {
    case "LSR_AGENT":
      role = LSR_AGENT_ROLE;
      break;
    case "LOGISTICS_APPROVAL":
      role = LOGISTICS_APPROVAL_AGENT_ROLE;
      break;
    case "LOAD_ADMIN":
      role = LOAD_MANAGEMENT_ADMIN_ROLE;
      break;
    default:
      return false;
  }
  
  return role.allowedRoutes.some(route => 
    requestedRoute.startsWith(route) || route === requestedRoute
  );
}

// Helper function to get visible menu items based on role
export function getLoadManagementMenuItems(userRole: string) {
  switch (userRole) {
    case "LSR_AGENT":
      return {
        showLoadManagement: true,
        showLSR: true,
        showLogisticsApproval: false
      };
    case "LOGISTICS_APPROVAL":
      return {
        showLoadManagement: true,
        showLSR: false,
        showLogisticsApproval: true
      };
    case "LOAD_ADMIN":
      return {
        showLoadManagement: true,
        showLSR: true,
        showLogisticsApproval: true
      };
    default:
      return {
        showLoadManagement: false,
        showLSR: false,
        showLogisticsApproval: false
      };
  }
}