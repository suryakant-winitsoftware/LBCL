"use client";

import React, { useState, useEffect, useCallback } from 'react';
import { useRouter } from 'next/navigation';
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from '@/components/ui/alert-dialog';
import { useToast } from '@/components/ui/use-toast';
import {
  Plus,
  Search,
  RefreshCw,
  Download,
  Filter,
} from 'lucide-react';
import { apiService } from '@/services/api';
import { RouteGrid } from '@/components/admin/routes/route-grid';

interface FilterState {
  search: string;
  orgUID: string;
  status: string;
  isActive: boolean | null;
  dateRange: {
    startDate: string;
    endDate: string;
    dateType: 'created' | 'modified' | 'valid';
  };
  jobPosition: string;
  hasCustomers: boolean | null;
}

interface PaginationState {
  current: number;
  pageSize: number;
  total: number;
}

const RouteManagement: React.FC = () => {
  const router = useRouter();
  const { toast } = useToast();

  // State management
  const [loading, setLoading] = useState(false);
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  const [deleteRouteId, setDeleteRouteId] = useState<string | null>(null);
  const [organizations, setOrganizations] = useState<{ value: string; label: string }[]>([]);
  const [refreshTrigger, setRefreshTrigger] = useState(0);

  const [filters, setFilters] = useState<FilterState>({
    search: '',
    orgUID: '',
    status: '',
    isActive: null,
    dateRange: {
      startDate: '',
      endDate: '',
      dateType: 'created',
    },
    jobPosition: '',
    hasCustomers: null,
  });

  // Load organizations on mount
  useEffect(() => {
    loadOrganizations();
  }, []);

  const loadOrganizations = async () => {
    try {
      const data = await apiService.post<any>('/Org/GetOrgDetails', {
        pageNumber: 0,
        pageSize: 100,
        isCountRequired: false,
        sortCriterias: [],
        filterCriterias: []
      });
      
      if (data?.IsSuccess && data?.Data?.PagedData) {
        const orgList = data.Data.PagedData.map((org: any) => ({
          value: org.UID,
          label: org.Name || org.Code,
        }));
        setOrganizations(orgList);
      }
    } catch (error) {
      console.error('Error loading organizations:', error);
    }
  };


  const handleSearch = (value: string) => {
    setFilters(prev => ({ ...prev, search: value }));
  };

  const handleFilterChange = (key: keyof FilterState, value: any) => {
    setFilters(prev => ({ ...prev, [key]: value }));
  };

  const handleCreate = () => {
    router.push('/updatedfeatures/route-management/routes/create');
  };

  const handleView = (uid: string) => {
    router.push(`/updatedfeatures/route-management/routes/view/${uid}`);
  };

  const handleEdit = (uid: string) => {
    router.push(`/updatedfeatures/route-management/routes/edit/${uid}`);
  };

  const handleDeleteClick = (uid: string) => {
    setDeleteRouteId(uid);
    setDeleteDialogOpen(true);
  };

  const handleDeleteConfirm = async () => {
    if (!deleteRouteId) return;

    try {
      // Import routeService dynamically
      const { routeService } = await import('@/services/routeService');
      
      // CASCADE DELETE - This will delete EVERYTHING
      await routeService.deleteRoute(deleteRouteId);
      
      toast({
        title: "Success",
        description: "Route and all associated data deleted successfully",
      });
      
      triggerRefresh();
      setDeleteDialogOpen(false);
      setDeleteRouteId(null);
    } catch (error: any) {
      console.log('Delete error:', error);
      const errorMessage = error?.message || 'Failed to delete route';
      
      toast({
        title: "Error",
        description: errorMessage,
        variant: "destructive",
      });
      
      setDeleteDialogOpen(false);
    }
  };

  const triggerRefresh = useCallback(() => {
    setRefreshTrigger(prev => prev + 1);
  }, []);

  const handleBulkAction = useCallback(async (action: string, selectedIds: string[]) => {
    try {
      if (action === 'delete' && selectedIds.length > 0) {
        // Import routeService dynamically
        const { routeService } = await import('@/services/routeService');
        let deletedCount = 0;
        let failedCount = 0;
        const failureReasons: { [key: string]: string } = {};
        
        for (const id of selectedIds) {
          try {
            await routeService.deleteRoute(id);
            deletedCount++;
          } catch (error: any) {
            failedCount++;
            const errorMessage = error?.message || '';
            
            // Log individual failures but continue with others
            if (errorMessage.includes('foreign key constraint') || 
                errorMessage.includes('violates foreign key constraint')) {
              if (errorMessage.includes('route_customer')) {
                failureReasons[id] = 'has customer assignments';
              } else if (errorMessage.includes('route_schedule')) {
                failureReasons[id] = 'has route schedules';
              } else if (errorMessage.includes('beat_history')) {
                failureReasons[id] = 'has journey plans';
              } else {
                failureReasons[id] = 'has associated data';
              }
              console.warn(`Cannot delete route ${id}: ${failureReasons[id]}`);
            } else {
              failureReasons[id] = 'unknown error';
            }
          }
        }
        
        if (deletedCount > 0 && failedCount === 0) {
          toast({
            title: "Success",
            description: `${deletedCount} route(s) deleted successfully`,
          });
        } else if (deletedCount > 0 && failedCount > 0) {
          toast({
            title: "Partial Success",
            description: `${deletedCount} route(s) deleted. ${failedCount} route(s) could not be deleted because they have associated data (customers, schedules, or journey plans).`,
            variant: "default",
          });
        } else {
          toast({
            title: "Cannot Delete Routes",
            description: "All selected routes have associated data:\n• Customer assignments\n• Route schedules\n• Journey plans\n\nPlease remove these associations first.",
            variant: "destructive",
          });
        }
        
        if (deletedCount > 0) {
          triggerRefresh();
        }
      } else if (action === 'deactivate' && selectedIds.length > 0) {
        // No longer support soft delete - show message
        toast({
          title: "Info",
          description: "Deactivation is no longer supported. Use delete to remove routes permanently.",
          variant: "default",
        });
      } else if (action === 'activate' && selectedIds.length > 0) {
        toast({
          title: "Info",
          description: "Activate functionality will be implemented",
        });
      } else if (action === 'export') {
        toast({
          title: "Info",
          description: "Export functionality will be implemented",
        });
      }
    } catch (error) {
      toast({
        title: "Error",
        description: `Failed to perform bulk action`,
        variant: "destructive",
      });
    }
  }, [toast, triggerRefresh]);

  const handleRefresh = () => {
    triggerRefresh();
  };

  const handleExport = () => {
    toast({
      title: "Info",
      description: "Export functionality will be implemented",
    });
  };

  return (
    <div className="container mx-auto px-4 py-8">
      <Card>
        <CardHeader>
          <div className="flex items-center justify-between">
            <div>
              <CardTitle>Route Management</CardTitle>
              <CardDescription>
                Manage delivery routes and assignments
              </CardDescription>
            </div>
            <div className="flex gap-2">
              <Button 
                variant="outline" 
                size="sm"
                onClick={handleRefresh}
              >
                <RefreshCw className="h-4 w-4 mr-2" />
                Refresh
              </Button>
              <Button 
                variant="outline" 
                size="sm"
                onClick={handleExport}
              >
                <Download className="h-4 w-4 mr-2" />
                Export
              </Button>
              <Button 
                size="sm"
                onClick={handleCreate}
              >
                <Plus className="h-4 w-4 mr-2" />
                Create Route
              </Button>
            </div>
          </div>
        </CardHeader>
        
        <CardContent className="space-y-4">
          {/* Filters */}
          <div className="flex flex-wrap gap-4">
            <div className="flex-1 min-w-[300px]">
              <div className="relative">
                <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
                <Input
                  placeholder="Search by code, name, or ID..."
                  value={filters.search}
                  onChange={(e) => handleSearch(e.target.value)}
                  className="pl-10"
                />
              </div>
            </div>
            
            <Select
              value={filters.orgUID || 'all'}
              onValueChange={(value) => handleFilterChange('orgUID', value === 'all' ? '' : value)}
            >
              <SelectTrigger className="w-[200px]">
                <SelectValue placeholder="Select Organization" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All Organizations</SelectItem>
                {organizations.map((org) => (
                  <SelectItem key={org.value} value={org.value}>
                    {org.label}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            
            <Select
              value={filters.status || 'all'}
              onValueChange={(value) => handleFilterChange('status', value === 'all' ? '' : value)}
            >
              <SelectTrigger className="w-[150px]">
                <SelectValue placeholder="Status" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All Status</SelectItem>
                <SelectItem value="Active">Active</SelectItem>
                <SelectItem value="Inactive">Inactive</SelectItem>
                <SelectItem value="Pending">Pending</SelectItem>
              </SelectContent>
            </Select>
            
            <Select
              value={filters.isActive === null ? 'all' : filters.isActive.toString()}
              onValueChange={(value) => handleFilterChange('isActive', value === 'all' ? null : value === 'true')}
            >
              <SelectTrigger className="w-[150px]">
                <SelectValue placeholder="Active State" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All</SelectItem>
                <SelectItem value="true">Active</SelectItem>
                <SelectItem value="false">Inactive</SelectItem>
              </SelectContent>
            </Select>
            
            <Button variant="outline" size="sm">
              <Filter className="h-4 w-4 mr-2" />
              More Filters
            </Button>
          </div>
          
          {/* Data Grid */}
          <RouteGrid
            searchQuery={filters.search}
            filters={filters}
            refreshTrigger={refreshTrigger}
            onView={handleView}
            onEdit={handleEdit}
            onDelete={handleDeleteClick}
            onBulkAction={handleBulkAction}
          />
        </CardContent>
      </Card>

      {/* Delete Confirmation Dialog */}
      <AlertDialog open={deleteDialogOpen} onOpenChange={setDeleteDialogOpen}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle className="text-destructive">⚠️ Permanent Delete - This Cannot Be Undone!</AlertDialogTitle>
            <AlertDialogDescription asChild>
              <div className="space-y-3">
                <div className="font-semibold text-red-600">
                  WARNING: This will PERMANENTLY DELETE:
                </div>
                <ul className="list-disc list-inside space-y-1 text-sm">
                  <li>The route itself</li>
                  <li>ALL customer assignments to this route</li>
                  <li>ALL user assignments to this route</li>
                  <li>ALL journey plans using this route</li>
                  <li>ALL route schedules (daily, weekly, fortnight)</li>
                  <li>ALL beat history for this route</li>
                  <li>ALL change logs for this route</li>
                </ul>
                <div className="bg-red-50 border border-red-200 rounded p-2 text-red-700 font-medium">
                  This action is IRREVERSIBLE and will remove ALL data associated with this route from the database.
                </div>
                <div className="text-sm">
                  Are you absolutely sure you want to proceed?
                </div>
              </div>
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Cancel</AlertDialogCancel>
            <AlertDialogAction
              onClick={handleDeleteConfirm}
              className="bg-destructive text-destructive-foreground hover:bg-destructive/90"
            >
              Yes, Delete Everything
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  );
};

export default RouteManagement;