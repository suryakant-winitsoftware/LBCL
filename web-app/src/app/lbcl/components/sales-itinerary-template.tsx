"use client";

import { useState, useEffect } from "react";
import { useRouter } from "next/navigation";
import { Card, CardContent, CardHeader, CardTitle } from "@/app/lbcl/components/ui/card";
import { Button } from "@/app/lbcl/components/ui/button";
import { Badge } from "@/app/lbcl/components/ui/badge";
import { Plus, Trash2, Download, Upload, RefreshCw, Eye, CalendarIcon, ChevronLeft, ChevronRight } from "lucide-react";
import * as XLSX from "xlsx";
import { Calendar } from "@/app/lbcl/components/ui/calendar";
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/app/lbcl/components/ui/popover";
import { cn } from "@/lib/utils";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/app/lbcl/components/ui/table";
import {
  ItineraryEntry,
  MOCK_ITINERARY_DATA,
  ACTIVITY_TYPES,
  MARKETS,
} from "./itinerary-mock-data";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/app/lbcl/components/ui/select";
import { Input } from "@/app/lbcl/components/ui/input";

// Holiday interface
interface Holiday {
  date: string; // Format: DD/MM/YY
  name: string;
  type: "public" | "bank" | "optional";
}

// Mock holidays data for 2025
const HOLIDAYS_2025: Record<string, Holiday[]> = {
  "01": [ // January
    { date: "01/01/25", name: "New Year's Day", type: "public" },
    { date: "15/01/25", name: "Thai Pongal", type: "public" },
  ],
  "02": [ // February
    { date: "04/02/25", name: "Independence Day", type: "public" },
    { date: "26/02/25", name: "Maha Shivaratri", type: "public" },
  ],
  "03": [ // March
    { date: "14/03/25", name: "Medin Poya Day", type: "public" },
  ],
  "04": [ // April
    { date: "12/04/25", name: "Bak Poya Day", type: "public" },
    { date: "13/04/25", name: "Sinhala & Tamil New Year's Eve", type: "public" },
    { date: "14/04/25", name: "Sinhala & Tamil New Year's Day", type: "public" },
    { date: "18/04/25", name: "Good Friday", type: "public" },
  ],
  "05": [ // May
    { date: "01/05/25", name: "May Day", type: "public" },
    { date: "12/05/25", name: "Vesak Day", type: "public" },
    { date: "13/05/25", name: "Day following Vesak Full Moon Poya", type: "public" },
  ],
  "06": [ // June
    { date: "10/06/25", name: "Poson Poya Day", type: "public" },
  ],
  "07": [ // July
    { date: "09/07/25", name: "Esala Poya Day", type: "public" },
  ],
  "08": [ // August
    { date: "08/08/25", name: "Nikini Poya Day", type: "public" },
  ],
  "09": [ // September
    { date: "06/09/25", name: "Binara Poya Day", type: "public" },
  ],
  "10": [ // October
    { date: "06/10/25", name: "Vap Poya Day", type: "public" },
    { date: "21/10/25", name: "Deepavali", type: "public" },
  ],
  "11": [ // November
    { date: "05/11/25", name: "Il Poya Day", type: "public" },
  ],
  "12": [ // December
    { date: "04/12/25", name: "Unduvap Poya Day", type: "public" },
    { date: "25/12/25", name: "Christmas Day", type: "public" },
  ],
};

export function SalesItineraryTemplate() {
  const router = useRouter();
  const [entries, setEntries] = useState<ItineraryEntry[]>([]);
  const [filteredEntries, setFilteredEntries] = useState<ItineraryEntry[]>([]);

  // Month/Year selection
  const [selectedMonth, setSelectedMonth] = useState<number>(new Date().getMonth() + 1); // 1-12
  const [selectedYear, setSelectedYear] = useState<number>(new Date().getFullYear());

  // Filter states
  const [filterType, setFilterType] = useState<string>("all");
  const [filterMarket, setFilterMarket] = useState<string>("all");
  const [filterSearch, setFilterSearch] = useState<string>("");
  const [filterDateFrom, setFilterDateFrom] = useState<Date | undefined>(undefined);
  const [filterDateTo, setFilterDateTo] = useState<Date | undefined>(undefined);

  // Helper function to convert DD/MM/YY to Date
  const parseDateString = (dateStr: string): Date | null => {
    if (!dateStr) return null;
    const parts = dateStr.split('/');
    if (parts.length !== 3) return null;
    const day = parseInt(parts[0], 10);
    const month = parseInt(parts[1], 10) - 1; // Month is 0-indexed
    const year = parseInt(parts[2], 10) + 2000; // Convert YY to YYYY
    return new Date(year, month, day);
  };

  // Helper function to format Date to DD/MM/YY
  const formatDateString = (date: Date): string => {
    const day = String(date.getDate()).padStart(2, '0');
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const year = String(date.getFullYear()).slice(-2);
    return `${day}/${month}/${year}`;
  };

  // Get holidays for selected month
  const getHolidaysForMonth = (): Holiday[] => {
    const monthKey = String(selectedMonth).padStart(2, '0');
    return HOLIDAYS_2025[monthKey] || [];
  };

  // Format month name
  const getMonthName = (month: number): string => {
    const months = [
      "January", "February", "March", "April", "May", "June",
      "July", "August", "September", "October", "November", "December"
    ];
    return months[month - 1];
  };

  // Navigate to previous month
  const handlePreviousMonth = () => {
    if (selectedMonth === 1) {
      setSelectedMonth(12);
      setSelectedYear(selectedYear - 1);
    } else {
      setSelectedMonth(selectedMonth - 1);
    }
  };

  // Navigate to next month
  const handleNextMonth = () => {
    if (selectedMonth === 12) {
      setSelectedMonth(1);
      setSelectedYear(selectedYear + 1);
    } else {
      setSelectedMonth(selectedMonth + 1);
    }
  };

  // Check if a date is a holiday
  const isHoliday = (dateStr: string): boolean => {
    const holidays = getHolidaysForMonth();
    return holidays.some(h => h.date === dateStr);
  };

  // Get holiday badge color
  const getHolidayTypeColor = (type: string) => {
    switch (type) {
      case "public":
        return "bg-red-100 text-red-700 border-red-200";
      case "bank":
        return "bg-blue-100 text-blue-700 border-blue-200";
      case "optional":
        return "bg-yellow-100 text-yellow-700 border-yellow-200";
      default:
        return "bg-gray-100 text-gray-700 border-gray-200";
    }
  };

  // Function to load entries from localStorage
  const loadEntries = () => {
    const savedEntries = localStorage.getItem('itinerary_entries');
    if (savedEntries) {
      const parsedEntries = JSON.parse(savedEntries);
      setEntries(parsedEntries);
      console.log("✅ Loaded entries from localStorage:", parsedEntries);
    } else {
      // If no saved entries, use mock data
      setEntries(MOCK_ITINERARY_DATA);
      console.log("ℹ️ No saved entries, using mock data");
    }
  };

  // Apply filters whenever entries or filter values change
  useEffect(() => {
    let filtered = [...entries];

    // Filter by Activity Type
    if (filterType !== "all") {
      filtered = filtered.filter(entry => entry.type === filterType);
    }

    // Filter by Market
    if (filterMarket !== "all") {
      filtered = filtered.filter(entry => entry.market === filterMarket);
    }

    // Filter by Search (searches in focusArea, kraPlan, accompaniedBy)
    if (filterSearch.trim() !== "") {
      const searchLower = filterSearch.toLowerCase();
      filtered = filtered.filter(entry =>
        entry.focusArea.toLowerCase().includes(searchLower) ||
        entry.kraPlan.toLowerCase().includes(searchLower) ||
        entry.accompaniedBy.toLowerCase().includes(searchLower) ||
        entry.market.toLowerCase().includes(searchLower)
      );
    }

    // Filter by Date Range
    if (filterDateFrom || filterDateTo) {
      filtered = filtered.filter(entry => {
        const entryDate = parseDateString(entry.date);
        if (!entryDate) return true;

        if (filterDateFrom && filterDateTo) {
          return entryDate >= filterDateFrom && entryDate <= filterDateTo;
        } else if (filterDateFrom) {
          return entryDate >= filterDateFrom;
        } else if (filterDateTo) {
          return entryDate <= filterDateTo;
        }
        return true;
      });
    }

    setFilteredEntries(filtered);
  }, [entries, filterType, filterMarket, filterSearch, filterDateFrom, filterDateTo]);

  const handleClearFilters = () => {
    setFilterType("all");
    setFilterMarket("all");
    setFilterSearch("");
    setFilterDateFrom(undefined);
    setFilterDateTo(undefined);
  };

  // Load entries on component mount
  useEffect(() => {
    loadEntries();
  }, []);

  // Reload entries when window gains focus (user comes back to this tab/page)
  useEffect(() => {
    const handleFocus = () => {
      console.log("🔄 Page focused, reloading entries...");
      loadEntries();
    };

    window.addEventListener('focus', handleFocus);

    // Also listen to visibility change
    const handleVisibilityChange = () => {
      if (!document.hidden) {
        console.log("👁️ Page visible, reloading entries...");
        loadEntries();
      }
    };

    document.addEventListener('visibilitychange', handleVisibilityChange);

    return () => {
      window.removeEventListener('focus', handleFocus);
      document.removeEventListener('visibilitychange', handleVisibilityChange);
    };
  }, []);

  const handleDeleteEntry = (id: number) => {
    const updatedEntries = entries.filter(e => e.id !== id);
    setEntries(updatedEntries);

    // Update localStorage
    localStorage.setItem('itinerary_entries', JSON.stringify(updatedEntries));
    console.log("🗑️ Entry deleted, localStorage updated");
  };

  const handleViewDetails = (id: number) => {
    router.push(`/lbcl/sales-itinerary/view?id=${id}`);
  };

  const handleExportToExcel = () => {
    if (entries.length === 0) {
      alert("No entries to export");
      return;
    }

    // Prepare data for Excel
    const excelData = entries.map((entry, index) => ({
      "#": index + 1,
      "Activity Type": entry.type,
      "Focus Area": entry.focusArea,
      "Date": entry.date,
      "Day": entry.day,
      "KRA Plan": entry.kraPlan,
      "Morning Meeting": entry.morningMeeting || "NA",
      "Time From": entry.timeFrom,
      "Time To": entry.timeTo,
      "Market": entry.market,
      "Channel": entry.channel || "NA",
      "Route No": entry.routeNo || "NA",
      "Outlet No": entry.outletNo || "NA",
      "Accompanied By": entry.accompaniedBy,
      "Night Out": entry.nightOut,
      "No. of Days": entry.noOfDays,
      "Planned Mileage (km)": entry.plannedMileage,
    }));

    // Create workbook and worksheet
    const wb = XLSX.utils.book_new();
    const ws = XLSX.utils.json_to_sheet(excelData);

    // Set column widths
    const colWidths = [
      { wch: 5 },  // #
      { wch: 20 }, // Activity Type
      { wch: 25 }, // Focus Area
      { wch: 12 }, // Date
      { wch: 8 },  // Day
      { wch: 40 }, // KRA Plan
      { wch: 15 }, // Morning Meeting
      { wch: 12 }, // Time From
      { wch: 12 }, // Time To
      { wch: 15 }, // Market
      { wch: 15 }, // Channel
      { wch: 12 }, // Route No
      { wch: 12 }, // Outlet No
      { wch: 20 }, // Accompanied By
      { wch: 15 }, // Night Out
      { wch: 12 }, // No. of Days
      { wch: 18 }, // Planned Mileage
    ];
    ws['!cols'] = colWidths;

    // Add worksheet to workbook
    XLSX.utils.book_append_sheet(wb, ws, "Itinerary Plan");

    // Generate filename with current date
    const today = new Date().toISOString().split('T')[0];
    const filename = `Sales_Itinerary_${today}.xlsx`;

    // Save file
    XLSX.writeFile(wb, filename);
    console.log(`✅ Exported ${entries.length} entries to ${filename}`);
  };

  return (
    <div className="space-y-4">
      {/* Month/Year Selector and Holidays */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-4">
        {/* Month/Year Selector */}
        <Card className="lg:col-span-2">
          <CardHeader className="py-3 bg-gradient-to-r from-[#A08B5C]/10 to-transparent">
            <CardTitle className="text-base flex items-center justify-between">
              <span>📅 Itinerary Planning Period</span>
            </CardTitle>
          </CardHeader>
          <CardContent className="p-4">
            <div className="flex items-center justify-between">
              <Button
                variant="outline"
                size="sm"
                onClick={handlePreviousMonth}
                className="h-10 px-3"
              >
                <ChevronLeft className="h-4 w-4" />
              </Button>

              <div className="text-center">
                <div className="text-2xl font-bold text-[#A08B5C]">
                  {getMonthName(selectedMonth)} {selectedYear}
                </div>
                <p className="text-xs text-gray-500 mt-1">
                  Planning period for this month's sales itinerary
                </p>
              </div>

              <Button
                variant="outline"
                size="sm"
                onClick={handleNextMonth}
                className="h-10 px-3"
              >
                <ChevronRight className="h-4 w-4" />
              </Button>
            </div>

            {/* Month Stats */}
            <div className="grid grid-cols-3 gap-3 mt-4 pt-4 border-t">
              <div className="text-center">
                <div className="text-xs text-gray-500 mb-1">Working Days</div>
                <div className="text-lg font-semibold text-gray-900">
                  {new Date(selectedYear, selectedMonth, 0).getDate() - getHolidaysForMonth().length}
                </div>
              </div>
              <div className="text-center">
                <div className="text-xs text-gray-500 mb-1">Holidays</div>
                <div className="text-lg font-semibold text-red-600">
                  {getHolidaysForMonth().length}
                </div>
              </div>
              <div className="text-center">
                <div className="text-xs text-gray-500 mb-1">Total Days</div>
                <div className="text-lg font-semibold text-gray-900">
                  {new Date(selectedYear, selectedMonth, 0).getDate()}
                </div>
              </div>
            </div>
          </CardContent>
        </Card>

        {/* Holidays List */}
        <Card>
          <CardHeader className="py-3 bg-gradient-to-r from-red-50 to-transparent">
            <CardTitle className="text-base flex items-center justify-between">
              <span>🎉 Holidays in {getMonthName(selectedMonth)}</span>
              <Badge variant="outline" className="text-xs">
                {getHolidaysForMonth().length}
              </Badge>
            </CardTitle>
          </CardHeader>
          <CardContent className="p-4">
            {getHolidaysForMonth().length === 0 ? (
              <div className="text-center py-8 text-gray-500">
                <p className="text-sm">No holidays this month</p>
                <p className="text-xs mt-1">Full working month ahead! 🚀</p>
              </div>
            ) : (
              <div className="space-y-2 max-h-64 overflow-y-auto">
                {getHolidaysForMonth().map((holiday, index) => (
                  <div
                    key={index}
                    className="flex items-start gap-2 p-2 rounded-lg border bg-white hover:bg-gray-50 transition-colors"
                  >
                    <div className="flex-shrink-0 w-12 h-12 flex items-center justify-center bg-red-100 rounded-lg">
                      <div className="text-center">
                        <div className="text-lg font-bold text-red-600">
                          {holiday.date.split('/')[0]}
                        </div>
                        <div className="text-[8px] text-red-600 uppercase">
                          {getMonthName(parseInt(holiday.date.split('/')[1])).slice(0, 3)}
                        </div>
                      </div>
                    </div>
                    <div className="flex-1 min-w-0">
                      <div className="font-medium text-sm text-gray-900 line-clamp-2">
                        {holiday.name}
                      </div>
                      <Badge
                        variant="outline"
                        className={`mt-1 text-[10px] ${getHolidayTypeColor(holiday.type)}`}
                      >
                        {holiday.type.toUpperCase()}
                      </Badge>
                    </div>
                  </div>
                ))}
              </div>
            )}
          </CardContent>
        </Card>
      </div>

      {/* Filters Section */}
      <Card>
        <CardHeader className="py-3 bg-gray-50">
          <CardTitle className="text-base">Filter Entries</CardTitle>
        </CardHeader>
        <CardContent className="p-4">
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-5 gap-3">
            {/* Activity Type Filter */}
            <div>
              <label className="block text-xs font-medium text-gray-700 mb-1.5">
                Activity Type
              </label>
              <Select value={filterType} onValueChange={setFilterType}>
                <SelectTrigger className="h-9 text-xs">
                  <SelectValue placeholder="All Types" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all" className="text-xs">All Types</SelectItem>
                  {ACTIVITY_TYPES.map((type) => (
                    <SelectItem key={type} value={type} className="text-xs">
                      {type}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            {/* Market Filter */}
            <div>
              <label className="block text-xs font-medium text-gray-700 mb-1.5">
                Market
              </label>
              <Select value={filterMarket} onValueChange={setFilterMarket}>
                <SelectTrigger className="h-9 text-xs">
                  <SelectValue placeholder="All Markets" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all" className="text-xs">All Markets</SelectItem>
                  {MARKETS.map((market) => (
                    <SelectItem key={market} value={market} className="text-xs">
                      {market}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            {/* Date From */}
            <div>
              <label className="block text-xs font-medium text-gray-700 mb-1.5">
                Date From
              </label>
              <Popover>
                <PopoverTrigger asChild>
                  <Button
                    variant="outline"
                    className={cn(
                      "w-full h-9 justify-start text-left font-normal text-xs",
                      !filterDateFrom && "text-muted-foreground"
                    )}
                  >
                    <CalendarIcon className="mr-2 h-3.5 w-3.5" />
                    {filterDateFrom ? formatDateString(filterDateFrom) : <span>Pick a date</span>}
                  </Button>
                </PopoverTrigger>
                <PopoverContent className="w-auto p-0" align="start">
                  <Calendar
                    mode="single"
                    selected={filterDateFrom}
                    onSelect={setFilterDateFrom}
                    initialFocus
                  />
                </PopoverContent>
              </Popover>
            </div>

            {/* Date To */}
            <div>
              <label className="block text-xs font-medium text-gray-700 mb-1.5">
                Date To
              </label>
              <Popover>
                <PopoverTrigger asChild>
                  <Button
                    variant="outline"
                    className={cn(
                      "w-full h-9 justify-start text-left font-normal text-xs",
                      !filterDateTo && "text-muted-foreground"
                    )}
                  >
                    <CalendarIcon className="mr-2 h-3.5 w-3.5" />
                    {filterDateTo ? formatDateString(filterDateTo) : <span>Pick a date</span>}
                  </Button>
                </PopoverTrigger>
                <PopoverContent className="w-auto p-0" align="start">
                  <Calendar
                    mode="single"
                    selected={filterDateTo}
                    onSelect={setFilterDateTo}
                    initialFocus
                  />
                </PopoverContent>
              </Popover>
            </div>

            {/* Search */}
            <div>
              <label className="block text-xs font-medium text-gray-700 mb-1.5">
                Search
              </label>
              <Input
                type="text"
                placeholder="Search..."
                value={filterSearch}
                onChange={(e) => setFilterSearch(e.target.value)}
                className="h-9 text-xs"
              />
            </div>
          </div>

          {/* Filter Summary and Clear Button */}
          <div className="flex items-center justify-between mt-3 pt-3 border-t border-gray-200">
            <div className="text-xs text-gray-600">
              Showing <strong>{filteredEntries.length}</strong> of <strong>{entries.length}</strong> entries
              {(filterType !== "all" || filterMarket !== "all" || filterSearch || filterDateFrom || filterDateTo) && (
                <span className="ml-2 text-[#A08B5C]">(filtered)</span>
              )}
            </div>
            <Button
              size="sm"
              variant="outline"
              className="text-xs h-8"
              onClick={handleClearFilters}
            >
              Clear Filters
            </Button>
          </div>
        </CardContent>
      </Card>

      {/* Itinerary Table */}
      <Card>
        <CardHeader className="py-3">
          <div className="flex items-center justify-between">
            <CardTitle className="text-base">Itinerary Plan</CardTitle>
            <div className="flex gap-2">
              <Button
                size="sm"
                variant="outline"
                className="text-xs h-8"
                onClick={loadEntries}
                title="Refresh data from localStorage"
              >
                <RefreshCw className="w-3 h-3 mr-1.5" />
                Refresh
              </Button>
              <Button
                size="sm"
                variant="outline"
                className="text-xs h-8"
                onClick={handleExportToExcel}
              >
                <Download className="w-3 h-3 mr-1.5" />
                Export
              </Button>
              <Button size="sm" variant="outline" className="text-xs h-8">
                <Upload className="w-3 h-3 mr-1.5" />
                Import
              </Button>
              <Button
                size="sm"
                className="bg-[#A08B5C] hover:bg-[#8A7548] text-xs h-8"
                onClick={() => router.push('/lbcl/sales-itinerary/add-entry')}
              >
                <Plus className="w-3 h-3 mr-1.5" />
                Add Entry
              </Button>
            </div>
          </div>
        </CardHeader>
        <CardContent className="p-0">
          <div className="overflow-x-auto">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead className="text-xs py-2 w-8">#</TableHead>
                  <TableHead className="text-xs py-2 min-w-[150px]">Activity Type</TableHead>
                  <TableHead className="text-xs py-2 min-w-[180px]">Focus Area</TableHead>
                  <TableHead className="text-xs py-2 min-w-[100px]">Date</TableHead>
                  <TableHead className="text-xs py-2 min-w-[60px]">Day</TableHead>
                  <TableHead className="text-xs py-2 min-w-[250px]">KRA Plan</TableHead>
                  <TableHead className="text-xs py-2 min-w-[120px]">Morning Meeting</TableHead>
                  <TableHead className="text-xs py-2 min-w-[90px]">Time From</TableHead>
                  <TableHead className="text-xs py-2 min-w-[90px]">Time To</TableHead>
                  <TableHead className="text-xs py-2 min-w-[120px]">Market</TableHead>
                  <TableHead className="text-xs py-2 min-w-[120px]">Channel</TableHead>
                  <TableHead className="text-xs py-2 min-w-[100px]">Route No.</TableHead>
                  <TableHead className="text-xs py-2 min-w-[100px]">Outlet No.</TableHead>
                  <TableHead className="text-xs py-2 min-w-[150px]">Accompanied By</TableHead>
                  <TableHead className="text-xs py-2 min-w-[120px]">Night Out</TableHead>
                  <TableHead className="text-xs py-2 min-w-[90px]">No. of Days</TableHead>
                  <TableHead className="text-xs py-2 min-w-[100px]">Planned Mileage</TableHead>
                  <TableHead className="text-xs py-2 min-w-[200px]">KRA Review</TableHead>
                  <TableHead className="text-xs py-2 min-w-[200px]">Comments</TableHead>
                  <TableHead className="text-xs py-2 w-28 sticky right-0 bg-white">Actions</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {filteredEntries.length === 0 ? (
                  <TableRow>
                    <TableCell colSpan={20} className="text-center py-8 text-gray-500">
                      <div className="flex flex-col items-center gap-2">
                        {entries.length === 0 ? (
                          <>
                            <p className="text-sm">No itinerary entries found</p>
                            <p className="text-xs">Click "Add Entry" to create your first itinerary entry</p>
                          </>
                        ) : (
                          <>
                            <p className="text-sm">No entries match the current filters</p>
                            <p className="text-xs">Try adjusting or clearing the filters</p>
                          </>
                        )}
                      </div>
                    </TableCell>
                  </TableRow>
                ) : (
                  filteredEntries.map((entry, index) => {
                    const entryIsHoliday = isHoliday(entry.date);
                    return (
                      <TableRow key={entry.id} className={entryIsHoliday ? "bg-red-50" : ""}>
                        <TableCell className="text-xs py-2">{index + 1}</TableCell>
                        <TableCell className="text-xs py-2">
                          <Badge className="text-xs whitespace-normal" variant="outline">{entry.type}</Badge>
                        </TableCell>
                        <TableCell className="text-xs py-2 font-medium">{entry.focusArea}</TableCell>
                        <TableCell className="text-xs py-2">
                          <div className="flex items-center gap-1">
                            {entry.date}
                            {entryIsHoliday && (
                              <Badge variant="outline" className="text-[9px] bg-red-100 text-red-700 border-red-200 ml-1">
                                Holiday
                              </Badge>
                            )}
                          </div>
                        </TableCell>
                      <TableCell className="text-xs py-2">{entry.day}</TableCell>
                      <TableCell className="text-xs py-2" title={entry.kraPlan}>
                        {entry.kraPlan}
                      </TableCell>
                      <TableCell className="text-xs py-2">{entry.morningMeeting || "NA"}</TableCell>
                      <TableCell className="text-xs py-2 whitespace-nowrap">{entry.timeFrom}</TableCell>
                      <TableCell className="text-xs py-2 whitespace-nowrap">{entry.timeTo}</TableCell>
                      <TableCell className="text-xs py-2">{entry.market}</TableCell>
                      <TableCell className="text-xs py-2">{entry.channel || "NA"}</TableCell>
                      <TableCell className="text-xs py-2">{entry.routeNo || "NA"}</TableCell>
                      <TableCell className="text-xs py-2">{entry.outletNo || "NA"}</TableCell>
                      <TableCell className="text-xs py-2">{entry.accompaniedBy}</TableCell>
                      <TableCell className="text-xs py-2">{entry.nightOut}</TableCell>
                      <TableCell className="text-xs py-2 text-center">{entry.noOfDays}</TableCell>
                      <TableCell className="text-xs py-2 text-right">{entry.plannedMileage} km</TableCell>
                      <TableCell className="text-xs py-2">{entry.kraReview || "-"}</TableCell>
                      <TableCell className="text-xs py-2">{entry.comments || "-"}</TableCell>
                      <TableCell className="py-2 sticky right-0 bg-white">
                        <div className="flex gap-1">
                          <Button
                            size="icon"
                            variant="ghost"
                            className="h-7 w-7"
                            onClick={() => handleViewDetails(entry.id)}
                            title="View Details"
                          >
                            <Eye className="w-3 h-3 text-green-600" />
                          </Button>
                          <Button
                            size="icon"
                            variant="ghost"
                            className="h-7 w-7"
                            onClick={() => handleDeleteEntry(entry.id)}
                          >
                            <Trash2 className="w-3 h-3 text-red-600" />
                          </Button>
                        </div>
                      </TableCell>
                    </TableRow>
                    );
                  })
                )}
              </TableBody>
            </Table>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
