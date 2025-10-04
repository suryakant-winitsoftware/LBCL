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
import { Checkbox } from '@/components/ui/checkbox';
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
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from '@/components/ui/alert-dialog';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { Textarea } from '@/components/ui/textarea';
import {
  Search,
  RefreshCw,
  CheckSquare,
  Square,
  Users,
  Calendar,
  Route,
  PlayCircle,
  PauseCircle,
  XCircle,
  Copy,
  Trash2,
  Edit3,
  MoreHorizontal,
  ChevronDown,
  Clock,
  MapPin,
  Filter,
  Download,
  Upload,
  Settings,
} from 'lucide-react';
import moment from 'moment';
import { formatDateToDayMonthYear, formatTime, formatDayOfWeek } from '@/utils/date-formatter';
import { api } from '@/services/api';

interface JourneyPlan {
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
  PlannedStoreVisits: number;
  ActualStoreVisits: number;
  SkippedStoreVisits: number;
  Coverage: number;
  Status: string;
  InvoiceStatus: string;
  StartTime?: string;
  EndTime?: string;
  PlannedStartTime?: string;
  PlannedEndTime?: string;
  Notes?: string;
  CreatedTime: string;
  ModifiedTime: string;
}

interface BulkOperation {
  type: 'start' | 'pause' | 'complete' | 'cancel' | 'reschedule' | 'reassign' | 'duplicate' | 'delete';
  label: string;
  icon: React.ReactNode;
  description: string;
  requiresInput?: boolean;
  inputType?: 'date' | 'employee' | 'notes';
  inputLabel?: string;
  variant?: 'default' | 'destructive';
}

interface FilterState {
  search: string;
  status: string;
  dateRange: string;
  orgUID: string;
  employeeUID: string;
  routeUID: string;
}

interface PaginationState {
  current: number;
  pageSize: number;
  total: number;
}

const BulkJourneyManagement: React.FC = () => {
  const router = useRouter();
  const { toast } = useToast();
  
  const [journeyPlans, setJourneyPlans] = useState<JourneyPlan[]>([]);
  const [loading, setLoading] = useState(true);
  const [processing, setProcessing] = useState(false);
  const [selectedPlans, setSelectedPlans] = useState<string[]>([]);
  const [organizations, setOrganizations] = useState<{ value: string; label: string }[]>([]);
  const [employees, setEmployees] = useState<{ value: string; label: string }[]>([]);
  const [routes, setRoutes] = useState<{ value: string; label: string }[]>([]);
  
  // Dialog states
  const [operationDialogOpen, setOperationDialogOpen] = useState(false);
  const [confirmDialogOpen, setConfirmDialogOpen] = useState(false);
  const [selectedOperation, setSelectedOperation] = useState<BulkOperation | null>(null);
  const [operationInput, setOperationInput] = useState('');
  const [operationNotes, setOperationNotes] = useState('');

  const [pagination, setPagination] = useState<PaginationState>({
    current: 1,
    pageSize: 20,
    total: 0,
  });

  const [filters, setFilters] = useState<FilterState>({
    search: '',
    status: '',
    dateRange: 'thisweek',
    orgUID: 'Farmley',
    employeeUID: '',
    routeUID: '',
  });

  const bulkOperations: BulkOperation[] = [
    {
      type: 'start',
      label: 'Start Journeys',
      icon: <PlayCircle className="h-4 w-4" />,
      description: 'Start selected journey plans',
    },
    {
      type: 'pause',
      label: 'Pause Journeys',
      icon: <PauseCircle className="h-4 w-4" />,
      description: 'Pause selected journey plans',
    },
    {
      type: 'complete',
      label: 'Mark Complete',
      icon: <CheckSquare className="h-4 w-4" />,
      description: 'Mark selected journeys as completed',
    },
    {
      type: 'cancel',
      label: 'Cancel Journeys',
      icon: <XCircle className="h-4 w-4" />,
      description: 'Cancel selected journey plans',
      variant: 'destructive',
      requiresInput: true,
      inputType: 'notes',
      inputLabel: 'Cancellation reason',
    },
    {
      type: 'reschedule',
      label: 'Reschedule',
      icon: <Calendar className="h-4 w-4" />,
      description: 'Reschedule selected journeys to a new date',
      requiresInput: true,
      inputType: 'date',
      inputLabel: 'New visit date',
    },
    {
      type: 'reassign',
      label: 'Reassign Employee',
      icon: <Users className="h-4 w-4" />,
      description: 'Reassign selected journeys to a different employee',
      requiresInput: true,
      inputType: 'employee',
      inputLabel: 'New employee',
    },
    {
      type: 'duplicate',
      label: 'Duplicate',
      icon: <Copy className="h-4 w-4" />,
      description: 'Create copies of selected journey plans',
      requiresInput: true,
      inputType: 'date',
      inputLabel: 'Target date for duplicates',
    },
    {
      type: 'delete',
      label: 'Delete',
      icon: <Trash2 className="h-4 w-4" />,
      description: 'Permanently delete selected journey plans',
      variant: 'destructive',
      requiresInput: true,
      inputType: 'notes',
      inputLabel: 'Deletion reason (optional)',
    },
  ];

  useEffect(() => {
    loadInitialData();
  }, []);

  useEffect(() => {
    fetchJourneyPlans();
  }, [pagination.current, pagination.pageSize, filters]);

  const loadInitialData = async () => {
    try {
      // Load organizations
      const orgData = await api.org.getDetails({
        pageNumber: 0,
        pageSize: 100,
        isCountRequired: false,
        sortCriterias: [],
        filterCriterias: []
      });
      
      if (orgData.IsSuccess && orgData.Data?.PagedData) {
        setOrganizations(
          orgData.Data.PagedData.map((org: any) => ({
            value: org.UID,
            label: org.Name || org.Code,
          }))
        );
      }

      // Load employees for selected org
      if (filters.orgUID) {
        const empData = await api.dropdown.getEmployee(filters.orgUID, false);
        if (empData?.IsSuccess && empData?.Data) {
          setEmployees(
            empData.Data.map((emp: any) => ({
              value: emp.UID || emp.Value,
              label: emp.Label || emp.Name,
            }))
          );
        }

        // Load routes for selected org
        const routeData = await api.dropdown.getRoute(filters.orgUID);
        if (routeData?.IsSuccess && routeData?.Data) {
          setRoutes(
            routeData.Data.map((route: any) => ({
              value: route.UID || route.Value,
              label: route.Label || route.Name,
            }))
          );
        }
      }
    } catch (error) {
      console.error('Error loading initial data:', error);
    }
  };

  const fetchJourneyPlans = async () => {
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

      if (filters.employeeUID) {
        filterCriterias.push({
          filterBy: 'EmpUID',
          filterValue: filters.employeeUID,
          filterOperator: 'equals'
        });
      }

      if (filters.routeUID) {
        filterCriterias.push({
          filterBy: 'RouteUID',
          filterValue: filters.routeUID,
          filterOperator: 'equals'
        });
      }

      // Add date filter
      const today = moment();
      let startDate, endDate;
      
      switch (filters.dateRange) {
        case 'today':
          startDate = today.format('YYYY-MM-DD');
          endDate = today.format('YYYY-MM-DD');
          break;
        case 'thisweek':
          startDate = today.startOf('week').format('YYYY-MM-DD');
          endDate = today.endOf('week').format('YYYY-MM-DD');
          break;
        case 'nextweek':
          const nextWeek = moment().add(1, 'week');
          startDate = nextWeek.startOf('week').format('YYYY-MM-DD');
          endDate = nextWeek.endOf('week').format('YYYY-MM-DD');
          break;
        case 'thismonth':
          startDate = today.startOf('month').format('YYYY-MM-DD');
          endDate = today.endOf('month').format('YYYY-MM-DD');
          break;
        default:
          startDate = today.startOf('week').format('YYYY-MM-DD');
          endDate = today.endOf('week').format('YYYY-MM-DD');
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
          sortOrder: 'asc'
        }],
        filterCriterias
      };

      const response = await api.beatHistory.selectAll(request);
      
      if (response.IsSuccess && response.Data) {
        const plans = (response.Data.PagedData || []).map((item: any) => ({
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
          PlannedStoreVisits: item.PlannedStoreVisits || 0,
          ActualStoreVisits: item.ActualStoreVisits || 0,
          SkippedStoreVisits: item.SkippedStoreVisits || 0,
          Coverage: item.Coverage || 0,
          Status: item.Status || 'Pending',
          InvoiceStatus: item.InvoiceStatus || 'Pending',
          StartTime: item.StartTime,
          EndTime: item.EndTime,
          PlannedStartTime: item.PlannedStartTime,
          PlannedEndTime: item.PlannedEndTime,
          Notes: item.Notes,
          CreatedTime: item.CreatedTime,
          ModifiedTime: item.ModifiedTime,
        }));
        
        setJourneyPlans(plans);
        setPagination(prev => ({
          ...prev,
          total: response.Data.TotalCount || 0,
        }));
      } else {
        setJourneyPlans([]);
        setPagination(prev => ({ ...prev, total: 0 }));
      }
    } catch (error) {
      console.error('Error fetching journey plans:', error);
      toast({
        title: "Error",
        description: "Failed to fetch journey plans. Please try again.",
        variant: "destructive",
      });
    } finally {
      setLoading(false);
    }
  };

  const handleSelectAll = () => {
    if (selectedPlans.length === journeyPlans.length) {
      setSelectedPlans([]);
    } else {
      setSelectedPlans(journeyPlans.map(plan => plan.UID));
    }
  };

  const handleSelectPlan = (planUID: string) => {
    setSelectedPlans(prev => 
      prev.includes(planUID)
        ? prev.filter(id => id !== planUID)
        : [...prev, planUID]
    );
  };

  const handleBulkOperation = (operation: BulkOperation) => {
    if (selectedPlans.length === 0) {
      toast({
        title: "No Selection",
        description: "Please select at least one journey plan to perform bulk operations.",
        variant: "destructive",
      });
      return;
    }

    setSelectedOperation(operation);
    setOperationInput('');
    setOperationNotes('');
    
    if (operation.requiresInput) {
      setOperationDialogOpen(true);
    } else {
      setConfirmDialogOpen(true);
    }
  };

  const handleOperationConfirm = async () => {
    if (!selectedOperation) return;
    
    try {
      setProcessing(true);
      
      // Simulate API call for bulk operation
      await new Promise(resolve => setTimeout(resolve, 2000));
      
      const operationMessages = {
        start: 'Journey plans started successfully',
        pause: 'Journey plans paused successfully',
        complete: 'Journey plans marked as completed',
        cancel: 'Journey plans cancelled successfully',
        reschedule: `Journey plans rescheduled to ${operationInput}`,
        reassign: 'Journey plans reassigned successfully',
        duplicate: `${selectedPlans.length} journey plans duplicated for ${operationInput}`,
        delete: 'Journey plans deleted successfully',
      };

      toast({
        title: "Success",
        description: operationMessages[selectedOperation.type],
      });

      // Clear selection and refresh data
      setSelectedPlans([]);
      fetchJourneyPlans();
      
    } catch (error) {
      toast({
        title: "Error",
        description: `Failed to ${selectedOperation.label.toLowerCase()}`,
        variant: "destructive",
      });
    } finally {
      setProcessing(false);
      setOperationDialogOpen(false);
      setConfirmDialogOpen(false);
      setSelectedOperation(null);
    }
  };

  const handleFilterChange = (key: keyof FilterState, value: string) => {
    setFilters(prev => ({ ...prev, [key]: value }));
    setPagination(prev => ({ ...prev, current: 1 }));
    setSelectedPlans([]); // Clear selection when filters change
  };

  const handleSearch = (value: string) => {
    setFilters(prev => ({ ...prev, search: value }));
    setPagination(prev => ({ ...prev, current: 1 }));
    setSelectedPlans([]);
  };

  const handleRefresh = () => {
    fetchJourneyPlans();
    setSelectedPlans([]);
  };

  const getStatusVariant = (status: string): "default" | "secondary" | "destructive" | "outline" => {
    switch (status?.toLowerCase()) {
      case 'completed': return 'default';
      case 'in_progress': case 'started': return 'secondary';
      case 'pending': return 'outline';
      case 'cancelled': case 'paused': return 'destructive';
      default: return 'outline';
    }
  };

  const renderInputField = () => {
    if (!selectedOperation || !selectedOperation.requiresInput) return null;

    switch (selectedOperation.inputType) {
      case 'date':
        return (
          <Input
            type="date"
            value={operationInput}
            onChange={(e) => setOperationInput(e.target.value)}
            min={moment().format('YYYY-MM-DD')}
          />
        );
      case 'employee':
        return (
          <Select value={operationInput} onValueChange={setOperationInput}>
            <SelectTrigger>
              <SelectValue placeholder="Select employee" />
            </SelectTrigger>
            <SelectContent>
              {employees.map(emp => (
                <SelectItem key={emp.value} value={emp.value}>
                  {emp.label}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        );
      case 'notes':
        return (
          <Textarea
            placeholder={`Enter ${selectedOperation.inputLabel?.toLowerCase()}...`}
            value={operationNotes}
            onChange={(e) => setOperationNotes(e.target.value)}
            rows={3}
          />
        );
      default:
        return (
          <Input
            placeholder={selectedOperation.inputLabel}
            value={operationInput}
            onChange={(e) => setOperationInput(e.target.value)}
          />
        );
    }
  };

  const TableSkeleton = () => (
    <>
      {[...Array(5)].map((_, index) => (
        <TableRow key={index}>
          <TableCell><Skeleton className="h-4 w-4" /></TableCell>
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
          <h1 className="text-2xl font-bold flex items-center gap-2">
            <Settings className="h-6 w-6" />
            Bulk Journey Management
          </h1>
          <p className="text-muted-foreground">
            Perform bulk operations on multiple journey plans simultaneously
          </p>
        </div>
        <div className="flex items-center gap-2">
          <Badge variant="secondary" className="px-3 py-1">
            {selectedPlans.length} selected
          </Badge>
          <Button 
            variant="outline" 
            onClick={() => router.push('/updatedfeatures/journey-plan-management/journey-plans/manage')}
          >
            <Edit3 className="mr-2 h-4 w-4" />
            Regular Management
          </Button>
        </div>
      </div>

      {/* Bulk Actions Bar */}
      {selectedPlans.length > 0 && (
        <Card className="border-blue-200 bg-blue-50">
          <CardContent className="p-4">
            <div className="flex items-center justify-between">
              <div className="flex items-center gap-2">
                <CheckSquare className="h-5 w-5 text-blue-600" />
                <span className="font-medium text-blue-900">
                  {selectedPlans.length} journey plan{selectedPlans.length > 1 ? 's' : ''} selected
                </span>
              </div>
              <div className="flex items-center gap-2">
                <DropdownMenu>
                  <DropdownMenuTrigger asChild>
                    <Button variant="outline" className="flex items-center gap-2">
                      Bulk Actions
                      <ChevronDown className="h-4 w-4" />
                    </Button>
                  </DropdownMenuTrigger>
                  <DropdownMenuContent align="end" className="w-56">
                    {bulkOperations.map((operation, index) => (
                      <React.Fragment key={operation.type}>
                        <DropdownMenuItem
                          onClick={() => handleBulkOperation(operation)}
                          className={operation.variant === 'destructive' ? 'text-red-600' : ''}
                        >
                          {operation.icon}
                          <span className="ml-2">{operation.label}</span>
                        </DropdownMenuItem>
                        {(index === 3 || index === 6) && <DropdownMenuSeparator />}
                      </React.Fragment>
                    ))}
                  </DropdownMenuContent>
                </DropdownMenu>
                <Button variant="outline" size="sm" onClick={() => setSelectedPlans([])}>
                  Clear Selection
                </Button>
              </div>
            </div>
          </CardContent>
        </Card>
      )}

      {/* Filters */}
      <Card>
        <CardContent className="p-6">
          <div className="grid grid-cols-1 md:grid-cols-6 gap-4">
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
                <SelectValue placeholder="Organization" />
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
                <SelectItem value="thisweek">This Week</SelectItem>
                <SelectItem value="nextweek">Next Week</SelectItem>
                <SelectItem value="thismonth">This Month</SelectItem>
              </SelectContent>
            </Select>

            <Select
              value={filters.employeeUID}
              onValueChange={(value) => handleFilterChange('employeeUID', value === 'all' ? '' : value)}
            >
              <SelectTrigger>
                <SelectValue placeholder="Employee" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All Employees</SelectItem>
                {employees.map(emp => (
                  <SelectItem key={emp.value} value={emp.value}>
                    {emp.label}
                  </SelectItem>
                ))}
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

      {/* Journey Plans Table */}
      <Card>
        <CardContent className="p-0">
          <div className="overflow-x-auto">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead className="w-12">
                    <Checkbox
                      checked={journeyPlans.length > 0 && selectedPlans.length === journeyPlans.length}
                      onCheckedChange={handleSelectAll}
                      disabled={journeyPlans.length === 0}
                    />
                  </TableHead>
                  <TableHead>Employee & Route</TableHead>
                  <TableHead>Visit Date</TableHead>
                  <TableHead>Store Coverage</TableHead>
                  <TableHead>Schedule</TableHead>
                  <TableHead>Status</TableHead>
                  <TableHead className="text-right">Actions</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {loading ? (
                  <TableSkeleton />
                ) : journeyPlans.length === 0 ? (
                  <TableRow>
                    <TableCell colSpan={7} className="text-center py-10">
                      <div className="text-muted-foreground">
                        No journey plans found for the selected filters.
                      </div>
                    </TableCell>
                  </TableRow>
                ) : (
                  journeyPlans.map((plan) => (
                    <TableRow key={plan.UID} className={selectedPlans.includes(plan.UID) ? 'bg-blue-50' : ''}>
                      <TableCell>
                        <Checkbox
                          checked={selectedPlans.includes(plan.UID)}
                          onCheckedChange={() => handleSelectPlan(plan.UID)}
                        />
                      </TableCell>
                      <TableCell>
                        <div className="space-y-1">
                          <div className="flex items-center gap-2">
                            <Users className="h-4 w-4" />
                            <span className="font-medium">{plan.EmployeeName}</span>
                          </div>
                          <div className="text-sm text-muted-foreground">
                            {plan.LoginId}
                          </div>
                          <div className="flex items-center gap-2 text-sm">
                            <Route className="h-3 w-3" />
                            <span>{plan.RouteName}</span>
                          </div>
                        </div>
                      </TableCell>
                      <TableCell>
                        <div className="flex items-center gap-2">
                          <Calendar className="h-4 w-4" />
                          <span>{formatDateToDayMonthYear(plan.VisitDate)}</span>
                        </div>
                        <div className="text-sm text-muted-foreground">
                          {formatDayOfWeek(plan.VisitDate)}
                        </div>
                      </TableCell>
                      <TableCell>
                        <div className="space-y-2">
                          <Progress value={plan.Coverage} className="h-2" />
                          <div className="flex justify-between text-xs">
                            <span>Planned: {plan.PlannedStoreVisits}</span>
                            <span>Actual: {plan.ActualStoreVisits}</span>
                          </div>
                          {plan.SkippedStoreVisits > 0 && (
                            <div className="text-xs text-red-600">
                              Skipped: {plan.SkippedStoreVisits}
                            </div>
                          )}
                        </div>
                      </TableCell>
                      <TableCell>
                        <div className="space-y-1 text-sm">
                          <div className="flex items-center gap-1">
                            <Clock className="h-3 w-3" />
                            <span>
                              {plan.PlannedStartTime || '09:00'} - {plan.PlannedEndTime || '18:00'}
                            </span>
                          </div>
                          {plan.StartTime && (
                            <div className="text-muted-foreground">
                              Actual: {formatTime(plan.StartTime)} - {plan.EndTime ? formatTime(plan.EndTime) : 'In Progress'}
                            </div>
                          )}
                        </div>
                      </TableCell>
                      <TableCell>
                        <div className="space-y-1">
                          <Badge variant={getStatusVariant(plan.Status)}>
                            {plan.Status}
                          </Badge>
                          {plan.InvoiceStatus && (
                            <div className="text-xs text-muted-foreground">
                              Invoice: {plan.InvoiceStatus}
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
                            <DropdownMenuItem 
                              onClick={() => router.push(`/updatedfeatures/journey-plan-management/journey-plans/view/${plan.UID}`)}
                            >
                              <MapPin className="mr-2 h-4 w-4" />
                              View Details
                            </DropdownMenuItem>
                            <DropdownMenuItem 
                              onClick={() => router.push(`/updatedfeatures/journey-plan-management/journey-plans/edit/${plan.UID}`)}
                            >
                              <Edit3 className="mr-2 h-4 w-4" />
                              Edit Plan
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
          {journeyPlans.length > 0 && (
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

      {/* Operation Input Dialog */}
      <Dialog open={operationDialogOpen} onOpenChange={setOperationDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle className="flex items-center gap-2">
              {selectedOperation?.icon}
              {selectedOperation?.label}
            </DialogTitle>
            <DialogDescription>
              {selectedOperation?.description} for {selectedPlans.length} selected journey plan{selectedPlans.length > 1 ? 's' : ''}.
            </DialogDescription>
          </DialogHeader>
          
          <div className="space-y-4">
            {selectedOperation?.inputLabel && (
              <div className="space-y-2">
                <label className="text-sm font-medium">
                  {selectedOperation.inputLabel}
                </label>
                {renderInputField()}
              </div>
            )}
            
            <div className="space-y-2">
              <label className="text-sm font-medium">Additional Notes (Optional)</label>
              <Textarea
                placeholder="Enter any additional notes..."
                value={operationNotes}
                onChange={(e) => setOperationNotes(e.target.value)}
                rows={2}
              />
            </div>
          </div>

          <div className="flex justify-end gap-2">
            <Button variant="outline" onClick={() => setOperationDialogOpen(false)}>
              Cancel
            </Button>
            <Button 
              onClick={() => {
                setOperationDialogOpen(false);
                setConfirmDialogOpen(true);
              }}
              variant={selectedOperation?.variant === 'destructive' ? 'destructive' : 'default'}
              disabled={selectedOperation?.requiresInput && !operationInput && selectedOperation.inputType !== 'notes'}
            >
              Continue
            </Button>
          </div>
        </DialogContent>
      </Dialog>

      {/* Confirmation Dialog */}
      <AlertDialog open={confirmDialogOpen} onOpenChange={setConfirmDialogOpen}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Confirm Bulk Operation</AlertDialogTitle>
            <AlertDialogDescription>
              Are you sure you want to {selectedOperation?.label.toLowerCase()} {selectedPlans.length} journey plan{selectedPlans.length > 1 ? 's' : ''}?
              {selectedOperation?.type === 'delete' && ' This action cannot be undone.'}
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel disabled={processing}>Cancel</AlertDialogCancel>
            <AlertDialogAction 
              onClick={handleOperationConfirm}
              disabled={processing}
              className={selectedOperation?.variant === 'destructive' ? 'bg-destructive text-destructive-foreground' : ''}
            >
              {processing ? (
                <>
                  <RefreshCw className="mr-2 h-4 w-4 animate-spin" />
                  Processing...
                </>
              ) : (
                <>
                  {selectedOperation?.icon}
                  <span className="ml-2">Confirm</span>
                </>
              )}
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  );
};

export default BulkJourneyManagement;