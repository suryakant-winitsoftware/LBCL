import { useState, useEffect } from "react";
import { API_CONFIG, getCommonHeaders } from "../services/api-config";
import { SKUGroupType } from "../components/product-selection/DynamicProductSelector";

export function useProductHierarchy(orgUid?: string, showEmptyTypes: boolean = false) {
  const [data, setData] = useState<SKUGroupType[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<Error | null>(null);

  useEffect(() => {
    const fetchHierarchy = async () => {
      console.log("🔍 useProductHierarchy: Starting fetch...");
      setIsLoading(true);
      setError(null);
      
      try {
        // Debug auth token
        const token = typeof window !== 'undefined' ? localStorage.getItem('auth_token') : null;
        console.log("🔑 Auth token:", token ? `${token.substring(0, 20)}...` : 'No token found');
        
        const response = await fetch(
          `${API_CONFIG.baseURL}${API_CONFIG.endpoints.skuGroupType.selectAll}`,
          {
            method: "POST",
            headers: getCommonHeaders(),
            body: JSON.stringify({
              PageNumber: 1,
              PageSize: 100,
              SortCriterias: [{ SortParameter: 'ItemLevel', Direction: 'Asc' }],
              FilterCriterias: [],
              IsCountRequired: false
            })
          }
        );

        if (!response.ok) {
          throw new Error("Failed to load product hierarchy");
        }

        const result = await response.json();
        console.log("📊 Product Hierarchy API Response:", result);
        console.log("📈 Response status:", response.status, response.statusText);
        
        // Handle API response structure - the API returns PagedData directly
        const items = result?.PagedData || result?.Data?.PagedData || [];
        console.log("📋 Extracted items:", items);
        console.log("🔢 Items length:", items.length);
        
        // Sort group types by ItemLevel (no filtering like SKU management)
        const types = items.sort((a: SKUGroupType, b: SKUGroupType) => 
          a.ItemLevel - b.ItemLevel
        );

        console.log("✅ Filtered types:", types);
        console.log("🎯 Final data length:", types.length);
        setData(types);
      } catch (err) {
        setError(err as Error);
      } finally {
        setIsLoading(false);
      }
    };

    fetchHierarchy();
  }, [orgUid, showEmptyTypes]);

  return {
    data,
    isLoading,
    error,
    refetch: () => {
      // Trigger a re-fetch by changing a dependency
      setData([]);
      setIsLoading(true);
    }
  };
}