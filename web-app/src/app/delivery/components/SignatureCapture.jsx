"use client"
import { useState, useRef, useEffect } from 'react'
import { useRouter } from 'next/navigation'
import { Button } from '../../../components/ui/button'
import { 
  Menu, 
  X
} from 'lucide-react'
import { useAuth } from '../../../contexts/AuthContext'
import ProtectedRoute from '../../../components/ProtectedRoute'
import BusinessLayout from '../../../components/layouts/BusinessLayout'

const SignatureCapture = () => {
  const router = useRouter()
  const [logisticSignature, setLogisticSignature] = useState('')
  const [driverSignature, setDriverSignature] = useState('')
  const [notes, setNotes] = useState('')
  const [isDrawingLogistic, setIsDrawingLogistic] = useState(false)
  const [isDrawingDriver, setIsDrawingDriver] = useState(false)
  const logisticCanvasRef = useRef(null)
  const driverCanvasRef = useRef(null)

  const handleBack = () => {
    router.back()
  }

  const handleSubmit = () => {
    if (!logisticSignature || !driverSignature) {
      alert('Please provide both signatures before submitting!')
      return
    }
    alert('Signatures submitted successfully!')
    router.push(`/user/delivery/delivery-activity-log/${deliveryData.planNo || '85444127121'}`)
  }

  const handleDone = () => {
    if (logisticSignature && driverSignature) {
      alert('Signatures saved successfully!')
      console.log('Signatures saved:', { logisticSignature, driverSignature, notes })
    } else {
      alert('Please provide both signatures before completing!')
      return
    }
    router.back()
  }

  const clearLogisticSignature = () => {
    setLogisticSignature('')
    if (logisticCanvasRef.current) {
      const ctx = logisticCanvasRef.current.getContext('2d')
      ctx.clearRect(0, 0, logisticCanvasRef.current.width, logisticCanvasRef.current.height)
    }
  }

  const clearDriverSignature = () => {
    setDriverSignature('')
    if (driverCanvasRef.current) {
      const ctx = driverCanvasRef.current.getContext('2d')
      ctx.clearRect(0, 0, driverCanvasRef.current.width, driverCanvasRef.current.height)
    }
  }

  // Initialize canvas drawing functions
  useEffect(() => {
    const setupCanvas = (canvas) => {
      if (!canvas) return
      const rect = canvas.getBoundingClientRect()
      canvas.width = rect.width
      canvas.height = rect.height
      const ctx = canvas.getContext('2d')
      ctx.strokeStyle = '#000'
      ctx.lineWidth = 2
      ctx.lineCap = 'round'
    }

    setupCanvas(logisticCanvasRef.current)
    setupCanvas(driverCanvasRef.current)
  }, [])

  const startDrawing = (e, canvasRef, setIsDrawing, setSignature) => {
    setIsDrawing(true)
    const canvas = canvasRef.current
    const rect = canvas.getBoundingClientRect()
    const x = e.clientX - rect.left
    const y = e.clientY - rect.top
    const ctx = canvas.getContext('2d')
    ctx.beginPath()
    ctx.moveTo(x, y)
    setSignature('signing...')
  }

  const draw = (e, canvasRef, isDrawing) => {
    if (!isDrawing) return
    const canvas = canvasRef.current
    const rect = canvas.getBoundingClientRect()
    const x = e.clientX - rect.left
    const y = e.clientY - rect.top
    const ctx = canvas.getContext('2d')
    ctx.lineTo(x, y)
    ctx.stroke()
  }

  const stopDrawing = (setIsDrawing, setSignature, signatureName) => {
    setIsDrawing(false)
    setSignature(signatureName)
  }

  const clearNotes = () => {
    setNotes('')
  }

  // Fetch delivery data from centralized data.json
  const [deliveryData, setDeliveryData] = useState({})

  useEffect(() => {
    const fetchData = async () => {
      try {
        const response = await fetch('/data.json')
        const data = await response.json()
        setDeliveryData(data.deliveryData)
      } catch (error) {
        console.error('Error fetching data:', error)
      }
    }
    fetchData()
  }, [])

  return (
    <ProtectedRoute requiredSystem="delivery">
      <BusinessLayout title="Signature Capture">
        <div className="space-y-6">
          {/* Header with Actions */}
          <div className="flex items-center justify-between">
            <h1 className="text-2xl font-bold text-gray-900">Signature Capture</h1>
            <div className="flex items-center gap-3">
              <Button 
                variant="outline" 
                size="sm"
                onClick={handleBack}
                className="text-[#375AE6] border-[#375AE6] hover:bg-[#375AE6] hover:text-white"
              >
                Back
              </Button>
              <Button 
                size="sm"
                onClick={handleSubmit}
                className="bg-[#375AE6] hover:bg-[#375AE6] hover:bg-opacity-90 text-white"
              >
                Submit
              </Button>
            </div>
          </div>
        </div>

        {/* Plan Info */}
        <div className="bg-white px-4 py-3 border-b border-black">
          <div className="grid grid-cols-3 gap-4">
            <div>
              <div className="text-sm text-black">Delivery Plan No</div>
              <div className="font-semibold">{deliveryData.planNo}</div>
            </div>
            <div>
              <div className="text-sm text-black">Distributor / Agent</div>
              <div className="font-semibold">{deliveryData.distributor}</div>
            </div>
            <div>
              <div className="text-sm text-black">Date</div>
              <div className="font-semibold">{deliveryData.date}</div>
            </div>
          </div>
        </div>

        {/* Main Content - Signature Capture */}
        <div className="p-4 flex items-center justify-center min-h-[calc(100vh-200px)]">
          <div className="bg-white rounded-lg w-full max-w-md shadow-xl border">
            <div className="p-6">
              <div className="flex items-center justify-between mb-4">
                <h3 className="text-lg font-semibold">Signature</h3>
                <button onClick={handleBack}>
                  <X className="w-5 h-5 text-black" />
                </button>
              </div>
              
              <div className="space-y-6">
                {/* Logistic Signature */}
                <div>
                  <div className="flex items-center justify-between mb-2">
                    <label className="font-medium text-black">Logistic Signature</label>
                    <button 
                      onClick={clearLogisticSignature}
                      className="text-[#375AE6] text-sm hover:text-[#375AE6] hover:opacity-70"
                    >
                      Clear
                    </button>
                  </div>
                  <div className="border-2 border-black rounded-lg h-32 bg-white relative overflow-hidden">
                    <canvas
                      ref={logisticCanvasRef}
                      className="absolute inset-0 w-full h-full cursor-crosshair"
                      onMouseDown={(e) => startDrawing(e, logisticCanvasRef, setIsDrawingLogistic, setLogisticSignature)}
                      onMouseMove={(e) => draw(e, logisticCanvasRef, isDrawingLogistic)}
                      onMouseUp={() => stopDrawing(setIsDrawingLogistic, setLogisticSignature, 'LION Logistics')}
                      onMouseLeave={() => stopDrawing(setIsDrawingLogistic, setLogisticSignature, 'LION Logistics')}
                      onTouchStart={(e) => {
                        const touch = e.touches[0]
                        const mouseEvent = new MouseEvent('mousedown', {
                          clientX: touch.clientX,
                          clientY: touch.clientY
                        })
                        e.target.dispatchEvent(mouseEvent)
                      }}
                      onTouchMove={(e) => {
                        const touch = e.touches[0]
                        const mouseEvent = new MouseEvent('mousemove', {
                          clientX: touch.clientX,
                          clientY: touch.clientY
                        })
                        e.target.dispatchEvent(mouseEvent)
                      }}
                      onTouchEnd={(e) => {
                        const mouseEvent = new MouseEvent('mouseup', {})
                        e.target.dispatchEvent(mouseEvent)
                      }}
                    />
                    {logisticSignature && logisticSignature !== 'signing...' && (
                      <div className="absolute bottom-2 left-2 text-sm text-black font-medium pointer-events-none">
                        {logisticSignature}
                      </div>
                    )}
                    {!logisticSignature && (
                      <div className="absolute inset-0 flex items-center justify-center text-black text-sm pointer-events-none">
                        Tap to sign
                      </div>
                    )}
                  </div>
                </div>
                
                {/* Driver Signature */}
                <div>
                  <div className="flex items-center justify-between mb-2">
                    <label className="font-medium text-black">Driver Signature</label>
                    <button 
                      onClick={clearDriverSignature}
                      className="text-[#375AE6] text-sm hover:text-[#375AE6] hover:opacity-70"
                    >
                      Clear
                    </button>
                  </div>
                  <div className="border-2 border-black rounded-lg h-32 bg-white relative overflow-hidden">
                    <canvas
                      ref={driverCanvasRef}
                      className="absolute inset-0 w-full h-full cursor-crosshair"
                      onMouseDown={(e) => startDrawing(e, driverCanvasRef, setIsDrawingDriver, setDriverSignature)}
                      onMouseMove={(e) => draw(e, driverCanvasRef, isDrawingDriver)}
                      onMouseUp={() => stopDrawing(setIsDrawingDriver, setDriverSignature, 'R.M.K.P. Rathnayake')}
                      onMouseLeave={() => stopDrawing(setIsDrawingDriver, setDriverSignature, 'R.M.K.P. Rathnayake')}
                      onTouchStart={(e) => {
                        const touch = e.touches[0]
                        const mouseEvent = new MouseEvent('mousedown', {
                          clientX: touch.clientX,
                          clientY: touch.clientY
                        })
                        e.target.dispatchEvent(mouseEvent)
                      }}
                      onTouchMove={(e) => {
                        const touch = e.touches[0]
                        const mouseEvent = new MouseEvent('mousemove', {
                          clientX: touch.clientX,
                          clientY: touch.clientY
                        })
                        e.target.dispatchEvent(mouseEvent)
                      }}
                      onTouchEnd={(e) => {
                        const mouseEvent = new MouseEvent('mouseup', {})
                        e.target.dispatchEvent(mouseEvent)
                      }}
                    />
                    {driverSignature && driverSignature !== 'signing...' && (
                      <div className="absolute bottom-2 left-2 text-sm text-black font-medium pointer-events-none">
                        {driverSignature}
                      </div>
                    )}
                    {!driverSignature && (
                      <div className="absolute inset-0 flex items-center justify-center text-black text-sm pointer-events-none">
                        Tap to sign
                      </div>
                    )}
                  </div>
                </div>
                
                {/* Notes */}
                <div>
                  <div className="flex items-center justify-between mb-2">
                    <label className="font-medium text-black">Notes</label>
                    <button 
                      onClick={clearNotes}
                      className="text-[#375AE6] text-sm hover:text-[#375AE6] hover:opacity-70"
                    >
                      Clear
                    </button>
                  </div>
                  <textarea
                    placeholder="Enter Here"
                    className="w-full border-2 border-black rounded-lg p-3 h-24 resize-none focus:border-[#375AE6] focus:ring-2 focus:ring-[#375AE6] focus:ring-opacity-20"
                    value={notes}
                    onChange={(e) => setNotes(e.target.value)}
                  />
                </div>
              </div>
            </div>
            
            <div 
              className="bg-[#375AE6] hover:bg-[#375AE6] hover:bg-opacity-90 text-white text-center py-3 rounded-b-lg cursor-pointer transition-colors"
              onClick={handleDone}
            >
              <span className="font-medium">DONE</span>
            </div>
          </div>
        </div>
      </BusinessLayout>
    </ProtectedRoute>
  )
}

export default SignatureCapture