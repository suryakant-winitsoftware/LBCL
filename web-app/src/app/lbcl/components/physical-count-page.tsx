"use client"

import React, { useState, useEffect, useRef } from "react"
import { Search, Clock, RefreshCw, UploadCloud, ImageOff, X, Plus, Trash2 } from "lucide-react"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select"
import { AlertDialog, AlertDialogContent } from "@/components/ui/alert-dialog"
import { Dialog, DialogContent, DialogTitle } from "@/components/ui/dialog"
import { useRouter } from "next/navigation"
import purchaseOrderService from "@/services/purchaseOrder"
import { inventoryService } from "@/services/inventory/inventory.service"
import { deliveryLoadingService } from "@/services/deliveryLoadingService"
import { stockReceivingService } from "@/services/stockReceivingService"
import { stockReceivingDetailService } from "@/services/stockReceivingDetailService"
import { useToast } from "@/hooks/use-toast"

type Adjustment = {
  id: string
  reason: string
  qty: number | ''
}

export default function PhysicalCountPage({ deliveryId, readOnly = false }: { deliveryId: string; readOnly?: boolean }) {
  const router = useRouter()
  const { toast } = useToast()
  const fileInputRefs = useRef<{ [key: string]: HTMLInputElement | null }>({})
  const isMountedRef = useRef(true)
  const [showAlert, setShowAlert] = useState(false)
  const [showSuccess, setShowSuccess] = useState(false)
  const [purchaseOrder, setPurchaseOrder] = useState<any>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState("")
  const [productData, setProductData] = useState<any[]>([])
  const [filteredProductData, setFilteredProductData] = useState<any[]>([])
  const [searchQuery, setSearchQuery] = useState("")
  const [countStartTime, setCountStartTime] = useState<string | null>(null)
  const [elapsedTime, setElapsedTime] = useState(0) // in seconds
  const [uploadingImages, setUploadingImages] = useState<{ [key: string]: boolean }>({})
  const [imageUrls, setImageUrls] = useState<{ [key: string]: string[] }>({})

  useEffect(() => {
    isMountedRef.current = true
    fetchPurchaseOrder()

    return () => {
      isMountedRef.current = false
    }
  }, [deliveryId])

  // Timer useEffect - only run in non-readOnly mode
  useEffect(() => {
    if (readOnly) return

    // Set start time when component mounts
    if (!countStartTime) {
      setCountStartTime(new Date().toISOString())
    }

    // Update timer every second
    const interval = setInterval(() => {
      if (isMountedRef.current) {
        setElapsedTime(prev => prev + 1)
      }
    }, 1000)

    return () => clearInterval(interval)
  }, [readOnly, countStartTime])

  // Format elapsed time as HH:MM:SS
  const formatElapsedTime = (seconds: number) => {
    const hours = Math.floor(seconds / 3600)
    const minutes = Math.floor((seconds % 3600) / 60)
    const secs = seconds % 60
    return `${hours.toString().padStart(2, '0')}:${minutes.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`
  }

  const fetchPurchaseOrder = async () => {
    try {
      if (!isMountedRef.current) return
      setLoading(true)
      setError("")

      // Fetch WH Stock Request data and Delivery Loading data in parallel
      const [whStockResponse, deliveryLoadingData] = await Promise.all([
        inventoryService.selectLoadRequestDataByUID(deliveryId),
        deliveryLoadingService.getByWHStockRequestUID(deliveryId)
      ])

      if (!isMountedRef.current) return // Check again after async operation

      console.log("📦 WH Stock Request Response:", whStockResponse)
      console.log("🚚 Delivery Loading Data:", deliveryLoadingData)

      if (whStockResponse && whStockResponse.WHStockRequest) {
        const header = whStockResponse.WHStockRequest
        const lines = whStockResponse.WHStockRequestLines || []

        console.log("📦 WH Stock Request Header:", header)
        console.log("📦 WH Stock Request Lines:", lines)
        if (lines.length > 0) {
          console.log("📦 First Line Keys:", Object.keys(lines[0]))
          console.log("📦 First Line Data:", lines[0])
        }

        // Transform header to match expected format
        const transformedHeader = {
          UID: header.UID,
          uid: header.UID,
          Code: header.Code || header.RequestCode,
          RequestCode: header.RequestCode || header.Code,
          OrderNumber: header.Code || header.RequestCode,
          DeliveryNoteNumber: deliveryLoadingData?.DeliveryNoteNumber || deliveryLoadingData?.deliveryNoteNumber || null,
          OrderDate: header.RequestedTime || header.CreatedTime,
          orderDate: header.RequestedTime || header.CreatedTime,
          DepartureTime: deliveryLoadingData?.DepartureTime || deliveryLoadingData?.departureTime || null,
          Status: header.Status,
          OrgName: header.TargetOrgName,
          WarehouseName: header.TargetWHName
        }

        setPurchaseOrder(transformedHeader)

        // Map WH Stock Request lines to product data
        const mapped = lines.map((line: any, index: number) => ({
          id: line.SKUUID || line.sku_uid || `sku-${index}`,
          name: line.SKUName || line.sku_name || line.SKUCode || line.sku_code || 'Unknown Product',
          deliveryQty: line.ApprovedQty || line.RequestedQty || line.approved_qty || line.requested_qty || 0,
          shippedQty: line.ApprovedQty || line.RequestedQty || line.approved_qty || line.requested_qty || 0,
          receivedQty: 0,  // Default to 0
          // First adjustment is in main row
          adjustmentReason: "Not Applicable",
          adjustmentQty: '',
          // Additional adjustments are in child rows
          adjustments: [] as Adjustment[],
          lineUID: line.UID || line.uid || `line-${index}`,
          imageUrl: null
        }))

        // If in readOnly mode, load saved stock receiving details
        if (readOnly) {
          try {
            const savedDetails = await stockReceivingDetailService.getByWHStockRequestUID(deliveryId)

            if (!isMountedRef.current) return // Check after async operation

            console.log("📋 Loaded saved stock receiving details:", savedDetails)

            if (savedDetails && savedDetails.length > 0) {
              // Merge saved details with mapped data
              const mergedData = mapped.map((item: any) => {
                const savedDetail = savedDetails.find((detail: any) => detail.WHStockRequestLineUID === item.lineUID)
                if (savedDetail) {
                  // For now, put the first adjustment in the main row
                  // In the future, we can parse multiple adjustments from the saved data
                  return {
                    ...item,
                    receivedQty: savedDetail.ReceivedQty,
                    adjustmentReason: savedDetail.AdjustmentReason || "Not Applicable",
                    adjustmentQty: savedDetail.AdjustmentQty || 0,
                    adjustments: [], // Additional adjustments (empty for now)
                    imageUrl: savedDetail.ImageURL
                  }
                }
                return item
              })
              setProductData(mergedData)
              setFilteredProductData(mergedData)
            } else {
              setProductData(mapped)
              setFilteredProductData(mapped)
            }
          } catch (error) {
            console.error("Error loading saved details:", error)
            setProductData(mapped)
            setFilteredProductData(mapped)
          }
        } else {
          setProductData(mapped)
          setFilteredProductData(mapped)
        }

        setCountStartTime(new Date().toISOString())
      } else {
        if (!isMountedRef.current) return
        setError("Failed to load purchase order")
      }
    } catch (error) {
      if (!isMountedRef.current) return
      console.error("Error fetching purchase order:", error)
      setError("Failed to load purchase order")
    } finally {
      if (isMountedRef.current) {
        setLoading(false)
      }
    }
  }

  const handleSearch = () => {
    if (!searchQuery.trim()) {
      setFilteredProductData(productData)
      return
    }

    const filtered = productData.filter(product => {
      const query = searchQuery.toLowerCase()
      return (
        product.id?.toLowerCase().includes(query) ||
        product.name?.toLowerCase().includes(query)
      )
    })

    setFilteredProductData(filtered)
  }

  useEffect(() => {
    handleSearch()
  }, [searchQuery, productData])

  const formatDate = (dateString: string) => {
    if (!dateString) return "N/A"
    const date = new Date(dateString)
    return date.toLocaleDateString('en-GB', {
      day: '2-digit',
      month: 'short',
      year: 'numeric'
    }).toUpperCase()
  }

  // Image upload handler using same approach as initiatives - supports multiple images
  // key can be product index (string) or composite key like "${index}-adj-${adjustmentId}"
  const handleImageUpload = async (key: string, files: FileList) => {
    try {
      if (!isMountedRef.current) return
      setUploadingImages(prev => ({ ...prev, [key]: true }))

      const authToken = localStorage.getItem("auth_token")
      let empUID: string | null = null
      try {
        const userInfoStr = localStorage.getItem("user_info")
        if (userInfoStr) {
          const userInfo = JSON.parse(userInfoStr)
          empUID = userInfo.uid || userInfo.id || userInfo.UID
        }
      } catch (e) {
        console.error("Failed to parse user_info:", e)
      }

      if (!empUID) {
        throw new Error("User not authenticated")
      }

      const uploadedPaths: string[] = []

      // Process each file
      for (let i = 0; i < files.length; i++) {
        const file = files[i]

        // Validate file
        const maxSize = 10 * 1024 * 1024 // 10MB
        if (file.size > maxSize) {
          toast({
            title: "Error",
            description: `${file.name}: File size exceeds 10MB limit`,
            variant: "destructive"
          })
          continue
        }

        if (!file.type.startsWith("image/")) {
          toast({
            title: "Error",
            description: `${file.name}: Not an image file`,
            variant: "destructive"
          })
          continue
        }

        // Step 1: Upload the physical file
        const formData = new FormData()
        formData.append("files", file, file.name)
        formData.append("folderPath", `stock-receiving/${deliveryId}/images`)

        const uploadResponse = await fetch(
          `${process.env.NEXT_PUBLIC_API_URL || "http://localhost:8000/api"}/FileUpload/UploadFile`,
          {
            method: "POST",
            headers: {
              Authorization: authToken ? `Bearer ${authToken}` : ""
            },
            body: formData
          }
        )

        if (!uploadResponse.ok) {
          toast({
            title: "Error",
            description: `${file.name}: Upload failed`,
            variant: "destructive"
          })
          continue
        }

        const uploadResult = await uploadResponse.json()
        if (uploadResult.Status !== 1) {
          toast({
            title: "Error",
            description: `${file.name}: ${uploadResult.Message || "Upload failed"}`,
            variant: "destructive"
          })
          continue
        }

        // Extract relative path
        let relativePath = ""
        if (uploadResult.SavedImgsPath && uploadResult.SavedImgsPath.length > 0) {
          relativePath = uploadResult.SavedImgsPath[0]
          if (relativePath.startsWith("Data/Data/")) {
            relativePath = relativePath.substring(5)
          }
        } else {
          relativePath = `Data/stock-receiving/${deliveryId}/images/${file.name}`
        }

        // Step 2: Create FileSys record
        const uniqueUID = 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {
          const r = (Math.random() * 16) | 0
          const v = c === 'x' ? r : (r & 0x3) | 0x8
          return v.toString(16)
        })

        const truncatedFileName = file.name.length > 50
          ? file.name.substring(0, 46) + file.name.slice(-4)
          : file.name

        const fileSysData = {
          UID: uniqueUID,
          SS: 1,
          CreatedBy: empUID,
          CreatedTime: new Date().toISOString(),
          ModifiedBy: empUID,
          ModifiedTime: new Date().toISOString(),
          LinkedItemType: "StockReceiving",
          LinkedItemUID: deliveryId,
          FileSysType: "PhysicalCountImage",
          FileType: file.type,
          FileName: truncatedFileName,
          DisplayName: truncatedFileName,
          FileSize: file.size,
          IsDefault: uploadedPaths.length === 0, // First image is default
          IsDirectory: false,
          RelativePath: relativePath,
          FileSysFileType: 1, // Image type
          CreatedByEmpUID: empUID
        }

        const fileSysResponse = await fetch(
          `${process.env.NEXT_PUBLIC_API_URL || "http://localhost:8000/api"}/FileSys/CUDFileSys`,
          {
            method: "POST",
            headers: {
              Authorization: authToken ? `Bearer ${authToken}` : "",
              "Content-Type": "application/json"
            },
            body: JSON.stringify(fileSysData)
          }
        )

        if (!fileSysResponse.ok) {
          toast({
            title: "Error",
            description: `${file.name}: Failed to save metadata`,
            variant: "destructive"
          })
          continue
        }

        const fileSysResult = await fileSysResponse.json()
        if (fileSysResult.IsSuccess === false) {
          toast({
            title: "Error",
            description: `${file.name}: ${fileSysResult.ErrorMessage || "Failed to save metadata"}`,
            variant: "destructive"
          })
          continue
        }

        uploadedPaths.push(relativePath)
      }

      if (uploadedPaths.length > 0) {
        if (!isMountedRef.current) return

        // Update image URLs for this product or adjustment
        setImageUrls(prev => ({
          ...prev,
          [key]: [...(prev[key] || []), ...uploadedPaths]
        }))

        toast({
          title: "Success",
          description: `${uploadedPaths.length} image${uploadedPaths.length > 1 ? 's' : ''} uploaded successfully`
        })
      }
    } catch (error) {
      if (!isMountedRef.current) return

      console.error("Error uploading images:", error)
      toast({
        title: "Error",
        description: "Failed to upload images",
        variant: "destructive"
      })
    } finally {
      if (isMountedRef.current) {
        setUploadingImages(prev => ({ ...prev, [key]: false }))
      }
    }
  }

  // Remove single image from a product or adjustment
  const handleRemoveImage = (key: string, imageIndex: number) => {
    setImageUrls(prev => {
      const newUrls = { ...prev }
      if (newUrls[key]) {
        newUrls[key] = newUrls[key].filter((_, i) => i !== imageIndex)
        if (newUrls[key].length === 0) {
          delete newUrls[key]
        }
      }
      return newUrls
    })
  }

  // Add new adjustment row for a product (adds complete new row)
  const handleAddAdjustment = (productLineUID: string) => {
    const newData = [...productData]
    const productIndex = newData.findIndex(p => p.lineUID === productLineUID)

    if (productIndex === -1) return

    const product = newData[productIndex]

    // Check if there's already a pending adjustment with "Not Applicable"
    const hasPendingAdjustment = product.adjustments.some((adj: Adjustment) => adj.reason === "Not Applicable")
    if (hasPendingAdjustment) return // Don't allow adding another until user selects a reason

    // Check if all adjustment reasons are already used
    const usedReasons = [product.adjustmentReason]
    product.adjustments.forEach((adj: Adjustment) => {
      if (adj.reason !== "Not Applicable") {
        usedReasons.push(adj.reason)
      }
    })

    // Count unique reasons (excluding "Not Applicable")
    const uniqueReasons = [...new Set(usedReasons.filter(r => r !== "Not Applicable"))]
    const availableReasons = ["Leakage", "Damage", "Expiry"].filter(r => !uniqueReasons.includes(r))

    // Button should be disabled in UI, but extra check here
    if (availableReasons.length === 0) return

    // Calculate main row adjustment qty
    const mainAdjustmentQty = typeof product.adjustmentQty === 'number' ? product.adjustmentQty : (product.adjustmentQty === '' ? 0 : Number.parseInt(product.adjustmentQty) || 0)

    // Calculate total child adjustments
    const totalChildAdjusted = product.adjustments.reduce((sum: number, adj: Adjustment) => {
      return sum + (typeof adj.qty === 'number' ? adj.qty : 0)
    }, 0)

    const receivedQty = typeof product.receivedQty === 'number' ? product.receivedQty : 0
    // Remaining = received qty - main adjustment - total child adjustments
    const remaining = receivedQty - mainAdjustmentQty - totalChildAdjusted

    // Button should be disabled in UI, but extra check here
    if (remaining <= 0) return

    const newAdjustment: Adjustment = {
      id: `adj-${Date.now()}-${product.adjustments.length}`,
      reason: "Not Applicable",
      qty: ''
    }

    newData[productIndex].adjustments = [...product.adjustments, newAdjustment]
    setProductData(newData)
    setFilteredProductData(newData.filter(product => {
      const query = searchQuery.toLowerCase()
      return (
        !searchQuery.trim() ||
        product.id?.toLowerCase().includes(query) ||
        product.name?.toLowerCase().includes(query)
      )
    }))
  }

  // Remove adjustment row
  const handleRemoveAdjustment = (productLineUID: string, adjustmentId: string) => {
    const newData = [...productData]
    const productIndex = newData.findIndex(p => p.lineUID === productLineUID)

    if (productIndex === -1) return

    newData[productIndex].adjustments = newData[productIndex].adjustments.filter(
      (adj: Adjustment) => adj.id !== adjustmentId
    )
    setProductData(newData)
    setFilteredProductData(newData.filter(product => {
      const query = searchQuery.toLowerCase()
      return (
        !searchQuery.trim() ||
        product.id?.toLowerCase().includes(query) ||
        product.name?.toLowerCase().includes(query)
      )
    }))
  }

  // Update adjustment reason
  const handleAdjustmentReasonChange = (productLineUID: string, adjustmentId: string, reason: string) => {
    const newData = [...productData]
    const productIndex = newData.findIndex(p => p.lineUID === productLineUID)

    if (productIndex === -1) return

    const adjustment = newData[productIndex].adjustments.find((adj: Adjustment) => adj.id === adjustmentId)
    if (adjustment) {
      adjustment.reason = reason
      setProductData(newData)
      setFilteredProductData(newData.filter(product => {
        const query = searchQuery.toLowerCase()
        return (
          !searchQuery.trim() ||
          product.id?.toLowerCase().includes(query) ||
          product.name?.toLowerCase().includes(query)
        )
      }))
    }
  }

  // Update adjustment quantity with validation
  const handleAdjustmentQtyChange = (productLineUID: string, adjustmentId: string, qty: number | '') => {
    const newData = [...productData]
    const productIndex = newData.findIndex(p => p.lineUID === productLineUID)

    if (productIndex === -1) return

    const product = newData[productIndex]
    const adjustment = product.adjustments.find((adj: Adjustment) => adj.id === adjustmentId)

    if (!adjustment) return

    // Calculate main row adjustment qty
    const mainAdjustmentQty = typeof product.adjustmentQty === 'number' ? product.adjustmentQty : (product.adjustmentQty === '' ? 0 : Number.parseInt(product.adjustmentQty) || 0)

    // Calculate total of other child adjustments (excluding the current one being edited)
    const otherChildAdjustmentsTotal = product.adjustments
      .filter((adj: Adjustment) => adj.id !== adjustmentId)
      .reduce((sum: number, adj: Adjustment) => {
        return sum + (typeof adj.qty === 'number' ? adj.qty : 0)
      }, 0)

    const receivedQty = typeof product.receivedQty === 'number' ? product.receivedQty : 0
    // Max allowed = received qty - main adjustment - other child adjustments
    const maxAllowed = receivedQty - mainAdjustmentQty - otherChildAdjustmentsTotal

    // Validate new quantity
    if (qty !== '' && qty > maxAllowed) {
      toast({
        title: "Invalid Quantity",
        description: `Maximum allowed: ${maxAllowed} (Received: ${receivedQty}, Main adjustment: ${mainAdjustmentQty}, Other adjustments: ${otherChildAdjustmentsTotal})`,
        variant: "destructive",
      })
      return
    }

    adjustment.qty = qty
    setProductData(newData)
    setFilteredProductData(newData.filter(product => {
      const query = searchQuery.toLowerCase()
      return (
        !searchQuery.trim() ||
        product.id?.toLowerCase().includes(query) ||
        product.name?.toLowerCase().includes(query)
      )
    }))
  }

  const handleSubmit = async () => {
    try {
      if (!isMountedRef.current) return

      // Update stock receiving tracking with physical count end time
      const countEndTime = new Date().toISOString()

      console.log("💾 Saving physical count for deliveryId:", deliveryId)
      console.log("⏱️ Count Start Time:", countStartTime)
      console.log("⏱️ Count End Time:", countEndTime)

      const stockReceivingData = {
        WHStockRequestUID: deliveryId,
        PhysicalCountStartTime: countStartTime,
        PhysicalCountEndTime: countEndTime,
        IsActive: true
      }

      console.log("📦 Stock Receiving Data:", stockReceivingData)
      await stockReceivingService.saveStockReceivingTracking(stockReceivingData)

      if (!isMountedRef.current) return
      console.log("✅ Stock receiving tracking saved")

      // Save stock receiving details (line-level data)
      // For now, we aggregate all adjustments into a summary format
      // Future enhancement: Create separate API endpoint for saving individual adjustment records
      const stockReceivingDetails = productData.map((product, productIndex) => {
        // Include main row adjustment qty
        const mainAdjustmentQty = typeof product.adjustmentQty === 'number' ? product.adjustmentQty : (product.adjustmentQty === '' ? 0 : Number.parseInt(product.adjustmentQty) || 0)

        // Include additional child adjustments
        const additionalAdjustmentQty = product.adjustments.reduce((sum: number, adj: Adjustment) => {
          return sum + (typeof adj.qty === 'number' ? adj.qty : 0)
        }, 0)

        const totalAdjustmentQty = mainAdjustmentQty + additionalAdjustmentQty

        // Create a summary of adjustment reasons
        const adjustmentsList = []

        // Add main row adjustment if it exists
        if (product.adjustmentReason !== "Not Applicable" && mainAdjustmentQty > 0) {
          adjustmentsList.push(`${product.adjustmentReason}(${mainAdjustmentQty})`)
        }

        // Add additional adjustments
        product.adjustments
          .filter((adj: Adjustment) => adj.reason !== "Not Applicable" && (typeof adj.qty === 'number' && adj.qty > 0))
          .forEach((adj: Adjustment) => {
            adjustmentsList.push(`${adj.reason}(${adj.qty})`)
          })

        const adjustmentReasons = adjustmentsList.join(', ') || "Not Applicable"

        // Collect all images from main product and all adjustments
        const allImages: string[] = []

        // Add main product images
        const mainProductKey = `product-${productIndex}`
        if (imageUrls[mainProductKey]) {
          allImages.push(...imageUrls[mainProductKey])
        }

        // Add images from all adjustment rows
        product.adjustments.forEach((adj: Adjustment) => {
          const adjKey = `product-${productIndex}-adj-${adj.id}`
          if (imageUrls[adjKey]) {
            allImages.push(...imageUrls[adjKey])
          }
        })

        return {
          WHStockRequestUID: deliveryId,
          WHStockRequestLineUID: product.lineUID,
          SKUCode: product.id,
          SKUName: product.name,
          OrderedQty: product.deliveryQty,
          ReceivedQty: product.receivedQty === '' ? 0 : product.receivedQty,
          AdjustmentReason: adjustmentReasons,
          AdjustmentQty: totalAdjustmentQty,
          ImageURL: allImages.length > 0 ? allImages.join(',') : null,
          IsActive: true
        }
      })

      console.log("💾 Saving stock receiving details:", stockReceivingDetails)

      try {
        await stockReceivingDetailService.saveStockReceivingDetails(stockReceivingDetails)
        if (!isMountedRef.current) return
        console.log("✅ Stock receiving details saved")
      } catch (detailError) {
        if (!isMountedRef.current) return
        console.warn("⚠️ Error saving stock receiving details (non-critical):", detailError)
        // Continue to show success even if details fail
      }

      // Always show success if we got this far
      if (isMountedRef.current) {
        setShowSuccess(true)
      }
    } catch (error) {
      if (!isMountedRef.current) return
      console.error("❌ Error saving physical count:", error)
      toast({
        title: "Error",
        description: "Failed to save physical count. Please try again.",
        variant: "destructive",
      })
    }
  }

  if (loading) {
    return (
      <div className="min-h-screen bg-white">
        {/* Skeleton Info Section */}
        <div className="bg-gray-50 p-4 border-b border-gray-200">
          <div className="grid grid-cols-2 sm:grid-cols-4 gap-4 mb-4">
            {[1, 2, 3, 4].map((i) => (
              <div key={i}>
                <div className="h-3 bg-gray-200 rounded w-24 mb-2 animate-pulse"></div>
                <div className="h-4 bg-gray-300 rounded w-32 animate-pulse"></div>
              </div>
            ))}
          </div>
          <div className="h-10 bg-gray-300 rounded w-32 animate-pulse"></div>
        </div>

        {/* Skeleton Search Section */}
        <div className="p-4 bg-gray-50 border-b border-gray-200">
          <div className="flex gap-2">
            <div className="flex-1 h-10 bg-gray-200 rounded animate-pulse"></div>
            <div className="h-10 w-20 bg-gray-200 rounded animate-pulse"></div>
            <div className="h-10 w-32 bg-gray-200 rounded animate-pulse"></div>
          </div>
        </div>

        {/* Skeleton Table - Desktop */}
        <div className="hidden lg:block overflow-x-auto">
          <table className="w-full">
            <thead className="bg-[#F5E6D3]">
              <tr>
                {[1, 2, 3, 4, 5, 6, 7, 8].map((i) => (
                  <th key={i} className="p-3">
                    <div className="h-4 bg-gray-300 rounded animate-pulse"></div>
                  </th>
                ))}
              </tr>
            </thead>
            <tbody>
              {[1, 2, 3, 4, 5].map((row) => (
                <tr key={row} className="border-b border-gray-200">
                  <td className="p-3">
                    <div className="flex items-center gap-3">
                      <div className="w-12 h-12 bg-gray-200 rounded animate-pulse"></div>
                      <div className="flex-1">
                        <div className="h-4 bg-gray-300 rounded w-48 mb-2 animate-pulse"></div>
                        <div className="h-3 bg-gray-200 rounded w-32 animate-pulse"></div>
                      </div>
                    </div>
                  </td>
                  {[1, 2, 3, 4, 5, 6, 7].map((col) => (
                    <td key={col} className="p-3 text-center">
                      <div className="h-4 bg-gray-200 rounded w-16 mx-auto animate-pulse"></div>
                    </td>
                  ))}
                </tr>
              ))}
            </tbody>
          </table>
        </div>

        {/* Skeleton Cards - Mobile */}
        <div className="lg:hidden p-4 space-y-4">
          {[1, 2, 3].map((card) => (
            <div key={card} className="bg-white border border-gray-200 rounded-lg p-4">
              <div className="flex items-start gap-3 mb-4">
                <div className="w-16 h-16 bg-gray-200 rounded animate-pulse"></div>
                <div className="flex-1">
                  <div className="h-4 bg-gray-300 rounded w-full mb-2 animate-pulse"></div>
                  <div className="h-3 bg-gray-200 rounded w-24 animate-pulse"></div>
                </div>
              </div>
              <div className="space-y-2">
                <div className="h-3 bg-gray-200 rounded w-32 animate-pulse"></div>
                <div className="h-8 bg-gray-200 rounded animate-pulse"></div>
                <div className="h-3 bg-gray-200 rounded w-32 animate-pulse"></div>
                <div className="h-8 bg-gray-200 rounded animate-pulse"></div>
              </div>
            </div>
          ))}
        </div>
      </div>
    )
  }

  if (error || !purchaseOrder) {
    return (
      <div className="min-h-screen bg-white flex items-center justify-center">
        <div className="bg-red-50 border border-red-200 rounded-lg p-4 text-center max-w-md">
          <p className="text-red-700">{error || "Purchase order not found"}</p>
          <button
            onClick={fetchPurchaseOrder}
            className="mt-2 text-sm text-red-600 hover:text-red-800 underline"
          >
            Try again
          </button>
        </div>
      </div>
    )
  }

  return (
    <div className="min-h-screen bg-white">
      {/* Header with Timer */}
      {/* <header className="bg-white border-b border-gray-200 px-4 py-4 flex items-center justify-between sticky top-0 z-30">
        <h1 className="text-base sm:text-lg md:text-xl font-bold text-center flex-1 px-4">
          Physical Count & Perform Stock Receiving
        </h1>
        {!readOnly && (
          <div className="flex items-center gap-2 bg-[#A08B5C] text-white px-4 py-2 rounded-lg">
            <Clock className="w-5 h-5" />
            <span className="font-mono font-bold text-lg">{formatElapsedTime(elapsedTime)}</span>
          </div>
        )}
      </header> */}

      {/* Info Section */}
      <div className="bg-gray-50 p-4 border-b border-gray-200">
        <div className="grid grid-cols-2 sm:grid-cols-4 gap-4 mb-4">
          <div>
            <div className="text-xs text-gray-600 mb-1">Delivery Plan No</div>
            <div className="font-bold text-sm">{purchaseOrder.OrderNumber || purchaseOrder.order_number || purchaseOrder.orderNumber || deliveryId}</div>
          </div>
          <div>
            <div className="text-xs text-gray-600 mb-1">SAP Delivery Note No</div>
            <div className="font-bold text-sm">{purchaseOrder.DeliveryNoteNumber || purchaseOrder.delivery_note_number || purchaseOrder.deliveryNoteNumber || 'N/A'}</div>
          </div>
          <div>
            <div className="text-xs text-gray-600 mb-1">Order Date</div>
            <div className="font-bold text-sm">{formatDate(purchaseOrder.OrderDate || purchaseOrder.order_date || purchaseOrder.orderDate || '')}</div>
          </div>
          <div>
            <div className="text-xs text-gray-600 mb-1">No of Unique Sku</div>
            <div className="font-bold text-sm">{productData.length}</div>
          </div>
        </div>
        {!readOnly && (
          <Button
            onClick={handleSubmit}
            className="w-full sm:w-auto bg-[#A08B5C] hover:bg-[#8A7549] text-white"
          >
            Submit
          </Button>
        )}
      </div>

      {/* Search Section */}
      <div className="p-4 bg-gray-50 border-b border-gray-200">
        <div className="flex flex-col sm:flex-row gap-2">
          <div className="flex-1 relative">
            <Input
              placeholder="Search by Item Code/Description"
              className="pr-10"
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              onKeyPress={(e) => e.key === 'Enter' && handleSearch()}
            />
            <Search className="absolute right-3 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400" />
          </div>
          <Button
            variant="outline"
            className="border-[#A08B5C] text-[#A08B5C] bg-transparent"
            onClick={handleSearch}
          >
            Search
          </Button>
          <Button variant="outline" className="border-[#A08B5C] text-[#A08B5C] bg-transparent">
            Add Products
          </Button>
        </div>
      </div>

      {/* Table - Desktop */}
      <div className="hidden lg:block overflow-x-auto">
        <table className="w-full">
          <thead className="bg-[#F5E6D3] sticky top-0 z-20">
            <tr>
              <th className="text-left p-3 font-semibold text-sm">SKU Name / Code / Description</th>
              <th className="text-center p-3 font-semibold text-sm">Delivery Plan Qty</th>
              <th className="text-center p-3 font-semibold text-sm">Shipped Qty</th>
              <th className="text-center p-3 font-semibold text-sm">Received Qty</th>
              <th className="text-center p-3 font-semibold text-sm">Adjustment Reason</th>
              <th className="text-center p-3 font-semibold text-sm">Adjustment Qty</th>
              <th className="text-center p-3 font-semibold text-sm">Image</th>
              <th className="text-center p-3 font-semibold text-sm">Action</th>
            </tr>
          </thead>
          <tbody>
            {filteredProductData.map((product, index) => (
              <React.Fragment key={product.lineUID || `product-${index}`}>
                {/* Main Product Row */}
                <tr className="border-b border-gray-200 bg-white">
                  <td className="p-3">
                    <div className="flex items-center gap-3">
                      <div className="w-12 h-12 bg-gray-200 rounded flex items-center justify-center">
                        <span className="text-2xl">🍺</span>
                      </div>
                      <div>
                        <div className="font-semibold">{product.name}</div>
                        <div className="text-sm text-gray-600">{product.id}</div>
                      </div>
                    </div>
                  </td>
                  <td className="text-center p-3 font-medium">{product.deliveryQty}</td>
                  <td className="text-center p-3 font-medium">{product.shippedQty}</td>
                  <td className="text-center p-3">
                    <Input
                      type="number"
                      value={product.receivedQty}
                      onFocus={(e) => e.target.select()}
                      onKeyDown={(e) => {
                        const allowedKeys = ['Backspace', 'Delete', 'Tab', 'ArrowLeft', 'ArrowRight', 'ArrowUp', 'ArrowDown', 'Home', 'End']
                        if (e.ctrlKey || e.metaKey) return
                        if (allowedKeys.includes(e.key)) return
                        if (/[a-zA-Z]/.test(e.key) || (e.key === '-' || e.key === '+' || e.key === 'e' || e.key === 'E')) {
                          e.preventDefault()
                        }
                      }}
                      onChange={(e) => {
                        const newData = [...productData]
                        const receivedQty = e.target.value === '' ? '' : Number.parseInt(e.target.value)
                        if (receivedQty !== '' && receivedQty < 0) return
                        newData[index].receivedQty = receivedQty
                        setProductData(newData)
                      }}
                      className={`w-24 mx-auto text-center ${readOnly ? 'bg-gray-50 text-gray-900 font-medium cursor-default opacity-100' : ''}`}
                      disabled={readOnly}
                      min="0"
                      inputMode="numeric"
                    />
                  </td>
                  <td className="text-center p-3">
                    <Select
                      value={product.adjustmentReason}
                      onValueChange={(value) => {
                        const newData = [...productData]
                        const productIndex = newData.findIndex(p => p.lineUID === product.lineUID)
                        if (productIndex !== -1) {
                          newData[productIndex].adjustmentReason = value
                          setProductData(newData)
                          setFilteredProductData(newData.filter(product => {
                            const query = searchQuery.toLowerCase()
                            return (
                              !searchQuery.trim() ||
                              product.id?.toLowerCase().includes(query) ||
                              product.name?.toLowerCase().includes(query)
                            )
                          }))
                        }
                      }}
                      disabled={readOnly}
                    >
                      <SelectTrigger className={`w-40 mx-auto ${readOnly ? 'bg-gray-50 text-gray-900 font-medium cursor-default opacity-100' : ''}`}>
                        <SelectValue />
                      </SelectTrigger>
                      <SelectContent>
                        <SelectItem value="Not Applicable">Not Applicable</SelectItem>
                        <SelectItem value="Leakage">Leakage</SelectItem>
                        <SelectItem value="Damage">Damage</SelectItem>
                        <SelectItem value="Expiry">Expiry</SelectItem>
                      </SelectContent>
                    </Select>
                  </td>
                  <td className="text-center p-3">
                    <Input
                      type="number"
                      value={product.adjustmentQty}
                      onFocus={(e) => e.target.select()}
                      onKeyDown={(e) => {
                        const allowedKeys = ['Backspace', 'Delete', 'Tab', 'ArrowLeft', 'ArrowRight', 'ArrowUp', 'ArrowDown', 'Home', 'End']
                        if (e.ctrlKey || e.metaKey) return
                        if (allowedKeys.includes(e.key)) return
                        if (/[a-zA-Z]/.test(e.key) || (e.key === '-' || e.key === '+' || e.key === 'e' || e.key === 'E')) {
                          e.preventDefault()
                        }
                      }}
                      onChange={(e) => {
                        const newData = [...productData]
                        const productIndex = newData.findIndex(p => p.lineUID === product.lineUID)
                        if (productIndex !== -1) {
                          const adjustmentQty = e.target.value === '' ? '' : Number.parseInt(e.target.value)

                          // Prevent negative values
                          if (adjustmentQty !== '' && adjustmentQty < 0) return

                          // Calculate total from additional adjustments
                          const additionalTotal = product.adjustments.reduce((sum: number, adj: Adjustment) => {
                            return sum + (typeof adj.qty === 'number' ? adj.qty : 0)
                          }, 0)

                          const receivedQty = typeof product.receivedQty === 'number' ? product.receivedQty : 0
                          const totalAdjustments = (typeof adjustmentQty === 'number' ? adjustmentQty : 0) + additionalTotal

                          // Validate total adjustments don't exceed received qty
                          if (totalAdjustments > receivedQty) {
                            toast({
                              title: "Invalid Quantity",
                              description: `Total adjustments cannot exceed received quantity`,
                              variant: "destructive",
                            })
                            return
                          }

                          newData[productIndex].adjustmentQty = adjustmentQty
                          setProductData(newData)
                          setFilteredProductData(newData.filter(product => {
                            const query = searchQuery.toLowerCase()
                            return (
                              !searchQuery.trim() ||
                              product.id?.toLowerCase().includes(query) ||
                              product.name?.toLowerCase().includes(query)
                            )
                          }))
                        }
                      }}
                      className={`w-24 mx-auto text-center ${readOnly ? 'bg-gray-50 text-gray-900 font-medium cursor-default opacity-100' : ''}`}
                      disabled={readOnly}
                      min="0"
                      inputMode="numeric"
                      placeholder="Qty"
                    />
                  </td>
                  <td className="text-center p-3">
                    <input
                      ref={el => fileInputRefs.current[`product-${index}`] = el}
                      type="file"
                      accept="image/*"
                      multiple
                      className="hidden"
                      onChange={(e) => {
                        const files = e.target.files
                        if (files && files.length > 0) {
                          handleImageUpload(`product-${index}`, files)
                        }
                      }}
                      disabled={readOnly}
                    />
                    <div className="flex items-center justify-center gap-1 flex-wrap">
                      {uploadingImages[`product-${index}`] ? (
                        <div className="h-10 w-10 rounded-md border-2 border-dashed border-gray-300 bg-gray-50 flex items-center justify-center">
                          <RefreshCw className="h-4 w-4 text-gray-400 animate-spin" />
                        </div>
                      ) : imageUrls[`product-${index}`] && imageUrls[`product-${index}`].length > 0 ? (
                        <>
                          {imageUrls[`product-${index}`].map((imageUrl, imgIndex) => (
                            <div key={imgIndex} className="relative group h-10 w-10">
                              <img
                                src={`${process.env.NEXT_PUBLIC_API_URL || "http://localhost:8000"}/${imageUrl}`}
                                alt={`Product ${imgIndex + 1}`}
                                className="h-10 w-10 rounded-md object-cover"
                                onError={(e) => {
                                  e.currentTarget.style.display = 'none'
                                  const errorDiv = e.currentTarget.nextElementSibling as HTMLElement
                                  if (errorDiv) errorDiv.classList.remove('hidden')
                                }}
                              />
                              <div className="hidden h-10 w-10 rounded-md border-2 border-dashed border-red-300 bg-red-50 flex-col items-center justify-center">
                                <ImageOff className="h-3 w-3 text-red-400" />
                                <span className="text-[8px] text-red-500 font-medium">Failed</span>
                              </div>
                              {!readOnly && (
                                <div className="absolute inset-0 bg-black/60 opacity-0 group-hover:opacity-100 transition-opacity rounded-md flex items-center justify-center">
                                  <button
                                    onClick={() => handleRemoveImage(`product-${index}`, imgIndex)}
                                    className="h-6 w-6 p-0 bg-white/90 hover:bg-white rounded flex items-center justify-center"
                                    title="Remove Image"
                                  >
                                    <X className="h-3 w-3 text-red-600" />
                                  </button>
                                </div>
                              )}
                            </div>
                          ))}
                          {!readOnly && (
                            <button
                              onClick={() => fileInputRefs.current[`product-${index}`]?.click()}
                              className="h-10 w-10 rounded-md border-2 border-dashed border-gray-300 bg-gray-50 hover:bg-gray-100 hover:border-[#A08B5C]/50 transition-all flex flex-col items-center justify-center cursor-pointer group"
                              title="Add More Images"
                            >
                              <UploadCloud className="h-3 w-3 text-gray-400 group-hover:text-[#A08B5C]" />
                              <span className="text-[8px] text-gray-500 group-hover:text-[#A08B5C] font-medium">
                                Add
                              </span>
                            </button>
                          )}
                        </>
                      ) : (
                        <button
                          onClick={() => fileInputRefs.current[`product-${index}`]?.click()}
                          disabled={readOnly}
                          className="h-10 w-10 rounded-md border-2 border-dashed border-gray-300 bg-gray-50 hover:bg-gray-100 hover:border-[#A08B5C]/50 transition-all flex flex-col items-center justify-center cursor-pointer group disabled:opacity-50 disabled:cursor-not-allowed"
                          title="Upload Product Images"
                        >
                          <UploadCloud className="h-3 w-3 text-gray-400 group-hover:text-[#A08B5C]" />
                          <span className="text-[8px] text-gray-500 group-hover:text-[#A08B5C] font-medium">
                            Upload
                          </span>
                        </button>
                      )}
                    </div>
                  </td>
                  <td className="text-center p-3">
                    {!readOnly && (() => {
                      // Check if there's a pending adjustment with "Not Applicable"
                      const hasPendingAdjustment = product.adjustments.some((adj: Adjustment) => adj.reason === "Not Applicable")

                      // Check if all adjustment reasons are already used
                      const usedReasons = [product.adjustmentReason]
                      product.adjustments.forEach((adj: Adjustment) => {
                        if (adj.reason !== "Not Applicable") {
                          usedReasons.push(adj.reason)
                        }
                      })

                      const uniqueReasons = [...new Set(usedReasons.filter(r => r !== "Not Applicable"))]
                      const availableReasons = ["Leakage", "Damage", "Expiry"].filter(r => !uniqueReasons.includes(r))

                      // Calculate remaining quantity
                      const mainAdjustmentQty = typeof product.adjustmentQty === 'number' ? product.adjustmentQty : (product.adjustmentQty === '' ? 0 : Number.parseInt(product.adjustmentQty) || 0)
                      const totalChildAdjusted = product.adjustments.reduce((sum: number, adj: Adjustment) => {
                        return sum + (typeof adj.qty === 'number' ? adj.qty : 0)
                      }, 0)
                      const receivedQty = typeof product.receivedQty === 'number' ? product.receivedQty : 0
                      const remaining = receivedQty - mainAdjustmentQty - totalChildAdjusted

                      const cannotAddReason = hasPendingAdjustment
                        ? "Select reason first"
                        : availableReasons.length === 0
                        ? "All reasons used"
                        : remaining <= 0
                        ? "No quantity remaining"
                        : null

                      const isDisabled = cannotAddReason !== null

                      return (
                        <div className="flex flex-col items-center gap-1">
                          <button
                            onClick={() => handleAddAdjustment(product.lineUID)}
                            disabled={isDisabled}
                            className={`px-3 py-1.5 text-sm rounded transition-colors flex items-center gap-1 ${
                              isDisabled
                                ? 'bg-gray-300 text-gray-500 cursor-not-allowed'
                                : 'bg-[#A08B5C] hover:bg-[#8A7549] text-white'
                            }`}
                            title={cannotAddReason || "Add another adjustment"}
                          >
                            <Plus className="w-4 h-4" />
                            Add
                          </button>
                          {cannotAddReason && (
                            <span className="text-[10px] text-gray-500 italic">{cannotAddReason}</span>
                          )}
                        </div>
                      )
                    })()}
                  </td>
                </tr>

                {/* Adjustment Rows (Child Rows) */}
                {product.adjustments.map((adjustment: Adjustment, adjIndex: number) => {
                  // Get all already-used adjustment reasons (from main row and other child rows)
                  const usedReasons: string[] = []

                  // Add main row reason if it's not "Not Applicable"
                  if (product.adjustmentReason !== "Not Applicable") {
                    usedReasons.push(product.adjustmentReason)
                  }

                  // Add reasons from other child adjustments (not the current one)
                  product.adjustments.forEach((adj: Adjustment) => {
                    if (adj.id !== adjustment.id && adj.reason !== "Not Applicable") {
                      usedReasons.push(adj.reason)
                    }
                  })

                  const adjKey = `product-${index}-adj-${adjustment.id}`
                  const adjustmentNumber = adjIndex + 2 // +2 because main row is "Adjustment 1"

                  return (
                    <tr key={adjustment.id} className="border-b border-gray-100 bg-amber-50/30">
                      <td className="p-3 pl-16">
                        <div className="flex items-center gap-2 text-gray-700">
                          <div className="w-1 h-10 bg-[#A08B5C] rounded"></div>
                          <div>
                            <div className="font-semibold text-sm">Adjustment Reason {adjustmentNumber}</div>
                            <div className="text-xs text-gray-600">{product.name}</div>
                            <div className="text-xs text-gray-500">{product.id}</div>
                          </div>
                        </div>
                      </td>
                      <td className="text-center p-3 font-medium text-gray-600">{product.deliveryQty}</td>
                      <td className="text-center p-3 font-medium text-gray-600">{product.shippedQty}</td>
                      <td className="text-center p-3 font-medium text-gray-600">
                        {typeof product.receivedQty === 'number' ? product.receivedQty : (product.receivedQty || 0)}
                      </td>
                      <td className="text-center p-3">
                        <Select
                          value={adjustment.reason}
                          onValueChange={(value) => handleAdjustmentReasonChange(product.lineUID, adjustment.id, value)}
                          disabled={readOnly}
                        >
                          <SelectTrigger className={`w-40 mx-auto ${readOnly ? 'bg-gray-50 text-gray-900 font-medium cursor-default opacity-100' : ''}`}>
                            <SelectValue />
                          </SelectTrigger>
                          <SelectContent>
                            <SelectItem value="Not Applicable">Not Applicable</SelectItem>
                            {!usedReasons.includes("Leakage") && <SelectItem value="Leakage">Leakage</SelectItem>}
                            {!usedReasons.includes("Damage") && <SelectItem value="Damage">Damage</SelectItem>}
                            {!usedReasons.includes("Expiry") && <SelectItem value="Expiry">Expiry</SelectItem>}
                          </SelectContent>
                        </Select>
                      </td>
                      <td className="text-center p-3">
                        <Input
                          type="number"
                          value={adjustment.qty}
                          onFocus={(e) => e.target.select()}
                          onKeyDown={(e) => {
                            const allowedKeys = ['Backspace', 'Delete', 'Tab', 'ArrowLeft', 'ArrowRight', 'ArrowUp', 'ArrowDown', 'Home', 'End']
                            if (e.ctrlKey || e.metaKey) return
                            if (allowedKeys.includes(e.key)) return
                            if (/[a-zA-Z]/.test(e.key) || (e.key === '-' || e.key === '+' || e.key === 'e' || e.key === 'E')) {
                              e.preventDefault()
                            }
                          }}
                          onChange={(e) => {
                            const qty = e.target.value === '' ? '' : Number.parseInt(e.target.value)
                            if (qty !== '' && qty < 0) return
                            handleAdjustmentQtyChange(product.lineUID, adjustment.id, qty)
                          }}
                          className={`w-24 mx-auto text-center ${readOnly ? 'bg-gray-50 text-gray-900 font-medium cursor-default opacity-100' : ''}`}
                          disabled={readOnly}
                          min="0"
                          inputMode="numeric"
                          placeholder="Qty"
                        />
                      </td>
                      <td className="text-center p-3">
                        <input
                          ref={el => fileInputRefs.current[adjKey] = el}
                          type="file"
                          accept="image/*"
                          multiple
                          className="hidden"
                          onChange={(e) => {
                            const files = e.target.files
                            if (files && files.length > 0) {
                              handleImageUpload(adjKey, files)
                            }
                          }}
                          disabled={readOnly}
                        />
                        <div className="flex items-center justify-center gap-1 flex-wrap">
                          {uploadingImages[adjKey] ? (
                            <div className="h-10 w-10 rounded-md border-2 border-dashed border-gray-300 bg-gray-50 flex items-center justify-center">
                              <RefreshCw className="h-4 w-4 text-gray-400 animate-spin" />
                            </div>
                          ) : imageUrls[adjKey] && imageUrls[adjKey].length > 0 ? (
                            <>
                              {imageUrls[adjKey].map((imageUrl, imgIndex) => (
                                <div key={imgIndex} className="relative group h-10 w-10">
                                  <img
                                    src={`${process.env.NEXT_PUBLIC_API_URL || "http://localhost:8000"}/${imageUrl}`}
                                    alt={`Adjustment ${imgIndex + 1}`}
                                    className="h-10 w-10 rounded-md object-cover"
                                    onError={(e) => {
                                      e.currentTarget.style.display = 'none'
                                      const errorDiv = e.currentTarget.nextElementSibling as HTMLElement
                                      if (errorDiv) errorDiv.classList.remove('hidden')
                                    }}
                                  />
                                  <div className="hidden h-10 w-10 rounded-md border-2 border-dashed border-red-300 bg-red-50 flex-col items-center justify-center">
                                    <ImageOff className="h-3 w-3 text-red-400" />
                                    <span className="text-[8px] text-red-500 font-medium">Failed</span>
                                  </div>
                                  {!readOnly && (
                                    <div className="absolute inset-0 bg-black/60 opacity-0 group-hover:opacity-100 transition-opacity rounded-md flex items-center justify-center">
                                      <button
                                        onClick={() => handleRemoveImage(adjKey, imgIndex)}
                                        className="h-6 w-6 p-0 bg-white/90 hover:bg-white rounded flex items-center justify-center"
                                        title="Remove Image"
                                      >
                                        <X className="h-3 w-3 text-red-600" />
                                      </button>
                                    </div>
                                  )}
                                </div>
                              ))}
                              {!readOnly && (
                                <button
                                  onClick={() => fileInputRefs.current[adjKey]?.click()}
                                  className="h-10 w-10 rounded-md border-2 border-dashed border-gray-300 bg-gray-50 hover:bg-gray-100 hover:border-[#A08B5C]/50 transition-all flex flex-col items-center justify-center cursor-pointer group"
                                  title="Add More Images"
                                >
                                  <UploadCloud className="h-3 w-3 text-gray-400 group-hover:text-[#A08B5C]" />
                                  <span className="text-[8px] text-gray-500 group-hover:text-[#A08B5C] font-medium">
                                    Add
                                  </span>
                                </button>
                              )}
                            </>
                          ) : (
                            <button
                              onClick={() => fileInputRefs.current[adjKey]?.click()}
                              disabled={readOnly}
                              className="h-10 w-10 rounded-md border-2 border-dashed border-gray-300 bg-gray-50 hover:bg-gray-100 hover:border-[#A08B5C]/50 transition-all flex flex-col items-center justify-center cursor-pointer group disabled:opacity-50 disabled:cursor-not-allowed"
                              title="Upload Adjustment Images"
                            >
                              <UploadCloud className="h-3 w-3 text-gray-400 group-hover:text-[#A08B5C]" />
                              <span className="text-[8px] text-gray-500 group-hover:text-[#A08B5C] font-medium">
                                Upload
                              </span>
                            </button>
                          )}
                        </div>
                      </td>
                      <td className="text-center p-3">
                        {!readOnly && (
                          <button
                            onClick={() => handleRemoveAdjustment(product.lineUID, adjustment.id)}
                            className="p-2 hover:bg-red-50 rounded transition-colors mx-auto"
                            title="Remove adjustment"
                          >
                            <Trash2 className="w-4 h-4 text-red-500" />
                          </button>
                        )}
                      </td>
                    </tr>
                  )
                })}
              </React.Fragment>
            ))}
          </tbody>
        </table>
      </div>

      {/* Cards - Mobile/Tablet */}
      <div className="lg:hidden p-4 space-y-4">
        {filteredProductData.map((product, index) => {
          const productKey = `product-${index}`

          return (
            <div key={product.lineUID || `product-card-${index}`} className="bg-white border border-gray-200 rounded-lg p-4">
              <div className="flex items-start gap-3 mb-4">
                <div className="w-16 h-16 bg-gray-200 rounded flex items-center justify-center flex-shrink-0">
                  <span className="text-3xl">🍺</span>
                </div>
                <div className="flex-1">
                  <div className="font-semibold">{product.name}</div>
                  <div className="text-sm text-gray-600">{product.id}</div>
                </div>
              </div>

              <div className="grid grid-cols-3 gap-2 mb-3 text-sm">
                <div>
                  <div className="text-gray-600 mb-1">Delivery</div>
                  <div className="font-semibold">{product.deliveryQty}</div>
                </div>
                <div>
                  <div className="text-gray-600 mb-1">Shipped</div>
                  <div className="font-semibold">{product.shippedQty}</div>
                </div>
                <div>
                  <div className="text-gray-600 mb-1">Received</div>
                  <Input
                    type="number"
                    value={product.receivedQty}
                    onFocus={(e) => e.target.select()}
                    onKeyDown={(e) => {
                      // Allow: Backspace, Delete, Tab, Arrow keys, Home, End
                      const allowedKeys = ['Backspace', 'Delete', 'Tab', 'ArrowLeft', 'ArrowRight', 'ArrowUp', 'ArrowDown', 'Home', 'End']

                      // Allow: Ctrl+A, Ctrl+C, Ctrl+V, Ctrl+X (for copy/paste/select all)
                      if (e.ctrlKey || e.metaKey) {
                        return
                      }

                      // Allow navigation and editing keys
                      if (allowedKeys.includes(e.key)) {
                        return
                      }

                      // Block alphabetic characters and special characters except numbers
                      if (
                        /[a-zA-Z]/.test(e.key) || // Block all letters
                        (e.key === '-' || e.key === '+' || e.key === 'e' || e.key === 'E') // Block minus, plus, e/E
                      ) {
                        e.preventDefault()
                      }
                    }}
                    onChange={(e) => {
                      const newData = [...productData]
                      const receivedQty = e.target.value === '' ? '' : Number.parseInt(e.target.value)

                      // Prevent negative values
                      if (receivedQty !== '' && receivedQty < 0) {
                        return
                      }

                      newData[index].receivedQty = receivedQty
                      setProductData(newData)
                    }}
                    className={`h-8 text-center ${readOnly ? 'bg-gray-50 text-gray-900 font-medium cursor-default opacity-100' : ''}`}
                    disabled={readOnly}
                    min="0"
                    inputMode="numeric"
                  />
                </div>
              </div>

              <div className="space-y-2">
                <div className="text-sm font-semibold text-gray-700 mb-2">Adjustments</div>
                {product.adjustments.length === 0 && !readOnly && (
                  <div className="text-center text-gray-400 text-sm py-2 border border-dashed border-gray-300 rounded">
                    No adjustments
                  </div>
                )}
                {product.adjustments.map((adjustment: Adjustment, adjIndex: number) => {
                  // Get all already-used adjustment reasons (from main row and other child rows)
                  const usedReasons: string[] = []

                  // Add main row reason if it's not "Not Applicable"
                  if (product.adjustmentReason !== "Not Applicable") {
                    usedReasons.push(product.adjustmentReason)
                  }

                  // Add reasons from other child adjustments (not the current one)
                  product.adjustments.forEach((adj: Adjustment) => {
                    if (adj.id !== adjustment.id && adj.reason !== "Not Applicable") {
                      usedReasons.push(adj.reason)
                    }
                  })

                  const adjKey = `product-${index}-adj-${adjustment.id}`
                  const adjustmentNumber = adjIndex + 2 // +2 because main row is "Adjustment 1"

                  return (
                    <div key={adjustment.id} className="space-y-2 p-3 bg-gray-50 rounded border border-gray-200">
                      <div className="pb-2 border-b border-gray-300">
                        <div className="font-semibold text-sm text-gray-800">Adjustment Reason {adjustmentNumber}</div>
                        <div className="text-xs text-gray-600 mt-0.5">{product.name}</div>
                        <div className="text-xs text-gray-500">{product.id}</div>
                      </div>
                      <Select
                        value={adjustment.reason}
                        onValueChange={(value) => handleAdjustmentReasonChange(product.lineUID, adjustment.id, value)}
                        disabled={readOnly}
                      >
                        <SelectTrigger className={readOnly ? 'bg-white text-gray-900 font-medium cursor-default opacity-100' : 'bg-white'}>
                          <SelectValue />
                        </SelectTrigger>
                        <SelectContent>
                          <SelectItem value="Not Applicable">Not Applicable</SelectItem>
                          {!usedReasons.includes("Leakage") && <SelectItem value="Leakage">Leakage</SelectItem>}
                          {!usedReasons.includes("Damage") && <SelectItem value="Damage">Damage</SelectItem>}
                          {!usedReasons.includes("Expiry") && <SelectItem value="Expiry">Expiry</SelectItem>}
                        </SelectContent>
                      </Select>
                      <div className="flex items-center gap-2">
                        <Input
                          type="number"
                          value={adjustment.qty}
                          onFocus={(e) => e.target.select()}
                          onKeyDown={(e) => {
                            const allowedKeys = ['Backspace', 'Delete', 'Tab', 'ArrowLeft', 'ArrowRight', 'ArrowUp', 'ArrowDown', 'Home', 'End']
                            if (e.ctrlKey || e.metaKey) return
                            if (allowedKeys.includes(e.key)) return
                            if (/[a-zA-Z]/.test(e.key) || (e.key === '-' || e.key === '+' || e.key === 'e' || e.key === 'E')) {
                              e.preventDefault()
                            }
                          }}
                          onChange={(e) => {
                            const qty = e.target.value === '' ? '' : Number.parseInt(e.target.value)
                            if (qty !== '' && qty < 0) return
                            handleAdjustmentQtyChange(product.lineUID, adjustment.id, qty)
                          }}
                          placeholder="Quantity"
                          className={`flex-1 ${readOnly ? 'bg-white text-gray-900 font-medium cursor-default opacity-100' : 'bg-white'}`}
                          disabled={readOnly}
                          min="0"
                          inputMode="numeric"
                        />
                        {!readOnly && (
                          <button
                            onClick={() => handleRemoveAdjustment(product.lineUID, adjustment.id)}
                            className="p-2 hover:bg-red-50 rounded transition-colors"
                            title="Remove adjustment"
                          >
                            <Trash2 className="w-5 h-5 text-red-500" />
                          </button>
                        )}
                      </div>

                      {/* Adjustment Images */}
                      <div className="pt-2 border-t border-gray-300">
                        <div className="text-xs font-medium text-gray-600 mb-1">Adjustment Images</div>
                        <input
                          ref={el => fileInputRefs.current[adjKey] = el}
                          type="file"
                          accept="image/*"
                          multiple
                          className="hidden"
                          onChange={(e) => {
                            const files = e.target.files
                            if (files && files.length > 0) {
                              handleImageUpload(adjKey, files)
                            }
                          }}
                          disabled={readOnly}
                        />
                        <div className="flex items-center gap-1 flex-wrap">
                          {uploadingImages[adjKey] ? (
                            <div className="h-10 w-10 rounded-md border-2 border-dashed border-gray-300 bg-white flex items-center justify-center">
                              <RefreshCw className="h-4 w-4 text-gray-400 animate-spin" />
                            </div>
                          ) : imageUrls[adjKey] && imageUrls[adjKey].length > 0 ? (
                            <>
                              {imageUrls[adjKey].map((imageUrl, imgIndex) => (
                                <div key={imgIndex} className="relative group h-10 w-10">
                                  <img
                                    src={`${process.env.NEXT_PUBLIC_API_URL || "http://localhost:8000"}/${imageUrl}`}
                                    alt={`Adjustment ${imgIndex + 1}`}
                                    className="h-10 w-10 rounded-md object-cover"
                                    onError={(e) => {
                                      e.currentTarget.style.display = 'none'
                                      const errorDiv = e.currentTarget.nextElementSibling as HTMLElement
                                      if (errorDiv) errorDiv.classList.remove('hidden')
                                    }}
                                  />
                                  <div className="hidden h-10 w-10 rounded-md border-2 border-dashed border-red-300 bg-red-50 flex-col items-center justify-center">
                                    <ImageOff className="h-3 w-3 text-red-400" />
                                    <span className="text-[8px] text-red-500 font-medium">Failed</span>
                                  </div>
                                  {!readOnly && (
                                    <div className="absolute inset-0 bg-black/60 opacity-0 group-hover:opacity-100 transition-opacity rounded-md flex items-center justify-center">
                                      <button
                                        onClick={() => handleRemoveImage(adjKey, imgIndex)}
                                        className="h-6 w-6 p-0 bg-white/90 hover:bg-white rounded flex items-center justify-center"
                                        title="Remove Image"
                                      >
                                        <X className="h-3 w-3 text-red-600" />
                                      </button>
                                    </div>
                                  )}
                                </div>
                              ))}
                              {!readOnly && (
                                <button
                                  onClick={() => fileInputRefs.current[adjKey]?.click()}
                                  className="h-10 w-10 rounded-md border-2 border-dashed border-gray-300 bg-white hover:bg-gray-100 hover:border-[#A08B5C]/50 transition-all flex flex-col items-center justify-center cursor-pointer group"
                                  title="Add More Images"
                                >
                                  <UploadCloud className="h-3 w-3 text-gray-400 group-hover:text-[#A08B5C]" />
                                  <span className="text-[8px] text-gray-500 group-hover:text-[#A08B5C] font-medium">
                                    Add
                                  </span>
                                </button>
                              )}
                            </>
                          ) : (
                            <button
                              onClick={() => fileInputRefs.current[adjKey]?.click()}
                              disabled={readOnly}
                              className="h-10 w-10 rounded-md border-2 border-dashed border-gray-300 bg-white hover:bg-gray-100 hover:border-[#A08B5C]/50 transition-all flex flex-col items-center justify-center cursor-pointer group disabled:opacity-50 disabled:cursor-not-allowed"
                              title="Upload Adjustment Images"
                            >
                              <UploadCloud className="h-3 w-3 text-gray-400 group-hover:text-[#A08B5C]" />
                              <span className="text-[8px] text-gray-500 group-hover:text-[#A08B5C] font-medium">
                                Upload
                              </span>
                            </button>
                          )}
                        </div>
                      </div>
                    </div>
                  )
                })}
                {!readOnly && (() => {
                  // Check if there's a pending adjustment with "Not Applicable"
                  const hasPendingAdjustment = product.adjustments.some((adj: Adjustment) => adj.reason === "Not Applicable")

                  // Check if all adjustment reasons are already used
                  const usedReasons = [product.adjustmentReason]
                  product.adjustments.forEach((adj: Adjustment) => {
                    if (adj.reason !== "Not Applicable") {
                      usedReasons.push(adj.reason)
                    }
                  })

                  const uniqueReasons = [...new Set(usedReasons.filter(r => r !== "Not Applicable"))]
                  const availableReasons = ["Leakage", "Damage", "Expiry"].filter(r => !uniqueReasons.includes(r))

                  // Calculate remaining quantity
                  const mainAdjustmentQty = typeof product.adjustmentQty === 'number' ? product.adjustmentQty : (product.adjustmentQty === '' ? 0 : Number.parseInt(product.adjustmentQty) || 0)
                  const totalChildAdjusted = product.adjustments.reduce((sum: number, adj: Adjustment) => {
                    return sum + (typeof adj.qty === 'number' ? adj.qty : 0)
                  }, 0)
                  const receivedQty = typeof product.receivedQty === 'number' ? product.receivedQty : 0
                  const remaining = receivedQty - mainAdjustmentQty - totalChildAdjusted

                  const cannotAddReason = hasPendingAdjustment
                    ? "Please select a reason for the existing adjustment before adding another"
                    : availableReasons.length === 0
                    ? "All adjustment reasons (Leakage, Damage, Expiry) are already in use"
                    : remaining <= 0
                    ? "No quantity remaining to adjust"
                    : null

                  const isDisabled = cannotAddReason !== null

                  return (
                    <div className="space-y-1">
                      <button
                        onClick={() => handleAddAdjustment(product.lineUID)}
                        disabled={isDisabled}
                        className={`w-full flex items-center justify-center gap-1 px-3 py-2 text-sm rounded transition-colors ${
                          isDisabled
                            ? 'bg-gray-300 text-gray-500 cursor-not-allowed'
                            : 'bg-[#A08B5C] hover:bg-[#8A7549] text-white'
                        }`}
                        title={cannotAddReason || "Add another adjustment"}
                      >
                        <Plus className="w-4 h-4" />
                        Add Adjustment
                      </button>
                      {cannotAddReason && (
                        <p className="text-xs text-gray-500 italic text-center">{cannotAddReason}</p>
                      )}
                    </div>
                  )
                })()}

                <div className="space-y-1 pt-2 border-t border-gray-200 mt-3">
                  <div className="text-sm font-semibold text-gray-700 mb-2">Images</div>
                  <input
                    ref={el => fileInputRefs.current[productKey] = el}
                    type="file"
                    accept="image/*"
                    multiple
                    className="hidden"
                    onChange={(e) => {
                      const files = e.target.files
                      if (files && files.length > 0) {
                        handleImageUpload(productKey, files)
                      }
                    }}
                    disabled={readOnly}
                  />
                  <div className="flex items-center gap-1 flex-wrap">
                    {uploadingImages[productKey] ? (
                      <div className="h-10 w-10 rounded-md border-2 border-dashed border-gray-300 bg-gray-50 flex items-center justify-center">
                        <RefreshCw className="h-4 w-4 text-gray-400 animate-spin" />
                      </div>
                    ) : imageUrls[productKey] && imageUrls[productKey].length > 0 ? (
                      <>
                        {imageUrls[productKey].map((imageUrl, imgIndex) => (
                          <div key={imgIndex} className="relative group h-10 w-10">
                            <img
                              src={`${process.env.NEXT_PUBLIC_API_URL || "http://localhost:8000"}/${imageUrl}`}
                              alt={`Product ${imgIndex + 1}`}
                              className="h-10 w-10 rounded-md object-cover"
                              onError={(e) => {
                                e.currentTarget.style.display = 'none'
                                const errorDiv = e.currentTarget.nextElementSibling as HTMLElement
                                if (errorDiv) errorDiv.classList.remove('hidden')
                              }}
                            />
                            <div className="hidden h-10 w-10 rounded-md border-2 border-dashed border-red-300 bg-red-50 flex-col items-center justify-center">
                              <ImageOff className="h-3 w-3 text-red-400" />
                              <span className="text-[8px] text-red-500 font-medium">Failed</span>
                            </div>
                            {!readOnly && (
                              <div className="absolute inset-0 bg-black/60 opacity-0 group-hover:opacity-100 transition-opacity rounded-md flex items-center justify-center">
                                <button
                                  onClick={() => handleRemoveImage(productKey, imgIndex)}
                                  className="h-6 w-6 p-0 bg-white/90 hover:bg-white rounded flex items-center justify-center"
                                  title="Remove Image"
                                >
                                  <X className="h-3 w-3 text-red-600" />
                                </button>
                              </div>
                            )}
                          </div>
                        ))}
                        {!readOnly && (
                          <button
                            onClick={() => fileInputRefs.current[productKey]?.click()}
                            className="h-10 w-10 rounded-md border-2 border-dashed border-gray-300 bg-gray-50 hover:bg-gray-100 hover:border-[#A08B5C]/50 transition-all flex flex-col items-center justify-center cursor-pointer group"
                            title="Add More Images"
                          >
                            <UploadCloud className="h-3 w-3 text-gray-400 group-hover:text-[#A08B5C]" />
                            <span className="text-[8px] text-gray-500 group-hover:text-[#A08B5C] font-medium">
                              Add
                            </span>
                          </button>
                        )}
                      </>
                    ) : (
                      <button
                        onClick={() => fileInputRefs.current[productKey]?.click()}
                        disabled={readOnly}
                        className="h-10 w-10 rounded-md border-2 border-dashed border-gray-300 bg-gray-50 hover:bg-gray-100 hover:border-[#A08B5C]/50 transition-all flex flex-col items-center justify-center cursor-pointer group disabled:opacity-50 disabled:cursor-not-allowed"
                        title="Upload Product Images"
                      >
                        <UploadCloud className="h-3 w-3 text-gray-400 group-hover:text-[#A08B5C]" />
                        <span className="text-[8px] text-gray-500 group-hover:text-[#A08B5C] font-medium">
                          Upload
                        </span>
                      </button>
                    )}
                  </div>
                </div>
              </div>
            </div>
          )
        })}
      </div>

      {/* Alert Dialog */}
      <AlertDialog open={showAlert} onOpenChange={setShowAlert}>
        <AlertDialogContent className="max-w-2xl">
          <div className="flex items-start justify-between mb-4">
            <h2 className="text-2xl font-bold">Alert</h2>
            <button onClick={() => setShowAlert(false)} className="text-2xl">
              ×
            </button>
          </div>
          <div className="space-y-4">
            <p className="font-semibold text-lg">Note: Delivery Plan Qty, Shipped Qty & Received Qty must match.</p>
            <p className="text-gray-700">
              System will treat full Shipped Qty as received. Damaged/missing items will be logged separately — please
              move them to the "Damaged Stock" location. Unseen damages may exist if stock isn't unpacked during
              unloading
            </p>
          </div>
        </AlertDialogContent>
      </AlertDialog>

      {/* Success Dialog */}
      <Dialog open={showSuccess} onOpenChange={setShowSuccess}>
        <DialogContent className="max-w-md">
          <DialogTitle className="text-2xl font-bold flex items-start justify-between mb-6">
            <span>Success</span>
            <button onClick={() => setShowSuccess(false)} className="text-2xl">
              ×
            </button>
          </DialogTitle>

          <div className="text-center space-y-4">
            <div className="w-20 h-20 bg-green-500 rounded-full flex items-center justify-center mx-auto">
              <svg className="w-12 h-12 text-white" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={3} d="M5 13l4 4L19 7" />
              </svg>
            </div>

            <h3 className="text-xl font-bold">Physical count completed successfully</h3>
          </div>

          <div className="mt-6">
            <Button
              className="w-full bg-[#A08B5C] hover:bg-[#8A7549] text-white"
              onClick={() => {
                setShowSuccess(false)
                router.push(`/lbcl/stock-receiving/${deliveryId}/activity-log`)
              }}
            >
              DONE
            </Button>
          </div>
        </DialogContent>
      </Dialog>
    </div>
  )
}
