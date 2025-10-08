"use client";

import { useState, useEffect } from "react";
import { useRouter } from "next/navigation";
import { Card, CardContent, CardHeader, CardTitle } from "@/app/lbcl/components/ui/card";
import { Button } from "@/app/lbcl/components/ui/button";
import { Badge } from "@/app/lbcl/components/ui/badge";
import { Plus, Edit, Trash2, Download, Upload, RefreshCw, Eye } from "lucide-react";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from "@/app/lbcl/components/ui/dialog";
import * as XLSX from "xlsx";
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
  ACTIVITY_SUMMARY,
} from "./itinerary-mock-data";

export function SalesItineraryTemplate() {
  const router = useRouter();
  const [entries, setEntries] = useState<ItineraryEntry[]>([]);
  const [selectedEntry, setSelectedEntry] = useState<ItineraryEntry | null>(null);
  const [isDetailOpen, setIsDetailOpen] = useState(false);

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

  const handleViewDetails = (entry: ItineraryEntry) => {
    setSelectedEntry(entry);
    setIsDetailOpen(true);
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

  // Calculate summary statistics
  const totalEntries = entries.length;
  const totalMileage = entries.reduce((sum, entry) => sum + (entry.plannedMileage || 0), 0);
  const totalDays = entries.reduce((sum, entry) => sum + (entry.noOfDays || 0), 0);

  return (
    <div className="space-y-4">
      {/* Header with Summary Stats */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-3">
        <Card className="border-[#A08B5C] border-2">
          <CardContent className="p-3 text-center">
            <h3 className="text-xs text-gray-600 mb-1">Total Entries</h3>
            <div className="text-2xl font-bold text-[#A08B5C]">{totalEntries}</div>
          </CardContent>
        </Card>
        <Card className="border-[#A08B5C] border-2">
          <CardContent className="p-3 text-center">
            <h3 className="text-xs text-gray-600 mb-1">Total Days</h3>
            <div className="text-2xl font-bold text-[#A08B5C]">{totalDays}</div>
          </CardContent>
        </Card>
        <Card className="border-[#A08B5C] border-2">
          <CardContent className="p-3 text-center">
            <h3 className="text-xs text-gray-600 mb-1">Planned Mileage</h3>
            <div className="text-2xl font-bold text-[#A08B5C]">{totalMileage} km</div>
          </CardContent>
        </Card>
        <Card className="border-[#A08B5C] border-2">
          <CardContent className="p-3 text-center">
            <h3 className="text-xs text-gray-600 mb-1">Completion</h3>
            <div className="text-2xl font-bold text-[#A08B5C]">100%</div>
          </CardContent>
        </Card>
      </div>

      {/* Activity Summary */}
      <Card>
        <CardHeader className="py-3">
          <CardTitle className="text-base">Activity Summary</CardTitle>
        </CardHeader>
        <CardContent className="p-4">
          <div className="grid grid-cols-2 md:grid-cols-5 gap-3">
            {Object.entries(ACTIVITY_SUMMARY).map(([activity, count]) => (
              <div key={activity} className="text-center p-2 bg-gray-50 rounded">
                <div className="text-xl font-bold text-[#A08B5C]">{count}</div>
                <div className="text-xs text-gray-600">{activity}</div>
              </div>
            ))}
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
              <Button size="sm" variant="outline" className="text-xs h-8">
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
                  <TableHead className="text-xs py-2">Type</TableHead>
                  <TableHead className="text-xs py-2">Focus Area</TableHead>
                  <TableHead className="text-xs py-2">Date</TableHead>
                  <TableHead className="text-xs py-2">Day</TableHead>
                  <TableHead className="text-xs py-2">KRA Plan</TableHead>
                  <TableHead className="text-xs py-2">Time</TableHead>
                  <TableHead className="text-xs py-2">Market</TableHead>
                  <TableHead className="text-xs py-2">Accompanied By</TableHead>
                  <TableHead className="text-xs py-2">Mileage</TableHead>
                  <TableHead className="text-xs py-2 w-20">Actions</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {entries.length === 0 ? (
                  <TableRow>
                    <TableCell colSpan={11} className="text-center py-8 text-gray-500">
                      <div className="flex flex-col items-center gap-2">
                        <p className="text-sm">No itinerary entries found</p>
                        <p className="text-xs">Click "Add Entry" to create your first itinerary entry</p>
                      </div>
                    </TableCell>
                  </TableRow>
                ) : (
                  entries.map((entry, index) => (
                    <TableRow key={entry.id}>
                      <TableCell className="text-xs py-2">{index + 1}</TableCell>
                      <TableCell className="text-xs py-2">
                        <Badge className="text-xs" variant="outline">{entry.type}</Badge>
                      </TableCell>
                      <TableCell className="text-xs py-2 font-medium">{entry.focusArea}</TableCell>
                      <TableCell className="text-xs py-2">{entry.date}</TableCell>
                      <TableCell className="text-xs py-2">{entry.day}</TableCell>
                      <TableCell className="text-xs py-2 max-w-xs truncate" title={entry.kraPlan}>
                        {entry.kraPlan}
                      </TableCell>
                      <TableCell className="text-xs py-2 whitespace-nowrap">
                        {entry.timeFrom} - {entry.timeTo}
                      </TableCell>
                      <TableCell className="text-xs py-2">{entry.market}</TableCell>
                      <TableCell className="text-xs py-2">{entry.accompaniedBy}</TableCell>
                      <TableCell className="text-xs py-2">{entry.plannedMileage} km</TableCell>
                      <TableCell className="py-2">
                        <div className="flex gap-1">
                          <Button size="icon" variant="ghost" className="h-7 w-7">
                            <Edit className="w-3 h-3 text-blue-600" />
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
