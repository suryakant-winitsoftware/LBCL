"use client"

import { useRouter } from 'next/navigation'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { ArrowLeft, Package, CheckCircle } from 'lucide-react'

export default function PickListViewPage() {
  const router = useRouter()

  const pickListItems = [
    { id: 1, product: 'Product A', sku: 'SKU-001', quantity: 50, location: 'A-01-01', picked: true },
    { id: 2, product: 'Product B', sku: 'SKU-002', quantity: 30, location: 'A-01-02', picked: true },
    { id: 3, product: 'Product C', sku: 'SKU-003', quantity: 25, location: 'B-02-01', picked: false },
    { id: 4, product: 'Product D', sku: 'SKU-004', quantity: 40, location: 'B-02-02', picked: false }
  ]

  return (
    <div className="space-y-6">
      <Card>
        <CardContent className="pt-6">
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-4">
              <Button variant="outline" size="icon" onClick={() => router.back()}>
                <ArrowLeft className="w-4 h-4" />
              </Button>
              <div>
                <h1 className="text-2xl font-bold">Pick List</h1>
                <p className="text-sm text-muted-foreground mt-1">Review items to be picked</p>
              </div>
            </div>
            <Button>Complete Picking</Button>
          </div>
        </CardContent>
      </Card>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        <Card>
          <CardContent className="pt-6">
            <div className="flex items-center gap-4">
              <div className="p-3 bg-blue-100 rounded-lg">
                <Package className="w-6 h-6 text-blue-600" />
              </div>
              <div>
                <p className="text-sm font-medium text-muted-foreground">Total Items</p>
                <p className="text-2xl font-bold">{pickListItems.length}</p>
              </div>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardContent className="pt-6">
            <div className="flex items-center gap-4">
              <div className="p-3 bg-green-100 rounded-lg">
                <CheckCircle className="w-6 h-6 text-green-600" />
              </div>
              <div>
                <p className="text-sm font-medium text-muted-foreground">Picked</p>
                <p className="text-2xl font-bold">{pickListItems.filter(i => i.picked).length}</p>
              </div>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardContent className="pt-6">
            <div className="flex items-center gap-4">
              <div className="p-3 bg-yellow-100 rounded-lg">
                <Package className="w-6 h-6 text-yellow-600" />
              </div>
              <div>
                <p className="text-sm font-medium text-muted-foreground">Pending</p>
                <p className="text-2xl font-bold">{pickListItems.filter(i => !i.picked).length}</p>
              </div>
            </div>
          </CardContent>
        </Card>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Items to Pick</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="space-y-3">
            {pickListItems.map((item) => (
              <Card key={item.id} className={`border ${item.picked ? 'bg-green-50' : ''}`}>
                <CardContent className="p-4">
                  <div className="flex items-center justify-between">
                    <div className="flex-1 grid grid-cols-1 md:grid-cols-4 gap-4">
                      <div>
                        <p className="text-sm text-muted-foreground">Product</p>
                        <p className="font-medium">{item.product}</p>
                        <p className="text-xs text-muted-foreground">{item.sku}</p>
                      </div>
                      <div>
                        <p className="text-sm text-muted-foreground">Quantity</p>
                        <p className="font-semibold">{item.quantity}</p>
                      </div>
                      <div>
                        <p className="text-sm text-muted-foreground">Location</p>
                        <p className="font-medium">{item.location}</p>
                      </div>
                      <div>
                        <p className="text-sm text-muted-foreground">Status</p>
                        <span className={`inline-flex px-2 py-1 text-xs rounded-full ${
                          item.picked ? 'bg-green-100 text-green-800' : 'bg-yellow-100 text-yellow-800'
                        }`}>
                          {item.picked ? 'Picked' : 'Pending'}
                        </span>
                      </div>
                    </div>
                    <Button variant={item.picked ? 'outline' : 'default'} size="sm" disabled={item.picked}>
                      {item.picked ? 'Picked' : 'Mark Picked'}
                    </Button>
                  </div>
                </CardContent>
              </Card>
            ))}
          </div>
        </CardContent>
      </Card>
    </div>
  )
}