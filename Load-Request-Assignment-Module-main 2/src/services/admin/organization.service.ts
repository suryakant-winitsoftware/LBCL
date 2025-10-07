import { ApiResponse } from "@/types/admin.types";
import { authService } from "@/lib/auth-service";

const API_BASE_URL =
  process.env.NEXT_PUBLIC_API_URL || "http://localhost:8000/api";

interface Organization {
  uid: string;
  code: string;
  name: string;
  isActive: boolean;
  parentOrgUID?: string;
  orgType?: string;
}

class OrganizationService {
  private async apiCall<T>(
    endpoint: string,
    options: RequestInit = {}
  ): Promise<ApiResponse<T>> {
    try {
      const response = await fetch(`${API_BASE_URL}${endpoint}`, {
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${authService.getToken()}`,
          ...options.headers
        },
        ...options
      });

      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }

      const data = await response.json();
      return {
        success: true,
        data: data.data || data,
        message: data.message
      };
    } catch (error) {
      console.error(`API call failed for ${endpoint}:`, error);
      return {
        success: false,
        message:
          error instanceof Error ? error.message : "Unknown error occurred"
      };
    }
  }

  async getAllOrganizations(): Promise<Organization[]> {
    const response = await this.apiCall<Organization[]>(
      "/Organization/SelectAllOrganizations"
    );

    if (!response.success || !response.data) {
      console.warn("Failed to fetch organizations, using fallback");
      // Return empty array or basic fallback data
      return [];
    }

    return response.data;
  }

  async getOrganizationsByUID(uid: string): Promise<Organization> {
    const response = await this.apiCall<Organization>(
      `/Organization/SelectOrganizationByUID?uid=${uid}`
    );

    if (!response.success || !response.data) {
      throw new Error(response.message || "Failed to fetch organization");
    }

    return response.data;
  }

  async getActiveOrganizations(): Promise<Organization[]> {
    const organizations = await this.getAllOrganizations();
    return organizations.filter((org) => org.isActive);
  }
}

export const organizationService = new OrganizationService();
export type { Organization };
