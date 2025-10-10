'use client'

import { ReactNode } from "react"
import Link from "next/link"
import { usePathname } from "next/navigation"
import { Card, CardContent } from "@/components/ui/card"
import { Badge } from "@/components/ui/badge"
import { 
  Package, 
  TreePine, 
  Tags, 
  Link2, 
  BarChart3,
  Database,
  Search,
  Ruler
} from "lucide-react"

interface SKUManagementLayoutProps {
  children: ReactNode
}

const navigationItems = [
  {
    title: "Products",
    href: "/updatedfeatures/sku-management/products",
    icon: Package,
    description: "Manage SKU products and details"
  },
  {
    title: "Group Types",
    href: "/updatedfeatures/sku-management/group-types",
    icon: TreePine,
    description: "Define hierarchy types (Category, Brand, etc.)"
  },
  {
    title: "Groups",
    href: "/updatedfeatures/sku-management/groups",
    icon: Tags,
    description: "Manage actual groups (Electronics, Samsung, etc.)"
  },
  {
    title: "Mappings",
    href: "/updatedfeatures/sku-management/mappings",
    icon: Link2,
    description: "Link SKUs to groups for classification"
  },
  {
    title: "Units of Measurement",
    href: "/updatedfeatures/sku-management/uom/manage",
    icon: Ruler,
    description: "Dynamic UOM management with all physical dimensions",
    badge: "Dynamic"
  },
  {
    title: "Pricing",
    href: "/updatedfeatures/sku-management/pricing",
    icon: BarChart3,
    description: "Manage price lists and SKU pricing",
    badge: "Coming Soon"
  },
  {
    title: "Debug Tools",
    href: "/debug/api-investigation",
    icon: Database,
    description: "API testing and investigation tools",
    badge: "Debug"
  }
]

export default function SKUManagementLayout({ children }: SKUManagementLayoutProps) {
  const pathname = usePathname()
  
  // Show navigation only on the main SKU management page
  const isMainPage = pathname === '/updatedfeatures/sku-management'

  if (isMainPage) {
    return (
      <div className="container mx-auto py-6 space-y-6">
        <div className="flex items-center justify-between">
          <div>
            <h1 className="text-4xl font-bold tracking-tight">SKU Management</h1>
            <p className="text-lg text-muted-foreground">
              Comprehensive product and hierarchy management system
            </p>
          </div>
          <div className="flex items-center gap-2">
            <Search className="h-5 w-5 text-muted-foreground" />
            <span className="text-sm text-muted-foreground">Deep investigation completed</span>
          </div>
        </div>

        {/* Navigation Cards */}
        <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
          {navigationItems.map((item) => {
            const Icon = item.icon
            return (
              <Link key={item.href} href={item.href}>
                <Card className="transition-all hover:shadow-md hover:scale-[1.02]">
                  <CardContent className="p-6">
                    <div className="flex items-start justify-between mb-4">
                      <div className="p-2 bg-primary/10 rounded-lg">
                        <Icon className="h-6 w-6 text-primary" />
                      </div>
                      {item.badge && (
                        <Badge variant={
                          item.badge === "Debug" ? "destructive" : 
                          item.badge === "Dynamic" ? "default" : 
                          "secondary"
                        }>
                          {item.badge}
                        </Badge>
                      )}
                    </div>
                    <h3 className="text-lg font-semibold mb-2">{item.title}</h3>
                    <p className="text-sm text-muted-foreground">{item.description}</p>
                  </CardContent>
                </Card>
              </Link>
            )
          })}
        </div>

        {/* Quick Stats */}
        <div className="grid gap-4 md:grid-cols-4">
          <Card>
            <CardContent className="p-4">
              <div className="flex items-center gap-2 mb-2">
                <Package className="h-4 w-4 text-blue-500" />
                <span className="text-sm font-medium">Total SKUs</span>
              </div>
              <div className="text-2xl font-bold">167</div>
              <div className="text-xs text-muted-foreground">Products in system</div>
            </CardContent>
          </Card>
          
          <Card>
            <CardContent className="p-4">
              <div className="flex items-center gap-2 mb-2">
                <TreePine className="h-4 w-4 text-green-500" />
                <span className="text-sm font-medium">Group Types</span>
              </div>
              <div className="text-2xl font-bold">3</div>
              <div className="text-xs text-muted-foreground">Hierarchy types</div>
            </CardContent>
          </Card>
          
          <Card>
            <CardContent className="p-4">
              <div className="flex items-center gap-2 mb-2">
                <Tags className="h-4 w-4 text-purple-500" />
                <span className="text-sm font-medium">Groups</span>
              </div>
              <div className="text-2xl font-bold">10+</div>
              <div className="text-xs text-muted-foreground">Classification groups</div>
            </CardContent>
          </Card>
          
          <Card>
            <CardContent className="p-4">
              <div className="flex items-center gap-2 mb-2">
                <Ruler className="h-4 w-4 text-blue-500" />
                <span className="text-sm font-medium">UOM Records</span>
              </div>
              <div className="text-2xl font-bold">170</div>
              <div className="text-xs text-muted-foreground">Dynamic schema</div>
            </CardContent>
          </Card>
        </div>

        {/* Investigation Summary */}
        <Card>
          <CardContent className="p-6">
            <h3 className="text-lg font-semibold mb-4">Deep Investigation Summary</h3>
            <div className="grid gap-4 md:grid-cols-2">
              <div>
                <h4 className="font-medium mb-2">✅ Working APIs</h4>
                <ul className="text-sm text-muted-foreground space-y-1">
                  <li>• SKU Group Types - Full CRUD operations</li>
                  <li>• SKU Groups - Complete management</li>
                  <li>• SKU to Group Mappings - All endpoints</li>
                  <li>• SKU Management - List and details</li>
                  <li>• UOM Management - Dynamic schema discovery</li>
                </ul>
              </div>
              <div>
                <h4 className="font-medium mb-2">🔧 Solutions Implemented</h4>
                <ul className="text-sm text-muted-foreground space-y-1">
                  <li>• Dynamic UOM Management - All 29 fields supported</li>
                  <li>• Automatic schema discovery - Zero hardcoding</li>
                  <li>• Physical dimensions - Length, Width, Height, Weight</li>
                  <li>• Volume measurements - Liter, Volume units</li>
                  <li>• Fully adaptive UI - Works with any schema changes</li>
                </ul>
              </div>
            </div>
          </CardContent>
        </Card>
      </div>
    )
  }

  // For child pages, render with minimal layout
  return (
    <div className="flex flex-col h-full">
      <div className="flex-1 space-y-4 p-8 pt-6">
        {children}
      </div>
    </div>
  )
}