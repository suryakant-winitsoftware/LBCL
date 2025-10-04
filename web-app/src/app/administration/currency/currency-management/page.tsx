'use client'

import React, { useState, useEffect, useRef } from 'react'
import { useRouter } from 'next/navigation'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { SafeSelect, SafeSelectItem } from '@/components/ui/safe-select'
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
import { PaginationControls } from '@/components/ui/pagination-controls'
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
  DollarSign,
  Hash,
  Globe,
  X
} from 'lucide-react'
import { Switch } from '@/components/ui/switch'
import currencyService, { Currency, PagingRequest } from '@/services/currencyService'

export default function CurrencyManagementPage() {
  const router = useRouter()
  const { toast } = useToast()
  const searchInputRef = useRef<HTMLInputElement>(null)

  // State management
  const [currencies, setCurrencies] = useState<Currency[]>([])
  const [filteredCurrencies, setFilteredCurrencies] = useState<Currency[]>([])
  const [loading, setLoading] = useState(true)
  const [searchTerm, setSearchTerm] = useState('')
  const [selectedCurrency, setSelectedCurrency] = useState<Currency | null>(null)
  const [showCreateDialog, setShowCreateDialog] = useState(false)
  const [showEditDialog, setShowEditDialog] = useState(false)
  const [showDeleteDialog, setShowDeleteDialog] = useState(false)
  const [formLoading, setFormLoading] = useState(false)

  // Pagination state
  const [currentPage, setCurrentPage] = useState(1)
  const [pageSize, setPageSize] = useState(10)
  const [totalCount, setTotalCount] = useState(0)

  // Form state
  const [formData, setFormData] = useState<Currency>({
    UID: '',
    Name: '',
    Symbol: '',
    Code: '',
    Digits: 2,
    FractionName: '',
    IsPrimary: false,
    RoundOffMinLimit: 0,
    RoundOffMaxLimit: 0
  })

  // Load currencies
  const loadCurrencies = async () => {
    setLoading(true)
    try {
      const pagingRequest: PagingRequest = {
        pageNumber: currentPage,
        pageSize: pageSize,
        isCountRequired: true
      }

      if (searchTerm) {
        pagingRequest.filterCriterias = [
          {
            PropertyName: "Name",
            FilterOperator: "Contains",
            FilterValue: searchTerm
          }
        ]
      }

      const response = await currencyService.getCurrencyDetails(pagingRequest)
      setCurrencies(response.PagedData || [])
      setFilteredCurrencies(response.PagedData || [])
      setTotalCount(response.TotalCount || 0)
    } catch (error) {
      console.error('Error loading currencies:', error)
      toast({
        title: "Error",
        description: "Failed to load currencies. Please try again.",
        variant: "destructive"
      })
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    loadCurrencies()
  }, [currentPage, pageSize, searchTerm])

  // Handle search
  const handleSearch = (value: string) => {
    setSearchTerm(value)
    setCurrentPage(1)
  }

  // Reset form
  const resetForm = () => {
    setFormData({
      UID: '',
      Name: '',
      Symbol: '',
      Code: '',
      Digits: 2,
      FractionName: '',
      IsPrimary: false,
      RoundOffMinLimit: 0,
      RoundOffMaxLimit: 0
    })
  }

  // Handle create
  const handleCreate = async () => {
    if (!formData.Code || !formData.Name || !formData.Symbol) {
      toast({
        title: "Validation Error",
        description: "Please fill in all required fields.",
        variant: "destructive"
      })
      return
    }

    // Ensure UID is set to Code
    formData.UID = formData.Code;

    setFormLoading(true)
    try {
      await currencyService.createCurrency(formData)
      toast({
        title: "Success",
        description: "Currency created successfully.",
      })
      setShowCreateDialog(false)
      resetForm()
      loadCurrencies()
    } catch (error) {
      console.error('Error creating currency:', error)
      toast({
        title: "Error",
        description: "Failed to create currency. Please try again.",
        variant: "destructive"
      })
    } finally {
      setFormLoading(false)
    }
  }

  // Handle update
  const handleUpdate = async () => {
    if (!formData.Code || !formData.Name || !formData.Symbol) {
      toast({
        title: "Validation Error",
        description: "Please fill in all required fields.",
        variant: "destructive"
      })
      return
    }

    // Ensure UID matches Code for consistency
    formData.UID = formData.Code;

    setFormLoading(true)
    try {
      await currencyService.updateCurrency(formData)
      toast({
        title: "Success",
        description: "Currency updated successfully.",
      })
      setShowEditDialog(false)
      resetForm()
      loadCurrencies()
    } catch (error) {
      console.error('Error updating currency:', error)
      toast({
        title: "Error",
        description: "Failed to update currency. Please try again.",
        variant: "destructive"
      })
    } finally {
      setFormLoading(false)
    }
  }

  // Handle delete
  const handleDelete = async () => {
    if (!selectedCurrency?.UID) return

    setFormLoading(true)
    try {
      await currencyService.deleteCurrency(selectedCurrency.UID)
      toast({
        title: "Success",
        description: "Currency deleted successfully.",
      })
      setShowDeleteDialog(false)
      setSelectedCurrency(null)
      loadCurrencies()
    } catch (error) {
      console.error('Error deleting currency:', error)
      toast({
        title: "Error",
        description: "Failed to delete currency. Please try again.",
        variant: "destructive"
      })
    } finally {
      setFormLoading(false)
    }
  }

  // Handle edit click
  const handleEditClick = (currency: Currency) => {
    setSelectedCurrency(currency)
    setFormData({ ...currency })
    setShowEditDialog(true)
  }

  // Handle view click
  const handleViewClick = (currency: Currency) => {
    router.push(`/administration/currency/currency-management/view/${currency.UID}`)
  }

  // Handle delete click
  const handleDeleteClick = (currency: Currency) => {
    setSelectedCurrency(currency)
    setShowDeleteDialog(true)
  }

  return (
    <div className="p-6 space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Currency Management</h1>
          <p className="text-muted-foreground mt-2">
            Manage currencies for your organization
          </p>
        </div>
        <Button
          onClick={() => setShowCreateDialog(true)}
          className="bg-primary hover:bg-primary/90"
        >
          <Plus className="h-4 w-4 mr-2" />
          Add Currency
        </Button>
      </div>

      {/* Stats Cards */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        <Card>
          <CardHeader className="pb-3">
            <CardTitle className="text-sm font-medium text-muted-foreground">
              Total Currencies
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="flex items-center gap-2">
              <Database className="h-4 w-4 text-primary" />
              <span className="text-2xl font-bold">{totalCount}</span>
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
              <DollarSign className="h-4 w-4 text-green-600" />
              <span className="text-2xl font-bold">
                {currencies.filter(c => c.IsPrimary).length}
              </span>
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-3">
            <CardTitle className="text-sm font-medium text-muted-foreground">
              Decimal Places
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="flex items-center gap-2">
              <Hash className="h-4 w-4 text-blue-600" />
              <span className="text-2xl font-bold">
                {currencies.length > 0 ? Math.max(...currencies.map(c => c.Digits)) : 0}
              </span>
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-3">
            <CardTitle className="text-sm font-medium text-muted-foreground">
              Active Regions
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="flex items-center gap-2">
              <Globe className="h-4 w-4 text-purple-600" />
              <span className="text-2xl font-bold">{currencies.length}</span>
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Search and Filters */}
      <Card>
        <CardHeader className="pb-4">
          <div className="flex items-center justify-between">
            <CardTitle>Currency List</CardTitle>
            <div className="flex items-center gap-2">
              <Button
                variant="outline"
                size="sm"
                onClick={loadCurrencies}
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
          {/* Search Bar */}
          <div className="flex gap-4 mb-6">
            <div className="relative flex-1">
              <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-muted-foreground" />
              <Input
                ref={searchInputRef}
                placeholder="Search currencies by name..."
                value={searchTerm}
                onChange={(e) => handleSearch(e.target.value)}
                className="pl-10"
              />
            </div>
            <Button variant="outline">
              <Filter className="h-4 w-4 mr-2" />
              Filters
            </Button>
          </div>

          {/* Table */}
          {loading ? (
            <div className="space-y-2">
              {[...Array(5)].map((_, i) => (
                <Skeleton key={i} className="h-12 w-full" />
              ))}
            </div>
          ) : (
            <>
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Code</TableHead>
                    <TableHead>Name</TableHead>
                    <TableHead>Symbol</TableHead>
                    <TableHead>Decimal Places</TableHead>
                    <TableHead>Fraction Name</TableHead>
                    <TableHead>Round Off Min</TableHead>
                    <TableHead>Round Off Max</TableHead>
                    <TableHead className="text-center">Primary</TableHead>
                    <TableHead>Modified</TableHead>
                    <TableHead className="text-right">Actions</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {filteredCurrencies.length > 0 ? (
                    filteredCurrencies.map((currency) => (
                      <TableRow key={currency.UID}>
                        <TableCell className="font-medium">
                          <Badge variant="outline">{currency.Code}</Badge>
                        </TableCell>
                        <TableCell>{currency.Name}</TableCell>
                        <TableCell>
                          <span className="text-lg font-semibold">{currency.Symbol}</span>
                        </TableCell>
                        <TableCell>{currency.Digits}</TableCell>
                        <TableCell>{currency.FractionName || '-'}</TableCell>
                        <TableCell>
                          <span className="font-medium">
                            {currency.RoundOffMinLimit?.toFixed(2) || '0.00'}
                          </span>
                        </TableCell>
                        <TableCell>
                          <span className="font-medium">
                            {currency.RoundOffMaxLimit?.toFixed(2) || '0.00'}
                          </span>
                        </TableCell>
                        <TableCell className="text-center">
                          {currency.IsPrimary && (
                            <Badge className="bg-green-100 text-green-800">Primary</Badge>
                          )}
                        </TableCell>
                        <TableCell>
                          {currency.ServerModifiedTime
                            ? formatDateToDayMonthYear(currency.ServerModifiedTime)
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
                              <DropdownMenuItem onClick={() => handleViewClick(currency)}>
                                <Eye className="h-4 w-4 mr-2" />
                                View Details
                              </DropdownMenuItem>
                              <DropdownMenuItem onClick={() => handleEditClick(currency)}>
                                <Edit className="h-4 w-4 mr-2" />
                                Edit
                              </DropdownMenuItem>
                              <DropdownMenuSeparator />
                              <DropdownMenuItem
                                onClick={() => handleDeleteClick(currency)}
                                className="text-red-600"
                              >
                                <Trash2 className="h-4 w-4 mr-2" />
                                Delete
                              </DropdownMenuItem>
                            </DropdownMenuContent>
                          </DropdownMenu>
                        </TableCell>
                      </TableRow>
                    ))
                  ) : (
                    <TableRow>
                      <TableCell colSpan={10} className="text-center py-12">
                        <div className="flex flex-col items-center gap-2">
                          <AlertCircle className="h-8 w-8 text-muted-foreground" />
                          <p className="text-muted-foreground">No currencies found</p>
                        </div>
                      </TableCell>
                    </TableRow>
                  )}
                </TableBody>
              </Table>

              {/* Pagination */}
              {totalCount > 0 && (
                <div className="mt-4">
                  <PaginationControls
                    currentPage={currentPage}
                    totalCount={totalCount}
                    onPageChange={setCurrentPage}
                    pageSize={pageSize}
                    onPageSizeChange={(size) => {
                      setPageSize(size)
                      setCurrentPage(1)
                    }}
                    itemName="currencies"
                  />
                </div>
              )}
            </>
          )}
        </CardContent>
      </Card>

      {/* Create Dialog */}
      <Dialog open={showCreateDialog} onOpenChange={setShowCreateDialog}>
        <DialogContent className="max-w-2xl">
          <DialogHeader>
            <DialogTitle>Create New Currency</DialogTitle>
            <DialogDescription>
              Add a new currency to the system. All fields marked with * are required.
            </DialogDescription>
          </DialogHeader>
          <div className="grid gap-4 py-4">
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label htmlFor="code">Currency Code *</Label>
                <Input
                  id="code"
                  value={formData.Code}
                  onChange={(e) => {
                    const code = e.target.value.toUpperCase();
                    setFormData({ ...formData, Code: code, UID: code });
                  }}
                  placeholder="e.g., USD, EUR, GBP"
                  maxLength={3}
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="name">Currency Name *</Label>
                <Input
                  id="name"
                  value={formData.Name}
                  onChange={(e) => setFormData({ ...formData, Name: e.target.value })}
                  placeholder="e.g., US Dollar"
                />
              </div>
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label htmlFor="symbol">Symbol *</Label>
                <Input
                  id="symbol"
                  value={formData.Symbol}
                  onChange={(e) => setFormData({ ...formData, Symbol: e.target.value })}
                  placeholder="e.g., $, €, £"
                  maxLength={5}
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="digits">Decimal Places *</Label>
                <Input
                  id="digits"
                  type="number"
                  value={formData.Digits}
                  onChange={(e) => setFormData({ ...formData, Digits: parseInt(e.target.value) || 0 })}
                  min="0"
                  max="6"
                />
              </div>
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label htmlFor="fraction">Fraction Name</Label>
                <Input
                  id="fraction"
                  value={formData.FractionName}
                  onChange={(e) => setFormData({ ...formData, FractionName: e.target.value })}
                  placeholder="e.g., Cents, Pence, Paise"
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="minLimit">Round Off Min Limit</Label>
                <Input
                  id="minLimit"
                  type="number"
                  value={formData.RoundOffMinLimit}
                  onChange={(e) => setFormData({ ...formData, RoundOffMinLimit: parseFloat(e.target.value) || 0 })}
                  step="0.01"
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="maxLimit">Round Off Max Limit</Label>
                <Input
                  id="maxLimit"
                  type="number"
                  value={formData.RoundOffMaxLimit}
                  onChange={(e) => setFormData({ ...formData, RoundOffMaxLimit: parseFloat(e.target.value) || 0 })}
                  step="0.01"
                />
              </div>
            </div>
            <div className="flex items-center space-x-2">
              <Switch
                id="primary"
                checked={formData.IsPrimary}
                onCheckedChange={(checked) => setFormData({ ...formData, IsPrimary: checked })}
              />
              <Label htmlFor="primary">Set as Primary Currency</Label>
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
              {formLoading ? 'Creating...' : 'Create Currency'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* Edit Dialog */}
      <Dialog open={showEditDialog} onOpenChange={setShowEditDialog}>
        <DialogContent className="max-w-2xl">
          <DialogHeader>
            <DialogTitle>Edit Currency</DialogTitle>
            <DialogDescription>
              Update currency details. All fields marked with * are required.
            </DialogDescription>
          </DialogHeader>
          <div className="grid gap-4 py-4">
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label htmlFor="edit-code">Currency Code *</Label>
                <Input
                  id="edit-code"
                  value={formData.Code}
                  disabled
                  className="bg-muted"
                  title="Currency code cannot be changed after creation"
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="edit-name">Currency Name *</Label>
                <Input
                  id="edit-name"
                  value={formData.Name}
                  onChange={(e) => setFormData({ ...formData, Name: e.target.value })}
                  placeholder="e.g., US Dollar"
                />
              </div>
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label htmlFor="edit-symbol">Symbol *</Label>
                <Input
                  id="edit-symbol"
                  value={formData.Symbol}
                  onChange={(e) => setFormData({ ...formData, Symbol: e.target.value })}
                  placeholder="e.g., $, €, £"
                  maxLength={5}
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="edit-digits">Decimal Places *</Label>
                <Input
                  id="edit-digits"
                  type="number"
                  value={formData.Digits}
                  onChange={(e) => setFormData({ ...formData, Digits: parseInt(e.target.value) || 0 })}
                  min="0"
                  max="6"
                />
              </div>
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label htmlFor="edit-fraction">Fraction Name</Label>
                <Input
                  id="edit-fraction"
                  value={formData.FractionName}
                  onChange={(e) => setFormData({ ...formData, FractionName: e.target.value })}
                  placeholder="e.g., Cents, Pence, Paise"
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="edit-minLimit">Round Off Min Limit</Label>
                <Input
                  id="edit-minLimit"
                  type="number"
                  value={formData.RoundOffMinLimit}
                  onChange={(e) => setFormData({ ...formData, RoundOffMinLimit: parseFloat(e.target.value) || 0 })}
                  step="0.01"
                />
              </div>
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label htmlFor="edit-maxLimit">Round Off Max Limit</Label>
                <Input
                  id="edit-maxLimit"
                  type="number"
                  value={formData.RoundOffMaxLimit}
                  onChange={(e) => setFormData({ ...formData, RoundOffMaxLimit: parseFloat(e.target.value) || 0 })}
                  step="0.01"
                />
              </div>
              <div></div>
            </div>
            <div className="flex items-center space-x-2">
              <Switch
                id="edit-primary"
                checked={formData.IsPrimary}
                onCheckedChange={(checked) => setFormData({ ...formData, IsPrimary: checked })}
              />
              <Label htmlFor="edit-primary">Set as Primary Currency</Label>
            </div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => {
              setShowEditDialog(false)
              resetForm()
            }}>
              Cancel
            </Button>
            <Button onClick={handleUpdate} disabled={formLoading}>
              {formLoading ? 'Updating...' : 'Update Currency'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>


      {/* Delete Confirmation Dialog */}
      <Dialog open={showDeleteDialog} onOpenChange={setShowDeleteDialog}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Delete Currency</DialogTitle>
            <DialogDescription>
              Are you sure you want to delete this currency? This action cannot be undone.
            </DialogDescription>
          </DialogHeader>
          {selectedCurrency && (
            <div className="py-4">
              <div className="bg-destructive/10 border border-destructive/20 rounded-lg p-4">
                <div className="flex items-center gap-2">
                  <AlertCircle className="h-5 w-5 text-destructive" />
                  <p className="font-medium">
                    You are about to delete: {selectedCurrency.Name} ({selectedCurrency.Code})
                  </p>
                </div>
              </div>
            </div>
          )}
          <DialogFooter>
            <Button variant="outline" onClick={() => {
              setShowDeleteDialog(false)
              setSelectedCurrency(null)
            }}>
              Cancel
            </Button>
            <Button
              variant="destructive"
              onClick={handleDelete}
              disabled={formLoading}
            >
              {formLoading ? 'Deleting...' : 'Delete Currency'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  )
}