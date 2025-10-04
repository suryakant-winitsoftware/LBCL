"use client"
import { useState, useEffect } from 'react'
import { useRouter } from 'next/navigation'
import { 
  Search,
  Plus,
  Image as ImageIcon,
  Clock,
  X,
  Save,
  CheckCircle,
  AlertTriangle,
  Package,
  FileText
} from 'lucide-react'
import { Button } from '../../../components/ui/button'
import ProtectedRoute from '../../../components/ProtectedRoute'
import BusinessLayout from '../../../components/layouts/BusinessLayout'

const PhysicalCountEdit = () => {
  const router = useRouter()
  const [searchTerm, setSearchTerm] = useState('')
  const [showAlert, setShowAlert] = useState(false)
  const [showSuccess, setShowSuccess] = useState(false)
  const [showAddItemModal, setShowAddItemModal] = useState(false)
  const [newItem, setNewItem] = useState({
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
        setProducts(data.products)
        setDeliveryInfo(data.deliveryData)
      } catch (error) {
        console.error('Error fetching data:', error)
      }
    }
    fetchData()
  }, [])

  const handlePreview = () => {
    setShowAlert(true)
  }

  const handleAlertClose = () => {
    setShowAlert(false)
    setShowSuccess(true)
  }

  const handleSuccessClose = () => {
    setShowSuccess(false)
    router.push('/user/manager/stock-receiving-history')
  }

  const updateProduct = (id, field, value) => {
    setProducts(prev => prev.map(product => 
      product.id === id ? { ...product, [field]: value } : product
    ))
  }

  const handleAddItem = () => {
    setShowAddItemModal(true)
  }

  const handleSearch = () => {
    console.log('Searching for:', searchTerm)
  }

  const handleNewItemChange = (field, value) => {
    setNewItem(prev => ({
      ...prev,
      [field]: value
    }))
  }

  const handleAddNewItem = () => {
    // Validate form
    if (!newItem.name.trim()) {
      alert('Please enter an item name')
      return
    }
    if (!newItem.sku.trim()) {
      alert('Please enter a SKU code')
      return
    }
    if (!newItem.quantity || parseInt(newItem.quantity) <= 0) {
      alert('Please enter a valid quantity (greater than 0)')
      return
    }

    // Check if SKU already exists
    const existingProduct = products.find(p => p.id === newItem.sku.trim())
    if (existingProduct) {
      alert('A product with this SKU already exists!')
      return
    }

    // Create new product object
    const newProductItem = {
      id: newItem.sku.trim(),
      name: newItem.name.trim(),
      image: '📦',
      deliveryPlanQty: parseInt(newItem.quantity),
      shippedQty: parseInt(newItem.quantity),
      receivedQty: parseInt(newItem.quantity),
      adjustmentReason: 'N/A',
      qty: 0
    }

    // Add to products array
    setProducts(prev => [...prev, newProductItem])

    // Show success message
    alert(`✅ Item "${newItem.name}" has been successfully added!`)

    // Reset form and close modal
    setNewItem({ name: '', sku: '', quantity: '' })
    setShowAddItemModal(false)

    console.log('New item added:', newProductItem)
  }

  const filteredProducts = products.filter(product =>
    product.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
    product.id.toLowerCase().includes(searchTerm.toLowerCase())
  )

  return (
    <ProtectedRoute requiredSystem="stock">
      <BusinessLayout title="Stock Receiving">
        <div className="min-h-screen bg-gray-50 p-6">
          {/* Header */}
          <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6 mb-6">
            <div className="flex items-center justify-between">
              <div>
                <h1 className="text-2xl font-bold text-gray-900 mb-2">Physical Count & Stock Receiving</h1>
                <div className="flex flex-wrap items-center gap-4 text-sm text-gray-600">
                  <div className="flex items-center gap-2">
                    <FileText className="w-4 h-4" />
                    <span>Plan: <span className="font-semibold">{deliveryInfo.deliveryPlanNo}</span></span>
                  </div>
                  <div className="flex items-center gap-2">
                    <Package className="w-4 h-4" />
                    <span>SAP: <span className="font-semibold">{deliveryInfo.sapDeliveryNoteNo}</span></span>
                  </div>
                  <div>Date: <span className="font-semibold">{deliveryInfo.orderDate}</span></div>
                  <div>SKUs: <span className="font-semibold">{deliveryInfo.uniqueSkuCount}</span></div>
                </div>
              </div>
              <div className="bg-blue-50 border border-blue-200 rounded-lg px-4 py-3 flex items-center gap-3">
                <Clock className="w-5 h-5 text-blue-600" />
                <div>
                  <div className="font-semibold text-blue-900">Duration</div>
                  <div className="text-sm text-blue-700">24m 15s</div>
                </div>
              </div>
            </div>
          </div>

          {/* Search Bar */}
          <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6 mb-6">
            <div className="flex flex-col sm:flex-row items-start sm:items-center gap-4">
              <div className="flex-1 w-full sm:w-auto">
                <div className="relative">
                  <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 w-5 h-5 text-gray-400" />
                  <input
                    type="text"
                    placeholder="Search item code or description..."
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
                  onClick={handleAddItem}
                  variant="outline" 
                  className="px-4 py-3 border-gray-300 text-gray-700 hover:bg-gray-50"
                >
                  <Plus className="w-4 h-4 mr-2" />
                  Add Item
                </Button>
                <Button 
                  onClick={handlePreview}
                  className="px-4 py-3 bg-blue-600 hover:bg-blue-700 text-white"
                >
                  Preview
                </Button>
              </div>
            </div>
          </div>

          {/* Table */}
          <div className="bg-white rounded-lg shadow-sm border border-gray-200 mb-6 overflow-hidden">
            <div className="overflow-x-auto">
              <table className="w-full">
                <thead className="bg-gray-900 text-white">
                  <tr>
                    <th className="text-left px-6 py-4 font-semibold text-sm">ITEM DETAILS</th>
                    <th className="text-center px-4 py-4 font-semibold text-sm">PLAN</th>
                    <th className="text-center px-4 py-4 font-semibold text-sm">SHIP</th>
                    <th className="text-center px-4 py-4 font-semibold text-sm">RECV</th>
                    <th className="text-center px-4 py-4 font-semibold text-sm">REASON</th>
                    <th className="text-center px-4 py-4 font-semibold text-sm">ADJ QTY</th>
                    <th className="text-center px-4 py-4 font-semibold text-sm">ACTION</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-gray-200">
                  {filteredProducts.map((product, index) => (
                    <tr key={product.id} className="hover:bg-gray-50 transition-colors duration-150">
                      <td className="px-6 py-4">
                        <div className="flex items-center gap-4">
                          <div className="w-12 h-12 bg-gray-100 rounded-lg flex items-center justify-center border border-gray-200">
                            <span className="text-2xl">{product.image}</span>
                          </div>
                          <div>
                            <div className="font-semibold text-gray-900">{product.name}</div>
                            <div className="text-sm text-blue-600">{product.id}</div>
                          </div>
                        </div>
                      </td>
                      <td className="text-center px-4 py-4 font-semibold text-gray-900">{product.deliveryPlanQty}</td>
                      <td className="text-center px-4 py-4 font-semibold text-gray-900">{product.shippedQty}</td>
                      <td className="text-center px-4 py-4">
                        <input
                          type="number"
                          value={product.receivedQty}
                          onChange={(e) => updateProduct(product.id, 'receivedQty', parseInt(e.target.value) || 0)}
                          className="w-20 px-3 py-2 text-center border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all duration-200"
                        />
                      </td>
                      <td className="text-center px-4 py-4">
                        <select
                          value={product.adjustmentReason}
                          onChange={(e) => updateProduct(product.id, 'adjustmentReason', e.target.value)}
                          className="px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all duration-200"
                        >
                          <option value="N/A">No Issue</option>
                          <option value="Leakage">Leakage</option>
                          <option value="Damage">Damage</option>
                          <option value="Expiry">Expiry</option>
                        </select>
                      </td>
                      <td className="text-center px-4 py-4">
                        <input
                          type="number"
                          value={product.qty}
                          onChange={(e) => updateProduct(product.id, 'qty', parseInt(e.target.value) || 0)}
                          disabled={product.adjustmentReason === 'N/A'}
                          className={`w-20 px-3 py-2 text-center border rounded-lg transition-all duration-200 ${
                            product.adjustmentReason === 'N/A' 
                              ? 'border-gray-200 bg-gray-50 text-gray-400 cursor-not-allowed' 
                              : 'border-gray-300 focus:ring-2 focus:ring-blue-500 focus:border-transparent'
                          }`}
                        />
                      </td>
                      <td className="text-center px-4 py-4">
                        <div className="flex justify-center gap-2">
                          <button className="w-8 h-8 flex items-center justify-center rounded-lg border border-gray-300 hover:bg-gray-50 transition-colors duration-150">
                            <ImageIcon className="w-4 h-4 text-gray-600" />
                          </button>
                          <button className="w-8 h-8 flex items-center justify-center rounded-lg bg-blue-600 hover:bg-blue-700 text-white transition-colors duration-150">
                            <Plus className="w-4 h-4" />
                          </button>
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>

          {/* Footer */}
          <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
            <div className="flex flex-col lg:flex-row justify-between items-start lg:items-center gap-4">
              <div className="text-sm text-gray-600 space-y-1">
                <div>Total Items: <span className="font-semibold text-gray-900">{products.length}</span></div>
                <div>Total Quantity: <span className="font-semibold text-gray-900">{products.reduce((sum, p) => sum + p.deliveryPlanQty, 0)}</span></div>
                <div>Received: <span className="font-semibold text-gray-900">{products.reduce((sum, p) => sum + p.receivedQty, 0)}</span></div>
              </div>
              <div className="flex gap-3">
                <Button variant="outline" className="px-6 py-3 border-gray-300 text-gray-700 hover:bg-gray-50">
                  <Save className="w-4 h-4 mr-2" />
                  Save Draft
                </Button>
                <Button 
                  onClick={handlePreview}
                  className="px-6 py-3 bg-green-600 hover:bg-green-700 text-white font-semibold"
                >
                  <CheckCircle className="w-4 h-4 mr-2" />
                  Complete Receiving
                </Button>
              </div>
            </div>
          </div>

          {/* Alert Modal */}
          {showAlert && (
            <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
              <div className="bg-white rounded-lg max-w-md w-full mx-4 shadow-xl">
                <div className="flex items-center justify-between p-6 border-b border-gray-200">
                  <div className="flex items-center gap-3">
                    <AlertTriangle className="w-6 h-6 text-orange-600" />
                    <h3 className="text-lg font-semibold text-gray-900">Confirm Action</h3>
                  </div>
                  <button 
                    onClick={() => setShowAlert(false)}
                    className="text-gray-400 hover:text-gray-600 transition-colors duration-150"
                  >
                    <X className="w-6 h-6" />
                  </button>
                </div>
                
                <div className="p-6">
                  <p className="text-gray-700 mb-4">
                    Quantities must match between plan, shipped, and received.
                  </p>
                  <div className="bg-gray-50 rounded-lg p-4 mb-6">
                    <ul className="text-sm text-gray-600 space-y-2">
                      <li className="flex items-center gap-2">
                        <CheckCircle className="w-4 h-4 text-green-600" />
                        System will mark shipped quantity as received
                      </li>
                      <li className="flex items-center gap-2">
                        <CheckCircle className="w-4 h-4 text-green-600" />
                        Damaged items logged separately
                      </li>
                      <li className="flex items-center gap-2">
                        <CheckCircle className="w-4 h-4 text-green-600" />
                        Move damaged stock to designated area
                      </li>
                    </ul>
                  </div>

                  <div className="flex gap-3 justify-end">
                    <Button 
                      variant="outline"
                      onClick={() => setShowAlert(false)}
                      className="px-4 py-2 border-gray-300 text-gray-700 hover:bg-gray-50"
                    >
                      Cancel
                    </Button>
                    <Button 
                      onClick={handleAlertClose}
                      className="px-4 py-2 bg-orange-600 hover:bg-orange-700 text-white"
                    >
                      Proceed
                    </Button>
                  </div>
                </div>
              </div>
            </div>
          )}

          {/* Success Modal */}
          {showSuccess && (
            <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
              <div className="bg-white rounded-lg max-w-md w-full mx-4 shadow-xl">
                <div className="flex items-center justify-between p-6 border-b border-gray-200">
                  <div className="flex items-center gap-3">
                    <div className="w-8 h-8 bg-green-100 rounded-full flex items-center justify-center">
                      <CheckCircle className="w-5 h-5 text-green-600" />
                    </div>
                    <h3 className="text-lg font-semibold text-gray-900">Process Complete</h3>
                  </div>
                  <button 
                    onClick={() => setShowSuccess(false)}
                    className="text-gray-400 hover:text-gray-600 transition-colors duration-150"
                  >
                    <X className="w-6 h-6" />
                  </button>
                </div>
                
                <div className="p-6">
                  <div className="text-center mb-6">
                    <div className="w-16 h-16 bg-green-100 rounded-full flex items-center justify-center mx-auto mb-4">
                      <CheckCircle className="w-8 h-8 text-green-600" />
                    </div>
                    <h4 className="text-lg font-semibold text-gray-900 mb-2">Stock Receiving Completed</h4>
                    <div className="text-sm text-gray-600 space-y-1">
                      <p>Process completed in 24 minutes</p>
                      <p className="font-medium">20-May-2025 | 11:05 AM - 11:29 AM</p>
                    </div>
                  </div>

                  <div className="flex gap-3 justify-end">
                    <Button 
                      variant="outline"
                      className="px-4 py-2 border-gray-300 text-gray-700 hover:bg-gray-50"
                    >
                      Print Report
                    </Button>
                    <Button 
                      onClick={handleSuccessClose}
                      className="px-4 py-2 bg-green-600 hover:bg-green-700 text-white"
                    >
                      Close
                    </Button>
                  </div>
                </div>
              </div>
            </div>
          )}

          {/* Add Item Modal */}
          {showAddItemModal && (
            <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
              <div className="bg-white rounded-lg max-w-md w-full mx-4 shadow-xl">
                <div className="flex items-center justify-between p-6 border-b border-gray-200">
                  <h3 className="text-lg font-semibold text-gray-900">Add New Item</h3>
                  <button 
                    onClick={() => setShowAddItemModal(false)}
                    className="text-gray-400 hover:text-gray-600 transition-colors duration-150"
                  >
                    <X className="w-6 h-6" />
                  </button>
                </div>
                
                <div className="p-6">
                  <div className="space-y-4">
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-2">
                        Item Name *
                      </label>
                      <input
                        type="text"
                        value={newItem.name}
                        onChange={(e) => handleNewItemChange('name', e.target.value)}
                        placeholder="Enter item name"
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
                        value={newItem.sku}
                        onChange={(e) => handleNewItemChange('sku', e.target.value)}
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
                        value={newItem.quantity}
                        onChange={(e) => handleNewItemChange('quantity', e.target.value)}
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
                        setNewItem({ name: '', sku: '', quantity: '' })
                        setShowAddItemModal(false)
                      }}
                      className="px-4 py-2 border-gray-300 text-gray-700 hover:bg-gray-50"
                    >
                      Cancel
                    </Button>
                    <Button 
                      onClick={handleAddNewItem}
                      className="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white"
                    >
                      Add Item
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

export default PhysicalCountEdit