import { ApiResponse } from "@/types/common.types";

export interface StockReceivingDetail {
  UID?: string;
  PurchaseOrderUID: string;
  PurchaseOrderLineUID: string;
  SKUCode?: string;
  SKUName?: string;
  OrderedQty: number;
  ReceivedQty: number;
  AdjustmentReason?: string;
  AdjustmentQty: number;
  ImageURL?: string;
  IsActive?: boolean;
  CreatedBy?: string;
  CreatedDate?: string;
  ModifiedBy?: string;
  ModifiedDate?: string;
}

const API_BASE_URL =
  process.env.NEXT_PUBLIC_API_URL || "http://localhost:8000/api";

class StockReceivingDetailService {
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
            errorMessage = errorText;
          }
        } catch (e) {
          // Ignore parse errors
        }
        throw new Error(errorMessage);
      }

      const data = await response.json();
      return data as ApiResponse<T>;
    } catch (error) {
      console.error("API call failed:", error);
      throw error;
    }
  }

  /**
   * Get stock receiving details by purchase order UID
   */
  async getByPurchaseOrderUID(
    purchaseOrderUID: string
  ): Promise<StockReceivingDetail[]> {
    const response = await this.apiCall<any>(
      `/StockReceivingDetail/GetByPurchaseOrderUID/${purchaseOrderUID}`,
      {
        method: "GET",
      }
    );

    if (!response.success) {
      throw new Error(
        response.message || "Failed to fetch stock receiving details"
      );
    }

    return response.data || [];
  }

  /**
   * Save stock receiving details
   */
  async saveStockReceivingDetails(
    details: StockReceivingDetail[]
  ): Promise<boolean> {
    const response = await this.apiCall<any>(
      "/StockReceivingDetail/SaveStockReceivingDetails",
      {
        method: "POST",
        body: JSON.stringify(details),
      }
    );

    if (!response.success) {
      throw new Error(
        response.message || "Failed to save stock receiving details"
      );
    }

    return true;
  }
}

export const stockReceivingDetailService = new StockReceivingDetailService();
