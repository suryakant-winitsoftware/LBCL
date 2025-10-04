import { apiService } from "@/services/api";

// Define interfaces first
export interface AllocationMaster {
  allocationNo: string;
  activityNo: string;
  allocationName: string;
  allocationDescription: string;
  totalAllocationAmount: number;
  availableAllocationAmount: number;
  consumedAmount: number;
  brand: string;
  salesOrgCode: string;
  startDate: string;
  endDate: string;
  daysLeft: number;
  isActive: boolean;
}

// Mock data for development/fallback
const mockAllocations: AllocationMaster[] = [
  {
    allocationNo: "ALLOC-2024-001",
    activityNo: "ACT-001",
    allocationName: "Q1 Marketing Budget",
    allocationDescription: "Marketing budget allocation for Q1 2024",
    totalAllocationAmount: 500000,
    availableAllocationAmount: 350000,
    consumedAmount: 150000,
    brand: "Brand A",
    salesOrgCode: "ORG001",
    startDate: "2024-01-01",
    endDate: "2024-03-31",
    daysLeft: 45,
    isActive: true,
  },
  {
    allocationNo: "ALLOC-2024-002",
    activityNo: "ACT-002",
    allocationName: "Product Launch Campaign",
    allocationDescription: "Budget for new product launch",
    totalAllocationAmount: 750000,
    availableAllocationAmount: 750000,
    consumedAmount: 0,
    brand: "Brand B",
    salesOrgCode: "ORG002",
    startDate: "2024-02-01",
    endDate: "2024-06-30",
    daysLeft: 120,
    isActive: true,
  },
];

export interface Initiative {
  initiativeId: number;
  contractCode: string;
  allocationNo: string;
  name: string;
  description: string;
  salesOrgCode: string;
  brand: string;
  contractAmount: number;
  activityType: string;
  displayType: string;
  displayLocation: string;
  customerType: string;
  customerGroup?: string;
  posmFile?: string;
  defaultImage?: string;
  emailAttachment?: string;
  startDate: string;
  endDate: string;
  status: "DRAFT" | "SUBMITTED" | "ACTIVE" | "CANCELLED";
  cancelReason?: string;
  isActive: boolean;
  customers?: InitiativeCustomer[];
  products?: InitiativeProduct[];
}

export interface InitiativeCustomer {
  initiativeCustomerId?: number;
  customerCode: string;
  customerName: string;
  displayType?: string;
  displayLocation?: string;
  executionStatus?: string;
  executionDate?: string;
  executionNotes?: string;
}

export interface InitiativeProduct {
  initiativeProductId?: number;
  itemCode: string;
  itemName: string;
  category?: string;
  subCategory?: string;
}

export interface CreateInitiativeRequest {
  allocationNo: string;
  name: string;
  description: string;
  salesOrgCode: string;
  brand: string;
  contractAmount: number;
  activityType: string;
  displayType: string;
  displayLocation: string;
  customerType: string;
  customerGroup?: string;
  startDate: string;
  endDate: string;
  customerCodes?: string[]; // Backend expects array of customer codes, not full objects
  products?: InitiativeProduct[];
}

export interface InitiativeSearchRequest {
  pageNumber?: number;
  pageSize?: number;
  searchText?: string;
  salesOrgCode?: string;
  brand?: string;
  status?: string;
  startDate?: string;
  endDate?: string;
  sortBy?: string;
  sortDirection?: "ASC" | "DESC";
}

export interface ValidationResult {
  isValid: boolean;
  errors: string[];
}

class InitiativeService {
  private baseUrl = "https://multiplex-promotions-api.winitsoftware.com/api/Initiative";

  // Initiative CRUD operations
  async getInitiativeById(id: number): Promise<Initiative> {
    return await apiService.get<Initiative>(`${this.baseUrl}/${id}`);
  }

  async searchInitiatives(searchRequest: InitiativeSearchRequest): Promise<{
    pagedData: Initiative[];
    totalCount: number;
  }> {
    try {
      const response = await apiService.post<any>(
        `${this.baseUrl}/search`,
        searchRequest
      );

      // Handle different API response formats
      if (response?.PagedData) {
        // Direct PagedData format from backend
        return {
          pagedData: response.PagedData,
          totalCount: response.TotalCount || 0,
        };
      } else if (response?.Data?.PagedData) {
        // Standard API response format
        return {
          pagedData: response.Data.PagedData,
          totalCount: response.Data.TotalCount || 0,
        };
      } else if (response?.data?.PagedData) {
        // Lowercase data property
        return {
          pagedData: response.data.PagedData,
          totalCount: response.data.TotalCount || 0,
        };
      } else if (response?.pagedData) {
        // Direct pagedData format
        return {
          pagedData: response.pagedData,
          totalCount: response.totalCount || 0,
        };
      } else if (Array.isArray(response)) {
        // Direct array response
        return {
          pagedData: response,
          totalCount: response.length,
        };
      } else {
        console.warn("Unexpected initiative search response format:", response);
        return {
          pagedData: [],
          totalCount: 0,
        };
      }
    } catch (error: any) {
      throw error;
    }
  }

  async createInitiative(
    request: CreateInitiativeRequest
  ): Promise<Initiative> {
    return await apiService.post<Initiative>(this.baseUrl, request);
  }

  async getInitiativeById(id: number): Promise<Initiative> {
    return await apiService.get<Initiative>(`${this.baseUrl}/${id}`);
  }

  async updateInitiative(
    id: number,
    request: CreateInitiativeRequest
  ): Promise<Initiative> {
    return await apiService.put<Initiative>(`${this.baseUrl}/${id}`, request);
  }

  async deleteInitiative(id: number): Promise<boolean> {
    const response = await apiService.delete<{ success: boolean }>(
      `${this.baseUrl}/${id}`
    );
    return response.success;
  }

  async submitInitiative(id: number): Promise<Initiative> {
    return await apiService.post<Initiative>(`${this.baseUrl}/${id}/submit`);
  }

  async cancelInitiative(id: number, cancelReason: string): Promise<boolean> {
    const response = await apiService.post<{ success: boolean }>(
      `${this.baseUrl}/${id}/cancel`,
      { cancelReason }
    );
    return response.success;
  }

  async saveDraft(
    id: number,
    request: CreateInitiativeRequest
  ): Promise<Initiative> {
    return await apiService.post<Initiative>(
      `${this.baseUrl}/${id}/draft`,
      request
    );
  }

  // Customer operations
  async getInitiativeCustomers(id: number): Promise<InitiativeCustomer[]> {
    return await apiService.get<InitiativeCustomer[]>(
      `${this.baseUrl}/${id}/customers`
    );
  }

  async updateInitiativeCustomers(
    id: number,
    customers: InitiativeCustomer[]
  ): Promise<boolean> {
    const response = await apiService.put<{ success: boolean }>(
      `${this.baseUrl}/${id}/customers`,
      customers
    );
    return response.success;
  }

  // Product operations
  async getInitiativeProducts(id: number): Promise<InitiativeProduct[]> {
    return await apiService.get<InitiativeProduct[]>(
      `${this.baseUrl}/${id}/products`
    );
  }

  async updateInitiativeProducts(
    id: number,
    products: InitiativeProduct[]
  ): Promise<boolean> {
    const response = await apiService.put<{ success: boolean }>(
      `${this.baseUrl}/${id}/products`,
      products
    );
    return response.success;
  }

  // Allocation operations
  async getAvailableAllocations(
    salesOrgCode: string,
    brand?: string,
    startDate?: string,
    endDate?: string
  ): Promise<AllocationMaster[]> {
    try {
      const params = new URLSearchParams({ salesOrgCode });
      if (brand) params.append("brand", brand);
      if (startDate) params.append("startDate", startDate);
      if (endDate) params.append("endDate", endDate);

      const response = await apiService.get<any[]>(
        `${this.baseUrl}/allocations?${params.toString()}`
      );

      // Transform PascalCase response to camelCase
      return response.map((item) => ({
        allocationNo: item.AllocationNo || item.allocationNo,
        activityNo: item.ActivityNo || item.activityNo,
        allocationName: item.AllocationName || item.allocationName,
        allocationDescription:
          item.AllocationDescription || item.allocationDescription,
        totalAllocationAmount:
          item.TotalAllocationAmount || item.totalAllocationAmount,
        availableAllocationAmount:
          item.AvailableAllocationAmount || item.availableAllocationAmount,
        consumedAmount: item.ConsumedAmount || item.consumedAmount,
        brand: item.Brand || item.brand,
        salesOrgCode: item.SalesOrgCode || item.salesOrgCode,
        startDate: item.StartDate || item.startDate,
        endDate: item.EndDate || item.endDate,
        daysLeft: item.DaysLeft || item.daysLeft,
        isActive: item.IsActive !== undefined ? item.IsActive : item.isActive,
      }));
    } catch (error: any) {
      if (error?.response?.status === 404 || error?.status === 404) {
        console.warn("Allocation API not available, using mock data");
        let filtered = [...mockAllocations];
        if (salesOrgCode) {
          filtered = filtered.filter((a) => a.salesOrgCode === salesOrgCode);
        }
        if (brand) {
          filtered = filtered.filter((a) => a.brand === brand);
        }
        return filtered;
      }
      throw error;
    }
  }

  async getAllocationDetails(allocationNo: string): Promise<AllocationMaster> {
    return await apiService.get<AllocationMaster>(
      `${this.baseUrl}/allocations/${allocationNo}`
    );
  }

  async validateAllocationAmount(
    allocationNo: string,
    contractAmount: number,
    initiativeId?: number
  ): Promise<{
    isValid: boolean;
    availableAmount: number;
    requestedAmount: number;
    message: string;
  }> {
    return await apiService.post<{
      isValid: boolean;
      availableAmount: number;
      requestedAmount: number;
      message: string;
    }>(`${this.baseUrl}/allocations/${allocationNo}/validate`, {
      contractAmount,
      initiativeId,
    });
  }

  // File operations
  async uploadFile(
    id: number,
    fileType: "posm" | "default_image" | "email_attachment",
    file: File
  ): Promise<{
    success: boolean;
    filePath: string;
    fileName: string;
    fileSize: number;
  }> {
    const formData = new FormData();
    formData.append("file", file);

    return await apiService.post<{
      success: boolean;
      filePath: string;
      fileName: string;
      fileSize: number;
    }>(`${this.baseUrl}/${id}/files/${fileType}`, formData, {
      "Content-Type": "multipart/form-data",
    });
  }

  async deleteFile(
    id: number,
    fileType: "posm" | "default_image" | "email_attachment"
  ): Promise<boolean> {
    const response = await apiService.delete<{ success: boolean }>(
      `${this.baseUrl}/${id}/files/${fileType}`
    );
    return response.success;
  }

  async downloadFile(
    id: number,
    fileType: "posm" | "default_image" | "email_attachment"
  ): Promise<Blob> {
    // Note: apiService doesn't support responseType blob, may need custom implementation
    const response = await fetch(`${this.baseUrl}/${id}/files/${fileType}`, {
      headers: {
        Authorization: `Bearer ${localStorage.getItem("token")}`,
      },
    });
    return await response.blob();
  }

  // Validation
  async validateInitiative(
    request: CreateInitiativeRequest,
    initiativeId?: number
  ): Promise<ValidationResult> {
    return await apiService.post<ValidationResult>(`${this.baseUrl}/validate`, {
      ...request,
      initiativeId,
    });
  }

  // Helper method to format currency
  formatCurrency(amount: number): string {
    return new Intl.NumberFormat("en-US", {
      style: "currency",
      currency: "USD",
      minimumFractionDigits: 0,
      maximumFractionDigits: 0,
    }).format(amount);
  }

  // Helper method to calculate days left
  calculateDaysLeft(endDate: string): number {
    const end = new Date(endDate);
    const today = new Date();
    const diffTime = end.getTime() - today.getTime();
    const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
    return diffDays > 0 ? diffDays : 0;
  }

  // Helper method to get status color
  getStatusColor(status: string): string {
    switch (status) {
      case "ACTIVE":
        return "bg-green-100 text-green-800";
      case "DRAFT":
        return "bg-yellow-100 text-yellow-800";
      case "SUBMITTED":
        return "bg-blue-100 text-blue-800";
      case "CANCELLED":
        return "bg-red-100 text-red-800";
      default:
        return "bg-gray-100 text-gray-800";
    }
  }

  // Helper method to validate date range
  validateDateRange(startDate: string, endDate: string): boolean {
    const start = new Date(startDate);
    const end = new Date(endDate);
    return start < end;
  }

  // Helper method to check if initiative is editable
  isEditable(status: string): boolean {
    return status === "DRAFT";
  }

  // Helper method to check if initiative can be submitted
  canSubmit(initiative: Initiative): boolean {
    return (
      initiative.status === "DRAFT" &&
      initiative.customers &&
      initiative.customers.length > 0 &&
      initiative.products &&
      initiative.products.length > 0
    );
  }

  // Helper method to check if initiative can be cancelled
  canCancel(status: string): boolean {
    return status === "SUBMITTED" || status === "ACTIVE";
  }
}

export const initiativeService = new InitiativeService();
