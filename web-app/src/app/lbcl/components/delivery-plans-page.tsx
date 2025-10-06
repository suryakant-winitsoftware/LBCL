"use client"

import { useState, useEffect } from "react"
import { Button } from "@/components/ui/button"
import { Calendar, RefreshCw, ChevronRight, RotateCcw } from "lucide-react"
import { useRouter } from "next/navigation"
import purchaseOrderService from "@/services/purchaseOrder"

export function DeliveryPlansPage() {
  const router = useRouter()
  const [activeTab, setActiveTab] = useState("pending")
  const [fromDate, setFromDate] = useState("")
  const [toDate, setToDate] = useState("")
  const [dateFilterType, setDateFilterType] = useState<"order" | "delivery">("order")
  const [deliveryPlans, setDeliveryPlans] = useState<any[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState("")
  const [totalCount, setTotalCount] = useState(0)

  // Initialize dates to current date range (last 15 days)
  useEffect(() => {
    const today = new Date()
    const fifteenDaysAgo = new Date(today.getTime() - 15 * 24 * 60 * 60 * 1000)

    setFromDate(fifteenDaysAgo.toISOString().split('T')[0])
    setToDate(today.toISOString().split('T')[0])
  }, [])

  // Fetch delivery plans when tab or dates change
  useEffect(() => {
    if (fromDate && toDate) {
      fetchDeliveryPlans()
    }
  }, [activeTab, fromDate, toDate, dateFilterType])

  const fetchDeliveryPlans = async () => {
    try {
      setLoading(true)
      setError("")

      console.log("🔍 Fetching delivery plans for tab:", activeTab)

      let response: any;

      // For "approved" tab, we need to fetch both CONFIRMED and SHIPPED orders
      if (activeTab === "approved") {
        const filters = {
          startDate: fromDate,
          endDate: toDate,
          page: 1,
          pageSize: 100
        }

        // Fetch both CONFIRMED and SHIPPED orders
        const [confirmedResponse, shippedResponse] = await Promise.all([
          purchaseOrderService.getPurchaseOrdersByStatus("CONFIRMED", { ...filters, status: "CONFIRMED" }),
          purchaseOrderService.getPurchaseOrdersByStatus("SHIPPED", { ...filters, status: "SHIPPED" })
        ])

        // Combine the results and remove duplicates based on UID
        const confirmedHeaders = confirmedResponse.success ? (confirmedResponse.headers || []) : []
        const shippedHeaders = shippedResponse.success ? (shippedResponse.headers || []) : []

        const allHeaders = [...confirmedHeaders, ...shippedHeaders]

        // Remove duplicates by UID
        const uniqueHeaders = allHeaders.filter((plan, index, self) => {
          const uid = plan.UID || plan.uid
          return index === self.findIndex(p => (p.UID || p.uid) === uid)
        })

        response = {
          success: true,
          headers: uniqueHeaders
        }
      } else {
        // For other tabs, use single status
        const statusMap: Record<string, string> = {
          pending: "PENDING",
          received: "RECEIVED"
        }

        const status = statusMap[activeTab] || "PENDING"

        const filters = {
          status: status,
          startDate: fromDate,
          endDate: toDate,
          page: 1,
          pageSize: 100
        }

        response = await purchaseOrderService.getPurchaseOrdersByStatus(status, filters)
      }

      console.log("📦 API Response:", response)
      console.log("📊 Filtered plans:", response.headers?.length || 0)

      if (response.success) {
        // Additional client-side filtering for date range and status validation
        const filteredPlans = (response.headers || []).filter((plan: any) => {
          const planStatus = plan.Status || plan.status || ''
          console.log(`Plan ${plan.OrderNumber}: Status = "${planStatus}", Tab = "${activeTab}"`)

          // Validate status based on tab
          let statusValid = false
          if (activeTab === "pending") {
            statusValid = planStatus === "PENDING"
          } else if (activeTab === "approved") {
            statusValid = planStatus === "CONFIRMED" || planStatus === "SHIPPED"
          } else if (activeTab === "received") {
            statusValid = planStatus === "RECEIVED"
          }

          if (!statusValid) {
            console.log(`❌ Filtered out: Status "${planStatus}" not valid for tab "${activeTab}"`)
            return false
          }

          // Check date range based on selected filter type
          const dateToCheck = dateFilterType === "order"
            ? (plan.OrderDate || plan.orderDate)
            : (plan.RequestedDeliveryDate || plan.requestedDeliveryDate || plan.ExpectedDeliveryDate || plan.expectedDeliveryDate)

          if (!dateToCheck) return true // Include if date is missing

          const checkDate = new Date(dateToCheck).setHours(0, 0, 0, 0)
          const from = new Date(fromDate).setHours(0, 0, 0, 0)
          const to = new Date(toDate).setHours(23, 59, 59, 999)

          return checkDate >= from && checkDate <= to
        })

        console.log("✅ Final filtered plans:", filteredPlans.length)

        setDeliveryPlans(filteredPlans)
        setTotalCount(filteredPlans.length)
      } else {
        setError(response.error || "Failed to fetch delivery plans")
        setDeliveryPlans([])
        setTotalCount(0)
      }
    } catch (error) {
      console.error("Error fetching delivery plans:", error)
      setError("Failed to fetch delivery plans")
      setDeliveryPlans([])
      setTotalCount(0)
    } finally {
      setLoading(false)
    }
  }

  const handleRefresh = () => {
    fetchDeliveryPlans()
  }

  const handleReset = () => {
    const today = new Date()
    const fifteenDaysAgo = new Date(today.getTime() - 15 * 24 * 60 * 60 * 1000)

    setFromDate(fifteenDaysAgo.toISOString().split('T')[0])
    setToDate(today.toISOString().split('T')[0])
    setDateFilterType("order")
    setActiveTab("pending")
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

  const formatDateTime = (dateString: string) => {
    if (!dateString) return "N/A"
    const date = new Date(dateString)
    return date.toLocaleDateString('en-GB', {
      day: '2-digit',
      month: 'short',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    }).toUpperCase()
  }

  const getStatusDisplay = (status: string) => {
    const statusMap: Record<string, string> = {
      DRAFT: "Pending Approval",
      PENDING: "Pending Approval",
      CONFIRMED: "Approved Pending Shipment",
      SHIPPED: "Shipped",
      DISPATCHED: "Shipped",
      SENT: "Sent to SAP",
      RECEIVED: "Received",
      REJECTED: "Rejected",
      ERROR: "Error"
    }
    return statusMap[status] || status
  }

  return (
    <div>
      {/* Date Filters */}
      <div className="px-4 sm:px-6 lg:px-8 py-3 sm:py-4 bg-gray-50 border-t">
        <div className="flex flex-col gap-3">
          {/* Date Type Selector */}
          <div className="flex items-center gap-4">
            <span className="text-sm font-medium">Filter by:</span>
            <div className="flex gap-2">
              <button
                onClick={() => setDateFilterType("order")}
                className={`px-4 py-2 text-sm rounded-lg transition-colors ${
                  dateFilterType === "order"
                    ? "bg-[#A08B5C] text-white"
                    : "bg-white text-gray-600 border hover:bg-gray-100"
                }`}
              >
                Order Date
              </button>
              <button
                onClick={() => setDateFilterType("delivery")}
                className={`px-4 py-2 text-sm rounded-lg transition-colors ${
                  dateFilterType === "delivery"
                    ? "bg-[#A08B5C] text-white"
                    : "bg-white text-gray-600 border hover:bg-gray-100"
                }`}
              >
                Delivery Date
              </button>
            </div>
          </div>

          {/* Date Range Inputs */}
          <div className="flex flex-col sm:flex-row items-stretch sm:items-center gap-3 sm:gap-4 max-w-4xl">
            <div className="flex items-center gap-2 flex-1">
              <span className="text-sm font-medium whitespace-nowrap">From Date</span>
              <div className="bg-white px-3 py-2 rounded-lg border flex-1">
                <input
                  type="date"
                  value={fromDate}
                  onChange={(e) => setFromDate(e.target.value)}
                  className="text-sm text-gray-600 border-none outline-none w-full"
                />
              </div>
            </div>

            <div className="flex items-center gap-2 flex-1">
              <span className="text-sm font-medium whitespace-nowrap">To Date</span>
              <div className="bg-white px-3 py-2 rounded-lg border flex-1">
                <input
                  type="date"
                  value={toDate}
                  onChange={(e) => setToDate(e.target.value)}
                  className="text-sm text-gray-600 border-none outline-none w-full"
                />
              </div>
            </div>

            <div className="flex gap-2 self-center sm:self-auto">
              <button
                onClick={handleRefresh}
                className="p-2 hover:bg-gray-200 rounded-lg"
                disabled={loading}
                title="Reload"
              >
                <RefreshCw className={`w-5 h-5 text-[#A08B5C] ${loading ? 'animate-spin' : ''}`} />
              </button>
              <button
                onClick={handleReset}
                className="p-2 hover:bg-gray-200 rounded-lg"
                disabled={loading}
                title="Reset Filters"
              >
                <RotateCcw className="w-5 h-5 text-[#A08B5C]" />
              </button>
            </div>
          </div>
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
          onClick={() => setActiveTab("received")}
          className={`flex-1 min-w-[120px] px-4 py-3 sm:py-4 text-xs sm:text-sm font-medium whitespace-nowrap ${
            activeTab === "received" ? "text-[#A08B5C] border-b-2 border-[#A08B5C]" : "text-gray-600"
          }`}
        >
          SHIPPED
        </button>
      </div>

      {/* Delivery Plans List */}
      <main className="px-4 sm:px-6 lg:px-8 py-4 sm:py-6">
        {loading ? (
          <div className="flex items-center justify-center py-12">
            <div className="text-center">
              <RefreshCw className="w-8 h-8 text-[#A08B5C] animate-spin mx-auto mb-2" />
              <p className="text-gray-600">Loading delivery plans...</p>
            </div>
          </div>
        ) : error ? (
          <div className="bg-red-50 border border-red-200 rounded-lg p-4 text-center">
            <p className="text-red-700">{error}</p>
            <button
              onClick={handleRefresh}
              className="mt-2 text-sm text-red-600 hover:text-red-800 underline"
            >
              Try again
            </button>
          </div>
        ) : deliveryPlans.length === 0 ? (
          <div className="bg-gray-50 border border-gray-200 rounded-lg p-8 text-center">
            <p className="text-gray-600 mb-2">No delivery plans found</p>
            <p className="text-sm text-gray-500">Try adjusting your date range or filters</p>
          </div>
        ) : (
          <div>
            <div className="mb-4 text-sm text-gray-600">
              Showing {deliveryPlans.length} of {totalCount} delivery plan{totalCount !== 1 ? 's' : ''}
            </div>
            <div className="grid grid-cols-1 lg:grid-cols-2 gap-4 max-w-7xl mx-auto">
              {deliveryPlans.map((plan) => {
                return (
                  <button
                    key={plan.UID || plan.uid}
                    onClick={() => {
                      router.push(`/lbcl/activity-log/${plan.UID || plan.uid}`)
                    }}
                    className="bg-white rounded-lg p-4 sm:p-6 shadow-sm hover:shadow-md transition-shadow text-left cursor-pointer"
                  >
                  <div className="flex items-start justify-between gap-4">
                    <div className="flex-1 min-w-0">
                      <h3 className="text-base sm:text-lg font-bold mb-2 break-all">
                        {plan.OrderNumber || plan.orderNumber || plan.DraftOrderNumber || plan.draftOrderNumber || 'N/A'}
                      </h3>
                      <p className="text-sm sm:text-base mb-2 break-words">
                        <span className="font-bold text-gray-900">
                          {plan.OrgName || plan.orgName || plan.ChannelPartnerName || plan.channelPartnerName || plan.OrgUID || plan.orgUID || 'N/A'}
                        </span>
                        {' '}
                        <span className="text-orange-500 font-medium">
                          {getStatusDisplay(plan.Status || plan.status || '')}
                        </span>
                      </p>

                      {/* Order Date and Delivery Date */}
                      <div className="space-y-1 mb-2">
                        <div className="flex items-center gap-2 text-xs sm:text-sm text-gray-600">
                          <span className="font-medium">Order Date:</span>
                          <span>{formatDate(plan.OrderDate || plan.orderDate || '')}</span>
                        </div>
                        {plan.RequestedDeliveryDate || plan.requestedDeliveryDate || plan.ExpectedDeliveryDate || plan.expectedDeliveryDate ? (
                          <div className="flex items-center gap-2 text-xs sm:text-sm text-gray-600">
                            <span className="font-medium">Delivery Date:</span>
                            <span className="text-green-600 font-medium">
                              {formatDate(plan.RequestedDeliveryDate || plan.requestedDeliveryDate || plan.ExpectedDeliveryDate || plan.expectedDeliveryDate)}
                            </span>
                          </div>
                        ) : null}
                      </div>

                    </div>
                    <ChevronRight className="w-5 h-5 sm:w-6 sm:h-6 text-gray-400 flex-shrink-0 mt-1" />
                  </div>
                </button>
                );
              })}
            </div>
          </div>
        )}
      </main>
    </div>
  )
}
