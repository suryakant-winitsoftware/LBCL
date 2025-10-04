"use client"
import { useState, useEffect } from 'react'
import { useRouter } from 'next/navigation'
import { Filter as FilterIcon, Calendar } from 'lucide-react'
import { Button } from '../../../components/ui/button'
import * as XLSX from 'xlsx'
import jsPDF from 'jspdf'
import 'jspdf-autotable'

const ReceiptStock = () => {
  const router = useRouter()
  const [receipts, setReceipts] = useState([])
  const [loading, setLoading] = useState(true)
  const [showFilterSection, setShowFilterSection] = useState(false)
  const [activeTab, setActiveTab] = useState('Open')

  // Filter states
  const [purchaseOrderNo, setPurchaseOrderNo] = useState('')
  const [status, setStatus] = useState('Select')
  const [startDate, setStartDate] = useState('09/09/2025')
  const [endDate, setEndDate] = useState('16/09/2025')

  const tabs = [
    'Open', 'Confirmed', 'Manual'
  ]

  useEffect(() => {
    const fetchReceipts = async () => {
      try {
        // Simulated data - replace with actual API call
        const mockReceipts = []
        setReceipts(mockReceipts)
      } catch (error) {
        console.error('Error fetching receipts:', error)
      } finally {
        setLoading(false)
      }
    }
    fetchReceipts()
  }, [])

  const handleFilter = () => {
    setShowFilterSection(!showFilterSection)
  }

  const handleSearch = () => {
    console.log('Searching with filters:', {
      purchaseOrderNo,
      status,
      startDate,
      endDate
    })
    // Implement search functionality
  }

  const handleReset = () => {
    setPurchaseOrderNo('')
    setStatus('Select')
    setStartDate('09/09/2025')
    setEndDate('16/09/2025')
  }

  const handleExportToExcel = () => {
    try {
      // Sample data for demonstration - replace with actual filtered data
      const sampleData = [
        {
          'Stock Receipt No': 'SR001',
          'Purchase Order No': 'PO001',
          'Warehouse': 'Main Warehouse',
          'Order Date': '2025-09-10',
          'Status': 'Open'
        },
        {
          'Stock Receipt No': 'SR002',
          'Purchase Order No': 'PO002',
          'Warehouse': 'Secondary Warehouse',
          'Order Date': '2025-09-11',
          'Status': 'Confirmed'
        }
      ]

      const worksheet = XLSX.utils.json_to_sheet(sampleData)
      const workbook = XLSX.utils.book_new()
      XLSX.utils.book_append_sheet(workbook, worksheet, `Receipt Stock - ${activeTab}`)

      const fileName = `Receipt_Stock_${activeTab}_${new Date().toISOString().split('T')[0]}.xlsx`
      XLSX.writeFile(workbook, fileName)
    } catch (error) {
      console.error('Error exporting to Excel:', error)
      alert('Error exporting to Excel. Please try again.')
    }
  }

  const handleExportToCSV = () => {
    try {
      // Sample data for demonstration - replace with actual filtered data
      const sampleData = [
        {
          'Stock Receipt No': 'SR001',
          'Purchase Order No': 'PO001',
          'Warehouse': 'Main Warehouse',
          'Order Date': '2025-09-10',
          'Status': 'Open'
        },
        {
          'Stock Receipt No': 'SR002',
          'Purchase Order No': 'PO002',
          'Warehouse': 'Secondary Warehouse',
          'Order Date': '2025-09-11',
          'Status': 'Confirmed'
        }
      ]

      const worksheet = XLSX.utils.json_to_sheet(sampleData)
      const workbook = XLSX.utils.book_new()
      XLSX.utils.book_append_sheet(workbook, worksheet, `Receipt Stock - ${activeTab}`)

      const fileName = `Receipt_Stock_${activeTab}_${new Date().toISOString().split('T')[0]}.csv`
      XLSX.writeFile(workbook, fileName, { bookType: 'csv' })
    } catch (error) {
      console.error('Error exporting to CSV:', error)
      alert('Error exporting to CSV. Please try again.')
    }
  }

  const handleExportToPDF = () => {
    try {
      const doc = new jsPDF()

      // Sample data for demonstration - replace with actual filtered data
      const sampleData = [
        ['SR001', 'PO001', 'Main Warehouse', '2025-09-10', 'Open'],
        ['SR002', 'PO002', 'Secondary Warehouse', '2025-09-11', 'Confirmed']
      ]

      const columns = [
        'Stock Receipt No',
        'Purchase Order No',
        'Warehouse',
        'Order Date',
        'Status'
      ]

      // Add title
      doc.setFontSize(18)
      doc.text(`Receipt Stock - ${activeTab}`, 14, 20)

      // Add date range
      doc.setFontSize(12)
      doc.text(`Date Range: ${startDate} to ${endDate}`, 14, 30)

      // Add table
      doc.autoTable({
        head: [columns],
        body: sampleData,
        startY: 40,
        styles: {
          fontSize: 10,
          cellPadding: 3
        },
        headStyles: {
          fillColor: [66, 139, 202],
          textColor: [255, 255, 255]
        }
      })

      const fileName = `Receipt_Stock_${activeTab}_${new Date().toISOString().split('T')[0]}.pdf`
      doc.save(fileName)
    } catch (error) {
      console.error('Error exporting to PDF:', error)
      alert('Error exporting to PDF. Please try again.')
    }
  }

  return (
    <div className="min-h-screen bg-gray-50 p-4">
      {/* Header Section */}
      <div className="bg-white rounded-lg shadow-sm p-6 mb-4">
        <div className="flex flex-col space-y-4">
          <div>
            <h1 className="text-2xl font-bold text-blue-700">Receipt Stock</h1>
            <div className="flex items-center gap-2 mt-2 text-sm text-gray-600">
              <span>Home</span>
              <span>»</span>
              <span className="text-gray-900">Receipt Stock</span>
            </div>
          </div>

          {/* Search Criteria */}
          <div className="flex items-center gap-4 text-sm">
            <span className="font-medium">Search Criteria : Purchase Order No:</span>
            <select className="px-3 py-1 border border-gray-300 rounded bg-white text-sm font-bold">
              <option value="All">All</option>
            </select>
            <span className="ml-4">Start Date:</span>
            <span className="font-bold">09 Sep, 2025</span>
            <span className="ml-4">End Date:</span>
            <span className="font-bold">16 Sep, 2025</span>
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

      {/* Filter Section - Only show when filter is clicked */}
      {showFilterSection && (
        <div className="bg-white rounded-lg shadow-sm p-4 mb-4">
          <div className="border border-gray-300 rounded p-4">
            <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-4">
              {/* Purchase Order No */}
              <div>
                <label className="block text-sm font-medium mb-2">Purchase Order No</label>
                <input
                  type="text"
                  value={purchaseOrderNo}
                  onChange={(e) => setPurchaseOrderNo(e.target.value)}
                  className="w-full px-3 py-2 border border-gray-300 rounded"
                  placeholder="Enter purchase order number"
                />
              </div>

              {/* Status */}
              <div>
                <label className="block text-sm font-medium mb-2">Status</label>
                <select
                  value={status}
                  onChange={(e) => setStatus(e.target.value)}
                  className="w-full px-3 py-2 border border-gray-300 rounded bg-white"
                >
                  <option value="Select">Select</option>
                  <option value="Open">Open</option>
                  <option value="Confirmed">Confirmed</option>
                  <option value="Manual">Manual</option>
                </select>
              </div>

              {/* Start Date */}
              <div>
                <label className="block text-sm font-medium mb-2">Start Date</label>
                <div className="relative">
                  <input
                    type="date"
                    value={startDate.split('/').reverse().join('-')}
                    onChange={(e) => {
                      const date = new Date(e.target.value)
                      setStartDate(date.toLocaleDateString('en-GB'))
                    }}
                    className="w-full px-3 py-2 border border-gray-300 rounded"
                  />
                  <Calendar className="absolute right-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400 pointer-events-none" />
                </div>
              </div>
            </div>

            {/* End Date */}
            <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-4">
              <div>
                <label className="block text-sm font-medium mb-2">End Date</label>
                <div className="relative">
                  <input
                    type="date"
                    value={endDate.split('/').reverse().join('-')}
                    onChange={(e) => {
                      const date = new Date(e.target.value)
                      setEndDate(date.toLocaleDateString('en-GB'))
                    }}
                    className="w-full px-3 py-2 border border-gray-300 rounded"
                  />
                  <Calendar className="absolute right-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400 pointer-events-none" />
                </div>
              </div>
            </div>

            {/* Search and Reset Buttons */}
            <div className="flex gap-3">
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

      {/* Status Tabs and Export Buttons */}
      <div className="bg-white rounded-lg shadow-sm overflow-hidden">
        <div className="border-b border-gray-200">
          <div className="flex justify-between items-center">
            {/* Status Tabs */}
            <div className="flex">
              {tabs.map((tab) => (
                <button
                  key={tab}
                  onClick={() => setActiveTab(tab)}
                  className={`px-6 py-3 text-sm font-medium border-b-2 ${
                    activeTab === tab
                      ? 'border-blue-600 text-blue-600 bg-blue-50'
                      : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
                  }`}
                >
                  {tab}
                </button>
              ))}
            </div>

            {/* Export Buttons */}
            <div className="flex gap-2 p-3">
              <Button
                onClick={handleExportToExcel}
                variant="outline"
                className="border-blue-600 text-blue-600 hover:bg-blue-50 px-4 py-2 text-sm"
              >
                Export To Excel
              </Button>
              <Button
                onClick={handleExportToCSV}
                variant="outline"
                className="border-blue-600 text-blue-600 hover:bg-blue-50 px-4 py-2 text-sm"
              >
                Export To CSV
              </Button>
              <Button
                onClick={handleExportToPDF}
                variant="outline"
                className="border-blue-600 text-blue-600 hover:bg-blue-50 px-4 py-2 text-sm"
              >
                Export To PDF
              </Button>
            </div>
          </div>
        </div>

        {/* Table */}
        <div className="overflow-x-auto">
          <table className="min-w-full">
            <thead className="bg-gray-600 text-white">
              <tr>
                <th className="px-4 py-2 text-left text-sm font-medium">Stock Receipt No</th>
                <th className="px-4 py-2 text-left text-sm font-medium">Purchase Order No</th>
                <th className="px-4 py-2 text-left text-sm font-medium">Warehouse</th>
                <th className="px-4 py-2 text-left text-sm font-medium">Order Date</th>
                <th className="px-4 py-2 text-left text-sm font-medium">Status</th>
                <th className="px-4 py-2 text-center text-sm font-medium">Actions</th>
              </tr>
            </thead>
            <tbody className="bg-white">
              {loading ? (
                <tr>
                  <td colSpan="6" className="px-4 py-8 text-center">
                    <div className="animate-pulse">Loading...</div>
                  </td>
                </tr>
              ) : receipts.length === 0 ? (
                <tr>
                  <td colSpan="6" className="px-4 py-8 text-center text-gray-500">
                    No record found
                  </td>
                </tr>
              ) : (
                receipts.map((receipt, index) => (
                  <tr key={receipt.id} className="hover:bg-gray-50 border-b border-gray-200">
                    <td className="px-4 py-3 text-sm">{receipt.receiptNo}</td>
                    <td className="px-4 py-3 text-sm">{receipt.purchaseOrderNo}</td>
                    <td className="px-4 py-3 text-sm">{receipt.warehouse}</td>
                    <td className="px-4 py-3 text-sm">{receipt.orderDate}</td>
                    <td className="px-4 py-3 text-sm">{receipt.status}</td>
                    <td className="px-4 py-3 text-sm text-center">
                      <div className="flex items-center justify-center gap-1">
                        {/* Action buttons would go here */}
                      </div>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  )
}

export default ReceiptStock