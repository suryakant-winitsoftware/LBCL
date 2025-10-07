"use client"

import { useState, useEffect } from "react"
import { useRouter } from "next/navigation"
import { ChevronRight, FileText, ClipboardList, RefreshCw } from "lucide-react"
import { deliveryLoadingService } from "@/services/deliveryLoadingService"
import { stockReceivingService } from "@/services/stockReceivingService"
import { useAuth } from "@/providers/auth-provider"
import { inventoryService } from "@/services/inventory/inventory.service"

export default function StockReceivingList() {
  const [activeTab, setActiveTab] = useState<"pending" | "completed">("pending")
  const [deliveries, setDeliveries] = useState<any[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState("")
  const router = useRouter()
  const { user } = useAuth()

  useEffect(() => {
    fetchDeliveries()
  }, [activeTab, user])

  const fetchDeliveries = async () => {
    try {
      setLoading(true)
      setError("")

      if (!user?.companyUID) {
        console.warn("⚠️ No companyUID found for user")
        setDeliveries([])
        setLoading(false)
        return
      }

      // Fetch SHIPPED deliveries (ready to receive)
      console.log("🔍 Fetching SHIPPED deliveries")
      console.log("🏢 User CompanyUID:", user.companyUID)

      const shippedDeliveries = await deliveryLoadingService.getByStatus("SHIPPED")

      console.log("📦 SHIPPED Deliveries (before filter):", shippedDeliveries)

      // Filter deliveries to only show those matching the user's organization and activeTab
      const filteredDeliveries = await filterDeliveriesByOrganization(shippedDeliveries, user.companyUID)

      console.log("✅ Filtered Delivery Data (after filter):", filteredDeliveries)

      setDeliveries(filteredDeliveries || [])
    } catch (error) {
      console.error("Error fetching deliveries:", error)
      setError("Failed to fetch deliveries")
      setDeliveries([])
    } finally {
      setLoading(false)
    }
  }

  const filterDeliveriesByOrganization = async (deliveries: any[], companyUID: string) => {
    if (!deliveries || deliveries.length === 0) {
      return []
    }

    const filtered = []

    for (const delivery of deliveries) {
      try {
        const whStockRequestUID = delivery.WHStockRequestUID || delivery.whStockRequestUID
        if (!whStockRequestUID) continue

        // Fetch WH Stock Request details
        const whStockRequest = await inventoryService.selectLoadRequestDataByUID(whStockRequestUID)

        if (whStockRequest) {
          const header = whStockRequest.WHStockRequest
          const targetOrgUID = header?.TargetOrgUID
          const targetWHUID = header?.TargetWHUID

          console.log(`🔍 WH Stock Request ${whStockRequestUID}: TargetOrgUID = ${targetOrgUID}, TargetWHUID = ${targetWHUID}, User CompanyUID = ${companyUID}`)

          if (targetOrgUID === companyUID || targetWHUID === companyUID) {
            // Fetch StockReceivingTracking to get the status
            const stockReceiving = await stockReceivingService.getByWHStockRequestUID(whStockRequestUID)
            const receivingStatus = stockReceiving?.Status || null

            console.log(`📋 Stock Receiving Status for ${whStockRequestUID}: ${receivingStatus}`)

            // Filter based on active tab and receiving status
            const isPending = !receivingStatus || receivingStatus === "PENDING" || receivingStatus === "GATE_ENTRY" || receivingStatus === "UNLOADING" || receivingStatus === "LOAD_EMPTY"
            const isCompleted = receivingStatus === "COMPLETED"

            const shouldInclude = (activeTab === "pending" && isPending) || (activeTab === "completed" && isCompleted)

            if (shouldInclude) {
              filtered.push({
                ...delivery,
                request_code: header?.Code || header?.RequestCode,
                created_time: header?.RequestedTime || header?.CreatedTime,
                warehouse_uid: targetWHUID,
                OrgName: header?.TargetOrgName,
                receivingStatus
              })
            }
          }
        }
      } catch (error) {
        console.error(`Error fetching WH Stock Request details for ${delivery.WHStockRequestUID}:`, error)
      }
    }

    console.log(`✅ Filtered ${filtered.length} out of ${deliveries.length} deliveries for ${activeTab} tab`)
    return filtered
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

  const getUniqueSkuCount = (delivery: any) => {
    return delivery.Lines?.length || delivery.lines?.length || 0
  }

  return (
    <div>
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
        {loading ? (
          <div className="flex items-center justify-center py-12">
            <div className="text-center">
              <RefreshCw className="w-8 h-8 text-[#A08B5C] animate-spin mx-auto mb-2" />
              <p className="text-gray-600">Loading deliveries...</p>
            </div>
          </div>
        ) : error ? (
          <div className="bg-red-50 border border-red-200 rounded-lg p-4 text-center">
            <p className="text-red-700">{error}</p>
            <button
              onClick={fetchDeliveries}
              className="mt-2 text-sm text-red-600 hover:text-red-800 underline"
            >
              Try again
            </button>
          </div>
        ) : deliveries.length === 0 ? (
          <div className="text-center py-12 text-gray-500">
            {activeTab === "pending" ? "No pending deliveries" : "No completed deliveries"}
          </div>
        ) : (
          deliveries.map((delivery) => (
            <div
              key={delivery.UID || delivery.uid}
              className="bg-white rounded-lg shadow-sm border border-gray-200 overflow-hidden hover:shadow-md transition-shadow"
            >
              <div className="p-4 flex items-center gap-4">
                {/* Delivery Plan No */}
                <div className="flex-shrink-0">
                  <div className="text-xs text-gray-500 mb-1">Delivery Plan No</div>
                  <div className="font-bold text-gray-900 text-sm">
                    {delivery.request_code || delivery.RequestCode || 'N/A'}
                  </div>
                </div>

                {/* Delivery Note No */}
                <div className="flex-shrink-0">
                  <div className="text-xs text-gray-500 mb-1">Delivery Note No</div>
                  <div className="font-bold text-gray-900 text-sm">
                    {delivery.DeliveryNoteNumber || delivery.deliveryNoteNumber || 'N/A'}
                  </div>
                </div>

                {/* Request Date */}
                <div className="flex-shrink-0">
                  <div className="text-xs text-gray-500 mb-1">Request Date</div>
                  <div className="font-bold text-gray-900 text-sm">
                    {formatDate(delivery.created_time || delivery.CreatedTime || '')}
                  </div>
                </div>

                {/* Warehouse/Distributor */}
                {/* <div className="flex-shrink-0">
                  <div className="text-xs text-gray-500 mb-1">Warehouse</div>
                  <div className="font-bold text-gray-900 text-sm">
                    {delivery.warehouse_uid || delivery.WarehouseUID || 'N/A'}
                  </div>
                </div> */}

                {/* Action Buttons */}
                <div className="ml-auto flex items-center gap-2">
                  <button
                    onClick={() => router.push(`/lbcl/stock-receiving/${delivery.WHStockRequestUID || delivery.whStockRequestUID}/activity-log`)}
                    className="flex flex-col items-center justify-center px-3 py-2 text-gray-600 hover:text-[#A08B5C] transition-colors"
                  >
                    <ClipboardList className="w-6 h-6 mb-1" />
                    <span className="text-xs font-medium">Activity Log</span>
                  </button>

                  <button
                    onClick={() => router.push(`/lbcl/stock-receiving/${delivery.WHStockRequestUID || delivery.whStockRequestUID}`)}
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
    </div>
  )
}
