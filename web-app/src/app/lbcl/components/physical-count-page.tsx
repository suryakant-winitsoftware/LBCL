"use client"

import { useState, useEffect, useRef } from "react"
import { Search, Clock, RefreshCw, UploadCloud, ImageOff, X } from "lucide-react"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select"
import { AlertDialog, AlertDialogContent } from "@/components/ui/alert-dialog"
import { Dialog, DialogContent, DialogTitle } from "@/components/ui/dialog"
import { useRouter } from "next/navigation"
import purchaseOrderService from "@/services/purchaseOrder"
import { stockReceivingService } from "@/services/stockReceivingService"
import { stockReceivingDetailService } from "@/services/stockReceivingDetailService"
import { useToast } from "@/hooks/use-toast"

export default function PhysicalCountPage({ deliveryId, readOnly = false }: { deliveryId: string; readOnly?: boolean }) {
  const router = useRouter()
  const { toast } = useToast()
  const fileInputRefs = useRef<{ [key: number]: HTMLInputElement | null }>({})
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
  const [uploadingImages, setUploadingImages] = useState<{ [key: number]: boolean }>({})
  const [imageUrls, setImageUrls] = useState<{ [key: number]: string[] }>({})

  useEffect(() => {
    fetchPurchaseOrder()
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
      setElapsedTime(prev => prev + 1)
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
      setLoading(true)
      setError("")

      const response = await purchaseOrderService.getPurchaseOrderMasterByUID(deliveryId)

      if (response.success && response.data) {
        const poData = response.data
        console.log("📦 Full Purchase Order Response:", poData)

        // API returns data with PurchaseOrderHeader and PurchaseOrderLines
        const header = poData.PurchaseOrderHeader || poData
        setPurchaseOrder(header)

        // Map purchase order lines to product data
        // API returns PurchaseOrderLines with PascalCase field names
        const lines = poData.PurchaseOrderLines || poData.purchaseOrderLines || poData.Lines || poData.lines || []

        console.log("📦 Purchase Order Header:", header)
        console.log("📦 Purchase Order Lines:", lines)
        if (lines.length > 0) {
          console.log("📦 First Line Keys:", Object.keys(lines[0]))
          console.log("📦 First Line Data:", lines[0])
        }

        const mapped = lines.map((line: any, index: number) => ({
          id: line.SKUUID || line.sku_uid || line.SKUID || line.skuId || line.SKUId || `sku-${index}`,
          name: line.SKUName || line.sku_name || line.skuName || line.Description || line.description || line.SKUCode || line.sku_code || 'Unknown Product',
          deliveryQty: line.FinalQty || line.RequestedQty || line.final_qty || line.requested_qty || line.Quantity || line.quantity || 0,
          shippedQty: line.FinalQty || line.RequestedQty || line.final_qty || line.requested_qty || line.ShippedQuantity || line.shippedQuantity || line.Quantity || line.quantity || 0,
          receivedQty: line.FinalQty || line.RequestedQty || line.final_qty || line.requested_qty || line.ShippedQuantity || line.shippedQuantity || line.Quantity || line.quantity || 0,
          adjustmentReason: "Not Applicable",
          adjustmentQty: 0,
          lineUID: line.UID || line.uid || `line-${index}`,
          imageUrl: null
        }))

        // If in readOnly mode, load saved stock receiving details
        if (readOnly) {
          try {
            const savedDetails = await stockReceivingDetailService.getByPurchaseOrderUID(deliveryId)
            console.log("📋 Loaded saved stock receiving details:", savedDetails)

            if (savedDetails && savedDetails.length > 0) {
              // Merge saved details with mapped data
              const mergedData = mapped.map((item: any) => {
                const savedDetail = savedDetails.find((detail: any) => detail.PurchaseOrderLineUID === item.lineUID)
                if (savedDetail) {
                  return {
                    ...item,
                    receivedQty: savedDetail.ReceivedQty,
                    adjustmentReason: savedDetail.AdjustmentReason || "Not Applicable",
                    adjustmentQty: savedDetail.AdjustmentQty || 0,
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
        setError("Failed to load purchase order")
      }
    } catch (error) {
      console.error("Error fetching purchase order:", error)
      setError("Failed to load purchase order")
    } finally {
      setLoading(false)
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
  const handleImageUpload = async (index: number, files: FileList) => {
    try {
      setUploadingImages(prev => ({ ...prev, [index]: true }))

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
        // Update image URLs for this product
        setImageUrls(prev => ({
          ...prev,
          [index]: [...(prev[index] || []), ...uploadedPaths]
        }))

        toast({
          title: "Success",
          description: `${uploadedPaths.length} image${uploadedPaths.length > 1 ? 's' : ''} uploaded successfully`
        })
      }
    } catch (error) {
      console.error("Error uploading images:", error)
      toast({
        title: "Error",
        description: "Failed to upload images",
        variant: "destructive"
      })
    } finally {
      setUploadingImages(prev => ({ ...prev, [index]: false }))
    }
  }

  // Remove single image from a product
  const handleRemoveImage = (productIndex: number, imageIndex: number) => {
    setImageUrls(prev => {
      const newUrls = { ...prev }
      if (newUrls[productIndex]) {
        newUrls[productIndex] = newUrls[productIndex].filter((_, i) => i !== imageIndex)
        if (newUrls[productIndex].length === 0) {
          delete newUrls[productIndex]
        }
      }
      return newUrls
    })
  }

  const handleSubmit = async () => {
    try {
      // Update stock receiving tracking with physical count end time
      const countEndTime = new Date().toISOString()

      const stockReceivingData = {
        PurchaseOrderUID: deliveryId,
        PhysicalCountStartTime: countStartTime,
        PhysicalCountEndTime: countEndTime,
        IsActive: true
      }

      await stockReceivingService.saveStockReceivingTracking(stockReceivingData)

      // Save stock receiving details (line-level data)
      const stockReceivingDetails = productData.map((product) => ({
        PurchaseOrderUID: deliveryId,
        PurchaseOrderLineUID: product.lineUID,
        SKUCode: product.id,
        SKUName: product.name,
        OrderedQty: product.deliveryQty,
        ReceivedQty: product.receivedQty,
        AdjustmentReason: product.adjustmentReason,
        AdjustmentQty: product.adjustmentQty,
        ImageURL: product.imageUrl || null, // Assuming you'll add image upload functionality
        IsActive: true
      }))

      console.log("💾 Saving stock receiving details:", stockReceivingDetails)
      await stockReceivingDetailService.saveStockReceivingDetails(stockReceivingDetails)

      setShowSuccess(true)
    } catch (error) {
      console.error("Error saving physical count:", error)
      alert("Failed to save physical count")
    }
  }

  if (loading) {
    return (
      <div className="min-h-screen bg-white flex items-center justify-center">
        <div className="text-center">
          <RefreshCw className="w-8 h-8 text-[#A08B5C] animate-spin mx-auto mb-2" />
          <p className="text-gray-600">Loading purchase order...</p>
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
      <header className="bg-white border-b border-gray-200 px-4 py-4 flex items-center justify-between sticky top-0 z-30">
        <h1 className="text-base sm:text-lg md:text-xl font-bold text-center flex-1 px-4">
          Physical Count & Perform Stock Receiving
        </h1>
        {!readOnly && (
          <div className="flex items-center gap-2 bg-[#A08B5C] text-white px-4 py-2 rounded-lg">
            <Clock className="w-5 h-5" />
            <span className="font-mono font-bold text-lg">{formatElapsedTime(elapsedTime)}</span>
          </div>
        )}
      </header>

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
              <th className="text-center p-3 font-semibold text-sm">Qty</th>
              <th className="text-center p-3 font-semibold text-sm">Image</th>
            </tr>
          </thead>
          <tbody>
            {filteredProductData.map((product, index) => (
              <tr key={product.lineUID || `product-${index}`} className="border-b border-gray-200 relative">
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
                <td className="text-center p-3">{product.deliveryQty}</td>
                <td className="text-center p-3">{product.shippedQty}</td>
                <td className="text-center p-3">
                  <Input
                    type="number"
                    value={product.receivedQty}
                    onChange={(e) => {
                      const newData = [...productData]
                      newData[index].receivedQty = Number.parseInt(e.target.value) || 0
                      setProductData(newData)
                    }}
                    className={`w-24 mx-auto text-center ${readOnly ? 'bg-gray-50 text-gray-900 font-medium cursor-default opacity-100' : ''}`}
                    disabled={readOnly}
                  />
                </td>
                <td className="text-center p-3">
                  <Select
                    value={product.adjustmentReason}
                    onValueChange={(value) => {
                      const newData = [...productData]
                      newData[index].adjustmentReason = value
                      setProductData(newData)
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
                    onChange={(e) => {
                      const newData = [...productData]
                      newData[index].adjustmentQty = Number.parseInt(e.target.value) || 0
                      setProductData(newData)
                    }}
                    className={`w-20 mx-auto text-center ${readOnly ? 'bg-gray-50 text-gray-900 font-medium cursor-default opacity-100' : ''}`}
                    disabled={readOnly}
                  />
                </td>
                <td className="text-center p-3">
                  <input
                    ref={el => fileInputRefs.current[index] = el}
                    type="file"
                    accept="image/*"
                    multiple
                    className="hidden"
                    onChange={(e) => {
                      const files = e.target.files
                      if (files && files.length > 0) {
                        handleImageUpload(index, files)
                      }
                    }}
                    disabled={readOnly}
                  />
                  <div className="flex items-center justify-center gap-1 flex-wrap">
                    {uploadingImages[index] ? (
                      <div className="h-10 w-10 rounded-md border-2 border-dashed border-gray-300 bg-gray-50 flex items-center justify-center">
                        <RefreshCw className="h-4 w-4 text-gray-400 animate-spin" />
                      </div>
                    ) : imageUrls[index] && imageUrls[index].length > 0 ? (
                      <>
                        {imageUrls[index].map((imageUrl, imgIndex) => (
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
                                  onClick={() => handleRemoveImage(index, imgIndex)}
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
                            onClick={() => fileInputRefs.current[index]?.click()}
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
                        onClick={() => fileInputRefs.current[index]?.click()}
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
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {/* Cards - Mobile/Tablet */}
      <div className="lg:hidden p-4 space-y-4">
        {filteredProductData.map((product, index) => (
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
                  onChange={(e) => {
                    const newData = [...productData]
                    newData[index].receivedQty = Number.parseInt(e.target.value) || 0
                    setProductData(newData)
                  }}
                  className={`h-8 text-center ${readOnly ? 'bg-gray-50 text-gray-900 font-medium cursor-default opacity-100' : ''}`}
                  disabled={readOnly}
                />
              </div>
            </div>

            <div className="space-y-2">
              <Select
                value={product.adjustmentReason}
                onValueChange={(value) => {
                  const newData = [...productData]
                  newData[index].adjustmentReason = value
                  setProductData(newData)
                }}
                disabled={readOnly}
              >
                <SelectTrigger className={readOnly ? 'bg-gray-50 text-gray-900 font-medium cursor-default opacity-100' : ''}>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="Not Applicable">Not Applicable</SelectItem>
                  <SelectItem value="Leakage">Leakage</SelectItem>
                  <SelectItem value="Damage">Damage</SelectItem>
                  <SelectItem value="Expiry">Expiry</SelectItem>
                </SelectContent>
              </Select>

              <div className="flex items-center gap-2">
                <Input
                  type="number"
                  value={product.adjustmentQty}
                  onChange={(e) => {
                    const newData = [...productData]
                    newData[index].adjustmentQty = Number.parseInt(e.target.value) || 0
                    setProductData(newData)
                  }}
                  placeholder="Qty"
                  className={`flex-1 ${readOnly ? 'bg-gray-50 text-gray-900 font-medium cursor-default opacity-100' : ''}`}
                  disabled={readOnly}
                />
                <div className="flex items-center gap-1 flex-wrap">
                  {uploadingImages[index] ? (
                    <div className="h-10 w-10 rounded-md border-2 border-dashed border-gray-300 bg-gray-50 flex items-center justify-center">
                      <RefreshCw className="h-4 w-4 text-gray-400 animate-spin" />
                    </div>
                  ) : imageUrls[index] && imageUrls[index].length > 0 ? (
                    <>
                      {imageUrls[index].map((imageUrl, imgIndex) => (
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
                                onClick={() => handleRemoveImage(index, imgIndex)}
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
                          onClick={() => fileInputRefs.current[index]?.click()}
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
                      onClick={() => fileInputRefs.current[index]?.click()}
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
        ))}
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
          <DialogTitle className="flex items-start justify-between mb-6">
            <h2 className="text-2xl font-bold">Success</h2>
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
                router.push("/lbcl/stock-receiving")
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
