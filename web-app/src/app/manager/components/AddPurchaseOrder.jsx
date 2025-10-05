"use client"
import { useState, useEffect } from 'react'
import { useRouter } from 'next/navigation'
import { Search, Calendar, ArrowLeft, Home } from 'lucide-react'
import { Button } from '../../../components/ui/button'
import ProductSelectionModal from './ProductSelectionModal'
import purchaseOrderService from '../../../services/purchaseOrder'
import { organizationService } from '../../../services/organizationService'

const AddPurchaseOrder = () => {
  const router = useRouter()
  const [products, setProducts] = useState([])
  const [plants, setPlants] = useState([])
  const [employees, setEmployees] = useState([])
  const [loading, setLoading] = useState(false)
  const [plantsLoading, setPlantsLoading] = useState(true)
  const [employeesLoading, setEmployeesLoading] = useState(true)
  const [searchQuery, setSearchQuery] = useState('')
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState('')
  const [showProductModal, setShowProductModal] = useState(false)
  const [recentlyAdded, setRecentlyAdded] = useState([])
  const [selectedProducts, setSelectedProducts] = useState([])

  // Order form data
  const [orderData, setOrderData] = useState({
    plant: 'Select',
    orderDate: new Date().toISOString().split('T')[0],
    requestedBy: '',
    requestedByFranchisee: '',
    preparedBy: 'Admin',
    sapStatus: 'Pending',
    requestedDeliveryDate: new Date().toISOString().split('T')[0]
  })

  useEffect(() => {
    loadPlants()
    loadEmployees()
  }, [])

  const loadPlants = async () => {
    try {
      setPlantsLoading(true)
      console.log('🏭 Loading plants from organization service...')

      // Fetch organizations using the proper organizationService (same as administration section)
      const filters = [
        {
          Name: "IsActive",
          Value: true,
          Type: 0, // Equal
          FilterType: 0
        }
      ]

      const response = await organizationService.getOrganizations(1, 1000, filters)
      console.log('🏭 Organization service response:', response)

      if (response && response.data) {
        // Filter to show only organizations with ShowInUI = true (same as administration)
        const visibleOrgs = response.data.filter(org => org.ShowInUI !== false && org.IsActive)

        const plantList = visibleOrgs.map(org => ({
          value: org.UID,
          label: org.Name,
          code: org.Code,
          type: org.OrgTypeName || 'Organization'
        }))

        console.log('🏭 Loaded plants successfully:', plantList)
        setPlants(plantList)
      } else {
        console.error('❌ Failed to load plants: No response data')
        setPlants([])
      }
    } catch (error) {
      console.error('❌ Error loading plants:', error)
      setPlants([])
    } finally {
      setPlantsLoading(false)
    }
  }

  const loadEmployees = async () => {
    try {
      setEmployeesLoading(true)
      console.log('👤 Loading employees from purchase order service...')

      const response = await purchaseOrderService.getEmployeeDropdownOptions()
      console.log('👤 Employee service response:', response)

      if (response.success && response.employees) {
        console.log('👤 Loaded employees successfully:', response.employees)
        setEmployees(response.employees)
      } else {
        console.error('❌ Failed to load employees')
        setEmployees([])
      }
    } catch (error) {
      console.error('❌ Error loading employees:', error)
      setEmployees([])
    } finally {
      setEmployeesLoading(false)
    }
  }

  const handleOrderChange = (field, value) => {
    setOrderData(prev => ({
      ...prev,
      [field]: value
    }))
  }

  const handleAddProductToOrder = () => {
    if (!orderData.plant || orderData.plant === 'Select') {
      alert('Please select a plant first before adding products.')
      return
    }
    setShowProductModal(true)
  }

  const handleProductsAdded = (newProducts) => {
    if (newProducts && newProducts.length > 0) {
      setProducts(prevProducts => {
        const updatedProducts = [...prevProducts, ...newProducts]
        console.log('✅ Products added to order:', newProducts)
        console.log('📋 Updated order products:', updatedProducts)

        const newProductIds = newProducts.map(p => p.id)
        setRecentlyAdded(newProductIds)

        setTimeout(() => {
          setRecentlyAdded([])
        }, 3000)

        return updatedProducts
      })
    }
  }

  const handleRemoveProduct = (productId) => {
    setProducts(prevProducts => prevProducts.filter(product => product.id !== productId))
  }

  const handleQuantityChange = (productId, value) => {
    setProducts(prevProducts =>
      prevProducts.map(product =>
        product.id === productId
          ? {
              ...product,
              orderQty: parseInt(value) || 0,
              value: (parseInt(value) || 0) * (product.unitPrice || 0)
            }
          : product
      )
    )
  }

  const handleSelectProduct = (productId) => {
    setSelectedProducts(prev => {
      if (prev.includes(productId)) {
        return prev.filter(id => id !== productId)
      } else {
        return [...prev, productId]
      }
    })
  }

  const handleDeleteSelected = () => {
    if (selectedProducts.length === 0) {
      alert('Please select at least one product to delete')
      return
    }

    if (window.confirm(`Are you sure you want to remove ${selectedProducts.length} selected product(s)?`)) {
      setProducts(prevProducts => prevProducts.filter(p => !selectedProducts.includes(p.id)))
      setSelectedProducts([])
    }
  }

  const handleClearOrderQty = () => {
    if (window.confirm('Are you sure you want to clear all order quantities?')) {
      setProducts(prevProducts =>
        prevProducts.map(product => ({
          ...product,
          orderQty: 0,
          value: 0
        }))
      )
    }
  }

  const handleCancel = () => {
    router.push('/manager/purchase-order-status')
  }

  const handleSave = async (saveType = 'draft') => {
    try {
      setSaving(true)
      setError('')

      // Validate order data
      if (!orderData.plant || orderData.plant === 'Select') {
        setError('Please select a plant')
        return
      }

      if (products.length === 0) {
        setError('Please add at least one product to the order')
        return
      }

      const validProducts = products.filter(p => p.orderQty && p.orderQty > 0)
      if (validProducts.length === 0) {
        setError('Please add at least one product with Order Qty greater than zero.')
        return
      }

      // Find the selected plant to get its UID
      const selectedPlant = plants.find(plant => plant.value === orderData.plant)
      const plantUID = selectedPlant ? selectedPlant.value : orderData.plant
      const plantCode = selectedPlant ? (selectedPlant.code || selectedPlant.label) : 'PLANT'

      // Get user and org UIDs from localStorage - backend will handle invalid values
      const userUID = typeof window !== 'undefined' ? localStorage.getItem('userUID') : ''
      const validUserUID = (userUID && userUID !== 'default-user' && userUID !== 'system-user' && userUID !== 'null') ? userUID : ''

      // Get orgUID from localStorage, but allow null if invalid
      const orgUID = typeof window !== 'undefined' ? localStorage.getItem('orgUID') : null
      const validOrgUID = (orgUID && orgUID !== 'default-org' && orgUID !== 'null') ? orgUID : null

      const currentDate = new Date().toISOString()

      // Generate order number based on saveType
      // Format for DRAFT: PLANTCODE-YYYYMMDD-XXX (e.g., LBCL-20250510-001)
      // Format for CONFIRMED: PO-PLANTCODE-YYYYMMDD-XXX (e.g., PO-LBCL-20250510-001)
      const now = new Date()
      const dateStr = String(now.getFullYear()) +
                      String(now.getMonth() + 1).padStart(2, '0') +
                      String(now.getDate()).padStart(2, '0')

      // Generate sequence number (3 digits) based on current time for uniqueness
      // In production, this should come from a database sequence
      const timeBasedSeq = String(now.getHours()).padStart(2, '0') +
                           String(now.getMinutes()).padStart(2, '0') +
                           String(now.getSeconds()).padStart(2, '0')
      const sequenceNum = timeBasedSeq.slice(-3) // Take last 3 digits

      const draftOrderNumber = `${plantCode}-${dateStr}-${sequenceNum}`
      const orderNumber = saveType === 'confirm' ? `PO-${draftOrderNumber}` : null

      console.log('🔢 Generated Order Numbers:', {
        saveType,
        draftOrderNumber,
        orderNumber,
        plantCode
      })

      // Generate a unique UID for the purchase order header
      // Format: UUID v4 (xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx)
      const generateUID = () => {
        return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {
          const r = Math.random() * 16 | 0
          const v = c === 'x' ? r : (r & 0x3 | 0x8)
          return v.toString(16)
        })
      }

      const purchaseOrderHeaderUID = generateUID()

      // Get employee names from the employees array
      const requestedByEmployee = employees.find(emp => emp.value === orderData.requestedBy)
      const requestedByName = requestedByEmployee ? requestedByEmployee.name : orderData.preparedBy

      // Prepare purchase order header with dynamic status and order numbers
      const purchaseOrderHeader = {
        uid: purchaseOrderHeaderUID, // CRITICAL: Set the UID so lines can reference it
        orgUID: validOrgUID,
        wareHouseUID: plantUID,
        warehouse_uid: plantUID,
        orderDate: orderData.orderDate ? new Date(orderData.orderDate).toISOString() : currentDate,
        expectedDeliveryDate: orderData.requestedDeliveryDate ? new Date(orderData.requestedDeliveryDate).toISOString() : currentDate,
        reportingEmpName: requestedByName,
        createdByEmpName: orderData.preparedBy,
        sapStatus: 'N/A',
        status: saveType === 'confirm' ? 'CONFIRMED' : 'DRAFT',
        orderNumber: orderNumber, // Set for confirmed orders (PO-xxx)
        draftOrderNumber: draftOrderNumber, // ALWAYS set the draft number
        qtyCount: validProducts.reduce((sum, p) => sum + (p.orderQty || 0), 0),
        lineCount: validProducts.length,
        totalAmount: validProducts.reduce((sum, p) => sum + ((p.orderQty || 0) * (p.unitPrice || 0)), 0),
        netAmount: validProducts.reduce((sum, p) => sum + ((p.orderQty || 0) * (p.unitPrice || 0)), 0),
        createdBy: validUserUID,
        modifiedBy: validUserUID,
        // Explicitly set address UIDs to null if not provided to avoid FK constraint violations
        shippingAddressUID: null,
        billingAddressUID: null
      }

      // Prepare purchase order lines with pricing
      const purchaseOrderLines = validProducts.map((product, index) => {
        const lineUID = generateUID() // Generate unique UID for each line
        const qty = product.orderQty || 0
        const price = product.unitPrice || 0
        const lineTotal = qty * price

        return {
          uid: lineUID, // CRITICAL: Set the line UID
          purchaseOrderHeaderUID: purchaseOrderHeaderUID, // CRITICAL: Link line to header
          lineNumber: index + 1,
          skuuid: product.skuuid || product.uid || product.id,
          skuCode: product.skuCode,
          skuType: product.skuType || '',
          uom: product.uom || 'OU',
          baseUOM: product.baseUOM || product.uom || 'OU',
          availableQty: product.availQty || 0,
          modelQty: product.modelQty || 0,
          inTransitQty: product.inTransit || 0,
          suggestedQty: product.suggestedOrderQty || 0,
          past3MonthAvg: product.past3MonthAverage || 0,
          requestedQty: qty,
          finalQty: qty,
          finalQtyBU: qty,
          unitPrice: price,
          basePrice: product.basePrice || price,
          totalAmount: lineTotal,
          netAmount: lineTotal,
          mrp: product.mrp || 0,
          dpPrice: product.dpPrice || 0,
          totalDiscount: 0,
          lineDiscount: 0,
          headerDiscount: 0,
          totalTaxAmount: 0,
          lineTaxAmount: 0,
          headerTaxAmount: 0,
          taxData: '{}',
          createdBy: validUserUID,
          modifiedBy: validUserUID
        }
      })

      console.log('💾 Saving purchase order:', { purchaseOrderHeader, purchaseOrderLines })
      console.log('📊 Purchase Order Header:', JSON.stringify(purchaseOrderHeader, null, 2))
      console.log('📦 Purchase Order Lines:', JSON.stringify(purchaseOrderLines, null, 2))

      // Create new purchase order
      console.log('🚀 Calling createPurchaseOrder API...')
      const response = await purchaseOrderService.createPurchaseOrder(
        purchaseOrderHeader,
        purchaseOrderLines,
        []
      )

      console.log('📡 API Response:', response)

      if (response.success) {
        console.log('✅ Purchase order created successfully!')
        const statusText = saveType === 'confirm' ? 'confirmed' : 'saved as draft'
        alert(`✅ Purchase order ${statusText} successfully with ${validProducts.length} products!`)
        router.push('/manager/purchase-order-status')
      } else {
        console.error('❌ Failed to create purchase order:', response.error)
        alert(`❌ Failed to create purchase order: ${response.error}`)
        setError(response.error || 'Failed to create purchase order')
      }
    } catch (error) {
      console.error('❌ Error saving purchase order:', error)
      console.error('❌ Error details:', {
        message: error.message,
        stack: error.stack,
        error: error
      })
      alert(`❌ Error: ${error.message || 'An unexpected error occurred'}`)
      setError(`Error: ${error.message || 'An unexpected error occurred'}`)
    } finally {
      setSaving(false)
    }
  }

  const filteredProducts = products.filter(product =>
    product.skuName?.toLowerCase().includes(searchQuery.toLowerCase()) ||
    product.skuCode?.toLowerCase().includes(searchQuery.toLowerCase())
  )

  return (
    
      <div className="min-h-screen bg-gray-50">
      {/* Top Navigation Bar */}
      <div className="bg-white border-b border-gray-200 px-4 py-3">
        <div className="flex items-center gap-4">
          <Button
            onClick={() => router.push('/manager/purchase-order-status')}
            variant="ghost"
            size="sm"
            className="flex items-center gap-2 text-blue-600 hover:text-blue-700"
          >
            <ArrowLeft className="w-4 h-4" />
            Back to Purchase Orders
          </Button>
          <div className="flex items-center gap-2 text-sm text-gray-500">
            <Home className="w-4 h-4" />
            <span>Home</span>
            <span>»</span>
            <span>Purchase Order</span>
            <span>»</span>
            <span className="text-gray-900 font-medium">Add Purchase Order</span>
          </div>
        </div>
      </div>

      <div className="p-4">
        {/* Header Section */}
        <div className="bg-white rounded-lg shadow-sm p-6 mb-4">
          <div>
            <h1 className="text-3xl font-bold text-blue-700">Add Purchase Order</h1>
            <p className="text-gray-600 mt-2">Create a new purchase order by filling out the form below</p>
          </div>
        </div>

        {/* Error Message */}
        {error && (
          <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded mb-4">
            <p className="font-medium">Error: {error}</p>
          </div>
        )}

      {/* Order Form */}
      <div className="bg-white rounded-lg shadow-sm p-6 mb-4">
        <div className="border-t-4 border-blue-500 p-4">
          <div className="grid grid-cols-1 md:grid-cols-4 gap-6">
            {/* Plant */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                <span className="text-red-500">*</span>Plant
              </label>
              <select
                value={orderData.plant}
                onChange={(e) => handleOrderChange('plant', e.target.value)}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                disabled={plantsLoading}
                required
              >
                <option value="Select">
                  {plantsLoading ? 'Loading plants...' : 'Select Plant'}
                </option>
                {plants.map((plant) => (
                  <option key={plant.value} value={plant.value}>
                    {plant.label}
                  </option>
                ))}
              </select>
            </div>

            {/* Order Date */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Order Date</label>
              <input
                type="date"
                value={orderData.orderDate}
                onChange={(e) => handleOrderChange('orderDate', e.target.value)}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              />
            </div>

            {/* Requested By */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Requested by</label>
              <select
                value={orderData.requestedBy}
                onChange={(e) => handleOrderChange('requestedBy', e.target.value)}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                disabled={employeesLoading}
              >
                <option value="">{employeesLoading ? 'Loading employees...' : 'Select'}</option>
                {employees.map((employee) => (
                  <option key={employee.value} value={employee.value}>
                    {employee.label}
                  </option>
                ))}
              </select>
            </div>

            {/* Requested by Franchisee */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Requested by Franchisee</label>
              <select
                value={orderData.requestedByFranchisee}
                onChange={(e) => handleOrderChange('requestedByFranchisee', e.target.value)}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                disabled={employeesLoading}
              >
                <option value="">{employeesLoading ? 'Loading employees...' : 'Select'}</option>
                {employees.map((employee) => (
                  <option key={employee.value} value={employee.value}>
                    {employee.label}
                  </option>
                ))}
              </select>
            </div>
          </div>

          {/* Second Row */}
          <div className="grid grid-cols-1 md:grid-cols-4 gap-6 mt-4">
            {/* Prepared by */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Prepared by</label>
              <input
                type="text"
                value={orderData.preparedBy}
                readOnly
                className="w-full px-3 py-2 border border-gray-300 rounded-md bg-gray-100"
              />
            </div>

            {/* SAP Status */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">SAP Status</label>
              <select
                value={orderData.sapStatus}
                onChange={(e) => handleOrderChange('sapStatus', e.target.value)}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              >
                <option value="Pending">Pending</option>
                <option value="Approved">Approved</option>
                <option value="Rejected">Rejected</option>
                <option value="In Progress">In Progress</option>
                <option value="Completed">Completed</option>
              </select>
            </div>

            {/* Requested Delivery Date */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                <span className="text-red-500">*</span>Requested Delivery Date
              </label>
              <div className="relative">
                <input
                  type="date"
                  value={orderData.requestedDeliveryDate}
                  onChange={(e) => handleOrderChange('requestedDeliveryDate', e.target.value)}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  required
                />
                <Calendar className="absolute right-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400 pointer-events-none" />
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* Error Message - shown above table */}
      {products.length === 0 && (
        <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded mb-4">
          <p>Please add at least one product with Order Qty greater than zero.</p>
        </div>
      )}

      {/* Search and Add Product Section */}
      <div className="bg-white rounded-lg shadow-sm p-6 mb-4">
        <div className="flex justify-between items-center mb-4">
          <div className="relative flex-1 max-w-md">
            <input
              type="text"
              placeholder="Search"
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              className="w-full px-3 py-2 pl-10 border border-gray-300 rounded-md focus:ring-2 focus:ring-blue-500 focus:border-transparent"
            />
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
          </div>
          <div className="flex items-center gap-4">
            <Button
              onClick={handleClearOrderQty}
              variant="outline"
              className="border-blue-600 text-blue-600 hover:bg-blue-50 px-4 py-2"
            >
              Clear Order Qty
            </Button>
            <Button
              onClick={handleAddProductToOrder}
              className="bg-blue-600 hover:bg-blue-700 text-white px-6 py-2"
              disabled={!orderData.plant || orderData.plant === 'Select'}
            >
              Add Product
            </Button>
          </div>
        </div>

        {/* Products Table */}
        <div className="overflow-x-auto">
          <table className="min-w-full border-collapse">
            <thead className="bg-gray-600 text-white">
              <tr>
                <th className="px-3 py-2 text-left text-xs font-medium border border-gray-500">
                  <input
                    type="checkbox"
                    checked={selectedProducts.length === filteredProducts.length && filteredProducts.length > 0}
                    onChange={(e) => {
                      if (e.target.checked) {
                        setSelectedProducts(filteredProducts.map(p => p.id))
                      } else {
                        setSelectedProducts([])
                      }
                    }}
                    className="w-4 h-4"
                  />
                </th>
                <th className="px-3 py-2 text-left text-xs font-medium border border-gray-500">#</th>
                <th className="px-3 py-2 text-left text-xs font-medium border border-gray-500">SKU Code</th>
                <th className="px-3 py-2 text-left text-xs font-medium border border-gray-500">SKU Name</th>
                <th className="px-3 py-2 text-center text-xs font-medium border border-gray-500">Avl Qty</th>
                <th className="px-3 py-2 text-center text-xs font-medium border border-gray-500">Model Qty</th>
                <th className="px-3 py-2 text-center text-xs font-medium border border-gray-500">In Transit</th>
                <th className="px-3 py-2 text-center text-xs font-medium border border-gray-500">Booked</th>
                <th className="px-3 py-2 text-center text-xs font-medium border border-gray-500">EA Per Case</th>
                <th className="px-3 py-2 text-center text-xs font-medium border border-gray-500">Suggested Order Qty</th>
                <th className="px-3 py-2 text-center text-xs font-medium border border-gray-500">UOM</th>
                <th className="px-3 py-2 text-center text-xs font-medium border border-gray-500">Order Qty</th>
                <th className="px-3 py-2 text-center text-xs font-medium border border-gray-500">Unit Price</th>
                <th className="px-3 py-2 text-center text-xs font-medium border border-gray-500">Value</th>
                <th className="px-3 py-2 text-center text-xs font-medium border border-gray-500">Past 3 Month Average</th>
              </tr>
            </thead>
            <tbody className="bg-white">
              {loading ? (
                <tr>
                  <td colSpan="15" className="px-4 py-8 text-center border border-gray-300">
                    <div className="animate-pulse">Loading...</div>
                  </td>
                </tr>
              ) : filteredProducts.length === 0 ? (
                <tr>
                  <td colSpan="15" className="px-4 py-8 text-center border border-gray-300 text-gray-500">
                    {products.length === 0 ? 'No products added yet. Click "Add Product" to add products from your catalog' : 'No products match your search'}
                  </td>
                </tr>
              ) : (
                filteredProducts.map((product, index) => {
                  const isRecentlyAdded = recentlyAdded.includes(product.id)
                  const isSelected = selectedProducts.includes(product.id)
                  return (
                    <tr
                      key={product.id}
                      className={`hover:bg-gray-50 transition-colors ${
                        isRecentlyAdded ? 'bg-green-50' : isSelected ? 'bg-blue-50' : ''
                      }`}
                    >
                      <td className="px-3 py-2 border border-gray-300">
                        <input
                          type="checkbox"
                          checked={isSelected}
                          onChange={() => handleSelectProduct(product.id)}
                          className="w-4 h-4"
                        />
                      </td>
                      <td className="px-3 py-2 text-sm border border-gray-300">{index + 1}</td>
                      <td className="px-3 py-2 text-sm border border-gray-300">{product.skuCode}</td>
                      <td className="px-3 py-2 text-sm border border-gray-300">{product.skuName}</td>
                      <td className="px-3 py-2 text-sm text-center border border-gray-300">{product.availQty || 0}</td>
                      <td className="px-3 py-2 text-sm text-center border border-gray-300">{product.modelQty || 0}</td>
                      <td className="px-3 py-2 text-sm text-center border border-gray-300">{product.inTransit || 0}</td>
                      <td className="px-3 py-2 text-sm text-center border border-gray-300">{product.booked || 0}</td>
                      <td className="px-3 py-2 text-sm text-center border border-gray-300">{product.eaPerCase || 1}</td>
                      <td className="px-3 py-2 text-sm text-center border border-gray-300 bg-yellow-50">{product.suggestedOrderQty || 0}</td>
                      <td className="px-3 py-2 text-sm text-center border border-gray-300">{product.uom || 'OU'}</td>
                      <td className="px-3 py-2 text-sm text-center border border-gray-300">
                        <input
                          type="number"
                          value={product.orderQty || ''}
                          onChange={(e) => handleQuantityChange(product.id, e.target.value)}
                          className="w-16 px-2 py-1 border border-orange-400 rounded text-center focus:ring-2 focus:ring-orange-500"
                          min="0"
                        />
                      </td>
                      <td className="px-3 py-2 text-sm text-right border border-gray-300">{(product.unitPrice || 0).toFixed(2)}</td>
                      <td className="px-3 py-2 text-sm text-right border border-gray-300">{(product.value || 0).toFixed(2)}</td>
                      <td className="px-3 py-2 text-sm text-center border border-gray-300">{(product.past3MonthAverage || 0).toFixed(2)}</td>
                    </tr>
                  )
                })
              )}
            </tbody>
          </table>
        </div>

        {/* Total Section */}
        {filteredProducts.length > 0 && (
          <div className="mt-4 flex justify-end">
            <div className="text-right">
              <div className="flex items-center gap-8">
                <span className="font-bold text-gray-700">Total</span>
                <span className="font-bold text-lg">{products.reduce((sum, p) => sum + (p.orderQty || 0), 0)}</span>
                <span className="font-bold text-lg text-blue-600">{products.reduce((sum, p) => sum + (p.value || 0), 0).toFixed(2)}</span>
              </div>
            </div>
          </div>
        )}
      </div>

      {/* Action Buttons */}
      <div className="flex justify-center gap-3 mb-6">
        <Button
          onClick={handleDeleteSelected}
          variant="outline"
          disabled={selectedProducts.length === 0}
          className="border-blue-600 text-blue-600 hover:bg-blue-50 px-6 py-2 disabled:opacity-50"
        >
          Delete Selected From List
        </Button>
        <Button
          onClick={() => handleSave('draft')}
          disabled={saving || !orderData.plant || orderData.plant === 'Select' || products.length === 0}
          className="bg-blue-600 hover:bg-blue-700 text-white px-6 py-2 disabled:opacity-50"
        >
          {saving ? 'Saving...' : 'Save As Draft'}
        </Button>
        <Button
          onClick={() => handleSave('confirm')}
          disabled={saving || !orderData.plant || orderData.plant === 'Select' || products.length === 0}
          className="bg-blue-600 hover:bg-blue-700 text-white px-6 py-2 disabled:opacity-50"
        >
          {saving ? 'Confirming...' : 'Confirm'}
        </Button>
        <Button
          onClick={handleCancel}
          variant="outline"
          className="border-blue-600 text-blue-600 hover:bg-blue-50 px-6 py-2"
        >
          Cancel
        </Button>
        </div>

        {/* Product Selection Modal */}
        <ProductSelectionModal
          isOpen={showProductModal}
          onClose={() => setShowProductModal(false)}
          onAddProducts={handleProductsAdded}
          existingProducts={products}
          selectedPlant={orderData.plant && orderData.plant !== 'Select' ? orderData.plant : null}
        />
      </div>
      </div>
    
  )
}

export default AddPurchaseOrder