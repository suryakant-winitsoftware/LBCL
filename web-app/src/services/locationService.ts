import {
  ApiResponse,
  FilterCriteria,
  SortCriteria,
} from "@/types/common.types";

export interface LocationType {
  UID: string;
  Code: string;
  Name: string;
  ParentUID?: string | null;
  LevelNo: number;
  ShowInUI?: boolean;
  ShowInTemplate?: boolean;
  CompanyUID?: string;
}

export interface Location {
  UID: string;
  Code: string;
  Name: string;
  ParentUID?: string | null;
  LocationTypeUID?: string;
  LocationTypeName?: string;
  LocationTypeCode?: string;
  ItemLevel?: number;
  HasChild?: boolean;
  CompanyUID?: string;
  CreatedBy?: string;
  CreatedTime?: string;
  ModifiedBy?: string;
  ModifiedTime?: string;
  ServerAddTime?: string;
  ServerModifiedTime?: string;
}

export interface LocationHierarchyNode {
  uid: string;
  code: string;
  name: string;
  locationTypeUID: string;
  locationTypeName: string;
  levelNo: number;
  children?: LocationHierarchyNode[];
  expanded?: boolean;
}

export interface LocationMapping {
  uid: string;
  linkedItemUID: string;
  linkedItemType: string;
  locationTypeUID: string;
  locationUID: string;
  createdBy?: string;
  createdTime?: string;
  modifiedBy?: string;
  modifiedTime?: string;
}

const API_BASE_URL =
  process.env.NEXT_PUBLIC_API_URL || "http://localhost:8000/api";

class LocationService {
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

  // Location Types
  async getLocationTypes(): Promise<LocationType[]> {
    const pagingRequest = {
      pageNumber: 1,
      pageSize: 1000, // Get all location types
      sortCriterias: [],
      filterCriterias: [],
      isCountRequired: true,
    };

    const response = await this.apiCall<any>(
      "/LocationType/SelectAllLocationTypeDetails",
      {
        method: "POST",
        body: JSON.stringify(pagingRequest),
      }
    );

    if (!response.success || !response.data) {
      throw new Error(response.message || "Failed to fetch location types");
    }

    // Extract paged data
    const items = response.data.PagedData || response.data.Data || [];
    return items;
  }

  async createLocationType(
    locationType: Partial<LocationType>
  ): Promise<LocationType> {
    const response = await this.apiCall<any>(
      "/LocationType/CreateLocationType",
      {
        method: "POST",
        body: JSON.stringify(locationType),
      }
    );
    if (!response.success) {
      throw new Error(response.message || "Failed to create location type");
    }
    return locationType as LocationType;
  }

  async updateLocationType(
    uid: string,
    locationType: Partial<LocationType>
  ): Promise<LocationType> {
    const response = await this.apiCall<any>(
      "/LocationType/UpdateLocationTypeDetails",
      {
        method: "PUT",
        body: JSON.stringify({ ...locationType, UID: uid }),
      }
    );
    if (!response.success) {
      throw new Error(response.message || "Failed to update location type");
    }
    return { ...locationType, UID: uid } as LocationType;
  }

  async deleteLocationType(uid: string): Promise<boolean> {
    const response = await this.apiCall<any>(
      `/LocationType/DeleteLocationTypeDetails?UID=${uid}`,
      {
        method: "DELETE",
      }
    );
    return response.success;
  }

  // Locations
  async getLocations(
    pageNumber: number = 1,
    pageSize: number = 20,
    filters?: FilterCriteria[],
    sorts?: SortCriteria[]
  ): Promise<{ data: Location[]; total: number }> {
    const pagingRequest = {
      pageNumber: pageNumber,
      pageSize: pageSize,
      sortCriterias: sorts || [],
      filterCriterias: filters || [],
      isCountRequired: true,
    };

    const response = await this.apiCall<any>(
      "/Location/SelectAllLocationDetails",
      {
        method: "POST",
        body: JSON.stringify(pagingRequest),
      }
    );

    if (!response.success) {
      throw new Error(response.message || "Failed to fetch locations");
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

  async getLocationById(uid: string): Promise<Location> {
    // Validate and sanitize UID parameter
    if (!uid || typeof uid !== "string" || uid.trim() === "") {
      throw new Error("Invalid location UID provided");
    }

    // Encode the UID to prevent URL injection issues
    const encodedUID = encodeURIComponent(uid.trim());
    const response = await this.apiCall<any>(
      `/Location/GetLocationByUID?UID=${encodedUID}`
    );
    if (!response.success || !response.data) {
      throw new Error(response.message || "Failed to fetch location");
    }
    return response.data.Data || response.data;
  }

  async createLocation(location: Partial<Location>): Promise<Location> {
    const response = await this.apiCall<Location>("/Location/CreateLocation", {
      method: "POST",
      body: JSON.stringify(location),
    });
    if (!response.success || !response.data) {
      throw new Error(response.message || "Failed to create location");
    }
    return response.data;
  }

  async updateLocation(
    uid: string,
    location: Partial<Location>
  ): Promise<Location> {
    const response = await this.apiCall<any>(
      "/Location/UpdateLocationDetails",
      {
        method: "PUT",
        body: JSON.stringify({ ...location, UID: uid }),
      }
    );
    if (!response.success) {
      throw new Error(response.message || "Failed to update location");
    }
    return { ...location, UID: uid } as Location;
  }

  async deleteLocation(uid: string): Promise<boolean> {
    const response = await this.apiCall<any>(
      `/Location/DeleteLocationDetails?UID=${uid}`,
      {
        method: "DELETE",
      }
    );
    return response.success;
  }

  async insertLocationHierarchy(
    type: string,
    uid: string
  ): Promise<{ IsSuccess: boolean; Error?: string }> {
    const params = new URLSearchParams({ type, uid });
    const response = await this.apiCall<any>(
      `/LocationMapping/InsertLocationHierarchy?${params.toString()}`,
      {
        method: "POST",
      }
    );
    return {
      IsSuccess: response.success,
      Error: response.success ? undefined : response.message,
    };
  }

  // Location Hierarchy
  async getLocationHierarchy(): Promise<Location[]> {
    // Get all locations to build hierarchy
    const { data } = await this.getLocations(1, 5000);
    return data;
  }

  buildLocationTree(
    locations: Location[],
    locationTypeUID?: string
  ): LocationHierarchyNode[] {
    let filteredLocations = [...locations];

    // Filter by type if specified
    if (locationTypeUID) {
      // Get locations of the specified type
      const typeLocations = locations.filter(
        (loc) => loc.LocationTypeUID === locationTypeUID
      );

      // Include all children of filtered locations recursively
      const includeChildren = (parentUIDs: string[]) => {
        const children = locations.filter(
          (loc) =>
            loc.ParentUID &&
            parentUIDs.includes(loc.ParentUID) &&
            !filteredLocations.some((fl) => fl.UID === loc.UID)
        );

        if (children.length > 0) {
          filteredLocations = [...filteredLocations, ...children];
          includeChildren(children.map((c) => c.UID));
        }
      };

      filteredLocations = typeLocations;
      includeChildren(filteredLocations.map((loc) => loc.UID));
    }

    const nodeMap = new Map<string, LocationHierarchyNode>();
    const roots: LocationHierarchyNode[] = [];

    // Create nodes
    filteredLocations.forEach((loc) => {
      nodeMap.set(loc.UID, {
        uid: loc.UID,
        code: loc.Code,
        name: loc.Name,
        locationTypeUID: loc.LocationTypeUID || "",
        locationTypeName: loc.LocationTypeName || "",
        levelNo: loc.ItemLevel || 0,
        children: [],
        expanded: false,
      });
    });

    // Build tree structure
    filteredLocations.forEach((loc) => {
      const node = nodeMap.get(loc.UID)!;
      if (loc.ParentUID && nodeMap.has(loc.ParentUID)) {
        const parent = nodeMap.get(loc.ParentUID)!;
        parent.children = parent.children || [];
        parent.children.push(node);
      } else if (!locationTypeUID || loc.LocationTypeUID === locationTypeUID) {
        roots.push(node);
      }
    });

    // Sort nodes alphabetically
    const sortNodes = (nodes: LocationHierarchyNode[]) => {
      nodes.sort((a, b) => a.name.localeCompare(b.name));
      nodes.forEach((node) => {
        if (node.children && node.children.length > 0) {
          sortNodes(node.children);
        }
      });
    };

    sortNodes(roots);
    return roots;
  }

  async getLocationsByParent(parentUID: string): Promise<Location[]> {
    // Get all locations and filter by parent
    const { data } = await this.getLocations(1, 1000);
    return data.filter((loc) => loc.ParentUID === parentUID);
  }

  // Location Search
  async searchLocations(
    searchText: string,
    locationTypeUID?: string
  ): Promise<Location[]> {
    const filters: FilterCriteria[] = [
      {
        Name: "Name",
        Value: searchText,
        Type: 1, // 1 = LIKE
        FilterType: 1,
      },
    ];

    if (locationTypeUID) {
      filters.push({
        Name: "LocationTypeUID",
        Value: locationTypeUID,
        Type: 0, // 0 = Equal
        FilterType: 0,
      });
    }

    const { data } = await this.getLocations(1, 100, filters);
    return data;
  }

  // Utility methods
  async validateLocationCode(
    code: string,
    excludeUID?: string
  ): Promise<boolean> {
    // Check if code exists in locations
    const filters: FilterCriteria[] = [
      {
        Name: "Code",
        Value: code,
        Type: 0, // 0 = Equal
        FilterType: 0,
      },
    ];

    const { data } = await this.getLocations(1, 10, filters);
    const exists = data.some((loc) => loc.UID !== excludeUID);
    return !exists; // Return true if code is valid (doesn't exist)
  }

  async getLocationBreadcrumb(locationUID: string): Promise<Location[]> {
    const { data: allLocations } = await this.getLocations(1, 1000);
    const breadcrumb: Location[] = [];

    let currentLocation = allLocations.find((loc) => loc.UID === locationUID);
    while (currentLocation) {
      breadcrumb.unshift(currentLocation);
      if (currentLocation.ParentUID) {
        currentLocation = allLocations.find(
          (loc) => loc.UID === currentLocation!.ParentUID
        );
      } else {
        break;
      }
    }

    return breadcrumb;
  }

  // Export functionality for Locations
  async exportLocations(
    format: "csv" | "excel",
    filters?: FilterCriteria[],
    searchQuery?: string
  ): Promise<Blob> {
    try {
      // Get all locations for export (up to 10,000)
      const { data: locations } = await this.getLocations(
        1,
        10000,
        filters || [],
        []
      );

      // Apply search filtering if provided
      let filteredLocations = locations;
      if (searchQuery) {
        const searchLower = searchQuery.toLowerCase();
        filteredLocations = locations.filter(
          (location) =>
            location.Name?.toLowerCase().includes(searchLower) ||
            location.Code?.toLowerCase().includes(searchLower)
        );
      }

      if (format === "csv") {
        return this.exportLocationsToCSV(filteredLocations);
      } else {
        return this.exportLocationsToExcel(filteredLocations);
      }
    } catch (error) {
      console.error("Failed to export locations:", error);
      throw new Error("Failed to export locations");
    }
  }

  // Export functionality for Location Types
  async exportLocationTypes(format: "csv" | "excel"): Promise<Blob> {
    try {
      const locationTypes = await this.getLocationTypes();

      if (format === "csv") {
        return this.exportLocationTypesToCSV(locationTypes);
      } else {
        return this.exportLocationTypesToExcel(locationTypes);
      }
    } catch (error) {
      console.error("Failed to export location types:", error);
      throw new Error("Failed to export location types");
    }
  }

  // Export functionality for Location Hierarchy
  async exportLocationHierarchy(
    format: "csv" | "excel",
    searchQuery?: string
  ): Promise<Blob> {
    try {
      const locations = await this.getLocationHierarchy();
      const hierarchyNodes = this.buildLocationTree(locations);

      // Flatten hierarchy for export
      const flattenedData: any[] = [];
      const flattenHierarchy = (
        nodes: LocationHierarchyNode[],
        parentPath = "",
        level = 0
      ) => {
        nodes.forEach((node) => {
          const currentPath = parentPath
            ? `${parentPath} > ${node.name}`
            : node.name;

          flattenedData.push({
            ...node,
            hierarchyPath: currentPath,
            hierarchyLevel: level,
            parentPath: parentPath || "Root",
          });

          if (node.children && node.children.length > 0) {
            flattenHierarchy(node.children, currentPath, level + 1);
          }
        });
      };

      if (hierarchyNodes && Array.isArray(hierarchyNodes)) {
        flattenHierarchy(hierarchyNodes);
      }

      // Apply search filtering if provided
      let filteredData = flattenedData;
      if (searchQuery) {
        const searchLower = searchQuery.toLowerCase();
        filteredData = flattenedData.filter(
          (item) =>
            item.name?.toLowerCase().includes(searchLower) ||
            item.code?.toLowerCase().includes(searchLower) ||
            item.hierarchyPath?.toLowerCase().includes(searchLower)
        );
      }

      if (format === "csv") {
        return this.exportLocationHierarchyToCSV(filteredData);
      } else {
        return this.exportLocationHierarchyToExcel(filteredData);
      }
    } catch (error) {
      console.error("Failed to export location hierarchy:", error);
      throw new Error("Failed to export location hierarchy");
    }
  }

  private exportLocationsToCSV(locations: Location[]): Blob {
    const headers = [
      "Code",
      "Name",
      "Location Type",
      "Parent UID",
      "Item Level",
      "Has Child",
      "Company UID",
      "Created By",
      "Created Date",
      "Modified By",
      "Modified Date",
    ];

    const csvContent = [
      headers.join(","),
      ...locations.map((location) =>
        [
          `"${location.Code || ""}"`,
          `"${location.Name || ""}"`,
          `"${location.LocationTypeName || ""}"`,
          `"${location.ParentUID || ""}"`,
          `"${location.ItemLevel || 0}"`,
          `"${location.HasChild ? "Yes" : "No"}"`,
          `"${location.CompanyUID || ""}"`,
          `"${location.CreatedBy || ""}"`,
          `"${
            location.CreatedTime
              ? new Date(location.CreatedTime).toLocaleDateString()
              : ""
          }"`,
          `"${location.ModifiedBy || ""}"`,
          `"${
            location.ModifiedTime
              ? new Date(location.ModifiedTime).toLocaleDateString()
              : ""
          }"`,
        ].join(",")
      ),
    ].join("\n");

    return new Blob([csvContent], { type: "text/csv;charset=utf-8;" });
  }

  private exportLocationTypesToCSV(locationTypes: LocationType[]): Blob {
    const headers = [
      "Code",
      "Name",
      "Parent UID",
      "Level No",
      "Show In UI",
      "Show In Template",
      "Company UID",
    ];

    const csvContent = [
      headers.join(","),
      ...locationTypes.map((type) =>
        [
          `"${type.Code || ""}"`,
          `"${type.Name || ""}"`,
          `"${type.ParentUID || ""}"`,
          `"${type.LevelNo || 0}"`,
          `"${type.ShowInUI ? "Yes" : "No"}"`,
          `"${type.ShowInTemplate ? "Yes" : "No"}"`,
          `"${type.CompanyUID || ""}"`,
        ].join(",")
      ),
    ].join("\n");

    return new Blob([csvContent], { type: "text/csv;charset=utf-8;" });
  }

  private exportLocationHierarchyToCSV(hierarchyData: any[]): Blob {
    const headers = [
      "Code",
      "Name",
      "Location Type",
      "Level No",
      "Hierarchy Path",
      "Hierarchy Level",
      "Parent Path",
      "Has Children",
    ];

    const csvContent = [
      headers.join(","),
      ...hierarchyData.map((item) =>
        [
          `"${item.code || ""}"`,
          `"${item.name || ""}"`,
          `"${item.locationTypeName || ""}"`,
          `"${item.levelNo || 0}"`,
          `"${item.hierarchyPath || ""}"`,
          `"${item.hierarchyLevel || 0}"`,
          `"${item.parentPath || ""}"`,
          `"${item.children && item.children.length > 0 ? "Yes" : "No"}"`,
        ].join(",")
      ),
    ].join("\n");

    return new Blob([csvContent], { type: "text/csv;charset=utf-8;" });
  }

  private exportLocationsToExcel(locations: Location[]): Blob {
    const csvContent = this.exportLocationsToCSV(locations);
    return new Blob([csvContent], {
      type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet;charset=utf-8;",
    });
  }

  private exportLocationTypesToExcel(locationTypes: LocationType[]): Blob {
    const csvContent = this.exportLocationTypesToCSV(locationTypes);
    return new Blob([csvContent], {
      type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet;charset=utf-8;",
    });
  }

  private exportLocationHierarchyToExcel(hierarchyData: any[]): Blob {
    const csvContent = this.exportLocationHierarchyToCSV(hierarchyData);
    return new Blob([csvContent], {
      type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet;charset=utf-8;",
    });
  }
}

export const locationService = new LocationService();
