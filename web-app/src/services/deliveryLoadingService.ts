import { ApiResponse } from "@/types/common.types";

export interface DeliveryLoadingTracking {
  UID?: string;
  WHStockRequestUID: string;
  VehicleUID?: string | null;
  DriverEmployeeUID?: string | null;
  ForkLiftOperatorUID?: string | null;
  SecurityOfficerUID?: string | null;
  ArrivalTime?: string | null;
  LoadingStartTime?: string | null;
  LoadingEndTime?: string | null;
  DepartureTime?: string | null;
  EmptyLoadingStartTime?: string | null; // Start time for empties loading
  EmptyLoadingEndTime?: string | null; // End time for empties loading
  EmptiesLoadingTime?: number | null; // Time in seconds for empties loading
  LogisticsSignature?: string | null;
  DriverSignature?: string | null;
  Notes?: string | null;
  DeliveryNoteFilePath?: string | null;
  DeliveryNoteNumber?: string | null;
  Status?: string | null;
  IsActive?: boolean;
  CreatedBy?: string;
  CreatedDate?: string;
  ModifiedBy?: string;
  ModifiedDate?: string;
  // WH Stock Request fields (from JOIN)
  request_code?: string;
  created_time?: string;
  warehouse_uid?: string;
  status?: string;
  OrgName?: string;
}

const API_BASE_URL =
  process.env.NEXT_PUBLIC_API_URL || "http://localhost:8000/api";

class DeliveryLoadingService {
  private async apiCall<T>(
    endpoint: string,
    options: RequestInit = {}
  ): Promise<ApiResponse<T>> {
    try {
      const token =
        typeof window !== "undefined"
          ? localStorage.getItem("auth_token")
          : null;

      const response = await fetch(`${API_BASE_URL}${endpoint}`, {
        ...options,
        headers: {
          "Content-Type": "application/json",
          Authorization: token ? `Bearer ${token}` : "",
          ...options.headers,
        },
      });

      if (!response.ok) {
        let errorMessage = `HTTP error! status: ${response.status}`;
        try {
          const errorText = await response.text();
          if (errorText) {
            console.error("API Error:", errorText);
            errorMessage += ` - ${errorText}`;
          }
        } catch (e) {
          // Ignore if we can't read error text
        }
        throw new Error(errorMessage);
      }

      const data = await response.json();
      return {
        success: data.success !== false,
        data: data.data || data,
        message: data.message || data.error,
      };
    } catch (error) {
      console.error(`API call failed for ${endpoint}:`, error);
      return {
        success: false,
        message:
          error instanceof Error ? error.message : "Unknown error occurred",
      };
    }
  }

  // Get delivery loading tracking records by status
  async getByStatus(status: string): Promise<DeliveryLoadingTracking[]> {
    const response = await this.apiCall<DeliveryLoadingTracking[]>(
      `/DeliveryLoadingTracking/GetByStatus/${status}`,
      {
        method: "GET",
      }
    );

    if (!response.success) {
      console.warn(response.message || "No delivery loading tracking found");
      return [];
    }

    return response.data || [];
  }

  // Get delivery loading tracking by WH Stock Request UID
  async getByWHStockRequestUID(
    whStockRequestUID: string
  ): Promise<DeliveryLoadingTracking | null> {
    const response = await this.apiCall<DeliveryLoadingTracking>(
      `/DeliveryLoadingTracking/GetByWHStockRequestUID/${whStockRequestUID}`,
      {
        method: "GET",
      }
    );

    if (!response.success) {
      console.warn(
        response.message || "No delivery loading tracking found"
      );
      return null;
    }

    return response.data || null;
  }

  // Alternative method that directly calls the API (for debugging)
  async getDeliveryLoadingByWHStockRequest(whStockRequestUID: string): Promise<any> {
    try {
      const token =
        typeof window !== "undefined"
          ? localStorage.getItem("auth_token")
          : null;

      const response = await fetch(
        `${API_BASE_URL}/DeliveryLoadingTracking/GetByWHStockRequestUID/${whStockRequestUID}`,
        {
          method: "GET",
          headers: {
            "Content-Type": "application/json",
            Authorization: token ? `Bearer ${token}` : "",
          },
        }
      );

      if (!response.ok) {
        console.warn("No existing delivery loading data found");
        return null;
      }

      const result = await response.json();
      console.log("🔍 Raw API response for existing data:", result);

      return result;
    } catch (error) {
      console.error("Error fetching existing delivery loading data:", error);
      return null;
    }
  }

  // Save delivery loading tracking
  async saveDeliveryLoadingTracking(
    deliveryLoadingTracking: DeliveryLoadingTracking
  ): Promise<boolean> {
    console.log("📤 Saving delivery loading tracking:", deliveryLoadingTracking);
    console.log("📦 JSON payload:", JSON.stringify(deliveryLoadingTracking, null, 2));

    const response = await this.apiCall<any>(
      "/DeliveryLoadingTracking/SaveDeliveryLoadingTracking",
      {
        method: "POST",
        body: JSON.stringify(deliveryLoadingTracking),
      }
    );

    if (!response.success) {
      console.error("❌ Save failed:", response);
      throw new Error(
        response.message || "Failed to save delivery loading tracking"
      );
    }

    return true;
  }

  // Update empties loading time
  async updateEmptiesLoadingTime(
    whStockRequestUID: string,
    emptiesLoadingTime: number,
    startTime: string,
    endTime: string
  ): Promise<boolean> {
    try {
      // Get existing delivery loading tracking
      const existingData = await this.getByWHStockRequestUID(whStockRequestUID);

      if (!existingData) {
        console.error("No existing delivery loading tracking found");
        return false;
      }

      // Update with empties loading time and timestamps
      const updatedData: DeliveryLoadingTracking = {
        ...existingData,
        EmptiesLoadingTime: emptiesLoadingTime,
        EmptyLoadingStartTime: startTime,
        EmptyLoadingEndTime: endTime,
      };

      // Save the updated data
      await this.saveDeliveryLoadingTracking(updatedData);

      console.log("✅ Empties loading time saved:", emptiesLoadingTime, "seconds");
      console.log("✅ Start time:", startTime);
      console.log("✅ End time:", endTime);
      return true;
    } catch (error) {
      console.error("Error updating empties loading time:", error);
      return false;
    }
  }
}

export const deliveryLoadingService = new DeliveryLoadingService();
