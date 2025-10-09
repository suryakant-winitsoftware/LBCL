"use client";

import { useState, useEffect } from "react";
import { useRouter } from "next/navigation";
import { Card, CardContent, CardHeader, CardTitle } from "@/app/lbcl/components/ui/card";
import { Button } from "@/app/lbcl/components/ui/button";
import { Badge } from "@/app/lbcl/components/ui/badge";
import { Plus, Trash2, Download, Upload, RefreshCw, Eye, CalendarIcon } from "lucide-react";
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

export function SalesItineraryTemplate() {
  const router = useRouter();
  const [entries, setEntries] = useState<ItineraryEntry[]>([]);
  const [filteredEntries, setFilteredEntries] = useState<ItineraryEntry[]>([]);

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
                  filteredEntries.map((entry, index) => (
                    <TableRow key={entry.id}>
                      <TableCell className="text-xs py-2">{index + 1}</TableCell>
                      <TableCell className="text-xs py-2">
                        <Badge className="text-xs whitespace-normal" variant="outline">{entry.type}</Badge>
                      </TableCell>
                      <TableCell className="text-xs py-2 font-medium">{entry.focusArea}</TableCell>
                      <TableCell className="text-xs py-2">{entry.date}</TableCell>
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
                  ))
                )}
              </TableBody>
            </Table>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
