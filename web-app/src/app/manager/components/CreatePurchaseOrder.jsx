"use client"
import { useState, useEffect } from 'react'
import { useRouter, useParams } from 'next/navigation'
import { Search, Calendar, X, ArrowLeft, Home } from 'lucide-react'
import { Button } from '../../../components/ui/button'
import purchaseOrderService from '../../../services/purchaseOrder'
import skuService from '../../../services/sku'
import warehouseService from '../../../services/warehouse'

const CreatePurchaseOrder = () => {
  const router = useRouter()
  const params = useParams()
  const templateId = params?.templateId

  // State management
  const [warehouses, setWarehouses] = useState([])
  const [products, setProducts] = useState([])
  const [loading, setLoading] = useState(true)
  const [loadingProducts, setLoadingProducts] = useState(false)
  const [error, setError] = useState('')

  const [orderData, setOrderData] = useState({
    plant: '',
    orderDate: new Date().toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' }),
    requestedBy: '',
    requestedByFranchisee: {
      name: '',
      poBox: '',
      city: '',
      region: '',
      country: '',
      postalCode: '',
      telephone: '',
      mobile: '',
      address: ''
    },
    requestedDeliveryDate: '',
    preparedBy: ''
  })

  // Load data on component mount
  useEffect(() => {
    fetchWarehouses()
  }, [])

  // Load products when plant/warehouse is selected
  useEffect(() => {
    if (orderData.plant && orderData.plant !== 'Select' && orderData.plant !== '') {
      fetchProductsForPlant(orderData.plant)
    }
  }, [orderData.plant])

  const fetchWarehouses = async () => {
    try {
      setLoading(true)
      setError('')

      console.log('🔄 Fetching warehouses from Store API...')
      const response = await warehouseService.getWarehouseDropdownOptions({
        pageSize: 100,
        page: 1
      })

      if (response.success) {
        console.log('✅ Warehouses fetched successfully:', response.options)
        setWarehouses(response.options || [])
      } else {
        console.error('❌ Failed to fetch warehouses:', response.error)
        setError(response.error || 'Failed to fetch warehouses')
      }
    } catch (error) {
      console.error('Error fetching warehouses:', error)
      setError('Failed to fetch warehouses')
    } finally {
      setLoading(false)
    }
  }

  const fetchProductsForPlant = async (plantUID) => {
    try {
      setLoadingProducts(true)

      console.log('🔄 Fetching products for plant:', plantUID)
      const response = await skuService.getProductsForStore(plantUID, {
        pageSize: 100,
        page: 1
      })

      if (response.success) {
        console.log('✅ Products fetched successfully:', response.products)
        // Add UI state for product selection
        const productsWithState = (response.products || []).map(product => ({
          ...product,
          checked: false,
          orderQty: 0,
          value: 0
        }))
        setProducts(productsWithState)
      } else {
        console.error('❌ Failed to fetch products:', response.error)
        setError(response.error || 'Failed to fetch products')
        setProducts([])
      }
    } catch (error) {
      console.error('Error fetching products for plant:', error)
      setError('Failed to fetch products')
      setProducts([])
    } finally {
      setLoadingProducts(false)
    }
  }

  // Additional state
  const [selectedProducts, setSelectedProducts] = useState([])
  const [searchQuery, setSearchQuery] = useState('')
  const [saving, setSaving] = useState(false)

  useEffect(() => {
    fetchTemplateData()
  }, [templateId])

  // Function to handle plant selection
  const handlePlantChange = (selectedPlant) => {
    setOrderData(prev => ({ ...prev, plant: selectedPlant }))

    // Reset selected products when plant changes
    setSelectedProducts([])

    // Products will be fetched automatically by the useEffect that watches orderData.plant
  }

  const fetchTemplateData = async () => {
    try {
      setLoading(true)
      setError('')

      if (templateId) {
        // Fetch template data using API
        const response = await purchaseOrderService.getPurchaseOrderMasterByUID(templateId)

        if (response.success) {
          const templateData = response.master

          // Update order data with template information
          if (templateData) {
            setOrderData(prev => ({
              ...prev,
              plant: templateData.plant || prev.plant,
              requestedBy: templateData.requestedBy || prev.requestedBy,
              requestedByFranchisee: templateData.requestedByFranchisee || prev.requestedByFranchisee
            }))

            // Set products from template
            if (templateData.products && Array.isArray(templateData.products) && templateData.products.length > 0) {
              const formattedProducts = templateData.products.map((product, index) => ({
                id: product.uid || product.id || `product_${index + 1}`,
                checked: false,
                skuCode: product.skuCode || product.SKUCode || '',
                skuName: product.skuName || product.SKUName || '',
                availQty: product.availQty || product.AvailQty || 0,
                modelQty: product.modelQty || product.ModelQty || 0,
                inTransit: product.inTransit || product.InTransit || 0,
                booked: product.booked || product.Booked || 0,
                eaPerCase: product.eaPerCase || product.EAPerCase || 1,
                suggestedOrderQty: product.suggestedOrderQty || product.SuggestedOrderQty || 0,
                uom: product.uom || product.UOM || 'OU',
                orderQty: product.orderQty || product.OrderQty || 0,
                unitPrice: product.unitPrice || product.UnitPrice || 0,
                value: product.value || product.Value || 0,
                past3MonthAverage: product.past3MonthAverage || product.Past3MonthAverage || 0
              }))
              setProducts(formattedProducts)
            } else {
              // No products in template
              setProducts([])
            }
          } else {
            // No template data received
            setProducts([])
          }
        } else {
          setError(response.error || 'Failed to fetch template data')
          setProducts([])
        }
      } else {
        // No template ID provided - start with empty product list
        setProducts([])
      }
    } catch (error) {
      console.error('Error fetching template data:', error)
      setError('Failed to fetch template data')
    } finally {
      setLoading(false)
    }
  }

  const handleProductCheck = (productId) => {
    setProducts(products.map(product =>
      product.id === productId ? { ...product, checked: !product.checked } : product
    ))
    if (selectedProducts.includes(productId)) {
      setSelectedProducts(selectedProducts.filter(id => id !== productId))
    } else {
      setSelectedProducts([...selectedProducts, productId])
    }
  }

  const handleQuantityChange = (productId, quantity) => {
    setProducts(products.map(product => {
      if (product.id === productId) {
        const qty = parseInt(quantity) || 0
        const value = qty * product.unitPrice
        return { ...product, orderQty: qty, value: value }
      }
      return product
    }))
  }

  const handleDeleteSelected = () => {
    setProducts(products.filter(product => !product.checked))
    setSelectedProducts([])
  }

  const handleSaveAsDraft = async () => {
    try {
      setSaving(true)
      setError('')

      // Validate order data
      const validation = purchaseOrderService.validatePurchaseOrderData(orderData, products)
      if (!validation.isValid) {
        setError(validation.errors.join(', '))
        return
      }

      // Format data for API
      const { purchaseOrderHeader, purchaseOrderLines, purchaseOrderLineProvisions } =
        purchaseOrderService.formatPurchaseOrderData(orderData, products)

      // Set status as DRAFT
      purchaseOrderHeader.status = 'DRAFT'

      // Save as draft
      const response = await purchaseOrderService.savePurchaseOrderAsDraft(
        purchaseOrderHeader,
        purchaseOrderLines,
        purchaseOrderLineProvisions
      )

      if (response.success) {
        alert('Purchase order saved as draft successfully!')
        router.push('/manager/purchase-order-status')
      } else {
        setError(response.error || 'Failed to save purchase order as draft')
      }
    } catch (error) {
      console.error('Error saving as draft:', error)
      setError('Failed to save purchase order as draft')
    } finally {
      setSaving(false)
    }
  }

  const handleConfirm = async () => {
    try {
      setSaving(true)
      setError('')

      // Validate order data
      const validation = purchaseOrderService.validatePurchaseOrderData(orderData, products)
      if (!validation.isValid) {
        setError(validation.errors.join(', '))
        return
      }

      // Format data for API
      const { purchaseOrderHeader, purchaseOrderLines, purchaseOrderLineProvisions } =
        purchaseOrderService.formatPurchaseOrderData(orderData, products)

      // Set status as CONFIRMED
      purchaseOrderHeader.status = 'CONFIRMED'

      // Create confirmed order
      const response = await purchaseOrderService.confirmPurchaseOrder(
        purchaseOrderHeader,
        purchaseOrderLines,
        purchaseOrderLineProvisions
      )

      if (response.success) {
        alert('Purchase order confirmed successfully!')
        router.push('/manager/purchase-order-status')
      } else {
        setError(response.error || 'Failed to confirm purchase order')
      }
    } catch (error) {
      console.error('Error confirming order:', error)
      setError('Failed to confirm purchase order')
    } finally {
      setSaving(false)
    }
  }

  const handleCancel = () => {
    router.push('/manager/purchase-order-templates')
  }

  const handleAddProduct = () => {
    console.log('Add product clicked')
    // Implement add product functionality
  }

  const handleClearOrder = () => {
    setProducts(products.map(product => ({ ...product, orderQty: 0, value: 0 })))
  }

  const calculateTotal = () => {
    return products.reduce((total, product) => total + product.value, 0)
  }

  const calculateTotalQuantity = () => {
    return products.reduce((total, product) => total + product.orderQty, 0)
  }

  // Check if any products have been added (have order quantity > 0)
  const hasProductsAdded = () => {
    return products.some(product => product.orderQty > 0)
  }

  const filteredProducts = products.filter(product =>
    product.skuName.toLowerCase().includes(searchQuery.toLowerCase()) ||
    product.skuCode.includes(searchQuery)
  )

  return (
    
      <div className="min-h-screen bg-gray-50">
      {/* Top Navigation Bar */}
      <div className="bg-white border-b border-gray-200 px-4 py-3">
        <div className="flex items-center gap-4">
          <Button
            onClick={() => router.push('/manager/purchase-order-templates')}
            variant="ghost"
            size="sm"
            className="flex items-center gap-2 text-blue-600 hover:text-blue-700"
          >
            <ArrowLeft className="w-4 h-4" />
            Back to Templates
          </Button>
          <div className="flex items-center gap-2 text-sm text-gray-500">
            <Home className="w-4 h-4" />
            <span>Home</span>
            <span>»</span>
            <span>Purchase Order</span>
            <span>»</span>
            <span className="text-gray-900 font-medium">Create Purchase Order</span>
          </div>
        </div>
      </div>

      <div className="p-4">
        {/* Header */}
        <div className="bg-white rounded-lg shadow-sm p-6 mb-4">
          <h1 className="text-3xl font-bold text-blue-700 mb-2">Create Purchase Order</h1>
          <p className="text-gray-600">Create a purchase order from the selected template</p>
        </div>

        {/* Error Message */}
        {error && (
          <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded mb-4">
            <p className="font-medium">Error: {error}</p>
          </div>
        )}

      {/* Order Details */}
      <div className="bg-white rounded-lg shadow-sm p-6 mb-4">
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
          {/* Left Column */}
          <div className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Plant</label>
              <select
                value={orderData.plant}
                onChange={(e) => handlePlantChange(e.target.value)}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              >
                <option value="">Select a plant</option>
                {warehouses.map((warehouse) => (
                  <option key={warehouse.value} value={warehouse.value}>
                    {warehouse.label}
                  </option>
                ))}
              </select>
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Prepared by</label>
              <div className="relative">
                <input
                  type="text"
                  value={orderData.preparedBy}
                  readOnly
                  className="w-full px-3 py-2 border border-gray-300 rounded-md bg-gray-100"
                />
              </div>
            </div>
          </div>

          {/* Middle Column */}
          <div className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Order Date</label>
              <input
                type="text"
                value={orderData.orderDate}
                readOnly
                className="w-full px-3 py-2 border border-gray-300 rounded-md bg-gray-100"
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Requested Delivery Date</label>
              <div className="relative">
                <input
                  type="date"
                  value={orderData.requestedDeliveryDate}
                  onChange={(e) => setOrderData({ ...orderData, requestedDeliveryDate: e.target.value })}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md"
                />
                <Calendar className="absolute right-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400 pointer-events-none" />
              </div>
            </div>
          </div>

          {/* Right Column - Address */}
          <div className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Requested by</label>
              <select className="w-full px-3 py-2 border border-gray-300 rounded-md">
                <option>{orderData.requestedBy}</option>
              </select>
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Requested by Franchisee</label>
              <div className="text-sm text-gray-600 space-y-1 p-3 bg-gray-50 rounded-md">
                <div className="font-medium">{orderData.requestedByFranchisee.name}</div>
                <div>{orderData.requestedByFranchisee.poBox}</div>
                <div>City: {orderData.requestedByFranchisee.city}</div>
                <div>Region: {orderData.requestedByFranchisee.region}</div>
                <div>Country: {orderData.requestedByFranchisee.country}</div>
                <div>Postal Code: {orderData.requestedByFranchisee.postalCode}</div>
                <div>Telephone: {orderData.requestedByFranchisee.telephone}</div>
                <div>Mobile Phone: {orderData.requestedByFranchisee.mobile}</div>
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* Products Section */}
      <div className="bg-white rounded-lg shadow-sm p-6">
        {/* Search and Actions */}
        <div className="flex justify-between items-center mb-4">
          <div className="relative flex-1 max-w-md">
            <input
              type="text"
              placeholder="Search"
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              className="w-full px-3 py-2 pl-10 border border-gray-300 rounded-md"
            />
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
          </div>
          <div className="flex gap-2">
            <Button
              onClick={handleClearOrder}
              className="px-4 py-2 border border-gray-300 bg-white text-gray-700 hover:bg-gray-50"
            >
              Clear Order Qty
            </Button>
            <Button
              onClick={handleAddProduct}
              className="px-4 py-2 bg-blue-600 text-white hover:bg-blue-700"
            >
              Add Product
            </Button>
          </div>
        </div>

        {/* Products Table */}
        <div className="overflow-x-auto">
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-100">
              <tr>
                <th className="px-3 py-3 text-left text-xs font-medium text-gray-700 uppercase tracking-wider">
                  <input
                    type="checkbox"
                    onChange={(e) => {
                      const checked = e.target.checked
                      setProducts(products.map(p => ({ ...p, checked })))
                      setSelectedProducts(checked ? products.map(p => p.id) : [])
                    }}
                    className="rounded border-gray-300"
                  />
                </th>
                <th className="px-3 py-3 text-left text-xs font-medium text-gray-700 uppercase tracking-wider">#</th>
                <th className="px-3 py-3 text-left text-xs font-medium text-gray-700 uppercase tracking-wider">SKU Code</th>
                <th className="px-3 py-3 text-left text-xs font-medium text-gray-700 uppercase tracking-wider">SKU Name</th>
                <th className="px-3 py-3 text-center text-xs font-medium text-gray-700 uppercase tracking-wider">Avl Qty</th>
                <th className="px-3 py-3 text-center text-xs font-medium text-gray-700 uppercase tracking-wider">Model Qty</th>
                <th className="px-3 py-3 text-center text-xs font-medium text-gray-700 uppercase tracking-wider">In Transit</th>
                <th className="px-3 py-3 text-center text-xs font-medium text-gray-700 uppercase tracking-wider">Booked</th>
                <th className="px-3 py-3 text-center text-xs font-medium text-gray-700 uppercase tracking-wider">EA Per Case</th>
                <th className="px-3 py-3 text-center text-xs font-medium text-gray-700 uppercase tracking-wider">Suggested Order Qty</th>
                <th className="px-3 py-3 text-center text-xs font-medium text-gray-700 uppercase tracking-wider">UOM</th>
                <th className="px-3 py-3 text-center text-xs font-medium text-gray-700 uppercase tracking-wider">Order Qty</th>
                <th className="px-3 py-3 text-center text-xs font-medium text-gray-700 uppercase tracking-wider">Unit Price</th>
                <th className="px-3 py-3 text-center text-xs font-medium text-gray-700 uppercase tracking-wider">Value</th>
                <th className="px-3 py-3 text-center text-xs font-medium text-gray-700 uppercase tracking-wider">Past 3 Month Average</th>
              </tr>
            </thead>
            <tbody className="bg-white divide-y divide-gray-200">
              {loading ? (
                <tr>
                  <td colSpan="15" className="px-6 py-4 text-center">Loading template data...</td>
                </tr>
              ) : filteredProducts.length === 0 ? (
                <tr>
                  <td colSpan="15" className="px-6 py-4 text-center text-gray-500">
                    {!orderData.plant
                      ? 'Please select a plant to view available products.'
                      : searchQuery
                        ? 'No products found matching your search.'
                        : 'No products available for this plant.'}
                  </td>
                </tr>
              ) : (
                filteredProducts.map((product, index) => (
                  <tr key={product.id} className="hover:bg-gray-50">
                    <td className="px-3 py-2">
                      <input
                        type="checkbox"
                        checked={product.checked}
                        onChange={() => handleProductCheck(product.id)}
                        className="rounded border-gray-300"
                      />
                    </td>
                    <td className="px-3 py-2 text-sm">{index + 1}</td>
                    <td className="px-3 py-2 text-sm">{product.skuCode}</td>
                    <td className="px-3 py-2 text-sm">{product.skuName}</td>
                    <td className="px-3 py-2 text-sm text-center">{product.availQty}</td>
                    <td className="px-3 py-2 text-sm text-center">{product.modelQty}</td>
                    <td className="px-3 py-2 text-sm text-center">{product.inTransit}</td>
                    <td className="px-3 py-2 text-sm text-center">{product.booked}</td>
                    <td className="px-3 py-2 text-sm text-center">{product.eaPerCase}</td>
                    <td className="px-3 py-2 text-sm text-center">{product.suggestedOrderQty}</td>
                    <td className="px-3 py-2 text-sm text-center">{product.uom}</td>
                    <td className="px-3 py-2 text-sm text-center">
                      <input
                        type="number"
                        value={product.orderQty}
                        onChange={(e) => handleQuantityChange(product.id, e.target.value)}
                        className="w-20 px-2 py-1 border border-gray-300 rounded text-center"
                        min="0"
                      />
                    </td>
                    <td className="px-3 py-2 text-sm text-center">{product.unitPrice.toFixed(2)}</td>
                    <td className="px-3 py-2 text-sm text-center">{product.value.toFixed(2)}</td>
                    <td className="px-3 py-2 text-sm text-center">{product.past3MonthAverage.toFixed(2)}</td>
                  </tr>
                ))
              )}
            </tbody>
            <tfoot className="bg-gray-100">
              <tr>
                <td colSpan="11" className="px-3 py-3 text-right font-medium">Total</td>
                <td className="px-3 py-3 text-center font-medium text-blue-600">{calculateTotalQuantity()}</td>
                <td></td>
                <td className="px-3 py-3 text-center font-medium text-blue-600">{calculateTotal().toFixed(2)}</td>
                <td></td>
              </tr>
            </tfoot>
          </table>
        </div>

        {/* Action Buttons */}
        <div className="flex justify-center gap-3 mt-6">
          <Button
            onClick={handleDeleteSelected}
            className="px-6 py-2 border border-gray-300 bg-white text-gray-700 hover:bg-gray-50"
            disabled={selectedProducts.length === 0}
          >
            Delete Selected From List
          </Button>
          {hasProductsAdded() && (
            <>
              <Button
                onClick={handleSaveAsDraft}
                disabled={saving}
                className="px-6 py-2 bg-blue-600 text-white hover:bg-blue-700 disabled:opacity-50"
              >
                {saving ? 'Saving...' : 'Save As Draft'}
              </Button>
              <Button
                onClick={handleConfirm}
                disabled={saving}
                className="px-6 py-2 bg-green-600 text-white hover:bg-green-700 disabled:opacity-50"
              >
                {saving ? 'Confirming...' : 'Confirm'}
              </Button>
            </>
          )}
          <Button
            onClick={handleCancel}
            className="px-6 py-2 border border-gray-300 bg-white text-gray-700 hover:bg-gray-50"
          >
            Cancel
          </Button>
        </div>

        {/* Show message when no products added */}
        {!hasProductsAdded() && products.length > 0 && (
          <div className="text-center mt-4 p-4 bg-blue-50 border border-blue-200 rounded-lg">
            <p className="text-blue-700 font-medium">
              Add quantities to products to enable save options
            </p>
            <p className="text-blue-600 text-sm mt-1">
              Enter order quantities in the table above to proceed with saving your purchase order
            </p>
          </div>
        )}
      </div>
      </div>
      </div>
    
  )
}

export default CreatePurchaseOrder