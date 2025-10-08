"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { Button } from "@/app/lbcl/components/ui/button";
import { Input } from "@/app/lbcl/components/ui/input";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/app/lbcl/components/ui/select";
import { Textarea } from "@/app/lbcl/components/ui/textarea";
import { ChevronDown } from "lucide-react";
import {
  ItineraryEntry,
  ACTIVITY_TYPES,
  TYPE_FOCUS_AREA_MAPPING,
  MARKETS,
  CHANNELS,
  ROUTE_NUMBERS,
  OUTLET_NUMBERS,
  ACCOMPANIED_BY_OPTIONS,
  KRA_PLAN_OPTIONS,
} from "./itinerary-mock-data";

export function AddItineraryEntry() {
  const router = useRouter();

  // Expanded sections state
  const [expandedSections, setExpandedSections] = useState<number[]>([1, 2, 3, 4]);

  const toggleSection = (section: number) => {
    setExpandedSections((prev) =>
      prev.includes(section)
        ? prev.filter((s) => s !== section)
        : [...prev, section]
    );
  };

  // Form state
  const [formData, setFormData] = useState<Partial<ItineraryEntry>>({
    type: "",
    focusArea: "",
    date: "",
    day: "",
    kraPlan: "",
    morningMeeting: "NA",
    timeFrom: "09:00 AM",
    timeTo: "05:00 PM",
    market: "",
    channel: "NA",
    routeNo: "NA",
    outletNo: "NA",
    accompaniedBy: "",
    nightOut: "",
    noOfDays: 1,
    plannedMileage: 0,
  });

  // Time state for Time From
  const [timeFromHour, setTimeFromHour] = useState("09");
  const [timeFromMin, setTimeFromMin] = useState("00");
  const [timeFromPeriod, setTimeFromPeriod] = useState("AM");

  // Time state for Time To
  const [timeToHour, setTimeToHour] = useState("05");
  const [timeToMin, setTimeToMin] = useState("00");
  const [timeToPeriod, setTimeToPeriod] = useState("PM");

  // KRA Plan Other text
  const [kraPlanOther, setKraPlanOther] = useState("");

  // Get available focus areas based on selected type
  const availableFocusAreas = formData.type
    ? TYPE_FOCUS_AREA_MAPPING[formData.type] || []
    : [];

  // Update timeFrom in formData when time components change
  const updateTimeFrom = (hour: string, min: string, period: string) => {
    const formattedTime = `${hour}:${min} ${period}`;
    setFormData({...formData, timeFrom: formattedTime});
  };

  // Update timeTo in formData when time components change
  const updateTimeTo = (hour: string, min: string, period: string) => {
    const formattedTime = `${hour}:${min} ${period}`;
    setFormData({...formData, timeTo: formattedTime});
  };

  // Handle type change - reset focus area when type changes
  const handleTypeChange = (newType: string) => {
    setFormData({
      ...formData,
      type: newType,
      focusArea: "", // Reset focus area when type changes
    });
  };

  // Handle date change - automatically calculate and set day of week
  const handleDateChange = (newDate: string) => {
    const date = new Date(newDate);
    const dayNames = ["Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"];
    const dayOfWeek = dayNames[date.getDay()];

    setFormData({
      ...formData,
      date: newDate,
      day: dayOfWeek, // Auto-populate day
    });
  };

  const handleSave = () => {
    // Use kraPlanOther if "Other" is selected, otherwise use formData.kraPlan
    const finalFormData = {
      ...formData,
      kraPlan: formData.kraPlan === "Other" ? kraPlanOther : formData.kraPlan,
      timeFrom: `${timeFromHour}:${timeFromMin} ${timeFromPeriod}`,
      timeTo: `${timeToHour}:${timeToMin} ${timeToPeriod}`,
    };

    // Get existing entries from localStorage
    const existingEntries = localStorage.getItem('itinerary_entries');
    const entries = existingEntries ? JSON.parse(existingEntries) : [];

    // Create new entry with ID
    const newEntry: ItineraryEntry = {
      id: entries.length > 0 ? Math.max(...entries.map((e: ItineraryEntry) => e.id)) + 1 : 1,
      type: finalFormData.type || "",
      focusArea: finalFormData.focusArea || "",
      date: finalFormData.date || "",
      day: finalFormData.day || "",
      kraPlan: finalFormData.kraPlan || "",
      morningMeeting: finalFormData.morningMeeting || "NA",
      timeFrom: finalFormData.timeFrom || "",
      timeTo: finalFormData.timeTo || "",
      market: finalFormData.market || "",
      channel: finalFormData.channel || "NA",
      routeNo: finalFormData.routeNo || "NA",
      outletNo: finalFormData.outletNo || "NA",
      accompaniedBy: finalFormData.accompaniedBy || "",
      nightOut: finalFormData.nightOut || "",
      noOfDays: finalFormData.noOfDays || 1,
      plannedMileage: finalFormData.plannedMileage || 0,
    };

    // Add new entry to array
    entries.push(newEntry);

    // Save to localStorage
    localStorage.setItem('itinerary_entries', JSON.stringify(entries));

    console.log("✅ Entry saved to localStorage:", newEntry);

    // Navigate back to template page
    router.push("/lbcl/sales-itinerary");
  };

  const handleCancel = () => {
    router.push("/lbcl/sales-itinerary");
  };

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Info Section */}
      <div className="px-4 sm:px-6 lg:px-8 py-3 sm:py-4 bg-white border-b">
        <div className="flex items-center justify-end gap-2 mb-4">
          <Button
            variant="outline"
            onClick={handleCancel}
            className="text-[#A08B5C] border-[#A08B5C] text-xs sm:text-sm px-3 sm:px-6 bg-transparent"
          >
            Back
          </Button>
          <Button
            onClick={handleSave}
            className="bg-[#A08B5C] hover:bg-[#8F7A4B] text-white text-xs sm:text-sm px-3 sm:px-6"
          >
            Save Entry
          </Button>
        </div>
        <div className="text-center mb-2">
          <h1 className="text-lg font-bold text-gray-900">Add New Itinerary Entry</h1>
          <p className="text-xs text-gray-500 mt-1">Fill in the details for your itinerary entry</p>
        </div>
      </div>

      {/* Form Steps */}
      <main className="px-4 sm:px-6 lg:px-8 py-4 sm:py-6">
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-4 max-w-7xl mx-auto">

          {/* Step 1: Basic Information */}
          <div className="bg-white rounded-lg p-4 sm:p-6 shadow-sm lg:col-span-2">
            <button
              onClick={() => toggleSection(1)}
              className="flex items-center gap-3 sm:gap-4 w-full"
            >
              <div className="w-8 h-8 sm:w-10 sm:h-10 rounded-full bg-[#F5E6C8] flex items-center justify-center flex-shrink-0">
                <span className="font-bold text-sm sm:text-base">1</span>
              </div>
              <span className="text-sm sm:text-base font-medium flex-1 text-left">
                Basic Information
              </span>
              <ChevronDown
                className={`w-5 h-5 text-gray-400 transition-transform flex-shrink-0 ${
                  expandedSections.includes(1) ? "rotate-180" : ""
                }`}
              />
            </button>

            {expandedSections.includes(1) && (
              <div className="mt-4 sm:mt-6 space-y-4">
                <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
              {/* Activity Type */}
              <div>
                <label className="text-xs sm:text-sm font-medium mb-2 block">
                  Activity Type *
                </label>
                <Select
                  value={formData.type}
                  onValueChange={handleTypeChange}
                >
                  <SelectTrigger>
                    <SelectValue placeholder="Select Activity Type" />
                  </SelectTrigger>
                  <SelectContent>
                    {ACTIVITY_TYPES.map((type) => (
                      <SelectItem key={type} value={type}>{type}</SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>

              {/* Focus Area */}
              <div>
                <label className="text-xs sm:text-sm font-medium mb-2 block">
                  Focus Area *
                </label>
                <Select
                  value={formData.focusArea}
                  onValueChange={(value) => setFormData({...formData, focusArea: value})}
                  disabled={!formData.type || availableFocusAreas.length === 0}
                >
                  <SelectTrigger>
                    <SelectValue placeholder={
                      !formData.type
                        ? "Select Type First"
                        : availableFocusAreas.length === 0
                          ? "No Focus Areas Available"
                          : "Select Focus Area"
                    } />
                  </SelectTrigger>
                  <SelectContent>
                    {availableFocusAreas.map((area) => (
                      <SelectItem key={area} value={area}>{area}</SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>

              {/* Date */}
              <div>
                <label className="text-xs sm:text-sm font-medium mb-2 block">
                  Date *
                </label>
                <Input
                  type="date"
                  value={formData.date}
                  onChange={(e) => handleDateChange(e.target.value)}
                />
              </div>

              {/* Day */}
              <div>
                <label className="text-xs sm:text-sm font-medium mb-2 block">
                  Day <span className="text-gray-500 font-normal">(Auto)</span>
                </label>
                <Input
                  type="text"
                  value={formData.day}
                  readOnly
                  className="bg-gray-50"
                  placeholder="Select Date First"
                />
              </div>

                  {/* KRA Plan - Full width */}
                  <div className="sm:col-span-2">
                    <label className="text-xs sm:text-sm font-medium mb-2 block">
                      KRA Plan *
                    </label>
                    <Select
                      value={formData.kraPlan}
                      onValueChange={(value) => {
                        setFormData({...formData, kraPlan: value});
                        if (value !== "Other") {
                          setKraPlanOther("");
                        }
                      }}
                    >
                      <SelectTrigger>
                        <SelectValue placeholder="Select KRA Plan" />
                      </SelectTrigger>
                      <SelectContent>
                        {KRA_PLAN_OPTIONS.map((plan) => (
                          <SelectItem key={plan} value={plan}>{plan}</SelectItem>
                        ))}
                      </SelectContent>
                    </Select>

                    {formData.kraPlan === "Other" && (
                      <Textarea
                        value={kraPlanOther}
                        onChange={(e) => setKraPlanOther(e.target.value)}
                        placeholder="Enter custom KRA Plan details"
                        rows={2}
                        className="mt-2"
                      />
                    )}
                  </div>
                </div>
              </div>
            )}
          </div>

          {/* Step 2: Time Details */}
          <div className="bg-white rounded-lg p-4 sm:p-6 shadow-sm lg:col-span-2">
            <button
              onClick={() => toggleSection(2)}
              className="flex items-center gap-3 sm:gap-4 w-full"
            >
              <div className="w-8 h-8 sm:w-10 sm:h-10 rounded-full bg-[#F5E6C8] flex items-center justify-center flex-shrink-0">
                <span className="font-bold text-sm sm:text-base">2</span>
              </div>
              <span className="text-sm sm:text-base font-medium flex-1 text-left">
                Time Details
              </span>
              <ChevronDown
                className={`w-5 h-5 text-gray-400 transition-transform flex-shrink-0 ${
                  expandedSections.includes(2) ? "rotate-180" : ""
                }`}
              />
            </button>

            {expandedSections.includes(2) && (
              <div className="mt-4 sm:mt-6">
                <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">

              {/* Time From */}
              <div>
                <label className="text-xs sm:text-sm font-medium mb-2 block">
                  Time From
                </label>
                <div className="flex gap-2">
                  <Input
                    type="text"
                    value={timeFromHour}
                    onChange={(e) => {
                      const val = e.target.value;
                      // Allow empty or numeric values only
                      if (val === '' || /^\d{0,2}$/.test(val)) {
                        setTimeFromHour(val);
                        updateTimeFrom(val, timeFromMin, timeFromPeriod);
                      }
                    }}
                    placeholder="HH"
                    maxLength={2}
                    className="text-center"
                  />
                  <span className="text-xs text-gray-500 self-center">HH</span>
                  <Input
                    type="text"
                    value={timeFromMin}
                    onChange={(e) => {
                      const val = e.target.value;
                      // Allow empty or numeric values only
                      if (val === '' || /^\d{0,2}$/.test(val)) {
                        setTimeFromMin(val);
                        updateTimeFrom(timeFromHour, val, timeFromPeriod);
                      }
                    }}
                    placeholder="MM"
                    maxLength={2}
                    className="text-center"
                  />
                  <span className="text-xs text-gray-500 self-center">Min</span>
                  <Select
                    value={timeFromPeriod}
                    onValueChange={(value) => {
                      setTimeFromPeriod(value);
                      updateTimeFrom(timeFromHour, timeFromMin, value);
                    }}
                  >
                    <SelectTrigger className="w-20">
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="AM">AM</SelectItem>
                      <SelectItem value="PM">PM</SelectItem>
                    </SelectContent>
                  </Select>
                </div>
              </div>

              {/* Time To */}
              <div>
                <label className="text-xs sm:text-sm font-medium mb-2 block">
                  Time To
                </label>
                <div className="flex gap-2">
                  <Input
                    type="text"
                    value={timeToHour}
                    onChange={(e) => {
                      const val = e.target.value;
                      // Allow empty or numeric values only
                      if (val === '' || /^\d{0,2}$/.test(val)) {
                        setTimeToHour(val);
                        updateTimeTo(val, timeToMin, timeToPeriod);
                      }
                    }}
                    placeholder="HH"
                    maxLength={2}
                    className="text-center"
                  />
                  <span className="text-xs text-gray-500 self-center">HH</span>
                  <Input
                    type="text"
                    value={timeToMin}
                    onChange={(e) => {
                      const val = e.target.value;
                      // Allow empty or numeric values only
                      if (val === '' || /^\d{0,2}$/.test(val)) {
                        setTimeToMin(val);
                        updateTimeTo(timeToHour, val, timeToPeriod);
                      }
                    }}
                    placeholder="MM"
                    maxLength={2}
                    className="text-center"
                  />
                  <span className="text-xs text-gray-500 self-center">Min</span>
                  <Select
                    value={timeToPeriod}
                    onValueChange={(value) => {
                      setTimeToPeriod(value);
                      updateTimeTo(timeToHour, timeToMin, value);
                    }}
                  >
                    <SelectTrigger className="w-20">
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="AM">AM</SelectItem>
                      <SelectItem value="PM">PM</SelectItem>
                    </SelectContent>
                  </Select>
                </div>
              </div>
                </div>
              </div>
            )}
          </div>

          {/* Step 3: Location Details */}
          <div className="bg-white rounded-lg p-4 sm:p-6 shadow-sm lg:col-span-2">
            <button
              onClick={() => toggleSection(3)}
              className="flex items-center gap-3 sm:gap-4 w-full"
            >
              <div className="w-8 h-8 sm:w-10 sm:h-10 rounded-full bg-[#F5E6C8] flex items-center justify-center flex-shrink-0">
                <span className="font-bold text-sm sm:text-base">3</span>
              </div>
              <span className="text-sm sm:text-base font-medium flex-1 text-left">
                Location Details
              </span>
              <ChevronDown
                className={`w-5 h-5 text-gray-400 transition-transform flex-shrink-0 ${
                  expandedSections.includes(3) ? "rotate-180" : ""
                }`}
              />
            </button>

            {expandedSections.includes(3) && (
              <div className="mt-4 sm:mt-6">
                <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">

              {/* Market */}
              <div>
                <label className="text-xs sm:text-sm font-medium mb-2 block">
                  Market *
                </label>
                <Select
                  value={formData.market}
                  onValueChange={(value) => setFormData({...formData, market: value})}
                >
                  <SelectTrigger>
                    <SelectValue placeholder="Select Market" />
                  </SelectTrigger>
                  <SelectContent>
                    {MARKETS.map((market) => (
                      <SelectItem key={market} value={market}>{market}</SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>

              {/* Channel */}
              <div>
                <label className="text-xs sm:text-sm font-medium mb-2 block">
                  Channel
                </label>
                <Select
                  value={formData.channel}
                  onValueChange={(value) => setFormData({...formData, channel: value})}
                >
                  <SelectTrigger>
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    {CHANNELS.map((channel) => (
                      <SelectItem key={channel} value={channel}>{channel}</SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>

              {/* Route No. */}
              <div>
                <label className="text-xs sm:text-sm font-medium mb-2 block">
                  Route No.
                </label>
                <Select
                  value={formData.routeNo}
                  onValueChange={(value) => setFormData({...formData, routeNo: value})}
                >
                  <SelectTrigger>
                    <SelectValue placeholder="Select Route" />
                  </SelectTrigger>
                  <SelectContent>
                    {ROUTE_NUMBERS.map((route) => (
                      <SelectItem key={route} value={route}>{route}</SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>

              {/* Outlet No. */}
              <div>
                <label className="text-xs sm:text-sm font-medium mb-2 block">
                  Outlet No.
                </label>
                <Select
                  value={formData.outletNo}
                  onValueChange={(value) => setFormData({...formData, outletNo: value})}
                >
                  <SelectTrigger>
                    <SelectValue placeholder="Select Outlet" />
                  </SelectTrigger>
                  <SelectContent>
                    {OUTLET_NUMBERS.map((outlet) => (
                      <SelectItem key={outlet} value={outlet}>{outlet}</SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
                </div>
              </div>
            )}
          </div>

          {/* Step 4: Team & Logistics */}
          <div className="bg-white rounded-lg p-4 sm:p-6 shadow-sm lg:col-span-2">
            <button
              onClick={() => toggleSection(4)}
              className="flex items-center gap-3 sm:gap-4 w-full"
            >
              <div className="w-8 h-8 sm:w-10 sm:h-10 rounded-full bg-[#F5E6C8] flex items-center justify-center flex-shrink-0">
                <span className="font-bold text-sm sm:text-base">4</span>
              </div>
              <span className="text-sm sm:text-base font-medium flex-1 text-left">
                Team & Logistics
              </span>
              <ChevronDown
                className={`w-5 h-5 text-gray-400 transition-transform flex-shrink-0 ${
                  expandedSections.includes(4) ? "rotate-180" : ""
                }`}
              />
            </button>

            {expandedSections.includes(4) && (
              <div className="mt-4 sm:mt-6">
                <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">

              {/* Accompanied By */}
              <div>
                <label className="text-xs sm:text-sm font-medium mb-2 block">
                  Accompanied By *
                </label>
                <Select
                  value={formData.accompaniedBy}
                  onValueChange={(value) => setFormData({...formData, accompaniedBy: value})}
                >
                  <SelectTrigger>
                    <SelectValue placeholder="Select Team" />
                  </SelectTrigger>
                  <SelectContent>
                    {ACCOMPANIED_BY_OPTIONS.map((option) => (
                      <SelectItem key={option} value={option}>{option}</SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>

              {/* Night Out Market */}
              <div>
                <label className="text-xs sm:text-sm font-medium mb-2 block">
                  Night Out Market *
                </label>
                <Select
                  value={formData.nightOut}
                  onValueChange={(value) => setFormData({...formData, nightOut: value})}
                >
                  <SelectTrigger>
                    <SelectValue placeholder="Select Night Out Market" />
                  </SelectTrigger>
                  <SelectContent>
                    {MARKETS.map((market) => (
                      <SelectItem key={market} value={market}>{market}</SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>

              {/* No. of Days */}
              <div>
                <label className="text-xs sm:text-sm font-medium mb-2 block">
                  No. of Days
                </label>
                <Input
                  type="number"
                  value={formData.noOfDays}
                  onChange={(e) => setFormData({...formData, noOfDays: parseInt(e.target.value) || 0})}
                  min="1"
                />
              </div>

              {/* Planned Mileage */}
              <div>
                <label className="text-xs sm:text-sm font-medium mb-2 block">
                  Planned Mileage (km)
                </label>
                <Input
                  type="number"
                  value={formData.plannedMileage}
                  onChange={(e) => setFormData({...formData, plannedMileage: parseInt(e.target.value) || 0})}
                  min="0"
                />
              </div>
                </div>
              </div>
            )}
          </div>

        </div>
      </main>
    </div>
  );
}
