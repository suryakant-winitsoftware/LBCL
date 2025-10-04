"use client"
import { useState, useEffect } from 'react'
import { useRouter } from 'next/navigation'
import { 
  ChevronDown,
  ChevronUp,
  Check,
  FileText,
  Package,
  Clock,
  User,
  Truck,
  Shield,
  Calendar,
  MapPin
} from 'lucide-react'
import { Button } from '../../../components/ui/button'
import BusinessLayout from '../../../components/layouts/BusinessLayout'
import ProtectedRoute from '../../../components/ProtectedRoute'
import ViewDeliveryNote from './ViewDeliveryNote'

const ActivityLogReport = () => {
  const router = useRouter()
  const [expandedSteps, setExpandedSteps] = useState({
    2: true,
    4: true,
    5: true,
    6: true
  })
  const [showDeliveryNote, setShowDeliveryNote] = useState(false)
  
  // Fetch activity log data from centralized data.json
  const [activityData, setActivityData] = useState({})
  const [formData, setFormData] = useState({})
  const [defaultValues, setDefaultValues] = useState({})

  useEffect(() => {
    const fetchData = async () => {
      try {
        const response = await fetch('/data.json')
        const data = await response.json()
        setActivityData(data.activityLog)
        setFormData(data.activityLog.formData)
        setDefaultValues(data.defaultValues)
      } catch (error) {
        console.error('Error fetching data:', error)
      }
    }
    fetchData()
  }, [])

  const toggleStep = (stepId) => {
    setExpandedSteps(prev => ({
      ...prev,
      [stepId]: !prev[stepId]
    }))
  }

  const handleBack = () => {
    router.back()
  }

  const handleSubmit = () => {
    console.log('Form submitted:', formData)
    router.push('/user/manager/physical-count')
  }

  const handleViewDeliveryNote = () => {
    setShowDeliveryNote(true)
  }

  const handlePhysicalCount = () => {
    router.push('/user/manager/physical-count')
  }

  const handleInputChange = (field, value) => {
    setFormData(prev => ({
      ...prev,
      [field]: value
    }))
  }

  const steps = [
    {
      id: 1,
      title: "View Delivery Note",
      icon: FileText,
      completed: true,
      expandable: false,
      action: handleViewDeliveryNote
    },
    {
      id: 2,
      title: "Prime Mover Arrival",
      icon: Truck,
      completed: true,
      expandable: true
    },
    {
      id: 3,
      title: "Start Unloading",
      icon: Package,
      completed: true,
      expandable: false
    },
    {
      id: 4,
      title: "End Unloading",
      icon: Package,
      completed: true,
      expandable: true
    },
    {
      id: 5,
      title: "Start Empty Loading",
      icon: Package,
      completed: true,
      expandable: true
    },
    {
      id: 6,
      title: "End Empty Loading / Prime Mover Departure",
      icon: Truck,
      completed: true,
      expandable: true
    },
    {
      id: 7,
      title: "Physical Count",
      icon: Package,
      completed: false,
      expandable: false,
      action: handlePhysicalCount
    }
  ]

  return (
    <ProtectedRoute requiredSystem="stock">
      <BusinessLayout title="Activity Log Report">
        <div className="min-h-screen bg-gray-50 p-6">
          {/* Page Header */}
          <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6 mb-6">
            <div className="flex items-center justify-between">
              <div>
                <h1 className="text-2xl font-bold text-gray-900 mb-2">Agent Stock Receiving Activity Log</h1>
                <p className="text-gray-600">Track and manage stock receiving activities</p>
              </div>
              <div className="flex items-center gap-3">
                <Button 
                  onClick={handleBack}
                  variant="outline"
                  className="px-4 py-2 border-gray-300 text-gray-700 hover:bg-gray-50"
                >
                  Back
                </Button>
                <Button 
                  onClick={handleSubmit}
                  className="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white"
                >
                  Submit Report
                </Button>
              </div>
            </div>
          </div>

          {/* Delivery Information Card */}
          <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6 mb-6">
            <div className="flex items-center gap-3 mb-4">
              <Calendar className="w-5 h-5 text-gray-700" />
              <h2 className="text-lg font-semibold text-gray-900">Delivery Information</h2>
            </div>
            <div className="grid grid-cols-1 md:grid-cols-3 lg:grid-cols-6 gap-4">
              <div className="bg-gray-50 rounded-lg p-4 border border-gray-100">
                <div className="text-sm text-gray-600 mb-1">ASR No</div>
                <div className="font-semibold text-gray-900">{activityData.agentStockReceivingNo}</div>
              </div>
              <div className="bg-gray-50 rounded-lg p-4 border border-gray-100">
                <div className="text-sm text-gray-600 mb-1">SAP Note</div>
                <div className="font-semibold text-gray-900">{activityData.sapDeliveryNoteNo}</div>
              </div>
              <div className="bg-gray-50 rounded-lg p-4 border border-gray-100">
                <div className="text-sm text-gray-600 mb-1">Distributor</div>
                <div className="font-semibold text-gray-900">{activityData.distributor}</div>
              </div>
              <div className="bg-gray-50 rounded-lg p-4 border border-gray-100">
                <div className="text-sm text-gray-600 mb-1">Prime Mover</div>
                <div className="font-semibold text-gray-900">{activityData.primeMover}</div>
              </div>
              <div className="bg-gray-50 rounded-lg p-4 border border-gray-100">
                <div className="text-sm text-gray-600 mb-1">Date</div>
                <div className="font-semibold text-gray-900">{activityData.date}</div>
              </div>
              <div className="bg-gray-50 rounded-lg p-4 border border-gray-100">
                <div className="text-sm text-gray-600 mb-1">Departure</div>
                <div className="font-semibold text-gray-900">{activityData.lionDepartureTime}</div>
              </div>
            </div>
          </div>

          {/* Activity Steps */}
          <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
            <div className="flex items-center gap-3 mb-6">
              <Package className="w-5 h-5 text-gray-700" />
              <h2 className="text-lg font-semibold text-gray-900">Activity Log Steps</h2>
            </div>

            <div className="space-y-4">
              {steps.map((step) => (
                <div key={step.id} className="border border-gray-200 rounded-lg bg-gray-50">
                  <div className="p-4">
                    <div className="flex items-center justify-between">
                      <div className="flex items-center gap-4">
                        <div 
                          className={`w-12 h-12 flex items-center justify-center rounded-lg ${
                            step.completed 
                              ? 'bg-green-600 text-white' 
                              : 'bg-white text-gray-600 border-2 border-gray-300'
                          }`}
                        >
                          {step.completed ? (
                            <Check className="w-6 h-6" />
                          ) : (
                            <step.icon className="w-6 h-6" />
                          )}
                        </div>
                        <div>
                          <h3 className="text-lg font-semibold text-gray-900">{step.title}</h3>
                          <p className="text-sm text-gray-600">
                            Status: <span className={`font-medium ${step.completed ? 'text-green-600' : 'text-orange-600'}`}>
                              {step.completed ? 'Completed' : 'Pending'}
                            </span>
                          </p>
                        </div>
                      </div>
                      
                      <div className="flex items-center gap-3">
                        {step.action && (
                          <Button
                            onClick={step.action}
                            className="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white"
                          >
                            {step.id === 1 ? 'View Note' : 'Start Process'}
                          </Button>
                        )}
                        {step.expandable && (
                          <button
                            onClick={() => toggleStep(step.id)}
                            className="p-2 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors duration-150"
                          >
                            {expandedSteps[step.id] ? (
                              <ChevronUp className="w-5 h-5 text-gray-600" />
                            ) : (
                              <ChevronDown className="w-5 h-5 text-gray-600" />
                            )}
                          </button>
                        )}
                      </div>
                    </div>

                    {/* Expandable Content */}
                    {step.expandable && expandedSteps[step.id] && (
                      <div className="mt-6 pt-4 border-t border-gray-200 bg-white rounded-lg p-4">
                        {step.id === 2 && (
                          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                            <div>
                              <label className="block text-sm font-medium text-gray-700 mb-2">
                                <User className="w-4 h-4 inline mr-2" />
                                Security Officer
                              </label>
                              <select 
                                value={formData.securityOfficer || ''}
                                onChange={(e) => handleInputChange('securityOfficer', e.target.value)}
                                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                              >
                                {defaultValues.securityOfficerOptions?.map((officer, index) => (
                                  <option key={index} value={officer}>{officer}</option>
                                ))}
                              </select>
                            </div>
                            <div>
                              <label className="block text-sm font-medium text-gray-700 mb-2">
                                <Clock className="w-4 h-4 inline mr-2" />
                                Prime Arrival Time
                              </label>
                              <div className="flex gap-2 items-center">
                                <input 
                                  type="number" 
                                  value={formData.primeArrivalHour || ''}
                                  onChange={(e) => handleInputChange('primeArrivalHour', e.target.value)}
                                  className="w-20 px-3 py-2 text-center border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                                  placeholder="HH"
                                  min="0"
                                  max="23"
                                />
                                <span className="text-gray-600 font-semibold">:</span>
                                <input 
                                  type="number" 
                                  value={formData.primeArrivalMin || ''}
                                  onChange={(e) => handleInputChange('primeArrivalMin', e.target.value)}
                                  className="w-20 px-3 py-2 text-center border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                                  placeholder="MM"
                                  min="0"
                                  max="59"
                                />
                              </div>
                            </div>
                          </div>
                        )}

                        {step.id === 4 && (
                          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                            <div>
                              <label className="block text-sm font-medium text-gray-700 mb-2">
                                <Clock className="w-4 h-4 inline mr-2" />
                                Unload Start Time
                              </label>
                              <div className="flex gap-2 items-center">
                                <input 
                                  type="number" 
                                  value={formData.unloadStartHour || ''}
                                  onChange={(e) => handleInputChange('unloadStartHour', e.target.value)}
                                  className="w-20 px-3 py-2 text-center border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                                  placeholder="HH"
                                  min="0"
                                  max="23"
                                />
                                <span className="text-gray-600 font-semibold">:</span>
                                <input 
                                  type="number" 
                                  value={formData.unloadStartMin || ''}
                                  onChange={(e) => handleInputChange('unloadStartMin', e.target.value)}
                                  className="w-20 px-3 py-2 text-center border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                                  placeholder="MM"
                                  min="0"
                                  max="59"
                                />
                              </div>
                            </div>
                            <div>
                              <label className="block text-sm font-medium text-gray-700 mb-2">
                                <Clock className="w-4 h-4 inline mr-2" />
                                Unload End Time
                              </label>
                              <div className="flex gap-2 items-center">
                                <input 
                                  type="number" 
                                  value={formData.unloadEndHour || ''}
                                  onChange={(e) => handleInputChange('unloadEndHour', e.target.value)}
                                  className="w-20 px-3 py-2 text-center border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                                  placeholder="HH"
                                  min="0"
                                  max="23"
                                />
                                <span className="text-gray-600 font-semibold">:</span>
                                <input 
                                  type="number" 
                                  value={formData.unloadEndMin || ''}
                                  onChange={(e) => handleInputChange('unloadEndMin', e.target.value)}
                                  className="w-20 px-3 py-2 text-center border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                                  placeholder="MM"
                                  min="0"
                                  max="59"
                                />
                              </div>
                            </div>
                          </div>
                        )}

                        {step.id === 5 && (
                          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                            <div>
                              <label className="block text-sm font-medium text-gray-700 mb-2">
                                <Clock className="w-4 h-4 inline mr-2" />
                                Load Start Time
                              </label>
                              <div className="flex gap-2 items-center">
                                <input 
                                  type="number" 
                                  value={formData.loadStartHour || ''}
                                  onChange={(e) => handleInputChange('loadStartHour', e.target.value)}
                                  className="w-20 px-3 py-2 text-center border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                                  placeholder="HH"
                                  min="0"
                                  max="23"
                                />
                                <span className="text-gray-600 font-semibold">:</span>
                                <input 
                                  type="number" 
                                  value={formData.loadStartMin || ''}
                                  onChange={(e) => handleInputChange('loadStartMin', e.target.value)}
                                  className="w-20 px-3 py-2 text-center border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                                  placeholder="MM"
                                  min="0"
                                  max="59"
                                />
                              </div>
                            </div>
                            <div>
                              <label className="block text-sm font-medium text-gray-700 mb-2">
                                <Clock className="w-4 h-4 inline mr-2" />
                                Load End Time
                              </label>
                              <div className="flex gap-2 items-center">
                                <input 
                                  type="number" 
                                  value={formData.loadEndHour || ''}
                                  onChange={(e) => handleInputChange('loadEndHour', e.target.value)}
                                  className="w-20 px-3 py-2 text-center border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                                  placeholder="HH"
                                  min="0"
                                  max="23"
                                />
                                <span className="text-gray-600 font-semibold">:</span>
                                <input 
                                  type="number" 
                                  value={formData.loadEndMin || ''}
                                  onChange={(e) => handleInputChange('loadEndMin', e.target.value)}
                                  className="w-20 px-3 py-2 text-center border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                                  placeholder="MM"
                                  min="0"
                                  max="59"
                                />
                              </div>
                            </div>
                          </div>
                        )}

                        {step.id === 6 && (
                          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                            <div>
                              <label className="block text-sm font-medium text-gray-700 mb-2">
                                <Clock className="w-4 h-4 inline mr-2" />
                                Departure Time
                              </label>
                              <div className="flex gap-2 items-center">
                                <input 
                                  type="number" 
                                  value={formData.departureHour || ''}
                                  onChange={(e) => handleInputChange('departureHour', e.target.value)}
                                  className="w-20 px-3 py-2 text-center border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                                  placeholder="HH"
                                  min="0"
                                  max="23"
                                />
                                <span className="text-gray-600 font-semibold">:</span>
                                <input 
                                  type="number" 
                                  value={formData.departureMin || ''}
                                  onChange={(e) => handleInputChange('departureMin', e.target.value)}
                                  className="w-20 px-3 py-2 text-center border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                                  placeholder="MM"
                                  min="0"
                                  max="59"
                                />
                              </div>
                            </div>
                            <div>
                              <label className="block text-sm font-medium text-gray-700 mb-2">
                                <User className="w-4 h-4 inline mr-2" />
                                Fork Lift Operator
                              </label>
                              <select 
                                value={formData.forkLiftOperator || ''}
                                onChange={(e) => handleInputChange('forkLiftOperator', e.target.value)}
                                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                              >
                                {defaultValues.forkLiftOperatorOptions?.map((operator, index) => (
                                  <option key={index} value={operator}>{operator}</option>
                                ))}
                              </select>
                            </div>
                          </div>
                        )}
                      </div>
                    )}
                  </div>
                </div>
              ))}
            </div>
          </div>
        </div>

        {/* View Delivery Note Modal */}
        <ViewDeliveryNote 
          isOpen={showDeliveryNote} 
          onClose={() => setShowDeliveryNote(false)} 
        />
      </BusinessLayout>
    </ProtectedRoute>
  )
}

export default ActivityLogReport