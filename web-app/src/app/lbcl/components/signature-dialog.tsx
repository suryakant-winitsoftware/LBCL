"use client"

import { useState, useRef, useEffect } from "react"
import { Dialog, DialogContent, DialogHeader, DialogTitle } from "@/components/ui/dialog"
import { Button } from "@/components/ui/button"
import { Textarea } from "@/components/ui/textarea"

interface SignatureDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  selectedDriverName?: string
  organizationName?: string
  onSave?: (logisticsSignature: string, driverSignature: string, notes: string) => void
}

export function SignatureDialog({
  open,
  onOpenChange,
  selectedDriverName = "N/A",
  organizationName = "LBCL",
  onSave
}: SignatureDialogProps) {
  const [notes, setNotes] = useState("")

  // Canvas refs
  const logisticCanvasRef = useRef<HTMLCanvasElement>(null)
  const driverCanvasRef = useRef<HTMLCanvasElement>(null)

  // Drawing states
  const [isDrawingLogistic, setIsDrawingLogistic] = useState(false)
  const [isDrawingDriver, setIsDrawingDriver] = useState(false)

  // Saved signature data
  const [savedLogisticSignature, setSavedLogisticSignature] = useState<string | null>(null)
  const [savedDriverSignature, setSavedDriverSignature] = useState<string | null>(null)
  const [savedNotes, setSavedNotes] = useState("")

  // Get current date
  const getCurrentDate = () => {
    const now = new Date()
    return now.toLocaleDateString('en-GB', {
      day: '2-digit',
      month: 'short',
      year: 'numeric'
    }).toUpperCase()
  }

  // Initialize canvas
  useEffect(() => {
    if (open) {
      initializeCanvas(logisticCanvasRef)
      initializeCanvas(driverCanvasRef)

      // Restore saved signatures
      if (savedLogisticSignature) {
        restoreSignature(logisticCanvasRef, savedLogisticSignature)
      }
      if (savedDriverSignature) {
        restoreSignature(driverCanvasRef, savedDriverSignature)
      }

      // Restore saved notes
      setNotes(savedNotes)
    }
  }, [open, savedLogisticSignature, savedDriverSignature, savedNotes])

  const initializeCanvas = (canvasRef: React.RefObject<HTMLCanvasElement>) => {
    const canvas = canvasRef.current
    if (!canvas) return

    const ctx = canvas.getContext('2d')
    if (!ctx) return

    // Set canvas size
    const rect = canvas.getBoundingClientRect()
    canvas.width = rect.width
    canvas.height = rect.height

    // Set drawing styles
    ctx.strokeStyle = '#000000'
    ctx.lineWidth = 2
    ctx.lineCap = 'round'
    ctx.lineJoin = 'round'
  }

  const startDrawing = (
    e: React.MouseEvent<HTMLCanvasElement>,
    canvasRef: React.RefObject<HTMLCanvasElement>,
    setIsDrawing: (value: boolean) => void
  ) => {
    const canvas = canvasRef.current
    if (!canvas) return

    const ctx = canvas.getContext('2d')
    if (!ctx) return

    const rect = canvas.getBoundingClientRect()
    const x = e.clientX - rect.left
    const y = e.clientY - rect.top

    ctx.beginPath()
    ctx.moveTo(x, y)
    setIsDrawing(true)
  }

  const draw = (
    e: React.MouseEvent<HTMLCanvasElement>,
    canvasRef: React.RefObject<HTMLCanvasElement>,
    isDrawing: boolean
  ) => {
    if (!isDrawing) return

    const canvas = canvasRef.current
    if (!canvas) return

    const ctx = canvas.getContext('2d')
    if (!ctx) return

    const rect = canvas.getBoundingClientRect()
    const x = e.clientX - rect.left
    const y = e.clientY - rect.top

    ctx.lineTo(x, y)
    ctx.stroke()
  }

  const stopDrawing = (setIsDrawing: (value: boolean) => void) => {
    setIsDrawing(false)
  }

  const clearCanvas = (canvasRef: React.RefObject<HTMLCanvasElement>) => {
    const canvas = canvasRef.current
    if (!canvas) return

    const ctx = canvas.getContext('2d')
    if (!ctx) return

    ctx.clearRect(0, 0, canvas.width, canvas.height)
  }

  const restoreSignature = (canvasRef: React.RefObject<HTMLCanvasElement>, imageData: string) => {
    const canvas = canvasRef.current
    if (!canvas) return

    const ctx = canvas.getContext('2d')
    if (!ctx) return

    const img = new Image()
    img.onload = () => {
      ctx.clearRect(0, 0, canvas.width, canvas.height)
      ctx.drawImage(img, 0, 0)
    }
    img.src = imageData
  }

  const saveSignatures = () => {
    // Get logistic signature
    const logisticCanvas = logisticCanvasRef.current
    let logisticData = ''
    if (logisticCanvas) {
      logisticData = logisticCanvas.toDataURL('image/png')
      setSavedLogisticSignature(logisticData)
    }

    // Get driver signature
    const driverCanvas = driverCanvasRef.current
    let driverData = ''
    if (driverCanvas) {
      driverData = driverCanvas.toDataURL('image/png')
      setSavedDriverSignature(driverData)
    }

    // Save notes
    setSavedNotes(notes)

    // Call onSave callback if provided with signature data
    if (onSave) {
      onSave(logisticData, driverData, notes)
    }

    // Close dialog
    onOpenChange(false)
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-3xl max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle className="text-lg sm:text-xl">Signature</DialogTitle>
        </DialogHeader>

        <div className="py-4 space-y-4 sm:space-y-6">
          {/* Signatures */}
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div>
              <div className="flex items-center justify-between mb-2">
                <label className="text-sm font-medium">Logistic Signature</label>
                <button
                  onClick={() => clearCanvas(logisticCanvasRef)}
                  className="text-xs sm:text-sm text-[#A08B5C] hover:underline"
                >
                  Clear
                </button>
              </div>
              <div className="border-2 border-gray-300 rounded-lg h-32 sm:h-40 bg-white relative">
                <canvas
                  ref={logisticCanvasRef}
                  className="w-full h-full cursor-crosshair rounded-lg"
                  onMouseDown={(e) => startDrawing(e, logisticCanvasRef, setIsDrawingLogistic)}
                  onMouseMove={(e) => draw(e, logisticCanvasRef, isDrawingLogistic)}
                  onMouseUp={() => stopDrawing(setIsDrawingLogistic)}
                  onMouseLeave={() => stopDrawing(setIsDrawingLogistic)}
                />
                {!savedLogisticSignature && !isDrawingLogistic && (
                  <div className="absolute inset-0 flex items-center justify-center pointer-events-none">
                    <p className="text-xs text-gray-400">Sign here with mouse</p>
                  </div>
                )}
                <div className="absolute bottom-0 left-0 right-0 pointer-events-none">
                  <div className="bg-blue-50 px-3 py-1.5 rounded-b-lg w-full flex items-center justify-between">
                    <p className="text-xs font-medium text-gray-700">{organizationName}</p>
                    <p className="text-[10px] text-gray-500">{getCurrentDate()}</p>
                  </div>
                </div>
              </div>
            </div>

            <div>
              <div className="flex items-center justify-between mb-2">
                <label className="text-sm font-medium">Driver Signature</label>
                <button
                  onClick={() => clearCanvas(driverCanvasRef)}
                  className="text-xs sm:text-sm text-[#A08B5C] hover:underline"
                >
                  Clear
                </button>
              </div>
              <div className="border-2 border-gray-300 rounded-lg h-32 sm:h-40 bg-white relative">
                <canvas
                  ref={driverCanvasRef}
                  className="w-full h-full cursor-crosshair rounded-lg"
                  onMouseDown={(e) => startDrawing(e, driverCanvasRef, setIsDrawingDriver)}
                  onMouseMove={(e) => draw(e, driverCanvasRef, isDrawingDriver)}
                  onMouseUp={() => stopDrawing(setIsDrawingDriver)}
                  onMouseLeave={() => stopDrawing(setIsDrawingDriver)}
                />
                {!savedDriverSignature && !isDrawingDriver && (
                  <div className="absolute inset-0 flex items-center justify-center pointer-events-none">
                    <p className="text-xs text-gray-400">Sign here with mouse</p>
                  </div>
                )}
                <div className="absolute bottom-0 left-0 right-0 pointer-events-none">
                  <div className="bg-blue-50 px-3 py-1.5 rounded-b-lg w-full flex items-center justify-between">
                    <p className="text-xs font-medium text-gray-700">{selectedDriverName}</p>
                    <p className="text-[10px] text-gray-500">{getCurrentDate()}</p>
                  </div>
                </div>
              </div>
            </div>
          </div>

          {/* Notes */}
          <div>
            <div className="flex items-center justify-between mb-2">
              <label className="text-sm font-medium">Notes</label>
              <button className="text-xs sm:text-sm text-[#A08B5C] hover:underline">Clear</button>
            </div>
            <Textarea
              placeholder="Enter Here"
              value={notes}
              onChange={(e) => setNotes(e.target.value)}
              className="min-h-[120px] resize-none"
            />
          </div>
        </div>

        <Button onClick={saveSignatures} className="w-full bg-[#A08B5C] hover:bg-[#8F7A4B] text-white h-12">
          DONE
        </Button>
      </DialogContent>
    </Dialog>
  )
}
