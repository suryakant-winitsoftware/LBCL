"use client"

import React, { useState, useEffect } from "react"
import { KPICards } from "@/components/dashboard/KPICards"
import { FilterBar } from "@/components/dashboard/FilterBar"
import { SalesChart } from "@/components/dashboard/SalesChart"
import { JourneyCompliance } from "@/components/JourneyCompliance"
import { TimeMotionAnalysis } from "@/components/TimeMotionAnalysis"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs"
import { Button } from "@/components/ui/button"
import { RefreshCw, Download, TrendingUp, Package, Users, MapPin } from "lucide-react"
import { DashboardKPI, SalesTrendData, FilterOptions } from "@/types"

const DashboardPage = () => {
  const [loading, setLoading] = useState(false)
  const [kpiData, setKpiData] = useState<DashboardKPI>({
    todaySales: 45678.90,
    todayOrders: 234,
    todayCustomers: 156,
    averageOrderValue: 195.25,
    conversionRate: 68.5,
    mtdSales: 985432.10,
    ytdSales: 12456789.50,
    growthPercentage: 12.5
  })
  
  const [salesData, setSalesData] = useState<SalesTrendData[]>([])
  const [filters, setFilters] = useState<Partial<FilterOptions>>({})
  const [searchTerm, setSearchTerm] = useState("")
  const [selectedTab, setSelectedTab] = useState("overview")

  useEffect(() => {
    generateSalesData()
    fetchDashboardData()
  }, [])

  const generateSalesData = () => {
    const data: SalesTrendData[] = []
    const today = new Date()
    
    for (let i = 89; i >= 0; i--) {
      const date = new Date(today)
      date.setDate(date.getDate() - i)
      
      data.push({
        date: date.toISOString().split('T')[0],
        sales: Math.floor(Math.random() * 50000) + 30000,
        orders: Math.floor(Math.random() * 200) + 150,
        customers: Math.floor(Math.random() * 150) + 100,
        averageOrderValue: Math.floor(Math.random() * 100) + 150
      })
    }
    
    setSalesData(data)
  }

  const fetchDashboardData = async () => {
    setLoading(true)
    try {
      // Simulate API call
      await new Promise(resolve => setTimeout(resolve, 1000))
      // Data would be fetched from API here
    } catch (error) {
      console.error("Error fetching dashboard data:", error)
    } finally {
      setLoading(false)
    }
  }

  const handleRefresh = () => {
    fetchDashboardData()
    generateSalesData()
  }

  const handleExport = () => {
    // Export functionality
    console.log("Exporting dashboard data...")
  }

  const handleFilterChange = (newFilters: Partial<FilterOptions>) => {
    setFilters(newFilters)
    // Apply filters to data
    fetchDashboardData()
  }

  const handleSearchChange = (term: string) => {
    setSearchTerm(term)
    // Apply search
  }

  const quickStats = [
    {
      label: "Active Routes",
      value: "24",
      icon: <MapPin className="h-5 w-5" />,
      change: "+2 from last week",
      trend: "up"
    },
    {
      label: "Total SKUs",
      value: "1,234",
      icon: <Package className="h-5 w-5" />,
      change: "+45 new products",
      trend: "up"
    },
    {
      label: "Active Users",
      value: "89",
      icon: <Users className="h-5 w-5" />,
      change: "12 online now",
      trend: "neutral"
    },
    {
      label: "Completion Rate",
      value: "94.5%",
      icon: <TrendingUp className="h-5 w-5" />,
      change: "+2.3% from yesterday",
      trend: "up"
    }
  ]

  return (
    <div className="flex flex-col gap-6 p-6">
      {/* Page Header */}
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Dashboard</h1>
          <p className="text-gray-500 mt-1">
            Welcome back! Here's what's happening with your business today.
          </p>
        </div>
        <div className="flex gap-2">
          <Button
            variant="outline"
            size="sm"
            onClick={handleRefresh}
            disabled={loading}
          >
            <RefreshCw className={`h-4 w-4 mr-2 ${loading ? 'animate-spin' : ''}`} />
            Refresh
          </Button>
          <Button
            variant="outline"
            size="sm"
            onClick={handleExport}
          >
            <Download className="h-4 w-4 mr-2" />
            Export
          </Button>
        </div>
      </div>

      {/* Filter Bar */}
      <FilterBar
        onFilterChange={handleFilterChange}
        onSearchChange={handleSearchChange}
      />

      {/* KPI Cards */}
      <KPICards data={kpiData} loading={loading} />

      {/* Main Content Tabs */}
      <Tabs value={selectedTab} onValueChange={setSelectedTab} className="space-y-4">
        <TabsList className="grid w-full grid-cols-4 lg:w-auto lg:inline-grid">
          <TabsTrigger value="overview">Overview</TabsTrigger>
          <TabsTrigger value="sales">Sales Analysis</TabsTrigger>
          <TabsTrigger value="customers">Customers</TabsTrigger>
          <TabsTrigger value="operations">Operations</TabsTrigger>
        </TabsList>

        <TabsContent value="overview" className="space-y-4">
          {/* Quick Stats */}
          <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
            {quickStats.map((stat, index) => (
              <Card key={index}>
                <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                  <CardTitle className="text-sm font-medium">
                    {stat.label}
                  </CardTitle>
                  <div className="text-gray-500">
                    {stat.icon}
                  </div>
                </CardHeader>
                <CardContent>
                  <div className="text-2xl font-bold">{stat.value}</div>
                  <p className={`text-xs ${
                    stat.trend === 'up' ? 'text-green-600' :
                    stat.trend === 'down' ? 'text-red-600' :
                    'text-gray-500'
                  }`}>
                    {stat.change}
                  </p>
                </CardContent>
              </Card>
            ))}
          </div>

          {/* Sales Chart */}
          <SalesChart data={salesData} loading={loading} chartType="composed" />

          {/* Additional Charts Grid */}
          <div className="grid gap-4 md:grid-cols-2">
            <Card>
              <CardHeader>
                <CardTitle>Top Performing Routes</CardTitle>
              </CardHeader>
              <CardContent>
                <div className="space-y-3">
                  {[
                    { route: "Downtown", sales: 125000, completion: 98 },
                    { route: "Uptown", sales: 98000, completion: 95 },
                    { route: "Suburbs", sales: 87000, completion: 92 },
                    { route: "Industrial", sales: 76000, completion: 89 },
                  ].map((item, index) => (
                    <div key={index} className="flex items-center justify-between">
                      <div className="flex-1">
                        <div className="flex justify-between mb-1">
                          <span className="text-sm font-medium">{item.route}</span>
                          <span className="text-sm text-gray-500">
                            ${(item.sales / 1000).toFixed(0)}k
                          </span>
                        </div>
                        <div className="w-full bg-gray-200 rounded-full h-2">
                          <div
                            className="bg-blue-600 h-2 rounded-full"
                            style={{ width: `${item.completion}%` }}
                          />
                        </div>
                      </div>
                    </div>
                  ))}
                </div>
              </CardContent>
            </Card>

            <Card>
              <CardHeader>
                <CardTitle>Product Categories</CardTitle>
              </CardHeader>
              <CardContent>
                <div className="space-y-3">
                  {[
                    { category: "Beverages", value: 45, color: "bg-blue-500" },
                    { category: "Snacks", value: 30, color: "bg-green-500" },
                    { category: "Dairy", value: 15, color: "bg-yellow-500" },
                    { category: "Bakery", value: 10, color: "bg-purple-500" },
                  ].map((item, index) => (
                    <div key={index} className="flex items-center gap-3">
                      <div className={`w-3 h-3 rounded-full ${item.color}`} />
                      <div className="flex-1">
                        <div className="flex justify-between mb-1">
                          <span className="text-sm">{item.category}</span>
                          <span className="text-sm font-medium">{item.value}%</span>
                        </div>
                        <div className="w-full bg-gray-200 rounded-full h-1.5">
                          <div
                            className={`${item.color} h-1.5 rounded-full`}
                            style={{ width: `${item.value}%` }}
                          />
                        </div>
                      </div>
                    </div>
                  ))}
                </div>
              </CardContent>
            </Card>
          </div>
        </TabsContent>

        <TabsContent value="sales" className="space-y-4">
          <div className="grid gap-4 md:grid-cols-2">
            <SalesChart data={salesData} loading={loading} chartType="line" />
            <SalesChart data={salesData} loading={loading} chartType="bar" />
          </div>
          {/* ProductScorecard removed - requires product selection to display */}
        </TabsContent>

        <TabsContent value="customers" className="space-y-4">
          {/* CustomerScorecard removed - requires customer selection to display */}
          <div className="grid gap-4 md:grid-cols-2">
            <Card>
              <CardHeader>
                <CardTitle>Customer Segments</CardTitle>
              </CardHeader>
              <CardContent>
                <div className="h-64 flex items-center justify-center text-gray-500">
                  Customer segmentation chart
                </div>
              </CardContent>
            </Card>
            <Card>
              <CardHeader>
                <CardTitle>Visit Frequency</CardTitle>
              </CardHeader>
              <CardContent>
                <div className="h-64 flex items-center justify-center text-gray-500">
                  Visit frequency analysis
                </div>
              </CardContent>
            </Card>
          </div>
        </TabsContent>

        <TabsContent value="operations" className="space-y-4">
          <div className="grid gap-4 md:grid-cols-2">
            <JourneyCompliance 
              salesmen={[{ id: '1', name: 'John Doe' }]} 
              selectedSalesman="1" 
              date={new Date().toLocaleDateString()} 
            />
            <TimeMotionAnalysis 
              salesmen={[{ id: '1', name: 'John Doe' }]} 
              selectedSalesman="1" 
              date={new Date().toLocaleDateString()} 
            />
          </div>
          <Card>
            <CardHeader>
              <CardTitle>Route Efficiency</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="h-64 flex items-center justify-center text-gray-500">
                Route efficiency metrics and map
              </div>
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>
    </div>
  )
}

export default DashboardPage