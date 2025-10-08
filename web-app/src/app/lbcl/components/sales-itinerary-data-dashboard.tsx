"use client";

import { useState, useEffect } from "react";
import { Card, CardContent, CardHeader, CardTitle } from "@/app/lbcl/components/ui/card";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/app/lbcl/components/ui/select";
import { Progress } from "@/app/lbcl/components/ui/progress";
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, ResponsiveContainer, Legend, Tooltip, PieChart, Pie, Cell } from "recharts";
import { Loader2 } from "lucide-react";

interface ItineraryEntry {
  focusArea: string;
  date: string;
  day: string;
  kraPlan: string;
  market: string;
  channel: string;
  accompaniedBy: string;
  plannedMileage: number;
  noOfDays: number;
}

interface ApiResponse {
  success: boolean;
  data: {
    metadata: {
      name: string;
      cluster: string;
      month: string;
      date: string;
    };
    entries: ItineraryEntry[];
    filters: {
      focusAreas: string[];
      markets: string[];
      channels: string[];
      accompaniedBy: string[];
    };
    summary: {
      meetingsTrainings: number;
      coaching: number;
      event: number;
    };
  };
}

export function SalesItineraryDataDashboard() {
  const [loading, setLoading] = useState(true);
  const [data, setData] = useState<ApiResponse["data"] | null>(null);
  const [selectedFocusArea, setSelectedFocusArea] = useState<string>("all");
  const [selectedMarket, setSelectedMarket] = useState<string>("all");
  const [selectedAccompaniedBy, setSelectedAccompaniedBy] = useState<string>("all");

  useEffect(() => {
    fetchData();
  }, []);

  const fetchData = async () => {
    try {
      setLoading(true);
      const response = await fetch("/api/sales-itinerary");
      const result = await response.json();
      if (result.success) {
        setData(result.data);
      }
    } catch (error) {
      console.error("Error fetching data:", error);
    } finally {
      setLoading(false);
    }
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <Loader2 className="w-8 h-8 animate-spin text-[#A08B5C]" />
      </div>
    );
  }

  if (!data) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="text-center">
          <p className="text-red-500">Failed to load data</p>
        </div>
      </div>
    );
  }

  // Filter entries based on selected filters
  const filteredEntries = data.entries.filter((entry) => {
    if (selectedFocusArea !== "all" && entry.focusArea !== selectedFocusArea) return false;
    if (selectedMarket !== "all" && entry.market !== selectedMarket) return false;
    if (selectedAccompaniedBy !== "all" && entry.accompaniedBy !== selectedAccompaniedBy) return false;
    return true;
  });

  // Calculate statistics from filtered data
  const totalEntries = filteredEntries.length;
  const totalMileage = filteredEntries.reduce((sum, entry) => sum + (entry.plannedMileage || 0), 0);
  const totalDays = filteredEntries.reduce((sum, entry) => sum + (entry.noOfDays || 0), 0);

  // Prepare chart data - Group by Focus Area
  const focusAreaCounts: { [key: string]: number } = {};
  filteredEntries.forEach((entry) => {
    if (entry.focusArea) {
      focusAreaCounts[entry.focusArea] = (focusAreaCounts[entry.focusArea] || 0) + 1;
    }
  });

  const chartData = Object.entries(focusAreaCounts)
    .map(([name, count]) => ({ name, count }))
    .sort((a, b) => b.count - a.count)
    .slice(0, 10); // Top 10

  // Prepare market distribution for pie chart
  const marketCounts: { [key: string]: number } = {};
  filteredEntries.forEach((entry) => {
    if (entry.market && entry.market !== "NA") {
      marketCounts[entry.market] = (marketCounts[entry.market] || 0) + 1;
    }
  });

  const pieData = Object.entries(marketCounts)
    .map(([name, value]) => ({ name, value }))
    .sort((a, b) => b.value - a.value);

  const COLORS = ["#F4D03F", "#E67E22", "#34495E", "#85C1E9", "#A569BD", "#45B7D1", "#96CEB4", "#FF6B6B"];

  return (
    <div className="space-y-5 pb-6 w-full">
      {/* Filters */}
      <Card>
        <CardHeader className="py-3">
          <CardTitle className="text-base">Filters</CardTitle>
        </CardHeader>
        <CardContent className="p-4">
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            <div>
              <label className="block text-xs font-medium text-gray-700 mb-1.5">
                Focus Area
              </label>
              <Select value={selectedFocusArea} onValueChange={setSelectedFocusArea}>
                <SelectTrigger className="h-9 text-sm">
                  <SelectValue placeholder="All Focus Areas" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">All Focus Areas</SelectItem>
                  {data.filters.focusAreas.map((area) => (
                    <SelectItem key={area} value={area}>
                      {area}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            <div>
              <label className="block text-xs font-medium text-gray-700 mb-1.5">
                Market
              </label>
              <Select value={selectedMarket} onValueChange={setSelectedMarket}>
                <SelectTrigger className="h-9 text-sm">
                  <SelectValue placeholder="All Markets" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">All Markets</SelectItem>
                  {data.filters.markets.map((market) => (
                    <SelectItem key={market} value={market}>
                      {market}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            <div>
              <label className="block text-xs font-medium text-gray-700 mb-1.5">
                Accompanied By
              </label>
              <Select value={selectedAccompaniedBy} onValueChange={setSelectedAccompaniedBy}>
                <SelectTrigger className="h-9 text-sm">
                  <SelectValue placeholder="All" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">All</SelectItem>
                  {data.filters.accompaniedBy.map((person) => (
                    <SelectItem key={person} value={person}>
                      {person}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* KPI Cards */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4 w-full">
        <Card className="bg-gradient-to-br from-yellow-50 to-yellow-100 border-yellow-200 shadow-sm hover:shadow-md transition-shadow">
          <CardContent className="p-4">
            <h3 className="text-xs font-semibold text-gray-700 mb-2 uppercase tracking-wide">Total Entries</h3>
            <div className="text-3xl font-bold text-[#A08B5C] mb-2">{totalEntries}</div>
            <p className="text-xs text-gray-600">Filtered activities</p>
          </CardContent>
        </Card>

        <Card className="bg-gradient-to-br from-yellow-50 to-yellow-100 border-yellow-200 shadow-sm hover:shadow-md transition-shadow">
          <CardContent className="p-4">
            <h3 className="text-xs font-semibold text-gray-700 mb-2 uppercase tracking-wide">Total Mileage</h3>
            <div className="text-3xl font-bold text-[#A08B5C] mb-2">{totalMileage} km</div>
            <p className="text-xs text-gray-600">Planned travel distance</p>
          </CardContent>
        </Card>

        <Card className="bg-gradient-to-br from-yellow-50 to-yellow-100 border-yellow-200 shadow-sm hover:shadow-md transition-shadow">
          <CardContent className="p-4">
            <h3 className="text-xs font-semibold text-gray-700 mb-2 uppercase tracking-wide">Total Days</h3>
            <div className="text-3xl font-bold text-[#A08B5C] mb-2">{totalDays}</div>
            <p className="text-xs text-gray-600">Activity days</p>
          </CardContent>
        </Card>
      </div>

      {/* Charts */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
        {/* Bar Chart - Top Focus Areas */}
        <Card className="shadow-sm">
          <CardHeader className="border-b bg-gray-50 py-3">
            <CardTitle className="text-base font-bold text-gray-800">
              Top Focus Areas
            </CardTitle>
          </CardHeader>
          <CardContent className="p-4 sm:p-5">
            <div className="w-full h-[350px]">
              <ResponsiveContainer width="100%" height="100%">
                <BarChart
                  data={chartData}
                  margin={{ top: 10, right: 10, left: 10, bottom: 70 }}
                >
                  <CartesianGrid strokeDasharray="3 3" stroke="#e5e7eb" vertical={false} />
                  <XAxis
                    dataKey="name"
                    tick={{ fontSize: 10, fill: '#4B5563' }}
                    angle={-45}
                    textAnchor="end"
                    height={100}
                    interval={0}
                  />
                  <YAxis
                    tick={{ fontSize: 11, fill: '#4B5563' }}
                    label={{
                      value: 'Count',
                      angle: -90,
                      position: 'insideLeft',
                      style: { fontSize: 12, fill: '#374151', fontWeight: 600 }
                    }}
                  />
                  <Tooltip
                    contentStyle={{
                      backgroundColor: 'white',
                      border: '1px solid #A08B5C',
                      borderRadius: '6px',
                      padding: '8px',
                      fontSize: '12px',
                    }}
                  />
                  <Bar dataKey="count" fill="#F4D03F" radius={[6, 6, 0, 0]} />
                </BarChart>
              </ResponsiveContainer>
            </div>
          </CardContent>
        </Card>

        {/* Pie Chart - Market Distribution */}
        <Card className="shadow-sm">
          <CardHeader className="border-b bg-gray-50 py-3">
            <CardTitle className="text-base font-bold text-gray-800">
              Market Distribution
            </CardTitle>
          </CardHeader>
          <CardContent className="p-4 sm:p-5">
            <div className="w-full h-[350px]">
              <ResponsiveContainer width="100%" height="100%">
                <PieChart>
                  <Pie
                    data={pieData}
                    cx="50%"
                    cy="50%"
                    labelLine={false}
                    label={({ name, percent }) => `${name} (${(percent * 100).toFixed(0)}%)`}
                    outerRadius={100}
                    fill="#8884d8"
                    dataKey="value"
                  >
                    {pieData.map((entry, index) => (
                      <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                    ))}
                  </Pie>
                  <Tooltip />
                </PieChart>
              </ResponsiveContainer>
            </div>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
