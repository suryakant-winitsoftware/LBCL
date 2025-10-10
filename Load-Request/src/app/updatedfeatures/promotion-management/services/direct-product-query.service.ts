/**
 * Dynamic Product Query Service
 * Uses the exact same WINTI API approach as the working SKU management system
 */

import { FinalProduct } from "../components/product-selection/DynamicProductAttributes";
import { skuService, SKUGroup, SKUToGroupMapping } from '@/services/sku/sku.service';
import { PagingRequest } from '@/types/common.types';

export class DirectProductQueryService {

  /**
   * Get ALL products without any filtering - optimized for performance
   */
  async getAllProducts(): Promise<FinalProduct[]> {
    try {
      const allProducts: any[] = [];
      let pageNumber = 1;
      const pageSize = 1000;
      let hasMorePages = true;

      // Fetch ALL pages to get complete product list
      while (hasMorePages) {
        
        const skusResponse = await skuService.getAllSKUs({
          PageNumber: pageNumber,
          PageSize: pageSize,
          FilterCriterias: [],
          SortCriterias: [{ SortParameter: 'SKUCode', Direction: 'Asc' }],
          IsCountRequired: true // Enable count to check if more pages exist
        });

        // Handle the wrapped response from skuService
        const actualResponse = skusResponse.data || skusResponse;
        
        const pageData = actualResponse.PagedData || [];
        const totalRecords = actualResponse.TotalCount || actualResponse.TotalRecords || 0;
        
        
        if (pageData.length > 0) {
          allProducts.push(...pageData);
        }

        // Check if we have more pages
        const currentTotal = pageNumber * pageSize;
        hasMorePages = pageData.length === pageSize && currentTotal < totalRecords;
        
        if (hasMorePages) {
          pageNumber++;
        }
      }


      if (allProducts.length === 0) {
        return [];
      }

      // Convert to FinalProduct format - handle both API response formats
      const products: FinalProduct[] = allProducts.map((sku: any) => {
        const mapped = {
          UID: sku.SKUUID || sku.UID,
          Code: sku.SKUCode || sku.Code,
          Name: sku.SKULongName || sku.LongName || sku.Name,
          GroupName: "All Products",
          GroupTypeName: "Product",
          MRP: 0,
          IsActive: sku.IsActive !== undefined ? sku.IsActive : true
        };
        return mapped;
      });

      return products;

    } catch (error) {
      return [];
    }
  }

  // PERFORMANCE CACHE: Store fetched data to avoid redundant API calls
  private static allGroupsCache: { data: any[], timestamp: number } | null = null;
  private static allMappingsCache: { data: any[], timestamp: number } | null = null;
  private static allSKUsCache: { data: any[], timestamp: number } | null = null;
  private static readonly PERF_CACHE_DURATION = 5 * 60 * 1000; // 5 minutes

  /**
   * Get products for a specific group - OPTIMIZED for instant loading
   * Caches all data on first call, then filters client-side for instant responses
   */
  async getProductsForGroup(groupCode: string, groupName: string): Promise<FinalProduct[]> {
    try {
      // Step 1: Get ALL groups (use cache if available)
      let allGroups: any[] = [];
      if (DirectProductQueryService.allGroupsCache && 
          Date.now() - DirectProductQueryService.allGroupsCache.timestamp < DirectProductQueryService.PERF_CACHE_DURATION) {
        allGroups = DirectProductQueryService.allGroupsCache.data;
      } else {
        const groupsResponse = await skuService.getAllSKUGroups({
          PageNumber: 1,
          PageSize: 1000,
          FilterCriterias: [], // NO server-side filtering - get ALL groups
          SortCriterias: [{ SortParameter: 'ItemLevel', Direction: 'Asc' }],
          IsCountRequired: false
        });
        allGroups = groupsResponse.PagedData || [];
        DirectProductQueryService.allGroupsCache = { data: allGroups, timestamp: Date.now() };
      }
      
      // Find target group by code (client-side filtering like working system)
      const targetGroup = allGroups.find(group => group.Code === groupCode);
      if (!targetGroup) {
        return [];
      }

      // Step 2: Get ALL SKU-to-Group mappings (use cache if available)
      let allMappings: any[] = [];
      if (DirectProductQueryService.allMappingsCache && 
          Date.now() - DirectProductQueryService.allMappingsCache.timestamp < DirectProductQueryService.PERF_CACHE_DURATION) {
        allMappings = DirectProductQueryService.allMappingsCache.data;
      } else {
        let pageNumber = 1;
        let hasMore = true;
        allMappings = [];
        
        // Fetch ALL mappings with pagination to ensure we don't miss any
        while (hasMore) {
          const mappingsResponse = await skuService.getSKUToGroupMappings({
            PageNumber: pageNumber,
            PageSize: 1000,
            FilterCriterias: [], // CRITICAL: No server-side filtering - fetch ALL mappings
            SortCriterias: [],
            IsCountRequired: true
          });
          
          const pageData = mappingsResponse.PagedData || [];
          allMappings = allMappings.concat(pageData);
          
          // Check if there are more pages
          const totalRecords = mappingsResponse.TotalRecords || 0;
          hasMore = pageData.length === 1000 && (pageNumber * 1000) < totalRecords;
          
          if (hasMore) {
            pageNumber++;
          }
        }
        
        DirectProductQueryService.allMappingsCache = { data: allMappings, timestamp: Date.now() };
      }

      // Step 3: Find ALL related groups - dynamically handle any hierarchy structure
      const findAllRelatedGroups = (targetGroup: any): string[] => {
        let allRelevantUIDs: string[] = [];
        
        // Helper function to find all descendants recursively
        const findAllDescendants = (parentUID: string): string[] => {
          const directChildren = allGroups.filter(g => g.ParentUID === parentUID);
          let allDescendants = [parentUID]; // Include the parent itself
          
          // Recursively get all descendants
          directChildren.forEach(child => {
            allDescendants = allDescendants.concat(findAllDescendants(child.UID));
          });
          
          return allDescendants;
        };
        
        // First, get direct descendants
        allRelevantUIDs = findAllDescendants(targetGroup.UID);
        
        // If this group has no direct children, we need to find products differently
        if (allRelevantUIDs.length === 1) {
          // Get the hierarchy level of the selected group
          const targetLevel = targetGroup.ItemLevel || 1;
          
          // Find ALL groups at levels below this one (could be any type)
          const lowerLevelGroups = allGroups.filter(g => 
            (g.ItemLevel || 0) > targetLevel
          );
          
          // If the hierarchy isn't properly connected but groups exist at lower levels,
          // include ALL lower level groups to ensure products are shown
          if (lowerLevelGroups.length > 0) {
            // Include all lower level groups and their descendants
            lowerLevelGroups.forEach(group => {
              const descendants = findAllDescendants(group.UID);
              allRelevantUIDs = allRelevantUIDs.concat(descendants);
            });
            
            // Remove duplicates
            allRelevantUIDs = [...new Set(allRelevantUIDs)];
          }
        }
        
        return allRelevantUIDs;
      };

      // Get ALL UIDs in the hierarchy (target group + all descendants + related groups)
      const allRelevantUIDs = findAllRelatedGroups(targetGroup);

      // Step 4: Get mappings for the target group AND ALL its descendants
      const finalMappings = allMappings.filter(mapping => 
        allRelevantUIDs.includes(mapping.SKUGroupUID)
      );

      if (finalMappings.length === 0) {
        return [];
      }

      // Step 5: Get unique SKU UIDs from mappings
      const skuUIDs = Array.from(new Set(finalMappings.map(mapping => mapping.SKUUID)));

      // Step 6: Get ALL SKUs (use cache if available)
      let allSKUs: any[] = [];
      if (DirectProductQueryService.allSKUsCache && 
          Date.now() - DirectProductQueryService.allSKUsCache.timestamp < DirectProductQueryService.PERF_CACHE_DURATION) {
        allSKUs = DirectProductQueryService.allSKUsCache.data;
      } else {
        let pageNumber = 1;
        let hasMore = true;
        allSKUs = [];
        
        // Fetch ALL SKUs with pagination
        while (hasMore) {
          const skusResponse = await skuService.getAllSKUs({
            PageNumber: pageNumber,
            PageSize: 1000,
            FilterCriterias: [], // NO server-side filtering - get ALL SKUs
            SortCriterias: [{ SortParameter: 'SKUCode', Direction: 'Asc' }],
            IsCountRequired: true
          });
          
          
          // Handle wrapped response structure
          const actualResponse = skusResponse.data || skusResponse;
          const pageData = actualResponse.PagedData || [];
          allSKUs = allSKUs.concat(pageData);
          
          // Check if there are more pages
          const totalRecords = actualResponse.TotalRecords || actualResponse.TotalCount || 0;
          hasMore = pageData.length === 1000 && (pageNumber * 1000) < totalRecords;
          
          if (hasMore) {
            pageNumber++;
          }
        }
        
        DirectProductQueryService.allSKUsCache = { data: allSKUs, timestamp: Date.now() };
      }


      // Step 7: Client-side filtering to match our SKU UIDs (instant with cache)
      // Handle both possible SKU property names (SKUUID or UID)
      const filteredSKUs = allSKUs.filter(sku => 
        skuUIDs.includes(sku.SKUUID || sku.UID)
      );

      // Step 8: Convert to FinalProduct format with proper group names
      const products: FinalProduct[] = filteredSKUs.map(sku => {
        // Find which group this SKU belongs to for better categorization
        const skuUID = sku.SKUUID || sku.UID;
        const mapping = finalMappings.find(m => m.SKUUID === skuUID);
        const skuGroup = allGroups.find(g => g.UID === mapping?.SKUGroupUID);
        
        return {
          UID: skuUID,
          Code: sku.SKUCode || sku.Code,
          Name: sku.SKULongName || sku.LongName || sku.Name,
          GroupName: skuGroup?.Name || groupName,  // Use actual group name if found
          GroupTypeName: "Product",
          MRP: 0,
          IsActive: sku.IsActive !== undefined ? sku.IsActive : true
        };
      });
      
      
      return products;

    } catch (error) {
      return [];
    }
  }

  /**
   * Get all products for ANY selected group at ANY level
   * Completely dynamic - no assumptions about position, naming, or structure
   */
  async getProductsForTopLevel(groupCode: string, groupName: string): Promise<FinalProduct[]> {
    try {

      // Get all groups
      const groupsResponse = await skuService.getAllSKUGroups({
        PageNumber: 1,
        PageSize: 5000,
        FilterCriterias: [],
        SortCriterias: [],
        IsCountRequired: false
      });

      const allGroups = groupsResponse.PagedData || [];
      
      // Find the selected group by code or name
      const selectedGroup = allGroups.find(g => 
        g.Code === groupCode || g.Name === groupName
      );

      // Get all mappings to understand which groups have products
      const mappingsResponse = await skuService.getSKUToGroupMappings({
        PageNumber: 1,
        PageSize: 10000,
        FilterCriterias: [],
        SortCriterias: [],
        IsCountRequired: false
      });

      const allMappings = mappingsResponse.PagedData || [];
      const groupsWithDirectProducts = new Set(allMappings.map(m => m.SKUGroupUID));

      let targetGroups = [];

      if (selectedGroup) {
        
        // Function to get ALL related groups (descendants OR the group itself if it has products)
        const getRelevantGroups = (groupUID: string): any[] => {
          const group = allGroups.find(g => g.UID === groupUID);
          if (!group) return [];

          // Check if this group has direct products
          if (groupsWithDirectProducts.has(groupUID)) {
            // This group has products, include it
            let result = [group];
            
            // Also check for any descendants
            const children = allGroups.filter(g => g.ParentUID === groupUID);
            children.forEach(child => {
              result = [...result, ...getRelevantGroups(child.UID)];
            });
            
            return result;
          } else {
            // No direct products, check descendants
            const children = allGroups.filter(g => g.ParentUID === groupUID);
            if (children.length === 0) {
              // Leaf node with no products - still include it
              return [group];
            }
            
            let result: any[] = [];
            children.forEach(child => {
              result = [...result, ...getRelevantGroups(child.UID)];
            });
            return result;
          }
        };

        targetGroups = getRelevantGroups(selectedGroup.UID);
      } else {
        // Selected group not found - get ALL groups that have products
        targetGroups = allGroups.filter(g => groupsWithDirectProducts.has(g.UID));
      }


      if (targetGroups.length === 0) {
        // Fallback: just get ALL products when no groups found
        const skusResponse = await skuService.getAllSKUs({
          PageNumber: 1,
          PageSize: 500,
          FilterCriterias: [],
          SortCriterias: [{ SortParameter: 'SKUCode', Direction: 'Asc' }],
          IsCountRequired: false
        });

        const products: FinalProduct[] = (skusResponse.PagedData || []).map((sku: any) => ({
          UID: sku.SKUUID || sku.UID,
          Code: sku.SKUCode || sku.Code,
          Name: sku.SKULongName || sku.LongName || sku.Name,
          GroupName: groupName,
          GroupTypeName: "Product",
          MRP: 0,
          IsActive: sku.IsActive !== undefined ? sku.IsActive : true
        }));

        return products;
      }

      // We already have mappings from earlier, no need to fetch again

      // Collect all SKU UIDs from target groups
      const targetGroupUIDs = targetGroups.map(group => group.UID);
      const relevantMappings = allMappings.filter(mapping => 
        targetGroupUIDs.includes(mapping.SKUGroupUID)
      ) || [];


      if (relevantMappings.length === 0) {
        // Another fallback: get all SKUs directly
        const skusResponse = await skuService.getAllSKUs({
          PageNumber: 1,
          PageSize: 500,
          FilterCriterias: [],
          SortCriterias: [{ SortParameter: 'SKUCode', Direction: 'Asc' }],
          IsCountRequired: false
        });

        const products: FinalProduct[] = (skusResponse.PagedData || []).map((sku: any) => ({
          UID: sku.SKUUID || sku.UID,
          Code: sku.SKUCode || sku.Code,
          Name: sku.SKULongName || sku.LongName || sku.Name,
          GroupName: groupName,
          GroupTypeName: "Product",
          MRP: 0,
          IsActive: sku.IsActive !== undefined ? sku.IsActive : true
        }));

        return products;
      }

      // Get all unique SKU UIDs
      const allSKUUIDs = [...new Set(relevantMappings.map(mapping => mapping.SKUUID))];

      // Fetch all SKUs 
      const skusResponse = await skuService.getAllSKUs({
        PageNumber: 1,
        PageSize: 2000,
        FilterCriterias: [],
        SortCriterias: [{ SortParameter: 'SKUCode', Direction: 'Asc' }],
        IsCountRequired: false
      });

      // Filter SKUs that belong to this brand
      const brandSKUs = skusResponse.PagedData?.filter((sku: any) => 
        allSKUUIDs.includes(sku.SKUUID || sku.UID)
      ) || [];


      // Convert to FinalProduct format and group by category
      const products: FinalProduct[] = brandSKUs.map((sku: any) => {
        // Find which group this SKU belongs to
        const skuUID = sku.SKUUID || sku.UID;
        const mapping = relevantMappings.find(m => m.SKUUID === skuUID);
        const group = targetGroups.find(g => g.UID === mapping?.SKUGroupUID);
        
        return {
          UID: skuUID,
          Code: sku.SKUCode || sku.Code,
          Name: sku.SKULongName || sku.LongName || sku.Name,
          GroupName: group?.Name || groupName,
          GroupTypeName: "Category",
          MRP: 0, // Will be populated from price API if needed
          IsActive: sku.IsActive !== undefined ? sku.IsActive : true
        };
      });

      const sampleByCategory = products.reduce((acc, product) => {
        if (!acc[product.GroupName!]) acc[product.GroupName!] = [];
        if (acc[product.GroupName!].length < 2) acc[product.GroupName!].push(product.Name);
        return acc;
      }, {} as Record<string, string[]>);
      
      Object.entries(sampleByCategory).forEach(([category, names]) => {
      });

      return products;

    } catch (error) {
      return [];
    }
  }

  /**
   * FIXED: Get products for cascading selections (parent → child) where child has no code
   * This handles cases like "Farmley → Makhana" where child categories are filtered by name
   */
  async getProductsForCascadingSelection(
    parentCode: string, 
    parentName: string, 
    childCategoryName: string
  ): Promise<FinalProduct[]> {
    try {

      // Step 1: Get ALL groups to find parent and child groups
      const groupsResponse = await skuService.getAllSKUGroups({
        PageNumber: 1,
        PageSize: 1000,
        FilterCriterias: [],
        SortCriterias: [{ SortParameter: 'ItemLevel', Direction: 'Asc' }],
        IsCountRequired: false
      });

      const allGroups = groupsResponse.PagedData || [];
      
      // Find parent group by code
      const parentGroup = allGroups.find(group => group.Code === parentCode);
      if (!parentGroup) {
        return [];
      }
      

      // Find child group by name that is a descendant of the parent
      const findChildInHierarchy = (parentUID: string, targetName: string): SKUGroup | null => {
        // Find direct children of parent
        const directChildren = allGroups.filter(g => g.ParentUID === parentUID);
        
        // Check if any direct child matches the target name
        const directMatch = directChildren.find(c => c.Name === targetName);
        if (directMatch) {
          return directMatch;
        }
        
        // Recursively search in grandchildren
        for (const child of directChildren) {
          const grandchildMatch = findChildInHierarchy(child.UID, targetName);
          if (grandchildMatch) {
            return grandchildMatch;
          }
        }
        
        return null;
      };

      const childGroup = findChildInHierarchy(parentGroup.UID, childCategoryName);
      
      if (!childGroup) {
        return [];
      }
      

      // Step 2: Get products mapped to this specific child group
      const products = await this.getProductsForGroup(childGroup.Code, childGroup.Name);
      
      
      return products;
      
    } catch (error) {
      return [];
    }
  }

  /**
   * Alias for backward compatibility - calls getProductsForTopLevel
   */
  async getProductsForBrand(brandCode: string, brandName: string): Promise<FinalProduct[]> {
    return this.getProductsForTopLevel(brandCode, brandName);
  }

  /**
   * Get available product groups using WINTI APIs
   */
  async getAvailableGroups(): Promise<{ code: string, name: string, type: string }[]> {
    try {

      // Get all groups and group types
      const [groupsResponse, groupTypesResponse] = await Promise.all([
        skuService.getAllSKUGroups({
          PageNumber: 1,
          PageSize: 1000,
          FilterCriterias: [],
          SortCriterias: [{ SortParameter: 'ItemLevel', Direction: 'Asc' }],
          IsCountRequired: false
        }),
        skuService.getAllSKUGroupTypes({
          PageNumber: 1,
          PageSize: 100,
          FilterCriterias: [],
          SortCriterias: [{ SortParameter: 'ItemLevel', Direction: 'Asc' }],
          IsCountRequired: false
        })
      ]);

      const groups = groupsResponse.PagedData || [];
      const groupTypes = groupTypesResponse.PagedData || [];

      // Map groups to the expected format
      const availableGroups = groups.map(group => {
        const groupType = groupTypes.find(gt => gt.UID === group.SKUGroupTypeUID);
        return {
          code: group.Code,
          name: group.Name,
          type: groupType?.Name || "Unknown"
        };
      });

      return availableGroups;
      
    } catch (error) {
      return [];
    }
  }

  /**
   * Preload all data for instant subsequent queries
   * Call this on component mount to warm up the cache
   */
  async preloadAllData(): Promise<void> {
    try {
      // Load groups
      const groupsResponse = await skuService.getAllSKUGroups({
        PageNumber: 1,
        PageSize: 1000,
        FilterCriterias: [],
        SortCriterias: [{ SortParameter: 'ItemLevel', Direction: 'Asc' }],
        IsCountRequired: false
      });
      const allGroups = groupsResponse.PagedData || [];
      
      // Load ALL mappings with pagination
      let allMappings: any[] = [];
      let pageNumber = 1;
      let hasMore = true;
      
      while (hasMore) {
        const mappingsResponse = await skuService.getSKUToGroupMappings({
          PageNumber: pageNumber,
          PageSize: 1000,
          FilterCriterias: [],
          SortCriterias: [],
          IsCountRequired: true
        });
        
        const pageData = mappingsResponse.PagedData || [];
        allMappings = allMappings.concat(pageData);
        
        const totalRecords = mappingsResponse.TotalCount || mappingsResponse.TotalRecords || 0;
        hasMore = pageData.length === 1000 && (pageNumber * 1000) < totalRecords;
        
        if (hasMore) {
          pageNumber++;
        }
      }
      
      // Load ALL SKUs with pagination
      let allSKUs: any[] = [];
      pageNumber = 1;
      hasMore = true;
      
      while (hasMore) {
        const skusResponse = await skuService.getAllSKUs({
          PageNumber: pageNumber,
          PageSize: 1000,
          FilterCriterias: [],
          SortCriterias: [{ SortParameter: 'SKUCode', Direction: 'Asc' }],
          IsCountRequired: true
        });
        
        // Handle wrapped response structure (same as in getProductsForGroup)
        const actualResponse = skusResponse.data || skusResponse;
        const pageData = actualResponse.PagedData || [];
        allSKUs = allSKUs.concat(pageData);
        
        const totalRecords = actualResponse.TotalRecords || actualResponse.TotalCount || 0;
        hasMore = pageData.length === 1000 && (pageNumber * 1000) < totalRecords;
        
        if (hasMore) {
          pageNumber++;
        }
      }
      
      // Cache all data
      DirectProductQueryService.allGroupsCache = { data: allGroups, timestamp: Date.now() };
      DirectProductQueryService.allMappingsCache = { data: allMappings, timestamp: Date.now() };
      DirectProductQueryService.allSKUsCache = { data: allSKUs, timestamp: Date.now() };
      
      
    } catch (error) {
      // Silent fail for preload
    }
  }
}

export const directProductQueryService = new DirectProductQueryService();