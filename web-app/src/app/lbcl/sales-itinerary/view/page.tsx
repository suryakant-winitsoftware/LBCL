"use client";

import { useEffect, useState } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import { Card, CardContent, CardHeader, CardTitle } from "@/app/lbcl/components/ui/card";
import { Button } from "@/app/lbcl/components/ui/button";
import { Badge } from "@/app/lbcl/components/ui/badge";
import { ArrowLeft, Edit, Trash2 } from "lucide-react";
import { ItineraryEntry } from "@/app/lbcl/components/itinerary-mock-data";

export default function ViewItineraryEntry() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const [entry, setEntry] = useState<ItineraryEntry | null>(null);

  useEffect(() => {
    const id = searchParams.get('id');
    if (id) {
      // Load from localStorage
      const savedEntries = localStorage.getItem('itinerary_entries');
      if (savedEntries) {
        const entries: ItineraryEntry[] = JSON.parse(savedEntries);
        const foundEntry = entries.find(e => e.id === parseInt(id));
        if (foundEntry) {
          setEntry(foundEntry);
        } else {
          alert("Entry not found");
          router.push("/lbcl/sales-itinerary");
        }
      } else {
        alert("No entries found");
        router.push("/lbcl/sales-itinerary");
      }
    }
  }, [searchParams, router]);

  const handleDelete = () => {
    if (!entry) return;

    if (confirm("Are you sure you want to delete this entry?")) {
      const savedEntries = localStorage.getItem('itinerary_entries');
      if (savedEntries) {
        const entries: ItineraryEntry[] = JSON.parse(savedEntries);
        const updatedEntries = entries.filter(e => e.id !== entry.id);
        localStorage.setItem('itinerary_entries', JSON.stringify(updatedEntries));
        console.log("🗑️ Entry deleted");
        router.push("/lbcl/sales-itinerary");
      }
    }
  };

  if (!entry) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <p className="text-gray-500">Loading...</p>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <div className="bg-white border-b border-gray-200 sticky top-0 z-10">
        <div className="max-w-6xl mx-auto px-4 sm:px-6 lg:px-8 py-4">
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-3">
              <Button
                variant="ghost"
                size="sm"
                onClick={() => router.push("/lbcl/sales-itinerary")}
                className="gap-2"
              >
                <ArrowLeft className="w-4 h-4" />
                Back to List
              </Button>
              <div className="border-l border-gray-300 h-6"></div>
              <h1 className="text-xl font-semibold text-[#A08B5C]">Itinerary Entry Details</h1>
            </div>
            <div className="flex gap-2">
              <Button variant="outline" size="sm" className="gap-2">
                <Edit className="w-4 h-4" />
                Edit
              </Button>
              <Button
                variant="outline"
                size="sm"
                className="gap-2 text-red-600 hover:text-red-700 hover:bg-red-50"
                onClick={handleDelete}
              >
                <Trash2 className="w-4 h-4" />
                Delete
              </Button>
            </div>
          </div>
        </div>
      </div>

      {/* Content */}
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-4">
        {/* Single Comprehensive Card */}
        <Card>
          <CardHeader className="bg-gradient-to-r from-[#A08B5C]/10 to-transparent border-b border-gray-200 py-3">
            <div className="flex items-center justify-between">
              <div className="flex items-center gap-3">
                <div className="bg-[#A08B5C] text-white px-3 py-1 rounded-md font-bold text-sm">
                  #{entry.id}
                </div>
                <Badge variant="outline" className="text-sm px-3 py-1 border-[#A08B5C] text-[#A08B5C]">
                  {entry.type}
                </Badge>
              </div>
              <div className="text-sm text-gray-600">
                {entry.date} ({entry.day})
              </div>
            </div>
          </CardHeader>

          <CardContent className="p-4">
            <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
              {/* Left Column */}
              <div className="space-y-4">
                {/* Basic Information */}
                <div className="bg-gray-50 p-3 rounded-md border border-gray-200">
                  <h3 className="text-xs font-semibold text-[#A08B5C] uppercase tracking-wide mb-2 pb-1 border-b border-gray-300">
                    Basic Information
                  </h3>
                  <div className="grid grid-cols-2 gap-x-4 gap-y-2">
                    <div>
                      <label className="text-xs text-gray-500">Activity Type</label>
                      <p className="text-sm font-medium mt-0.5">{entry.type}</p>
                    </div>
                    <div>
                      <label className="text-xs text-gray-500">Focus Area</label>
                      <p className="text-sm font-medium mt-0.5">{entry.focusArea}</p>
                    </div>
                    <div>
                      <label className="text-xs text-gray-500">Date</label>
                      <p className="text-sm font-medium mt-0.5">{entry.date}</p>
                    </div>
                    <div>
                      <label className="text-xs text-gray-500">Day</label>
                      <p className="text-sm font-medium mt-0.5">{entry.day}</p>
                    </div>
                  </div>
                </div>

                {/* Time Details */}
                <div className="bg-gray-50 p-3 rounded-md border border-gray-200">
                  <h3 className="text-xs font-semibold text-[#A08B5C] uppercase tracking-wide mb-2 pb-1 border-b border-gray-300">
                    Time Details
                  </h3>
                  <div className="grid grid-cols-3 gap-x-4 gap-y-2">
                    <div>
                      <label className="text-xs text-gray-500">Morning Meeting</label>
                      <p className="text-sm font-medium mt-0.5">{entry.morningMeeting || "NA"}</p>
                    </div>
                    <div>
                      <label className="text-xs text-gray-500">Time From</label>
                      <p className="text-sm font-medium mt-0.5">{entry.timeFrom}</p>
                    </div>
                    <div>
                      <label className="text-xs text-gray-500">Time To</label>
                      <p className="text-sm font-medium mt-0.5">{entry.timeTo}</p>
                    </div>
                  </div>
                </div>

                {/* Location Details */}
                <div className="bg-gray-50 p-3 rounded-md border border-gray-200">
                  <h3 className="text-xs font-semibold text-[#A08B5C] uppercase tracking-wide mb-2 pb-1 border-b border-gray-300">
                    Location Details
                  </h3>
                  <div className="grid grid-cols-2 gap-x-4 gap-y-2">
                    <div>
                      <label className="text-xs text-gray-500">Market</label>
                      <p className="text-sm font-medium mt-0.5">{entry.market}</p>
                    </div>
                    <div>
                      <label className="text-xs text-gray-500">Channel</label>
                      <p className="text-sm font-medium mt-0.5">{entry.channel || "NA"}</p>
                    </div>
                    <div>
                      <label className="text-xs text-gray-500">Route No.</label>
                      <p className="text-sm font-medium mt-0.5">{entry.routeNo || "NA"}</p>
                    </div>
                    <div>
                      <label className="text-xs text-gray-500">Outlet No.</label>
                      <p className="text-sm font-medium mt-0.5">{entry.outletNo || "NA"}</p>
                    </div>
                  </div>
                </div>
              </div>

              {/* Right Column */}
              <div className="space-y-4">
                {/* KRA Plan */}
                <div className="bg-gray-50 p-3 rounded-md border border-gray-200">
                  <h3 className="text-xs font-semibold text-[#A08B5C] uppercase tracking-wide mb-2 pb-1 border-b border-gray-300">
                    KRA Plan
                  </h3>
                  <p className="text-sm leading-relaxed">{entry.kraPlan}</p>
                </div>

                {/* Team & Logistics */}
                <div className="bg-gray-50 p-3 rounded-md border border-gray-200">
                  <h3 className="text-xs font-semibold text-[#A08B5C] uppercase tracking-wide mb-2 pb-1 border-b border-gray-300">
                    Team & Logistics
                  </h3>
                  <div className="grid grid-cols-2 gap-x-4 gap-y-2">
                    <div>
                      <label className="text-xs text-gray-500">Accompanied By</label>
                      <p className="text-sm font-medium mt-0.5">{entry.accompaniedBy}</p>
                    </div>
                    <div>
                      <label className="text-xs text-gray-500">Night Out Market</label>
                      <p className="text-sm font-medium mt-0.5">{entry.nightOut}</p>
                    </div>
                    <div>
                      <label className="text-xs text-gray-500">No. of Days</label>
                      <p className="text-sm font-medium mt-0.5">{entry.noOfDays}</p>
                    </div>
                    <div>
                      <label className="text-xs text-gray-500">Planned Mileage</label>
                      <p className="text-sm font-medium mt-0.5">{entry.plannedMileage} km</p>
                    </div>
                  </div>
                </div>

                {/* Additional Information */}
                {(entry.kraReview || entry.comments) && (
                  <div className="bg-gray-50 p-3 rounded-md border border-gray-200">
                    <h3 className="text-xs font-semibold text-[#A08B5C] uppercase tracking-wide mb-2 pb-1 border-b border-gray-300">
                      Additional Information
                    </h3>
                    <div className="space-y-2">
                      {entry.kraReview && (
                        <div>
                          <label className="text-xs text-gray-500">KRA Review</label>
                          <p className="text-sm mt-0.5 leading-relaxed">{entry.kraReview}</p>
                        </div>
                      )}
                      {entry.comments && (
                        <div>
                          <label className="text-xs text-gray-500">Comments</label>
                          <p className="text-sm mt-0.5 leading-relaxed">{entry.comments}</p>
                        </div>
                      )}
                    </div>
                  </div>
                )}
              </div>
            </div>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
