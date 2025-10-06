"use client"

import { useState } from "react"
import { Button } from "@/components/ui/button"
import { Calendar, RefreshCw, ChevronRight } from "lucide-react"
import { useRouter } from "next/navigation"
import { PageHeader } from "@/components/page-header"

const deliveryPlans = [
  {
    id: "85444127121",
    distributor: "[5844] R.T DISTRIBUTOR",
    status: "Pending Approval",
    date: "28 OCT, 2025 AT 14:23",
  },
  {
    id: "85444127122",
    distributor: "[5844] R.T DISTRIBUTOR",
    status: "Pending Approval",
    date: "28 OCT, 2025 AT 14:23",
  },
]

export function DeliveryPlansPage() {
  const router = useRouter()
  const [activeTab, setActiveTab] = useState("pending")
  const [fromDate, setFromDate] = useState("05-May-2025")
  const [toDate, setToDate] = useState("20-May-2025")

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <PageHeader
        title="Delivery Plan"
        rightContent={
          <Button
            variant="ghost"
            className="text-[#A08B5C] text-xs sm:text-sm font-medium hidden sm:inline-flex"
            onClick={() => router.push("/activity-log")}
          >
            VIEW ACTIVITY LOG
          </Button>
        }
      />

      {/* Date Filters */}
      <div className="px-4 sm:px-6 lg:px-8 py-3 sm:py-4 bg-gray-50 border-t">
        <div className="flex flex-col sm:flex-row items-stretch sm:items-center gap-3 sm:gap-4 max-w-4xl">
          <div className="flex items-center gap-2 flex-1">
            <span className="text-sm font-medium whitespace-nowrap">From Date</span>
            <div className="flex items-center gap-2 bg-white px-3 py-2 rounded-lg border flex-1">
              <span className="text-sm text-gray-600">{fromDate}</span>
              <Calendar className="w-4 h-4 text-[#A08B5C] ml-auto" />
            </div>
          </div>

          <div className="flex items-center gap-2 flex-1">
            <span className="text-sm font-medium whitespace-nowrap">To Date</span>
            <div className="flex items-center gap-2 bg-white px-3 py-2 rounded-lg border flex-1">
              <span className="text-sm text-gray-600">{toDate}</span>
              <Calendar className="w-4 h-4 text-[#A08B5C] ml-auto" />
            </div>
          </div>

          <button className="p-2 hover:bg-gray-200 rounded-lg self-center sm:self-auto">
            <RefreshCw className="w-5 h-5 text-[#A08B5C]" />
          </button>
        </div>
      </div>

      {/* Tabs */}
      <div className="flex border-t overflow-x-auto">
        <button
          onClick={() => setActiveTab("pending")}
          className={`flex-1 min-w-[120px] px-4 py-3 sm:py-4 text-xs sm:text-sm font-medium whitespace-nowrap ${
            activeTab === "pending" ? "text-[#A08B5C] border-b-2 border-[#A08B5C]" : "text-gray-600"
          }`}
        >
          PENDING APPROVAL
        </button>
        <button
          onClick={() => setActiveTab("approved")}
          className={`flex-1 min-w-[180px] px-4 py-3 sm:py-4 text-xs sm:text-sm font-medium whitespace-nowrap ${
            activeTab === "approved" ? "text-[#A08B5C] border-b-2 border-[#A08B5C]" : "text-gray-600"
          }`}
        >
          APPROVED PENDING SHIPMENT
        </button>
        <button
          onClick={() => setActiveTab("shipped")}
          className={`flex-1 min-w-[120px] px-4 py-3 sm:py-4 text-xs sm:text-sm font-medium whitespace-nowrap ${
            activeTab === "shipped" ? "text-[#A08B5C] border-b-2 border-[#A08B5C]" : "text-gray-600"
          }`}
        >
          SHIPPED
        </button>
      </div>

      {/* Delivery Plans List */}
      <main className="px-4 sm:px-6 lg:px-8 py-4 sm:py-6">
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-4 max-w-7xl mx-auto">
          {deliveryPlans.map((plan) => (
            <button
              key={plan.id}
              onClick={() => router.push(`/activity-log/${plan.id}`)}
              className="bg-white rounded-lg p-4 sm:p-6 shadow-sm hover:shadow-md transition-shadow text-left"
            >
              <div className="flex items-start justify-between gap-4">
                <div className="flex-1 min-w-0">
                  <h3 className="text-base sm:text-lg font-bold mb-2 break-all">{plan.id}</h3>
                  <p className="text-sm sm:text-base text-gray-700 mb-1 break-words">
                    {plan.distributor} <span className="text-orange-500 font-medium">{plan.status}</span>
                  </p>
                  <p className="text-xs sm:text-sm text-gray-500">{plan.date}</p>
                </div>
                <ChevronRight className="w-5 h-5 sm:w-6 sm:h-6 text-gray-400 flex-shrink-0 mt-1" />
              </div>
            </button>
          ))}
        </div>
      </main>
    </div>
  )
}
