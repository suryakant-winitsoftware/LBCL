"use client"
import { useState, useEffect } from 'react'
import { useRouter } from 'next/navigation'
import { Button } from '../../../components/ui/button'
import { 
  Menu, 
  ChevronRight,
  ChevronDown,
  FileText,
  Share2,
  ClipboardList,
  Package,
  FileCheck,
  Truck,
  Shield,
  Clock,
  Users,
  CheckCircle,
  AlertCircle,
  Activity
} from 'lucide-react'
import { useAuth } from '../../../contexts/AuthContext'
import { useWorkflow } from '../../../contexts/WorkflowContext'
import ProtectedRoute from '../../../components/ProtectedRoute'
import BusinessLayout from '../../../components/layouts/BusinessLayout'
import ViewDeliveryNote from '../../manager/components/ViewDeliveryNote'

const DeliveryActivityLog = ({ planId }) => {
  const router = useRouter()
  const { markSubmissionSuccessful, isStepCompleted, markStepCompleted } = useWorkflow()
  const [expandedSections, setExpandedSections] = useState({ loading: false, gatepass: false, deliveryNote: false, receiveStock: false })
  const [selectedPrimeMover, setSelectedPrimeMover] = useState('')
  const [selectedDriver, setSelectedDriver] = useState('U Kumar')
  const [selectedForkLiftOperator, setSelectedForkLiftOperator] = useState('Praveen Varma')
  const [loadStartTime, setLoadStartTime] = useState({ hour: '11', minute: '14' })
  const [loadEndTime, setLoadEndTime] = useState({ hour: '12', minute: '25' })
  const [selectedSecurityOfficer, setSelectedSecurityOfficer] = useState('')
  const [primeMoverDeparture, setPrimeMoverDeparture] = useState({ hour: '13', minute: '26' })
  const [notifyAgency, setNotifyAgency] = useState(true)
  const [deliveryNoteGenerated, setDeliveryNoteGenerated] = useState(false)
  const [showDeliveryNote, setShowDeliveryNote] = useState(false)
  const [receivedQuantity, setReceivedQuantity] = useState('')
  const [receivedCondition, setReceivedCondition] = useState('Good')
  const [receivedNotes, setReceivedNotes] = useState('')

  const toggleSection = (section) => {
    setExpandedSections(prev => {
      // Close all sections first, then toggle the clicked one
      const allClosed = {
        loading: false,
        gatepass: false,
        deliveryNote: false,
        receiveStock: false
      }
      return {
        ...allClosed,
        [section]: !prev[section]
      }
    })
  }

  const handleBack = () => {
    router.back()
  }

  const handleSubmit = () => {
    // Validate required fields
    if (!selectedPrimeMover || !selectedDriver || !selectedForkLiftOperator) {
      alert('Please complete all required fields in the Loading section!')
      setExpandedSections(prev => ({ ...prev, loading: true }))
      return
    }
    if (!selectedSecurityOfficer) {
      alert('Please select a Security Officer in the Gate Pass section!')
      setExpandedSections(prev => ({ ...prev, gatepass: true }))
      return
    }
    
    // Mark all steps as completed
    markStepCompleted('loading', planId)
    markStepCompleted('delivery-note', planId)
    markStepCompleted('receive-stock', planId)
    markStepCompleted('gate-pass', planId)
    
    // Handle form submission
    markSubmissionSuccessful()
    alert('Activity log submitted successfully!')
    // Navigate to Share Delivery Plan after successful submission
    router.push('/user/delivery/share-delivery-plan')
  }

  const handleShareDeliveryPlan = () => {
    router.push('/user/delivery/share-delivery-plan')
  }

  const handleViewPickList = () => {
    router.push('/user/delivery/pick-list-view')
  }

  const handleReceiveStock = () => {
    router.push('/user/delivery/signature-capture')
  }

  const handleGenerateDeliveryNote = () => {
    setTimeout(() => {
      setDeliveryNoteGenerated(true)
      markStepCompleted('delivery-note', planId)
      alert('Delivery note generated successfully!')
    }, 1500)
  }

  const handleViewDeliveryNote = () => {
    if (!deliveryNoteGenerated) {
      alert('Please generate the delivery note first!')
      return
    }
    setShowDeliveryNote(true)
  }

  const handleSaveLoading = () => {
    if (loadStartTime.hour && loadStartTime.minute && loadEndTime.hour && loadEndTime.minute) {
      markStepCompleted('loading', planId)
      alert('Loading information saved successfully!')
      setExpandedSections(prev => ({ ...prev, loading: false }))
    } else {
      alert('Please fill in all time fields!')
    }
  }

  const handleSaveGatePass = () => {
    if (primeMoverDeparture.hour && primeMoverDeparture.minute) {
      markStepCompleted('gate-pass', planId)
      alert('Gate pass information saved successfully!')
      setExpandedSections(prev => ({ ...prev, gatepass: false }))
    } else {
      alert('Please fill in the departure time!')
    }
  }

  const handleSaveReceiveStock = () => {
    if (receivedQuantity && receivedCondition) {
      markStepCompleted('receive-stock', planId)
      alert('Stock receipt information saved successfully!')
      setExpandedSections(prev => ({ ...prev, receiveStock: false }))
    } else {
      alert('Please fill in quantity and condition!')
    }
  }

  // Fetch delivery data from centralized data.json
  const [deliveryData, setDeliveryData] = useState({})
  const [defaultValues, setDefaultValues] = useState({})

  useEffect(() => {
    const fetchData = async () => {
      try {
        const response = await fetch('/data.json')
        const data = await response.json()
        setDeliveryData(data.deliveryData)
        setDefaultValues(data.defaultValues)
        // Set default values from JSON
        setSelectedPrimeMover(data.defaultValues.selectedPrimeMover || '')
        setSelectedSecurityOfficer(data.defaultValues.selectedSecurityOfficer || '')
      } catch (error) {
        console.error('Error fetching data:', error)
      }
    }
    fetchData()
  }, [])

  return (
    <ProtectedRoute requiredSystem="delivery">
      <BusinessLayout title="Delivery Plan Activity Log">
        <div className="space-y-6">
          {/* Header with Actions */}
          <div className="flex items-center justify-between">
            <h1 style={{color: '#000000'}} className="text-2xl font-bold">Delivery Plan Activity Log Report</h1>
            <div className="flex items-center gap-2">
              <Button 
                onClick={handleBack}
                style={{backgroundColor: '#ffffff', color: '#000000', border: '1px solid #000000'}}
                className="text-sm px-2 py-1"
              >
                Back
              </Button>
              <Button 
                onClick={handleSubmit}
                style={{backgroundColor: '#375AE6', color: '#ffffff', border: '1px solid #000000'}}
                className="text-sm px-2 py-1"
              >
                Submit
              </Button>
            </div>
          </div>

          {/* Plan Info Cards */}
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            <div className="bg-white rounded-lg shadow-sm border border-black p-4">
              <div className="flex items-center gap-2 mb-2">
                <FileText className="w-5 h-5 text-[#375AE6]" />
                <div className="text-gray-600 text-sm font-medium uppercase">Delivery Plan No</div>
              </div>
              <div className="font-bold text-gray-900 text-lg">{deliveryData.planNo}</div>
            </div>
            <div className="bg-white rounded-lg shadow-sm border border-black p-4">
              <div className="flex items-center gap-2 mb-2">
                <Users className="w-5 h-5 text-[#375AE6]" />
                <div className="text-gray-600 text-sm font-medium uppercase">Distributor / Agent</div>
              </div>
              <div className="font-bold text-gray-900 text-lg">{deliveryData.distributor}</div>
            </div>
            <div className="bg-white rounded-lg shadow-sm border border-black p-4">
              <div className="flex items-center gap-2 mb-2">
                <Clock className="w-5 h-5 text-[#375AE6]" />
                <div className="text-gray-600 text-sm font-medium uppercase">Date</div>
              </div>
              <div className="font-bold text-gray-900 text-lg">{deliveryData.date}</div>
            </div>
          </div>

        {/* Activity Steps - Compact Layout */}
        <div className="p-4">
          <div className="grid grid-cols-1 gap-3">
            {/* Step 1: Share Delivery Plan */}
            <div className="bg-white rounded-lg border border-black">
              <button
                onClick={handleShareDeliveryPlan}
                className="w-full flex items-center justify-between text-left p-4 rounded-lg"
              >
                <div className="flex items-center gap-3">
                  <div className={`w-10 h-10 ${isStepCompleted('share-delivery', planId) ? 'bg-[#375AE6]' : 'bg-white border-2 border-[#375AE6]'} rounded-full flex items-center justify-center ${isStepCompleted('share-delivery', planId) ? 'text-white' : 'text-[#375AE6]'} font-bold`}>
                    {isStepCompleted('share-delivery', planId) ? <CheckCircle className="w-5 h-5" /> : <Share2 className="w-5 h-5" />}
                  </div>
                  <div>
                    <span className="font-semibold text-black">Share Delivery Plan</span>
                    {isStepCompleted('share-delivery', planId) && (
                      <div className="text-xs text-[#375AE6] font-medium mt-1">✓ Completed</div>
                    )}
                  </div>
                </div>
                <ChevronRight className="w-5 h-5 text-black" />
              </button>
            </div>

            {/* Step 2: View/Generate Pick List */}
            <div className="bg-white rounded-lg border border-black">
              <button
                onClick={handleViewPickList}
                className="w-full flex items-center justify-between text-left p-4 rounded-lg"
              >
                <div className="flex items-center gap-3">
                  <div className={`w-10 h-10 ${isStepCompleted('pick-list', planId) ? 'bg-[#375AE6]' : 'bg-white border-2 border-[#375AE6]'} rounded-full flex items-center justify-center ${isStepCompleted('pick-list', planId) ? 'text-white' : 'text-[#375AE6]'} font-bold`}>
                    {isStepCompleted('pick-list', planId) ? <CheckCircle className="w-5 h-5" /> : <ClipboardList className="w-5 h-5" />}
                  </div>
                  <div>
                    <span className="font-semibold text-black">View / Generate Pick List</span>
                    {isStepCompleted('pick-list', planId) && (
                      <div className="text-xs text-[#375AE6] font-medium mt-1">✓ Generated</div>
                    )}
                  </div>
                </div>
                <div className="flex items-center gap-2">
                  <FileText className="w-4 h-4 text-[#375AE6]" />
                  <ChevronRight className="w-5 h-5 text-black" />
                </div>
              </button>
            </div>

            {/* Step 3: Loading - Expandable */}
            <div className="bg-white rounded-lg border border-black">
              <button
                onClick={() => toggleSection('loading')}
                className="w-full flex items-center justify-between text-left p-4 rounded-t-lg"
              >
                <div className="flex items-center gap-3">
                  <div className={`w-10 h-10 ${isStepCompleted('loading', planId) ? 'bg-[#375AE6]' : 'bg-white border-2 border-[#375AE6]'} rounded-full flex items-center justify-center ${isStepCompleted('loading', planId) ? 'text-white' : 'text-[#375AE6]'} font-bold`}>
                    {isStepCompleted('loading', planId) ? <CheckCircle className="w-5 h-5" /> : <Package className="w-5 h-5" />}
                  </div>
                  <div>
                    <span className="font-semibold text-black">Loading</span>
                    {isStepCompleted('loading', planId) && (
                      <div className="text-xs text-[#375AE6] font-medium mt-1">✓ Completed</div>
                    )}
                  </div>
                </div>
                {expandedSections.loading ? (
                  <ChevronDown className="w-5 h-5 text-black" />
                ) : (
                  <ChevronRight className="w-5 h-5 text-black" />
                )}
              </button>
            
              {expandedSections.loading && (
                <div className="px-4 pb-4 pt-2 bg-white border-t border-black rounded-b-lg">
                  <div className="space-y-4">
                  <div className="grid grid-cols-2 gap-4">
                    <div>
                      <label className="block text-sm font-semibold text-black mb-2 flex items-center gap-2">
                        <Truck className="w-4 h-4 text-[#375AE6]" />
                        Select Prime Mover
                      </label>
                      <select 
                        value={selectedPrimeMover}
                        onChange={(e) => setSelectedPrimeMover(e.target.value)}
                        className="w-full border-2 border-black rounded-lg px-4 py-2 bg-white focus:outline-none focus:ring-2 focus:ring-[#375AE6] focus:border-[#375AE6]"
                      >
                        {defaultValues.primeMoverOptions?.map((primeMover, index) => (
                          <option key={index} value={primeMover}>{primeMover}</option>
                        ))}
                      </select>
                    </div>
                    <div>
                      <label className="block text-sm font-semibold text-black mb-2 flex items-center gap-2">
                        <Users className="w-4 h-4 text-[#375AE6]" />
                        Select Driver
                      </label>
                      <select 
                        value={selectedDriver}
                        onChange={(e) => setSelectedDriver(e.target.value)}
                        className="w-full border-2 border-black rounded-lg px-4 py-2 bg-white focus:outline-none focus:ring-2 focus:ring-[#375AE6] focus:border-[#375AE6]"
                      >
                        <option value="U Kumar">U Kumar</option>
                        <option value="S Perera">S Perera</option>
                        <option value="R Silva">R Silva</option>
                      </select>
                    </div>
                  </div>
                  
                  <div className="grid grid-cols-3 gap-4">
                    <div>
                      <label className="block text-sm text-black mb-2">Fork Lift Operator</label>
                      <select 
                        value={selectedForkLiftOperator}
                        onChange={(e) => setSelectedForkLiftOperator(e.target.value)}
                        className="w-full border border-black rounded-lg px-3 py-2 bg-white focus:outline-none focus:ring-2 focus:ring-[#375AE6] focus:border-[#375AE6]"
                      >
                        <option value="Praveen Varma">Praveen Varma</option>
                        <option value="Sunil Kumar">Sunil Kumar</option>
                        <option value="Raj Patel">Raj Patel</option>
                      </select>
                    </div>
                    <div>
                      <label className="block text-sm text-black mb-2">Load Start Time</label>
                      <div className="flex gap-1">
                        <input 
                          type="number" 
                          value={loadStartTime.hour}
                          onChange={(e) => setLoadStartTime(prev => ({ ...prev, hour: e.target.value }))}
                          className="w-16 border border-black rounded-lg px-2 py-2 text-center focus:outline-none focus:ring-2 focus:ring-[#375AE6] focus:border-[#375AE6]"
                          placeholder="HH"
                          min="0"
                          max="23"
                        />
                        <span className="self-center">:</span>
                        <input 
                          type="number" 
                          value={loadStartTime.minute}
                          onChange={(e) => setLoadStartTime(prev => ({ ...prev, minute: e.target.value }))}
                          className="w-16 border border-black rounded-lg px-2 py-2 text-center focus:outline-none focus:ring-2 focus:ring-[#375AE6] focus:border-[#375AE6]"
                          placeholder="MM"
                          min="0"
                          max="59"
                        />
                      </div>
                    </div>
                    <div>
                      <label className="block text-sm text-black mb-2">Load End Time</label>
                      <div className="flex gap-1">
                        <input 
                          type="number" 
                          value={loadEndTime.hour}
                          onChange={(e) => setLoadEndTime(prev => ({ ...prev, hour: e.target.value }))}
                          className="w-16 border border-black rounded-lg px-2 py-2 text-center focus:outline-none focus:ring-2 focus:ring-[#375AE6] focus:border-[#375AE6]"
                          placeholder="HH"
                          min="0"
                          max="23"
                        />
                        <span className="self-center">:</span>
                        <input 
                          type="number" 
                          value={loadEndTime.minute}
                          onChange={(e) => setLoadEndTime(prev => ({ ...prev, minute: e.target.value }))}
                          className="w-16 border border-black rounded-lg px-2 py-2 text-center focus:outline-none focus:ring-2 focus:ring-[#375AE6] focus:border-[#375AE6]"
                          placeholder="MM"
                          min="0"
                          max="59"
                        />
                      </div>
                    </div>
                  </div>
                </div>
                <div className="mt-4 flex justify-end">
                  <Button 
                    size="md"
                    onClick={handleSaveLoading}
                    className="bg-[#375AE6] text-white px-6 py-2 rounded-lg flex items-center gap-2"
                  >
                    <CheckCircle className="w-4 h-4" />
                    Save Loading Info
                  </Button>
                </div>
                </div>
              )}
            </div>

            {/* Step 4: View/Generate Delivery Note */}
            <div className="bg-white rounded-lg border border-black">
              <button 
                onClick={() => toggleSection('deliveryNote')}
                className="w-full flex items-center justify-between text-left p-4 rounded-t-lg"
              >
                <div className="flex items-center gap-3">
                  <div className={`w-10 h-10 ${isStepCompleted('delivery-note', planId) ? 'bg-[#375AE6]' : 'bg-white border-2 border-[#375AE6]'} rounded-full flex items-center justify-center ${isStepCompleted('delivery-note', planId) ? 'text-white' : 'text-[#375AE6]'} font-bold`}>
                    {isStepCompleted('delivery-note', planId) ? <CheckCircle className="w-5 h-5" /> : <FileCheck className="w-5 h-5" />}
                  </div>
                  <div>
                    <span className="font-semibold text-black">View / Generate Delivery Note</span>
                    {deliveryNoteGenerated && (
                      <div className="text-xs text-[#375AE6] font-medium mt-1">✓ Generated</div>
                    )}
                  </div>
                </div>
                <div className="flex items-center gap-2">
                  <FileText className="w-4 h-4 text-[#375AE6]" />
                  {expandedSections.deliveryNote ? (
                    <ChevronDown className="w-5 h-5 text-black" />
                  ) : (
                    <ChevronRight className="w-5 h-5 text-black" />
                  )}
                </div>
              </button>
            
              {expandedSections.deliveryNote && (
                <div className="px-4 pb-4 pt-2 bg-white border-t border-black rounded-b-lg">
                  <div className="space-y-4">
                    <div className="bg-white border-l-4 border-[#375AE6] p-3 rounded-lg">
                      <div className="flex items-center gap-2">
                        <Activity className="w-4 h-4 text-[#375AE6]" />
                        <p className="text-sm text-[#375AE6] font-semibold">Generate or view the delivery note for this shipment.</p>
                      </div>
                    </div>
                    <div className="flex gap-3">
                      <Button 
                        size="md"
                        onClick={handleGenerateDeliveryNote}
                        disabled={deliveryNoteGenerated}
                        className={deliveryNoteGenerated ? 
                          "px-6 py-2 border-2 border-[#375AE6] text-white bg-[#375AE6] rounded-lg flex items-center gap-2" : 
                          "bg-[#375AE6] text-white px-6 py-2 rounded-lg flex items-center gap-2"
                        }
                      >
                        {deliveryNoteGenerated ? (
                          <>
                            <CheckCircle className="w-4 h-4" /> 
                            Generated
                          </>
                        ) : (
                          <>
                            <FileCheck className="w-4 h-4" /> 
                            Generate Note
                          </>
                        )}
                      </Button>
                      <Button 
                        variant="outline"
                        size="md"
                        onClick={handleViewDeliveryNote}
                        disabled={!deliveryNoteGenerated}
                        className={!deliveryNoteGenerated ? 
                          "px-6 py-2 border-2 border-black text-black bg-white cursor-not-allowed rounded-lg flex items-center gap-2" :
                          "px-6 py-2 border-2 border-[#375AE6] text-[#375AE6] rounded-lg flex items-center gap-2"
                        }
                      >
                        <FileText className="w-4 h-4" />
                        View PDF
                      </Button>
                    </div>
                  </div>
                </div>
              )}
          </div>

            {/* Step 5: Receive Stock */}
            <div className="bg-white rounded-lg border border-black">
              <button 
                onClick={() => toggleSection('receiveStock')}
                className="w-full flex items-center justify-between text-left p-4 rounded-t-lg"
              >
                <div className="flex items-center gap-3">
                  <div className={`w-10 h-10 ${isStepCompleted('receive-stock', planId) ? 'bg-[#375AE6]' : 'bg-white border-2 border-[#375AE6]'} rounded-full flex items-center justify-center ${isStepCompleted('receive-stock', planId) ? 'text-white' : 'text-[#375AE6]'} font-bold`}>
                    {isStepCompleted('receive-stock', planId) ? <CheckCircle className="w-5 h-5" /> : <Package className="w-5 h-5" />}
                  </div>
                  <div>
                    <span className="font-semibold text-black">Receive Stock</span>
                    {isStepCompleted('receive-stock', planId) && (
                      <div className="text-xs text-[#375AE6] font-medium mt-1">✓ Received</div>
                    )}
                  </div>
                </div>
                <div className="flex items-center gap-2">
                  <FileText className="w-4 h-4 text-[#375AE6]" />
                  {expandedSections.receiveStock ? (
                    <ChevronDown className="w-5 h-5 text-black" />
                  ) : (
                    <ChevronRight className="w-5 h-5 text-black" />
                  )}
                </div>
              </button>
            
              {expandedSections.receiveStock && (
                <div className="px-4 pb-4 pt-2 bg-white border-t border-black rounded-b-lg">
                  <div className="space-y-4">
                    <div className="grid grid-cols-2 gap-4">
                      <div>
                        <label className="block text-sm font-semibold text-black mb-2 flex items-center gap-2">
                          <Package className="w-4 h-4 text-[#375AE6]" />
                          Received Quantity
                        </label>
                        <input 
                          type="number"
                          value={receivedQuantity}
                          onChange={(e) => setReceivedQuantity(e.target.value)}
                          placeholder="Enter quantity"
                          className="w-full border-2 border-black rounded-lg px-4 py-2 focus:outline-none focus:ring-2 focus:ring-[#375AE6] focus:border-[#375AE6]"
                        />
                    </div>
                      <div>
                        <label className="block text-sm font-semibold text-black mb-2 flex items-center gap-2">
                          <CheckCircle className="w-4 h-4 text-[#375AE6]" />
                          Condition
                        </label>
                        <select 
                          value={receivedCondition}
                          onChange={(e) => setReceivedCondition(e.target.value)}
                          className="w-full border-2 border-black rounded-lg px-4 py-2 bg-white focus:outline-none focus:ring-2 focus:ring-[#375AE6] focus:border-[#375AE6]"
                      >
                        <option value="Good">Good</option>
                        <option value="Damaged">Damaged</option>
                        <option value="Partial">Partial</option>
                      </select>
                    </div>
                  </div>
                  <div>
                    <label className="block text-sm text-black mb-2">Notes</label>
                    <textarea
                      value={receivedNotes}
                      onChange={(e) => setReceivedNotes(e.target.value)}
                      placeholder="Enter any notes about the received stock"
                      className="w-full border border-black rounded-lg px-3 py-2 h-20 resize-none focus:outline-none focus:ring-2 focus:ring-[#375AE6] focus:border-[#375AE6]"
                    />
                  </div>
                    <div className="flex gap-3 mt-4">
                      <Button 
                        size="md"
                        onClick={handleSaveReceiveStock}
                        className="bg-[#375AE6] text-white px-6 py-2 rounded-lg flex items-center gap-2"
                      >
                        <CheckCircle className="w-4 h-4" />
                        Save Receipt Info
                      </Button>
                      <Button 
                        variant="outline"
                        size="md"
                        onClick={handleReceiveStock}
                        className="border-2 border-[#375AE6] text-[#375AE6] px-6 py-2 rounded-lg flex items-center gap-2"
                      >
                        <FileText className="w-4 h-4" />
                        Capture Signature
                      </Button>
                    </div>
                </div>
              </div>
            )}
          </div>

            {/* Step 6: Gate Pass - Expandable */}
            <div className="bg-white rounded-lg border border-black">
              <button
                onClick={() => toggleSection('gatepass')}
                className="w-full flex items-center justify-between text-left p-4 rounded-t-lg"
              >
                <div className="flex items-center gap-3">
                  <div className={`w-10 h-10 ${isStepCompleted('gate-pass', planId) ? 'bg-[#375AE6]' : 'bg-white border-2 border-[#375AE6]'} rounded-full flex items-center justify-center ${isStepCompleted('gate-pass', planId) ? 'text-white' : 'text-[#375AE6]'} font-bold`}>
                    {isStepCompleted('gate-pass', planId) ? <CheckCircle className="w-5 h-5" /> : <Shield className="w-5 h-5" />}
                  </div>
                  <div>
                    <span className="font-semibold text-black">Gate Pass</span>
                    {isStepCompleted('gate-pass', planId) && (
                      <div className="text-xs text-[#375AE6] font-medium mt-1">✓ Processed</div>
                    )}
                  </div>
                </div>
                {expandedSections.gatepass ? (
                  <ChevronDown className="w-5 h-5 text-black" />
                ) : (
                  <ChevronRight className="w-5 h-5 text-black" />
                )}
              </button>
            
              {expandedSections.gatepass && (
                <div className="px-4 pb-4 pt-2 bg-white border-t border-black rounded-b-lg">
                  <div className="space-y-4">
                    <div className="grid grid-cols-2 gap-4">
                      <div>
                        <label className="block text-sm font-semibold text-black mb-2 flex items-center gap-2">
                          <Shield className="w-4 h-4 text-[#375AE6]" />
                          Select Security Officer
                        </label>
                        <select 
                          value={selectedSecurityOfficer}
                          onChange={(e) => setSelectedSecurityOfficer(e.target.value)}
                          className="w-full border-2 border-black rounded-lg px-4 py-2 bg-white focus:outline-none focus:ring-2 focus:ring-[#375AE6] focus:border-[#375AE6]"
                      >
                        {defaultValues.securityOfficerOptions?.map((officer, index) => (
                          <option key={index} value={officer}>{officer}</option>
                        ))}
                      </select>
                    </div>
                    <div>
                      <label className="block text-sm text-black mb-2">Prime Mover Departure</label>
                      <div className="flex gap-1">
                        <input 
                          type="number" 
                          value={primeMoverDeparture.hour}
                          onChange={(e) => setPrimeMoverDeparture(prev => ({ ...prev, hour: e.target.value }))}
                          className="w-16 border border-black rounded-lg px-2 py-2 text-center focus:outline-none focus:ring-2 focus:ring-[#375AE6] focus:border-[#375AE6]"
                          placeholder="HH"
                          min="0"
                          max="23"
                        />
                        <span className="self-center">:</span>
                        <input 
                          type="number" 
                          value={primeMoverDeparture.minute}
                          onChange={(e) => setPrimeMoverDeparture(prev => ({ ...prev, minute: e.target.value }))}
                          className="w-16 border border-black rounded-lg px-2 py-2 text-center focus:outline-none focus:ring-2 focus:ring-[#375AE6] focus:border-[#375AE6]"
                          placeholder="MM"
                          min="0"
                          max="59"
                        />
                      </div>
                    </div>
                  </div>
                  
                  <div className="flex items-center gap-2">
                    <input 
                      type="checkbox" 
                      id="notify" 
                      checked={notifyAgency}
                      onChange={(e) => setNotifyAgency(e.target.checked)}
                      className="rounded border-black text-[#375AE6] focus:ring-[#375AE6]" 
                    />
                    <label htmlFor="notify" className="text-sm text-black">
                      Notify Agency {notifyAgency && '✓'}
                    </label>
                  </div>
                </div>
                  <div className="mt-4 flex justify-end">
                    <Button 
                      size="md"
                      onClick={handleSaveGatePass}
                      className="bg-[#375AE6] text-white px-6 py-2 rounded-lg flex items-center gap-2"
                    >
                      <CheckCircle className="w-4 h-4" />
                      Save Gate Pass Info
                    </Button>
                  </div>
              </div>
            )}
          </div>
          </div>
        </div>

        {/* Status Tracking Bar - Compact */}
        <div className="bg-white border-t-2 border-black px-4 py-3">
          <div className="text-[#375AE6] text-sm flex items-center gap-2 mb-2">
            <div className="w-2 h-2 bg-[#375AE6] rounded-full"></div>
            Current pace suggests arrival at destination may be delayed by ~15 minutes
          </div>
          
          <div className="flex items-center justify-between text-xs">
            <div className="bg-[#375AE6] p-2 rounded text-center flex-1">
              <div className="w-2 h-2 bg-white rounded-full mx-auto mb-1"></div>
              <div className="text-white font-medium">LBC Warehouse</div>
              <div className="text-white">Distance 5.8 km</div>
              <div className="text-white">Travel time: 12-25min</div>
            </div>
            <div className="px-2 text-[#375AE6] text-lg font-bold">→</div>
            <div className="bg-[#375AE6] p-2 rounded text-center flex-1">
              <div className="w-2 h-2 bg-white rounded-full mx-auto mb-1"></div>
              <div className="text-white font-medium">Checkpoint Alpha</div>
              <div className="text-white">Distance 48 km arrived</div>
              <div className="text-white">Travel time: 50-70 min</div>
            </div>
            <div className="px-2 text-[#375AE6] text-lg font-bold">→</div>
            <div className="bg-[#375AE6] p-2 rounded text-center flex-1">
              <div className="w-2 h-2 bg-white rounded-full mx-auto mb-1"></div>
              <div className="text-white font-medium">Checkpoint Beta</div>
              <div className="text-white">Distance 22.8km ETA from start</div>
              <div className="text-white">Travel time: 18-30min</div>
            </div>
            <div className="px-2 text-[#375AE6] text-lg font-bold">→</div>
            <div className="bg-[#375AE6] p-2 rounded text-center flex-1">
              <div className="w-2 h-2 bg-white rounded-full mx-auto mb-1"></div>
              <div className="text-white font-medium">Distributor Agent Warehouse</div>
              <div className="text-white">Pending</div>
              <div className="text-white">Distance 48 km planned</div>
            </div>
          </div>
        </div>

          {/* View Delivery Note Modal */}
          <ViewDeliveryNote 
            isOpen={showDeliveryNote} 
            onClose={() => setShowDeliveryNote(false)} 
          />
        </div>
      </BusinessLayout>
    </ProtectedRoute>
  )
}

export default DeliveryActivityLog