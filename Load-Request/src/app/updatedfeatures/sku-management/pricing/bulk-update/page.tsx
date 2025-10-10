'use client'

import { useState } from 'react'
import { useRouter } from 'next/navigation'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Textarea } from '@/components/ui/textarea'
import { ArrowLeft, Upload, Download, FileSpreadsheet, AlertCircle } from 'lucide-react'
import { useToast } from '@/components/ui/use-toast'
import { Alert, AlertDescription, AlertTitle } from '@/components/ui/alert'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table'

interface PreviewRow {
  skuCode: string
  skuName: string
  currentPrice: number
  newPrice: number
  change: number
  changePercent: number
}

export default function BulkUpdatePricingPage() {
  const router = useRouter()
  const { toast } = useToast()
  const [loading, setLoading] = useState(false)
  const [file, setFile] = useState<File | null>(null)
  const [updateType, setUpdateType] = useState<'replace' | 'percentage' | 'fixed'>('replace')
  const [updateValue, setUpdateValue] = useState('')
  const [priceList, setPriceList] = useState('')
  const [preview, setPreview] = useState<PreviewRow[]>([])
  const [showPreview, setShowPreview] = useState(false)

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files && e.target.files[0]) {
      setFile(e.target.files[0])
      // Reset preview when new file is selected
      setShowPreview(false)
      setPreview([])
    }
  }

  const handleDownloadTemplate = () => {
    // Create a CSV template
    const csvContent = `SKU Code,SKU Name,Current Price,New Price
PROD_001,Sample Product 1,100,120
PROD_002,Sample Product 2,200,250
PROD_003,Sample Product 3,150,180`
    
    const blob = new Blob([csvContent], { type: 'text/csv' })
    const url = window.URL.createObjectURL(blob)
    const a = document.createElement('a')
    a.href = url
    a.download = 'sku_price_update_template.csv'
    a.click()
    window.URL.revokeObjectURL(url)
    
    toast({
      title: 'Template Downloaded',
      description: 'Price update template has been downloaded'
    })
  }

  const handlePreview = async () => {
    if (!file && updateType === 'replace') {
      toast({
        title: 'Validation Error',
        description: 'Please select a file to upload',
        variant: 'destructive'
      })
      return
    }

    if ((updateType === 'percentage' || updateType === 'fixed') && !updateValue) {
      toast({
        title: 'Validation Error',
        description: 'Please enter an update value',
        variant: 'destructive'
      })
      return
    }

    // Simulate preview generation
    setLoading(true)
    try {
      // Mock preview data
      const mockPreview: PreviewRow[] = [
        {
          skuCode: 'Seeds_11-2953',
          skuName: 'Watermelon Seeds Farmley Standee Pouch 500g',
          currentPrice: 250,
          newPrice: updateType === 'percentage' ? 275 : updateType === 'fixed' ? 260 : 280,
          change: updateType === 'percentage' ? 25 : updateType === 'fixed' ? 10 : 30,
          changePercent: updateType === 'percentage' ? 10 : updateType === 'fixed' ? 4 : 12
        },
        {
          skuCode: 'Makhana_7-2745',
          skuName: 'Premium Makhana 100g',
          currentPrice: 180,
          newPrice: updateType === 'percentage' ? 198 : updateType === 'fixed' ? 190 : 200,
          change: updateType === 'percentage' ? 18 : updateType === 'fixed' ? 10 : 20,
          changePercent: updateType === 'percentage' ? 10 : updateType === 'fixed' ? 5.5 : 11.1
        }
      ]
      
      setPreview(mockPreview)
      setShowPreview(true)
    } catch (error) {
      toast({
        title: 'Error',
        description: 'Failed to generate preview',
        variant: 'destructive'
      })
    } finally {
      setLoading(false)
    }
  }

  const handleApplyUpdates = async () => {
    setLoading(true)
    try {
      // TODO: Implement bulk price update API call
      toast({
        title: 'Success',
        description: `${preview.length} prices updated successfully`
      })
      router.push('/updatedfeatures/sku-management/pricing/manage')
    } catch (error) {
      toast({
        title: 'Error',
        description: 'Failed to update prices',
        variant: 'destructive'
      })
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
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Bulk Update Pricing</h1>
          <p className="text-muted-foreground">
            Update multiple product prices at once
          </p>
        </div>
      </div>

      <Alert>
        <AlertCircle className="h-4 w-4" />
        <AlertTitle>Important</AlertTitle>
        <AlertDescription>
          Bulk price updates will affect all selected products immediately. 
          Please review the preview carefully before applying changes.
        </AlertDescription>
      </Alert>

      <div className="grid gap-6">
        <Card>
          <CardHeader>
            <CardTitle>Update Method</CardTitle>
            <CardDescription>
              Choose how you want to update prices
            </CardDescription>
          </CardHeader>
          <CardContent className="grid gap-4">
            <div className="space-y-2">
              <Label htmlFor="priceList">Target Price List</Label>
              <Select
                value={priceList}
                onValueChange={setPriceList}
              >
                <SelectTrigger>
                  <SelectValue placeholder="Select a price list" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="standard">Standard Price List</SelectItem>
                  <SelectItem value="promo">Promotional Price List</SelectItem>
                  <SelectItem value="wholesale">Wholesale Price List</SelectItem>
                </SelectContent>
              </Select>
            </div>

            <div className="space-y-2">
              <Label>Update Type</Label>
              <Select
                value={updateType}
                onValueChange={(value: any) => setUpdateType(value)}
              >
                <SelectTrigger>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="replace">Replace with File Upload</SelectItem>
                  <SelectItem value="percentage">Percentage Change</SelectItem>
                  <SelectItem value="fixed">Fixed Amount Change</SelectItem>
                </SelectContent>
              </Select>
            </div>

            {updateType === 'replace' ? (
              <div className="space-y-4">
                <div className="space-y-2">
                  <Label htmlFor="file">Upload Price File</Label>
                  <Input
                    id="file"
                    type="file"
                    accept=".csv,.xlsx"
                    onChange={handleFileChange}
                  />
                  <p className="text-sm text-muted-foreground">
                    Supported formats: CSV, Excel (.xlsx)
                  </p>
                </div>
                <Button
                  variant="outline"
                  onClick={handleDownloadTemplate}
                >
                  <FileSpreadsheet className="h-4 w-4 mr-2" />
                  Download Template
                </Button>
              </div>
            ) : (
              <div className="space-y-2">
                <Label htmlFor="updateValue">
                  {updateType === 'percentage' ? 'Percentage Change (%)' : 'Fixed Amount Change'}
                </Label>
                <Input
                  id="updateValue"
                  type="number"
                  value={updateValue}
                  onChange={(e) => setUpdateValue(e.target.value)}
                  placeholder={updateType === 'percentage' ? 'e.g., 10 for 10% increase' : 'e.g., 50 for ₹50 increase'}
                />
                <p className="text-sm text-muted-foreground">
                  Use negative values for price decreases
                </p>
              </div>
            )}
          </CardContent>
        </Card>

        {!showPreview ? (
          <div className="flex justify-end">
            <Button onClick={handlePreview} disabled={loading}>
              {loading ? 'Generating...' : 'Preview Changes'}
            </Button>
          </div>
        ) : (
          <>
            <Card>
              <CardHeader>
                <CardTitle>Price Update Preview</CardTitle>
                <CardDescription>
                  Review the changes before applying them
                </CardDescription>
              </CardHeader>
              <CardContent>
                <div className="rounded-md border">
                  <Table>
                    <TableHeader>
                      <TableRow>
                        <TableHead>SKU Code</TableHead>
                        <TableHead>Product Name</TableHead>
                        <TableHead className="text-right">Current Price</TableHead>
                        <TableHead className="text-right">New Price</TableHead>
                        <TableHead className="text-right">Change</TableHead>
                        <TableHead className="text-right">Change %</TableHead>
                      </TableRow>
                    </TableHeader>
                    <TableBody>
                      {preview.map((row) => (
                        <TableRow key={row.skuCode}>
                          <TableCell className="font-medium">{row.skuCode}</TableCell>
                          <TableCell>{row.skuName}</TableCell>
                          <TableCell className="text-right">₹{row.currentPrice}</TableCell>
                          <TableCell className="text-right font-medium">₹{row.newPrice}</TableCell>
                          <TableCell className={`text-right ${row.change > 0 ? 'text-green-600' : 'text-red-600'}`}>
                            {row.change > 0 ? '+' : ''}₹{row.change}
                          </TableCell>
                          <TableCell className={`text-right ${row.changePercent > 0 ? 'text-green-600' : 'text-red-600'}`}>
                            {row.changePercent > 0 ? '+' : ''}{row.changePercent}%
                          </TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                </div>
                
                <div className="mt-4 p-4 bg-gray-50 rounded-lg">
                  <div className="flex justify-between text-sm">
                    <span>Total Products: {preview.length}</span>
                    <span>Average Change: +8.5%</span>
                  </div>
                </div>
              </CardContent>
            </Card>

            <div className="flex justify-end gap-4">
              <Button
                variant="outline"
                onClick={() => setShowPreview(false)}
              >
                Back to Edit
              </Button>
              <Button onClick={handleApplyUpdates} disabled={loading}>
                <Upload className="h-4 w-4 mr-2" />
                {loading ? 'Applying...' : 'Apply Updates'}
              </Button>
            </div>
          </>
        )}
      </div>
    </div>
  )
}