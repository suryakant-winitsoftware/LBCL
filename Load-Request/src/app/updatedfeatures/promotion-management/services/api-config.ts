/**
 * Centralized API Configuration
 * Single source of truth for all API endpoints and configurations
 */

export const API_BASE_URL =
  process.env.NEXT_PUBLIC_API_URL || "http://localhost:8000/api";

export const API_CONFIG = {
  baseURL: API_BASE_URL,
  endpoints: {
    // Promotion APIs
    promotion: {
      getDetails: "/Promotion/GetPromotionDetails",
      getByUID: "/Promotion/GetPromotionDetailsByUID",
      createUpdate: "/Promotion/CUDPromotionMaster",
      delete: "/Promotion/DeletePromotionDetailsbyUID",
      validate: "/Promotion/GetPromotionDetailsValidated",
      getOrder: "/PromoOrder/GetPromoOrderDetailsByPromoUID",
      getOffer: "/PromoOffer/GetPromoOfferDetailsByPromoUID",
      getBranch: "/SchemeBranch/GetSchemeBranchDetailsByPromoUID",
      getOrg: "/SchemeOrg/GetSchemeOrgDetailsByPromoUID"
    },
    // Product Hierarchy APIs
    sku: {
      selectAll: "/SKU/SelectAllSKUDetails",
      getByUID: "/SKU/GetSKUDetailsByUID",
      createUpdate: "/SKU/CUDSKU",
      delete: "/SKU/DeleteSKUDetailsbyUID",
      getAllMasterData: "/SKU/GetAllSKUMasterData"
    },
    skuGroup: {
      selectAll: "/SKUGroup/SelectAllSKUGroupDetails",
      getByUID: "/SKUGroup/GetSKUGroupDetailsByUID",
      createUpdate: "/SKUGroup/CUDSKUGroup",
      delete: "/SKUGroup/DeleteSKUGroupDetailsbyUID",
      getByTypeUID: "/SKUGroup/GetSKUGroupDetailsBySKUGroupTypeUID"
    },
    skuGroupType: {
      selectAll: "/SKUGroupType/SelectAllSKUGroupTypeDetails",
      getByUID: "/SKUGroupType/GetSKUGroupTypeDetailsByUID",
      createUpdate: "/SKUGroupType/CUDSKUGroupType",
      delete: "/SKUGroupType/DeleteSKUGroupTypeDetailsbyUID"
    },
    skuToGroupMapping: {
      selectAll: "/SKUToGroupMapping/SelectAllSKUToGroupMappingDetails",
      getByUID: "/SKUToGroupMapping/GetSKUToGroupMappingDetailsByUID",
      createUpdate: "/SKUToGroupMapping/CUDSKUToGroupMapping",
      delete: "/SKUToGroupMapping/DeleteSKUToGroupMappingDetailsbyUID"
    },
    skuClassGroupItems: {
      selectAll: "/SKUClassGroupItems/SelectAllSKUClassGroupItemsDetails",
      selectView: "/SKUClassGroupItems/SelectAllSKUClassGroupItemView",
      getByUID: "/SKUClassGroupItems/GetSKUClassGroupItemsByUID"
    },
    broadClassification: {
      selectAll: "/BroadClassification/SelectAllBroadClassificationDetails",
      getByUID: "/BroadClassification/GetBroadClassificationDetailsByUID",
      createUpdate: "/BroadClassification/CUDBroadClassification",
      delete: "/BroadClassification/DeleteBroadClassificationDetailsbyUID"
    },
    // Store Hierarchy APIs
    branch: {
      selectAll: "/Branch/SelectAllBranchDetails",
      getByUID: "/Branch/GetBranchDetailsByUID",
      createUpdate: "/Branch/CUDBranch",
      delete: "/Branch/DeleteBranchDetailsbyUID"
    },
    storeGroup: {
      selectAll: "/StoreGroup/SelectAllStoreGroup",
      getByUID: "/StoreGroup/GetStoreGroupDetailsByUID",
      createUpdate: "/StoreGroup/CUDStoreGroup",
      delete: "/StoreGroup/DeleteStoreGroupDetailsbyUID"
    },
    organization: {
      selectAll: "/Organization/SelectAllOrganizationDetails",
      getByUID: "/Organization/GetOrganizationDetailsByUID",
      createUpdate: "/Organization/CUDOrganization",
      delete: "/Organization/DeleteOrganizationDetailsbyUID"
    },
    // Additional APIs
    org: {
      getDetails: "/Org/GetOrgDetails"
    },
    location: {
      getDetails: "/Location/GetLocationDetails"
    },
    employee: {
      getDetails: "/MaintainUser/SelectAllMaintainUserDetails"
    },
    store: {
      selectAll: "/Store/SelectAllStore"
    },
    broadClassificationHeader: {
      getDetails:
        "/BroadClassificationHeader/GetBroadClassificationHeaderDetails"
    },
    broadClassificationLine: {
      getDetails: "/BroadClassificationLine/GetBroadClassificationLineDetails",
      getByUID:
        "/BroadClassificationLine/GetBroadClassificationLineDetailsByUID",
      getByHeaderUID:
        "/BroadClassificationLine/GetBroadClassificationLineByHeaderUID"
    }
  }
};

import { getAuthHeaders } from "@/lib/auth-service";

// Helper function to get common headers - matching SKU management implementation
export const getCommonHeaders = () => {
  // Use the centralized auth service like SKU management does
  const authHeaders = getAuthHeaders();

  return {
    "Content-Type": "application/json",
    Accept: "application/json",
    ...authHeaders
  };
};

// Helper function to extract paged data
export const extractPagedData = (data: any) => {
  if (!data) return { items: [], totalCount: 0 };

  // Handle different response structures
  if (data.Data) {
    return {
      items: data.Data,
      totalCount: data.TotalRecords || data.TotalCount || data.Data.length
    };
  }

  if (Array.isArray(data)) {
    return {
      items: data,
      totalCount: data.length
    };
  }

  if (data.items) {
    return {
      items: data.items,
      totalCount: data.totalCount || data.items.length
    };
  }

  return { items: [], totalCount: 0 };
};
