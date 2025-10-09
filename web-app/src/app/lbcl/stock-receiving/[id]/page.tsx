"use client"

import { useState, useEffect } from "react"
import { use } from "react"
import { useRouter } from "next/navigation"
import PhysicalCountPage from "@/app/lbcl/components/physical-count-page"
import { stockReceivingService } from "@/services/stockReceivingService"
import { inventoryService } from "@/services/inventory/inventory.service"
import { useAuth } from "@/providers/auth-provider"

export default function StockReceivingDetailsPage({
  params,
}: {
  params: Promise<{ id: string }>
}) {
  const { id } = use(params)
  const router = useRouter()
  const { user } = useAuth()
  const [isReadOnly, setIsReadOnly] = useState(false)
  const [loading, setLoading] = useState(true)
  const [hasAccess, setHasAccess] = useState(false)

  useEffect(() => {
    const checkAccess = async () => {
      try {
        // Always grant access to physical count
        setHasAccess(true)

        // Set read-only if physical count is already completed
        const stockReceiving = await stockReceivingService.getByWHStockRequestUID(id)
        if (stockReceiving && stockReceiving.PhysicalCountEndTime) {
          console.log("🔒 Physical count already completed - setting to read-only")
          setIsReadOnly(true)
        } else {
          console.log("✏️ Physical count not completed - allowing edit")
          setIsReadOnly(false)
        }
      } catch (error) {
        console.error("Error checking access:", error)
        setHasAccess(true) // Still allow access even if there's an error
      } finally {
        setLoading(false)
      }
    }

    checkAccess()
  }, [id, user, router])

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

  if (!hasAccess) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-[#A08B5C] mx-auto"></div>
          <p className="mt-4 text-gray-600">Redirecting...</p>
        </div>
      </div>
    )
  }

  return <PhysicalCountPage deliveryId={id} readOnly={isReadOnly} />
}
