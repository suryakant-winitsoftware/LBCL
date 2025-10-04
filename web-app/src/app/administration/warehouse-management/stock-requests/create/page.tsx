'use client'

import { useState, useEffect, useCallback } from 'react'
import { useRouter } from 'next/navigation'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/components/ui/card'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { Textarea } from '@/components/ui/textarea'
import { Skeleton } from '@/components/ui/skeleton'
import { Popover, PopoverContent, PopoverTrigger } from '@/components/ui/popover'
import { Command, CommandEmpty, CommandGroup, CommandInput, CommandItem, CommandList } from '@/components/ui/command'
import { useToast } from '@/components/ui/use-toast'
import { api, apiService } from '@/services/api'
import { organizationService, Organization as OrgType, OrgType as OrgTypeInterface } from '@/services/organizationService'
import { employeeService } from '@/services/admin/employee.service'
import { ArrowLeft, Plus, Trash2, Package, X, Check, ChevronDown, Search, Building2 } from 'lucide-react'
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table'
import {
  initializeOrganizationHierarchy,
  handleOrganizationSelection,
  getFinalSelectedOrganization,
  resetOrganizationHierarchy,
  OrganizationLevel,
} from '@/utils/organizationHierarchyUtils'

interface Organization {
  UID: string
  Name: string
  Code: string
}

interface Warehouse {
  UID: string
  Name: string
  Code: string
  OrgUID: string
}

interface Employee {
  UID: string
  Name: string
  Code: string
}

interface JobPosition {
  UID: string
  Name: string
}

interface SKU {
  UID: string
  Code: string
  Name: string
  UOM: string
  UOM1?: string
  UOM2?: string
  UOM1CNF?: number
  UOM2CNF?: number
}

interface StockRequestLine {
  UID: string
  SKUUID: string
  SKUCode: string
  SKUName: string
  UOM: string
  UOM1?: string
  UOM2?: string
  UOM1CNF?: number
  UOM2CNF?: number
  RequestedQty: number
  RequestedQty1?: number
  RequestedQty2?: number
  ApprovedQty?: number
  ApprovedQty1?: number
  ApprovedQty2?: number
  LineNumber: number
}

interface DropdownOption {
  value: string
  label: string
}

export default function CreateStockRequestPage() {
  const router = useRouter()
  const { toast } = useToast()

  // Loading states
  const [loading, setLoading] = useState({
    organizations: false,
    warehouses: false,
    roles: false,
    employees: false,
    jobPositions: false,
    skus: false,
  })

  // Organization hierarchy states (like route create)
  const [organizations, setOrganizations] = useState<OrgType[]>([])
  const [orgTypes, setOrgTypes] = useState<OrgTypeInterface[]>([])
  const [orgLevels, setOrgLevels] = useState<OrganizationLevel[]>([])
  const [selectedOrgs, setSelectedOrgs] = useState<string[]>([])

  // Separate hierarchy for source and target
  const [sourceOrgLevels, setSourceOrgLevels] = useState<OrganizationLevel[]>([])
  const [sourceSelectedOrgs, setSourceSelectedOrgs] = useState<string[]>([])
  const [targetOrgLevels, setTargetOrgLevels] = useState<OrganizationLevel[]>([])
  const [targetSelectedOrgs, setTargetSelectedOrgs] = useState<string[]>([])

  // Dropdown states
  const [warehouses, setWarehouses] = useState<Warehouse[]>([])
  const [sourceWarehouses, setSourceWarehouses] = useState<Warehouse[]>([])
  const [targetWarehouses, setTargetWarehouses] = useState<Warehouse[]>([])
  const [roles, setRoles] = useState<DropdownOption[]>([])
  const [employees, setEmployees] = useState<DropdownOption[]>([])
  const [jobPositions, setJobPositions] = useState<JobPosition[]>([])
  const [skus, setSKUs] = useState<SKU[]>([])
  const [lines, setLines] = useState<StockRequestLine[]>([])

  // Popover states
  const [rolePopoverOpen, setRolePopoverOpen] = useState(false)
  const [employeePopoverOpen, setEmployeePopoverOpen] = useState(false)

  const [formData, setFormData] = useState({
    CompanyUID: '',
    SourceOrgUID: '',
    SourceWHUID: '',
    TargetOrgUID: '',
    TargetWHUID: '',
    Code: '',
    RequestType: '',
    RoleUID: '',
    RequestByEmpUID: '',
    JobPositionUID: '',
    RequiredByDate: new Date().toISOString().split('T')[0],
    Status: 'Pending',
    Remarks: '',
    StockType: 'Saleable',
    RouteUID: '',
    OrgUID: '',
    WareHouseUID: '',
    YearMonth: parseInt(new Date().toISOString().slice(0, 7).replace('-', '')),
  })

  useEffect(() => {
    loadOrganizationsAndTypes()
    loadRoles()
    loadJobPositions()
    loadSKUs()
  }, [])

  useEffect(() => {
    if (formData.SourceOrgUID) {
      loadWarehouses(formData.SourceOrgUID)
      setSourceWarehouses(warehouses.filter(w => w.OrgUID === formData.SourceOrgUID))
    } else {
      setSourceWarehouses([])
    }
  }, [formData.SourceOrgUID])

  useEffect(() => {
    if (formData.TargetOrgUID) {
      loadWarehouses(formData.TargetOrgUID)
      loadEmployees(formData.TargetOrgUID)
      setTargetWarehouses(warehouses.filter(w => w.OrgUID === formData.TargetOrgUID))
    } else {
      setTargetWarehouses([])
    }
  }, [formData.TargetOrgUID])

  // Load organizations and types (like route create)
  const loadOrganizationsAndTypes = async () => {
    setLoading((prev) => ({ ...prev, organizations: true }))
    try {
      const [typesResult, orgsResult] = await Promise.all([
        organizationService.getOrganizationTypes(),
        organizationService.getOrganizations(1, 10000),
      ])

      const filteredOrganizations = orgsResult.data.filter((org) => org.ShowInTemplate === true)
      const filteredOrgTypes = typesResult.filter((type) => type.ShowInTemplate !== false)

      setOrgTypes(filteredOrgTypes)
      setOrganizations(filteredOrganizations)

      // Initialize hierarchy for both source and target
      const initialLevels = initializeOrganizationHierarchy(filteredOrganizations, filteredOrgTypes)
      setSourceOrgLevels(initialLevels)
      setTargetOrgLevels(initialLevels)
    } catch (error) {
      console.error('Error fetching organization data:', error)
      toast({
        title: 'Warning',
        description: 'Could not load organization hierarchy',
        variant: 'default',
      })
    } finally {
      setLoading((prev) => ({ ...prev, organizations: false }))
    }
  }

  const loadWarehouses = useCallback(async (orgUID: string) => {
    setLoading((prev) => ({ ...prev, warehouses: true }))
    try {
      const data = await apiService.post(`/Dropdown/GetWareHouseTypeDropDown?parentUID=${orgUID}`)
      if (data.IsSuccess && data.Data) {
        const warehousesData = data.Data.map((wh: any) => ({
          UID: wh.Value || wh.UID,
          Name: wh.Label || wh.Name,
          Code: wh.Code || '',
          OrgUID: orgUID
        }))
        setWarehouses(prev => [...prev.filter(w => w.OrgUID !== orgUID), ...warehousesData])
      }
    } catch (error) {
      console.error('Error loading warehouses:', error)
    } finally {
      setLoading((prev) => ({ ...prev, warehouses: false }))
    }
  }, [])

  const loadRoles = useCallback(async () => {
    setLoading((prev) => ({ ...prev, roles: true }))
    try {
      const data = await apiService.post('/Role/SelectAllRoles', {
        pageNumber: 0,
        pageSize: 100,
        sortCriterias: [],
        filterCriterias: [],
      })
      if (data.IsSuccess && data.Data) {
        const roleData = data.Data.PagedData || data.Data || []
        setRoles(roleData.map((role: any) => ({
          value: role.UID,
          label: role.Name,
        })))
      }
    } catch (error) {
      console.error('Error loading roles:', error)
    } finally {
      setLoading((prev) => ({ ...prev, roles: false }))
    }
  }, [])

  const loadEmployees = useCallback(
    async (orgUID: string, roleUID?: string) => {
      setLoading((prev) => ({ ...prev, employees: true }))
      try {
        let employees = []

        if (roleUID) {
          console.log('🔍 Loading employees for org + role:', orgUID, roleUID)
          try {
            const orgRoleEmployees = await employeeService.getEmployeesSelectionItemByRoleUID(orgUID, roleUID)
            if (orgRoleEmployees && orgRoleEmployees.length > 0) {
              employees = orgRoleEmployees.map((item: any) => ({
                value: item.UID || item.Value || item.uid,
                label: item.Label || item.Text || `[${item.Code || item.code}] ${item.Name || item.name}`,
              }))
              console.log(`✅ Found ${employees.length} employees for org '${orgUID}' + role '${roleUID}'`)
            } else {
              const roleBasedEmployees = await employeeService.getReportsToEmployeesByRoleUID(roleUID)
              if (roleBasedEmployees && roleBasedEmployees.length > 0) {
                const orgData = await api.dropdown.getEmployee(orgUID, false)
                const orgEmployeeUIDs = new Set()
                if (orgData.IsSuccess && orgData.Data) {
                  orgData.Data.forEach((emp: any) => {
                    orgEmployeeUIDs.add(emp.UID)
                  })
                }
                const filteredRoleEmployees = roleBasedEmployees.filter((emp: any) =>
                  orgEmployeeUIDs.has(emp.UID || emp.uid)
                )
                employees = filteredRoleEmployees.map((emp: any) => ({
                  value: emp.UID || emp.uid,
                  label: `[${emp.Code || emp.code}] ${emp.Name || emp.name}`,
                }))
                console.log(`✅ Found ${employees.length} role-based employees filtered for org '${orgUID}'`)
              }
            }
          } catch (roleError) {
            console.error('Role-based employee loading failed:', roleError)
          }
        }

        if (employees.length === 0) {
          console.log('📄 Loading all employees for organization:', orgUID)
          const data = await api.dropdown.getEmployee(orgUID, false)
          if (data.IsSuccess && data.Data) {
            employees = data.Data.map((emp: any) => ({
              value: emp.UID,
              label: emp.Label,
            }))
            console.log(`📊 Loaded ${employees.length} org-based employees`)
          }
        }

        setEmployees(employees)
      } catch (error) {
        console.error('Error loading employees:', error)
      } finally {
        setLoading((prev) => ({ ...prev, employees: false }))
      }
    },
    []
  )

  const loadJobPositions = async () => {
    try {
      const response = await api.jobPosition.selectAll({
        pageNumber: 1,
        pageSize: 1000,
        sortCriterias: [],
        filterCriterias: [],
        isCountRequired: false
      })
      if (response.IsSuccess && response.Data) {
        const items = response.Data.PagedData || response.Data || []
        setJobPositions(items.map((jp: any) => ({
          UID: jp.UID,
          Name: jp.Name
        })))
      }
    } catch (error) {
      console.error('Failed to load job positions:', error)
    }
  }

  const loadSKUs = async () => {
    try {
      const response = await apiService.post('/SKU/SelectAllSKUDetails', {
        pageNumber: 1,
        pageSize: 10000,
        sortCriterias: [],
        filterCriterias: [],
        isCountRequired: false
      })
      if (response.IsSuccess && response.Data) {
        const items = response.Data.PagedData || response.Data || []
        setSKUs(items.map((sku: any) => ({
          UID: sku.UID,
          Code: sku.Code,
          Name: sku.Name,
          UOM: sku.UOM || 'EA',
          UOM1: sku.UOM1,
          UOM2: sku.UOM2,
          UOM1CNF: sku.UOM1CNF,
          UOM2CNF: sku.UOM2CNF
        })))
      }
    } catch (error) {
      console.error('Failed to load SKUs:', error)
    }
  }

  // Organization hierarchy handlers (like route create)
  const handleSourceOrgSelect = (levelIndex: number, value: string) => {
    if (!value) return
    const { updatedLevels, updatedSelectedOrgs } = handleOrganizationSelection(
      levelIndex,
      value,
      sourceOrgLevels,
      sourceSelectedOrgs,
      organizations,
      orgTypes
    )
    setSourceOrgLevels(updatedLevels)
    setSourceSelectedOrgs(updatedSelectedOrgs)
    const finalOrg = getFinalSelectedOrganization(updatedSelectedOrgs)
    if (finalOrg) {
      setFormData(prev => ({ ...prev, SourceOrgUID: finalOrg }))
    }
  }

  const handleTargetOrgSelect = (levelIndex: number, value: string) => {
    if (!value) return
    const { updatedLevels, updatedSelectedOrgs } = handleOrganizationSelection(
      levelIndex,
      value,
      targetOrgLevels,
      targetSelectedOrgs,
      organizations,
      orgTypes
    )
    setTargetOrgLevels(updatedLevels)
    setTargetSelectedOrgs(updatedSelectedOrgs)
    const finalOrg = getFinalSelectedOrganization(updatedSelectedOrgs)
    if (finalOrg) {
      setFormData(prev => ({ ...prev, TargetOrgUID: finalOrg, OrgUID: finalOrg, WareHouseUID: '' }))
    }
  }

  const handleSourceOrgReset = () => {
    const { resetLevels, resetSelectedOrgs } = resetOrganizationHierarchy(organizations, orgTypes)
    setSourceOrgLevels(resetLevels)
    setSourceSelectedOrgs(resetSelectedOrgs)
    setFormData(prev => ({ ...prev, SourceOrgUID: '', SourceWHUID: '' }))
    setSourceWarehouses([])
  }

  const handleTargetOrgReset = () => {
    const { resetLevels, resetSelectedOrgs } = resetOrganizationHierarchy(organizations, orgTypes)
    setTargetOrgLevels(resetLevels)
    setTargetSelectedOrgs(resetSelectedOrgs)
    setFormData(prev => ({ ...prev, TargetOrgUID: '', TargetWHUID: '', OrgUID: '', WareHouseUID: '' }))
    setTargetWarehouses([])
    setEmployees([])
  }

  // Cascading dependencies (like route create)
  useEffect(() => {
    if (formData.TargetOrgUID && formData.RoleUID) {
      console.log('🎯 Loading role-based employees for:', formData.RoleUID)
      loadEmployees(formData.TargetOrgUID, formData.RoleUID)
    } else if (formData.TargetOrgUID) {
      console.log('📄 Loading all org employees (no role filter)')
      loadEmployees(formData.TargetOrgUID)
    }

    // Clear employee selection when role changes
    if (formData.RequestByEmpUID) {
      console.log('🔄 Clearing employee selection due to role change')
      setFormData(prev => ({ ...prev, RequestByEmpUID: '' }))
    }
  }, [formData.RoleUID, formData.TargetOrgUID, loadEmployees])

  const addLine = () => {
    const newLine: StockRequestLine = {
      UID: `LINE-${Date.now()}-${lines.length}`,
      SKUUID: '',
      SKUCode: '',
      SKUName: '',
      UOM: 'EA',
      RequestedQty: 0,
      LineNumber: lines.length + 1
    }
    setLines([...lines, newLine])
  }

  const removeLine = (index: number) => {
    setLines(lines.filter((_, i) => i !== index))
  }

  const updateLine = (index: number, field: keyof StockRequestLine, value: any) => {
    const updatedLines = [...lines]

    if (field === 'SKUUID') {
      const sku = skus.find(s => s.UID === value)
      if (sku) {
        updatedLines[index] = {
          ...updatedLines[index],
          SKUUID: sku.UID,
          SKUCode: sku.Code,
          SKUName: sku.Name,
          UOM: sku.UOM,
          UOM1: sku.UOM1,
          UOM2: sku.UOM2,
          UOM1CNF: sku.UOM1CNF,
          UOM2CNF: sku.UOM2CNF,
        }
      }
    } else {
      updatedLines[index] = { ...updatedLines[index], [field]: value }
    }

    setLines(updatedLines)
  }

  const handleSubmit = async () => {
    // Validation
    if (!formData.Code) {
      toast({ title: 'Error', description: 'Request code is required', variant: 'destructive' })
      return
    }
    if (!formData.SourceOrgUID || !formData.SourceWHUID) {
      toast({ title: 'Error', description: 'Source organization and warehouse are required', variant: 'destructive' })
      return
    }
    if (!formData.TargetOrgUID || !formData.TargetWHUID) {
      toast({ title: 'Error', description: 'Target organization and warehouse are required', variant: 'destructive' })
      return
    }
    if (!formData.RequestType) {
      toast({ title: 'Error', description: 'Request type is required', variant: 'destructive' })
      return
    }
    if (!formData.RequestByEmpUID) {
      toast({ title: 'Error', description: 'Requesting employee is required', variant: 'destructive' })
      return
    }
    if (!formData.JobPositionUID) {
      toast({ title: 'Error', description: 'Job position is required', variant: 'destructive' })
      return
    }
    if (!formData.StockType) {
      toast({ title: 'Error', description: 'Stock type is required', variant: 'destructive' })
      return
    }
    if (lines.length === 0) {
      toast({ title: 'Error', description: 'At least one line item is required', variant: 'destructive' })
      return
    }

    // Validate all lines have SKU and quantity
    for (const line of lines) {
      if (!line.SKUUID) {
        toast({ title: 'Error', description: `Line ${line.LineNumber}: SKU is required`, variant: 'destructive' })
        return
      }
      if (!line.RequestedQty || line.RequestedQty <= 0) {
        toast({ title: 'Error', description: `Line ${line.LineNumber}: Quantity must be greater than 0`, variant: 'destructive' })
        return
      }
    }

    setLoading(true)
    try {
      const now = new Date().toISOString()
      const requestUID = `WH-${Date.now()}`

      // Prepare request lines
      const requestLines = lines.map((line, index) => ({
        UID: line.UID,
        CompanyUID: formData.CompanyUID,
        WHStockRequestUID: requestUID,
        StockSubType: '',
        SKUUID: line.SKUUID,
        UOM: line.UOM,
        UOM1: line.UOM1 || '',
        UOM2: line.UOM2 || '',
        UOM1CNF: line.UOM1CNF || 0,
        UOM2CNF: line.UOM2CNF || 0,
        RequestedQty: line.RequestedQty,
        RequestedQty1: line.RequestedQty1 || 0,
        RequestedQty2: line.RequestedQty2 || 0,
        ApprovedQty: line.ApprovedQty || 0,
        ApprovedQty1: line.ApprovedQty1 || 0,
        ApprovedQty2: line.ApprovedQty2 || 0,
        CollectedQty: 0,
        CollectedQty1: 0,
        CollectedQty2: 0,
        CPEApprovedQty: 0,
        CPEApprovedQty1: 0,
        CPEApprovedQty2: 0,
        ForwardQty: 0,
        ForwardQty1: 0,
        ForwardQty2: 0,
        WHQty: 0,
        TemplateQty1: 0,
        TemplateQty2: 0,
        SKUCode: line.SKUCode,
        LineNumber: index + 1,
        OrgUID: formData.OrgUID || formData.TargetOrgUID,
        WareHouseUID: formData.WareHouseUID || formData.TargetWHUID,
        YearMonth: formData.YearMonth,
        ActionType: 1,
        CreatedBy: 'ADMIN',
        ModifiedBy: 'ADMIN',
        CreatedTime: now,
        ModifiedTime: now,
        ServerAddTime: now,
        ServerModifiedTime: now,
      }))

      const payload = {
        WHStockRequest: {
          UID: requestUID,
          CompanyUID: formData.CompanyUID,
          SourceOrgUID: formData.SourceOrgUID,
          SourceWHUID: formData.SourceWHUID,
          TargetOrgUID: formData.TargetOrgUID,
          TargetWHUID: formData.TargetWHUID,
          Code: formData.Code,
          RequestType: formData.RequestType,
          RequestByEmpUID: formData.RequestByEmpUID,
          JobPositionUID: formData.JobPositionUID,
          RequiredByDate: formData.RequiredByDate,
          Status: formData.Status,
          Remarks: formData.Remarks,
          StockType: formData.StockType,
          RouteUID: formData.RouteUID,
          OrgUID: formData.OrgUID || formData.TargetOrgUID,
          WareHouseUID: formData.WareHouseUID || formData.TargetWHUID,
          YearMonth: formData.YearMonth,
          ActionType: 1,
          CreatedBy: 'ADMIN',
          ModifiedBy: 'ADMIN',
          CreatedTime: now,
          ModifiedTime: now,
          ServerAddTime: now,
          ServerModifiedTime: now,
        },
        WHStockRequestLines: requestLines,
        WHStockLedgerList: null
      }

      await apiService.post('/WHStock/CUDWHStock', payload)

      toast({
        title: 'Success',
        description: 'Stock request created successfully',
      })

      router.push('/administration/warehouse-management/stock-requests')
    } catch (error: any) {
      toast({
        title: 'Error',
        description: error?.response?.data?.message || 'Failed to create stock request',
        variant: 'destructive'
      })
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="container mx-auto py-6 space-y-6">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-4">
          <Button
            variant="ghost"
            size="icon"
            onClick={() => router.back()}
          >
            <ArrowLeft className="h-4 w-4" />
          </Button>
          <div>
            <h1 className="text-2xl font-bold">Create Stock Request</h1>
            <p className="text-sm text-muted-foreground">Add a new warehouse stock request with line items</p>
          </div>
        </div>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Request Details</CardTitle>
          <CardDescription>Enter the stock request information</CardDescription>
        </CardHeader>
        <CardContent className="space-y-6">
          {/* Basic Information */}
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
            <div className="space-y-2">
              <Label htmlFor="code">Request Code *</Label>
              <Input
                id="code"
                value={formData.Code}
                onChange={(e) => setFormData({ ...formData, Code: e.target.value })}
                placeholder="SR-2025-001"
              />
            </div>

            <div className="space-y-2">
              <Label htmlFor="requestType">Request Type *</Label>
              <Select
                value={formData.RequestType}
                onValueChange={(value) => setFormData({ ...formData, RequestType: value })}
              >
                <SelectTrigger id="requestType">
                  <SelectValue placeholder="Select type" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="StockIn">Stock In</SelectItem>
                  <SelectItem value="Transfer">Transfer</SelectItem>
                  <SelectItem value="Adjustment">Adjustment</SelectItem>
                  <SelectItem value="Return">Return</SelectItem>
                  <SelectItem value="Damage">Damage</SelectItem>
                </SelectContent>
              </Select>
            </div>

            <div className="space-y-2">
              <Label htmlFor="stockType">Stock Type *</Label>
              <Select
                value={formData.StockType}
                onValueChange={(value) => setFormData({ ...formData, StockType: value })}
              >
                <SelectTrigger id="stockType">
                  <SelectValue placeholder="Select stock type" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="Saleable">Saleable</SelectItem>
                  <SelectItem value="FOC">FOC (Free of Charge)</SelectItem>
                  <SelectItem value="Damaged">Damaged</SelectItem>
                  <SelectItem value="Sample">Sample</SelectItem>
                  <SelectItem value="Promotional">Promotional</SelectItem>
                </SelectContent>
              </Select>
            </div>

            <div className="space-y-2">
              <Label htmlFor="status">Status *</Label>
              <Select
                value={formData.Status}
                onValueChange={(value) => setFormData({ ...formData, Status: value })}
              >
                <SelectTrigger id="status">
                  <SelectValue placeholder="Select status" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="Pending">Pending</SelectItem>
                  <SelectItem value="Approved">Approved</SelectItem>
                  <SelectItem value="Rejected">Rejected</SelectItem>
                  <SelectItem value="Completed">Completed</SelectItem>
                </SelectContent>
              </Select>
            </div>

            <div className="space-y-2">
              <Label htmlFor="requiredByDate">Required By Date *</Label>
              <Input
                id="requiredByDate"
                type="date"
                value={formData.RequiredByDate}
                onChange={(e) => setFormData({ ...formData, RequiredByDate: e.target.value })}
              />
            </div>

            <div className="space-y-2">
              <Label htmlFor="yearMonth">Year Month *</Label>
              <Input
                id="yearMonth"
                type="number"
                value={formData.YearMonth}
                onChange={(e) => setFormData({ ...formData, YearMonth: parseInt(e.target.value) })}
                placeholder="202510"
              />
            </div>
          </div>

          {/* Source Information */}
          <div className="space-y-4">
            <h3 className="text-lg font-semibold">Source Information</h3>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label htmlFor="sourceOrg">Source Organization *</Label>
                <Select
                  value={formData.SourceOrgUID}
                  onValueChange={(value) => setFormData({ ...formData, SourceOrgUID: value, SourceWHUID: '' })}
                >
                  <SelectTrigger id="sourceOrg">
                    <SelectValue placeholder="Select organization" />
                  </SelectTrigger>
                  <SelectContent>
                    {organizations.map((org) => (
                      <SelectItem key={org.UID} value={org.UID}>
                        {org.Code} - {org.Name}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>

              <div className="space-y-2">
                <Label htmlFor="sourceWH">Source Warehouse *</Label>
                <Select
                  value={formData.SourceWHUID}
                  onValueChange={(value) => setFormData({ ...formData, SourceWHUID: value })}
                  disabled={!formData.SourceOrgUID}
                >
                  <SelectTrigger id="sourceWH">
                    <SelectValue placeholder="Select warehouse" />
                  </SelectTrigger>
                  <SelectContent>
                    {sourceWarehouses.map((wh) => (
                      <SelectItem key={wh.UID} value={wh.UID}>
                        {wh.Code} - {wh.Name}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
            </div>
          </div>

          {/* Target Information */}
          <div className="space-y-4">
            <h3 className="text-lg font-semibold">Target Information</h3>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label htmlFor="targetOrg">Target Organization *</Label>
                <Select
                  value={formData.TargetOrgUID}
                  onValueChange={(value) => setFormData({ ...formData, TargetOrgUID: value, TargetWHUID: '' })}
                >
                  <SelectTrigger id="targetOrg">
                    <SelectValue placeholder="Select organization" />
                  </SelectTrigger>
                  <SelectContent>
                    {organizations.map((org) => (
                      <SelectItem key={org.UID} value={org.UID}>
                        {org.Code} - {org.Name}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>

              <div className="space-y-2">
                <Label htmlFor="targetWH">Target Warehouse *</Label>
                <Select
                  value={formData.TargetWHUID}
                  onValueChange={(value) => setFormData({ ...formData, TargetWHUID: value })}
                  disabled={!formData.TargetOrgUID}
                >
                  <SelectTrigger id="targetWH">
                    <SelectValue placeholder="Select warehouse" />
                  </SelectTrigger>
                  <SelectContent>
                    {targetWarehouses.map((wh) => (
                      <SelectItem key={wh.UID} value={wh.UID}>
                        {wh.Code} - {wh.Name}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
            </div>
          </div>

          {/* Requester Information */}
          <div className="space-y-4">
            <h3 className="text-lg font-semibold">Requester Information</h3>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label htmlFor="employee">Requesting Employee *</Label>
                <Select
                  value={formData.RequestByEmpUID}
                  onValueChange={(value) => setFormData({ ...formData, RequestByEmpUID: value })}
                >
                  <SelectTrigger id="employee">
                    <SelectValue placeholder="Select employee" />
                  </SelectTrigger>
                  <SelectContent>
                    {employees.map((emp) => (
                      <SelectItem key={emp.UID} value={emp.UID}>
                        {emp.Code} - {emp.Name}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>

              <div className="space-y-2">
                <Label htmlFor="jobPosition">Job Position *</Label>
                <Select
                  value={formData.JobPositionUID}
                  onValueChange={(value) => setFormData({ ...formData, JobPositionUID: value })}
                >
                  <SelectTrigger id="jobPosition">
                    <SelectValue placeholder="Select job position" />
                  </SelectTrigger>
                  <SelectContent>
                    {jobPositions.map((jp) => (
                      <SelectItem key={jp.UID} value={jp.UID}>
                        {jp.Name}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
            </div>
          </div>

          {/* Optional Fields */}
          <div className="space-y-4">
            <h3 className="text-lg font-semibold">Additional Information</h3>
            <div className="grid grid-cols-1 gap-4">
              <div className="space-y-2">
                <Label htmlFor="remarks">Remarks</Label>
                <Textarea
                  id="remarks"
                  value={formData.Remarks}
                  onChange={(e) => setFormData({ ...formData, Remarks: e.target.value })}
                  placeholder="Enter any additional notes or remarks"
                  rows={3}
                />
              </div>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Line Items */}
      <Card>
        <CardHeader>
          <div className="flex items-center justify-between">
            <div>
              <CardTitle>Line Items</CardTitle>
              <CardDescription>Add SKU items to this stock request</CardDescription>
            </div>
            <Button onClick={addLine} size="sm">
              <Plus className="h-4 w-4 mr-2" />
              Add Line
            </Button>
          </div>
        </CardHeader>
        <CardContent>
          {lines.length === 0 ? (
            <div className="text-center py-12 text-muted-foreground">
              <Package className="h-12 w-12 mx-auto mb-4 opacity-50" />
              <p>No line items added yet</p>
              <p className="text-sm">Click "Add Line" to add SKU items</p>
            </div>
          ) : (
            <div className="border rounded-lg overflow-hidden">
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead className="w-12">#</TableHead>
                    <TableHead>SKU *</TableHead>
                    <TableHead>Code</TableHead>
                    <TableHead>UOM</TableHead>
                    <TableHead className="w-32">Requested Qty *</TableHead>
                    <TableHead className="w-32">Approved Qty</TableHead>
                    <TableHead className="w-12"></TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {lines.map((line, index) => (
                    <TableRow key={line.UID}>
                      <TableCell>{index + 1}</TableCell>
                      <TableCell>
                        <Select
                          value={line.SKUUID}
                          onValueChange={(value) => updateLine(index, 'SKUUID', value)}
                        >
                          <SelectTrigger className="w-full">
                            <SelectValue placeholder="Select SKU" />
                          </SelectTrigger>
                          <SelectContent>
                            {skus.map((sku) => (
                              <SelectItem key={sku.UID} value={sku.UID}>
                                {sku.Code} - {sku.Name}
                              </SelectItem>
                            ))}
                          </SelectContent>
                        </Select>
                      </TableCell>
                      <TableCell>{line.SKUCode || '-'}</TableCell>
                      <TableCell>{line.UOM}</TableCell>
                      <TableCell>
                        <Input
                          type="number"
                          min="0"
                          value={line.RequestedQty}
                          onChange={(e) => updateLine(index, 'RequestedQty', parseFloat(e.target.value) || 0)}
                          placeholder="0"
                        />
                      </TableCell>
                      <TableCell>
                        <Input
                          type="number"
                          min="0"
                          value={line.ApprovedQty || 0}
                          onChange={(e) => updateLine(index, 'ApprovedQty', parseFloat(e.target.value) || 0)}
                          placeholder="0"
                        />
                      </TableCell>
                      <TableCell>
                        <Button
                          variant="ghost"
                          size="icon"
                          onClick={() => removeLine(index)}
                          className="text-destructive hover:text-destructive"
                        >
                          <Trash2 className="h-4 w-4" />
                        </Button>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </div>
          )}
        </CardContent>
      </Card>

      {/* Actions */}
      <div className="flex justify-end gap-4">
        <Button
          variant="outline"
          onClick={() => router.back()}
          disabled={loading}
        >
          Cancel
        </Button>
        <Button
          onClick={handleSubmit}
          disabled={loading}
        >
          {loading ? 'Creating...' : 'Create Stock Request'}
        </Button>
      </div>
    </div>
  )
}
