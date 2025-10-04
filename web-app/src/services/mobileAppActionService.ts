import { apiService } from "./api";

export interface MobileAppAction {
  uid?: string;
  empUID: string;
  action: string;
  status: number;
  actionDate: Date | string;
  result?: string;
  orgUID: string;
  loginId?: string;
  ss?: number; // Add SS field (sync status)
  createdBy?: string;
  createdTime?: Date | string;
  modifiedBy?: string;
  modifiedTime?: Date | string;
  serverAddTime?: Date | string;
  serverModifiedTime?: Date | string;
}

export interface MobileAppActionResponse {
  success: boolean;
  data?: MobileAppAction;
  message?: string;
}

class MobileAppActionService {
  private baseUrl = "MobileAppAction";

  /**
   * Create a mobile app action record for an employee
   * This is called AFTER the employee is successfully created
   */
  async createMobileAppAction(
    empUID: string,
    orgUID: string,
    loginId?: string
  ): Promise<MobileAppActionResponse> {
    try {
      // No need to check for existing action - backend will handle it
      
      // Generate a unique UID for the mobile_app_action record
      // Use empUID as the primary identifier to ensure uniqueness
      const uniqueId = empUID; // Use empUID as the UID to avoid duplicates
      
      const currentTime = new Date().toISOString();
      const currentUser = loginId || "SYSTEM"; // Use loginId or default to SYSTEM
      
      const mobileAppAction: MobileAppAction = {
        uid: uniqueId, // Use empUID as the unique ID
        empUID: empUID,
        action: "NO_ACTION", // Default initial action
        status: 1, // Active status
        actionDate: currentTime,
        result: "Success",
        orgUID: orgUID,
        loginId: loginId,
        ss: 0, // Initial sync status
        createdBy: currentUser,
        createdTime: currentTime,
        modifiedBy: currentUser,
        modifiedTime: currentTime,
        serverAddTime: currentTime,
        serverModifiedTime: currentTime
      };

      // Using PerformCUD endpoint which accepts array of actions
      await apiService.post(`${this.baseUrl}/PerformCUD`, [mobileAppAction]);
      
      return {
        success: true,
        data: mobileAppAction,
        message: "Mobile app access enabled successfully"
      };
    } catch (error: unknown) {
      console.error("Failed to create mobile app action:", error);
      const errorMessage = (error as any)?.response?.data?.message || 
                          (error as any)?.message ||
                          "Failed to enable mobile app access";
      
      // If it's a duplicate key error, try to update instead
      if (errorMessage.includes('duplicate') || errorMessage.includes('already exists')) {
        console.log("Duplicate mobile app action detected, attempting update");
        return await this.updateMobileAppAccess(empUID, true, orgUID);
      }
      
      // If API is not available, return a specific message
      if (errorMessage.includes('404') || (error as any)?.status === 404) {
        return {
          success: false,
          message: "Mobile app API not available - feature disabled"
        };
      }
      
      return {
        success: false,
        message: errorMessage
      };
    }
  }

  /**
   * Initialize database creation for mobile app
   * This triggers the SQLite database preparation for the employee's mobile app
   */
  async initiateMobileDBCreation(
    employeeUID: string,
    jobPositionUID: string,
    roleUID: string,
    orgUID: string,
    vehicleUID: string = "",
    empCode: string
  ): Promise<any> {
    try {
      const params = new URLSearchParams({
        employeeUID,
        jobPositionUID,
        roleUID,
        orgUID,
        vehicleUID,
        empCode
      });

      const response = await apiService.post(
        `${this.baseUrl}/InitiateDBCreation?${params.toString()}`
      );
      
      return response.data;
    } catch (error) {
      console.error("Failed to initiate DB creation:", error);
      throw error;
    }
  }

  /**
   * Get mobile app action details for an employee
   */
  async getMobileAppAction(empUID: string): Promise<MobileAppAction | null> {
    try {
      // Use the centralized API service for consistent authentication
      const response: any = await apiService.get(`${this.baseUrl}/GetMobileAppAction?empUID=${empUID}`);
      return response?.data || response || null;
    } catch (error: unknown) {
      const status = (error as any)?.status;
      
      // 404 is normal - employee simply doesn't have mobile app access record yet
      if (status === 404) {
        console.log("📱 No mobile app action record found for employee - this is normal for new employees");
        return null;
      }
      
      // Only log as error for actual problems
      if (status === 401) {
        console.error("🔒 Authentication failed for mobile app action request");
      } else if (status && status !== 404) {
        console.error("📡 Mobile app service error:", status, (error as any)?.message);
      }
      
      return null;
    }
  }

  /**
   * Check if employee has mobile app access
   */
  async hasMobileAppAccess(empUID: string): Promise<boolean> {
    try {
      const mobileAppAction = await this.getMobileAppAction(empUID);
      return mobileAppAction !== null && mobileAppAction.status === 1;
    } catch (error: any) {
      // If API is not available (404), assume no mobile access
      if (error?.status === 404 || error?.message?.includes('404')) {
        console.log("MobileAppAction API not available, defaulting to false");
        return false;
      }
      console.error("Error checking mobile app access:", error);
      return false;
    }
  }

  /**
   * Update mobile app action status (enable/disable access)
   */
  async updateMobileAppAccess(
    empUID: string,
    enableAccess: boolean,
    orgUID: string
  ): Promise<MobileAppActionResponse> {
    try {
      // Use empUID as UID for consistency
      const uid = empUID;
      
      const currentTime = new Date().toISOString();
      
      const mobileAppAction: MobileAppAction = {
        uid: uid,
        empUID: empUID,
        action: enableAccess ? "NO_ACTION" : "DISABLED",
        status: enableAccess ? 1 : 0,
        actionDate: currentTime,
        result: "Success",
        orgUID: orgUID,
        ss: 0, // Sync status
        modifiedBy: "SYSTEM",
        modifiedTime: currentTime,
        serverModifiedTime: currentTime
      };

      await apiService.post(`${this.baseUrl}/PerformCUD`, [mobileAppAction]);
      
      return {
        success: true,
        data: mobileAppAction,
        message: enableAccess 
          ? "Mobile app access enabled successfully" 
          : "Mobile app access disabled successfully"
      };
    } catch (error: unknown) {
      console.error("Failed to update mobile app access:", error);
      const errorMessage = (error as any)?.response?.data?.message || 
                          (error as any)?.message ||
                          "Failed to update mobile app access";
      
      // If API is not available, return a specific message
      if (errorMessage.includes('404') || (error as any)?.status === 404) {
        return {
          success: false,
          message: "Mobile app API not available - feature disabled"
        };
      }
      
      return {
        success: false,
        message: errorMessage
      };
    }
  }
}

export const mobileAppActionService = new MobileAppActionService();