"use client"
import { useState, useEffect } from 'react'
import { 
  TrendingUp, 
  TrendingDown, 
  Package, 
  Truck, 
  Clock, 
  AlertTriangle,
  CheckCircle,
  BarChart3,
  MapPin,
  Calendar
} from 'lucide-react'

const MetricCard = ({ title, value, change, changeType, icon: Icon, color = "blue" }) => {
  const colorClasses = {
    blue: "bg-[#375AE6]",
    green: "bg-[#375AE6]", 
    yellow: "bg-[#375AE6]",
    red: "bg-black",
    purple: "bg-black",
    orange: "bg-[#375AE6]"
  }

  return (
    <div className="bg-white rounded-lg shadow-sm border border-black p-6">
      <div className="flex items-center justify-between">
        <div>
          <p className="text-sm font-medium text-black mb-1">{title}</p>
          <p className="text-3xl font-bold text-black">{value}</p>
          {change && (
            <div className={`flex items-center mt-2 text-sm ${
              changeType === 'positive' ? 'text-[#375AE6]' : 'text-black'
            }`}>
              {changeType === 'positive' ? 
                <TrendingUp className="w-4 h-4 mr-1" /> : 
                <TrendingDown className="w-4 h-4 mr-1" />
              }
              {change}
            </div>
          )}
        </div>
        <div className={`w-12 h-12 ${colorClasses[color]} rounded-lg flex items-center justify-center`}>
          <Icon className="w-6 h-6 text-white" />
        </div>
      </div>
    </div>
  )
}

const DashboardMetrics = ({ userSystem }) => {
  const [data, setData] = useState({})
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    const fetchData = async () => {
      try {
        const response = await fetch('/data.json')
        const jsonData = await response.json()
        setData(jsonData)
      } catch (error) {
        console.error('Error fetching data:', error)
      } finally {
        setLoading(false)
      }
    }
    fetchData()
  }, [])

  if (loading) {
    return (
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
        {[...Array(4)].map((_, i) => (
          <div key={i} className="bg-white rounded-lg shadow-sm border border-gray-200 p-6 animate-pulse">
            <div className="h-4 bg-gray-200 rounded w-3/4 mb-2"></div>
            <div className="h-8 bg-gray-200 rounded w-1/2 mb-2"></div>
            <div className="h-4 bg-gray-200 rounded w-1/4"></div>
          </div>
        ))}
      </div>
    )
  }

  const deliveryMetrics = [
    {
      title: "Total Deliveries",
      value: data.deliveries?.length || 0,
      change: "+12% from last week",
      changeType: "positive",
      icon: Truck,
      color: "blue"
    },
    {
      title: "Pending Deliveries", 
      value: data.deliveries?.filter(d => d.status === 'pending').length || 0,
      change: "-5% from yesterday",
      changeType: "positive",
      icon: Clock,
      color: "yellow"
    },
    {
      title: "Delivery Plans",
      value: Object.values(data.deliveryPlans || {}).flat().length || 0,
      change: "+8% this month",
      changeType: "positive", 
      icon: Package,
      color: "green"
    },
    {
      title: "Completion Rate",
      value: "94.2%",
      change: "+2.1% improvement",
      changeType: "positive",
      icon: CheckCircle,
      color: "purple"
    }
  ]

  const stockMetrics = [
    {
      title: "Stock Items",
      value: data.products?.length || 0,
      change: "+5 new items",
      changeType: "positive", 
      icon: Package,
      color: "blue"
    },
    {
      title: "Low Stock Alerts",
      value: data.products?.filter(p => p.receivedQty < p.deliveryPlanQty * 0.1).length || 0,
      change: "2 critical items",
      changeType: "negative",
      icon: AlertTriangle,
      color: "red"
    },
    {
      title: "Stock Received Today",
      value: data.stockReceivingHistory?.filter(h => h.status === 'completed').length || 0,
      change: "+15% vs yesterday",
      changeType: "positive",
      icon: TrendingUp,
      color: "green"
    },
    {
      title: "Processing Time",
      value: "22 min",
      change: "-3 min improvement",
      changeType: "positive",
      icon: Clock,
      color: "purple"
    }
  ]

  const itineraryMetrics = [
    {
      title: "Coverage Compliance Rate",
      value: "94%",
      change: "Target: 80% Minimum",
      changeType: "positive",
      icon: CheckCircle,
      color: "green"
    },
    {
      title: "Coverage / Scheduled Outlets",
      value: "77/82",
      change: "of Total Grouped Outlets",
      changeType: "positive",
      icon: MapPin,
      color: "blue"
    },
    {
      title: "Time Gone",
      value: "56%",
      change: "14 / 25 days",
      changeType: "positive",
      icon: Clock,
      color: "orange"
    },
    {
      title: "Planned Rate",
      value: "100%",
      change: "Target: 80% Minimum",
      changeType: "positive",
      icon: TrendingUp,
      color: "purple"
    }
  ]

  const metrics = userSystem === 'delivery' ? deliveryMetrics : 
                  userSystem === 'itinerary' ? itineraryMetrics : stockMetrics

  return (
    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
      {metrics.map((metric, index) => (
        <MetricCard key={`${metric.title}-${index}`} {...metric} />
      ))}
    </div>
  )
}

export default DashboardMetrics