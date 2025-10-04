"use client";

import React, { useState, useEffect } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import {
  Edit2,
  Trash2,
  Search,
  Plus,
} from "lucide-react";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Badge } from "@/components/ui/badge";
import { useToast } from "@/components/ui/use-toast";
import { skuPriceService, ISKUPrice } from "@/services/sku/sku-price.service";
import { PagingRequest } from "@/types/common.types";
import { PaginationControls } from "@/components/ui/pagination-controls";
import { formatDateToDayMonthYear } from "@/utils/date-formatter";
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from "@/components/ui/alert-dialog";

interface PriceRow extends ISKUPrice {
  isEditing?: boolean;
  // Temporary fields for editing
  TempPrice?: number;
  TempDefaultRetPrice?: number;
  TempValidFrom?: Date | string;
  TempValidUpto?: Date | string;
}

export default function TPPPriceTab() {
  const { toast } = useToast();
  
  // Simple state - just show individual price records
  const [prices, setPrices] = useState<PriceRow[]>([]);
  const [loading, setLoading] = useState(false);
  const [searchTerm, setSearchTerm] = useState("");
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [totalCount, setTotalCount] = useState(0);
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  const [priceToDelete, setPriceToDelete] = useState<PriceRow | null>(null);
  const [editingRows, setEditingRows] = useState<Set<string>>(new Set());

  // Simple fetch - get individual price records with server-side pagination
  const fetchPrices = async () => {
    setLoading(true);
    try {
      const request: PagingRequest = {
        PageNumber: currentPage,
        PageSize: pageSize,
        FilterCriterias: searchTerm
          ? [{ Name: "SKUCode", Value: searchTerm }]
          : [],
        SortCriterias: [{ SortParameter: "SKUCode", Direction: "Asc" }],
        IsCountRequired: true,
      };
      
      const response = await skuPriceService.getAllSKUPrices(request);
      setPrices(response.PagedData || []);
      setTotalCount(response.TotalCount || 0);
      
    } catch (error) {
      toast({
        title: "Error",
        description: "Failed to fetch prices",
        variant: "destructive",
      });
      console.error("Error fetching prices:", error);
    } finally {
      setLoading(false);
    }
  };

  // Fetch prices when page, search, or page size changes
  useEffect(() => {
    fetchPrices();
  }, [currentPage, pageSize]);

  // Handle search with debounce
  useEffect(() => {
    const debounceTimer = setTimeout(() => {
      setCurrentPage(1); // Reset to first page on search
      fetchPrices();
    }, searchTerm ? 500 : 0);
    
    return () => clearTimeout(debounceTimer);
  }, [searchTerm]);

  const handleSearch = (value: string) => {
    setSearchTerm(value);
  };

  // Start editing a price record
  const startEditing = (priceUID: string) => {
    setEditingRows(prev => new Set([...prev, priceUID]));
  };

  // Cancel editing
  const cancelEditing = (priceUID: string) => {
    setEditingRows(prev => {
      const newSet = new Set(prev);
      newSet.delete(priceUID);
      return newSet;
    });
    // Reset temp values
    setPrices(prev => prev.map(p => 
      p.UID === priceUID 
        ? { ...p, TempPrice: undefined, TempDefaultRetPrice: undefined, TempValidFrom: undefined, TempValidUpto: undefined }
        : p
    ));
  };

  // Save edited price
  const saveEdit = async (price: PriceRow) => {
    try {
      const updatedPrice: ISKUPrice = {
        ...price,
        Price: price.TempPrice ?? price.Price,
        DefaultRetPrice: price.TempDefaultRetPrice ?? price.DefaultRetPrice,
        ValidFrom: price.TempValidFrom ?? price.ValidFrom,
        ValidUpto: price.TempValidUpto ?? price.ValidUpto,
      };
      
      await skuPriceService.updateSKUPrice(updatedPrice);
      toast({
        title: "Success",
        description: "Price updated successfully",
      });
      setEditingRows(prev => {
        const newSet = new Set(prev);
        newSet.delete(price.UID!);
        return newSet;
      });
      fetchPrices(); // Refresh data
    } catch (error) {
      toast({
        title: "Error",
        description: "Failed to update price",
        variant: "destructive",
      });
    }
  };

  // Delete price
  const confirmDelete = (price: PriceRow) => {
    setPriceToDelete(price);
    setDeleteDialogOpen(true);
  };

  const handleDelete = async () => {
    if (!priceToDelete) return;

    try {
      await skuPriceService.deleteSKUPrice(priceToDelete.UID!);

      toast({
        title: "Success",
        description: "Price deleted successfully",
      });

      // Refresh data
      fetchPrices();
      
    } catch (error) {
      toast({
        title: "Error",
        description: "Failed to delete price",
        variant: "destructive",
      });
    } finally {
      setDeleteDialogOpen(false);
      setPriceToDelete(null);
    }
  };

  const formatCurrency = (amount: number) => {
    return new Intl.NumberFormat("en-US", {
      minimumFractionDigits: 2,
      maximumFractionDigits: 2,
    }).format(amount || 0);
  };

  const formatDate = (date: Date | string | undefined) => {
    if (!date) return "";
    return new Date(date).toISOString().split('T')[0];
  };

  return (
    <div className="space-y-4">
      <div className="flex justify-between items-center mb-4">
        <h3 className="text-lg font-semibold">SKU Price Records</h3>
        <div className="flex gap-2">
          <div className="relative">
            <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 h-4 w-4" />
            <Input
              placeholder="Search by SKU code..."
              value={searchTerm}
              onChange={(e) => handleSearch(e.target.value)}
              className="pl-10 w-64"
            />
          </div>
          <Button>
            <Plus className="h-4 w-4 mr-2" />
            Add New Price
          </Button>
        </div>
      </div>

      <div className="rounded-md border">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>SKU Code</TableHead>
              <TableHead>SKU Name</TableHead>
              <TableHead>Price List</TableHead>
              <TableHead>Cost Price</TableHead>
              <TableHead>Retail Price</TableHead>
              <TableHead>Valid From</TableHead>
              <TableHead>Valid To</TableHead>
              <TableHead className="text-right">Actions</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {loading ? (
              <TableRow>
                <TableCell colSpan={8} className="text-center py-8">
                  <div className="flex justify-center">
                    <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary"></div>
                  </div>
                </TableCell>
              </TableRow>
            ) : prices.length === 0 ? (
              <TableRow>
                <TableCell colSpan={8} className="text-center py-8">
                  No prices found
                </TableCell>
              </TableRow>
            ) : (
              prices.map((price) => (
                <TableRow
                  key={price.UID}
                  className={editingRows.has(price.UID!) ? "bg-blue-50" : ""}
                >
                  <TableCell className="font-medium">{price.SKUCode}</TableCell>
                  <TableCell>{price.SKUName || "-"}</TableCell>
                  <TableCell>
                    <Badge variant="outline">{price.SKUPriceListUID || "Default"}</Badge>
                  </TableCell>
                  
                  {/* Cost Price */}
                  <TableCell>
                    {editingRows.has(price.UID!) ? (
                      <Input
                        type="number"
                        step="0.01"
                        value={price.TempPrice ?? price.Price}
                        onChange={(e) => {
                          setPrices(prev => prev.map(p => 
                            p.UID === price.UID 
                              ? {...p, TempPrice: parseFloat(e.target.value) || 0}
                              : p
                          ));
                        }}
                        className="w-24"
                      />
                    ) : (
                      formatCurrency(price.Price)
                    )}
                  </TableCell>

                  {/* Retail Price */}
                  <TableCell>
                    {editingRows.has(price.UID!) ? (
                      <Input
                        type="number"
                        step="0.01"
                        value={price.TempDefaultRetPrice ?? price.DefaultRetPrice}
                        onChange={(e) => {
                          setPrices(prev => prev.map(p => 
                            p.UID === price.UID 
                              ? {...p, TempDefaultRetPrice: parseFloat(e.target.value) || 0}
                              : p
                          ));
                        }}
                        className="w-24"
                      />
                    ) : (
                      formatCurrency(price.DefaultRetPrice || 0)
                    )}
                  </TableCell>

                  {/* Valid From */}
                  <TableCell>
                    {editingRows.has(price.UID!) ? (
                      <Input
                        type="date"
                        value={price.TempValidFrom ? formatDate(price.TempValidFrom) : formatDate(price.ValidFrom)}
                        onChange={(e) => {
                          setPrices(prev => prev.map(p => 
                            p.UID === price.UID 
                              ? {...p, TempValidFrom: e.target.value}
                              : p
                          ));
                        }}
                        className="w-36"
                      />
                    ) : (
                      formatDateToDayMonthYear(price.ValidFrom, "-")
                    )}
                  </TableCell>

                  {/* Valid To */}
                  <TableCell>
                    {editingRows.has(price.UID!) ? (
                      <Input
                        type="date"
                        value={price.TempValidUpto ? formatDate(price.TempValidUpto) : formatDate(price.ValidUpto)}
                        onChange={(e) => {
                          setPrices(prev => prev.map(p => 
                            p.UID === price.UID 
                              ? {...p, TempValidUpto: e.target.value}
                              : p
                          ));
                        }}
                        className="w-36"
                      />
                    ) : (
                      formatDateToDayMonthYear(price.ValidUpto, "-")
                    )}
                  </TableCell>

                  {/* Actions */}
                  <TableCell className="text-right">
                    <div className="flex justify-end gap-2">
                      {editingRows.has(price.UID!) ? (
                        <>
                          <Button
                            variant="outline"
                            size="sm"
                            onClick={() => saveEdit(price)}
                          >
                            Save
                          </Button>
                          <Button
                            variant="outline"
                            size="sm"
                            onClick={() => cancelEditing(price.UID!)}
                          >
                            Cancel
                          </Button>
                        </>
                      ) : (
                        <>
                          <Button
                            variant="ghost"
                            size="icon"
                            className="h-8 w-8"
                            onClick={() => startEditing(price.UID!)}
                          >
                            <Edit2 className="h-4 w-4" />
                          </Button>
                          <Button
                            variant="ghost"
                            size="icon"
                            className="h-8 w-8 text-red-600"
                            onClick={() => confirmDelete(price)}
                          >
                            <Trash2 className="h-4 w-4" />
                          </Button>
                        </>
                      )}
                    </div>
                  </TableCell>
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>
      </div>

      {totalCount > 0 && (
        <PaginationControls
          currentPage={currentPage}
          totalCount={totalCount}
          pageSize={pageSize}
          onPageChange={setCurrentPage}
          onPageSizeChange={setPageSize}
          itemName="price records"
        />
      )}

      <AlertDialog open={deleteDialogOpen} onOpenChange={setDeleteDialogOpen}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Are you sure?</AlertDialogTitle>
            <AlertDialogDescription>
              This action cannot be undone. This will permanently delete the price record
              {priceToDelete && (
                <span className="font-semibold">
                  {" "}for SKU {priceToDelete.SKUCode} (Valid from{" "}
                  {formatDateToDayMonthYear(priceToDelete.ValidFrom, "-")} to{" "}
                  {formatDateToDayMonthYear(priceToDelete.ValidUpto, "-")})
                </span>
              )}
              .
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel onClick={() => setPriceToDelete(null)}>
              Cancel
            </AlertDialogCancel>
            <AlertDialogAction
              onClick={handleDelete}
              className="bg-red-600 hover:bg-red-700"
            >
              Delete
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  );
}