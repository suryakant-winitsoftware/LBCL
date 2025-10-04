"use client"
import { useState, useEffect } from 'react'
import { useRouter } from 'next/navigation'
import { 
  Truck, 
  Package, 
  Clock, 
  CheckCircle,
  AlertTriangle,
  TrendingUp,
  Eye,
  Download,
  Plus
} from 'lucide-react'
import BusinessLayout from '../../../components/layouts/BusinessLayout'
import DashboardMetrics from '../../../components/dashboard/DashboardMetrics'
import DataTable from '../../../components/ui/DataTable'
import { Button } from '../../../components/ui/button'
import { useAuth } from '../../../contexts/AuthContext'
import ProtectedRoute from '../../../components/ProtectedRoute'

const DeliveryDashboard = () => {
  const [deliveryData, setDeliveryData] = useState({})
  const [loading, setLoading] = useState(true)
  const { user } = useAuth()
  const router = useRouter()

  useEffect(() => {
    const fetchData = async () => {
      try {
        const response = await fetch('/data.json')
        const data = await response.json()
        setDeliveryData(data)
      } catch (error) {
        console.error('Error fetching data:', error)
      } finally {
        setLoading(false)
      }
    }
    fetchData()
  }, [])

  // Format delivery plans data for table
  const deliveryPlansData = []
  if (deliveryData.deliveryPlans) {
    Object.entries(deliveryData.deliveryPlans).forEach(([status, plans]) => {
      plans.forEach(plan => {
        deliveryPlansData.push({
          ...plan,
          statusCategory: status
        })
      })
    })
  }

  const columns = [
    {
      key: 'id',
      title: 'Plan ID',
      render: (value) => (
        <div className="font-mono text-sm">{value}</div>
      )
    },
    {
      key: 'distributor',
      title: 'Distributor',
      render: (value) => (
        <div className="text-sm font-medium">{value}</div>
      )
    },
    {
      key: 'status',
      title: 'Status',
      render: (value, row) => {
        const getStatusColor = (status) => {
          switch (status.toLowerCase()) {
            case 'pending approval': return 'bg-yellow-100 text-yellow-800 border border-yellow-200'
            case 'approved': return 'bg-blue-100 text-blue-800 border border-blue-200' 
            case 'shipped': return 'bg-purple-100 text-purple-800 border border-purple-200'
            case 'delivered': return 'bg-green-100 text-green-800 border border-green-200'
            default: return 'bg-gray-100 text-gray-800 border border-gray-200'
          }
        }
        return (
          <span className={`inline-flex px-3 py-1 text-xs font-medium rounded-full ${getStatusColor(value)}`}>
            {value}
          </span>
        )
      }
    },
    {
      key: 'date',
      title: 'Date',
      render: (value) => (
        <div className="text-sm text-gray-900">{value}</div>
      )
    }
  ]

  const actions = [
    {
      icon: Eye,
      title: 'View Details',
      onClick: (row) => {
        router.push(`/user/delivery/delivery-activity-log/${row.id}`)
      },
      className: 'text-blue-600 hover:text-blue-800 transition-colors duration-200'
    },
    {
      icon: Download,
      title: 'Download',
      onClick: (row) => {
        console.log('Download plan:', row.id)
      },
      className: 'text-gray-600 hover:text-blue-600 transition-colors duration-200'
    }
  ]

  const recentDeliveries = deliveryData.deliveries?.slice(0, 5) || []

  return (
    <ProtectedRoute requiredSystem="delivery">
      <BusinessLayout title="Delivery Management System">
        <div className="space-y-6">
          {/* Welcome Section */}
          <div className="bg-blue-600 rounded-lg shadow-sm p-6 text-white mb-6">
            <div className="flex flex-col lg:flex-row lg:items-center lg:justify-between gap-4">
              <div>
                <h1 className="text-2xl font-bold text-white mb-2">
                  Welcome back, {user?.name || 'Delivery Manager'}!
                </h1>
                <p className="text-blue-100">
                  Here's what's happening with your deliveries today
                </p>
              </div>
              <div className="text-left lg:text-right">
                <p className="text-blue-200 text-sm mb-1">Today's Date</p>
                <p className="text-xl font-semibold text-white">
                  {new Date().toLocaleDateString('en-GB', {
                    day: '2-digit',
                    month: 'short',
                    year: 'numeric'
                  })}
                </p>
              </div>
            </div>
          </div>

          {/* Metrics */}
          <DashboardMetrics userSystem="delivery" />

          {/* Quick Actions */}
          <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
            <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
              <div className="flex items-center justify-between mb-4">
                <h3 className="text-lg font-semibold text-gray-900">Quick Actions</h3>
                <Plus className="w-5 h-5 text-blue-600" />
              </div>
              <div className="space-y-3">
                <Button
                  onClick={() => router.push('/user/delivery/delivery-plan-dashboard')}
                  className="w-full bg-blue-600 hover:bg-blue-700 text-white font-medium transition-all duration-200"
                >
                  <Package className="w-4 h-4 mr-2" />
                  View All Plans
                </Button>
                <Button
                  onClick={() => router.push(`/user/delivery/delivery-activity-log/${deliveryData.deliveryData?.planNo || '85444127121'}`)}
                  variant="outline"
                  className="w-full border-blue-600 text-blue-600 hover:bg-blue-600 hover:text-white transition-all duration-200"
                >
                  <Clock className="w-4 h-4 mr-2" />
                  Activity Log
                </Button>
              </div>
            </div>

            {/* System Status */}
            <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
              <div className="flex items-center justify-between mb-4">
                <h3 className="text-lg font-semibold text-gray-900">System Status</h3>
                <div className="w-3 h-3 bg-green-500 rounded-full animate-pulse"></div>
              </div>
              <div className="space-y-3">
                <div className="flex items-center justify-between">
                  <span className="text-sm text-gray-600">Server Status</span>
                  <span className="text-sm font-medium text-green-600">Online</span>
                </div>
                <div className="flex items-center justify-between">
                  <span className="text-sm text-gray-600">Last Sync</span>
                  <span className="text-sm font-medium text-gray-900">2 min ago</span>
                </div>
                <div className="flex items-center justify-between">
                  <span className="text-sm text-gray-600">Active Users</span>
                  <span className="text-sm font-medium text-gray-900">12</span>
                </div>
              </div>
            </div>

            {/* Recent Activity */}
            <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
              <h3 className="text-lg font-semibold text-gray-900 mb-4">Recent Activity</h3>
              <div className="space-y-3">
                {recentDeliveries.map((delivery, index) => (
                  <div key={index} className="flex items-center space-x-3">
                    <div className="w-2 h-2 bg-blue-600 rounded-full"></div>
                    <div className="flex-1 min-w-0">
                      <p className="text-sm text-gray-900 truncate">
                        Plan {delivery.deliveryPlan}
                      </p>
                      <p className="text-xs text-gray-500">{delivery.date}</p>
                    </div>
                    <span className={`text-xs px-2 py-1 rounded-full font-medium ${
                      delivery.status === 'pending' 
                        ? 'bg-yellow-100 text-yellow-800 border border-yellow-200' 
                        : 'bg-green-100 text-green-800 border border-green-200'
                    }`}>
                      {delivery.status}
                    </span>
                  </div>
                ))}
              </div>
            </div>
          </div>

          {/* Delivery Plans Table */}
          <DataTable
            data={deliveryPlansData}
            columns={columns}
            title="Delivery Plans Overview"
            actions={actions}
            loading={loading}
            onRowClick={(row) => router.push(`/user/delivery/delivery-activity-log/${row.id}`)}
            className="mt-6"
          />
        </div>
      </BusinessLayout>
    </ProtectedRoute>
  )
}

export default DeliveryDashboard