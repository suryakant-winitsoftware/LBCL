"use client"
import { useState, useEffect } from 'react'
import { useRouter } from 'next/navigation'
import { Search, ArrowLeft, Home } from 'lucide-react'
import { Button } from '../../../components/ui/button'
import { useAuth } from '../../../contexts/AuthContext'
import BusinessLayout from '../../../components/layouts/BusinessLayout'
import ProductSelectionModal from './ProductSelectionModal'
import organizationService from '../../../services/organization'
import purchaseOrderService from '../../../services/purchaseOrder'

const AddPurchaseOrderTemplate = () => {
  const { user } = useAuth()
  const router = useRouter()
  const [products, setProducts] = useState([])
  const [loading, setLoading] = useState(true)
  const [searchQuery, setSearchQuery] = useState('')
  const [showProductModal, setShowProductModal] = useState(false)

  // Template form data
  const [templateData, setTemplateData] = useState({
    templateName: '',
    plant: 'Select',
    requestedBy: '',
    requestedByFranchisee: '',
    date: 'N/A',
    preparedBy: user?.username || '[13010435] G & M Paterson'
  })
  const [recentlyAdded, setRecentlyAdded] = useState([])
  const [plants, setPlants] = useState([])
  const [plantsLoading, setPlantsLoading] = useState(false)

  useEffect(() => {
    const fetchInitialData = async () => {
      try {
        // Start with empty products - user can add products manually
        setProducts([])
        // Load real plant names from database
        await loadPlants()
      } catch (error) {
        console.error('Error fetching initial data:', error)
      } finally {
        setLoading(false)
      }
    }
    fetchInitialData()
  }, [])

  const loadPlants = async () => {
    try {
      setPlantsLoading(true)
      console.log('🏭 Loading plants from organization service...')

      // Ensure auth token is available
      if (typeof window !== 'undefined' && !localStorage.getItem('authToken')) {
        console.log('⚠️ Setting temporary auth token for plants loading');
        localStorage.setItem('authToken', 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiQURNSU4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3VzZXJkYXRhIjoiYWRtaW4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBZG1pbiIsInBlcm1pc3Npb25zIjpbInJlYWQiLCJ3cml0ZSJdLCJleHAiOjE3NTg3ODQ0MjIsImlzcyI6Im15aXNzdWVyIn0.ik92mNR1NYUR0m5R1UvciIjEKu-qSZoqoio78ancnm8');
      }

      const response = await organizationService.getOrganizationDropdownOptions()
      console.log('🏭 Organization service response:', response)

      if (response.success && response.organizations) {
        // Extract unique plant names from organizations
        const plantList = response.organizations.map(org => ({
          value: org.uid,
          label: org.name,
          type: org.type
        }))

        console.log('🏭 Loaded plants successfully:', plantList)
        setPlants(plantList)
      } else {
        console.error('❌ Failed to load plants:', response.error || 'No response data')
        // Fallback to hardcoded values
        const fallbackPlants = [
          { value: 'EPIC01', label: 'EPIC01', type: 'Organization' },
          { value: 'Farmley', label: 'Farmley', type: 'Organization' },
          { value: 'Supplier', label: 'Supplier', type: 'Division' }
        ]
        console.log('🏭 Using fallback plants:', fallbackPlants)
        setPlants(fallbackPlants)
      }
    } catch (error) {
      console.error('❌ Error loading plants:', error)
      console.error('❌ Error details:', error.message, error.stack)
      // Fallback to hardcoded values
      const fallbackPlants = [
        { value: 'EPIC01', label: 'EPIC01', type: 'Organization' },
        { value: 'Farmley', label: 'Farmley', type: 'Organization' },
        { value: 'Supplier', label: 'Supplier', type: 'Division' }
      ]
      console.log('🏭 Using fallback plants after error:', fallbackPlants)
      setPlants(fallbackPlants)
    } finally {
      setPlantsLoading(false)
    }
  }

  const handleTemplateChange = (field, value) => {
    setTemplateData(prev => ({
      ...prev,
      [field]: value
    }))
  }

  const handleAddProductToTemplate = () => {
    setShowProductModal(true)
  }

  const handleProductsAdded = (newProducts) => {
    if (newProducts && newProducts.length > 0) {
      setProducts(prevProducts => {
        const updatedProducts = [...prevProducts, ...newProducts]
        console.log('✅ Products added to template:', newProducts)
        console.log('📋 Updated template products:', updatedProducts)

        // Track recently added products for visual feedback
        const newProductIds = newProducts.map(p => p.id)
        setRecentlyAdded(newProductIds)

        // Clear the recently added highlighting after a delay
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
          ? { ...product, orderQty: parseInt(value) || 0 }
          : product
      )
    )
  }

  const handleCancel = () => {
    router.push('/user/manager/purchase-order-templates')
  }

  const handleSave = async () => {
    // Validate required fields
    if (!templateData.templateName) {
      alert('Please enter a template name.')
      return
    }

    if (!templateData.plant || templateData.plant === 'Select') {
      alert('Please select a plant.')
      return
    }

    if (products.length === 0) {
      alert('Please add at least one product to the template before saving.')
      return
    }

    try {
      setLoading(true)

      // Find the selected plant to get its UID
      const selectedPlant = plants.find(plant => plant.value === templateData.plant)
      const plantUID = selectedPlant ? selectedPlant.value : templateData.plant

      // Ensure we have required user information
      const userUID = typeof window !== 'undefined' ? localStorage.getItem('userUID') : 'default-user'
      const orgUID = typeof window !== 'undefined' ? localStorage.getItem('orgUID') : 'default-org'

      // Generate a unique UID for the template header
      const generateUID = () => {
        return 'template_' + Date.now() + '_' + Math.random().toString(36).substr(2, 9)
      }
      const templateHeaderUID = generateUID()

      // Prepare template header data according to IPurchaseOrderTemplateHeader interface
      const templateHeader = {
        uid: templateHeaderUID,              // Set the UID that lines will reference
        templateName: templateData.templateName,
        storeUid: plantUID,                  // Use proper plant UID
        wareHouseUID: plantUID,              // Keep for backward compatibility
        warehouse_uid: plantUID,             // Keep for backward compatibility
        orgUID: orgUID,
        isCreatedByStore: true,
        isActive: true,
        createdBy: userUID,
        modifiedBy: userUID,

        // Additional fields that might be useful for tracking
        requestedBy: templateData.requestedBy,
        requestedByFranchisee: templateData.requestedByFranchisee,
        preparedBy: templateData.preparedBy,
        status: 'ACTIVE'
      }

      // Prepare template lines data according to IPurchaseOrderTemplateLine interface
      const templateLines = products.map((product, index) => ({
        uid: generateUID(),                                    // Generate UID for each line
        purchaseOrderTemplateHeaderUID: templateHeaderUID,     // Link to header
        PurchaseOrderTemplateHeaderUID: templateHeaderUID,     // Backend expects this format too
        lineNumber: index + 1,
        skuuid: product.skuuid || product.uid || product.id,
        SKUUID: product.skuuid || product.uid || product.id,  // Backend expects uppercase
        skuCode: product.skuCode,
        SKUCode: product.skuCode,  // Backend expects uppercase
        uom: product.uom || 'OU',
        UOM: product.uom || 'OU',  // Backend expects uppercase
        qty: product.orderQty || 0,
        Qty: product.orderQty || 0,  // Backend expects uppercase
        requestedQty: product.orderQty || 0,
        inputQty: product.orderQty || 0,
        createdBy: userUID,
        modifiedBy: userUID,

        // Additional fields for context (not part of interface but might be useful)
        skuName: product.skuName,
        suggestedQty: product.suggestedOrderQty || 0,
        availableQty: product.availQty || 0,
        modelQty: product.modelQty || 0,
        inTransitQty: product.inTransit || 0,
        past3MonthAvg: product.past3MonthAverage || 0,
        casePerPC: product.casePerPC || 0,
        unitPrice: product.unitPrice || 0
      }))

      console.log('💾 Saving template to database:', { templateHeader, templateLines })

      // Call API to save template
      const response = await purchaseOrderService.createPurchaseOrderTemplate(templateHeader, templateLines)

      if (response.success) {
        alert(`✅ Template "${templateData.templateName}" saved successfully with ${products.length} products!`)
        router.push('/user/manager/purchase-order-templates')
      } else {
        console.error('❌ Failed to save template:', response.error)
        alert(`Failed to save template: ${response.error || 'Unknown error occurred'}`)
      }
    } catch (error) {
      console.error('❌ Error saving template:', error)
      alert(`Error saving template: ${error.message || 'An unexpected error occurred'}`)
    } finally {
      setLoading(false)
    }
  }

  const filteredProducts = products.filter(product =>
    product.skuName?.toLowerCase().includes(searchQuery.toLowerCase()) ||
    product.skuCode?.toLowerCase().includes(searchQuery.toLowerCase())
  )

  return (
    <BusinessLayout>
      <div className="min-h-screen bg-gray-50">
      {/* Top Navigation Bar */}
      <div className="bg-white border-b border-gray-200 px-4 py-3">
        <div className="flex items-center gap-4">
          <Button
            onClick={() => router.push('/user/manager/purchase-order-templates')}
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
            <span>Purchase Order Templates</span>
            <span>»</span>
            <span className="text-gray-900 font-medium">Add Purchase Order Template</span>
          </div>
        </div>
      </div>

      <div className="p-4">
        {/* Header Section */}
        <div className="bg-white rounded-lg shadow-sm p-6 mb-4">
          <div>
            <h1 className="text-3xl font-bold text-blue-700">Add Purchase Order Template</h1>
            <p className="text-gray-600 mt-2">Create a reusable template for purchase orders</p>
          </div>
        </div>

      {/* Template Form */}
      <div className="bg-white rounded-lg shadow-sm p-6 mb-4">
        <div className="border-t-4 border-blue-500 p-4">
          <div className="grid grid-cols-1 md:grid-cols-4 gap-6">
            {/* Template Name */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                <span className="text-red-500">*</span>Template Name
              </label>
              <input
                type="text"
                value={templateData.templateName}
                onChange={(e) => handleTemplateChange('templateName', e.target.value)}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                placeholder="Enter template name"
                required
              />
            </div>

            {/* Plant */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                <span className="text-red-500">*</span>Plant
              </label>
              <select
                value={templateData.plant}
                onChange={(e) => handleTemplateChange('plant', e.target.value)}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                required
                disabled={plantsLoading}
              >
                <option value="Select">
                  {plantsLoading ? 'Loading plants...' : 'Select Plant'}
                </option>
                {plants.map((plant) => (
                  <option key={plant.value} value={plant.value}>
                    {plant.label} ({plant.type})
                  </option>
                ))}
              </select>
            </div>

            {/* Requested By */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Requested By</label>
              <select
                value={templateData.requestedBy}
                onChange={(e) => handleTemplateChange('requestedBy', e.target.value)}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              >
                <option value="">Select</option>
                <option value="[13010435] G & M Paterson">[13010435] G & M Paterson</option>
                <option value="Other Franchisee">Other Franchisee</option>
              </select>
            </div>

            {/* Requested By Franchisee */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Requested By Franchisee</label>
              <select
                value={templateData.requestedByFranchisee}
                onChange={(e) => handleTemplateChange('requestedByFranchisee', e.target.value)}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              >
                <option value="">Select</option>
                <option value="[13010435] G & M Paterson">[13010435] G & M Paterson</option>
                <option value="Other Franchisee">Other Franchisee</option>
              </select>
            </div>
          </div>

          {/* Date and Prepared By Row */}
          <div className="grid grid-cols-1 md:grid-cols-4 gap-6 mt-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Date</label>
              <input
                type="text"
                value={templateData.date}
                readOnly
                className="w-full px-3 py-2 border border-gray-300 rounded-md bg-gray-100"
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Prepared By</label>
              <input
                type="text"
                value={templateData.preparedBy}
                readOnly
                className="w-full px-3 py-2 border border-gray-300 rounded-md bg-gray-100"
              />
            </div>
          </div>
        </div>
      </div>

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
            {products.length > 0 && (
              <div className="text-sm text-gray-600">
                <span className="font-medium text-blue-600">{products.length}</span> product{products.length !== 1 ? 's' : ''} in template
              </div>
            )}
            <Button
              onClick={handleAddProductToTemplate}
              className="bg-blue-600 hover:bg-blue-700 text-white px-6 py-2"
            >
              Add Product to Template
            </Button>
          </div>
        </div>

        {/* Products Table */}
        <div className="overflow-x-auto">
          <table className="min-w-full">
            <thead className="bg-gray-600 text-white">
              <tr>
                <th className="px-3 py-2 text-left text-xs font-medium">
                  Action
                </th>
                <th className="px-3 py-2 text-left text-xs font-medium">#</th>
                <th className="px-3 py-2 text-left text-xs font-medium">SKU Code</th>
                <th className="px-3 py-2 text-left text-xs font-medium">SKU Name</th>
                <th className="px-3 py-2 text-center text-xs font-medium">Avl Qty</th>
                <th className="px-3 py-2 text-center text-xs font-medium">Model Qty</th>
                <th className="px-3 py-2 text-center text-xs font-medium">In Transit</th>
                <th className="px-3 py-2 text-center text-xs font-medium">Booked</th>
                <th className="px-3 py-2 text-center text-xs font-medium">To Fill Qty</th>
                <th className="px-3 py-2 text-center text-xs font-medium">Case Per PC</th>
                <th className="px-3 py-2 text-center text-xs font-medium">Suggested Order Qty</th>
                <th className="px-3 py-2 text-center text-xs font-medium">UOM</th>
                <th className="px-3 py-2 text-center text-xs font-medium">Order Qty</th>
                <th className="px-3 py-2 text-center text-xs font-medium">Past 3 Month Average</th>
              </tr>
            </thead>
            <tbody className="bg-white divide-y divide-gray-200">
              {loading ? (
                <tr>
                  <td colSpan="14" className="px-4 py-8 text-center">
                    <div className="animate-pulse">Loading...</div>
                  </td>
                </tr>
              ) : filteredProducts.length === 0 ? (
                <tr>
                  <td colSpan="14" className="px-4 py-8 text-center">
                    <div className="flex flex-col items-center gap-3 text-gray-500">
                      <div className="w-12 h-12 bg-gray-100 rounded-full flex items-center justify-center">
                        <Search className="w-6 h-6 text-gray-400" />
                      </div>
                      <div>
                        {products.length === 0 ? (
                          <>
                            <p className="font-medium text-gray-700">No products added yet</p>
                            <p className="text-sm">Click "Add Product to Template" to add products from your catalog</p>
                          </>
                        ) : (
                          <>
                            <p className="font-medium text-gray-700">No products match your search</p>
                            <p className="text-sm">Try adjusting your search terms</p>
                          </>
                        )}
                      </div>
                    </div>
                  </td>
                </tr>
              ) : (
                filteredProducts.map((product, index) => {
                  const isRecentlyAdded = recentlyAdded.includes(product.id)
                  return (
                  <tr
                    key={product.id}
                    className={`hover:bg-gray-50 transition-colors duration-500 ${
                      isRecentlyAdded ? 'bg-green-50 border-l-4 border-green-400' : ''
                    }`}
                  >
                    <td className="px-3 py-2">
                      <button
                        onClick={() => handleRemoveProduct(product.id)}
                        className="text-red-600 hover:text-red-800 text-xs px-2 py-1 border border-red-300 rounded hover:bg-red-50"
                        title="Remove from template"
                      >
                        Remove
                      </button>
                    </td>
                    <td className="px-3 py-2 text-sm">{index + 1}</td>
                    <td className="px-3 py-2 text-sm">{product.skuCode}</td>
                    <td className="px-3 py-2 text-sm">{product.skuName}</td>
                    <td className="px-3 py-2 text-sm text-center">{product.availQty}</td>
                    <td className="px-3 py-2 text-sm text-center">{product.modelQty?.toLocaleString() || 0}</td>
                    <td className="px-3 py-2 text-sm text-center">{product.inTransit?.toLocaleString() || 0}</td>
                    <td className="px-3 py-2 text-sm text-center">{product.booked || 0}</td>
                    <td className="px-3 py-2 text-sm text-center">{product.toFillQty || 0}</td>
                    <td className="px-3 py-2 text-sm text-center">{product.casePerPC}</td>
                    <td className="px-3 py-2 text-sm text-center">{product.suggestedOrderQty || 0}</td>
                    <td className="px-3 py-2 text-sm text-center">{product.uom}</td>
                    <td className="px-3 py-2 text-sm text-center">
                      <input
                        type="number"
                        value={product.orderQty || 0}
                        onChange={(e) => handleQuantityChange(product.id, e.target.value)}
                        className="w-16 px-2 py-1 border border-gray-300 rounded text-center"
                        min="0"
                      />
                    </td>
                    <td className="px-3 py-2 text-sm text-center">{product.past3MonthAverage}</td>
                  </tr>
                  )
                })
              )}
            </tbody>
          </table>
        </div>
      </div>

      {/* Action Buttons */}
      <div className="flex justify-start gap-3 mb-6">
        <Button
          onClick={handleCancel}
          variant="outline"
          className="border-gray-300 text-gray-700 hover:bg-gray-50 px-6 py-2"
        >
          Cancel
        </Button>
        <Button
          onClick={handleSave}
          className="bg-blue-600 hover:bg-blue-700 text-white px-6 py-2"
          disabled={!templateData.templateName || !templateData.plant || templateData.plant === 'Select'}
        >
          Save Template
        </Button>
        </div>

        {/* Product Selection Modal */}
        <ProductSelectionModal
          isOpen={showProductModal}
          onClose={() => setShowProductModal(false)}
          onAddProducts={handleProductsAdded}
          existingProducts={products}
          selectedPlant={templateData.plant && templateData.plant !== 'Select' ? templateData.plant : null}
        />
      </div>
      </div>
    </BusinessLayout>
  )
}

export default AddPurchaseOrderTemplate