"use client";

import React, { useState, useCallback, useRef, useEffect } from 'react';
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
  ChevronDown,
  X,
} from 'lucide-react';
import { Badge } from '@/components/ui/badge';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
  DropdownMenuCheckboxItem,
} from '@/components/ui/dropdown-menu';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { RouteGrid } from '@/components/admin/routes/route-grid';
import salesTeamActivityService from '@/services/sales-team-activity.service';

interface FilterState {
  search: string;
  validFromFilter: string;
  validToFilter: string;
  selectedRole: string;
  selectedEmployee: string;
}

const RouteManagement: React.FC = () => {
  const router = useRouter();
  const { toast } = useToast();

  // State management
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  const [deleteRouteId, setDeleteRouteId] = useState<string | null>(null);
  const [refreshTrigger, setRefreshTrigger] = useState(0);

  const [filters, setFilters] = useState<FilterState>({
    search: '',
    validFromFilter: '',
    validToFilter: '',
    selectedRole: '',
    selectedEmployee: '',
  });
  const [statusFilter, setStatusFilter] = useState<string[]>([]); // Filter for route status
  const [jobPositions, setJobPositions] = useState<any[]>([]);
  const [employees, setEmployees] = useState<any[]>([]);
  const searchInputRef = useRef<HTMLInputElement>(null); // Ref for search input


  const handleSearch = (value: string) => {
    setFilters(prev => ({ ...prev, search: value }));
  };

  // Load roles for role filter
  const loadRoles = async () => {
    try {
      const roles = await salesTeamActivityService.getRoles();
      setJobPositions(roles);
      console.log(`Loaded ${roles.length} roles`);
    } catch (error) {
      console.error('Error loading roles:', error);
    }
  };

  // Load employees for employee filter
  const loadEmployees = async (roleUID?: string) => {
    try {
      const empData = await salesTeamActivityService.getEmployees('EPIC01', roleUID);
      setEmployees(empData);
      console.log(`Loaded ${empData.length} employees${roleUID ? ` for role ${roleUID}` : ''}`);
    } catch (error) {
      console.error('Error loading employees:', error);
    }
  };

  // Handle role filter change
  const handleRoleChange = (value: string) => {
    const newRoleValue = value === 'all' ? '' : value;
    setFilters(prev => ({ ...prev, selectedRole: newRoleValue }));
    
    // Clear current employee selection since role changed
    if (filters.selectedEmployee) {
      setFilters(prev => ({ ...prev, selectedEmployee: '' }));
    }
  };

  // Handle employee filter change
  const handleEmployeeChange = (value: string) => {
    setFilters(prev => ({ ...prev, selectedEmployee: value === 'all' ? '' : value }));
  };

  // Add keyboard shortcut for Ctrl+F
  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if ((e.ctrlKey || e.metaKey) && e.key === 'f') {
        e.preventDefault();
        searchInputRef.current?.focus();
        searchInputRef.current?.select();
      }
    };

    document.addEventListener('keydown', handleKeyDown);
    return () => document.removeEventListener('keydown', handleKeyDown);
  }, []);

  // Load initial data
  useEffect(() => {
    loadRoles();
    loadEmployees();
  }, []);

  // Reload employees when role changes
  useEffect(() => {
    if (filters.selectedRole) {
      console.log('🔄 Role changed, reloading employees for role:', filters.selectedRole);
      loadEmployees(filters.selectedRole);
    } else {
      console.log('📄 Loading all employees (no role filter)');
      loadEmployees();
    }
  }, [filters.selectedRole]);


  const handleCreate = () => {
    router.push('/distributiondelivery/route-management/routes/create');
  };

  const handleView = (uid: string) => {
    router.push(`/distributiondelivery/route-management/routes/view/${uid}`);
  };

  const handleEdit = (uid: string) => {
    router.push(`/distributiondelivery/route-management/routes/edit/${uid}`);
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
        // Export selected routes - for now export all since we don't have selectedIds filtering in export function
        const { routeService } = await import('@/services/routeService');
        
        const blob = await routeService.exportRoutes("csv", { search: filters.search });
        const url = URL.createObjectURL(blob);
        const a = document.createElement("a");
        a.href = url;
        a.download = `routes-selected-export-${new Date().toISOString().split("T")[0]}.csv`;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        URL.revokeObjectURL(url);
        
        toast({
          title: "Success",
          description: "Routes exported successfully.",
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

  const handleExport = useCallback(async () => {
    try {
      // Import routeService dynamically
      const { routeService } = await import('@/services/routeService');
      
      const blob = await routeService.exportRoutes("csv", { search: filters.search });
      const url = URL.createObjectURL(blob);
      const a = document.createElement("a");
      a.href = url;
      a.download = `routes-export-${new Date().toISOString().split("T")[0]}.csv`;
      document.body.appendChild(a);
      a.click();
      document.body.removeChild(a);
      URL.revokeObjectURL(url);
      
      toast({
        title: "Success",
        description: "Routes exported successfully.",
      });
    } catch (error) {
      toast({
        title: "Error",
        description: "Failed to export routes. Please try again.",
        variant: "destructive",
      });
    }
  }, [filters.search, toast]);

  return (
    <div className="container mx-auto p-6 space-y-6">
      {/* Page Header */}
      <div className="flex flex-col gap-4 md:flex-row md:items-center md:justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Route Management</h1>
          <p className="text-muted-foreground">
            Manage delivery routes and assignments
          </p>
        </div>
        <div className="flex gap-2">
          <Button 
            variant="outline" 
            size="icon"
            onClick={handleRefresh}
          >
            <RefreshCw className="h-4 w-4" />
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

      {/* Search and Filters */}
      <Card className="shadow-sm border-gray-200">
        <CardContent className="py-3">
          <div className="flex gap-3">
            <div className="relative flex-1">
              <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 h-4 w-4" />
              <Input
                ref={searchInputRef}
                placeholder="Search by code, name, or ID... (Ctrl+F)"
                value={filters.search}
                onChange={(e) => handleSearch(e.target.value)}
                className="pl-10 h-9 border-gray-200 focus:border-primary focus:ring-1 focus:ring-primary/20"
              />
            </div>
            
            {/* Role Filter */}
            <Select value={filters.selectedRole || 'all'} onValueChange={handleRoleChange}>
              <SelectTrigger className="w-48 h-9">
                <SelectValue placeholder="Filter by role..." />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All Roles</SelectItem>
                {jobPositions.map((role) => (
                  <SelectItem key={role.UID} value={role.UID}>
                    {role.Name || role.Code || role.UID}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>

            {/* Employee Filter */}
            <Select value={filters.selectedEmployee || 'all'} onValueChange={handleEmployeeChange}>
              <SelectTrigger className="w-48 h-9">
                <SelectValue placeholder="Filter by employee..." />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All Employees</SelectItem>
                {employees.map((employee) => (
                  <SelectItem key={employee.UID} value={employee.UID}>
                    {employee.Label || employee.Name || employee.Code || employee.UID}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            
            {/* Route Status Filter Dropdown */}
            <DropdownMenu>
              <DropdownMenuTrigger asChild>
                <Button variant="outline">
                  <Filter className="h-4 w-4 mr-2" />
                  Status
                  {statusFilter.length > 0 && (
                    <Badge variant="secondary" className="ml-2">
                      {statusFilter.length}
                    </Badge>
                  )}
                  <ChevronDown className="h-4 w-4 ml-2" />
                </Button>
              </DropdownMenuTrigger>
              <DropdownMenuContent align="end" className="w-48">
                <DropdownMenuLabel>Filter by Status</DropdownMenuLabel>
                <DropdownMenuSeparator />
                <DropdownMenuCheckboxItem
                  checked={statusFilter.includes("Active")}
                  onCheckedChange={(checked) => {
                    setStatusFilter(prev => 
                      checked 
                        ? [...prev, "Active"]
                        : prev.filter(s => s !== "Active")
                    )
                  }}
                >
                  Active
                </DropdownMenuCheckboxItem>
                <DropdownMenuCheckboxItem
                  checked={statusFilter.includes("Inactive")}
                  onCheckedChange={(checked) => {
                    setStatusFilter(prev => 
                      checked 
                        ? [...prev, "Inactive"]
                        : prev.filter(s => s !== "Inactive")
                    )
                  }}
                >
                  Inactive
                </DropdownMenuCheckboxItem>
                {statusFilter.length > 0 && (
                  <>
                    <DropdownMenuSeparator />
                    <DropdownMenuItem
                      onClick={() => setStatusFilter([])}
                    >
                      <X className="h-4 w-4 mr-2" />
                      Clear Filter
                    </DropdownMenuItem>
                  </>
                )}
              </DropdownMenuContent>
            </DropdownMenu>

            {/* Valid From Date Filter */}
            <div className="flex items-center gap-2">
              <Input
                type="date"
                value={filters.validFromFilter}
                onChange={(e) => {
                  setFilters(prev => ({ ...prev, validFromFilter: e.target.value }))
                }}
                className="w-40 h-9"
                placeholder="Valid From"
              />
              {filters.validFromFilter && (
                <Button
                  variant="ghost"
                  size="sm"
                  onClick={() => {
                    setFilters(prev => ({ ...prev, validFromFilter: '' }))
                  }}
                  className="h-8 w-8 p-0"
                >
                  <X className="h-4 w-4" />
                </Button>
              )}
            </div>

            {/* Valid To Date Filter */}
            <div className="flex items-center gap-2">
              <Input
                type="date"
                value={filters.validToFilter}
                onChange={(e) => {
                  setFilters(prev => ({ ...prev, validToFilter: e.target.value }))
                }}
                className="w-40 h-9"
                placeholder="Valid To"
              />
              {filters.validToFilter && (
                <Button
                  variant="ghost"
                  size="sm"
                  onClick={() => {
                    setFilters(prev => ({ ...prev, validToFilter: '' }))
                  }}
                  className="h-8 w-8 p-0"
                >
                  <X className="h-4 w-4" />
                </Button>
              )}
            </div>
            
            <Button
              variant="outline"
              size="default"
              onClick={handleRefresh}
              disabled={false}
            >
              <RefreshCw className="h-4 w-4" />
            </Button>
          </div>
        </CardContent>
      </Card>

      {/* Data Table */}
      <Card className="shadow-sm border-gray-200">
        <CardContent className="p-0">
          <RouteGrid
            searchQuery={filters.search}
            filters={{
              ...filters,
              status: statusFilter
            }}
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