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

export default function ActivityLogReportPage() {
  const params = useParams()
  const router = useRouter()
  const planId = params.id as string

  const [expandedStep, setExpandedStep] = useState<number | null>(null)
  const [showDeliveryNoteModal, setShowDeliveryNoteModal] = useState(false)

  const [gateEntryData, setGateEntryData] = useState({
    securityOfficer: 'Vasanth Kumar',
    arrivalHour: '15',
    arrivalMin: '00',
    notifyLBCL: true
  })

  const [unloadingData, setUnloadingData] = useState({
    forkLiftOperator: 'Tarun Prasad',
    unloadStartHour: '16',
    unloadStartMin: '02',
    unloadEndHour: '16',
    unloadEndMin: '58'
  })

  const [loadEmptyData, setLoadEmptyData] = useState({
    forkLiftOperator: 'Tarun Prasad',
    loadStartHour: '17',
    loadStartMin: '25',
    loadEndHour: '18',
    loadEndMin: '08'
  })

  const [gatePassData, setGatePassData] = useState({
    securityOfficer: 'Vasanth Kumar',
    departureHour: '18',
    departureMin: '14',
    notifyLBCL: true
  })

  const workflowSteps = [
    {
      id: 1,
      title: 'View Delivery Note',
      icon: <FileText className="w-5 h-5 text-red-600" />,
      expandable: false,
      completed: true,
      onClick: () => setShowDeliveryNoteModal(true)
    },
    {
      id: 2,
      title: 'Gate Entry',
      icon: null,
      expandable: true,
      completed: false
    },
    {
      id: 3,
      title: 'Physical Count & Perform Stock Receiving',
      icon: null,
      expandable: false,
      completed: false,
      onClick: () => router.push('/manager/physical-count')
    },
    {
      id: 4,
      title: 'Unloading',
      icon: null,
      expandable: true,
      completed: false
    },
    {
      id: 5,
      title: 'Load Empty Stock',
      icon: null,
      expandable: true,
      completed: false
    },
    {
      id: 6,
      title: 'Gate Pass (Empties)',
      icon: null,
      expandable: true,
      completed: false
    }
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
          <h1 className="text-xl font-bold">Agent Stock Receiving Activity Log Report</h1>
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

      {/* Header Details */}
      <Card>
        <CardContent className="pt-6">
          <div className="grid grid-cols-6 gap-4 text-sm">
            <div>
              <p className="text-xs text-muted-foreground mb-1">Agent Stock Receiving No</p>
              <p className="font-bold">ASR{planId}</p>
            </div>
            <div>
              <p className="text-xs text-muted-foreground mb-1">SAP Delivery Note No</p>
              <p className="font-bold">{planId}</p>
            </div>
            <div>
              <p className="text-xs text-muted-foreground mb-1">Distributor</p>
              <p className="font-bold">[5844] R.T DISTRIBUTOR</p>
            </div>
            <div>
              <p className="text-xs text-muted-foreground mb-1">Prime Mover</p>
              <p className="font-bold">LK1673 (U KUMAR)</p>
            </div>
            <div>
              <p className="text-xs text-muted-foreground mb-1">Date</p>
              <p className="font-bold">20 MAY 2025</p>
            </div>
            <div>
              <p className="text-xs text-muted-foreground mb-1">LBCL Departure Time</p>
              <p className="font-bold">13:26 HRS</p>
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
                  <span className="font-medium text-sm">{step.title}</span>
                </div>
                <div className="flex items-center gap-2">
                  {step.icon}
                  {step.expandable && (
                    expandedStep === step.id ? <ChevronDown className="w-5 h-5" /> : <ChevronRight className="w-5 h-5" />
                  )}
                  {!step.expandable && step.onClick && <ChevronRight className="w-5 h-5 text-gray-400" />}
                </div>
              </div>

              {/* Expanded Content for Step 2 - Gate Entry */}
              {step.id === 2 && expandedStep === 2 && (
                <div className="mt-4 pt-4 border-t space-y-4">
                  <div className="grid grid-cols-2 gap-4">
                    <div>
                      <label className="text-xs font-medium mb-2 block">Select Security Officer</label>
                      <Input value={gateEntryData.securityOfficer} onChange={(e) => setGateEntryData({...gateEntryData, securityOfficer: e.target.value})} className="text-sm" />
                    </div>
                    <div>
                      <label className="text-xs font-medium mb-2 block">Prime Mover Arrival</label>
                      <div className="flex gap-2">
                        <Input value={gateEntryData.arrivalHour} onChange={(e) => setGateEntryData({...gateEntryData, arrivalHour: e.target.value})} className="text-sm text-center flex-1" />
                        <span className="text-xs self-end pb-2">HH</span>
                        <Input value={gateEntryData.arrivalMin} onChange={(e) => setGateEntryData({...gateEntryData, arrivalMin: e.target.value})} className="text-sm text-center flex-1" />
                        <span className="text-xs self-end pb-2">Min</span>
                      </div>
                    </div>
                  </div>
                  <div className="flex items-center gap-2">
                    <span className="text-sm">Notify LBCL Logistics</span>
                    <Check className="w-4 h-4 text-green-600" />
                  </div>
                </div>
              )}

              {/* Expanded Content for Step 4 - Unloading */}
              {step.id === 4 && expandedStep === 4 && (
                <div className="mt-4 pt-4 border-t space-y-4">
                  <div className="grid grid-cols-2 gap-4">
                    <div>
                      <label className="text-xs font-medium mb-2 block">Agent Fork Lift Operator</label>
                      <Input value={unloadingData.forkLiftOperator} onChange={(e) => setUnloadingData({...unloadingData, forkLiftOperator: e.target.value})} className="text-sm" />
                    </div>
                    <div className="grid grid-cols-2 gap-2">
                      <div>
                        <label className="text-xs font-medium mb-2 block">Unload Start Time</label>
                        <div className="flex gap-1">
                          <Input value={unloadingData.unloadStartHour} onChange={(e) => setUnloadingData({...unloadingData, unloadStartHour: e.target.value})} className="text-sm text-center" />
                          <span className="text-xs self-end pb-2">HH</span>
                          <Input value={unloadingData.unloadStartMin} onChange={(e) => setUnloadingData({...unloadingData, unloadStartMin: e.target.value})} className="text-sm text-center" />
                          <span className="text-xs self-end pb-2">Min</span>
                        </div>
                      </div>
                      <div>
                        <label className="text-xs font-medium mb-2 block">Unload End Time</label>
                        <div className="flex gap-1">
                          <Input value={unloadingData.unloadEndHour} onChange={(e) => setUnloadingData({...unloadingData, unloadEndHour: e.target.value})} className="text-sm text-center" />
                          <span className="text-xs self-end pb-2">HH</span>
                          <Input value={unloadingData.unloadEndMin} onChange={(e) => setUnloadingData({...unloadingData, unloadEndMin: e.target.value})} className="text-sm text-center" />
                          <span className="text-xs self-end pb-2">Min</span>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              )}

              {/* Expanded Content for Step 5 - Load Empty Stock */}
              {step.id === 5 && expandedStep === 5 && (
                <div className="mt-4 pt-4 border-t space-y-4">
                  <div className="grid grid-cols-2 gap-4">
                    <div>
                      <label className="text-xs font-medium mb-2 block">Agent Fork Lift Operator</label>
                      <Input value={loadEmptyData.forkLiftOperator} onChange={(e) => setLoadEmptyData({...loadEmptyData, forkLiftOperator: e.target.value})} className="text-sm" />
                    </div>
                    <div className="grid grid-cols-2 gap-2">
                      <div>
                        <label className="text-xs font-medium mb-2 block">Load Start Time</label>
                        <div className="flex gap-1">
                          <Input value={loadEmptyData.loadStartHour} onChange={(e) => setLoadEmptyData({...loadEmptyData, loadStartHour: e.target.value})} className="text-sm text-center" />
                          <span className="text-xs self-end pb-2">HH</span>
                          <Input value={loadEmptyData.loadStartMin} onChange={(e) => setLoadEmptyData({...loadEmptyData, loadStartMin: e.target.value})} className="text-sm text-center" />
                          <span className="text-xs self-end pb-2">Min</span>
                        </div>
                      </div>
                      <div>
                        <label className="text-xs font-medium mb-2 block">Load End Time</label>
                        <div className="flex gap-1">
                          <Input value={loadEmptyData.loadEndHour} onChange={(e) => setLoadEmptyData({...loadEmptyData, loadEndHour: e.target.value})} className="text-sm text-center" />
                          <span className="text-xs self-end pb-2">HH</span>
                          <Input value={loadEmptyData.loadEndMin} onChange={(e) => setLoadEmptyData({...loadEmptyData, loadEndMin: e.target.value})} className="text-sm text-center" />
                          <span className="text-xs self-end pb-2">Min</span>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              )}

              {/* Expanded Content for Step 6 - Gate Pass (Empties) */}
              {step.id === 6 && expandedStep === 6 && (
                <div className="mt-4 pt-4 border-t space-y-4">
                  <div className="grid grid-cols-2 gap-4">
                    <div>
                      <label className="text-xs font-medium mb-2 block">Select Security Officer</label>
                      <Input value={gatePassData.securityOfficer} onChange={(e) => setGatePassData({...gatePassData, securityOfficer: e.target.value})} className="text-sm" />
                    </div>
                    <div>
                      <label className="text-xs font-medium mb-2 block">Prime Mover Departure</label>
                      <div className="flex gap-2">
                        <Input value={gatePassData.departureHour} onChange={(e) => setGatePassData({...gatePassData, departureHour: e.target.value})} className="text-sm text-center flex-1" />
                        <span className="text-xs self-end pb-2">HH</span>
                        <Input value={gatePassData.departureMin} onChange={(e) => setGatePassData({...gatePassData, departureMin: e.target.value})} className="text-sm text-center flex-1" />
                        <span className="text-xs self-end pb-2">Min</span>
                      </div>
                    </div>
                  </div>
                  <div className="flex items-center gap-2">
                    <span className="text-sm">Notify LBCL Logistics</span>
                    <Check className="w-4 h-4 text-green-600" />
                  </div>
                </div>
              )}
            </CardContent>
          </Card>
        ))}
      </div>

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
            <h2 className="text-xl font-bold mb-4">View Delivery Note</h2>
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
    </div>
  )
}
