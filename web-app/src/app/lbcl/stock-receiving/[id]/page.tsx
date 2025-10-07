"use client"

import { useState, useEffect } from "react"
import { use } from "react"
import PhysicalCountPage from "@/app/lbcl/components/physical-count-page"
import { stockReceivingService } from "@/services/stockReceivingService"

export default function StockReceivingDetailsPage({
  params,
}: {
  params: Promise<{ id: string }>
}) {
  const { id } = use(params)
  const [isReadOnly, setIsReadOnly] = useState(false)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    const checkStatus = async () => {
      try {
        // Check if stock receiving is completed
        const stockReceiving = await stockReceivingService.getByWHStockRequestUID(id)
        console.log("📦 Stock Receiving Status:", stockReceiving)

        // Only set read-only if physical count is already completed
        if (stockReceiving && stockReceiving.PhysicalCountEndTime) {
          console.log("🔒 Physical count already completed - setting to read-only")
          setIsReadOnly(true)
        } else {
          console.log("✏️ Physical count not completed - allowing edit")
          setIsReadOnly(false)
        }
      } catch (error) {
        console.error("Error checking stock receiving status:", error)
        // If no stock receiving record exists, allow editing
        setIsReadOnly(false)
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

  return <PhysicalCountPage deliveryId={id} readOnly={isReadOnly} />
}
