"use client";

import { useState } from "react";
import { Card, CardContent, CardHeader, CardTitle } from "@/app/lbcl/components/ui/card";
import { Button } from "@/app/lbcl/components/ui/button";
import { Progress } from "@/app/lbcl/components/ui/progress";
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, ResponsiveContainer, Legend, Tooltip } from "recharts";

export function SalesItineraryDashboard() {
  const [selectedMonth, setSelectedMonth] = useState<"current" | "next">("current");

  // Mock data for May 2025
  const mayData = [
    { date: "May 1", lbcl: 5, heineken: 7, special: 3, activity: 5, type: "Working", total: 20 },
    { date: "May 2", lbcl: 6, heineken: 8, special: 3, activity: 4, type: "Working", total: 21 },
    { date: "May 3", lbcl: 3, heineken: 5, special: 2, activity: 3, type: "Working", total: 13 },
    { date: "May 4", lbcl: 0, heineken: 0, special: 0, activity: 0, type: "Sunday", total: 0 },
    { date: "May 5", lbcl: 4, heineken: 7, special: 3, activity: 4, type: "Working", total: 18 },
    { date: "May 6", lbcl: 4, heineken: 6, special: 2, activity: 4, type: "Holiday", total: 16 },
    { date: "May 7", lbcl: 4, heineken: 6, special: 3, activity: 3, type: "Working", total: 16 },
    { date: "May 8", lbcl: 5, heineken: 8, special: 3, activity: 4, type: "Working", total: 20 },
    { date: "May 9", lbcl: 5, heineken: 7, special: 3, activity: 5, type: "Working", total: 20 },
    { date: "May 10", lbcl: 3, heineken: 5, special: 2, activity: 3, type: "Working", total: 13 },
    { date: "May 11", lbcl: 0, heineken: 0, special: 0, activity: 0, type: "Sunday", total: 0 },
    { date: "May 12", lbcl: 0, heineken: 0, special: 0, activity: 0, type: "Holiday", total: 0 },
    { date: "May 13", lbcl: 4, heineken: 6, special: 3, activity: 3, type: "Working", total: 16 },
    { date: "May 14", lbcl: 4, heineken: 6, special: 3, activity: 3, type: "Working", total: 16 },
    { date: "May 15", lbcl: 5, heineken: 8, special: 3, activity: 4, type: "Working", total: 20 },
    { date: "May 16", lbcl: 5, heineken: 7, special: 3, activity: 4, type: "Working", total: 19 },
    { date: "May 17", lbcl: 3, heineken: 5, special: 2, activity: 3, type: "Working", total: 13 },
  ];

  // Mock data for June 2025
  const juneData = [
    { date: "Jun 1", lbcl: 0, heineken: 0, special: 0, activity: 0, type: "Sunday", total: 0 },
    { date: "Jun 2", lbcl: 5, heineken: 7, special: 3, activity: 4, type: "Working", total: 19 },
    { date: "Jun 3", lbcl: 3, heineken: 5, special: 2, activity: 3, type: "Working", total: 13 },
    { date: "Jun 4", lbcl: 4, heineken: 7, special: 3, activity: 4, type: "Working", total: 18 },
    { date: "Jun 5", lbcl: 3, heineken: 4, special: 2, activity: 3, type: "Working", total: 12 },
    { date: "Jun 6", lbcl: 4, heineken: 6, special: 3, activity: 3, type: "Working", total: 16 },
    { date: "Jun 7", lbcl: 4, heineken: 6, special: 3, activity: 3, type: "Working", total: 16 },
    { date: "Jun 8", lbcl: 0, heineken: 0, special: 0, activity: 0, type: "Sunday", total: 0 },
    { date: "Jun 9", lbcl: 5, heineken: 7, special: 3, activity: 4, type: "Working", total: 19 },
    { date: "Jun 10", lbcl: 3, heineken: 5, special: 2, activity: 3, type: "Working", total: 13 },
    { date: "Jun 11", lbcl: 4, heineken: 7, special: 3, activity: 4, type: "Working", total: 18 },
    { date: "Jun 12", lbcl: 3, heineken: 4, special: 2, activity: 3, type: "Working", total: 12 },
    { date: "Jun 13", lbcl: 4, heineken: 6, special: 3, activity: 3, type: "Working", total: 16 },
    { date: "Jun 14", lbcl: 4, heineken: 6, special: 3, activity: 3, type: "Working", total: 16 },
    { date: "Jun 15", lbcl: 0, heineken: 0, special: 0, activity: 0, type: "Sunday", total: 0 },
    { date: "Jun 16", lbcl: 5, heineken: 7, special: 3, activity: 4, type: "Working", total: 19 },
    { date: "Jun 17", lbcl: 3, heineken: 5, special: 2, activity: 3, type: "Working", total: 13 },
    { date: "Jun 18", lbcl: 4, heineken: 7, special: 3, activity: 3, type: "Working", total: 17 },
    { date: "Jun 19", lbcl: 3, heineken: 4, special: 2, activity: 3, type: "Working", total: 12 },
    { date: "Jun 20", lbcl: 4, heineken: 6, special: 3, activity: 3, type: "Working", total: 16 },
    { date: "Jun 21", lbcl: 4, heineken: 6, special: 3, activity: 3, type: "Working", total: 16 },
  ];

  const currentData = selectedMonth === "current" ? mayData : juneData;
  const currentMonthLabel = selectedMonth === "current" ? "May 2025" : "June 2025";

  return (
    <div className="space-y-5 pb-6 w-full">
      {/* KPI Cards */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4 w-full">
        <Card className="bg-gradient-to-br from-yellow-50 to-yellow-100 border-yellow-200 shadow-sm hover:shadow-md transition-shadow">
          <CardContent className="p-4">
            <h3 className="text-xs font-semibold text-gray-700 mb-2 uppercase tracking-wide">Coverage Compliance Rate</h3>
            <div className="text-3xl font-bold text-[#A08B5C] mb-2">94%</div>
            <Progress value={94} className="h-2 mb-1 bg-yellow-200" />
            <p className="text-xs text-gray-600">Target: 80% Minimum</p>
          </CardContent>
        </Card>

        <Card className="bg-gradient-to-br from-yellow-50 to-yellow-100 border-yellow-200 shadow-sm hover:shadow-md transition-shadow">
          <CardContent className="p-4">
            <h3 className="text-xs font-semibold text-gray-700 mb-2 uppercase tracking-wide">Coverage / Scheduled Outlets</h3>
            <div className="text-3xl font-bold text-[#A08B5C] mb-2">77/82</div>
            <p className="text-xs text-gray-600">Of Total Outlets In Plan 128</p>
          </CardContent>
        </Card>

        <Card className="bg-gradient-to-br from-yellow-50 to-yellow-100 border-yellow-200 shadow-sm hover:shadow-md transition-shadow">
          <CardContent className="p-4">
            <h3 className="text-xs font-semibold text-gray-700 mb-2 uppercase tracking-wide">Time Gone</h3>
            <div className="text-3xl font-bold text-[#A08B5C] mb-2">56%</div>
            <Progress value={56} className="h-2 mb-1 bg-yellow-200" />
            <p className="text-xs text-gray-600">(14 / 25 days)</p>
          </CardContent>
        </Card>
      </div>

      {/* Month Selector */}
      <div className="flex gap-4 border-b-2 border-gray-200 overflow-x-auto w-full">
        <Button
          variant="ghost"
          onClick={() => setSelectedMonth("current")}
          className={`px-6 py-3 rounded-none font-semibold whitespace-nowrap transition-all text-sm ${
            selectedMonth === "current"
              ? "border-b-3 border-[#A08B5C] text-[#A08B5C] bg-[#A08B5C]/5"
              : "text-gray-600 hover:text-[#A08B5C] hover:bg-gray-50"
          }`}
        >
          CURRENT MONTH (MAY 2025)
        </Button>
        <Button
          variant="ghost"
          onClick={() => setSelectedMonth("next")}
          className={`px-6 py-3 rounded-none font-semibold whitespace-nowrap transition-all text-sm ${
            selectedMonth === "next"
              ? "border-b-3 border-[#A08B5C] text-[#A08B5C] bg-[#A08B5C]/5"
              : "text-gray-600 hover:text-[#A08B5C] hover:bg-gray-50"
          }`}
        >
          NEXT MONTH (JUNE 2025)
        </Button>
      </div>

      {/* Chart */}
      <Card className="shadow-sm w-full">
        <CardHeader className="border-b bg-gray-50 py-3">
          <CardTitle className="text-base font-bold text-gray-800">
            {currentMonthLabel} - Sales Itinerary Planning (Approved)
          </CardTitle>
        </CardHeader>
        <CardContent className="p-4 sm:p-5">
          <div className="w-full h-[400px] lg:h-[450px]">
            <ResponsiveContainer width="100%" height="100%">
              <BarChart
                data={currentData}
                margin={{ top: 10, right: 20, left: 10, bottom: 60 }}
                barCategoryGap="18%"
              >
                <CartesianGrid strokeDasharray="3 3" stroke="#e5e7eb" vertical={false} />
                <XAxis
                  dataKey="date"
                  tick={{ fontSize: 11, fill: '#4B5563' }}
                  angle={-45}
                  textAnchor="end"
                  height={70}
                  interval={0}
                />
                <YAxis
                  tick={{ fontSize: 11, fill: '#4B5563' }}
                  label={{
                    value: 'Outlets',
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
                    boxShadow: '0 2px 4px rgba(0,0,0,0.1)'
                  }}
                  cursor={{ fill: 'rgba(160, 139, 92, 0.1)' }}
                />
                <Legend
                  wrapperStyle={{ paddingTop: '15px', fontSize: '11px' }}
                  iconType="square"
                  iconSize={12}
                />
                <Bar
                  dataKey="lbcl"
                  stackId="a"
                  fill="#F4D03F"
                  name="80% Contribution Outlets (LBCL)"
                />
                <Bar
                  dataKey="heineken"
                  stackId="a"
                  fill="#E67E22"
                  name="80% Contribution Outlets (Heineken)"
                />
                <Bar
                  dataKey="special"
                  stackId="a"
                  fill="#34495E"
                  name="Special Outlets"
                />
                <Bar
                  dataKey="activity"
                  stackId="a"
                  fill="#85C1E9"
                  name="Activity (National, Brand, Tourist Board License)"
                  radius={[6, 6, 0, 0]}
                />
              </BarChart>
            </ResponsiveContainer>
          </div>

          {/* Summary Stats */}
          <div className="mt-6 p-4 bg-gradient-to-br from-gray-50 to-gray-100 rounded-lg border border-gray-200">
            <h3 className="text-sm font-bold text-gray-800 mb-3 text-center">Monthly Summary</h3>
            <div className="grid grid-cols-2 md:grid-cols-4 gap-3 text-center">
              <div className="p-3 bg-white rounded-lg shadow-sm border border-yellow-200">
                <div className="text-2xl font-bold text-[#F4D03F] mb-1">
                  {currentData.reduce((sum, day) => sum + day.lbcl, 0)}
                </div>
                <div className="text-xs text-gray-700 font-semibold">LBCL Outlets</div>
              </div>
              <div className="p-3 bg-white rounded-lg shadow-sm border border-orange-200">
                <div className="text-2xl font-bold text-[#E67E22] mb-1">
                  {currentData.reduce((sum, day) => sum + day.heineken, 0)}
                </div>
                <div className="text-xs text-gray-700 font-semibold">Heineken Outlets</div>
              </div>
              <div className="p-3 bg-white rounded-lg shadow-sm border border-gray-300">
                <div className="text-2xl font-bold text-[#34495E] mb-1">
                  {currentData.reduce((sum, day) => sum + day.special, 0)}
                </div>
                <div className="text-xs text-gray-700 font-semibold">Special Outlets</div>
              </div>
              <div className="p-3 bg-white rounded-lg shadow-sm border border-blue-200">
                <div className="text-2xl font-bold text-[#85C1E9] mb-1">
                  {currentData.reduce((sum, day) => sum + day.activity, 0)}
                </div>
                <div className="text-xs text-gray-700 font-semibold">Activity Outlets</div>
              </div>
            </div>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
