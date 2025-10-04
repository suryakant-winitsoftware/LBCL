"use client"
import { useState, useEffect } from 'react'
import { useRouter } from 'next/navigation'
import {
  Search,
  ChevronLeft,
  Plus,
  Trash2
} from 'lucide-react'
import { Button } from '../../../components/ui/button'
import { useAuth } from '../../../contexts/AuthContext'
import BusinessLayout from '../../../components/layouts/BusinessLayout'
import ProtectedRoute from '../../../components/ProtectedRoute'
import ProductSelectionModal from './ProductSelectionModal'

const EditPurchaseOrderTemplate = ({ templateId }) => {
  const { user } = useAuth()
  const router = useRouter()
  const [loading, setLoading] = useState(true)
  const [searchQuery, setSearchQuery] = useState('')
  const [selectedProducts, setSelectedProducts] = useState([])
  const [showProductModal, setShowProductModal] = useState(false)
  const [templateData, setTemplateData] = useState({
    templateName: 'Test',
    plant: 'N002',
    requestedBy: 'Remuera - G & M Paterson',
    date: '26 Aug, 2025',
    status: 'Saved',
    preparedBy: '[13010435]G & M Paterson',
    franchiseeDetails: {
      name: '[13010435] G & M Paterson',
      poBox: 'PO Box 25666',
      city: 'City: St Heliers',
      region: 'Region: Upper North Island',
      country: 'Country: New Zealand',
      postalCode: 'Postal Code: 1740',
      telephone: 'Telephone: 09 5735220',
      mobile: 'Mobile Phone:021 499006',
      address: '10 Southpark Pl\nPENROSE\nCity: Auckland\nRegion: Upper North Island\nCountry: New Zealand\nPostal Code: 1061\nTelephone: 021 499 006\nMobile Phone:09 575 7147'
    }
  })

  useEffect(() => {
    const fetchTemplateData = async () => {
      try {
        if (templateId) {
          // TODO: Fetch template data using the API when template management endpoints are available
          // const response = await purchaseOrderService.getPurchaseOrderMasterByUID(templateId)
          // For now, start with empty products - user can add products manually
          setSelectedProducts([])
        } else {
          setSelectedProducts([])
        }
      } catch (error) {
        console.error('Error fetching template data:', error)
      } finally {
        setLoading(false)
      }
    }
    fetchTemplateData()
  }, [templateId])

  const handleQuantityChange = (productId, value) => {
    setSelectedProducts(products =>
      products.map(product =>
        product.id === productId
          ? { ...product, orderQty: parseInt(value) || 0 }
          : product
      )
    )
  }

  const handleSelectProduct = (productId) => {
    setSelectedProducts(products =>
      products.map(product =>
        product.id === productId
          ? { ...product, selected: !product.selected }
          : product
      )
    )
  }

  const handleSelectAll = () => {
    const allSelected = selectedProducts.every(p => p.selected)
    setSelectedProducts(products =>
      products.map(product => ({ ...product, selected: !allSelected }))
    )
  }

  const handleDeleteSelected = () => {
    setSelectedProducts(products => products.filter(p => !p.selected))
  }

  const handleSave = () => {
    console.log('Saving template:', { templateData, selectedProducts })
    router.push('/user/manager/purchase-order-templates')
  }

  const handleCancel = () => {
    router.push('/user/manager/purchase-order-templates')
  }

  const handleClearOrder = () => {
    setSelectedProducts(products =>
      products.map(product => ({ ...product, orderQty: 0 }))
    )
  }

  const handleAddProduct = () => {
    setShowProductModal(true)
  }

  const handleProductsAdded = (newProducts) => {
    setSelectedProducts(prevProducts => {
      const updatedProducts = [...prevProducts, ...newProducts]
      console.log('Products added to template:', newProducts)
      console.log('Updated template products:', updatedProducts)
      return updatedProducts
    })
  }

  const calculateTotal = () => {
    return selectedProducts.reduce((sum, product) => sum + (product.orderQty || 0), 0)
  }

  return (
    <ProtectedRoute requiredSystem="stock">
      <BusinessLayout title="Edit Purchase Order Template">
        <div className="min-h-screen bg-[var(--background)] p-4 sm:p-6">
          {/* Header Section */}
          <div className="bg-[var(--card)] rounded-lg shadow-sm border border-[var(--border)] p-6 mb-6">
            <div className="flex flex-col space-y-4">
              <div>
                <h1 className="text-2xl font-bold text-[var(--foreground)]">Edit Purchase Order Template</h1>
                <div className="flex items-center gap-2 mt-2 text-sm text-[var(--muted-foreground)]">
                  <span>Home</span>
                  <span>»</span>
                  <span>Purchase Order Templates</span>
                  <span>»</span>
                  <span className="text-[var(--primary)]">Edit Purchase Order Template</span>
                </div>
              </div>

              {/* Template Details */}
              <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
                {/* Left Column */}
                <div className="space-y-4">
                  <div>
                    <label className="text-sm font-medium text-[var(--destructive)]">
                      *Template Name
                    </label>
                    <input
                      type="text"
                      value={templateData.templateName}
                      onChange={(e) => setTemplateData({...templateData, templateName: e.target.value})}
                      className="mt-1 w-full px-3 py-2 border border-[var(--border)] rounded-md bg-[var(--background)] text-[var(--foreground)]"
                    />
                  </div>

                  <div className="space-y-2">
                    <div>
                      <label className="text-sm font-medium text-[var(--foreground)]">Prepared By</label>
                      <div className="mt-1 text-sm text-[var(--foreground)]">{templateData.preparedBy}</div>
                    </div>
                  </div>
                </div>

                {/* Middle Column */}
                <div className="space-y-4">
                  <div>
                    <label className="text-sm font-medium text-[var(--foreground)]">Plant</label>
                    <div className="mt-1 text-sm text-[var(--foreground)]">{templateData.plant}</div>
                  </div>

                  <div>
                    <label className="text-sm font-medium text-[var(--foreground)]">Date</label>
                    <div className="mt-1 text-sm text-[var(--foreground)]">{templateData.date}</div>
                  </div>

                  <div>
                    <label className="text-sm font-medium text-[var(--foreground)]">Status</label>
                    <div className="mt-1 text-sm text-[var(--foreground)]">{templateData.status}</div>
                  </div>
                </div>

                {/* Right Column */}
                <div className="space-y-4">
                  <div>
                    <label className="text-sm font-medium text-[var(--foreground)]">Requested By</label>
                    <select
                      value={templateData.requestedBy}
                      onChange={(e) => setTemplateData({...templateData, requestedBy: e.target.value})}
                      className="mt-1 w-full px-3 py-2 border border-[var(--border)] rounded-md bg-[var(--background)] text-[var(--foreground)]"
                    >
                      <option value="Remuera - G & M Paterson">Remuera - G & M Paterson</option>
                    </select>
                  </div>

                  <div>
                    <label className="text-sm font-medium text-[var(--foreground)]">Requested By Franchisee</label>
                    <div className="mt-1 p-3 border border-[var(--border)] rounded-md bg-[var(--muted)] text-xs">
                      <div>{templateData.franchiseeDetails.name}</div>
                      <div>{templateData.franchiseeDetails.poBox}</div>
                      <div>{templateData.franchiseeDetails.city}</div>
                      <div>{templateData.franchiseeDetails.region}</div>
                      <div>{templateData.franchiseeDetails.country}</div>
                      <div>{templateData.franchiseeDetails.postalCode}</div>
                      <div>{templateData.franchiseeDetails.telephone}</div>
                      <div>{templateData.franchiseeDetails.mobile}</div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>

          {/* Search and Actions Bar */}
          <div className="bg-[var(--card)] rounded-lg shadow-sm border border-[var(--border)] p-4 mb-4">
            <div className="flex flex-col sm:flex-row items-center gap-4">
              <div className="flex-1 relative">
                <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-[var(--muted-foreground)] w-4 h-4" />
                <input
                  type="text"
                  placeholder="Search"
                  value={searchQuery}
                  onChange={(e) => setSearchQuery(e.target.value)}
                  className="w-full pl-10 pr-4 py-2 border border-[var(--border)] rounded-md bg-[var(--background)] text-[var(--foreground)]"
                />
              </div>
              <div className="flex gap-2">
                <Button
                  variant="outline"
                  onClick={handleClearOrder}
                  className="border-[var(--border)] text-[var(--foreground)] hover:bg-[var(--muted)]"
                >
                  Clear Order Qty
                </Button>
                <Button
                  onClick={handleAddProduct}
                  className="bg-[var(--primary)] hover:bg-[var(--primary-hover)] text-white"
                >
                  Add Product to Template
                </Button>
              </div>
            </div>
          </div>

          {/* Products Table */}
          <div className="bg-[var(--card)] rounded-lg shadow-sm border border-[var(--border)] overflow-hidden mb-4">
            <div className="overflow-x-auto">
              <table className="min-w-full divide-y divide-[var(--border)]">
                <thead className="bg-[var(--muted)]">
                  <tr>
                    <th className="px-4 py-3 text-center">
                      <input
                        type="checkbox"
                        onChange={handleSelectAll}
                        checked={selectedProducts.length > 0 && selectedProducts.every(p => p.selected)}
                        className="rounded border-[var(--border)]"
                      />
                    </th>
                    <th className="px-4 py-3 text-center text-xs font-medium text-[var(--muted-foreground)] uppercase">#</th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-[var(--muted-foreground)] uppercase">SKUCode</th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-[var(--muted-foreground)] uppercase">SKU Name</th>
                    <th className="px-6 py-3 text-center text-xs font-medium text-[var(--muted-foreground)] uppercase">Avl Qty</th>
                    <th className="px-6 py-3 text-center text-xs font-medium text-[var(--muted-foreground)] uppercase">Model Qty</th>
                    <th className="px-6 py-3 text-center text-xs font-medium text-[var(--muted-foreground)] uppercase">In Transit</th>
                    <th className="px-6 py-3 text-center text-xs font-medium text-[var(--muted-foreground)] uppercase">Booked</th>
                    <th className="px-6 py-3 text-center text-xs font-medium text-[var(--muted-foreground)] uppercase">To Fill Qty</th>
                    <th className="px-6 py-3 text-center text-xs font-medium text-[var(--muted-foreground)] uppercase">Case Per PC</th>
                    <th className="px-6 py-3 text-center text-xs font-medium text-[var(--muted-foreground)] uppercase">Suggested Order Qty</th>
                    <th className="px-6 py-3 text-center text-xs font-medium text-[var(--muted-foreground)] uppercase">UOM</th>
                    <th className="px-6 py-3 text-center text-xs font-medium text-[var(--muted-foreground)] uppercase">Order Qty</th>
                    <th className="px-6 py-3 text-center text-xs font-medium text-[var(--muted-foreground)] uppercase">Past 3 Month Average</th>
                  </tr>
                </thead>
                <tbody className="bg-[var(--card)] divide-y divide-[var(--border)]">
                  {loading ? (
                    <tr>
                      <td colSpan="14" className="px-6 py-4 text-center">
                        <div className="animate-pulse">Loading...</div>
                      </td>
                    </tr>
                  ) : selectedProducts.length === 0 ? (
                    <tr>
                      <td colSpan="14" className="px-6 py-12 text-center">
                        <div className="text-[var(--muted-foreground)]">No products added to template</div>
                      </td>
                    </tr>
                  ) : (
                    selectedProducts.map((product, index) => (
                      <tr key={product.id} className="hover:bg-[var(--muted)]">
                        <td className="px-4 py-3 text-center">
                          <input
                            type="checkbox"
                            checked={product.selected || false}
                            onChange={() => handleSelectProduct(product.id)}
                            className="rounded border-[var(--border)]"
                          />
                        </td>
                        <td className="px-4 py-3 text-center text-sm">{index + 1}</td>
                        <td className="px-6 py-3 text-sm">{product.skuCode}</td>
                        <td className="px-6 py-3 text-sm">{product.skuName}</td>
                        <td className="px-6 py-3 text-center text-sm">{product.availQty}</td>
                        <td className="px-6 py-3 text-center text-sm">{product.modelQty.toLocaleString()}</td>
                        <td className="px-6 py-3 text-center text-sm">{product.inTransit.toLocaleString()}</td>
                        <td className="px-6 py-3 text-center text-sm">{product.booked}</td>
                        <td className="px-6 py-3 text-center text-sm">{product.toFillQty}</td>
                        <td className="px-6 py-3 text-center text-sm">{product.casePerPC}</td>
                        <td className="px-6 py-3 text-center text-sm">{product.suggestedOrderQty}</td>
                        <td className="px-6 py-3 text-center text-sm">{product.uom}</td>
                        <td className="px-6 py-3 text-center">
                          <input
                            type="number"
                            value={product.orderQty}
                            onChange={(e) => handleQuantityChange(product.id, e.target.value)}
                            className="w-20 px-2 py-1 border border-[var(--border)] rounded text-center"
                            min="0"
                          />
                        </td>
                        <td className="px-6 py-3 text-center text-sm">{product.past3MonthAvg}</td>
                      </tr>
                    ))
                  )}
                  {selectedProducts.length > 0 && (
                    <tr className="bg-[var(--muted)] font-semibold">
                      <td colSpan="12" className="px-6 py-3 text-right text-sm">Total</td>
                      <td className="px-6 py-3 text-center text-sm">{calculateTotal()}</td>
                      <td></td>
                    </tr>
                  )}
                </tbody>
              </table>
            </div>
          </div>

          {/* Action Buttons */}
          <div className="bg-[var(--card)] rounded-lg shadow-sm border border-[var(--border)] p-4">
            <div className="flex items-center justify-center gap-4">
              <Button
                variant="outline"
                onClick={handleDeleteSelected}
                disabled={!selectedProducts.some(p => p.selected)}
                className="border-[var(--border)] text-[var(--foreground)] hover:bg-[var(--muted)]"
              >
                Delete Selected From List
              </Button>
              <Button
                onClick={handleSave}
                className="bg-[var(--primary)] hover:bg-[var(--primary-hover)] text-white px-8"
              >
                Save
              </Button>
              <Button
                variant="outline"
                onClick={handleCancel}
                className="border-[var(--border)] text-[var(--foreground)] hover:bg-[var(--muted)]"
              >
                Cancel
              </Button>
            </div>
          </div>

          {/* Product Selection Modal */}
          <ProductSelectionModal
            isOpen={showProductModal}
            onClose={() => setShowProductModal(false)}
            onAddProducts={handleProductsAdded}
            existingProducts={selectedProducts}
          />
        </div>
      </BusinessLayout>
    </ProtectedRoute>
  )
}

export default EditPurchaseOrderTemplate