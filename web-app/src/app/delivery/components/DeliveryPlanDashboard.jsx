"use client"
import { useState, useEffect } from 'react'
import { useRouter } from 'next/navigation'
import { Button } from '../../../components/ui/button'
import { Card, CardHeader, CardTitle, CardContent } from '../../../components/ui/card'
import {
  Calendar,
  RefreshCw,
  ChevronRight,
  Bell,
  Menu
} from 'lucide-react'

const DeliveryPlanDashboard = () => {
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
        // Mock data matching PDF design
        const mockData = {
          deliveryPlans: {
            'PENDING APPROVAL': [
              {
                id: '85444127121',
                distributorCode: '[5844]',
                distributorName: 'R.T DISTRIBUTOR',
                status: 'Pending Approval',
                statusColor: 'bg-orange-100 text-orange-700',
                date: '28 OCT, 2025 AT 14:23'
              },
              {
                id: '85444127122',
                distributorCode: '[5844]',
                distributorName: 'R.T DISTRIBUTOR',
                status: 'Pending Approval',
                statusColor: 'bg-orange-100 text-orange-700',
                date: '28 OCT, 2025 AT 14:23'
              }
            ],
            'APPROVED PENDING SHIPMENT': [
              {
                id: '85444127123',
                distributorCode: '[5844]',
                distributorName: 'R.T DISTRIBUTOR',
                status: 'Approved',
                statusColor: 'bg-green-100 text-green-700',
                date: '29 OCT, 2025 AT 10:15'
              }
            ],
            'SHIPPED': [
              {
                id: '85444127124',
                distributorCode: '[5844]',
                distributorName: 'R.T DISTRIBUTOR',
                status: 'Shipped',
                statusColor: 'bg-blue-100 text-blue-700',
                date: '30 OCT, 2025 AT 08:00'
              }
            ]
          },
          defaultValues: {
            planId: '85444127121'
          }
        }
        setDeliveryPlans(mockData.deliveryPlans)
        setDefaultValues(mockData.defaultValues)
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
    router.push(`/delivery/delivery-activity-log/${planId}`)
  }

  const handleActivityLog = () => {
    router.push(`/delivery/delivery-activity-log/${defaultValues?.planId || "85444127121"}`)
  }

  const handleRefresh = () => {
    window.location.reload()
  }

  return (
    <div className="space-y-4">
      {/* Header Section matching PDF */}
      <div className="flex items-center justify-between py-4 px-2">
        <div className="flex items-center gap-4">
          <Button variant="ghost" size="icon">
            <Menu className="w-6 h-6" />
          </Button>
          <h1 className="text-xl font-bold">Delivery Plan</h1>
        </div>
        <div className="flex items-center gap-4">
          <Button
            onClick={handleActivityLog}
            variant="ghost"
            className="text-sm font-medium text-muted-foreground hover:text-foreground"
          >
            VIEW ACTIVITY LOG
          </Button>
          <Button variant="ghost" size="icon">
            <Bell className="w-5 h-5" />
          </Button>
        </div>
      </div>

      {/* Date Range Section */}
      <Card>
        <CardContent className="pt-6">
          <div className="flex items-center gap-4">
            <div className="flex-1 flex items-center gap-2">
              <span className="text-sm font-semibold">From Date</span>
              <div className="flex-1 flex items-center justify-between px-3 py-2 rounded-md border hover:border-primary/50 focus-within:border-primary focus-within:ring-1 focus-within:ring-primary">
                <span className="text-sm">{fromDate}</span>
                <Calendar className="w-4 h-4 text-primary" />
              </div>
            </div>

            <div className="flex-1 flex items-center gap-2">
              <span className="text-sm font-semibold">To Date</span>
              <div className="flex-1 flex items-center justify-between px-3 py-2 rounded-md border hover:border-primary/50 focus-within:border-primary focus-within:ring-1 focus-within:ring-primary">
                <span className="text-sm">{toDate}</span>
                <Calendar className="w-4 h-4 text-primary" />
              </div>
            </div>

            <Button
              onClick={handleRefresh}
              variant="outline"
              size="icon"
            >
              <RefreshCw className="w-4 h-4" />
            </Button>
          </div>
        </CardContent>
      </Card>

      {/* Tabs */}
      <Card>
        <div className="flex border-b">
          {tabs.map((tab) => (
            <button
              key={tab}
              onClick={() => setActiveTab(tab)}
              className={`flex-1 py-3 px-4 text-sm font-medium text-center transition-colors relative ${
                activeTab === tab
                  ? 'text-primary bg-primary/5 border-b-2 border-primary'
                  : 'text-muted-foreground hover:text-foreground hover:bg-muted/50'
              }`}
            >
              {tab}
            </button>
          ))}
        </div>

        {/* Content */}
        <CardContent className="p-6">
          {!deliveryPlans[activeTab] || deliveryPlans[activeTab].length === 0 ? (
            <div className="text-center py-20 text-muted-foreground">
              <p className="text-lg">No delivery plans found</p>
              <p className="text-sm mt-2">Try adjusting your date range or check back later.</p>
            </div>
          ) : (
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-4">
              {deliveryPlans[activeTab]?.map((plan) => (
                <Card
                  key={plan.id}
                  onClick={() => handlePlanClick(plan.id)}
                  className="cursor-pointer hover:border-primary hover:shadow-lg transition-all bg-gradient-to-br from-white to-gray-50"
                >
                  <CardContent className="p-5">
                    <div className="flex items-start justify-between mb-3">
                      <h3 className="text-2xl font-bold tracking-tight">{plan.id}</h3>
                      <ChevronRight className="w-5 h-5 text-gray-400 mt-1" />
                    </div>

                    <div className="space-y-2">
                      <p className="text-sm font-semibold text-gray-900">
                        <span className="text-gray-600">{plan.distributorCode}</span> {plan.distributorName}
                      </p>
                      <span className={`inline-block px-3 py-1 rounded-full text-xs font-medium ${plan.statusColor}`}>
                        {plan.status}
                      </span>
                      <p className="text-xs text-gray-500">{plan.date}</p>
                    </div>
                  </CardContent>
                </Card>
              ))}
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  )
}

export default DeliveryPlanDashboard