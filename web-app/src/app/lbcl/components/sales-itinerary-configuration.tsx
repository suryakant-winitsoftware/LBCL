"use client";

import { useState } from "react";
import { Card, CardContent, CardHeader, CardTitle } from "@/app/lbcl/components/ui/card";
import { Button } from "@/app/lbcl/components/ui/button";
import { Input } from "@/app/lbcl/components/ui/input";
import { Badge } from "@/app/lbcl/components/ui/badge";
import { Plus, Edit, Trash2 } from "lucide-react";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/app/lbcl/components/ui/table";
import { ImportExcelData } from "./import-excel-data";
import { ACTIVITY_TYPES, TYPE_FOCUS_AREA_MAPPING } from "./itinerary-mock-data";

interface Holiday {
  sNo: number;
  date: string;
  day: string;
  reason: string;
}

interface KPICategory {
  label: string;
  percentage: string;
  description: string;
  color: string;
}

export function SalesItineraryConfiguration() {
  const [workingDays, setWorkingDays] = useState("22");
  const [avgOutlets, setAvgOutlets] = useState("6");
  const [estimatedCapacity, setEstimatedCapacity] = useState("132");

  const [holidays, setHolidays] = useState<Holiday[]>([
    { sNo: 1, date: "June 1, 2025", day: "Sunday", reason: "Week-Off (Default)" },
    { sNo: 2, date: "June 6, 2025", day: "Saturday", reason: "Eid Holiday" },
    { sNo: 3, date: "June 8, 2025", day: "Sunday", reason: "Week-Off (Default)" },
    { sNo: 4, date: "June 15, 2025", day: "Sunday", reason: "Week-Off (Default)" },
    { sNo: 5, date: "June 22, 2025", day: "Sunday", reason: "Week-Off (Default)" },
    { sNo: 6, date: "June 29, 2025", day: "Sunday", reason: "Week-Off (Default)" },
  ]);

  const kpiCategories: KPICategory[] = [
    {
      label: "Vulnerable Outlet Base",
      percentage: "90%",
      description: "Outlets At Risk Of Churn Or Reduction In Orders",
      color: "bg-red-100 text-red-700"
    },
    {
      label: "Isolated Outlet Base",
      percentage: "30%",
      description: "Outlets In Remote Or Hard To Reach Locations",
      color: "bg-orange-100 text-orange-700"
    },
    {
      label: "Exceptional Effort",
      percentage: "60%",
      description: "Outlets Requiring Special Attention Or Effort",
      color: "bg-green-100 text-green-700"
    },
    {
      label: "New Licenses",
      percentage: "10%",
      description: "Newly Licensed Outlets Requiring Onboarding",
      color: "bg-yellow-100 text-yellow-700"
    },
    {
      label: "80% Contribution Outlets (Heineken)",
      percentage: "90%",
      description: "Key Outlets That Contribute To 80% Of Heineken Sales",
      color: "bg-red-100 text-red-700"
    },
  ];

  const handleAddHoliday = () => {
    const newHoliday: Holiday = {
      sNo: holidays.length + 1,
      date: "Select Date",
      day: "Day",
      reason: "New Holiday"
    };
    setHolidays([...holidays, newHoliday]);
  };

  const handleDeleteHoliday = (sNo: number) => {
    setHolidays(holidays.filter(h => h.sNo !== sNo));
  };

  const calculateEstimatedCapacity = () => {
    const days = parseInt(workingDays) || 0;
    const outlets = parseInt(avgOutlets) || 0;
    setEstimatedCapacity((days * outlets).toString());
  };

  return (
    <div className="space-y-4">
      {/* Import Excel Data Section */}
      <ImportExcelData />

      {/* Activity Types Reference */}
      <Card>
        <CardHeader className="py-3 bg-gradient-to-r from-[#A08B5C]/10 to-transparent border-b">
          <CardTitle className="text-base">Activity Types & Focus Areas Reference</CardTitle>
        </CardHeader>
        <CardContent className="p-4">
          <p className="text-xs text-gray-600 mb-4">
            Below are all available activity types and their associated focus areas that can be used when creating itinerary entries.
          </p>
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
            {ACTIVITY_TYPES.map((activityType, index) => (
              <div key={index} className="border border-gray-200 rounded-lg p-3 bg-gray-50">
                <div className="flex items-start gap-2 mb-2">
                  <Badge className="bg-[#A08B5C] text-white text-xs px-2 py-1">
                    {index + 1}
                  </Badge>
                  <h3 className="font-semibold text-sm text-[#A08B5C] leading-tight flex-1">
                    {activityType}
                  </h3>
                </div>
                <div className="mt-2 pl-2">
                  <p className="text-xs font-medium text-gray-700 mb-1.5">Focus Areas:</p>
                  {TYPE_FOCUS_AREA_MAPPING[activityType] && TYPE_FOCUS_AREA_MAPPING[activityType].length > 0 ? (
                    <ul className="space-y-1">
                      {TYPE_FOCUS_AREA_MAPPING[activityType].map((focusArea, idx) => (
                        <li key={idx} className="text-xs text-gray-600 flex items-start gap-1.5">
                          <span className="text-[#A08B5C] mt-0.5">•</span>
                          <span>{focusArea}</span>
                        </li>
                      ))}
                    </ul>
                  ) : (
                    <p className="text-xs text-gray-400 italic">No predefined focus areas</p>
                  )}
                </div>
              </div>
            ))}
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardHeader className="py-3">
          <CardTitle className="text-base">Sales Itinerary Configuration</CardTitle>
        </CardHeader>
        <CardContent className="space-y-4 p-4">
          {/* Configuration Inputs */}
          <div className="grid grid-cols-1 md:grid-cols-3 gap-3">
            <div>
              <label className="block text-xs font-medium text-gray-700 mb-1.5">
                Total Working Days In Month (June 2025)
              </label>
              <Input
                type="number"
                value={workingDays}
                onChange={(e) => {
                  setWorkingDays(e.target.value);
                  calculateEstimatedCapacity();
                }}
                className="w-full h-9 text-sm"
              />
            </div>
            <div>
              <label className="block text-xs font-medium text-gray-700 mb-1.5">
                Average Outlets Per Day
              </label>
              <Input
                type="number"
                value={avgOutlets}
                onChange={(e) => {
                  setAvgOutlets(e.target.value);
                  calculateEstimatedCapacity();
                }}
                className="w-full h-9 text-sm"
              />
            </div>
            <div>
              <label className="block text-xs font-medium text-gray-700 mb-1.5">
                Estimated Monthly Capacity
              </label>
              <Input
                type="number"
                value={estimatedCapacity}
                readOnly
                className="w-full h-9 text-sm bg-gray-50"
              />
            </div>
          </div>

          <div className="flex justify-end">
            <Button className="bg-[#A08B5C] hover:bg-[#8A7548] text-sm h-9 px-4">
              Save Configuration
            </Button>
          </div>
        </CardContent>
      </Card>

      {/* Holidays Section */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
        <Card>
          <CardHeader className="py-3">
            <div className="flex items-center justify-between">
              <CardTitle className="text-base">Holidays In Month</CardTitle>
              <Button
                size="sm"
                className="bg-[#A08B5C] hover:bg-[#8A7548] text-xs h-8"
                onClick={handleAddHoliday}
              >
                <Plus className="w-3 h-3 mr-1.5" />
                Add New
              </Button>
            </div>
          </CardHeader>
          <CardContent className="p-0">
            <div className="overflow-x-auto">
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead className="w-12 text-xs py-2">S.No</TableHead>
                    <TableHead className="text-xs py-2">Date</TableHead>
                    <TableHead className="text-xs py-2">Day</TableHead>
                    <TableHead className="text-xs py-2">Reason</TableHead>
                    <TableHead className="w-20 text-xs py-2">Action</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {holidays.map((holiday) => (
                    <TableRow key={holiday.sNo}>
                      <TableCell className="text-xs py-2">{holiday.sNo}</TableCell>
                      <TableCell className="text-xs py-2">{holiday.date}</TableCell>
                      <TableCell className="text-xs py-2">{holiday.day}</TableCell>
                      <TableCell className="text-xs py-2">{holiday.reason}</TableCell>
                      <TableCell className="py-2">
                        <div className="flex gap-1">
                          <Button size="icon" variant="ghost" className="h-7 w-7">
                            <Edit className="w-3 h-3 text-blue-600" />
                          </Button>
                          <Button
                            size="icon"
                            variant="ghost"
                            className="h-7 w-7"
                            onClick={() => handleDeleteHoliday(holiday.sNo)}
                          >
                            <Trash2 className="w-3 h-3 text-red-600" />
                          </Button>
                        </div>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </div>
          </CardContent>
        </Card>

        {/* Days I Don't Want To Travel & KPI Categories */}
        <div className="space-y-4">
          <Card>
            <CardHeader className="py-3">
              <div className="flex items-center justify-between">
                <CardTitle className="text-base">Days I Don't Want To Travel</CardTitle>
                <Button size="sm" className="bg-[#A08B5C] hover:bg-[#8A7548] text-xs h-8">
                  Add New
                </Button>
              </div>
            </CardHeader>
            <CardContent className="p-4">
              <p className="text-xs text-gray-600 mb-3">
                Configure days when you prefer not to schedule outlet visits.
              </p>
              <div className="p-3 bg-gray-50 rounded-lg border border-dashed border-gray-300 text-center text-gray-500 text-xs">
                No restricted days configured
              </div>
            </CardContent>
          </Card>

          <Card>
            <CardHeader className="py-3">
              <CardTitle className="text-base">General KPI Category</CardTitle>
            </CardHeader>
            <CardContent className="p-4">
              <div className="space-y-2.5">
                {kpiCategories.map((category, index) => (
                  <div key={index} className="space-y-0.5">
                    <div className="flex items-center justify-between">
                      <span className="text-xs font-medium">{category.label}</span>
                      <Badge className={`${category.color} text-xs px-2 py-0.5`}>{category.percentage}</Badge>
                    </div>
                    <p className="text-xs text-gray-600">{category.description}</p>
                  </div>
                ))}
              </div>
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  );
}
