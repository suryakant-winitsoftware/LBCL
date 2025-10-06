import { ApiResponse } from "@/types/common.types";

export interface DeliveryLoadingTracking {
  UID?: string;
  PurchaseOrderUID: string;
  VehicleUID?: string | null;
  DriverEmployeeUID?: string | null;
  ForkLiftOperatorUID?: string | null;
  SecurityOfficerUID?: string | null;
  ArrivalTime?: string | null;
  LoadingStartTime?: string | null;
  LoadingEndTime?: string | null;
  DepartureTime?: string | null;
  LogisticsSignature?: string | null;
  DriverSignature?: string | null;
  Notes?: string | null;
  DeliveryNoteFilePath?: string | null;
  DeliveryNoteNumber?: string | null;
  IsActive?: boolean;
  CreatedBy?: string;
  CreatedDate?: string;
  ModifiedBy?: string;
  ModifiedDate?: string;
  // Purchase Order fields (from JOIN)
  order_number?: string;
  order_date?: string;
  warehouse_uid?: string;
  status?: string;
  org_name?: string;
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

  // Get delivery loading tracking by Purchase Order UID
  async getByPurchaseOrderUID(
    purchaseOrderUID: string
  ): Promise<DeliveryLoadingTracking | null> {
    const response = await this.apiCall<DeliveryLoadingTracking>(
      `/DeliveryLoadingTracking/GetByPurchaseOrderUID/${purchaseOrderUID}`,
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
}

export const deliveryLoadingService = new DeliveryLoadingService();
