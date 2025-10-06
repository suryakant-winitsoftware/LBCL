"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { ChevronRight, ChevronDown, FileText, Check } from "lucide-react";
import { ShareDialog } from "@/components/share-dialog";
import { PickListDialog } from "@/components/pick-list-dialog";
import { SignatureDialog } from "@/components/signature-dialog";
import { NavigationMenu } from "@/components/navigation-menu";

interface ActivityLogPageProps {
  deliveryPlanId: string;
}

export function ActivityLogPage({ deliveryPlanId }: ActivityLogPageProps) {
  const router = useRouter();
  const [expandedSections, setExpandedSections] = useState<number[]>([3, 6]);
  const [showShareDialog, setShowShareDialog] = useState(false);
  const [showPickListDialog, setShowPickListDialog] = useState(false);
  const [showSignatureDialog, setShowSignatureDialog] = useState(false);

  const toggleSection = (section: number) => {
    setExpandedSections((prev) =>
      prev.includes(section)
        ? prev.filter((s) => s !== section)
        : [...prev, section]
    );
  };

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <header className="bg-white border-b sticky top-0 z-10">
        <div className="flex items-center justify-between px-4 sm:px-6 lg:px-8 py-3 sm:py-4">
          <NavigationMenu />

          <h1 className="text-base sm:text-lg lg:text-xl font-bold text-center flex-1 lg:flex-none">
            Delivery Plan Activity Log Report
          </h1>

          <div className="flex items-center gap-2">
            <Button
              variant="outline"
              onClick={() => router.push("/lbcl-plans")}
              className="text-[#A08B5C] border-[#A08B5C] text-xs sm:text-sm px-3 sm:px-6 bg-transparent"
            >
              Back
            </Button>
            <Button className="bg-[#A08B5C] hover:bg-[#8F7A4B] text-white text-xs sm:text-sm px-3 sm:px-6">
              Submit
            </Button>
          </div>
        </div>

        {/* Delivery Info */}
        <div className="px-4 sm:px-6 lg:px-8 py-3 sm:py-4 bg-gray-50 border-t">
          <div className="grid grid-cols-1 sm:grid-cols-3 gap-3 sm:gap-4 text-sm">
            <div>
              <div className="font-medium text-gray-600 mb-1">
                Delivery Plan No
              </div>
              <div className="font-bold break-all">{deliveryPlanId}</div>
            </div>
            <div>
              <div className="font-medium text-gray-600 mb-1">
                Distributor / Agent
              </div>
              <div className="font-bold break-words">
                [5844] R.T DISTRIBUTOR
              </div>
            </div>
            <div>
              <div className="font-medium text-gray-600 mb-1">Date</div>
              <div className="font-bold">20 MAY 2025</div>
            </div>
          </div>
        </div>
      </header>

      {/* Activity Steps */}
      <main className="px-4 sm:px-6 lg:px-8 py-4 sm:py-6">
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-4 max-w-7xl mx-auto">
          {/* Step 1: Share Delivery Plan */}
          <button
            onClick={() => setShowShareDialog(true)}
            className="bg-white rounded-lg p-4 sm:p-6 shadow-sm hover:shadow-md transition-shadow"
          >
            <div className="flex items-center gap-3 sm:gap-4">
              <div className="w-8 h-8 sm:w-10 sm:h-10 rounded-full bg-[#F5E6C8] flex items-center justify-center flex-shrink-0">
                <span className="font-bold text-sm sm:text-base">1</span>
              </div>
              <span className="text-sm sm:text-base font-medium flex-1 text-left">
                Share Delivery Plan
              </span>
              <ChevronRight className="w-5 h-5 text-gray-400 flex-shrink-0" />
            </div>
          </button>

          {/* Step 2: View/Generate Pick List */}
          <button
            onClick={() => setShowPickListDialog(true)}
            className="bg-white rounded-lg p-4 sm:p-6 shadow-sm hover:shadow-md transition-shadow"
          >
            <div className="flex items-center gap-3 sm:gap-4">
              <div className="w-8 h-8 sm:w-10 sm:h-10 rounded-full bg-[#F5E6C8] flex items-center justify-center flex-shrink-0">
                <span className="font-bold text-sm sm:text-base">2</span>
              </div>
              <span className="text-sm sm:text-base font-medium flex-1 text-left">
                View / Generate Pick List
              </span>
              <FileText className="w-4 h-4 text-red-500 flex-shrink-0" />
              <ChevronRight className="w-5 h-5 text-gray-400 flex-shrink-0" />
            </div>
          </button>

          {/* Step 3: Loading */}
          <div className="bg-white rounded-lg p-4 sm:p-6 shadow-sm lg:col-span-2">
            <button
              onClick={() => toggleSection(3)}
              className="flex items-center gap-3 sm:gap-4 w-full"
            >
              <div className="w-8 h-8 sm:w-10 sm:h-10 rounded-full bg-[#F5E6C8] flex items-center justify-center flex-shrink-0">
                <span className="font-bold text-sm sm:text-base">3</span>
              </div>
              <span className="text-sm sm:text-base font-medium flex-1 text-left">
                Loading
              </span>
              <ChevronDown
                className={`w-5 h-5 text-gray-400 transition-transform flex-shrink-0 ${
                  expandedSections.includes(3) ? "rotate-180" : ""
                }`}
              />
            </button>

            {expandedSections.includes(3) && (
              <div className="mt-4 sm:mt-6 space-y-4">
                <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                  <div>
                    <label className="text-xs sm:text-sm font-medium mb-2 block">
                      Select Vehicle / Prime Mover
                    </label>
                    <Select defaultValue="LK1673">
                      <SelectTrigger>
                        <SelectValue />
                      </SelectTrigger>
                      <SelectContent>
                        <SelectItem value="LK1673">LK1673</SelectItem>
                      </SelectContent>
                    </Select>
                  </div>
                  <div>
                    <label className="text-xs sm:text-sm font-medium mb-2 block">
                      Select Driver
                    </label>
                    <Select defaultValue="ukumar">
                      <SelectTrigger>
                        <SelectValue />
                      </SelectTrigger>
                      <SelectContent>
                        <SelectItem value="ukumar">U Kumar</SelectItem>
                      </SelectContent>
                    </Select>
                  </div>
                </div>

                <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                  <div>
                    <label className="text-xs sm:text-sm font-medium mb-2 block">
                      Fork Lift Operator
                    </label>
                    <Select defaultValue="praveen">
                      <SelectTrigger>
                        <SelectValue />
                      </SelectTrigger>
                      <SelectContent>
                        <SelectItem value="praveen">Praveen Varma</SelectItem>
                      </SelectContent>
                    </Select>
                  </div>
                  <div className="grid grid-cols-2 gap-2">
                    <div>
                      <label className="text-xs sm:text-sm font-medium mb-2 block">
                        Load Start Time
                      </label>
                      <div className="flex gap-2">
                        <Input defaultValue="11" className="text-center" />
                        <span className="text-xs text-gray-500 self-center">
                          HH
                        </span>
                        <Input defaultValue="14" className="text-center" />
                        <span className="text-xs text-gray-500 self-center">
                          Min
                        </span>
                      </div>
                    </div>
                    <div>
                      <label className="text-xs sm:text-sm font-medium mb-2 block">
                        Load End Time
                      </label>
                      <div className="flex gap-2">
                        <Input defaultValue="12" className="text-center" />
                        <span className="text-xs text-gray-500 self-center">
                          HH
                        </span>
                        <Input defaultValue="25" className="text-center" />
                        <span className="text-xs text-gray-500 self-center">
                          Min
                        </span>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            )}
          </div>

          {/* Step 4: View/Generate Delivery Note */}
          <button className="bg-white rounded-lg p-4 sm:p-6 shadow-sm hover:shadow-md transition-shadow">
            <div className="flex items-center gap-3 sm:gap-4">
              <div className="w-8 h-8 sm:w-10 sm:h-10 rounded-full bg-[#F5E6C8] flex items-center justify-center flex-shrink-0">
                <span className="font-bold text-sm sm:text-base">4</span>
              </div>
              <span className="text-sm sm:text-base font-medium flex-1 text-left">
                View / Generate Delivery Note
              </span>
              <FileText className="w-4 h-4 text-red-500 flex-shrink-0" />
              <ChevronRight className="w-5 h-5 text-gray-400 flex-shrink-0" />
            </div>
          </button>

          {/* Step 5: Receive Stock */}
          <button
            onClick={() => setShowSignatureDialog(true)}
            className="bg-white rounded-lg p-4 sm:p-6 shadow-sm hover:shadow-md transition-shadow"
          >
            <div className="flex items-center gap-3 sm:gap-4">
              <div className="w-8 h-8 sm:w-10 sm:h-10 rounded-full bg-[#F5E6C8] flex items-center justify-center flex-shrink-0">
                <span className="font-bold text-sm sm:text-base">5</span>
              </div>
              <span className="text-sm sm:text-base font-medium flex-1 text-left">
                Receive Stock
              </span>
              <FileText className="w-4 h-4 text-red-500 flex-shrink-0" />
              <ChevronRight className="w-5 h-5 text-gray-400 flex-shrink-0" />
            </div>
          </button>

          {/* Step 6: Gate Pass */}
          <div className="bg-white rounded-lg p-4 sm:p-6 shadow-sm lg:col-span-2">
            <button
              onClick={() => toggleSection(6)}
              className="flex items-center gap-3 sm:gap-4 w-full"
            >
              <div className="w-8 h-8 sm:w-10 sm:h-10 rounded-full bg-[#F5E6C8] flex items-center justify-center flex-shrink-0">
                <span className="font-bold text-sm sm:text-base">6</span>
              </div>
              <span className="text-sm sm:text-base font-medium flex-1 text-left">
                Gate Pass
              </span>
              <ChevronDown
                className={`w-5 h-5 text-gray-400 transition-transform flex-shrink-0 ${
                  expandedSections.includes(6) ? "rotate-180" : ""
                }`}
              />
            </button>

            {expandedSections.includes(6) && (
              <div className="mt-4 sm:mt-6 space-y-4">
                <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                  <div>
                    <label className="text-xs sm:text-sm font-medium mb-2 block">
                      Select Security Officer
                    </label>
                    <Select defaultValue="vasanth">
                      <SelectTrigger>
                        <SelectValue />
                      </SelectTrigger>
                      <SelectContent>
                        <SelectItem value="vasanth">Vasanth Kumar</SelectItem>
                      </SelectContent>
                    </Select>
                  </div>
                  <div>
                    <label className="text-xs sm:text-sm font-medium mb-2 block">
                      Prime Mover Departure
                    </label>
                    <div className="flex gap-2">
                      <Input defaultValue="13" className="text-center flex-1" />
                      <span className="text-xs text-gray-500 self-center">
                        HH
                      </span>
                      <Input defaultValue="26" className="text-center flex-1" />
                      <span className="text-xs text-gray-500 self-center">
                        Min
                      </span>
                    </div>
                  </div>
                </div>

                <div className="flex items-center gap-2">
                  <Check className="w-5 h-5 text-green-600" />
                  <span className="text-sm text-gray-700">Notify Agency</span>
                </div>
              </div>
            )}
          </div>
        </div>

        {/* Progress Tracker */}
        <div className="mt-6 sm:mt-8 bg-white rounded-lg p-4 sm:p-6 shadow-sm max-w-7xl mx-auto overflow-x-auto">
          <div className="flex items-center gap-2 sm:gap-4 min-w-max pb-2">
            {/* Checkpoint 1 */}
            <div className="flex flex-col items-center flex-shrink-0">
              <div className="w-8 h-8 sm:w-10 sm:h-10 rounded-full bg-green-500 flex items-center justify-center mb-2">
                <Check className="w-4 h-4 sm:w-5 sm:h-5 text-white" />
              </div>
              <div className="text-center">
                <div className="text-xs sm:text-sm font-medium">
                  LBCL Warehouse
                </div>
                <div className="text-xs text-green-600">Completed</div>
                <div className="text-xs text-gray-500">Arrived: 08:00 AM</div>
                <div className="text-xs text-gray-500">Distance: 0 km</div>
                <div className="text-xs text-gray-500">
                  Travel time: 1h 25min
                </div>
              </div>
            </div>

            <div className="h-1 flex-1 bg-green-500 min-w-[40px]" />

            {/* Checkpoint 2 */}
            <div className="flex flex-col items-center flex-shrink-0">
              <div className="w-8 h-8 sm:w-10 sm:h-10 rounded-full bg-green-500 flex items-center justify-center mb-2">
                <Check className="w-4 h-4 sm:w-5 sm:h-5 text-white" />
              </div>
              <div className="text-center">
                <div className="text-xs sm:text-sm font-medium">
                  Checkpoint Alpha
                </div>
                <div className="text-xs text-green-600">Completed</div>
                <div className="text-xs text-gray-500">
                  Arrived: 09:40 AM (+5 min)
                </div>
                <div className="text-xs text-gray-500">
                  Distance: 48 km (+3 km)
                </div>
                <div className="text-xs text-gray-500">
                  Travel time: 1h 35min
                </div>
              </div>
            </div>

            <div className="h-1 flex-1 bg-yellow-500 min-w-[40px]" />

            {/* Checkpoint 3 */}
            <div className="flex flex-col items-center flex-shrink-0">
              <div className="w-8 h-8 sm:w-10 sm:h-10 rounded-full bg-blue-500 flex items-center justify-center mb-2">
                <span className="text-white text-xs sm:text-sm font-bold">
                  3
                </span>
              </div>
              <div className="text-center">
                <div className="text-xs sm:text-sm font-medium">
                  Checkpoint Beta
                </div>
                <div className="text-xs text-blue-600">In Progress</div>
                <div className="text-xs text-gray-500">
                  Arrived: 11:40 AM (+30 min)
                </div>
                <div className="text-xs text-gray-500">Distance: 33 km</div>
                <div className="text-xs text-gray-500">
                  Travel time: 1h 15min
                </div>
              </div>
            </div>

            <div className="h-1 flex-1 bg-gray-300 min-w-[40px]" />

            {/* Checkpoint 4 */}
            <div className="flex flex-col items-center flex-shrink-0">
              <div className="w-8 h-8 sm:w-10 sm:h-10 rounded-full bg-gray-300 flex items-center justify-center mb-2">
                <span className="text-gray-600 text-xs sm:text-sm font-bold">
                  4
                </span>
              </div>
              <div className="text-center">
                <div className="text-xs sm:text-sm font-medium">
                  Distributor Agent Warehouse
                </div>
                <div className="text-xs text-gray-500">Pending</div>
                <div className="text-xs text-gray-500">
                  Distance: 40 km (planned)
                </div>
              </div>
            </div>
          </div>

          <div className="mt-4 p-3 bg-red-50 rounded-lg">
            <p className="text-xs sm:text-sm text-red-600 flex items-center gap-2">
              <span className="flex-shrink-0">⚠️</span>
              <span>
                Current pace suggests arrival at destination may be delayed by
                ~15 minutes
              </span>
            </p>
          </div>
        </div>
      </main>

      {/* Dialogs */}
      <ShareDialog open={showShareDialog} onOpenChange={setShowShareDialog} />
      <PickListDialog
        open={showPickListDialog}
        onOpenChange={setShowPickListDialog}
      />
      <SignatureDialog
        open={showSignatureDialog}
        onOpenChange={setShowSignatureDialog}
      />
    </div>
  );
}
