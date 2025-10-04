"use client"
import { useState, useEffect } from 'react'
import { useRouter } from 'next/navigation'
import { 
  Search,
  Plus,
  Image as ImageIcon,
  Clock,
  Package,
  FileText,
  TrendingUp,
  AlertCircle,
  X
} from 'lucide-react'
import { Button } from '../../../components/ui/button'
import BusinessLayout from '../../../components/layouts/BusinessLayout'
import ProtectedRoute from '../../../components/ProtectedRoute'

const PhysicalCount = () => {
  const router = useRouter()
  const [searchTerm, setSearchTerm] = useState('')
  const [showAddProductModal, setShowAddProductModal] = useState(false)
  const [newProduct, setNewProduct] = useState({
    name: '',
    sku: '',
    quantity: ''
  })
  
  // Fetch product data from centralized data.json
  const [products, setProducts] = useState([])
  const [deliveryInfo, setDeliveryInfo] = useState({})

  useEffect(() => {
    const fetchData = async () => {
      try {
        const response = await fetch('/data.json')
        const data = await response.json()
        setProducts(data.products.map(product => ({
          ...product,
          receivedQty: product.deliveryPlanQty,
          adjustmentReason: 'Not Applicable',
          qty: 0
        })))
        setDeliveryInfo(data.deliveryData)
      } catch (error) {
        console.error('Error fetching data:', error)
      }
    }
    fetchData()
  }, [])

  const handleSubmit = () => {
    router.push('/user/manager/physical-count-edit')
  }

  const handleAddProducts = () => {
    setShowAddProductModal(true)
    // Alternative: Navigate to add products page
    // router.push('/user/manager/add-products')
  }

  const handleSearch = () => {
    // Search functionality is already handled by the filteredProducts
    console.log('Searching for:', searchTerm)
  }

  const handleImageUpload = (productId) => {
    // Create a file input and trigger it
    const input = document.createElement('input')
    input.type = 'file'
    input.accept = 'image/*'
    input.onchange = (e) => {
      const file = e.target.files[0]
      if (file) {
        // Handle image upload logic here
        console.log('Uploading image for product:', productId, file)
        // You can implement actual image upload to your backend here
      }
    }
    input.click()
  }

  const handleAddToCount = (productId) => {
    setProducts(prev => prev.map(product => 
      product.id === productId 
        ? { ...product, qty: product.qty + 1 }
        : product
    ))
  }

  const handleNewProductChange = (field, value) => {
    setNewProduct(prev => ({
      ...prev,
      [field]: value
    }))
  }

  const handleAddNewProduct = () => {
    // Validate form
    if (!newProduct.name.trim()) {
      alert('Please enter a product name')
      return
    }
    if (!newProduct.sku.trim()) {
      alert('Please enter a SKU code')
      return
    }
    if (!newProduct.quantity || parseInt(newProduct.quantity) <= 0) {
      alert('Please enter a valid quantity (greater than 0)')
      return
    }

    // Check if SKU already exists
    const existingProduct = products.find(p => p.id === newProduct.sku.trim())
    if (existingProduct) {
      alert('A product with this SKU already exists!')
      return
    }

    // Create new product object
    const newProductItem = {
      id: newProduct.sku.trim(),
      name: newProduct.name.trim(),
      image: '📦', // Default emoji
      deliveryPlanQty: parseInt(newProduct.quantity),
      shippedQty: parseInt(newProduct.quantity),
      receivedQty: parseInt(newProduct.quantity),
      adjustmentReason: 'Not Applicable',
      qty: 0
    }

    // Add to products array
    setProducts(prev => [...prev, newProductItem])

    // Show success message
    alert(`✅ Product "${newProduct.name}" has been successfully added!`)

    // Reset form and close modal
    setNewProduct({ name: '', sku: '', quantity: '' })
    setShowAddProductModal(false)

    console.log('New product added:', newProductItem)
  }

  const filteredProducts = products.filter(product =>
    product.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
    product.id.toLowerCase().includes(searchTerm.toLowerCase())
  )

  return (
    <ProtectedRoute requiredSystem="stock">
      <BusinessLayout title="Physical Count & Stock Receiving">
        <div className="min-h-screen bg-gray-50 p-6">
          {/* Page Header */}
          <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6 mb-6">
            <div className="flex items-center justify-between">
              <div>
                <h1 className="text-2xl font-bold text-gray-900 mb-2">Physical Count & Stock Receiving</h1>
                <p className="text-gray-600">Verify and update stock quantities efficiently</p>
              </div>
              <div className="bg-blue-50 border border-blue-200 rounded-lg px-4 py-3 flex items-center gap-3">
                <Clock className="w-5 h-5 text-blue-600" />
                <div>
                  <div className="font-semibold text-blue-900">Processing Time</div>
                  <div className="text-sm text-blue-700">24:15 Minutes</div>
                </div>
              </div>
            </div>
          </div>

          {/* Delivery Information Card */}
          <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6 mb-6">
            <div className="flex items-center gap-3 mb-4">
              <FileText className="w-5 h-5 text-gray-700" />
              <h2 className="text-lg font-semibold text-gray-900">Delivery Information</h2>
            </div>
            <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
              <div className="bg-gray-50 rounded-lg p-4 border border-gray-100">
                <div className="text-sm text-gray-600 mb-1">Delivery Plan No</div>
                <div className="font-semibold text-gray-900">{deliveryInfo.deliveryPlanNo}</div>
              </div>
              <div className="bg-gray-50 rounded-lg p-4 border border-gray-100">
                <div className="text-sm text-gray-600 mb-1">SAP Note</div>
                <div className="font-semibold text-gray-900">{deliveryInfo.sapDeliveryNoteNo}</div>
              </div>
              <div className="bg-gray-50 rounded-lg p-4 border border-gray-100">
                <div className="text-sm text-gray-600 mb-1">Order Date</div>
                <div className="font-semibold text-gray-900">{deliveryInfo.orderDate}</div>
              </div>
              <div className="bg-gray-50 rounded-lg p-4 border border-gray-100">
                <div className="text-sm text-gray-600 mb-1">Unique SKUs</div>
                <div className="font-semibold text-gray-900">{deliveryInfo.uniqueSkuCount}</div>
              </div>
            </div>
          </div>

          {/* Search and Controls */}
          <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6 mb-6">
            <div className="flex flex-col sm:flex-row items-start sm:items-center gap-4">
              <div className="flex-1 w-full sm:w-auto">
                <div className="relative">
                  <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 w-5 h-5 text-gray-400" />
                  <input
                    type="text"
                    placeholder="Search by Item Code or Description..."
                    value={searchTerm}
                    onChange={(e) => setSearchTerm(e.target.value)}
                    className="w-full pl-10 pr-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all duration-200"
                  />
                </div>
              </div>
              <div className="flex gap-3">
                <Button 
                  onClick={handleSearch}
                  variant="outline"
                  className="px-4 py-3 border-gray-300 text-gray-700 hover:bg-gray-50"
                >
                  <Search className="w-4 h-4 mr-2" />
                  Search
                </Button>
                <Button 
                  onClick={handleAddProducts}
                  className="px-4 py-3 bg-blue-600 hover:bg-blue-700 text-white"
                >
                  <Plus className="w-4 h-4 mr-2" />
                  Add Products
                </Button>
              </div>
            </div>
          </div>

          {/* Products Table */}
          <div className="bg-white rounded-lg shadow-sm border border-gray-200 mb-6">
            <div className="border-b border-gray-200 p-6">
              <div className="flex items-center justify-between">
                <div className="flex items-center gap-3">
                  <Package className="w-5 h-5 text-gray-700" />
                  <h2 className="text-lg font-semibold text-gray-900">Product Inventory</h2>
                </div>
                <div className="flex items-center gap-2">
                  <TrendingUp className="w-4 h-4 text-green-600" />
                  <span className="text-sm text-gray-600">
                    {filteredProducts.length} items found
                  </span>
                </div>
              </div>
            </div>

            {/* Table */}
            <div className="overflow-x-auto">
              <table className="w-full">
                <thead className="bg-gray-50">
                  <tr>
                    <th className="text-left px-6 py-4 text-sm font-semibold text-gray-700 w-1/3">
                      SKU / Description
                    </th>
                    <th className="text-center px-4 py-4 text-sm font-semibold text-gray-700 w-20">
                      Plan Qty
                    </th>
                    <th className="text-center px-4 py-4 text-sm font-semibold text-gray-700 w-20">
                      Ship Qty
                    </th>
                    <th className="text-center px-4 py-4 text-sm font-semibold text-gray-700 w-20">
                      Recv Qty
                    </th>
                    <th className="text-center px-4 py-4 text-sm font-semibold text-gray-700 w-32">
                      Reason
                    </th>
                    <th className="text-center px-4 py-4 text-sm font-semibold text-gray-700 w-20">
                      Adj Qty
                    </th>
                    <th className="text-center px-4 py-4 text-sm font-semibold text-gray-700 w-16">
                      Image
                    </th>
                    <th className="text-center px-4 py-4 text-sm font-semibold text-gray-700 w-16">
                      Actions
                    </th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-gray-200">
                  {filteredProducts.map((product, index) => (
                    <tr 
                      key={product.id} 
                      className="hover:bg-gray-50 transition-colors duration-150"
                    >
                      <td className="px-6 py-4">
                        <div className="flex items-center gap-3">
                          <div className="w-12 h-12 bg-gray-100 rounded-lg flex items-center justify-center border border-gray-200 flex-shrink-0">
                            <span className="text-2xl">{product.image}</span>
                          </div>
                          <div className="min-w-0 flex-1">
                            <div className="font-semibold text-gray-900 truncate">{product.name}</div>
                            <div className="text-sm text-gray-500 truncate">{product.id}</div>
                          </div>
                        </div>
                      </td>
                      <td className="px-4 py-4 text-center">
                        <span className="font-semibold text-gray-900">{product.deliveryPlanQty}</span>
                      </td>
                      <td className="px-4 py-4 text-center">
                        <span className="font-semibold text-gray-900">{product.shippedQty}</span>
                      </td>
                      <td className="px-4 py-4 text-center">
                        <span className="font-semibold text-gray-900">{product.receivedQty}</span>
                      </td>
                      <td className="px-4 py-4 text-center">
                        <span className="inline-flex items-center px-2 py-1 rounded-md text-xs font-medium bg-gray-100 text-gray-700 border border-gray-200">
                          {product.adjustmentReason}
                        </span>
                      </td>
                      <td className="px-4 py-4 text-center">
                        <span className="inline-flex items-center px-2 py-1 rounded-md text-sm font-semibold bg-blue-50 text-blue-700 border border-blue-200">
                          {product.qty}
                        </span>
                      </td>
                      <td className="px-4 py-4 text-center">
                        <button 
                          onClick={() => handleImageUpload(product.id)}
                          title="Upload image"
                          className="w-8 h-8 flex items-center justify-center rounded-lg border border-gray-300 hover:bg-gray-50 transition-colors duration-150 mx-auto"
                        >
                          <ImageIcon className="w-4 h-4 text-gray-600" />
                        </button>
                      </td>
                      <td className="px-4 py-4 text-center">
                        <button 
                          onClick={() => handleAddToCount(product.id)}
                          title="Add to count"
                          className="w-8 h-8 flex items-center justify-center rounded-lg bg-blue-600 hover:bg-blue-700 text-white transition-colors duration-150 mx-auto"
                        >
                          <Plus className="w-4 h-4" />
                        </button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>

          {/* Submit Section */}
          <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
            <div className="flex items-center justify-between">
              <div className="flex items-center gap-4">
                <div className="w-12 h-12 bg-green-100 rounded-lg flex items-center justify-center">
                  <AlertCircle className="w-6 h-6 text-green-600" />
                </div>
                <div>
                  <h3 className="text-lg font-semibold text-gray-900">Complete Physical Count</h3>
                  <p className="text-gray-600 mt-1">
                    Review all items and submit the physical count results for processing
                  </p>
                </div>
              </div>
              <Button 
                onClick={handleSubmit}
                className="px-6 py-3 bg-green-600 hover:bg-green-700 text-white font-semibold"
              >
                Submit Physical Count
              </Button>
            </div>
          </div>

          {/* Add Products Modal */}
          {showAddProductModal && (
            <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
              <div className="bg-white rounded-lg max-w-md w-full mx-4 shadow-xl">
                <div className="flex items-center justify-between p-6 border-b border-gray-200">
                  <h3 className="text-lg font-semibold text-gray-900">Add New Product</h3>
                  <button 
                    onClick={() => setShowAddProductModal(false)}
                    className="text-gray-400 hover:text-gray-600 transition-colors duration-150"
                  >
                    <X className="w-6 h-6" />
                  </button>
                </div>
                
                <div className="p-6">
                  <div className="space-y-4">
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-2">
                        Product Name *
                      </label>
                      <input
                        type="text"
                        value={newProduct.name}
                        onChange={(e) => handleNewProductChange('name', e.target.value)}
                        placeholder="Enter product name"
                        className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                        required
                      />
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-2">
                        SKU Code *
                      </label>
                      <input
                        type="text"
                        value={newProduct.sku}
                        onChange={(e) => handleNewProductChange('sku', e.target.value)}
                        placeholder="Enter SKU code"
                        className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                        required
                      />
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-2">
                        Quantity *
                      </label>
                      <input
                        type="number"
                        value={newProduct.quantity}
                        onChange={(e) => handleNewProductChange('quantity', e.target.value)}
                        placeholder="Enter quantity"
                        min="1"
                        className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                        required
                      />
                    </div>
                  </div>

                  <div className="flex gap-3 justify-end mt-6">
                    <Button 
                      variant="outline"
                      onClick={() => {
                        setNewProduct({ name: '', sku: '', quantity: '' })
                        setShowAddProductModal(false)
                      }}
                      className="px-4 py-2 border-gray-300 text-gray-700 hover:bg-gray-50"
                    >
                      Cancel
                    </Button>
                    <Button 
                      onClick={handleAddNewProduct}
                      className="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white"
                    >
                      Add Product
                    </Button>
                  </div>
                </div>
              </div>
            </div>
          )}
        </div>
      </BusinessLayout>
    </ProtectedRoute>
  )
}

export default PhysicalCount