"use client"

import { Button } from "@/components/ui/button"
import { Package, Printer, Download } from "lucide-react"

interface DeliveryNoteDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  orderLines?: any[]
  purchaseOrder?: any
  onSaveDeliveryNote?: () => void
  isSaving?: boolean
  isSaved?: boolean
}

export function DeliveryNoteDialog({ open, onOpenChange, orderLines = [], purchaseOrder, onSaveDeliveryNote, isSaving = false, isSaved = false }: DeliveryNoteDialogProps) {
  const formatDate = (dateString: string) => {
    if (!dateString) return "N/A";
    const date = new Date(dateString);
    return date.toLocaleDateString('en-GB', {
      day: '2-digit',
      month: 'short',
      year: 'numeric'
    }).toUpperCase();
  };

  const formatDateTime = (dateString: string) => {
    if (!dateString) return "N/A";
    const date = new Date(dateString);
    return date.toLocaleDateString('en-GB', {
      day: '2-digit',
      month: 'short',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    }).toUpperCase();
  };

  const handlePrint = () => {
    window.print();
  };

  const handleDownloadPDF = () => {
    window.print();
  };

  if (!open) return null;

  return (
    <>
      {/* Print Styles */}
      <style jsx global>{`
        @media print {
          body * {
            visibility: hidden;
          }
          .print-content,
          .print-content * {
            visibility: visible;
          }
          .print-content {
            position: absolute;
            left: 0;
            top: 0;
            width: 100%;
          }
          .no-print {
            display: none !important;
          }
          .print-content {
            background: white !important;
            padding: 0 !important;
            margin: 0 !important;
          }
        }
      `}</style>

      <div className="fixed inset-0 bg-gray-100 z-50 overflow-y-auto">
        <div className="max-w-5xl mx-auto p-4 sm:p-6 lg:p-8">
          {/* Delivery Note Paper */}
          <div className="bg-white shadow-lg rounded-lg overflow-hidden print-content">
          {/* Header */}
          <div className="bg-[#A08B5C] text-white px-6 py-8">
            <h1 className="text-3xl font-bold">Delivery Note</h1>
            <p className="text-lg mt-2 opacity-90">{purchaseOrder?.OrderNumber || purchaseOrder?.orderNumber || 'N/A'}</p>
          </div>

          {/* Content */}
          <div className="p-6 sm:p-8">
            {/* Info Section */}
            <div className="grid grid-cols-1 lg:grid-cols-2 gap-6 mb-8">
              {/* Order Information */}
              <div>
                <h3 className="text-sm font-bold text-gray-700 mb-4 uppercase tracking-wide">Order Information</h3>
                <div className="space-y-3">
                  <div className="flex justify-between py-2 border-b border-gray-200">
                    <span className="text-sm text-gray-600">Order Number</span>
                    <span className="text-sm font-semibold text-gray-900">{purchaseOrder?.OrderNumber || purchaseOrder?.orderNumber || 'N/A'}</span>
                  </div>
                  <div className="flex justify-between py-2 border-b border-gray-200">
                    <span className="text-sm text-gray-600">Order Date</span>
                    <span className="text-sm font-semibold text-gray-900">{formatDate(purchaseOrder?.OrderDate || purchaseOrder?.orderDate || '')}</span>
                  </div>
                  <div className="flex justify-between py-2 border-b border-gray-200">
                    <span className="text-sm text-gray-600">Delivery Date</span>
                    <span className="text-sm font-semibold text-gray-900">{formatDateTime(new Date().toISOString())}</span>
                  </div>
                </div>
              </div>

              {/* Shipping Details */}
              <div>
                <h3 className="text-sm font-bold text-gray-700 mb-4 uppercase tracking-wide">Shipping Details</h3>
                <div className="space-y-3">
                  <div className="py-2 border-b border-gray-200">
                    <p className="text-xs text-gray-500 mb-1">Ship To</p>
                    <p className="text-sm font-semibold text-gray-900">{purchaseOrder?.OrgName || purchaseOrder?.orgName || 'N/A'}</p>
                  </div>
                  <div className="py-2 border-b border-gray-200">
                    <p className="text-xs text-gray-500 mb-1">From Warehouse</p>
                    <p className="text-sm font-semibold text-gray-900">{purchaseOrder?.WarehouseName || purchaseOrder?.warehouseName || 'N/A'}</p>
                  </div>
                </div>
              </div>
            </div>

            {/* Items Table */}
            <div>
              <h3 className="text-sm font-bold text-gray-700 mb-4 uppercase tracking-wide">Order Items</h3>
              <div className="overflow-x-auto">
                <table className="w-full border-collapse">
                  <thead>
                    <tr className="bg-gray-50 border-y border-gray-200">
                      <th className="px-4 py-3 text-left text-xs font-semibold text-gray-700 uppercase tracking-wider">#</th>
                      <th className="px-4 py-3 text-left text-xs font-semibold text-gray-700 uppercase tracking-wider">Product Code</th>
                      <th className="px-4 py-3 text-left text-xs font-semibold text-gray-700 uppercase tracking-wider">Description</th>
                      <th className="px-4 py-3 text-center text-xs font-semibold text-gray-700 uppercase tracking-wider">Unit</th>
                      <th className="px-4 py-3 text-right text-xs font-semibold text-gray-700 uppercase tracking-wider">Quantity</th>
                    </tr>
                  </thead>
                  <tbody className="bg-white divide-y divide-gray-200">
                    {orderLines.length === 0 ? (
                      <tr>
                        <td colSpan={5} className="px-4 py-12 text-center">
                          <Package className="w-12 h-12 mx-auto mb-3 text-gray-300" />
                          <p className="text-gray-500 text-sm">No items in this delivery</p>
                        </td>
                      </tr>
                    ) : (
                      orderLines.map((line, index) => (
                        <tr key={line.UID || line.uid || index} className="hover:bg-gray-50">
                          <td className="px-4 py-4 text-sm text-gray-900">{index + 1}</td>
                          <td className="px-4 py-4 text-sm font-medium text-gray-900">{line.SKUCode || line.skuCode || 'N/A'}</td>
                          <td className="px-4 py-4 text-sm text-gray-700">{line.SKUName || line.skuName || line.ProductName || 'N/A'}</td>
                          <td className="px-4 py-4 text-sm text-gray-600 text-center">{line.UOM || line.uom || 'N/A'}</td>
                          <td className="px-4 py-4 text-sm font-semibold text-gray-900 text-right">{line.RequestedQty || line.requestedQty || 0}</td>
                        </tr>
                      ))
                    )}
                  </tbody>
                  {orderLines.length > 0 && (
                    <tfoot>
                      <tr className="bg-gray-50 border-t-2 border-gray-300">
                        <td colSpan={4} className="px-4 py-4 text-sm font-bold text-right text-gray-700 uppercase">
                          Total Items:
                        </td>
                        <td className="px-4 py-4 text-right">
                          <span className="text-lg font-bold text-[#A08B5C]">
                            {orderLines.reduce((sum, line) => sum + (line.RequestedQty || line.requestedQty || 0), 0)}
                          </span>
                        </td>
                      </tr>
                    </tfoot>
                  )}
                </table>
              </div>
            </div>
          </div>

          {/* Action Buttons */}
          <div className="bg-gray-50 px-6 py-4 border-t border-gray-200 flex flex-wrap gap-3 no-print">
            <Button
              variant="outline"
              onClick={handlePrint}
              className="flex-1 min-w-[150px] border-[#A08B5C] text-[#A08B5C] hover:bg-[#A08B5C] hover:text-white h-11 font-semibold"
            >
              <Printer className="w-4 h-4 mr-2" />
              Print
            </Button>
            <Button
              variant="outline"
              onClick={handleDownloadPDF}
              className="flex-1 min-w-[150px] border-[#A08B5C] text-[#A08B5C] hover:bg-[#A08B5C] hover:text-white h-11 font-semibold"
            >
              <Download className="w-4 h-4 mr-2" />
              Download PDF
            </Button>
            {onSaveDeliveryNote && !isSaved && (
              <Button
                onClick={onSaveDeliveryNote}
                disabled={isSaving}
                className="flex-1 min-w-[150px] bg-green-600 hover:bg-green-700 text-white h-11 font-semibold"
              >
                {isSaving ? "Saving..." : "Save Delivery Note"}
              </Button>
            )}
            <Button
              onClick={() => onOpenChange(false)}
              className="flex-1 min-w-[150px] bg-[#A08B5C] hover:bg-[#8F7A4B] text-white h-11 font-semibold"
            >
              Close
            </Button>
          </div>
        </div>
      </div>
      </div>
    </>
  )
}
