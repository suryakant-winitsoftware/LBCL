"use client";

import React, { useState, useEffect, useRef, useCallback } from "react";
import { useRouter } from "next/navigation";
import { motion } from "framer-motion";
import moment from "moment";
import { cn } from "@/lib/utils";
import { usePagePermissions } from "@/hooks/use-page-permissions";

// UI Components
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { useToast } from "@/components/ui/use-toast";
import { Badge } from "@/components/ui/badge";
import { Separator } from "@/components/ui/separator";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
  DropdownMenuCheckboxItem,
} from "@/components/ui/dropdown-menu";
import { Popover, PopoverContent, PopoverTrigger } from "@/components/ui/popover";
import { Calendar as CalendarComponent } from "@/components/ui/calendar";
import { format } from "date-fns";

// Icons
import {
  Calendar,
  Plus,
  Edit3,
  Trash2,
  Loader2,
  CalendarCheck,
  CalendarX,
  AlertTriangle,
  Clock,
  Filter,
  Search,
  ChevronDown,
  X,
  FileDown,
  Upload
} from "lucide-react";

// Services
import { holidayService, Holiday, PagingRequest } from "@/services/holidayService";
import { listItemService, ListItem } from "@/services/listItemService";

interface HolidayManagementState {
  holidays: Holiday[];
  loading: boolean;
  creating: boolean;
  updating: boolean;
  deleting: string | null;
  searchTerm: string;
  selectedYear: number;
  selectedTypes: string[];
}

export default function HolidayManagementPage() {
  const router = useRouter();
  const { toast } = useToast();
  const searchInputRef = useRef<HTMLInputElement>(null);
  // Get permissions dynamically based on the current page path
  const permissions = usePagePermissions();
  
  const [state, setState] = useState<HolidayManagementState>({
    holidays: [],
    loading: true,
    creating: false,
    updating: false,
    deleting: null,
    searchTerm: "",
    selectedYear: new Date().getFullYear(),
    selectedTypes: []
  });
  const [modalOpen, setModalOpen] = useState(false);
  const [calendarOpen, setCalendarOpen] = useState(false);
  const [selectedDate, setSelectedDate] = useState<Date>(new Date());
  const [holidayTypes, setHolidayTypes] = useState<ListItem[]>([]);

  const [newHoliday, setNewHoliday] = useState({
    name: "",
    date: "",
    type: "" as Holiday["Type"],
    description: "",
    isRecurring: false
  });

  const [editingHolidayId, setEditingHolidayId] = useState<string | null>(null);
  const [editFormData, setEditFormData] = useState({
    name: "",
    date: "",
    type: "National" as Holiday["Type"],
    description: "",
    isRecurring: false
  });

  // Add keyboard shortcut for Ctrl+F / Cmd+F
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

  const loadHolidays = useCallback(async (year?: number, types?: string[]) => {
    setState(prev => ({ ...prev, loading: true }));
    
    const yearToUse = year !== undefined ? year : state.selectedYear;
    const typesToUse = types !== undefined ? types : state.selectedTypes;
    
    try {
      const pagingRequest: PagingRequest = {
        pageNumber: 1,
        pageSize: 1000,
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
            filterValue: yearToUse.toString(),
            filterType: "Equals"
          }
        ]
      };

      const response = await holidayService.getHolidayDetails(pagingRequest);
      
      let holidays = response.pagedData || [];
      
      // Apply client-side type filtering if types are selected
      if (typesToUse.length > 0) {
        holidays = holidays.filter(h => typesToUse.includes(h.Type));
      }
      
      setState(prev => ({
        ...prev,
        holidays: holidays,
        loading: false,
        selectedYear: yearToUse,
        selectedTypes: typesToUse
      }));

      if (holidays.length === 0 && !typesToUse.length) {
        toast({
          title: "No holidays found",
          description: `No holidays found for year ${yearToUse}`,
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
  }, [toast]);

  // Load holidays and holiday types on component mount
  useEffect(() => {
    loadHolidays(state.selectedYear, state.selectedTypes);
    fetchHolidayTypes();
  }, []);

  // Fetch holiday types from database
  const fetchHolidayTypes = async () => {
    try {
      const types = await listItemService.getHolidayTypes();
      setHolidayTypes(types);
      // Set default type for new holiday form
      if (types.length > 0) {
        setNewHoliday(prev => ({ ...prev, type: types[0].code as Holiday["Type"] }));
        setEditFormData(prev => ({ ...prev, type: types[0].code as Holiday["Type"] }));
      }
    } catch (error) {
      console.error("Error fetching holiday types:", error);
      // Fallback to default types if API fails
      const defaultTypes = [
        { id: 1, uid: 'HT_NATIONAL', code: 'National', name: 'National', isEditable: true, serialNo: 1, listHeaderUID: 'HolidayType' },
        { id: 2, uid: 'HT_REGIONAL', code: 'Regional', name: 'Regional', isEditable: true, serialNo: 2, listHeaderUID: 'HolidayType' },
        { id: 3, uid: 'HT_COMPANY', code: 'Company', name: 'Company', isEditable: true, serialNo: 3, listHeaderUID: 'HolidayType' },
        { id: 4, uid: 'HT_OPTIONAL', code: 'Optional', name: 'Optional', isEditable: true, serialNo: 4, listHeaderUID: 'HolidayType' }
      ];
      setHolidayTypes(defaultTypes);
      setNewHoliday(prev => ({ ...prev, type: 'National' as Holiday["Type"] }));
      setEditFormData(prev => ({ ...prev, type: 'National' as Holiday["Type"] }));
    }
  };
  
  // Keep selectedDate in sync with selectedYear
  useEffect(() => {
    const newDate = new Date(state.selectedYear, selectedDate.getMonth(), selectedDate.getDate());
    setSelectedDate(newDate);
  }, [state.selectedYear]);

  const handleYearChange = (year: number) => {
    setState(prev => ({ ...prev, selectedYear: year }));
    loadHolidays(year, state.selectedTypes);
  };

  const handleTypeFilterChange = (types: string[]) => {
    setState(prev => ({ ...prev, selectedTypes: types }));
    loadHolidays(state.selectedYear, types);
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
          type: holidayTypes.length > 0 ? holidayTypes[0].code as Holiday["Type"] : "National",
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

  const handleEditHoliday = (holiday: Holiday) => {
    const holidayDate = holiday.HolidayDate || holiday.Date || '';
    const formattedDate = holidayDate ? new Date(holidayDate).toISOString().split('T')[0] : '';
    
    setEditFormData({
      name: holiday.Name || '',
      date: formattedDate,
      type: holiday.Type || 'National',
      description: holiday.Description || "",
      isRecurring: holiday.IsRecurring || false
    });
    
    setEditingHolidayId(holiday.UID);
  };

  const updateHoliday = async () => {
    if (!editingHolidayId || !editFormData.name || !editFormData.date) {
      toast({
        title: "Validation Error",
        description: "Holiday name and date are required",
        variant: "destructive"
      });
      return;
    }

    setState(prev => ({ ...prev, updating: true }));

    try {
      const editingHoliday = state.holidays.find(h => h.UID === editingHolidayId);
      const year = new Date(editFormData.date).getFullYear();
      
      const holidayData = {
        ...editingHoliday,
        UID: editingHolidayId,
        Name: editFormData.name,
        HolidayDate: editFormData.date,
        Type: editFormData.type,
        IsOptional: editFormData.type === 'Optional',
        Year: year,
        HolidayListUID: editingHoliday?.HolidayListUID || `HOLIDAY_${year}`,
        Description: editFormData.description,
        IsRecurring: editFormData.isRecurring,
        ModifiedBy: "ADMIN",
        ModifiedTime: new Date().toISOString()
      };
      
      const result = await holidayService.updateHoliday(holidayData);
      
      if (result) {
        toast({
          title: "Success",
          description: `Holiday "${editFormData.name}" updated successfully`,
          variant: "default"
        });

        setEditingHolidayId(null);
        setEditFormData({
          name: "",
          date: "",
          type: holidayTypes.length > 0 ? holidayTypes[0].code as Holiday["Type"] : "National",
          description: "",
          isRecurring: false
        });
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

  const handleExport = () => {
    const csvContent = [
      ["Holiday Name", "Date", "Type", "Year"],
      ...filteredHolidays.map(holiday => [
        holiday.Name,
        moment(holiday.HolidayDate || holiday.Date).format("DD MMM YYYY"),
        holiday.Type,
        holiday.Year
      ])
    ].map(row => row.join(",")).join("\n");

    const blob = new Blob([csvContent], { type: "text/csv" });
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement("a");
    a.href = url;
    a.download = `holidays_${state.selectedYear}_${new Date().toISOString()}.csv`;
    a.click();
  };

  return (
    <div className="container mx-auto p-6">
      {/* Header */}
      <div className="flex justify-between items-center mb-4">
        <h2 className="text-2xl font-bold">Holiday Management</h2>
        <div className="flex gap-2">
          {permissions.canDownload && (
            <>
              <Button variant="outline" size="sm">
                <Upload className="h-4 w-4 mr-2" />
                Import
              </Button>
              <Button variant="outline" size="sm" onClick={handleExport}>
                <FileDown className="h-4 w-4 mr-2" />
                Export
              </Button>
            </>
          )}
          {permissions.canAdd && (
            <Button size="sm" onClick={() => setModalOpen(true)}>
              <Plus className="h-4 w-4 mr-2" />
              Add Holiday
            </Button>
          )}
        </div>
      </div>
      {/* Search and Filters */}
      <Card className="shadow-sm border-gray-200 mb-4">
        <CardContent className="py-3">
            <div className="flex gap-3">
              <div className="relative flex-1">
                <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 h-4 w-4" />
                <Input
                  ref={searchInputRef}
                  placeholder="Search holidays... (Ctrl+F)"
                  value={state.searchTerm}
                  onChange={(e) => setState(prev => ({ 
                    ...prev, 
                    searchTerm: e.target.value 
                  }))}
                  className="pl-10 border-gray-200 focus:border-primary focus:ring-1 focus:ring-primary/20"
                />
              </div>
              
              {/* Year Calendar Picker */}
              <Popover open={calendarOpen} onOpenChange={setCalendarOpen}>
                <PopoverTrigger asChild>
                  <Button variant="outline">
                    <Calendar className="h-4 w-4 mr-2" />
                    Year
                    <Badge variant="secondary" className="ml-2">
                      {state.selectedYear}
                    </Badge>
                    <ChevronDown className="h-4 w-4 ml-2" />
                  </Button>
                </PopoverTrigger>
                <PopoverContent className="w-auto p-0" align="start">
                  <CalendarComponent
                    mode="single"
                    selected={selectedDate}
                    onSelect={(date) => {
                      if (date) {
                        setSelectedDate(date);
                        const year = date.getFullYear();
                        if (year !== state.selectedYear) {
                          handleYearChange(year);
                        }
                        setCalendarOpen(false);
                      }
                    }}
                    defaultMonth={selectedDate}
                    initialFocus
                  />
                </PopoverContent>
              </Popover>
              
              {/* Type Filter */}
              <DropdownMenu>
                <DropdownMenuTrigger asChild>
                  <Button variant="outline">
                    <Filter className="h-4 w-4 mr-2" />
                    Type
                    {state.selectedTypes.length > 0 && (
                      <Badge variant="secondary" className="ml-2">
                        {state.selectedTypes.length}
                      </Badge>
                    )}
                    <ChevronDown className="h-4 w-4 ml-2" />
                  </Button>
                </DropdownMenuTrigger>
                <DropdownMenuContent align="end" className="w-48">
                  <DropdownMenuLabel>Filter by Type</DropdownMenuLabel>
                  <DropdownMenuSeparator />
                  {holidayTypes.map(type => (
                    <DropdownMenuCheckboxItem
                      key={type.uid}
                      checked={state.selectedTypes.includes(type.code)}
                      onCheckedChange={(checked) => {
                        const newTypes = checked 
                          ? [...state.selectedTypes, type.code]
                          : state.selectedTypes.filter(t => t !== type.code);
                        handleTypeFilterChange(newTypes);
                      }}
                    >
                      {type.name}
                    </DropdownMenuCheckboxItem>
                  ))}
                </DropdownMenuContent>
              </DropdownMenu>
              
              {/* Clear All Button */}
              {(state.searchTerm || state.selectedTypes.length > 0) && (
                <Button
                  variant="outline"
                  onClick={() => {
                    setState(prev => ({ 
                      ...prev, 
                      searchTerm: "",
                      selectedTypes: []
                    }));
                    loadHolidays(state.selectedYear, []);
                  }}
                >
                  <X className="h-4 w-4 mr-2" />
                  Clear All
                </Button>
              )}
            </div>
          </CardContent>
        </Card>

      {/* Stats */}
      <Card className="mb-4 shadow-sm border-gray-200">
        <CardContent className="pt-6">
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
          {permissions.canAdd && (
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
                  {holidayTypes.map(type => (
                    <option key={type.uid} value={type.code}>
                      {type.name}
                    </option>
                  ))}
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
          )}

          {/* Holiday List */}
          <Card className={permissions.canAdd ? "lg:col-span-2" : "lg:col-span-3"}>
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
                    {state.searchTerm || state.selectedTypes.length > 0 
                      ? "Try adjusting your search or filters" 
                      : "Add a new holiday using the form on the left"}
                  </p>
                </div>
              ) : (
                <div className="space-y-3 max-h-96 overflow-y-auto">
                  {filteredHolidays.map((holiday) => (
                    <motion.div
                      key={holiday.UID}
                      initial={{ opacity: 0, y: 20 }}
                      animate={{ opacity: 1, y: 0 }}
                      className="p-3 border rounded-lg hover:bg-gray-50"
                    >
                      {editingHolidayId === holiday.UID ? (
                        // Inline edit form
                        <div className="space-y-3">
                          <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
                            <div>
                              <label className="text-sm font-medium">Holiday Name</label>
                              <input
                                type="text"
                                value={editFormData.name}
                                onChange={(e) => setEditFormData(prev => ({
                                  ...prev,
                                  name: e.target.value
                                }))}
                                className="w-full border rounded px-3 py-2 mt-1"
                                placeholder="Holiday name"
                              />
                            </div>
                            <div>
                              <label className="text-sm font-medium">Date</label>
                              <input
                                type="date"
                                value={editFormData.date}
                                onChange={(e) => setEditFormData(prev => ({
                                  ...prev,
                                  date: e.target.value
                                }))}
                                className="w-full border rounded px-3 py-2 mt-1"
                              />
                            </div>
                          </div>
                          <div>
                            <label className="text-sm font-medium">Type</label>
                            <select
                              value={editFormData.type}
                              onChange={(e) => setEditFormData(prev => ({
                                ...prev,
                                type: e.target.value as Holiday["Type"]
                              }))}
                              className="w-full border rounded px-3 py-2 mt-1"
                            >
                              {holidayTypes.map(type => (
                                <option key={type.uid} value={type.code}>
                                  {type.name}
                                </option>
                              ))}
                            </select>
                          </div>
                          <div className="flex justify-end gap-2 pt-2">
                            <Button
                              variant="outline"
                              size="sm"
                              onClick={() => {
                                setEditingHolidayId(null);
                                setEditFormData({
                                  name: "",
                                  date: "",
                                  type: holidayTypes.length > 0 ? holidayTypes[0].code as Holiday["Type"] : "National",
                                  description: "",
                                  isRecurring: false
                                });
                              }}
                            >
                              Cancel
                            </Button>
                            <Button
                              size="sm"
                              onClick={updateHoliday}
                              disabled={state.updating}
                            >
                              {state.updating ? (
                                <>
                                  <Loader2 className="h-4 w-4 mr-2 animate-spin" />
                                  Updating...
                                </>
                              ) : (
                                <>
                                  <CheckCircle2 className="h-4 w-4 mr-2" />
                                  Update Holiday
                                </>
                              )}
                            </Button>
                          </div>
                        </div>
                      ) : (
                        // Normal display
                        <div className="flex items-center justify-between">
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

                          {(permissions.canEdit || permissions.canDelete) && (
                          <div className="flex items-center space-x-2">
                            {permissions.canEdit && (
                            <Button
                              variant="ghost"
                              size="sm"
                              onClick={() => handleEditHoliday(holiday)}
                            >
                              <Edit3 className="h-4 w-4" />
                            </Button>
                            )}
                            {permissions.canDelete && (
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
                            )}
                          </div>
                          )}
                        </div>
                      )}
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
  );
}