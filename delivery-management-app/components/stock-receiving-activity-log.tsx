"use client"

import { useState } from "react"
import { ChevronDown, ChevronRight, Clock, Bell } from "lucide-react"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select"
import { Checkbox } from "@/components/ui/checkbox"
import { Dialog, DialogContent, DialogTitle } from "@/components/ui/dialog"
import { useRouter } from "next/navigation"

export default function StockReceivingActivityLog({ deliveryId }: { deliveryId: string }) {
  const router = useRouter()
  const [expandedSections, setExpandedSections] = useState<Record<number, boolean>>({
    2: true,
    4: true,
    6: true,
  })
  const [showDeliveryNote, setShowDeliveryNote] = useState(false)

  const toggleSection = (section: number) => {
    setExpandedSections((prev) => ({ ...prev, [section]: !prev[section] }))
  }

  return (
    <div className="min-h-screen bg-white">
      {/* Header */}
      <header className="bg-white border-b border-gray-200 px-4 py-4 flex items-center justify-between sticky top-0 z-10">
        <h1 className="text-base sm:text-lg md:text-xl font-bold text-center flex-1 px-2">
          Agent Stock Receiving Activity Log Report
        </h1>
        <div className="flex gap-2">
          <button className="p-2">
            <Clock className="w-6 h-6" />
          </button>
          <button className="p-2">
            <Bell className="w-6 h-6" />
          </button>
          <Button
            variant="outline"
            className="border-[#A08B5C] text-[#A08B5C] bg-transparent"
            onClick={() => router.back()}
          >
            Back
          </Button>
          <Button className="bg-[#A08B5C] hover:bg-[#8A7549] text-white">Submit</Button>
        </div>
      </header>

      {/* Info Section */}
      <div className="bg-gray-50 p-4 border-b border-gray-200">
        <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-6 gap-4 text-sm">
          <div>
            <div className="text-gray-600 mb-1">Agent Stock Receiving No</div>
            <div className="font-bold">ASR{deliveryId}</div>
          </div>
          <div>
            <div className="text-gray-600 mb-1">SAP Delivery Note No</div>
            <div className="font-bold">{deliveryId}</div>
          </div>
          <div>
            <div className="text-gray-600 mb-1">Distributor</div>
            <div className="font-bold">[5844] R.T DISTRIBUTOR</div>
          </div>
          <div>
            <div className="text-gray-600 mb-1">Prime Mover</div>
            <div className="font-bold">LK1673 (U KUMAR)</div>
          </div>
          <div>
            <div className="text-gray-600 mb-1">Date</div>
            <div className="font-bold">20 MAY 2025</div>
          </div>
          <div>
            <div className="text-gray-600 mb-1">LBCL Departure Time</div>
            <div className="font-bold">13:26 HRS</div>
          </div>
        </div>
      </div>

      {/* Activity Steps */}
      <div className="p-4 grid grid-cols-1 lg:grid-cols-2 gap-4">
        {/* Left Column */}
        <div className="space-y-4">
          {/* Step 1: View Delivery Note */}
          <div className="bg-white border border-gray-200 rounded-lg p-4">
            <button onClick={() => setShowDeliveryNote(true)} className="flex items-center justify-between w-full">
              <div className="flex items-center gap-3">
                <div className="w-10 h-10 bg-[#F5E6D3] rounded-full flex items-center justify-center font-bold">1</div>
                <span className="font-semibold">View Delivery Note</span>
              </div>
              <div className="flex items-center gap-2">
                <span className="text-red-600 text-sm">PDF</span>
                <ChevronRight className="w-5 h-5" />
              </div>
            </button>
          </div>

          {/* Step 2: Gate Entry */}
          <div className="bg-white border border-gray-200 rounded-lg p-4">
            <button onClick={() => toggleSection(2)} className="flex items-center justify-between w-full mb-4">
              <div className="flex items-center gap-3">
                <div className="w-10 h-10 bg-[#F5E6D3] rounded-full flex items-center justify-center font-bold">2</div>
                <span className="font-semibold">Gate Entry</span>
              </div>
              {expandedSections[2] ? <ChevronDown className="w-5 h-5" /> : <ChevronRight className="w-5 h-5" />}
            </button>

            {expandedSections[2] && (
              <div className="space-y-4 pl-13">
                <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                  <div>
                    <label className="text-sm font-medium mb-2 block">Select Security Officer</label>
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
                    <label className="text-sm font-medium mb-2 block">Prime Mover Arrival</label>
                    <div className="flex gap-2">
                      <Input type="number" placeholder="15" className="w-20" />
                      <span className="text-gray-400 self-center">HH</span>
                      <Input type="number" placeholder="00" className="w-20" />
                      <span className="text-gray-400 self-center">Min</span>
                    </div>
                  </div>
                </div>
                <div className="flex items-center gap-2">
                  <Checkbox id="notify-lbcl" defaultChecked />
                  <label htmlFor="notify-lbcl" className="text-sm font-medium">
                    Notify LBCL Logistics
                  </label>
                </div>
              </div>
            )}
          </div>

          {/* Step 3: Physical Count */}
          <div className="bg-white border border-gray-200 rounded-lg p-4">
            <button
              onClick={() => router.push(`/stock-receiving/${deliveryId}`)}
              className="flex items-center justify-between w-full"
            >
              <div className="flex items-center gap-3">
                <div className="w-10 h-10 bg-[#F5E6D3] rounded-full flex items-center justify-center font-bold">3</div>
                <span className="font-semibold">Physical Count & Perform Stock Receiving</span>
              </div>
              <ChevronRight className="w-5 h-5" />
            </button>
          </div>
        </div>

        {/* Right Column */}
        <div className="space-y-4">
          {/* Step 4: Unloading */}
          <div className="bg-white border border-gray-200 rounded-lg p-4">
            <button onClick={() => toggleSection(4)} className="flex items-center justify-between w-full mb-4">
              <div className="flex items-center gap-3">
                <div className="w-10 h-10 bg-[#F5E6D3] rounded-full flex items-center justify-center font-bold">4</div>
                <span className="font-semibold">Unloading</span>
              </div>
              {expandedSections[4] ? <ChevronDown className="w-5 h-5" /> : <ChevronRight className="w-5 h-5" />}
            </button>

            {expandedSections[4] && (
              <div className="space-y-4 pl-13">
                <div>
                  <label className="text-sm font-medium mb-2 block">Agent Fork Lift Operator</label>
                  <Select defaultValue="tarun">
                    <SelectTrigger>
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="tarun">Tarun Prasad</SelectItem>
                    </SelectContent>
                  </Select>
                </div>
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <label className="text-sm font-medium mb-2 block">Unload Start Time</label>
                    <div className="flex gap-2">
                      <Input type="number" placeholder="16" className="w-16" />
                      <span className="text-gray-400 self-center text-xs">HH</span>
                      <Input type="number" placeholder="02" className="w-16" />
                      <span className="text-gray-400 self-center text-xs">Min</span>
                    </div>
                  </div>
                  <div>
                    <label className="text-sm font-medium mb-2 block">Unload End Time</label>
                    <div className="flex gap-2">
                      <Input type="number" placeholder="16" className="w-16" />
                      <span className="text-gray-400 self-center text-xs">HH</span>
                      <Input type="number" placeholder="58" className="w-16" />
                      <span className="text-gray-400 self-center text-xs">Min</span>
                    </div>
                  </div>
                </div>
              </div>
            )}
          </div>

          {/* Step 5: Load Empty Stock */}
          <div className="bg-white border border-gray-200 rounded-lg p-4">
            <button className="flex items-center justify-between w-full mb-4">
              <div className="flex items-center gap-3">
                <div className="w-10 h-10 bg-[#F5E6D3] rounded-full flex items-center justify-center font-bold">5</div>
                <span className="font-semibold">Load Empty Stock</span>
              </div>
              <ChevronRight className="w-5 h-5" />
            </button>

            <div className="space-y-4 pl-13">
              <div>
                <label className="text-sm font-medium mb-2 block">Agent Fork Lift Operator</label>
                <Select defaultValue="tarun">
                  <SelectTrigger>
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="tarun">Tarun Prasad</SelectItem>
                  </SelectContent>
                </Select>
              </div>
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="text-sm font-medium mb-2 block">Load Start Time</label>
                  <div className="flex gap-2">
                    <Input type="number" placeholder="17" className="w-16" />
                    <span className="text-gray-400 self-center text-xs">HH</span>
                    <Input type="number" placeholder="25" className="w-16" />
                    <span className="text-gray-400 self-center text-xs">Min</span>
                  </div>
                </div>
                <div>
                  <label className="text-sm font-medium mb-2 block">Load End Time</label>
                  <div className="flex gap-2">
                    <Input type="number" placeholder="18" className="w-16" />
                    <span className="text-gray-400 self-center text-xs">HH</span>
                    <Input type="number" placeholder="08" className="w-16" />
                    <span className="text-gray-400 self-center text-xs">Min</span>
                  </div>
                </div>
              </div>
            </div>
          </div>

          {/* Step 6: Gate Pass */}
          <div className="bg-white border border-gray-200 rounded-lg p-4">
            <button onClick={() => toggleSection(6)} className="flex items-center justify-between w-full mb-4">
              <div className="flex items-center gap-3">
                <div className="w-10 h-10 bg-[#F5E6D3] rounded-full flex items-center justify-center font-bold">6</div>
                <span className="font-semibold">Gate Pass (Empties)</span>
              </div>
              {expandedSections[6] ? <ChevronDown className="w-5 h-5" /> : <ChevronRight className="w-5 h-5" />}
            </button>

            {expandedSections[6] && (
              <div className="space-y-4 pl-13">
                <div>
                  <label className="text-sm font-medium mb-2 block">Select Security Officer</label>
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
                  <label className="text-sm font-medium mb-2 block">Prime Mover Departure</label>
                  <div className="flex gap-2">
                    <Input type="number" placeholder="18" className="w-20" />
                    <span className="text-gray-400 self-center">HH</span>
                    <Input type="number" placeholder="14" className="w-20" />
                    <span className="text-gray-400 self-center">Min</span>
                  </div>
                </div>
                <div className="flex items-center gap-2">
                  <Checkbox id="notify-lbcl-2" defaultChecked />
                  <label htmlFor="notify-lbcl-2" className="text-sm font-medium">
                    Notify LBCL Logistics
                  </label>
                </div>
              </div>
            )}
          </div>
        </div>
      </div>

      {/* Delivery Note Dialog */}
      <Dialog open={showDeliveryNote} onOpenChange={setShowDeliveryNote}>
        <DialogContent className="max-w-4xl max-h-[90vh]">
          <DialogTitle className="text-xl font-bold">View Delivery Note</DialogTitle>
          <div className="flex items-center justify-between mb-4">
            <button onClick={() => setShowDeliveryNote(false)} className="text-2xl ml-auto">
              ×
            </button>
          </div>
          <div className="bg-gray-100 rounded-lg p-4 min-h-[500px] flex items-center justify-center">
            <p className="text-gray-500">PDF Preview would appear here</p>
          </div>
          <Button
            className="w-full bg-[#A08B5C] hover:bg-[#8A7549] text-white"
            onClick={() => setShowDeliveryNote(false)}
          >
            DONE
          </Button>
        </DialogContent>
      </Dialog>
    </div>
  )
}
