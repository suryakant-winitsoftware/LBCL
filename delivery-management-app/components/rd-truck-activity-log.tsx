"use client"

import { useState } from "react"
import { useRouter } from "next/navigation"
import { ChevronDown, ChevronRight } from "lucide-react"
import { NavigationMenu } from "@/components/navigation-menu"

export function RDTruckActivityLog() {
  const router = useRouter()
  const [expandedSections, setExpandedSections] = useState<{ [key: number]: boolean }>({
    3: true,
    7: true,
  })

  const toggleSection = (section: number) => {
    setExpandedSections((prev) => ({
      ...prev,
      [section]: !prev[section],
    }))
  }

  const handleSubmit = () => {
    router.push("/truck-loading/collection")
  }

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <header className="bg-white border-b border-gray-200 px-4 py-4 sticky top-0 z-10">
        <div className="flex items-center justify-between">
          <NavigationMenu />
          <h1 className="text-lg sm:text-xl font-bold text-gray-900 flex-1 text-center">
            RD Truck Load Activity Log Report
          </h1>
          <div className="flex items-center gap-2">
            <button
              onClick={() => router.back()}
              className="px-6 py-2 border-2 border-[#A08B5C] text-[#A08B5C] font-medium rounded-lg hover:bg-[#FFF8E7] transition-colors"
            >
              Back
            </button>
            <button
              onClick={handleSubmit}
              className="px-6 py-2 bg-[#A08B5C] text-white font-medium rounded-lg hover:bg-[#8F7A4D] transition-colors"
            >
              Submit
            </button>
          </div>
        </div>
      </header>

      {/* Info Section */}
      <div className="bg-gray-100 border-b border-gray-200 px-4 py-4">
        <div className="max-w-7xl mx-auto grid grid-cols-5 gap-4 text-sm">
          <div>
            <div className="text-xs text-gray-600 mb-1">Load Request No</div>
            <div className="font-bold text-gray-900">85444127121</div>
          </div>
          <div>
            <div className="text-xs text-gray-600 mb-1">Distributor</div>
            <div className="font-bold text-gray-900">[5844] R.T DISTRIBUTOR</div>
          </div>
          <div>
            <div className="text-xs text-gray-600 mb-1">Date</div>
            <div className="font-bold text-gray-900">25 MAY 2025</div>
          </div>
          <div>
            <div className="text-xs text-gray-600 mb-1">Driver Details</div>
            <div className="font-bold text-gray-900">[3678] VANSANTH KUMAR</div>
          </div>
          <div>
            <div className="text-xs text-gray-600 mb-1">Helper Details</div>
            <div className="font-bold text-gray-900">[5378] PRASHANTH</div>
          </div>
        </div>
      </div>

      {/* Activity Steps */}
      <div className="p-6 max-w-7xl mx-auto">
        <div className="grid grid-cols-2 gap-6">
          {/* Left Column */}
          <div className="space-y-4">
            {/* Step 1 */}
            <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-4 flex items-center justify-between hover:shadow-md transition-shadow cursor-pointer">
              <div className="flex items-center gap-4">
                <div className="w-10 h-10 bg-[#FFF8E7] rounded-full flex items-center justify-center">
                  <span className="text-lg font-bold text-gray-900">1</span>
                </div>
                <span className="font-medium text-gray-900">Share RD Truck Load Sheet</span>
              </div>
              <ChevronRight className="w-5 h-5 text-gray-400" />
            </div>

            {/* Step 2 */}
            <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-4 flex items-center justify-between hover:shadow-md transition-shadow cursor-pointer">
              <div className="flex items-center gap-4">
                <div className="w-10 h-10 bg-[#FFF8E7] rounded-full flex items-center justify-center">
                  <span className="text-lg font-bold text-gray-900">2</span>
                </div>
                <span className="font-medium text-gray-900">View / Generate Pick List</span>
              </div>
              <div className="flex items-center gap-2">
                <svg className="w-6 h-6 text-red-600" viewBox="0 0 24 24" fill="currentColor">
                  <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8l-6-6z" />
                  <path d="M14 2v6h6" fill="white" />
                  <text x="12" y="18" fontSize="8" fill="white" textAnchor="middle" fontWeight="bold">
                    PDF
                  </text>
                </svg>
                <ChevronRight className="w-5 h-5 text-gray-400" />
              </div>
            </div>

            {/* Step 3 - Loading (Expandable) */}
            <div className="bg-white rounded-lg shadow-sm border border-gray-200">
              <div className="p-4 flex items-center justify-between cursor-pointer" onClick={() => toggleSection(3)}>
                <div className="flex items-center gap-4">
                  <div className="w-10 h-10 bg-[#FFF8E7] rounded-full flex items-center justify-center">
                    <span className="text-lg font-bold text-gray-900">3</span>
                  </div>
                  <span className="font-medium text-gray-900">Loading</span>
                </div>
                {expandedSections[3] ? (
                  <ChevronDown className="w-5 h-5 text-gray-400" />
                ) : (
                  <ChevronRight className="w-5 h-5 text-gray-400" />
                )}
              </div>

              {expandedSections[3] && (
                <div className="px-4 pb-4 space-y-4 border-t border-gray-100 pt-4">
                  <div className="grid grid-cols-2 gap-4">
                    <div>
                      <label className="text-sm text-gray-600 mb-2 block">Select Vehicle / Prime Mover</label>
                      <select className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-[#A08B5C]">
                        <option>LK1673</option>
                      </select>
                    </div>
                    <div>
                      <label className="text-sm text-gray-600 mb-2 block">Select Driver</label>
                      <select className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-[#A08B5C]">
                        <option>U Kumar</option>
                      </select>
                    </div>
                  </div>
                  <div>
                    <label className="text-sm text-gray-600 mb-2 block">Fork Lift Operator</label>
                    <select className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-[#A08B5C]">
                      <option>Praveen Varma</option>
                    </select>
                  </div>
                  <div className="grid grid-cols-2 gap-4">
                    <div>
                      <label className="text-sm text-gray-600 mb-2 block">Load Start Time</label>
                      <div className="flex gap-2">
                        <input
                          type="number"
                          value="11"
                          className="w-20 px-3 py-2 border border-gray-300 rounded-lg text-center"
                          placeholder="HH"
                        />
                        <input
                          type="number"
                          value="14"
                          className="w-20 px-3 py-2 border border-gray-300 rounded-lg text-center"
                          placeholder="Min"
                        />
                      </div>
                    </div>
                    <div>
                      <label className="text-sm text-gray-600 mb-2 block">Load End Time</label>
                      <div className="flex gap-2">
                        <input
                          type="number"
                          value="12"
                          className="w-20 px-3 py-2 border border-gray-300 rounded-lg text-center"
                          placeholder="HH"
                        />
                        <input
                          type="number"
                          value="25"
                          className="w-20 px-3 py-2 border border-gray-300 rounded-lg text-center"
                          placeholder="Min"
                        />
                      </div>
                    </div>
                  </div>
                </div>
              )}
            </div>

            {/* Step 4 */}
            <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-4 flex items-center justify-between hover:shadow-md transition-shadow cursor-pointer">
              <div className="flex items-center gap-4">
                <div className="w-10 h-10 bg-[#FFF8E7] rounded-full flex items-center justify-center">
                  <span className="text-lg font-bold text-gray-900">4</span>
                </div>
                <span className="font-medium text-gray-900">View / Generate Delivery Note</span>
              </div>
              <div className="flex items-center gap-2">
                <svg className="w-6 h-6 text-red-600" viewBox="0 0 24 24" fill="currentColor">
                  <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8l-6-6z" />
                  <path d="M14 2v6h6" fill="white" />
                  <text x="12" y="18" fontSize="8" fill="white" textAnchor="middle" fontWeight="bold">
                    PDF
                  </text>
                </svg>
                <ChevronRight className="w-5 h-5 text-gray-400" />
              </div>
            </div>
          </div>

          {/* Right Column */}
          <div className="space-y-4">
            {/* Step 5 */}
            <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-4 flex items-center justify-between hover:shadow-md transition-shadow cursor-pointer">
              <div className="flex items-center gap-4">
                <div className="w-10 h-10 bg-[#FFF8E7] rounded-full flex items-center justify-center">
                  <span className="text-lg font-bold text-gray-900">5</span>
                </div>
                <span className="font-medium text-gray-900">Receive Stock & Keg</span>
              </div>
              <div className="flex items-center gap-2">
                <svg className="w-6 h-6 text-red-600" viewBox="0 0 24 24" fill="currentColor">
                  <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8l-6-6z" />
                  <path d="M14 2v6h6" fill="white" />
                  <text x="12" y="18" fontSize="8" fill="white" textAnchor="middle" fontWeight="bold">
                    PDF
                  </text>
                </svg>
                <ChevronRight className="w-5 h-5 text-gray-400" />
              </div>
            </div>

            {/* Step 6 */}
            <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-4 flex items-center justify-between hover:shadow-md transition-shadow cursor-pointer">
              <div className="flex items-center gap-4">
                <div className="w-10 h-10 bg-[#FFF8E7] rounded-full flex items-center justify-center">
                  <span className="text-lg font-bold text-gray-900">6</span>
                </div>
                <span className="font-medium text-gray-900">IT Officer Confirmation</span>
              </div>
              <ChevronRight className="w-5 h-5 text-gray-400" />
            </div>

            {/* Step 7 - Gate Pass (Expandable) */}
            <div className="bg-white rounded-lg shadow-sm border border-gray-200">
              <div className="p-4 flex items-center justify-between cursor-pointer" onClick={() => toggleSection(7)}>
                <div className="flex items-center gap-4">
                  <div className="w-10 h-10 bg-[#FFF8E7] rounded-full flex items-center justify-center">
                    <span className="text-lg font-bold text-gray-900">7</span>
                  </div>
                  <span className="font-medium text-gray-900">Gate Pass</span>
                </div>
                {expandedSections[7] ? (
                  <ChevronDown className="w-5 h-5 text-gray-400" />
                ) : (
                  <ChevronRight className="w-5 h-5 text-gray-400" />
                )}
              </div>

              {expandedSections[7] && (
                <div className="px-4 pb-4 space-y-4 border-t border-gray-100 pt-4">
                  <div>
                    <label className="text-sm text-gray-600 mb-2 block">Select Security Officer</label>
                    <select className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-[#A08B5C]">
                      <option>Vasanth Kumar</option>
                    </select>
                  </div>
                  <div>
                    <label className="text-sm text-gray-600 mb-2 block">Prime Mover Departure</label>
                    <div className="flex gap-2">
                      <input
                        type="number"
                        value="13"
                        className="w-20 px-3 py-2 border border-gray-300 rounded-lg text-center"
                        placeholder="HH"
                      />
                      <input
                        type="number"
                        value="26"
                        className="w-20 px-3 py-2 border border-gray-300 rounded-lg text-center"
                        placeholder="Min"
                      />
                    </div>
                  </div>
                </div>
              )}
            </div>
          </div>
        </div>
      </div>
    </div>
  )
}
