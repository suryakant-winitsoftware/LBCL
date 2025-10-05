"use client"

import { Dialog, DialogContent, DialogHeader, DialogTitle } from "@/components/ui/dialog"
import { Button } from "@/components/ui/button"
import { Package } from "lucide-react"

interface PickListDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  orderLines?: any[]
  purchaseOrder?: any
}

export function PickListDialog({ open, onOpenChange, orderLines = [], purchaseOrder }: PickListDialogProps) {
  const formatDate = (dateString: string) => {
    if (!dateString) return "N/A";
    const date = new Date(dateString);
    return date.toLocaleDateString('en-GB', {
      day: '2-digit',
      month: 'short',
      year: 'numeric'
    }).toUpperCase();
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-4xl max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle className="text-lg sm:text-xl">Pick List - {purchaseOrder?.OrderNumber || purchaseOrder?.orderNumber || 'N/A'}</DialogTitle>
        </DialogHeader>

        <div className="py-4">
          {/* Pick List Header Info */}
          <div className="bg-gray-50 rounded-lg p-4 mb-4 grid grid-cols-3 gap-4 text-sm">
            <div>
              <span className="font-medium text-gray-600">Order Date:</span>
              <span className="ml-2">{formatDate(purchaseOrder?.OrderDate || purchaseOrder?.orderDate || '')}</span>
            </div>
            <div>
              <span className="font-medium text-gray-600">Print Date:</span>
              <span className="ml-2">{formatDate(new Date().toISOString())}</span>
            </div>
            <div>
              <span className="font-medium text-gray-600">Total Lines:</span>
              <span className="ml-2 font-bold">{orderLines.length}</span>
            </div>
          </div>

          {/* Pick List Items Table */}
          <div className="border rounded-lg overflow-hidden">
            <div className="overflow-x-auto">
              <table className="w-full">
                <thead className="bg-[#FFF8E7]">
                  <tr>
                    <th className="px-4 py-3 text-left text-xs font-medium text-gray-700">#</th>
                    <th className="px-4 py-3 text-left text-xs font-medium text-gray-700">SKU Code</th>
                    <th className="px-4 py-3 text-left text-xs font-medium text-gray-700">Description</th>
                    <th className="px-4 py-3 text-center text-xs font-medium text-gray-700">UOM</th>
                    <th className="px-4 py-3 text-center text-xs font-medium text-gray-700">Requested Qty</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-gray-200">
                  {orderLines.length === 0 ? (
                    <tr>
                      <td colSpan={5} className="px-4 py-8 text-center text-gray-500">
                        <Package className="w-12 h-12 mx-auto mb-2 text-gray-400" />
                        <p>No items in this pick list</p>
                      </td>
                    </tr>
                  ) : (
                    orderLines.map((line, index) => (
                      <tr key={line.UID || line.uid || index} className="hover:bg-gray-50">
                        <td className="px-4 py-3 text-sm">{index + 1}</td>
                        <td className="px-4 py-3 text-sm font-medium">
                          {line.SKUCode || line.skuCode || 'N/A'}
                        </td>
                        <td className="px-4 py-3 text-sm">
                          {line.SKUName || line.skuName || line.ProductName || 'N/A'}
                        </td>
                        <td className="px-4 py-3 text-sm text-center">
                          {line.UOM || line.uom || 'N/A'}
                        </td>
                        <td className="px-4 py-3 text-sm text-center font-medium">
                          {line.RequestedQty || line.requestedQty || 0}
                        </td>
                      </tr>
                    ))
                  )}
                </tbody>
                {orderLines.length > 0 && (
                  <tfoot className="bg-gray-50">
                    <tr>
                      <td colSpan={4} className="px-4 py-3 text-sm font-bold text-right">
                        Total:
                      </td>
                      <td className="px-4 py-3 text-sm text-center font-bold">
                        {orderLines.reduce((sum, line) => sum + (line.RequestedQty || line.requestedQty || 0), 0)}
                      </td>
                    </tr>
                  </tfoot>
                )}
              </table>
            </div>
          </div>
        </div>

        <div className="flex gap-2">
          <Button
            variant="outline"
            onClick={() => window.print()}
            className="flex-1 border-[#A08B5C] text-[#A08B5C] hover:bg-[#FFF8E7]"
          >
            Print Pick List
          </Button>
          <Button
            onClick={() => onOpenChange(false)}
            className="flex-1 bg-[#A08B5C] hover:bg-[#8F7A4B] text-white"
          >
            DONE
          </Button>
        </div>
      </DialogContent>
    </Dialog>
  )
}
