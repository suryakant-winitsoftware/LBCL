import { ApiResponse } from "@/types/common.types";

export interface StockReceivingTracking {
  UID?: string;
  WHStockRequestUID: string;
  ReceiverName?: string | null;
  ReceiverEmployeeCode?: string | null;
  ForkLiftOperatorUID?: string | null;
  LoadEmptyStockEmployeeUID?: string | null;
  GetpassEmployeeUID?: string | null;
  ArrivalTime?: string | null;
  UnloadingStartTime?: string | null;
  UnloadingEndTime?: string | null;
  LoadEmptyStockTime?: string | null;
  GetpassTime?: string | null;
  PhysicalCountStartTime?: string | null;
  PhysicalCountEndTime?: string | null;
  ReceiverSignature?: string | null;
  Notes?: string | null;
  Status?: string | null;
  IsActive?: boolean;
  CreatedBy?: string;
  CreatedDate?: string;
  ModifiedBy?: string;
  ModifiedDate?: string;
  // Additional fields from JOIN queries
  DeliveryNoteNumber?: string | null;
  request_code?: string | null;
  created_time?: string | null;
}

const API_BASE_URL =
  process.env.NEXT_PUBLIC_API_URL || "http://localhost:8000/api";

class StockReceivingService {
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

  // Get all stock receiving tracking records
  async getAll(): Promise<StockReceivingTracking[]> {
    const response = await this.apiCall<StockReceivingTracking[]>(
      `/StockReceivingTracking/GetAll`,
      {
        method: "GET",
      }
    );

    if (!response.success) {
      console.warn(
        response.message || "Failed to fetch stock receiving tracking records"
      );
      return [];
    }

    return response.data || [];
  }

  // Get stock receiving tracking by WH Stock Request UID
  async getByWHStockRequestUID(
    whStockRequestUID: string
  ): Promise<StockReceivingTracking | null> {
    const response = await this.apiCall<StockReceivingTracking>(
      `/StockReceivingTracking/GetByWHStockRequestUID/${whStockRequestUID}`,
      {
        method: "GET",
      }
    );

    if (!response.success) {
      console.warn(
        response.message || "No stock receiving tracking found"
      );
      return null;
    }

    return response.data || null;
  }

  // Keep old method for backward compatibility
  async getByPurchaseOrderUID(
    purchaseOrderUID: string
  ): Promise<StockReceivingTracking | null> {
    return this.getByWHStockRequestUID(purchaseOrderUID);
  }

  // Save stock receiving tracking
  async saveStockReceivingTracking(
    stockReceivingTracking: StockReceivingTracking
  ): Promise<boolean> {
    const response = await this.apiCall<any>(
      "/StockReceivingTracking/SaveStockReceivingTracking",
      {
        method: "POST",
        body: JSON.stringify(stockReceivingTracking),
      }
    );

    if (!response.success) {
      throw new Error(
        response.message || "Failed to save stock receiving tracking"
      );
    }

    return true;
  }
}

export const stockReceivingService = new StockReceivingService();
