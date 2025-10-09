"use client"

import { useState } from "react"
import { useRouter } from "next/navigation"
import { ChevronRight } from "lucide-react"

interface EmptiesRecord {
  emptiesCode: string
  agent: string
  date: string
  collectedQty: number
  approvalStatus: "Pending" | "Approved"
}

export function EmptiesReceivingList() {
  const router = useRouter()
  const [activeTab, setActiveTab] = useState<"ALL" | "PENDING" | "APPROVED">("ALL")

  const records: EmptiesRecord[] = [
    {
      emptiesCode: "64785685678",
      agent: "R.T DISTRIBUTORS",
      date: "23 MAY 2025",
      collectedQty: 45,
      approvalStatus: "Pending",
    },
  ]

  const filteredRecords = records.filter((record) => {
    if (activeTab === "ALL") return true
    return record.approvalStatus.toUpperCase() === activeTab
  })

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <header className="bg-white border-b border-gray-200 px-4 py-4 sticky top-0 z-10">
        <h1 className="text-base sm:text-lg md:text-xl font-bold text-center">
          Empties Receiving
        </h1>
      </header>

      {/* Tabs */}
      <div className="bg-white border-b border-gray-200">
        <div className="flex">
          {(["ALL", "PENDING", "APPROVED"] as const).map((tab) => (
            <button
              key={tab}
              onClick={() => setActiveTab(tab)}
              className={`flex-1 py-5 text-center text-sm font-bold transition-colors relative ${
                activeTab === tab ? "text-[#A08B5C]" : "text-gray-500 hover:text-gray-700"
              }`}
            >
              {tab}
              {activeTab === tab && <div className="absolute bottom-0 left-0 right-0 h-1 bg-[#A08B5C]" />}
            </button>
          ))}
        </div>
      </div>

      {/* Records List */}
      <div className="p-6 space-y-5">
        {filteredRecords.map((record) => (
          <button
            key={record.emptiesCode}
            onClick={() => router.push("/lbcl/empties-receiving/activity-log")}
            className="w-full bg-white rounded-xl shadow-sm border border-gray-200 overflow-hidden hover:shadow-lg hover:border-[#A08B5C]/30 transition-all"
          >
            {/* Main Info */}
            <div className="p-5">
              <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-5 mb-4">
                <div>
                  <div className="text-xs text-gray-600 mb-2 font-medium">Empties Code</div>
                  <div className="font-bold text-base text-gray-900">{record.emptiesCode}</div>
                </div>
                <div>
                  <div className="text-xs text-gray-600 mb-2 font-medium">Agent</div>
                  <div className="font-bold text-base text-gray-900">{record.agent}</div>
                </div>
                <div>
                  <div className="text-xs text-gray-600 mb-2 font-medium">Date</div>
                  <div className="font-bold text-base text-gray-900">{record.date}</div>
                </div>
              </div>

              {/* Status Bar */}
              <div className="bg-[#FFF8E7] -mx-5 -mb-5 mt-4 px-5 py-4 flex items-center justify-between border-t border-gray-200">
                <div className="flex items-center gap-8">
                  <div>
                    <div className="text-xs text-gray-700 mb-1 font-medium">Collected Qty</div>
                    <div className="font-bold text-lg text-gray-900">{record.collectedQty}</div>
                  </div>
                  <div className="h-12 w-px bg-gray-300" />
                  <div>
                    <div className="text-xs text-gray-700 mb-1 font-medium">Approval Status</div>
                    <div
                      className={`font-bold text-base ${record.approvalStatus === "Pending" ? "text-orange-600" : "text-green-600"}`}
                    >
                      {record.approvalStatus}
                    </div>
                  </div>
                </div>
                <div className="flex items-center justify-center w-10 h-10 bg-white rounded-full shadow-sm">
                  <ChevronRight className="w-5 h-5 text-[#A08B5C]" />
                </div>
              </div>
            </div>
          </button>
        ))}
      </div>
    </div>
  )
}
