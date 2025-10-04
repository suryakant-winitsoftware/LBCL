"use client";

import { FilterCriteria, SortCriteria } from "@/types/common.types";
import { locationService, Location } from "./locationService";
import { getCurrentUser } from "@/utils/auth";

// Types
export interface Organization {
  UID: string;
  Code: string;
  Name: string;
  OrgTypeUID: string;
  OrgTypeName?: string;
  ParentUID?: string;
  ParentName?: string;
  CountryUID?: string;
  CountryName?: string;
  RegionUID?: string;
  RegionName?: string;
  CityUID?: string;
  CityName?: string;
  CompanyUID?: string;
  CompanyName?: string;
  TaxGroupUID?: string;
  TaxGroupName?: string;
  IsActive: boolean;
  Status: string;
  SeqCode?: string;
  HasEarlyAccess?: boolean;
  ShowInUI?: boolean;
  ShowInTemplate?: boolean;
  CreatedBy?: string;
  CreatedTime?: string;
  ModifiedBy?: string;
  ModifiedTime?: string;
  children?: Organization[];
}

export interface OrgType {
  UID: string;
  Name: string;
  ParentUID?: string;
  IsCompanyOrg: boolean;
  IsFranchiseeOrg: boolean;
  IsWh: boolean;
  ShowInUI?: boolean;
  ShowInTemplate?: boolean;
  level?: number;
  children?: OrgType[];
}

export interface TaxGroup {
  UID: string;
  Name: string;
  Description?: string;
  TaxPercentage?: number;
}

export interface CreateOrganizationForm {
  uid?: string;
  code: string;
  name: string;
  orgTypeUID: string;
  parentUID?: string;
  countryUID?: string;
  regionUID?: string;
  cityUID?: string;
  companyUID?: string;
  taxGroupUID?: string;
  status: string;
  isActive: boolean;
  hasEarlyAccess?: boolean;
  createdBy?: string;
  createdTime?: string;
  modifiedBy?: string;
  modifiedTime?: string;
  seqCode?: string;
}

export interface OrganizationHierarchyNode {
  uid: string;
  name: string;
  code: string;
  orgTypeName: string;
  orgTypeUID: string;
  level: number;
  children: OrganizationHierarchyNode[];
}

const API_BASE_URL =
  process.env.NEXT_PUBLIC_API_URL || "https://multiplex-promotions-api.winitsoftware.com/api";

class OrganizationService {
  private baseUrl: string;

  constructor() {
    this.baseUrl = API_BASE_URL;
  }

  private async apiCall<T>(
    endpoint: string,
    options: RequestInit = {}
  ): Promise<{ success: boolean; data?: T; message?: string }> {
    try {
      const token =
        typeof window !== "undefined"
          ? localStorage.getItem("auth_token")
          : null;

      const response = await fetch(`${this.baseUrl}${endpoint}`, {
        ...options,
        headers: {
          "Content-Type": "application/json",
          ...(token ? { Authorization: `Bearer ${token}` } : {}),
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
        success: data.IsSuccess !== false,
        data: data.Data || data,
        message: data.Message || data.ErrorMessage || data.message,
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

  // Organization Types
  async getOrganizationTypes(
    pageNumber: number = 1,
    pageSize: number = 1000
  ): Promise<OrgType[]> {
    const pagingRequest = {
      pageNumber,
      pageSize,
      sortCriterias: [],
      filterCriterias: [],
      isCountRequired: true,
    };

    const result = await this.apiCall<any>("/Org/GetOrgTypeDetails", {
      method: "POST",
      body: JSON.stringify(pagingRequest),
    });

    if (result.success && result.data) {
      const items =
        result.data.PagedData || result.data.Data || result.data || [];
      return items;
    }

    return [];
  }

  // Organizations
  async getOrganizations(
    pageNumber: number = 1,
    pageSize: number = 100,
    filters?: FilterCriteria[],
    sorts?: SortCriteria[]
  ): Promise<{ data: Organization[]; total: number }> {
    const pagingRequest = {
      pageNumber: pageNumber,
      pageSize: pageSize,
      sortCriterias: sorts || [],
      filterCriterias: filters || [],
      isCountRequired: true,
    };

    const response = await this.apiCall<any>("/Org/GetOrgDetails", {
      method: "POST",
      body: JSON.stringify(pagingRequest),
    });

    if (!response.success) {
      throw new Error(response.message || "Failed to fetch organizations");
    }

    // Extract paged data - handle different response formats
    let items = [];
    let totalCount = 0;

    if (response.data) {
      if (response.data.PagedData) {
        items = response.data.PagedData;
        totalCount = response.data.TotalCount || items.length;
      } else if (response.data.Data && response.data.Data.PagedData) {
        items = response.data.Data.PagedData;
        totalCount = response.data.Data.TotalCount || items.length;
      } else if (Array.isArray(response.data.Data)) {
        items = response.data.Data;
        totalCount = response.data.TotalCount || items.length;
      } else if (Array.isArray(response.data)) {
        items = response.data;
        totalCount = items.length;
      }
    }

    return {
      data: items,
      total: totalCount,
    };
  }

  async getOrganizationByUID(uid: string): Promise<Organization> {
    // Validate and sanitize UID parameter
    if (!uid || typeof uid !== "string" || uid.trim() === "") {
      throw new Error("Invalid organization UID provided");
    }

    // Encode the UID to prevent URL injection issues
    const encodedUID = encodeURIComponent(uid.trim());
    const response = await this.apiCall<any>(
      `/Org/GetOrgByUID?UID=${encodedUID}`
    );
    if (!response.success || !response.data) {
      throw new Error(response.message || "Failed to fetch organization");
    }
    return response.data.Data || response.data;
  }

  async createOrganization(
    organization: CreateOrganizationForm
  ): Promise<Organization> {
    // Get current user from token
    const token =
      typeof window !== "undefined" ? localStorage.getItem("auth_token") : null;
    const currentUser = token ? getCurrentUser(token) : "SYSTEM";
    const currentTime = new Date().toISOString();

    // Prepare the payload with required fields for backend (PascalCase to match C# model)
    const orgPayload = {
      UID: organization.uid || "",
      Code: organization.code || "",
      Name: organization.name || "",
      IsActive:
        organization.isActive !== undefined ? organization.isActive : true,
      OrgTypeUID: organization.orgTypeUID || "",
      ParentUID: organization.parentUID || "",
      CountryUID: organization.countryUID || "",
      CompanyUID: organization.companyUID || "",
      TaxGroupUID: organization.taxGroupUID || "",
      Status: organization.status || "Active",
      SeqCode: organization.seqCode || "",
      HasEarlyAccess: organization.hasEarlyAccess || false,
      ShowInUI: true,
      ShowInTemplate: true,
      CreatedBy: organization.createdBy || currentUser,
      CreatedTime: organization.createdTime || currentTime,
      ModifiedBy: organization.modifiedBy || currentUser,
      ModifiedTime: organization.modifiedTime || currentTime,
    };

    const response = await this.apiCall<Organization>("/Org/CreateOrg", {
      method: "POST",
      body: JSON.stringify(orgPayload),
    });
    if (!response.success || !response.data) {
      throw new Error(response.message || "Failed to create organization");
    }
    return response.data;
  }

  async updateOrganization(
    uid: string,
    organization: Partial<Organization>
  ): Promise<Organization> {
    // Get current user from token for ModifiedBy field
    const token =
      typeof window !== "undefined" ? localStorage.getItem("auth_token") : null;
    const currentUser = token ? getCurrentUser(token) : "SYSTEM";
    const currentTime = new Date().toISOString();

    const updatePayload = {
      UID: uid,
      Code: organization.Code,
      Name: organization.Name,
      IsActive: organization.IsActive,
      OrgTypeUID: organization.OrgTypeUID,
      ParentUID: organization.ParentUID,
      CountryUID: organization.CountryUID,
      CompanyUID: organization.CompanyUID,
      TaxGroupUID: organization.TaxGroupUID,
      Status: organization.Status,
      SeqCode: organization.SeqCode,
      HasEarlyAccess: organization.HasEarlyAccess,
      ModifiedBy: currentUser,
      ModifiedTime: currentTime,
    };

    const response = await this.apiCall<Organization>("/Org/UpdateOrg", {
      method: "PUT",
      body: JSON.stringify(updatePayload),
    });
    if (!response.success || !response.data) {
      throw new Error(response.message || "Failed to update organization");
    }
    return response.data;
  }

  async deleteOrganization(uid: string): Promise<boolean> {
    const response = await this.apiCall<any>(`/Org/DeleteOrg?UID=${uid}`, {
      method: "DELETE",
    });
    return response.success;
  }

  // Tax Groups
  async getTaxGroups(): Promise<TaxGroup[]> {
    const pagingRequest = {
      pageNumber: 1,
      pageSize: 1000,
      sortCriterias: [],
      filterCriterias: [],
      isCountRequired: true,
    };

    // Try multiple endpoints as fallback
    const endpoints = [
      "/Tax/GetTaxGroupDetails",
      "/TaxMaster/SelectAllTaxMasterDetails",
    ];

    for (const endpoint of endpoints) {
      const result = await this.apiCall<any>(endpoint, {
        method: "POST",
        body: JSON.stringify(pagingRequest),
      });

      if (result.success && result.data) {
        const items =
          result.data.PagedData || result.data.Data || result.data || [];
        if (items.length > 0) {
          return items;
        }
      }
    }

    return [];
  }

  // Organization Hierarchy
  async getOrganizationHierarchy(): Promise<Organization[]> {
    const { data } = await this.getOrganizations(1, 1000);
    return data;
  }

  async insertOrganizationHierarchy(
    orgUID: string
  ): Promise<{ IsSuccess: boolean; Error?: string }> {
    const response = await this.apiCall<any>("/Org/InsertOrgHierarchy", {
      method: "POST",
      body: JSON.stringify({ orgUID }),
    });
    return {
      IsSuccess: response.success,
      Error: response.success ? undefined : response.message,
    };
  }

  // Build organization hierarchy tree
  buildOrganizationTree(
    organizations: Organization[],
    orgTypeUID?: string
  ): OrganizationHierarchyNode[] {
    let filteredOrgs = [...organizations];

    // Filter by org type if specified
    if (orgTypeUID) {
      filteredOrgs = filteredOrgs.filter(
        (org) => org.OrgTypeUID === orgTypeUID
      );
    }

    const orgMap = new Map<string, OrganizationHierarchyNode>();
    const rootNodes: OrganizationHierarchyNode[] = [];

    // First, create all nodes
    filteredOrgs.forEach((org) => {
      orgMap.set(org.UID, {
        uid: org.UID,
        name: org.Name,
        code: org.Code,
        orgTypeName: org.OrgTypeName || "",
        orgTypeUID: org.OrgTypeUID,
        level: 0,
        children: [],
      });
    });

    // Then, build the tree structure
    filteredOrgs.forEach((org) => {
      const node = orgMap.get(org.UID)!;
      if (org.ParentUID && orgMap.has(org.ParentUID)) {
        const parent = orgMap.get(org.ParentUID)!;
        parent.children.push(node);
        node.level = parent.level + 1;
      } else {
        rootNodes.push(node);
      }
    });

    return rootNodes;
  }

  // Build org type hierarchy
  buildOrgTypeHierarchy(
    orgTypes: OrgType[]
  ): Map<string, OrgType & { level: number; children: OrgType[] }> {
    const typeMap = new Map<
      string,
      OrgType & { level: number; children: OrgType[] }
    >();

    // Initialize all types with level 0
    orgTypes.forEach((type) => {
      typeMap.set(type.UID, { ...type, level: 0, children: [] });
    });

    // Build parent-child relationships and calculate levels
    const calculateLevels = (
      typeUID: string,
      currentLevel: number = 0,
      visited = new Set<string>()
    ): number => {
      if (visited.has(typeUID)) {
        console.warn(
          "Circular reference detected in org_type hierarchy:",
          typeUID
        );
        return currentLevel;
      }

      const type = typeMap.get(typeUID);
      if (!type) return currentLevel;

      visited.add(typeUID);

      if (type.level < currentLevel) {
        type.level = currentLevel;
      }

      // Find all children of this type
      const children = orgTypes.filter((t) => t.ParentUID === typeUID);
      children.forEach((childType) => {
        const child = typeMap.get(childType.UID);
        if (child && !type.children.find((c) => c.UID === childType.UID)) {
          type.children.push(child);
          calculateLevels(childType.UID, currentLevel + 1, new Set(visited));
        }
      });

      visited.delete(typeUID);
      return type.level;
    };

    // Start from root types
    const rootTypes = orgTypes.filter((type) => {
      if (!type.ParentUID || type.ParentUID === "" || type.ParentUID === null) {
        return true;
      }
      const parentExists = orgTypes.find((t) => t.UID === type.ParentUID);
      return !parentExists;
    });

    rootTypes.forEach((rootType) => {
      calculateLevels(rootType.UID, 0);
    });

    return typeMap;
  }

  // Get organizations by type
  async getOrganizationsByType(orgTypeUID: string): Promise<Organization[]> {
    const filters = [
      {
        Name: "OrgTypeUID",
        Value: orgTypeUID,
        Type: 0, // Equal
        FilterType: 0,
      },
    ];

    const { data } = await this.getOrganizations(1, 1000, filters);
    return data;
  }

  // Check if UID is unique
  async isUIDUnique(uid: string, excludeUID?: string): Promise<boolean> {
    const { data } = await this.getOrganizations(1, 1000);
    return !data.some((org) => org.UID === uid && org.UID !== excludeUID);
  }

  // Check if Code is unique
  async isCodeUnique(code: string, excludeUID?: string): Promise<boolean> {
    const { data } = await this.getOrganizations(1, 1000);
    return !data.some((org) => org.Code === code && org.UID !== excludeUID);
  }

  // Get company org types (types that have IsCompanyOrg flag)
  async getCompanyOrgTypes(): Promise<OrgType[]> {
    try {
      // Get all organization types
      const orgTypes = await this.getOrganizationTypes();

      // Filter for company org types
      const companyOrgTypes = orgTypes.filter((type) => type.IsCompanyOrg);

      return companyOrgTypes;
    } catch (error) {
      console.error("Error fetching company org types:", error);
      return [];
    }
  }

  // Get organizations that can be selected as company UIDs
  async getOrganizationsForCompanyUID(): Promise<Organization[]> {
    try {
      // Get company org types
      const companyOrgTypes = await this.getCompanyOrgTypes();

      if (companyOrgTypes.length === 0) {
        // If no specific company org types, return all organizations
        const { data } = await this.getOrganizations(1, 1000);
        return data;
      }

      // Get all organizations
      const { data: allOrgs } = await this.getOrganizations(1, 1000);

      // Create a map of org type UIDs to names for quick lookup
      const orgTypeMap = new Map(
        companyOrgTypes.map((type) => [type.UID, type.Name])
      );

      // Filter organizations that belong to company org types
      return allOrgs
        .filter((org) =>
          companyOrgTypes.some((type) => type.UID === org.OrgTypeUID)
        )
        .map((org) => ({
          ...org,
          OrgTypeName: org.OrgTypeName || orgTypeMap.get(org.OrgTypeUID) || "",
        }));
    } catch (error) {
      console.error("Error fetching organizations for company UID:", error);
      return [];
    }
  }

  // Get locations by parent (for cascading dropdowns)
  async getLocationsByParent(parentUID: string): Promise<Location[]> {
    try {
      const { data } = await locationService.getLocations(1, 1000);
      return data.filter((loc) => loc.ParentUID === parentUID);
    } catch (error) {
      console.error("Error fetching locations by parent:", error);
      return [];
    }
  }

  // Get supplier organizations (divisions) by parent organization UID
  async getSupplierOrganizations(
    parentOrgUID?: string
  ): Promise<Organization[]> {
    try {
      // Get all active organizations without ParentUID filter (since column may not exist)
      const filters = [
        {
          Name: "IsActive",
          Value: true,
          Type: 0, // Equal
          FilterType: 0,
        },
      ];

      const { data } = await this.getOrganizations(1, 1000, filters);

      // Client-side filtering by ParentUID if it exists in the data
      let filteredOrgs = data;

      if (parentOrgUID) {
        // Filter organizations that have the specified parent
        filteredOrgs = data.filter(
          (org) => org.ParentUID && org.ParentUID === parentOrgUID
        );
      } else {
        // Return organizations that have any parent (are supplier orgs/divisions)
        filteredOrgs = data.filter(
          (org) =>
            org.ParentUID && org.ParentUID !== "" && org.ParentUID !== null
        );
      }

      return filteredOrgs
        .map((org) => ({
          ...org,
          // Enhance display name to show hierarchy
          Name: `[${org.Code || "DIV"}] ${org.Name}`,
          // Keep original name for processing
          OriginalName: org.Name,
        }))
        .sort(
          (a, b) => a.OriginalName?.localeCompare(b.OriginalName || "") || 0
        );
    } catch (error) {
      console.error("Error fetching supplier organizations:", error);
      // If error occurs, return empty array to prevent app crash
      return [];
    }
  }

  // Get supplier organizations that belong to a specific parent organization
  async getSupplierOrganizationsByParent(
    parentOrgUID: string
  ): Promise<Organization[]> {
    if (!parentOrgUID) {
      return [];
    }

    try {
      const supplierOrgs = await this.getSupplierOrganizations(parentOrgUID);
      console.log(
        `Found ${supplierOrgs.length} supplier organizations for parent: ${parentOrgUID}`
      );
      return supplierOrgs;
    } catch (error) {
      console.error("Error fetching supplier organizations by parent:", error);
      return [];
    }
  }

  // Validate that supplier org belongs to the selected primary org
  async validateSupplierOrgHierarchy(
    primaryOrgUID: string,
    supplierOrgUID: string
  ): Promise<boolean> {
    if (!primaryOrgUID || !supplierOrgUID) {
      return false;
    }

    try {
      const supplierOrg = await this.getOrganizationByUID(supplierOrgUID);
      return supplierOrg.ParentUID === primaryOrgUID;
    } catch (error) {
      console.error("Error validating supplier org hierarchy:", error);
      return false;
    }
  }

  // Export functionality
  async exportOrganizations(
    format: "csv" | "excel",
    filters?: FilterCriteria[]
  ): Promise<Blob> {
    try {
      // Get all organizations for export (up to 10,000)
      const { data: organizations } = await this.getOrganizations(
        1,
        10000,
        filters || [],
        []
      );

      if (format === "csv") {
        return this.exportToCSV(organizations);
      } else {
        return this.exportToExcel(organizations);
      }
    } catch (error) {
      console.error("Failed to export organizations:", error);
      throw new Error("Failed to export organizations");
    }
  }

  private exportToCSV(organizations: Organization[]): Blob {
    const headers = [
      "Code",
      "Name",
      "Type",
      "Parent Organization",
      "Country",
      "Region",
      "City",
      "Company",
      "Tax Group",
      "Status",
      "Is Active",
      "Early Access",
      "Created By",
      "Created Date",
      "Modified By",
      "Modified Date",
    ];

    const csvContent = [
      headers.join(","),
      ...organizations.map((org) =>
        [
          `"${org.Code || ""}"`,
          `"${org.Name || ""}"`,
          `"${org.OrgTypeName || ""}"`,
          `"${org.ParentName || ""}"`,
          `"${org.CountryName || ""}"`,
          `"${org.RegionName || ""}"`,
          `"${org.CityName || ""}"`,
          `"${org.CompanyName || ""}"`,
          `"${org.TaxGroupName || ""}"`,
          `"${org.Status || ""}"`,
          `"${org.IsActive ? "Active" : "Inactive"}"`,
          `"${org.HasEarlyAccess ? "Yes" : "No"}"`,
          `"${org.CreatedBy || ""}"`,
          `"${
            org.CreatedTime
              ? new Date(org.CreatedTime).toLocaleDateString()
              : ""
          }"`,
          `"${org.ModifiedBy || ""}"`,
          `"${
            org.ModifiedTime
              ? new Date(org.ModifiedTime).toLocaleDateString()
              : ""
          }"`,
        ].join(",")
      ),
    ].join("\n");

    return new Blob([csvContent], { type: "text/csv;charset=utf-8;" });
  }

  private exportToExcel(organizations: Organization[]): Blob {
    // For now, return CSV format with Excel MIME type
    // In the future, could use libraries like xlsx for proper Excel export
    const csvContent = this.exportToCSV(organizations);
    return new Blob([csvContent], {
      type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet;charset=utf-8;",
    });
  }
}

export const organizationService = new OrganizationService();
export default organizationService;
