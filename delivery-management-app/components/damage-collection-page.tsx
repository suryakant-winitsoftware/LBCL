"use client"

import { useState } from "react"
import { useRouter } from "next/navigation"
import { Search, Plus, X, Check } from "lucide-react"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { PageHeader } from "@/components/page-header"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select"
import { Dialog, DialogContent, DialogHeader, DialogTitle } from "@/components/ui/dialog"
import { RadioGroup, RadioGroupItem } from "@/components/ui/radio-group"
import { Label } from "@/components/ui/label"

interface DamageItem {
  id: string
  code: string
  description: string
  inventory: string
  type: string
  reason: string
  source: string
  qty: number
  image?: string
}

const availableItems = [
  {
    code: "5213",
    description: "Lion Large Beer can 625ml",
    inventory: "33212514121",
  },
  {
    code: "2456",
    description: "Lion Large Beer can 330ml",
    inventory: "33212514121",
  },
  {
    code: "4578",
    description: "Lion Large Beer bottle 625ml",
    inventory: "33212514121",
  },
  {
    code: "5468",
    description: "Lion Large Beer bottle 330ml",
    inventory: "33212514121",
  },
  {
    code: "6789",
    description: "Lion Strong Beer can 625ml",
    inventory: "33212514121",
  },
]

export function DamageCollectionPage() {
  const router = useRouter()
  const [items, setItems] = useState<DamageItem[]>([])
  const [showItemSelector, setShowItemSelector] = useState(false)
  const [showSuccessModal, setShowSuccessModal] = useState(false)
  const [selectedItemCode, setSelectedItemCode] = useState("")
  const [brandSearch, setBrandSearch] = useState("")
  const [itemSearch, setItemSearch] = useState("")

  const handleAddItem = () => {
    if (!selectedItemCode) return

    const selectedItem = availableItems.find((item) => item.code === selectedItemCode)
    if (!selectedItem) return

    const newItem: DamageItem = {
      id: Date.now().toString(),
      code: selectedItem.code,
      description: selectedItem.description,
      inventory: selectedItem.inventory,
      type: "Unsellable",
      reason: "Damage Supply Chain",
      source: "Warehouse",
      qty: 0,
    }

    setItems([...items, newItem])
    setShowItemSelector(false)
    setSelectedItemCode("")
  }

  const handleFinalize = () => {
    setShowSuccessModal(true)
  }

  const handleSuccessClose = () => {
    setShowSuccessModal(false)
    setItems([])
  }

  const updateItem = (id: string, field: keyof DamageItem, value: string | number) => {
    setItems(items.map((item) => (item.id === id ? { ...item, [field]: value } : item)))
  }

  return (
    <div className="min-h-screen bg-gray-50">
      {/* New Header */}
      <PageHeader title="Damage Collection & Scrapping" />

      {/* Search Section */}
      <div className="bg-white border-b border-gray-200 px-4 py-4">
        <div className="max-w-7xl mx-auto">
          <div className="flex flex-col sm:flex-row gap-3 items-stretch sm:items-center">
            <div className="flex-1 relative">
              <Input
                placeholder="Search by Brand"
                value={brandSearch}
                onChange={(e) => setBrandSearch(e.target.value)}
                className="pr-10 h-12"
              />
              <Search className="absolute right-3 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400" />
            </div>
            <div className="flex-1 relative">
              <Input
                placeholder="Search by Item Code/Description"
                value={itemSearch}
                onChange={(e) => setItemSearch(e.target.value)}
                className="pr-10 h-12"
              />
              <Search className="absolute right-3 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400" />
            </div>
            <Button
              onClick={items.length > 0 ? handleFinalize : undefined}
              disabled={items.length === 0}
              className="bg-[#A08B5C] hover:bg-[#8A7549] text-white h-12 px-8 whitespace-nowrap"
            >
              {items.length > 0 ? "Finalize" : "Done"}
            </Button>
          </div>
        </div>
      </div>

      {/* Main Content */}
      <main className="max-w-7xl mx-auto px-4 py-6">
        {items.length === 0 ? (
          <div className="flex items-center justify-center h-[60vh]">
            <p className="text-gray-400 text-lg">No items added yet. Click the + button to add items.</p>
          </div>
        ) : (
          <div className="space-y-6">
            {items.map((item) => (
              <div key={item.id} className="bg-white rounded-lg shadow-sm border border-gray-200 overflow-hidden">
                {/* Item Header */}
                <div className="px-4 py-3 border-b border-gray-200">
                  <h3 className="font-semibold text-gray-900">{item.description}</h3>
                </div>

                {/* Item Details Table */}
                <div className="overflow-x-auto">
                  <table className="w-full">
                    <thead className="bg-[#FFF8E7]">
                      <tr>
                        <th className="px-4 py-3 text-left text-sm font-semibold text-gray-700">
                          Item Code & Description
                        </th>
                        <th className="px-4 py-3 text-center text-sm font-semibold text-gray-700 w-32">Image</th>
                        <th className="px-4 py-3 text-center text-sm font-semibold text-gray-700 w-24">Qty</th>
                      </tr>
                    </thead>
                    <tbody>
                      <tr className="border-b border-gray-200">
                        <td className="px-4 py-4">
                          <div className="font-semibold text-gray-900">{item.description}</div>
                          <div className="text-sm text-gray-500">{item.code}</div>
                        </td>
                        <td className="px-4 py-4">
                          <div className="flex justify-center">
                            <div className="w-16 h-16 bg-gray-100 rounded flex items-center justify-center">
                              <div className="text-center">
                                <div className="text-xs text-gray-400">No</div>
                                <div className="text-xs text-gray-400">Image</div>
                              </div>
                            </div>
                          </div>
                        </td>
                        <td className="px-4 py-4">
                          <Input
                            type="number"
                            value={item.qty}
                            onChange={(e) => updateItem(item.id, "qty", Number.parseInt(e.target.value) || 0)}
                            className="w-20 text-center mx-auto"
                          />
                        </td>
                      </tr>
                    </tbody>
                  </table>
                </div>

                {/* Dropdowns Section */}
                <div className="grid grid-cols-1 md:grid-cols-3 border-t border-gray-200">
                  <div className="p-4 border-b md:border-b-0 md:border-r border-gray-200">
                    <label className="text-sm text-gray-600 mb-2 block">Type :</label>
                    <Select value={item.type} onValueChange={(value) => updateItem(item.id, "type", value)}>
                      <SelectTrigger className="w-full">
                        <SelectValue />
                      </SelectTrigger>
                      <SelectContent>
                        <SelectItem value="Unsellable">Unsellable</SelectItem>
                        <SelectItem value="Sellable">Sellable</SelectItem>
                      </SelectContent>
                    </Select>
                  </div>

                  <div className="p-4 border-b md:border-b-0 md:border-r border-gray-200">
                    <label className="text-sm text-gray-600 mb-2 block">Reason :</label>
                    <Select value={item.reason} onValueChange={(value) => updateItem(item.id, "reason", value)}>
                      <SelectTrigger className="w-full">
                        <SelectValue />
                      </SelectTrigger>
                      <SelectContent>
                        <SelectItem value="Damage Supply Chain">Damage Supply Chain</SelectItem>
                        <SelectItem value="Expired">Expired</SelectItem>
                        <SelectItem value="Quality Issue">Quality Issue</SelectItem>
                      </SelectContent>
                    </Select>
                  </div>

                  <div className="p-4">
                    <label className="text-sm text-gray-600 mb-2 block">Source :</label>
                    <Select value={item.source} onValueChange={(value) => updateItem(item.id, "source", value)}>
                      <SelectTrigger className="w-full">
                        <SelectValue />
                      </SelectTrigger>
                      <SelectContent>
                        <SelectItem value="Warehouse">Warehouse</SelectItem>
                        <SelectItem value="Prime move">Prime move</SelectItem>
                        <SelectItem value="RD truck">RD truck</SelectItem>
                      </SelectContent>
                    </Select>
                  </div>
                </div>
              </div>
            ))}
          </div>
        )}
      </main>

      {/* Floating Add Button */}
      <button
        onClick={() => setShowItemSelector(true)}
        className="fixed bottom-8 right-8 w-14 h-14 bg-[#A08B5C] hover:bg-[#8A7549] text-white rounded-full shadow-lg flex items-center justify-center transition-transform hover:scale-110 z-20"
      >
        <Plus className="w-6 h-6" />
      </button>

      {/* Item Selector Modal */}
      <Dialog open={showItemSelector} onOpenChange={setShowItemSelector}>
        <DialogContent className="max-w-2xl max-h-[80vh] p-0">
          <DialogHeader className="px-6 py-4 border-b border-gray-200">
            <div className="flex items-center justify-between">
              <DialogTitle className="text-xl font-bold">Select Item</DialogTitle>
              <button onClick={() => setShowItemSelector(false)} className="p-1 hover:bg-gray-100 rounded">
                <X className="w-5 h-5" />
              </button>
            </div>
          </DialogHeader>

          <div className="overflow-y-auto max-h-[60vh]">
            <div className="bg-[#FFF8E7] px-6 py-3 border-b border-gray-200">
              <h3 className="font-semibold text-gray-700">Item Code/Description</h3>
            </div>

            <RadioGroup value={selectedItemCode} onValueChange={setSelectedItemCode} className="p-0">
              {availableItems.map((item) => (
                <div
                  key={item.code}
                  className="flex items-center gap-4 px-6 py-4 border-b border-gray-200 hover:bg-gray-50 cursor-pointer"
                  onClick={() => setSelectedItemCode(item.code)}
                >
                  <RadioGroupItem value={item.code} id={item.code} className="border-[#A08B5C]" />
                  <Label htmlFor={item.code} className="flex-1 cursor-pointer">
                    <div className="font-semibold text-gray-900">{item.description}</div>
                    <div className="text-sm text-gray-500 mt-1">
                      {item.code} <span className="ml-4">Inv : {item.inventory}</span>
                    </div>
                  </Label>
                </div>
              ))}
            </RadioGroup>
          </div>

          <div className="p-4 border-t border-gray-200">
            <Button
              onClick={handleAddItem}
              disabled={!selectedItemCode}
              className="w-full bg-[#A08B5C] hover:bg-[#8A7549] text-white h-12 text-base font-semibold"
            >
              DONE
            </Button>
          </div>
        </DialogContent>
      </Dialog>

      {/* Success Modal */}
      <Dialog open={showSuccessModal} onOpenChange={setShowSuccessModal}>
        <DialogContent className="max-w-md p-0">
          <div className="relative">
            <button onClick={handleSuccessClose} className="absolute top-4 right-4 p-1 hover:bg-gray-100 rounded z-10">
              <X className="w-5 h-5" />
            </button>

            <div className="px-8 py-8 text-center">
              <h2 className="text-2xl font-bold mb-6">Sucessfull</h2>

              <div className="flex justify-center mb-6">
                <div className="w-24 h-24 bg-green-500 rounded-full flex items-center justify-center">
                  <Check className="w-12 h-12 text-white stroke-[3]" />
                </div>
              </div>

              <p className="text-gray-900 font-medium mb-2">Damage Collection & Scrapping</p>
              <p className="text-gray-900 font-medium mb-8">Created Successfully</p>

              <div className="grid grid-cols-2 gap-4">
                <Button variant="outline" className="h-12 text-base font-semibold border-gray-300 bg-transparent">
                  PRINT
                </Button>
                <Button className="h-12 text-base font-semibold bg-[#A08B5C] hover:bg-[#8A7549] text-white">
                  EMAIL
                </Button>
              </div>
            </div>
          </div>
        </DialogContent>
      </Dialog>
    </div>
  )
}
