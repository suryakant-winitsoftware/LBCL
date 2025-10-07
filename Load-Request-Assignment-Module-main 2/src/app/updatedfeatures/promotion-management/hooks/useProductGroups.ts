import { useState, useEffect } from "react";
import { API_CONFIG, getCommonHeaders } from "../services/api-config";
import { SKUGroup } from "../components/product-selection/DynamicProductSelector";

export function useProductGroups(groupTypeUID: string, parentUID?: string) {
  const [data, setData] = useState<SKUGroup[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<Error | null>(null);

  useEffect(() => {
    if (!groupTypeUID) {
      setData([]);
      return;
    }

    const fetchGroups = async () => {
      setIsLoading(true);
      setError(null);
      
      try {
        const FilterCriterias = [
          {
            Field: "SKUGroupTypeUID",
            Value: groupTypeUID,
            Operator: "Equal"
          }
        ];

        if (parentUID) {
          FilterCriterias.push({
            Field: "ParentUID",
            Value: parentUID,
            Operator: "Equal"
          });
        }

        const response = await fetch(
          `${API_CONFIG.baseURL}${API_CONFIG.endpoints.skuGroup.selectAll}`,
          {
            method: "POST",
            headers: getCommonHeaders(),
            body: JSON.stringify({
              PageNumber: 1,
              PageSize: 1000,
              SortCriterias: [],
              FilterCriterias,
              IsCountRequired: true
            })
          }
        );

        if (!response.ok) {
          throw new Error("Failed to load product groups");
        }

        const result = await response.json();
        console.log("Product Groups API Response:", result);
        
        // Handle API response structure - the API returns PagedData directly
        const items = result?.PagedData || result?.Data?.PagedData || [];
        
        setData(items);
      } catch (err) {
        setError(err as Error);
        setData([]);
      } finally {
        setIsLoading(false);
      }
    };

    fetchGroups();
  }, [groupTypeUID, parentUID]);

  return {
    data,
    isLoading,
    error,
    refetch: () => {
      setData([]);
      setIsLoading(true);
    }
  };
}