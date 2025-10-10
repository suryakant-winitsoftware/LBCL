"use client"

import { useState, useEffect } from "react"
import { ChevronDown, ChevronRight, Clock, Bell, RefreshCw, FileText } from "lucide-react"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select"
import { Checkbox } from "@/components/ui/checkbox"
import { Dialog, DialogContent, DialogTitle } from "@/components/ui/dialog"
import { useRouter } from "next/navigation"
import purchaseOrderService from "@/services/purchaseOrder"
import { stockReceivingService } from "@/services/stockReceivingService"
import { deliveryLoadingService } from "@/services/deliveryLoadingService"
import { DeliveryNoteDialog } from "@/app/lbcl/components/delivery-note-dialog"
import { roleService } from "@/services/admin/role.service"
import { employeeService } from "@/services/admin/employee.service"
import { SuccessDialog } from "@/components/dialogs/success-dialog"

export default function StockReceivingActivityLogView({ deliveryId }: { deliveryId: string }) {
  const router = useRouter()
  const [expandedSections, setExpandedSections] = useState<Record<number, boolean>>({
    2: true,
    4: true,
    6: true,
  })
  const [showDeliveryNote, setShowDeliveryNote] = useState(false)
  const [purchaseOrder, setPurchaseOrder] = useState<any>(null)
  const [deliveryLoading, setDeliveryLoading] = useState<any>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState("")
  const [orderLines, setOrderLines] = useState<any[]>([])

  // Security Officer states
  const [securityOfficers, setSecurityOfficers] = useState<any[]>([])
  const [loadingSecurityOfficers, setLoadingSecurityOfficers] = useState(false)

  // Fork Lift Operator states
  const [forkLiftOperators, setForkLiftOperators] = useState<any[]>([])
  const [loadingForkLiftOperators, setLoadingForkLiftOperators] = useState(false)

  // Form state
  const [securityOfficer, setSecurityOfficer] = useState("")
  const [arrivalHour, setArrivalHour] = useState("")
  const [arrivalMin, setArrivalMin] = useState("")
  const [forkLiftOperator, setForkLiftOperator] = useState("")
  const [unloadStartHour, setUnloadStartHour] = useState("")
  const [unloadStartMin, setUnloadStartMin] = useState("")
  const [unloadEndHour, setUnloadEndHour] = useState("")
  const [unloadEndMin, setUnloadEndMin] = useState("")
  const [loadEmptyStartHour, setLoadEmptyStartHour] = useState("")
  const [loadEmptyStartMin, setLoadEmptyStartMin] = useState("")
  const [loadEmptyEndHour, setLoadEmptyEndHour] = useState("")
  const [loadEmptyEndMin, setLoadEmptyEndMin] = useState("")
  const [getpassEmployee, setGetpassEmployee] = useState("")
  const [getpassHour, setGetpassHour] = useState("")
  const [getpassMin, setGetpassMin] = useState("")
  const [notifyLBCL, setNotifyLBCL] = useState(true)
  const [showSuccessDialog, setShowSuccessDialog] = useState(false)
  const [successData, setSuccessData] = useState<any>(null)

  useEffect(() => {
    fetchData()
  }, [deliveryId])

  // Load employees when purchase order is loaded
  useEffect(() => {
    console.log("🔄 useEffect triggered for loading employees")
    console.log("📦 Purchase Order state:", purchaseOrder)

    if (purchaseOrder) {
      console.log("✅ Purchase Order exists, checking for OrgUID")
      const orgUID = purchaseOrder?.OrgUID || purchaseOrder?.org_uid || purchaseOrder?.orgUID
      console.log("🔍 Extracted OrgUID:", orgUID)

      if (orgUID) {
        console.log("✅ OrgUID found, loading employees...")
        loadSecurityOfficers()
        loadForkLiftOperators()
      } else {
        console.warn("⚠️ No OrgUID found in purchase order")
        console.log("📋 Available purchase order fields:", Object.keys(purchaseOrder))
      }
    } else {
      console.log("⏳ Purchase order not loaded yet")
    }
  }, [purchaseOrder])

  const loadSecurityOfficers = async () => {
    try {
      setLoadingSecurityOfficers(true)

      // Get the distributor org UID from purchase order
      const distributorOrgUID = purchaseOrder?.OrgUID || purchaseOrder?.org_uid || purchaseOrder?.orgUID
      if (!distributorOrgUID) {
        console.log("⚠️ No distributor org UID found in purchase order")
        console.log("📦 Available fields:", Object.keys(purchaseOrder || {}))
        return
      }

      console.log("🏢 Loading security officers for distributor org:", distributorOrgUID)

      // Get roles for the distributor organization
      const orgRoles = await roleService.getRolesByOrg(distributorOrgUID, false)
      console.log("🔍 Roles for distributor org:", orgRoles)

      let roles: any[] = []
      if (orgRoles && orgRoles.length > 0) {
        roles = orgRoles
      } else {
        // If no roles found for org, load all roles
        console.log("⚠️ No roles found for distributor org, loading all roles")
        const allRolesResult = await roleService.getRoles({
          pageNumber: 1,
          pageSize: 1000,
          isCountRequired: false
        })

        console.log("📊 All roles result:", allRolesResult)

        // Extract roles from pagedData or items array
        roles = allRolesResult.pagedData || allRolesResult.data?.items || allRolesResult.data || []
        console.log("📋 Extracted roles array:", roles)
        console.log("📋 Number of roles:", roles.length)
      }

      // Find the SECURITYOFFICER role - check multiple fields
      const securityRole = roles.find((r: any) =>
        r.UID === 'SECURITYOFFICER' ||
        r.uid === 'SECURITYOFFICER' ||
        r.Code === 'SECURITYOFFICER' ||
        r.code === 'SECURITYOFFICER' ||
        r.RoleNameEn?.toLowerCase().includes('security') ||
        r.roleNameEn?.toLowerCase().includes('security')
      )

      if (securityRole) {
        const roleUID = securityRole.UID || securityRole.uid
        console.log("🔑 Security Officer role UID:", roleUID)

        // Get employees with SECURITYOFFICER role for the distributor org
        console.log(`📋 Loading employees for org: ${distributorOrgUID}, role: ${roleUID}`)
        const employees = await employeeService.getEmployeesSelectionItemByRoleUID(
          distributorOrgUID,
          roleUID
        )
        console.log("👥 Security officers:", employees)

        if (employees && employees.length > 0) {
          const mappedEmployees = employees.map((emp: any) => ({
            UID: emp.UID || emp.Value,
            Name: emp.Label || emp.Name || `[${emp.Code}] ${emp.Name}`,
            Code: emp.Code
          }))

          console.log("✅ Mapped security officers:", mappedEmployees)
          setSecurityOfficers(mappedEmployees)

          // Auto-select the first officer
          if (mappedEmployees.length > 0) {
            setSecurityOfficer(mappedEmployees[0].UID)
          }
        } else {
          console.log("⚠️ No security officers found")
        }
      } else {
        console.log("⚠️ SECURITYOFFICER role not found")
      }
    } catch (error) {
      console.error("❌ Error loading security officers:", error)
    } finally {
      setLoadingSecurityOfficers(false)
    }
  }

  const loadForkLiftOperators = async () => {
    try {
      setLoadingForkLiftOperators(true)

      // Get the distributor org UID from purchase order
      const distributorOrgUID = purchaseOrder?.OrgUID || purchaseOrder?.org_uid || purchaseOrder?.orgUID
      if (!distributorOrgUID) {
        console.log("⚠️ No distributor org UID found in purchase order")
        return
      }

      console.log("🏢 Loading fork lift operators for distributor org:", distributorOrgUID)

      // Get roles for the distributor organization
      const orgRoles = await roleService.getRolesByOrg(distributorOrgUID, false)
      console.log("🔍 Roles for distributor org:", orgRoles)

      let roles: any[] = []
      if (orgRoles && orgRoles.length > 0) {
        roles = orgRoles
      } else {
        // If no roles found for org, load all roles
        console.log("⚠️ No roles found for distributor org, loading all roles")
        const allRolesResult = await roleService.getRoles({
          pageNumber: 1,
          pageSize: 1000,
          isCountRequired: false
        })

        // Extract roles from pagedData or items array
        roles = allRolesResult.pagedData || allRolesResult.data?.items || allRolesResult.data || []
        console.log("📋 Extracted roles array:", roles)
      }

      // Find the OPERATOR role - check multiple fields
      const operatorRole = roles.find((r: any) =>
        r.UID === 'OPERATOR' ||
        r.uid === 'OPERATOR' ||
        r.Code === 'OPERATOR' ||
        r.code === 'OPERATOR' ||
        r.RoleNameEn?.toLowerCase().includes('operator') ||
        r.roleNameEn?.toLowerCase().includes('operator')
      )

      if (operatorRole) {
        const roleUID = operatorRole.UID || operatorRole.uid
        console.log("🔑 Operator role UID:", roleUID)

        // Get employees with OPERATOR role for the distributor org
        console.log(`📋 Loading employees for org: ${distributorOrgUID}, role: ${roleUID}`)
        const employees = await employeeService.getEmployeesSelectionItemByRoleUID(
          distributorOrgUID,
          roleUID
        )
        console.log("👥 Fork lift operators:", employees)

        if (employees && employees.length > 0) {
          const mappedEmployees = employees.map((emp: any) => ({
            UID: emp.UID || emp.Value,
            Name: emp.Label || emp.Name || `[${emp.Code}] ${emp.Name}`,
            Code: emp.Code
          }))

          console.log("✅ Mapped fork lift operators:", mappedEmployees)
          setForkLiftOperators(mappedEmployees)

          // Auto-select the first operator
          if (mappedEmployees.length > 0) {
            setForkLiftOperator(mappedEmployees[0].UID)
          }
        } else {
          console.log("⚠️ No fork lift operators found")
        }
      } else {
        console.log("⚠️ OPERATOR role not found")
      }
    } catch (error) {
      console.error("❌ Error loading fork lift operators:", error)
    } finally {
      setLoadingForkLiftOperators(false)
    }
  }

  const fetchData = async () => {
    try {
      setLoading(true)
      setError("")

      // Fetch purchase order, delivery loading tracking, and stock receiving tracking in parallel
      const [poResponse, dlResponse, srResponse] = await Promise.all([
        purchaseOrderService.getPurchaseOrderMasterByUID(deliveryId),
        deliveryLoadingService.getByPurchaseOrderUID(deliveryId),
        stockReceivingService.getByPurchaseOrderUID(deliveryId)
      ])

      console.log("🔍 Purchase Order Response:", poResponse)
      console.log("🔍 Delivery Loading Response:", dlResponse)
      console.log("🔍 Stock Receiving Response:", srResponse)

      if (poResponse.success && poResponse.data) {
        console.log("📦 Purchase Order Data:", poResponse.data)
        console.log("📦 PO Keys:", Object.keys(poResponse.data))

        // Extract the header from nested structure
        const poData = poResponse.data
        const header = poData.PurchaseOrderHeader || poData
        console.log("📦 Extracted Header:", header)
        console.log("📦 Header Keys:", Object.keys(header))

        setPurchaseOrder(header)

        // Extract order lines for delivery note dialog
        const lines = poData.PurchaseOrderLines || poData.purchaseOrderLines || poData.Lines || poData.lines || []
        setOrderLines(lines)

        // Log org UID for debugging
        console.log("🏢 Purchase Order OrgUID:", header.OrgUID || header.org_uid || header.orgUID)
      } else {
        setError("Failed to load purchase order")
      }

      // Delivery loading might not exist yet
      if (dlResponse) {
        console.log("🚚 Delivery Loading Data:", dlResponse)
        setDeliveryLoading(dlResponse)
      }

      // Load saved stock receiving data and populate form fields
      if (srResponse) {
        console.log("📋 Stock Receiving Data:", srResponse)

        // Populate form fields from saved data
        setSecurityOfficer(srResponse.ReceiverEmployeeCode || "")
        setForkLiftOperator(srResponse.ForkLiftOperatorUID || "")
        setGetpassEmployee(srResponse.GetpassEmployeeUID || "")

        // Parse and set time fields
        if (srResponse.ArrivalTime) {
          const arrivalDate = new Date(srResponse.ArrivalTime)
          setArrivalHour(arrivalDate.getHours().toString())
          setArrivalMin(arrivalDate.getMinutes().toString())
        }

        if (srResponse.UnloadingStartTime) {
          const unloadStart = new Date(srResponse.UnloadingStartTime)
          setUnloadStartHour(unloadStart.getHours().toString())
          setUnloadStartMin(unloadStart.getMinutes().toString())
        }

        if (srResponse.UnloadingEndTime) {
          const unloadEnd = new Date(srResponse.UnloadingEndTime)
          setUnloadEndHour(unloadEnd.getHours().toString())
          setUnloadEndMin(unloadEnd.getMinutes().toString())
        }

        if (srResponse.LoadEmptyStockTime) {
          const loadEmpty = new Date(srResponse.LoadEmptyStockTime)
          setLoadEmptyStartHour(loadEmpty.getHours().toString())
          setLoadEmptyStartMin(loadEmpty.getMinutes().toString())
        }

        if (srResponse.GetpassTime) {
          const getpass = new Date(srResponse.GetpassTime)
          setGetpassHour(getpass.getHours().toString())
          setGetpassMin(getpass.getMinutes().toString())
        }
      }
    } catch (error) {
      console.error("Error fetching data:", error)
      setError("Failed to load data")
    } finally {
      setLoading(false)
    }
  }

  const formatDate = (dateString: string) => {
    if (!dateString) return "N/A"
    const date = new Date(dateString)
    return date.toLocaleDateString('en-GB', {
      day: '2-digit',
      month: 'short',
      year: 'numeric'
    }).toUpperCase()
  }

  const formatTime = (dateString: string) => {
    if (!dateString) return "N/A"
    const date = new Date(dateString)
    return date.toLocaleTimeString('en-GB', {
      hour: '2-digit',
      minute: '2-digit',
      hour12: false
    }) + " HRS"
  }

  const handleSubmit = async () => {
    try {
      const now = new Date()
      const todayDate = now.toISOString().split('T')[0]

      const arrivalTime = arrivalHour && arrivalMin
        ? `${todayDate}T${arrivalHour.padStart(2, '0')}:${arrivalMin.padStart(2, '0')}:00`
        : null

      const unloadingStartTime = unloadStartHour && unloadStartMin
        ? `${todayDate}T${unloadStartHour.padStart(2, '0')}:${unloadStartMin.padStart(2, '0')}:00`
        : null

      const unloadingEndTime = unloadEndHour && unloadEndMin
        ? `${todayDate}T${unloadEndHour.padStart(2, '0')}:${unloadEndMin.padStart(2, '0')}:00`
        : null

      const loadEmptyStockTime = loadEmptyStartHour && loadEmptyStartMin
        ? `${todayDate}T${loadEmptyStartHour.padStart(2, '0')}:${loadEmptyStartMin.padStart(2, '0')}:00`
        : null

      const getpassTime = getpassHour && getpassMin
        ? `${todayDate}T${getpassHour.padStart(2, '0')}:${getpassMin.padStart(2, '0')}:00`
        : null

      const stockReceivingData = {
        PurchaseOrderUID: deliveryId,
        ReceiverName: null,
        ReceiverEmployeeCode: securityOfficer || null,
        ForkLiftOperatorUID: forkLiftOperator || null,
        LoadEmptyStockEmployeeUID: forkLiftOperator || null, // Using same fork lift operator for loading empties
        GetpassEmployeeUID: getpassEmployee || null,
        ArrivalTime: arrivalTime,
        UnloadingStartTime: unloadingStartTime,
        UnloadingEndTime: unloadingEndTime,
        LoadEmptyStockTime: loadEmptyStockTime,
        GetpassTime: getpassTime,
        PhysicalCountStartTime: null,
        PhysicalCountEndTime: null,
        ReceiverSignature: null,
        Notes: null,
        IsActive: true
      }

      console.log("💾 Saving stock receiving data:", stockReceivingData)

      await stockReceivingService.saveStockReceivingTracking(stockReceivingData)

      // Calculate completion time
      let completionTime = "N/A"
      if (unloadingStartTime && unloadingEndTime) {
        const start = new Date(unloadingStartTime)
        const end = new Date(unloadingEndTime)
        const diffMs = end.getTime() - start.getTime()
        const diffMins = Math.round(diffMs / 60000)
        completionTime = `${diffMins} Min`
      }

      // Format times for display
      const formatTime = (timeStr: string | null) => {
        if (!timeStr) return "N/A"
        const date = new Date(timeStr)
        return date.toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit', hour12: true })
      }

      // Set success data and show dialog
      setSuccessData({
        completionTime,
        date: new Date().toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' }),
        startTime: formatTime(unloadingStartTime),
        endTime: formatTime(unloadingEndTime)
      })
      setShowSuccessDialog(true)
    } catch (error) {
      console.error("Error saving stock receiving:", error)
      alert("Failed to save stock receiving activity log")
    }
  }

  const toggleSection = (section: number) => {
    setExpandedSections((prev) => ({ ...prev, [section]: !prev[section] }))
  }

  if (loading) {
    return (
      <div className="min-h-screen bg-white flex items-center justify-center">
        <div className="text-center">
          <RefreshCw className="w-8 h-8 text-[#A08B5C] animate-spin mx-auto mb-2" />
          <p className="text-gray-600">Loading purchase order...</p>
        </div>
      </div>
    )
  }

  if (error || !purchaseOrder) {
    return (
      <div className="min-h-screen bg-white flex items-center justify-center">
        <div className="bg-red-50 border border-red-200 rounded-lg p-4 text-center max-w-md">
          <p className="text-red-700">{error || "Purchase order not found"}</p>
          <button
            onClick={fetchData}
            className="mt-2 text-sm text-red-600 hover:text-red-800 underline"
          >
            Try again
          </button>
        </div>
      </div>
    )
  }

  // Debug: Log the purchase order data when rendering
  console.log("🎨 Rendering with purchaseOrder:", purchaseOrder)
  console.log("🎨 Purchase Order Keys:", purchaseOrder ? Object.keys(purchaseOrder) : 'no PO')
  console.log("🎨 order_number:", purchaseOrder?.order_number)
  console.log("🎨 OrderNumber:", purchaseOrder?.OrderNumber)
  console.log("🎨 order_date:", purchaseOrder?.order_date)

  return (
    <div className="min-h-screen bg-white">
      {/* Header */}
      {/* <header className="bg-white border-b border-gray-200 px-4 py-4 flex items-center justify-between sticky top-0 z-10">
        <h1 className="text-base sm:text-lg md:text-xl font-bold text-center flex-1 px-2">
          Agent Stock Receiving Activity Log Report
        </h1>
        <Button
          variant="outline"
          className="border-[#A08B5C] text-[#A08B5C] bg-transparent"
          onClick={() => router.back()}
        >
          Back
        </Button>
      </header> */}

      {/* Info Section */}
      <div className="bg-gray-50 p-4 border-b border-gray-200">
        <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-6 gap-4 text-sm">
          <div>
            <div className="text-gray-600 mb-1">Agent Stock Receiving No</div>
            <div className="font-bold">ASR{purchaseOrder.order_number || purchaseOrder.OrderNumber || purchaseOrder.orderNumber || deliveryId}</div>
          </div>
          <div>
            <div className="text-gray-600 mb-1">SAP Delivery Note No</div>
            <div className="font-bold">{deliveryLoading?.DeliveryNoteNumber || deliveryLoading?.deliveryNoteNumber || 'N/A'}</div>
          </div>
          <div>
            <div className="text-gray-600 mb-1">Distributor</div>
            <div className="font-bold">{deliveryLoading?.org_name || deliveryLoading?.OrgName || deliveryLoading?.orgName || purchaseOrder.org_name || purchaseOrder.OrgName || purchaseOrder.orgName || 'N/A'}</div>
          </div>
          <div>
            <div className="text-gray-600 mb-1">Prime Mover</div>
            <div className="font-bold">{deliveryLoading?.VehicleUID || deliveryLoading?.vehicleUID || 'N/A'}</div>
          </div>
          <div>
            <div className="text-gray-600 mb-1">Date</div>
            <div className="font-bold">{formatDate(deliveryLoading?.order_date || purchaseOrder.order_date || purchaseOrder.OrderDate || purchaseOrder.orderDate || '')}</div>
          </div>
          <div>
            <div className="text-gray-600 mb-1">LBCL Departure Time</div>
            <div className="font-bold">{formatTime(deliveryLoading?.DepartureTime || deliveryLoading?.departureTime || '')}</div>
          </div>
        </div>
      </div>

      {/* Activity Steps */}
      <div className="p-4 grid grid-cols-1 lg:grid-cols-2 gap-4">
        {/* Left Column */}
        <div className="space-y-4">
          {/* Step 1: View Delivery Note */}
          <div className="bg-white border border-gray-200 rounded-lg p-4 shadow-sm">
            <button onClick={() => setShowDeliveryNote(true)} className="flex items-center justify-between w-full">
              <div className="flex items-center gap-3">
                <div className="w-10 h-10 bg-[#A08B5C] text-white rounded-full flex items-center justify-center font-bold text-sm">1</div>
                <span className="font-semibold text-gray-800">View Delivery Note</span>
              </div>
              <div className="flex items-center gap-2">
                <span className="text-red-600 text-sm font-medium">PDF</span>
                <ChevronRight className="w-5 h-5 text-gray-500" />
              </div>
            </button>
          </div>

          {/* Step 2: Gate Entry */}
          <div className="bg-white border border-gray-200 rounded-lg p-4 shadow-sm">
            <button onClick={() => toggleSection(2)} className="flex items-center justify-between w-full mb-4">
              <div className="flex items-center gap-3">
                <div className="w-10 h-10 bg-[#A08B5C] text-white rounded-full flex items-center justify-center font-bold text-sm">2</div>
                <span className="font-semibold text-gray-800">Gate Entry</span>
              </div>
              {expandedSections[2] ? <ChevronDown className="w-5 h-5 text-gray-500" /> : <ChevronRight className="w-5 h-5 text-gray-500" />}
            </button>

            {expandedSections[2] && (
              <div className="space-y-4 pl-13 bg-gray-50 p-4 rounded-md">
                <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                  <div>
                    <label className="text-sm text-gray-600 mb-2 block">Security Officer</label>
                    <Select
                      value={securityOfficer}
                      onValueChange={setSecurityOfficer}
                      disabled={true}
                    >
                      <SelectTrigger className="bg-white">
                        <SelectValue placeholder={loadingSecurityOfficers ? "Loading security officers..." : "Select security officer"} />
                      </SelectTrigger>
                      <SelectContent>
                        {securityOfficers.map((officer) => (
                          <SelectItem key={officer.UID} value={officer.UID}>
                            {officer.Name}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                  </div>
                  <div>
                    <label className="text-sm text-gray-600 mb-2 block">Prime Mover Arrival</label>
                    <div className="flex gap-2">
                      <Input
                        type="number"
                        placeholder="15"
                        className="w-20 bg-white"
                        value={arrivalHour}
                        onChange={(e) => setArrivalHour(e.target.value)}
                        disabled
                      />
                      <span className="text-gray-500 self-center text-sm">HH</span>
                      <Input
                        type="number"
                        placeholder="00"
                        className="w-20 bg-white"
                        value={arrivalMin}
                        onChange={(e) => setArrivalMin(e.target.value)}
                        disabled
                      />
                      <span className="text-gray-500 self-center text-sm">Min</span>
                    </div>
                  </div>
                </div>
                <div className="flex items-center gap-2">
                  <Checkbox
                    id="notify-lbcl"
                    checked={notifyLBCL}
                    onCheckedChange={(checked) => setNotifyLBCL(checked as boolean)}
                    disabled
                  />
                  <label htmlFor="notify-lbcl" className="text-sm text-gray-700">
                    Notify LBCL Logistics
                  </label>
                </div>
              </div>
            )}
          </div>

          {/* Step 3: Physical Count */}
          <div className="bg-white border border-gray-200 rounded-lg p-4 shadow-sm">
            <button
              onClick={() => router.push(`/lbcl/stock-receiving-history/${deliveryId}`)}
              className="flex items-center justify-between w-full"
            >
              <div className="flex items-center gap-3">
                <div className="w-10 h-10 bg-[#A08B5C] text-white rounded-full flex items-center justify-center font-bold text-sm">3</div>
                <span className="font-semibold text-gray-800">Physical Count & Perform Stock Receiving</span>
              </div>
              <ChevronRight className="w-5 h-5 text-gray-500" />
            </button>
          </div>
        </div>

        {/* Right Column */}
        <div className="space-y-4">
          {/* Step 4: Unloading */}
          <div className="bg-white border border-gray-200 rounded-lg p-4 shadow-sm">
            <button onClick={() => toggleSection(4)} className="flex items-center justify-between w-full mb-4">
              <div className="flex items-center gap-3">
                <div className="w-10 h-10 bg-[#A08B5C] text-white rounded-full flex items-center justify-center font-bold text-sm">4</div>
                <span className="font-semibold text-gray-800">Unloading</span>
              </div>
              {expandedSections[4] ? <ChevronDown className="w-5 h-5 text-gray-500" /> : <ChevronRight className="w-5 h-5 text-gray-500" />}
            </button>

            {expandedSections[4] && (
              <div className="space-y-4 pl-13 bg-gray-50 p-4 rounded-md">
                <div>
                  <label className="text-sm text-gray-600 mb-2 block">Agent Fork Lift Operator</label>
                  <Select
                    value={forkLiftOperator}
                    onValueChange={setForkLiftOperator}
                    disabled={true}
                  >
                    <SelectTrigger className="bg-white">
                      <SelectValue placeholder={loadingForkLiftOperators ? "Loading operators..." : "Select fork lift operator"} />
                    </SelectTrigger>
                    <SelectContent>
                      {forkLiftOperators.map((operator) => (
                        <SelectItem key={operator.UID} value={operator.UID}>
                          {operator.Name}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </div>
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <label className="text-sm text-gray-600 mb-2 block">Unload Start Time</label>
                    <div className="flex gap-2">
                      <Input
                        type="number"
                        placeholder="16"
                        className="w-16 bg-white"
                        value={unloadStartHour}
                        onChange={(e) => setUnloadStartHour(e.target.value)}
                        disabled
                      />
                      <span className="text-gray-500 self-center text-sm">HH</span>
                      <Input
                        type="number"
                        placeholder="02"
                        className="w-16 bg-white"
                        value={unloadStartMin}
                        onChange={(e) => setUnloadStartMin(e.target.value)}
                        disabled
                      />
                      <span className="text-gray-500 self-center text-sm">Min</span>
                    </div>
                  </div>
                  <div>
                    <label className="text-sm text-gray-600 mb-2 block">Unload End Time</label>
                    <div className="flex gap-2">
                      <Input
                        type="number"
                        placeholder="16"
                        className="w-16 bg-white"
                        value={unloadEndHour}
                        onChange={(e) => setUnloadEndHour(e.target.value)}
                        disabled
                      />
                      <span className="text-gray-500 self-center text-sm">HH</span>
                      <Input
                        type="number"
                        placeholder="58"
                        className="w-16 bg-white"
                        value={unloadEndMin}
                        onChange={(e) => setUnloadEndMin(e.target.value)}
                        disabled
                      />
                      <span className="text-gray-500 self-center text-sm">Min</span>
                    </div>
                  </div>
                </div>
              </div>
            )}
          </div>

          {/* Step 5: Load Empty Stock */}
          <div className="bg-white border border-gray-200 rounded-lg p-4 shadow-sm">
            <button className="flex items-center justify-between w-full mb-4">
              <div className="flex items-center gap-3">
                <div className="w-10 h-10 bg-[#A08B5C] text-white rounded-full flex items-center justify-center font-bold text-sm">5</div>
                <span className="font-semibold text-gray-800">Load Empty Stock</span>
              </div>
              <ChevronRight className="w-5 h-5 text-gray-500" />
            </button>

            <div className="space-y-4 pl-13 bg-gray-50 p-4 rounded-md">
              <div>
                <label className="text-sm text-gray-600 mb-2 block">Agent Fork Lift Operator</label>
                <Select
                  value={forkLiftOperator}
                  onValueChange={setForkLiftOperator}
                  disabled={true}
                >
                  <SelectTrigger className="bg-white">
                    <SelectValue placeholder={loadingForkLiftOperators ? "Loading operators..." : "Select fork lift operator"} />
                  </SelectTrigger>
                  <SelectContent>
                    {forkLiftOperators.map((operator) => (
                      <SelectItem key={operator.UID} value={operator.UID}>
                        {operator.Name}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="text-sm text-gray-600 mb-2 block">Load Start Time</label>
                  <div className="flex gap-2">
                    <Input
                      type="number"
                      placeholder="17"
                      className="w-16 bg-white"
                      value={loadEmptyStartHour}
                      onChange={(e) => setLoadEmptyStartHour(e.target.value)}
                      min="0"
                      max="23"
                      disabled
                    />
                    <span className="text-gray-500 self-center text-sm">HH</span>
                    <Input
                      type="number"
                      placeholder="25"
                      className="w-16 bg-white"
                      value={loadEmptyStartMin}
                      onChange={(e) => setLoadEmptyStartMin(e.target.value)}
                      min="0"
                      max="59"
                      disabled
                    />
                    <span className="text-gray-500 self-center text-sm">Min</span>
                  </div>
                </div>
                <div>
                  <label className="text-sm text-gray-600 mb-2 block">Load End Time</label>
                  <div className="flex gap-2">
                    <Input
                      type="number"
                      placeholder="18"
                      className="w-16 bg-white"
                      value={loadEmptyEndHour}
                      onChange={(e) => setLoadEmptyEndHour(e.target.value)}
                      min="0"
                      max="23"
                      disabled
                    />
                    <span className="text-gray-500 self-center text-sm">HH</span>
                    <Input
                      type="number"
                      placeholder="08"
                      className="w-16 bg-white"
                      value={loadEmptyEndMin}
                      onChange={(e) => setLoadEmptyEndMin(e.target.value)}
                      min="0"
                      max="59"
                      disabled
                    />
                    <span className="text-gray-500 self-center text-sm">Min</span>
                  </div>
                </div>
              </div>
            </div>
          </div>

          {/* Step 6: Gate Pass */}
          <div className="bg-white border border-gray-200 rounded-lg p-4 shadow-sm">
            <button onClick={() => toggleSection(6)} className="flex items-center justify-between w-full mb-4">
              <div className="flex items-center gap-3">
                <div className="w-10 h-10 bg-[#A08B5C] text-white rounded-full flex items-center justify-center font-bold text-sm">6</div>
                <span className="font-semibold text-gray-800">Gate Pass (Empties)</span>
              </div>
              {expandedSections[6] ? <ChevronDown className="w-5 h-5 text-gray-500" /> : <ChevronRight className="w-5 h-5 text-gray-500" />}
            </button>

            {expandedSections[6] && (
              <div className="space-y-4 pl-13 bg-gray-50 p-4 rounded-md">
                <div>
                  <label className="text-sm text-gray-600 mb-2 block">Getpass Employee (Security Officer)</label>
                  <Select
                    value={getpassEmployee}
                    onValueChange={setGetpassEmployee}
                    disabled={true}
                  >
                    <SelectTrigger className="bg-white">
                      <SelectValue placeholder={loadingSecurityOfficers ? "Loading security officers..." : "Select getpass employee"} />
                    </SelectTrigger>
                    <SelectContent>
                      {securityOfficers.map((officer) => (
                        <SelectItem key={officer.UID} value={officer.UID}>
                          {officer.Name}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </div>
                <div>
                  <label className="text-sm text-gray-600 mb-2 block">Getpass Time (Prime Mover Departure)</label>
                  <div className="flex gap-2">
                    <Input
                      type="number"
                      placeholder="18"
                      className="w-20 bg-white"
                      value={getpassHour}
                      onChange={(e) => setGetpassHour(e.target.value)}
                      min="0"
                      max="23"
                      disabled
                    />
                    <span className="text-gray-500 self-center text-sm">HH</span>
                    <Input
                      type="number"
                      placeholder="14"
                      className="w-20 bg-white"
                      value={getpassMin}
                      onChange={(e) => setGetpassMin(e.target.value)}
                      min="0"
                      max="59"
                      disabled
                    />
                    <span className="text-gray-500 self-center text-sm">Min</span>
                  </div>
                </div>
                <div className="flex items-center gap-2">
                  <Checkbox id="notify-lbcl-2" defaultChecked disabled />
                  <label htmlFor="notify-lbcl-2" className="text-sm text-gray-700">
                    Notify LBCL Logistics
                  </label>
                </div>
              </div>
            )}
          </div>
        </div>
      </div>

      {/* Delivery Note Dialog */}
      <DeliveryNoteDialog
        open={showDeliveryNote}
        onOpenChange={setShowDeliveryNote}
        orderLines={orderLines}
        purchaseOrder={purchaseOrder}
      />

      {/* Success Dialog */}
      {successData && (
        <SuccessDialog
          open={showSuccessDialog}
          onOpenChange={setShowSuccessDialog}
          title="Physical Count & Perform Stock Receiving Successfully Completed"
          message={`Physical Count & Perform Stock Receiving Completed in ${successData.completionTime}`}
          completionTime={successData.completionTime}
          date={successData.date}
          startTime={successData.startTime}
          endTime={successData.endTime}
          showPrintButton={true}
          onDone={() => {
            setShowSuccessDialog(false)
            router.push("/lbcl/stock-receiving")
          }}
        />
      )}
    </div>
  )
}
