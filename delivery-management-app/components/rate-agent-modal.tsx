"use client"

import { useState } from "react"
import { X } from "lucide-react"

interface RateAgentModalProps {
  onClose: () => void
}

export function RateAgentModal({ onClose }: RateAgentModalProps) {
  const [selectedRating, setSelectedRating] = useState<number | null>(null)
  const [notes, setNotes] = useState("")

  const handleSubmit = () => {
    // Handle submit logic here
    console.log("[v0] Rating submitted:", { rating: selectedRating, notes })
    onClose()
  }

  return (
    <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
      <div className="bg-white rounded-lg w-full max-w-2xl shadow-xl">
        {/* Header */}
        <div className="flex items-center justify-between p-6 border-b border-gray-200">
          <h2 className="text-2xl font-bold text-gray-900">Rate Agent</h2>
          <button
            onClick={onClose}
            className="w-8 h-8 flex items-center justify-center hover:bg-gray-100 rounded-lg transition-colors"
          >
            <X className="w-6 h-6 text-gray-600" />
          </button>
        </div>

        {/* Content */}
        <div className="p-6 space-y-6">
          {/* Agent Info */}
          <div>
            <div className="text-sm text-gray-600 mb-1">Agent</div>
            <div className="text-xl font-bold text-gray-900">R.T DISTRIBUTORS</div>
            <div className="text-sm text-gray-500">23 Nov 2024</div>
          </div>

          {/* Rating */}
          <div>
            <h3 className="text-lg font-semibold text-gray-900 mb-4">Rank agents</h3>
            <div className="flex gap-3">
              {[1, 2, 3, 4, 5, 6, 7, 8, 9, 10].map((rating) => (
                <button
                  key={rating}
                  onClick={() => setSelectedRating(rating)}
                  className={`w-12 h-12 rounded-full flex items-center justify-center font-bold text-lg transition-all ${
                    rating <= 7
                      ? selectedRating === rating
                        ? "bg-[#D4A574] text-white scale-110"
                        : "bg-[#F5C563] text-white hover:bg-[#D4A574]"
                      : "bg-gray-200 text-gray-400 cursor-not-allowed"
                  }`}
                  disabled={rating > 7}
                >
                  {rating}
                </button>
              ))}
            </div>
          </div>

          {/* Notes */}
          <div>
            <div className="flex items-center justify-between mb-2">
              <h3 className="text-lg font-semibold text-gray-900">Notes</h3>
              <button onClick={() => setNotes("")} className="text-[#A08B5C] text-sm font-medium hover:underline">
                Clear
              </button>
            </div>
            <textarea
              value={notes}
              onChange={(e) => setNotes(e.target.value)}
              placeholder="Enter Here"
              className="w-full px-4 py-3 border border-gray-300 rounded-lg resize-none focus:outline-none focus:ring-2 focus:ring-[#A08B5C] focus:border-transparent"
              rows={4}
            />
          </div>
        </div>

        {/* Submit Button */}
        <div className="p-6 pt-0">
          <button
            onClick={handleSubmit}
            className="w-full py-4 bg-[#A08B5C] text-white rounded-lg font-bold text-lg hover:bg-[#8F7A4D] transition-colors"
          >
            SUBMIT
          </button>
        </div>
      </div>
    </div>
  )
}
