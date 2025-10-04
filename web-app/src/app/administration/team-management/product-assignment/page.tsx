"use client"

import { useState } from "react"
import { EmployeeProductAttributes } from "@/components/admin/employees/employee-product-attributes"
import ProductAttributesWithHierarchyFilter from "@/components/common/ProductAttributesWithHierarchyFilter"
import ProductAttributes from "@/components/sku/ProductAttributes"
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Badge } from "@/components/ui/badge"
import { Button } from "@/components/ui/button"
import { Package2, Users, Layers, Save, RefreshCw } from "lucide-react"
import { useToast } from "@/components/ui/use-toast"

export default function ProductAssignmentPage() {
  const [selectedProducts, setSelectedProducts] = useState<any[]>([])
  const [selectedAttributes, setSelectedAttributes] = useState<any[]>([])
  const { toast } = useToast()

  const handleProductsChange = (products: any[]) => {
    console.log("Selected products from hierarchy filter:", products)
    setSelectedProducts(products)
  }

  const handleAttributesChange = (attributes: any[]) => {
    console.log("Selected attributes:", attributes)
    setSelectedAttributes(attributes)

    // Show feedback when attributes change
    if (attributes.length > 0) {
      const lastAttribute = attributes[attributes.length - 1]
      toast({
        title: "Attribute Selected",
        description: `${lastAttribute.type}: ${lastAttribute.value}`,
      })
    }
  }

  const handleSaveAssignment = () => {
    // Handle saving the assignment
    toast({
      title: "Assignment Saved",
      description: `Successfully saved ${selectedProducts.length} product assignments`,
    })
  }

  return (
    <div className="space-y-6">
      {/* Page Header */}
      <div className="flex flex-col gap-4 md:flex-row md:items-center md:justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Product Assignment</h1>
          <p className="text-muted-foreground">
            Manage employee product and attribute assignments using dynamic hierarchy selection
          </p>
        </div>
        <div className="flex gap-2">
          <Button variant="outline" size="sm">
            <RefreshCw className="h-4 w-4 mr-2" />
            Refresh
          </Button>
          <Button onClick={handleSaveAssignment} size="sm">
            <Save className="h-4 w-4 mr-2" />
            Save Assignment
          </Button>
        </div>
      </div>

      {/* Tabbed Interface for Different Assignment Methods */}
      <Tabs defaultValue="employee-attributes" className="w-full">
        <TabsList className="grid w-full max-w-lg grid-cols-3">
          <TabsTrigger value="employee-attributes" className="flex items-center gap-2">
            <Users className="h-4 w-4" />
            Employee
          </TabsTrigger>
          <TabsTrigger value="hierarchy-filter" className="flex items-center gap-2">
            <Package2 className="h-4 w-4" />
            Hierarchy Filter
          </TabsTrigger>
          <TabsTrigger value="dynamic-attributes" className="flex items-center gap-2">
            <Layers className="h-4 w-4" />
            Dynamic Attributes
          </TabsTrigger>
        </TabsList>

        {/* Employee Attributes Tab */}
        <TabsContent value="employee-attributes" className="space-y-4 mt-6">
          <EmployeeProductAttributes />
        </TabsContent>

        {/* Hierarchy Filter Tab */}
        <TabsContent value="hierarchy-filter" className="space-y-4 mt-6">
          <ProductAttributesWithHierarchyFilter
            onProductsChange={handleProductsChange}
            multiSelectProducts={true}
            showSelectedCount={true}
            enableProductSearch={true}
            productPageSize={100}
            title="Product Selection by SKU Group Hierarchy"
            description="Filter and select products based on the SKU group hierarchy. Products are filtered by their parent UID relationship."
          />

          {/* Display selected products summary */}
          {selectedProducts.length > 0 && (
            <Card>
              <CardHeader>
                <CardTitle className="text-lg">Selection Summary</CardTitle>
              </CardHeader>
              <CardContent>
                <div className="flex items-center justify-between">
                  <p className="text-sm font-medium">
                    {selectedProducts.length} product{selectedProducts.length !== 1 ? 's' : ''} selected for assignment
                  </p>
                  <Badge variant="secondary" className="ml-2">
                    {selectedProducts.length} Items
                  </Badge>
                </div>
              </CardContent>
            </Card>
          )}
        </TabsContent>

        {/* Dynamic Attributes Tab - Using the common ProductAttributes component */}
        <TabsContent value="dynamic-attributes" className="space-y-4 mt-6">
          <Card>
            <CardHeader>
              <CardTitle>Dynamic Product Attributes</CardTitle>
              <CardDescription>
                Select product attributes dynamically through the SKU hierarchy. This component supports unlimited hierarchy levels and adapts to your data structure.
              </CardDescription>
            </CardHeader>
            <CardContent>
              <ProductAttributes
                onAttributesChange={handleAttributesChange}
                fieldNamePattern="L{n}"
                enableMultiSelect={true}
                showLevelNumbers={true}
                initialMaxLevels={5}
                levelLabelGenerator={(level, typeName) => `${typeName} (Level ${level})`}
                disabled={false}
                allowDynamicLevelAddition={false}
              />
            </CardContent>
          </Card>

          {/* Display selected attributes */}
          {selectedAttributes.length > 0 && (
            <Card>
              <CardHeader>
                <CardTitle className="text-lg">Selected Attributes</CardTitle>
              </CardHeader>
              <CardContent>
                <div className="space-y-2">
                  {selectedAttributes.map((attr, index) => (
                    <div key={index} className="flex items-center gap-2">
                      <Badge variant="outline">Level {attr.level}</Badge>
                      <span className="text-sm">
                        <strong>{attr.type}:</strong> {attr.value} ({attr.code})
                      </span>
                    </div>
                  ))}
                </div>
              </CardContent>
            </Card>
          )}
        </TabsContent>
      </Tabs>
    </div>
  )
}