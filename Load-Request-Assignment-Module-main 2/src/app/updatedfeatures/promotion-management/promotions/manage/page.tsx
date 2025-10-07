"use client";

import React, { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import {
  Plus,
  Pencil,
  Trash2,
  Search,
  Eye,
  Calendar,
  Tag,
  CheckCircle,
  XCircle,
  FileText,
  MoreHorizontal,
  Clock,
  TrendingUp,
} from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { Badge } from "@/components/ui/badge";
import { Card, CardHeader, CardTitle, CardContent } from "@/components/ui/card";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Checkbox } from "@/components/ui/checkbox";
import { Switch } from "@/components/ui/switch";
import { promotionService } from "../../services/promotion.service";
import { IPromotionView } from "../../types/promotion.types";
import { toast } from "sonner";
import {
  getPromotionDisplayType,
  getPromotionDescription,
  getPromotionStatus,
} from "../../utils/promotionDisplayUtils";

type PromotionListItem = IPromotionView;

interface Filters {
  search: string;
  status: "all" | "active" | "inactive" | "scheduled" | "expired";
  type: string;
  dateRange: {
    start: string | null;
    end: string | null;
  };
}

export default function PromotionManagePage() {
  const router = useRouter();
  const [promotions, setPromotions] = useState<PromotionListItem[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [deletingIds, setDeletingIds] = useState<Set<string>>(new Set());
  const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set());
  const [togglingIds, setTogglingIds] = useState<Set<string>>(new Set());

  // Filters and pagination
  const [filters, setFilters] = useState<Filters>({
    search: "",
    status: "all",
    type: "all",
    dateRange: {
      start: null,
      end: null,
    },
  });

  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);

  // Load promotions - optimized to fetch less data initially
  useEffect(() => {
    loadPromotions();
  }, []);

  const loadPromotions = async () => {
    try {
      setLoading(true);
      setError(null);

      // Reduced initial load from 1000 to 50 for better performance
      const response = await promotionService.getPromotionDetails(1, 50);
      
      console.log('Raw API Response:', response);
      console.log('Response data:', response.data);
      console.log('Response data type:', typeof response.data);
      console.log('Response data keys:', response.data ? Object.keys(response.data) : 'null');

      if (response.success && response.data) {
        let promotionData: IPromotionView[] = [];

        // The API response transformer handles case sensitivity automatically
        // Check for nested Data.PagedData structure (backend wraps response)
        if (response.data.Data?.PagedData && Array.isArray(response.data.Data.PagedData)) {
          console.log('Found Data.PagedData with', response.data.Data.PagedData.length, 'items');
          promotionData = response.data.Data.PagedData;
        } else if (response.data.PagedData && Array.isArray(response.data.PagedData)) {
          console.log('Found PagedData with', response.data.PagedData.length, 'items');
          promotionData = response.data.PagedData;
        } else if (response.data.Data && Array.isArray(response.data.Data)) {
          console.log('Found Data with', response.data.Data.length, 'items');
          promotionData = response.data.Data;
        } else if (response.data.items && Array.isArray(response.data.items)) {
          console.log('Found items with', response.data.items.length, 'items');
          promotionData = response.data.items;
        } else if (Array.isArray(response.data)) {
          console.log('Response data is array with', response.data.length, 'items');
          promotionData = response.data;
        } else {
          console.log('No recognized data structure found in response');
          console.log('Available keys in response.data:', Object.keys(response.data));
          if (response.data.Data) {
            console.log('Available keys in response.data.Data:', Object.keys(response.data.Data));
          }
        }

        // Log first promotion to see field names
        if (promotionData.length > 0) {
          console.log('First promotion data structure:', promotionData[0]);
          console.log('IsActive field:', promotionData[0].IsActive);
          console.log('isActive field:', promotionData[0].isActive);
        }
        console.log('Setting promotions:', promotionData);
        setPromotions(promotionData);
      }
    } catch (err: any) {
      setError(err.message || "Failed to load promotions");
    } finally {
      setLoading(false);
    }
  };

  const handleViewDetails = (promotionId: string) => {
    if (!promotionId) {
      console.error('Invalid promotion ID for view details');
      return;
    }
    router.push(`/updatedfeatures/promotion-management/promotions/${promotionId}`);
  };

  const handleEdit = (promotionId: string) => {
    if (!promotionId) {
      console.error('Invalid promotion ID for edit');
      return;
    }
    router.push(`/updatedfeatures/promotion-management/promotions/manage?edit=${promotionId}`);
  };

  const handleDelete = async (promotionId: string) => {
    if (!promotionId) {
      console.error('Invalid promotion ID for delete');
      return;
    }
    if (window.confirm("Are you sure you want to delete this promotion?")) {
      setDeletingIds(prev => new Set([...prev, promotionId]));
      
      try {
        const response = await promotionService.deletePromotion(promotionId);
        if (response.success) {
          await loadPromotions(); // Reload the list
        } else {
          throw new Error(response.error || "Failed to delete promotion");
        }
      } catch (err: any) {
        alert(`Error deleting promotion: ${err.message}`);
      } finally {
        setDeletingIds(prev => {
          const newSet = new Set([...prev]);
          newSet.delete(promotionId);
          return newSet;
        });
      }
    }
  };

  const handleToggleStatus = async (promotionId: string, currentStatus: boolean) => {
    if (!promotionId) {
      console.error('Invalid promotion ID for toggle');
      return;
    }
    
    console.log(`[DEBUG] Toggling promotion ${promotionId} - Current status: ${currentStatus}, Action: ${currentStatus ? 'Deactivating' : 'Activating'}`);
    
    setTogglingIds(prev => new Set([...prev, promotionId]));
    
    try {
      const response = currentStatus 
        ? await promotionService.deactivatePromotion(promotionId)
        : await promotionService.activatePromotion(promotionId);
        
      if (response.IsSuccess) {
        console.log('[DEBUG] Toggle response:', response);
        
        // If the backend returned the updated promotion, use it
        if (response.Data?.Promotion) {
          const updatedPromotion = response.Data.Promotion;
          console.log('[DEBUG] Updated promotion from backend:', updatedPromotion);
          console.log('[DEBUG] IsActive value:', updatedPromotion.IsActive, 'isActive value:', updatedPromotion.isActive);
          
          setPromotions(prev => prev.map(p => 
            p.UID === promotionId 
              ? { 
                  ...p, 
                  ...updatedPromotion,
                  // Ensure both field name cases are updated
                  IsActive: updatedPromotion.IsActive ?? updatedPromotion.isActive ?? !currentStatus,
                  isActive: updatedPromotion.IsActive ?? updatedPromotion.isActive ?? !currentStatus
                }
              : p
          ));
        } else {
          console.log('[DEBUG] No promotion data in response, using fallback');
          // Fallback: Update local state immediately for better UX
          setPromotions(prev => prev.map(p => 
            p.UID === promotionId 
              ? { 
                  ...p, 
                  IsActive: !currentStatus,
                  isActive: !currentStatus  // Handle both field name cases
                }
              : p
          ));
        }
        
        toast.success(
          currentStatus 
            ? "Promotion deactivated successfully" 
            : "Promotion activated successfully"
        );
        
        // Clear cache to ensure fresh data
        promotionService.clearCache();
        
        // Reload to get fresh data from server immediately
        await loadPromotions();
      } else {
        throw new Error(response.ErrorMessage || "Failed to update promotion status");
      }
    } catch (err: any) {
      toast.error(`Error updating promotion status: ${err.message}`);
      console.error('Toggle status error:', err);
    } finally {
      setTogglingIds(prev => {
        const newSet = new Set([...prev]);
        newSet.delete(promotionId);
        return newSet;
      });
    }
  };

  const handleCreateNew = () => {
    router.push("/updatedfeatures/promotion-management/promotions/create");
  };

  const handleBulkDelete = async () => {
    if (selectedIds.size === 0) return;
    
    if (window.confirm(`Are you sure you want to delete ${selectedIds.size} promotions?`)) {
      for (const id of selectedIds) {
        setDeletingIds(prev => new Set([...prev, id]));
        try {
          await promotionService.deletePromotion(id);
        } catch (err) {
          console.error(`Failed to delete promotion ${id}:`, err);
        }
      }
      setSelectedIds(new Set());
      await loadPromotions();
      setDeletingIds(new Set());
    }
  };

  // Filter promotions based on current filters
  const filteredPromotions = promotions.filter(promotion => {
    // Search filter
    if (filters.search) {
      const search = filters.search.toLowerCase();
      const matches = 
        promotion.Name?.toLowerCase().includes(search) ||
        promotion.Code?.toLowerCase().includes(search) ||
        promotion.Description?.toLowerCase().includes(search);
      if (!matches) return false;
    }

    // Status filter
    if (filters.status !== "all") {
      const status = getPromotionStatus(promotion);
      if (status.status !== filters.status) return false;
    }

    return true;
  });

  // Paginate results
  const startIndex = (currentPage - 1) * pageSize;
  const paginatedPromotions = filteredPromotions.slice(startIndex, startIndex + pageSize);
  const totalPages = Math.ceil(filteredPromotions.length / pageSize);

  const formatDate = (dateString: string) => {
    if (!dateString) return "N/A";
    try {
      return new Date(dateString).toLocaleDateString();
    } catch {
      return "Invalid Date";
    }
  };

  const handleSelectAll = (checked: boolean) => {
    if (checked) {
      setSelectedIds(new Set(paginatedPromotions.map(p => p.UID || p.uid || p.code)));
    } else {
      setSelectedIds(new Set());
    }
  };

  const handleSelectItem = (promotionId: string, checked: boolean) => {
    const newSelected = new Set(selectedIds);
    if (checked) {
      newSelected.add(promotionId);
    } else {
      newSelected.delete(promotionId);
    }
    setSelectedIds(newSelected);
  };

  const getStatusBadge = (promotion: IPromotionView) => {
    const status = getPromotionStatus(promotion);
    const variants: Record<string, "default" | "secondary" | "destructive" | "outline"> = {
      green: "default",
      blue: "secondary", 
      red: "destructive",
      gray: "outline"
    };
    
    const isActive = Boolean(promotion.IsActive || promotion.isActive);
    
    return (
      <div className="flex items-center gap-2">
        <Badge variant={variants[status.color]}>
          {status.label}
        </Badge>
        <div className={`flex items-center gap-1 text-xs ${isActive ? 'text-green-600' : 'text-gray-500'}`}>
          <div className={`w-2 h-2 rounded-full ${isActive ? 'bg-green-500' : 'bg-gray-400'}`}></div>
          <span className="font-medium">{isActive ? 'Active' : 'Inactive'}</span>
        </div>
      </div>
    );
  };

  const TableRowSkeleton = () => (
    <TableRow>
      <TableCell><div className="w-4 h-4 bg-gray-200 rounded animate-pulse"></div></TableCell>
      <TableCell>
        <div className="space-y-2">
          <div className="h-4 bg-gray-200 rounded animate-pulse w-32"></div>
          <div className="h-3 bg-gray-200 rounded animate-pulse w-48"></div>
        </div>
      </TableCell>
      <TableCell><div className="h-4 bg-gray-200 rounded animate-pulse w-20"></div></TableCell>
      <TableCell><div className="h-4 bg-gray-200 rounded animate-pulse w-24"></div></TableCell>
      <TableCell><div className="w-16 h-5 bg-gray-200 rounded animate-pulse"></div></TableCell>
      <TableCell><div className="w-12 h-6 bg-gray-200 rounded-full animate-pulse"></div></TableCell>
      <TableCell><div className="h-3 bg-gray-200 rounded animate-pulse w-20"></div></TableCell>
      <TableCell><div className="h-3 bg-gray-200 rounded animate-pulse w-20"></div></TableCell>
      <TableCell><div className="w-12 h-5 bg-gray-200 rounded animate-pulse"></div></TableCell>
      <TableCell><div className="w-8 h-8 bg-gray-200 rounded animate-pulse"></div></TableCell>
    </TableRow>
  );

  if (loading && promotions.length === 0) {
    return (
      <div className="container mx-auto py-6 space-y-6">
        {/* Header Skeleton */}
        <div className="flex justify-between items-center">
          <div className="space-y-2">
            <div className="h-8 bg-gray-200 rounded animate-pulse w-64"></div>
            <div className="h-4 bg-gray-200 rounded animate-pulse w-80"></div>
          </div>
          <div className="w-48 h-10 bg-gray-200 rounded animate-pulse"></div>
        </div>

        {/* Stats Cards Skeleton */}
        <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
          {[1,2,3,4].map(i => (
            <Card key={i}>
              <CardHeader className="pb-2">
                <div className="h-4 bg-gray-200 rounded animate-pulse w-24"></div>
              </CardHeader>
              <CardContent>
                <div className="h-8 bg-gray-200 rounded animate-pulse w-16"></div>
              </CardContent>
            </Card>
          ))}
        </div>

        {/* Filters Skeleton */}
        <Card>
          <CardContent className="py-4">
            <div className="flex flex-col md:flex-row gap-4 items-center">
              <div className="w-full h-10 bg-gray-200 rounded animate-pulse"></div>
              <div className="w-40 h-10 bg-gray-200 rounded animate-pulse"></div>
            </div>
          </CardContent>
        </Card>

        {/* Table Skeleton */}
        <Card>
          <CardContent className="p-0">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead className="w-12"><div className="w-4 h-4 bg-gray-200 rounded animate-pulse"></div></TableHead>
                  <TableHead><div className="h-4 bg-gray-200 rounded animate-pulse w-16"></div></TableHead>
                  <TableHead><div className="h-4 bg-gray-200 rounded animate-pulse w-12"></div></TableHead>
                  <TableHead><div className="h-4 bg-gray-200 rounded animate-pulse w-12"></div></TableHead>
                  <TableHead><div className="h-4 bg-gray-200 rounded animate-pulse w-16"></div></TableHead>
                  <TableHead><div className="h-4 bg-gray-200 rounded animate-pulse w-12"></div></TableHead>
                  <TableHead><div className="h-4 bg-gray-200 rounded animate-pulse w-20"></div></TableHead>
                  <TableHead><div className="h-4 bg-gray-200 rounded animate-pulse w-20"></div></TableHead>
                  <TableHead><div className="h-4 bg-gray-200 rounded animate-pulse w-16"></div></TableHead>
                  <TableHead><div className="h-4 bg-gray-200 rounded animate-pulse w-16"></div></TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {[1,2,3,4,5,6,7,8].map(i => <TableRowSkeleton key={i} />)}
              </TableBody>
            </Table>
          </CardContent>
        </Card>
      </div>
    );
  }

  return (
    <div className="container mx-auto py-6 space-y-6">
      {/* Header */}
      <div className="bg-white rounded-lg border border-gray-200 p-6 mb-6">
        <div className="flex justify-between items-center">
          <div>
            <div className="flex items-center mb-3">
              <div className="p-2 bg-blue-50 rounded-lg mr-3">
                <Tag className="h-6 w-6 text-blue-600" />
              </div>
              <div>
                <h1 className="text-2xl font-semibold text-gray-900">Promotion Management</h1>
                <p className="text-sm text-gray-500 mt-1">
                  Manage all your promotional campaigns and marketing initiatives
                </p>
              </div>
            </div>
          </div>
          <Button 
            onClick={handleCreateNew}
            className="bg-blue-600 hover:bg-blue-700 text-white"
          >
            <Plus className="h-4 w-4 mr-2" />
            Create New Promotion
          </Button>
        </div>
      </div>

      {/* Stats Cards */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4 mb-6">
        <Card className="border-gray-200 hover:border-gray-300 transition-colors">
          <CardContent className="p-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm font-medium text-gray-500 mb-1">Total Promotions</p>
                <div className="text-3xl font-bold text-gray-900">{promotions.length}</div>
              </div>
              <div className="p-3 bg-gray-50 rounded-lg">
                <Tag className="h-6 w-6 text-gray-600" />
              </div>
            </div>
          </CardContent>
        </Card>

        <Card className="border-gray-200 hover:border-green-300 transition-colors">
          <CardContent className="p-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm font-medium text-gray-500 mb-1">Active</p>
                <div className="text-3xl font-bold text-green-600">
                  {promotions.filter(p => getPromotionStatus(p).status === 'active').length}
                </div>
              </div>
              <div className="p-3 bg-green-50 rounded-lg">
                <CheckCircle className="h-6 w-6 text-green-600" />
              </div>
            </div>
            <div className="mt-2">
              <div className="flex items-center text-xs text-gray-500">
                <TrendingUp className="h-3 w-3 mr-1 text-green-500" />
                {promotions.length > 0 
                  ? `${Math.round((promotions.filter(p => getPromotionStatus(p).status === 'active').length / promotions.length) * 100)}% of total`
                  : '0% of total'
                }
              </div>
            </div>
          </CardContent>
        </Card>

        <Card className="border-gray-200 hover:border-blue-300 transition-colors">
          <CardContent className="p-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm font-medium text-gray-500 mb-1">Scheduled</p>
                <div className="text-3xl font-bold text-blue-600">
                  {promotions.filter(p => getPromotionStatus(p).status === 'scheduled').length}
                </div>
              </div>
              <div className="p-3 bg-blue-50 rounded-lg">
                <Clock className="h-6 w-6 text-blue-600" />
              </div>
            </div>
            <div className="mt-2">
              <div className="flex items-center text-xs text-gray-500">
                <Calendar className="h-3 w-3 mr-1 text-blue-500" />
                Ready to launch
              </div>
            </div>
          </CardContent>
        </Card>

        <Card className="border-gray-200 hover:border-gray-300 transition-colors">
          <CardContent className="p-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm font-medium text-gray-500 mb-1">Expired</p>
                <div className="text-3xl font-bold text-gray-600">
                  {promotions.filter(p => getPromotionStatus(p).status === 'expired').length}
                </div>
              </div>
              <div className="p-3 bg-gray-50 rounded-lg">
                <XCircle className="h-6 w-6 text-gray-600" />
              </div>
            </div>
            <div className="mt-2">
              <div className="flex items-center text-xs text-gray-500">
                <Calendar className="h-3 w-3 mr-1 text-gray-400" />
                Past campaigns
              </div>
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Filters */}
      <Card className="border-gray-200 mb-6">
        <CardContent className="p-6">
          <div className="flex flex-col md:flex-row gap-4 items-center">
            <div className="relative flex-1">
              <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 h-5 w-5" />
              <Input
                placeholder="Search by name, code, or description..."
                value={filters.search}
                onChange={(e) => setFilters({ ...filters, search: e.target.value })}
                className="pl-10 h-11 border-gray-300 focus:border-blue-500 focus:ring-blue-500"
              />
            </div>
            <Select
              value={filters.status}
              onValueChange={(value) => setFilters({ ...filters, status: value as any })}
            >
              <SelectTrigger className="w-40 h-11 border-gray-300">
                <SelectValue placeholder="Filter by status" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All Status</SelectItem>
                <SelectItem value="active">
                  <div className="flex items-center">
                    <div className="w-2 h-2 bg-green-500 rounded-full mr-2"></div>
                    Active
                  </div>
                </SelectItem>
                <SelectItem value="inactive">
                  <div className="flex items-center">
                    <div className="w-2 h-2 bg-gray-500 rounded-full mr-2"></div>
                    Inactive
                  </div>
                </SelectItem>
                <SelectItem value="scheduled">
                  <div className="flex items-center">
                    <div className="w-2 h-2 bg-blue-500 rounded-full mr-2"></div>
                    Scheduled
                  </div>
                </SelectItem>
                <SelectItem value="expired">
                  <div className="flex items-center">
                    <div className="w-2 h-2 bg-red-500 rounded-full mr-2"></div>
                    Expired
                  </div>
                </SelectItem>
              </SelectContent>
            </Select>
            {selectedIds.size > 0 && (
              <Button 
                variant="destructive" 
                onClick={handleBulkDelete}
                className="h-11"
              >
                <Trash2 className="h-4 w-4 mr-2" />
                Delete Selected ({selectedIds.size})
              </Button>
            )}
          </div>
        </CardContent>
      </Card>

      {/* Promotions Table */}
      <Card className="border-gray-200 overflow-hidden">
        <CardContent className="p-0">
          <Table>
            <TableHeader className="bg-gray-50 border-b border-gray-200">
              <TableRow>
                <TableHead className="w-12 pl-6">
                  <Checkbox
                    checked={paginatedPromotions.length > 0 && selectedIds.size === paginatedPromotions.length}
                    onCheckedChange={handleSelectAll}
                    className="border-gray-300"
                  />
                </TableHead>
                <TableHead className="font-semibold text-gray-700">Name</TableHead>
                <TableHead className="font-semibold text-gray-700">Code</TableHead>
                <TableHead className="font-semibold text-gray-700">Type</TableHead>
                <TableHead className="font-semibold text-gray-700">Status</TableHead>
                <TableHead className="font-semibold text-gray-700">Valid From</TableHead>
                <TableHead className="font-semibold text-gray-700">Valid Until</TableHead>
                <TableHead className="font-semibold text-gray-700">Priority</TableHead>
                <TableHead className="text-right font-semibold text-gray-700 pr-6">Actions</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {paginatedPromotions.map((promotion, index) => (
                <TableRow 
                  key={promotion.UID || promotion.Code || `promotion-${index}`}
                  className={`hover:bg-gray-50 transition-colors ${index % 2 === 0 ? 'bg-white' : 'bg-gray-50/50'}`}
                >
                  <TableCell className="pl-6">
                    <Checkbox
                      checked={selectedIds.has(promotion.UID)}
                      onCheckedChange={(checked) => handleSelectItem(promotion.UID, checked as boolean)}
                      className="border-gray-300"
                    />
                  </TableCell>
                  <TableCell className="font-medium">
                    <div>
                      <div className="font-semibold text-gray-900">{promotion.Name || promotion.Code}</div>
                      {(promotion.Description || promotion.Remarks) && (
                        <div className="text-sm text-gray-500 truncate max-w-xs mt-1">
                          {promotion.Description || promotion.Remarks}
                        </div>
                      )}
                    </div>
                  </TableCell>
                  <TableCell>
                    <code className="text-sm bg-blue-50 text-blue-700 px-2 py-1 rounded font-mono">
                      {promotion.Code || "N/A"}
                    </code>
                  </TableCell>
                  <TableCell>
                    <span className="text-sm font-medium text-gray-700">
                      {getPromotionDisplayType(promotion)}
                    </span>
                  </TableCell>
                  <TableCell>{getStatusBadge(promotion)}</TableCell>
                  <TableCell>
                    <div className="flex items-center text-sm text-gray-600">
                      <Calendar className="h-3 w-3 mr-1 text-gray-400" />
                      {formatDate(promotion.ValidFrom)}
                    </div>
                  </TableCell>
                  <TableCell>
                    <div className="flex items-center text-sm text-gray-600">
                      <Calendar className="h-3 w-3 mr-1 text-gray-400" />
                      {formatDate(promotion.ValidUpto)}
                    </div>
                  </TableCell>
                  <TableCell>
                    <Badge variant="outline" className="font-semibold">
                      {promotion.Priority || "N/A"}
                    </Badge>
                  </TableCell>
                  <TableCell className="text-right pr-6">
                    <DropdownMenu key={`${promotion.UID}-${promotion.IsActive}-${promotion.isActive}`}>
                      <DropdownMenuTrigger asChild>
                        <Button 
                          variant="ghost" 
                          className="h-8 w-8 p-0 hover:bg-gray-100"
                        >
                          <MoreHorizontal className="h-4 w-4 text-gray-600" />
                        </Button>
                      </DropdownMenuTrigger>
                      <DropdownMenuContent align="end" className="w-48">
                        <DropdownMenuItem 
                          onClick={() => handleViewDetails(promotion.UID)}
                          className="cursor-pointer"
                        >
                          <Eye className="mr-2 h-4 w-4 text-gray-500" />
                          View Details
                        </DropdownMenuItem>
                        <DropdownMenuItem 
                          onClick={() => handleEdit(promotion.UID)}
                          className="cursor-pointer"
                        >
                          <Pencil className="mr-2 h-4 w-4 text-gray-500" />
                          Edit Promotion
                        </DropdownMenuItem>
                        <DropdownMenuItem
                          onClick={() => handleToggleStatus(promotion.UID, Boolean(promotion.IsActive || promotion.isActive))}
                          className="cursor-pointer"
                          disabled={togglingIds.has(promotion.UID)}
                        >
                          {(promotion.IsActive || promotion.isActive) ? (
                            <>
                              <XCircle className="mr-2 h-4 w-4 text-orange-500" />
                              {togglingIds.has(promotion.UID) ? "Processing..." : "Deactivate"}
                            </>
                          ) : (
                            <>
                              <CheckCircle className="mr-2 h-4 w-4 text-green-500" />
                              {togglingIds.has(promotion.UID) ? "Processing..." : "Activate"}
                            </>
                          )}
                        </DropdownMenuItem>
                        <DropdownMenuItem
                          onClick={() => handleDelete(promotion.UID)}
                          className="text-red-600 cursor-pointer hover:bg-red-50"
                          disabled={deletingIds.has(promotion.UID)}
                        >
                          <Trash2 className="mr-2 h-4 w-4" />
                          {deletingIds.has(promotion.UID) ? "Deleting..." : "Delete Promotion"}
                        </DropdownMenuItem>
                      </DropdownMenuContent>
                    </DropdownMenu>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>

          {paginatedPromotions.length === 0 && !loading && (
            <div className="text-center py-12">
              <FileText className="mx-auto h-12 w-12 text-gray-400" />
              <h3 className="mt-2 text-sm font-medium text-gray-900">No promotions found</h3>
              <p className="mt-1 text-sm text-gray-500">
                {filters.search || filters.status !== "all"
                  ? "Try adjusting your filters"
                  : "Get started by creating a new promotion"}
              </p>
              {!filters.search && filters.status === "all" && (
                <div className="mt-6">
                  <Button onClick={handleCreateNew}>
                    <Plus className="mr-2 h-4 w-4" />
                    Create New Promotion
                  </Button>
                </div>
              )}
            </div>
          )}
        </CardContent>
      </Card>

      {/* Pagination */}
      {totalPages > 1 && (
        <Card className="border-gray-200">
          <CardContent className="p-4">
            <div className="flex items-center justify-between">
              <div className="text-sm text-gray-600">
                Showing <span className="font-semibold text-gray-900">{startIndex + 1}</span> to{" "}
                <span className="font-semibold text-gray-900">
                  {Math.min(startIndex + pageSize, filteredPromotions.length)}
                </span>{" "}
                of <span className="font-semibold text-gray-900">{filteredPromotions.length}</span> results
              </div>
              <div className="flex items-center space-x-2">
                <Button
                  variant="outline"
                  size="sm"
                  onClick={() => setCurrentPage(prev => Math.max(prev - 1, 1))}
                  disabled={currentPage === 1}
                  className="h-9 px-4"
                >
                  Previous
                </Button>
                <div className="flex items-center space-x-1">
                  {Array.from({ length: Math.min(5, totalPages) }, (_, i) => {
                    let pageNum;
                    if (totalPages <= 5) {
                      pageNum = i + 1;
                    } else if (currentPage <= 3) {
                      pageNum = i + 1;
                    } else if (currentPage >= totalPages - 2) {
                      pageNum = totalPages - 4 + i;
                    } else {
                      pageNum = currentPage - 2 + i;
                    }
                    return (
                      <Button
                        key={i}
                        variant={pageNum === currentPage ? "default" : "outline"}
                        size="sm"
                        onClick={() => setCurrentPage(pageNum)}
                        className={`h-9 w-9 ${pageNum === currentPage ? 'bg-blue-600 text-white' : ''}`}
                      >
                        {pageNum}
                      </Button>
                    );
                  })}
                </div>
                <Button
                  variant="outline"
                  size="sm"
                  onClick={() => setCurrentPage(prev => Math.min(prev + 1, totalPages))}
                  disabled={currentPage === totalPages}
                  className="h-9 px-4"
                >
                  Next
                </Button>
              </div>
            </div>
          </CardContent>
        </Card>
      )}

      {error && (
        <div className="text-red-600 text-center py-4">
          Error: {error}
        </div>
      )}
    </div>
  );
}