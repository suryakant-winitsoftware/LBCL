'use client'

import { useState, useEffect } from 'react'
import { useRouter } from 'next/navigation'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Switch } from '@/components/ui/switch'
import { ArrowLeft, Save, Building, Plus, Trash2, ChevronDown, ChevronUp, Package } from 'lucide-react'
import { useToast } from '@/components/ui/use-toast'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { Textarea } from '@/components/ui/textarea'
import { Badge } from '@/components/ui/badge'
import { Separator } from '@/components/ui/separator'

// Import enterprise services - This is the KEY difference
import { enterpriseSkuService, ISKUV1, CustomFieldData, EnterpriseSkuCreationRequest } from '@/services/sku/enterprise-sku.service'
import { hierarchyService, SKUGroupType, HierarchyOption } from '@/services/hierarchy.service'
import { uomService } from '@/services/uom.service'
import { useOrganizationHierarchy } from '@/hooks/useOrganizationHierarchy'
import { organizationService } from '@/services/organizationService'

export default function EnterpriseCreateSKUPage() {
  const router = useRouter()
  const { toast } = useToast()
  const [loading, setLoading] = useState(false)
  const [showIndustryFields, setShowIndustryFields] = useState(false)
  const [showCustomFields, setShowCustomFields] = useState(false)
  
  // Organization hierarchy hook - Advanced feature
  const {
    orgLevels,
    selectedOrgs,
    initializeHierarchy,
    selectOrganization,
    resetHierarchy,
    finalSelectedOrganization,
    hasSelection
  } = useOrganizationHierarchy()

  // Product hierarchy
  const [hierarchyTypes, setHierarchyTypes] = useState<SKUGroupType[]>([])
  const [hierarchyOptions, setHierarchyOptions] = useState<Record<string, HierarchyOption[]>>({})
  const [selectedAttributes, setSelectedAttributes] = useState<Array<{ type: string; code: string; value: string }>>([])
  const [loadingHierarchy, setLoadingHierarchy] = useState(true)

  // UOM options
  const [baseUomOptions, setBaseUomOptions] = useState<{ code: string; name: string; label: string }[]>([])
  const [outerUomOptions, setOuterUomOptions] = useState<{ code: string; name: string; label: string }[]>([])
  const [loadingUOM, setLoadingUOM] = useState(true)

  // Supplier organization
  const HARDCODED_SUPPLIER_UID = 'Supplier'

  // Custom fields - Enterprise feature
  const [customFields, setCustomFields] = useState<CustomFieldData[]>([])

  // ISKUV1 form data - Full enterprise model with all fields
  const [formData, setFormData] = useState<ISKUV1>({
    // Base SKU fields
    UID: '',
    Code: '',
    Name: '',
    ArabicName: '',
    AliasName: '',
    LongName: '',
    OrgUID: '',
    SupplierOrgUID: '',
    BaseUOM: '',
    OuterUOM: '',
    FromDate: new Date().toISOString().split('T')[0],
    ToDate: '2099-12-31',
    IsStockable: true,
    ParentUID: '',
    IsActive: true,
    IsThirdParty: false,
    CreatedBy: 'ADMIN',
    ModifiedBy: 'ADMIN',
    
    // Extended ISKUV1 fields - Enterprise specific
    L1: '',
    L2: '',
    L3: '',
    L4: '',
    L5: '',
    L6: '',
    ModelCode: '',
    Year: new Date().getFullYear(),
    Type: '',
    ProductType: '',
    Category: '',
    Tonnage: '',
    Capacity: '',
    StarRating: '',
    ProductCategoryId: 1,
    ProductCategoryName: '',
    ItemSeries: '',
    HSNCode: '',
    IsAvailableInApMaster: false,
    FilterKeys: []
  })

  // Load organization hierarchy
  useEffect(() => {
    const loadOrganizationHierarchy = async () => {
      try {
        const [orgTypesResult, orgsResult] = await Promise.all([
          organizationService.getOrganizationTypes(),
          organizationService.getOrganizations(1, 1000)
        ])
        
        if (orgsResult.data.length > 0 && orgTypesResult.length > 0) {
          initializeHierarchy(orgsResult.data, orgTypesResult)
        }
      } catch (error) {
        console.error('Failed to load organization hierarchy:', error)
        toast({
          title: 'Warning',
          description: 'Failed to load organization data',
          variant: 'default'
        })
      }
    }

    loadOrganizationHierarchy()
  }, [initializeHierarchy, toast])

  // Load UOM options
  useEffect(() => {
    const loadUOMData = async () => {
      try {
        setLoadingUOM(true)
        const [baseOptions, outerOptions] = await Promise.all([
          uomService.getBaseUOMs(),
          uomService.getOuterUOMs()
        ])
        setBaseUomOptions(baseOptions)
        setOuterUomOptions(outerOptions)
      } catch (error) {
        console.error('Failed to load UOM data:', error)
        setBaseUomOptions([])
        setOuterUomOptions([])
      } finally {
        setLoadingUOM(false)
      }
    }
    
    loadUOMData()
  }, [])

  // Load product hierarchy
  useEffect(() => {
    const loadHierarchyData = async () => {
      try {
        setLoadingHierarchy(true)
        const [types, options] = await Promise.all([
          hierarchyService.getHierarchyTypes(),
          hierarchyService.getAllHierarchyOptions()
        ])
        
        setHierarchyTypes(types)
        setHierarchyOptions(options)
        
        if (types.length > 0) {
          const sortedTypes = types.sort((a, b) => (a.ItemLevel || 0) - (b.ItemLevel || 0))
          const typesWithData = sortedTypes.filter(type => {
            const hasData = options[type.Name] && options[type.Name].length > 0
            return hasData
          })
          
          const initialAttributes = typesWithData.map(type => ({ type: type.Name, code: '', value: '' }))
          setSelectedAttributes(initialAttributes)
        }
      } catch (error) {
        console.error('Failed to load hierarchy data:', error)
        toast({
          title: 'Warning', 
          description: 'Failed to load hierarchy data',
          variant: 'default'
        })
      } finally {
        setLoadingHierarchy(false)
      }
    }
    
    loadHierarchyData()
  }, [toast])

  // Set supplier organization
  useEffect(() => {
    if (finalSelectedOrganization) {
      setFormData(prev => ({
        ...prev,
        SupplierOrgUID: HARDCODED_SUPPLIER_UID
      }))
    }
  }, [finalSelectedOrganization])

  const handleInputChange = (field: keyof ISKUV1, value: any) => {
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

    // Auto-fill related fields from Name
    if (field === 'Name' && value) {
      setFormData(prev => ({
        ...prev,
        LongName: value,
        AliasName: value,
        ArabicName: value
      }))
    }
  }

  const handleOrganizationSelection = (levelIndex: number, value: string) => {
    selectOrganization(levelIndex, value)
    
    if (value) {
      setFormData(prev => ({
        ...prev,
        OrgUID: finalSelectedOrganization || value
      }))
    }
  }

  const addCustomField = () => {
    const newField: CustomFieldData = {
      SNo: customFields.length + 1,
      UID: `${formData.UID}_CUSTOM_${customFields.length + 1}`,
      Label: '',
      Type: 'text',
      Value: ''
    }
    setCustomFields([...customFields, newField])
  }

  const updateCustomField = (index: number, field: keyof CustomFieldData, value: string) => {
    const updated = [...customFields]
    updated[index] = { ...updated[index], [field]: value }
    setCustomFields(updated)
  }

  const removeCustomField = (index: number) => {
    setCustomFields(customFields.filter((_, i) => i !== index))
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

    if (!hasSelection) {
      toast({
        title: 'Validation Error',
        description: 'Please select an organization',
        variant: 'destructive'
      })
      return
    }

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
      const request: EnterpriseSkuCreationRequest = {
        skuData: {
          ...formData,
          OrgUID: finalSelectedOrganization || formData.OrgUID,
          SupplierOrgUID: HARDCODED_SUPPLIER_UID
        },
        attributes: validAttributes,
        customFields: customFields.length > 0 ? customFields : undefined,
        organizationUID: finalSelectedOrganization || formData.OrgUID,
        supplierOrganizationUID: HARDCODED_SUPPLIER_UID,
        distributionChannelUID: finalSelectedOrganization || formData.OrgUID
      }
      
      console.log('Creating Enterprise SKU with request:', request)
      
      const result = await enterpriseSkuService.createEnterpriseSKU(request)
      console.log('Enterprise SKU creation result:', result)
      
      toast({
        title: 'Product Created Successfully!',
        description: 'Enterprise product has been created with all extended features'
      })
      router.push('/updatedfeatures/sku-management/products/manage')
    } catch (error: unknown) {
      const errorMessage = error instanceof Error ? error.message : 'Failed to create product'
      toast({
        title: 'Error',
        description: errorMessage,
        variant: 'destructive'
      })
      console.error('Error creating enterprise SKU:', error)
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
          <h1 className="text-3xl font-bold tracking-tight flex items-center gap-2">
            <Package className="h-6 w-6 text-blue-600" />
            Create Enterprise Product
          </h1>
          <p className="text-muted-foreground">
            Full-featured product creation with organization hierarchy, custom fields, and extended attributes
          </p>
        </div>
      </div>

      <form onSubmit={handleSubmit}>
        <div className="grid gap-6">
          {/* Organization Hierarchy - Enterprise Feature */}
          <Card className="border-blue-200">
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Building className="h-5 w-5 text-blue-600" />
                Organization Hierarchy
              </CardTitle>
              <CardDescription>
                Select the organization hierarchy for this product
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              {orgLevels.map((level, index) => (
                <div key={`${level.orgTypeUID}-${level.level}`} className="space-y-2">
                  <Label htmlFor={`org-level-${index}`} className="font-medium">
                    {level.dynamicLabel || level.orgTypeName}
                  </Label>
                  <Select
                    value={level.selectedOrgUID || ''}
                    onValueChange={(value) => handleOrganizationSelection(index, value)}
                  >
                    <SelectTrigger>
                      <SelectValue placeholder={`Select ${level.orgTypeName.toLowerCase()}`} />
                    </SelectTrigger>
                    <SelectContent>
                      {level.organizations.map((org) => (
                        <SelectItem key={org.UID} value={org.UID}>
                          {org.Code ? `${org.Name} (${org.Code})` : org.Name}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </div>
              ))}
              
              {hasSelection && (
                <div className="p-3 bg-green-50 rounded-md">
                  <p className="text-sm text-green-800">
                    ✓ Selected Organization: {finalSelectedOrganization}
                  </p>
                </div>
              )}
            </CardContent>
          </Card>

          {/* Essential Product Information */}
          <Card>
            <CardHeader>
              <CardTitle>Essential Product Information</CardTitle>
              <CardDescription>
                Core product details required for all products
              </CardDescription>
            </CardHeader>
            <CardContent className="grid gap-6">
              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="code">
                    Product Code <span className="text-red-500">*</span>
                  </Label>
                  <Input
                    id="code"
                    value={formData.Code}
                    onChange={(e) => handleInputChange('Code', e.target.value)}
                    placeholder="e.g., ALM_001"
                    required
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="name">
                    Product Name <span className="text-red-500">*</span>
                  </Label>
                  <Input
                    id="name"
                    value={formData.Name}
                    onChange={(e) => handleInputChange('Name', e.target.value)}
                    placeholder="e.g., Premium California Almonds"
                    required
                  />
                </div>
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="longName">Long Name</Label>
                  <Input
                    id="longName"
                    value={formData.LongName}
                    onChange={(e) => handleInputChange('LongName', e.target.value)}
                    placeholder="Extended product description"
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="aliasName">Alias Name</Label>
                  <Input
                    id="aliasName"
                    value={formData.AliasName}
                    onChange={(e) => handleInputChange('AliasName', e.target.value)}
                    placeholder="Alternative name"
                  />
                </div>
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="arabicName">Arabic Name</Label>
                  <Input
                    id="arabicName"
                    value={formData.ArabicName}
                    onChange={(e) => handleInputChange('ArabicName', e.target.value)}
                    placeholder="اسم المنتج بالعربية"
                    dir="rtl"
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="hsnCode">HSN/SAC Code</Label>
                  <Input
                    id="hsnCode"
                    value={formData.HSNCode}
                    onChange={(e) => handleInputChange('HSNCode', e.target.value)}
                    placeholder="e.g., 08021100"
                  />
                </div>
              </div>
            </CardContent>
          </Card>

          {/* Product Attributes (L1-L6) */}
          <Card>
            <CardHeader>
              <CardTitle>Product Attributes</CardTitle>
              <CardDescription>
                Configure product hierarchy (maps to L1-L6 fields)
              </CardDescription>
            </CardHeader>
            <CardContent>
              <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
                {selectedAttributes.map((attr, index) => {
                  const isFirstLevel = index === 0
                  const parentSelected = index === 0 || (selectedAttributes[index - 1]?.code && selectedAttributes[index - 1]?.value)
                  const isEnabled = isFirstLevel || parentSelected
                  const shouldShow = index === 0 || selectedAttributes[index - 1]?.code
                  
                  if (!shouldShow) return null
                  
                  return (
                    <div key={`${attr.type}-${index}`} className="space-y-2">
                      <Label className="font-medium text-sm">
                        {attr.type} → L{index + 1}
                      </Label>
                      <Select
                        value={attr.code}
                        disabled={!isEnabled}
                        onValueChange={(value) => {
                          const updated = [...selectedAttributes]
                          updated[index].code = value
                          const option = hierarchyOptions[attr.type]?.find(opt => opt.code === value)
                          if (option) updated[index].value = option.value
                          
                          for (let i = index + 1; i < updated.length; i++) {
                            updated[i].code = ''
                            updated[i].value = ''
                          }
                          
                          setSelectedAttributes(updated)
                        }}
                      >
                        <SelectTrigger>
                          <SelectValue placeholder={`Select ${attr.type.toLowerCase()}`} />
                        </SelectTrigger>
                        <SelectContent>
                          {hierarchyOptions[attr.type]?.map((option) => (
                            <SelectItem key={option.code} value={option.code}>
                              {option.code} - {option.value}
                            </SelectItem>
                          ))}
                        </SelectContent>
                      </Select>
                    </div>
                  )
                })}
              </div>
            </CardContent>
          </Card>

          {/* Units of Measurement */}
          <Card>
            <CardHeader>
              <CardTitle>Units of Measurement</CardTitle>
              <CardDescription>
                Configure base and outer units
              </CardDescription>
            </CardHeader>
            <CardContent className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label htmlFor="baseUOM">
                  Base UOM <span className="text-red-500">*</span>
                </Label>
                <Select
                  value={formData.BaseUOM}
                  onValueChange={(value) => handleInputChange('BaseUOM', value)}
                  disabled={loadingUOM}
                >
                  <SelectTrigger>
                    <SelectValue placeholder={loadingUOM ? "Loading..." : "Select base unit"} />
                  </SelectTrigger>
                  <SelectContent>
                    {baseUomOptions.map((uom) => (
                      <SelectItem key={uom.code} value={uom.code}>
                        {uom.label}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
              <div className="space-y-2">
                <Label htmlFor="outerUOM">
                  Outer UOM <span className="text-red-500">*</span>
                </Label>
                <Select
                  value={formData.OuterUOM}
                  onValueChange={(value) => handleInputChange('OuterUOM', value)}
                  disabled={loadingUOM}
                >
                  <SelectTrigger>
                    <SelectValue placeholder={loadingUOM ? "Loading..." : "Select outer unit"} />
                  </SelectTrigger>
                  <SelectContent>
                    {outerUomOptions.map((uom) => (
                      <SelectItem key={uom.code} value={uom.code}>
                        {uom.label}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
            </CardContent>
          </Card>

          {/* Industry-Specific Fields (Collapsible) */}
          <Card className="border-dashed">
            <CardHeader 
              className="cursor-pointer select-none"
              onClick={() => setShowIndustryFields(!showIndustryFields)}
            >
              <div className="flex items-center justify-between">
                <div>
                  <CardTitle>Industry-Specific Information</CardTitle>
                  <CardDescription>
                    Optional fields for specific industries
                  </CardDescription>
                </div>
                <Button type="button" variant="ghost" size="sm">
                  {showIndustryFields ? <ChevronUp /> : <ChevronDown />}
                </Button>
              </div>
            </CardHeader>
            {showIndustryFields && (
              <CardContent className="grid gap-4">
                <div className="grid grid-cols-3 gap-4">
                  <div className="space-y-2">
                    <Label htmlFor="modelCode">Model Code</Label>
                    <Input
                      id="modelCode"
                      value={formData.ModelCode}
                      onChange={(e) => handleInputChange('ModelCode', e.target.value)}
                      placeholder="e.g., MDL-2024"
                    />
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="year">Year</Label>
                    <Input
                      id="year"
                      type="number"
                      value={formData.Year}
                      onChange={(e) => handleInputChange('Year', parseInt(e.target.value))}
                      placeholder="2024"
                    />
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="itemSeries">Item Series</Label>
                    <Input
                      id="itemSeries"
                      value={formData.ItemSeries}
                      onChange={(e) => handleInputChange('ItemSeries', e.target.value)}
                      placeholder="e.g., Premium"
                    />
                  </div>
                </div>

                <div className="grid grid-cols-3 gap-4">
                  <div className="space-y-2">
                    <Label htmlFor="type">Type</Label>
                    <Input
                      id="type"
                      value={formData.Type}
                      onChange={(e) => handleInputChange('Type', e.target.value)}
                      placeholder="e.g., Food"
                    />
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="productType">Product Type</Label>
                    <Input
                      id="productType"
                      value={formData.ProductType}
                      onChange={(e) => handleInputChange('ProductType', e.target.value)}
                      placeholder="e.g., Dry Fruits"
                    />
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="category">Category</Label>
                    <Input
                      id="category"
                      value={formData.Category}
                      onChange={(e) => handleInputChange('Category', e.target.value)}
                      placeholder="e.g., Nuts"
                    />
                  </div>
                </div>

                <div className="grid grid-cols-3 gap-4">
                  <div className="space-y-2">
                    <Label htmlFor="capacity">Capacity</Label>
                    <Input
                      id="capacity"
                      value={formData.Capacity}
                      onChange={(e) => handleInputChange('Capacity', e.target.value)}
                      placeholder="e.g., 500g"
                    />
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="tonnage">Tonnage</Label>
                    <Select
                      value={formData.Tonnage || 'NA'}
                      onValueChange={(value) => handleInputChange('Tonnage', value === 'NA' ? '' : value)}
                    >
                      <SelectTrigger>
                        <SelectValue placeholder="Select tonnage" />
                      </SelectTrigger>
                      <SelectContent>
                        <SelectItem value="NA">Not Applicable</SelectItem>
                        <SelectItem value="1">1 Ton</SelectItem>
                        <SelectItem value="1.5">1.5 Ton</SelectItem>
                        <SelectItem value="2">2 Ton</SelectItem>
                      </SelectContent>
                    </Select>
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="starRating">Star Rating</Label>
                    <Select
                      value={formData.StarRating || 'NA'}
                      onValueChange={(value) => handleInputChange('StarRating', value === 'NA' ? '' : value)}
                    >
                      <SelectTrigger>
                        <SelectValue placeholder="Select rating" />
                      </SelectTrigger>
                      <SelectContent>
                        <SelectItem value="NA">Not Applicable</SelectItem>
                        {[1, 2, 3, 4, 5].map(rating => (
                          <SelectItem key={rating} value={rating.toString()}>
                            {rating} Star{rating > 1 ? 's' : ''}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                  </div>
                </div>
              </CardContent>
            )}
          </Card>

          {/* Custom Fields (Collapsible) */}
          <Card className="border-dashed">
            <CardHeader 
              className="cursor-pointer select-none"
              onClick={() => setShowCustomFields(!showCustomFields)}
            >
              <div className="flex items-center justify-between">
                <div>
                  <CardTitle>Custom Fields</CardTitle>
                  <CardDescription>
                    Add custom fields specific to this product
                  </CardDescription>
                </div>
                <div className="flex items-center gap-2">
                  {showCustomFields && (
                    <Button
                      type="button"
                      variant="outline"
                      size="sm"
                      onClick={(e) => {
                        e.stopPropagation();
                        addCustomField();
                      }}
                    >
                      <Plus className="h-4 w-4 mr-2" />
                      Add Field
                    </Button>
                  )}
                  <Button type="button" variant="ghost" size="sm">
                    {showCustomFields ? <ChevronUp /> : <ChevronDown />}
                  </Button>
                </div>
              </div>
            </CardHeader>
            {showCustomFields && (
              <CardContent className="space-y-4">
                {customFields.length === 0 ? (
                  <div className="text-center py-8 text-gray-500">
                    <p>No custom fields added yet.</p>
                    <p className="text-sm">Click "Add Field" to add custom fields.</p>
                  </div>
                ) : (
                  customFields.map((field, index) => (
                    <div key={index} className="grid grid-cols-4 gap-4 p-4 border rounded-lg">
                      <div className="space-y-2">
                        <Label>Label</Label>
                        <Input
                          value={field.Label}
                          onChange={(e) => updateCustomField(index, 'Label', e.target.value)}
                          placeholder="Field label"
                        />
                      </div>
                      <div className="space-y-2">
                        <Label>Type</Label>
                        <Select
                          value={field.Type}
                          onValueChange={(value) => updateCustomField(index, 'Type', value)}
                        >
                          <SelectTrigger>
                            <SelectValue />
                          </SelectTrigger>
                          <SelectContent>
                            <SelectItem value="text">Text</SelectItem>
                            <SelectItem value="number">Number</SelectItem>
                            <SelectItem value="date">Date</SelectItem>
                            <SelectItem value="boolean">Boolean</SelectItem>
                          </SelectContent>
                        </Select>
                      </div>
                      <div className="space-y-2">
                        <Label>Value</Label>
                        <Input
                          value={field.Value}
                          onChange={(e) => updateCustomField(index, 'Value', e.target.value)}
                          placeholder="Field value"
                        />
                      </div>
                      <div className="space-y-2">
                        <Label>Actions</Label>
                        <Button
                          type="button"
                          variant="destructive"
                          size="sm"
                          onClick={() => removeCustomField(index)}
                        >
                          <Trash2 className="h-4 w-4" />
                        </Button>
                      </div>
                    </div>
                  ))
                )}
              </CardContent>
            )}
          </Card>

          {/* Product Settings */}
          <Card>
            <CardHeader>
              <CardTitle>Product Settings</CardTitle>
              <CardDescription>
                Configure product behavior
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="grid grid-cols-2 gap-4">
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
              </div>
              <div className="grid grid-cols-2 gap-4">
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
                <div className="flex items-center justify-between">
                  <div className="space-y-0.5">
                    <Label>Available in AP Master</Label>
                    <p className="text-sm text-muted-foreground">
                      Available in AP master system
                    </p>
                  </div>
                  <Switch
                    checked={formData.IsAvailableInApMaster}
                    onCheckedChange={(checked) => handleInputChange('IsAvailableInApMaster', checked)}
                  />
                </div>
              </div>
            </CardContent>
          </Card>

          {/* Submit Button */}
          <div className="flex justify-end gap-4">
            <Button
              type="button"
              variant="outline"
              onClick={() => router.back()}
            >
              Cancel
            </Button>
            <Button 
              type="submit" 
              disabled={loading}
              className="bg-blue-600 hover:bg-blue-700"
            >
              <Save className="h-4 w-4 mr-2" />
              {loading ? 'Creating Enterprise Product...' : 'Create Enterprise Product'}
            </Button>
          </div>
        </div>
      </form>
    </div>
  )
}