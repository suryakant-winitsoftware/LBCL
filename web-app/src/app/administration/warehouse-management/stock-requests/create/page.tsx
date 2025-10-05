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
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from '@/components/ui/dialog'
import { Checkbox } from '@/components/ui/checkbox'
import { Badge } from '@/components/ui/badge'
import { useToast } from '@/components/ui/use-toast'
import { api, apiService } from '@/services/api'
import { organizationService, Organization as OrgType, OrgType as OrgTypeInterface } from '@/services/organizationService'
import { employeeService } from '@/services/admin/employee.service'
import { ArrowLeft, Plus, Trash2, Package, X, Check, ChevronDown, Search, Building2, Users, Filter } from 'lucide-react'
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table'
import {
  initializeOrganizationHierarchy,
  handleOrganizationSelection,
  getFinalSelectedOrganization,
  resetOrganizationHierarchy,
  OrganizationLevel,
} from '@/utils/organizationHierarchyUtils'
import { uomTypesService, UOMType } from '@/services/sku/uom-types.service'

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
    uom: false,
  })

  // UOM Types from database
  const [uomOptions, setUomOptions] = useState<UOMType[]>([])

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

  // SKU Multi-Select Dialog States
  const [skuDialogOpen, setSkuDialogOpen] = useState(false)
  const [skuSearchQuery, setSkuSearchQuery] = useState('')
  const [selectedSKUs, setSelectedSKUs] = useState<Set<string>>(new Set())

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
    YearMonth: parseInt(new Date().toISOString().slice(2, 7).replace('-', '')), // YYMM format (e.g., 2510 for Oct 2025)
  })

  useEffect(() => {
    loadOrganizationsAndTypes()
    loadWarehouses('') // Load all FRWH warehouses on mount
    loadRoles()
    loadSKUs()
    loadUOMTypes() // Load UOM types from database
  }, [])

  useEffect(() => {
    console.log('🔵 Source Org Changed:', formData.SourceOrgUID)
    console.log('🔵 All Warehouses:', warehouses)

    if (formData.SourceOrgUID) {
      if (warehouses.length === 0) {
        loadWarehouses(formData.SourceOrgUID)
      }
      // Filter warehouses by ParentUID matching the selected organization
      const filtered = warehouses.filter(w => w.OrgUID === formData.SourceOrgUID)
      console.log('🔵 Filtered Source Warehouses (by ParentUID):', filtered)
      setSourceWarehouses(filtered)
    } else {
      setSourceWarehouses([])
    }
  }, [formData.SourceOrgUID, warehouses])

  useEffect(() => {
    console.log('🔵 Target Org Changed:', formData.TargetOrgUID)

    if (formData.TargetOrgUID) {
      if (warehouses.length === 0) {
        loadWarehouses(formData.TargetOrgUID)
      }
      loadEmployees(formData.TargetOrgUID)
      // Filter warehouses by ParentUID matching the selected organization
      const filtered = warehouses.filter(w => w.OrgUID === formData.TargetOrgUID)
      console.log('🔵 Filtered Target Warehouses (by ParentUID):', filtered)
      setTargetWarehouses(filtered)
    } else {
      setTargetWarehouses([])
    }
  }, [formData.TargetOrgUID, warehouses])

  // Load organizations and types (like route create)
  const loadOrganizationsAndTypes = async () => {
    setLoading((prev) => ({ ...prev, organizations: true }))
    try {
      const [typesResult, orgsResult] = await Promise.all([
        organizationService.getOrganizationTypes(),
        organizationService.getOrganizations(1, 10000),
      ])

      // Exclude only warehouse organization types, keep FR (Franchisee) for distributors
      const warehouseOrgTypes = ['FRWH', 'VWH', 'WH', 'Warehouse']
      const filteredOrganizations = orgsResult.data.filter(
        (org) => org.ShowInTemplate === true && !warehouseOrgTypes.includes(org.OrgTypeUID)
      )
      const filteredOrgTypes = typesResult.filter(
        (type) => type.ShowInTemplate !== false && !warehouseOrgTypes.includes(type.UID)
      )

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

  const loadUOMTypes = useCallback(async () => {
    setLoading((prev) => ({ ...prev, uom: true }))
    try {
      const allTypes = await uomTypesService.getAllUOMTypes()
      setUomOptions(allTypes)
      console.log('✅ Loaded UOM Types:', allTypes.length)
    } catch (error) {
      console.error('Failed to load UOM types:', error)
      toast({
        title: 'Warning',
        description: 'Could not load UOM types',
        variant: 'default',
      })
    } finally {
      setLoading((prev) => ({ ...prev, uom: false }))
    }
  }, [toast])

  const loadWarehouses = useCallback(async (orgUID: string) => {
    setLoading((prev) => ({ ...prev, warehouses: true }))
    try {
      // Use the same API as warehouses list page - get all FRWH warehouses
      console.log('🔵 Loading FRWH warehouses...')
      const data = await apiService.get('/Org/GetOrgByOrgTypeUID?OrgTypeUID=FRWH')
      console.log('🔵 Warehouse API Response:', data)

      if (data.IsSuccess && data.Data) {
        const warehousesData = data.Data.map((wh: any) => ({
          UID: wh.UID,
          Name: wh.Name,
          Code: wh.Code,
          OrgUID: wh.ParentUID || ''
        }))
        console.log('🔵 Mapped Warehouses:', warehousesData)
        setWarehouses(warehousesData)
      }
    } catch (error) {
      console.error('❌ Error loading warehouses:', error)
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
      console.log('🔵 Roles API Response:', data)
      if (data.IsSuccess && data.Data) {
        const roleData = data.Data.PagedData || data.Data || []
        const mappedRoles = roleData.map((role: any) => ({
          value: role.UID,
          label: role.Name || role.UID, // Fallback to UID if Name is undefined
        }))
        console.log('🔵 Mapped Roles:', mappedRoles)
        setRoles(mappedRoles)
      }
    } catch (error) {
      console.error('Error loading roles:', error)
    } finally {
      setLoading((prev) => ({ ...prev, roles: false }))
    }
  }, [])

  const loadEmployees = useCallback(
    async (orgUID: string, roleUID?: string) => {
      console.log('🔵 loadEmployees called with:', { orgUID, roleUID })
      setLoading((prev) => ({ ...prev, employees: true }))
      try {
        let employees = []

        if (roleUID) {
          console.log('🔍 Loading employees for org + role:', orgUID, roleUID)
          try {
            const orgRoleEmployees = await employeeService.getEmployeesSelectionItemByRoleUID(orgUID, roleUID)
            console.log('🔵 orgRoleEmployees response:', orgRoleEmployees)

            if (orgRoleEmployees && orgRoleEmployees.length > 0) {
              employees = orgRoleEmployees.map((item: any) => ({
                value: item.UID || item.Value || item.uid,
                label: item.Label || item.Text || `[${item.Code || item.code}] ${item.Name || item.name}`,
              }))
              console.log(`✅ Found ${employees.length} employees for org '${orgUID}' + role '${roleUID}'`)
            } else {
              console.log('🔍 No direct org+role employees, trying role-based fallback')
              const roleBasedEmployees = await employeeService.getReportsToEmployeesByRoleUID(roleUID)
              console.log('🔵 roleBasedEmployees response:', roleBasedEmployees)

              if (roleBasedEmployees && roleBasedEmployees.length > 0) {
                const orgData = await api.dropdown.getEmployee(orgUID, false)
                console.log('🔵 orgData response:', orgData)

                const orgEmployeeUIDs = new Set()
                if (orgData.IsSuccess && orgData.Data) {
                  orgData.Data.forEach((emp: any) => {
                    orgEmployeeUIDs.add(emp.UID)
                  })
                }
                console.log('🔵 orgEmployeeUIDs count:', orgEmployeeUIDs.size)

                const filteredRoleEmployees = roleBasedEmployees.filter((emp: any) =>
                  orgEmployeeUIDs.has(emp.UID || emp.uid)
                )
                console.log('🔵 filteredRoleEmployees count:', filteredRoleEmployees.length)

                employees = filteredRoleEmployees.map((emp: any) => ({
                  value: emp.UID || emp.uid,
                  label: `[${emp.Code || emp.code}] ${emp.Name || emp.name}`,
                }))
                console.log(`✅ Found ${employees.length} role-based employees filtered for org '${orgUID}'`)
              }
            }
          } catch (roleError) {
            console.error('❌ Role-based employee loading failed:', roleError)
          }
        }

        if (employees.length === 0) {
          console.log('📄 Loading all employees for organization:', orgUID)
          const data = await api.dropdown.getEmployee(orgUID, false)
          console.log('🔵 All org employees response:', data)

          if (data.IsSuccess && data.Data) {
            employees = data.Data.map((emp: any) => ({
              value: emp.UID,
              label: emp.Label,
            }))
            console.log(`📊 Loaded ${employees.length} org-based employees`)
          }
        }

        console.log('🔵 Final employees array:', employees)
        setEmployees(employees)
      } catch (error) {
        console.error('❌ Error loading employees:', error)
      } finally {
        setLoading((prev) => ({ ...prev, employees: false }))
      }
    },
    []
  )

  const loadEmployeesByRole = useCallback(
    async (roleUID: string, orgUID?: string) => {
      console.log('🔵 loadEmployeesByRole called with:', { roleUID, orgUID })
      setLoading((prev) => ({ ...prev, employees: true }))
      try {
        let employees = []

        // Use the Role API to get employees by role
        console.log('🔍 Loading employees by role using Role API:', roleUID)
        const response = await api.role.getEmployeesByRoleUID(roleUID)
        console.log('🔵 Role employees API response:', response)

        if (response.IsSuccess && response.Data) {
          let roleEmployees = Array.isArray(response.Data) ? response.Data : []
          console.log('🔵 Total employees with role:', roleEmployees.length)

          // If org is specified, filter by org
          if (orgUID) {
            console.log('🔍 Filtering by organization:', orgUID)
            roleEmployees = roleEmployees.filter((emp: any) => emp.OrgUID === orgUID || emp.orgUID === orgUID)
            console.log('🔵 Employees after org filter:', roleEmployees.length)
          }

          employees = roleEmployees.map((emp: any) => ({
            value: emp.UID || emp.uid || emp.Value,
            label: emp.Label || emp.Text || `[${emp.Code || emp.code}] ${emp.Name || emp.name}`,
          }))
          console.log(`✅ Found ${employees.length} employees with role '${roleUID}'`)
        } else {
          console.log('⚠️ No employees found with this role')
        }

        console.log('🔵 Final employees array:', employees)
        setEmployees(employees)
      } catch (error) {
        console.error('❌ Error loading employees by role:', error)
        setEmployees([])
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

  // Cascading dependencies - load employees by role (use Principal org if no target org selected)
  useEffect(() => {
    console.log('🔵 useEffect triggered - TargetOrgUID:', formData.TargetOrgUID, 'RoleUID:', formData.RoleUID)

    if (formData.RoleUID) {
      // Get org UID - use target org if selected, otherwise use Principal org (first level)
      let orgUID = formData.TargetOrgUID

      if (!orgUID && sourceOrgLevels.length > 0 && sourceOrgLevels[0].organizations.length > 0) {
        // Use the first Principal organization
        orgUID = sourceOrgLevels[0].organizations[0].UID
        console.log('📌 Using default Principal org:', orgUID)
      }

      if (orgUID) {
        console.log('🎯 Loading employees for org + role:', { org: orgUID, role: formData.RoleUID })
        loadEmployees(orgUID, formData.RoleUID)
      } else {
        console.log('⚠️ No org available, clearing employees')
        setEmployees([])
      }
    } else if (formData.TargetOrgUID) {
      console.log('📄 Loading all org employees (no role filter) for org:', formData.TargetOrgUID)
      loadEmployees(formData.TargetOrgUID)
    } else {
      console.log('⚠️ No role selected, clearing employees')
      setEmployees([])
    }
  }, [formData.RoleUID, formData.TargetOrgUID, sourceOrgLevels, loadEmployees])

  const addLine = () => {
    // Use first available UOM or fallback to 'EA'
    const defaultUOM = uomOptions.length > 0 ? uomOptions[0].UID : 'EA'

    const newLine: StockRequestLine = {
      UID: `LINE-${Date.now()}-${lines.length}`,
      SKUUID: '',
      SKUCode: '',
      SKUName: '',
      UOM: defaultUOM,
      RequestedQty: 0,
      LineNumber: lines.length + 1
    }
    setLines([...lines, newLine])
  }

  // Open multi-select SKU dialog
  const openSKUDialog = () => {
    setSkuDialogOpen(true)
    setSkuSearchQuery('')
  }

  // Handle SKU selection toggle
  const toggleSKUSelection = (skuUID: string) => {
    const newSelected = new Set(selectedSKUs)
    if (newSelected.has(skuUID)) {
      newSelected.delete(skuUID)
    } else {
      newSelected.add(skuUID)
    }
    setSelectedSKUs(newSelected)
  }

  // Add selected SKUs as lines
  const addSelectedSKUs = () => {
    const defaultUOM = uomOptions.length > 0 ? uomOptions[0].UID : 'EA'
    const newLines: StockRequestLine[] = []

    selectedSKUs.forEach((skuUID) => {
      const sku = skus.find(s => s.UID === skuUID)
      if (sku && !lines.find(line => line.SKUUID === skuUID)) {
        newLines.push({
          UID: `LINE-${Date.now()}-${lines.length + newLines.length}`,
          SKUUID: sku.UID,
          SKUCode: sku.Code,
          SKUName: sku.Name,
          UOM: sku.UOM || defaultUOM,
          UOM1: sku.UOM1,
          UOM2: sku.UOM2,
          UOM1CNF: sku.UOM1CNF,
          UOM2CNF: sku.UOM2CNF,
          RequestedQty: 0,
          LineNumber: lines.length + newLines.length + 1
        })
      }
    })

    setLines([...lines, ...newLines])
    setSelectedSKUs(new Set())
    setSkuDialogOpen(false)

    toast({
      title: 'Success',
      description: `Added ${newLines.length} SKU(s) to request`,
    })
  }

  // Filter SKUs based on search query
  const filteredSKUs = skus.filter(sku => {
    if (!skuSearchQuery) return true
    const query = skuSearchQuery.toLowerCase()
    return (
      sku.Code?.toLowerCase().includes(query) ||
      sku.Name?.toLowerCase().includes(query) ||
      sku.UID?.toLowerCase().includes(query)
    )
  })

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

    setLoading((prev) => ({ ...prev, skus: true }))
    try {
      const now = new Date().toISOString()
      const requestUID = `WH-${Date.now()}`

      // Get target org and warehouse for partitioning
      const targetOrgUID = formData.TargetOrgUID
      const targetWarehouseUID = formData.TargetWHUID

      // IMPORTANT: Use warehouse UID as-is (DO NOT combine with org_uid)
      // Partition structure is inconsistent across orgs:
      // - DIST1759603974795 partition expects just 'WH001', not 'DIST1759603974795_WH001'
      // - LBCL partition expects just 'TEST12', not 'LBCL_TEST12'
      // - DIST001 partition expects combined 'DIST001_FRWH'
      // We use TargetWHUID as-is since it already contains the correct format

      console.log('DEBUG: Target Org:', targetOrgUID)
      console.log('DEBUG: Target Warehouse (for partition):', targetWarehouseUID)

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
        OrgUID: targetOrgUID,
        WareHouseUID: targetWarehouseUID, // Use as-is for partition
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
          JobPositionUID: formData.RoleUID, // Send RoleUID as JobPositionUID
          RequiredByDate: formData.RequiredByDate,
          Status: formData.Status,
          Remarks: formData.Remarks,
          StockType: formData.StockType,
          RouteUID: formData.RouteUID,
          OrgUID: targetOrgUID,
          WareHouseUID: targetWarehouseUID, // Use as-is for partition
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

      console.log('DEBUG: Stock Request Payload:', JSON.stringify(payload, null, 2))

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
      setLoading((prev) => ({ ...prev, skus: false }))
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

            {/* Source Organization Hierarchy */}
            <div className="space-y-3">
              <Label>Source Organization *</Label>
              <div className="grid grid-cols-1 md:grid-cols-3 gap-3">
                {sourceOrgLevels.map((level, index) => (
                  <div key={index} className="space-y-2">
                    <Label className="text-xs text-muted-foreground">{level.typeName || 'Organization'}</Label>
                    <Select
                      value={sourceSelectedOrgs[index] || ''}
                      onValueChange={(value) => handleSourceOrgSelect(index, value)}
                      disabled={index > 0 && !sourceSelectedOrgs[index - 1]}
                    >
                      <SelectTrigger className="w-full">
                        <SelectValue placeholder={`Select ${level.typeName || 'Organization'}`} />
                      </SelectTrigger>
                      <SelectContent>
                        {level.organizations.map((org) => (
                          <SelectItem key={org.UID} value={org.UID}>
                            {org.Code} - {org.Name}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                  </div>
                ))}
              </div>
              {formData.SourceOrgUID && (
                <Button
                  type="button"
                  variant="outline"
                  size="sm"
                  onClick={handleSourceOrgReset}
                  className="mt-2"
                >
                  <X className="h-4 w-4 mr-2" />
                  Clear Selection
                </Button>
              )}
            </div>

            {/* Source Warehouse */}
            <div className="space-y-2">
              <Label htmlFor="sourceWH">Source Warehouse *</Label>
              <Select
                value={formData.SourceWHUID}
                onValueChange={(value) => setFormData({ ...formData, SourceWHUID: value })}
                disabled={!formData.SourceOrgUID || sourceWarehouses.length === 0}
              >
                <SelectTrigger id="sourceWH">
                  <SelectValue placeholder={sourceWarehouses.length === 0 ? "No warehouses available" : "Select warehouse"} />
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

          {/* Target Information */}
          <div className="space-y-4">
            <h3 className="text-lg font-semibold">Target Information</h3>

            {/* Target Organization Hierarchy */}
            <div className="space-y-3">
              <Label>Target Organization *</Label>
              <div className="grid grid-cols-1 md:grid-cols-3 gap-3">
                {targetOrgLevels.map((level, index) => (
                  <div key={index} className="space-y-2">
                    <Label className="text-xs text-muted-foreground">{level.typeName || 'Organization'}</Label>
                    <Select
                      value={targetSelectedOrgs[index] || ''}
                      onValueChange={(value) => handleTargetOrgSelect(index, value)}
                      disabled={index > 0 && !targetSelectedOrgs[index - 1]}
                    >
                      <SelectTrigger className="w-full">
                        <SelectValue placeholder={`Select ${level.typeName || 'Organization'}`} />
                      </SelectTrigger>
                      <SelectContent>
                        {level.organizations.map((org) => (
                          <SelectItem key={org.UID} value={org.UID}>
                            {org.Code} - {org.Name}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                  </div>
                ))}
              </div>
              {formData.TargetOrgUID && (
                <Button
                  type="button"
                  variant="outline"
                  size="sm"
                  onClick={handleTargetOrgReset}
                  className="mt-2"
                >
                  <X className="h-4 w-4 mr-2" />
                  Clear Selection
                </Button>
              )}
            </div>

            {/* Target Warehouse */}
            <div className="space-y-2">
              <Label htmlFor="targetWH">Target Warehouse *</Label>
              <Select
                value={formData.TargetWHUID}
                onValueChange={(value) => setFormData({ ...formData, TargetWHUID: value })}
                disabled={!formData.TargetOrgUID || targetWarehouses.length === 0}
              >
                <SelectTrigger id="targetWH">
                  <SelectValue placeholder={targetWarehouses.length === 0 ? "No warehouses available" : "Select warehouse"} />
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

          {/* Requester Information */}
          <div className="space-y-4">
            <h3 className="text-lg font-semibold">Requester Information</h3>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              {/* Role Field */}
              <div className="space-y-2">
                <Label className="text-sm font-medium">
                  Role <span className="text-red-500">*</span>
                  {roles.length > 0 && <span className="text-xs text-gray-500 ml-2">({roles.length} available)</span>}
                </Label>
                {loading.roles ? (
                  <Skeleton className="h-10 w-full" />
                ) : (
                  <Popover open={rolePopoverOpen} onOpenChange={setRolePopoverOpen}>
                    <PopoverTrigger asChild>
                      <Button
                        variant="outline"
                        role="combobox"
                        aria-expanded={rolePopoverOpen}
                        className="w-full h-10 justify-between text-left font-normal text-gray-900"
                      >
                        <span className="truncate text-gray-900">
                          {formData.RoleUID
                            ? roles.find((role) => role.value === formData.RoleUID)?.label || "Select role"
                            : "Select role"}
                        </span>
                        <ChevronDown className="ml-2 h-4 w-4 shrink-0 opacity-50" />
                      </Button>
                    </PopoverTrigger>
                    <PopoverContent className="w-[var(--radix-popover-trigger-width)] p-0" align="start">
                      <Command>
                        <div className="flex items-center border-b px-4 py-2">
                          <Search className="mr-3 h-4 w-4 shrink-0 opacity-50" />
                          <CommandInput
                            placeholder="Search roles..."
                            className="flex h-9 w-full rounded-md bg-transparent py-2 text-sm outline-none placeholder:text-muted-foreground disabled:cursor-not-allowed disabled:opacity-50 border-0 focus:ring-0"
                          />
                        </div>
                        <CommandEmpty className="py-6 text-center text-sm text-gray-500">
                          <div className="flex flex-col items-center gap-2">
                            <Search className="h-8 w-8 text-gray-400" />
                            <p className="text-gray-600">No roles found</p>
                          </div>
                        </CommandEmpty>
                        <CommandGroup>
                          <CommandList className="max-h-[280px] overflow-y-auto">
                            {roles.map((role) => (
                              <CommandItem
                                key={role.value}
                                value={role.label}
                                onSelect={() => {
                                  setFormData({ ...formData, RoleUID: role.value, RequestByEmpUID: '' })
                                  setRolePopoverOpen(false)
                                }}
                                className="flex items-center justify-between px-4 py-3 cursor-pointer hover:bg-accent rounded-none text-gray-900"
                              >
                                <div className="flex items-center gap-4 flex-1">
                                  <Check
                                    className={`h-4 w-4 shrink-0 ${
                                      formData.RoleUID === role.value ? "opacity-100 text-primary" : "opacity-0"
                                    }`}
                                  />
                                  <div className="font-medium text-sm truncate text-gray-900">{role.label}</div>
                                </div>
                              </CommandItem>
                            ))}
                          </CommandList>
                        </CommandGroup>
                      </Command>
                    </PopoverContent>
                  </Popover>
                )}
              </div>

              {/* Employee Field */}
              <div className="space-y-2">
                <Label className="text-sm font-medium">
                  Requesting Employee <span className="text-red-500">*</span>
                  {employees.length > 0 && <span className="text-xs text-gray-500 ml-2">({employees.length} available)</span>}
                </Label>
                {loading.employees ? (
                  <Skeleton className="h-10 w-full" />
                ) : (
                  <Popover open={employeePopoverOpen} onOpenChange={setEmployeePopoverOpen}>
                    <PopoverTrigger asChild>
                      <Button
                        variant="outline"
                        role="combobox"
                        aria-expanded={employeePopoverOpen}
                        disabled={!formData.RoleUID}
                        className="w-full h-10 justify-between text-left font-normal text-gray-900"
                      >
                        <span className="truncate text-gray-900">
                          {formData.RequestByEmpUID
                            ? employees.find((emp) => emp.value === formData.RequestByEmpUID)?.label || "Select employee"
                            : !formData.RoleUID
                            ? "Select role first"
                            : "Select employee"}
                        </span>
                        <ChevronDown className="ml-2 h-4 w-4 shrink-0 opacity-50" />
                      </Button>
                    </PopoverTrigger>
                    <PopoverContent className="w-[var(--radix-popover-trigger-width)] p-0" align="start">
                      <Command>
                        <div className="flex items-center border-b px-4 py-2">
                          <Search className="mr-3 h-4 w-4 shrink-0 opacity-50" />
                          <CommandInput
                            placeholder="Search employees..."
                            className="flex h-9 w-full rounded-md bg-transparent py-2 text-sm outline-none placeholder:text-muted-foreground disabled:cursor-not-allowed disabled:opacity-50 border-0 focus:ring-0"
                          />
                        </div>
                        <CommandEmpty className="py-6 text-center text-sm text-gray-500">
                          <div className="flex flex-col items-center gap-2">
                            <Users className="h-8 w-8 text-gray-400" />
                            <p className="text-gray-600">No employees found</p>
                          </div>
                        </CommandEmpty>
                        <CommandGroup>
                          <CommandList className="max-h-[280px] overflow-y-auto">
                            {employees.map((emp) => (
                              <CommandItem
                                key={emp.value}
                                value={emp.label}
                                onSelect={() => {
                                  setFormData({ ...formData, RequestByEmpUID: emp.value })
                                  setEmployeePopoverOpen(false)
                                }}
                                className="flex items-center justify-between px-4 py-3 cursor-pointer hover:bg-accent rounded-none text-gray-900"
                              >
                                <div className="flex items-center gap-4 flex-1">
                                  <Check
                                    className={`h-4 w-4 shrink-0 ${
                                      formData.RequestByEmpUID === emp.value ? "opacity-100 text-primary" : "opacity-0"
                                    }`}
                                  />
                                  <div className="font-medium text-sm truncate text-gray-900">{emp.label}</div>
                                </div>
                              </CommandItem>
                            ))}
                          </CommandList>
                        </CommandGroup>
                      </Command>
                    </PopoverContent>
                  </Popover>
                )}
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
            <div className="flex gap-2">
              <Button onClick={openSKUDialog} size="sm" variant="outline">
                <Package className="h-4 w-4 mr-2" />
                Add Multiple SKUs
              </Button>
              <Button onClick={addLine} size="sm">
                <Plus className="h-4 w-4 mr-2" />
                Add Single Line
              </Button>
            </div>
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
                      <TableCell>
                        <Select
                          value={line.UOM}
                          onValueChange={(value) => updateLine(index, 'UOM', value)}
                          disabled={loading.uom || uomOptions.length === 0}
                        >
                          <SelectTrigger className="w-full">
                            <SelectValue placeholder="Select UOM" />
                          </SelectTrigger>
                          <SelectContent>
                            {uomOptions.map((uom) => (
                              <SelectItem key={uom.UID} value={uom.UID}>
                                {uom.UID} - {uom.Name}
                              </SelectItem>
                            ))}
                          </SelectContent>
                        </Select>
                      </TableCell>
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
          disabled={loading.skus}
        >
          Cancel
        </Button>
        <Button
          onClick={handleSubmit}
          disabled={loading.skus}
        >
          {loading.skus ? 'Creating...' : 'Create Stock Request'}
        </Button>
      </div>

      {/* Multi-Select SKU Dialog */}
      <Dialog open={skuDialogOpen} onOpenChange={setSkuDialogOpen}>
        <DialogContent className="max-w-4xl max-h-[80vh] flex flex-col">
          <DialogHeader>
            <DialogTitle>Add Multiple SKUs</DialogTitle>
            <DialogDescription>
              Search and select multiple SKUs to add to the stock request
            </DialogDescription>
          </DialogHeader>

          {/* Search Bar */}
          <div className="flex items-center gap-2 pb-4 border-b">
            <div className="relative flex-1">
              <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-muted-foreground" />
              <Input
                placeholder="Search by SKU Code, Name, or UID..."
                value={skuSearchQuery}
                onChange={(e) => setSkuSearchQuery(e.target.value)}
                className="pl-10"
              />
            </div>
            <Badge variant="secondary" className="px-3 py-1">
              {selectedSKUs.size} selected
            </Badge>
          </div>

          {/* SKU List */}
          <div className="flex-1 overflow-y-auto border rounded-lg">
            <Table>
              <TableHeader className="sticky top-0 bg-background z-10">
                <TableRow>
                  <TableHead className="w-12">
                    <Checkbox
                      checked={selectedSKUs.size === filteredSKUs.length && filteredSKUs.length > 0}
                      onCheckedChange={(checked) => {
                        if (checked) {
                          setSelectedSKUs(new Set(filteredSKUs.map(s => s.UID)))
                        } else {
                          setSelectedSKUs(new Set())
                        }
                      }}
                    />
                  </TableHead>
                  <TableHead>Code</TableHead>
                  <TableHead>Name</TableHead>
                  <TableHead>UOM</TableHead>
                  <TableHead>UID</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {filteredSKUs.length === 0 ? (
                  <TableRow>
                    <TableCell colSpan={5} className="text-center py-12 text-muted-foreground">
                      <Package className="h-12 w-12 mx-auto mb-4 opacity-50" />
                      <p>No SKUs found</p>
                      <p className="text-sm">Try adjusting your search query</p>
                    </TableCell>
                  </TableRow>
                ) : (
                  filteredSKUs.map((sku) => {
                    const isSelected = selectedSKUs.has(sku.UID)
                    const isAlreadyInLines = lines.some(line => line.SKUUID === sku.UID)

                    return (
                      <TableRow
                        key={sku.UID}
                        className={`cursor-pointer ${isSelected ? 'bg-accent' : ''} ${isAlreadyInLines ? 'opacity-50' : ''}`}
                        onClick={() => !isAlreadyInLines && toggleSKUSelection(sku.UID)}
                      >
                        <TableCell>
                          <Checkbox
                            checked={isSelected}
                            disabled={isAlreadyInLines}
                            onCheckedChange={() => toggleSKUSelection(sku.UID)}
                            onClick={(e) => e.stopPropagation()}
                          />
                        </TableCell>
                        <TableCell className="font-medium">
                          {sku.Code}
                          {isAlreadyInLines && (
                            <Badge variant="outline" className="ml-2 text-xs">
                              Added
                            </Badge>
                          )}
                        </TableCell>
                        <TableCell>{sku.Name}</TableCell>
                        <TableCell>{sku.UOM || 'EA'}</TableCell>
                        <TableCell className="text-xs text-muted-foreground">{sku.UID}</TableCell>
                      </TableRow>
                    )
                  })
                )}
              </TableBody>
            </Table>
          </div>

          <DialogFooter className="border-t pt-4">
            <div className="flex items-center justify-between w-full">
              <div className="text-sm text-muted-foreground">
                {filteredSKUs.length} SKU(s) found
                {skuSearchQuery && ` (filtered from ${skus.length} total)`}
              </div>
              <div className="flex gap-2">
                <Button
                  variant="outline"
                  onClick={() => {
                    setSkuDialogOpen(false)
                    setSelectedSKUs(new Set())
                  }}
                >
                  Cancel
                </Button>
                <Button
                  onClick={addSelectedSKUs}
                  disabled={selectedSKUs.size === 0}
                >
                  Add {selectedSKUs.size > 0 && `(${selectedSKUs.size})`} SKU{selectedSKUs.size !== 1 ? 's' : ''}
                </Button>
              </div>
            </div>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  )
}
