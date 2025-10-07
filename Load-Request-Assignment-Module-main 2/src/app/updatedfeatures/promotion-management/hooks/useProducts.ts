import { useState, useEffect } from "react";
import { API_CONFIG, getCommonHeaders, extractPagedData } from "../services/api-config";

export interface Product {
  UID: string;
  Code: string;
  Name: string;
  MRP?: number;
  IsActive?: boolean;
  L1?: string;
  L2?: string;
  L3?: string;
  OrgUID?: string;
  SupplierOrgUID?: string;
  FilterKeys?: string[];
}

export function useProducts(groupUID?: string, groupName?: string) {
  const [data, setData] = useState<Product[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<Error | null>(null);

  useEffect(() => {
    if (!groupUID) {
      setData([]);
      return;
    }

    const fetchProducts = async () => {
      setIsLoading(true);
      setError(null);
      
      try {
        // Load all SKU Master data
        const response = await fetch(
          `${API_CONFIG.baseURL}${API_CONFIG.endpoints.sku.getAllMasterData}`,
          {
            method: "POST",
            headers: getCommonHeaders(),
            body: JSON.stringify({
              SKUUIDs: [],
              OrgUIDs: [],
              DistributionChannelUIDs: [],
              AttributeTypes: []
            })
          }
        );

        if (!response.ok) {
          throw new Error("Failed to load products");
        }

        const jsonData = await response.json();
        const extracted = extractPagedData(jsonData.Data || jsonData);
        
        if (!extracted.items || extracted.items.length === 0) {
          setData([]);
          return;
        }

        // Filter SKUs based on the group
        const filteredProducts = extracted.items
          .filter((item: any) => {
            const sku = item.SKU || item;
            
            if (!groupName) return true;

            // Check different ways SKU might be associated with the group
            // Method 1: Check L1, L2, L3 fields
            if (
              sku.L1?.includes(groupName) ||
              sku.L2?.includes(groupName) ||
              sku.L3?.includes(groupName)
            ) {
              return true;
            }

            // Method 2: Check OrgUID, SupplierOrgUID for Brand matching
            if (sku.OrgUID === groupName || sku.SupplierOrgUID === groupName) {
              return true;
            }

            // Method 3: Check FilterKeys array
            if (sku.FilterKeys && Array.isArray(sku.FilterKeys)) {
              const hasGroupMatch = sku.FilterKeys.some((key: string) =>
                key.toLowerCase().includes(groupName.toLowerCase())
              );
              if (hasGroupMatch) return true;
            }

            // Method 4: Check SKUAttributes for group information
            if (item.SKUAttributes && Array.isArray(item.SKUAttributes)) {
              const hasAttributeMatch = item.SKUAttributes.some(
                (attr: any) =>
                  attr.AttributeValue?.toLowerCase().includes(
                    groupName.toLowerCase()
                  ) || attr.Value?.toLowerCase().includes(groupName.toLowerCase())
              );
              if (hasAttributeMatch) return true;
            }

            return false;
          })
          .map((item: any) => {
            const sku = item.SKU || item;
            return {
              UID: sku.UID || sku.uid || `${sku.Code}_${Date.now()}`,
              Code: sku.Code || sku.code || "N/A",
              Name: sku.Name || sku.name || sku.LongName || "Unknown Product",
              MRP: item.SKUPriceList?.[0]?.MRP || sku.MRP || 0,
              IsActive: sku.IsActive !== false,
              L1: sku.L1,
              L2: sku.L2,
              L3: sku.L3,
              OrgUID: sku.OrgUID,
              SupplierOrgUID: sku.SupplierOrgUID,
              FilterKeys: sku.FilterKeys
            };
          });

        setData(filteredProducts);
      } catch (err) {
        setError(err as Error);
        setData([]);
      } finally {
        setIsLoading(false);
      }
    };

    fetchProducts();
  }, [groupUID, groupName]);

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