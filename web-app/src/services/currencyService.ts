import axios from "axios";
import { authService } from "@/lib/auth-service";

const API_BASE_URL =
  process.env.NEXT_PUBLIC_API_URL || "http://localhost:8000/api";

export interface Currency {
  Id?: number;
  UID: string;
  Name: string;
  Symbol: string;
  Code: string;
  Digits: number;
  FractionName?: string;
  IsPrimary?: boolean;
  RoundOffMinLimit?: number;
  RoundOffMaxLimit?: number;
  SS?: number;
  CreatedTime?: string;
  ModifiedTime?: string;
  ServerAddTime?: string;
  ServerModifiedTime?: string;
  IsSelected?: boolean;
}

export interface OrgCurrency extends Currency {
  OrgUID: string;
  CurrencyUID: string;
  OrgName?: string; // For display purposes
  CurrencyName?: string; // For display purposes
}

export interface OrgCurrencyRequest {
  UID: string;
  OrgUID: string;
  CurrencyUID: string;
  Name: string;
  Code: string;
  Symbol: string;
  Digits: number;
  FractionName?: string;
  IsPrimary: boolean;
  RoundOffMinLimit?: number;
  RoundOffMaxLimit?: number;
  SS?: number;
  CreatedTime?: string;
  ModifiedTime?: string;
}

export interface PagingRequest {
  pageNumber: number;
  pageSize: number;
  isCountRequired?: boolean;
  filterCriterias?: any[];
  sortCriterias?: any[];
}

export interface PagedResponse<T> {
  PagedData: T[];
  TotalCount: number;
}

export interface ApiResponse<T> {
  Data: T;
  StatusCode: number;
  IsSuccess: boolean;
  Message?: string;
}

class CurrencyService {
  private baseUrl = `${API_BASE_URL}/Currency`;

  private async getHeaders() {
    const token = await authService.getToken();
    return {
      Authorization: `Bearer ${token}`,
      "Content-Type": "application/json"
    };
  }

  async getCurrencyDetails(
    pagingRequest: PagingRequest
  ): Promise<PagedResponse<Currency>> {
    try {
      const headers = await this.getHeaders();
      const response = await axios.post<ApiResponse<PagedResponse<Currency>>>(
        `${this.baseUrl}/GetCurrencyDetails`,
        pagingRequest,
        { headers }
      );
      return response.data.Data;
    } catch (error) {
      console.error("Error fetching currency details:", error);
      throw error;
    }
  }

  async getCurrencyById(uid: string): Promise<Currency> {
    try {
      const headers = await this.getHeaders();
      const response = await axios.get<ApiResponse<Currency>>(
        `${this.baseUrl}/GetCurrencyById`,
        { params: { UID: uid }, headers }
      );
      return response.data.Data;
    } catch (error) {
      console.error("Error fetching currency by ID:", error);
      throw error;
    }
  }

  async createCurrency(currency: Currency): Promise<number> {
    try {
      const headers = await this.getHeaders();
      const response = await axios.post<number>(
        `${this.baseUrl}/CreateCurrency`,
        currency,
        { headers }
      );
      return response.data;
    } catch (error) {
      console.error("Error creating currency:", error);
      throw error;
    }
  }

  async updateCurrency(currency: Currency): Promise<number> {
    try {
      const headers = await this.getHeaders();
      const response = await axios.put<number>(
        `${this.baseUrl}/UpdateCurrency`,
        currency,
        { headers }
      );
      return response.data;
    } catch (error) {
      console.error("Error updating currency:", error);
      throw error;
    }
  }

  async deleteCurrency(uid: string): Promise<number> {
    try {
      const headers = await this.getHeaders();
      const response = await axios.delete<number>(
        `${this.baseUrl}/DeleteCurrency`,
        { params: { UID: uid }, headers }
      );
      return response.data;
    } catch (error) {
      console.error("Error deleting currency:", error);
      throw error;
    }
  }

  async getCurrencyListByOrgUID(orgUID: string): Promise<OrgCurrency[]> {
    try {
      const headers = await this.getHeaders();
      const response = await axios.get<ApiResponse<OrgCurrency[]>>(
        `${this.baseUrl}/GetCurrencyListByOrgUID`,
        { params: { orgUID }, headers }
      );
      return response.data.Data;
    } catch (error) {
      console.error("Error fetching currency list by org UID:", error);
      throw error;
    }
  }

  async createOrgCurrency(currencies: any[]): Promise<number> {
    try {
      const headers = await this.getHeaders();
      console.log("Sending request to:", `${this.baseUrl}/CreateOrgCurrency`);
      console.log("Request payload:", JSON.stringify(currencies, null, 2));
      console.log("Headers:", headers);

      const response = await axios.post<ApiResponse<number>>(
        `${this.baseUrl}/CreateOrgCurrency`,
        currencies,
        { headers }
      );
      return response.data.Data;
    } catch (error: any) {
      console.error("Error creating org currency:", error);
      console.error("Error response:", error.response?.data);
      console.error("Error status:", error.response?.status);
      throw error;
    }
  }

  async deleteOrgCurrency(uid: string): Promise<number> {
    try {
      const headers = await this.getHeaders();
      const response = await axios.delete<number>(
        `${this.baseUrl}/DeleteOrgCurrency`,
        { params: { UID: uid }, headers }
      );
      return response.data;
    } catch (error) {
      console.error("Error deleting org currency:", error);
      throw error;
    }
  }
}

export default new CurrencyService();
