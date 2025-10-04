import { authService } from "@/lib/auth-service";

const API_BASE_URL =
  process.env.NEXT_PUBLIC_API_URL || "http://localhost:8000/api";

export interface ListItem {
  id: number;
  uid: string;
  code: string;
  name: string;
  isEditable: boolean;
  serialNo: number;
  listHeaderUID: string;
}

export interface ListItemRequest {
  Codes: string[];
  isCountRequired: boolean;
}

class ListItemService {
  private baseUrl = `${API_BASE_URL}/ListItemHeader`;

  /**
   * Get list items by header code
   */
  async getListItemsByHeaderCode(headerCode: string): Promise<ListItem[]> {
    try {
      const request: ListItemRequest = {
        Codes: [headerCode],
        isCountRequired: false,
      };

      const response = await fetch(`${this.baseUrl}/GetListItemsByCodes`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${authService.getToken()}`,
        },
        body: JSON.stringify(request),
      });

      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }

      const data = await response.json();

      // Extract paged data from response and map to lowercase field names
      let items = [];
      if (data?.Data?.PagedData) {
        items = data.Data.PagedData;
      } else if (data?.PagedData) {
        items = data.PagedData;
      } else if (data?.pagedData) {
        items = data.pagedData;
      }

      // Map the response to match our interface (handle both uppercase and lowercase)
      return items.map((item: any) => ({
        id: item.Id || item.id || 0,
        uid: item.UID || item.uid || "",
        code: item.Code || item.code || "",
        name: item.Name || item.name || "",
        isEditable:
          item.IsEditable !== undefined
            ? item.IsEditable
            : item.isEditable || false,
        serialNo: item.SerialNo || item.serialNo || 0,
        listHeaderUID: item.ListHeaderUID || item.listHeaderUID || "",
      }));
    } catch (error) {
      console.error(`Error fetching list items for ${headerCode}:`, error);
      return [];
    }
  }

  /**
   * Get auth type options
   */
  async getAuthTypes(): Promise<ListItem[]> {
    return this.getListItemsByHeaderCode("AuthType");
  }

  /**
   * Get department options
   */
  async getDepartments(): Promise<ListItem[]> {
    return this.getListItemsByHeaderCode("Department");
  }

  /**
   * Get holiday type options
   */
  async getHolidayTypes(): Promise<ListItem[]> {
    return this.getListItemsByHeaderCode("HolidayType");
  }

  /**
   * Get task type options
   */
  async getTaskTypes(): Promise<ListItem[]> {
    return this.getListItemsByHeaderCode("TaskType");
  }

  /**
   * Get list items by multiple header codes
   */
  async getListItemsByHeaderCodes(headerCodes: string[]): Promise<ListItem[]> {
    try {
      const request: ListItemRequest = {
        Codes: headerCodes,
        isCountRequired: false,
      };

      const response = await fetch(`${this.baseUrl}/GetListItemsByCodes`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${authService.getToken()}`,
        },
        body: JSON.stringify(request),
      });

      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }

      const data = await response.json();

      // Extract paged data from response and map to lowercase field names
      let items = [];
      if (data?.Data?.PagedData) {
        items = data.Data.PagedData;
      } else if (data?.PagedData) {
        items = data.PagedData;
      } else if (data?.pagedData) {
        items = data.pagedData;
      }

      // Map the response to match our interface (handle both uppercase and lowercase)
      return items.map((item: any) => ({
        id: item.Id || item.id || 0,
        uid: item.UID || item.uid || "",
        code: item.Code || item.code || "",
        name: item.Name || item.name || "",
        isEditable:
          item.IsEditable !== undefined
            ? item.IsEditable
            : item.isEditable || false,
        serialNo: item.SerialNo || item.serialNo || 0,
        listHeaderUID: item.ListHeaderUID || item.listHeaderUID || "",
      }));
    } catch (error) {
      console.error("Error fetching list items:", error);
      return [];
    }
  }
}

export const listItemService = new ListItemService();
