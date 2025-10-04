/**
 * Store Group Service
 * Handles all Store Group related API calls
 */

import { apiService } from './api';
import { authService } from '@/lib/auth-service';
import {
  IStoreGroup,
  IStoreGroupType,
  StoreGroupListRequest,
  PagedResponse,
  ApiResponse,
  StoreGroupFormData,
  StoreGroupSearchParams,
  FilterCriteria
} from '@/types/storeGroup.types';

class StoreGroupService {
  /**
   * Get all store groups with pagination and filtering
   */
  async getAllStoreGroups(request: StoreGroupListRequest): Promise<PagedResponse<IStoreGroup>> {
    try {
      const response = await apiService.post('/StoreGroup/SelectAllStoreGroup', request);
      
      // Handle different response structures from .NET API
      if (response && typeof response === 'object') {
        let pagedData = [];
        let totalCount = 0;
        
        // If response has Data property with PagedData (wrapped response)
        if (response.Data && response.Data.PagedData !== undefined) {
          pagedData = response.Data.PagedData;
          totalCount = response.Data.TotalCount || 0;
        }
        // Check for capital case PagedData (from .NET API)
        else if (response.PagedData !== undefined) {
          pagedData = response.PagedData;
          totalCount = response.TotalCount || 0;
        }
        
        // Add missing HasChild field to each store group record
        const processedData = pagedData.map((item: any) => ({
          ...item,
          HasChild: item.HasChild ?? false // Default to false if not present
        }));
        
        return {
          PagedData: processedData,
          TotalCount: totalCount
        };
      }
      
      return {
        PagedData: [],
        TotalCount: 0
      };
    } catch (error) {
      console.error('Error fetching store groups:', error);
      throw error;
    }
  }

  /**
   * Get store group by UID
   */
  async getStoreGroupByUID(uid: string): Promise<IStoreGroup | null> {
    try {
      const response = await apiService.get(`/StoreGroup/SelectStoreGroupByUID?UID=${uid}`);
      
      if (response?.Data) {
        return response.Data;
      }
      
      return null;
    } catch (error) {
      console.error('Error fetching store group by UID:', error);
      throw error;
    }
  }

  /**
   * Create new store group
   */
  async createStoreGroup(storeGroup: StoreGroupFormData): Promise<ApiResponse> {
    try {
      const currentUser = authService.getCurrentUser();
      const now = new Date().toISOString();
      
      const payload: IStoreGroup = {
        ...storeGroup as IStoreGroup,
        UID: storeGroup.UID || this.generateUID(storeGroup.Code),
        ItemLevel: storeGroup.ItemLevel || 1,
        HasChild: false,
        CreatedBy: currentUser?.uid || 'ADMIN',
        ModifiedBy: currentUser?.uid || 'ADMIN',
        CreatedTime: now,
        ModifiedTime: now
      };
      
      const response = await apiService.post('/StoreGroup/CreateStoreGroup', payload);
      return response;
    } catch (error) {
      console.error('Error creating store group:', error);
      throw error;
    }
  }

  /**
   * Update existing store group
   */
  async updateStoreGroup(storeGroup: IStoreGroup): Promise<ApiResponse> {
    try {
      const currentUser = authService.getCurrentUser();
      const now = new Date().toISOString();
      
      const payload = {
        ...storeGroup,
        ModifiedBy: currentUser?.uid || 'ADMIN',
        ModifiedTime: now
      };
      
      const response = await apiService.put('/StoreGroup/UpdateStoreGroup', payload);
      return response;
    } catch (error) {
      console.error('Error updating store group:', error);
      throw error;
    }
  }

  /**
   * Delete store group
   */
  async deleteStoreGroup(uid: string): Promise<ApiResponse> {
    try {
      const response = await apiService.delete(`/StoreGroup/DeleteStoreGroup?UID=${uid}`);
      return response;
    } catch (error) {
      console.error('Error deleting store group:', error);
      throw error;
    }
  }

  /**
   * Insert store group hierarchy
   */
  async insertStoreGroupHierarchy(type: string, uid: string): Promise<ApiResponse> {
    try {
      const response = await apiService.post(
        `/StoreGroup/InsertStoreGroupHierarchy?type=${type}&uid=${uid}`,
        {}
      );
      return response;
    } catch (error) {
      console.error('Error inserting store group hierarchy:', error);
      throw error;
    }
  }

  /**
   * Search store groups with filters
   */
  async searchStoreGroups(params: StoreGroupSearchParams): Promise<PagedResponse<IStoreGroup>> {
    const filterCriterias: FilterCriteria[] = [];
    
    if (params.searchTerm) {
      filterCriterias.push({
        PropertyName: 'store_group_hierarchy_level_code_name',
        Name: 'store_group_hierarchy_level_code_name',
        Value: params.searchTerm,
        Operator: 'contains'
      });
    }
    
    if (params.storeGroupTypeUID) {
      filterCriterias.push({
        PropertyName: 'StoreGroupTypeUID',
        Value: params.storeGroupTypeUID,
        Operator: '='
      });
    }
    
    if (params.parentUID) {
      filterCriterias.push({
        PropertyName: 'ParentUID',
        Value: params.parentUID,
        Operator: '='
      });
    }
    
    if (params.itemLevel !== undefined) {
      filterCriterias.push({
        PropertyName: 'ItemLevel',
        Value: params.itemLevel,
        Operator: '='
      });
    }
    
    const request: StoreGroupListRequest = {
      PageNumber: 1,
      PageSize: 100,
      FilterCriterias: filterCriterias,
      SortCriterias: [],  // Remove sort criteria to avoid SQL error
      IsCountRequired: true
    };
    
    return this.getAllStoreGroups(request);
  }

  /**
   * Get all store group types with pagination
   */
  async getAllStoreGroupTypes(request?: StoreGroupListRequest): Promise<PagedResponse<IStoreGroupType>> {
    try {
      const defaultRequest = {
        PageNumber: 1,
        PageSize: 100,
        FilterCriterias: [],
        SortCriterias: [],
        IsCountRequired: true,
        ...request
      };
      
      const response = await apiService.post('/StoreGroupType/SelectAllStoreGroupType', defaultRequest);
      
      if (response?.Data && response.Data.PagedData !== undefined) {
        return {
          PagedData: response.Data.PagedData,
          TotalCount: response.Data.TotalCount || 0
        };
      }
      
      return {
        PagedData: [],
        TotalCount: 0
      };
    } catch (error) {
      console.error('Error fetching store group types:', error);
      throw error;
    }
  }

  /**
   * Get store group types (simple list)
   */
  async getStoreGroupTypes(): Promise<IStoreGroupType[]> {
    try {
      const response = await this.getAllStoreGroupTypes({
        PageNumber: 1,
        PageSize: 100,
        FilterCriterias: [],
        SortCriterias: [],
        IsCountRequired: false
      });
      return response.PagedData || [];
    } catch (error) {
      console.error('Error fetching store group types:', error);
      return [];
    }
  }

  /**
   * Get store group type by UID
   */
  async getStoreGroupTypeByUID(uid: string): Promise<IStoreGroupType | null> {
    try {
      const response = await apiService.get(`/StoreGroupType/SelectStoreGroupTypeByUID?UID=${uid}`);
      return response?.Data || null;
    } catch (error) {
      console.error('Error fetching store group type by UID:', error);
      throw error;
    }
  }

  /**
   * Create store group type
   */
  async createStoreGroupType(storeGroupType: Partial<IStoreGroupType>): Promise<ApiResponse> {
    try {
      const currentUser = authService.getCurrentUser();
      const now = new Date().toISOString();
      
      const payload = {
        ...storeGroupType,
        CreatedBy: currentUser?.uid || 'ADMIN',
        ModifiedBy: currentUser?.uid || 'ADMIN',
        CreatedTime: now,
        ModifiedTime: now
      };
      
      const response = await apiService.post('/StoreGroupType/CreateStoreGroupType', payload);
      return response;
    } catch (error) {
      console.error('Error creating store group type:', error);
      throw error;
    }
  }

  /**
   * Update store group type
   */
  async updateStoreGroupType(storeGroupType: IStoreGroupType): Promise<ApiResponse> {
    try {
      const currentUser = authService.getCurrentUser();
      const now = new Date().toISOString();
      
      const payload = {
        ...storeGroupType,
        ModifiedBy: currentUser?.uid || 'ADMIN',
        ModifiedTime: now
      };
      
      const response = await apiService.put('/StoreGroupType/UpdateStoreGroupType', payload);
      return response;
    } catch (error) {
      console.error('Error updating store group type:', error);
      throw error;
    }
  }

  /**
   * Delete store group type
   */
  async deleteStoreGroupType(uid: string): Promise<ApiResponse> {
    try {
      const response = await apiService.delete(`/StoreGroupType/DeleteStoreGroupType?UID=${uid}`);
      return response;
    } catch (error) {
      console.error('Error deleting store group type:', error);
      throw error;
    }
  }

  /**
   * Generate unique ID for store group
   */
  private generateUID(code: string): string {
    const timestamp = Date.now();
    return `${code}_${timestamp}`;
  }

  /**
   * Build store group hierarchy tree
   */
  buildHierarchyTree(storeGroups: IStoreGroup[]): any[] {
    const map = new Map<string, any>();
    const roots: any[] = [];
    
    // Create a map of all store groups
    storeGroups.forEach(group => {
      map.set(group.UID, { ...group, children: [] });
    });
    
    // Build the tree
    storeGroups.forEach(group => {
      const node = map.get(group.UID)!;
      if (group.ParentUID && map.has(group.ParentUID)) {
        const parent = map.get(group.ParentUID)!;
        parent.children.push(node);
      } else {
        roots.push(node);
      }
    });
    
    return roots;
  }
}

export const storeGroupService = new StoreGroupService();