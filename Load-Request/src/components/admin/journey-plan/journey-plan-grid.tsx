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
      criteria.push({
        fieldName: "User",
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
        fieldName: "JourneyDate",
        operator: "LIKE",
        value: `${dateStr}%` // Match dates starting with this date
      });
    }

    // Note: orgUID and jobPositionUID filtering will be handled by the today plan endpoint
    // if date filtering is used, otherwise use general endpoint

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

  // Load all data from backend using BeatHistory API (correct for journey plans)
  const loadAllData = useCallback(async () => {
    try {
      setLoading(true);

      const request: PagedRequest = {
        pageNumber: 0, // Start from 0 to get first page
        pageSize: 1000, // Reasonable page size
        isCountRequired: true,
        sortCriterias: sorting.field ? [{
          fieldName: sorting.field,
          sortOrder: sorting.direction || "asc"
        }] : [],
        filterCriterias: []
      };

      // FIXED: Use BeatHistory API for journey plan data (planning data)
      // getUserJourneys was calling UserJourney API (execution data) - WRONG
      // getJourneyPlans calls BeatHistory API (planning data) - CORRECT  
      const response = await journeyPlanService.getJourneyPlans(request);

      if (response?.IsSuccess && response.Data?.PagedData) {
        // Transform BeatHistory (journey plan) data using the correct transformer
        const transformedJourneys = response.Data.PagedData.map((journey: any) =>
          journeyPlanService.transformBeatHistoryToJourney(journey)
        );
        
        setAllData(transformedJourneys);
        setPagination((prev) => ({
          ...prev,
          total: transformedJourneys.length,
          current: 1
        }));
      } else {
        setAllData([]);
        setPagination((prev) => ({
          ...prev,
          total: 0,
          current: 1
        }));
      }
    } catch (error) {
      console.error("Failed to load journey plans:", error);
      toastRef.current({
        title: "Error",
        description: "Failed to load journey plans. Please try again.",
        variant: "destructive"
      });
      setAllData([]);
      setPagination((prev) => ({
        ...prev,
        total: 0,
        current: 1
      }));
    } finally {
      setLoading(false);
    }
  }, [sorting.field, sorting.direction]);

  // Load data on mount and when dependencies change
  useEffect(() => {
    loadAllData();
  }, [loadAllData, refreshTrigger]);

  // Client-side filtering, sorting and pagination
  const paginatedData = useMemo(() => {
    let filteredData = [...allData];

    // Apply client-side filtering
    if (searchQuery) {
      const query = searchQuery.toLowerCase();
      filteredData = filteredData.filter(journey => 
        journey.LoginId?.toLowerCase().includes(query) ||
        journey.JobPositionUID?.toLowerCase().includes(query) ||
        journey.EmpUID?.toLowerCase().includes(query)
      );
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

    // Date filtering - temporarily disabled to show all journey data
    // Uncomment below to enable date filtering
    /*
    if (filters.visitDate) {
      const filterDate = moment(filters.visitDate).format('YYYY-MM-DD');
      filteredData = filteredData.filter(journey => {
        if (!journey.JourneyStartTime && !journey.CreatedTime) return false;
        const journeyDate = moment(journey.JourneyStartTime || journey.CreatedTime).format('YYYY-MM-DD');
        return journeyDate === filterDate;
      });
    }
    */

    // Update pagination total with filtered count
    if (filteredData.length !== pagination.total) {
      setPagination(prev => ({
        ...prev,
        total: filteredData.length,
        current: Math.min(prev.current, Math.ceil(filteredData.length / prev.pageSize) || 1)
      }));
    }

    // Apply client-side sorting if needed
    if (sorting.field) {
      filteredData.sort((a, b) => {
        let aValue = a[sorting.field as keyof UserJourney];
        let bValue = b[sorting.field as keyof UserJourney];

        // Handle undefined/null values
        if (aValue == null && bValue == null) return 0;
        if (aValue == null) return sorting.direction === "asc" ? -1 : 1;
        if (bValue == null) return sorting.direction === "asc" ? 1 : -1;

        // Handle string comparison
        if (typeof aValue === "string") aValue = aValue.toLowerCase();
        if (typeof bValue === "string") bValue = bValue.toLowerCase();

        if (aValue < bValue) return sorting.direction === "asc" ? -1 : 1;
        if (aValue > bValue) return sorting.direction === "asc" ? 1 : -1;
        return 0;
      });
    }

    // Apply pagination
    const startIndex = (pagination.current - 1) * pagination.pageSize;
    const endIndex = startIndex + pagination.pageSize;

    return filteredData.slice(startIndex, endIndex);
  }, [
    allData,
    searchQuery,
    filters.eotStatus,
    filters.attendanceStatus,
    filters.visitDate,
    pagination.current,
    pagination.pageSize,
    sorting.field,
    sorting.direction
  ]);

  // Handle pagination change
  const handlePaginationChange = useCallback(
    (page: number, pageSize: number) => {
      setPagination((prev) => ({
        ...prev,
        current: page,
        pageSize
      }));
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
        render: (value: string, record: UserJourney) => (
          <span className="text-sm">
            {record.JourneyStartTime 
              ? moment(record.JourneyStartTime).format("DD/MM/YYYY")
              : record.CreatedTime 
              ? moment(record.CreatedTime).format("DD/MM/YYYY")
              : "-"}
          </span>
        )
      },
      {
        key: "VehicleUID",
        title: "Route/Vehicle",
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
        key: "JourneyStartTime",
        title: "Start Time",
        width: "100px",
        render: (value: string) => (
          <span className="text-sm">
            {value ? moment(value).format("HH:mm") : "-"}
          </span>
        )
      },
      {
        key: "JourneyEndTime",
        title: "End Time",
        width: "100px",
        render: (value: string) => (
          <span className="text-sm">
            {value ? moment(value).format("HH:mm") : "-"}
          </span>
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
