"use client"

import { useState } from "react"
import { Dialog, DialogContent, DialogHeader, DialogTitle } from "@/components/ui/dialog"
import { Button } from "@/components/ui/button"
import { MessageSquare, Mail, MessageCircle, Download } from "lucide-react"
import jsPDF from "jspdf"
import autoTable from "jspdf-autotable"

interface ShareDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  purchaseOrder?: any
  orderLines?: any[]
}

export function ShareDialog({ open, onOpenChange, purchaseOrder, orderLines = [] }: ShareDialogProps) {
  const [generating, setGenerating] = useState(false)

  const generateDeliveryNotePDF = () => {
    setGenerating(true)
    try {
      const doc = new jsPDF()

      // Header with background color
      doc.setFillColor(160, 139, 92) // #A08B5C
      doc.rect(0, 0, 210, 40, 'F')

      // Title
      doc.setTextColor(255, 255, 255)
      doc.setFontSize(24)
      doc.setFont('helvetica', 'bold')
      doc.text('DELIVERY NOTE', 105, 20, { align: 'center' })

      // Delivery Note Number
      const now = new Date()
      const orderNumber = purchaseOrder?.OrderNumber || purchaseOrder?.orderNumber || 'PO'
      const timestamp = now.getTime().toString().slice(-6)
      const deliveryNoteNumber = `DN-${orderNumber}-${timestamp}`

      doc.setFontSize(12)
      doc.text(deliveryNoteNumber, 105, 32, { align: 'center' })

      // Reset text color
      doc.setTextColor(0, 0, 0)
      doc.setFont('helvetica', 'normal')

      // Order Information Section
      let yPos = 50
      doc.setFontSize(14)
      doc.setFont('helvetica', 'bold')
      doc.text('Order Information', 14, yPos)

      yPos += 8
      doc.setFontSize(10)
      doc.setFont('helvetica', 'normal')
      doc.text(`Order Number: ${purchaseOrder?.OrderNumber || purchaseOrder?.orderNumber || 'N/A'}`, 14, yPos)
      yPos += 6
      doc.text(`Order Date: ${purchaseOrder?.OrderDate || purchaseOrder?.orderDate ? new Date(purchaseOrder.OrderDate || purchaseOrder.orderDate).toLocaleDateString() : 'N/A'}`, 14, yPos)
      yPos += 6
      doc.text(`Delivery Date: ${now.toLocaleDateString()}`, 14, yPos)

      // Shipping Details Section
      yPos += 12
      doc.setFontSize(14)
      doc.setFont('helvetica', 'bold')
      doc.text('Shipping Details', 14, yPos)

      yPos += 8
      doc.setFontSize(10)
      doc.setFont('helvetica', 'normal')
      doc.text(`Ship To: ${purchaseOrder?.OrgName || purchaseOrder?.orgName || 'N/A'}`, 14, yPos)
      yPos += 6
      doc.text(`From Warehouse: ${purchaseOrder?.WarehouseName || purchaseOrder?.warehouseName || 'N/A'}`, 14, yPos)

      // Order Items Table
      yPos += 12
      const tableData = orderLines.map((line: any, index: number) => [
        index + 1,
        line.SKUCode || line.skuCode || 'N/A',
        line.SKUName || line.skuName || line.ProductName || 'N/A',
        line.UOM || line.uom || 'N/A',
        line.RequestedQty || line.requestedQty || 0
      ])

      autoTable(doc, {
        startY: yPos,
        head: [['#', 'Product Code', 'Description', 'Unit', 'Quantity']],
        body: tableData,
        theme: 'grid',
        headStyles: {
          fillColor: [160, 139, 92], // #A08B5C
          textColor: [255, 255, 255],
          fontStyle: 'bold'
        },
        styles: {
          fontSize: 9,
          cellPadding: 3
        },
        columnStyles: {
          0: { cellWidth: 15 },
          1: { cellWidth: 35 },
          2: { cellWidth: 70 },
          3: { cellWidth: 25 },
          4: { cellWidth: 30, halign: 'right' }
        }
      })

      // Total Items
      const finalY = (doc as any).lastAutoTable.finalY || yPos + 50
      const totalQty = orderLines.reduce((sum: number, line: any) =>
        sum + (line.RequestedQty || line.requestedQty || 0), 0)

      doc.setFont('helvetica', 'bold')
      doc.text(`Total Items: ${totalQty}`, 14, finalY + 10)

      // Footer
      const pageHeight = doc.internal.pageSize.height
      doc.setFontSize(8)
      doc.setFont('helvetica', 'italic')
      doc.setTextColor(128, 128, 128)
      doc.text('This is a computer-generated delivery note', 105, pageHeight - 10, { align: 'center' })

      return doc
    } catch (error) {
      console.error('Error generating PDF:', error)
      return null
    } finally {
      setGenerating(false)
    }
  }

  const handleDownloadPDF = () => {
    const doc = generateDeliveryNotePDF()
    if (doc) {
      const orderNumber = purchaseOrder?.OrderNumber || purchaseOrder?.orderNumber || 'PO'
      doc.save(`Delivery_Note_${orderNumber}.pdf`)
    }
  }

  const handleShareSMS = () => {
    const doc = generateDeliveryNotePDF()
    if (doc) {
      // For SMS, we'll provide a message with the delivery note details
      const orderNumber = purchaseOrder?.OrderNumber || purchaseOrder?.orderNumber || 'N/A'
      const message = `Delivery Note for Order ${orderNumber}. Please download the PDF from the shared link.`

      // Open SMS with pre-filled message (mobile devices will handle this)
      window.open(`sms:?body=${encodeURIComponent(message)}`, '_blank')

      // Also download the PDF for the user to attach manually
      handleDownloadPDF()
    }
  }

  const handleShareEmail = () => {
    const doc = generateDeliveryNotePDF()
    if (doc) {
      const orderNumber = purchaseOrder?.OrderNumber || purchaseOrder?.orderNumber || 'N/A'
      const orgName = purchaseOrder?.OrgName || purchaseOrder?.orgName || 'Customer'
      const subject = `Delivery Note - ${orderNumber}`
      const body = `Dear ${orgName},\n\nPlease find attached the delivery note for order ${orderNumber}.\n\nTotal Items: ${orderLines.length}\n\nThank you for your business.\n\nBest regards,\nLBCL Logistics`

      // Download PDF first
      handleDownloadPDF()

      // Open email client
      window.open(`mailto:?subject=${encodeURIComponent(subject)}&body=${encodeURIComponent(body)}`, '_blank')
    }
  }

  const handleShareWhatsApp = () => {
    const doc = generateDeliveryNotePDF()
    if (doc) {
      const orderNumber = purchaseOrder?.OrderNumber || purchaseOrder?.orderNumber || 'N/A'
      const message = `📦 *Delivery Note*\n\nOrder Number: ${orderNumber}\nTotal Items: ${orderLines.length}\n\nPlease check the attached PDF for complete details.`

      // Download PDF first
      handleDownloadPDF()

      // Open WhatsApp with pre-filled message
      window.open(`https://wa.me/?text=${encodeURIComponent(message)}`, '_blank')
    }
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-md">
        <DialogHeader>
          <DialogTitle className="text-lg sm:text-xl">Share Delivery Plan</DialogTitle>
        </DialogHeader>

        <div className="py-4 sm:py-6">
          <p className="text-sm text-gray-600 mb-6">
            Generate and share the delivery note PDF via your preferred method
          </p>

          <div className="mb-6">
            <h3 className="font-medium mb-4">Share Files Via</h3>
            <div className="grid grid-cols-2 gap-4">
              <button
                onClick={handleDownloadPDF}
                disabled={generating}
                className="flex flex-col items-center gap-2 p-4 border rounded-lg hover:bg-gray-50 transition-colors disabled:opacity-50"
              >
                <Download className="w-8 h-8 text-[#A08B5C]" />
                <span className="text-sm font-medium">Download PDF</span>
              </button>

              <button
                onClick={handleShareSMS}
                disabled={generating}
                className="flex flex-col items-center gap-2 p-4 border rounded-lg hover:bg-gray-50 transition-colors disabled:opacity-50"
              >
                <MessageSquare className="w-8 h-8 text-[#A08B5C]" />
                <span className="text-sm font-medium">SMS</span>
              </button>

              <button
                onClick={handleShareEmail}
                disabled={generating}
                className="flex flex-col items-center gap-2 p-4 border rounded-lg hover:bg-gray-50 transition-colors disabled:opacity-50"
              >
                <Mail className="w-8 h-8 text-[#A08B5C]" />
                <span className="text-sm font-medium">Email</span>
              </button>

              <button
                onClick={handleShareWhatsApp}
                disabled={generating}
                className="flex flex-col items-center gap-2 p-4 border rounded-lg hover:bg-gray-50 transition-colors disabled:opacity-50"
              >
                <MessageCircle className="w-8 h-8 text-[#A08B5C]" />
                <span className="text-sm font-medium">WhatsApp</span>
              </button>
            </div>
          </div>
        </div>

        <Button
          onClick={() => onOpenChange(false)}
          className="w-full bg-[#A08B5C] hover:bg-[#8F7A4B] text-white h-12"
        >
          DONE
        </Button>
      </DialogContent>
    </Dialog>
  )
}
