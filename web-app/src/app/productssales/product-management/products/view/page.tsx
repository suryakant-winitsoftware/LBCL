'use client'

import { useState, useEffect, useCallback } from 'react'
import { useRouter, useSearchParams } from 'next/navigation'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Badge } from '@/components/ui/badge'
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs'
import { Skeleton } from '@/components/ui/skeleton'
import { ArrowLeft, Package, Tag, Settings, Calendar, User, Image as ImageIcon, Box, Barcode, Building2, Hash, ShoppingCart, Truck, Globe, Activity, Clock, Shield, X, ChevronLeft, ChevronRight, Expand } from 'lucide-react'
import { skuService, SKU, SKUMasterData } from '@/services/sku/sku.service'
import { useToast } from '@/components/ui/use-toast'
import { formatDateToDayMonthYear } from '@/utils/date-formatter'

export default function ViewSKUPage() {
  const router = useRouter()
  const { toast } = useToast()
  const searchParams = useSearchParams()
  const uid = searchParams.get('uid')
  
  const [loading, setLoading] = useState(true)
  const [sku, setSKU] = useState<SKU | null>(null)
  const [masterData, setMasterData] = useState<SKUMasterData | null>(null)
  const [showImageModal, setShowImageModal] = useState(false)
  const [currentImageIndex, setCurrentImageIndex] = useState(0)

  // Image navigation handlers
  const handlePreviousImage = useCallback(() => {
    if (masterData?.FileSysList) {
      setCurrentImageIndex((prev) => 
        prev === 0 ? masterData.FileSysList!.length - 1 : prev - 1
      );
    }
  }, [masterData]);

  const handleNextImage = useCallback(() => {
    if (masterData?.FileSysList) {
      setCurrentImageIndex((prev) => 
        prev === masterData.FileSysList!.length - 1 ? 0 : prev + 1
      );
    }
  }, [masterData]);

  const openImageModal = useCallback((index: number = 0) => {
    setCurrentImageIndex(index);
    setShowImageModal(true);
  }, []);

  // Keyboard navigation for modal
  const handleKeyDown = useCallback((e: KeyboardEvent) => {
    if (!showImageModal) return;
    
    if (e.key === 'Escape') {
      setShowImageModal(false);
    } else if (e.key === 'ArrowLeft') {
      handlePreviousImage();
    } else if (e.key === 'ArrowRight') {
      handleNextImage();
    }
  }, [showImageModal, handlePreviousImage, handleNextImage]);

  useEffect(() => {
    if (uid) {
      fetchSKUDetails(uid)
    }
  }, [uid])

  useEffect(() => {
    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, [handleKeyDown])

  const fetchSKUDetails = async (skuUID: string) => {
    try {
      // Fetch SKU master data using the new API
      const masterDataResponse = await skuService.getSKUMasterByUID(skuUID)
      
      if (masterDataResponse) {
        // Set SKU data
        if (masterDataResponse.SKU) {
          setSKU(masterDataResponse.SKU)
        }
        
        // Set master data with all components
        setMasterData(masterDataResponse)
      } else {
        // Fallback: Fetch basic SKU details if master API fails
        const skuData = await skuService.getSKUByUID(skuUID)
        setSKU(skuData)
        
        // Try to fetch individual components
        try {
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
        } catch (error) {
          // If individual fetches fail, set minimal master data
          setMasterData({
            SKU: skuData,
            SKUAttributes: [],
            SKUUOMs: [],
            SKUConfigs: []
          })
        }
      }
    } catch (error) {
      // If master API fails, try fallback approach
      try {
        const skuData = await skuService.getSKUByUID(skuUID)
        setSKU(skuData)
        
        setMasterData({
          SKU: skuData,
          SKUAttributes: [],
          SKUUOMs: [],
          SKUConfigs: []
        })
      } catch (fallbackError) {
        toast({
          title: 'Error',
          description: 'Failed to fetch product details',
          variant: 'destructive'
        })
        console.error('Error fetching SKU:', fallbackError)
      }
    } finally {
      setLoading(false)
    }
  }

  if (loading) {
    return (
      <div className="space-y-6">
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-4">
            <Skeleton className="h-10 w-10" />
            <div>
              <Skeleton className="h-8 w-64 mb-2" />
              <Skeleton className="h-4 w-32" />
            </div>
          </div>
          <Skeleton className="h-10 w-32" />
        </div>
        
        <Card>
          <CardHeader>
            <Skeleton className="h-6 w-48 mb-2" />
          </CardHeader>
          <CardContent>
            <div className="grid grid-cols-2 gap-4">
              <Skeleton className="h-20 w-full" />
              <Skeleton className="h-20 w-full" />
              <Skeleton className="h-20 w-full" />
              <Skeleton className="h-20 w-full" />
            </div>
          </CardContent>
        </Card>
        
        <Card>
          <CardHeader>
            <Skeleton className="h-10 w-full" />
          </CardHeader>
          <CardContent>
            <div className="space-y-4">
              <Skeleton className="h-32 w-full" />
              <Skeleton className="h-32 w-full" />
              <Skeleton className="h-32 w-full" />
            </div>
          </CardContent>
        </Card>
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

  // Get default image for display
  const defaultImage = masterData?.FileSysList?.find(file => file.IsDefault) || masterData?.FileSysList?.[0];
  const baseUrl = 'http://localhost:8000'; // Update with your actual API base URL

  return (
    <>
    <div className="space-y-6">
      {/* Header Section */}
      <div className="bg-gradient-to-r from-blue-50 to-indigo-50 dark:from-gray-800 dark:to-gray-900 rounded-xl p-6">
        <div className="flex items-center gap-4">
          <Button
            variant="ghost"
            size="icon"
            className="hover:bg-white/50 dark:hover:bg-gray-700/50"
            onClick={() => router.back()}
          >
            <ArrowLeft className="h-4 w-4" />
          </Button>
          <div className="flex-1">
            <div>
              <h1 className="text-2xl font-bold text-gray-900 dark:text-white">{sku.Name}</h1>
              <div className="flex items-center gap-4 mt-1">
                <span className="text-sm text-gray-600 dark:text-gray-400 font-mono bg-gray-100 dark:bg-gray-800 px-2 py-0.5 rounded">
                  SKU: {sku.Code}
                </span>
                <Badge variant={sku.IsActive ? 'default' : 'secondary'} className="font-medium">
                  {sku.IsActive ? 'Active' : 'Inactive'}
                </Badge>
              </div>
            </div>
          </div>
        </div>
      </div>

      <div className="space-y-6">
        {/* Product Details with Image */}
        <Card className="border">
          <CardHeader className="bg-gradient-to-r from-gray-50 to-white dark:from-gray-900 dark:to-gray-800 border-b">
            <div className="flex items-start justify-between">
              <CardTitle className="text-lg">
                Product Information
              </CardTitle>
              {/* Compact Image Display - Clickable */}
              {masterData?.FileSysList && masterData.FileSysList.length > 0 && defaultImage && (
                <div className="flex items-center gap-3">
                  <button
                    onClick={() => openImageModal(0)}
                    className="relative w-16 h-16 rounded border overflow-hidden bg-white dark:bg-gray-800 hover:opacity-80 transition-opacity group"
                  >
                    <img
                      src={`${baseUrl}/${defaultImage.RelativePath}`}
                      alt={sku.Name}
                      className="w-full h-full object-contain p-1"
                      onError={(e) => {
                        const target = e.target as HTMLImageElement;
                        target.style.display = 'none';
                      }}
                    />
                    <div className="absolute inset-0 bg-black/0 group-hover:bg-black/20 transition-colors flex items-center justify-center">
                      <Expand className="h-4 w-4 text-white opacity-0 group-hover:opacity-100 transition-opacity" />
                    </div>
                  </button>
                  {masterData.FileSysList.length > 1 && (
                    <span className="text-xs text-gray-500 dark:text-gray-400">
                      +{masterData.FileSysList.length - 1} images
                    </span>
                  )}
                </div>
              )}
            </div>
          </CardHeader>
          <CardContent className="p-6">
            <div className="grid grid-cols-2 md:grid-cols-3 gap-4">
              {/* Core Information */}
              <div>
                <p className="text-sm font-medium text-muted-foreground">Status</p>
                <Badge variant={sku.IsActive ? 'default' : 'secondary'} className="mt-1">
                  {sku.IsActive ? 'Active' : 'Inactive'}
                </Badge>
              </div>
              
              {sku.ParentUID && (
                <div>
                  <p className="text-sm font-medium text-muted-foreground">SKU Group</p>
                  <p className="text-sm mt-1 font-mono">{sku.ParentUID}</p>
                </div>
              )}

              {sku.OrgUID && (
                <div>
                  <p className="text-sm font-medium text-muted-foreground">Organization</p>
                  <p className="text-sm mt-1">{sku.OrgUID}</p>
                </div>
              )}

              {/* Stock and Type Information */}
              {sku.IsStockable !== undefined && (
                <div>
                  <p className="text-sm font-medium text-muted-foreground">Stock Tracking</p>
                  <p className="text-sm mt-1">{sku.IsStockable ? 'Enabled' : 'Disabled'}</p>
                </div>
              )}

              {sku.IsThirdParty !== undefined && (
                <div>
                  <p className="text-sm font-medium text-muted-foreground">Third Party</p>
                  <p className="text-sm mt-1">{sku.IsThirdParty ? 'Yes' : 'No'}</p>
                </div>
              )}
              
              {sku.IsFocusSKU !== undefined && (
                <div>
                  <p className="text-sm font-medium text-muted-foreground">Focus SKU</p>
                  <p className="text-sm mt-1">{sku.IsFocusSKU ? 'Yes' : 'No'}</p>
                </div>
              )}

              {/* Date Information */}
              {sku.FromDate && (
                <div>
                  <p className="text-sm font-medium text-muted-foreground">Valid From</p>
                  <p className="text-sm mt-1">{formatDateToDayMonthYear(sku.FromDate)}</p>
                </div>
              )}
              
              {sku.ToDate && (
                <div>
                  <p className="text-sm font-medium text-muted-foreground">Valid Until</p>
                  <p className="text-sm mt-1">{formatDateToDayMonthYear(sku.ToDate)}</p>
                </div>
              )}

              {/* Name Variants */}
              {sku.LongName && (
                <div>
                  <p className="text-sm font-medium text-muted-foreground">Long Name</p>
                  <p className="text-sm mt-1">{sku.LongName}</p>
                </div>
              )}
              
              {sku.ArabicName && (
                <div>
                  <p className="text-sm font-medium text-muted-foreground">Arabic Name</p>
                  <p className="text-sm mt-1">{sku.ArabicName}</p>
                </div>
              )}

              {sku.AliasName && (
                <div>
                  <p className="text-sm font-medium text-muted-foreground">Alias Name</p>
                  <p className="text-sm mt-1">{sku.AliasName}</p>
                </div>
              )}

              {/* UOM Information */}
              {sku.BaseUOM && (
                <div>
                  <p className="text-sm font-medium text-muted-foreground">Base UOM</p>
                  <p className="text-sm mt-1">{sku.BaseUOM}</p>
                </div>
              )}
              
              {sku.OuterUOM && (
                <div>
                  <p className="text-sm font-medium text-muted-foreground">Outer UOM</p>
                  <p className="text-sm mt-1">{sku.OuterUOM}</p>
                </div>
              )}

              {/* Additional Codes */}
              {sku.HSNCode && (
                <div>
                  <p className="text-sm font-medium text-muted-foreground">HSN Code</p>
                  <p className="text-sm mt-1">{sku.HSNCode}</p>
                </div>
              )}

              {sku.ProductCategoryId !== undefined && sku.ProductCategoryId !== 0 && (
                <div>
                  <p className="text-sm font-medium text-muted-foreground">Category ID</p>
                  <p className="text-sm mt-1">{sku.ProductCategoryId}</p>
                </div>
              )}
            </div>
          </CardContent>
        </Card>

        <Tabs defaultValue="attributes" className="w-full">
          <TabsList className="grid w-full grid-cols-3 bg-gray-100/50 dark:bg-gray-800/50 p-1 rounded-lg">
            <TabsTrigger value="attributes" className="data-[state=active]:bg-white dark:data-[state=active]:bg-gray-900 ">
              <Tag className="h-4 w-4 mr-2" />
              Attributes
            </TabsTrigger>
            <TabsTrigger value="uoms" className="data-[state=active]:bg-white dark:data-[state=active]:bg-gray-900 ">
              <Package className="h-4 w-4 mr-2" />
              Units
            </TabsTrigger>
            {/* <TabsTrigger value="configs" className="data-[state=active]:bg-white dark:data-[state=active]:bg-gray-900 ">
              <Settings className="h-4 w-4 mr-2" />
              Configs
            </TabsTrigger> */}
            <TabsTrigger value="metadata" className="data-[state=active]:bg-white dark:data-[state=active]:bg-gray-900 ">
              <User className="h-4 w-4 mr-2" />
              Metadata
            </TabsTrigger>
          </TabsList>

          <TabsContent value="attributes" className="space-y-4">
            <Card className="border">
              <CardHeader className="bg-gradient-to-r from-gray-50 to-white dark:from-gray-900 dark:to-gray-800">
                <CardTitle className="text-lg">
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
            <Card className="border">
              <CardHeader className="bg-gradient-to-r from-gray-50 to-white dark:from-gray-900 dark:to-gray-800">
                <CardTitle className="text-lg">
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
                          {uom.Barcodes && (
                            <div className="col-span-2">
                              <span className="text-muted-foreground">Barcode:</span> {uom.Barcodes}
                            </div>
                          )}
                          <div>
                            <span className="text-muted-foreground">Label:</span> {uom.Label}
                          </div>
                          <div>
                            <span className="text-muted-foreground">Multiplier:</span> {uom.Multiplier}
                          </div>
                          {(uom.Length !== undefined && uom.Length !== 0) && (
                            <div>
                              <span className="text-muted-foreground">Length:</span> {uom.Length}
                            </div>
                          )}
                          {(uom.Width !== undefined && uom.Width !== 0) && (
                            <div>
                              <span className="text-muted-foreground">Width:</span> {uom.Width}
                            </div>
                          )}
                          {(uom.Height !== undefined && uom.Height !== 0) && (
                            <div>
                              <span className="text-muted-foreground">Height:</span> {uom.Height}
                            </div>
                          )}
                          {(uom.Weight !== undefined && uom.Weight !== 0) && (
                            <div>
                              <span className="text-muted-foreground">Weight:</span> {uom.Weight}
                            </div>
                          )}
                          {(uom.Volume !== undefined && uom.Volume !== 0) && (
                            <div>
                              <span className="text-muted-foreground">Volume:</span> {uom.Volume}
                            </div>
                          )}
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

          {/* <TabsContent value="configs" className="space-y-4">
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
          </TabsContent> */}

          <TabsContent value="metadata" className="space-y-4">
            {/* Only show validity period if dates exist */}
            {(sku.FromDate || sku.ToDate) && (
              <Card>
                <CardHeader>
                  <CardTitle className="flex items-center gap-2">
                    <Calendar className="h-5 w-5" />
                    Validity Period
                  </CardTitle>
                </CardHeader>
                <CardContent className="grid gap-4">
                  <div className="grid grid-cols-2 gap-4">
                    {sku.FromDate && (
                      <div>
                        <p className="text-sm font-medium text-muted-foreground">Valid From</p>
                        <p className="text-sm mt-1">{formatDateToDayMonthYear(sku.FromDate)}</p>
                      </div>
                    )}
                    {sku.ToDate && (
                      <div>
                        <p className="text-sm font-medium text-muted-foreground">Valid To</p>
                        <p className="text-sm mt-1">{formatDateToDayMonthYear(sku.ToDate)}</p>
                      </div>
                    )}
                  </div>
                </CardContent>
              </Card>
            )}

            {/* Only show audit info if it exists */}
            {(sku.CreatedBy || sku.ModifiedBy) && (
              <Card>
                <CardHeader>
                  <CardTitle className="flex items-center gap-2">
                    <User className="h-5 w-5" />
                    Audit Information
                  </CardTitle>
                </CardHeader>
                <CardContent className="grid gap-4">
                  <div className="grid grid-cols-2 gap-4">
                    {sku.CreatedBy && (
                      <div>
                        <p className="text-sm font-medium text-muted-foreground">Created By</p>
                        <p className="text-sm mt-1">{sku.CreatedBy}</p>
                      </div>
                    )}
                    {sku.CreatedTime && (
                      <div>
                        <p className="text-sm font-medium text-muted-foreground">Created Time</p>
                        <p className="text-sm mt-1">{new Date(sku.CreatedTime).toLocaleString()}</p>
                      </div>
                    )}
                    {sku.ModifiedBy && (
                      <div>
                        <p className="text-sm font-medium text-muted-foreground">Modified By</p>
                        <p className="text-sm mt-1">{sku.ModifiedBy}</p>
                      </div>
                    )}
                    {sku.ModifiedTime && (
                      <div>
                        <p className="text-sm font-medium text-muted-foreground">Modified Time</p>
                        <p className="text-sm mt-1">{new Date(sku.ModifiedTime).toLocaleString()}</p>
                      </div>
                    )}
                  </div>
                </CardContent>
              </Card>
            )}
          </TabsContent>
        </Tabs>
      </div>
    </div>

    {/* Image Modal */}
    {showImageModal && masterData?.FileSysList && masterData.FileSysList.length > 0 && (
      <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/80 backdrop-blur-sm">
        <div className="relative w-full max-w-4xl mx-4">
          {/* Close button */}
          <button
            onClick={() => setShowImageModal(false)}
            className="absolute -top-12 right-0 text-white hover:text-gray-300 transition-colors"
          >
            <X className="h-8 w-8" />
          </button>

          {/* Main image display */}
          <div className="relative bg-white dark:bg-gray-900 rounded-lg overflow-hidden">
            <div className="aspect-square md:aspect-video flex items-center justify-center bg-gray-100 dark:bg-gray-800">
              <img
                src={`${baseUrl}/${masterData.FileSysList[currentImageIndex].RelativePath}`}
                alt={`${sku.Name} - Image ${currentImageIndex + 1}`}
                className="max-w-full max-h-full object-contain p-4"
                onError={(e) => {
                  const target = e.target as HTMLImageElement;
                  target.src = '';
                  target.alt = 'Image failed to load';
                }}
              />
            </div>

            {/* Navigation buttons */}
            {masterData.FileSysList.length > 1 && (
              <>
                <button
                  onClick={handlePreviousImage}
                  className="absolute left-4 top-1/2 -translate-y-1/2 bg-white/90 dark:bg-gray-800/90 hover:bg-white dark:hover:bg-gray-800 rounded-full p-2 transition-colors"
                >
                  <ChevronLeft className="h-6 w-6" />
                </button>
                <button
                  onClick={handleNextImage}
                  className="absolute right-4 top-1/2 -translate-y-1/2 bg-white/90 dark:bg-gray-800/90 hover:bg-white dark:hover:bg-gray-800 rounded-full p-2 transition-colors"
                >
                  <ChevronRight className="h-6 w-6" />
                </button>
              </>
            )}

            {/* Image counter */}
            <div className="absolute bottom-4 left-1/2 -translate-x-1/2 bg-black/50 text-white px-3 py-1 rounded-full text-sm">
              {currentImageIndex + 1} / {masterData.FileSysList.length}
            </div>
          </div>

          {/* Thumbnail navigation */}
          {masterData.FileSysList.length > 1 && (
            <div className="mt-4 flex gap-2 justify-center overflow-x-auto pb-2">
              {masterData.FileSysList.map((file, index) => (
                <button
                  key={file.UID}
                  onClick={() => setCurrentImageIndex(index)}
                  className={`w-16 h-16 rounded border-2 overflow-hidden flex-shrink-0 transition-all ${
                    index === currentImageIndex
                      ? 'border-blue-500 scale-110'
                      : 'border-gray-300 dark:border-gray-600 hover:border-blue-400'
                  }`}
                >
                  <img
                    src={`${baseUrl}/${file.RelativePath}`}
                    alt={`Thumbnail ${index + 1}`}
                    className="w-full h-full object-cover"
                  />
                </button>
              ))}
            </div>
          )}
        </div>
      </div>
    )}
    </>
  )
}