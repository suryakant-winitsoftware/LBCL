"use client"
import { useState, useEffect } from 'react'
import { useRouter } from 'next/navigation'
import {
  Menu,
  RefreshCw,
  Bell,
  FileText,
  ChevronRight
} from 'lucide-react'
import { Button } from '../../../components/ui/button'
import { Card, CardContent } from '../../../components/ui/card'

const StockReceivingDashboard = () => {
  const router = useRouter()
  const [activeTab, setActiveTab] = useState('PENDING')
  const [stockData, setStockData] = useState({})

  useEffect(() => {
    fetchData()
  }, [])

  const fetchData = async () => {
    const mockData = {
      PENDING: [
        {
          deliveryPlanNo: '85444127121',
          deliveryNoteNo: 'DN4673899',
          orderDate: '20-MAY-2025',
          uniqueSkuCount: 40
        }
      ],
      COMPLETED: [
        {
          deliveryPlanNo: '85444127120',
          deliveryNoteNo: 'DN4673898',
          orderDate: '19-MAY-2025',
          uniqueSkuCount: 35
        }
      ]
    }
    setStockData(mockData)
  }

  const handleActivityLog = (item) => {
    router.push(`/manager/activity-log-report/${item.deliveryPlanNo}`)
  }

  const handleViewDetails = (item) => {
    router.push('/manager/physical-count')
  }

  const handleRefresh = () => {
    window.location.reload()
  }

  const tabs = ['PENDING', 'COMPLETED']

  return (
    <div className="space-y-4">
      {/* Header */}
      <div className="flex items-center justify-between py-4 px-2">
        <div className="flex items-center gap-4">
          <Button variant="ghost" size="icon">
            <Menu className="w-6 h-6" />
          </Button>
          <h1 className="text-xl font-bold">Manage Agent Stock Receiving</h1>
        </div>
        <div className="flex items-center gap-2">
          <Button variant="ghost" size="icon" onClick={handleRefresh}>
            <RefreshCw className="w-5 h-5" />
          </Button>
          <Button variant="ghost" size="icon">
            <Bell className="w-5 h-5" />
          </Button>
        </div>
      </div>

      {/* Tabs and Content */}
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

        <CardContent className="p-0">
          {!stockData[activeTab] || stockData[activeTab].length === 0 ? (
            <div className="text-center py-20 text-muted-foreground">
              <p className="text-lg">No records found</p>
              <p className="text-sm mt-2">No {activeTab.toLowerCase()} stock receiving records.</p>
            </div>
          ) : (
            <div className="divide-y">
              {stockData[activeTab]?.map((item, index) => (
                <div
                  key={index}
                  className="p-6 hover:bg-gray-50 transition-colors"
                >
                  <div className="flex items-center justify-between">
                    <div className="grid grid-cols-4 gap-8 flex-1">
                      <div>
                        <p className="text-xs text-muted-foreground mb-1">Delivery Plan No</p>
                        <p className="font-bold">{item.deliveryPlanNo}</p>
                      </div>
                      <div>
                        <p className="text-xs text-muted-foreground mb-1">Delivery Note No</p>
                        <p className="font-bold">{item.deliveryNoteNo}</p>
                      </div>
                      <div>
                        <p className="text-xs text-muted-foreground mb-1">Order Date</p>
                        <p className="font-bold">{item.orderDate}</p>
                      </div>
                      <div>
                        <p className="text-xs text-muted-foreground mb-1">No of Unique Sku</p>
                        <p className="font-bold">{item.uniqueSkuCount}</p>
                      </div>
                    </div>

                    <div className="flex items-center gap-3 ml-4">
                      <button
                        onClick={() => handleActivityLog(item)}
                        className="flex flex-col items-center gap-1 p-2 hover:bg-gray-100 rounded-lg transition-colors"
                      >
                        <FileText className="w-5 h-5 text-gray-600" />
                        <span className="text-xs text-gray-600">Activity Log</span>
                      </button>
                      <button
                        onClick={() => handleViewDetails(item)}
                        className="flex flex-col items-center gap-1 p-2 hover:bg-gray-100 rounded-lg transition-colors"
                      >
                        <FileText className="w-5 h-5 text-gray-600" />
                        <span className="text-xs text-gray-600">View Details</span>
                      </button>
                      <ChevronRight className="w-5 h-5 text-gray-400" />
                    </div>
                  </div>
                </div>
              ))}
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  )
}

export default StockReceivingDashboard
