"use client"

import { Menu, Search, Clock, Bell } from "lucide-react"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { useRouter } from "next/navigation"

const historyData = [
  {
    deliveryPlanNo: "85444127121",
    deliveryNoteNo: "DN4673899",
    orderDate: "20-MAY-2025",
    uniqueSkus: 40,
  },
  {
    deliveryPlanNo: "85444127954",
    deliveryNoteNo: "DN4673834",
    orderDate: "20-MAY-2025",
    uniqueSkus: 20,
  },
  {
    deliveryPlanNo: "85444127633",
    deliveryNoteNo: "DN4673536",
    orderDate: "20-MAY-2025",
    uniqueSkus: 20,
  },
]

export default function StockReceivingHistory() {
  const router = useRouter()

  return (
    <div className="min-h-screen bg-white">
      {/* Header */}
      <header className="bg-white border-b border-gray-200 px-4 py-4 flex items-center gap-4 sticky top-0 z-10">
        <button className="p-2">
          <Menu className="w-6 h-6" />
        </button>
        <h1 className="text-lg sm:text-xl md:text-2xl font-bold flex-1">Agent Stock Receiving History</h1>
        <div className="flex items-center gap-2">
          <button className="p-2">
            <Clock className="w-6 h-6" />
          </button>
          <button className="p-2">
            <Bell className="w-6 h-6" />
          </button>
        </div>
      </header>

      {/* Search */}
      <div className="p-4 bg-gray-50 border-b border-gray-200">
        <div className="flex gap-2">
          <div className="flex-1 relative">
            <Input placeholder="Search by Delivery Number" className="pr-10" />
            <Search className="absolute right-3 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400" />
          </div>
          <Button variant="outline" className="border-[#A08B5C] text-[#A08B5C] bg-transparent">
            Search
          </Button>
        </div>
      </div>

      {/* History List */}
      <div className="p-4 space-y-4">
        {historyData.map((item) => (
          <div key={item.deliveryPlanNo} className="bg-white border border-gray-200 rounded-lg p-4 shadow-sm">
            <div className="grid grid-cols-2 sm:grid-cols-4 gap-4 mb-4">
              <div>
                <div className="text-sm text-gray-600 mb-1">Delivery Plan No</div>
                <div className="font-bold">{item.deliveryPlanNo}</div>
              </div>
              <div>
                <div className="text-sm text-gray-600 mb-1">Delivery Note No</div>
                <div className="font-bold">{item.deliveryNoteNo}</div>
              </div>
              <div>
                <div className="text-sm text-gray-600 mb-1">Order Date</div>
                <div className="font-bold">{item.orderDate}</div>
              </div>
              <div>
                <div className="text-sm text-gray-600 mb-1">No of Unique Sku</div>
                <div className="font-bold">{item.uniqueSkus}</div>
              </div>
            </div>

            <div className="flex flex-col sm:flex-row gap-2 sm:gap-4">
              <Button
                variant="outline"
                className="flex-1 border-[#A08B5C] text-[#A08B5C] hover:bg-[#A08B5C] hover:text-white bg-transparent"
                onClick={() => router.push(`/stock-receiving/${item.deliveryPlanNo}/activity-log`)}
              >
                <span className="mr-2">📋</span>
                Activity Log
              </Button>
              <Button
                variant="outline"
                className="flex-1 border-[#A08B5C] text-[#A08B5C] hover:bg-[#A08B5C] hover:text-white bg-transparent"
                onClick={() => router.push(`/stock-receiving/${item.deliveryPlanNo}`)}
              >
                <span className="mr-2">📄</span>
                View Details
              </Button>
            </div>
          </div>
        ))}
      </div>
    </div>
  )
}
