"use client"

import { useState } from "react"
import { useRouter } from "next/navigation"
import { PageHeader } from "@/components/page-header"
import { Calendar, RefreshCw, ChevronRight } from "lucide-react"

interface ReconciliationItem {
  id: string
  warehouse: string
  date: string
  status: "Completed" | "Pending" | "In Progress"
  processingStatus: "Completed" | "Pending" | "In Progress"
  adjustmentStatus: "Approved" | "Pending" | "Rejected"
}

const mockData: ReconciliationItem[] = [
  {
    id: "85444127121",
    warehouse: "[5844] LBCL WAREHOUSE",
    date: "24 NOV 2024",
    status: "Completed",
    processingStatus: "Completed",
    adjustmentStatus: "Approved",
  },
  {
    id: "85444127121",
    warehouse: "[5844] LBCL WAREHOUSE",
    date: "24 NOV 2024",
    status: "Completed",
    processingStatus: "Completed",
    adjustmentStatus: "Pending",
  },
]

export function StockReconciliationList() {
  const router = useRouter()
  const [activeTab, setActiveTab] = useState<"ALL" | "PENDING" | "APPROVED" | "REJECTED">("ALL")
  const [fromDate, setFromDate] = useState("04 Nov 2024")
  const [toDate, setToDate] = useState("07 Nov 2024")

  const getStatusColor = (status: string) => {
    switch (status) {
      case "Completed":
      case "Approved":
        return "text-green-600"
      case "Pending":
        return "text-orange-500"
      case "Rejected":
        return "text-red-600"
      default:
        return "text-gray-600"
    }
  }

  const handleItemClick = (id: string) => {
    router.push(`/stock-reconciliation/${id}`)
  }

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <PageHeader title="Perform Warehouse Stock Take Reconciliation" />

      {/* Date Range Filter */}
      <div className="bg-white border-b border-gray-200 px-4 py-4">
        <div className="max-w-7xl mx-auto flex flex-wrap items-center gap-4">
          <div className="flex items-center gap-2 flex-1 min-w-[200px]">
            <span className="text-sm font-semibold text-gray-700 whitespace-nowrap">From Date</span>
            <div className="flex items-center gap-2 flex-1">
              <input
                type="text"
                value={fromDate}
                onChange={(e) => setFromDate(e.target.value)}
                className="flex-1 px-3 py-2 border border-gray-300 rounded-lg text-sm text-gray-600"
              />
              <button className="p-2 hover:bg-gray-100 rounded-lg">
                <Calendar className="w-5 h-5 text-[#A08B5C]" />
              </button>
            </div>
          </div>

          <div className="flex items-center gap-2 flex-1 min-w-[200px]">
            <span className="text-sm font-semibold text-gray-700 whitespace-nowrap">To Date</span>
            <div className="flex items-center gap-2 flex-1">
              <input
                type="text"
                value={toDate}
                onChange={(e) => setToDate(e.target.value)}
                className="flex-1 px-3 py-2 border border-gray-300 rounded-lg text-sm text-gray-600"
              />
              <button className="p-2 hover:bg-gray-100 rounded-lg">
                <Calendar className="w-5 h-5 text-[#A08B5C]" />
              </button>
            </div>
          </div>

          <button className="p-2 hover:bg-gray-100 rounded-lg">
            <RefreshCw className="w-5 h-5 text-[#A08B5C]" />
          </button>
        </div>
      </div>

      {/* Tabs */}
      <div className="bg-white border-b border-gray-200">
        <div className="max-w-7xl mx-auto px-4">
          <div className="flex gap-8">
            {(["ALL", "PENDING", "APPROVED", "REJECTED"] as const).map((tab) => (
              <button
                key={tab}
                onClick={() => setActiveTab(tab)}
                className={`py-4 px-2 font-semibold text-sm sm:text-base transition-colors relative ${
                  activeTab === tab ? "text-gray-900" : "text-gray-500 hover:text-gray-700"
                }`}
              >
                {tab}
                {activeTab === tab && (
                  <div className="absolute bottom-0 left-0 right-0 h-1 bg-[#A08B5C] rounded-t-full" />
                )}
              </button>
            ))}
          </div>
        </div>
      </div>

      {/* Content */}
      <main className="max-w-7xl mx-auto px-4 py-6">
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
          {mockData.map((item, index) => (
            <button
              key={`${item.id}-${index}`}
              onClick={() => handleItemClick(item.id)}
              className="bg-white rounded-lg shadow-sm hover:shadow-md transition-shadow p-6 text-left border border-gray-200"
            >
              <div className="flex items-start justify-between mb-4">
                <div className="flex-1">
                  <h3 className="text-xl font-bold text-gray-900 mb-1">{item.id}</h3>
                  <p className="text-base font-semibold text-gray-700 mb-1">{item.warehouse}</p>
                  <p className="text-sm text-gray-500">{item.date}</p>
                </div>
                <ChevronRight className="w-6 h-6 text-gray-400 flex-shrink-0 ml-4" />
              </div>

              <div className="bg-[#FFF8E7] rounded-lg p-4">
                <div className="grid grid-cols-3 gap-4">
                  <div>
                    <p className="text-xs text-gray-600 mb-1">Status</p>
                    <p className={`text-sm font-semibold ${getStatusColor(item.status)}`}>{item.status}</p>
                  </div>
                  <div>
                    <p className="text-xs text-gray-600 mb-1">Processing Status</p>
                    <p className={`text-sm font-semibold ${getStatusColor(item.processingStatus)}`}>
                      {item.processingStatus}
                    </p>
                  </div>
                  <div>
                    <p className="text-xs text-gray-600 mb-1">Adjustment Status</p>
                    <p className={`text-sm font-semibold ${getStatusColor(item.adjustmentStatus)}`}>
                      {item.adjustmentStatus}
                    </p>
                  </div>
                </div>
              </div>
            </button>
          ))}
        </div>
      </main>
    </div>
  )
}
