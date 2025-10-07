"use client"

import { useState, useEffect } from "react"
import { use } from "react"
import StockReceivingActivityLog from "@/app/lbcl/components/stock-receiving-activity-log"
import { stockReceivingService } from "@/services/stockReceivingService"

export default function ActivityLogPage({ params }: { params: Promise<{ id: string }> }) {
  const { id } = use(params)
  const [isReadOnly, setIsReadOnly] = useState(false)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    const checkStatus = async () => {
      try {
        const stockReceiving = await stockReceivingService.getByWHStockRequestUID(id)
        if (stockReceiving && stockReceiving.Status === "RECEIVED") {
          setIsReadOnly(true)
        }
      } catch (error) {
        console.error("Error checking stock receiving status:", error)
      } finally {
        setLoading(false)
      }
    }

    checkStatus()
  }, [id])

  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-[#A08B5C] mx-auto"></div>
          <p className="mt-4 text-gray-600">Loading...</p>
        </div>
      </div>
    )
  }

  return <StockReceivingActivityLog deliveryId={id} readOnly={isReadOnly} />
}
