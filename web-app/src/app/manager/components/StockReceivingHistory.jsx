"use client"
import { useState, useEffect } from 'react'
import { useRouter } from 'next/navigation'
import { 
  Search,
  FileText,
  Eye,
  History,
  Calendar,
  Package,
  Download,
  Clock,
  Filter,
  CheckCircle
} from 'lucide-react'
import { Button } from '../../../components/ui/button'
import DataTable from '../../../components/ui/DataTable'
import ProtectedRoute from '../../../components/ProtectedRoute'

const StockReceivingHistory = () => {
  const router = useRouter()
  const [searchTerm, setSearchTerm] = useState('')
  
  // Fetch history data from API
  const [historyData, setHistoryData] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
    const fetchData = async () => {
      try {
        // TODO: Replace with actual API endpoint for stock receiving history
        // For now, initialize with empty data to avoid showing mock data
        setHistoryData([])
      } catch (error) {
        console.error('Error fetching data:', error)
        setError('Failed to fetch stock receiving history')
        setHistoryData([])
      } finally {
        setLoading(false)
      }
    }
    fetchData()
  }, [])

  const handleActivityLog = (item) => {
    router.push(`/manager/timeline-stamps/${item.id}`)
  }

  const handleViewDetails = (item) => {
    router.push(`/manager/activity-log-report/${item.sapDeliveryNoteNo}`)
  }

  const columns = [
    {
      key: 'agentStockReceivingNo',
      title: 'ASR Number',
      render: (value) => (
        <div className="font-mono text-sm text-gray-900 font-semibold">{value}</div>
      )
    },
    {
      key: 'sapDeliveryNoteNo',
      title: 'SAP Delivery Note',
      render: (value) => (
        <div className="text-sm font-medium text-blue-600">{value}</div>
      )
    },
    {
      key: 'date',
      title: 'Date',
      render: (value) => (
        <div className="text-sm text-gray-700">{value}</div>
      )
    },
    {
      key: 'time',
      title: 'Time',
      render: (value) => (
        <div className="text-sm text-gray-600">{value}</div>
      )
    },
    {
      key: 'primeMover',
      title: 'Prime Mover',
      render: (value) => (
        <div className="text-sm text-gray-700">{value}</div>
      )
    },
    {
      key: 'totalTimeTaken',
      title: 'Processing Time',
      render: (value) => (
        <div className="text-sm font-medium text-gray-900">{value}</div>
      )
    },
    {
      key: 'status',
      title: 'Status',
      render: (value) => {
        const statusClasses = value === 'completed' 
          ? 'bg-green-100 text-green-800 border-green-200' 
          : 'bg-yellow-100 text-yellow-800 border-yellow-200'
        return (
          <span className={`inline-flex px-3 py-1 text-xs font-medium rounded-full border ${statusClasses}`}>
            {value === 'completed' ? 'Completed' : 'In Progress'}
          </span>
        )
      }
    }
  ]

  const actions = [
    {
      icon: History,
      title: 'View Timeline',
      onClick: handleActivityLog,
      className: 'text-gray-600 hover:text-blue-600'
    },
    {
      icon: Eye,
      title: 'View Details',
      onClick: handleViewDetails,
      className: 'text-gray-600 hover:text-green-600'
    },
    {
      icon: Download,
      title: 'Download Report',
      onClick: (row) => console.log('Download report:', row.id),
      className: 'text-gray-600 hover:text-purple-600'
    }
  ]

  const filteredData = historyData.filter(item =>
    item.sapDeliveryNoteNo?.toLowerCase().includes(searchTerm.toLowerCase()) ||
    item.agentStockReceivingNo?.toLowerCase().includes(searchTerm.toLowerCase()) ||
    item.primeMover?.toLowerCase().includes(searchTerm.toLowerCase())
  )

  const completedCount = historyData.filter(item => item.status === 'completed').length

  return (
    <ProtectedRoute requiredSystem="stock">
      <BusinessLayout title="Stock Receiving History">
        <div className="min-h-screen bg-gray-50 p-6">
          {/* Header */}
          <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6 mb-6">
            <div className="flex items-center justify-between">
              <div>
                <h1 className="text-2xl font-bold text-gray-900 mb-2">Stock Receiving History</h1>
                <p className="text-gray-600">Comprehensive view of all stock receiving operations</p>
              </div>
              <div className="flex items-center gap-2">
                <History className="w-5 h-5 text-gray-600" />
                <span className="text-sm text-gray-600">{historyData.length} Total Records</span>
              </div>
            </div>
          </div>

          {/* Summary Stats */}
          <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-6">
            <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
              <div className="flex items-center">
                <div className="w-12 h-12 bg-green-100 rounded-lg flex items-center justify-center">
                  <CheckCircle className="w-6 h-6 text-green-600" />
                </div>
                <div className="ml-4">
                  <p className="text-sm font-medium text-gray-600">Completed Operations</p>
                  <p className="text-2xl font-bold text-gray-900">{completedCount}</p>
                </div>
              </div>
            </div>

            <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
              <div className="flex items-center">
                <div className="w-12 h-12 bg-blue-100 rounded-lg flex items-center justify-center">
                  <Clock className="w-6 h-6 text-blue-600" />
                </div>
                <div className="ml-4">
                  <p className="text-sm font-medium text-gray-600">In Progress</p>
                  <p className="text-2xl font-bold text-gray-900">{historyData.length - completedCount}</p>
                </div>
              </div>
            </div>

            <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
              <div className="flex items-center">
                <div className="w-12 h-12 bg-purple-100 rounded-lg flex items-center justify-center">
                  <Calendar className="w-6 h-6 text-purple-600" />
                </div>
                <div className="ml-4">
                  <p className="text-sm font-medium text-gray-600">Today's Operations</p>
                  <p className="text-2xl font-bold text-gray-900">
                    {historyData.filter(item => new Date(item.date).toDateString() === new Date().toDateString()).length}
                  </p>
                </div>
              </div>
            </div>
          </div>

          {/* Search and Controls */}
          <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6 mb-6">
            <div className="flex flex-col sm:flex-row items-start sm:items-center gap-4">
              <div className="flex-1 w-full sm:w-auto">
                <div className="relative">
                  <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 w-5 h-5" />
                  <input
                    type="text"
                    placeholder="Search by delivery note, ASR number, or prime mover..."
                    value={searchTerm}
                    onChange={(e) => setSearchTerm(e.target.value)}
                    className="w-full pl-10 pr-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all duration-200"
                  />
                </div>
              </div>
              <div className="flex gap-3">
                <Button 
                  variant="outline"
                  className="px-4 py-3 border-gray-300 text-gray-700 hover:bg-gray-50"
                >
                  <Filter className="w-4 h-4 mr-2" />
                  Filter
                </Button>
                <Button className="px-4 py-3 bg-green-600 hover:bg-green-700 text-white">
                  <Download className="w-4 h-4 mr-2" />
                  Export Data
                </Button>
              </div>
            </div>
          </div>

          {/* Data Table */}
          <div className="bg-white rounded-lg shadow-sm border border-gray-200">
            <div className="p-6 border-b border-gray-200">
              <div className="flex items-center justify-between">
                <div className="flex items-center gap-3">
                  <FileText className="w-5 h-5 text-gray-700" />
                  <h2 className="text-lg font-semibold text-gray-900">Historical Records</h2>
                </div>
                <span className="text-sm text-gray-500">
                  Showing {filteredData.length} of {historyData.length} records
                </span>
              </div>
            </div>
            <DataTable
              data={filteredData}
              columns={columns}
              actions={actions}
              loading={loading}
              onRowClick={handleViewDetails}
              searchable={false}
              className="border-0"
            />
          </div>
        </div>
      
    </ProtectedRoute>
  )
}

export default StockReceivingHistory