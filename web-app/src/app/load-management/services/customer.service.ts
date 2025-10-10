import { getAuthHeaders } from "@/lib/auth-service";

export interface Customer {
  UID: string;
  Code: string;
  Name: string;
  Address?: string;
  AddressLine1?: string;
  AddressLine2?: string;
  City?: string;
  State?: string;
  Country?: string;
  PinCode?: string;
  ContactNo?: string;
  ContactPerson?: string;
  Email?: string;
  Type?: string;
  Status?: string;
  IsActive?: boolean;
  OrgUID?: string;
  CompanyUID?: string;
  LocationUID?: string;
  Latitude?: string;
  Longitude?: string;
  CreditLimit?: number;
  CreditDays?: number;
  CreatedBy?: string;
  CreatedTime?: string;
  ModifiedBy?: string;
  ModifiedTime?: string;
}

export interface CustomerFilterCriteria {
  pageNumber: number;
  pageSize: number;
  filterCriterias: any[];
  isCountRequired: boolean;
  sortCriterias?: Array<{
    columnName: string;
    sortDirection: "ASC" | "DESC";
  }>;
}

export interface CustomerResponse {
  Data: Customer[];
  TotalCount: number;
  IsSuccess: boolean;
  Message?: string;
}

class CustomerService {
  private baseUrl: string;

  constructor() {
    this.baseUrl = process.env.NEXT_PUBLIC_API_URL || "http://localhost:8000";
  }

  async getAllCustomers(): Promise<Customer[]> {
    try {
      const response = await fetch(
        `${this.baseUrl}/api/Customer/SelectAllCustomers`,
        {
          method: "GET",
          headers: {
            "Content-Type": "application/json",
            ...getAuthHeaders()
          }
        }
      );

      if (!response.ok) {
        throw new Error(`Failed to fetch customers: ${response.statusText}`);
      }

      const data = await response.json();
      return data || [];
    } catch (error) {
      console.error("Error fetching customers:", error);
      return [];
    }
  }

  async getCustomersPaged(
    criteria?: Partial<CustomerFilterCriteria>
  ): Promise<CustomerResponse> {
    const defaultCriteria: CustomerFilterCriteria = {
      pageNumber: 1,
      pageSize: 50,
      filterCriterias: [],
      isCountRequired: true,
      ...criteria
    };

    try {
      // Use GET with query parameters as the API expects
      const response = await fetch(
        `${this.baseUrl}/api/Customer/GetCustomersPaged?pageNumber=${defaultCriteria.pageNumber}&pageSize=${defaultCriteria.pageSize}`,
        {
          method: "GET",
          headers: {
            "Content-Type": "application/json",
            ...getAuthHeaders()
          }
        }
      );

      if (!response.ok) {
        throw new Error(`Failed to fetch customers: ${response.statusText}`);
      }

      const data = await response.json();
      return {
        Data: data.pagedData || data || [],
        TotalCount: data.totalCount || 0,
        IsSuccess: true
      };
    } catch (error) {
      console.error("Error fetching paged customers:", error);
      return {
        Data: [],
        TotalCount: 0,
        IsSuccess: false,
        Message: error instanceof Error ? error.message : "Unknown error"
      };
    }
  }

  async searchCustomers(searchTerm: string): Promise<Customer[]> {
    try {
      const response = await fetch(
        `${
          this.baseUrl
        }/api/Customer/GetCustomersFiltered?filter=${encodeURIComponent(
          searchTerm
        )}`,
        {
          method: "GET",
          headers: {
            "Content-Type": "application/json",
            ...getAuthHeaders()
          }
        }
      );

      if (!response.ok) {
        throw new Error(`Failed to search customers: ${response.statusText}`);
      }

      const data = await response.json();
      return data || [];
    } catch (error) {
      console.error("Error searching customers:", error);
      return [];
    }
  }

  async getCustomerByUID(uid: string): Promise<Customer | null> {
    try {
      const response = await fetch(
        `${this.baseUrl}/api/Customer/GetCustomerByUID?uid=${uid}`,
        {
          method: "GET",
          headers: {
            "Content-Type": "application/json",
            ...getAuthHeaders()
          }
        }
      );

      if (!response.ok) {
        throw new Error(`Failed to fetch customer: ${response.statusText}`);
      }

      const data = await response.json();
      return data;
    } catch (error) {
      console.error("Error fetching customer by UID:", error);
      return null;
    }
  }

  async getActiveCustomers(
    pageNumber: number = 1,
    pageSize: number = 100
  ): Promise<CustomerResponse> {
    try {
      // Step 1: Populate Redis cache first
      try {
        const cacheResponse = await fetch(
          `${this.baseUrl}/api/Store/PopulateStoreCache`,
          {
            method: "GET",
            headers: {
              "Content-Type": "application/json",
              ...getAuthHeaders()
            }
          }
        );

        if (!cacheResponse.ok) {
          // Cache population failed, continue anyway
        } else {
          await cacheResponse.text();
        }
      } catch (cacheError) {
        // Cache population failed, continue anyway
      }

      // Step 2: Fetch stores (customers) using the Store API
      const requestBody = {
        PageNumber: pageNumber,
        PageSize: pageSize,
        FilterCriterias: [],
        SortCriterias: [
          {
            SortParameter: "Name",
            Direction: 0 // 0 = ASC
          }
        ],
        IsCountRequired: true
      };

      const response = await fetch(`${this.baseUrl}/api/Store/SelectAllStore`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          ...getAuthHeaders()
        },
        body: JSON.stringify(requestBody)
      });

      if (!response.ok) {
        throw new Error(`Failed to fetch stores: ${response.statusText}`);
      }

      const data = await response.json();
      console.log("=== CUSTOMER SERVICE DEBUG ===");
      console.log("Raw Store API response:", data);
      console.log("Response keys:", Object.keys(data || {}));

      // Handle different response structures from the Store API
      let stores = [];
      let totalCount = 0;

      if (data.PagedData) {
        stores = data.PagedData;
        totalCount = data.TotalCount || stores.length;
        console.log("Using data.PagedData - found", stores.length, "items");
      } else if (data.Data?.PagedData) {
        stores = data.Data.PagedData;
        totalCount = data.Data.TotalCount || stores.length;
        console.log(
          "Using data.Data.PagedData - found",
          stores.length,
          "items"
        );
      } else if (Array.isArray(data.Data)) {
        stores = data.Data;
        totalCount = data.TotalCount || stores.length;
        console.log("Using data.Data array - found", stores.length, "items");
      } else if (Array.isArray(data)) {
        stores = data;
        totalCount = stores.length;
        console.log("Using data array - found", stores.length, "items");
      } else {
        console.log("Unknown response structure");
        console.log("Data type:", typeof data);
        console.log("Full data:", data);
      }

      // Transform store data to customer format
      console.log("Sample store data:", stores[0]);
      const customers = stores.map((store: any) => ({
        UID: store.UID || store.uid || store.Code || store.code,
        Code: store.Code || store.code,
        Name: store.Name || store.name || store.Code || store.code,
        Address: store.Address || store.address || "",
        AddressLine1: store.AddressLine1 || store.address_line1 || "",
        AddressLine2: store.AddressLine2 || store.address_line2 || "",
        City: store.City || store.city || "",
        State: store.State || store.state || "",
        Country: store.Country || store.country || "",
        PinCode: store.PinCode || store.pin_code || "",
        ContactNo: store.ContactNo || store.mobile || store.contact_no || "",
        ContactPerson: store.ContactPerson || store.contact_person || "",
        Email: store.Email || store.email || "",
        Type: store.Type || store.type || "Store",
        Status: store.Status || store.status || "Active",
        IsActive:
          store.IsActive !== undefined
            ? store.IsActive
            : store.is_active !== undefined
            ? store.is_active
            : true,
        OrgUID: store.OrgUID || store.org_uid || "",
        CompanyUID: store.CompanyUID || store.company_uid || ""
      }));

      console.log("Mapped customers:", customers.length, "items");
      console.log("Sample customer:", customers[0]);

      return {
        Data: customers,
        TotalCount: totalCount,
        IsSuccess: true
      };
    } catch (error) {
      // Fallback: Try the original customer API
      try {
        const allCustomers = await this.getAllCustomers();
        const activeCustomers = allCustomers.filter(
          (c) => c.IsActive !== false
        );
        return {
          Data: activeCustomers.slice(
            (pageNumber - 1) * pageSize,
            pageNumber * pageSize
          ),
          TotalCount: activeCustomers.length,
          IsSuccess: true
        };
      } catch (fallbackError) {
        return {
          Data: [],
          TotalCount: 0,
          IsSuccess: false,
          Message: error instanceof Error ? error.message : "Unknown error"
        };
      }
    }
  }

  async getCustomersByType(
    type: string,
    pageNumber: number = 1,
    pageSize: number = 50
  ): Promise<CustomerResponse> {
    const criteria: CustomerFilterCriteria = {
      pageNumber,
      pageSize,
      filterCriterias: [
        {
          columnName: "Type",
          filterType: "Equals",
          filterValue: type
        }
      ],
      isCountRequired: true
    };

    return this.getCustomersPaged(criteria);
  }

  async getCustomersByOrganization(orgUID: string): Promise<Customer[]> {
    const criteria: CustomerFilterCriteria = {
      pageNumber: 1,
      pageSize: 1000,
      filterCriterias: [
        {
          columnName: "OrgUID",
          filterType: "Equals",
          filterValue: orgUID
        }
      ],
      isCountRequired: false
    };

    const response = await this.getCustomersPaged(criteria);
    return response.Data;
  }
}

export const customerService = new CustomerService();
