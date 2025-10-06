"use client"

import { useState } from "react"
import { useRouter } from "next/navigation"
import { Search, Check, X } from "lucide-react"
import { NavigationMenu } from "@/components/navigation-menu"
import { Dialog, DialogContent } from "@/components/ui/dialog"

type Product = {
  id: string
  code: string
  name: string
  image: string
  plannedQty: number
  approvedPC: number
  collectedPC: number
  varPc: number
}

export function RDTruckLoadCollection() {
  const router = useRouter()
  const [searchItem, setSearchItem] = useState("")
  const [showSignatureDialog, setShowSignatureDialog] = useState(false)
  const [showSuccessDialog, setShowSuccessDialog] = useState(false)
  const [salesmenSignature, setSalesmenSignature] = useState("Praveen Kumar")
  const [driverSignature, setDriverSignature] = useState("R.M.K.P. Rathnayake")
  const [notes, setNotes] = useState("")

  const products: Product[] = [
    {
      id: "1",
      code: "5213",
      name: "Lion Large Beer can 625ml",
      image: "/amber-beer-bottle.png",
      plannedQty: 100,
      approvedPC: 99,
      collectedPC: 99,
      varPc: 0.0,
    },
    {
      id: "2",
      code: "5214",
      name: "Lion Large Beer can 330ml",
      image: "/amber-beer-bottle.png",
      plannedQty: 100,
      approvedPC: 99,
      collectedPC: 99,
      varPc: 0.0,
    },
    {
      id: "3",
      code: "5216",
      name: "Lion Large Beer bottle 625ml",
      image: "/amber-beer-bottle.png",
      plannedQty: 100,
      approvedPC: 99,
      collectedPC: 99,
      varPc: 0.0,
    },
    {
      id: "4",
      code: "5210",
      name: "Lion Large Beer bottle 330ml",
      image: "/amber-beer-bottle.png",
      plannedQty: 100,
      approvedPC: 99,
      collectedPC: 99,
      varPc: 0.0,
    },
  ]

  const handleConfirm = () => {
    setShowSignatureDialog(true)
  }

  const handleSignatureSubmit = () => {
    setShowSignatureDialog(false)
    setShowSuccessDialog(true)
  }

  const handleSuccessDone = () => {
    setShowSuccessDialog(false)
    router.push("/truck-loading/activity-log")
  }

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <header className="bg-white border-b border-gray-200 px-4 py-4 sticky top-0 z-10">
        <div className="flex items-center justify-between">
          <NavigationMenu />
          <h1 className="text-lg sm:text-xl font-bold text-gray-900 flex-1 text-center">Load Request Collection</h1>
          <div className="w-10" />
        </div>
      </header>

      {/* Search Section */}
      <div className="bg-white border-b border-gray-200 px-4 py-4">
        <div className="max-w-7xl mx-auto flex items-center gap-4">
          <div className="flex-1 relative">
            <input
              type="text"
              placeholder="Search by Item Code/Description"
              value={searchItem}
              onChange={(e) => setSearchItem(e.target.value)}
              className="w-full pl-4 pr-10 py-3 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-[#A08B5C]"
            />
            <Search className="absolute right-3 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400" />
          </div>
          <button
            onClick={handleConfirm}
            className="px-8 py-3 bg-[#A08B5C] text-white font-medium rounded-lg hover:bg-[#8F7A4D] transition-colors"
          >
            Confirm
          </button>
        </div>
      </div>

      {/* Products Table */}
      <div className="p-4 max-w-7xl mx-auto">
        <div className="bg-white rounded-lg shadow-sm border border-gray-200 overflow-x-auto">
          <table className="w-full">
            <thead className="bg-[#FFF8E7]">
              <tr>
                <th className="px-4 py-3 text-left text-xs font-medium text-gray-700 min-w-[250px]">
                  Item Code/Description
                </th>
                <th className="px-4 py-3 text-center text-xs font-medium text-gray-700 min-w-[100px]">Planned Qty</th>
                <th className="px-4 py-3 text-center text-xs font-medium text-gray-700 min-w-[120px]">Approved PC</th>
                <th className="px-4 py-3 text-center text-xs font-medium text-gray-700 min-w-[120px]">Collected PC</th>
                <th className="px-4 py-3 text-center text-xs font-medium text-gray-700 min-w-[100px]">Var Pc</th>
                <th className="px-4 py-3 text-center text-xs font-medium text-gray-700 min-w-[80px]"></th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-200">
              {products.map((product) => (
                <tr key={product.id} className="hover:bg-gray-50">
                  <td className="px-4 py-4">
                    <div className="flex items-center gap-3">
                      <img
                        src={product.image || "/placeholder.svg"}
                        alt={product.name}
                        className="w-10 h-10 object-contain"
                      />
                      <div>
                        <div className="font-medium text-gray-900">{product.name}</div>
                        <div className="text-sm text-gray-500 flex items-center gap-2">
                          {product.code}
                          <span className="inline-flex items-center justify-center w-5 h-5 bg-[#D4A853] rounded-full">
                            <svg className="w-3 h-3 text-white" viewBox="0 0 20 20" fill="currentColor">
                              <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z" />
                            </svg>
                          </span>
                        </div>
                      </div>
                    </div>
                  </td>
                  <td className="px-4 py-4 text-center">
                    <div className="font-medium text-gray-900">{product.plannedQty}</div>
                  </td>
                  <td className="px-4 py-4 text-center">
                    <div className="font-medium text-gray-900">{product.approvedPC}</div>
                  </td>
                  <td className="px-4 py-4">
                    <div className="flex justify-center">
                      <input
                        type="number"
                        value={product.collectedPC}
                        className="w-24 px-3 py-2 border-b-2 border-gray-900 text-center focus:outline-none focus:border-[#A08B5C] bg-transparent"
                      />
                    </div>
                  </td>
                  <td className="px-4 py-4 text-center">
                    <div className="font-medium text-gray-900">{product.varPc.toFixed(2)}</div>
                  </td>
                  <td className="px-4 py-4 text-center">
                    <Check className="w-6 h-6 text-[#A08B5C] mx-auto" />
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>

      {/* Signature Dialog */}
      <Dialog open={showSignatureDialog} onOpenChange={setShowSignatureDialog}>
        <DialogContent className="max-w-2xl">
          <div className="p-6">
            <div className="flex items-center justify-between mb-6">
              <h2 className="text-2xl font-bold text-gray-900">Signature</h2>
              <button onClick={() => setShowSignatureDialog(false)} className="p-2 hover:bg-gray-100 rounded-lg">
                <X className="w-5 h-5" />
              </button>
            </div>

            <div className="space-y-6">
              {/* Signatures */}
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <div className="flex items-center justify-between mb-2">
                    <label className="text-sm font-medium text-gray-700">Salesmen Signature</label>
                    <button className="text-sm text-[#A08B5C] hover:underline">Clear</button>
                  </div>
                  <div className="border-2 border-gray-300 rounded-lg h-40 bg-white flex items-end justify-center p-4">
                    <div className="bg-[#FFF8E7] px-4 py-2 rounded w-full text-center">
                      <div className="text-sm text-gray-900 font-medium">{salesmenSignature}</div>
                    </div>
                  </div>
                </div>

                <div>
                  <div className="flex items-center justify-between mb-2">
                    <label className="text-sm font-medium text-gray-700">Driver Signature</label>
                    <button className="text-sm text-[#A08B5C] hover:underline">Clear</button>
                  </div>
                  <div className="border-2 border-gray-300 rounded-lg h-40 bg-white flex items-end justify-center p-4">
                    <div className="bg-[#FFF8E7] px-4 py-2 rounded w-full text-center">
                      <div className="text-sm text-gray-900 font-medium">{driverSignature}</div>
                    </div>
                  </div>
                </div>
              </div>

              {/* Notes */}
              <div>
                <div className="flex items-center justify-between mb-2">
                  <label className="text-sm font-medium text-gray-700">Notes</label>
                  <button className="text-sm text-[#A08B5C] hover:underline">Clear</button>
                </div>
                <textarea
                  value={notes}
                  onChange={(e) => setNotes(e.target.value)}
                  placeholder="Enter Here"
                  className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-[#A08B5C] resize-none"
                  rows={4}
                />
              </div>
            </div>

            {/* Done Button */}
            <button
              onClick={handleSignatureSubmit}
              className="w-full mt-6 py-4 bg-[#A08B5C] text-white font-medium rounded-lg hover:bg-[#8F7A4D] transition-colors"
            >
              DONE
            </button>
          </div>
        </DialogContent>
      </Dialog>

      {/* Success Dialog */}
      <Dialog open={showSuccessDialog} onOpenChange={setShowSuccessDialog}>
        <DialogContent className="max-w-xl">
          <div className="p-6 text-center">
            <button
              onClick={() => setShowSuccessDialog(false)}
              className="absolute top-4 right-4 p-2 hover:bg-gray-100 rounded-lg"
            >
              <X className="w-5 h-5" />
            </button>

            <h2 className="text-2xl font-bold text-gray-900 mb-6">Success</h2>

            <div className="flex justify-center mb-6">
              <div className="w-24 h-24 bg-green-500 rounded-full flex items-center justify-center">
                <Check className="w-12 h-12 text-white" strokeWidth={3} />
              </div>
            </div>

            <h3 className="text-xl font-bold text-gray-900 mb-2">RD Truck Load Request Successfully Collected</h3>

            <div className="grid grid-cols-2 gap-4 mt-8">
              <button className="py-4 bg-gray-400 text-white font-medium rounded-lg hover:bg-gray-500 transition-colors">
                PRINT
              </button>
              <button
                onClick={handleSuccessDone}
                className="py-4 bg-[#A08B5C] text-white font-medium rounded-lg hover:bg-[#8F7A4D] transition-colors"
              >
                DONE
              </button>
            </div>
          </div>
        </DialogContent>
      </Dialog>
    </div>
  )
}
