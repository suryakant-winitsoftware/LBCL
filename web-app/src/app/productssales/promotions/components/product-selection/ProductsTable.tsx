"use client";

import React, { useState, useEffect, useCallback, useRef } from "react";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Badge } from "@/components/ui/badge";
import { Skeleton } from "@/components/ui/skeleton";
import { Checkbox } from "@/components/ui/checkbox";
import { useToast } from "@/components/ui/use-toast";
import { ScrollArea } from "@/components/ui/scroll-area";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import {
  Search,
  Filter,
  ChevronDown,
  X,
  Plus,
  Minus,
  Loader2,
} from "lucide-react";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuCheckboxItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
  DropdownMenuItem,
} from "@/components/ui/dropdown-menu";

export interface SKUListView {
  SKUUID?: string;
  UID?: string;
  SKUCode?: string;
  Code?: string;
  SKULongName?: string;
  LongName?: string;
  Name?: string;
  IsActive?: boolean;
  [key: string]: any;
}

interface ProductsTableProps {
  onProductSelectionChange?: (
    selectedUIDs: string[],
    quantities: Record<string, number>
  ) => void;
  initialSelected?: string[];
  initialQuantities?: Record<string, number>;
}

const DEFAULT_PAGE_SIZE = 50;

export function ProductsTable({
  onProductSelectionChange,
  initialSelected = [],
  initialQuantities = {},
}: ProductsTableProps) {
  const { toast } = useToast();
  const [products, setProducts] = useState<SKUListView[]>([]);
  const [loading, setLoading] = useState(false);
  const [loadingMore, setLoadingMore] = useState(false);
  const [searchTerm, setSearchTerm] = useState("");
  const [currentPage, setCurrentPage] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const [pageSize] = useState(DEFAULT_PAGE_SIZE);
  const [hasMore, setHasMore] = useState(false);
  const [statusFilter, setStatusFilter] = useState<string[]>([]);
  const [selectedProducts, setSelectedProducts] = useState<Set<string>>(
    new Set(initialSelected)
  );
  const [productQuantities, setProductQuantities] =
    useState<Record<string, number>>(initialQuantities);
  const searchInputRef = useRef<HTMLInputElement>(null);
  const scrollAreaRef = useRef<HTMLDivElement>(null);

  // Debounce search to reduce API calls
  const [debouncedSearchTerm, setDebouncedSearchTerm] = useState(searchTerm);

  useEffect(() => {
    const timer = setTimeout(() => {
      setDebouncedSearchTerm(searchTerm);
    }, 300); // 300ms delay

    return () => clearTimeout(timer);
  }, [searchTerm]);

  // Add keyboard shortcut for Ctrl+F / Cmd+F
  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      // Check for Ctrl+F (Windows/Linux) or Cmd+F (Mac)
      if ((e.ctrlKey || e.metaKey) && e.key === "f") {
        e.preventDefault(); // Prevent browser's default find
        searchInputRef.current?.focus();
        searchInputRef.current?.select(); // Select existing text for easy replacement
      }
    };

    document.addEventListener("keydown", handleKeyDown);
    return () => document.removeEventListener("keydown", handleKeyDown);
  }, []);

  // Primary API endpoint - Using SelectAllSKUDetailsWebView with lazy loading
  const fetchProductsPage = useCallback(
    async (page: number, isLoadingMore: boolean = false) => {
      if (isLoadingMore) {
        setLoadingMore(true);
      } else {
        setLoading(true);
      }

      try {
        // Build request body matching backend PagingRequest model
        const filterCriterias = [];

        // Add search filter if present
        if (debouncedSearchTerm && debouncedSearchTerm.trim()) {
          filterCriterias.push({
            Name: "skucodeandname",
            Value: debouncedSearchTerm.trim(),
          });
        }

        // Add status filter if selected
        if (statusFilter.length > 0) {
          // If only one status is selected, filter by IsActive
          if (statusFilter.length === 1) {
            filterCriterias.push({
              Name: "IsActive",
              Value: statusFilter[0] === "Active" ? "true" : "false",
            });
          }
          // If both are selected, no need to filter (show all)
        }

        const requestBody = {
          PageNumber: page,
          PageSize: pageSize,
          IsCountRequired: page === 1, // Only get count on first request
          FilterCriterias: filterCriterias,
          SortCriterias: [{ SortParameter: "SKUCode", Direction: "Asc" }],
        };

        console.log(
          `📊 ${
            isLoadingMore ? "Loading More" : "Fetching"
          } Products - Page: ${page}, Size: ${pageSize}`,
          debouncedSearchTerm ? `Search: ${debouncedSearchTerm}` : ""
        );

        const response = await fetch(
          `${
            process.env.NEXT_PUBLIC_API_URL || "http://localhost:8000/api"
          }/SKU/SelectAllSKUDetailsWebView`,
          {
            method: "POST",
            headers: {
              Authorization: `Bearer ${localStorage.getItem("auth_token")}`,
              "Content-Type": "application/json",
              Accept: "application/json",
            },
            body: JSON.stringify(requestBody),
          }
        );

        if (response.ok) {
          const result = await response.json();

          if (result.IsSuccess && result.Data) {
            const data = result.Data;
            let newProducts: any[] = [];

            // Handle PagedResponse structure from backend
            if (data.PagedData && Array.isArray(data.PagedData)) {
              newProducts = data.PagedData.map((product: any) => ({
                SKUUID: product.SKUUID || product.UID || product.Id,
                SKUCode: product.SKUCode || product.Code,
                SKULongName:
                  product.SKULongName || product.LongName || product.Name,
                IsActive: product.IsActive !== false,
                ...product,
              }));
            }

            let updatedProducts: any[] = [];
            let newTotalDisplayed = 0;

            if (isLoadingMore) {
              // Append new products to existing ones using functional update to get latest state
              let currentProductsLength = 0;
              setProducts((prevProducts) => {
                currentProductsLength = prevProducts.length;
                updatedProducts = [...prevProducts, ...newProducts];
                return updatedProducts;
              });
              newTotalDisplayed = currentProductsLength + newProducts.length;
            } else {
              // Replace products for new search/filter
              setProducts(newProducts);
              // Only update total count if we have a valid count (not -1 or undefined)
              if (data.TotalCount && data.TotalCount > 0) {
                setTotalCount(data.TotalCount);
              } else {
                setTotalCount(0); // Set to 0 when we don't have a reliable count
              }
              updatedProducts = newProducts;
              newTotalDisplayed = newProducts.length;
            }

            // Update hasMore: we have more if we got a full page AND haven't reached the total count
            let newHasMore = false;

            // Debug the total count issue
            console.log(
              `🔍 TotalCount Debug: data.TotalCount=${data.TotalCount}, newProducts.length=${newProducts.length}, pageSize=${pageSize}, isLoadingMore=${isLoadingMore}, newTotalDisplayed=${newTotalDisplayed}`
            );

            if (data.TotalCount && data.TotalCount > 0) {
              // If we have total count, check if we've loaded everything
              newHasMore = newTotalDisplayed < data.TotalCount;
              console.log(
                `📊 Using TotalCount: ${newTotalDisplayed} < ${data.TotalCount} = ${newHasMore}`
              );
            } else {
              // If no total count, assume we have more if we got a full page of results
              newHasMore = newProducts.length === pageSize;
              console.log(
                `📊 Using PageSize: ${newProducts.length} === ${pageSize} = ${newHasMore}`
              );
            }

            setHasMore(newHasMore);

            console.log(
              `✅ ${
                isLoadingMore ? "Loaded More" : "Loaded"
              } - Page: ${page}, Items: ${
                newProducts.length
              }, Total Displayed: ${newTotalDisplayed}${
                data.TotalCount ? ` of ${data.TotalCount}` : ""
              }, HasMore: ${newHasMore}`
            );
          } else {
            // Handle empty response
            if (!isLoadingMore) {
              setProducts([]);
              setTotalCount(0);
            }
            setHasMore(false);
          }
        } else {
          // Handle non-OK response
          const errorData = await response.json().catch(() => null);
          toast({
            title: "Error",
            description: errorData?.Message || "Failed to fetch products",
            variant: "destructive",
          });
          if (!isLoadingMore) {
            setProducts([]);
            setTotalCount(0);
          }
          setHasMore(false);
        }
      } catch (error) {
        console.error("❌ Error fetching products:", error);
        toast({
          title: "Error",
          description: `Failed to fetch products: ${
            error instanceof Error ? error.message : "Unknown error"
          }`,
          variant: "destructive",
        });
        if (!isLoadingMore) {
          setProducts([]);
          setTotalCount(0);
        }
        setHasMore(false);
      } finally {
        if (isLoadingMore) {
          setLoadingMore(false);
        } else {
          setLoading(false);
        }
      }
    },
    [debouncedSearchTerm, statusFilter, pageSize]
  );

  // Load more products when scrolling
  const loadMoreProducts = useCallback(async () => {
    if (!hasMore || loadingMore || loading) {
      console.log(
        `🚫 Skipping loadMore: hasMore=${hasMore}, loadingMore=${loadingMore}, loading=${loading}`
      );
      return;
    }

    console.log(
      `🔄 Loading more products - Page: ${currentPage + 1}, Current products: ${
        products.length
      }`
    );
    await fetchProductsPage(currentPage + 1, true);
    setCurrentPage((prev) => prev + 1);
  }, [
    hasMore,
    loadingMore,
    loading,
    currentPage,
    fetchProductsPage,
    products.length,
  ]);

  // Auto-load more when scrolling near bottom
  const handleScroll = useCallback(
    (event: React.UIEvent<HTMLDivElement>) => {
      const { scrollTop, scrollHeight, clientHeight } = event.currentTarget;
      const scrollPercentage = (scrollTop + clientHeight) / scrollHeight;

      // Debug scroll position (only log when near trigger point)
      if (scrollPercentage > 0.75) {
        console.log(
          `📜 Scroll: ${Math.round(
            scrollPercentage * 100
          )}%, hasMore=${hasMore}, loading=${loading || loadingMore}`
        );
      }

      // Load more when user scrolls to 80% of the content
      if (scrollPercentage > 0.8 && hasMore && !loadingMore && !loading) {
        console.log(
          `🎯 Triggering loadMore at ${Math.round(scrollPercentage * 100)}%`
        );
        loadMoreProducts();
      }
    },
    [hasMore, loadingMore, loading, loadMoreProducts]
  );

  // Reset and load initial data when search/filter changes
  useEffect(() => {
    setCurrentPage(1);
    fetchProductsPage(1, false);
  }, [debouncedSearchTerm, statusFilter]);

  // Reset scroll position when search changes
  useEffect(() => {
    if (scrollAreaRef.current) {
      scrollAreaRef.current.scrollTo({ top: 0 });
    }
  }, [debouncedSearchTerm, statusFilter]);

  const handleSearch = (value: string) => {
    setSearchTerm(value);
  };

  const handleProductSelect = (productUID: string, selected: boolean) => {
    const newSelected = new Set(selectedProducts);
    const newQuantities = { ...productQuantities };

    if (selected) {
      newSelected.add(productUID);
      if (!newQuantities[productUID]) {
        newQuantities[productUID] = 1;
      }
    } else {
      newSelected.delete(productUID);
      delete newQuantities[productUID];
    }

    setSelectedProducts(newSelected);
    setProductQuantities(newQuantities);

    // Notify parent of changes
    onProductSelectionChange?.(Array.from(newSelected), newQuantities);
  };

  const handleSelectAll = (checked: boolean) => {
    if (checked) {
      // Select all products on current page
      const newSelected = new Set(selectedProducts);
      const newQuantities = { ...productQuantities };

      products.forEach((product) => {
        const uid = product.SKUUID || product.UID;
        if (uid) {
          newSelected.add(uid);
          if (!newQuantities[uid]) {
            newQuantities[uid] = 1;
          }
        }
      });

      setSelectedProducts(newSelected);
      setProductQuantities(newQuantities);
      onProductSelectionChange?.(Array.from(newSelected), newQuantities);
    } else {
      // Deselect all products on current page
      const newSelected = new Set(selectedProducts);
      const newQuantities = { ...productQuantities };

      products.forEach((product) => {
        const uid = product.SKUUID || product.UID;
        if (uid) {
          newSelected.delete(uid);
          delete newQuantities[uid];
        }
      });

      setSelectedProducts(newSelected);
      setProductQuantities(newQuantities);
      onProductSelectionChange?.(Array.from(newSelected), newQuantities);
    }
  };

  const handleQuantityChange = (productUID: string, quantity: number) => {
    if (quantity <= 0) return;

    const newQuantities = { ...productQuantities, [productUID]: quantity };
    setProductQuantities(newQuantities);

    // Notify parent of changes
    onProductSelectionChange?.(Array.from(selectedProducts), newQuantities);
  };

  const isAllPageSelected = products.every((product) => {
    const uid = product.SKUUID || product.UID;
    return uid ? selectedProducts.has(uid) : false;
  });

  const isSomePageSelected = products.some((product) => {
    const uid = product.SKUUID || product.UID;
    return uid ? selectedProducts.has(uid) : false;
  });

  return (
    <div className="space-y-4">
      {/* Search and Filters */}
      <Card className="shadow-sm border-gray-200">
        <CardContent className="py-3 px-4">
          <div className="flex items-center gap-3">
            <div className="relative flex-1">
              <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 h-4 w-4" />
              <Input
                ref={searchInputRef}
                placeholder="Search by product name or code... (Ctrl+F)"
                value={searchTerm}
                onChange={(e) => handleSearch(e.target.value)}
                className="pl-10 h-9 border-gray-200 focus:border-primary focus:ring-1 focus:ring-primary/20"
              />
            </div>

            {/* Status Filter Dropdown */}
            <DropdownMenu>
              <DropdownMenuTrigger asChild>
                <Button variant="outline" className="h-9 px-3">
                  <Filter className="h-4 w-4 mr-2" />
                  Status
                  {statusFilter.length > 0 && (
                    <Badge
                      variant="secondary"
                      className="ml-2 px-1.5 py-0 text-xs"
                    >
                      {statusFilter.length}
                    </Badge>
                  )}
                  <ChevronDown className="h-4 w-4 ml-1" />
                </Button>
              </DropdownMenuTrigger>
              <DropdownMenuContent align="end" className="w-48">
                <DropdownMenuLabel>Filter by Status</DropdownMenuLabel>
                <DropdownMenuSeparator />
                <DropdownMenuCheckboxItem
                  checked={statusFilter.includes("Active")}
                  onCheckedChange={(checked) => {
                    setStatusFilter((prev) =>
                      checked
                        ? [...prev, "Active"]
                        : prev.filter((s) => s !== "Active")
                    );
                    setCurrentPage(1); // Reset to first page when filter changes
                  }}
                >
                  <div className="flex items-center gap-2">
                    <div className="w-2 h-2 bg-green-500 rounded-full" />
                    Active
                  </div>
                </DropdownMenuCheckboxItem>
                <DropdownMenuCheckboxItem
                  checked={statusFilter.includes("Inactive")}
                  onCheckedChange={(checked) => {
                    setStatusFilter((prev) =>
                      checked
                        ? [...prev, "Inactive"]
                        : prev.filter((s) => s !== "Inactive")
                    );
                    setCurrentPage(1); // Reset to first page when filter changes
                  }}
                >
                  <div className="flex items-center gap-2">
                    <div className="w-2 h-2 bg-red-500 rounded-full" />
                    Inactive
                  </div>
                </DropdownMenuCheckboxItem>
                {statusFilter.length > 0 && (
                  <>
                    <DropdownMenuSeparator />
                    <DropdownMenuItem
                      onClick={() => {
                        setStatusFilter([]);
                        setCurrentPage(1);
                      }}
                    >
                      <X className="h-4 w-4 mr-2" />
                      Clear Filter
                    </DropdownMenuItem>
                  </>
                )}
              </DropdownMenuContent>
            </DropdownMenu>

            {/* Selection summary */}
            {selectedProducts.size > 0 && (
              <div className="text-sm text-gray-600">
                <Badge variant="default" className="bg-blue-100 text-blue-800">
                  {selectedProducts.size} selected
                </Badge>
              </div>
            )}
          </div>
        </CardContent>
      </Card>

      {/* Products Table with Infinite Scroll */}
      <Card className="shadow-sm">
        <div className="overflow-hidden rounded-lg">
          <div className="border-b bg-gray-50/50 px-6 py-3 flex items-center justify-between">
            <div className="flex items-center gap-4">
              <div className="flex items-center gap-2">
                <Checkbox
                  checked={products.length > 0 && isAllPageSelected}
                  onCheckedChange={handleSelectAll}
                  ref={(el) => {
                    if (el) {
                      el.indeterminate =
                        isSomePageSelected && !isAllPageSelected;
                    }
                  }}
                />
                <span className="text-sm font-medium">Select All</span>
              </div>
              <div className="text-sm text-gray-600">
                {loading ? (
                  <span>Loading products...</span>
                ) : (
                  <span>
                    Showing {products.length}
                    {totalCount > 0 &&
                      ` of ${totalCount.toLocaleString()}`}{" "}
                    products
                    {hasMore && " (scroll for more)"}
                  </span>
                )}
              </div>
            </div>
          </div>

          <ScrollArea
            className="h-[600px]"
            ref={scrollAreaRef}
            onScrollCapture={handleScroll}
          >
            {loading && products.length === 0 ? (
              // Initial loading skeletons
              <div className="p-4 space-y-3">
                {[...Array(10)].map((_, index) => (
                  <div
                    key={`skeleton-${index}`}
                    className="flex items-center gap-4 p-3 border rounded-lg"
                  >
                    <Skeleton className="h-4 w-4" />
                    <Skeleton className="h-4 w-24" />
                    <Skeleton className="h-4 w-48 flex-1" />
                    <Skeleton className="h-5 w-16 rounded-full" />
                    <Skeleton className="h-8 w-20" />
                  </div>
                ))}
              </div>
            ) : products.length === 0 ? (
              <div className="flex items-center justify-center py-16">
                <div className="text-center">
                  <div className="text-gray-400 text-lg mb-2">
                    No products found
                  </div>
                  <div className="text-gray-500 text-sm">
                    {debouncedSearchTerm || statusFilter.length > 0
                      ? "Try adjusting your search or filters"
                      : "No products available"}
                  </div>
                </div>
              </div>
            ) : (
              <div className="divide-y divide-gray-200">
                {products.map((product, index) => {
                  const productUID = product.SKUUID || product.UID;
                  const isSelected = productUID
                    ? selectedProducts.has(productUID)
                    : false;
                  const quantity = productUID
                    ? productQuantities[productUID] || 1
                    : 1;

                  return (
                    <div
                      key={productUID || `product-${index}`}
                      className="flex items-center gap-4 p-4 hover:bg-gray-50/50 transition-colors"
                    >
                      <Checkbox
                        checked={isSelected}
                        onCheckedChange={(checked) => {
                          if (productUID) {
                            handleProductSelect(productUID, checked as boolean);
                          }
                        }}
                      />

                      <div className="min-w-[120px]">
                        <span className="text-sm font-medium">
                          {product.SKUCode || product.Code}
                        </span>
                      </div>

                      <div className="flex-1 min-w-0">
                        <span className="text-sm text-gray-900 truncate">
                          {product.SKULongName ||
                            product.LongName ||
                            product.Name}
                        </span>
                      </div>

                      <div className="min-w-[80px]">
                        <Badge
                          variant={product.IsActive ? "default" : "secondary"}
                          className={
                            product.IsActive
                              ? "bg-green-100 text-green-800 hover:bg-green-100 text-xs"
                              : "bg-gray-100 text-gray-600 hover:bg-gray-100 text-xs"
                          }
                        >
                          {product.IsActive ? "Active" : "Inactive"}
                        </Badge>
                      </div>

                      <div className="min-w-[140px] flex justify-center">
                        {isSelected ? (
                          <div className="flex items-center gap-1">
                            <Button
                              variant="outline"
                              size="sm"
                              className="h-8 w-8 p-0"
                              onClick={() => {
                                if (productUID && quantity > 1) {
                                  handleQuantityChange(
                                    productUID,
                                    quantity - 1
                                  );
                                }
                              }}
                              disabled={quantity <= 1}
                            >
                              <Minus className="h-3 w-3" />
                            </Button>
                            <span className="min-w-[2rem] text-center text-sm font-medium">
                              {quantity}
                            </span>
                            <Button
                              variant="outline"
                              size="sm"
                              className="h-8 w-8 p-0"
                              onClick={() => {
                                if (productUID) {
                                  handleQuantityChange(
                                    productUID,
                                    quantity + 1
                                  );
                                }
                              }}
                            >
                              <Plus className="h-3 w-3" />
                            </Button>
                          </div>
                        ) : (
                          <span className="text-gray-400 text-sm">-</span>
                        )}
                      </div>
                    </div>
                  );
                })}

                {/* Loading more indicator */}
                {loadingMore && (
                  <div className="p-4 flex items-center justify-center">
                    <div className="flex items-center gap-2 text-gray-600">
                      <Loader2 className="h-4 w-4 animate-spin" />
                      <span className="text-sm">Loading more products...</span>
                    </div>
                  </div>
                )}

                {/* End of data indicator */}
                {!hasMore && products.length > 0 && (
                  <div className="p-4 text-center">
                    <span className="text-sm text-gray-500">
                      No more products to load ({products.length} total)
                    </span>
                  </div>
                )}
              </div>
            )}
          </ScrollArea>
        </div>
      </Card>
    </div>
  );
}
