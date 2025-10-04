'use client'

import React, { useState, useEffect } from 'react'
import { useRouter } from 'next/navigation'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue
} from '@/components/ui/select'
import { formatDateToDayMonthYear } from '@/utils/date-formatter'
import { Badge } from '@/components/ui/badge'
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogDescription,
  DialogFooter
} from '@/components/ui/dialog'
import { useToast } from '@/components/ui/use-toast'
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow
} from '@/components/ui/table'
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger
} from '@/components/ui/dropdown-menu'
import { Skeleton } from '@/components/ui/skeleton'
import {
  Plus,
  Search,
  Filter,
  Edit,
  Trash2,
  Database,
  RefreshCw,
  Download,
  Eye,
  AlertCircle,
  MoreHorizontal,
  Building2,
  DollarSign,
  Link2,
  Star,
  ArrowUpDown,
  X,
  ChevronUp,
  ChevronDown,
  Info
} from 'lucide-react'
import { Switch } from '@/components/ui/switch'
import currencyService, { Currency, OrgCurrency, PagingRequest } from '@/services/currencyService'
import {
  organizationService,
  Organization,
  OrgType
} from '@/services/organizationService'
import {
  initializeOrganizationHierarchy,
  handleOrganizationSelection,
  getFinalSelectedOrganization,
  resetOrganizationHierarchy,
  OrganizationLevel
} from '@/utils/organizationHierarchyUtils'

export default function OrgCurrencyManagementPage() {
  const router = useRouter()
  const { toast } = useToast()

  // State management
  const [orgCurrencies, setOrgCurrencies] = useState<OrgCurrency[]>([])
  const [filteredOrgCurrencies, setFilteredOrgCurrencies] = useState<OrgCurrency[]>([])
  const [availableCurrencies, setAvailableCurrencies] = useState<Currency[]>([])
  const [loading, setLoading] = useState(true)
  const [searchTerm, setSearchTerm] = useState('')
  const [selectedOrgCurrency, setSelectedOrgCurrency] = useState<OrgCurrency | null>(null)
  const [selectedOrgFilter, setSelectedOrgFilter] = useState<string>('')
  const [showCreateDialog, setShowCreateDialog] = useState(false)
  const [showDeleteDialog, setShowDeleteDialog] = useState(false)
  const [showViewDialog, setShowViewDialog] = useState(false)
  const [formLoading, setFormLoading] = useState(false)
  const [showOrgSelector, setShowOrgSelector] = useState(true)

  // Organization hierarchy states - Initialize with empty arrays to prevent undefined errors
  const [orgLevels, setOrgLevels] = useState<OrganizationLevel[]>([])
  const [selectedOrgs, setSelectedOrgs] = useState<string[]>([])
  const [orgTypes, setOrgTypes] = useState<OrgType[]>([])
  const [organizations, setOrganizations] = useState<Organization[]>([])
  const [selectedOrgCode, setSelectedOrgCode] = useState<string>('')

  // Form state
  const [formData, setFormData] = useState({
    OrgUID: '',
    CurrencyUID: '',
    IsPrimary: false
  })

  // Load initial data
  useEffect(() => {
    loadInitialData()
  }, [])

  // Initialize organization hierarchy
  useEffect(() => {
    const initHierarchy = async () => {
      try {
        // Fetch organization types and organizations
        const [typesResult, orgsResult] = await Promise.all([
          organizationService.getOrganizationTypes(),
          organizationService.getOrganizations(1, 1000)
        ])

        if (orgsResult.data && typesResult) {
          // For currency management, show all organizations
          const allOrganizations = orgsResult.data

          const initialLevels = initializeOrganizationHierarchy(
            allOrganizations,
            typesResult
          )

          setOrgLevels(initialLevels)
          setOrgTypes(typesResult)
          setOrganizations(allOrganizations)
        }
      } catch (error) {
        console.error('Error initializing organization hierarchy:', error)
        toast({
          title: "Error",
          description: "Failed to load organization hierarchy",
          variant: "destructive"
        })
      }
    }
    initHierarchy()
  }, [])

  // Load org currencies when organization is selected
  useEffect(() => {
    const selectedOrgUID = getFinalSelectedOrganization(selectedOrgs)

    console.log('Selected Orgs:', selectedOrgs)
    console.log('Final Selected Org UID:', selectedOrgUID)

    if (selectedOrgUID) {
      const selectedOrg = organizations.find(o => o.UID === selectedOrgUID)
      console.log('Loading currencies for org:', selectedOrg?.Name, selectedOrg?.Code)
      setSelectedOrgCode(selectedOrg?.Code || '')
      loadOrgCurrenciesForSelectedOrg(selectedOrgUID)
    } else {
      setSelectedOrgCode('')
      setOrgCurrencies([])
      setFilteredOrgCurrencies([])
    }
  }, [selectedOrgs]) // Only depend on selectedOrgs changes

  // Filter by search and organization
  useEffect(() => {
    filterData()
  }, [searchTerm, selectedOrgFilter, orgCurrencies])

  const loadInitialData = async () => {
    setLoading(true)
    try {
      // Load all currencies
      const currencyResponse = await currencyService.getCurrencyDetails({
        pageNumber: 1,
        pageSize: 100,
        isCountRequired: false
      })
      setAvailableCurrencies(currencyResponse.PagedData || [])
    } catch (error) {
      console.error('Error loading initial data:', error)
      toast({
        title: "Error",
        description: "Failed to load data. Please try again.",
        variant: "destructive"
      })
    } finally {
      setLoading(false)
    }
  }

  const loadOrgCurrenciesForSelectedOrg = async (orgUID: string) => {
    if (!orgUID) return

    setLoading(true)
    try {
      const orgCurrencyList = await currencyService.getCurrencyListByOrgUID(orgUID)
      const selectedOrg = organizations.find(o => o.UID === orgUID)

      if (orgCurrencyList && orgCurrencyList.length > 0) {
        // Add organization name to each record for display
        const enrichedList = orgCurrencyList.map(oc => ({
          ...oc,
          OrgName: selectedOrg?.Name || orgUID,
          OrgUID: orgUID
        }))
        setOrgCurrencies(enrichedList)
        setFilteredOrgCurrencies(enrichedList)
      } else {
        setOrgCurrencies([])
        setFilteredOrgCurrencies([])
      }
    } catch (error) {
      console.error('Error loading org currencies:', error)
      setOrgCurrencies([])
      setFilteredOrgCurrencies([])
    } finally {
      setLoading(false)
    }
  }

  // Handle organization selection in hierarchy
  const handleOrgSelection = (levelIndex: number, value: string) => {
    if (!value) return

    // Use utility function to handle organization selection
    const { updatedLevels, updatedSelectedOrgs } = handleOrganizationSelection(
      levelIndex,
      value,
      orgLevels,
      selectedOrgs,
      organizations,
      orgTypes
    )

    setOrgLevels(updatedLevels)
    setSelectedOrgs(updatedSelectedOrgs)

    // Get the final selected org to trigger currency loading
    const finalSelectedOrg = getFinalSelectedOrganization(updatedSelectedOrgs)
    if (finalSelectedOrg) {
      const selectedOrg = organizations.find(o => o.UID === finalSelectedOrg)
      setSelectedOrgCode(selectedOrg?.Code || '')

      // Auto-collapse the organization selector after selection
      setShowOrgSelector(false)
    }
  }

  // Handle organization reset
  const handleOrganizationReset = () => {
    if (organizations.length > 0 && orgTypes.length > 0) {
      const { resetLevels, resetSelectedOrgs } = resetOrganizationHierarchy(organizations, orgTypes)
      setOrgLevels(resetLevels)
      setSelectedOrgs(resetSelectedOrgs)
      setSelectedOrgCode('')
      setOrgCurrencies([])
      setFilteredOrgCurrencies([])

      // Show the org selector again when reset
      setShowOrgSelector(true)
    }
  }

  const filterData = () => {
    let filtered = [...orgCurrencies]

    // Filter by search term
    if (searchTerm) {
      filtered = filtered.filter(oc =>
        oc.Name?.toLowerCase().includes(searchTerm.toLowerCase()) ||
        oc.Code?.toLowerCase().includes(searchTerm.toLowerCase()) ||
        oc.OrgName?.toLowerCase().includes(searchTerm.toLowerCase())
      )
    }

    // Filter by organization
    if (selectedOrgFilter && selectedOrgFilter !== 'all') {
      filtered = filtered.filter(oc => oc.OrgUID === selectedOrgFilter)
    }

    setFilteredOrgCurrencies(filtered)
  }

  // Reset form
  const resetForm = () => {
    setFormData({
      OrgUID: '',
      CurrencyUID: '',
      IsPrimary: false
    })
  }

  // Handle create
  const handleCreate = async () => {
    const selectedOrgUID = getFinalSelectedOrganization(selectedOrgs)
    if (!selectedOrgUID || !formData.CurrencyUID) {
      toast({
        title: "Validation Error",
        description: "Please select both organization and currency.",
        variant: "destructive"
      })
      return
    }

    // Check if this combination already exists
    const exists = orgCurrencies.some(oc =>
      oc.OrgUID === selectedOrgUID && oc.CurrencyUID === formData.CurrencyUID
    )

    if (exists) {
      toast({
        title: "Validation Error",
        description: "This organization-currency combination already exists.",
        variant: "destructive"
      })
      return
    }

    setFormLoading(true)
    try {
      // Get the full currency details
      const currency = availableCurrencies.find(c => c.UID === formData.CurrencyUID)
      if (!currency) {
        throw new Error("Currency not found")
      }

      // Create the org-currency association
      // Backend expects a Currency object with OrgUID and CurrencyUID
      const orgCurrencyData = {
        OrgUID: selectedOrgUID,
        CurrencyUID: formData.CurrencyUID,
        UID: currency.UID, // Use the currency's UID
        Name: currency.Name,
        Code: currency.Code,
        Symbol: currency.Symbol,
        Digits: currency.Digits,
        FractionName: currency.FractionName || '',
        IsPrimary: formData.IsPrimary || false,
        RoundOffMinLimit: currency.RoundOffMinLimit || 0,
        RoundOffMaxLimit: currency.RoundOffMaxLimit || 0,
        SS: currency.SS || 0
      }

      console.log('Sending org-currency data:', orgCurrencyData)
      await currencyService.createOrgCurrency([orgCurrencyData])

      toast({
        title: "Success",
        description: "Organization currency association created successfully.",
      })

      setShowCreateDialog(false)
      resetForm()

      // Reload the currencies for the selected org
      if (selectedOrgUID) {
        await loadOrgCurrenciesForSelectedOrg(selectedOrgUID)
      }
    } catch (error: any) {
      console.error('Error creating org currency:', error)

      // Show more detailed error message
      const errorMessage = error.response?.data?.Message ||
                          error.response?.data?.message ||
                          error.message ||
                          "Failed to create association. Please try again."

      toast({
        title: "Error",
        description: errorMessage,
        variant: "destructive"
      })
    } finally {
      setFormLoading(false)
    }
  }

  // Handle delete
  const handleDelete = async () => {
    if (!selectedOrgCurrency?.UID) return

    setFormLoading(true)
    try {
      await currencyService.deleteOrgCurrency(selectedOrgCurrency.UID)
      toast({
        title: "Success",
        description: "Organization currency association deleted successfully.",
      })
      setShowDeleteDialog(false)
      setSelectedOrgCurrency(null)

      // Reload the currencies for the currently selected organization
      const selectedOrgUID = getFinalSelectedOrganization(selectedOrgs)
      if (selectedOrgUID) {
        await loadOrgCurrenciesForSelectedOrg(selectedOrgUID)
      }
    } catch (error: any) {
      console.error('Error deleting org currency:', error)

      // Show more detailed error message
      const errorMessage = error.response?.data?.Message ||
                          error.response?.data?.message ||
                          error.message ||
                          "Failed to delete association. Please try again."

      toast({
        title: "Error",
        description: errorMessage,
        variant: "destructive"
      })
    } finally {
      setFormLoading(false)
    }
  }

  // Handle view click
  const handleViewClick = (orgCurrency: OrgCurrency) => {
    setSelectedOrgCurrency(orgCurrency)
    setShowViewDialog(true)
  }

  // Handle delete click
  const handleDeleteClick = (orgCurrency: OrgCurrency) => {
    setSelectedOrgCurrency(orgCurrency)
    setShowDeleteDialog(true)
  }

  // Get unique organizations from org currencies
  const getUniqueOrganizations = () => {
    const uniqueOrgs = new Map()
    orgCurrencies.forEach(oc => {
      if (oc.OrgUID && !uniqueOrgs.has(oc.OrgUID)) {
        uniqueOrgs.set(oc.OrgUID, oc.OrgName || oc.OrgUID)
      }
    })
    return Array.from(uniqueOrgs, ([uid, name]) => ({ uid, name }))
  }

  return (
    <div className="p-6 space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Organization Currency Management</h1>
          <p className="text-muted-foreground mt-2">
            Manage currency assignments for organizations
          </p>
        </div>
        <Button
          onClick={() => setShowCreateDialog(true)}
          className="bg-primary hover:bg-primary/90"
          disabled={!getFinalSelectedOrganization(selectedOrgs)}
        >
          <Plus className="h-4 w-4 mr-2" />
          Assign Currency
        </Button>
      </div>

      {/* Organization Selector - Collapsible */}
      <Card className="bg-gradient-to-r from-blue-50 to-indigo-50 border-blue-200">
        <CardHeader
          className="pb-3"
        >
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-2">
              <Building2 className="h-5 w-5 text-blue-600" />
              <CardTitle className="text-lg">Organization Selection</CardTitle>
              {selectedOrgCode && (
                <Badge variant="secondary" className="bg-white ml-2">
                  {selectedOrgCode}
                </Badge>
              )}
            </div>
            <div className="flex items-center gap-2">
              {selectedOrgs.length > 0 && (
                <Badge className="bg-blue-100 text-blue-700 border-0">
                  <Info className="mr-1 h-3 w-3" />
                  {orgCurrencies.length} Currencies
                </Badge>
              )}
              <Button
                variant="ghost"
                size="sm"
                onClick={() => setShowOrgSelector(!showOrgSelector)}
                className="h-8 w-8 p-0"
              >
                {showOrgSelector ? (
                  <ChevronUp className="h-4 w-4" />
                ) : (
                  <ChevronDown className="h-4 w-4" />
                )}
              </Button>
            </div>
          </div>
        </CardHeader>
        {showOrgSelector && (
          <CardContent>
            <div className="space-y-4">
              {selectedOrgs.length === 0 && (
                <p className="text-sm text-gray-600 mb-3">
                  Select organization hierarchy to manage currency assignments
                </p>
              )}
              <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
                {orgLevels && orgLevels.length > 0 ? (
                  <>
                    {orgLevels.map((level, index) => (
                      <div key={level.orgTypeUID}>
                        <Label className="text-xs font-medium text-gray-600 mb-1">
                          {level.dynamicLabel || level.orgTypeName}
                          {index === 0 && (
                            <span className="text-red-500 ml-1">*</span>
                          )}
                        </Label>
                        <Select
                          value={level.selectedOrgUID || ""}
                          onValueChange={(value) => handleOrgSelection(index, value)}
                          disabled={index > 0 && !selectedOrgs[index - 1]}
                        >
                          <SelectTrigger className="h-9 text-sm">
                            <SelectValue placeholder={`Select ${level.orgTypeName}`} />
                          </SelectTrigger>
                          <SelectContent className="z-50">
                            {level.organizations && level.organizations.length > 0 ? (
                              level.organizations.map((org) => (
                                <SelectItem key={org.UID} value={org.UID}>
                                  <div className="flex items-center justify-between w-full">
                                    <span className="text-sm">{org.Name}</span>
                                    {org.Code && (
                                      <span className="text-muted-foreground ml-2 text-xs">
                                        ({org.Code})
                                      </span>
                                    )}
                                  </div>
                                </SelectItem>
                              ))
                            ) : (
                              <SelectItem value="no-data" disabled>
                                No organizations available
                              </SelectItem>
                            )}
                          </SelectContent>
                        </Select>
                      </div>
                    ))}
                  </>
                ) : (
                  <div className="col-span-2">
                    <Skeleton className="h-10 w-full" />
                  </div>
                )}
              </div>
              {selectedOrgs.length > 0 && (
                <div className="flex justify-end">
                  <Button
                    type="button"
                    variant="outline"
                    size="sm"
                    onClick={handleOrganizationReset}
                    className="text-xs h-8"
                  >
                    <X className="h-3 w-3 mr-1" />
                    Reset Selection
                  </Button>
                </div>
              )}
            </div>
          </CardContent>
        )}
      </Card>

      {/* Stats Cards - Show only when org is selected */}
      {getFinalSelectedOrganization(selectedOrgs) && (
        <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
          <Card>
            <CardHeader className="pb-3">
              <CardTitle className="text-sm font-medium text-muted-foreground">
                Assigned Currencies
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="flex items-center gap-2">
                <Link2 className="h-4 w-4 text-primary" />
                <span className="text-2xl font-bold">{orgCurrencies.length}</span>
              </div>
            </CardContent>
          </Card>
          <Card>
            <CardHeader className="pb-3">
              <CardTitle className="text-sm font-medium text-muted-foreground">
                Selected Organization
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="flex items-center gap-2">
                <Building2 className="h-4 w-4 text-blue-600" />
                <span className="text-lg font-semibold truncate">
                  {selectedOrgCode || '-'}
                </span>
              </div>
            </CardContent>
          </Card>
        <Card>
          <CardHeader className="pb-3">
            <CardTitle className="text-sm font-medium text-muted-foreground">
              Primary Currencies
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="flex items-center gap-2">
              <Star className="h-4 w-4 text-yellow-600" />
              <span className="text-2xl font-bold">
                {orgCurrencies.filter(oc => oc.IsPrimary).length}
              </span>
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-3">
            <CardTitle className="text-sm font-medium text-muted-foreground">
              Available Currencies
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="flex items-center gap-2">
              <Database className="h-4 w-4 text-green-600" />
              <span className="text-2xl font-bold">{availableCurrencies.length}</span>
            </div>
          </CardContent>
        </Card>
      </div>
      )}

      {/* Filters and Table */}
      <Card>
        <CardHeader className="pb-4">
          <div className="flex items-center justify-between">
            <CardTitle>Organization Currency Assignments</CardTitle>
            <div className="flex items-center gap-2">
              <Button
                variant="outline"
                size="sm"
                onClick={loadInitialData}
              >
                <RefreshCw className="h-4 w-4 mr-2" />
                Refresh
              </Button>
              <Button variant="outline" size="sm">
                <Download className="h-4 w-4 mr-2" />
                Export
              </Button>
            </div>
          </div>
        </CardHeader>
        <CardContent>
          {/* Search and Filters */}
          <div className="flex gap-4 mb-6">
            <div className="relative flex-1">
              <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-muted-foreground" />
              <Input
                placeholder="Search by currency or organization..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="pl-10"
              />
            </div>
            <Select value={selectedOrgFilter} onValueChange={setSelectedOrgFilter}>
              <SelectTrigger className="w-[200px]">
                <SelectValue placeholder="All Organizations" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All Organizations</SelectItem>
                {getUniqueOrganizations().map(org => (
                  <SelectItem key={org.uid} value={org.uid}>
                    {org.name}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>

          {/* Table */}
          {loading ? (
            <div className="space-y-2">
              {[...Array(5)].map((_, i) => (
                <Skeleton key={i} className="h-12 w-full" />
              ))}
            </div>
          ) : (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Organization</TableHead>
                  <TableHead>Currency</TableHead>
                  <TableHead>Code</TableHead>
                  <TableHead>Symbol</TableHead>
                  <TableHead className="text-center">Primary</TableHead>
                  <TableHead>Decimal Places</TableHead>
                  <TableHead>Created Date</TableHead>
                  <TableHead className="text-right">Actions</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {filteredOrgCurrencies.length > 0 ? (
                  filteredOrgCurrencies.map((orgCurrency, index) => (
                    <TableRow key={`${orgCurrency.OrgUID}-${orgCurrency.CurrencyUID}-${index}`}>
                      <TableCell>
                        <div className="flex items-center gap-2">
                          <Building2 className="h-4 w-4 text-muted-foreground" />
                          <span className="font-medium">
                            {orgCurrency.OrgName || orgCurrency.OrgUID}
                          </span>
                        </div>
                      </TableCell>
                      <TableCell>{orgCurrency.Name}</TableCell>
                      <TableCell>
                        <Badge variant="outline">{orgCurrency.Code}</Badge>
                      </TableCell>
                      <TableCell>
                        <span className="text-lg font-semibold">{orgCurrency.Symbol}</span>
                      </TableCell>
                      <TableCell className="text-center">
                        {orgCurrency.IsPrimary ? (
                          <Badge className="bg-yellow-100 text-yellow-800">
                            <Star className="h-3 w-3 mr-1" />
                            Primary
                          </Badge>
                        ) : (
                          <Badge variant="outline">Secondary</Badge>
                        )}
                      </TableCell>
                      <TableCell>{orgCurrency.Digits}</TableCell>
                      <TableCell>
                        {orgCurrency.CreatedTime
                          ? formatDateToDayMonthYear(orgCurrency.CreatedTime)
                          : '-'}
                      </TableCell>
                      <TableCell className="text-right">
                        <DropdownMenu>
                          <DropdownMenuTrigger asChild>
                            <Button variant="ghost" size="icon">
                              <MoreHorizontal className="h-4 w-4" />
                            </Button>
                          </DropdownMenuTrigger>
                          <DropdownMenuContent align="end">
                            <DropdownMenuLabel>Actions</DropdownMenuLabel>
                            <DropdownMenuSeparator />
                            <DropdownMenuItem onClick={() => handleViewClick(orgCurrency)}>
                              <Eye className="h-4 w-4 mr-2" />
                              View Details
                            </DropdownMenuItem>
                            <DropdownMenuSeparator />
                            <DropdownMenuItem
                              onClick={() => handleDeleteClick(orgCurrency)}
                              className="text-red-600"
                            >
                              <Trash2 className="h-4 w-4 mr-2" />
                              Remove Assignment
                            </DropdownMenuItem>
                          </DropdownMenuContent>
                        </DropdownMenu>
                      </TableCell>
                    </TableRow>
                  ))
                ) : (
                  <TableRow>
                    <TableCell colSpan={8} className="text-center py-12">
                      <div className="flex flex-col items-center gap-2">
                        <AlertCircle className="h-8 w-8 text-muted-foreground" />
                        <p className="text-muted-foreground">
                          {getFinalSelectedOrganization(selectedOrgs)
                            ? "No currency assignments found for this organization"
                            : "Please select an organization to view currency assignments"}
                        </p>
                      </div>
                    </TableCell>
                  </TableRow>
                )}
              </TableBody>
            </Table>
          )}
        </CardContent>
      </Card>

      {/* Create Dialog */}
      <Dialog open={showCreateDialog} onOpenChange={setShowCreateDialog}>
        <DialogContent className="max-w-lg">
          <DialogHeader>
            <DialogTitle>Assign Currency to Organization</DialogTitle>
            <DialogDescription>
              Select a currency to assign to {selectedOrgCode || 'the selected organization'}.
            </DialogDescription>
          </DialogHeader>
          <div className="grid gap-4 py-4">
            <div className="space-y-2">
              <Label htmlFor="org" className="text-sm text-muted-foreground">Organization</Label>
              <div className="flex items-center gap-2 p-2 bg-muted rounded">
                <Building2 className="h-4 w-4 text-muted-foreground" />
                <span className="font-medium">
                  {organizations.find(o => o.UID === getFinalSelectedOrganization(selectedOrgs))?.Name || 'Selected Organization'}
                </span>
                {selectedOrgCode && (
                  <Badge variant="outline" className="ml-auto">{selectedOrgCode}</Badge>
                )}
              </div>
            </div>
            <div className="space-y-2">
              <Label htmlFor="currency">Currency *</Label>
              <Select
                value={formData.CurrencyUID}
                onValueChange={(value) => setFormData({ ...formData, CurrencyUID: value })}
              >
                <SelectTrigger>
                  <SelectValue placeholder="Select Currency" />
                </SelectTrigger>
                <SelectContent>
                  {availableCurrencies.map(currency => (
                    <SelectItem key={currency.UID} value={currency.UID}>
                      {currency.Name} ({currency.Code}) - {currency.Symbol}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
            <div className="flex items-center space-x-2">
              <Switch
                id="primary"
                checked={formData.IsPrimary}
                onCheckedChange={(checked) => setFormData({ ...formData, IsPrimary: checked })}
              />
              <Label htmlFor="primary">Set as Primary Currency for this Organization</Label>
            </div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => {
              setShowCreateDialog(false)
              resetForm()
            }}>
              Cancel
            </Button>
            <Button onClick={handleCreate} disabled={formLoading}>
              {formLoading ? 'Creating...' : 'Assign Currency'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* View Dialog */}
      <Dialog open={showViewDialog} onOpenChange={setShowViewDialog}>
        <DialogContent className="max-w-2xl">
          <DialogHeader>
            <DialogTitle>Organization Currency Details</DialogTitle>
            <DialogDescription>
              View detailed information about this currency assignment.
            </DialogDescription>
          </DialogHeader>
          {selectedOrgCurrency && (
            <div className="grid gap-4 py-4">
              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-1">
                  <Label className="text-sm text-muted-foreground">Organization</Label>
                  <p className="font-medium flex items-center gap-2">
                    <Building2 className="h-4 w-4 text-muted-foreground" />
                    {selectedOrgCurrency.OrgName || selectedOrgCurrency.OrgUID}
                  </p>
                </div>
                <div className="space-y-1">
                  <Label className="text-sm text-muted-foreground">Currency Status</Label>
                  <p className="font-medium">
                    {selectedOrgCurrency.IsPrimary ? (
                      <Badge className="bg-yellow-100 text-yellow-800">Primary Currency</Badge>
                    ) : (
                      <Badge variant="outline">Secondary Currency</Badge>
                    )}
                  </p>
                </div>
              </div>
              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-1">
                  <Label className="text-sm text-muted-foreground">Currency Name</Label>
                  <p className="font-medium">{selectedOrgCurrency.Name}</p>
                </div>
                <div className="space-y-1">
                  <Label className="text-sm text-muted-foreground">Currency Code</Label>
                  <p className="font-medium">{selectedOrgCurrency.Code}</p>
                </div>
              </div>
              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-1">
                  <Label className="text-sm text-muted-foreground">Symbol</Label>
                  <p className="font-medium text-lg">{selectedOrgCurrency.Symbol}</p>
                </div>
                <div className="space-y-1">
                  <Label className="text-sm text-muted-foreground">Decimal Places</Label>
                  <p className="font-medium">{selectedOrgCurrency.Digits}</p>
                </div>
              </div>
              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-1">
                  <Label className="text-sm text-muted-foreground">Fraction Name</Label>
                  <p className="font-medium">{selectedOrgCurrency.FractionName || '-'}</p>
                </div>
                <div className="space-y-1">
                  <Label className="text-sm text-muted-foreground">Organization UID</Label>
                  <p className="font-mono text-sm bg-muted px-2 py-1 rounded">
                    {selectedOrgCurrency.OrgUID}
                  </p>
                </div>
              </div>
              <div className="border-t pt-4">
                <div className="grid grid-cols-2 gap-4">
                  <div className="space-y-1">
                    <Label className="text-sm text-muted-foreground">Created Date</Label>
                    <p className="font-medium">
                      {selectedOrgCurrency.CreatedTime
                        ? formatDateToDayMonthYear(selectedOrgCurrency.CreatedTime)
                        : '-'}
                    </p>
                  </div>
                  <div className="space-y-1">
                    <Label className="text-sm text-muted-foreground">Last Modified</Label>
                    <p className="font-medium">
                      {selectedOrgCurrency.ModifiedTime
                        ? formatDateToDayMonthYear(selectedOrgCurrency.ModifiedTime)
                        : '-'}
                    </p>
                  </div>
                </div>
              </div>
            </div>
          )}
          <DialogFooter>
            <Button variant="outline" onClick={() => {
              setShowViewDialog(false)
              setSelectedOrgCurrency(null)
            }}>
              Close
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* Delete Confirmation Dialog */}
      <Dialog open={showDeleteDialog} onOpenChange={setShowDeleteDialog}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Remove Currency Assignment</DialogTitle>
            <DialogDescription>
              Are you sure you want to remove this currency assignment? This action cannot be undone.
            </DialogDescription>
          </DialogHeader>
          {selectedOrgCurrency && (
            <div className="py-4">
              <div className="bg-destructive/10 border border-destructive/20 rounded-lg p-4">
                <div className="flex items-center gap-2">
                  <AlertCircle className="h-5 w-5 text-destructive" />
                  <div>
                    <p className="font-medium">
                      Removing: {selectedOrgCurrency.Name} ({selectedOrgCurrency.Code})
                    </p>
                    <p className="text-sm text-muted-foreground">
                      From: {selectedOrgCurrency.OrgName || selectedOrgCurrency.OrgUID}
                    </p>
                  </div>
                </div>
              </div>
            </div>
          )}
          <DialogFooter>
            <Button variant="outline" onClick={() => {
              setShowDeleteDialog(false)
              setSelectedOrgCurrency(null)
            }}>
              Cancel
            </Button>
            <Button
              variant="destructive"
              onClick={handleDelete}
              disabled={formLoading}
            >
              {formLoading ? 'Removing...' : 'Remove Assignment'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  )
}