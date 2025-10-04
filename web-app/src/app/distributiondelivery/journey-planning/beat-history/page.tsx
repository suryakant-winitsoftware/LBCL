"use client";

import React, { useState, useEffect } from 'react';
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
import { Badge } from '@/components/ui/badge';
import { Progress } from '@/components/ui/progress';
import { Skeleton } from '@/components/ui/skeleton';
import { useToast } from '@/components/ui/use-toast';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import {
  Search,
  RefreshCw,
  MoreHorizontal,
  Eye,
  Edit,
  Calendar,
  Clock,
  Users,
  MapPin,
  CheckCircle,
  AlertCircle,
  Loader,
  Route,
  FileText,
  Plus,
  Settings,
} from 'lucide-react';
import moment from 'moment';
import { formatDateToDayMonthYear, formatTime, formatDayOfWeek } from '@/utils/date-formatter';
import { api } from '@/services/api';

interface BeatHistory {
  UID: string;
  RouteUID: string;
  RouteName: string;
  RouteCode: string;
  OrgUID: string;
  JobPositionUID: string;
  EmpUID: string;
  EmployeeName: string;
  LoginId: string;
  VisitDate: string;
  YearMonth: number;
  PlannedStoreVisits: number;
  ActualStoreVisits: number;
  SkippedStoreVisits: number;
  Coverage: number;
  ACoverage: number;
  TCoverage: number;
  Status: string;
  InvoiceStatus: string;
  StartTime?: string;
  EndTime?: string;
  PlannedStartTime?: string;
  PlannedEndTime?: string;
  Notes?: string;
  CreatedBy: string;
  CreatedTime: string;
  ModifiedBy: string;
  ModifiedTime: string;
  // Route Schedule Integration
  RouteScheduleUID?: string;
  ScheduleType?: string;
  // Holiday Information
  IsHoliday?: boolean;
  HolidayName?: string;
  // Working Day Information
  IsWorkingDay?: boolean;
  DayType?: string; // WorkDay, OffDay, Holiday
}

interface StoreVisit {
  UID: string;
  StoreUID: string;
  StoreName: string;
  StoreCode: string;
  Status: string;
  LoginTime?: string;
  LogoutTime?: string;
  VisitDuration: number;
  SerialNo: number;
  IsProductive: boolean;
  IsSkipped: boolean;
  ActualValue: number;
  TargetValue: number;
}

interface FilterState {
  search: string;
  status: string;
  dateRange: string;
  orgUID: string;
}

interface PaginationState {
  current: number;
  pageSize: number;
  total: number;
}

const BeatHistoryManagement: React.FC = () => {
  const router = useRouter();
  const { toast } = useToast();
  
  const [beatHistories, setBeatHistories] = useState<BeatHistory[]>([]);
  const [loading, setLoading] = useState(true);
  const [detailsLoading, setDetailsLoading] = useState(false);
  const [selectedBeatHistory, setSelectedBeatHistory] = useState<BeatHistory | null>(null);
  const [storeVisits, setStoreVisits] = useState<StoreVisit[]>([]);
  const [detailsOpen, setDetailsOpen] = useState(false);
  const [organizations, setOrganizations] = useState<{ value: string; label: string }[]>([]);

  const [pagination, setPagination] = useState<PaginationState>({
    current: 1,
    pageSize: 10,
    total: 0,
  });

  const [filters, setFilters] = useState<FilterState>({
    search: '',
    status: '',
    dateRange: 'today',
    orgUID: 'Farmley',
  });

  useEffect(() => {
    loadOrganizations();
  }, []);

  useEffect(() => {
    fetchBeatHistories();
  }, [pagination.current, pagination.pageSize, filters]);

  const loadOrganizations = async () => {
    try {
      const data = await api.org.getDetails({
        pageNumber: 0,
        pageSize: 100,
        isCountRequired: false,
        sortCriterias: [],
        filterCriterias: []
      });
      
      if (data.IsSuccess && data.Data?.PagedData) {
        setOrganizations(
          data.Data.PagedData.map((org: any) => ({
            value: org.UID,
            label: org.Name || org.Code,
          }))
        );
      }
    } catch (error) {
      console.error('Error loading organizations:', error);
    }
  };

  const fetchBeatHistories = async () => {
    try {
      setLoading(true);
      
      const filterCriterias = [];
      
      if (filters.orgUID) {
        filterCriterias.push({
          filterBy: 'OrgUID',
          filterValue: filters.orgUID,
          filterOperator: 'equals'
        });
      }
      
      if (filters.search) {
        filterCriterias.push({
          filterBy: 'LoginId',
          filterValue: filters.search,
          filterOperator: 'contains'
        });
      }
      
      if (filters.status) {
        filterCriterias.push({
          filterBy: 'Status',
          filterValue: filters.status,
          filterOperator: 'equals'
        });
      }

      // Add date filter based on dateRange selection
      const today = moment();
      let startDate, endDate;
      
      switch (filters.dateRange) {
        case 'today':
          startDate = today.format('YYYY-MM-DD');
          endDate = today.format('YYYY-MM-DD');
          break;
        case 'week':
          startDate = today.startOf('week').format('YYYY-MM-DD');
          endDate = today.endOf('week').format('YYYY-MM-DD');
          break;
        case 'month':
          startDate = today.startOf('month').format('YYYY-MM-DD');
          endDate = today.endOf('month').format('YYYY-MM-DD');
          break;
        default:
          startDate = today.format('YYYY-MM-DD');
          endDate = today.format('YYYY-MM-DD');
      }

      filterCriterias.push({
        filterBy: 'VisitDate',
        filterValue: startDate,
        filterOperator: 'greaterThanOrEqual'
      });
      
      filterCriterias.push({
        filterBy: 'VisitDate',
        filterValue: endDate,
        filterOperator: 'lessThanOrEqual'
      });

      const request = {
        pageNumber: pagination.current - 1,
        pageSize: pagination.pageSize,
        isCountRequired: true,
        sortCriterias: [{
          sortBy: 'VisitDate',
          sortOrder: 'desc'
        }],
        filterCriterias
      };

      const response = await api.beatHistory.selectAll(request);
      
      if (response.IsSuccess && response.Data) {
        const histories = (response.Data.PagedData || []).map((item: any) => ({
          UID: item.UID,
          RouteUID: item.RouteUID,
          RouteName: item.RouteName || 'Unknown Route',
          RouteCode: item.RouteCode || '',
          OrgUID: item.OrgUID,
          JobPositionUID: item.JobPositionUID,
          EmpUID: item.EmpUID || item.LoginId,
          EmployeeName: item.EmployeeName || item.LoginId,
          LoginId: item.LoginId,
          VisitDate: item.VisitDate,
          YearMonth: item.YearMonth,
          PlannedStoreVisits: item.PlannedStoreVisits || 0,
          ActualStoreVisits: item.ActualStoreVisits || 0,
          SkippedStoreVisits: item.SkippedStoreVisits || 0,
          Coverage: item.Coverage || 0,
          ACoverage: item.ACoverage || 0,
          TCoverage: item.TCoverage || 0,
          Status: item.Status || 'Pending',
          InvoiceStatus: item.InvoiceStatus || 'Pending',
          StartTime: item.StartTime,
          EndTime: item.EndTime,
          PlannedStartTime: item.PlannedStartTime,
          PlannedEndTime: item.PlannedEndTime,
          Notes: item.Notes,
          CreatedBy: item.CreatedBy,
          CreatedTime: item.CreatedTime,
          ModifiedBy: item.ModifiedBy,
          ModifiedTime: item.ModifiedTime,
        }));
        
        setBeatHistories(histories);
        setPagination(prev => ({
          ...prev,
          total: response.Data.TotalCount || 0,
        }));
      } else {
        setBeatHistories([]);
        setPagination(prev => ({ ...prev, total: 0 }));
      }
    } catch (error) {
      console.error('Error fetching beat histories:', error);
      toast({
        title: "Error",
        description: "Failed to fetch beat histories. Please try again.",
        variant: "destructive",
      });
    } finally {
      setLoading(false);
    }
  };

  const fetchStoreVisitDetails = async (beatHistoryUID: string) => {
    try {
      setDetailsLoading(true);
      const response = await api.beatHistory.getCustomersByBeatHistoryUID(beatHistoryUID);
      
      if (response?.IsSuccess && response?.Data) {
        const visits = Array.isArray(response.Data) 
          ? response.Data 
          : response.Data.StoreHistories || [];
        
        const transformedVisits = visits.map((visit: any) => ({
          UID: visit.UID,
          StoreUID: visit.StoreUID,
          StoreName: visit.StoreName || visit.CustomerName || 'Unknown Store',
          StoreCode: visit.StoreCode || visit.CustomerCode || '',
          Status: visit.Status || 'Pending',
          LoginTime: visit.LoginTime,
          LogoutTime: visit.LogoutTime,
          VisitDuration: visit.VisitDuration || 30,
          SerialNo: visit.SerialNo || 0,
          IsProductive: visit.IsProductive || false,
          IsSkipped: visit.IsSkipped || false,
          ActualValue: visit.ActualValue || 0,
          TargetValue: visit.TargetValue || 0,
        }));

        setStoreVisits(transformedVisits.sort((a, b) => a.SerialNo - b.SerialNo));
      } else {
        setStoreVisits([]);
      }
    } catch (error) {
      console.error('Error fetching store visit details:', error);
      toast({
        title: "Error",
        description: "Failed to fetch store visit details",
        variant: "destructive",
      });
    } finally {
      setDetailsLoading(false);
    }
  };

  const handleViewBeatHistory = async (beatHistory: BeatHistory) => {
    setSelectedBeatHistory(beatHistory);
    setDetailsOpen(true);
    await fetchStoreVisitDetails(beatHistory.UID);
  };

  const handleEditBeatHistory = (uid: string) => {
    router.push(`/updatedfeatures/journey-plan-management/journey-plans/edit/${uid}`);
  };

  const handleViewJourneyPlan = (uid: string) => {
    router.push(`/updatedfeatures/journey-plan-management/journey-plans/view/${uid}`);
  };

  const handleCreateJourneyPlan = () => {
    router.push('/updatedfeatures/journey-plan-management/journey-plans/create');
  };

  const handleSearch = (value: string) => {
    setFilters(prev => ({ ...prev, search: value }));
    setPagination(prev => ({ ...prev, current: 1 }));
  };

  const handleFilterChange = (key: keyof FilterState, value: string) => {
    setFilters(prev => ({ ...prev, [key]: value }));
    setPagination(prev => ({ ...prev, current: 1 }));
  };

  const handleRefresh = () => {
    fetchBeatHistories();
  };

  const getStatusVariant = (status: string): "default" | "secondary" | "destructive" | "outline" => {
    switch (status?.toLowerCase()) {
      case 'completed': return 'default';
      case 'in_progress': case 'started': return 'secondary';
      case 'pending': return 'outline';
      case 'cancelled': case 'skipped': return 'destructive';
      default: return 'outline';
    }
  };

  const TableSkeleton = () => (
    <>
      {[...Array(5)].map((_, index) => (
        <TableRow key={index}>
          <TableCell><Skeleton className="h-16 w-full" /></TableCell>
          <TableCell><Skeleton className="h-16 w-full" /></TableCell>
          <TableCell><Skeleton className="h-16 w-full" /></TableCell>
          <TableCell><Skeleton className="h-16 w-full" /></TableCell>
          <TableCell><Skeleton className="h-16 w-full" /></TableCell>
          <TableCell><Skeleton className="h-16 w-full" /></TableCell>
          <TableCell><Skeleton className="h-8 w-20" /></TableCell>
        </TableRow>
      ))}
    </>
  );

  return (
    <div className="container mx-auto py-6 space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div className="space-y-1">
          <h1 className="text-2xl font-bold">Beat History Management</h1>
          <p className="text-muted-foreground">
            Monitor and manage journey plan execution history
          </p>
        </div>
        <div className="flex gap-2">
          <Button 
            variant="outline"
            onClick={() => router.push('/updatedfeatures/journey-plan-management/bulk-management')}
          >
            <Settings className="mr-2 h-4 w-4" />
            Bulk Management
          </Button>
          <Button onClick={handleCreateJourneyPlan}>
            <Plus className="mr-2 h-4 w-4" />
            Create Journey Plan
          </Button>
        </div>
      </div>

      {/* Filters */}
      <Card>
        <CardContent className="p-6">
          <div className="grid grid-cols-1 md:grid-cols-5 gap-4">
            <div className="relative">
              <Search className="absolute left-2 top-2.5 h-4 w-4 text-muted-foreground" />
              <Input
                placeholder="Search by employee..."
                value={filters.search}
                onChange={(e) => handleSearch(e.target.value)}
                className="pl-8"
              />
            </div>

            <Select
              value={filters.orgUID}
              onValueChange={(value) => handleFilterChange('orgUID', value)}
            >
              <SelectTrigger>
                <SelectValue placeholder="Select Organization" />
              </SelectTrigger>
              <SelectContent>
                {organizations.map(org => (
                  <SelectItem key={org.value} value={org.value}>
                    {org.label}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>

            <Select
              value={filters.status}
              onValueChange={(value) => handleFilterChange('status', value === 'all' ? '' : value)}
            >
              <SelectTrigger>
                <SelectValue placeholder="Status" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All Status</SelectItem>
                <SelectItem value="Pending">Pending</SelectItem>
                <SelectItem value="In Progress">In Progress</SelectItem>
                <SelectItem value="Completed">Completed</SelectItem>
                <SelectItem value="Cancelled">Cancelled</SelectItem>
              </SelectContent>
            </Select>

            <Select
              value={filters.dateRange}
              onValueChange={(value) => handleFilterChange('dateRange', value)}
            >
              <SelectTrigger>
                <SelectValue placeholder="Date Range" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="today">Today</SelectItem>
                <SelectItem value="week">This Week</SelectItem>
                <SelectItem value="month">This Month</SelectItem>
              </SelectContent>
            </Select>

            <Button
              variant="outline"
              onClick={handleRefresh}
              disabled={loading}
            >
              <RefreshCw className={`h-4 w-4 ${loading ? 'animate-spin' : ''}`} />
            </Button>
          </div>
        </CardContent>
      </Card>

      {/* Beat History Table */}
      <Card>
        <CardContent className="p-0">
          <div className="overflow-x-auto">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Employee & Route</TableHead>
                  <TableHead>Visit Date</TableHead>
                  <TableHead>Store Coverage</TableHead>
                  <TableHead>Schedule</TableHead>
                  <TableHead>Performance</TableHead>
                  <TableHead>Status</TableHead>
                  <TableHead className="text-right">Actions</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {loading ? (
                  <TableSkeleton />
                ) : beatHistories.length === 0 ? (
                  <TableRow>
                    <TableCell colSpan={7} className="text-center py-10">
                      <div className="text-muted-foreground">
                        No beat histories found for the selected filters.
                      </div>
                    </TableCell>
                  </TableRow>
                ) : (
                  beatHistories.map((history) => (
                    <TableRow key={history.UID}>
                      <TableCell>
                        <div className="space-y-1">
                          <div className="flex items-center gap-2">
                            <Users className="h-4 w-4" />
                            <span className="font-medium">{history.EmployeeName}</span>
                          </div>
                          <div className="text-sm text-muted-foreground">
                            {history.LoginId}
                          </div>
                          <div className="flex items-center gap-2 text-sm">
                            <Route className="h-3 w-3" />
                            <span>{history.RouteName}</span>
                          </div>
                        </div>
                      </TableCell>
                      <TableCell>
                        <div className="space-y-1">
                          <div className="flex items-center gap-2">
                            <Calendar className="h-4 w-4" />
                            <span>{formatDateToDayMonthYear(history.VisitDate)}</span>
                          </div>
                          <div className="text-sm text-muted-foreground">
                            {formatDayOfWeek(history.VisitDate)}
                          </div>
                          <div className="flex items-center gap-2 flex-wrap">
                            {history.IsHoliday && (
                              <Badge variant="destructive" className="text-xs">
                                Holiday: {history.HolidayName || 'Holiday'}
                              </Badge>
                            )}
                            {!history.IsWorkingDay && !history.IsHoliday && (
                              <Badge variant="secondary" className="text-xs">
                                Off Day
                              </Badge>
                            )}
                            {history.ScheduleType && (
                              <Badge variant="outline" className="text-xs">
                                {history.ScheduleType}
                              </Badge>
                            )}
                          </div>
                        </div>
                      </TableCell>
                      <TableCell>
                        <div className="space-y-2">
                          <Progress value={history.Coverage} className="h-2" />
                          <div className="flex justify-between text-xs">
                            <span>Planned: {history.PlannedStoreVisits}</span>
                            <span>Actual: {history.ActualStoreVisits}</span>
                          </div>
                          {history.SkippedStoreVisits > 0 && (
                            <div className="text-xs text-red-600">
                              Skipped: {history.SkippedStoreVisits}
                            </div>
                          )}
                        </div>
                      </TableCell>
                      <TableCell>
                        <div className="space-y-1 text-sm">
                          <div className="flex items-center gap-1">
                            <Clock className="h-3 w-3" />
                            <span>
                              {history.PlannedStartTime || '09:00'} - {history.PlannedEndTime || '18:00'}
                            </span>
                          </div>
                          {history.StartTime && (
                            <div className="text-muted-foreground">
                              Actual: {formatTime(history.StartTime)} - {history.EndTime ? formatTime(history.EndTime) : 'In Progress'}
                            </div>
                          )}
                        </div>
                      </TableCell>
                      <TableCell>
                        <div className="space-y-1 text-sm">
                          <div>Coverage: {history.Coverage}%</div>
                          <div>A-Coverage: {history.ACoverage}%</div>
                          <div>T-Coverage: {history.TCoverage}%</div>
                        </div>
                      </TableCell>
                      <TableCell>
                        <div className="space-y-1">
                          <Badge variant={getStatusVariant(history.Status)}>
                            {history.Status}
                          </Badge>
                          {history.InvoiceStatus && (
                            <div className="text-xs text-muted-foreground">
                              Invoice: {history.InvoiceStatus}
                            </div>
                          )}
                        </div>
                      </TableCell>
                      <TableCell className="text-right">
                        <DropdownMenu>
                          <DropdownMenuTrigger asChild>
                            <Button variant="ghost" size="icon">
                              <MoreHorizontal className="h-4 w-4" />
                            </Button>
                          </DropdownMenuTrigger>
                          <DropdownMenuContent align="end">
                            <DropdownMenuItem onClick={() => handleViewBeatHistory(history)}>
                              <Eye className="mr-2 h-4 w-4" />
                              View Details
                            </DropdownMenuItem>
                            <DropdownMenuItem onClick={() => handleViewJourneyPlan(history.UID)}>
                              <FileText className="mr-2 h-4 w-4" />
                              View Journey Plan
                            </DropdownMenuItem>
                            <DropdownMenuItem onClick={() => handleEditBeatHistory(history.UID)}>
                              <Edit className="mr-2 h-4 w-4" />
                              Edit Journey Plan
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

          {/* Pagination */}
          {beatHistories.length > 0 && (
            <div className="flex items-center justify-between p-4 border-t">
              <div className="text-sm text-muted-foreground">
                Showing {((pagination.current - 1) * pagination.pageSize) + 1} to{' '}
                {Math.min(pagination.current * pagination.pageSize, pagination.total)} of{' '}
                {pagination.total} results
              </div>
              <div className="flex gap-2">
                <Button
                  variant="outline"
                  size="sm"
                  onClick={() => setPagination(prev => ({ ...prev, current: prev.current - 1 }))}
                  disabled={pagination.current === 1}
                >
                  Previous
                </Button>
                <Button
                  variant="outline"
                  size="sm"
                  onClick={() => setPagination(prev => ({ ...prev, current: prev.current + 1 }))}
                  disabled={pagination.current * pagination.pageSize >= pagination.total}
                >
                  Next
                </Button>
              </div>
            </div>
          )}
        </CardContent>
      </Card>

      {/* Beat History Details Dialog */}
      <Dialog open={detailsOpen} onOpenChange={setDetailsOpen}>
        <DialogContent className="max-w-4xl max-h-[80vh] overflow-y-auto">
          <DialogHeader>
            <DialogTitle className="flex items-center gap-2">
              <Route className="h-5 w-5" />
              Beat History Details
            </DialogTitle>
            <DialogDescription>
              {selectedBeatHistory && (
                <>
                  {selectedBeatHistory.EmployeeName} - {selectedBeatHistory.RouteName} - {formatDateToDayMonthYear(selectedBeatHistory.VisitDate)}
                </>
              )}
            </DialogDescription>
          </DialogHeader>

          {selectedBeatHistory && (
            <div className="space-y-6">
              {/* Summary */}
              <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
                <Card>
                  <CardContent className="p-4 text-center">
                    <div className="text-2xl font-bold">{selectedBeatHistory.PlannedStoreVisits}</div>
                    <div className="text-sm text-muted-foreground">Planned</div>
                  </CardContent>
                </Card>
                <Card>
                  <CardContent className="p-4 text-center">
                    <div className="text-2xl font-bold text-green-600">{selectedBeatHistory.ActualStoreVisits}</div>
                    <div className="text-sm text-muted-foreground">Completed</div>
                  </CardContent>
                </Card>
                <Card>
                  <CardContent className="p-4 text-center">
                    <div className="text-2xl font-bold text-red-600">{selectedBeatHistory.SkippedStoreVisits}</div>
                    <div className="text-sm text-muted-foreground">Skipped</div>
                  </CardContent>
                </Card>
                <Card>
                  <CardContent className="p-4 text-center">
                    <div className="text-2xl font-bold text-blue-600">{selectedBeatHistory.Coverage}%</div>
                    <div className="text-sm text-muted-foreground">Coverage</div>
                  </CardContent>
                </Card>
              </div>

              {/* Store Visits */}
              <div>
                <h3 className="font-semibold mb-4 flex items-center gap-2">
                  <MapPin className="h-4 w-4" />
                  Store Visits ({storeVisits.length})
                </h3>
                
                {detailsLoading ? (
                  <div className="space-y-2">
                    {[...Array(3)].map((_, i) => (
                      <Skeleton key={i} className="h-16 w-full" />
                    ))}
                  </div>
                ) : storeVisits.length === 0 ? (
                  <div className="text-center py-8 text-muted-foreground">
                    No store visits found
                  </div>
                ) : (
                  <div className="space-y-3 max-h-96 overflow-y-auto">
                    {storeVisits.map((visit) => (
                      <div
                        key={visit.UID}
                        className="border rounded-lg p-4 hover:bg-muted/50 transition-colors"
                      >
                        <div className="flex items-start justify-between">
                          <div className="flex-1">
                            <div className="flex items-center gap-2 mb-2">
                              <span className="font-medium text-sm bg-primary/10 text-primary px-2 py-1 rounded">
                                #{visit.SerialNo}
                              </span>
                              <h4 className="font-medium">{visit.StoreName}</h4>
                              <Badge variant={getStatusVariant(visit.Status)}>
                                {visit.Status}
                              </Badge>
                            </div>
                            
                            {visit.StoreCode && (
                              <div className="text-sm text-muted-foreground mb-2">
                                Code: {visit.StoreCode}
                              </div>
                            )}

                            <div className="grid grid-cols-2 gap-4 text-sm">
                              <div>
                                <span className="text-muted-foreground">Visit Time:</span>
                                <div>
                                  {visit.LoginTime ? formatTime(visit.LoginTime) : '-'} - {visit.LogoutTime ? formatTime(visit.LogoutTime) : '-'}
                                </div>
                              </div>
                              <div>
                                <span className="text-muted-foreground">Duration:</span>
                                <div>{visit.VisitDuration} mins</div>
                              </div>
                              <div>
                                <span className="text-muted-foreground">Target:</span>
                                <div>₹{visit.TargetValue}</div>
                              </div>
                              <div>
                                <span className="text-muted-foreground">Actual:</span>
                                <div>₹{visit.ActualValue}</div>
                              </div>
                            </div>
                          </div>

                          <div className="flex flex-col items-end gap-2">
                            {visit.IsProductive && (
                              <Badge variant="default" className="text-xs">
                                Productive
                              </Badge>
                            )}
                            {visit.IsSkipped && (
                              <Badge variant="destructive" className="text-xs">
                                Skipped
                              </Badge>
                            )}
                          </div>
                        </div>
                      </div>
                    ))}
                  </div>
                )}
              </div>
            </div>
          )}
        </DialogContent>
      </Dialog>
    </div>
  );
};

export default BeatHistoryManagement;