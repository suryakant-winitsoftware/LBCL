"use client";

import React, { useState, useEffect, useCallback, useMemo, useRef, useTransition } from "react";
import { Card, CardContent } from "@/components/ui/card";
import { RadioGroup, RadioGroupItem } from "@/components/ui/radio-group";
import { Label } from "@/components/ui/label";
import { Input } from "@/components/ui/input";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { Badge } from "@/components/ui/badge";
import { Checkbox } from "@/components/ui/checkbox";
import { Skeleton } from "@/components/ui/skeleton";
import { Button } from "@/components/ui/button";
import { 
  MapPin, 
  Store, 
  Search, 
  Building2, 
  Globe,
  AlertCircle,
  Check,
  X
} from "lucide-react";
import CascadingOrganizationSelector from "../../organization-hierarchy/CascadingOrganizationSelector";
import { api } from "@/services/api";
import { storeService } from "@/services/storeService";

interface Store {
  uid: string;
  name: string;
  code?: string;
  address?: string;
  orgUID?: string;
}

interface FootprintStepProps {
  footprint: {
    type: "all" | "hierarchy" | "specific";
    dynamicHierarchy?: { [key: string]: string[] };
    selectedOrgs?: string[];
    finalOrgUID?: string;
    companyUID?: string;
    organizationUID?: string;
    selectedStoreGroups?: string[];
    selectedBranches?: string[];
    selectedStores?: string[];
    selectedCustomers?: string[];
    selectedSalesmen?: string[];
    specificStores?: string[];
    organization?: string[];
    location?: string[];
    branch?: string[];
    storeGroup?: string[];
    route?: string[];
    salesPerson?: string[];
    stores?: string[];
    selectedCountries?: string[];
    selectedDivisions?: string[];
    locationHierarchy?: {
      countries: string[];
      divisions: string[];
    };
  };
  onFootprintUpdate: (field: string, value: string | string[] | boolean | any) => void;
}

// Constants for performance optimization
const INITIAL_LOAD_SIZE = 100; // Load first 100 stores immediately
const CHUNK_SIZE = 200; // Load 200 stores per chunk after initial load
const CACHE_DURATION = 10 * 60 * 1000; // 10 minutes cache
const SEARCH_DEBOUNCE = 300; // 300ms debounce for search

// Enhanced cache using sessionStorage for persistence across navigation
const storeCache = {
  get: (key: string): { data: Store[], timestamp: number, totalCount?: number } | null => {
    try {
      const cached = sessionStorage.getItem(`store_cache_${key}`);
      if (cached) {
        const parsed = JSON.parse(cached);
        if (Date.now() - parsed.timestamp < CACHE_DURATION) {
          console.log(`Cache hit for ${key}: ${parsed.data.length} stores`);
          return parsed;
        }
        sessionStorage.removeItem(`store_cache_${key}`);
      }
    } catch (e) {
      console.warn('Cache read error:', e);
    }
    return null;
  },
  set: (key: string, value: { data: Store[], timestamp: number, totalCount?: number }) => {
    try {
      sessionStorage.setItem(`store_cache_${key}`, JSON.stringify(value));
      console.log(`Cached ${value.data.length} stores for ${key}`);
    } catch (e) {
      console.warn('Cache write error:', e);
      // Clear old cache entries if storage is full
      const keys = Object.keys(sessionStorage);
      keys.filter(k => k.startsWith('store_cache_') && k !== `store_cache_${key}`)
        .slice(0, 5)
        .forEach(k => sessionStorage.removeItem(k));
    }
  }
};
const STORES_PER_PAGE = 50; // Virtual scrolling page size

// Debounce hook for search
function useDebounce<T>(value: T, delay: number): T {
  const [debouncedValue, setDebouncedValue] = useState<T>(value);
  
  useEffect(() => {
    const handler = setTimeout(() => {
      setDebouncedValue(value);
    }, delay);
    
    return () => {
      clearTimeout(handler);
    };
  }, [value, delay]);
  
  return debouncedValue;
}

export default function FootprintStep({
  footprint,
  onFootprintUpdate
}: FootprintStepProps) {
  const [footprintType, setFootprintType] = useState<"all" | "hierarchy" | "specific">(
    footprint.type || "all"
  );
  const [searchTerm, setSearchTerm] = useState("");
  const [availableStores, setAvailableStores] = useState<Store[]>([]);
  const [selectedStores, setSelectedStores] = useState<Store[]>([]);
  const [loadingStores, setLoadingStores] = useState(false);
  const [loadingMoreStores, setLoadingMoreStores] = useState(false);
  const [totalStoreCount, setTotalStoreCount] = useState(0);
  const [storeError, setStoreError] = useState<string | null>(null);
  const loadingRef = useRef(false);
  const [isPending, startTransition] = useTransition();
  const [visibleStoreCount, setVisibleStoreCount] = useState(STORES_PER_PAGE);
  const observerRef = useRef<IntersectionObserver | null>(null);
  const loadMoreRef = useRef<HTMLDivElement>(null);
  
  // Debounced search for better performance
  const debouncedSearchTerm = useDebounce(searchTerm, 300);

  const handleFootprintTypeChange = (type: "all" | "hierarchy" | "specific") => {
    setFootprintType(type);
    onFootprintUpdate("type", type);
  };

  // Load stores based on selected organization with chunked loading and caching
  const loadStores = useCallback(async (orgUID: string, append: boolean = false, currentCount: number = 0) => {
    console.log('loadStores called with orgUID:', orgUID, 'append:', append);
    if (!orgUID || loadingRef.current) {
      console.log('Early return - orgUID:', orgUID, 'loadingRef.current:', loadingRef.current);
      return;
    }
    
    // Check cache first (only if not appending)
    if (!append) {
      const cacheKey = `org_${orgUID}`;
      const cached = storeCache.get(cacheKey);
      if (cached) {
        setAvailableStores(cached.data);
        setStoreError(null);
        setVisibleStoreCount(Math.min(STORES_PER_PAGE, cached.data.length));
        return;
      }
    }
    
    loadingRef.current = true;
    startTransition(() => {
      setLoadingStores(true);
      setStoreError(null);
    });
    
    try {
      // Calculate pagination for load more
      let pageNumber = 0;
      let pageSize = INITIAL_LOAD_SIZE;
      
      if (append) {
        // For "Load More", get the next chunk
        pageNumber = Math.floor(currentCount / CHUNK_SIZE);
        pageSize = CHUNK_SIZE;
      }
      
      const requestBody = {
        pageNumber,
        pageSize,
        isCountRequired: !append, // Only get count on first load
        sortCriterias: [{ sortParameter: 'Name', direction: 0 }],
        filterCriterias: orgUID ? [
          {
            name: 'FranchiseeOrgUID',
            value: orgUID,
            operator: 'equals'
          }
        ] : []
      };
      
      const response = await storeService.getAllStores(requestBody);
      console.log(`Store API response for org ${orgUID} (page ${requestBody.pageNumber}):`, response);
      
      if (response.pagedData && response.pagedData.length > 0) {
        console.log('First store in chunk:', response.pagedData[0]);
        const stores: Store[] = response.pagedData.map((store: {
          UID?: string;
          uid?: string;
          Uid?: string;
          Name?: string;
          name?: string;
          Code?: string;
          code?: string;
          Address?: string;
          address?: string;
          OrgUID?: string;
          orgUID?: string;
          OrgUid?: string;
        }) => ({
          uid: store.UID || store.uid || store.Uid,
          name: store.Name || store.name,
          code: store.Code || store.code,
          address: store.Address || store.address,
          orgUID: store.OrgUID || store.orgUID || store.OrgUid
        }));
        
        const totalCount = response.totalCount || stores.length;
        
        startTransition(() => {
          setAvailableStores(prevStores => {
            const allStores = append ? [...prevStores, ...stores] : stores;
            
            // Update cache with all loaded stores
            const cacheKey = `org_${orgUID}`;
            if (!append || allStores.length >= totalCount) {
              storeCache.set(cacheKey, { 
                data: allStores, 
                timestamp: Date.now(),
                totalCount: totalCount
              });
            }
            
            console.log(`Loaded ${stores.length} stores, total: ${allStores.length}/${totalCount}`);
            
            // Store total count for "Load More" functionality
            if (!append) {
              setTotalStoreCount(totalCount);
            }
            
            return allStores;
          });
          
          setStoreError(null);
          setVisibleStoreCount(prev => Math.min(STORES_PER_PAGE, append ? prev + stores.length : stores.length));
        });
      } else {
        startTransition(() => {
          setAvailableStores(prevStores => append ? prevStores : []);
          setStoreError("No stores found for this organization");
        });
      }
    } catch (error) {
      setAvailableStores([]);
      setStoreError(`Error loading stores: ${error instanceof Error ? error.message : 'Unknown error'}`);
    } finally {
      startTransition(() => {
        setLoadingStores(false);
      });
      loadingRef.current = false;
    }
  }, []);

  // Load all stores with caching and pagination
  const loadAllStores = useCallback(async () => {
    if (loadingRef.current) {
      return;
    }
    
    // Check cache first
    const cacheKey = 'all_stores';
    const cached = storeCache.get(cacheKey);
    if (cached && Date.now() - cached.timestamp < CACHE_DURATION) {
      setAvailableStores(cached.data);
      setStoreError(null);
      return;
    }
    
    loadingRef.current = true;
    startTransition(() => {
      setLoadingStores(true);
      setStoreError(null);
    });
    
    try {
      const requestBody = {
        pageNumber: 0,
        pageSize: 1000, // Reduced initial load
        isCountRequired: false,
        sortCriterias: [{ sortParameter: 'Name', direction: 0 }],
        filterCriterias: []
      };
      
      const response = await storeService.getAllStores(requestBody);
      
      if (response.pagedData && response.pagedData.length > 0) {
        const stores: Store[] = response.pagedData.map((store: {
          UID?: string;
          uid?: string;
          Uid?: string;
          Name?: string;
          name?: string;
          Code?: string;
          code?: string;
          Address?: string;
          address?: string;
          OrgUID?: string;
          orgUID?: string;
          OrgUid?: string;
        }) => ({
          uid: store.UID || store.uid || store.Uid,
          name: store.Name || store.name,
          code: store.Code || store.code,
          address: store.Address || store.address,
          orgUID: store.OrgUID || store.orgUID || store.OrgUid
        }));
        
        // Update cache
        storeCache.set(cacheKey, { data: stores, timestamp: Date.now() });
        
        startTransition(() => {
          setAvailableStores(stores);
          setStoreError(null);
          setVisibleStoreCount(STORES_PER_PAGE);
        });
      } else {
        setAvailableStores([]);
        setStoreError("No stores found");
      }
    } catch (error) {
      setAvailableStores([]);
      setStoreError(`Error loading stores: ${error instanceof Error ? error.message : 'Unknown error'}`);
    } finally {
      startTransition(() => {
        setLoadingStores(false);
      });
      loadingRef.current = false;
    }
  }, []);

  // Pre-fetch stores on mount for instant loading
  useEffect(() => {
    const prefetchStores = async () => {
      const cacheKey = 'all_stores';
      const cached = storeCache.get(cacheKey);
      
      // Only prefetch if cache is stale or empty
      if (!cached || Date.now() - cached.timestamp >= CACHE_DURATION) {
        try {
          const requestBody = {
            pageNumber: 0,
            pageSize: 500, // Smaller initial batch
            isCountRequired: false,
            sortCriterias: [{ sortParameter: 'Name', direction: 0 }],
            filterCriterias: []
          };
          
          const response = await storeService.getAllStores(requestBody);
          
          if (response.pagedData && response.pagedData.length > 0) {
            const stores: Store[] = response.pagedData.map((store: any) => ({
              uid: store.UID || store.uid || store.Uid,
              name: store.Name || store.name,
              code: store.Code || store.code,
              address: store.Address || store.address,
              orgUID: store.OrgUID || store.orgUID || store.OrgUid
            }));
            
            // Cache the pre-fetched data
            storeCache.set(cacheKey, { data: stores, timestamp: Date.now() });
          }
        } catch (error) {
          // Silent fail for pre-fetch
        }
      }
    };
    
    // Run prefetch in background
    prefetchStores();
  }, []);
  
  // Load stores based on footprint type
  useEffect(() => {
    if (footprintType === "specific") {
      // Load all stores for specific selection
      loadAllStores();
    } else if (footprintType === "hierarchy") {
      // Use finalOrgUID if available, as it contains the correct organization UID
      if (footprint.finalOrgUID) {
        console.log('UseEffect: Loading stores for finalOrgUID:', footprint.finalOrgUID);
        loadStores(footprint.finalOrgUID);
      } else {
        console.log('UseEffect: No finalOrgUID, clearing stores');
        setAvailableStores([]);
        setSelectedStores([]);
      }
    } else {
      // For "all" type, clear the stores
      setAvailableStores([]);
      setSelectedStores([]);
    }
  }, [footprint.finalOrgUID, footprintType, loadStores, loadAllStores]);

  // Handle store selection
  const handleStoreSelection = (store: Store, checked: boolean) => {
    let updatedStores: Store[];
    if (checked) {
      updatedStores = [...selectedStores, store];
    } else {
      updatedStores = selectedStores.filter(s => s.uid !== store.uid);
    }
    setSelectedStores(updatedStores);
    
    // Pass both UIDs and full store objects for review in a single batch update
    onFootprintUpdate("_batch", {
      ...footprint,
      selectedStores: updatedStores.map(s => s.uid),
      specificStores: updatedStores.map(s => ({
        uid: s.uid,
        storeName: s.name,
        storeCode: s.code,
        address: s.address
      }))
    });
  };

  const handleSelectAllStores = (checked: boolean | "indeterminate") => {
    if (checked === true) {
      // Only select filtered stores if search is active
      const storesToSelect = filteredStores.length > 0 ? filteredStores : availableStores;
      setSelectedStores([...storesToSelect]);
      onFootprintUpdate("_batch", {
        ...footprint,
        selectedStores: storesToSelect.map(s => s.uid),
        specificStores: storesToSelect.map(s => ({
          uid: s.uid,
          storeName: s.name,
          storeCode: s.code,
          address: s.address
        }))
      });
    } else {
      setSelectedStores([]);
      onFootprintUpdate("_batch", {
        ...footprint,
        selectedStores: [],
        specificStores: []
      });
    }
  };

  // Memoized filtered stores with debounced search
  const filteredStores = useMemo(() => {
    if (!debouncedSearchTerm) return availableStores;
    
    const searchLower = debouncedSearchTerm.toLowerCase();
    return availableStores.filter(store => 
      store.name?.toLowerCase().includes(searchLower) ||
      store.code?.toLowerCase().includes(searchLower) ||
      store.address?.toLowerCase().includes(searchLower)
    );
  }, [availableStores, debouncedSearchTerm]);
  
  // Virtual scrolling - only show visible stores
  const visibleStores = useMemo(() => {
    const visible = filteredStores.slice(0, visibleStoreCount);
    return visible;
  }, [filteredStores, visibleStoreCount]);
  
  // Set up intersection observer for infinite scrolling
  useEffect(() => {
    if (loadMoreRef.current) {
      observerRef.current = new IntersectionObserver(
        (entries) => {
          if (entries[0].isIntersecting && visibleStoreCount < filteredStores.length) {
            startTransition(() => {
              setVisibleStoreCount(prev => Math.min(prev + STORES_PER_PAGE, filteredStores.length));
            });
          }
        },
        { threshold: 0.1 }
      );
      observerRef.current.observe(loadMoreRef.current);
    }
    
    return () => {
      if (observerRef.current) {
        observerRef.current.disconnect();
      }
    };
  }, [visibleStoreCount, filteredStores.length, startTransition, setVisibleStoreCount]);
  
  // Reset visible count when search changes
  useEffect(() => {
    setVisibleStoreCount(STORES_PER_PAGE);
  }, [debouncedSearchTerm, setVisibleStoreCount]);

  return (
    <div className="max-w-[75%] space-y-4">
      {/* Header */}
      <div className="mb-6">
        <h2 className="text-xl font-medium text-gray-900">Promotion Footprint</h2>
      </div>

      {/* Coverage Type Selection */}
      <div className="border border-gray-200 rounded-lg p-5">
          <div className="space-y-4">
            <h3 className="text-sm font-semibold text-gray-900 mb-4">
              Coverage Type <span className="text-red-500">*</span>
            </h3>
            
            <RadioGroup
              value={footprintType}
              onValueChange={(value) => handleFootprintTypeChange(value as "all" | "hierarchy" | "specific")}
              className="grid grid-cols-1 md:grid-cols-3 gap-4"
            >
              <label
                htmlFor="all"
                className={`relative flex cursor-pointer rounded-lg border ${
                  footprintType === "all" 
                    ? "border-blue-500 bg-blue-50" 
                    : "border-gray-200 bg-white hover:bg-gray-50"
                } p-4 transition-all`}
              >
                <RadioGroupItem value="all" id="all" className="sr-only" />
                <div className="flex flex-col">
                  <div className="flex items-center gap-2 mb-2">
                    <Globe className="w-5 h-5 text-blue-600" />
                    <span className="font-medium text-gray-900">All Stores</span>
                  </div>
                  <p className="text-xs text-gray-500">Apply to all organizations network-wide</p>
                </div>
                {footprintType === "all" && (
                  <Check className="absolute top-3 right-3 w-5 h-5 text-blue-600" />
                )}
              </label>

              <label
                htmlFor="hierarchy"
                className={`relative flex cursor-pointer rounded-lg border ${
                  footprintType === "hierarchy" 
                    ? "border-blue-500 bg-blue-50" 
                    : "border-gray-200 bg-white hover:bg-gray-50"
                } p-4 transition-all`}
              >
                <RadioGroupItem value="hierarchy" id="hierarchy" className="sr-only" />
                <div className="flex flex-col">
                  <div className="flex items-center gap-2 mb-2">
                    <Building2 className="w-5 h-5 text-purple-600" />
                    <span className="font-medium text-gray-900">By Hierarchy</span>
                  </div>
                  <p className="text-xs text-gray-500">Select by organization</p>
                </div>
                {footprintType === "hierarchy" && (
                  <Check className="absolute top-3 right-3 w-5 h-5 text-blue-600" />
                )}
              </label>

              <label
                htmlFor="specific"
                className={`relative flex cursor-pointer rounded-lg border ${
                  footprintType === "specific" 
                    ? "border-blue-500 bg-blue-50" 
                    : "border-gray-200 bg-white hover:bg-gray-50"
                } p-4 transition-all`}
              >
                <RadioGroupItem value="specific" id="specific" className="sr-only" />
                <div className="flex flex-col">
                  <div className="flex items-center gap-2 mb-2">
                    <Store className="w-5 h-5 text-green-600" />
                    <span className="font-medium text-gray-900">Specific Stores</span>
                  </div>
                  <p className="text-xs text-gray-500">Hand-pick stores</p>
                </div>
                {footprintType === "specific" && (
                  <Check className="absolute top-3 right-3 w-5 h-5 text-blue-600" />
                )}
              </label>
            </RadioGroup>
          </div>
      </div>

      {/* Content based on selection */}
      {footprintType === "all" && (
        <div className="border border-gray-200 rounded-lg p-5">
            <div className="flex items-start gap-4">
              <div className="w-12 h-12 rounded-full bg-blue-100 flex items-center justify-center flex-shrink-0">
                <Globe className="w-6 h-6 text-blue-600" />
              </div>
              <div className="flex-1">
                <h4 className="font-medium text-gray-900 mb-1">All Stores Coverage Selected</h4>
                <p className="text-sm text-gray-600">
                  This promotion will be available across all organizations and their stores in your network. 
                  No additional configuration required.
                </p>
              </div>
            </div>
        </div>
      )}

      {footprintType === "hierarchy" && (
        <div className="space-y-4">
          {/* Organization Selection */}
          <div className="border border-gray-200 rounded-lg p-5">
              <div className="space-y-4">
                <h3 className="text-sm font-semibold text-gray-900 mb-4">
                  Organization Selection
                </h3>
                  
                <CascadingOrganizationSelector
                  selectedOrgs={footprint.selectedOrgs || []}
                  onSelectionChange={(selectedOrgs, finalOrgObj, hierarchyData) => {
                    // Extract UID from the organization object
                    const finalOrgUID = finalOrgObj?.UID || finalOrgObj?.uid || finalOrgObj;
                    
                    // Extract company and organization UIDs from hierarchy data
                    // For our structure: company (single) -> organization (single) -> stores (multiple)
                    let companyUID = undefined;
                    let organizationUID = undefined;
                    
                    // Look for company and organization in the hierarchy data
                    if (hierarchyData && typeof hierarchyData === 'object') {
                      // Find the company level (usually the highest level)
                      const companyKey = Object.keys(hierarchyData).find(key => 
                        key.toLowerCase().includes('company') || 
                        key.toLowerCase().includes('franchisee') ||
                        key === 'level1' // fallback to level1 as company
                      );
                      
                      if (companyKey && hierarchyData[companyKey]?.length > 0) {
                        companyUID = hierarchyData[companyKey][0]; // Single company
                      }
                      
                      // Use finalOrgUID as the organization
                      organizationUID = finalOrgUID;
                    }
                    
                    console.log('Organization selection changed:', {
                      companyUID,
                      organizationUID,
                      finalOrgUID,
                      hierarchyData
                    });
                    
                    // Update all footprint fields at once to avoid race conditions
                    const updatedFootprint = {
                      ...footprint,
                      selectedOrgs: selectedOrgs || [],
                      finalOrgUID: finalOrgUID || undefined,
                      companyUID: companyUID || undefined,
                      organizationUID: organizationUID || undefined,
                      dynamicHierarchy: hierarchyData || {}
                    };
                    
                    // Update parent state with the complete footprint object
                    onFootprintUpdate("_batch", updatedFootprint);
                    
                    // Immediately load stores for the selected organization
                    if (footprintType === "hierarchy" && finalOrgUID) {
                      console.log('Loading stores for org UID:', finalOrgUID);
                      loadStores(finalOrgUID);
                    } else {
                      console.log('Clearing stores - footprintType:', footprintType, 'finalOrgUID:', finalOrgUID);
                      setAvailableStores([]);
                      setSelectedStores([]);
                    }
                  }}
                  multiSelect={true}
                  placeholder="Select from organization hierarchy"
                  compact={false}
                />
              </div>
          </div>

          {/* Store Selection */}
          <div className="border border-gray-200 rounded-lg p-5">
              <div className="space-y-4">
                <div className="flex items-center justify-between mb-4">
                  <h3 className="text-sm font-semibold text-gray-900">
                    Store Selection
                  </h3>
                  {availableStores.length > 0 && (
                    <span className="text-xs text-gray-500">
                      {availableStores.length} stores available
                    </span>
                  )}
                </div>
                  
                {/* Check if organization is selected - only for hierarchy mode */}
                {footprintType === "hierarchy" && (!footprint.selectedOrgs || footprint.selectedOrgs.length === 0) && !footprint.finalOrgUID ? (
                  <div className="flex flex-col items-center justify-center py-8 px-4 bg-gray-50 rounded-lg">
                    <div className="w-12 h-12 rounded-full bg-amber-100 flex items-center justify-center mb-3">
                      <AlertCircle className="w-6 h-6 text-amber-600" />
                    </div>
                    <p className="text-sm font-medium text-gray-900 mb-1">No Organization Selected</p>
                    <p className="text-xs text-gray-500 text-center">
                      Please select an organization above to view available stores
                    </p>
                  </div>
                ) : (
                  <div className="space-y-4">

                    {/* Store Selector */}
                    {loadingStores ? (
                      <div className="space-y-3">
                        {/* Skeleton for select all */}
                        <div className="p-3 bg-gray-50 rounded-lg">
                          <div className="flex items-center justify-between">
                            <div className="flex items-center gap-3">
                              <Skeleton className="h-4 w-4 rounded" />
                              <Skeleton className="h-4 w-32" />
                            </div>
                            <Skeleton className="h-5 w-20 rounded-full" />
                          </div>
                        </div>
                        {/* Skeleton for store list */}
                        <div className="border border-gray-200 rounded-lg overflow-hidden">
                          <div className="space-y-0">
                            {[...Array(5)].map((_, i) => (
                              <div key={i} className="flex items-center gap-3 px-4 py-3 border-b last:border-b-0">
                                <Skeleton className="h-4 w-4 rounded" />
                                <div className="flex-1">
                                  <Skeleton className="h-4 w-48 mb-1" />
                                  <Skeleton className="h-3 w-32" />
                                </div>
                                <Skeleton className="h-3 w-8" />
                              </div>
                            ))}
                          </div>
                        </div>
                      </div>
                    ) : storeError ? (
                      <div className="flex flex-col items-center justify-center py-8 px-4 bg-red-50 rounded-lg">
                        <div className="w-12 h-12 rounded-full bg-red-100 flex items-center justify-center mb-3">
                          <X className="w-6 h-6 text-red-600" />
                        </div>
                        <p className="text-sm font-medium text-gray-900 mb-1">Error Loading Stores</p>
                        <p className="text-xs text-red-600 text-center">{storeError}</p>
                      </div>
                    ) : availableStores.length === 0 ? (
                      <div className="flex flex-col items-center justify-center py-8 px-4 bg-gray-50 rounded-lg">
                        <div className="w-12 h-12 rounded-full bg-gray-100 flex items-center justify-center mb-3">
                          <Store className="w-6 h-6 text-gray-400" />
                        </div>
                        <p className="text-sm font-medium text-gray-900 mb-1">No Stores Available</p>
                        <p className="text-xs text-gray-500 text-center">
                          No stores found for the selected organization
                        </p>
                      </div>
                    ) : (
                      <div className="space-y-3">
                        {/* Store Selection Controls */}
                        <div className="flex items-center justify-between p-3 bg-gray-50 rounded-lg">
                          <div className="flex items-center gap-3">
                            <Checkbox
                              id="select-all-stores"
                              checked={selectedStores.length === filteredStores.length && filteredStores.length > 0}
                              onCheckedChange={handleSelectAllStores}
                              className="data-[state=checked]:bg-blue-600"
                            />
                            <Label htmlFor="select-all-stores" className="text-sm font-medium text-gray-700 cursor-pointer">
                              Select All Stores
                            </Label>
                          </div>
                          <div className="flex items-center gap-2">
                            {availableStores.length < totalStoreCount && (
                              <Button
                                variant="outline"
                                size="sm"
                                onClick={() => {
                                  setLoadingMoreStores(true);
                                  loadStores(footprint.finalOrgUID!, true, availableStores.length).finally(() => {
                                    setLoadingMoreStores(false);
                                  });
                                }}
                                disabled={loadingMoreStores}
                                className="text-xs"
                              >
                                {loadingMoreStores ? (
                                  <>Loading...</>
                                ) : (
                                  <>Load More ({totalStoreCount - availableStores.length})</>
                                )}
                              </Button>
                            )}
                            <Badge variant={selectedStores.length > 0 ? "default" : "secondary"} className="text-xs">
                              {selectedStores.length} / {filteredStores.length} selected
                            </Badge>
                          </div>
                        </div>

                        {/* Store List */}
                        <div className="border border-gray-200 rounded-lg overflow-hidden">
                          <div className="max-h-96 overflow-y-auto">
                            {visibleStores.map((store, index) => {
                              const isSelected = selectedStores.some(s => s.uid === store.uid);
                              return (
                                <div
                                  key={store.uid}
                                  className={`
                                    flex items-center gap-3 px-4 py-3 border-b last:border-b-0 
                                    transition-colors cursor-pointer
                                    ${isSelected 
                                      ? 'bg-blue-50 hover:bg-blue-100 border-blue-200' 
                                      : 'bg-white hover:bg-gray-50 border-gray-100'
                                    }
                                  `}
                                  onClick={() => handleStoreSelection(store, !isSelected)}
                                >
                                  <Checkbox
                                    id={`store-${store.uid}`}
                                    checked={isSelected}
                                    onCheckedChange={(checked) => handleStoreSelection(store, checked === true)}
                                    className="data-[state=checked]:bg-blue-600"
                                    onClick={(e) => e.stopPropagation()}
                                  />
                                  <div className="flex-1 min-w-0">
                                    <div className="flex items-start justify-between">
                                      <div className="flex-1">
                                        <div className="flex items-center gap-2">
                                          <p className="text-sm font-medium text-gray-900">
                                            {store.name}
                                          </p>
                                          {store.code && (
                                            <Badge variant="outline" className="text-xs">
                                              {store.code}
                                            </Badge>
                                          )}
                                        </div>
                                        {store.address && (
                                          <p className="text-xs text-gray-500 mt-0.5">
                                            <MapPin className="w-3 h-3 inline mr-1" />
                                            {store.address}
                                          </p>
                                        )}
                                      </div>
                                      <span className="text-xs text-gray-400">
                                        #{index + 1}
                                      </span>
                                    </div>
                                  </div>
                                </div>
                              );
                            })}
                            {visibleStores.length < filteredStores.length && (
                              <div ref={loadMoreRef} className="p-3 text-center">
                                <Skeleton className="h-8 w-32 mx-auto" />
                              </div>
                            )}
                          </div>
                        </div>

                        {/* Store Selection Summary */}
                        {footprint.finalOrgUID && (
                          <Alert className="bg-blue-50 border-blue-200">
                            <Check className="h-4 w-4 text-blue-600" />
                            <AlertDescription className="text-blue-800">
                              {selectedStores.length > 0 ? (
                                <span><strong>Selected:</strong> {selectedStores.length} specific store{selectedStores.length !== 1 ? 's' : ''} from this organization</span>
                              ) : (
                                <span><strong>Coverage:</strong> ALL stores within the selected organization will be included</span>
                              )}
                            </AlertDescription>
                          </Alert>
                        )}
                      </div>
                    )}
                  </div>
                )}
              </div>
          </div>
        </div>
          )}

      {footprintType === "specific" && (
        <div className="border border-gray-200 rounded-lg p-5">
            <div className="space-y-4">
              <div className="flex items-center justify-between mb-4">
                <h3 className="text-sm font-semibold text-gray-900">
                  Specific Store Selection
                </h3>
                {availableStores.length > 0 && (
                  <span className="text-xs text-gray-500">
                    {availableStores.length} stores available
                  </span>
                )}
              </div>
              
              {/* Store search */}
              <div className="relative">
                <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 w-4 h-4 text-gray-400" />
                <Input
                  type="text"
                  placeholder="Search stores by name or code..."
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                  className="pl-10 h-10"
                />
              </div>

              {/* Store Selector */}
              {loadingStores ? (
                <div className="space-y-3">
                  {/* Skeleton for select all */}
                  <div className="p-3 bg-gray-50 rounded-lg">
                    <div className="flex items-center justify-between">
                      <div className="flex items-center gap-3">
                        <Skeleton className="h-4 w-4 rounded" />
                        <Skeleton className="h-4 w-32" />
                      </div>
                      <Skeleton className="h-5 w-20 rounded-full" />
                    </div>
                  </div>
                  {/* Skeleton for store list */}
                  <div className="border border-gray-200 rounded-lg overflow-hidden">
                    <div className="space-y-0">
                      {[...Array(5)].map((_, i) => (
                        <div key={i} className="flex items-center gap-3 px-4 py-3 border-b last:border-b-0">
                          <Skeleton className="h-4 w-4 rounded" />
                          <div className="flex-1">
                            <Skeleton className="h-4 w-48 mb-1" />
                            <Skeleton className="h-3 w-32" />
                          </div>
                          <Skeleton className="h-3 w-8" />
                        </div>
                      ))}
                    </div>
                  </div>
                </div>
              ) : storeError ? (
                <div className="flex flex-col items-center justify-center py-8 px-4 bg-red-50 rounded-lg">
                  <div className="w-12 h-12 rounded-full bg-red-100 flex items-center justify-center mb-3">
                    <X className="w-6 h-6 text-red-600" />
                  </div>
                  <p className="text-sm font-medium text-gray-900 mb-1">Error Loading Stores</p>
                  <p className="text-xs text-red-600 text-center">{storeError}</p>
                </div>
              ) : availableStores.length === 0 ? (
                <div className="flex flex-col items-center justify-center py-8 px-4 bg-gray-50 rounded-lg">
                  <div className="w-12 h-12 rounded-full bg-gray-100 flex items-center justify-center mb-3">
                    <Store className="w-6 h-6 text-gray-400" />
                  </div>
                  <p className="text-sm font-medium text-gray-900 mb-1">No Stores Available</p>
                  <p className="text-xs text-gray-500 text-center">
                    No stores found in the system
                  </p>
                </div>
              ) : (
                <div className="space-y-3">
                  {/* Store Selection Controls */}
                  <div className="flex items-center justify-between p-3 bg-gray-50 rounded-lg">
                    <div className="flex items-center gap-3">
                      <Checkbox
                        id="select-all-specific-stores"
                        checked={selectedStores.length === filteredStores.length && filteredStores.length > 0}
                        onCheckedChange={handleSelectAllStores}
                        className="data-[state=checked]:bg-blue-600"
                      />
                      <Label htmlFor="select-all-specific-stores" className="text-sm font-medium text-gray-700 cursor-pointer">
                        Select All Stores
                      </Label>
                    </div>
                    <div className="flex items-center gap-2">
                      <Badge variant={selectedStores.length > 0 ? "default" : "secondary"} className="text-xs">
                        {selectedStores.length} / {availableStores.length} selected
                      </Badge>
                    </div>
                  </div>

                  {/* Store List with Search Filter */}
                  <div className="border border-gray-200 rounded-lg overflow-hidden">
                    <div className="max-h-96 overflow-y-auto">
                      {visibleStores.map((store, index) => {
                          const isSelected = selectedStores.some(s => s.uid === store.uid);
                          return (
                            <div
                              key={store.uid}
                              className={`
                                flex items-center gap-3 px-4 py-3 border-b last:border-b-0 
                                transition-colors cursor-pointer
                                ${isSelected 
                                  ? 'bg-blue-50 hover:bg-blue-100 border-blue-200' 
                                  : 'bg-white hover:bg-gray-50 border-gray-100'
                                }
                              `}
                              onClick={() => handleStoreSelection(store, !isSelected)}
                            >
                              <Checkbox
                                id={`specific-store-${store.uid}`}
                                checked={isSelected}
                                onCheckedChange={(checked) => handleStoreSelection(store, checked === true)}
                                className="data-[state=checked]:bg-blue-600"
                                onClick={(e) => e.stopPropagation()}
                              />
                              <div className="flex-1 min-w-0">
                                <div className="flex items-start justify-between">
                                  <div className="flex-1">
                                    <div className="flex items-center gap-2">
                                      <p className="text-sm font-medium text-gray-900">
                                        {store.name}
                                      </p>
                                      {store.code && (
                                        <Badge variant="outline" className="text-xs">
                                          {store.code}
                                        </Badge>
                                      )}
                                    </div>
                                    {store.address && (
                                      <p className="text-xs text-gray-500 mt-0.5">
                                        <MapPin className="w-3 h-3 inline mr-1" />
                                        {store.address}
                                      </p>
                                    )}
                                  </div>
                                  <span className="text-xs text-gray-400">
                                    #{index + 1}
                                  </span>
                                </div>
                              </div>
                            </div>
                          );
                        })}
                      {visibleStores.length < filteredStores.length && (
                        <div ref={loadMoreRef} className="p-3 text-center">
                          {isPending ? (
                            <Skeleton className="h-8 w-32 mx-auto" />
                          ) : (
                            <p className="text-xs text-gray-500">Loading more stores...</p>
                          )}
                        </div>
                      )}
                    </div>
                  </div>

                  {/* Selected Stores Summary */}
                  {selectedStores.length > 0 && (
                    <Alert className="bg-blue-50 border-blue-200">
                      <Check className="h-4 w-4 text-blue-600" />
                      <AlertDescription className="text-blue-800">
                        <strong>Selected:</strong> {selectedStores.length} store{selectedStores.length !== 1 ? 's' : ''} selected for this promotion
                      </AlertDescription>
                    </Alert>
                  )}
                </div>
              )}
            </div>
        </div>
      )}

      {/* Coverage Summary */}
      <div className="border border-gray-200 rounded-lg p-5">
          <h3 className="text-sm font-semibold text-gray-900 mb-4">
            Coverage Summary
          </h3>
          <div className="grid grid-cols-3 gap-4">
            <div className="text-center">
              <div className="inline-flex items-center justify-center w-12 h-12 rounded-full bg-purple-100 mb-2">
                <Store className="w-6 h-6 text-purple-600" />
              </div>
              <div className="text-2xl font-bold text-gray-900">
                {footprintType === "all" 
                  ? "All" 
                  : selectedStores.length || 0}
              </div>
              <div className="text-xs text-gray-500">
                {footprintType === "hierarchy" ? "Selected Stores" : "Total Stores"}
              </div>
            </div>
            <div className="text-center">
              <div className="inline-flex items-center justify-center w-12 h-12 rounded-full bg-green-100 mb-2">
                <MapPin className="w-6 h-6 text-green-600" />
              </div>
              <div className="text-2xl font-bold text-gray-900">
                {footprintType === "all" 
                  ? "100%" 
                  : selectedStores.length > 0 && availableStores.length > 0
                  ? `${Math.round((selectedStores.length / availableStores.length) * 100)}%`
                  : "0%"}
              </div>
              <div className="text-xs text-gray-500">Coverage</div>
            </div>
            <div className="text-center">
              <div className="inline-flex items-center justify-center w-12 h-12 rounded-full bg-blue-100 mb-2">
                <Building2 className="w-6 h-6 text-blue-600" />
              </div>
              <div className="text-lg font-bold text-gray-900">
                {footprintType === "all" 
                  ? "Global" 
                  : footprintType === "hierarchy" 
                  ? "Org-based"
                  : "Manual"}
              </div>
              <div className="text-xs text-gray-500">Type</div>
            </div>
          </div>
      </div>

    </div>
  );
}