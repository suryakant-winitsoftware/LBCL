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
    <div>
      {/* Tabs */}
      <div className="bg-white border-b border-gray-200">
        <div className="flex">
          {(["ALL", "PENDING", "APPROVED"] as const).map((tab) => (
            <button
              key={tab}
              onClick={() => setActiveTab(tab)}
              className={`flex-1 py-4 text-center font-semibold transition-colors relative ${
                activeTab === tab ? "text-[#A08B5C]" : "text-gray-500"
              }`}
            >
              {tab}
              {activeTab === tab && <div className="absolute bottom-0 left-0 right-0 h-1 bg-[#A08B5C]" />}
            </button>
          ))}
        </div>
      </div>

      {/* Records List */}
      <div className="p-4 space-y-4">
        {filteredRecords.map((record) => (
          <button
            key={record.emptiesCode}
            onClick={() => router.push("/lbcl/empties-receiving/activity-log")}
            className="w-full bg-white rounded-lg shadow-sm border border-gray-200 overflow-hidden hover:shadow-md transition-shadow"
          >
            {/* Main Info */}
            <div className="p-4 grid grid-cols-3 gap-4">
              <div>
                <div className="text-xs text-gray-500 mb-1">Empties Code</div>
                <div className="font-bold text-gray-900">{record.emptiesCode}</div>
              </div>
              <div>
                <div className="text-xs text-gray-500 mb-1">Agent</div>
                <div className="font-bold text-gray-900">{record.agent}</div>
              </div>
              <div className="flex items-center justify-end">
                <ChevronRight className="w-6 h-6 text-gray-400" />
              </div>
            </div>

            <div className="px-4 pb-4">
              <div className="text-xs text-gray-500 mb-1">Date</div>
              <div className="font-bold text-gray-900">{record.date}</div>
            </div>

            {/* Status Bar */}
            <div className="bg-[#FFF8E7] px-4 py-3 flex items-center justify-between border-t border-gray-200">
              <div>
                <div className="text-xs text-gray-600 mb-1">Collected Qty</div>
                <div className="font-bold text-gray-900">{record.collectedQty}</div>
              </div>
              <div className="h-12 w-px bg-gray-300" />
              <div>
                <div className="text-xs text-gray-600 mb-1">Approval Status</div>
                <div
                  className={`font-bold ${record.approvalStatus === "Pending" ? "text-orange-600" : "text-green-600"}`}
                >
                  {record.approvalStatus}
                </div>
              </div>
            </div>
          </button>
        ))}
      </div>
    </div>
  )
}
