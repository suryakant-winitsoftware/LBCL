"use client";

import { useState, useEffect } from "react";
import { Card, CardContent, CardHeader, CardTitle } from "@/app/lbcl/components/ui/card";
import { Button } from "@/app/lbcl/components/ui/button";
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, ResponsiveContainer, Legend, Tooltip } from "recharts";
import { ItineraryEntry, MOCK_ITINERARY_DATA } from "./itinerary-mock-data";

export function SalesItineraryDashboard() {
  const [entries, setEntries] = useState<ItineraryEntry[]>([]);
  const [selectedMonth, setSelectedMonth] = useState<string>("");

  // Helper function to parse DD/MM/YY date format
  const parseDateString = (dateStr: string): Date | null => {
    if (!dateStr) return null;
    const parts = dateStr.split('/');
    if (parts.length !== 3) return null;
    const day = parseInt(parts[0], 10);
    const month = parseInt(parts[1], 10) - 1; // Month is 0-indexed
    const year = parseInt(parts[2], 10) + 2000; // Convert YY to YYYY
    return new Date(year, month, day);
  };

  // Load entries from localStorage
  useEffect(() => {
    const loadEntries = () => {
      const savedEntries = localStorage.getItem('itinerary_entries');
      if (savedEntries) {
        const parsedEntries = JSON.parse(savedEntries);
        setEntries(parsedEntries);
      } else {
        // If no saved entries, use mock data
        setEntries(MOCK_ITINERARY_DATA);
      }
    };

    loadEntries();

    // Reload when window gains focus
    const handleFocus = () => loadEntries();
    window.addEventListener('focus', handleFocus);
    return () => window.removeEventListener('focus', handleFocus);
  }, []);

  // Get available months from entries
  const availableMonths = Array.from(
    new Set(
      entries.map(entry => {
        const date = parseDateString(entry.date);
        if (!date) return '';
        return `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}`;
      }).filter(m => m !== '')
    )
  ).sort();

  // Set initial selected month
  useEffect(() => {
    if (availableMonths.length > 0 && !selectedMonth) {
      setSelectedMonth(availableMonths[0]);
    }
  }, [availableMonths, selectedMonth]);

  // Filter entries by selected month
  const monthEntries = entries.filter(entry => {
    const date = parseDateString(entry.date);
    if (!date) return false;
    const entryMonth = `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}`;
    return entryMonth === selectedMonth;
  });

  // Group entries by date for chart - create daily entries
  const chartData: Record<string, any> = {};

  // Get all dates in the month to show every day
  if (selectedMonth) {
    const [year, month] = selectedMonth.split('-').map(Number);
    const daysInMonth = new Date(year, month, 0).getDate();

    for (let day = 1; day <= daysInMonth; day++) {
      const currentDate = new Date(year, month - 1, day);
      const dateKey = currentDate.toLocaleDateString('en-US', { month: 'short', day: 'numeric' });
      const dayName = currentDate.toLocaleDateString('en-US', { weekday: 'short' });
      const isSunday = currentDate.getDay() === 0;

      chartData[dateKey] = {
        date: dateKey,
        fullDate: currentDate,
        dayName: dayName,
        isSunday: isSunday,
        sundayBg: isSunday ? 1 : 0, // Background bar for Sundays
        meetings: 0,
        coaching: 0,
        observations: 0,
        planning: 0,
        trainings: 0,
        events: 0,
        leave: 0,
        total: 0,
      };
    }
  }

  // Fill in actual data
  monthEntries.forEach(entry => {
    const date = parseDateString(entry.date);
    if (!date) return;
    const dateKey = date.toLocaleDateString('en-US', { month: 'short', day: 'numeric' });

    if (!chartData[dateKey]) return;

    // Map activity types to chart categories
    if (entry.type.includes("Meetings")) {
      chartData[dateKey].meetings += 1;
    } else if (entry.type.includes("Coaching") || entry.type.includes("Core Services")) {
      chartData[dateKey].coaching += 1;
    } else if (entry.type.includes("Observations") || entry.type.includes("Night Visits")) {
      chartData[dateKey].observations += 1;
    } else if (entry.type.includes("Planning") || entry.type.includes("Admin")) {
      chartData[dateKey].planning += 1;
    } else if (entry.type.includes("Trainings")) {
      chartData[dateKey].trainings += 1;
    } else if (entry.type.includes("Events")) {
      chartData[dateKey].events += 1;
    } else if (entry.type.includes("Leave")) {
      chartData[dateKey].leave += 1;
    }

    chartData[dateKey].total += 1;
  });

  const chartDataArray = Object.values(chartData).sort((a: any, b: any) => {
    return a.fullDate.getTime() - b.fullDate.getTime();
  });

  // Calculate max value for Sunday background bars
  const maxTotal = Math.max(...chartDataArray.map((d: any) => d.total), 20);
  chartDataArray.forEach((d: any) => {
    if (d.isSunday && d.total === 0) {
      d.sundayBg = maxTotal; // Full height for empty Sundays
    } else if (d.isSunday) {
      d.sundayBg = d.total; // Match height for Sundays with data
    }
  });

  // Calculate KPIs
  const totalEntries = monthEntries.length;
  const totalDays = new Set(monthEntries.map(e => e.date)).size;
  const totalMileage = monthEntries.reduce((sum, entry) => sum + (entry.plannedMileage || 0), 0);
  const avgOutletsPerDay = totalDays > 0 ? Math.round(totalEntries / totalDays) : 0;

  // Activity type summary
  const activitySummary = monthEntries.reduce((acc, entry) => {
    let category = "Other";

    if (entry.type.includes("Meetings")) {
      category = "Meetings";
    } else if (entry.type.includes("Coaching") || entry.type.includes("Core Services")) {
      category = "Coaching";
    } else if (entry.type.includes("Observations") || entry.type.includes("Night Visits")) {
      category = "Observations";
    } else if (entry.type.includes("Planning") || entry.type.includes("Admin")) {
      category = "Planning";
    } else if (entry.type.includes("Trainings")) {
      category = "Trainings";
    } else if (entry.type.includes("Events")) {
      category = "Events";
    } else if (entry.type.includes("Leave")) {
      category = "Leave";
    }

    acc[category] = (acc[category] || 0) + 1;
    return acc;
  }, {} as Record<string, number>);

  // Format month label
  const monthLabel = selectedMonth
    ? new Date(selectedMonth + "-01").toLocaleDateString('en-US', { month: 'long', year: 'numeric' })
    : "";

  // Calculate planned rate (assuming target is avgOutletsPerDay * totalDays)
  const targetOutlets = 6 * 22; // Average 6 outlets per day for 22 working days
  const plannedRate = targetOutlets > 0 ? Math.min(100, Math.round((totalEntries / targetOutlets) * 100)) : 0;

  return (
    <div className="space-y-5 pb-6 w-full">
      {/* Month Selector - Current/Next Month Toggle */}
      {availableMonths.length > 0 && (
        <div className="flex gap-0 w-full border-b border-gray-300">
          {availableMonths.slice(0, 2).map((month, index) => {
            const date = new Date(month + "-01");
            const label = date.toLocaleDateString('en-US', { month: 'long', year: 'numeric' }).toUpperCase();
            const prefix = index === 0 ? "CURRENT MONTH" : "NEXT MONTH";

            return (
              <button
                key={month}
                onClick={() => setSelectedMonth(month)}
                className={`flex-1 px-4 py-2.5 text-center font-semibold transition-all text-xs tracking-wide ${
                  selectedMonth === month
                    ? "border-b-3 border-[#A08B5C] text-[#A08B5C] bg-white"
                    : "text-gray-500 bg-gray-100 hover:bg-gray-50"
                }`}
              >
                <span className="text-[10px] font-medium text-gray-600 block mb-0.5">{prefix}</span>
                <span className="text-xs font-bold">{label}</span>
              </button>
            );
          })}
        </div>
      )}

      {/* Chart */}
      {chartDataArray.length > 0 ? (
        <Card className="shadow-sm w-full">
          <CardHeader className="border-b bg-white py-3">
            <CardTitle className="text-sm font-semibold text-gray-800">
              {monthLabel} - Sales Itinerary Planning (Approved)
            </CardTitle>
          </CardHeader>
          <CardContent className="p-4">
            <div className="w-full overflow-x-auto">
              <div style={{ width: '100%', minWidth: '1200px', height: '420px' }}>
                <ResponsiveContainer width="100%" height="100%">
                  <BarChart
                    data={chartDataArray}
                    margin={{ top: 10, right: 140, left: 30, bottom: 30 }}
                    barCategoryGap="8%"
                  >
                    <CartesianGrid strokeDasharray="3 3" stroke="#d1d5db" strokeOpacity={0.6} vertical={false} />
                    <XAxis
                      dataKey="date"
                      tick={{ fontSize: 9, fill: '#6b7280' }}
                      height={30}
                      interval={0}
                      axisLine={{ stroke: '#9ca3af' }}
                      tickLine={false}
                    />
                    <YAxis
                      tick={{ fontSize: 10, fill: '#6b7280' }}
                      axisLine={{ stroke: '#9ca3af' }}
                      tickLine={false}
                      allowDecimals={false}
                      domain={[0, 'auto']}
                    />
                    <Tooltip
                      contentStyle={{
                        backgroundColor: '#fff',
                        border: '1px solid #d1d5db',
                        borderRadius: '6px',
                        padding: '8px 12px',
                        fontSize: '11px',
                      }}
                      cursor={{ fill: 'rgba(0,0,0,0.05)' }}
                    />
                    <Legend
                      wrapperStyle={{
                        fontSize: '10px',
                        paddingRight: '5px'
                      }}
                      iconType="square"
                      iconSize={10}
                      layout="vertical"
                      align="right"
                      verticalAlign="middle"
                    />
                    {/* Sunday background */}
                    <Bar
                      dataKey="sundayBg"
                      fill="#FFC0CB"
                      fillOpacity={0.3}
                      name="Sunday"
                      radius={[4, 4, 0, 0]}
                    />
                    {/* Activity bars */}
                    <Bar
                      dataKey="meetings"
                      stackId="a"
                      fill="#8B6914"
                      name="Meetings"
                    />
                    <Bar
                      dataKey="coaching"
                      stackId="a"
                      fill="#D4AF37"
                      name="Coaching"
                    />
                    <Bar
                      dataKey="observations"
                      stackId="a"
                      fill="#87CEEB"
                      name="Observations"
                    />
                    <Bar
                      dataKey="planning"
                      stackId="a"
                      fill="#C9B99B"
                      name="Planning"
                      radius={[4, 4, 0, 0]}
                    />
                  </BarChart>
                </ResponsiveContainer>
              </div>
            </div>
          </CardContent>
        </Card>
      ) : (
        <Card className="shadow-sm w-full">
          <CardContent className="p-8 text-center">
            <p className="text-gray-500">No itinerary data available for this month.</p>
            <p className="text-sm text-gray-400 mt-2">Add entries in the Template tab to see analytics here.</p>
          </CardContent>
        </Card>
      )}
    </div>
  );
}
