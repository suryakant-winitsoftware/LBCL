"use client"

import { useState, useEffect } from "react"
import { useRouter } from "next/navigation"
import { ChevronRight, FileText, ClipboardList, RefreshCw } from "lucide-react"
import { stockReceivingService } from "@/services/stockReceivingService"

export default function StockReceivingHistory() {
  const router = useRouter()
  const [historyData, setHistoryData] = useState<any[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState("")

  useEffect(() => {
    fetchHistory()
  }, [])

  const fetchHistory = async () => {
    try {
      setLoading(true)
      setError("")

      console.log("🔍 Fetching all stock receiving history...")

      // Get all completed stock receiving records with joined data
      const stockReceivingRecords = await stockReceivingService.getAll()

      console.log("📦 Stock Receiving Records:", stockReceivingRecords)

      if (!stockReceivingRecords || stockReceivingRecords.length === 0) {
        setHistoryData([])
        return
      }

      // Map records to history format
      const historyWithDetails = stockReceivingRecords.map((record: any) => {
        console.log("📋 Processing record:", {
          PurchaseOrderUID: record.PurchaseOrderUID,
          DeliveryNoteNumber: record.DeliveryNoteNumber,
          order_number: record.order_number,
          order_date: record.order_date,
          CreatedDate: record.CreatedDate
        })

        return {
          id: record.PurchaseOrderUID,
          deliveryNoteNo: record.DeliveryNoteNumber || record.order_number || "N/A",
          orderDate: formatDate(record.order_date || record.CreatedDate),
          uniqueSkus: 0, // Will need to fetch purchase order lines separately if needed
          status: "completed",
        }
      })

      setHistoryData(historyWithDetails)

    } catch (error) {
      console.error("Error fetching history:", error)
      setError("Failed to fetch stock receiving history")
      setHistoryData([])
    } finally {
      setLoading(false)
    }
  }

  const formatDate = (dateString: string) => {
    if (!dateString) return "N/A"
    const date = new Date(dateString)
    return date.toLocaleDateString('en-GB', {
      day: '2-digit',
      month: 'short',
      year: 'numeric'
    }).toUpperCase()
  }

  return (
    <div className="p-4 space-y-4">
      {loading ? (
        <div className="flex items-center justify-center py-12">
          <div className="text-center">
            <RefreshCw className="w-8 h-8 text-[#A08B5C] animate-spin mx-auto mb-2" />
            <p className="text-gray-600">Loading history...</p>
          </div>
        </div>
      ) : error ? (
        <div className="bg-red-50 border border-red-200 rounded-lg p-4 text-center">
          <p className="text-red-700">{error}</p>
          <button
            onClick={fetchHistory}
            className="mt-2 text-sm text-red-600 hover:text-red-800 underline"
          >
            Try again
          </button>
        </div>
      ) : historyData.length === 0 ? (
        <div className="bg-gray-50 border border-gray-200 rounded-lg p-8 text-center">
          <ClipboardList className="w-12 h-12 text-gray-400 mx-auto mb-3" />
          <p className="text-gray-600 text-lg font-medium">No stock receiving history found</p>
          <p className="text-gray-500 text-sm mt-1">Completed stock receiving records will appear here</p>
        </div>
      ) : (
        historyData.map((delivery) => (
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
                  onClick={() => router.push(`/lbcl/stock-receiving-history/${delivery.id}/activity-log`)}
                  className="flex flex-col items-center justify-center px-3 py-2 text-gray-600 hover:text-[#A08B5C] transition-colors"
                >
                  <ClipboardList className="w-6 h-6 mb-1" />
                  <span className="text-xs font-medium">Activity Log</span>
                </button>

                <button
                  onClick={() => router.push(`/lbcl/stock-receiving-history/${delivery.id}`)}
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
      )}
    </div>
  )
}
