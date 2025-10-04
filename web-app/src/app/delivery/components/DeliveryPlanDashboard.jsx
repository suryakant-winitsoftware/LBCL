"use client"
import { useState, useEffect } from 'react'
import { useRouter } from 'next/navigation'
import { Button } from '../../../components/ui/button'
import { 
  Calendar,
  RefreshCw,
  ChevronRight
} from 'lucide-react'
import { useAuth } from '../../../contexts/AuthContext'
import ProtectedRoute from '../../../components/ProtectedRoute'
import BusinessLayout from '../../../components/layouts/BusinessLayout'

const DeliveryPlanDashboard = () => {
  const { user } = useAuth()
  const router = useRouter()
  const [activeTab, setActiveTab] = useState('PENDING APPROVAL')
  const [fromDate, setFromDate] = useState('05-May-2025')
  const [toDate, setToDate] = useState('20-May-2025')

  // Fetch delivery plans data from centralized data.json
  const [deliveryPlans, setDeliveryPlans] = useState({})
  const [defaultValues, setDefaultValues] = useState({})

  useEffect(() => {
    const fetchData = async () => {
      try {
        const response = await fetch('/data.json')
        const data = await response.json()
        setDeliveryPlans(data.deliveryPlans)
        setDefaultValues(data.defaultValues)
      } catch (error) {
        console.error('Error fetching data:', error)
      }
    }
    fetchData()
  }, [])

  const tabs = [
    'PENDING APPROVAL',
    'APPROVED PENDING SHIPMENT',
    'SHIPPED'
  ]

  const handlePlanClick = (planId) => {
    router.push(`/user/delivery/delivery-activity-log/${planId}`)
  }

  const handleActivityLog = () => {
    router.push(`/user/delivery/delivery-activity-log/${defaultValues?.planId || "85444127121"}`)
  }

  const handleRefresh = () => {
    window.location.reload()
  }

  return (
    <ProtectedRoute requiredSystem="delivery">
      <BusinessLayout title="Delivery Plan Dashboard">
        <div className="space-y-3">
          {/* Header Section with Action Button */}
          <div className="flex items-center justify-between">
            <h1 className="text-2xl font-bold text-gray-900">Delivery Plan</h1>
            <button 
              onClick={handleActivityLog}
              className="bg-[#375AE6] hover:bg-[#2947d1] text-white px-4 py-2 rounded-md text-sm font-medium uppercase tracking-wider transition-colors"
            >
              VIEW ACTIVITY LOG
            </button>
          </div>

          {/* Date Range Section */}
          <div className="bg-white border border-gray-200 rounded p-3">
            <div className="flex items-center justify-between gap-4">
              <div className="flex-1 flex items-center gap-2">
                <span className="text-sm text-gray-700 font-semibold">From Date</span>
                <div className="flex-1 flex items-center justify-between bg-white px-3 py-2 rounded-md border border-gray-300 hover:border-[#375AE6] focus-within:border-[#375AE6] focus-within:ring-1 focus-within:ring-[#375AE6]">
                  <span className="text-sm text-gray-900">{fromDate}</span>
                  <Calendar className="w-4 h-4 text-[#375AE6]" />
                </div>
              </div>
              
              <div className="flex-1 flex items-center gap-2">
                <span className="text-sm text-gray-700 font-semibold">To Date</span>
                <div className="flex-1 flex items-center justify-between bg-white px-3 py-2 rounded-md border border-gray-300 hover:border-[#375AE6] focus-within:border-[#375AE6] focus-within:ring-1 focus-within:ring-[#375AE6]">
                  <span className="text-sm text-gray-900">{toDate}</span>
                  <Calendar className="w-4 h-4 text-[#375AE6]" />
                </div>
              </div>
              
              <button 
                onClick={handleRefresh}
                className="p-2 rounded-md hover:bg-[#375AE6] hover:bg-opacity-10 transition-colors"
              >
                <RefreshCw className="w-5 h-5 text-[#375AE6]" />
              </button>
            </div>
          </div>

          {/* Tabs */}
          <div className="bg-white border border-gray-200 rounded overflow-hidden">
            <div className="flex border-b border-gray-200">
              {tabs.map((tab) => (
                <button
                  key={tab}
                  onClick={() => setActiveTab(tab)}
                  className={`flex-1 py-2 px-3 text-sm font-medium text-center transition-colors relative ${
                    activeTab === tab
                      ? 'text-white bg-[#375AE6] font-semibold shadow-sm'
                      : 'text-gray-600 hover:text-[#375AE6] hover:bg-gray-50'
                  }`}
                >
                  {tab}
                  {activeTab === tab && (
                    <div className="absolute bottom-0 left-0 right-0 h-0.5 bg-[#375AE6]"></div>
                  )}
                </button>
              ))}
            </div>
          </div>

          {/* Content */}
          <div className="bg-white border border-gray-200 rounded p-4">
            {!deliveryPlans[activeTab] || deliveryPlans[activeTab].length === 0 ? (
              <div className="text-center py-20 text-gray-500">
                <p className="text-lg">No delivery plans found</p>
                <p className="text-sm mt-2">Try adjusting your date range or check back later.</p>
              </div>
            ) : (
              <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-3">
                {deliveryPlans[activeTab]?.map((plan) => (
                  <div
                    key={plan.id}
                    onClick={() => handlePlanClick(plan.id)}
                    className="bg-white rounded border border-gray-200 cursor-pointer hover:border-[#375AE6] transition-colors p-3"
                  >
                    <div className="flex items-center justify-between mb-2">
                      <h3 className="text-base font-semibold text-gray-900">{plan.id}</h3>
                      <ChevronRight className="w-4 h-4 text-[#375AE6]" />
                    </div>
                    
                    <div className="space-y-1">
                      <p className="text-sm text-gray-700">{plan.distributor}</p>
                      <p className={`text-xs font-medium ${plan.statusColor}`}>
                        {plan.status}
                      </p>
                      <p className="text-xs text-gray-500">{plan.date}</p>
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>
        </div>
      </BusinessLayout>
    </ProtectedRoute>
  )
}

export default DeliveryPlanDashboard