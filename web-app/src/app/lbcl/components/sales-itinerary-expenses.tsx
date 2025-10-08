"use client";

import { useState } from "react";
import { Card, CardContent, CardHeader, CardTitle } from "@/app/lbcl/components/ui/card";
import { Button } from "@/app/lbcl/components/ui/button";
import { Input } from "@/app/lbcl/components/ui/input";
import { Textarea } from "@/app/lbcl/components/ui/textarea";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/app/lbcl/components/ui/select";
import { Calendar } from "@/app/lbcl/components/ui/calendar";
import { Clock, MapPin, Navigation, DollarSign, Upload, Image as ImageIcon } from "lucide-react";

export function SalesItineraryExpenses() {
  const [selectedDate, setSelectedDate] = useState<Date | undefined>(new Date(2025, 4, 16)); // May 16, 2025
  const [expenseType, setExpenseType] = useState("Food Bill");
  const [amount, setAmount] = useState("");
  const [remarks, setRemarks] = useState("");

  // Mock data for selected date
  const dayStats = {
    startTime: "8:46 AM",
    firstCheckIn: "09:08 AM",
    lastCheckOut: "05:08 PM",
    endTime: "05:25 PM",
    totalWorkingTime: "8 Hrs 39 Min",
    timeInOutlet: "5 Hrs 49 Min",
    travelTime: "2 Hrs 50 Min",
    totalDistance: "43.7 Kms",
    eligibleAmount: "4,031.70 LKR"
  };

  const currentMonth = selectedDate ? selectedDate.toLocaleString('default', { month: 'long', year: 'numeric' }) : 'May 2025';

  return (
    <div className="space-y-4">
      {/* Top Stats Bar */}
      <div className="grid grid-cols-2 md:grid-cols-7 gap-2 bg-white p-3 rounded-lg border text-xs">
        <div className="text-center">
          <div className="flex items-center justify-center gap-1 text-xs text-gray-600 mb-1">
            <Clock className="w-3 h-3" />
            <span>Start Time</span>
          </div>
          <div className="font-semibold">{dayStats.startTime}</div>
        </div>
        <div className="text-center">
          <div className="flex items-center justify-center gap-1 text-xs text-gray-600 mb-1">
            <MapPin className="w-3 h-3" />
            <span>First Check-in</span>
          </div>
          <div className="font-semibold">{dayStats.firstCheckIn}</div>
        </div>
        <div className="text-center">
          <div className="flex items-center justify-center gap-1 text-xs text-gray-600 mb-1">
            <MapPin className="w-3 h-3" />
            <span>Last Check-out</span>
          </div>
          <div className="font-semibold">{dayStats.lastCheckOut}</div>
        </div>
        <div className="text-center">
          <div className="flex items-center justify-center gap-1 text-xs text-gray-600 mb-1">
            <Clock className="w-3 h-3" />
            <span>EOD Time</span>
          </div>
          <div className="font-semibold">{dayStats.endTime}</div>
        </div>
        <div className="text-center">
          <div className="flex items-center justify-center gap-1 text-xs text-gray-600 mb-1">
            <Clock className="w-3 h-3" />
            <span>Total working time</span>
          </div>
          <div className="font-semibold">{dayStats.totalWorkingTime}</div>
        </div>
        <div className="text-center">
          <div className="flex items-center justify-center gap-1 text-xs text-gray-600 mb-1">
            <Clock className="w-3 h-3" />
            <span>Time Spent in the outlet</span>
          </div>
          <div className="font-semibold">{dayStats.timeInOutlet}</div>
        </div>
        <div className="text-center">
          <div className="flex items-center justify-center gap-1 text-xs text-gray-600 mb-1">
            <Navigation className="w-3 h-3" />
            <span>Time Spent while travelling</span>
          </div>
          <div className="font-semibold">{dayStats.travelTime}</div>
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-4">
        {/* Left Side - Calendar */}
        <div className="lg:col-span-1">
          <Card>
            <CardHeader className="py-3">
              <CardTitle className="text-base">{currentMonth}</CardTitle>
            </CardHeader>
            <CardContent className="p-3">
              <Calendar
                mode="single"
                selected={selectedDate}
                onSelect={setSelectedDate}
                className="rounded-md border text-sm"
              />
            </CardContent>
          </Card>
        </div>

        {/* Right Side - Map and Expense Form */}
        <div className="lg:col-span-2 space-y-4">
          {/* Map and Distance/Amount Cards */}
          <div className="grid grid-cols-1 md:grid-cols-3 gap-3">
            <Card className="md:col-span-2">
              <CardContent className="p-3">
                <div className="aspect-video bg-white rounded-lg border overflow-hidden">
                  <iframe
                    src="https://calendar.google.com/calendar/embed?height=400&wkst=1&bgcolor=%23ffffff&ctz=Asia%2FColombo&showTitle=0&showNav=1&showDate=1&showPrint=0&showTabs=0&showCalendars=0&showTz=0&mode=WEEK"
                    style={{ border: 0 }}
                    width="100%"
                    height="100%"
                    frameBorder="0"
                    scrolling="no"
                    title="Google Calendar"
                  ></iframe>
                </div>
              </CardContent>
            </Card>

            <div className="space-y-3">
              <Card className="border-[#A08B5C] border-2">
                <CardContent className="p-3 text-center">
                  <h3 className="text-xs text-gray-600 mb-1.5">Total Distance</h3>
                  <div className="text-2xl font-bold text-[#A08B5C]">{dayStats.totalDistance}</div>
                  <p className="text-xs text-gray-500 mt-1">Kms</p>
                </CardContent>
              </Card>

              <Card className="border-[#A08B5C] border-2">
                <CardContent className="p-3 text-center">
                  <h3 className="text-xs text-gray-600 mb-1.5">Eligible Amount</h3>
                  <div className="text-2xl font-bold text-[#A08B5C]">{dayStats.eligibleAmount}</div>
                  <p className="text-xs text-gray-500 mt-1">LKR</p>
                </CardContent>
              </Card>
            </div>
          </div>

          {/* Add New Expense Form */}
          <Card>
            <CardHeader className="py-3">
              <CardTitle className="text-base">Add New Expense</CardTitle>
            </CardHeader>
            <CardContent className="p-4">
              <div className="grid grid-cols-1 md:grid-cols-3 gap-3">
                <div>
                  <label className="block text-xs font-medium text-gray-700 mb-1.5">
                    Select Expense Type
                  </label>
                  <Select value={expenseType} onValueChange={setExpenseType}>
                    <SelectTrigger className="h-9 text-sm">
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="Food Bill">Food Bill</SelectItem>
                      <SelectItem value="Transportation">Transportation</SelectItem>
                      <SelectItem value="Accommodation">Accommodation</SelectItem>
                      <SelectItem value="Other">Other</SelectItem>
                    </SelectContent>
                  </Select>
                </div>

                <div>
                  <label className="block text-xs font-medium text-gray-700 mb-1.5">
                    Amount
                  </label>
                  <div className="relative">
                    <span className="absolute left-3 top-2 text-gray-500 text-xs">LKR</span>
                    <Input
                      type="number"
                      value={amount}
                      onChange={(e) => setAmount(e.target.value)}
                      placeholder="0.00"
                      className="pl-12 h-9 text-sm"
                      step="0.01"
                    />
                  </div>
                </div>

                <div>
                  <label className="block text-xs font-medium text-gray-700 mb-1.5">
                    Upload Image
                  </label>
                  <Button variant="outline" className="w-full h-9 text-sm">
                    <Upload className="w-3 h-3 mr-1.5" />
                    Upload
                  </Button>
                </div>
              </div>

              <div className="mt-3">
                <label className="block text-xs font-medium text-gray-700 mb-1.5">
                  Remarks
                </label>
                <Textarea
                  value={remarks}
                  onChange={(e) => setRemarks(e.target.value)}
                  placeholder="Enter Here"
                  rows={3}
                  className="resize-none text-sm"
                />
              </div>

              <div className="mt-4 flex justify-end">
                <Button className="bg-[#A08B5C] hover:bg-[#8A7548] px-6 text-sm h-9">
                  Submit for Review
                </Button>
              </div>
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  );
}
