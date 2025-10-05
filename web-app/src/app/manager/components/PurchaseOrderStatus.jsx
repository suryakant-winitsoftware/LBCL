"use client"
import { useState, useEffect } from 'react'
import { useRouter } from 'next/navigation'
import { Filter as FilterIcon, Calendar } from 'lucide-react'
import { Button } from '../../../components/ui/button'
import purchaseOrderService from '../../../services/purchaseOrder'

const PurchaseOrderStatus = () => {
  const router = useRouter()
  const [orders, setOrders] = useState([])
  const [loading, setLoading] = useState(true)
  const [showFilterSection, setShowFilterSection] = useState(false)
  const [activeTab, setActiveTab] = useState('Saved')
  const [totalCount, setTotalCount] = useState(0)
  const [error, setError] = useState('')

  // Filter states
  const [purchaseOrderNo, setPurchaseOrderNo] = useState('')
  const [sapStatus, setSapStatus] = useState('Select')
  const [startDate, setStartDate] = useState('')
  const [endDate, setEndDate] = useState('')

  const tabs = [
    { label: 'Saved', value: 'DRAFT', displayName: 'Draft' },
    { label: 'Confirm', value: 'CONFIRMED', displayName: 'Confirmed' },
    { label: 'Sent', value: 'SENT', displayName: 'Sent' },
    { label: 'Received', value: 'RECEIVED', displayName: 'Received' },
    { label: 'Rejected', value: 'REJECTED', displayName: 'Rejected' },
    { label: 'Error', value: 'ERROR', displayName: 'Error' },
    { label: 'Dispatched', value: 'DISPATCHED', displayName: 'Dispatched' }
  ]

  useEffect(() => {
    fetchOrders()
  }, [activeTab])

  const fetchOrders = async () => {
    try {
      setLoading(true)
      setError('')

      // Map the active tab to backend status value
      const currentTab = tabs.find(t => t.label === activeTab)
      const statusValue = currentTab ? currentTab.value : 'DRAFT'

      const filters = {
        status: statusValue,
        purchaseOrderNo: purchaseOrderNo || undefined,
        sapStatus: sapStatus !== 'Select' ? sapStatus : undefined,
        startDate: startDate || undefined,
        endDate: endDate || undefined
      }

      const response = await purchaseOrderService.getPurchaseOrdersByStatus(statusValue, filters)

      if (response.success) {
        setOrders(response.headers || [])
        setTotalCount(response.totalCount || 0)
      } else {
        setError(response.error || 'Failed to fetch purchase orders')
        setOrders([])
        setTotalCount(0)
      }
    } catch (error) {
      console.error('Error fetching orders:', error)
      setError('Failed to fetch purchase orders')
      setOrders([])
      setTotalCount(0)
    } finally {
      setLoading(false)
    }
  }

  const handleFilter = () => {
    setShowFilterSection(!showFilterSection)
  }

  const handleSearch = () => {
    fetchOrders()
  }

  const handleReset = () => {
    setPurchaseOrderNo('')
    setSapStatus('Select')
    setStartDate('')
    setEndDate('')
    fetchOrders()
  }

  const handleCreateNewOrder = () => {
    router.push('/manager/add-purchase-order')
  }

  const handleDeleteOrder = async (order) => {
    try {
      const confirmDelete = window.confirm(`Are you sure you want to delete Purchase Order ${order.purchaseOrderNo || order.orderNo || order.PurchaseOrderNo || 'this order'}?`)
      if (!confirmDelete) return

      setLoading(true)
      const response = await purchaseOrderService.deletePurchaseOrder(order.uid || order.id)

      if (response.success) {
        alert('Purchase order deleted successfully!')
        fetchOrders() // Refresh the list
      } else {
        alert(response.error || 'Failed to delete purchase order')
      }
    } catch (error) {
      console.error('Error deleting order:', error)
      alert('Failed to delete purchase order')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="min-h-screen bg-gray-50 p-4">
      {/* Header Section */}
      <div className="bg-white rounded-lg shadow-sm p-6 mb-4">
        <div className="flex flex-col space-y-4">
          <div>
            <h1 className="text-2xl font-bold text-blue-700">View Purchase Order Status</h1>
            <div className="flex items-center gap-2 mt-2 text-sm text-gray-600">
              <span>Home</span>
              <span>»</span>
              <span className="text-gray-900">View Purchase Order Status</span>
            </div>
          </div>

          {/* Search Criteria */}
          <div className="flex items-center gap-4 text-sm">
            <span className="font-medium">Search Criteria : Purchase Order No:</span>
            <select className="px-3 py-1 border border-gray-300 rounded bg-white text-sm font-bold">
              <option value="All">All</option>
            </select>
            <span className="ml-4">SAP Status:</span>
            <select className="px-3 py-1 border border-gray-300 rounded bg-white text-sm font-bold">
              <option value="All">All</option>
            </select>
            <span className="ml-4">Start Date:</span>
            <span className="font-bold">09 Sep, 2025</span>
            <span className="ml-4">End Date:</span>
            <span className="font-bold">16 Sep, 2025</span>
            <Button
              onClick={handleFilter}
              className="ml-auto bg-blue-600 hover:bg-blue-700 text-white px-6 py-1.5 flex items-center gap-2"
            >
              <FilterIcon className="w-4 h-4" />
              Filter
            </Button>
          </div>
        </div>
      </div>

      {/* Filter Section - Only show when filter is clicked */}
      {showFilterSection && (
        <div className="bg-white rounded-lg shadow-sm p-4 mb-4">
          <div className="border border-gray-300 rounded p-4">
            <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-4">
              {/* Purchase Order No */}
              <div>
                <label className="block text-sm font-medium mb-2">Purchase Order No</label>
                <input
                  type="text"
                  value={purchaseOrderNo}
                  onChange={(e) => setPurchaseOrderNo(e.target.value)}
                  className="w-full px-3 py-2 border border-gray-300 rounded"
                  placeholder="Enter purchase order number"
                />
              </div>

              {/* SAP Status */}
              <div>
                <label className="block text-sm font-medium mb-2">SAP Status</label>
                <select
                  value={sapStatus}
                  onChange={(e) => setSapStatus(e.target.value)}
                  className="w-full px-3 py-2 border border-gray-300 rounded bg-white"
                >
                  <option key="select" value="Select">Select</option>
                  <option key="saved" value="Saved">Saved</option>
                  <option key="confirmed" value="Confirmed">Confirmed</option>
                  <option key="sent" value="Sent">Sent</option>
                  <option key="received" value="Received">Received</option>
                  <option key="rejected" value="Rejected">Rejected</option>
                  <option key="error" value="Error">Error</option>
                  <option key="dispatched" value="Dispatched">Dispatched</option>
                </select>
              </div>

              {/* Start Date */}
              <div>
                <label className="block text-sm font-medium mb-2">Start Date</label>
                <div className="relative">
                  <input
                    type="date"
                    value={startDate.split('/').reverse().join('-')}
                    onChange={(e) => {
                      const date = new Date(e.target.value)
                      setStartDate(date.toLocaleDateString('en-GB'))
                    }}
                    className="w-full px-3 py-2 border border-gray-300 rounded"
                  />
                  <Calendar className="absolute right-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400 pointer-events-none" />
                </div>
              </div>
            </div>

            {/* End Date */}
            <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-4">
              <div>
                <label className="block text-sm font-medium mb-2">End Date</label>
                <div className="relative">
                  <input
                    type="date"
                    value={endDate.split('/').reverse().join('-')}
                    onChange={(e) => {
                      const date = new Date(e.target.value)
                      setEndDate(date.toLocaleDateString('en-GB'))
                    }}
                    className="w-full px-3 py-2 border border-gray-300 rounded"
                  />
                  <Calendar className="absolute right-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400 pointer-events-none" />
                </div>
              </div>
            </div>

            {/* Search and Reset Buttons */}
            <div className="flex gap-3">
              <Button
                onClick={handleSearch}
                className="bg-blue-600 hover:bg-blue-700 text-white px-6 py-2"
              >
                Search
              </Button>
              <Button
                onClick={handleReset}
                variant="outline"
                className="border-blue-600 text-blue-600 hover:bg-blue-50 px-6 py-2"
              >
                Reset
              </Button>
            </div>
          </div>
        </div>
      )}

      {/* Create New Purchase Order Button */}
      <div className="mb-4">
        <Button
          onClick={handleCreateNewOrder}
          className="bg-blue-600 hover:bg-blue-700 text-white px-4 py-2"
        >
          Create New Purchase Order
        </Button>
      </div>

      {/* Error Message */}
      {error && (
        <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded mb-4">
          <p className="font-medium">Error: {error}</p>
        </div>
      )}

      {/* Total Count */}
      {totalCount > 0 && (
        <div className="text-sm text-gray-600 mb-2 text-right">
          Total Records: {totalCount}
        </div>
      )}

      {/* Status Tabs */}
      <div className="bg-white rounded-lg shadow-sm overflow-hidden">
        <div className="border-b border-gray-200">
          <div className="flex">
            {tabs.map((tab) => (
              <button
                key={tab.label}
                onClick={() => setActiveTab(tab.label)}
                className={`px-6 py-3 text-sm font-medium border-b-2 ${
                  activeTab === tab.label
                    ? 'border-blue-600 text-blue-600 bg-blue-50'
                    : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
                }`}
              >
                {tab.label}
              </button>
            ))}
          </div>
        </div>

        {/* Table */}
        <div className="overflow-x-auto">
          <table className="min-w-full">
            <thead className="bg-gray-600 text-white">
              <tr>
                <th className="px-4 py-2 text-left text-sm font-medium">#</th>
                <th className="px-4 py-2 text-left text-sm font-medium">Purchase Order ID</th>
                <th className="px-4 py-2 text-left text-sm font-medium">Purchase Order No.</th>
                <th className="px-4 py-2 text-left text-sm font-medium">Franchisee</th>
                <th className="px-4 py-2 text-left text-sm font-medium">Order Date</th>
                <th className="px-4 py-2 text-left text-sm font-medium">Requested Delivery Date</th>
                <th className="px-4 py-2 text-right text-sm font-medium">Net Amount ($)</th>
                <th className="px-4 py-2 text-center text-sm font-medium">SAP Status</th>
                <th className="px-4 py-2 text-center text-sm font-medium">Actions</th>
              </tr>
            </thead>
            <tbody className="bg-white">
              {loading ? (
                <tr key="loading">
                  <td colSpan="9" className="px-4 py-8 text-center">
                    <div className="animate-pulse">Loading...</div>
                  </td>
                </tr>
              ) : orders.length === 0 ? (
                <tr key="no-records">
                  <td colSpan="9" className="px-4 py-8 text-center text-gray-500">
                    No record found
                  </td>
                </tr>
              ) : (
                orders.map((order, index) => {
                  // Map API field names (PascalCase from backend) to camelCase
                  const orderDate = order.OrderDate || order.orderDate || order.createDate
                  const requestedDeliveryDate = order.RequestedDeliveryDate || order.requestedDeliveryDate || order.expectedDeliveryDate || order.deliveryDate
                  const netAmount = order.NetAmount || order.netAmount || order.totalAmount || order.amount || 0
                  const orderNumber = order.OrderNumber || order.orderNumber
                  const draftOrderNumber = order.DraftOrderNumber || order.draftOrderNumber
                  const displayOrderNumber = orderNumber || draftOrderNumber || 'N/A'
                  const erpStatus = order.ERPStatus || order.erpStatus || order.sapStatus || 'N/A'

                  return (
                    <tr key={order.uid || order.UID || order.id} className="hover:bg-gray-50 border-b border-gray-200">
                      <td className="px-4 py-3 text-sm">{index + 1}</td>
                      <td className="px-4 py-3 text-sm">
                        {order.UID || order.uid || 'N/A'}
                      </td>
                      <td className="px-4 py-3 text-sm">
                        {displayOrderNumber}
                      </td>
                      <td className="px-4 py-3 text-sm">
                        {order.reportingEmpName || order.ReportingEmpName || order.createdByEmpName || order.CreatedByEmpName || order.franchiseeName || order.FranchiseeName || 'N/A'}
                      </td>
                      <td className="px-4 py-3 text-sm">
                        {orderDate ? new Date(orderDate).toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' }) : 'N/A'}
                      </td>
                      <td className="px-4 py-3 text-sm">
                        {requestedDeliveryDate ? new Date(requestedDeliveryDate).toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' }) : 'N/A'}
                      </td>
                      <td className="px-4 py-3 text-sm text-right">
                        {typeof netAmount === 'number' ? netAmount.toFixed(2) : parseFloat(netAmount || 0).toFixed(2)}
                      </td>
                      <td className="px-4 py-3 text-sm text-center">
                        {erpStatus}
                      </td>
                      <td className="px-4 py-3 text-sm text-center">
                        <div className="flex items-center justify-center gap-1">
                          <button
                            onClick={() => router.push(`/manager/purchase-order-details/${order.uid || order.id}`)}
                            className="w-8 h-8 flex items-center justify-center bg-blue-600 text-white rounded hover:bg-blue-700"
                            title="View Details"
                          >
                            👁️
                          </button>
                        </div>
                      </td>
                    </tr>
                  )
                })
              )}
            </tbody>
          </table>
        </div>

        {/* Pagination */}
        {orders.length > 0 && (
          <div className="mt-4 flex justify-end items-center gap-2">
            <span className="text-sm text-gray-600">Records: 1 - 1 of 1</span>
            <div className="flex gap-1">
              <button className="px-3 py-1 border border-gray-300 rounded hover:bg-gray-50 text-sm">«</button>
              <button className="px-3 py-1 border border-gray-300 rounded hover:bg-gray-50 text-sm">‹</button>
              <button className="px-3 py-1 border border-blue-600 bg-blue-600 text-white rounded text-sm">1</button>
              <button className="px-3 py-1 border border-gray-300 rounded hover:bg-gray-50 text-sm">›</button>
              <button className="px-3 py-1 border border-gray-300 rounded hover:bg-gray-50 text-sm">»</button>
            </div>
          </div>
        )}
      </div>
    </div>
  )
}

export default PurchaseOrderStatus