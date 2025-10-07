"use client";

import React, { useState, useEffect, useCallback } from "react";
import { useParams, useRouter } from "next/navigation";
import {
  ArrowLeft,
  Pencil,
  Trash2,
  Tag,
  ShoppingBag,
  Gift,
  CheckCircle,
  Info,
  Clock,
  AlertTriangle,
  Settings,
  Building,
  Store,
  MapPin,
  User,
  UserCheck,
} from "lucide-react";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Card, CardHeader, CardTitle, CardContent } from "@/components/ui/card";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { promotionService } from "../../services/promotion.service";
import { promotionV3Service } from "../../services/promotionV3.service";
import { IPromoMasterView } from "../../types/promotion.types";
import { PromotionV3FormData } from "../../types/promotionV3.types";
import {
  getPromotionDisplayType,
  getPromotionDescription,
  getPromotionStatus,
} from "../../utils/promotionDisplayUtils";

// Helper function to safely format dates
const formatDate = (
  date: string | undefined | null
): string => {
  if (!date) return "N/A";
  try {
    const parsedDate = new Date(date);
    if (isNaN(parsedDate.getTime())) return "Invalid date";
    return parsedDate.toLocaleDateString();
  } catch {
    return "Invalid date";
  }
};

export default function PromotionDetailPage() {
  const params = useParams();
  const router = useRouter();
  const id = params.id as string;
  
  const [promotion, setPromotion] = useState<IPromoMasterView | null>(null);
  const [promotionV3Data, setPromotionV3Data] = useState<Partial<PromotionV3FormData> | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [activeTab, setActiveTab] = useState<
    "type" | "basic" | "configuration" | "footprint" | "volumecaps" | "review"
  >("type");

  const fetchPromotionDetails = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);

      // Try V3 API first for enhanced data including organizations and stores
      try {
        console.log('Attempting to fetch promotion via V3 API...');
        const v3Response = await promotionV3Service.getPromotionById(id);

        if (v3Response.success && v3Response.data) {
          console.log('V3 API response:', v3Response.data);
          
          // Cast to any since V3 API returns dynamic structure
          const v3Data: any = v3Response.data;
          
          // Convert V3 data to legacy format for compatibility with existing UI
          const legacyFormat: any = {
            PromotionView: v3Data.Promotion || v3Data || {},
            // Preserve V3 organization and store data
            SchemeOrgs: v3Data.OrganizationSelections || [],
            ReconstructedFootprintData: {
              SelectionType: v3Data.OrganizationSelections?.length > 0 ? 'specific' : 'all',
              // Separate organizations and stores from V3 response
              OrganizationSelections: v3Data.OrganizationSelections?.filter((sel: any) => 
                sel.FootprintType === 'SPECIFIC_ORG' || sel.FootprintType === 'ALL_ORG'
              ).map((org: any) => org.OrganizationName || org.OrganizationId) || [],
              StoreSelections: v3Data.OrganizationSelections?.filter((sel: any) => 
                sel.FootprintType === 'SPECIFIC_STORE'
              ).map((store: any) => store.OrganizationName || store.OrganizationId) || [],
              // Coverage summary with breakdown
              CoverageByType: {
                Organizations: v3Data.OrganizationSelections?.filter((sel: any) => 
                  sel.FootprintType === 'SPECIFIC_ORG' || sel.FootprintType === 'ALL_ORG'
                ).length || 0,
                Stores: v3Data.OrganizationSelections?.filter((sel: any) => 
                  sel.FootprintType === 'SPECIFIC_STORE'
                ).length || 0
              },
              TotalCoverage: v3Data.OrganizationSelections?.length || 'All'
            },
            // Include product selections if available
            ReconstructedProductSelections: v3Data.ProductSelections || null,
            // Include volume caps and configurations
            PromotionVolumeCap: v3Data.VolumeCaps || null,
            ProductConfigs: v3Data.Configurations || [],
            // Ensure we have the minimum required legacy structure
            PromoOrderViewList: [],
            PromoOrderItemViewList: [],
            PromoOfferViewList: [],
            PromoOfferItemViewList: [],
            SchemeBranches: [],
            IsNew: false,
            // Preserve other V3 data
            ...v3Data
          };
          
          setPromotion(legacyFormat);
          setPromotionV3Data(v3Data);
          console.log('Successfully loaded V3 data with organizations:', v3Data.OrganizationSelections?.length || 0, 'entries');
          return;
        }
      } catch (v3Error) {
        console.log('V3 API not available, falling back to legacy API:', v3Error);
      }

      // Fallback to legacy API
      const response = await promotionService.getPromotionByUID(id);

      if (response.success && response.data) {
        // Transform legacy data to include reconstructed fields for UI
        const transformedData = {
          ...response.data,
          // Reconstruct product selections from ItemPromotionMapViewList
          ReconstructedProductSelections: response.data.ItemPromotionMapViewList ? {
            // Determine selection type - if we have non-SKU items, it's hierarchy based
            SelectionType: response.data.ItemPromotionMapViewList.some((item: any) => 
              item.SKUType !== 'SKU' && item.SKUType !== 'EXCLUDED_SKU') ? 'hierarchy' : 'specific',
            
            // Count actual products (SKUs)
            TotalProducts: response.data.ItemPromotionMapViewList.filter((item: any) => 
              item.SKUType === 'SKU').length,
            NetProducts: response.data.ItemPromotionMapViewList.filter((item: any) => 
              item.SKUType === 'SKU').length,
            ExcludedProducts: response.data.ItemPromotionMapViewList.filter((item: any) => 
              item.SKUType === 'EXCLUDED_SKU' || item.SKUType?.includes('EXCLUDED')).length,
            
            // Specific products list
            SpecificProducts: response.data.ItemPromotionMapViewList
              .filter((item: any) => item.SKUType === 'SKU')
              .map((item: any) => ({
                ItemCode: item.SKUTypeUID,
                Name: item.SKUTypeUID,
                UOM: item.UOM || 'PC'
              })),
            
            // Group all non-SKU items by their type (dynamic hierarchy)
            HierarchySelections: response.data.ItemPromotionMapViewList
              .filter((item: any) => 
                // Include anything that's not a direct SKU or excluded item
                item.SKUType !== 'SKU' && 
                !item.SKUType?.includes('EXCLUDED') && 
                item.SKUType !== 'ALL')
              .reduce((acc: any, item: any) => {
                // Group by SKUType dynamically
                const type = item.SKUType || 'Other';
                if (!acc[type]) acc[type] = [];
                acc[type].push({
                  Code: item.SKUTypeUID,
                  Name: item.SKUTypeUID,
                  SKUType: item.SKUType
                });
                return acc;
              }, {})
          } : null,
          // Add reconstructed footprint from SchemeOrgs/SchemeBranches
          ReconstructedFootprintData: {
            SelectionType: (response.data.SchemeOrgs?.length > 0 || response.data.SchemeBranches?.length > 0) ? 'specific' : 'all',
            OrganizationSelections: response.data.SchemeOrgs?.map((org: any) => org.OrgUID || org.UID) || [],
            BranchSelections: response.data.SchemeBranches?.map((branch: any) => branch.BranchCode || branch.BranchUID || branch.UID) || [],
            CoverageByType: {
              Organizations: response.data.SchemeOrgs?.length || 0,
              Branches: response.data.SchemeBranches?.length || 0,
              Stores: response.data.SchemeBranches?.length || 0
            },
            TotalCoverage: (response.data.SchemeOrgs?.length || 0) + (response.data.SchemeBranches?.length || 0) || 'All'
          }
        };
        
        setPromotion(transformedData);
        console.log('Loaded promotion via legacy API with data transformation');
        console.log('Transformed data:', transformedData);
      } else {
        throw new Error(response.error || "Failed to fetch promotion details");
      }
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : "Failed to fetch promotion details";
      setError(errorMessage);
    } finally {
      setLoading(false);
    }
  }, [id]);

  useEffect(() => {
    fetchPromotionDetails();
  }, [fetchPromotionDetails]);

  const handleEdit = () => {
    router.push(`/updatedfeatures/promotion-management/promotions/manage?edit=${id}`);
  };

  const handleDelete = async () => {
    if (window.confirm("Are you sure you want to delete this promotion?")) {
      try {
        const response = await promotionService.deletePromotion(id);
        if (response.success) {
          router.push("/updatedfeatures/promotion-management/promotions/manage");
        } else {
          throw new Error(response.error || "Failed to delete promotion");
        }
      } catch (err) {
        const errorMessage = err instanceof Error ? err.message : "Unknown error";
        alert(`Error deleting promotion: ${errorMessage}`);
      }
    }
  };

  const handleBack = () => {
    router.push("/updatedfeatures/promotion-management/promotions/manage");
  };

  const HeaderSkeleton = () => (
    <div className="flex items-center justify-between">
      <div className="flex items-center space-x-4">
        <div className="w-16 h-8 bg-gray-200 rounded animate-pulse"></div>
        <div>
          <div className="flex items-center space-x-3">
            <div className="h-8 bg-gray-200 rounded animate-pulse w-64"></div>
            <div className="w-16 h-5 bg-gray-200 rounded animate-pulse"></div>
          </div>
          <div className="h-4 bg-gray-200 rounded animate-pulse w-80 mt-2"></div>
        </div>
      </div>
      <div className="flex items-center space-x-2">
        <div className="w-16 h-10 bg-gray-200 rounded animate-pulse"></div>
        <div className="w-20 h-10 bg-gray-200 rounded animate-pulse"></div>
      </div>
    </div>
  );

  const StatCardSkeleton = () => (
    <Card>
      <CardContent className="py-4">
        <div className="flex items-center">
          <div className="w-8 h-8 bg-gray-200 rounded animate-pulse"></div>
          <div className="ml-4 space-y-2">
            <div className="h-3 bg-gray-200 rounded animate-pulse w-24"></div>
            <div className="h-5 bg-gray-200 rounded animate-pulse w-16"></div>
          </div>
        </div>
      </CardContent>
    </Card>
  );

  const TabContentSkeleton = () => (
    <div className="space-y-6">
      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        <Card>
          <CardHeader>
            <div className="h-5 bg-gray-200 rounded animate-pulse w-40"></div>
          </CardHeader>
          <CardContent>
            <div className="space-y-4">
              {[1,2,3,4].map(i => (
                <div key={i} className="space-y-2">
                  <div className="h-3 bg-gray-200 rounded animate-pulse w-20"></div>
                  <div className="h-4 bg-gray-200 rounded animate-pulse w-32"></div>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardHeader>
            <div className="h-5 bg-gray-200 rounded animate-pulse w-40"></div>
          </CardHeader>
          <CardContent>
            <div className="space-y-4">
              {[1,2,3,4].map(i => (
                <div key={i} className="space-y-2">
                  <div className="h-3 bg-gray-200 rounded animate-pulse w-20"></div>
                  <div className="h-4 bg-gray-200 rounded animate-pulse w-32"></div>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>
      </div>
      <Card>
        <CardHeader>
          <div className="h-5 bg-gray-200 rounded animate-pulse w-48"></div>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            {[1,2,3,4,5,6].map(i => (
              <div key={i} className="space-y-2">
                <div className="h-3 bg-gray-200 rounded animate-pulse w-20"></div>
                <div className="h-4 bg-gray-200 rounded animate-pulse w-32"></div>
              </div>
            ))}
          </div>
        </CardContent>
      </Card>
    </div>
  );

  if (loading) {
    return (
      <div className="container mx-auto py-6 space-y-6">
        {/* Header Skeleton */}
        <HeaderSkeleton />

        {/* Quick Stats Skeleton */}
        <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
          <StatCardSkeleton />
          <StatCardSkeleton />
          <StatCardSkeleton />
          <StatCardSkeleton />
        </div>

        {/* Main Content Skeleton */}
        <Card>
          <CardContent className="p-0">
            <div className="p-6">
              {/* Tabs Skeleton */}
              <div className="border-b border-gray-200 mb-6">
                <div className="flex space-x-8">
                  {[1,2,3,4,5,6].map(i => (
                    <div key={i} className="h-4 bg-gray-200 rounded animate-pulse w-16"></div>
                  ))}
                </div>
              </div>
              
              {/* Tab Content Skeleton */}
              <TabContentSkeleton />
            </div>
          </CardContent>
        </Card>
      </div>
    );
  }

  if (error || !promotion) {
    return (
      <div className="container mx-auto py-6">
        <Card>
          <CardContent className="py-12 text-center">
            <AlertTriangle className="mx-auto h-12 w-12 text-red-500 mb-4" />
            <h3 className="text-lg font-medium text-gray-900 mb-2">Error Loading Promotion</h3>
            <p className="text-gray-600 mb-4">{error}</p>
            <Button onClick={handleBack} variant="outline">
              <ArrowLeft className="mr-2 h-4 w-4" />
              Back to Promotions
            </Button>
          </CardContent>
        </Card>
      </div>
    );
  }

  const promotionData = promotion?.PromotionView || promotion?.promotionView || promotion;
  const status = getPromotionStatus(promotionData);

  const getStatusBadge = () => {
    const variants: Record<string, "default" | "secondary" | "destructive" | "outline"> = {
      green: "default",
      blue: "secondary", 
      red: "destructive",
      gray: "outline"
    };
    
    return (
      <Badge variant={variants[status.color]} className="text-sm">
        {status.label}
      </Badge>
    );
  };

  return (
    <div className="container mx-auto py-6 space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div className="flex items-center space-x-4">
          <Button variant="ghost" size="sm" onClick={handleBack}>
            <ArrowLeft className="h-4 w-4 mr-2" />
            Back
          </Button>
          <div>
            <div className="flex items-center space-x-3">
              <h1 className="text-3xl font-bold text-gray-900">{promotionData?.Name || "Unnamed Promotion"}</h1>
              {getStatusBadge()}
            </div>
            <p className="text-gray-600 mt-1">
              {getPromotionDescription(promotionData)}
            </p>
          </div>
        </div>
        <div className="flex items-center space-x-2">
          <Button variant="outline" onClick={handleEdit}>
            <Pencil className="h-4 w-4 mr-2" />
            Edit
          </Button>
          <Button variant="destructive" onClick={handleDelete}>
            <Trash2 className="h-4 w-4 mr-2" />
            Delete
          </Button>
        </div>
      </div>

      {/* Quick Stats */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        <Card>
          <CardContent className="py-4">
            <div className="flex items-center">
              <Tag className="h-8 w-8 text-blue-600" />
              <div className="ml-4">
                <p className="text-sm font-medium text-gray-600">Promotion Code</p>
                <p className="text-lg font-semibold">{promotionData?.Code || "N/A"}</p>
              </div>
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="py-4">
            <div className="flex items-center">
              <Settings className="h-8 w-8 text-green-600" />
              <div className="ml-4">
                <p className="text-sm font-medium text-gray-600">Type</p>
                <p className="text-lg font-semibold">{getPromotionDisplayType(promotionData)}</p>
              </div>
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="py-4">
            <div className="flex items-center">
              <Clock className="h-8 w-8 text-orange-600" />
              <div className="ml-4">
                <p className="text-sm font-medium text-gray-600">Priority</p>
                <p className="text-lg font-semibold">{promotionData?.Priority || "N/A"}</p>
              </div>
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="py-4">
            <div className="flex items-center">
              <CheckCircle className="h-8 w-8 text-purple-600" />
              <div className="ml-4">
                <p className="text-sm font-medium text-gray-600">Has Slabs</p>
                <p className="text-lg font-semibold">{promotionData?.HasSlabs ? "Yes" : "No"}</p>
              </div>
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Main Content */}
      <Card>
        <CardContent className="p-0">
          <Tabs value={activeTab} onValueChange={(value) => setActiveTab(value as "type" | "basic" | "configuration" | "footprint" | "volumecaps" | "review")}>
            <CardHeader>
              <TabsList className="grid w-full grid-cols-6">
                <TabsTrigger value="type">Type</TabsTrigger>
                <TabsTrigger value="basic">Basic</TabsTrigger>
                <TabsTrigger value="configuration">Configuration</TabsTrigger>
                <TabsTrigger value="footprint">Footprint</TabsTrigger>
                <TabsTrigger value="volumecaps">Volume Caps</TabsTrigger>
                <TabsTrigger value="review">Review</TabsTrigger>
              </TabsList>
            </CardHeader>

            <div className="p-6">
              {/* Type Tab */}
              <TabsContent value="type" className="space-y-6">
                <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
                  {/* Promotion Level */}
                  <Card>
                    <CardContent className="p-6">
                      <dt className="text-sm font-medium text-gray-500 truncate">
                        Promotion Level
                      </dt>
                      <dd className="mt-1 text-3xl font-semibold text-gray-900">
                        {promotionData?.Type === 'Invoice' ? 'INVOICE' : promotionData?.Type === 'Instant' ? 'ITEM_LEVEL' : (promotionData?.Level || promotionData?.OrderType || "ITEM_LEVEL")}
                      </dd>
                      <p className="mt-1 text-sm text-gray-600">
                        {(promotionData?.Type === "Invoice" || promotionData?.Level === "INVOICE_LEVEL" || promotionData?.OrderType === "INVOICE")
                          ? "Applied at invoice level with total discount calculations"
                          : "Applied at item/line level with individual product discounts"}
                      </p>
                    </CardContent>
                  </Card>

                  {/* Promotion Format */}
                  <Card>
                    <CardContent className="p-6">
                      <dt className="text-sm font-medium text-gray-500 truncate">
                        Promotion Format
                      </dt>
                      <dd className="mt-1 text-3xl font-semibold text-gray-900">
                        {promotionData?.PromoFormat || "IQPD"}
                      </dd>
                      <p className="mt-1 text-sm text-gray-600">
                        {(() => {
                          const format = promotionData?.PromoFormat;
                          switch(format) {
                            case "IQFD": return "Item Quantity Fixed Discount - Fixed discount amount per item";
                            case "IQPD": return "Item Quantity Percentage Discount - Percentage discount on item price";
                            case "IQXF": return "Item Quantity X Free - Buy X get Y free promotion";
                            case "BQXF": return "Buy Quantity X Free - Buy quantity X get free items (FOC)";
                            case "BYVALUE": return "By Value - Discount based on invoice value";
                            case "BYQTY": return "By Quantity - Discount based on total quantity";
                            case "LINECOUNT": return "Line Count - Discount based on number of line items";
                            case "BRANDCOUNT": return "Brand Count - Discount based on number of brands";
                            case "ANYVALUE": return "Any Value - Fixed discount on any invoice";
                            case "PERCENTAGE": return "Percentage-based discount promotion";
                            case "FIXED": return "Fixed amount discount promotion";
                            default: return "Standard promotion format";
                          }
                        })()}
                      </p>
                    </CardContent>
                  </Card>

                  {/* Status */}
                  <Card>
                    <CardContent className="p-6">
                      <dt className="text-sm font-medium text-gray-500 truncate">
                        Current Status
                      </dt>
                      <dd className="mt-1 flex items-center">
                        <Badge 
                          variant={promotionData?.IsActive ? "default" : "destructive"}
                          className="text-lg px-3 py-1"
                        >
                          {promotionData?.IsActive ? "Active" : "Inactive"}
                        </Badge>
                      </dd>
                      <p className="mt-1 text-sm text-gray-600">
                        {promotionData?.IsActive 
                          ? "This promotion is currently active and available for use"
                          : "This promotion is inactive and not available for use"}
                      </p>
                    </CardContent>
                  </Card>
                </div>

                {/* Promotion Details */}
                <Card>
                  <CardHeader>
                    <CardTitle>Promotion Type Details</CardTitle>
                  </CardHeader>
                  <CardContent>
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                      <div>
                        <label className="text-sm font-medium text-gray-500">Order Type</label>
                        <p className="text-base font-semibold">
                          {promotionData?.OrderType || 
                           (promotionData?.Level === "INVOICE_LEVEL" ? "INVOICE" : "LINE") || 
                           "LINE"}
                        </p>
                      </div>
                      <div>
                        <label className="text-sm font-medium text-gray-500">Selection Model</label>
                        <p className="text-base font-semibold">
                          {promotionData?.SelectionModel || "ANY"}
                        </p>
                      </div>
                      <div>
                        <label className="text-sm font-medium text-gray-500">Multi Product Enabled</label>
                        <p className="text-base font-semibold">
                          {promotionData?.MultiProductEnabled ? "Yes" : "No"}
                        </p>
                      </div>
                      <div>
                        <label className="text-sm font-medium text-gray-500">Has Slabs</label>
                        <p className="text-base font-semibold">
                          {promotionData?.HasSlabs ? "Yes" : "No"}
                        </p>
                      </div>
                      <div>
                        <label className="text-sm font-medium text-gray-500">Promotion Type</label>
                        <p className="text-base font-semibold">
                          {promotionData?.Type || getPromotionDisplayType(promotionData)}
                        </p>
                      </div>
                      <div>
                        <label className="text-sm font-medium text-gray-500">Category</label>
                        <p className="text-base font-semibold">
                          {promotionData?.Category || "General"}
                        </p>
                      </div>
                    </div>
                  </CardContent>
                </Card>
              </TabsContent>

              {/* Basic Tab */}
              <TabsContent value="basic" className="space-y-6">
                <Card>
                  <CardHeader>
                    <CardTitle className="flex items-center">
                      <Info className="h-5 w-5 mr-2 text-blue-600" />
                      Basic Promotion Information
                    </CardTitle>
                  </CardHeader>
                  <CardContent>
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                      <div className="space-y-4">
                        <div>
                          <label className="text-sm font-medium text-gray-500">Promotion Code</label>
                          <p className="text-lg font-semibold text-gray-900">{promotion?.PromotionView?.Code || "N/A"}</p>
                        </div>
                        <div>
                          <label className="text-sm font-medium text-gray-500">Promotion Name</label>
                          <p className="text-lg font-semibold text-gray-900">{promotion?.PromotionView?.Name || "N/A"}</p>
                        </div>
                        <div>
                          <label className="text-sm font-medium text-gray-500">Description</label>
                          <p className="text-base text-gray-700">{promotion?.PromotionView?.Description || promotion?.PromotionView?.Remarks || "N/A"}</p>
                        </div>
                        <div>
                          <label className="text-sm font-medium text-gray-500">Category</label>
                          <p className="text-base text-gray-700">{promotion?.PromotionView?.Category || "N/A"}</p>
                        </div>
                      </div>

                      <div className="space-y-4">
                        <div>
                          <label className="text-sm font-medium text-gray-500">Valid From</label>
                          <p className="text-base text-gray-700">{formatDate(promotion?.PromotionView?.ValidFrom)}</p>
                        </div>
                        <div>
                          <label className="text-sm font-medium text-gray-500">Valid Until</label>
                          <p className="text-base text-gray-700">{formatDate(promotion?.PromotionView?.ValidUpto)}</p>
                        </div>
                        <div>
                          <label className="text-sm font-medium text-gray-500">Priority</label>
                          <p className="text-base text-gray-700">{promotion?.PromotionView?.Priority || 1}</p>
                        </div>
                        <div>
                          <label className="text-sm font-medium text-gray-500">Status</label>
                          <div className="mt-1">
                            {getStatusBadge()}
                          </div>
                        </div>
                      </div>
                    </div>

                    {/* Additional Details */}
                    <div className="mt-6 border-t border-gray-200 pt-6">
                      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
                        <div>
                          <label className="text-sm font-medium text-gray-500">Promotion Title</label>
                          <p className="text-base text-gray-700">{promotion?.PromotionView?.PromoTitle || "N/A"}</p>
                        </div>
                        <div>
                          <label className="text-sm font-medium text-gray-500">Promotion Message</label>
                          <p className="text-base text-gray-700">{promotion?.PromotionView?.PromoMessage || "N/A"}</p>
                        </div>
                        <div>
                          <label className="text-sm font-medium text-gray-500">Created By</label>
                          <p className="text-base text-gray-700">{promotion?.PromotionView?.CreatedByEmpUID || "N/A"}</p>
                        </div>
                        <div>
                          <label className="text-sm font-medium text-gray-500">Created Time</label>
                          <p className="text-base text-gray-700">{formatDate(promotion?.PromotionView?.CreatedTime)}</p>
                        </div>
                        <div>
                          <label className="text-sm font-medium text-gray-500">Last Modified</label>
                          <p className="text-base text-gray-700">{formatDate(promotion?.PromotionView?.ModifiedTime)}</p>
                        </div>
                      </div>
                    </div>
                  </CardContent>
                </Card>
              </TabsContent>

              {/* Configuration Tab */}
              <TabsContent value="configuration" className="space-y-6">
                {/* Product Selection - Enhanced like React version */}
                <Card>
                  <CardHeader>
                    <CardTitle className="flex items-center">
                      <ShoppingBag className="h-5 w-5 mr-2 text-purple-600" />
                      Product Selection
                    </CardTitle>
                  </CardHeader>
                  <CardContent>
                    {promotion.ReconstructedProductSelections ? (
                      <div className="space-y-4">
                        <div className="flex items-center justify-between">
                          <div>
                            <dt className="text-sm font-medium text-gray-500">
                              Selection Type
                            </dt>
                            <dd className="mt-1 text-sm text-gray-900 capitalize">
                              {promotion.ReconstructedProductSelections.SelectionType}
                            </dd>
                          </div>
                          <div className="text-right">
                            <dt className="text-sm font-medium text-gray-500">
                              Total Products
                            </dt>
                            <dd className="mt-1 text-2xl font-semibold text-blue-600">
                              {promotion.ReconstructedProductSelections.TotalProducts || 0}
                            </dd>
                          </div>
                        </div>

                        {/* Specific Products */}
                        {promotion.ReconstructedProductSelections.SpecificProducts &&
                          promotion.ReconstructedProductSelections.SpecificProducts.length > 0 && (
                            <div>
                              <h4 className="text-md font-medium text-gray-900 mb-2">
                                Selected Products
                              </h4>
                              <div className="bg-gray-50 rounded-lg p-4">
                                <div className="grid grid-cols-1 gap-3">
                                  {promotion.ReconstructedProductSelections.SpecificProducts.map(
                                    (product, productIndex) => (
                                      <div
                                        key={productIndex}
                                        className="flex items-center justify-between bg-white p-3 rounded border"
                                      >
                                        <div>
                                          <div className="font-medium text-gray-900">
                                            {product.Name}
                                          </div>
                                          <div className="text-sm text-gray-500">
                                            Code: {product.Code}
                                          </div>
                                        </div>
                                        <div className="text-right">
                                          <div className="text-sm font-medium text-gray-900">
                                            Min Qty: {product.MinQuantity}
                                          </div>
                                          <div className="text-xs text-gray-500">
                                            {product.IsCompulsory ? "Compulsory" : "Optional"}
                                          </div>
                                        </div>
                                      </div>
                                    )
                                  )}
                                </div>
                              </div>
                            </div>
                          )}

                        {/* Hierarchy Selections */}
                        {promotion.ReconstructedProductSelections.HierarchySelections &&
                          Object.keys(promotion.ReconstructedProductSelections.HierarchySelections).length > 0 && (
                            <div>
                              <h4 className="text-md font-medium text-gray-900 mb-2">
                                Hierarchy Selections
                              </h4>
                              <div className="bg-gray-50 rounded-lg p-4">
                                <div className="grid grid-cols-1 gap-3">
                                  {Object.entries(promotion.ReconstructedProductSelections.HierarchySelections).map(([type, items]) => (
                                    <div key={type} className="bg-white p-3 rounded border">
                                      <div className="font-medium text-gray-900 mb-2">
                                        {type}
                                      </div>
                                      <div className="flex flex-wrap gap-2">
                                        {(items as any[]).map((item, idx) => (
                                          <span
                                            key={idx}
                                            className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-blue-100 text-blue-800"
                                          >
                                            {typeof item === 'string' ? item : (item.Name || item.Code || JSON.stringify(item))}
                                          </span>
                                        ))}
                                      </div>
                                    </div>
                                  ))}
                                </div>
                              </div>
                            </div>
                          )}

                        {/* Product Statistics */}
                        <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mt-4">
                          <div className="bg-blue-50 p-4 rounded-lg text-center">
                            <div className="text-2xl font-bold text-blue-600">
                              {promotion.ReconstructedProductSelections.TotalProducts || 0}
                            </div>
                            <div className="text-sm text-blue-600">Total Products</div>
                          </div>
                          <div className="bg-green-50 p-4 rounded-lg text-center">
                            <div className="text-2xl font-bold text-green-600">
                              {promotion.ReconstructedProductSelections.NetProducts || 0}
                            </div>
                            <div className="text-sm text-green-600">Net Products</div>
                          </div>
                          <div className="bg-red-50 p-4 rounded-lg text-center">
                            <div className="text-2xl font-bold text-red-600">
                              {promotion.ReconstructedProductSelections.ExcludedProducts || 0}
                            </div>
                            <div className="text-sm text-red-600">Excluded Products</div>
                          </div>
                        </div>
                      </div>
                    ) : (
                      <div className="text-center py-8 text-gray-500">
                        <ShoppingBag className="mx-auto h-12 w-12 text-gray-300 mb-4" />
                        <p>Product selection data not available</p>
                        <p className="text-sm">This may be a legacy promotion or the data is still processing</p>
                      </div>
                    )}
                  </CardContent>
                </Card>

                <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                  {/* IQXF/BQXF Buy X Get Y Configuration */}
                  {(promotionData?.PromoFormat === 'IQXF' || promotionData?.PromoFormat === 'BQXF') && 
                   promotion.PromoConditionViewList && promotion.PromoConditionViewList.length > 0 && (
                    <Card>
                      <CardHeader>
                        <CardTitle className="flex items-center">
                          <Gift className="h-5 w-5 mr-2 text-purple-600" />
                          Buy X Get Y Configuration
                        </CardTitle>
                      </CardHeader>
                      <CardContent>
                        <div className="space-y-3">
                          {promotion.PromoConditionViewList.map((condition, index) => (
                            <div key={condition.UID || index} className="p-3 border rounded-lg bg-purple-50">
                              <div className="grid grid-cols-2 gap-4 text-sm">
                                <div>
                                  <span className="text-gray-500">Buy Quantity:</span> {condition.Min || 'Not set'}
                                </div>
                                <div>
                                  <span className="text-gray-500">Get Free:</span> {(condition as any).FreeQuantity || 'Not set'}
                                </div>
                                {promotion.PromoOrderViewList?.[0]?.MaxDealCount && (
                                  <div className="col-span-2">
                                    <span className="text-gray-500">Max Applications per Product per Invoice:</span> {promotion.PromoOrderViewList[0].MaxDealCount}
                                  </div>
                                )}
                              </div>
                            </div>
                          ))}
                        </div>
                      </CardContent>
                    </Card>
                  )}

                  {/* Orders Configuration */}
                  <Card>
                    <CardHeader>
                      <CardTitle className="flex items-center">
                        <ShoppingBag className="h-5 w-5 mr-2 text-blue-600" />
                        Order Configuration
                      </CardTitle>
                    </CardHeader>
                    <CardContent>
                      {promotion.PromoOrderViewList && promotion.PromoOrderViewList.length > 0 ? (
                        <div className="space-y-3">
                          {promotion.PromoOrderViewList.map((order, index) => (
                            <div key={order.UID} className="p-3 border rounded-lg">
                              <div className="grid grid-cols-2 gap-4 text-sm">
                                <div className="col-span-2">
                                  <span className="text-gray-500">Order #</span>{index + 1}
                                </div>
                                <div>
                                  <span className="text-gray-500">Min Deal Count:</span> {order.MinDealCount || 0}
                                </div>
                                <div>
                                  <span className="text-gray-500">Max Deal Count:</span> {order.MaxDealCount || 0}
                                </div>
                                <div>
                                  <span className="text-gray-500">Min Purchase Qty:</span> {order.MinPurchaseQty || 0}
                                </div>
                                <div>
                                  <span className="text-gray-500">Min Purchase Value:</span> {order.MinPurchaseValue || 0}
                                </div>
                                <div className="col-span-2">
                                  <span className="text-gray-500">Selection Model:</span> {order.SelectionModel || 'any'}
                                </div>
                              </div>
                            </div>
                          ))}
                        </div>
                      ) : (
                        <p className="text-gray-500">No order configuration defined</p>
                      )}
                    </CardContent>
                  </Card>

                  {/* Offers Configuration */}
                  <Card>
                    <CardHeader>
                      <CardTitle className="flex items-center">
                        <Gift className="h-5 w-5 mr-2 text-green-600" />
                        Offer Configuration
                      </CardTitle>
                    </CardHeader>
                    <CardContent>
                      {promotion.PromoOfferViewList && promotion.PromoOfferViewList.length > 0 ? (
                        <div className="space-y-3">
                          {promotion.PromoOfferViewList.map((offer, index) => (
                            <div key={offer.UID} className="p-3 border rounded-lg">
                              <div className="text-sm space-y-2">
                                <div>
                                  <span className="font-medium">Offer #{offer.OfferNo || index + 1}</span>
                                </div>
                                {offer.OfferType && (
                                  <div>
                                    <span className="text-gray-500">Type:</span> {offer.OfferType}
                                  </div>
                                )}
                                {/* Display any additional offer properties available in the data */}
                                {(offer as any).BenefitType && (
                                  <div>
                                    <span className="text-gray-500">Benefit Type:</span> {(offer as any).BenefitType}
                                  </div>
                                )}
                                {(offer as any).DiscountType && (
                                  <div>
                                    <span className="text-gray-500">Discount Type:</span> {(offer as any).DiscountType}
                                  </div>
                                )}
                                {(offer as any).DiscountValue > 0 && (
                                  <div>
                                    <span className="text-gray-500">Discount Value:</span> {(offer as any).DiscountValue}{(offer as any).DiscountType === 'PERCENTAGE' ? '%' : ''}
                                  </div>
                                )}
                                {(offer as any).MaxDiscountAmount && (
                                  <div>
                                    <span className="text-gray-500">Max Discount:</span> {(offer as any).MaxDiscountAmount}
                                  </div>
                                )}
                                {(offer as any).FreeQuantity && (
                                  <div>
                                    <span className="text-gray-500">Free Quantity:</span> {(offer as any).FreeQuantity}
                                  </div>
                                )}
                                {(offer as any).SelectionModel && (
                                  <div>
                                    <span className="text-gray-500">Selection Model:</span> {(offer as any).SelectionModel}
                                  </div>
                                )}
                                {/* Display FOC Items if available */}
                                {promotion.PromoOfferItemViewList && promotion.PromoOfferItemViewList.filter(item => item.PromoOfferUID === offer.UID).length > 0 && (
                                  <div className="mt-3 pt-3 border-t">
                                    <div className="font-medium mb-2">FOC Items:</div>
                                    {promotion.PromoOfferItemViewList
                                      .filter(item => item.PromoOfferUID === offer.UID)
                                      .map((item, itemIndex) => (
                                        <div key={item.UID || itemIndex} className="ml-3 text-xs space-y-1 bg-gray-50 p-2 rounded mb-1">
                                          <div>
                                            <span className="text-gray-500">Product:</span> {(item as any).ProductCode || item.ItemCriteriaSelected}
                                          </div>
                                          <div>
                                            <span className="text-gray-500">Free Quantity:</span> {(item as any).Quantity || item.ItemQty || 1}
                                          </div>
                                          {((item as any).MinQuantity || (item as any).MaxQuantity) && (
                                            <div>
                                              <span className="text-gray-500">Quantity Range:</span> 
                                              {' '}{(item as any).MinQuantity || 1} - {(item as any).MaxQuantity || 'unlimited'}
                                            </div>
                                          )}
                                          {item.ItemUOM && (
                                            <div>
                                              <span className="text-gray-500">UOM:</span> {item.ItemUOM}
                                            </div>
                                          )}
                                          {(item as any).IsCompulsory && (
                                            <div className="text-orange-600">Compulsory Item</div>
                                          )}
                                          {item.ItemName && (
                                            <div>
                                              <span className="text-gray-500">Name:</span> {item.ItemName}
                                            </div>
                                          )}
                                        </div>
                                      ))}
                                  </div>
                                )}
                              </div>
                            </div>
                          ))}
                        </div>
                      ) : (
                        <p className="text-gray-500">No offer configuration defined</p>
                      )}
                    </CardContent>
                  </Card>
                </div>
              </TabsContent>

              {/* Footprint Tab */}
              <TabsContent value="footprint" className="space-y-6">
                <Card>
                  <CardHeader>
                    <CardTitle className="flex items-center">
                      <MapPin className="h-5 w-5 mr-2 text-blue-600" />
                      Promotion Footprint & Applicability
                    </CardTitle>
                  </CardHeader>
                  <CardContent>
                    {promotion.ReconstructedFootprintData || (!promotion.SchemeOrgs || promotion.SchemeOrgs.length === 0) ? (
                      <div className="space-y-6">
                        {/* Footprint Type Header */}
                        <div className="flex items-center justify-between">
                          <div>
                            <label className="text-sm font-medium text-gray-500">Footprint Type</label>
                            <p className="mt-1 text-lg font-semibold text-gray-900 capitalize">
                              {promotion.ReconstructedFootprintData?.SelectionType || 
                               (!promotion.SchemeOrgs || promotion.SchemeOrgs.length === 0 ? "Universal" : "Specific")}
                            </p>
                          </div>
                          <div className="text-right">
                            <label className="text-sm font-medium text-gray-500">Total Coverage</label>
                            <p className="mt-1 text-2xl font-semibold text-green-600">
                              {promotion.ReconstructedFootprintData?.TotalCoverage || 
                               (!promotion.SchemeOrgs || promotion.SchemeOrgs.length === 0 ? "All Locations" : "Selected")}
                            </p>
                          </div>
                        </div>

                        {/* Coverage Summary */}
                        {promotion.ReconstructedFootprintData?.CoverageByType && (
                          <div>
                            <h4 className="text-md font-medium text-gray-900 mb-3">Coverage Summary</h4>
                            <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-6 gap-4">
                              {Object.entries(promotion.ReconstructedFootprintData.CoverageByType).map(([type, count]) => (
                                <div key={type} className="bg-gray-50 p-3 rounded text-center">
                                  <div className="text-lg font-semibold text-gray-900">{count}</div>
                                  <div className="text-xs text-gray-500">{type}</div>
                                </div>
                              ))}
                            </div>
                          </div>
                        )}

                        {/* Organizations */}
                        {promotion.ReconstructedFootprintData?.OrganizationSelections &&
                          promotion.ReconstructedFootprintData.OrganizationSelections.length > 0 && (
                            <div>
                              <h4 className="text-md font-medium text-gray-900 mb-2">Selected Organizations</h4>
                              <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
                                {promotion.ReconstructedFootprintData.OrganizationSelections.map((org, index) => (
                                  <div key={index} className="flex items-center bg-gray-50 p-3 rounded">
                                    <Building className="h-5 w-5 text-gray-400 mr-3" />
                                    <div>
                                      <div className="font-medium text-gray-900">{org}</div>
                                      <div className="text-sm text-gray-500">Organization ID: {org}</div>
                                    </div>
                                  </div>
                                ))}
                              </div>
                            </div>
                          )}

                        {/* Store Groups */}
                        {promotion.ReconstructedFootprintData?.StoreGroupSelections &&
                          promotion.ReconstructedFootprintData.StoreGroupSelections.length > 0 && (
                            <div>
                              <h4 className="text-md font-medium text-gray-900 mb-2">Selected Store Groups</h4>
                              <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
                                {promotion.ReconstructedFootprintData.StoreGroupSelections.map((group, index) => (
                                  <div key={index} className="flex items-center bg-gray-50 p-3 rounded">
                                    <Store className="h-5 w-5 text-gray-400 mr-3" />
                                    <div>
                                      <div className="font-medium text-gray-900">{group}</div>
                                      <div className="text-sm text-gray-500">Group ID: {group}</div>
                                    </div>
                                  </div>
                                ))}
                              </div>
                            </div>
                          )}

                        {/* Branches */}
                        {promotion.ReconstructedFootprintData?.BranchSelections &&
                          promotion.ReconstructedFootprintData.BranchSelections.length > 0 && (
                            <div>
                              <h4 className="text-md font-medium text-gray-900 mb-2">Selected Branches</h4>
                              <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
                                {promotion.ReconstructedFootprintData.BranchSelections.map((branch, index) => (
                                  <div key={index} className="flex items-center bg-gray-50 p-3 rounded">
                                    <MapPin className="h-5 w-5 text-gray-400 mr-3" />
                                    <div>
                                      <div className="font-medium text-gray-900">{branch}</div>
                                      <div className="text-sm text-gray-500">Branch ID: {branch}</div>
                                    </div>
                                  </div>
                                ))}
                              </div>
                            </div>
                          )}

                        {/* Stores */}
                        {promotion.ReconstructedFootprintData?.StoreSelections &&
                          promotion.ReconstructedFootprintData.StoreSelections.length > 0 && (
                            <div>
                              <h4 className="text-md font-medium text-gray-900 mb-2">Selected Stores</h4>
                              <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-3">
                                {promotion.ReconstructedFootprintData.StoreSelections.map((store, index) => (
                                  <div key={index} className="flex items-center bg-gray-50 p-3 rounded">
                                    <Store className="h-5 w-5 text-gray-400 mr-3" />
                                    <div>
                                      <div className="font-medium text-gray-900">{store}</div>
                                      <div className="text-sm text-gray-500">Store ID: {store}</div>
                                    </div>
                                  </div>
                                ))}
                              </div>
                            </div>
                          )}

                        {/* Customers */}
                        {promotion.ReconstructedFootprintData?.CustomerSelections &&
                          promotion.ReconstructedFootprintData.CustomerSelections.length > 0 && (
                            <div>
                              <h4 className="text-md font-medium text-gray-900 mb-2">Selected Customers</h4>
                              <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-3">
                                {promotion.ReconstructedFootprintData.CustomerSelections.map((customer, index) => (
                                  <div key={index} className="flex items-center bg-gray-50 p-3 rounded">
                                    <User className="h-5 w-5 text-gray-400 mr-3" />
                                    <div>
                                      <div className="font-medium text-gray-900">{customer}</div>
                                      <div className="text-sm text-gray-500">Customer ID: {customer}</div>
                                    </div>
                                  </div>
                                ))}
                              </div>
                            </div>
                          )}

                        {/* Salesmen */}
                        {promotion.ReconstructedFootprintData?.SalesmanSelections &&
                          promotion.ReconstructedFootprintData.SalesmanSelections.length > 0 && (
                            <div>
                              <h4 className="text-md font-medium text-gray-900 mb-2">Selected Salesmen</h4>
                              <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-3">
                                {promotion.ReconstructedFootprintData.SalesmanSelections.map((salesman, index) => (
                                  <div key={index} className="flex items-center bg-gray-50 p-3 rounded">
                                    <UserCheck className="h-5 w-5 text-gray-400 mr-3" />
                                    <div>
                                      <div className="font-medium text-gray-900">{salesman}</div>
                                      <div className="text-sm text-gray-500">Salesman ID: {salesman}</div>
                                    </div>
                                  </div>
                                ))}
                              </div>
                            </div>
                          )}

                        {/* Universal Promotion Message */}
                        {(promotion.ReconstructedFootprintData?.SelectionType === "all" || 
                          (!promotion.SchemeOrgs || promotion.SchemeOrgs.length === 0)) && (
                          <div className="bg-green-50 border border-green-200 rounded-lg p-4">
                            <div className="flex items-center">
                              <CheckCircle className="h-5 w-5 text-green-400 mr-2" />
                              <div>
                                <h4 className="text-sm font-medium text-green-800">Universal Promotion</h4>
                                <p className="text-sm text-green-700 mt-1">
                                  This promotion applies to all organizations, stores, and customers without restrictions.
                                </p>
                              </div>
                            </div>
                          </div>
                        )}

                        {/* Legacy Footprint Data Fallback */}
                        {(!promotion.ReconstructedFootprintData?.OrganizationSelections ||
                          promotion.ReconstructedFootprintData?.OrganizationSelections.length === 0) &&
                          (!promotion.ReconstructedFootprintData?.BranchSelections ||
                            promotion.ReconstructedFootprintData?.BranchSelections.length === 0) &&
                          (promotion.SchemeBranches || promotion.SchemeOrgs) && (
                            <div>
                              <h4 className="text-md font-medium text-gray-900 mb-3">Legacy Coverage Data</h4>
                              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                                {/* Legacy Organizations */}
                                {promotion.SchemeOrgs && promotion.SchemeOrgs.length > 0 && (
                                  <div>
                                    <h5 className="text-sm font-medium text-gray-700 mb-2">Organizations</h5>
                                    <div className="space-y-2">
                                      {promotion.SchemeOrgs.map((org) => (
                                        <div key={org.UID} className="flex items-center bg-gray-50 p-2 rounded text-sm">
                                          <Building className="h-4 w-4 text-gray-400 mr-2" />
                                          {org.OrgUID}
                                        </div>
                                      ))}
                                    </div>
                                  </div>
                                )}

                                {/* Legacy Branches */}
                                {promotion.SchemeBranches && promotion.SchemeBranches.length > 0 && (
                                  <div>
                                    <h5 className="text-sm font-medium text-gray-700 mb-2">Branches</h5>
                                    <div className="space-y-2">
                                      {promotion.SchemeBranches.map((branch) => (
                                        <div key={branch.UID} className="flex items-center bg-gray-50 p-2 rounded text-sm">
                                          <MapPin className="h-4 w-4 text-gray-400 mr-2" />
                                          {branch.BranchUID}
                                        </div>
                                      ))}
                                    </div>
                                  </div>
                                )}
                              </div>
                            </div>
                          )}
                      </div>
                    ) : (
                      <div className="text-center py-8 text-gray-500">
                        <MapPin className="mx-auto h-12 w-12 text-gray-300 mb-4" />
                        <p>No footprint data available</p>
                        <p className="text-sm">This promotion may apply to all locations by default</p>
                      </div>
                    )}
                  </CardContent>
                </Card>
              </TabsContent>

              {/* Volume Caps Tab */}
              <TabsContent value="volumecaps" className="space-y-6">
                {promotion.PromotionVolumeCap ? (
                  <div className="space-y-6">
                    {/* Overall Cap */}
                    {(promotion.PromotionVolumeCap.OverallCapValue || 0) > 0 && (
                      <Card className="border-blue-200 bg-blue-50">
                        <CardHeader>
                          <CardTitle className="text-blue-900">Overall Volume Cap</CardTitle>
                        </CardHeader>
                        <CardContent>
                          <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-4">
                            <div>
                              <label className="text-sm font-medium text-blue-700">Cap Value</label>
                              <p className="text-2xl font-bold text-blue-900">
                                {promotion.PromotionVolumeCap.OverallCapValue}
                              </p>
                            </div>
                            <div>
                              <label className="text-sm font-medium text-blue-700">Cap Type</label>
                              <p className="text-lg font-semibold text-blue-900">
                                {promotion.PromotionVolumeCap.OverallCapType || "VALUE"}
                              </p>
                            </div>
                            <div>
                              <label className="text-sm font-medium text-blue-700">Consumed</label>
                              <p className="text-lg font-semibold text-blue-900">
                                {promotion.PromotionVolumeCap.OverallCapConsumed || 0}
                              </p>
                            </div>
                          </div>
                          
                          {/* Progress Bar */}
                          <div className="mt-3">
                            <div className="bg-blue-200 rounded-full h-3">
                              <div
                                className="bg-blue-600 h-3 rounded-full transition-all duration-300"
                                style={{
                                  width: `${Math.min(
                                    100,
                                    ((promotion.PromotionVolumeCap.OverallCapConsumed || 0) /
                                      (promotion.PromotionVolumeCap.OverallCapValue || 1)) * 100
                                  )}%`
                                }}
                              ></div>
                            </div>
                            <p className="text-xs text-blue-700 mt-1">
                              {(
                                ((promotion.PromotionVolumeCap.OverallCapConsumed || 0) /
                                  (promotion.PromotionVolumeCap.OverallCapValue || 1)) * 100
                              ).toFixed(1)}% consumed
                            </p>
                          </div>
                        </CardContent>
                      </Card>
                    )}

                    {/* Invoice Caps */}
                    {((promotion.PromotionVolumeCap.InvoiceMaxDiscountValue || 0) > 0 ||
                      (promotion.PromotionVolumeCap.InvoiceMaxQuantity || 0) > 0 ||
                      (promotion.PromotionVolumeCap.InvoiceMaxApplications || 0) > 1) && (
                      <Card className="border-yellow-200 bg-yellow-50">
                        <CardHeader>
                          <CardTitle className="text-yellow-900">Invoice Level Caps</CardTitle>
                        </CardHeader>
                        <CardContent>
                          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                            {(promotion.PromotionVolumeCap.InvoiceMaxDiscountValue || 0) > 0 && (
                              <div>
                                <label className="text-sm font-medium text-yellow-700">Max Discount Value</label>
                                <p className="text-xl font-bold text-yellow-900">
                                  ${promotion.PromotionVolumeCap.InvoiceMaxDiscountValue}
                                </p>
                              </div>
                            )}
                            {(promotion.PromotionVolumeCap.InvoiceMaxQuantity || 0) > 0 && (
                              <div>
                                <label className="text-sm font-medium text-yellow-700">Max Quantity</label>
                                <p className="text-xl font-bold text-yellow-900">
                                  {promotion.PromotionVolumeCap.InvoiceMaxQuantity}
                                </p>
                              </div>
                            )}
                            {(promotion.PromotionVolumeCap.InvoiceMaxApplications || 0) > 1 && (
                              <div>
                                <label className="text-sm font-medium text-yellow-700">Max Applications</label>
                                <p className="text-xl font-bold text-yellow-900">
                                  {promotion.PromotionVolumeCap.InvoiceMaxApplications}
                                </p>
                              </div>
                            )}
                          </div>
                        </CardContent>
                      </Card>
                    )}

                    {/* Period Caps */}
                    {(promotion.PromotionVolumeCap.TotalCapEnabled ||
                      promotion.PromotionVolumeCap.DailyCapEnabled ||
                      promotion.PromotionVolumeCap.WeeklyCapEnabled ||
                      promotion.PromotionVolumeCap.MonthlyCapEnabled) && (
                      <Card className="border-green-200 bg-green-50">
                        <CardHeader>
                          <CardTitle className="text-green-900">Period Caps</CardTitle>
                        </CardHeader>
                        <CardContent>
                          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
                            {promotion.PromotionVolumeCap.TotalCapEnabled && (
                              <div className="bg-white p-3 rounded border border-green-300">
                                <label className="text-sm font-medium text-green-700">Total Cap</label>
                                <p className="text-lg font-bold text-green-900">
                                  {promotion.PromotionVolumeCap.TotalCapValue} {promotion.PromotionVolumeCap.TotalCapUnit || "VALUE"}
                                </p>
                              </div>
                            )}
                            {promotion.PromotionVolumeCap.DailyCapEnabled && (
                              <div className="bg-white p-3 rounded border border-green-300">
                                <label className="text-sm font-medium text-green-700">Daily Cap</label>
                                <p className="text-lg font-bold text-green-900">
                                  {promotion.PromotionVolumeCap.DailyCapValue}
                                </p>
                              </div>
                            )}
                            {promotion.PromotionVolumeCap.WeeklyCapEnabled && (
                              <div className="bg-white p-3 rounded border border-green-300">
                                <label className="text-sm font-medium text-green-700">Weekly Cap</label>
                                <p className="text-lg font-bold text-green-900">
                                  {promotion.PromotionVolumeCap.WeeklyCapValue}
                                </p>
                              </div>
                            )}
                            {promotion.PromotionVolumeCap.MonthlyCapEnabled && (
                              <div className="bg-white p-3 rounded border border-green-300">
                                <label className="text-sm font-medium text-green-700">Monthly Cap</label>
                                <p className="text-lg font-bold text-green-900">
                                  {promotion.PromotionVolumeCap.MonthlyCapValue}
                                </p>
                              </div>
                            )}
                          </div>
                        </CardContent>
                      </Card>
                    )}

                    {/* Location Caps */}
                    {(promotion.PromotionVolumeCap.StoreCapEnabled ||
                      promotion.PromotionVolumeCap.RegionCapEnabled ||
                      promotion.PromotionVolumeCap.CustomerCapEnabled) && (
                      <Card className="border-purple-200 bg-purple-50">
                        <CardHeader>
                          <CardTitle className="text-purple-900">Location & Entity Caps</CardTitle>
                        </CardHeader>
                        <CardContent>
                          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                            {promotion.PromotionVolumeCap.StoreCapEnabled && (
                              <div className="bg-white p-3 rounded border border-purple-300">
                                <label className="text-sm font-medium text-purple-700">Store Cap</label>
                                <p className="text-lg font-bold text-purple-900">
                                  {promotion.PromotionVolumeCap.StoreCapValue}
                                </p>
                              </div>
                            )}
                            {promotion.PromotionVolumeCap.RegionCapEnabled && (
                              <div className="bg-white p-3 rounded border border-purple-300">
                                <label className="text-sm font-medium text-purple-700">Region Cap</label>
                                <p className="text-lg font-bold text-purple-900">
                                  {promotion.PromotionVolumeCap.RegionCapValue}
                                </p>
                              </div>
                            )}
                            {promotion.PromotionVolumeCap.CustomerCapEnabled && (
                              <div className="bg-white p-3 rounded border border-purple-300">
                                <label className="text-sm font-medium text-purple-700">Customer Cap</label>
                                <p className="text-lg font-bold text-purple-900">
                                  {promotion.PromotionVolumeCap.CustomerCapValue}
                                </p>
                              </div>
                            )}
                          </div>
                        </CardContent>
                      </Card>
                    )}

                    {/* Cap Reset Information */}
                    {promotion.PromotionVolumeCap.CapResetFrequency && (
                      <Card className="border-gray-200 bg-gray-50">
                        <CardHeader>
                          <CardTitle className="text-sm text-gray-900">Cap Reset Information</CardTitle>
                        </CardHeader>
                        <CardContent>
                          <div className="grid grid-cols-1 md:grid-cols-2 gap-4 text-sm">
                            <div>
                              <label className="font-medium text-gray-500">Reset Frequency</label>
                              <p className="text-gray-900">{promotion.PromotionVolumeCap.CapResetFrequency}</p>
                            </div>
                            {promotion.PromotionVolumeCap.CapResetDay && (
                              <div>
                                <label className="font-medium text-gray-500">Reset Day</label>
                                <p className="text-gray-900">{promotion.PromotionVolumeCap.CapResetDay}</p>
                              </div>
                            )}
                          </div>
                        </CardContent>
                      </Card>
                    )}
                  </div>
                ) : (
                  <Card>
                    <CardContent className="py-12 text-center">
                      <Settings className="mx-auto h-12 w-12 text-gray-300 mb-4" />
                      <h3 className="text-lg font-medium text-gray-900 mb-2">No Volume Caps</h3>
                      <p className="text-gray-500">This promotion does not have any volume caps configured.</p>
                    </CardContent>
                  </Card>
                )}
              </TabsContent>

              {/* Review Tab */}
              <TabsContent value="review" className="space-y-6">
                <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                  {/* Promotion Summary */}
                  <Card>
                    <CardHeader>
                      <CardTitle className="flex items-center">
                        <Info className="h-5 w-5 mr-2 text-blue-600" />
                        Promotion Summary
                      </CardTitle>
                    </CardHeader>
                    <CardContent className="space-y-4">
                      <div>
                        <label className="text-sm font-medium text-gray-500">Name</label>
                        <p className="text-base font-semibold">{promotionData?.Name || "N/A"}</p>
                      </div>
                      <div>
                        <label className="text-sm font-medium text-gray-500">Code</label>
                        <p className="text-base">{promotionData?.Code || "N/A"}</p>
                      </div>
                      <div>
                        <label className="text-sm font-medium text-gray-500">Type & Format</label>
                        <p className="text-base">{promotionData?.Level || "N/A"} - {promotionData?.PromoFormat || "N/A"}</p>
                      </div>
                      <div>
                        <label className="text-sm font-medium text-gray-500">Validity</label>
                        <p className="text-base">{formatDate(promotionData?.ValidFrom)} to {formatDate(promotionData?.ValidUpto)}</p>
                      </div>
                      <div>
                        <label className="text-sm font-medium text-gray-500">Status</label>
                        <div className="mt-1">{getStatusBadge()}</div>
                      </div>
                    </CardContent>
                  </Card>

                  {/* Configuration Summary */}
                  <Card>
                    <CardHeader>
                      <CardTitle className="flex items-center">
                        <Settings className="h-5 w-5 mr-2 text-green-600" />
                        Configuration Summary
                      </CardTitle>
                    </CardHeader>
                    <CardContent className="space-y-4">
                      <div>
                        <label className="text-sm font-medium text-gray-500">Products</label>
                        <p className="text-base">
                          {promotion.ReconstructedProductSelections?.TotalProducts || 
                           promotion.ItemPromotionMapViewList?.length || 0} products selected
                        </p>
                      </div>
                      <div>
                        <label className="text-sm font-medium text-gray-500">Orders</label>
                        <p className="text-base">{promotion.PromoOrderViewList?.length || 0} order configurations</p>
                      </div>
                      <div>
                        <label className="text-sm font-medium text-gray-500">Offers</label>
                        <p className="text-base">{promotion.PromoOfferViewList?.length || 0} offer configurations</p>
                      </div>
                      <div>
                        <label className="text-sm font-medium text-gray-500">Branches</label>
                        <p className="text-base">
                          {promotion.SchemeBranches?.length ? 
                            `${promotion.SchemeBranches.length} specific branches` : "All branches"}
                        </p>
                      </div>
                      <div>
                        <label className="text-sm font-medium text-gray-500">Organizations</label>
                        <p className="text-base">
                          {promotion.SchemeOrgs?.length ? 
                            `${promotion.SchemeOrgs.length} specific organizations` : "All organizations"}
                        </p>
                      </div>
                    </CardContent>
                  </Card>
                </div>

                {/* Audit Information */}
                <Card>
                  <CardHeader>
                    <CardTitle className="flex items-center">
                      <Clock className="h-5 w-5 mr-2 text-gray-600" />
                      Audit Information
                    </CardTitle>
                  </CardHeader>
                  <CardContent className="grid grid-cols-1 md:grid-cols-2 gap-6">
                    <div className="space-y-4">
                      <div>
                        <label className="text-sm font-medium text-gray-500">Created Time</label>
                        <p className="text-base">{formatDate(promotionData?.CreatedTime)}</p>
                      </div>
                      <div>
                        <label className="text-sm font-medium text-gray-500">Created By</label>
                        <p className="text-base">{promotionData?.CreatedByEmpUID || "N/A"}</p>
                      </div>
                    </div>
                    <div className="space-y-4">
                      <div>
                        <label className="text-sm font-medium text-gray-500">Last Modified</label>
                        <p className="text-base">{formatDate(promotionData?.ModifiedTime)}</p>
                      </div>
                      <div>
                        <label className="text-sm font-medium text-gray-500">Server Modified</label>
                        <p className="text-base">{formatDate(promotionData?.ServerModifiedTime)}</p>
                      </div>
                    </div>
                  </CardContent>
                </Card>
              </TabsContent>
            </div>
          </Tabs>
        </CardContent>
      </Card>
    </div>
  );
}