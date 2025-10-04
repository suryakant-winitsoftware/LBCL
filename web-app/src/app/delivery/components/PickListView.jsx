"use client"
import { useState, useEffect } from 'react'
import { useRouter } from 'next/navigation'
import { Button } from '../../../components/ui/button'
import { 
  Menu, 
  X,
  FileImage,
  Download,
  Printer
} from 'lucide-react'
import { useAuth } from '../../../contexts/AuthContext'
import { useWorkflow } from '../../../contexts/WorkflowContext'
import ProtectedRoute from '../../../components/ProtectedRoute'
import BusinessLayout from '../../../components/layouts/BusinessLayout'

const PickListView = () => {
  const router = useRouter()
  const { markStepCompleted } = useWorkflow()
  const [isGenerating, setIsGenerating] = useState(false)
  const [pickListGenerated, setPickListGenerated] = useState(false)

  const handleBack = () => {
    router.back()
  }

  const handleSubmit = () => {
    alert('Pick list submitted successfully!')
    router.push(`/user/delivery/delivery-activity-log/${deliveryData.planNo || '85444127121'}`)
  }

  const handleDone = () => {
    alert('Pick list view completed!')
    router.back()
  }

  const handlePrint = () => {
    // Trigger browser print dialog
    window.print()
    alert('Pick list sent to printer!')
  }

  const handleDownload = () => {
    // Simulate download
    const pickListData = {
      planNo: deliveryData.planNo,
      distributor: deliveryData.distributor,
      date: deliveryData.date,
      items: Array.from({length: 15}, (_, i) => ({
        id: `00${i + 1}`,
        name: `Item ${i + 1}`,
        quantity: Math.floor(Math.random() * 50) + 1,
        location: `A${i + 1}`
      }))
    }
    
    const dataStr = JSON.stringify(pickListData, null, 2)
    const dataUri = 'data:application/json;charset=utf-8,'+ encodeURIComponent(dataStr)
    
    const exportFileDefaultName = `pick-list-${deliveryData.planNo}.json`
    
    const linkElement = document.createElement('a')
    linkElement.setAttribute('href', dataUri)
    linkElement.setAttribute('download', exportFileDefaultName)
    linkElement.click()
    
    alert('Pick list downloaded successfully!')
  }

  const handleGeneratePickList = () => {
    setIsGenerating(true)
    setTimeout(() => {
      setIsGenerating(false)
      setPickListGenerated(true)
      markStepCompleted('pick-list', deliveryData.planNo || '85444127121')
      alert('Pick list generated successfully!')
    }, 2000)
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
      <BusinessLayout title="Pick List View">
        <div className="space-y-6">
          {/* Header with Actions */}
          <div className="flex items-center justify-between">
            <h1 className="text-2xl font-bold text-gray-900">Pick List View</h1>
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
                className="bg-[#375AE6] hover:bg-[#2947d1] text-white"
              >
                Submit
              </Button>
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

        {/* Main Content - Pick List View */}
        <div className="p-4 sm:p-6 flex items-center justify-center min-h-[calc(100vh-200px)] bg-gray-50">
          <div className="bg-white rounded-lg w-full max-w-4xl shadow-lg border border-gray-200">
            <div className="p-6 sm:p-8">
              <div className="flex items-center justify-between mb-6 pb-4 border-b border-gray-200">
                <div>
                  <h3 className="text-xl font-semibold text-gray-900 mb-1">Pick List Document</h3>
                  <p className="text-gray-600 text-sm">View and manage delivery pick lists</p>
                </div>
                <button onClick={handleBack} className="p-2 hover:bg-gray-100 rounded-md transition-colors">
                  <X className="w-5 h-5 text-gray-500" />
                </button>
              </div>
              
              {/* Document Preview */}
              <div className="bg-gray-50 border border-gray-200 rounded-md p-6 mb-6 h-96 overflow-y-auto">
                <div className="bg-white h-full rounded-md border border-gray-100 p-6">
                  {/* Document Header */}
                  <div className="flex justify-between items-center mb-4 pb-3 border-b border-gray-200">
                    <h4 className="font-bold text-lg text-gray-900">Pick Sheet</h4>
                    <span className="text-gray-500 text-sm">Page 1/3</span>
                  </div>
                  
                  <div className="grid grid-cols-2 gap-6 text-sm mb-6">
                    <div>
                      <div className="mb-2"><span className="font-medium text-gray-700">Store:</span> PHD | 06:00</div>
                      <div><span className="font-medium text-gray-700">Run:</span> SYROS9356 David_cast</div>
                    </div>
                    <div>
                      <div className="mb-2"><span className="font-medium text-gray-700">Run Date:</span> Wednesday 20 Nov 2024</div>
                      <div><span className="font-medium text-gray-700">Src Depot:</span> 34 | Print on 06.56 AM</div>
                    </div>
                  </div>
                  
                  {/* Table */}
                  <div className="border border-gray-200 rounded-md overflow-hidden">
                    {/* Table Header */}
                    <div className="bg-gray-50 border-b border-gray-200">
                      <div className="grid grid-cols-4 gap-4 py-3 px-4 text-sm font-medium text-gray-700">
                        <div>Code</div>
                        <div>Item</div>
                        <div className="text-center">Qty</div>
                        <div className="text-center">Location</div>
                      </div>
                    </div>
                    
                    {/* Table Body */}
                    <div className="divide-y divide-gray-200 bg-white">
                      {[...Array(5)].map((_, i) => (
                        <div key={i} className="grid grid-cols-4 gap-4 py-3 px-4 hover:bg-gray-50 transition-colors text-sm">
                          <div className="font-mono text-gray-900">00{i + 1}</div>
                          <div className="text-gray-900">Item {i + 1}</div>
                          <div className="text-center text-gray-900">{Math.floor(Math.random() * 50) + 1}</div>
                          <div className="text-center text-gray-600 font-medium">A{i + 1}</div>
                        </div>
                      ))}
                    </div>
                  </div>
                </div>
              </div>

              {/* Action Buttons */}
              <div className="flex flex-col sm:flex-row gap-3 mb-6">
                <Button 
                  variant="outline" 
                  className="flex-1 border-gray-300 text-gray-700 hover:bg-gray-50 transition-colors py-2 disabled:opacity-50"
                  onClick={handlePrint}
                  disabled={!pickListGenerated}
                >
                  <Printer className="w-4 h-4 mr-2" />
                  Print
                </Button>
                <Button 
                  variant="outline" 
                  className="flex-1 border-gray-300 text-gray-700 hover:bg-gray-50 transition-colors py-2 disabled:opacity-50"
                  onClick={handleDownload}
                  disabled={!pickListGenerated}
                >
                  <Download className="w-4 h-4 mr-2" />
                  Download
                </Button>
              </div>
              
              {/* Generate Pick List Button */}
              {!pickListGenerated && (
                <div className="mb-6">
                  <Button 
                    className="w-full bg-[#375AE6] hover:bg-[#2947d1] text-white font-medium py-3 transition-colors"
                    onClick={handleGeneratePickList}
                    disabled={isGenerating}
                  >
                    {isGenerating ? (
                      <>
                        <div className="w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin mr-2"></div>
                        Generating...
                      </>
                    ) : (
                      'Generate Pick List'
                    )}
                  </Button>
                </div>
              )}
              
              {pickListGenerated && (
                <div className="mb-6 p-4 bg-[#f3f6ff] border border-[#375AE6] rounded-md">
                  <div className="flex items-center gap-2">
                    <div className="w-5 h-5 bg-[#375AE6] rounded-full flex items-center justify-center">
                      <span className="text-white text-sm font-bold">✓</span>
                    </div>
                    <p className="text-[#375AE6] font-medium">Pick list generated successfully</p>
                  </div>
                </div>
              )}
            </div>
            
            <div 
              className="bg-[#375AE6] hover:bg-[#2947d1] text-white text-center py-4 rounded-b-lg cursor-pointer transition-colors"
              onClick={handleDone}
            >
              <span className="font-medium">DONE</span>
            </div>
          </div>
        </div>

        </div>
      </BusinessLayout>
    </ProtectedRoute>
  )
}

export default PickListView