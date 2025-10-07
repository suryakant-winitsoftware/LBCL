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
      // Generate a unique UID for the mobile_app_action record
      const uniqueId = `${empUID}-mobile-${Date.now()}`;

      const mobileAppAction: MobileAppAction = {
        uid: uniqueId, // Use a unique ID for the mobile_app_action record
        empUID: empUID,
        action: "NO_ACTION", // Default initial action
        status: 1, // Active status
        actionDate: new Date().toISOString(),
        result: "Success",
        orgUID: orgUID,
        loginId: loginId,
        ss: 0 // Initial sync status
      };

      // Using PerformCUD endpoint which accepts array of actions
      const response = await apiService.post(`${this.baseUrl}/PerformCUD`, [
        mobileAppAction
      ]);

      return {
        success: true,
        data: mobileAppAction,
        message: "Mobile app access enabled successfully"
      };
    } catch (error: unknown) {
      console.error("Failed to create mobile app action:", error);
      const errorMessage =
        (error as any)?.response?.data?.message ||
        (error as any)?.message ||
        "Failed to enable mobile app access";

      // If API is not available, return a specific message
      if (errorMessage.includes("404") || (error as any)?.status === 404) {
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
      // Try direct fetch approach like other working APIs
      const response = await fetch(
        `${
          process.env.NEXT_PUBLIC_API_URL || "http://localhost:8000/api"
        }/MobileAppAction/GetMobileAppAction?empUID=${empUID}`,
        {
          method: "GET",
          headers: {
            "Content-Type": "application/json",
            // Add auth headers if available
            ...(typeof window !== "undefined" &&
            localStorage.getItem("authToken")
              ? { Authorization: `Bearer ${localStorage.getItem("authToken")}` }
              : {})
          }
        }
      );

      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }

      const result = await response.json();
      return result?.data || result || null;
    } catch (error: unknown) {
      console.error("Failed to get mobile app action:", error);
      // If API is not available, return null
      if (
        (error as any)?.message?.includes("404") ||
        (error as any)?.status === 404
      ) {
        console.log(
          "MobileAppAction API endpoint not found - feature may not be available"
        );
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
      if (error?.status === 404 || error?.message?.includes("404")) {
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
      // First check if a record exists for this employee
      const existingRecord = await this.getMobileAppAction(empUID);

      // Use existing UID if updating, or create new unique UID if creating
      const uid = existingRecord?.uid || `${empUID}-mobile-${Date.now()}`;

      const mobileAppAction: MobileAppAction = {
        uid: uid,
        empUID: empUID,
        action: enableAccess ? "NO_ACTION" : "DISABLED",
        status: enableAccess ? 1 : 0,
        actionDate: new Date().toISOString(),
        result: "Success",
        orgUID: orgUID,
        ss: 0 // Sync status
      };

      const response = await apiService.post(`${this.baseUrl}/PerformCUD`, [
        mobileAppAction
      ]);

      return {
        success: true,
        data: mobileAppAction,
        message: enableAccess
          ? "Mobile app access enabled successfully"
          : "Mobile app access disabled successfully"
      };
    } catch (error: unknown) {
      console.error("Failed to update mobile app access:", error);
      const errorMessage =
        (error as any)?.response?.data?.message ||
        (error as any)?.message ||
        "Failed to update mobile app access";

      // If API is not available, return a specific message
      if (errorMessage.includes("404") || (error as any)?.status === 404) {
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
