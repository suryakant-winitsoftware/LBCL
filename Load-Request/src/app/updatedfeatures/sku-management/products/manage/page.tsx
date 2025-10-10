"use client";

import { useState, useEffect } from "react";
import { useRouter } from "next/navigation";
import { Button } from "@/components/ui/button";
import { Card } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import {
  Plus,
  Search,
  Filter,
  Download,
  Upload,
  GitBranch,
  MoreVertical,
  Eye,
  Edit,
  Trash2,
  TreePine,
  Tags,
  Link2,
  Database,
  BarChart3,
  Settings,
  Ruler
} from "lucide-react";
import {
  skuService,
  SKUListView,
  PagedResponse
} from "@/services/sku/sku.service";
import { PagingRequest } from "@/types/common.types";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow
} from "@/components/ui/table";
import { Badge } from "@/components/ui/badge";
import { useToast } from "@/components/ui/use-toast";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger
} from "@/components/ui/dropdown-menu";
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle
} from "@/components/ui/alert-dialog";

export default function ManageSKUsPage() {
  const router = useRouter();
  const { toast } = useToast();
  const [skus, setSKUs] = useState<SKUListView[]>([]);
  const [loading, setLoading] = useState(false);
  const [searchTerm, setSearchTerm] = useState("");
  const [currentPage, setCurrentPage] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  const [selectedSKU, setSelectedSKU] = useState<SKUListView | null>(null);
  const [deleting, setDeleting] = useState(false);
  const pageSize = 10;

  const fetchSKUs = async () => {
    setLoading(true);
    try {
      const request: PagingRequest = {
        PageNumber: currentPage,
        PageSize: 50, // Increase page size to see if more products exist
        FilterCriterias: searchTerm
          ? [{ Name: "skucodeandname", Value: searchTerm }]
          : [],
        SortCriterias: [{ SortParameter: "SKUCode", Direction: "Asc" }],
        IsCountRequired: true
      };

      console.log("🔍 Fetching SKUs with request:", request);
      console.log("🔍 Current user org:", localStorage.getItem("user_info"));
      console.log(
        "🔍 Auth token exists:",
        !!localStorage.getItem("auth_token")
      );

      // Use the same approach as promotions service
      const response = await skuService.getAllSKUs(request);

      console.log("📦 Raw SKU Response:", response);
      console.log("📦 Response data:", response.data || response);
      console.log("📦 Response data type:", typeof (response.data || response));

      if (response && response.success !== false) {
        let skuData: SKUListView[] = [];

        // Handle multiple response structures - same as promotions
        const dataSource = response.data || response;

        // Check for nested Data.PagedData structure (backend wraps response)
        if (
          dataSource.Data?.PagedData &&
          Array.isArray(dataSource.Data.PagedData)
        ) {
          console.log(
            "✅ Found Data.PagedData with",
            dataSource.Data.PagedData.length,
            "items"
          );
          skuData = dataSource.Data.PagedData;
          setTotalCount(
            dataSource.Data.TotalCount || dataSource.Data.PagedData.length
          );
        } else if (
          dataSource.PagedData &&
          Array.isArray(dataSource.PagedData)
        ) {
          console.log(
            "✅ Found PagedData with",
            dataSource.PagedData.length,
            "items"
          );
          skuData = dataSource.PagedData;
          setTotalCount(dataSource.TotalCount || dataSource.PagedData.length);
        } else if (dataSource.Data && Array.isArray(dataSource.Data)) {
          console.log("✅ Found Data with", dataSource.Data.length, "items");
          skuData = dataSource.Data;
          setTotalCount(dataSource.Data.length);
        } else if (dataSource.items && Array.isArray(dataSource.items)) {
          console.log("✅ Found items with", dataSource.items.length, "items");
          skuData = dataSource.items;
          setTotalCount(dataSource.items.length);
        } else if (Array.isArray(dataSource)) {
          console.log(
            "✅ Response data is array with",
            dataSource.length,
            "items"
          );
          skuData = dataSource;
          setTotalCount(dataSource.length);
        } else {
          console.log("⚠️ No recognized data structure found in response");
          console.log(
            "Available keys in response:",
            dataSource ? Object.keys(dataSource) : "null"
          );
          if (dataSource?.Data) {
            console.log(
              "Available keys in response.Data:",
              Object.keys(dataSource.Data)
            );
          }
        }

        // Log first SKU to see field names
        if (skuData.length > 0) {
          console.log("🔍 First SKU data structure:", skuData[0]);
          console.log("🔍 SKUUID field:", skuData[0]?.SKUUID);
          console.log("🔍 SKUCode field:", skuData[0]?.SKUCode);
          console.log("🔍 Available fields:", Object.keys(skuData[0] || {}));
        }

        // Map the actual field names to expected field names
        const mappedSKUs = skuData.map((sku: any) => ({
          SKUUID: sku.UID || sku.Id || sku.Code, // Map UID to SKUUID
          SKUCode: sku.Code || sku.UID, // Map Code to SKUCode
          SKULongName: sku.LongName || sku.Name || sku.AliasName, // Map LongName to SKULongName
          IsActive: sku.IsActive !== false, // Keep IsActive as is
          // Keep all original fields for debugging
          ...sku
        }));

        console.log("📋 Mapped SKUs:", mappedSKUs);
        console.log("🔍 First mapped SKU:", {
          SKUUID: mappedSKUs[0]?.SKUUID,
          SKUCode: mappedSKUs[0]?.SKUCode,
          SKULongName: mappedSKUs[0]?.SKULongName,
          IsActive: mappedSKUs[0]?.IsActive
        });
        setSKUs(mappedSKUs);
      } else {
        console.error("❌ API returned unsuccessful response:", response);
        setSKUs([]);
        setTotalCount(0);

        toast({
          title: "No Data",
          description: response.error || "No products found or API error",
          variant: "default"
        });
      }
    } catch (error) {
      console.error("❌ Error fetching SKUs:", error);
      toast({
        title: "Error",
        description: `Failed to fetch SKUs: ${
          error instanceof Error ? error.message : "Unknown error"
        }`,
        variant: "destructive"
      });
      // Set empty arrays on error
      setSKUs([]);
      setTotalCount(0);
    } finally {
      setLoading(false);
    }
  };

  // Try alternative API endpoint that might have all 167 SKUs
  const fetchAllSKUMasterData = async () => {
    setLoading(true);
    try {
      console.log("🧪 Trying getAllSKUMasterData endpoint...");

      // Use the same endpoint the promotion service uses for all SKU data
      const response = await fetch(
        `${
          process.env.NEXT_PUBLIC_API_URL || "http://localhost:8000/api"
        }/SKU/GetAllSKUMasterData`,
        {
          method: "POST",
          headers: {
            Authorization: `Bearer ${localStorage.getItem("auth_token")}`,
            "Content-Type": "application/json",
            Accept: "application/json"
          },
          body: JSON.stringify({
            SKUUIDs: [],
            OrgUIDs: [],
            DistributionChannelUIDs: [],
            AttributeTypes: [],
            PageNumber: 1,
            PageSize: 200 // Get more records
          })
        }
      );

      if (response.ok) {
        const result = await response.json();
        console.log("🎯 GetAllSKUMasterData response:", result);

        if (result.IsSuccess && result.Data?.PagedData) {
          const skuData = result.Data.PagedData.map((item: any) => {
            const sku = item.SKU || item;
            return {
              SKUUID: sku.UID || sku.Code,
              SKUCode: sku.Code,
              SKULongName: sku.LongName || sku.Name,
              IsActive: sku.IsActive !== false,
              ...sku
            };
          });
          console.log(
            `✅ Found ${skuData.length} SKUs from GetAllSKUMasterData`
          );
          setSKUs(skuData);
          setTotalCount(result.Data.TotalCount || skuData.length);

          toast({
            title: "Success",
            description: `Loaded ${skuData.length} products from master data API`
          });
        }
      }
    } catch (error) {
      console.error("❌ GetAllSKUMasterData failed:", error);
      toast({
        title: "Error",
        description: "Alternative endpoint also failed",
        variant: "destructive"
      });
    } finally {
      setLoading(false);
    }
  };

  // Refresh data method
  const refreshData = async () => {
    setCurrentPage(1);
    await fetchSKUs();
  };

  // Try master data method
  const tryMasterData = async () => {
    await fetchAllSKUMasterData();
  };

  useEffect(() => {
    fetchSKUs();
  }, [currentPage, searchTerm]);

  const handleSearch = (value: string) => {
    setSearchTerm(value);
    setCurrentPage(1);
  };

  const handleViewDetails = (skuUID: string | undefined) => {
    if (!skuUID) return;
    router.push(`/updatedfeatures/sku-management/products/view?uid=${skuUID}`);
  };

  const handleEdit = (skuUID: string | undefined) => {
    if (!skuUID) return;
    router.push(`/updatedfeatures/sku-management/products/edit?uid=${skuUID}`);
  };

  const handleDeleteClick = (sku: SKUListView) => {
    setSelectedSKU(sku);
    setDeleteDialogOpen(true);
  };

  const handleDeleteConfirm = async () => {
    if (!selectedSKU) return;

    setDeleting(true);
    try {
      const skuId = selectedSKU.SKUUID || selectedSKU.UID;
      if (!skuId) {
        throw new Error("No valid SKU ID found");
      }
      await skuService.deleteSKU(skuId);
      toast({
        title: "Success",
        description: `Product "${
          selectedSKU.SKULongName || selectedSKU.LongName || selectedSKU.Name
        }" has been deleted successfully.`
      });
      // Refresh the list
      fetchSKUs();
    } catch (error) {
      toast({
        title: "Error",
        description: "Failed to delete product. Please try again.",
        variant: "destructive"
      });
      console.error("Error deleting SKU:", error);
    } finally {
      setDeleting(false);
      setDeleteDialogOpen(false);
      setSelectedSKU(null);
    }
  };

  const totalPages = Math.ceil(totalCount / pageSize);

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Manage Products</h1>
          <p className="text-muted-foreground">
            Manage your product catalog and SKU information
          </p>
        </div>
        <div className="flex gap-2">
          <Button
            variant="outline"
            onClick={() =>
              router.push("/updatedfeatures/sku-management/products/hierarchy")
            }
          >
            <GitBranch className="h-4 w-4 mr-2" />
            View Hierarchy
          </Button>
          <Button variant="outline" size="sm">
            <Upload className="h-4 w-4 mr-2" />
            Import
          </Button>
          <Button variant="outline" size="sm">
            <Download className="h-4 w-4 mr-2" />
            Export
          </Button>
          <Button
            onClick={() =>
              router.push("/updatedfeatures/sku-management/products/create")
            }
          >
            <Plus className="h-4 w-4 mr-2" />
            Add Product
          </Button>
          <Button variant="outline" onClick={refreshData} disabled={loading}>
            <Database className="h-4 w-4 mr-2" />
            {loading ? "Refreshing..." : "Refresh"}
          </Button>
          <Button variant="outline" onClick={tryMasterData} disabled={loading}>
            <Search className="h-4 w-4 mr-2" />
            {loading ? "Loading..." : "Try Master Data API"}
          </Button>
        </div>
      </div>

      {/* Quick Navigation to New Management Pages */}
      <Card className="p-6 bg-gradient-to-r from-blue-50 to-indigo-50 border-blue-200">
        <div className="mb-4">
          <h2 className="text-lg font-semibold mb-2 flex items-center gap-2">
            <Settings className="h-5 w-5 text-blue-600" />
            Comprehensive SKU Management Suite
          </h2>
          <p className="text-sm text-muted-foreground">
            Access all advanced management tools with live data integration and
            professional features
          </p>
        </div>

        <div className="grid gap-3 md:grid-cols-2 lg:grid-cols-5">
          <Button
            variant="outline"
            className="flex items-center gap-2 h-auto p-3 flex-col"
            onClick={() =>
              router.push("/updatedfeatures/sku-management/group-types")
            }
          >
            <TreePine className="h-5 w-5 text-green-600" />
            <div className="text-left">
              <div className="font-medium">Group Types</div>
              <div className="text-xs text-muted-foreground">
                Manage hierarchy types
              </div>
            </div>
          </Button>

          <Button
            variant="outline"
            className="flex items-center gap-2 h-auto p-3 flex-col"
            onClick={() =>
              router.push("/updatedfeatures/sku-management/groups")
            }
          >
            <Tags className="h-5 w-5 text-purple-600" />
            <div className="text-left">
              <div className="font-medium">Groups</div>
              <div className="text-xs text-muted-foreground">
                Manage actual groups
              </div>
            </div>
          </Button>

          <Button
            variant="outline"
            className="flex items-center gap-2 h-auto p-3 flex-col"
            onClick={() =>
              router.push("/updatedfeatures/sku-management/mappings")
            }
          >
            <Link2 className="h-5 w-5 text-orange-600" />
            <div className="text-left">
              <div className="font-medium">Mappings</div>
              <div className="text-xs text-muted-foreground">
                Link SKUs to groups
              </div>
            </div>
          </Button>

          <Button
            variant="outline"
            className="flex items-center gap-2 h-auto p-3 flex-col"
            onClick={() =>
              router.push("/updatedfeatures/sku-management/uom/manage")
            }
          >
            <Ruler className="h-5 w-5 text-blue-600" />
            <div className="text-left">
              <div className="font-medium">UOM Management</div>
              <div className="text-xs text-muted-foreground">
                Dynamic measurements
              </div>
            </div>
          </Button>

          <Button
            variant="outline"
            className="flex items-center gap-2 h-auto p-3 flex-col"
            onClick={() => router.push("/debug/api-investigation")}
          >
            <Database className="h-5 w-5 text-red-600" />
            <div className="text-left">
              <div className="font-medium">Debug Tools</div>
              <div className="text-xs text-muted-foreground">
                API investigation
              </div>
            </div>
          </Button>
        </div>

        <div className="mt-4 p-3 bg-white rounded-lg border border-blue-200">
          <div className="flex items-center justify-between text-sm">
            <div className="flex items-center gap-4">
              <span className="text-green-600 font-medium">
                ✅ 167 SKUs Active
              </span>
              <span className="text-blue-600 font-medium">
                🏗️ 3 Group Types
              </span>
              <span className="text-purple-600 font-medium">📊 10+ Groups</span>
              <span className="text-orange-600 font-medium">
                🔗 Live API Integration
              </span>
            </div>
            <Button
              variant="ghost"
              size="sm"
              onClick={() => router.push("/updatedfeatures/sku-management")}
              className="text-blue-600 hover:text-blue-800"
            >
              View Full Dashboard →
            </Button>
          </div>
        </div>
      </Card>

      <Card className="p-6">
        <div className="flex gap-4 mb-6">
          <div className="relative flex-1">
            <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 h-4 w-4" />
            <Input
              placeholder="Search by product name or code..."
              value={searchTerm}
              onChange={(e) => handleSearch(e.target.value)}
              className="pl-10"
            />
          </div>
          <Button variant="outline" size="icon">
            <Filter className="h-4 w-4" />
          </Button>
        </div>

        <div className="rounded-md border">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>SKU Code</TableHead>
                <TableHead>Product Name</TableHead>
                <TableHead>Status</TableHead>
                <TableHead className="text-right">Actions</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {loading ? (
                <TableRow>
                  <TableCell colSpan={4} className="text-center py-8">
                    Loading...
                  </TableCell>
                </TableRow>
              ) : skus.length === 0 ? (
                <TableRow>
                  <TableCell colSpan={4} className="text-center py-8">
                    No products found
                  </TableCell>
                </TableRow>
              ) : (
                skus.map((sku, index) => (
                  <TableRow
                    key={sku.SKUUID || sku.UID || sku.Code || `sku-${index}`}
                  >
                    <TableCell className="font-medium">
                      {sku.SKUCode || sku.Code}
                    </TableCell>
                    <TableCell>
                      {sku.SKULongName || sku.LongName || sku.Name}
                    </TableCell>
                    <TableCell>
                      <Badge variant={sku.IsActive ? "default" : "secondary"}>
                        {sku.IsActive ? "Active" : "Inactive"}
                      </Badge>
                    </TableCell>
                    <TableCell className="text-right">
                      <DropdownMenu>
                        <DropdownMenuTrigger asChild>
                          <Button variant="ghost" className="h-8 w-8 p-0">
                            <span className="sr-only">Open menu</span>
                            <MoreVertical className="h-4 w-4" />
                          </Button>
                        </DropdownMenuTrigger>
                        <DropdownMenuContent align="end">
                          <DropdownMenuLabel>Actions</DropdownMenuLabel>
                          <DropdownMenuItem
                            onClick={() =>
                              handleViewDetails(sku.SKUUID || sku.UID)
                            }
                            className="cursor-pointer"
                          >
                            <Eye className="mr-2 h-4 w-4" />
                            View Details
                          </DropdownMenuItem>
                          <DropdownMenuItem
                            onClick={() => handleEdit(sku.SKUUID || sku.UID)}
                            className="cursor-pointer"
                          >
                            <Edit className="mr-2 h-4 w-4" />
                            Edit Product
                          </DropdownMenuItem>
                          <DropdownMenuSeparator />
                          <DropdownMenuItem
                            onClick={() => handleDeleteClick(sku)}
                            className="cursor-pointer text-red-600 focus:text-red-600"
                          >
                            <Trash2 className="mr-2 h-4 w-4" />
                            Delete Product
                          </DropdownMenuItem>
                        </DropdownMenuContent>
                      </DropdownMenu>
                    </TableCell>
                  </TableRow>
                ))
              )}
            </TableBody>
          </Table>
        </div>

        {totalPages > 1 && (
          <div className="flex items-center justify-between mt-4">
            <p className="text-sm text-muted-foreground">
              Showing {(currentPage - 1) * pageSize + 1} to{" "}
              {Math.min(currentPage * pageSize, totalCount)} of {totalCount}{" "}
              products
            </p>
            <div className="flex gap-2">
              <Button
                variant="outline"
                size="sm"
                onClick={() => setCurrentPage(currentPage - 1)}
                disabled={currentPage === 1}
              >
                Previous
              </Button>
              <Button
                variant="outline"
                size="sm"
                onClick={() => setCurrentPage(currentPage + 1)}
                disabled={currentPage === totalPages}
              >
                Next
              </Button>
            </div>
          </div>
        )}
      </Card>

      {/* Delete Confirmation Dialog */}
      <AlertDialog open={deleteDialogOpen} onOpenChange={setDeleteDialogOpen}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Are you sure?</AlertDialogTitle>
            <AlertDialogDescription>
              This action cannot be undone. This will permanently delete the
              product
              {selectedSKU && (
                <>
                  {" "}
                  <strong>
                    "
                    {selectedSKU.SKULongName ||
                      selectedSKU.LongName ||
                      selectedSKU.Name}
                    "
                  </strong>{" "}
                  (Code: {selectedSKU.SKUCode || selectedSKU.Code})
                </>
              )}{" "}
              and remove all associated data.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel disabled={deleting}>Cancel</AlertDialogCancel>
            <AlertDialogAction
              onClick={handleDeleteConfirm}
              disabled={deleting}
              className="bg-red-600 hover:bg-red-700"
            >
              {deleting ? "Deleting..." : "Delete"}
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  );
}
