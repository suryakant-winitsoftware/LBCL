"use client"

import { useState } from 'react'
import { useParams, useRouter } from 'next/navigation'
import { Card, CardContent } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import {
  ChevronDown,
  ChevronRight,
  FileText,
  Check,
  X
} from 'lucide-react'

export default function DeliveryActivityLogPage() {
  const params = useParams()
  const router = useRouter()
  const planId = params.id as string

  const [expandedStep, setExpandedStep] = useState<number | null>(null)
  const [showPickListModal, setShowPickListModal] = useState(false)
  const [showDeliveryNoteModal, setShowDeliveryNoteModal] = useState(false)
  const [showReceiveStockModal, setShowReceiveStockModal] = useState(false)

  const [loadingData, setLoadingData] = useState({
    primeMover: 'LK1673',
    driver: 'U Kumar',
    forkLiftOperator: 'Praveen Varma',
    loadStartTime: '11',
    loadStartMin: '14',
    loadEndTime: '12',
    loadEndMin: '25'
  })

  const [gatePassData, setGatePassData] = useState({
    securityOfficer: 'Vasanth Kumar',
    departureHour: '13',
    departureMin: '26',
    notifyAgency: true
  })

  const workflowSteps = [
    {
      id: 1,
      title: 'Share Delivery Plan',
      icon: null,
      expandable: false,
      completed: true,
      onClick: () => router.push('/delivery/share-delivery-plan')
    },
    {
      id: 2,
      title: 'View / Generate Pick List',
      icon: <FileText className="w-5 h-5 text-red-600" />,
      expandable: false,
      completed: true,
      onClick: () => setShowPickListModal(true)
    },
    {
      id: 3,
      title: 'Loading',
      icon: null,
      expandable: true,
      completed: false
    },
    {
      id: 4,
      title: 'View / Generate Delivery Note',
      icon: <FileText className="w-5 h-5 text-red-600" />,
      expandable: false,
      completed: false,
      onClick: () => setShowDeliveryNoteModal(true)
    },
    {
      id: 5,
      title: 'Receive Stock',
      icon: <FileText className="w-5 h-5 text-red-600" />,
      expandable: false,
      completed: false,
      onClick: () => setShowReceiveStockModal(true)
    },
    {
      id: 6,
      title: 'Gate Pass',
      icon: null,
      expandable: true,
      completed: false
    }
  ]

  const progressSteps = [
    { label: 'ABC Warehouse', sublabel: 'Home Base', time: 'Reached', status: 'completed' },
    { label: 'Checkpoint Alpha', sublabel: '20 Kms Checkpoint', time: '12:14 PM', status: 'completed' },
    { label: 'Checkpoint Beta', sublabel: '40 Kms Checkpoint', time: '', status: 'current' },
    { label: 'Distributor Agent Warehouse', sublabel: 'Final Arrival', time: '', status: 'pending' }
  ]

  const toggleStep = (stepId: number) => {
    if (expandedStep === stepId) {
      setExpandedStep(null)
    } else {
      setExpandedStep(stepId)
    }
  }

  return (
    <div className="space-y-4">
      {/* Header */}
      <div className="flex items-center justify-between py-4 px-2">
        <div className="flex items-center gap-4">
          <h1 className="text-xl font-bold">Delivery Plan Activity Log Report</h1>
        </div>
        <div className="flex items-center gap-2">
          <Button variant="outline" onClick={() => router.back()}>
            Back
          </Button>
          <Button className="bg-[#9d8b5c] hover:bg-[#8a7a50] text-white">
            Submit
          </Button>
        </div>
      </div>

      {/* Plan Details */}
      <Card>
        <CardContent className="pt-6">
          <div className="grid grid-cols-3 gap-4">
            <div>
              <p className="text-xs text-muted-foreground mb-1">Delivery Plan No</p>
              <p className="font-bold text-lg">{planId}</p>
            </div>
            <div>
              <p className="text-xs text-muted-foreground mb-1">Distributor / Agent</p>
              <p className="font-bold text-lg">[5844] R.T DISTRIBUTOR</p>
            </div>
            <div>
              <p className="text-xs text-muted-foreground mb-1">Date</p>
              <p className="font-bold text-lg">20 MAY 2025</p>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Workflow Steps */}
      <div className="grid grid-cols-2 gap-4">
        {workflowSteps.map((step) => (
          <Card
            key={step.id}
            className={`${step.expandable ? 'cursor-pointer' : ''} ${step.onClick ? 'cursor-pointer hover:border-primary' : ''}`}
            onClick={() => {
              if (step.expandable) {
                toggleStep(step.id)
              } else if (step.onClick) {
                step.onClick()
              }
            }}
          >
            <CardContent className="p-4">
              <div className="flex items-center justify-between">
                <div className="flex items-center gap-3">
                  <div className="w-8 h-8 rounded-full bg-[#f5f0e5] flex items-center justify-center">
                    <span className="text-sm font-bold text-[#9d8b5c]">{step.id}</span>
                  </div>
                  <span className="font-medium">{step.title}</span>
                </div>
                <div className="flex items-center gap-2">
                  {step.icon}
                  {step.expandable && (
                    expandedStep === step.id ? <ChevronDown className="w-5 h-5" /> : <ChevronRight className="w-5 h-5" />
                  )}
                  {!step.expandable && step.onClick && <ChevronRight className="w-5 h-5 text-gray-400" />}
                </div>
              </div>

              {/* Expanded Content for Step 3 - Loading */}
              {step.id === 3 && expandedStep === 3 && (
                <div className="mt-4 pt-4 border-t space-y-4">
                  <div className="grid grid-cols-2 gap-4">
                    <div>
                      <label className="text-xs font-medium mb-2 block">Select Prime Mover</label>
                      <Input
                        value={loadingData.primeMover}
                        onChange={(e) => setLoadingData({...loadingData, primeMover: e.target.value})}
                        className="text-sm"
                      />
                    </div>
                    <div>
                      <label className="text-xs font-medium mb-2 block">Select Driver</label>
                      <Input
                        value={loadingData.driver}
                        onChange={(e) => setLoadingData({...loadingData, driver: e.target.value})}
                        className="text-sm"
                      />
                    </div>
                  </div>
                  <div className="grid grid-cols-2 gap-4">
                    <div>
                      <label className="text-xs font-medium mb-2 block">Fork Lift Operator</label>
                      <Input
                        value={loadingData.forkLiftOperator}
                        onChange={(e) => setLoadingData({...loadingData, forkLiftOperator: e.target.value})}
                        className="text-sm"
                      />
                    </div>
                    <div className="grid grid-cols-2 gap-2">
                      <div>
                        <label className="text-xs font-medium mb-2 block">Load Start Time</label>
                        <div className="flex gap-1">
                          <Input
                            value={loadingData.loadStartTime}
                            onChange={(e) => setLoadingData({...loadingData, loadStartTime: e.target.value})}
                            className="text-sm text-center"
                          />
                          <span className="text-xs self-end pb-2">HH</span>
                          <Input
                            value={loadingData.loadStartMin}
                            onChange={(e) => setLoadingData({...loadingData, loadStartMin: e.target.value})}
                            className="text-sm text-center"
                          />
                          <span className="text-xs self-end pb-2">Min</span>
                        </div>
                      </div>
                      <div>
                        <label className="text-xs font-medium mb-2 block">Load End Time</label>
                        <div className="flex gap-1">
                          <Input
                            value={loadingData.loadEndTime}
                            onChange={(e) => setLoadingData({...loadingData, loadEndTime: e.target.value})}
                            className="text-sm text-center"
                          />
                          <span className="text-xs self-end pb-2">HH</span>
                          <Input
                            value={loadingData.loadEndMin}
                            onChange={(e) => setLoadingData({...loadingData, loadEndMin: e.target.value})}
                            className="text-sm text-center"
                          />
                          <span className="text-xs self-end pb-2">Min</span>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              )}

              {/* Expanded Content for Step 6 - Gate Pass */}
              {step.id === 6 && expandedStep === 6 && (
                <div className="mt-4 pt-4 border-t space-y-4">
                  <div className="grid grid-cols-2 gap-4">
                    <div>
                      <label className="text-xs font-medium mb-2 block">Select Security Officer</label>
                      <Input
                        value={gatePassData.securityOfficer}
                        onChange={(e) => setGatePassData({...gatePassData, securityOfficer: e.target.value})}
                        className="text-sm"
                      />
                    </div>
                    <div>
                      <label className="text-xs font-medium mb-2 block">Prime Mover Departure</label>
                      <div className="flex gap-2">
                        <Input
                          value={gatePassData.departureHour}
                          onChange={(e) => setGatePassData({...gatePassData, departureHour: e.target.value})}
                          className="text-sm text-center flex-1"
                        />
                        <span className="text-xs self-end pb-2">HH</span>
                        <Input
                          value={gatePassData.departureMin}
                          onChange={(e) => setGatePassData({...gatePassData, departureMin: e.target.value})}
                          className="text-sm text-center flex-1"
                        />
                        <span className="text-xs self-end pb-2">Min</span>
                      </div>
                    </div>
                  </div>
                  <div className="flex items-center gap-2">
                    <span className="text-sm">Notify Agency</span>
                    <div className="flex items-center gap-1">
                      <Check className="w-4 h-4 text-green-600" />
                    </div>
                  </div>
                </div>
              )}
            </CardContent>
          </Card>
        ))}
      </div>

      {/* Progress Timeline */}
      <Card>
        <CardContent className="pt-6">
          <div className="relative">
            <div className="absolute top-6 left-0 right-0 h-2 bg-gray-200 rounded-full overflow-hidden">
              <div className="h-full bg-gradient-to-r from-green-500 via-yellow-500 to-transparent w-1/2" />
            </div>
            <div className="relative flex justify-between">
              {progressSteps.map((step, index) => (
                <div key={index} className="flex flex-col items-center" style={{ width: '25%' }}>
                  <div
                    className={`w-12 h-12 rounded-full flex items-center justify-center mb-2 ${
                      step.status === 'completed'
                        ? 'bg-green-500'
                        : step.status === 'current'
                        ? 'bg-blue-500'
                        : 'bg-gray-300'
                    }`}
                  >
                    {step.status === 'completed' ? (
                      <Check className="w-6 h-6 text-white" />
                    ) : (
                      <div className={`w-6 h-6 rounded-full border-2 ${
                        step.status === 'current' ? 'border-white' : 'border-gray-400'
                      }`} />
                    )}
                  </div>
                  <p className="text-xs font-medium text-center">{step.label}</p>
                  <p className="text-xs text-muted-foreground text-center">{step.sublabel}</p>
                  {step.time && <p className="text-xs text-muted-foreground mt-1">{step.time}</p>}
                </div>
              ))}
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Pick List Modal */}
      {showPickListModal && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-lg max-w-4xl w-full p-6 relative max-h-[90vh] overflow-y-auto">
            <button
              onClick={() => setShowPickListModal(false)}
              className="absolute top-4 right-4 text-gray-400 hover:text-gray-600"
            >
              <X className="w-5 h-5" />
            </button>
            <h2 className="text-xl font-bold mb-4">View / Generate Pick List</h2>
            <div className="border rounded-lg p-4 bg-gray-50 min-h-[400px]">
              <p className="text-sm text-gray-600 mb-4">Pick List Document Preview</p>
              <div className="bg-white p-4 border rounded">
                <p className="text-xs text-gray-500">Pick List details would be displayed here...</p>
              </div>
            </div>
            <Button onClick={() => setShowPickListModal(false)} className="w-full mt-4 bg-[#9d8b5c] hover:bg-[#8a7a50]">
              DONE
            </Button>
          </div>
        </div>
      )}

      {/* Delivery Note Modal */}
      {showDeliveryNoteModal && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-lg max-w-4xl w-full p-6 relative max-h-[90vh] overflow-y-auto">
            <button
              onClick={() => setShowDeliveryNoteModal(false)}
              className="absolute top-4 right-4 text-gray-400 hover:text-gray-600"
            >
              <X className="w-5 h-5" />
            </button>
            <h2 className="text-xl font-bold mb-4">View / Generate Delivery Note</h2>
            <div className="border rounded-lg p-4 bg-gray-50 min-h-[400px]">
              <p className="text-sm text-gray-600 mb-4">Delivery Note Document Preview</p>
              <div className="bg-white p-4 border rounded">
                <p className="text-xs text-gray-500">Delivery Note details would be displayed here...</p>
              </div>
            </div>
            <Button onClick={() => setShowDeliveryNoteModal(false)} className="w-full mt-4 bg-[#9d8b5c] hover:bg-[#8a7a50]">
              DONE
            </Button>
          </div>
        </div>
      )}

      {/* Receive Stock Modal */}
      {showReceiveStockModal && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-lg max-w-4xl w-full p-6 relative max-h-[90vh] overflow-y-auto">
            <button
              onClick={() => setShowReceiveStockModal(false)}
              className="absolute top-4 right-4 text-gray-400 hover:text-gray-600"
            >
              <X className="w-5 h-5" />
            </button>
            <h2 className="text-xl font-bold mb-4">Receive Stock</h2>
            <div className="border rounded-lg p-4 bg-gray-50 min-h-[400px]">
              <p className="text-sm text-gray-600 mb-4">Stock Receiving Form</p>
              <div className="bg-white p-4 border rounded space-y-4">
                <div>
                  <label className="text-sm font-medium">Items Received</label>
                  <Input placeholder="Enter quantity" className="mt-2" />
                </div>
                <div>
                  <label className="text-sm font-medium">Notes</label>
                  <textarea
                    className="w-full mt-2 p-2 border rounded resize-none"
                    rows={4}
                    placeholder="Enter any notes about the stock received..."
                  />
                </div>
              </div>
            </div>
            <Button onClick={() => setShowReceiveStockModal(false)} className="w-full mt-4 bg-[#9d8b5c] hover:bg-[#8a7a50]">
              DONE
            </Button>
          </div>
        </div>
      )}
    </div>
  )
}
