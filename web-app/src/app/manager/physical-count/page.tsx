"use client"

import { useState } from 'react'
import { useRouter } from 'next/navigation'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { ArrowLeft, Save, Plus, Minus } from 'lucide-react'

export default function PhysicalCountPage() {
  const router = useRouter()
  const [items, setItems] = useState([
    { id: 1, name: 'Product A', systemQty: 100, countedQty: 98, variance: -2 },
    { id: 2, name: 'Product B', systemQty: 50, countedQty: 52, variance: 2 },
    { id: 3, name: 'Product C', systemQty: 75, countedQty: 75, variance: 0 }
  ])

  const updateCount = (id: number, value: number) => {
    setItems(items.map(item =>
      item.id === id
        ? { ...item, countedQty: value, variance: value - item.systemQty }
        : item
    ))
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <Card>
        <CardContent className="pt-6">
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-4">
              <Button
                variant="outline"
                size="icon"
                onClick={() => router.back()}
              >
                <ArrowLeft className="w-4 h-4" />
              </Button>
              <div>
                <h1 className="text-2xl font-bold">Physical Count</h1>
                <p className="text-sm text-muted-foreground mt-1">Record physical inventory count</p>
              </div>
            </div>
            <Button>
              <Save className="w-4 h-4 mr-2" />
              Save Count
            </Button>
          </div>
        </CardContent>
      </Card>

      {/* Summary Cards */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        <Card>
          <CardContent className="pt-6">
            <p className="text-sm font-medium text-muted-foreground">Total Items</p>
            <p className="text-2xl font-bold">{items.length}</p>
          </CardContent>
        </Card>

        <Card>
          <CardContent className="pt-6">
            <p className="text-sm font-medium text-muted-foreground">System Quantity</p>
            <p className="text-2xl font-bold">{items.reduce((sum, item) => sum + item.systemQty, 0)}</p>
          </CardContent>
        </Card>

        <Card>
          <CardContent className="pt-6">
            <p className="text-sm font-medium text-muted-foreground">Counted Quantity</p>
            <p className="text-2xl font-bold">{items.reduce((sum, item) => sum + item.countedQty, 0)}</p>
          </CardContent>
        </Card>
      </div>

      {/* Count Table */}
      <Card>
        <CardHeader>
          <CardTitle>Inventory Items</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="space-y-4">
            {items.map((item) => (
              <Card key={item.id} className="border">
                <CardContent className="pt-4">
                  <div className="grid grid-cols-1 md:grid-cols-5 gap-4 items-center">
                    <div className="md:col-span-2">
                      <p className="font-medium">{item.name}</p>
                      <p className="text-sm text-muted-foreground">ID: {item.id}</p>
                    </div>

                    <div>
                      <p className="text-sm text-muted-foreground">System Qty</p>
                      <p className="font-semibold">{item.systemQty}</p>
                    </div>

                    <div>
                      <p className="text-sm text-muted-foreground">Counted Qty</p>
                      <div className="flex items-center gap-2">
                        <Button
                          variant="outline"
                          size="sm"
                          onClick={() => updateCount(item.id, Math.max(0, item.countedQty - 1))}
                        >
                          <Minus className="w-3 h-3" />
                        </Button>
                        <Input
                          type="number"
                          value={item.countedQty}
                          onChange={(e) => updateCount(item.id, parseInt(e.target.value) || 0)}
                          className="w-20 text-center"
                        />
                        <Button
                          variant="outline"
                          size="sm"
                          onClick={() => updateCount(item.id, item.countedQty + 1)}
                        >
                          <Plus className="w-3 h-3" />
                        </Button>
                      </div>
                    </div>

                    <div>
                      <p className="text-sm text-muted-foreground">Variance</p>
                      <p className={`font-semibold ${
                        item.variance > 0 ? 'text-green-600' :
                        item.variance < 0 ? 'text-red-600' :
                        'text-gray-600'
                      }`}>
                        {item.variance > 0 ? '+' : ''}{item.variance}
                      </p>
                    </div>
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
