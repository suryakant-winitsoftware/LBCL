"use client"
import { useState, useEffect } from 'react'
import purchaseOrderService from '../../../services/purchaseOrder'
import useAuthToken from '../../../hooks/useAuthToken'

const PurchaseOrderFetcher = () => {
  const authToken = useAuthToken()
  const [orders, setOrders] = useState([])
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')
  const [totalCount, setTotalCount] = useState(0)

  const fetchPurchaseOrders = async () => {
    try {
      setLoading(true)
      setError('')

      // Check if user is authenticated
      if (!authToken) {
        setError('You must be logged in to view purchase orders')
        console.error('❌ No authentication token available')
        return
      }

      // Request body configuration
      const filters = {
        page: 1,
        pageSize: 10
      }

      console.log('📤 Sending request to fetch purchase orders...')
      console.log('🔐 Using auth token:', authToken?.substring(0, 30) + '...')

      // Call the service method
      const response = await purchaseOrderService.getPurchaseOrderHeaders(filters)

      console.log('📥 Response received:', response)

      if (response.success) {
        // Successfully fetched data
        setOrders(response.headers || [])
        setTotalCount(response.totalCount || 0)

        console.log('✅ Purchase orders fetched successfully!')
        console.log(`📊 Total Count: ${response.totalCount}`)
        console.log(`📋 Orders Retrieved: ${response.headers.length}`)
      } else {
        // Handle API error
        setError(response.error || 'Failed to fetch purchase orders')
        console.error('❌ Error fetching orders:', response.error)
      }
    } catch (err) {
      // Handle unexpected errors
      setError(err.message || 'An unexpected error occurred')
      console.error('💥 Unexpected error:', err)
    } finally {
      setLoading(false)
    }
  }

  // Fetch on component mount when token is available
  useEffect(() => {
    if (authToken) {
      fetchPurchaseOrders()
    }
  }, [authToken])

  return (
    <div className="p-6 bg-white rounded-lg shadow">
      <div className="mb-4">
        <h2 className="text-2xl font-bold text-gray-800">Purchase Orders</h2>
        <p className="text-sm text-gray-600">Total Records: {totalCount}</p>
      </div>

      <button
        onClick={fetchPurchaseOrders}
        disabled={loading}
        className="mb-4 px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700 disabled:bg-gray-400"
      >
        {loading ? 'Loading...' : 'Refresh Data'}
      </button>

      {error && (
        <div className="mb-4 p-3 bg-red-100 border border-red-400 text-red-700 rounded">
          Error: {error}
        </div>
      )}

      {loading ? (
        <div className="flex justify-center items-center py-8">
          <div className="animate-spin rounded-full h-12 w-12 border-t-2 border-b-2 border-blue-500"></div>
        </div>
      ) : (
        <div className="overflow-x-auto">
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Order ID
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Status
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Order Date
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Net Amount
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Organization
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Warehouse
                </th>
              </tr>
            </thead>
            <tbody className="bg-white divide-y divide-gray-200">
              {orders.map((order, index) => (
                <tr key={order.UID || index} className="hover:bg-gray-50">
                  <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
                    {order.UID}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <span
                      className={`px-2 inline-flex text-xs leading-5 font-semibold rounded-full ${
                        order.Status === 'Completed' ? 'bg-green-100 text-green-800' :
                        order.Status === 'Approved' ? 'bg-blue-100 text-blue-800' :
                        order.Status === 'Rejected' ? 'bg-red-100 text-red-800' :
                        order.Status === 'Draft' ? 'bg-gray-100 text-gray-800' :
                        order.Status === 'PendingForApproval' ? 'bg-yellow-100 text-yellow-800' :
                        'bg-purple-100 text-purple-800'
                      }`}
                    >
                      {order.Status}
                    </span>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                    {new Date(order.OrderDate).toLocaleDateString()}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                    ${order.NetAmount?.toFixed(2) || '0.00'}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                    {order.OrgUID}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                    {order.warehouse || order.Warehouse || order.warehouseName || order.WarehouseName || order.warehouse_uid || order.WareHouseUID || order.wareHouseUID || 'N/A'}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>

          {orders.length === 0 && !loading && (
            <div className="text-center py-8 text-gray-500">
              No purchase orders found
            </div>
          )}
        </div>
      )}
    </div>
  )
}

export default PurchaseOrderFetcher