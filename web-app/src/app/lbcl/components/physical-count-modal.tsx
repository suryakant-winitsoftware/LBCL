"use client"

import { useState } from "react"
import { X, ImageIcon, FileText } from "lucide-react"

interface PhysicalCountModalProps {
  onClose: () => void
}

interface Product {
  id: string
  name: string
  image: string
  goodCollected: number
  sampleGood: number
  damageCollected: number
  damage: number
  missing: number
}

export function PhysicalCountModal({ onClose }: PhysicalCountModalProps) {
  const [activeTab, setActiveTab] = useState("ALL")
  const [showRateAgent, setShowRateAgent] = useState(false)
  const [showSignature, setShowSignature] = useState(false)
  const [showSuccess, setShowSuccess] = useState(false)
  const [selectedRank, setSelectedRank] = useState<number | null>(null)

  const products: Product[] = [
    {
      id: "1",
      name: "Short Quarter Keg 7.75 Galon Beers",
      image: "/placeholder.svg?height=40&width=40",
      goodCollected: 25,
      sampleGood: 2,
      damageCollected: 25,
      damage: 2,
      missing: 2,
    },
    {
      id: "2",
      name: "Slim Quarter Keg 7.75 Galon",
      image: "/placeholder.svg?height=40&width=40",
      goodCollected: 10,
      sampleGood: 1,
      damageCollected: 10,
      damage: 1,
      missing: 0,
    },
    {
      id: "3",
      name: "Lion Large Beer bottle 625ml",
      image: "/placeholder.svg?height=40&width=40",
      goodCollected: 20,
      sampleGood: 1,
      damageCollected: 20,
      damage: 1,
      missing: 0,
    },
    {
      id: "4",
      name: "Lion Large Beer bottle 330ml",
      image: "/placeholder.svg?height=40&width=40",
      goodCollected: 5,
      sampleGood: 2,
      damageCollected: 5,
      damage: 2,
      missing: 0,
    },
  ]

  const handleSubmit = () => {
    setShowRateAgent(true)
  }

  const handleRateSubmit = () => {
    setShowRateAgent(false)
    setShowSignature(true)
  }

  const handleSignatureDone = () => {
    setShowSignature(false)
    setShowSuccess(true)
  }

  const handleSuccessDone = () => {
    setShowSuccess(false)
    onClose()
  }

  if (showSuccess) {
    return (
      <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
        <div className="bg-white rounded-lg max-w-md w-full p-6">
          <button onClick={handleSuccessDone} className="ml-auto block mb-4">
            <X className="w-6 h-6" />
          </button>
          <div className="text-center">
            <div className="w-24 h-24 bg-green-500 rounded-full flex items-center justify-center mx-auto mb-6">
              <svg className="w-12 h-12 text-white" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={3} d="M5 13l4 4L19 7" />
              </svg>
            </div>
            <h2 className="text-xl font-bold mb-2">Success</h2>
            <p className="text-lg font-semibold mb-2">Physical Count & Audit Empties Successfully Completed</p>
            <p className="text-gray-600 mb-6">
              Empties Stock has completed in <span className="font-bold">20 Min</span>
            </p>
            <div className="flex gap-3">
              <button className="flex-1 py-3 bg-gray-200 text-gray-700 rounded-lg font-semibold hover:bg-gray-300">
                PRINT
              </button>
              <button
                onClick={handleSuccessDone}
                className="flex-1 py-3 bg-[#A08B5C] text-white rounded-lg font-semibold hover:bg-[#8F7A4D]"
              >
                DONE
              </button>
            </div>
          </div>
        </div>
      </div>
    )
  }

  if (showSignature) {
    return (
      <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
        <div className="bg-white rounded-lg max-w-2xl w-full">
          <div className="flex items-center justify-between p-4 border-b">
            <h2 className="text-xl font-bold">Signature</h2>
            <button onClick={() => setShowSignature(false)}>
              <X className="w-6 h-6" />
            </button>
          </div>
          <div className="p-6 space-y-4">
            <div className="grid grid-cols-2 gap-4">
              <div>
                <div className="flex items-center justify-between mb-2">
                  <label className="text-sm font-semibold">Logistic Signature</label>
                  <button className="text-[#A08B5C] text-sm">Clear</button>
                </div>
                <div className="border-2 border-gray-300 rounded-lg h-32 bg-[#FFF8E7] flex items-center justify-center">
                  <span className="text-gray-600">R.T DISTRIBUTORS</span>
                </div>
              </div>
              <div>
                <div className="flex items-center justify-between mb-2">
                  <label className="text-sm font-semibold">Driver Signature</label>
                  <button className="text-[#A08B5C] text-sm">Clear</button>
                </div>
                <div className="border-2 border-gray-300 rounded-lg h-32 bg-[#FFF8E7] flex items-center justify-center">
                  <span className="text-gray-600">R.M.K.P. Rathnayake</span>
                </div>
              </div>
            </div>
            <div>
              <div className="flex items-center justify-between mb-2">
                <label className="text-sm font-semibold">Notes</label>
                <button className="text-[#A08B5C] text-sm">Clear</button>
              </div>
              <textarea
                className="w-full border-2 border-gray-300 rounded-lg p-3 h-24 resize-none"
                placeholder="Enter Here"
              />
            </div>
          </div>
          <button
            onClick={handleSignatureDone}
            className="w-full py-4 bg-[#A08B5C] text-white font-bold text-lg hover:bg-[#8F7A4D]"
          >
            DONE
          </button>
        </div>
      </div>
    )
  }

  if (showRateAgent) {
    return (
      <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
        <div className="bg-white rounded-lg max-w-2xl w-full">
          <div className="flex items-center justify-between p-4 border-b">
            <h2 className="text-xl font-bold">Rate Agent</h2>
            <button onClick={() => setShowRateAgent(false)}>
              <X className="w-6 h-6" />
            </button>
          </div>
          <div className="p-6 space-y-6">
            <div>
              <div className="text-sm text-gray-600 mb-1">Agent</div>
              <div className="font-bold text-lg">R.T DISTRIBUTORS</div>
              <div className="text-sm text-gray-500">23 Nov 2024</div>
            </div>
            <div>
              <div className="text-lg font-semibold mb-4">Rank agents</div>
              <div className="flex items-center justify-between mb-2">
                <span className="text-sm text-gray-600">Rank</span>
              </div>
              <div className="flex gap-2">
                {[1, 2, 3, 4, 5, 6, 7, 8, 9, 10].map((rank) => (
                  <button
                    key={rank}
                    onClick={() => setSelectedRank(rank)}
                    className={`w-12 h-12 rounded-full flex items-center justify-center font-bold text-lg transition-colors ${
                      selectedRank && rank <= selectedRank ? "bg-[#F5A623] text-white" : "bg-gray-200 text-gray-400"
                    }`}
                  >
                    {rank}
                  </button>
                ))}
              </div>
            </div>
            <div>
              <div className="flex items-center justify-between mb-2">
                <label className="text-sm font-semibold">Notes</label>
                <button className="text-[#A08B5C] text-sm">Clear</button>
              </div>
              <textarea
                className="w-full border-2 border-gray-300 rounded-lg p-3 h-24 resize-none"
                placeholder="Enter Here"
              />
            </div>
          </div>
          <button
            onClick={handleRateSubmit}
            className="w-full py-4 bg-[#A08B5C] text-white font-bold text-lg hover:bg-[#8F7A4D]"
          >
            SUBMIT
          </button>
        </div>
      </div>
    )
  }

  return (
    <div className="fixed inset-0 bg-gray-50 z-50 overflow-y-auto">
      {/* Header */}
      <header className="bg-white border-b border-gray-200 px-4 py-4 sticky top-0 z-10">
        <div className="flex items-center justify-between">
          <button onClick={onClose}>
            <X className="w-6 h-6" />
          </button>
          <h1 className="text-lg font-bold text-gray-900 flex-1 text-center">Physical Count & Audit Empties</h1>
          <div className="bg-[#F5A623] text-white px-3 py-1 rounded-lg text-sm font-semibold flex items-center gap-1">
            <span>⏱</span>
            <span>24:15 Min</span>
          </div>
        </div>
      </header>

      {/* Info Section */}
      <div className="bg-white p-4 border-b border-gray-200">
        <div className="grid grid-cols-3 gap-4 mb-4">
          <div>
            <div className="text-xs text-gray-500 mb-1">Agent Name</div>
            <div className="font-bold text-gray-900">R.T DISTRIBUTORS</div>
          </div>
          <div>
            <div className="text-xs text-gray-500 mb-1">Prime Mover</div>
            <div className="font-bold text-gray-900">LK1673</div>
          </div>
          <div>
            <div className="text-xs text-gray-500 mb-1">Date</div>
            <div className="font-bold text-gray-900">24 NOV 2024</div>
          </div>
        </div>
        <div className="flex justify-center">
          <button
            onClick={handleSubmit}
            className="px-8 py-2 bg-[#A08B5C] text-white rounded text-sm font-medium hover:bg-[#8F7A4D]"
          >
            Submit
          </button>
        </div>
      </div>

      {/* Tabs */}
      <div className="bg-white border-b border-gray-200">
        <div className="flex justify-center">
          {["ALL", "LION SCOUT", "LION LAGER", "CALSBURG", "LUXURY BRAND"].map((tab) => (
            <button
              key={tab}
              onClick={() => setActiveTab(tab)}
              className={`px-4 py-3 text-sm font-medium whitespace-nowrap transition-colors relative ${
                activeTab === tab ? "text-[#A08B5C]" : "text-gray-500"
              }`}
            >
              {tab}
              {activeTab === tab && <div className="absolute bottom-0 left-0 right-0 h-1 bg-[#A08B5C]" />}
            </button>
          ))}
        </div>
      </div>

      {/* Table Header */}
      <div className="bg-[#FFF8E7] px-4 py-2 border-b border-gray-200 overflow-x-auto">
        <div className="flex gap-3 min-w-max text-xs font-medium text-gray-700">
          <div className="w-56">Product Code/Description</div>
          <div className="w-32 text-center">Good Empties Collected Qty</div>
          <div className="w-28 text-center">Sample Good Qty</div>
          <div className="w-32 text-center">Damage Empties Collected Qty</div>
          <div className="w-24 text-center">Damage Qty</div>
          <div className="w-20 text-center">Missing</div>
          <div className="w-16 text-center">Image</div>
          <div className="w-16 text-center">Notes</div>
        </div>
      </div>

      {/* Products List */}
      <div className="divide-y divide-gray-200">
        {products.map((product) => (
          <div key={product.id} className="bg-white px-4 py-3 overflow-x-auto">
            <div className="flex gap-3 items-center min-w-max">
              <div className="w-56 flex items-center gap-2">
                <span className="text-sm">{product.name}</span>
              </div>
              <div className="w-32">
                <input
                  type="number"
                  value={product.goodCollected}
                  className="w-full px-2 py-1.5 border border-gray-300 rounded text-center text-sm"
                  readOnly
                />
              </div>
              <div className="w-28">
                <input
                  type="number"
                  value={product.sampleGood}
                  onChange={() => {}}
                  className="w-full px-2 py-1.5 border border-gray-300 rounded text-center text-sm bg-gray-50"
                />
              </div>
              <div className="w-32">
                <input
                  type="number"
                  value={product.damageCollected}
                  className="w-full px-2 py-1.5 border border-gray-300 rounded text-center text-sm"
                  readOnly
                />
              </div>
              <div className="w-24">
                <input
                  type="number"
                  value={product.damage}
                  onChange={() => {}}
                  className="w-full px-2 py-1.5 border border-gray-300 rounded text-center text-sm bg-gray-50"
                />
              </div>
              <div className="w-20">
                <input
                  type="number"
                  value={product.missing}
                  onChange={() => {}}
                  className="w-full px-2 py-1.5 border border-gray-300 rounded text-center text-sm bg-gray-50"
                />
              </div>
              <div className="w-16 flex justify-center">
                <button className="p-2 hover:bg-gray-100 rounded">
                  <ImageIcon className="w-5 h-5 text-gray-400" />
                </button>
              </div>
              <div className="w-16 flex justify-center">
                <button className="p-2 hover:bg-gray-100 rounded">
                  <FileText className="w-5 h-5 text-gray-400" />
                </button>
              </div>
            </div>
          </div>
        ))}
      </div>
    </div>
  )
}
