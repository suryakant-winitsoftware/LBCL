'use client'

import { useState, useEffect } from 'react'
import { useRouter, useSearchParams } from 'next/navigation'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Switch } from '@/components/ui/switch'
import { ArrowLeft, Save, Loader2 } from 'lucide-react'
import { skuService, SKU, UpdateSKURequest } from '@/services/sku/sku.service'
import { useToast } from '@/components/ui/use-toast'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'

export default function EditSKUPage() {
  const router = useRouter()
  const { toast } = useToast()
  const searchParams = useSearchParams()
  const uid = searchParams.get('uid')
  
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [originalSKU, setOriginalSKU] = useState<SKU | null>(null)
  const [formData, setFormData] = useState<UpdateSKURequest>({
    UID: '',
    ModifiedBy: 'ADMIN'
  })

  useEffect(() => {
    if (uid) {
      fetchSKUDetails(uid)
    }
  }, [uid])

  const fetchSKUDetails = async (skuUID: string) => {
    try {
      const sku = await skuService.getSKUByUID(skuUID)
      setOriginalSKU(sku)
      setFormData({
        UID: sku.UID,
        Code: sku.Code,
        Name: sku.Name,
        ArabicName: sku.ArabicName,
        AliasName: sku.AliasName,
        LongName: sku.LongName,
        OrgUID: sku.OrgUID,
        SupplierOrgUID: sku.SupplierOrgUID,
        BaseUOM: sku.BaseUOM,
        OuterUOM: sku.OuterUOM,
        IsActive: sku.IsActive,
        IsStockable: sku.IsStockable,
        IsThirdParty: sku.IsThirdParty,
        FromDate: sku.FromDate,
        ToDate: sku.ToDate,
        HSNCode: sku.HSNCode,
        ProductCategoryId: sku.ProductCategoryId,
        Year: sku.Year,
        ModifiedBy: 'ADMIN'
      })
    } catch (error) {
      toast({
        title: 'Error',
        description: 'Failed to fetch product details',
        variant: 'destructive'
      })
      console.error('Error fetching SKU:', error)
    } finally {
      setLoading(false)
    }
  }

  const handleInputChange = (field: keyof UpdateSKURequest, value: any) => {
    setFormData(prev => ({
      ...prev,
      [field]: value
    }))
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

    setSaving(true)
    try {
      await skuService.updateSKU(formData)
      toast({
        title: 'Success',
        description: 'Product updated successfully'
      })
      router.push('/productssales/product-management/products')
    } catch (error) {
      toast({
        title: 'Error',
        description: 'Failed to update product',
        variant: 'destructive'
      })
      console.error('Error updating SKU:', error)
    } finally {
      setSaving(false)
    }
  }

  if (loading) {
    return (
      <div className="flex items-center justify-center h-96">
        <Loader2 className="h-8 w-8 animate-spin" />
      </div>
    )
  }

  if (!originalSKU) {
    return (
      <div className="text-center py-12">
        <p className="text-muted-foreground">Product not found</p>
      </div>
    )
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
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Edit Product</h1>
          <p className="text-muted-foreground">
            Update product information for {originalSKU.Code}
          </p>
        </div>
      </div>

      <form onSubmit={handleSubmit}>
        <div className="grid gap-6">
          <Card>
            <CardHeader>
              <CardTitle>Basic Information</CardTitle>
              <CardDescription>
                Update the basic product details
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
                  value={formData.LongName || ''}
                  onChange={(e) => handleInputChange('LongName', e.target.value)}
                  placeholder="Detailed product name"
                />
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="aliasName">Alias Name</Label>
                  <Input
                    id="aliasName"
                    value={formData.AliasName || ''}
                    onChange={(e) => handleInputChange('AliasName', e.target.value)}
                    placeholder="Alternative name"
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="arabicName">Arabic Name</Label>
                  <Input
                    id="arabicName"
                    value={formData.ArabicName || ''}
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
                    value={formData.HSNCode || ''}
                    onChange={(e) => handleInputChange('HSNCode', e.target.value)}
                    placeholder="e.g., 12345678"
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="category">Category</Label>
                  <Select
                    value={formData.ProductCategoryId?.toString() || '1'}
                    onValueChange={(value) => handleInputChange('ProductCategoryId', parseInt(value))}
                  >
                    <SelectTrigger>
                      <SelectValue placeholder="Select category" />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="1">Cashewnut</SelectItem>
                      <SelectItem value="2">Makhana</SelectItem>
                      <SelectItem value="3">Dried Fruits</SelectItem>
                      <SelectItem value="4">Raisins</SelectItem>
                      <SelectItem value="5">Dates</SelectItem>
                      <SelectItem value="6">Dry Fruit Mix</SelectItem>
                      <SelectItem value="7">Seeds</SelectItem>
                      <SelectItem value="8">Others</SelectItem>
                      <SelectItem value="9">Almonds</SelectItem>
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
                  >
                    <SelectTrigger>
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="EA">Each (EA)</SelectItem>
                      <SelectItem value="KG">Kilogram (KG)</SelectItem>
                      <SelectItem value="G">Gram (G)</SelectItem>
                      <SelectItem value="L">Liter (L)</SelectItem>
                      <SelectItem value="ML">Milliliter (ML)</SelectItem>
                      <SelectItem value="PCS">Pieces (PCS)</SelectItem>
                    </SelectContent>
                  </Select>
                </div>
                <div className="space-y-2">
                  <Label htmlFor="outerUOM">Outer UOM</Label>
                  <Select
                    value={formData.OuterUOM}
                    onValueChange={(value) => handleInputChange('OuterUOM', value)}
                  >
                    <SelectTrigger>
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="EA">Each (EA)</SelectItem>
                      <SelectItem value="BOX">Box (BOX)</SelectItem>
                      <SelectItem value="CASE">Case (CASE)</SelectItem>
                      <SelectItem value="PALLET">Pallet (PALLET)</SelectItem>
                    </SelectContent>
                  </Select>
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

          <Card>
            <CardHeader>
              <CardTitle>Metadata</CardTitle>
              <CardDescription>
                System information and timestamps
              </CardDescription>
            </CardHeader>
            <CardContent className="grid gap-4 text-sm">
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <span className="text-muted-foreground">Created By:</span> {originalSKU.CreatedBy}
                </div>
                <div>
                  <span className="text-muted-foreground">Created Time:</span>{' '}
                  {new Date(originalSKU.CreatedTime).toLocaleString()}
                </div>
              </div>
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <span className="text-muted-foreground">Modified By:</span> {originalSKU.ModifiedBy}
                </div>
                <div>
                  <span className="text-muted-foreground">Modified Time:</span>{' '}
                  {new Date(originalSKU.ModifiedTime).toLocaleString()}
                </div>
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
            <Button type="submit" disabled={saving}>
              <Save className="h-4 w-4 mr-2" />
              {saving ? 'Saving...' : 'Save Changes'}
            </Button>
          </div>
        </div>
      </form>
    </div>
  )
}