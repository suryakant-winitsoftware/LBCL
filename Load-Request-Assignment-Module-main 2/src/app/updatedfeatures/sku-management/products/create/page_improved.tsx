'use client'

import { useState, useEffect } from 'react'
import { useRouter } from 'next/navigation'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Switch } from '@/components/ui/switch'
import { ArrowLeft, Save } from 'lucide-react'
import { CreateSKURequest } from '@/services/sku/sku.service'
import { skuAttributesService } from '@/services/sku/sku-attributes.service'
import { hierarchyService, SKUGroupType, HierarchyOption } from '@/services/hierarchy.service'
import { uomService } from '@/services/uom.service'
import { useToast } from '@/components/ui/use-toast'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'

export default function CreateSKUPage() {
  const router = useRouter()
  const { toast } = useToast()
  const [loading, setLoading] = useState(false)
  const [hierarchyTypes, setHierarchyTypes] = useState<SKUGroupType[]>([])
  const [hierarchyOptions, setHierarchyOptions] = useState<Record<string, HierarchyOption[]>>({})
  const [loadingHierarchy, setLoadingHierarchy] = useState(true)
  const [selectedAttributes, setSelectedAttributes] = useState<Array<{ type: string; code: string; value: string }>>([])
  const [baseUomOptions, setBaseUomOptions] = useState<{ code: string; name: string; label: string }[]>([])
  const [outerUomOptions, setOuterUomOptions] = useState<{ code: string; name: string; label: string }[]>([])
  const [loadingUOM, setLoadingUOM] = useState(true)
  const [formData, setFormData] = useState<CreateSKURequest>({
    UID: '',
    Code: '',
    Name: '',
    ArabicName: '',
    AliasName: '',
    LongName: '',
    OrgUID: 'Farmley',
    SupplierOrgUID: 'Supplier',
    BaseUOM: '',
    OuterUOM: '',
    BuyingUOM: '',
    SellingUOM: '',
    CanBuy: true,
    CanSell: true,
    IsActive: true,
    IsStockable: true,
    IsThirdParty: false,
    FromDate: new Date().toISOString().split('T')[0],
    ToDate: '2099-12-31',
    HSNCode: '',
    ProductCategoryId: 1,
    Year: new Date().getFullYear(),
    CreatedBy: 'ADMIN',
    ModifiedBy: 'ADMIN'
  })

  // Load UOM options dynamically - separate base and outer
  useEffect(() => {
    const loadUOMData = async () => {
      try {
        setLoadingUOM(true)
        const [baseOptions, outerOptions] = await Promise.all([
          uomService.getBaseUOMs(),
          uomService.getOuterUOMs()
        ])
        console.log('Base UOM options:', baseOptions)
        console.log('Outer UOM options:', outerOptions)
        setBaseUomOptions(baseOptions)
        setOuterUomOptions(outerOptions)
      } catch (error) {
        console.error('Failed to load UOM data:', error)
        toast({
          title: 'Warning',
          description: 'Failed to load units of measurement from server',
          variant: 'default'
        })
        setBaseUomOptions([])
        setOuterUomOptions([])
      } finally {
        setLoadingUOM(false)
      }
    }
    
    loadUOMData()
  }, [])

  // Load all hierarchy types and options dynamically - completely dynamic
  useEffect(() => {
    const loadHierarchyData = async () => {
      try {
        setLoadingHierarchy(true)
        const [types, options] = await Promise.all([
          hierarchyService.getHierarchyTypes(),
          hierarchyService.getAllHierarchyOptions()
        ])
        
        console.log('Available hierarchy types:', types)
        console.log('Available hierarchy options:', options)
        
        setHierarchyTypes(types)
        setHierarchyOptions(options)
        
        // Initialize with sorted hierarchy types, but only include types that have data
        if (types.length > 0) {
          const sortedTypes = types.sort((a, b) => (a.ItemLevel || 0) - (b.ItemLevel || 0))
          console.log('Sorted hierarchy types:', sortedTypes.map(t => `${t.Name} (Level ${t.ItemLevel})`));
          
          // Filter out hierarchy types that have no data available
          const typesWithData = sortedTypes.filter(type => {
            const hasData = options[type.Name] && options[type.Name].length > 0
            console.log(`${type.Name}: ${hasData ? options[type.Name].length : 0} options available`);
            return hasData
          })
          
          console.log('Types with data:', typesWithData.map(t => t.Name));
          const initialAttributes = typesWithData.map(type => ({ type: type.Name, code: '', value: '' }))
          setSelectedAttributes(initialAttributes)
        }
      } catch (error) {
        console.error('Failed to load hierarchy data:', error)
        toast({
          title: 'Warning', 
          description: 'Failed to load hierarchy data from server',
          variant: 'default'
        })
      } finally {
        setLoadingHierarchy(false)
      }
    }
    
    loadHierarchyData()
  }, [])

  const handleInputChange = (field: keyof CreateSKURequest, value: any) => {
    setFormData(prev => ({
      ...prev,
      [field]: value
    }))

    // Auto-generate UID from Code
    if (field === 'Code' && value) {
      setFormData(prev => ({
        ...prev,
        UID: value
      }))
    }

    // Auto-fill LongName from Name
    if (field === 'Name' && value) {
      setFormData(prev => ({
        ...prev,
        LongName: value,
        AliasName: value,
        ArabicName: value
      }))
    }
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    
    if (!formData.Code || !formData.Name) {
      toast({
        title: 'Validation Error',
        description: 'Product code and name are required',
        variant: 'destructive'
      })
      return
    }

    // Validate attributes
    const validAttributes = selectedAttributes.filter(attr => attr.code && attr.value)
    if (validAttributes.length === 0) {
      toast({
        title: 'Validation Error',
        description: 'At least one product attribute must be selected',
        variant: 'destructive'
      })
      return
    }

    setLoading(true)
    try {
      // Debug: Log the data being sent
      console.log('Creating SKU with data:', formData)
      console.log('Creating attributes:', validAttributes)
      
      // Use the comprehensive SKU creation process
      const result = await skuAttributesService.createSKUWithAttributes(formData, validAttributes)
      console.log('Complete SKU creation result:', result)
      
      // Count successful operations
      const operations = []
      if (result.skuResult > 0) operations.push('SKU created')
      if (result.attributesResult > 0) operations.push(`${validAttributes.length} attributes created`)
      if (result.uomResult) operations.push('UOM configurations created')
      if (result.configResult) operations.push('organization settings created')
      if (result.groupMappingResults && result.groupMappingResults.length > 0) {
        operations.push(`${result.groupMappingResults.length} hierarchy mappings created`)
      }
      if (result.hierarchyResults && result.hierarchyResults.length > 0) {
        operations.push(`${result.hierarchyResults.length} hierarchy data records created`)
      }
      
      toast({
        title: 'Product Created Successfully!',
        description: operations.join(', ')
      })
      router.push('/updatedfeatures/sku-management/products/manage')
    } catch (error: unknown) {
      const errorMessage = error instanceof Error ? error.message : 'Failed to create product'
      toast({
        title: 'Error',
        description: errorMessage,
        variant: 'destructive'
      })
      console.error('Error creating SKU with attributes:', error)
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-4">
        <Button
          variant="ghost"
          size="icon"
          onClick={() => router.back()}
        >
          <ArrowLeft className="h-4 w-4" />
        </Button>
        <div className="flex-1">
          <h1 className="text-3xl font-bold tracking-tight">Create Product - Standard Mode</h1>
          <p className="text-muted-foreground">
            Add a new product to your catalog with basic fields. Use Enterprise Mode for advanced features.
          </p>
        </div>
        <Button
          type="button"
          variant="outline"
          onClick={() => router.push('/updatedfeatures/sku-management/products/create/enterprise')}
          className="hidden md:flex"
        >
          Enterprise Mode
        </Button>
      </div>

      <form onSubmit={handleSubmit}>
        <div className="grid gap-6">
          <Card>
            <CardHeader>
              <CardTitle>Basic Information</CardTitle>
              <CardDescription>
                Enter the basic product details
              </CardDescription>
            </CardHeader>
            <CardContent className="grid gap-4">
              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="code">Product Code *</Label>
                  <Input
                    id="code"
                    value={formData.Code}
                    onChange={(e) => handleInputChange('Code', e.target.value)}
                    placeholder="e.g., PROD_001"
                    required
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="name">Product Name *</Label>
                  <Input
                    id="name"
                    value={formData.Name}
                    onChange={(e) => handleInputChange('Name', e.target.value)}
                    placeholder="e.g., Premium Almonds"
                    required
                  />
                </div>
              </div>

              <div className="space-y-2">
                <Label htmlFor="longName">Long Name</Label>
                <Input
                  id="longName"
                  value={formData.LongName}
                  onChange={(e) => handleInputChange('LongName', e.target.value)}
                  placeholder="Detailed product name"
                />
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="aliasName">Alias Name</Label>
                  <Input
                    id="aliasName"
                    value={formData.AliasName}
                    onChange={(e) => handleInputChange('AliasName', e.target.value)}
                    placeholder="Alternative name"
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="arabicName">Arabic Name</Label>
                  <Input
                    id="arabicName"
                    value={formData.ArabicName}
                    onChange={(e) => handleInputChange('ArabicName', e.target.value)}
                    placeholder="Product name in Arabic"
                  />
                </div>
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="hsnCode">HSN Code</Label>
                  <Input
                    id="hsnCode"
                    value={formData.HSNCode}
                    onChange={(e) => handleInputChange('HSNCode', e.target.value)}
                    placeholder="e.g., 12345678"
                  />
                </div>
              </div>

              <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
                {selectedAttributes.length === 0 ? (
                  <div className="col-span-full p-4 text-center text-gray-500 border rounded-lg bg-gray-50">
                    <p>No hierarchy levels available with data.</p>
                    <p className="text-xs mt-1">Please check your hierarchy configuration.</p>
                  </div>
                ) : (
                  selectedAttributes.map((attr, index) => {
                  // Sequential Cascading Logic
                  const isFirstLevel = index === 0
                  const parentSelected = index === 0 || (selectedAttributes[index - 1]?.code && selectedAttributes[index - 1]?.value)
                  const isEnabled = isFirstLevel || parentSelected
                  
                  // ONLY show current step and completed steps (true sequential)
                  const shouldShow = index === 0 || selectedAttributes[index - 1]?.code
                  
                  if (!shouldShow) return null
                  
                  return (
                    <div key={`${attr.type}-${index}`} className="space-y-2">
                      <Label htmlFor={`${attr.type}-${index}`} className="font-medium text-sm">{attr.type}</Label>
                      <Select
                        value={attr.code}
                        disabled={!isEnabled}
                        onValueChange={(value) => {
                          const updated = [...selectedAttributes]
                          updated[index].code = value
                          const option = hierarchyOptions[attr.type]?.find(opt => opt.code === value)
                          if (option) updated[index].value = option.value
                          
                          // Clear all subsequent levels (Sequential Cascading)
                          for (let i = index + 1; i < updated.length; i++) {
                            updated[i].code = ''
                            updated[i].value = ''
                          }
                          
                          setSelectedAttributes(updated)
                        }}
                      >
                        <SelectTrigger className="w-full">
                          <SelectValue placeholder={`Select ${attr.type.toLowerCase()}`} />
                        </SelectTrigger>
                        <SelectContent>
                          {loadingHierarchy ? (
                            <SelectItem value="loading" disabled>Loading...</SelectItem>
                          ) : hierarchyOptions[attr.type] ? (
                            hierarchyOptions[attr.type].map((option) => (
                              <SelectItem key={option.code} value={option.code}>
                                {option.code} - {option.value}
                              </SelectItem>
                            ))
                          ) : (
                            <SelectItem value="none" disabled>No options available</SelectItem>
                          )}
                        </SelectContent>
                      </Select>
                    </div>
                  )
                })
                )}
              </div>
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle>Organization Configurations</CardTitle>
              <CardDescription>
                Organization-specific product settings
              </CardDescription>
            </CardHeader>
            <CardContent className="grid gap-4">
              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="orgUID">Organization</Label>
                  <Input
                    id="orgUID"
                    value={formData.OrgUID}
                    onChange={(e) => handleInputChange('OrgUID', e.target.value)}
                    placeholder="Organization UID"
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="supplierOrgUID">Supplier Organization</Label>
                  <Input
                    id="supplierOrgUID"
                    value={formData.SupplierOrgUID}
                    onChange={(e) => handleInputChange('SupplierOrgUID', e.target.value)}
                    placeholder="Supplier Organization UID"
                  />
                </div>
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div className="flex items-center justify-between">
                  <div className="space-y-0.5">
                    <Label>Can Buy</Label>
                    <p className="text-sm text-muted-foreground">
                      Allow purchasing this product
                    </p>
                  </div>
                  <Switch
                    checked={formData.CanBuy}
                    onCheckedChange={(checked) => handleInputChange('CanBuy', checked)}
                  />
                </div>
                <div className="flex items-center justify-between">
                  <div className="space-y-0.5">
                    <Label>Can Sell</Label>
                    <p className="text-sm text-muted-foreground">
                      Allow selling this product
                    </p>
                  </div>
                  <Switch
                    checked={formData.CanSell}
                    onCheckedChange={(checked) => handleInputChange('CanSell', checked)}
                  />
                </div>
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="buyingUOM">Buying UOM</Label>
                  <Select
                    value={formData.BuyingUOM}
                    onValueChange={(value) => handleInputChange('BuyingUOM', value)}
                    disabled={loadingUOM || !formData.CanBuy}
                  >
                    <SelectTrigger>
                      <SelectValue placeholder={loadingUOM ? "Loading..." : "Select buying unit"} />
                    </SelectTrigger>
                    <SelectContent>
                      {loadingUOM ? (
                        <SelectItem value="loading" disabled>Loading...</SelectItem>
                      ) : baseUomOptions.length > 0 ? (
                        baseUomOptions.map((uom) => (
                          <SelectItem key={uom.code} value={uom.code}>
                            {uom.label}
                          </SelectItem>
                        ))
                      ) : (
                        <SelectItem value="none" disabled>No UOM options available</SelectItem>
                      )}
                    </SelectContent>
                  </Select>
                </div>
                <div className="space-y-2">
                  <Label htmlFor="sellingUOM">Selling UOM</Label>
                  <Select
                    value={formData.SellingUOM}
                    onValueChange={(value) => handleInputChange('SellingUOM', value)}
                    disabled={loadingUOM || !formData.CanSell}
                  >
                    <SelectTrigger>
                      <SelectValue placeholder={loadingUOM ? "Loading..." : "Select selling unit"} />
                    </SelectTrigger>
                    <SelectContent>
                      {loadingUOM ? (
                        <SelectItem value="loading" disabled>Loading...</SelectItem>
                      ) : outerUomOptions.length > 0 ? (
                        outerUomOptions.map((uom) => (
                          <SelectItem key={uom.code} value={uom.code}>
                            {uom.label}
                          </SelectItem>
                        ))
                      ) : (
                        <SelectItem value="none" disabled>No UOM options available</SelectItem>
                      )}
                    </SelectContent>
                  </Select>
                </div>
              </div>
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle>Units of Measurement</CardTitle>
              <CardDescription>
                Configure product units
              </CardDescription>
            </CardHeader>
            <CardContent className="grid gap-4">
              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="baseUOM">Base UOM</Label>
                  <Select
                    value={formData.BaseUOM}
                    onValueChange={(value) => handleInputChange('BaseUOM', value)}
                    disabled={loadingUOM}
                  >
                    <SelectTrigger>
                      <SelectValue placeholder={loadingUOM ? "Loading..." : "Select base unit"} />
                    </SelectTrigger>
                    <SelectContent>
                      {loadingUOM ? (
                        <SelectItem value="loading" disabled>Loading...</SelectItem>
                      ) : baseUomOptions.length > 0 ? (
                        baseUomOptions.map((uom) => (
                          <SelectItem key={uom.code} value={uom.code}>
                            {uom.label}
                          </SelectItem>
                        ))
                      ) : (
                        <SelectItem value="none" disabled>No base UOM options available</SelectItem>
                      )}
                    </SelectContent>
                  </Select>
                </div>
                <div className="space-y-2">
                  <Label htmlFor="outerUOM">Outer UOM</Label>
                  <Select
                    value={formData.OuterUOM}
                    onValueChange={(value) => handleInputChange('OuterUOM', value)}
                    disabled={loadingUOM}
                  >
                    <SelectTrigger>
                      <SelectValue placeholder={loadingUOM ? "Loading..." : "Select outer unit"} />
                    </SelectTrigger>
                    <SelectContent>
                      {loadingUOM ? (
                        <SelectItem value="loading" disabled>Loading...</SelectItem>
                      ) : outerUomOptions.length > 0 ? (
                        outerUomOptions.map((uom) => (
                          <SelectItem key={uom.code} value={uom.code}>
                            {uom.label}
                          </SelectItem>
                        ))
                      ) : (
                        <SelectItem value="none" disabled>No outer UOM options available</SelectItem>
                      )}
                    </SelectContent>
                  </Select>
                </div>
              </div>
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle>Validity Period</CardTitle>
              <CardDescription>
                Set the product's validity dates
              </CardDescription>
            </CardHeader>
            <CardContent className="grid gap-4">
              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="fromDate">Valid From</Label>
                  <Input
                    id="fromDate"
                    type="date"
                    value={formData.FromDate}
                    onChange={(e) => handleInputChange('FromDate', e.target.value)}
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="toDate">Valid To</Label>
                  <Input
                    id="toDate"
                    type="date"
                    value={formData.ToDate}
                    onChange={(e) => handleInputChange('ToDate', e.target.value)}
                  />
                </div>
              </div>
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle>Product Settings</CardTitle>
              <CardDescription>
                Configure product behavior and properties
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="flex items-center justify-between">
                <div className="space-y-0.5">
                  <Label>Active</Label>
                  <p className="text-sm text-muted-foreground">
                    Product is available for transactions
                  </p>
                </div>
                <Switch
                  checked={formData.IsActive}
                  onCheckedChange={(checked) => handleInputChange('IsActive', checked)}
                />
              </div>
              <div className="flex items-center justify-between">
                <div className="space-y-0.5">
                  <Label>Stockable</Label>
                  <p className="text-sm text-muted-foreground">
                    Track inventory for this product
                  </p>
                </div>
                <Switch
                  checked={formData.IsStockable}
                  onCheckedChange={(checked) => handleInputChange('IsStockable', checked)}
                />
              </div>
              <div className="flex items-center justify-between">
                <div className="space-y-0.5">
                  <Label>Third Party</Label>
                  <p className="text-sm text-muted-foreground">
                    Product is from a third-party supplier
                  </p>
                </div>
                <Switch
                  checked={formData.IsThirdParty}
                  onCheckedChange={(checked) => handleInputChange('IsThirdParty', checked)}
                />
              </div>
            </CardContent>
          </Card>

          <div className="flex justify-end gap-4">
            <Button
              type="button"
              variant="outline"
              onClick={() => router.back()}
            >
              Cancel
            </Button>
            <Button type="submit" disabled={loading}>
              <Save className="h-4 w-4 mr-2" />
              {loading ? 'Creating...' : 'Create Product'}
            </Button>
          </div>
        </div>
      </form>
    </div>
  )
}