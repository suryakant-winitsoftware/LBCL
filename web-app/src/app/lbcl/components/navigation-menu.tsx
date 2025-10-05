"use client"

import { useState } from "react"
import { useRouter, usePathname } from "next/navigation"
import { Sheet, SheetContent, SheetTrigger, SheetTitle } from "@/components/ui/sheet"
import {
  Menu,
  Home,
  Package,
  History,
  Clock,
  FileText,
  Truck,
  FileCheck,
  AlertTriangle,
  ClipboardCheck,
  ChevronDown,
  ChevronRight,
} from "lucide-react"

interface NavigationMenuProps {
  className?: string
}

interface MenuItem {
  title: string
  icon: any
  path?: string
  description: string
  children?: MenuItem[]
}

export function NavigationMenu({ className }: NavigationMenuProps) {
  const [open, setOpen] = useState(false)
  const [expandedItems, setExpandedItems] = useState<string[]>([])
  const router = useRouter()
  const pathname = usePathname()

  const menuItems: MenuItem[] = [
    {
      title: "Dashboard",
      icon: Home,
      path: "/lbcl/dashboard",
      description: "Main menu",
    },
    {
      title: "Delivery Plan Activity Log Report",
      icon: FileText,
      path: "/lbcl/delivery-plans",
      description: "View and manage delivery plans and activity logs",
    },
    {
      title: "Manage Agent Stock Receiving",
      icon: Package,
      description: "Manage and track agent stock inventory",
      children: [
        {
          title: "Stock Receiving",
          icon: Package,
          path: "/lbcl/stock-receiving",
          description: "Manage stock receiving",
        },
        {
          title: "Stock History",
          icon: History,
          path: "/lbcl/stock-receiving/history",
          description: "View stock history",
        },
        {
          title: "Timeline Stamps",
          icon: Clock,
          path: "/lbcl/stock-receiving/timeline",
          description: "View timeline stamps",
        },
      ],
    },
    {
      title: "RD Truck Loading",
      icon: Truck,
      description: "View truck loading list and details",
      children: [
        {
          title: "Truck Loading List",
          icon: Truck,
          path: "/lbcl/truck-loading",
          description: "View truck loading list",
        },
        {
          title: "Loading Request",
          icon: FileCheck,
          path: "/lbcl/truck-loading/request",
          description: "Create and manage loading requests",
        },
        {
          title: "Load Collection",
          icon: Package,
          path: "/lbcl/truck-loading/collection",
          description: "Collect loaded items",
        },
        {
          title: "Activity Log",
          icon: Clock,
          path: "/lbcl/truck-loading/activity-log",
          description: "View activity log",
        },
      ],
    },
    {
      title: "Empties Stock Receiving",
      icon: Package,
      description: "Manage empties stock receiving and physical count",
      children: [
        {
          title: "Empties Receiving",
          icon: Package,
          path: "/lbcl/empties-receiving",
          description: "Manage empties receiving",
        },
        {
          title: "Activity Log",
          icon: Clock,
          path: "/lbcl/empties-receiving/activity-log",
          description: "View empties activity log",
        },
      ],
    },
    {
      title: "Damage Collection & Scrapping",
      icon: AlertTriangle,
      path: "/lbcl/damage-collection",
      description: "Manage damaged items collection and scrapping process",
    },
    {
      title: "Warehouse Stock Take Reconciliation",
      icon: ClipboardCheck,
      path: "/lbcl/stock-reconciliation",
      description: "Perform warehouse stock take and reconciliation",
    },
  ]

  const toggleExpand = (title: string) => {
    setExpandedItems((prev) =>
      prev.includes(title) ? prev.filter((item) => item !== title) : [...prev, title]
    )
  }

  const handleNavigation = (path: string) => {
    router.push(path)
    setOpen(false)
  }

  const isItemActive = (item: MenuItem): boolean => {
    if (item.path && (pathname === item.path || pathname?.startsWith(item.path + "/"))) {
      return true
    }
    if (item.children) {
      return item.children.some((child) => isItemActive(child))
    }
    return false
  }

  return (
    <Sheet open={open} onOpenChange={setOpen}>
      <SheetTrigger asChild>
        <button className={`p-2 hover:bg-gray-100 rounded-lg ${className}`}>
          <Menu className="w-6 h-6" />
        </button>
      </SheetTrigger>
      <SheetContent side="left" className="w-[280px] sm:w-[320px] p-0">
        <div className="flex flex-col h-full">
          {/* Header */}
          <div className="p-6 border-b bg-[#A08B5C]">
            <SheetTitle className="text-xl font-bold text-white">Navigation</SheetTitle>
            <p className="text-sm text-white/80 mt-1">Select a page to navigate</p>
          </div>

          {/* Menu Items */}
          <nav className="flex-1 overflow-y-auto py-4">
            {menuItems.map((item) => {
              const IconComponent = item.icon
              const isActive = isItemActive(item)
              const isExpanded = expandedItems.includes(item.title)
              const hasChildren = item.children && item.children.length > 0

              return (
                <div key={item.title}>
                  {/* Parent Item */}
                  <button
                    onClick={() => {
                      if (hasChildren) {
                        toggleExpand(item.title)
                      } else if (item.path) {
                        handleNavigation(item.path)
                      }
                    }}
                    className={`w-full flex items-start gap-4 px-6 py-4 hover:bg-gray-50 transition-colors ${
                      isActive ? "bg-[#FFF8E7] border-l-4 border-[#A08B5C]" : ""
                    }`}
                  >
                    <div
                      className={`w-10 h-10 rounded-lg flex items-center justify-center flex-shrink-0 ${
                        isActive ? "bg-[#A08B5C]" : "bg-gray-100"
                      }`}
                    >
                      <IconComponent className={`w-5 h-5 ${isActive ? "text-white" : "text-gray-600"}`} />
                    </div>
                    <div className="flex-1 text-left">
                      <div className={`font-semibold ${isActive ? "text-[#A08B5C]" : "text-gray-900"}`}>
                        {item.title}
                      </div>
                      <div className="text-sm text-gray-500">{item.description}</div>
                    </div>
                    {hasChildren && (
                      <div className="flex items-center">
                        {isExpanded ? (
                          <ChevronDown className="w-5 h-5 text-gray-600" />
                        ) : (
                          <ChevronRight className="w-5 h-5 text-gray-600" />
                        )}
                      </div>
                    )}
                  </button>

                  {/* Child Items */}
                  {hasChildren && isExpanded && (
                    <div className="bg-gray-50">
                      {item.children!.map((child) => {
                        const ChildIconComponent = child.icon
                        const isChildActive =
                          pathname === child.path || pathname?.startsWith(child.path + "/")

                        return (
                          <button
                            key={child.path}
                            onClick={() => child.path && handleNavigation(child.path)}
                            className={`w-full flex items-start gap-3 pl-16 pr-6 py-3 hover:bg-gray-100 transition-colors ${
                              isChildActive ? "bg-[#FFF8E7] border-l-4 border-[#A08B5C]" : ""
                            }`}
                          >
                            <div
                              className={`w-8 h-8 rounded-lg flex items-center justify-center flex-shrink-0 ${
                                isChildActive ? "bg-[#A08B5C]" : "bg-gray-200"
                              }`}
                            >
                              <ChildIconComponent
                                className={`w-4 h-4 ${isChildActive ? "text-white" : "text-gray-600"}`}
                              />
                            </div>
                            <div className="flex-1 text-left">
                              <div
                                className={`font-medium text-sm ${
                                  isChildActive ? "text-[#A08B5C]" : "text-gray-900"
                                }`}
                              >
                                {child.title}
                              </div>
                              <div className="text-xs text-gray-500">{child.description}</div>
                            </div>
                          </button>
                        )
                      })}
                    </div>
                  )}
                </div>
              )
            })}
          </nav>

          {/* Footer */}
          <div className="p-6 border-t bg-gray-50">
            <div className="text-center">
              <p className="text-xs text-gray-600 mb-2">Powered by</p>
              <div className="flex items-center justify-center gap-2">
                <div className="text-[#A08B5C] font-bold text-base">WINIT</div>
                <div className="text-gray-600 text-[10px]">
                  <div>thinking</div>
                  <div>mobile</div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </SheetContent>
    </Sheet>
  )
}
