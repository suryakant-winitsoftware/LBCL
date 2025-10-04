"use client"

import { useState } from 'react'
import { useRouter } from 'next/navigation'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { ArrowLeft, RotateCcw, PenTool, X } from 'lucide-react'

export default function SignatureCapturePageDelivery() {
  const router = useRouter()
  const [notes, setNotes] = useState('')
  const [showModal, setShowModal] = useState(false)

  const clearSignature = () => {
    console.log('Signature cleared')
  }

  const saveSignature = () => {
    setShowModal(false)
    alert('Signatures saved successfully!')
    router.back()
  }

  return (
    <div className="space-y-6">
      <Card>
        <CardContent className="pt-6">
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-4">
              <Button variant="outline" size="icon" onClick={() => router.back()}>
                <ArrowLeft className="w-4 h-4" />
              </Button>
              <div>
                <h1 className="text-2xl font-bold">Signature Capture</h1>
                <p className="text-sm text-muted-foreground mt-1">Capture signatures for delivery</p>
              </div>
            </div>
            <Button onClick={() => setShowModal(true)}>
              Open Signature Modal
            </Button>
          </div>
        </CardContent>
      </Card>

      {/* Signature Modal */}
      {showModal && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-lg max-w-3xl w-full p-6 relative">
            <button
              onClick={() => setShowModal(false)}
              className="absolute top-4 right-4 text-gray-400 hover:text-gray-600"
            >
              <X className="w-5 h-5" />
            </button>

            <h2 className="text-xl font-bold mb-6">Signature</h2>

            {/* Dual Signature Canvases */}
            <div className="grid grid-cols-2 gap-4 mb-6">
              {/* Logistic Signature */}
              <div>
                <div className="flex items-center justify-between mb-2">
                  <h3 className="text-sm font-semibold">Logistic Signature</h3>
                  <Button
                    variant="ghost"
                    size="sm"
                    onClick={clearSignature}
                    className="text-xs text-[#9d8b5c]"
                  >
                    Clear
                  </Button>
                </div>
                <div className="border-2 border-dashed rounded-lg p-8 bg-gray-50 min-h-[200px] flex items-center justify-center">
                  <div className="text-center text-muted-foreground">
                    <PenTool className="w-8 h-8 mx-auto mb-2 opacity-50" />
                    <p className="text-xs">(Canvas for signature)</p>
                  </div>
                </div>
                <p className="text-xs text-center text-blue-600 mt-2 font-medium">LBCL Logistics</p>
              </div>

              {/* Driver Signature */}
              <div>
                <div className="flex items-center justify-between mb-2">
                  <h3 className="text-sm font-semibold">Driver Signature</h3>
                  <Button
                    variant="ghost"
                    size="sm"
                    onClick={clearSignature}
                    className="text-xs text-[#9d8b5c]"
                  >
                    Clear
                  </Button>
                </div>
                <div className="border-2 border-dashed rounded-lg p-8 bg-gray-50 min-h-[200px] flex items-center justify-center">
                  <div className="text-center text-muted-foreground">
                    <PenTool className="w-8 h-8 mx-auto mb-2 opacity-50" />
                    <p className="text-xs">(Canvas for signature)</p>
                  </div>
                </div>
                <p className="text-xs text-center text-blue-600 mt-2 font-medium">R.M.K.R. Rathnayake</p>
              </div>
            </div>

            {/* Notes Section */}
            <div className="mb-6">
              <div className="flex items-center justify-between mb-2">
                <h3 className="text-sm font-semibold">Notes</h3>
                <Button
                  variant="ghost"
                  size="sm"
                  onClick={() => setNotes('')}
                  className="text-xs text-[#9d8b5c]"
                >
                  Clear
                </Button>
              </div>
              <textarea
                placeholder="Enter Here"
                value={notes}
                onChange={(e) => setNotes(e.target.value)}
                className="w-full min-h-[120px] p-3 border-2 border-dashed rounded-lg bg-gray-50 resize-none focus:outline-none focus:border-primary"
              />
            </div>

            {/* Done Button */}
            <Button
              onClick={saveSignature}
              className="w-full bg-[#9d8b5c] hover:bg-[#8a7a50]"
            >
              DONE
            </Button>
          </div>
        </div>
      )}

      <Card>
        <CardHeader>
          <CardTitle>Delivery Details</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="space-y-3">
            <div className="flex justify-between py-2 border-b">
              <span className="text-muted-foreground">Delivery ID:</span>
              <span className="font-medium">DEL-2025-001</span>
            </div>
            <div className="flex justify-between py-2 border-b">
              <span className="text-muted-foreground">Date & Time:</span>
              <span className="font-medium">{new Date().toLocaleString()}</span>
            </div>
            <div className="flex justify-between py-2">
              <span className="text-muted-foreground">Total Items:</span>
              <span className="font-medium">45</span>
            </div>
          </div>
        </CardContent>
      </Card>
    </div>
  )
}
