import { useState, useEffect } from "react";
import { hierarchyService, SKUGroupType } from "@/services/hierarchy.service";

export function useProductHierarchy(orgUid?: string, showEmptyTypes: boolean = false) {
  const [data, setData] = useState<SKUGroupType[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<Error | null>(null);

  useEffect(() => {
    const fetchHierarchy = async () => {
      console.log("🔍 useProductHierarchy: Starting fetch using centralized service...");
      setIsLoading(true);
      setError(null);
      
      try {
        // Use the same hierarchy service as SKU management
        const types = await hierarchyService.getHierarchyTypes();
        
        console.log("📊 Product Hierarchy from centralized service:", types);
        console.log("🔢 Types length:", types.length);
        
        // Filter and sort types (already sorted by ItemLevel in the service)
        const validTypes = types.filter((type: SKUGroupType) => 
          type && type.UID && type.Name
        );

        console.log("✅ Valid hierarchy types:", validTypes);
        console.log("🎯 Final data length:", validTypes.length);
        setData(validTypes);
      } catch (err) {
        console.error("❌ Error fetching hierarchy:", err);
        setError(err as Error);
        setData([]); // Set empty array on error
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