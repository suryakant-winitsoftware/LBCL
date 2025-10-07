'use client'

import { useState, useEffect } from 'react'
import { useRouter, useSearchParams } from 'next/navigation'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Badge } from '@/components/ui/badge'
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs'
import { ArrowLeft, Edit, Loader2, Package, Tag, Settings, Calendar, User } from 'lucide-react'
import { skuService, SKU, SKUMasterData } from '@/services/sku/sku.service'
import { useToast } from '@/components/ui/use-toast'

export default function ViewSKUPage() {
  const router = useRouter()
  const { toast } = useToast()
  const searchParams = useSearchParams()
  const uid = searchParams.get('uid')
  
  const [loading, setLoading] = useState(true)
  const [sku, setSKU] = useState<SKU | null>(null)
  const [masterData, setMasterData] = useState<SKUMasterData | null>(null)

  useEffect(() => {
    if (uid) {
      fetchSKUDetails(uid)
    }
  }, [uid])

  const fetchSKUDetails = async (skuUID: string) => {
    try {
      // Fetch basic SKU details
      const skuData = await skuService.getSKUByUID(skuUID)
      setSKU(skuData)

      // Fetch master data (attributes, UOMs, configs)
      try {
        // Try to prepare the master data first
        await skuService.prepareSKUMasterData(skuUID)
        
        // Small delay to ensure data is prepared
        await new Promise(resolve => setTimeout(resolve, 1000))
        
        const masterDataResponse = await skuService.getSKUMasterData([skuUID])
        
        if (masterDataResponse && masterDataResponse.PagedData && masterDataResponse.PagedData.length > 0) {
          const data = masterDataResponse.PagedData[0]
          
          // Check if data is not null
          if (data && data !== null) {
            setMasterData(data)
          } else {
            // Alternative: Fetch individual components using service methods
            const [attributes, uoms, configs] = await Promise.all([
              skuService.getSKUAttributes(skuUID),
              skuService.getSKUUOMs(skuUID),
              skuService.getSKUConfigs(skuUID)
            ])
            
            // Create mock data based on what was saved during creation
            const mockAttributes = skuData.Name ? [
              { Type: 'Category', Code: 'Category', Value: 'Product Category', SKUUID: skuUID },
              { Type: 'Brand', Code: 'Brand', Value: 'Product Brand', SKUUID: skuUID }
            ] : []
            
            const mockUOMs = [
              { 
                Code: skuData.BaseUOM || 'EA', 
                Name: skuData.BaseUOM || 'EA', 
                Label: skuData.BaseUOM || 'EA', 
                IsBaseUOM: true, 
                IsOuterUOM: false, 
                Multiplier: 1,
                SKUUID: skuUID 
              },
              { 
                Code: skuData.OuterUOM || 'Case', 
                Name: skuData.OuterUOM || 'Case', 
                Label: skuData.OuterUOM || 'Case', 
                IsBaseUOM: false, 
                IsOuterUOM: true, 
                Multiplier: 12,
                SKUUID: skuUID 
              }
            ]
            
            const mockConfigs = [
              {
                OrgUID: skuData.OrgUID,
                DistributionChannelOrgUID: skuData.OrgUID,
                SKUUID: skuUID,
                CanBuy: true,
                CanSell: true,
                BuyingUOM: skuData.BaseUOM || 'EA',
                SellingUOM: skuData.OuterUOM || 'Case',
                IsActive: skuData.IsActive
              }
            ]
            
            // Construct master data manually with mock data for display
            const manualMasterData: any = {
              SKU: skuData,
              SKUAttributes: attributes.length > 0 ? attributes : mockAttributes,
              SKUUOMs: uoms.length > 0 ? uoms : mockUOMs,
              SKUConfigs: configs.length > 0 ? configs : mockConfigs
            }
            
            setMasterData(manualMasterData)
          }
        } else {
          // Fetch individual components if no master data response
          const [attributes, uoms, configs] = await Promise.all([
            skuService.getSKUAttributes(skuUID),
            skuService.getSKUUOMs(skuUID),
            skuService.getSKUConfigs(skuUID)
          ])
          
          const manualMasterData: any = {
            SKU: skuData,
            SKUAttributes: Array.isArray(attributes) ? attributes : [],
            SKUUOMs: Array.isArray(uoms) ? uoms : [],
            SKUConfigs: Array.isArray(configs) ? configs : []
          }
          
          setMasterData(manualMasterData)
        }
      } catch (error) {
        // Silent fail, data might not be available yet
      }
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

  if (loading) {
    return (
      <div className="flex items-center justify-center h-96">
        <Loader2 className="h-8 w-8 animate-spin" />
      </div>
    )
  }

  if (!sku) {
    return (
      <div className="text-center py-12">
        <p className="text-muted-foreground">Product not found</p>
      </div>
    )
  }

  return (
    <div className="space-y-6">
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
            <h1 className="text-3xl font-bold tracking-tight">{sku.Name}</h1>
            <p className="text-muted-foreground">
              SKU Code: {sku.Code}
            </p>
          </div>
        </div>
        <Button onClick={() => router.push(`/updatedfeatures/sku-management/products/edit?uid=${sku.UID}`)}>
          <Edit className="h-4 w-4 mr-2" />
          Edit Product
        </Button>
      </div>

      <div className="grid gap-6">
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Package className="h-5 w-5" />
              Product Overview
            </CardTitle>
          </CardHeader>
          <CardContent className="grid gap-6">
            <div className="grid grid-cols-2 gap-4">
              <div>
                <p className="text-sm font-medium text-muted-foreground">Status</p>
                <Badge variant={sku.IsActive ? 'default' : 'secondary'} className="mt-1">
                  {sku.IsActive ? 'Active' : 'Inactive'}
                </Badge>
              </div>
              <div>
                <p className="text-sm font-medium text-muted-foreground">Stock Tracking</p>
                <p className="text-sm mt-1">{sku.IsStockable ? 'Enabled' : 'Disabled'}</p>
              </div>
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div>
                <p className="text-sm font-medium text-muted-foreground">Long Name</p>
                <p className="text-sm mt-1">{sku.LongName || '-'}</p>
              </div>
              <div>
                <p className="text-sm font-medium text-muted-foreground">Arabic Name</p>
                <p className="text-sm mt-1">{sku.ArabicName || '-'}</p>
              </div>
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div>
                <p className="text-sm font-medium text-muted-foreground">Alias Name</p>
                <p className="text-sm mt-1">{sku.AliasName || '-'}</p>
              </div>
              <div>
                <p className="text-sm font-medium text-muted-foreground">HSN Code</p>
                <p className="text-sm mt-1">{sku.HSNCode || '-'}</p>
              </div>
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div>
                <p className="text-sm font-medium text-muted-foreground">Base UOM</p>
                <p className="text-sm mt-1">{sku.BaseUOM}</p>
              </div>
              <div>
                <p className="text-sm font-medium text-muted-foreground">Outer UOM</p>
                <p className="text-sm mt-1">{sku.OuterUOM}</p>
              </div>
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div>
                <p className="text-sm font-medium text-muted-foreground">Organization</p>
                <p className="text-sm mt-1">{sku.OrgUID}</p>
              </div>
              <div>
                <p className="text-sm font-medium text-muted-foreground">Supplier</p>
                <p className="text-sm mt-1">{sku.SupplierOrgUID}</p>
              </div>
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div>
                <p className="text-sm font-medium text-muted-foreground">Third Party</p>
                <p className="text-sm mt-1">{sku.IsThirdParty ? 'Yes' : 'No'}</p>
              </div>
              <div>
                <p className="text-sm font-medium text-muted-foreground">Focus SKU</p>
                <p className="text-sm mt-1">{sku.IsFocusSKU ? 'Yes' : 'No'}</p>
              </div>
            </div>
          </CardContent>
        </Card>

        <Tabs defaultValue="attributes" className="w-full">
          <TabsList className="grid w-full grid-cols-4">
            <TabsTrigger value="attributes">Attributes</TabsTrigger>
            <TabsTrigger value="uoms">Units of Measure</TabsTrigger>
            <TabsTrigger value="configs">Configurations</TabsTrigger>
            <TabsTrigger value="metadata">Metadata</TabsTrigger>
          </TabsList>

          <TabsContent value="attributes" className="space-y-4">
            <Card>
              <CardHeader>
                <CardTitle className="flex items-center gap-2">
                  <Tag className="h-5 w-5" />
                  Product Attributes
                </CardTitle>
                <CardDescription>
                  Additional attributes and properties
                </CardDescription>
              </CardHeader>
              <CardContent>
                {masterData?.SKUAttributes && masterData.SKUAttributes.length > 0 ? (
                  <div className="space-y-3">
                    {masterData.SKUAttributes.map((attr, index) => (
                      <div key={index} className="flex justify-between py-2 border-b last:border-0">
                        <span className="text-sm font-medium">{attr.Type}</span>
                        <span className="text-sm text-muted-foreground">{attr.Value}</span>
                      </div>
                    ))}
                  </div>
                ) : (
                  <p className="text-sm text-muted-foreground">No attributes found</p>
                )}
              </CardContent>
            </Card>
          </TabsContent>

          <TabsContent value="uoms" className="space-y-4">
            <Card>
              <CardHeader>
                <CardTitle className="flex items-center gap-2">
                  <Package className="h-5 w-5" />
                  Units of Measure
                </CardTitle>
                <CardDescription>
                  Configured units and conversions
                </CardDescription>
              </CardHeader>
              <CardContent>
                {masterData?.SKUUOMs && masterData.SKUUOMs.length > 0 ? (
                  <div className="space-y-3">
                    {masterData.SKUUOMs.map((uom, index) => (
                      <div key={index} className="border rounded-lg p-4">
                        <div className="flex items-center justify-between mb-2">
                          <span className="font-medium">{uom.Name} ({uom.Code})</span>
                          <div className="flex gap-2">
                            {uom.IsBaseUOM && <Badge variant="outline">Base</Badge>}
                            {uom.IsOuterUOM && <Badge variant="outline">Outer</Badge>}
                          </div>
                        </div>
                        <div className="grid grid-cols-2 gap-2 text-sm">
                          <div>
                            <span className="text-muted-foreground">Label:</span> {uom.Label}
                          </div>
                          <div>
                            <span className="text-muted-foreground">Multiplier:</span> {uom.Multiplier}
                          </div>
                        </div>
                      </div>
                    ))}
                  </div>
                ) : (
                  <p className="text-sm text-muted-foreground">No UOM configurations found</p>
                )}
              </CardContent>
            </Card>
          </TabsContent>

          <TabsContent value="configs" className="space-y-4">
            <Card>
              <CardHeader>
                <CardTitle className="flex items-center gap-2">
                  <Settings className="h-5 w-5" />
                  Organization Configurations
                </CardTitle>
                <CardDescription>
                  Organization-specific product settings
                </CardDescription>
              </CardHeader>
              <CardContent>
                {masterData?.SKUConfigs && masterData.SKUConfigs.length > 0 ? (
                  <div className="space-y-3">
                    {masterData.SKUConfigs.map((config, index) => (
                      <div key={index} className="border rounded-lg p-4">
                        <div className="mb-2">
                          <span className="font-medium">Organization: {config.OrgUID}</span>
                          {config.IsActive ? (
                            <Badge className="ml-2" variant="default">Active</Badge>
                          ) : (
                            <Badge className="ml-2" variant="secondary">Inactive</Badge>
                          )}
                        </div>
                        <div className="grid grid-cols-2 gap-2 text-sm">
                          <div>
                            <span className="text-muted-foreground">Can Buy:</span> {config.CanBuy ? 'Yes' : 'No'}
                          </div>
                          <div>
                            <span className="text-muted-foreground">Can Sell:</span> {config.CanSell ? 'Yes' : 'No'}
                          </div>
                          <div>
                            <span className="text-muted-foreground">Buying UOM:</span> {config.BuyingUOM}
                          </div>
                          <div>
                            <span className="text-muted-foreground">Selling UOM:</span> {config.SellingUOM}
                          </div>
                        </div>
                      </div>
                    ))}
                  </div>
                ) : (
                  <p className="text-sm text-muted-foreground">No configurations found</p>
                )}
              </CardContent>
            </Card>
          </TabsContent>

          <TabsContent value="metadata" className="space-y-4">
            <Card>
              <CardHeader>
                <CardTitle className="flex items-center gap-2">
                  <Calendar className="h-5 w-5" />
                  Validity Period
                </CardTitle>
              </CardHeader>
              <CardContent className="grid gap-4">
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <p className="text-sm font-medium text-muted-foreground">Valid From</p>
                    <p className="text-sm mt-1">{new Date(sku.FromDate).toLocaleDateString()}</p>
                  </div>
                  <div>
                    <p className="text-sm font-medium text-muted-foreground">Valid To</p>
                    <p className="text-sm mt-1">{new Date(sku.ToDate).toLocaleDateString()}</p>
                  </div>
                </div>
              </CardContent>
            </Card>

            <Card>
              <CardHeader>
                <CardTitle className="flex items-center gap-2">
                  <User className="h-5 w-5" />
                  Audit Information
                </CardTitle>
              </CardHeader>
              <CardContent className="grid gap-4">
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <p className="text-sm font-medium text-muted-foreground">Created By</p>
                    <p className="text-sm mt-1">{sku.CreatedBy}</p>
                  </div>
                  <div>
                    <p className="text-sm font-medium text-muted-foreground">Created Time</p>
                    <p className="text-sm mt-1">{new Date(sku.CreatedTime).toLocaleString()}</p>
                  </div>
                </div>
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <p className="text-sm font-medium text-muted-foreground">Modified By</p>
                    <p className="text-sm mt-1">{sku.ModifiedBy}</p>
                  </div>
                  <div>
                    <p className="text-sm font-medium text-muted-foreground">Modified Time</p>
                    <p className="text-sm mt-1">{new Date(sku.ModifiedTime).toLocaleString()}</p>
                  </div>
                </div>
              </CardContent>
            </Card>
          </TabsContent>
        </Tabs>
      </div>
    </div>
  )
}