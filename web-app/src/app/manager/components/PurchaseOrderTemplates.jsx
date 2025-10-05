"use client"
import { useState, useEffect } from 'react'
import { useRouter } from 'next/navigation'
import {
  Filter as FilterIcon,
  Edit,
  ShoppingCart,
  Trash2
} from 'lucide-react'
import { Button } from '../../../components/ui/button'
import purchaseOrderService from '../../../services/purchaseOrder'
import warehouseService from '../../../services/warehouse'

const PurchaseOrderTemplates = () => {
  const router = useRouter()
  const [templates, setTemplates] = useState([])
  const [loading, setLoading] = useState(true)
  const [templateNameSearch, setTemplateNameSearch] = useState('')
  const [templateNameFilter, setTemplateNameFilter] = useState('All')
  const [currentPage, setCurrentPage] = useState(1)
  const [showFilterSection, setShowFilterSection] = useState(false)
  const [totalCount, setTotalCount] = useState(0)
  const [error, setError] = useState('')
  const [warehouseMap, setWarehouseMap] = useState({})
  const itemsPerPage = 6

  useEffect(() => {
    console.log('🚀 Component mounting, fetching data...')
    fetchWarehouseData()
    fetchTemplates()
  }, [currentPage, templateNameFilter, templateNameSearch])

  const fetchWarehouseData = async () => {
    try {
      console.log('🔄 Starting warehouse data fetch...')
      const response = await warehouseService.getCachedWarehouseData()
      console.log('🏢 Warehouse service response:', response)

      if (response.success && response.warehouseMap) {
        setWarehouseMap(response.warehouseMap)
        console.log('🏢 Warehouse map loaded successfully:', response.warehouseMap)
        console.log('🏢 Number of warehouses in map:', Object.keys(response.warehouseMap).length)
      } else {
        console.error('❌ Warehouse data fetch failed:', response.error)
        console.log('🏢 Setting empty warehouse map')
        setWarehouseMap({})
      }
    } catch (error) {
      console.error('❌ Error fetching warehouse data:', error)
      setWarehouseMap({})
    }
  }

  const fetchTemplates = async () => {
    try {
      setLoading(true)
      setError('')

      const filters = {
        page: currentPage,
        pageSize: itemsPerPage,
        searchQuery: templateNameSearch
      }

      const response = await purchaseOrderService.getAllPurchaseOrderTemplateHeaders(filters)

      if (response.success) {
        setTemplates(response.headers || [])
        setTotalCount(response.totalCount || 0)
      } else {
        setError(response.error || 'Failed to fetch templates')
        setTemplates([])
        setTotalCount(0)
      }
    } catch (error) {
      console.error('Error fetching templates:', error)
      setError('Failed to fetch templates')
      setTemplates([])
      setTotalCount(0)
    } finally {
      setLoading(false)
    }
  }

  const handleSearch = () => {
    setCurrentPage(1)
    fetchTemplates()
  }

  const handleReset = () => {
    setTemplateNameSearch('')
    setTemplateNameFilter('All')
    setCurrentPage(1)
  }

  const handleFilter = () => {
    setShowFilterSection(!showFilterSection)
  }

  const totalPages = Math.ceil(totalCount / itemsPerPage)
  const startIndex = (currentPage - 1) * itemsPerPage

  const handleEdit = (template) => {
    router.push(`/manager/edit-purchase-order-template/${template.id}`)
  }

  const handleCreateOrder = (template) => {
    router.push(`/manager/create-purchase-order/${template.id}`)
  }

  const handleDelete = async (template) => {
    try {
      const confirmDelete = window.confirm(`Are you sure you want to delete the template "${template.name || template.templateName}"?`)
      if (!confirmDelete) return

      const response = await purchaseOrderService.deletePurchaseOrderTemplate(template.uid || template.id)

      if (response.success) {
        alert('Template deleted successfully')
        fetchTemplates() // Refresh the list
      } else {
        alert('Failed to delete template: ' + response.error)
      }
    } catch (error) {
      console.error('Error deleting template:', error)
      alert('Failed to delete template')
    }
  }

  return (
    <div className="min-h-screen bg-gray-50 p-4">
      {/* Header Section */}
      <div className="bg-white rounded-lg shadow-sm p-6 mb-4">
        <div className="flex flex-col space-y-4">
          <div>
            <h1 className="text-2xl font-bold text-blue-700">Create Purchase Order and Templates</h1>
            <div className="flex items-center gap-2 mt-2 text-sm text-gray-600">
              <span>Home</span>
              <span>»</span>
              <span className="text-gray-900">Create Purchase Order and Templates</span>
            </div>
          </div>

          {/* Search Criteria */}
          <div className="flex items-center gap-4 text-sm">
            <span className="font-medium">Search Criteria : Template Name:</span>
            <select
              value={templateNameFilter}
              onChange={(e) => setTemplateNameFilter(e.target.value)}
              className="px-3 py-1 border border-gray-300 rounded bg-white text-sm font-bold"
            >
              <option value="All">All</option>
            </select>
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

      {/* Template Name Search Box - Only show when filter is clicked */}
      {showFilterSection && (
        <div className="bg-white rounded-lg shadow-sm p-4 mb-4">
          <div className="border border-gray-300 rounded p-4">
            <div className="mb-3">
              <label className="text-sm font-medium">Template Name</label>
            </div>
            <div className="flex gap-3 items-center">
              <input
                type="text"
                value={templateNameSearch}
                onChange={(e) => setTemplateNameSearch(e.target.value)}
                className="flex-1 px-3 py-2 border border-gray-300 rounded"
                placeholder="Enter template name"
              />
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

      {/* Create New Template Button */}
      <div className="mb-4">
        <Button
          onClick={() => router.push('/manager/add-purchase-order-template')}
          className="bg-blue-600 hover:bg-blue-700 text-white px-4 py-2"
        >
          Create New Template
        </Button>
      </div>

      {/* Records Count */}
      <div className="text-sm text-gray-600 mb-2 text-right">
        Records: {startIndex + 1} - {Math.min(startIndex + itemsPerPage, totalCount)} of {totalCount}
      </div>

      {/* Error Message */}
      {error && (
        <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded mb-4">
          <p className="font-medium">Error: {error}</p>
        </div>
      )}

      {/* Templates Table */}
      <div className="bg-white rounded-lg shadow-sm overflow-hidden">
        <div className="overflow-x-auto">
          <table className="min-w-full">
            <thead className="bg-gray-600 text-white">
              <tr>
                <th className="px-4 py-2 text-left text-sm font-medium">
                  #
                </th>
                <th className="px-4 py-2 text-left text-sm font-medium">
                  Template Name
                </th>
                <th className="px-4 py-2 text-left text-sm font-medium">
                  Warehouse Name
                </th>
                <th className="px-4 py-2 text-left text-sm font-medium">
                  Created Date
                </th>
                <th className="px-4 py-2 text-center text-sm font-medium">
                  Actions
                </th>
              </tr>
            </thead>
            <tbody className="bg-white divide-y divide-gray-200">
              {loading ? (
                <tr>
                  <td colSpan="5" className="px-4 py-3 text-center">
                    <div className="animate-pulse">Loading...</div>
                  </td>
                </tr>
              ) : templates.length === 0 ? (
                <tr>
                  <td colSpan="5" className="px-4 py-3 text-center text-gray-500">
                    No templates found
                  </td>
                </tr>
              ) : (
                templates.map((template, index) => (
                  <tr key={template.uid || template.id || `template-${index}`} className="hover:bg-gray-50">
                    <td className="px-4 py-3 text-sm">
                      {startIndex + index + 1}
                    </td>
                    <td className="px-4 py-3 text-sm">
                      {template.templateName || template.TemplateName}
                    </td>
                    <td className="px-4 py-3 text-sm">
                      {(() => {
                        // DEBUG: Log everything about this template
                        console.log(`🔍 FULL Template Object:`, template);
                        console.log(`🔍 Template Keys:`, Object.keys(template));
                        console.log(`🔍 Warehouse Map:`, warehouseMap);
                        console.log(`🔍 Warehouse Map Keys:`, Object.keys(warehouseMap));

                        // TEMPORARY: For debugging, let's try to show some warehouse names from the map
                        const warehouseMapValues = Object.values(warehouseMap);
                        if (warehouseMapValues.length > 0) {
                          console.log(`✅ TEMP: Using first warehouse from map: ${warehouseMapValues[0]}`);
                          return warehouseMapValues[0]; // Temporarily show first warehouse
                        }

                        const warehouseUID = template.warehouse_uid || template.wareHouseUID || template.WareHouseUID || template.warehouseUID;
                        const warehouseName = template.warehouse || template.Warehouse || template.warehouseName || template.WarehouseName;

                        // First try to get from warehouse map using UID
                        if (warehouseUID && warehouseMap[warehouseUID]) {
                          console.log(`✅ Found warehouse name for UID ${warehouseUID}: ${warehouseMap[warehouseUID]}`);
                          return warehouseMap[warehouseUID];
                        }

                        // Fallback to existing warehouse name if available
                        if (warehouseName && warehouseName !== 'N/A') {
                          console.log(`✅ Using existing warehouse name: ${warehouseName}`);
                          return warehouseName;
                        }

                        console.log(`❌ No warehouse data found for template ${template.templateName}`);
                        return 'N/A';
                      })()}
                    </td>
                    <td className="px-4 py-3 text-sm">
                      {template.createdTime ? new Date(template.createdTime).toLocaleDateString() :
                       template.CreatedTime ? new Date(template.CreatedTime).toLocaleDateString() :
                       template.serverAddTime ? new Date(template.serverAddTime).toLocaleDateString() : ''}
                    </td>
                    <td className="px-4 py-3 text-sm">
                      <div className="flex items-center justify-center gap-1">
                        <button
                          onClick={() => handleEdit(template)}
                          className="p-1 text-blue-600 hover:text-blue-700"
                          title="Edit"
                        >
                          <Edit className="w-4 h-4" />
                        </button>
                        <button
                          onClick={() => handleCreateOrder(template)}
                          className="p-1 text-blue-600 hover:text-blue-700"
                          title="Create Purchase Order"
                        >
                          <ShoppingCart className="w-4 h-4" />
                        </button>
                        <button
                          onClick={() => handleDelete(template)}
                          className="p-1 text-red-600 hover:text-red-700"
                          title="Delete"
                        >
                          <Trash2 className="w-4 h-4" />
                        </button>
                      </div>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>

        {/* Pagination */}
        {totalPages > 1 && (
          <div className="px-4 py-3 border-t border-gray-200 flex items-center justify-center gap-1">
            <button
              key="first-page"
              onClick={() => setCurrentPage(1)}
              disabled={currentPage === 1}
              className="px-2 py-1 border border-gray-300 rounded hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              «
            </button>
            <button
              key="prev-page"
              onClick={() => setCurrentPage(prev => Math.max(prev - 1, 1))}
              disabled={currentPage === 1}
              className="px-2 py-1 border border-gray-300 rounded hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              ‹
            </button>
            <span key="current-page" className="px-3 py-1 bg-blue-600 text-white rounded">
              {currentPage}
            </span>
            <button
              key="next-page"
              onClick={() => setCurrentPage(prev => Math.min(prev + 1, totalPages))}
              disabled={currentPage === totalPages}
              className="px-2 py-1 border border-gray-300 rounded hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              ›
            </button>
            <button
              key="last-page"
              onClick={() => setCurrentPage(totalPages)}
              disabled={currentPage === totalPages}
              className="px-2 py-1 border border-gray-300 rounded hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              »
            </button>
          </div>
        )}
      </div>
    </div>
  )
}

export default PurchaseOrderTemplates