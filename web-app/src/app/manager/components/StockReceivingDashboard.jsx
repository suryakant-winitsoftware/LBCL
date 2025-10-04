"use client"
import { useState, useEffect } from 'react'
import { useRouter } from 'next/navigation'
import {
  FileText,
  Eye,
  Package,
  TrendingUp,
  Clock,
  CheckCircle,
  AlertTriangle,
  Download,
  RefreshCw
} from 'lucide-react'
import BusinessLayout from '../../../components/layouts/BusinessLayout'
import DashboardMetrics from '../../../components/dashboard/DashboardMetrics'
import DataTable from '../../../components/ui/DataTable'
import { Button } from '../../../components/ui/button'
import { useAuth } from '../../../contexts/AuthContext'
import ProtectedRoute from '../../../components/ProtectedRoute'
import purchaseOrderService from '../../../services/purchaseOrder'

const StockReceivingDashboard = () => {
  const { user } = useAuth()
  const router = useRouter()
  const [stockData, setStockData] = useState({})
  const [purchaseOrders, setPurchaseOrders] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [totalCount, setTotalCount] = useState(0)

  useEffect(() => {
    fetchData()
  }, [])

  const fetchData = async () => {
    try {
      setLoading(true)
      setError('')

      // Fetch purchase orders using the API
      const response = await purchaseOrderService.getPurchaseOrderHeaders({
        page: 1,
        pageSize: 50
      })

      if (response.success) {
        const orders = response.headers || []
        setPurchaseOrders(orders)
        setTotalCount(response.totalCount || 0)

        // Transform purchase orders to stock receiving format for compatibility
        const transformedData = {
          stockReceivingHistory: orders.map((order, index) => {
            // Get approval employees
            const approvalEmployees = [
              order.App1EmpUID, order.App2EmpUID, order.App3EmpUID,
              order.App4EmpUID, order.App5EmpUID, order.App6EmpUID
            ].filter(emp => emp && emp !== null && emp !== '');

            return {
              id: order.UID || index + 1,
              agentStockReceivingNo: order.UID || `PO-${index + 1}`,
              sapDeliveryNoteNo: order.OrderNumber || order.UID || 'N/A',
              date: order.OrderDate ? new Date(order.OrderDate).toLocaleDateString() : 'N/A',
              time: order.OrderDate ? new Date(order.OrderDate).toLocaleTimeString() : '00:00',
              primeMover: order.ChannelPartnerName || order.OrgUID || 'N/A',
              totalTimeTaken: 'N/A',
              status: mapPOStatusToStockStatus(order.Status),
              originalStatus: order.Status,
              franchisee: order.ChannelPartnerName || order.OrgUID || 'N/A',
              amount: order.NetAmount || 0,
              warehouse: order.warehouse || order.Warehouse || order.warehouse_uid || order.WareHouseUID || order.wareHouseUID || 'N/A',
              organization: order.OrgUID || 'N/A',
              deliveryDate: order.RequestedDeliveryDate && order.RequestedDeliveryDate !== '0001-01-01T00:00:00'
                ? new Date(order.RequestedDeliveryDate).toLocaleDateString()
                : 'Not Set',
              hasTemplate: order.HasTemplate || false,
              approvals: approvalEmployees.length > 0 ? `${approvalEmployees.length} Approved` : 'No Approvals',
              approvalEmployees: approvalEmployees,
              serialNumber: order.SerialNumber || 0,
              cpeConfirmDateTime: order.CPEConfirmDateTime && order.CPEConfirmDateTime !== '0001-01-01T00:00:00'
                ? new Date(order.CPEConfirmDateTime).toLocaleDateString()
                : 'Not Confirmed',
              channelPartnerCode: order.ChannelPartnerCode || 'N/A',
              channelPartnerName: order.ChannelPartnerName || 'N/A'
            }
          }),
          deliveries: orders.filter(order =>
            order.sapStatus === 'Dispatched' || order.status === 'Dispatched'
          ).map(order => ({
            id: order.uid || order.id,
            deliveryPlan: order.purchaseOrderNo || 'N/A',
            deliveryNote: order.sapOrderNo || 'N/A',
            date: order.requestedDeliveryDate || order.RequestedDeliveryDate || 'N/A',
            items: order.itemCount || order.productCount || '0',
            status: order.sapStatus === 'Dispatched' ? 'pending' : 'completed'
          }))
        }

        setStockData(transformedData)
      } else {
        setError(response.error || 'Failed to fetch purchase order data')
        setStockData({
          stockReceivingHistory: [],
          deliveries: []
        })
      }
    } catch (error) {
      console.error('Error fetching data:', error)
      setError('Failed to fetch purchase order data')
      setStockData({
        stockReceivingHistory: [],
        deliveries: []
      })
    } finally {
      setLoading(false)
    }
  }

  // Helper function to map PO status to stock receiving status
  const mapPOStatusToStockStatus = (poStatus) => {
    const statusMap = {
      'Draft': 'pending',
      'PendingForApproval': 'pending',
      'Approved': 'pending',
      'InProcessERP': 'pending',
      'Completed': 'completed',
      'Rejected': 'cancelled'
    }
    return statusMap[poStatus] || 'pending'
  }

  const handleActivityLog = (item) => {
    router.push(`/user/manager/activity-log-report/${item.sapDeliveryNoteNo || item.id}`)
  }

  const handleViewDetails = (item) => {
    router.push('/user/manager/physical-count')
  }

  // Format stock receiving history data for table
  const stockHistoryData = stockData.stockReceivingHistory || []
  const pendingDeliveries = stockData.deliveries?.filter(d => d.status === 'pending') || []
  const completedToday = stockHistoryData.filter(item => 
    item.status === 'completed' && new Date(item.date).toDateString() === new Date().toDateString()
  ).length
  
  // Comprehensive columns for purchase order table
  const columns = [
    {
      key: 'agentStockReceivingNo',
      title: 'PO UID',
      className: 'min-w-[180px]',
      render: (value) => (
        <div className="font-mono text-sm text-[var(--foreground)] font-semibold">{value}</div>
      )
    },
    {
      key: 'sapDeliveryNoteNo',
      title: 'Order Number',
      className: 'min-w-[140px]',
      render: (value) => (
        <div className="text-sm font-medium text-[var(--primary)]">{value}</div>
      )
    },
    {
      key: 'date',
      title: 'Order Date',
      className: 'min-w-[120px]',
      render: (value) => (
        <div className="text-sm text-[var(--foreground)]">{value}</div>
      )
    },
    {
      key: 'amount',
      title: 'Net Amount',
      className: 'min-w-[120px]',
      render: (value) => (
        <div className="text-sm font-semibold text-[var(--foreground)]">
          ${Number(value).toFixed(2)}
        </div>
      )
    },
    {
      key: 'warehouse',
      title: 'Warehouse',
      className: 'min-w-[100px]',
      render: (value) => (
        <div className="text-sm text-[var(--foreground)] bg-gray-100 px-2 py-1 rounded text-center">{value}</div>
      )
    },
    {
      key: 'organization',
      title: 'Organization',
      className: 'min-w-[100px]',
      render: (value) => (
        <div className="text-sm text-[var(--foreground)] bg-blue-100 px-2 py-1 rounded text-center">{value}</div>
      )
    },
    {
      key: 'deliveryDate',
      title: 'Requested Delivery',
      className: 'min-w-[140px]',
      render: (value) => (
        <div className="text-sm text-[var(--foreground)]">{value}</div>
      )
    },
    {
      key: 'cpeConfirmDateTime',
      title: 'CPE Confirmation',
      className: 'min-w-[140px]',
      render: (value) => (
        <div className="text-sm text-[var(--foreground)]">
          {value === 'Not Confirmed' ? (
            <span className="text-gray-500 italic">{value}</span>
          ) : (
            <span>{value}</span>
          )}
        </div>
      )
    },
    {
      key: 'approvalEmployees',
      title: 'Approval Employees',
      className: 'min-w-[200px]',
      render: (value) => (
        <div className="text-xs">
          {value && value.length > 0 ? (
            <div className="flex flex-wrap gap-1">
              {value.map((emp, index) => (
                <span key={index} className="bg-blue-100 text-blue-800 px-2 py-1 rounded text-xs">
                  {emp}
                </span>
              ))}
            </div>
          ) : (
            <span className="text-gray-500 italic">No Approvals</span>
          )}
        </div>
      )
    },
    {
      key: 'hasTemplate',
      title: 'Template',
      className: 'min-w-[80px]',
      render: (value) => (
        <div className="text-xs">
          {value ? (
            <span className="bg-green-100 text-green-800 px-2 py-1 rounded-full">Yes</span>
          ) : (
            <span className="bg-gray-100 text-gray-800 px-2 py-1 rounded-full">No</span>
          )}
        </div>
      )
    },
    {
      key: 'status',
      title: 'Status',
      className: 'min-w-[130px]',
      render: (value, row) => {
        const getStatusColor = (status) => {
          switch(status) {
            case 'completed': return 'bg-green-100 text-green-800 border-green-200'
            case 'pending': return 'bg-yellow-100 text-yellow-800 border-yellow-200'
            case 'cancelled': return 'bg-red-100 text-red-800 border-red-200'
            default: return 'bg-gray-100 text-gray-800 border-gray-200'
          }
        }
        return (
          <span className={`inline-flex px-2 py-1 text-xs font-medium rounded-full border ${getStatusColor(value)}`}>
            {row.originalStatus || value}
          </span>
        )
      }
    }
  ]

  const actions = [
    {
      icon: Eye,
      title: 'View Activity Log',
      onClick: handleActivityLog,
      className: 'text-[var(--muted-foreground)] hover:text-[var(--primary)]'
    },
    {
      icon: Package,
      title: 'View Physical Count',
      onClick: handleViewDetails,
      className: 'text-[var(--muted-foreground)] hover:text-[var(--primary)]'
    },
    {
      icon: Download,
      title: 'Download Report',
      onClick: (row) => console.log('Download report:', row.id),
      className: 'text-[var(--muted-foreground)] hover:text-[var(--primary)]'
    }
  ]

  return (
    <ProtectedRoute requiredSystem="stock">
      <BusinessLayout title="Stock Receiving Management System">
        <div className="min-h-screen bg-[var(--background)] p-4 sm:p-6 overflow-hidden max-w-full">
          {/* Dashboard Header */}
          <div className="bg-[var(--card)] rounded-lg shadow-sm border border-[var(--border)] p-4 sm:p-6 mb-4 sm:mb-6">
            <div className="flex flex-col lg:flex-row lg:items-center lg:justify-between gap-4">
              <div>
                <h1 className="text-xl sm:text-2xl font-bold text-[var(--foreground)] mb-2">Stock Receiving Dashboard</h1>
                <p className="text-[var(--muted-foreground)] text-sm sm:text-base">Monitor purchase orders and stock receiving operations</p>
              </div>
              <div className="flex items-center gap-2 sm:gap-4">
                <Button 
                  onClick={() => router.push('/user/manager/physical-count')}
                  className="px-3 sm:px-4 py-2 bg-[var(--primary)] hover:bg-[var(--primary-hover)] text-[var(--primary-foreground)] text-sm sm:text-base"
                >
                  <Package className="w-4 h-4 mr-2" />
                  New Physical Count
                </Button>
              </div>
            </div>
          </div>

          {/* Error Message */}
          {error && (
            <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-lg mb-4">
              <p className="font-medium">Error: {error}</p>
              <p className="text-sm mt-1">Unable to fetch purchase order data. Please check your API connection.</p>
            </div>
          )}

          {/* Key Metrics */}
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 sm:gap-6 mb-4 sm:mb-6">
            <div className="bg-[var(--card)] rounded-lg shadow-sm border border-[var(--border)] p-4 sm:p-6">
              <div className="flex items-center">
                <div className="w-10 h-10 sm:w-12 sm:h-12 bg-[var(--primary-light)] rounded-lg flex items-center justify-center">
                  <Package className="w-5 h-5 sm:w-6 sm:h-6 text-[var(--primary)]" />
                </div>
                <div className="ml-3 sm:ml-4">
                  <p className="text-xs sm:text-sm font-medium text-[var(--muted-foreground)]">Total Records</p>
                  <p className="text-xl sm:text-2xl font-bold text-[var(--foreground)]">{stockHistoryData.length}</p>
                </div>
              </div>
            </div>

            <div className="bg-[var(--card)] rounded-lg shadow-sm border border-[var(--border)] p-4 sm:p-6">
              <div className="flex items-center">
                <div className="w-10 h-10 sm:w-12 sm:h-12 bg-[var(--success-light)] rounded-lg flex items-center justify-center">
                  <CheckCircle className="w-5 h-5 sm:w-6 sm:h-6 text-[var(--primary)]" />
                </div>
                <div className="ml-3 sm:ml-4">
                  <p className="text-xs sm:text-sm font-medium text-[var(--muted-foreground)]">Completed Today</p>
                  <p className="text-xl sm:text-2xl font-bold text-[var(--foreground)]">{completedToday}</p>
                </div>
              </div>
            </div>

            <div className="bg-[var(--card)] rounded-lg shadow-sm border border-[var(--border)] p-4 sm:p-6">
              <div className="flex items-center">
                <div className="w-10 h-10 sm:w-12 sm:h-12 bg-[var(--warning-light)] rounded-lg flex items-center justify-center">
                  <Clock className="w-5 h-5 sm:w-6 sm:h-6 text-[var(--warning)]" />
                </div>
                <div className="ml-3 sm:ml-4">
                  <p className="text-xs sm:text-sm font-medium text-[var(--muted-foreground)]">Pending Deliveries</p>
                  <p className="text-xl sm:text-2xl font-bold text-[var(--foreground)]">{pendingDeliveries.length}</p>
                </div>
              </div>
            </div>

            <div className="bg-[var(--card)] rounded-lg shadow-sm border border-[var(--border)] p-4 sm:p-6">
              <div className="flex items-center">
                <div className="w-10 h-10 sm:w-12 sm:h-12 bg-[var(--primary-light)] rounded-lg flex items-center justify-center">
                  <TrendingUp className="w-5 h-5 sm:w-6 sm:h-6 text-[var(--primary)]" />
                </div>
                <div className="ml-3 sm:ml-4">
                  <p className="text-xs sm:text-sm font-medium text-[var(--muted-foreground)]">Avg. Processing Time</p>
                  <p className="text-xl sm:text-2xl font-bold text-[var(--foreground)]">24m</p>
                </div>
              </div>
            </div>
          </div>

          {/* Stock Receiving History Table */}
          <div className="bg-[var(--card)] rounded-lg shadow-sm border border-[var(--border)] overflow-hidden">
            <div className="p-4 sm:p-6 border-b border-[var(--border)]">
              <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
                <div className="flex items-center gap-2 sm:gap-3">
                  <FileText className="w-4 h-4 sm:w-5 sm:h-5 text-[var(--foreground)]" />
                  <h2 className="text-base sm:text-lg font-semibold text-[var(--foreground)]">Recent Purchase Orders</h2>
                </div>
                <div className="flex gap-2">
                  <Button
                    variant="outline"
                    onClick={fetchData}
                    disabled={loading}
                    className="px-3 sm:px-4 py-2 border-[var(--border)] text-[var(--foreground)] hover:bg-[var(--muted)] text-sm sm:text-base"
                  >
                    <RefreshCw className={`w-4 h-4 mr-2 ${loading ? 'animate-spin' : ''}`} />
                    Refresh
                  </Button>
                  <Button
                    variant="outline"
                    onClick={() => router.push('/user/manager/purchase-order-status')}
                    className="px-3 sm:px-4 py-2 border-[var(--border)] text-[var(--foreground)] hover:bg-[var(--muted)] text-sm sm:text-base"
                  >
                    View All Orders
                  </Button>
                </div>
              </div>
            </div>
            {/* Responsive Stock History */}
            <div className="p-4 sm:p-6">
              {loading ? (
                <div className="space-y-4">
                  {[...Array(5)].map((_, i) => (
                    <div key={i} className="animate-pulse">
                      <div className="h-16 bg-[var(--muted)] rounded-lg"></div>
                    </div>
                  ))}
                </div>
              ) : stockHistoryData.length === 0 ? (
                <div className="text-center py-12">
                  <Package className="mx-auto h-12 w-12 text-[var(--muted-foreground)]" />
                  <h3 className="mt-2 text-sm font-medium text-[var(--foreground)]">No records</h3>
                  <p className="mt-1 text-sm text-[var(--muted-foreground)]">No purchase orders found. Create a purchase order to see it here.</p>
                  <Button
                    onClick={() => router.push('/user/manager/add-purchase-order')}
                    className="mt-4 px-4 py-2 bg-[var(--primary)] hover:bg-[var(--primary-hover)] text-[var(--primary-foreground)]"
                  >
                    Create Purchase Order
                  </Button>
                </div>
              ) : (
                <>
                  {/* Mobile Card Layout */}
                  <div className="sm:hidden space-y-4">
                    {stockHistoryData.map((item, index) => (
                      <div
                        key={index}
                        onClick={() => handleActivityLog(item)}
                        className="bg-[var(--card)] border border-[var(--border)] rounded-lg p-4 cursor-pointer hover:bg-[var(--muted)] transition-colors"
                      >
                        <div className="flex items-start justify-between mb-3">
                          <div>
                            <div className="font-mono text-sm font-semibold text-[var(--foreground)]">
                              {item.agentStockReceivingNo}
                            </div>
                            <div className="text-sm font-medium text-[var(--primary)] mt-1">
                              {item.sapDeliveryNoteNo}
                            </div>
                          </div>
                          <span className={`inline-flex px-2 py-1 text-xs font-medium rounded-full border ${
                            item.status === 'completed'
                              ? 'bg-[var(--success-light)] text-[var(--success)] border-[var(--success)]'
                              : 'bg-[var(--warning-light)] text-[var(--warning)] border-[var(--warning)]'
                          }`}>
                            {item.status === 'completed' ? 'Done' : 'Progress'}
                          </span>
                        </div>
                        <div className="flex items-center justify-between text-sm text-[var(--muted-foreground)]">
                          <span>{item.date} • {item.time}</span>
                          <span>{item.totalTimeTaken}</span>
                        </div>
                        <div className="flex items-center gap-2 mt-3 pt-3 border-t border-[var(--border)]">
                          <Button
                            onClick={(e) => {
                              e.stopPropagation()
                              handleActivityLog(item)
                            }}
                            variant="ghost"
                            size="sm"
                            className="h-8 px-2"
                          >
                            <Eye className="w-4 h-4" />
                          </Button>
                          <Button
                            onClick={(e) => {
                              e.stopPropagation()
                              handleViewDetails(item)
                            }}
                            variant="ghost"
                            size="sm"
                            className="h-8 px-2"
                          >
                            <Package className="w-4 h-4" />
                          </Button>
                        </div>
                      </div>
                    ))}
                  </div>

                  {/* Desktop Table Layout */}
                  <div className="hidden sm:block overflow-hidden">
                    <table className="w-full">
                      <thead className="bg-[var(--muted)]">
                        <tr>
                          <th className="text-left py-3 px-3 text-xs font-medium text-[var(--muted-foreground)] uppercase">ASR No</th>
                          <th className="text-left py-3 px-3 text-xs font-medium text-[var(--muted-foreground)] uppercase">SAP Note</th>
                          <th className="text-left py-3 px-3 text-xs font-medium text-[var(--muted-foreground)] uppercase hidden sm:table-cell">Date</th>
                          <th className="text-left py-3 px-3 text-xs font-medium text-[var(--muted-foreground)] uppercase hidden md:table-cell">Prime Mover</th>
                          <th className="text-left py-3 px-3 text-xs font-medium text-[var(--muted-foreground)] uppercase">Status</th>
                          <th className="text-left py-3 px-3 text-xs font-medium text-[var(--muted-foreground)] uppercase">Actions</th>
                        </tr>
                      </thead>
                      <tbody className="divide-y divide-[var(--border)]">
                        {stockHistoryData.map((item, index) => (
                          <tr
                            key={index}
                            onClick={() => handleActivityLog(item)}
                            className="hover:bg-[var(--muted)] cursor-pointer transition-colors"
                          >
                            <td className="py-3 px-3">
                              <div className="font-mono text-sm font-semibold text-[var(--foreground)]">
                                {item.agentStockReceivingNo}
                              </div>
                            </td>
                            <td className="py-3 px-3">
                              <div className="text-sm font-medium text-[var(--primary)]">
                                {item.sapDeliveryNoteNo}
                              </div>
                            </td>
                            <td className="py-3 px-3 hidden sm:table-cell">
                              <div className="text-sm text-[var(--foreground)]">
                                {item.date}
                              </div>
                            </td>
                            <td className="py-3 px-3 hidden md:table-cell">
                              <div className="text-sm text-[var(--foreground)]">
                                {item.primeMover}
                              </div>
                            </td>
                            <td className="py-3 px-3">
                              <span className={`inline-flex px-2 py-1 text-xs font-medium rounded-full border ${
                                item.status === 'completed'
                                  ? 'bg-[var(--success-light)] text-[var(--success)] border-[var(--success)]'
                                  : 'bg-[var(--warning-light)] text-[var(--warning)] border-[var(--warning)]'
                              }`}>
                                {item.status === 'completed' ? 'Done' : 'Progress'}
                              </span>
                            </td>
                            <td className="py-3 px-3">
                              <div className="flex items-center gap-1">
                                <Button
                                  onClick={(e) => {
                                    e.stopPropagation()
                                    handleActivityLog(item)
                                  }}
                                  variant="ghost"
                                  size="sm"
                                  className="h-8 w-8 p-0"
                                >
                                  <Eye className="w-4 h-4" />
                                </Button>
                                <Button
                                  onClick={(e) => {
                                    e.stopPropagation()
                                    handleViewDetails(item)
                                  }}
                                  variant="ghost"
                                  size="sm"
                                  className="h-8 w-8 p-0"
                                >
                                  <Package className="w-4 h-4" />
                                </Button>
                              </div>
                            </td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>
                </>
              )}
            </div>
          </div>
        </div>
      </BusinessLayout>
    </ProtectedRoute>
  )
}

export default StockReceivingDashboard