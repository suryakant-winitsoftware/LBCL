"use client"
import { useEffect, useState } from 'react'
import { useRouter } from 'next/navigation'
import { ArrowLeft } from 'lucide-react'
import { Button } from '../../../components/ui/button'
import BusinessLayout from '../../../components/layouts/BusinessLayout'
import purchaseOrderService from '../../../services/purchaseOrder'

const PurchaseOrderDetails = ({ uid }) => {
  const router = useRouter()
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [orderData, setOrderData] = useState(null)

  useEffect(() => {
    if (uid) {
      fetchOrderDetails()
    }
  }, [uid])

  const fetchOrderDetails = async () => {
    try {
      setLoading(true)
      setError('')

      const response = await purchaseOrderService.getPurchaseOrderMasterByUID(uid)

      if (response.success && response.master) {
        console.log('📦 Fetched order data:', response.master)
        setOrderData(response.master)
      } else {
        setError(response.error || 'Failed to fetch purchase order details')
      }
    } catch (err) {
      console.error('Error fetching order details:', err)
      setError('Failed to fetch purchase order details')
    } finally {
      setLoading(false)
    }
  }

  const getStatusBadgeClass = (status) => {
    const statusColors = {
      'CONFIRMED': 'bg-green-100 text-green-800 border-green-200',
      'DRAFT': 'bg-gray-100 text-gray-800 border-gray-200',
      'SENT': 'bg-blue-100 text-blue-800 border-blue-200',
      'RECEIVED': 'bg-purple-100 text-purple-800 border-purple-200',
      'REJECTED': 'bg-red-100 text-red-800 border-red-200',
      'ERROR': 'bg-orange-100 text-orange-800 border-orange-200',
      'DISPATCHED': 'bg-indigo-100 text-indigo-800 border-indigo-200'
    }
    return statusColors[status] || 'bg-gray-100 text-gray-800 border-gray-200'
  }

  const formatDate = (dateValue) => {
    if (!dateValue) return 'N/A'
    try {
      return new Date(dateValue).toLocaleDateString('en-US', {
        year: 'numeric',
        month: 'short',
        day: 'numeric'
      })
    } catch {
      return 'N/A'
    }
  }

  const formatCurrency = (value) => {
    if (!value && value !== 0) return '$0.00'
    return `$${parseFloat(value).toFixed(2)}`
  }

  if (loading) {
    return (
      <BusinessLayout title="Purchase Order Details">
        <div className="flex items-center justify-center min-h-screen">
          <div className="text-lg">Loading purchase order details...</div>
        </div>
      </BusinessLayout>
    )
  }

  if (error) {
    return (
      <BusinessLayout title="Purchase Order Details">
        <div className="p-6">
          <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded mb-4">
            {error}
          </div>
          <Button
            onClick={() => router.back()}
            className="flex items-center gap-2"
          >
            <ArrowLeft className="w-4 h-4" />
            Go Back
          </Button>
        </div>
      </BusinessLayout>
    )
  }

  if (!orderData || !orderData.PurchaseOrderHeader) {
    return (
      <BusinessLayout title="Purchase Order Details">
        <div className="p-6">
          <div className="bg-yellow-50 border border-yellow-200 text-yellow-700 px-4 py-3 rounded mb-4">
            Purchase order not found
          </div>
          <Button
            onClick={() => router.back()}
            className="flex items-center gap-2"
          >
            <ArrowLeft className="w-4 h-4" />
            Go Back
          </Button>
        </div>
      </BusinessLayout>
    )
  }

  const header = orderData.PurchaseOrderHeader
  const lines = orderData.PurchaseOrderLines || []

  return (
    <BusinessLayout title="Purchase Order Details">
      <div className="p-6">
        {/* Header Section */}
        <div className="flex justify-between items-center mb-6">
          <div>
            <h1 className="text-2xl font-bold text-gray-900">Purchase Order Details</h1>
            <p className="text-sm text-gray-500 mt-1">
              {header.OrderNumber || header.DraftOrderNumber || 'Draft Order'}
            </p>
          </div>
          <Button
            onClick={() => router.back()}
            className="flex items-center gap-2"
          >
            <ArrowLeft className="w-4 h-4" />
            Back to Orders
          </Button>
        </div>

        {/* Order Information Card */}
        <div className="bg-white shadow-md rounded-lg p-6 mb-6">
          <h2 className="text-lg font-semibold text-gray-900 mb-4">Order Information</h2>

          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            <div>
              <label className="block text-sm font-medium text-gray-600 mb-1">Order Number</label>
              <p className="text-sm text-gray-900 font-medium">
                {header.OrderNumber || 'Not assigned'}
              </p>
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-600 mb-1">Draft Order Number</label>
              <p className="text-sm text-gray-900">
                {header.DraftOrderNumber || 'N/A'}
              </p>
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-600 mb-1">Status</label>
              <span className={`inline-flex px-3 py-1 text-xs font-semibold rounded-full border ${getStatusBadgeClass(header.Status)}`}>
                {header.Status || 'UNKNOWN'}
              </span>
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-600 mb-1">Order Date</label>
              <p className="text-sm text-gray-900">
                {formatDate(header.OrderDate)}
              </p>
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-600 mb-1">Expected Delivery Date</label>
              <p className="text-sm text-gray-900">
                {formatDate(header.RequestedDeliveryDate || header.ExpectedDeliveryDate)}
              </p>
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-600 mb-1">Warehouse</label>
              <p className="text-sm text-gray-900">
                {header.WarehouseName || header.WareHouseUID || 'N/A'}
              </p>
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-600 mb-1">ERP Status</label>
              <p className="text-sm text-gray-900">
                {header.ERPStatus || header.OracleOrderStatus || 'N/A'}
              </p>
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-600 mb-1">Line Count</label>
              <p className="text-sm text-gray-900 font-medium">
                {header.LineCount || lines.length || '0'}
              </p>
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-600 mb-1">Quantity Count</label>
              <p className="text-sm text-gray-900 font-medium">
                {header.QtyCount || '0'}
              </p>
            </div>
          </div>
        </div>

        {/* Financial Information Card */}
        <div className="bg-white shadow-md rounded-lg p-6 mb-6">
          <h2 className="text-lg font-semibold text-gray-900 mb-4">Financial Information</h2>

          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
            <div>
              <label className="block text-sm font-medium text-gray-600 mb-1">Total Amount</label>
              <p className="text-lg text-gray-900 font-bold">
                {formatCurrency(header.TotalAmount)}
              </p>
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-600 mb-1">Total Discount</label>
              <p className="text-lg text-green-600 font-semibold">
                {formatCurrency(header.TotalDiscount)}
              </p>
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-600 mb-1">Total Tax</label>
              <p className="text-lg text-gray-900 font-semibold">
                {formatCurrency(header.TotalTaxAmount)}
              </p>
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-600 mb-1">Net Amount</label>
              <p className="text-lg text-blue-600 font-bold">
                {formatCurrency(header.NetAmount)}
              </p>
            </div>
          </div>
        </div>

        {/* Order Lines Section */}
        {lines && lines.length > 0 && (
          <div className="bg-white shadow-md rounded-lg p-6">
            <h2 className="text-lg font-semibold text-gray-900 mb-4">Order Lines ({lines.length})</h2>

            <div className="overflow-x-auto">
              <table className="min-w-full divide-y divide-gray-200">
                <thead className="bg-gray-50">
                  <tr>
                    <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      SKU Code
                    </th>
                    <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Product Name
                    </th>
                    <th className="px-4 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Quantity
                    </th>
                    <th className="px-4 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Unit Price
                    </th>
                    <th className="px-4 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Discount
                    </th>
                    <th className="px-4 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Tax
                    </th>
                    <th className="px-4 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Total Amount
                    </th>
                  </tr>
                </thead>
                <tbody className="bg-white divide-y divide-gray-200">
                  {lines.map((line, index) => (
                    <tr key={index} className="hover:bg-gray-50">
                      <td className="px-4 py-3 text-sm text-gray-900">
                        {line.SKUCode || 'N/A'}
                      </td>
                      <td className="px-4 py-3 text-sm text-gray-900">
                        {line.ProductName || line.SKUName || 'N/A'}
                      </td>
                      <td className="px-4 py-3 text-sm text-gray-900 text-right font-medium">
                        {line.Quantity || '0'}
                      </td>
                      <td className="px-4 py-3 text-sm text-gray-900 text-right">
                        {formatCurrency(line.UnitPrice)}
                      </td>
                      <td className="px-4 py-3 text-sm text-green-600 text-right">
                        {formatCurrency(line.DiscountAmount)}
                      </td>
                      <td className="px-4 py-3 text-sm text-gray-900 text-right">
                        {formatCurrency(line.TaxAmount)}
                      </td>
                      <td className="px-4 py-3 text-sm text-gray-900 text-right font-semibold">
                        {formatCurrency(line.TotalAmount || line.NetAmount)}
                      </td>
                    </tr>
                  ))}
                </tbody>
                <tfoot className="bg-gray-50">
                  <tr>
                    <td colSpan="6" className="px-4 py-3 text-sm font-semibold text-gray-900 text-right">
                      Grand Total:
                    </td>
                    <td className="px-4 py-3 text-sm font-bold text-blue-600 text-right">
                      {formatCurrency(header.NetAmount)}
                    </td>
                  </tr>
                </tfoot>
              </table>
            </div>
          </div>
        )}

        {/* Empty State for No Lines */}
        {(!lines || lines.length === 0) && (
          <div className="bg-white shadow-md rounded-lg p-6">
            <div className="text-center py-8">
              <p className="text-gray-500">No order lines found for this purchase order.</p>
            </div>
          </div>
        )}
      </div>
    </BusinessLayout>
  )
}

export default PurchaseOrderDetails
