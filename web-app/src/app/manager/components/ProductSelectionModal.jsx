"use client"
import { useState, useEffect } from 'react'
import { Search, X, Check, RefreshCw, AlertCircle } from 'lucide-react'
import { Button } from '../../../components/ui/button'

const ProductSelectionModal = ({ isOpen, onClose, onAddProducts, existingProducts = [], selectedPlant = null }) => {
  const [products, setProducts] = useState([])
  const [loading, setLoading] = useState(true)
  const [searchQuery, setSearchQuery] = useState('')
  const [selectedProducts, setSelectedProducts] = useState([])
  const [currentPage, setCurrentPage] = useState(1)
  const [totalCount, setTotalCount] = useState(0)
  const [error, setError] = useState('')
  const [mismatchWarning, setMismatchWarning] = useState(false)
  const itemsPerPage = 10

  useEffect(() => {
    if (isOpen && selectedPlant) {
      setCurrentPage(1)
      fetchProducts(1)
    } else if (isOpen && !selectedPlant) {
      // Clear products when no plant is selected
      setProducts([])
      setTotalCount(0)
      setLoading(false)
    }
  }, [isOpen, selectedPlant])

  useEffect(() => {
    if (isOpen && selectedPlant) {
      fetchProducts(currentPage)
    }
  }, [currentPage])

  const fetchProducts = async (page = 1) => {
    try {
      setLoading(true)
      setError('')

      // Don't fetch if no plant is selected
      if (!selectedPlant) {
        setProducts([])
        setTotalCount(0)
        setLoading(false)
        return
      }

      // Temporarily set auth token if not present
      if (typeof window !== 'undefined' && !localStorage.getItem('authToken')) {
        console.log('⚠️ Setting temporary auth token for testing');
        localStorage.setItem('authToken', 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiQURNSU4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3VzZXJkYXRhIjoiYWRtaW4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBZG1pbiIsInBlcm1pc3Npb25zIjpbInJlYWQiLCJ3cml0ZSJdLCJleHAiOjE3NTg3ODQ0MjIsImlzcyI6Im15aXNzdWVyIn0.ik92mNR1NYUR0m5R1UvciIjEKu-qSZoqoio78ancnm8');
      }

      // Import SKU service dynamically
      const skuService = (await import('../../../services/sku')).default

      console.log('🔍 Fetching products for template selection...')

      // Fetch products from API
      const response = await skuService.getAllSKUDetails({
        page: page,
        pageSize: itemsPerPage, // Fetch 10 products per page
        searchQuery: searchQuery.trim() || undefined,
        orgUID: selectedPlant || undefined
      })

      console.log('📦 Product fetch response:', response)

      if (response.success) {
        // Debug: Log pricing information from first product
        if (response.products && response.products.length > 0) {
          const firstProduct = response.products[0]
          console.log('💰 First product pricing fields:', {
            unitPrice: firstProduct.unitPrice,
            UnitPrice: firstProduct.UnitPrice,
            price: firstProduct.price,
            Price: firstProduct.Price,
            basePrice: firstProduct.basePrice,
            BasePrice: firstProduct.BasePrice,
            sellingPrice: firstProduct.sellingPrice,
            SellingPrice: firstProduct.SellingPrice,
            dp: firstProduct.dp,
            DP: firstProduct.DP,
            mrp: firstProduct.mrp,
            MRP: firstProduct.MRP,
            allKeys: Object.keys(firstProduct).filter(key => key.toLowerCase().includes('price') || key.toLowerCase().includes('dp') || key.toLowerCase().includes('mrp'))
          })
        }

        // Check if products belong to the selected plant
        let hasMismatch = false
        if (selectedPlant && response.products.length > 0) {
          const firstProductOrg = response.products[0].orgUID || response.products[0].OrgUID
          if (firstProductOrg && firstProductOrg !== selectedPlant) {
            console.warn(`⚠️ Backend returned products for ${firstProductOrg} instead of ${selectedPlant}`)
            hasMismatch = true
          }
        }
        setMismatchWarning(hasMismatch)

        // Don't filter existing products here - show all products from API
        // Users can see which ones are already added and skip them
        const availableProducts = response.products.map(product => {
          // Extract unit price with multiple fallbacks
          const extractedPrice = product.unitPrice || product.UnitPrice || product.price || product.Price ||
                    product.basePrice || product.BasePrice || product.sellingPrice || product.SellingPrice ||
                    product.salePrice || product.SalePrice || product.dp || product.DP ||
                    product.dpPrice || product.DPPrice || product.mrp || product.MRP || 100

          return {
            // Ensure consistent product structure
            id: product.id || product.uid || product.skuCode,
            uid: product.uid || product.id,
            skuuid: product.skuuid || product.uid || product.id,
            skuCode: product.skuCode,
            skuName: product.skuName,
            availQty: product.availQty || 0,
            modelQty: product.modelQty || 0,
            inTransit: product.inTransit || 0,
            booked: product.booked || 0,
            toFillQty: product.toFillQty || 0,
            casePerPC: product.eaPerCase || 1,
            suggestedOrderQty: product.suggestedOrderQty || 0,
            uom: product.uom || 'OU',
            orderQty: 0, // Initialize for template
            past3MonthAverage: product.past3MonthAverage || 0,
            orgUID: product.orgUID || product.OrgUID, // Keep track of organization

            // Pricing information - check ALL possible field names from API
            unitPrice: extractedPrice,
            basePrice: product.basePrice || product.BasePrice || extractedPrice,
            mrp: product.mrp || product.MRP || extractedPrice,
            dpPrice: product.dpPrice || product.DPPrice || product.dp || product.DP || extractedPrice,
            sellingPrice: product.sellingPrice || product.SellingPrice || extractedPrice,
            value: 0, // Initialize value as 0

            isAlreadyInTemplate: false // Will be updated below
          }
        })

        // Mark products that are already in the template
        const existingProductIds = existingProducts.map(p => p.id || p.uid || p.skuCode)
        availableProducts.forEach(product => {
          product.isAlreadyInTemplate = existingProductIds.includes(product.id)
        })

        console.log(`✅ Loaded ${availableProducts.length} products for plant: ${selectedPlant || 'all plants'}`)
        if (availableProducts.length > 0) {
          const alreadyInTemplate = availableProducts.filter(p => p.isAlreadyInTemplate).length
          console.log(`📋 ${alreadyInTemplate} products already in template`)
          console.log(`💰 Sample product with pricing:`, {
            skuCode: availableProducts[0].skuCode,
            skuName: availableProducts[0].skuName,
            unitPrice: availableProducts[0].unitPrice,
            basePrice: availableProducts[0].basePrice,
            mrp: availableProducts[0].mrp
          })
        }

        setProducts(availableProducts)
        // Use backend's total count for proper pagination
        setTotalCount(response.totalCount || 0)
      } else {
        console.error('❌ Failed to fetch products:', response.error)
        console.error('Full response:', response)
        setError(response.error || 'Failed to load products')
        setProducts([])
      }
    } catch (error) {
      console.error('❌ Error fetching products:', error)
      console.error('Full error details:', error)
      setError(`Failed to fetch products: ${error.message || 'Please check your connection'}`)
      setProducts([])
    } finally {
      setLoading(false)
    }
  }

  const handleSearch = async () => {
    if (!selectedPlant) return
    setCurrentPage(1)
    await fetchProducts(1)
  }

  const handlePageChange = (newPage) => {
    setCurrentPage(newPage)
  }


  // Products are already paginated from server
  const totalPages = Math.ceil(totalCount / itemsPerPage)
  const paginatedProducts = products // Show all products returned from API

  const handleSelectProduct = (product) => {
    // Don't allow selection if already in template
    if (product.isAlreadyInTemplate) {
      return
    }

    setSelectedProducts(prev => {
      const isSelected = prev.find(p => p.id === product.id)
      if (isSelected) {
        return prev.filter(p => p.id !== product.id)
      } else {
        return [...prev, { ...product, orderQty: 0 }]
      }
    })
  }

  const handleSelectAll = () => {
    const allSelected = paginatedProducts.every(product => 
      selectedProducts.find(p => p.id === product.id)
    )
    
    if (allSelected) {
      // Deselect all from current page
      setSelectedProducts(prev => 
        prev.filter(selected => !paginatedProducts.find(p => p.id === selected.id))
      )
    } else {
      // Select all from current page
      const newSelections = paginatedProducts.filter(product => 
        !selectedProducts.find(p => p.id === product.id)
      ).map(product => ({ ...product, orderQty: 0 }))
      setSelectedProducts(prev => [...prev, ...newSelections])
    }
  }

  const handleAddToTemplate = () => {
    onAddProducts(selectedProducts)
    setSelectedProducts([])
    setSearchQuery('')
    setCurrentPage(1)
    onClose()
  }

  const handleClose = () => {
    setSelectedProducts([])
    setSearchQuery('')
    setCurrentPage(1)
    onClose()
  }

  if (!isOpen) return null

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
      <div className="bg-white rounded-lg shadow-xl max-w-6xl w-full max-h-[90vh] flex flex-col">
        {/* Header */}
        <div className="flex items-center justify-between p-6 border-b border-gray-200">
          <div>
            <h2 className="text-xl font-semibold text-gray-900">Add Products to Template</h2>
            {selectedPlant && (
              <p className="text-sm text-blue-600 mt-1">
                📍 Showing products for plant: <span className="font-medium">{selectedPlant}</span>
              </p>
            )}
          </div>
          <button
            onClick={handleClose}
            className="p-2 hover:bg-gray-100 rounded-full transition-colors"
          >
            <X className="w-5 h-5" />
          </button>
        </div>

        {/* Filters and Search Bar */}
        <div className="p-6 border-b border-gray-200">

          {/* Search Bar */}
          <div className="flex gap-3 items-center">
            <div className="relative flex-1">
              <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 w-4 h-4" />
              <input
                type="text"
                placeholder="Search by SKU code or product name..."
                value={searchQuery}
                onChange={(e) => {
                  setSearchQuery(e.target.value)
                }}
                onKeyPress={(e) => {
                  if (e.key === 'Enter') {
                    handleSearch()
                  }
                }}
                disabled={!selectedPlant}
                className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent disabled:bg-gray-100 disabled:cursor-not-allowed"
              />
            </div>
            <Button
              onClick={handleSearch}
              disabled={loading || !selectedPlant}
              variant="outline"
              size="sm"
              className="flex items-center gap-2 px-3 py-2 border-gray-300"
            >
              <Search className={`w-4 h-4 ${loading ? 'animate-spin' : ''}`} />
              Search
            </Button>
            <Button
              onClick={() => fetchProducts(currentPage)}
              disabled={loading || !selectedPlant}
              variant="outline"
              size="sm"
              className="flex items-center gap-2 px-3 py-2 border-gray-300"
            >
              <RefreshCw className={`w-4 h-4 ${loading ? 'animate-spin' : ''}`} />
              Refresh
            </Button>
          </div>

          {error && (
            <div className="mt-3 flex items-center gap-2 text-sm text-red-600 bg-red-50 border border-red-200 rounded-lg p-3">
              <AlertCircle className="w-4 h-4 flex-shrink-0" />
              <span>{error}</span>
              <Button
                onClick={fetchProducts}
                variant="ghost"
                size="sm"
                className="ml-auto text-red-600 hover:text-red-700 hover:bg-red-100 px-2 py-1 text-xs"
              >
                Retry
              </Button>
            </div>
          )}

          {mismatchWarning && selectedPlant && selectedPlant !== 'Farmley' && (
            <div className="mt-3 flex items-center gap-2 text-sm text-amber-600 bg-amber-50 border border-amber-200 rounded-lg p-3">
              <AlertCircle className="w-4 h-4 flex-shrink-0" />
              <span>
                Note: Currently showing Farmley products. {selectedPlant} may not have products configured yet.
              </span>
            </div>
          )}

          {selectedProducts.length > 0 && (
            <div className="mt-3 text-sm text-blue-600">
              {selectedProducts.length} product(s) selected
            </div>
          )}
        </div>

        {/* Products Table */}
        <div className="flex-1 overflow-auto">
          <div className="overflow-x-auto">
            <table className="min-w-full">
              <thead className="bg-gray-600 text-white sticky top-0">
                <tr>
                  <th className="px-4 py-3 text-left">
                    <input
                      type="checkbox"
                      checked={paginatedProducts.length > 0 && paginatedProducts.every(product => 
                        selectedProducts.find(p => p.id === product.id)
                      )}
                      onChange={handleSelectAll}
                      className="rounded border-gray-300"
                    />
                  </th>
                  <th className="px-3 py-3 text-left text-xs font-medium">#</th>
                  <th className="px-3 py-3 text-left text-xs font-medium">SKU Code</th>
                  <th className="px-3 py-3 text-left text-xs font-medium">SKU Name</th>
                  <th className="px-3 py-3 text-center text-xs font-medium">Avl Qty</th>
                  <th className="px-3 py-3 text-center text-xs font-medium">Model Qty</th>
                  <th className="px-3 py-3 text-center text-xs font-medium">In Transit</th>
                  <th className="px-3 py-3 text-center text-xs font-medium">Case Per PC</th>
                  <th className="px-3 py-3 text-center text-xs font-medium">UOM</th>
                  <th className="px-3 py-3 text-center text-xs font-medium">Past 3 Month Average</th>
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {loading ? (
                  <tr>
                    <td colSpan="10" className="px-4 py-8 text-center">
                      <div className="flex items-center justify-center gap-2">
                        <RefreshCw className="w-4 h-4 animate-spin text-blue-500" />
                        <span className="text-gray-600">Loading products...</span>
                      </div>
                    </td>
                  </tr>
                ) : error ? (
                  <tr>
                    <td colSpan="10" className="px-4 py-8 text-center">
                      <div className="flex flex-col items-center gap-3 text-gray-500">
                        <AlertCircle className="w-8 h-8 text-red-400" />
                        <div>
                          <p className="font-medium">Failed to load products</p>
                          <p className="text-sm">{error}</p>
                        </div>
                        <Button
                          onClick={fetchProducts}
                          variant="outline"
                          size="sm"
                          className="mt-2"
                        >
                          Try Again
                        </Button>
                      </div>
                    </td>
                  </tr>
                ) : paginatedProducts.length === 0 ? (
                  <tr>
                    <td colSpan="10" className="px-4 py-8 text-center text-gray-500">
                      <div className="flex flex-col items-center gap-2">
                        <Search className="w-8 h-8 text-gray-300" />
                        <div>
                          {searchQuery ? (
                            <>
                              <p className="font-medium">No products found</p>
                              <p className="text-sm">Try adjusting your search terms</p>
                            </>
                          ) : !selectedPlant ? (
                            <>
                              <p className="font-medium">Please select a plant first</p>
                              <p className="text-sm">Go back and select a plant in the template form to view products</p>
                            </>
                          ) : products.length === 0 ? (
                            <>
                              <p className="font-medium">No products available</p>
                              <p className="text-sm">All products may already be in your template</p>
                            </>
                          ) : (
                            <p>No products match your search criteria</p>
                          )}
                        </div>
                      </div>
                    </td>
                  </tr>
                ) : (
                  paginatedProducts.map((product, index) => {
                    const isSelected = selectedProducts.find(p => p.id === product.id)
                    return (
                      <tr key={product.id} className={`hover:bg-gray-50 ${isSelected ? 'bg-blue-50' : ''} ${product.isAlreadyInTemplate ? 'opacity-60' : ''}`}>
                        <td className="px-4 py-3">
                          <div className="flex items-center">
                            <input
                              type="checkbox"
                              checked={!!isSelected}
                              onChange={() => handleSelectProduct(product)}
                              className="rounded border-gray-300 text-blue-600 focus:ring-blue-500"
                              disabled={product.isAlreadyInTemplate}
                            />
                            {isSelected && (
                              <Check className="w-4 h-4 text-blue-600 ml-2" />
                            )}
                            {product.isAlreadyInTemplate && (
                              <span className="ml-2 text-xs text-gray-500">Already added</span>
                            )}
                          </div>
                        </td>
                        <td className="px-3 py-3 text-sm">{(currentPage - 1) * itemsPerPage + index + 1}</td>
                        <td className="px-3 py-3 text-sm font-medium">{product.skuCode}</td>
                        <td className="px-3 py-3 text-sm">{product.skuName}</td>
                        <td className="px-3 py-3 text-sm text-center">{product.availQty}</td>
                        <td className="px-3 py-3 text-sm text-center">{product.modelQty.toLocaleString()}</td>
                        <td className="px-3 py-3 text-sm text-center">{product.inTransit.toLocaleString()}</td>
                        <td className="px-3 py-3 text-sm text-center">{product.casePerPC}</td>
                        <td className="px-3 py-3 text-sm text-center">{product.uom}</td>
                        <td className="px-3 py-3 text-sm text-center">{product.past3MonthAverage}</td>
                      </tr>
                    )
                  })
                )}
              </tbody>
            </table>
          </div>
        </div>

        {/* Pagination */}
        {totalPages > 1 && (
          <div className="px-6 py-3 border-t border-gray-200 flex items-center justify-between">
            <div className="text-sm text-gray-700">
              Showing {(currentPage - 1) * itemsPerPage + 1} to {Math.min(currentPage * itemsPerPage, totalCount)} of {totalCount} products
              {selectedPlant && (
                <span className="text-blue-600 font-medium"> in {selectedPlant}</span>
              )}
            </div>
            <div className="flex items-center gap-1">
              <button
                onClick={() => setCurrentPage(1)}
                disabled={currentPage === 1}
                className="px-2 py-1 border border-gray-300 rounded hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
              >
                «
              </button>
              <button
                onClick={() => setCurrentPage(prev => Math.max(prev - 1, 1))}
                disabled={currentPage === 1}
                className="px-2 py-1 border border-gray-300 rounded hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
              >
                ‹
              </button>
              <span className="px-3 py-1 bg-blue-600 text-white rounded">
                {currentPage}
              </span>
              <button
                onClick={() => setCurrentPage(prev => Math.min(prev + 1, totalPages))}
                disabled={currentPage === totalPages}
                className="px-2 py-1 border border-gray-300 rounded hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
              >
                ›
              </button>
              <button
                onClick={() => setCurrentPage(totalPages)}
                disabled={currentPage === totalPages}
                className="px-2 py-1 border border-gray-300 rounded hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
              >
                »
              </button>
            </div>
          </div>
        )}

        {/* Footer */}
        <div className="p-6 border-t border-gray-200 flex items-center justify-end gap-3">
          <Button
            onClick={handleClose}
            variant="outline"
            className="border-gray-300 text-gray-700 hover:bg-gray-50"
          >
            Cancel
          </Button>
          <Button
            onClick={handleAddToTemplate}
            disabled={selectedProducts.length === 0}
            className="bg-blue-600 hover:bg-blue-700 text-white"
          >
            Add {selectedProducts.length > 0 ? `${selectedProducts.length} ` : ''}Product{selectedProducts.length !== 1 ? 's' : ''} to Template
          </Button>
        </div>
      </div>
    </div>
  )
}

export default ProductSelectionModal