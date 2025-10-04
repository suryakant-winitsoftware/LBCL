import { authService } from "@/lib/auth-service";

export interface Setting {
  id: number;
  uid: string;
  type: string;
  name: string;
  value: string;
  dataType: string;
  isEditable: boolean;
  ss?: number;
  createdTime: string;
  modifiedTime: string;
  serverAddTime: string;
  serverModifiedTime: string;
  isSelected?: boolean;
}

export interface CreateSettingRequest {
  type: string;
  name: string;
  value: string;
  dataType: string;
  isEditable: boolean;
  uid: string;
}

export interface UpdateSettingRequest extends CreateSettingRequest {
  id: number;
}

export interface PagingRequest {
  pageNumber: number;
  pageSize: number;
  sortCriterias: SortCriteria[];
  filterCriterias: FilterCriteria[];
  isCountRequired: boolean;
}

export interface SortCriteria {
  sortParameter: string;
  direction: "Asc" | "Desc";
}

export interface FilterCriteria {
  name: string;
  value: string;
  type: "Equal" | "Like" | "NotEqual";
}

export interface PagedResponse<T> {
  pagedData: T[];
  totalCount: number;
}

export interface ApiResponse<T> {
  Data: T;
  StatusCode: number;
  IsSuccess: boolean;
  ErrorMessage?: string;
}

class SettingsService {
  private baseUrl: string;

  constructor() {
    this.baseUrl =
      process.env.NEXT_PUBLIC_API_URL || "http://localhost:8000/api";
  }

  private async makeRequest<T>(
    endpoint: string,
    options: RequestInit = {}
  ): Promise<T> {
    const token = authService.getToken();
    if (!token) {
      throw new Error("No authentication token available");
    }

    const url = `${this.baseUrl}/Setting/${endpoint}`;
    const response = await fetch(url, {
      ...options,
      headers: {
        Authorization: `Bearer ${token}`,
        "Content-Type": "application/json",
        Accept: "application/json",
        ...options.headers,
      },
    });

    if (!response.ok) {
      const errorText = await response.text();
      throw new Error(`HTTP ${response.status}: ${errorText}`);
    }

    const data = await response.json();
    if (!data.IsSuccess) {
      throw new Error(data.ErrorMessage || "API request failed");
    }

    return data.Data;
  }

  async getAllSettings(
    request: PagingRequest
  ): Promise<PagedResponse<Setting>> {
    const requestBody = {
      PageNumber: request.pageNumber,
      PageSize: request.pageSize,
      SortCriterias: request.sortCriterias.map((sc) => ({
        SortParameter: sc.sortParameter,
        Direction: sc.direction,
      })),
      FilterCriterias: request.filterCriterias.map((fc) => ({
        Name: fc.name,
        Value: fc.value,
        Type: fc.type,
      })),
      IsCountRequired: request.isCountRequired,
    };

    const apiResponse = await this.makeRequest<{
      PagedData: any[];
      TotalCount: number;
    }>("SelectAllSettingDetails", {
      method: "POST",
      body: JSON.stringify(requestBody),
    });

    // Transform the PascalCase API response to camelCase for our interface
    const transformedData = (apiResponse.PagedData || []).map((item) => ({
      id: item.Id,
      uid: item.UID,
      type: item.Type,
      name: item.Name,
      value: item.Value,
      dataType: item.DataType,
      isEditable: item.IsEditable,
      ss: item.SS,
      createdTime: item.CreatedTime,
      modifiedTime: item.ModifiedTime,
      serverAddTime: item.ServerAddTime,
      serverModifiedTime: item.ServerModifiedTime,
      isSelected: item.IsSelected,
    }));

    const response: PagedResponse<Setting> = {
      pagedData: transformedData,
      totalCount: apiResponse.TotalCount || 0,
    };

    return response;
  }

  async getSettingByUID(uid: string): Promise<Setting> {
    const response = await this.makeRequest<any>(
      `GetSettingByUID?UID=${encodeURIComponent(uid)}`
    );

    // Transform PascalCase to camelCase
    return {
      id: response.Id,
      uid: response.UID,
      type: response.Type,
      name: response.Name,
      value: response.Value,
      dataType: response.DataType,
      isEditable: response.IsEditable,
      ss: response.SS,
      createdTime: response.CreatedTime,
      modifiedTime: response.ModifiedTime,
      serverAddTime: response.ServerAddTime,
      serverModifiedTime: response.ServerModifiedTime,
      isSelected: response.IsSelected,
    };
  }

  async createSetting(setting: CreateSettingRequest): Promise<number> {
    // Transform camelCase to PascalCase for API
    const requestBody = {
      Type: setting.type,
      Name: setting.name,
      Value: setting.value,
      DataType: setting.dataType,
      IsEditable: setting.isEditable,
      UID: setting.uid,
    };

    return this.makeRequest<number>("CreateSetting", {
      method: "POST",
      body: JSON.stringify(requestBody),
    });
  }

  async updateSetting(setting: UpdateSettingRequest): Promise<number> {
    // Transform camelCase to PascalCase for API
    const requestBody = {
      Id: setting.id,
      Type: setting.type,
      Name: setting.name,
      Value: setting.value,
      DataType: setting.dataType,
      IsEditable: setting.isEditable,
      UID: setting.uid,
    };

    return this.makeRequest<number>("UpdateSetting", {
      method: "PUT",
      body: JSON.stringify(requestBody),
    });
  }

  async deleteSetting(uid: string): Promise<number> {
    return this.makeRequest<number>(
      `DeleteSetting?UID=${encodeURIComponent(uid)}`,
      {
        method: "DELETE",
      }
    );
  }

  // Helper method to generate unique UID
  generateUID(name: string): string {
    const timestamp = new Date()
      .toISOString()
      .replace(/[-:T.]/g, "")
      .slice(0, 14);
    const cleanName = name.toUpperCase().replace(/[^A-Z0-9]/g, "_");
    return `${cleanName}_${timestamp}`;
  }

  // Validate setting data
  validateSetting(setting: Partial<CreateSettingRequest>): string[] {
    const errors: string[] = [];

    if (!setting.name?.trim()) {
      errors.push("Name is required");
    }
    if (!setting.type?.trim()) {
      errors.push("Type is required");
    }
    if (!setting.dataType?.trim()) {
      errors.push("Data Type is required");
    }
    if (setting.value === undefined || setting.value === null) {
      errors.push("Value is required");
    }

    // Validate data type specific values
    if (setting.dataType === "Int" && setting.value) {
      if (isNaN(Number(setting.value))) {
        errors.push("Value must be a valid integer for Int data type");
      }
    }
    if (setting.dataType === "Boolean" && setting.value) {
      if (!["0", "1", "true", "false"].includes(setting.value.toLowerCase())) {
        errors.push("Value must be 0, 1, true, or false for Boolean data type");
      }
    }

    return errors;
  }
}

export const settingsService = new SettingsService();
