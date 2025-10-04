"use client"
import { useState, useEffect } from 'react'
import { useRouter } from 'next/navigation'
import {
  Plus,
  Calendar,
  Trash2
} from 'lucide-react'
import { Button } from '../../../components/ui/button'
import { useAuth } from '../../../contexts/AuthContext'

const ManualStockReceipt = () => {
  const { user } = useAuth()
  const router = useRouter()
  const [loading, setLoading] = useState(false)
  const [products, setProducts] = useState([])
  const [formData, setFormData] = useState({
    stockReceiptNo: 'P7R206499',
    stockReceiptDate: '16/09/2025',
    purchaseOrderNumber: '',
    deliveryDocketNo: '',
    stockType: '',
    warehouse: '[13007436]Remuera - G & M Paterson'
  })

  const handleAddProduct = () => {
    setProducts([...products, {
      id: Date.now(),
      skuCode: '',
      skuName: '',
      uom: '',
      orderQty: 0,
      receivedQty: 0,
      storageCategory: '',
      tempValue: '',
      isFrozen: false,
      visualCheck: false
    }])
  }

  const handleDeleteProduct = (id) => {
    setProducts(products.filter(p => p.id !== id))
  }

  const handleProductChange = (id, field, value) => {
    setProducts(products.map(p =>
      p.id === id ? { ...p, [field]: value } : p
    ))
  }

  const handleConfirm = () => {
    console.log('Confirming receipt:', { formData, products })
  }

  const handleDeleteSelected = () => {
    const selected = products.filter(p => p.selected)
    if (selected.length > 0) {
      setProducts(products.filter(p => !p.selected))
    }
  }

  const toggleSelectAll = () => {
    const allSelected = products.every(p => p.selected)
    setProducts(products.map(p => ({ ...p, selected: !allSelected })))
  }

  const toggleSelectProduct = (id) => {
    setProducts(products.map(p =>
      p.id === id ? { ...p, selected: !p.selected } : p
    ))
  }

  return (
    <div className="min-h-screen bg-[var(--background)] p-4 sm:p-6">
      {/* Header Section */}
      <div className="bg-[var(--card)] rounded-lg shadow-sm border border-[var(--border)] p-6 mb-6">
        <div className="flex flex-col space-y-4">
          <div>
            <h1 className="text-2xl font-bold text-[var(--foreground)]">Create Manual Stock Receipt</h1>
            <div className="flex items-center gap-2 mt-2 text-sm text-[var(--muted-foreground)]">
              <span>Home</span>
              <span>»</span>
              <span className="text-[var(--primary)]">Create Manual Stock Receipt</span>
            </div>
          </div>

          {/* Form Fields */}
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
            <div className="flex flex-col gap-1">
              <label className="text-sm font-medium text-[var(--foreground)]">
                Stock Receipt No <span className="text-red-500">*</span>
              </label>
              <input
                type="text"
                value={formData.stockReceiptNo}
                readOnly
                className="px-3 py-2 border border-[var(--border)] rounded-md bg-[var(--muted)] text-[var(--muted-foreground)]"
              />
            </div>

            <div className="flex flex-col gap-1">
              <label className="text-sm font-medium text-[var(--foreground)]">
                Stock Receipt Date <span className="text-red-500">*</span>
              </label>
              <div className="relative">
                <input
                  type="text"
                  value={formData.stockReceiptDate}
                  readOnly
                  className="px-3 py-2 pr-10 border border-[var(--border)] rounded-md bg-[var(--background)] text-[var(--foreground)] w-full"
                />
                <Calendar className="absolute right-3 top-1/2 transform -translate-y-1/2 w-4 h-4 text-[var(--muted-foreground)]" />
              </div>
            </div>

            <div className="flex flex-col gap-1">
              <label className="text-sm font-medium text-[var(--foreground)]">
                Purchase Order Number
              </label>
              <select
                value={formData.purchaseOrderNumber}
                onChange={(e) => setFormData({...formData, purchaseOrderNumber: e.target.value})}
                className="px-3 py-2 border border-[var(--border)] rounded-md bg-[var(--background)] text-[var(--foreground)]"
              >
                <option value="">- Select -</option>
              </select>
            </div>

            <div className="flex flex-col gap-1">
              <label className="text-sm font-medium text-[var(--foreground)]">
                Delivery Docket No <span className="text-red-500">*</span>
              </label>
              <input
                type="text"
                value={formData.deliveryDocketNo}
                onChange={(e) => setFormData({...formData, deliveryDocketNo: e.target.value})}
                className="px-3 py-2 border border-[var(--border)] rounded-md bg-[var(--background)] text-[var(--foreground)]"
                placeholder="Enter docket number"
              />
            </div>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div className="flex flex-col gap-1">
              <label className="text-sm font-medium text-[var(--foreground)]">
                Stock Type <span className="text-red-500">*</span>
              </label>
              <select
                value={formData.stockType}
                onChange={(e) => setFormData({...formData, stockType: e.target.value})}
                className="px-3 py-2 border border-[var(--border)] rounded-md bg-[var(--background)] text-[var(--foreground)]"
              >
                <option value="">Select Stock Type</option>
                <option value="regular">Regular Stock</option>
                <option value="damaged">Damaged Stock</option>
                <option value="return">Return Stock</option>
              </select>
            </div>

            <div className="flex flex-col gap-1">
              <label className="text-sm font-medium text-[var(--foreground)]">
                Warehouse <span className="text-red-500">*</span>
              </label>
              <input
                type="text"
                value={formData.warehouse}
                readOnly
                className="px-3 py-2 border border-[var(--border)] rounded-md bg-[var(--muted)] text-[var(--muted-foreground)]"
              />
            </div>
          </div>

          {/* Add Product Button */}
          <div className="flex justify-end">
            <Button
              onClick={handleAddProduct}
              className="bg-[var(--primary)] hover:bg-[var(--primary-hover)] text-white px-6"
            >
              <Plus className="w-4 h-4 mr-2" />
              Add Product
            </Button>
          </div>
        </div>
      </div>

      {/* Products Table */}
      <div className="bg-[var(--card)] rounded-lg shadow-sm border border-[var(--border)] overflow-hidden">
        <div className="overflow-x-auto">
          <table className="min-w-full divide-y divide-[var(--border)]">
            <thead className="bg-[var(--muted)]">
              <tr>
                <th className="px-4 py-3 text-center">
                  <input
                    type="checkbox"
                    onChange={toggleSelectAll}
                    checked={products.length > 0 && products.every(p => p.selected)}
                    className="rounded border-[var(--border)]"
                  />
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-[var(--muted-foreground)] uppercase tracking-wider">
                  SKU Code
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-[var(--muted-foreground)] uppercase tracking-wider">
                  SKU Name
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-[var(--muted-foreground)] uppercase tracking-wider">
                  UOM
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-[var(--muted-foreground)] uppercase tracking-wider">
                  Order Qty
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-[var(--muted-foreground)] uppercase tracking-wider">
                  Received Qty
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-[var(--muted-foreground)] uppercase tracking-wider">
                  Storage Category
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-[var(--muted-foreground)] uppercase tracking-wider">
                  Temp. Value / Is Frozen
                </th>
                <th className="px-6 py-3 text-center text-xs font-medium text-[var(--muted-foreground)] uppercase tracking-wider">
                  Visual Check
                </th>
              </tr>
            </thead>
            <tbody className="bg-[var(--card)] divide-y divide-[var(--border)]">
              {products.length === 0 ? (
                <tr>
                  <td colSpan="9" className="px-6 py-12 text-center">
                    <div className="text-[var(--muted-foreground)]">Please add product</div>
                  </td>
                </tr>
              ) : (
                products.map((product) => (
                  <tr key={product.id} className="hover:bg-[var(--muted)]">
                    <td className="px-4 py-3 text-center">
                      <input
                        type="checkbox"
                        checked={product.selected || false}
                        onChange={() => toggleSelectProduct(product.id)}
                        className="rounded border-[var(--border)]"
                      />
                    </td>
                    <td className="px-6 py-3">
                      <input
                        type="text"
                        value={product.skuCode}
                        onChange={(e) => handleProductChange(product.id, 'skuCode', e.target.value)}
                        className="w-full px-2 py-1 border border-[var(--border)] rounded"
                      />
                    </td>
                    <td className="px-6 py-3">
                      <input
                        type="text"
                        value={product.skuName}
                        onChange={(e) => handleProductChange(product.id, 'skuName', e.target.value)}
                        className="w-full px-2 py-1 border border-[var(--border)] rounded"
                      />
                    </td>
                    <td className="px-6 py-3">
                      <select
                        value={product.uom}
                        onChange={(e) => handleProductChange(product.id, 'uom', e.target.value)}
                        className="w-full px-2 py-1 border border-[var(--border)] rounded"
                      >
                        <option value="">Select</option>
                        <option value="EA">EA</option>
                        <option value="CTN">CTN</option>
                        <option value="PKT">PKT</option>
                      </select>
                    </td>
                    <td className="px-6 py-3">
                      <input
                        type="number"
                        value={product.orderQty}
                        onChange={(e) => handleProductChange(product.id, 'orderQty', e.target.value)}
                        className="w-full px-2 py-1 border border-[var(--border)] rounded"
                      />
                    </td>
                    <td className="px-6 py-3">
                      <input
                        type="number"
                        value={product.receivedQty}
                        onChange={(e) => handleProductChange(product.id, 'receivedQty', e.target.value)}
                        className="w-full px-2 py-1 border border-[var(--border)] rounded"
                      />
                    </td>
                    <td className="px-6 py-3">
                      <select
                        value={product.storageCategory}
                        onChange={(e) => handleProductChange(product.id, 'storageCategory', e.target.value)}
                        className="w-full px-2 py-1 border border-[var(--border)] rounded"
                      >
                        <option value="">Select</option>
                        <option value="dry">Dry</option>
                        <option value="chilled">Chilled</option>
                        <option value="frozen">Frozen</option>
                      </select>
                    </td>
                    <td className="px-6 py-3">
                      <div className="flex items-center gap-2">
                        <input
                          type="text"
                          value={product.tempValue}
                          onChange={(e) => handleProductChange(product.id, 'tempValue', e.target.value)}
                          className="w-20 px-2 py-1 border border-[var(--border)] rounded"
                          placeholder="°C"
                        />
                        <input
                          type="checkbox"
                          checked={product.isFrozen}
                          onChange={(e) => handleProductChange(product.id, 'isFrozen', e.target.checked)}
                          className="rounded border-[var(--border)]"
                        />
                      </div>
                    </td>
                    <td className="px-6 py-3 text-center">
                      <input
                        type="checkbox"
                        checked={product.visualCheck}
                        onChange={(e) => handleProductChange(product.id, 'visualCheck', e.target.checked)}
                        className="rounded border-[var(--border)]"
                      />
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>

        {/* Action Buttons */}
        <div className="p-4 border-t border-[var(--border)] flex items-center justify-center gap-4">
          <Button
            variant="outline"
            onClick={handleDeleteSelected}
            disabled={!products.some(p => p.selected)}
            className="border-[var(--destructive)] text-[var(--destructive)] hover:bg-[var(--destructive)] hover:text-white"
          >
            <Trash2 className="w-4 h-4 mr-2" />
            Delete Selected From List
          </Button>
          <Button
            onClick={handleConfirm}
            className="bg-[var(--primary)] hover:bg-[var(--primary-hover)] text-white px-8"
          >
            Confirm
          </Button>
        </div>
      </div>
    </div>
  )
}

export default ManualStockReceipt