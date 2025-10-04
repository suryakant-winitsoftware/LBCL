"use client";

import { useState, useEffect, useCallback, useMemo, useRef } from "react";
import {
  Eye,
  Edit
} from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { useToast } from "@/components/ui/use-toast";
import {
  DataGrid,
  DataGridColumn,
  DataGridAction
} from "@/components/ui/data-grid";
import {
  UserJourney,
  PagedRequest,
  journeyPlanService
} from "@/services/journeyPlanService";
import { api } from "@/services/api";
import { authService } from "@/lib/auth-service";
import moment from "moment";
import { formatDateToDayMonthYear } from "@/utils/date-formatter";

interface JourneyPlanGridProps {
  searchQuery: string;
  filters: {
    jobPositionUID: string;
    empUID: string;
    eotStatus: string;
    attendanceStatus: string;
    visitDate: Date;
    orgUID: string;
  };
  refreshTrigger: number;
  onView: (journeyId: string) => void;
  onEdit: (journeyId: string) => void;
  onComplete: (journeyId: string) => void;
  onDelete: (journeyId: string) => void;
}

export function JourneyPlanGrid({
  searchQuery,
  filters,
  refreshTrigger,
  onView,
  onEdit,
  onComplete,
  onDelete
}: JourneyPlanGridProps) {
  const { toast } = useToast();
  const toastRef = useRef(toast);
  const [allData, setAllData] = useState<UserJourney[]>([]); // All data from backend
  const [loading, setLoading] = useState(true);

  // Pagination state
  const [pagination, setPagination] = useState({
    current: 1,
    pageSize: 10,
    total: 0
  });

  // Sorting state
  const [sorting, setSorting] = useState<{
    field?: string;
    direction?: "asc" | "desc";
  }>({});

  // Build filter criteria - use backend expected format
  const buildFilterCriteria = useCallback(() => {
    const criteria = [];

    if (searchQuery) {
      // Add multiple search criteria for different fields
      criteria.push({
        fieldName: "LoginId",
        operator: "LIKE", 
        value: `%${searchQuery}%`
      });
      criteria.push({
        fieldName: "RouteUID",
        operator: "LIKE", 
        value: `%${searchQuery}%`
      });
      criteria.push({
        fieldName: "JobPositionUID",
        operator: "LIKE", 
        value: `%${searchQuery}%`
      });
    }

    if (filters.eotStatus) {
      criteria.push({
        fieldName: "EOTStatus",
        operator: "EQ",
        value: filters.eotStatus
      });
    }

    // Handle date filtering - format the date properly
    if (filters.visitDate) {
      const dateStr = moment(filters.visitDate).format('YYYY-MM-DD');
      criteria.push({
        fieldName: "VisitDate",
        operator: "LIKE",
        value: `${dateStr}%` // Match dates starting with this date
      });
    }

    return criteria;
  }, [
    searchQuery,
    filters.eotStatus,
    filters.visitDate
  ]);

  // Update toast ref
  useEffect(() => {
    toastRef.current = toast;
  }, [toast]);

  // Load all data once and use client-side pagination
  const loadAllData = useCallback(async () => {
    try {
      setLoading(true);

      // Load all data with large page size for client-side pagination
      const request: PagedRequest = {
        pageNumber: 0,
        pageSize: 1000, // Load all data at once
        filterCriterias: [], // Start with empty filters
        isCountRequired: true
      };

      console.log('=== CLIENT-SIDE PAGINATION - LOADING ALL DATA ===');
      console.log('Request payload:', JSON.stringify(request, null, 2));

      // FIXED: Use BeatHistory API for journey plan data (planning data)
      // getUserJourneys was calling UserJourney API (execution data) - WRONG
      // getJourneyPlans calls BeatHistory API (planning data) - CORRECT  
      const response = await journeyPlanService.getJourneyPlans(request);

      if (response?.IsSuccess && response.Data?.PagedData) {
        console.log('=== CLIENT-SIDE PAGINATION - DATA LOADED ===');
        console.log('Total records received:', response.Data.PagedData.length);
        
        // Transform all data
        const transformedJourneys = response.Data.PagedData.map((journey: any) =>
          journeyPlanService.transformBeatHistoryToJourney(journey)
        );
        
        if (transformedJourneys.length > 0) {
          console.log('Sample RouteUID:', transformedJourneys[0].RouteUID);
        }

        // Store all data for client-side pagination
        setAllData(transformedJourneys);
        setPagination((prev) => ({
          ...prev,
          total: transformedJourneys.length
        }));
      } else {
        setAllData([]);
        setPagination((prev) => ({
          ...prev,
          total: 0
        }));
      }
    } catch (error) {
      console.error("=== API REQUEST FAILED ===");
      console.error("Error details:", error);
      console.error("Request was:", {
        pageNumber: pagination.current - 1,
        pageSize: pagination.pageSize,
        filterCriterias: [],
        isCountRequired: true
      });
      
      toastRef.current({
        title: "Error",
        description: "Failed to load journey plans. Please try again.",
        variant: "destructive"
      });
      setAllData([]);
      setPagination((prev) => ({
        ...prev,
        total: 0
      }));
    } finally {
      setLoading(false);
    }
  }, []); // Load all data once, no dependencies on pagination

  // Load data on mount and when refresh is triggered
  useEffect(() => {
    loadAllData();
  }, [loadAllData, refreshTrigger]);

  // Client-side filtering, sorting and pagination
  const paginatedData = useMemo(() => {
    let filteredData = [...allData];

    console.log('=== CLIENT-SIDE PAGINATION ===');
    console.log('All data count:', allData.length);
    console.log('Search query:', searchQuery);

    // Apply client-side filtering
    if (searchQuery) {
      const query = searchQuery.toLowerCase();
      filteredData = filteredData.filter(journey => 
        journey.LoginId?.toLowerCase().includes(query) ||
        journey.JobPositionUID?.toLowerCase().includes(query) ||
        journey.EmpUID?.toLowerCase().includes(query) ||
        journey.RouteUID?.toLowerCase().includes(query)
      );
      console.log('After search filter:', filteredData.length);
    }

    if (filters.eotStatus) {
      filteredData = filteredData.filter(journey => 
        journey.EOTStatus === filters.eotStatus
      );
    }

    if (filters.attendanceStatus) {
      filteredData = filteredData.filter(journey => 
        journey.AttendanceStatus === filters.attendanceStatus
      );
    }

    // Apply client-side sorting if needed
    if (sorting.field) {
      filteredData.sort((a, b) => {
        let aValue = a[sorting.field as keyof UserJourney];
        let bValue = b[sorting.field as keyof UserJourney];

        if (aValue == null && bValue == null) return 0;
        if (aValue == null) return sorting.direction === "asc" ? -1 : 1;
        if (bValue == null) return sorting.direction === "asc" ? 1 : -1;

        if (typeof aValue === "string") aValue = aValue.toLowerCase();
        if (typeof bValue === "string") bValue = bValue.toLowerCase();

        if (aValue < bValue) return sorting.direction === "asc" ? -1 : 1;
        if (aValue > bValue) return sorting.direction === "asc" ? 1 : -1;
        return 0;
      });
    }

    // Update pagination total with filtered count
    if (filteredData.length !== pagination.total) {
      setPagination(prev => ({
        ...prev,
        total: filteredData.length,
        current: Math.min(prev.current, Math.ceil(filteredData.length / prev.pageSize) || 1)
      }));
    }

    // Apply client-side pagination
    const startIndex = (pagination.current - 1) * pagination.pageSize;
    const endIndex = startIndex + pagination.pageSize;
    
    console.log(`Client-side pagination: showing ${startIndex}-${Math.min(endIndex-1, filteredData.length-1)} of ${filteredData.length} filtered records`);

    return filteredData.slice(startIndex, endIndex);
  }, [
    allData,
    searchQuery,
    filters.eotStatus,
    filters.attendanceStatus,
    pagination.current,
    pagination.pageSize,
    sorting.field,
    sorting.direction
  ]);

  // Handle pagination change - client-side only
  const handlePaginationChange = useCallback(
    (page: number, pageSize: number) => {
      console.log('=== CLIENT-SIDE PAGINATION CHANGE ===');
      console.log('New page:', page, 'New pageSize:', pageSize);
      setPagination((prev) => ({
        ...prev,
        current: page,
        pageSize
      }));
      // No data reload needed - client-side pagination will handle it
    },
    []
  );

  // Handle sorting change
  const handleSortingChange = useCallback(
    (field: string, direction: "asc" | "desc") => {
      setSorting({ field, direction });
      setPagination((prev) => ({ ...prev, current: 1 })); // Reset to first page
    },
    []
  );

  // Get status variant
  const getJourneyStatusVariant = (
    status: string
  ): "default" | "secondary" | "destructive" | "outline" => {
    switch (status?.toLowerCase()) {
      case "completed":
        return "default";
      case "in_progress":
        return "secondary";
      case "paused":
        return "outline";
      case "cancelled":
        return "destructive";
      default:
        return "outline";
    }
  };

  // Define columns - simplified like Today's Plans
  const columns: DataGridColumn<UserJourney>[] = useMemo(
    () => [
      {
        key: "CreatedTime",
        title: "Date",
        width: "100px",
        sortable: true,
        render: (value: string, record: UserJourney) => {
          // Try multiple date fields to find a valid date
          const dateValue = record.JourneyStartTime || record.CreatedTime || value;
          if (!dateValue) {
            console.warn('No date found for record:', record.UID, {
              JourneyStartTime: record.JourneyStartTime,
              CreatedTime: record.CreatedTime,
              value: value
            });
            return <span className="text-sm">-</span>;
          }
          
          const parsedDate = moment(dateValue);
          if (!parsedDate.isValid()) return <span className="text-sm">-</span>;
          
          return (
            <span className="text-sm">
              {formatDateToDayMonthYear(dateValue)}
            </span>
          );
        }
      },
      {
        key: "RouteUID",
        title: "Route",
        width: "120px",
        sortable: true,
        render: (value: string) => (
          <span className="text-sm">{value || "-"}</span>
        )
      },
      {
        key: "LoginId",
        title: "Employee ID",
        width: "120px",
        sortable: true,
        render: (value: string, record: UserJourney) => (
          <span className="text-sm">{value || record.EmpUID || "-"}</span>
        )
      },
      {
        key: "EOTStatus",
        title: "Status",
        width: "100px",
        sortable: true,
        render: (value: string) => (
          <Badge variant={getJourneyStatusVariant(value)} className="text-xs">
            {value || "Not Started"}
          </Badge>
        )
      }
    ],
    []
  );

  // Define actions
  const actions: DataGridAction<UserJourney>[] = useMemo(
    () => [
      {
        key: "view",
        label: "View",
        icon: Eye,
        onClick: (record) => onView(record.UID)
      },
      {
        key: "edit",
        label: "Edit",
        icon: Edit,
        onClick: (record) => onEdit(record.UID)
      }
    ],
    [onView, onEdit]
  );

  // Remove console.log for production

  return (
    <DataGrid
      data={paginatedData}
      columns={columns}
      loading={loading}
      pagination={{
        current: pagination.current,
        pageSize: pagination.pageSize,
        total: pagination.total,
        showSizeChanger: true,
        pageSizeOptions: [10, 20, 50, 100],
        onChange: handlePaginationChange
      }}
      sorting={{
        field: sorting.field,
        direction: sorting.direction,
        onChange: handleSortingChange
      }}
      actions={actions}
      emptyText="No user journeys found for the selected filters."
      className="min-h-[400px]"
    />
  );
}
