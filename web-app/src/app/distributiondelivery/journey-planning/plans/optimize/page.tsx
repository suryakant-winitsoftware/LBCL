"use client";

import React, { useState, useEffect } from "react";
import { useRouter } from "next/navigation";
import { motion } from "framer-motion";
import moment from "moment";
import { cn } from "@/lib/utils";

// UI Components
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { useToast } from "@/components/ui/use-toast";
import { Badge } from "@/components/ui/badge";
import { Separator } from "@/components/ui/separator";

// Icons
import {
  ArrowLeft,
  Calendar,
  Plus,
  Edit3,
  Trash2,
  Loader2,
  RefreshCw,
  CalendarCheck,
  CalendarX,
  CheckCircle2,
  AlertTriangle,
  Clock,
  Filter,
  Search
} from "lucide-react";

// Services
import { holidayService, Holiday, PagingRequest } from "@/services/holidayService";

interface HolidayManagementState {
  holidays: Holiday[];
  loading: boolean;
  creating: boolean;
  updating: boolean;
  deleting: string | null;
  searchTerm: string;
  selectedYear: number;
  filterType: string;
}

export default function HolidayOptimizePage() {
  const router = useRouter();
  const { toast } = useToast();
  
  const [state, setState] = useState<HolidayManagementState>({
    holidays: [],
    loading: true,
    creating: false,
    updating: false,
    deleting: null,
    searchTerm: "",
    selectedYear: new Date().getFullYear(),
    filterType: "all"
  });

  const [newHoliday, setNewHoliday] = useState({
    name: "",
    date: "",
    type: "National" as Holiday["Type"],
    description: "",
    isRecurring: false
  });

  const [editingHoliday, setEditingHoliday] = useState<Holiday | null>(null);

  // Load holidays on component mount and when filters change
  useEffect(() => {
    loadHolidays();
  }, [state.selectedYear, state.filterType]);

  const loadHolidays = async () => {
    setState(prev => ({ ...prev, loading: true }));
    
    try {
      const pagingRequest: PagingRequest = {
        pageNumber: 1,
        pageSize: 100,
        isCountRequired: true,
        sortCriterias: [
          {
            columnName: "HolidayDate",
            sortDirection: "ASC"
          }
        ],
        filterCriterias: [
          {
            columnName: "Year",
            filterValue: state.selectedYear.toString(),
            filterType: "Equals"
          }
        ]
      };

      // Add type filter if not "all"
      if (state.filterType !== "all") {
        pagingRequest.filterCriterias?.push({
          columnName: "Type",
          filterValue: state.filterType,
          filterType: "Equals"
        });
      }

      const response = await holidayService.getHolidayDetails(pagingRequest);
      
      setState(prev => ({
        ...prev,
        holidays: response.pagedData || [],
        loading: false
      }));

      if (response.pagedData.length === 0) {
        toast({
          title: "No holidays found",
          description: `No holidays found for year ${state.selectedYear}`,
          variant: "default"
        });
      }
    } catch (error) {
      console.error("Error loading holidays:", error);
      setState(prev => ({ ...prev, loading: false }));
      toast({
        title: "Error",
        description: "Failed to load holidays. Please try again.",
        variant: "destructive"
      });
    }
  };

  const createHoliday = async () => {
    if (!newHoliday.name || !newHoliday.date) {
      toast({
        title: "Validation Error",
        description: "Holiday name and date are required",
        variant: "destructive"
      });
      return;
    }

    setState(prev => ({ ...prev, creating: true }));

    try {
      const holidayData: Partial<Holiday> = {
        HolidayListUID: `HOLIDAY_${state.selectedYear}`,
        HolidayDate: newHoliday.date,
        Type: newHoliday.type,
        Name: newHoliday.name,
        IsOptional: false,
        Year: state.selectedYear,
        UID: `HOLIDAY_${state.selectedYear}_${Date.now()}`,
        CreatedBy: "ADMIN",
        IsSelected: false
      };

      const result = await holidayService.createHoliday(holidayData);
      
      if (result) {
        toast({
          title: "Success",
          description: `Holiday "${newHoliday.name}" created successfully`,
          variant: "default"
        });

        // Reset form
        setNewHoliday({
          name: "",
          date: "",
          type: "National",
          description: "",
          isRecurring: false
        });

        // Reload holidays
        loadHolidays();
      }
    } catch (error) {
      console.error("Error creating holiday:", error);
      toast({
        title: "Error",
        description: "Failed to create holiday. Please try again.",
        variant: "destructive"
      });
    } finally {
      setState(prev => ({ ...prev, creating: false }));
    }
  };

  const updateHoliday = async (holiday: Holiday) => {
    setState(prev => ({ ...prev, updating: true }));

    try {
      const result = await holidayService.updateHoliday(holiday);
      
      if (result) {
        toast({
          title: "Success",
          description: `Holiday "${holiday.Name}" updated successfully`,
          variant: "default"
        });

        setEditingHoliday(null);
        loadHolidays();
      }
    } catch (error) {
      console.error("Error updating holiday:", error);
      toast({
        title: "Error",
        description: "Failed to update holiday. Please try again.",
        variant: "destructive"
      });
    } finally {
      setState(prev => ({ ...prev, updating: false }));
    }
  };

  const deleteHoliday = async (uid: string, name: string) => {
    if (!confirm(`Are you sure you want to delete "${name}"?`)) {
      return;
    }

    setState(prev => ({ ...prev, deleting: uid }));

    try {
      const result = await holidayService.deleteHoliday(uid);
      
      if (result) {
        toast({
          title: "Success",
          description: `Holiday "${name}" deleted successfully`,
          variant: "default"
        });

        loadHolidays();
      }
    } catch (error) {
      console.error("Error deleting holiday:", error);
      toast({
        title: "Error",
        description: "Failed to delete holiday. Please try again.",
        variant: "destructive"
      });
    } finally {
      setState(prev => ({ ...prev, deleting: null }));
    }
  };

  const filteredHolidays = state.holidays.filter(holiday => {
    const matchesSearch = !state.searchTerm || 
      holiday.Name?.toLowerCase().includes(state.searchTerm.toLowerCase()) ||
      holiday.Type?.toLowerCase().includes(state.searchTerm.toLowerCase());
    
    return matchesSearch;
  });

  const getTypeColor = (type: string) => {
    switch (type) {
      case "National":
        return "bg-red-100 text-red-800";
      case "Regional":
        return "bg-blue-100 text-blue-800";
      case "Company":
        return "bg-green-100 text-green-800";
      case "Optional":
        return "bg-yellow-100 text-yellow-800";
      default:
        return "bg-gray-100 text-gray-800";
    }
  };

  const getUpcomingHolidays = () => {
    const today = new Date();
    return filteredHolidays
      .filter(holiday => new Date(holiday.HolidayDate || holiday.Date) > today)
      .slice(0, 5);
  };

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <div className="bg-white border-b px-6 py-5">
        <div className="max-w-7xl mx-auto">
          <div className="flex items-center justify-between">
            <div>
              <h1 className="text-2xl font-semibold text-gray-900">
                Holiday Management
              </h1>
              <p className="text-sm text-gray-600 mt-1">
                Manage holidays for journey plan optimization
              </p>
            </div>
            <Button
              variant="ghost"
              size="sm"
              onClick={() =>
                router.push(
                  "/updatedfeatures/journey-plan-management/journey-plans/manage"
                )
              }
            >
              <ArrowLeft className="h-4 w-4 mr-2" />
              Back to Journey Plans
            </Button>
          </div>
        </div>
      </div>

      <div className="max-w-7xl mx-auto px-6 py-6">
        {/* Control Panel */}
        <Card className="mb-6">
          <CardHeader>
            <div className="flex items-center justify-between">
              <div>
                <CardTitle className="flex items-center">
                  <CalendarCheck className="h-5 w-5 mr-2" />
                  Holiday Overview
                </CardTitle>
                <CardDescription>
                  Manage holidays that affect journey planning
                </CardDescription>
              </div>
              <Button onClick={loadHolidays} variant="outline" size="sm">
                <RefreshCw className="h-4 w-4 mr-2" />
                Refresh
              </Button>
            </div>
          </CardHeader>
          <CardContent>
            {/* Filters */}
            <div className="flex flex-wrap gap-4 mb-4">
              <div className="flex items-center space-x-2">
                <label className="text-sm font-medium">Year:</label>
                <select
                  value={state.selectedYear}
                  onChange={(e) => setState(prev => ({ 
                    ...prev, 
                    selectedYear: parseInt(e.target.value) 
                  }))}
                  className="border rounded px-3 py-1 text-sm"
                >
                  {[2023, 2024, 2025, 2026].map(year => (
                    <option key={year} value={year}>{year}</option>
                  ))}
                </select>
              </div>

              <div className="flex items-center space-x-2">
                <label className="text-sm font-medium">Type:</label>
                <select
                  value={state.filterType}
                  onChange={(e) => setState(prev => ({ 
                    ...prev, 
                    filterType: e.target.value 
                  }))}
                  className="border rounded px-3 py-1 text-sm"
                >
                  <option value="all">All Types</option>
                  <option value="National">National</option>
                  <option value="Regional">Regional</option>
                  <option value="Company">Company</option>
                  <option value="Optional">Optional</option>
                </select>
              </div>

              <div className="flex items-center space-x-2">
                <Search className="h-4 w-4" />
                <input
                  type="text"
                  placeholder="Search holidays..."
                  value={state.searchTerm}
                  onChange={(e) => setState(prev => ({ 
                    ...prev, 
                    searchTerm: e.target.value 
                  }))}
                  className="border rounded px-3 py-1 text-sm w-48"
                />
              </div>
            </div>

            {/* Stats */}
            <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
              <div className="bg-blue-50 p-4 rounded-lg">
                <div className="text-2xl font-bold text-blue-600">
                  {filteredHolidays.length}
                </div>
                <div className="text-sm text-blue-800">Total Holidays</div>
              </div>
              <div className="bg-green-50 p-4 rounded-lg">
                <div className="text-2xl font-bold text-green-600">
                  {filteredHolidays.filter(h => h.Type === "National").length}
                </div>
                <div className="text-sm text-green-800">National</div>
              </div>
              <div className="bg-yellow-50 p-4 rounded-lg">
                <div className="text-2xl font-bold text-yellow-600">
                  {filteredHolidays.filter(h => h.Type === "Company").length}
                </div>
                <div className="text-sm text-yellow-800">Company</div>
              </div>
              <div className="bg-red-50 p-4 rounded-lg">
                <div className="text-2xl font-bold text-red-600">
                  {getUpcomingHolidays().length}
                </div>
                <div className="text-sm text-red-800">Upcoming</div>
              </div>
            </div>
          </CardContent>
        </Card>

        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
          {/* Create Holiday Form */}
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center">
                <Plus className="h-4 w-4 mr-2" />
                Add New Holiday
              </CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div>
                <label className="text-sm font-medium">Holiday Name</label>
                <input
                  type="text"
                  value={newHoliday.name}
                  onChange={(e) => setNewHoliday(prev => ({
                    ...prev,
                    name: e.target.value
                  }))}
                  className="w-full border rounded px-3 py-2 mt-1"
                  placeholder="e.g., Independence Day"
                />
              </div>

              <div>
                <label className="text-sm font-medium">Date</label>
                <input
                  type="date"
                  value={newHoliday.date}
                  onChange={(e) => setNewHoliday(prev => ({
                    ...prev,
                    date: e.target.value
                  }))}
                  className="w-full border rounded px-3 py-2 mt-1"
                />
              </div>

              <div>
                <label className="text-sm font-medium">Type</label>
                <select
                  value={newHoliday.type}
                  onChange={(e) => setNewHoliday(prev => ({
                    ...prev,
                    type: e.target.value as Holiday["Type"]
                  }))}
                  className="w-full border rounded px-3 py-2 mt-1"
                >
                  <option value="National">National</option>
                  <option value="Regional">Regional</option>
                  <option value="Company">Company</option>
                  <option value="Optional">Optional</option>
                </select>
              </div>

              <Button
                onClick={createHoliday}
                disabled={state.creating || !newHoliday.name || !newHoliday.date}
                className="w-full"
              >
                {state.creating ? (
                  <>
                    <Loader2 className="h-4 w-4 mr-2 animate-spin" />
                    Creating...
                  </>
                ) : (
                  <>
                    <Plus className="h-4 w-4 mr-2" />
                    Add Holiday
                  </>
                )}
              </Button>
            </CardContent>
          </Card>

          {/* Holiday List */}
          <Card className="lg:col-span-2">
            <CardHeader>
              <CardTitle className="flex items-center">
                <Calendar className="h-4 w-4 mr-2" />
                Holidays for {state.selectedYear}
              </CardTitle>
            </CardHeader>
            <CardContent>
              {state.loading ? (
                <div className="flex items-center justify-center py-8">
                  <Loader2 className="h-6 w-6 animate-spin mr-2" />
                  Loading holidays...
                </div>
              ) : filteredHolidays.length === 0 ? (
                <div className="text-center py-8">
                  <CalendarX className="h-12 w-12 mx-auto text-gray-400 mb-4" />
                  <p className="text-gray-500">No holidays found</p>
                  <p className="text-sm text-gray-400">
                    Add a new holiday using the form on the left
                  </p>
                </div>
              ) : (
                <div className="space-y-3 max-h-96 overflow-y-auto">
                  {filteredHolidays.map((holiday) => (
                    <motion.div
                      key={holiday.UID}
                      initial={{ opacity: 0, y: 20 }}
                      animate={{ opacity: 1, y: 0 }}
                      className="flex items-center justify-between p-3 border rounded-lg hover:bg-gray-50"
                    >
                      <div className="flex-1">
                        <div className="flex items-center space-x-2">
                          <h4 className="font-medium">{holiday.Name}</h4>
                          <Badge className={getTypeColor(holiday.Type || "")}>
                            {holiday.Type}
                          </Badge>
                        </div>
                        <p className="text-sm text-gray-500">
                          {moment(holiday.HolidayDate || holiday.Date).format("DD MMM YYYY")}
                        </p>
                      </div>

                      <div className="flex items-center space-x-2">
                        <Button
                          variant="ghost"
                          size="sm"
                          onClick={() => setEditingHoliday(holiday)}
                        >
                          <Edit3 className="h-4 w-4" />
                        </Button>
                        <Button
                          variant="ghost"
                          size="sm"
                          onClick={() => deleteHoliday(holiday.UID, holiday.Name)}
                          disabled={state.deleting === holiday.UID}
                        >
                          {state.deleting === holiday.UID ? (
                            <Loader2 className="h-4 w-4 animate-spin" />
                          ) : (
                            <Trash2 className="h-4 w-4" />
                          )}
                        </Button>
                      </div>
                    </motion.div>
                  ))}
                </div>
              )}
            </CardContent>
          </Card>
        </div>

        {/* Upcoming Holidays Preview */}
        {getUpcomingHolidays().length > 0 && (
          <Card className="mt-6">
            <CardHeader>
              <CardTitle className="flex items-center">
                <Clock className="h-4 w-4 mr-2" />
                Upcoming Holidays
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="grid grid-cols-1 md:grid-cols-5 gap-4">
                {getUpcomingHolidays().map((holiday) => (
                  <div
                    key={holiday.UID}
                    className="text-center p-3 border rounded-lg bg-blue-50"
                  >
                    <div className="text-sm font-medium text-blue-900">
                      {holiday.Name}
                    </div>
                    <div className="text-xs text-blue-600">
                      {moment(holiday.HolidayDate || holiday.Date).format("DD MMM")}
                    </div>
                    <Badge size="sm" className={getTypeColor(holiday.Type || "")}>
                      {holiday.Type}
                    </Badge>
                  </div>
                ))}
              </div>
            </CardContent>
          </Card>
        )}
      </div>

      {/* Edit Holiday Modal - Simple implementation */}
      {editingHoliday && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
          <Card className="w-full max-w-md">
            <CardHeader>
              <CardTitle>Edit Holiday</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div>
                <label className="text-sm font-medium">Holiday Name</label>
                <input
                  type="text"
                  value={editingHoliday.Name}
                  onChange={(e) => setEditingHoliday(prev => prev ? ({
                    ...prev,
                    Name: e.target.value
                  }) : null)}
                  className="w-full border rounded px-3 py-2 mt-1"
                />
              </div>

              <div>
                <label className="text-sm font-medium">Date</label>
                <input
                  type="date"
                  value={moment(editingHoliday.HolidayDate || editingHoliday.Date).format("YYYY-MM-DD")}
                  onChange={(e) => setEditingHoliday(prev => prev ? ({
                    ...prev,
                    HolidayDate: e.target.value
                  }) : null)}
                  className="w-full border rounded px-3 py-2 mt-1"
                />
              </div>

              <div>
                <label className="text-sm font-medium">Type</label>
                <select
                  value={editingHoliday.Type}
                  onChange={(e) => setEditingHoliday(prev => prev ? ({
                    ...prev,
                    Type: e.target.value as Holiday["Type"]
                  }) : null)}
                  className="w-full border rounded px-3 py-2 mt-1"
                >
                  <option value="National">National</option>
                  <option value="Regional">Regional</option>
                  <option value="Company">Company</option>
                  <option value="Optional">Optional</option>
                </select>
              </div>

              <div className="flex space-x-2">
                <Button
                  onClick={() => updateHoliday(editingHoliday)}
                  disabled={state.updating}
                  className="flex-1"
                >
                  {state.updating ? (
                    <>
                      <Loader2 className="h-4 w-4 mr-2 animate-spin" />
                      Updating...
                    </>
                  ) : (
                    <>
                      <CheckCircle2 className="h-4 w-4 mr-2" />
                      Update
                    </>
                  )}
                </Button>
                <Button
                  variant="outline"
                  onClick={() => setEditingHoliday(null)}
                  className="flex-1"
                >
                  Cancel
                </Button>
              </div>
            </CardContent>
          </Card>
        </div>
      )}
    </div>
  );
}