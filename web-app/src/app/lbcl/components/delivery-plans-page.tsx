"use client"

import { useState, useEffect } from "react"
import { Button } from "@/components/ui/button"
import { Calendar, RefreshCw, ChevronRight, RotateCcw } from "lucide-react"
import { useRouter } from "next/navigation"
import { inventoryService, type PagingRequest, type IWHStockRequestItemView } from "@/services/inventory/inventory.service"

export function DeliveryPlansPage() {
  const router = useRouter()
  const [activeTab, setActiveTab] = useState("pending")
  const [fromDate, setFromDate] = useState("")
  const [toDate, setToDate] = useState("")
  const [dateFilterType, setDateFilterType] = useState<"order" | "delivery">("order")
  const [deliveryPlans, setDeliveryPlans] = useState<IWHStockRequestItemView[]>([])
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

      console.log("🔍 Fetching load requests for tab:", activeTab)

      const statusMap: Record<string, string[]> = {
        pending: ["Pending"],
        approved: ["Approved"],
        shipped: ["SHIPPED"]
      }

      const allowedStatuses = statusMap[activeTab] || ["Pending"]

      // Fetch all records without status filter (filter client-side instead)
      const pagingRequest: PagingRequest = {
        pageNumber: 1,
        pageSize: 1000, // Increased to get all records
        isCountRequired: true,
        filterCriterias: [], // No server-side filters
        sortCriterias: [] // No server-side sorting - will sort client-side
      }

      console.log("📤 Request:", pagingRequest)

      const response = await inventoryService.selectLoadRequestData(
        pagingRequest,
        "all" // StockType parameter
      )

      console.log("📦 API Response:", response)
      console.log("📊 Load requests count:", response.PagedData?.length || 0)

      if (response && response.PagedData) {
        // Client-side filtering for both status and date
        const filteredData = response.PagedData.filter((plan: IWHStockRequestItemView) => {
          // Status filter - check if plan status is in allowed statuses
          const planStatus = plan.Status || ''
          if (!allowedStatuses.includes(planStatus)) {
            return false
          }

          // Date filter
          const dateToCheck = dateFilterType === "order"
            ? (plan.RequestedTime || plan.ModifiedTime)
            : (plan.RequiredByDate)

          if (!dateToCheck) return true

          const checkDate = new Date(dateToCheck).setHours(0, 0, 0, 0)
          const from = new Date(fromDate).setHours(0, 0, 0, 0)
          const to = new Date(toDate).setHours(23, 59, 59, 999)

          return checkDate >= from && checkDate <= to
        })

        // Client-side sorting by ModifiedTime (newest first)
        const sortedData = filteredData.sort((a, b) => {
          const dateA = new Date(a.ModifiedTime || a.RequestedTime || 0).getTime()
          const dateB = new Date(b.ModifiedTime || b.RequestedTime || 0).getTime()
          return dateB - dateA // Descending order (newest first)
        })

        console.log("✅ After filtering - Allowed Statuses:", allowedStatuses, "Count:", sortedData.length)

        setDeliveryPlans(sortedData)
        setTotalCount(sortedData.length)
      } else {
        console.warn("⚠️ No data returned")
        setDeliveryPlans([])
        setTotalCount(0)
      }
    } catch (error) {
      console.error("Error fetching load requests:", error)
      setError("Failed to fetch load requests")
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
      Pending: "Pending Approval",
      Approved: "Approved",
      SHIPPED: "Shipped",
      Completed: "Completed",
      RECEIVED: "Received",
      Rejected: "Rejected"
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
                Request Date
              </button>
              <button
                onClick={() => setDateFilterType("delivery")}
                className={`px-4 py-2 text-sm rounded-lg transition-colors ${
                  dateFilterType === "delivery"
                    ? "bg-[#A08B5C] text-white"
                    : "bg-white text-gray-600 border hover:bg-gray-100"
                }`}
              >
                Required By Date
              </button>
            </div>
          </div>

          {/* Date Range Inputs */}
          <div className="flex flex-col sm:flex-row items-stretch sm:items-center gap-3 sm:gap-4 max-w-4xl">
            <div className="flex items-center gap-2 flex-1">
              <span className="text-sm font-medium whitespace-nowrap">From Date</span>
              <div className="relative flex-1 max-w-xs">
                <input
                  type="date"
                  value={fromDate}
                  onChange={(e) => setFromDate(e.target.value)}
                  className="w-full px-4 py-2.5 pr-10 border rounded-lg focus:outline-none focus:ring-2 focus:ring-[#A08B5C] text-sm"
                />
                <Calendar className="absolute right-3 top-1/2 transform -translate-y-1/2 text-gray-400 h-4 w-4 pointer-events-none" />
              </div>
            </div>

            <div className="flex items-center gap-2 flex-1">
              <span className="text-sm font-medium whitespace-nowrap">To Date</span>
              <div className="relative flex-1 max-w-xs">
                <input
                  type="date"
                  value={toDate}
                  onChange={(e) => setToDate(e.target.value)}
                  className="w-full px-4 py-2.5 pr-10 border rounded-lg focus:outline-none focus:ring-2 focus:ring-[#A08B5C] text-sm"
                />
                <Calendar className="absolute right-3 top-1/2 transform -translate-y-1/2 text-gray-400 h-4 w-4 pointer-events-none" />
              </div>
            </div>

            <div className="flex gap-2">
              <Button
                onClick={handleRefresh}
                variant="outline"
                size="sm"
                className="border-[#A08B5C] text-[#A08B5C] hover:bg-[#A08B5C] hover:text-white whitespace-nowrap"
              >
                <RefreshCw className="w-4 h-4 mr-2" />
                Refresh
              </Button>

              <Button
                onClick={handleReset}
                variant="outline"
                size="sm"
                className="border-gray-300 text-gray-600 hover:bg-gray-100 whitespace-nowrap"
              >
                <RotateCcw className="w-4 h-4 mr-2" />
                Reset
              </Button>
            </div>
          </div>
        </div>
      </div>

      {/* Tabs */}
      <div className="flex overflow-x-auto bg-white border-b sticky top-0 z-10 scrollbar-hide">
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
          className={`flex-1 min-w-[120px] px-4 py-3 sm:py-4 text-xs sm:text-sm font-medium whitespace-nowrap ${
            activeTab === "approved" ? "text-[#A08B5C] border-b-2 border-[#A08B5C]" : "text-gray-600"
          }`}
        >
          APPROVED/PENDING SHIPMENT
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
        {loading ? (
          <div className="flex items-center justify-center py-12">
            <div className="text-center">
              <RefreshCw className="w-8 h-8 text-[#A08B5C] animate-spin mx-auto mb-2" />
              <p className="text-gray-600">Loading load requests...</p>
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
            <p className="text-gray-600 mb-2">No load requests found</p>
            <p className="text-sm text-gray-500">Try adjusting your date range or filters</p>
          </div>
        ) : (
          <div>
            <div className="mb-4 text-sm text-gray-600">
              Showing {deliveryPlans.length} of {totalCount} load request{totalCount !== 1 ? 's' : ''}
            </div>
            <div className="grid grid-cols-1 lg:grid-cols-2 gap-4 max-w-7xl mx-auto">
              {deliveryPlans.map((plan) => {
                return (
                  <button
                    key={plan.UID}
                    onClick={() => {
                      router.push(`/lbcl/activity-log/${plan.UID}`)
                    }}
                    className="bg-white rounded-lg p-4 sm:p-6 shadow-sm hover:shadow-md transition-shadow text-left cursor-pointer"
                  >
                  <div className="flex items-start justify-between gap-4">
                    <div className="flex-1 min-w-0">
                      <h3 className="text-base sm:text-lg font-bold mb-2 break-all">
                        {plan.RequestCode || plan.UID || 'N/A'}
                      </h3>
                      <p className="text-sm sm:text-base mb-2 break-words">
                        <span className="font-bold text-gray-900">
                          {plan.TargetOrgName || plan.TargetWHName || 'N/A'}
                        </span>
                        {' '}
                        <span className="text-orange-500 font-medium">
                          {getStatusDisplay(plan.Status || '')}
                        </span>
                      </p>

                      {/* Requested On and Required By Date */}
                      <div className="space-y-1 mb-2">
                        <div className="flex items-center gap-2 text-xs sm:text-sm text-gray-600">
                          <span className="font-medium">Requested On:</span>
                          <span>{formatDateTime(plan.RequestedTime || '')}</span>
                        </div>
                        {plan.RequiredByDate ? (
                          <div className="flex items-center gap-2 text-xs sm:text-sm text-gray-600">
                            <span className="font-medium">Required By:</span>
                            <span className="text-green-600 font-medium">
                              {formatDate(plan.RequiredByDate)}
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
