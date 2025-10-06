"use client"

import { useState } from "react"
import { useRouter } from "next/navigation"
import { ChevronRight, FileText, ClipboardList } from "lucide-react"
import { PageHeader } from "@/components/page-header"

const deliveries = [
  {
    id: "85444127121",
    deliveryNoteNo: "DN4673899",
    orderDate: "20-MAY-2025",
    uniqueSkus: 40,
    status: "pending",
  },
  {
    id: "85444127954",
    deliveryNoteNo: "DN4673834",
    orderDate: "20-MAY-2025",
    uniqueSkus: 20,
    status: "pending",
  },
  {
    id: "85444127633",
    deliveryNoteNo: "DN4673536",
    orderDate: "20-MAY-2025",
    uniqueSkus: 20,
    status: "pending",
  },
]

export default function StockReceivingList() {
  const [activeTab, setActiveTab] = useState<"pending" | "completed">("pending")
  const router = useRouter()

  return (
    <div className="min-h-screen bg-gray-50">
      <PageHeader title="Manage Agent Stock Receiving" />

      <div className="bg-white border-b border-gray-200">
        <div className="flex">
          <button
            onClick={() => setActiveTab("pending")}
            className={`flex-1 py-4 text-center font-semibold transition-colors relative ${
              activeTab === "pending" ? "text-[#A08B5C]" : "text-gray-500"
            }`}
          >
            PENDING
            {activeTab === "pending" && <div className="absolute bottom-0 left-0 right-0 h-1 bg-[#A08B5C]" />}
          </button>
          <button
            onClick={() => setActiveTab("completed")}
            className={`flex-1 py-4 text-center font-semibold transition-colors relative ${
              activeTab === "completed" ? "text-[#A08B5C]" : "text-gray-500"
            }`}
          >
            COMPLETED
            {activeTab === "completed" && <div className="absolute bottom-0 left-0 right-0 h-1 bg-[#A08B5C]" />}
          </button>
        </div>
      </div>

      <div className="p-4 space-y-4">
        {activeTab === "pending" ? (
          deliveries.map((delivery) => (
            <div
              key={delivery.id}
              className="bg-white rounded-lg shadow-sm border border-gray-200 overflow-hidden hover:shadow-md transition-shadow"
            >
              <div className="p-4 flex items-center gap-4">
                {/* Delivery Plan No */}
                <div className="flex-shrink-0">
                  <div className="text-xs text-gray-500 mb-1">Delivery Plan No</div>
                  <div className="font-bold text-gray-900 text-sm">{delivery.id}</div>
                </div>

                {/* Delivery Note No */}
                <div className="flex-shrink-0">
                  <div className="text-xs text-gray-500 mb-1">Delivery Note No</div>
                  <div className="font-bold text-gray-900 text-sm">{delivery.deliveryNoteNo}</div>
                </div>

                {/* Order Date */}
                <div className="flex-shrink-0">
                  <div className="text-xs text-gray-500 mb-1">Order Date</div>
                  <div className="font-bold text-gray-900 text-sm">{delivery.orderDate}</div>
                </div>

                {/* No of Unique SKU */}
                <div className="flex-shrink-0">
                  <div className="text-xs text-gray-500 mb-1">No of Unique Sku</div>
                  <div className="font-bold text-gray-900 text-sm">{delivery.uniqueSkus}</div>
                </div>

                {/* Action Buttons */}
                <div className="ml-auto flex items-center gap-2">
                  <button
                    onClick={() => router.push(`/stock-receiving/${delivery.id}/activity-log`)}
                    className="flex flex-col items-center justify-center px-3 py-2 text-gray-600 hover:text-[#A08B5C] transition-colors"
                  >
                    <ClipboardList className="w-6 h-6 mb-1" />
                    <span className="text-xs font-medium">Activity Log</span>
                  </button>

                  <button
                    onClick={() => router.push(`/stock-receiving/${delivery.id}`)}
                    className="flex flex-col items-center justify-center px-3 py-2 text-gray-600 hover:text-[#A08B5C] transition-colors"
                  >
                    <FileText className="w-6 h-6 mb-1" />
                    <span className="text-xs font-medium">View Details</span>
                  </button>

                  <ChevronRight className="w-6 h-6 text-gray-400 ml-2" />
                </div>
              </div>
            </div>
          ))
        ) : (
          <div className="text-center py-12 text-gray-500">No completed deliveries</div>
        )}
      </div>
    </div>
  )
}
