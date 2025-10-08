"use client";

import { useState } from "react";
import { Card, CardContent, CardHeader, CardTitle } from "@/app/lbcl/components/ui/card";
import { Button } from "@/app/lbcl/components/ui/button";
import { Input } from "@/app/lbcl/components/ui/input";
import { Badge } from "@/app/lbcl/components/ui/badge";
import { Calendar } from "@/app/lbcl/components/ui/calendar";
import { Search, Calendar as CalendarIcon, Plus } from "lucide-react";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/app/lbcl/components/ui/table";

interface Outlet {
  id: string;
  name: string;
  channel: string;
  chain: string;
  category: string;
  priority: "Low" | "Medium" | "High";
  location: string;
}

export function SalesItineraryCalendar() {
  const [view, setView] = useState<"day" | "week" | "month">("day");
  const [searchQuery, setSearchQuery] = useState("");
  const [date, setDate] = useState<Date | undefined>(new Date(2025, 5, 2)); // June 2, 2025
  const [showPreview, setShowPreview] = useState(false);

  // Mock outlet data
  const outlets: Outlet[] = [
    {
      id: "RT050105",
      name: "Diyasenpura",
      channel: "TOFT",
      chain: "ROC",
      category: "80% Contribution Outlets (LBCL)",
      priority: "Low",
      location: "North Anuradapura"
    },
    {
      id: "RT050115",
      name: "Gajamuthu Upali",
      channel: "TOFT",
      chain: "GRD",
      category: "80% Contribution Outlets (Heineken)",
      priority: "High",
      location: "North Anuradapura"
    },
    {
      id: "RT050117",
      name: "Ajith Putha Ws - Rambewa",
      channel: "TOFT",
      chain: "AJP",
      category: "Special Outlets",
      priority: "High",
      location: "North Anuradapura"
    },
    {
      id: "RT050163",
      name: "Medawachchiya",
      channel: "TOFT",
      chain: "N/A",
      category: "Activity (National, Brand, Tourist Board License)",
      priority: "Medium",
      location: "North Anuradapura"
    },
    {
      id: "RT050168",
      name: "Midland Ws",
      channel: "TOFT",
      chain: "MID",
      category: "Special Outlets",
      priority: "High",
      location: "North Anuradapura"
    },
    {
      id: "RT050215",
      name: "Thambuthegama Ws",
      channel: "TOFT",
      chain: "ROC",
      category: "80% Contribution Outlets (Heineken)",
      priority: "High",
      location: "North Anuradapura"
    },
    {
      id: "RT050125",
      name: "Grand WS",
      channel: "TOFT",
      chain: "GRD",
      category: "Activity (National, Brand, Tourist Board License)",
      priority: "Medium",
      location: "North Anuradapura"
    },
    {
      id: "RT050153",
      name: "Luxman Wine Stores",
      channel: "TOFT",
      chain: "AJP",
      category: "80% Contribution Outlets (LBCL)",
      priority: "Low",
      location: "North Anuradapura"
    },
  ];

  const configSummary = [
    { count: 14, label: "80% Contribution Outlets (LBCL)", color: "bg-yellow-200" },
    { count: 75, label: "80% Contribution Outlets (Heineken)", color: "bg-orange-300" },
    { count: 30, label: "Special Outlets", color: "bg-gray-800" },
    { count: 4, label: "Activity (National, Brand, Tourist board license)", color: "bg-blue-200" },
  ];

  const filteredOutlets = outlets.filter(outlet =>
    outlet.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
    outlet.id.toLowerCase().includes(searchQuery.toLowerCase())
  );

  const getPriorityColor = (priority: string) => {
    switch (priority) {
      case "High":
        return "text-red-600";
      case "Medium":
        return "text-yellow-600";
      case "Low":
        return "text-green-600";
      default:
        return "text-gray-600";
    }
  };

  return (
    <div className="space-y-4">
      {!showPreview ? (
        <>
          {/* Dashboard View - First Screen */}
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            <Card className="bg-yellow-50 border-yellow-200">
              <CardContent className="p-4">
                <h3 className="text-xs font-semibold text-gray-700 mb-2">Planned Rate</h3>
                <div className="text-3xl font-bold text-gray-900 mb-2">100%</div>
                <div className="w-full bg-gray-200 h-2 rounded-full mb-1">
                  <div className="bg-[#A08B5C] h-2 rounded-full" style={{ width: "100%" }}></div>
                </div>
                <p className="text-xs text-gray-600">Target: 80% Minimum</p>
              </CardContent>
            </Card>

            <Card className="bg-yellow-50 border-yellow-200">
              <CardContent className="p-4">
                <h3 className="text-xs font-semibold text-gray-700 mb-2">Scheduled Outlets</h3>
                <div className="text-3xl font-bold text-gray-900 mb-2">123/123</div>
                <p className="text-xs text-gray-600">Total Qualified Outlets In Plan</p>
              </CardContent>
            </Card>

            <Card className="bg-yellow-50 border-yellow-200">
              <CardContent className="p-4">
                <h3 className="text-xs font-semibold text-gray-700 mb-2">Configuration Summary</h3>
                <div className="space-y-1 text-xs">
                  <div className="flex items-center gap-2">
                    <span className="w-3 h-3 bg-yellow-200 rounded"></span>
                    <span><strong>14</strong> 80% Contribution Outlets (LBCL)</span>
                  </div>
                  <div className="flex items-center gap-2">
                    <span className="w-3 h-3 bg-orange-300 rounded"></span>
                    <span><strong>75</strong> 80% Contribution Outlets (Heineken)</span>
                  </div>
                  <div className="flex items-center gap-2">
                    <span className="w-3 h-3 bg-gray-800 rounded"></span>
                    <span><strong>30</strong> Special Outlets</span>
                  </div>
                  <div className="flex items-center gap-2">
                    <span className="w-3 h-3 bg-blue-200 rounded"></span>
                    <span><strong>04</strong> Activity (National, Brand, Tourist board license)</span>
                  </div>
                </div>
              </CardContent>
            </Card>
          </div>

          {/* Month Selector */}
          <div className="flex gap-3 border-b-2 border-gray-200">
            <Button
              variant="ghost"
              className="px-6 py-3 rounded-none font-semibold text-sm border-b-3 border-[#A08B5C] text-[#A08B5C] bg-[#A08B5C]/5"
            >
              CURRENT MONTH (MAY 2025)
            </Button>
            <Button variant="ghost" className="px-6 py-3 rounded-none font-semibold text-sm text-gray-600 hover:text-[#A08B5C]">
              NEXT MONTH (JUNE 2025)
            </Button>
          </div>

          {/* Chart Placeholder */}
          <Card>
            <CardHeader className="py-3">
              <CardTitle className="text-base">June 2025 - Sales Itinerary Planning (Approved)</CardTitle>
            </CardHeader>
            <CardContent className="p-4">
              <div className="h-48 flex items-center justify-center bg-gray-50 rounded-lg border-2 border-dashed border-gray-300">
                <div className="text-center">
                  <CalendarIcon className="w-10 h-10 mx-auto text-gray-400 mb-2" />
                  <p className="text-sm text-gray-500 mb-3">Bar chart visualization would appear here</p>
                  <Button
                    onClick={() => setShowPreview(true)}
                    className="bg-[#A08B5C] hover:bg-[#8A7548] text-sm px-4 py-2"
                  >
                    View Configuration Preview
                  </Button>
                </div>
              </div>
            </CardContent>
          </Card>
        </>
      ) : (
        <>
          {/* Preview Configuration Screen */}
          <div className="flex items-center justify-between mb-3">
            <h2 className="text-xl font-bold">Preview Sales Itinerary Configuration</h2>
            <Button onClick={() => setShowPreview(false)} variant="outline" size="sm">
              Back to Dashboard
            </Button>
          </div>

          <div className="grid grid-cols-1 lg:grid-cols-4 gap-4">
            {/* Left Side - Calendar and Summary */}
            <div className="lg:col-span-1 space-y-3">
              <Card>
                <CardContent className="p-3">
                  <div className="flex items-center justify-center mb-3">
                    <Button variant="ghost" size="sm">←</Button>
                    <h3 className="text-sm font-semibold mx-4">June 2025</h3>
                    <Button variant="ghost" size="sm">→</Button>
                  </div>
                  <Calendar
                    mode="single"
                    selected={date}
                    onSelect={setDate}
                    className="rounded-md border text-sm"
                  />
                  <div className="mt-3">
                    <Button size="icon" className="rounded-full bg-[#A08B5C] hover:bg-[#8A7548] h-8 w-8">
                      <Plus className="w-4 h-4" />
                    </Button>
                  </div>
                </CardContent>
              </Card>

              <Card>
                <CardContent className="p-3">
                  <h3 className="text-sm font-semibold mb-2">Scheduled Outlets</h3>
                  <div className="text-2xl font-bold text-center">123/123</div>
                  <p className="text-xs text-center text-gray-600">Total Qualified Outlets In Plan</p>
                </CardContent>
              </Card>

              <Card>
                <CardContent className="p-3">
                  <h3 className="text-sm font-semibold mb-2">Configuration Summary</h3>
                  <div className="space-y-1.5">
                    {configSummary.map((item, index) => (
                      <div key={index} className="flex items-start gap-2 text-xs">
                        <span className={`w-3 h-3 rounded mt-0.5 ${item.color}`}></span>
                        <span className="flex-1">
                          <strong>{item.count}</strong> {item.label}
                        </span>
                      </div>
                    ))}
                  </div>
                </CardContent>
              </Card>

              <Button className="w-full bg-[#A08B5C] hover:bg-[#8A7548] text-sm py-2">
                Submit for Review
              </Button>
            </div>

            {/* Right Side - Outlet List */}
            <div className="lg:col-span-3">
              <Card>
                <CardHeader className="py-3">
                  <div className="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-3">
                    <div className="flex gap-2">
                      <Button size="sm" variant="default" className="bg-[#A08B5C] hover:bg-[#8A7548] text-xs h-8">Day</Button>
                      <Button size="sm" variant="outline" className="text-xs h-8">Week</Button>
                      <Button size="sm" variant="outline" className="text-xs h-8">Month</Button>
                    </div>
                    <div className="flex items-center gap-2 w-full sm:w-auto">
                      <div className="relative flex-1 sm:flex-initial">
                        <Search className="absolute left-2 top-2 h-3 w-3 text-gray-500" />
                        <Input
                          placeholder="Search"
                          className="pl-7 h-8 text-sm w-full sm:w-48"
                          value={searchQuery}
                          onChange={(e) => setSearchQuery(e.target.value)}
                        />
                      </div>
                      <Button className="bg-[#A08B5C] hover:bg-[#8A7548] text-xs h-8 whitespace-nowrap">
                        Add Outlet
                      </Button>
                    </div>
                  </div>
                </CardHeader>
                <CardContent className="p-0">
                  <div className="overflow-x-auto">
                    <Table>
                      <TableHeader>
                        <TableRow>
                          <TableHead className="text-xs py-2">Outlet Name</TableHead>
                          <TableHead className="text-xs py-2">Channel</TableHead>
                          <TableHead className="text-xs py-2">Chain</TableHead>
                          <TableHead className="text-xs py-2">Category</TableHead>
                          <TableHead className="text-xs py-2">Priority</TableHead>
                          <TableHead className="text-xs py-2">Location</TableHead>
                          <TableHead className="text-xs py-2">Change Date</TableHead>
                        </TableRow>
                      </TableHeader>
                      <TableBody>
                        {filteredOutlets.map((outlet) => (
                          <TableRow key={outlet.id}>
                            <TableCell className="font-medium text-xs py-2">
                              {outlet.id} {outlet.name}
                            </TableCell>
                            <TableCell className="text-xs py-2">{outlet.channel}</TableCell>
                            <TableCell className="text-xs py-2">{outlet.chain}</TableCell>
                            <TableCell className="py-2">
                              <span className="text-xs">{outlet.category}</span>
                            </TableCell>
                            <TableCell className="py-2">
                              <span className={`text-xs font-semibold ${getPriorityColor(outlet.priority)}`}>
                                {outlet.priority}
                              </span>
                            </TableCell>
                            <TableCell className="text-xs py-2">{outlet.location}</TableCell>
                            <TableCell className="py-2">
                              <Button variant="outline" size="sm" className="text-xs h-7 px-2">
                                📅 Set Date
                              </Button>
                            </TableCell>
                          </TableRow>
                        ))}
                      </TableBody>
                    </Table>
                  </div>
                </CardContent>
              </Card>
            </div>
          </div>
        </>
      )}
    </div>
  );
}
