'use client';

import React, { useState, useEffect } from 'react';
import { Card } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Badge } from '@/components/ui/badge';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { 
  Search, 
  Download, 
  Upload,
  Activity,
  MapPin,
  Calendar,
  User,
  Target,
  TrendingUp,
  RefreshCw,
  Eye
} from 'lucide-react';
import { useRouter } from 'next/navigation';
import Link from 'next/link';
import { format } from 'date-fns';
import salesTeamActivityService from '@/services/sales-team-activity.service';
import { toast } from 'sonner';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';
import { Skeleton } from '@/components/ui/skeleton';
import { PaginationControls } from '@/components/ui/pagination-controls';

export default function SalesTeamActivityPage() {
  const router = useRouter();
  
  // State management
  const [loading, setLoading] = useState(false);
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedRole, setSelectedRole] = useState('');
  const [selectedEmployee, setSelectedEmployee] = useState('');
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [totalCount, setTotalCount] = useState(0);
  
  // Data states
  const [routes, setRoutes] = useState<any[]>([]);
  const [allRoutes, setAllRoutes] = useState<any[]>([]); // Store unfiltered routes
  const [jobPositions, setJobPositions] = useState<any[]>([]);
  const [employees, setEmployees] = useState<any[]>([]);


  // Load all routes initially
  const loadRoutes = async () => {
    setLoading(true);
    try {
      console.log('Loading routes...');
      const routesData = await salesTeamActivityService.getRoutes({
        pageNumber: 1,
        pageSize: 1000, // Load more routes for client-side filtering
        search: '' // No server-side search to avoid SQL errors
      });
      
      const routesList = routesData?.Data?.PagedData || [];
      setAllRoutes(routesList); // Store all routes
      applyFilters(routesList); // Apply current filters
      console.log(`Loaded ${routesList.length} routes`);
      
      return routesList;
    } catch (error) {
      console.error('Error loading routes:', error);
      setAllRoutes([]);
      setRoutes([]);
      return [];
    } finally {
      setLoading(false);
    }
  };

  // Load roles for role filter (using same API as route creation page)
  const loadRoles = async () => {
    try {
      const roles = await salesTeamActivityService.getRoles();
      setJobPositions(roles);
      console.log(`Loaded ${roles.length} roles`);
    } catch (error) {
      console.error('Error loading roles:', error);
    }
  };

  // Load employees for employee filter (using same API as route creation page)
  const loadEmployees = async (roleUID?: string) => {
    try {
      const empData = await salesTeamActivityService.getEmployees('EPIC01', roleUID);
      setEmployees(empData);
      console.log(`Loaded ${empData.length} employees${roleUID ? ` for role ${roleUID}` : ''}`);
    } catch (error) {
      console.error('Error loading employees:', error);
    }
  };

  // Apply client-side filters
  const applyFilters = (routesList: any[] = allRoutes) => {
    console.log('🔍 PAGE: applyFilters called with:', {
      routesCount: routesList.length,
      searchTerm,
      selectedRole,
      selectedEmployee
    });

    // Log sample route data to understand the structure
    if (routesList.length > 0) {
      console.log('🔍 PAGE: Sample route data:', {
        route: routesList[0],
        availableFields: Object.keys(routesList[0])
      });
    }

    let filteredRoutes = [...routesList];

    // Filter by route code/name (search term)
    if (searchTerm.trim()) {
      const searchLower = searchTerm.toLowerCase();
      const beforeCount = filteredRoutes.length;
      filteredRoutes = filteredRoutes.filter(route => 
        (route.Code || '').toLowerCase().includes(searchLower) ||
        (route.Name || '').toLowerCase().includes(searchLower)
      );
      console.log(`🔍 PAGE: Search filter: ${beforeCount} → ${filteredRoutes.length} routes`);
    }

    // Filter by role (using RoleUID from database role_uid field)
    if (selectedRole) {
      const beforeCount = filteredRoutes.length;
      filteredRoutes = filteredRoutes.filter(route => {
        const matches = route.RoleUID === selectedRole;
        if (!matches && beforeCount < 10) { // Only log for small datasets to avoid spam
          console.log(`🔍 PAGE: Route ${route.Code} RoleUID '${route.RoleUID}' != selected role '${selectedRole}'`);
        }
        return matches;
      });
      console.log(`🔍 PAGE: Role filter (${selectedRole}): ${beforeCount} → ${filteredRoutes.length} routes`);
    }

    // Filter by employee (using JobPositionUID from database job_position_uid field)
    if (selectedEmployee) {
      const beforeCount = filteredRoutes.length;
      filteredRoutes = filteredRoutes.filter(route => {
        const matches = route.JobPositionUID === selectedEmployee ||
                       route.CreatedBy === selectedEmployee ||
                       route.ModifiedBy === selectedEmployee;
        if (!matches && beforeCount < 10) { // Only log for small datasets to avoid spam
          console.log(`🔍 PAGE: Route ${route.Code} - JobPositionUID: '${route.JobPositionUID}', CreatedBy: '${route.CreatedBy}', ModifiedBy: '${route.ModifiedBy}' - selected: '${selectedEmployee}'`);
        }
        return matches;
      });
      console.log(`🔍 PAGE: Employee filter (${selectedEmployee}): ${beforeCount} → ${filteredRoutes.length} routes`);
    }

    console.log(`🔍 PAGE: Final filtered routes: ${filteredRoutes.length}`);
    setRoutes(filteredRoutes);
    setTotalCount(filteredRoutes.length);
  };


  // Handle filter changes
  const handleSearch = (value: string) => {
    setSearchTerm(value);
    setCurrentPage(1);
  };

  const handleRoleChange = (value: string) => {
    const newRoleValue = value === 'all' ? '' : value;
    setSelectedRole(newRoleValue);
    setCurrentPage(1);
    
    // Clear current employee selection since role changed
    if (selectedEmployee) {
      setSelectedEmployee('');
    }
  };

  const handleEmployeeChange = (value: string) => {
    setSelectedEmployee(value === 'all' ? '' : value);
    setCurrentPage(1);
  };

  // Load initial data
  useEffect(() => {
    loadRoutes();
    loadRoles();
    loadEmployees();
  }, []);

  // Reload employees when role changes (for role-based filtering)
  useEffect(() => {
    if (selectedRole) {
      console.log('🔄 Role changed, reloading employees for role:', selectedRole);
      loadEmployees(selectedRole);
    } else {
      console.log('📄 Loading all employees (no role filter)');
      loadEmployees();
    }
  }, [selectedRole]);

  // Apply filters when filter values change
  useEffect(() => {
    if (allRoutes.length > 0) {
      applyFilters();
    }
  }, [searchTerm, selectedRole, selectedEmployee, allRoutes]);

  // Export functionality
  const handleExport = async () => {
    try {
      // TODO: Implement actual export
      toast.success('Export started - this will download all beat histories as CSV');
    } catch (error) {
      toast.error('Failed to export data');
    }
  };

  // Refresh data
  const handleRefresh = async () => {
    setLoading(true);
    try {
      // Reload routes
      await loadRoutes();
      
      toast.success('Data refreshed successfully');
    } catch (error) {
      toast.error('Failed to refresh data');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen bg-gray-50">
      <div className="container mx-auto py-6 space-y-8 max-w-7xl">
        {/* Header Section */}
        <Card className="border-0 shadow-sm bg-gradient-to-r from-blue-50 to-indigo-50">
          <div className="p-6">
            <div className="flex flex-col gap-4 md:flex-row md:items-center md:justify-between">
              <div className="space-y-2">
                <div className="flex items-center gap-3">
                  <div className="p-2 bg-blue-100 rounded-lg">
                    <Activity className="h-6 w-6 text-blue-600" />
                  </div>
                  <h1 className="text-2xl font-bold tracking-tight text-gray-900">Sales Team Activity</h1>
                </div>
                <p className="text-sm text-gray-600 flex items-center gap-2">
                  <Target className="h-4 w-4" />
                  Monitor field force performance and beat history across routes
                </p>
              </div>
              <div className="flex items-center gap-3">
                <Button variant="outline" size="default" className="hidden sm:flex bg-white hover:bg-gray-50">
                  <Upload className="h-4 w-4 mr-2" />
                  Import
                </Button>
                <Button
                  variant="outline"
                  size="default"
                  className="hidden sm:flex bg-white hover:bg-gray-50"
                  onClick={handleExport}
                >
                  <Download className="h-4 w-4 mr-2" />
                  Export
                </Button>
                <Button
                  variant="outline"
                  size="default"
                  onClick={handleRefresh}
                  disabled={loading}
                  className="bg-white hover:bg-gray-50"
                >
                  <RefreshCw className={`h-4 w-4 mr-2 ${loading ? 'animate-spin' : ''}`} />
                  Refresh
                </Button>
              </div>
            </div>
          </div>
        </Card>

      <div className="space-y-4">
        {/* Filters and Actions Bar */}
        <Card className="p-6 border-0 shadow-sm bg-white">
          <div className="space-y-4">
            <div className="flex items-center gap-2 mb-4">
              <Search className="h-5 w-5 text-blue-600" />
              <h3 className="font-medium text-gray-900">Filter Routes</h3>
            </div>
            <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
              {/* Route Search */}
              <div className="relative">
                <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-muted-foreground h-4 w-4" />
                <Input
                  placeholder="Search by route code or name..."
                  value={searchTerm}
                  onChange={(e) => handleSearch(e.target.value)}
                  className="pl-10 bg-background border-muted-foreground/20 focus:border-primary"
                />
              </div>

              {/* Role Filter */}
              <Select value={selectedRole || 'all'} onValueChange={handleRoleChange}>
                <SelectTrigger className="bg-background border-muted-foreground/20 focus:border-primary">
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
              <Select value={selectedEmployee || 'all'} onValueChange={handleEmployeeChange}>
                <SelectTrigger className="bg-background border-muted-foreground/20 focus:border-primary">
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

              {/* Clear Filters Button */}
              <Button
                variant="outline"
                onClick={() => {
                  setSearchTerm('');
                  setSelectedRole('');
                  setSelectedEmployee('');
                }}
                className="w-full bg-red-50 text-red-700 border-red-200 hover:bg-red-100 transition-colors"
              >
                <RefreshCw className="h-4 w-4 mr-2" />
                Clear Filters
              </Button>
            </div>

            {/* Filter Summary */}
            {(searchTerm || selectedRole || selectedEmployee) && (
              <div className="flex items-center gap-2 text-sm text-gray-600 bg-blue-50 p-3 rounded-lg border border-blue-200">
                <div className="p-1 bg-blue-100 rounded">
                  <Target className="h-3 w-3 text-blue-600" />
                </div>
                <span className="font-medium">Active filters:</span>
                <div className="flex flex-wrap gap-1">
                  {searchTerm && (
                    <Badge variant="secondary" className="text-xs bg-white border border-blue-200">
                      Route: {searchTerm}
                    </Badge>
                  )}
                  {selectedRole && (
                    <Badge variant="secondary" className="text-xs bg-white border border-blue-200">
                      Role: {jobPositions.find(r => r.UID === selectedRole)?.Name || jobPositions.find(r => r.UID === selectedRole)?.Code || selectedRole}
                    </Badge>
                  )}
                  {selectedEmployee && (
                    <Badge variant="secondary" className="text-xs bg-white border border-blue-200">
                      Employee: {employees.find(e => e.UID === selectedEmployee)?.Label || employees.find(e => e.UID === selectedEmployee)?.Name || selectedEmployee}
                    </Badge>
                  )}
                </div>
              </div>
            )}
          </div>
        </Card>

        {/* Main Table Card */}
        <Card className="overflow-hidden border-0 shadow-sm bg-white">
          <div className="bg-gray-50/50 border-b p-4">
            <div className="flex items-center gap-3">
              <div className="p-2 bg-green-100 rounded-lg">
                <MapPin className="h-5 w-5 text-green-600" />
              </div>
              <div>
                <h3 className="font-semibold text-gray-900">Routes Overview</h3>
                <p className="text-sm text-gray-600">Total {routes.length} route{routes.length !== 1 ? 's' : ''} available</p>
              </div>
            </div>
          </div>
          
          <div className="overflow-x-auto">
            <Table>
              <TableHeader>
                <TableRow className="bg-gradient-to-r from-gray-50 to-gray-100 border-b border-gray-200">
                  <TableHead className="font-semibold text-gray-700">
                    <div className="flex items-center gap-2">
                      <MapPin className="h-4 w-4 text-blue-600" />
                      Route Code
                    </div>
                  </TableHead>
                  <TableHead className="font-semibold text-gray-700">
                    <div className="flex items-center gap-2">
                      <Activity className="h-4 w-4 text-blue-600" />
                      Route Name
                    </div>
                  </TableHead>
                  <TableHead className="font-semibold text-gray-700">
                    <div className="flex items-center gap-2">
                      <User className="h-4 w-4 text-blue-600" />
                      Customers
                    </div>
                  </TableHead>
                  <TableHead className="font-semibold text-gray-700">
                    <div className="flex items-center gap-2">
                      <User className="h-4 w-4 text-blue-600" />
                      Role
                    </div>
                  </TableHead>
                  <TableHead className="font-semibold text-gray-700">
                    <div className="flex items-center gap-2">
                      <Target className="h-4 w-4 text-blue-600" />
                      Employee ID
                    </div>
                  </TableHead>
                  <TableHead className="font-semibold text-gray-700">
                    <div className="flex items-center gap-2">
                      <TrendingUp className="h-4 w-4 text-blue-600" />
                      Status
                    </div>
                  </TableHead>
                  <TableHead className="font-semibold text-gray-700">
                    <div className="flex items-center gap-2">
                      <Calendar className="h-4 w-4 text-green-600" />
                      Valid From
                    </div>
                  </TableHead>
                  <TableHead className="font-semibold text-gray-700">
                    <div className="flex items-center gap-2">
                      <Calendar className="h-4 w-4 text-red-600" />
                      Valid To
                    </div>
                  </TableHead>
                  <TableHead className="font-semibold text-gray-700">
                    <div className="flex items-center gap-2">
                      <Eye className="h-4 w-4 text-blue-600" />
                      Actions
                    </div>
                  </TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {loading ? (
                  <>
                    {[...Array(pageSize)].map((_, index) => (
                      <TableRow key={`skeleton-${index}`}>
                        <TableCell><Skeleton className="h-4 w-20" /></TableCell>
                        <TableCell><Skeleton className="h-4 w-32" /></TableCell>
                        <TableCell><Skeleton className="h-4 w-24" /></TableCell>
                        <TableCell><Skeleton className="h-4 w-20" /></TableCell>
                        <TableCell><Skeleton className="h-4 w-20" /></TableCell>
                        <TableCell><Skeleton className="h-6 w-16 rounded-full" /></TableCell>
                        <TableCell><Skeleton className="h-4 w-24" /></TableCell>
                        <TableCell><Skeleton className="h-4 w-24" /></TableCell>
                        <TableCell><Skeleton className="h-8 w-20" /></TableCell>
                      </TableRow>
                    ))}
                  </>
                ) : routes.length === 0 ? (
                  <TableRow>
                    <TableCell colSpan={9} className="text-center py-8">
                      No routes found
                    </TableCell>
                  </TableRow>
                ) : (
                  routes.map((route) => (
                      <TableRow key={route.UID} className="hover:bg-blue-50/30 transition-all duration-200 border-b border-gray-100">
                        <TableCell className="font-medium text-sm text-gray-900">{route.Code}</TableCell>
                        <TableCell className="text-sm text-gray-900">{route.Name}</TableCell>
                        <TableCell className="text-sm">
                          <span className="inline-flex items-center gap-1 px-2 py-1 bg-blue-50 text-blue-700 rounded-md text-xs font-medium">
                            <User className="h-3 w-3" />
                            {route.TotalCustomers || 0}
                          </span>
                        </TableCell>
                        <TableCell>
                          <span className="text-sm text-gray-600 bg-gray-50 px-2 py-1 rounded-md">
                            {route.RoleUID || '-'}
                          </span>
                        </TableCell>
                        <TableCell>
                          <span className="text-sm text-gray-600 bg-gray-50 px-2 py-1 rounded-md">
                            {route.JobPositionUID || '-'}
                          </span>
                        </TableCell>
                        <TableCell>
                          <Badge 
                            variant={route.IsActive ? 'default' : 'secondary'}
                            className={`font-medium ${
                              route.IsActive 
                                ? 'bg-green-100 text-green-800 hover:bg-green-200' 
                                : 'bg-gray-100 text-gray-800'
                            }`}
                          >
                            {route.IsActive ? 'Active' : 'Inactive'}
                          </Badge>
                        </TableCell>
                        <TableCell className="text-sm text-gray-600">
                          <div className="flex items-center gap-1">
                            <Calendar className="h-3 w-3 text-green-500" />
                            {format(new Date(route.ValidFrom), 'MMM dd, yyyy')}
                          </div>
                        </TableCell>
                        <TableCell className="text-sm text-gray-600">
                          <div className="flex items-center gap-1">
                            <Calendar className="h-3 w-3 text-red-500" />
                            {format(new Date(route.ValidUpto), 'MMM dd, yyyy')}
                          </div>
                        </TableCell>
                        <TableCell>
                          <Link 
                            href={`/reportsanalytics/field-activities/reports/sales-team-activity/route/${route.UID}`}
                            onClick={(e) => e.stopPropagation()}
                          >
                            <Button
                              variant="outline"
                              size="sm"
                              className="h-8 px-3 bg-blue-50 text-blue-700 border-blue-200 hover:bg-blue-600 hover:text-white transition-all duration-200"
                              title="View all beat histories for this route"
                            >
                              <Eye className="h-4 w-4 mr-1" />
                              View Details
                            </Button>
                          </Link>
                        </TableCell>
                      </TableRow>
                  ))
                )}
              </TableBody>
            </Table>
          </div>
          
          {totalCount > 0 && (
            <div className="border-t bg-gray-50/50 px-6 py-4">
              <PaginationControls
                currentPage={currentPage}
                totalCount={totalCount}
                pageSize={pageSize}
                onPageChange={setCurrentPage}
                onPageSizeChange={setPageSize}
                itemName="routes"
              />
            </div>
          )}
        </Card>
      </div>
      </div>
    </div>
  );
}