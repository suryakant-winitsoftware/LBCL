"use client"
import { useState, useEffect } from 'react'
import { useRouter } from 'next/navigation'
import { Button } from '../../../components/ui/button'
import { 
  MessageSquare,
  Mail,
  MessageCircle,
  Share2,
  Users,
  Send,
  CheckCircle
} from 'lucide-react'
import { useAuth } from '../../../contexts/AuthContext'
import { useWorkflow } from '../../../contexts/WorkflowContext'
import BusinessLayout from '../../../components/layouts/BusinessLayout'
import ProtectedRoute from '../../../components/ProtectedRoute'

const ShareDeliveryPlan = () => {
  const router = useRouter()
  const { markStepCompleted } = useWorkflow()
  const [selectedShareMethod, setSelectedShareMethod] = useState(null)

  const handleBack = () => {
    router.back()
  }

  const handleSubmit = () => {
    router.push(`/user/delivery/delivery-activity-log/${deliveryData.planNo || '85444127121'}`)
  }

  const handleDone = () => {
    if (selectedShareMethod) {
      alert(`Delivery plan shared via ${selectedShareMethod}!`)
      // Mark the share delivery plan step as completed
      markStepCompleted('share-delivery', deliveryData.planNo || '85444127121')
    }
    router.push(`/user/delivery/delivery-activity-log/${deliveryData.planNo || '85444127121'}`)
  }

  const handleShareMethod = (method) => {
    setSelectedShareMethod(method)
    // Simulate sharing action
    setTimeout(() => {
      alert(`Sharing via ${method}...`)
    }, 100)
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

  const shareOptions = [
    {
      id: 'messages',
      name: 'Messages',
      icon: MessageSquare,
      description: 'Share via SMS/Text Messages',
      color: 'bg-[#375AE6]'
    },
    {
      id: 'email',
      name: 'Email',
      icon: Mail,
      description: 'Share via Email',
      color: 'bg-[#000000]'
    },
    {
      id: 'whatsapp',
      name: 'WhatsApp',
      icon: MessageCircle,
      description: 'Share via WhatsApp',
      color: 'bg-[#375AE6]'
    }
  ]

  return (
    <ProtectedRoute requiredSystem="delivery">
      <BusinessLayout title="Share Delivery Plan">
        <div className="space-y-6">
          {/* Page Header */}
          <div className="bg-[#375AE6] rounded p-4 text-white">
            <div className="flex items-center justify-between">
              <div>
                <h1 className="text-2xl font-bold mb-2">Share Delivery Plan</h1>
                <p className="text-white/90">Share delivery plan with your team and stakeholders</p>
              </div>
              <div className="flex items-center gap-2">
                <Share2 className="w-8 h-8" />
              </div>
            </div>
          </div>

          {/* Delivery Plan Info */}
          <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
            <div className="flex items-center gap-2 mb-4">
              <Users className="w-5 h-5 text-[#375AE6]" />
              <h2 className="text-lg font-semibold text-gray-900">Delivery Plan Details</h2>
            </div>
            <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
              <div className="bg-gray-50 rounded-lg p-4">
                <div className="text-sm text-gray-600 mb-1">Plan Number</div>
                <div className="font-semibold text-gray-900">{deliveryData.planNo}</div>
              </div>
              <div className="bg-gray-50 rounded-lg p-4">
                <div className="text-sm text-gray-600 mb-1">Distributor</div>
                <div className="font-semibold text-gray-900">{deliveryData.distributor}</div>
              </div>
              <div className="bg-gray-50 rounded-lg p-4">
                <div className="text-sm text-gray-600 mb-1">Date</div>
                <div className="font-semibold text-gray-900">{deliveryData.date}</div>
              </div>
            </div>
          </div>

          {/* Share Information */}
          <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
            <div className="bg-[#f3f6ff] border-l-4 border-[#375AE6] p-4 rounded-lg mb-6">
              <div className="flex items-center gap-2">
                <Users className="w-5 h-5 text-[#375AE6]" />
                <p className="text-sm text-[#375AE6] font-medium">
                  The delivery plan will be automatically sent to a group of recipients who are part of the distribution list
                </p>
              </div>
            </div>

            <div className="text-center mb-6">
              <h3 className="text-lg font-semibold text-gray-900 mb-2">Choose Sharing Method</h3>
              <p className="text-gray-600">Select how you would like to share this delivery plan</p>
            </div>

            {/* Share Options */}
            <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-8">
              {shareOptions.map((option) => (
                <button
                  key={option.id}
                  onClick={() => handleShareMethod(option.name)}
                  className={`p-6 rounded-lg border-2 transition-all duration-200 ${
                    selectedShareMethod === option.name
                      ? 'border-[#375AE6] bg-[#375AE6]/10'
                      : 'border-gray-200 hover:border-[#375AE6]/50'
                  }`}
                >
                  <div className="flex flex-col items-center text-center">
                    <div className={`w-16 h-16 ${option.color} rounded-lg flex items-center justify-center mb-4`}>
                      <option.icon className="w-8 h-8 text-white" />
                    </div>
                    <h4 className="font-semibold text-gray-900 mb-2">{option.name}</h4>
                    <p className="text-sm text-gray-600">{option.description}</p>
                    {selectedShareMethod === option.name && (
                      <div className="mt-2 flex items-center text-[#375AE6] text-sm font-medium">
                        <CheckCircle className="w-4 h-4 mr-1" />
                        Selected
                      </div>
                    )}
                  </div>
                </button>
              ))}
            </div>

            {/* Recipients Info */}
            {selectedShareMethod && (
              <div className="bg-gray-50 rounded-lg p-4 mb-6">
                <h4 className="font-medium text-gray-900 mb-2">Recipients List</h4>
                <div className="text-sm text-gray-600 space-y-1">
                  <p>• Distribution Manager (manager@lion.com)</p>
                  <p>• Warehouse Coordinator (warehouse@lion.com)</p>
                  <p>• Logistics Team (logistics@lion.com)</p>
                  <p>• Agent Representative ({deliveryData.distributor})</p>
                </div>
              </div>
            )}
          </div>

          {/* Action Buttons */}
          <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
            <div className="flex items-center justify-between">
              <div>
                <h3 className="text-lg font-semibold text-gray-900">Ready to Share</h3>
                <p className="text-sm text-gray-600 mt-1">
                  {selectedShareMethod 
                    ? `Delivery plan will be shared via ${selectedShareMethod}`
                    : 'Please select a sharing method to continue'
                  }
                </p>
              </div>
              <div className="flex gap-3">
                <Button
                  variant="outline"
                  onClick={handleBack}
                  className="border-gray-300 text-gray-700"
                >
                  Back
                </Button>
                <Button
                  onClick={handleDone}
                  disabled={!selectedShareMethod}
                  className={`px-8 py-3 ${
                    selectedShareMethod 
                      ? 'bg-[#375AE6] text-white' 
                      : 'bg-gray-300 text-gray-500 cursor-not-allowed'
                  }`}
                >
                  <Send className="w-4 h-4 mr-2" />
                  Share Plan
                </Button>
              </div>
            </div>
          </div>
        </div>
      </BusinessLayout>
    </ProtectedRoute>
  )
}

export default ShareDeliveryPlan