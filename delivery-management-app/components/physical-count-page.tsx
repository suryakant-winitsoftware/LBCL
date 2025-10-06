"use client"

import { useState } from "react"
import { Search, Clock } from "lucide-react"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select"
import { AlertDialog, AlertDialogContent } from "@/components/ui/alert-dialog"
import { Dialog, DialogContent } from "@/components/ui/dialog"
import { useRouter } from "next/navigation"

const products = [
  { id: "5213", name: "Lion Large Beer can 625ml", deliveryQty: 240, shippedQty: 240 },
  { id: "5214", name: "Lion Lager Beer can 330ml", deliveryQty: 240, shippedQty: 240 },
  { id: "5216", name: "Lion Lager Beer bottle 625ml", deliveryQty: 240, shippedQty: 240 },
  { id: "5210", name: "Lion Lager Beer bottle 330ml", deliveryQty: 240, shippedQty: 240 },
]

export default function PhysicalCountPage({ deliveryId }: { deliveryId: string }) {
  const router = useRouter()
  const [showAlert, setShowAlert] = useState(false)
  const [showSuccess, setShowSuccess] = useState(false)
  const [productData, setProductData] = useState(
    products.map((p) => ({
      ...p,
      receivedQty: p.shippedQty,
      adjustmentReason: "Not Applicable",
      adjustmentQty: 0,
    })),
  )

  const handleSubmit = () => {
    setShowSuccess(true)
  }

  return (
    <div className="min-h-screen bg-white">
      {/* Header */}
      <header className="bg-white border-b border-gray-200 px-4 py-4 flex items-center justify-between sticky top-0 z-10">
        <h1 className="text-base sm:text-lg md:text-xl font-bold text-center flex-1 px-4">
          Physical Count & Perform Stock Receiving
        </h1>
        <div className="bg-[#F59E0B] text-white px-3 py-2 rounded-lg flex items-center gap-2 text-sm font-semibold whitespace-nowrap">
          <Clock className="w-4 h-4" />
          24:15 Min
        </div>
      </header>

      {/* Info Section */}
      <div className="bg-gray-50 p-4 border-b border-gray-200">
        <div className="grid grid-cols-2 sm:grid-cols-4 gap-4 mb-4">
          <div>
            <div className="text-xs text-gray-600 mb-1">Delivery Plan No</div>
            <div className="font-bold text-sm">{deliveryId}</div>
          </div>
          <div>
            <div className="text-xs text-gray-600 mb-1">SAP Delivery Note No</div>
            <div className="font-bold text-sm">DN4673899</div>
          </div>
          <div>
            <div className="text-xs text-gray-600 mb-1">Order Date</div>
            <div className="font-bold text-sm">20-MAY-2025</div>
          </div>
          <div>
            <div className="text-xs text-gray-600 mb-1">No of Unique Sku</div>
            <div className="font-bold text-sm">40</div>
          </div>
        </div>
        <Button className="w-full sm:w-auto bg-[#A08B5C] hover:bg-[#8A7549] text-white">Submit</Button>
      </div>

      {/* Search Section */}
      <div className="p-4 bg-gray-50 border-b border-gray-200">
        <div className="flex flex-col sm:flex-row gap-2">
          <div className="flex-1 relative">
            <Input placeholder="Search by Item Code/Description" className="pr-10" />
            <Search className="absolute right-3 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400" />
          </div>
          <Button variant="outline" className="border-[#A08B5C] text-[#A08B5C] bg-transparent">
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
          <thead className="bg-[#F5E6D3] sticky top-[73px] z-20">
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
            {productData.map((product, index) => (
              <tr key={product.id} className="border-b border-gray-200 relative">
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
                    className="w-24 mx-auto text-center"
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
                  >
                    <SelectTrigger className="w-40 mx-auto">
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
                    className="w-20 mx-auto text-center"
                  />
                </td>
                <td className="text-center p-3">
                  <button className="text-gray-400 hover:text-gray-600">
                    <span className="text-2xl">🖼️</span>
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {/* Cards - Mobile/Tablet */}
      <div className="lg:hidden p-4 space-y-4">
        {productData.map((product, index) => (
          <div key={product.id} className="bg-white border border-gray-200 rounded-lg p-4">
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
                  className="h-8 text-center"
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
              >
                <SelectTrigger>
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
                  className="flex-1"
                />
                <button className="text-gray-400 hover:text-gray-600 p-2">
                  <span className="text-2xl">🖼️</span>
                </button>
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
        <DialogContent className="max-w-2xl">
          <div className="flex items-start justify-between mb-6">
            <h2 className="text-2xl font-bold">Success</h2>
            <button onClick={() => setShowSuccess(false)} className="text-2xl">
              ×
            </button>
          </div>

          <div className="text-center space-y-4">
            <div className="w-20 h-20 bg-green-500 rounded-full flex items-center justify-center mx-auto">
              <svg className="w-12 h-12 text-white" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={3} d="M5 13l4 4L19 7" />
              </svg>
            </div>

            <h3 className="text-xl font-bold">Physical Count & Perform Stock Receiving Successfully Completed</h3>

            <p className="text-gray-700">
              Physical Count & Perform Stock Receiving Completed in <strong>24 Min</strong>
            </p>

            <p className="text-sm text-gray-600">
              Date: <strong>20-May-2025</strong> • Start Time: <strong>11: 05 AM</strong> • End Time:{" "}
              <strong>11: 29 AM</strong>
            </p>
          </div>

          <div className="flex gap-4 mt-6">
            <Button variant="outline" className="flex-1 border-[#A08B5C] text-[#A08B5C] bg-transparent">
              PRINT
            </Button>
            <Button
              className="flex-1 bg-[#A08B5C] hover:bg-[#8A7549] text-white"
              onClick={() => {
                setShowSuccess(false)
                router.push("/stock-receiving")
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
