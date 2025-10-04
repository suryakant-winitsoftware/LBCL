"use client"

import { useState, useEffect } from "react"
import { usePathname, useRouter } from "next/navigation"
import { SidebarTrigger, useSidebar } from "@/components/ui/sidebar"
import { useAutoCollapse } from "./main-layout"
import Image from "next/image"
import {
  Breadcrumb,
  BreadcrumbItem,
  BreadcrumbLink,
  BreadcrumbList,
  BreadcrumbPage,
  BreadcrumbSeparator,
} from "@/components/ui/breadcrumb"
import { usePermissions } from "@/providers/permission-provider"
import { useAuth } from "@/providers/auth-provider"
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu"
import {
  Tooltip,
  TooltipContent,
  TooltipTrigger,
} from "@/components/ui/tooltip"
import {
  CommandDialog,
  CommandEmpty,
  CommandGroup,
  CommandInput,
  CommandItem,
  CommandList,
} from "@/components/ui/command"
import { Button } from "@/components/ui/button"
import { Avatar, AvatarFallback } from "@/components/ui/avatar"
import {
  Search,
  User,
  Settings,
  Bug,
  LogOut,
  Bell,
  ChevronDown,
  Home,
  Pin,
  PinOff,
} from "lucide-react"
import { SubModule, SubSubModule } from "@/types/permission.types"
import { getModuleIcon, getSubModuleIcon } from "@/lib/navigation-icons"
import { RefreshMenuButton } from "@/components/ui/refresh-menu-button"
import { cn } from "@/lib/utils"

export function Header() {
  const pathname = usePathname()
  const router = useRouter()
  const { menuHierarchy } = usePermissions()
  const { user, logout } = useAuth()
  const { autoCollapseEnabled, setAutoCollapseEnabled } = useAutoCollapse()
  const { toggleSidebar, state } = useSidebar()
  const [searchOpen, setSearchOpen] = useState(false)
  const [searchQuery, setSearchQuery] = useState("")
  const isCollapsed = state === "collapsed"
  
  // Keyboard shortcut for search
  useEffect(() => {
    const down = (e: KeyboardEvent) => {
      if (e.key === "k" && (e.metaKey || e.ctrlKey)) {
        e.preventDefault()
        setSearchOpen((open) => !open)
      }
    }
    
    document.addEventListener("keydown", down)
    return () => document.removeEventListener("keydown", down)
  }, [])
  
  const handleNavigation = (path: string) => {
    router.push(`/${path}`)
    setSearchOpen(false)
  }
  
  const handleLogout = async () => {
    await logout()
    router.push("/login")
  }
  
  // Flatten menu for search
  const getSearchableItems = () => {
    const items: { type: string; name: string; path: string; icon: any }[] = []
    
    // Add quick actions
    items.push({
      type: 'quick',
      name: 'Dashboard',
      path: 'dashboard',
      icon: Home
    })
    
    menuHierarchy.forEach(menuModule => {
      const moduleChildren = ('children' in menuModule && Array.isArray(menuModule.children)) ? menuModule.children as SubModule[] : undefined
      
      if (!moduleChildren || moduleChildren.length === 0) {
        items.push({
          type: 'module',
          name: menuModule.moduleNameEn,
          path: menuModule.relativePath ? menuModule.relativePath : `module/${menuModule.uid}`,
          icon: getModuleIcon(menuModule.moduleNameEn, menuModule.uid)
        })
      } else {
        moduleChildren.forEach(subModule => {
          const subModuleChildren = ('children' in subModule && Array.isArray(subModule.children)) ? subModule.children as SubSubModule[] : undefined
          
          if (!subModuleChildren || subModuleChildren.length === 0) {
            items.push({
              type: 'submodule',
              name: `${menuModule.moduleNameEn} > ${subModule.submoduleNameEn}`,
              path: subModule.relativePath ? subModule.relativePath : `submodule/${subModule.uid}`,
              icon: getSubModuleIcon(subModule.submoduleNameEn, menuModule.moduleNameEn)
            })
          } else {
            subModuleChildren.forEach(page => {
              items.push({
                type: 'page',
                name: `${menuModule.moduleNameEn} > ${subModule.submoduleNameEn} > ${page.subSubModuleNameEn}`,
                path: page.relativePath,
                icon: getSubModuleIcon(subModule.submoduleNameEn, menuModule.moduleNameEn)
              })
            })
          }
        })
      }
    })
    
    return items
  }
  
  const filteredItems = getSearchableItems().filter(item =>
    item.name.toLowerCase().includes(searchQuery.toLowerCase())
  )

  // Generate breadcrumbs from the current path
  const generateBreadcrumbs = () => {
    const segments = pathname.split("/").filter(Boolean)
    if (segments.length === 0) return []

    const breadcrumbs = []

    // Only show breadcrumbs for actual pages, not dashboard
    if (segments[0] === "authenticated" && segments.length > 2) {
      // Find current page in menu hierarchy
      const currentPath = segments.slice(2).join("/")
      
      // Search through menu hierarchy to find the current page
      for (const menuModule of menuHierarchy) {
        const moduleChildren = ('children' in menuModule && Array.isArray(menuModule.children)) ? menuModule.children : []
        
        for (const subModule of moduleChildren) {
          const subModuleChildren = ('children' in subModule && Array.isArray(subModule.children)) ? subModule.children : []
          
          for (const page of subModuleChildren) {
            if (page.relativePath === currentPath) {
              breadcrumbs.push({
                label: menuModule.moduleNameEn,
                href: "#",
                isActive: false
              })
              breadcrumbs.push({
                label: subModule.submoduleNameEn,
                href: "#",
                isActive: false
              })
              breadcrumbs.push({
                label: page.subSubModuleNameEn,
                href: pathname,
                isActive: true
              })
              return breadcrumbs
            }
          }
        }
      }

      // Fallback if not found in menu hierarchy
      if (breadcrumbs.length === 0) {
        breadcrumbs.push({
          label: segments[segments.length - 1].charAt(0).toUpperCase() + segments[segments.length - 1].slice(1),
          href: pathname,
          isActive: true
        })
      }
    }

    return breadcrumbs
  }

  const breadcrumbs = generateBreadcrumbs()

  return (
    <>
    <header className="sticky top-0 z-50 flex h-14 shrink-0 items-center border-b border-gray-200 bg-white dark:border-gray-800 dark:bg-gray-950">
      <div className="flex w-full items-center justify-between px-4">
        {/* Left Section */}
        <div className="flex items-center gap-2">
          {/* Logo Section when collapsed */}
          {isCollapsed && (
            <div className="flex items-center mr-2">
              <Image
                src="/winitlogo.png"
                alt="WINIT"
                width={140}
                height={40}
                className="object-contain h-8 w-auto"
                priority
                unoptimized
              />
            </div>
          )}
          
          {/* Sidebar Controls - No extra spacing */}
          <div className="flex items-center gap-1">
            <SidebarTrigger className="h-8 w-8 hover:bg-gray-100 dark:hover:bg-gray-800 rounded-md transition-colors" />
            
            {/* Pin toggle with better visual */}
            <Tooltip>
              <TooltipTrigger asChild>
                <button
                  onClick={() => setAutoCollapseEnabled(!autoCollapseEnabled)}
                  className={cn(
                    "h-8 w-8 flex items-center justify-center rounded-md transition-all",
                    !autoCollapseEnabled 
                      ? "bg-blue-50 hover:bg-blue-100 dark:bg-blue-950/50 dark:hover:bg-blue-950/70" 
                      : "hover:bg-gray-100 dark:hover:bg-gray-800"
                  )}
                >
                  {!autoCollapseEnabled ? (
                    <Pin className="h-4 w-4 text-blue-600 dark:text-blue-400" />
                  ) : (
                    <PinOff className="h-4 w-4 text-gray-400 dark:text-gray-400" />
                  )}
                </button>
              </TooltipTrigger>
              <TooltipContent side="bottom">
                <p className="text-xs font-medium">
                  {!autoCollapseEnabled 
                    ? "Sidebar is pinned (won't auto-collapse)" 
                    : "Click to pin sidebar"}
                </p>
              </TooltipContent>
            </Tooltip>
          </div>
          
          {/* Search Bar - Only show when sidebar is expanded */}
          {!isCollapsed && (
            <div className="relative flex items-center ml-2">
              <Search className="absolute left-3 h-4 w-4 text-gray-400" />
              <input
                type="text"
                placeholder="Search..."
                className="h-8 w-64 pl-9 pr-16 text-sm bg-gray-50 dark:bg-gray-900 border border-gray-200 dark:border-gray-700 rounded-lg focus:outline-none focus:ring-1 focus:ring-blue-500 focus:border-blue-500 transition-all duration-200 placeholder:text-gray-400"
                onClick={() => setSearchOpen(true)}
                readOnly
              />
              <kbd className="absolute right-2 inline-flex h-5 select-none items-center gap-1 rounded border border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-900 px-1.5 font-mono text-[10px] font-medium text-gray-500 dark:text-gray-400">
                <span className="text-xs">⌘</span>K
              </kbd>
            </div>
          )}
          
          {/* Breadcrumbs - Only show when there are breadcrumbs */}
          {breadcrumbs.length > 0 && (
            <>
              <div className="h-5 w-px bg-gray-200 dark:bg-gray-700" />
              <Breadcrumb className="hidden lg:flex">
                <BreadcrumbList className="flex items-center gap-1">
                  {breadcrumbs.map((breadcrumb, index) => (
                    <div key={breadcrumb.href} className="flex items-center">
                      <BreadcrumbItem>
                        {breadcrumb.isActive ? (
                          <BreadcrumbPage className="font-semibold text-gray-900 dark:text-gray-100 text-sm">
                            {breadcrumb.label}
                          </BreadcrumbPage>
                        ) : (
                          <BreadcrumbLink 
                            href={breadcrumb.href}
                            className="text-sm font-medium text-gray-600 hover:text-gray-900 dark:text-gray-400 dark:hover:text-gray-100 transition-colors duration-200"
                          >
                            {breadcrumb.label}
                          </BreadcrumbLink>
                        )}
                      </BreadcrumbItem>
                      {index < breadcrumbs.length - 1 && (
                        <BreadcrumbSeparator className="text-gray-400 dark:text-gray-600" />
                      )}
                    </div>
                  ))}
                </BreadcrumbList>
              </Breadcrumb>
            </>
          )}
        </div>
        
        {/* Center Section - Page Title for Mobile */}
        <div className="flex-1 lg:hidden">
          {breadcrumbs.length > 0 && (
            <h1 className="text-sm font-semibold text-gray-900 dark:text-gray-100 truncate text-center">
              {breadcrumbs[breadcrumbs.length - 1].label}
            </h1>
          )}
        </div>
        
        {/* Right Section */}
        <div className="flex items-center gap-3">
          {/* Notification Button */}
          <Button 
            variant="ghost" 
            size="sm"
            className="relative h-9 w-9 rounded-md hover:bg-gray-50 dark:hover:bg-gray-800 transition-all duration-200"
          >
            <Bell className="h-4 w-4 text-gray-600 dark:text-gray-400" />
            <span className="absolute -top-0.5 -right-0.5 h-3.5 w-3.5 rounded-full bg-blue-600 text-[9px] font-medium text-white flex items-center justify-center">
              3
            </span>
          </Button>
          
          <div className="h-5 w-px bg-gray-200 dark:bg-gray-700" />
          
          {/* Refresh Menu Button */}
          <RefreshMenuButton 
            variant="ghost" 
            size="icon"
            className="h-9 w-9 rounded-md hover:bg-gray-50 dark:hover:bg-gray-800 transition-all duration-200"
          />
          
          {/* User Dropdown */}
          <DropdownMenu>
            <DropdownMenuTrigger asChild>
              <Button 
                variant="ghost" 
                className="flex items-center gap-2.5 h-9 px-3 rounded-md border border-gray-100 dark:border-gray-800 hover:bg-gray-50 dark:hover:bg-gray-800 transition-all duration-200"
              >
                <Avatar className="h-7 w-7">
                  <AvatarFallback className="bg-gray-900 dark:bg-gray-100 text-white dark:text-gray-900 font-medium text-xs">
                    {user?.name?.charAt(0).toUpperCase() || "A"}
                  </AvatarFallback>
                </Avatar>
                <div className="hidden lg:flex flex-col items-start text-left">
                  <span className="text-sm font-medium text-gray-900 dark:text-gray-100 leading-tight">{user?.name || "Admin"}</span>
                  <span className="text-[11px] text-gray-500 dark:text-gray-400 leading-tight">{user?.roles?.[0]?.roleNameEn || "Administrator"}</span>
                </div>
                <ChevronDown className="h-3.5 w-3.5 text-gray-400 hidden lg:block" />
              </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end" className="w-72 p-0 mt-1 border border-gray-100 dark:border-gray-800 rounded-lg overflow-hidden">
              {/* Professional Header Section */}
              <div className="bg-gray-50 dark:bg-gray-900/50 border-b border-gray-100 dark:border-gray-800 p-4">
                <div className="flex items-center gap-3">
                  <Avatar className="h-12 w-12">
                    <AvatarFallback className="bg-gray-900 dark:bg-gray-100 text-white dark:text-gray-900 font-medium text-sm">
                      {user?.name?.charAt(0).toUpperCase() || "A"}
                    </AvatarFallback>
                  </Avatar>
                  <div className="flex-1">
                    <h3 className="text-sm font-semibold text-gray-900 dark:text-gray-100">{user?.name || "Admin User"}</h3>
                    <p className="text-xs text-gray-600 dark:text-gray-400">{user?.email || "admin@company.com"}</p>
                    <div className="flex items-center gap-2 mt-1">
                      <span className="inline-flex items-center px-2 py-0.5 rounded text-[10px] font-medium bg-gray-100 text-gray-700 dark:bg-gray-800 dark:text-gray-300">
                        {user?.roles?.[0]?.roleNameEn || "Administrator"}
                      </span>
                    </div>
                  </div>
                </div>
              </div>
              
              {/* Menu Items */}
              <div className="p-1">
                <DropdownMenuItem 
                  onClick={() => handleNavigation("profile")}
                  className="px-3 py-2 rounded-md text-sm hover:bg-gray-50 dark:hover:bg-gray-800 transition-colors cursor-pointer"
                >
                  <div className="flex items-center gap-3">
                    <User className="h-4 w-4 text-gray-600 dark:text-gray-400" />
                    <span className="font-medium text-gray-700 dark:text-gray-300">Profile</span>
                  </div>
                </DropdownMenuItem>
                
                <DropdownMenuItem 
                  onClick={() => handleNavigation("settings")}
                  className="px-3 py-2 rounded-md text-sm hover:bg-gray-50 dark:hover:bg-gray-800 transition-colors cursor-pointer"
                >
                  <div className="flex items-center gap-3">
                    <Settings className="h-4 w-4 text-gray-600 dark:text-gray-400" />
                    <span className="font-medium text-gray-700 dark:text-gray-300">Settings</span>
                  </div>
                </DropdownMenuItem>
                
                <DropdownMenuItem 
                  onClick={() => handleNavigation("debug")} 
                  className="px-3 py-2 rounded-md text-sm hover:bg-gray-50 dark:hover:bg-gray-800 transition-colors cursor-pointer"
                >
                  <div className="flex items-center gap-3">
                    <Bug className="h-4 w-4 text-gray-600 dark:text-gray-400" />
                    <span className="font-medium text-gray-700 dark:text-gray-300">Debug</span>
                  </div>
                </DropdownMenuItem>
              </div>
              
              {/* Footer */}
              <div className="border-t border-gray-100 dark:border-gray-800 p-1">
                <DropdownMenuItem 
                  onClick={handleLogout} 
                  className="px-3 py-2 rounded-md text-sm hover:bg-red-50 dark:hover:bg-red-950/30 transition-colors cursor-pointer"
                >
                  <div className="flex items-center gap-3">
                    <LogOut className="h-4 w-4 text-red-600 dark:text-red-400" />
                    <span className="font-medium text-red-600 dark:text-red-400">Sign Out</span>
                  </div>
                </DropdownMenuItem>
              </div>
            </DropdownMenuContent>
          </DropdownMenu>
        </div>
      </div>
    </header>
    
    {/* Enterprise Command Search Dialog */}
    <CommandDialog open={searchOpen} onOpenChange={setSearchOpen}>
      <div className="relative border-b border-gray-200 dark:border-gray-700">
        <Search className="absolute left-4 top-1/2 -translate-y-1/2 h-5 w-5 text-gray-400" />
        <CommandInput 
          placeholder="Search for modules, pages, users, or settings..." 
          value={searchQuery}
          onValueChange={setSearchQuery}
          className="h-14 pl-12 text-base border-0 focus:ring-0 bg-transparent font-medium"
        />
      </div>
      <CommandList className="max-h-96 overflow-y-auto">
        <CommandEmpty className="py-8 text-center text-sm text-gray-500 dark:text-gray-400">
          <div className="flex flex-col items-center gap-3">
            <div className="h-12 w-12 rounded-full bg-gray-100 dark:bg-gray-800 flex items-center justify-center">
              <Search className="h-6 w-6 text-gray-400" />
            </div>
            <p className="font-medium">No results found for "{searchQuery}"</p>
            <p className="text-xs text-gray-400">Try searching for modules, pages, or settings</p>
          </div>
        </CommandEmpty>
        
        {filteredItems.filter(item => item.type === 'quick').length > 0 && (
          <CommandGroup heading="Quick Access" className="p-2">
            {filteredItems.filter(item => item.type === 'quick').map((item, index) => (
              <CommandItem
                key={index}
                onSelect={() => handleNavigation(item.path)}
                className="flex items-center gap-3 px-4 py-3 rounded-lg cursor-pointer hover:bg-blue-50 dark:hover:bg-blue-950/50 transition-colors data-[selected]:bg-blue-100 dark:data-[selected]:bg-blue-950/80"
              >
                <div className="h-10 w-10 rounded-lg bg-blue-100 dark:bg-blue-900/50 flex items-center justify-center">
                  <item.icon className="h-5 w-5 text-blue-600 dark:text-blue-400" />
                </div>
                <div className="flex flex-col items-start">
                  <span className="text-sm font-semibold text-gray-900 dark:text-gray-100">{item.name}</span>
                  <span className="text-xs text-gray-500 dark:text-gray-400">Quick navigation</span>
                </div>
              </CommandItem>
            ))}
          </CommandGroup>
        )}
        
        {filteredItems.filter(item => item.type !== 'quick').length > 0 && (
          <CommandGroup heading="Navigation" className="p-2">
            {filteredItems.filter(item => item.type !== 'quick').map((item, index) => (
              <CommandItem
                key={index}
                onSelect={() => handleNavigation(item.path)}
                className="flex items-center gap-3 px-4 py-3 rounded-lg cursor-pointer hover:bg-gray-50 dark:hover:bg-gray-800 transition-colors"
              >
                <div className="h-8 w-8 rounded-lg bg-gray-100 dark:bg-gray-800 flex items-center justify-center">
                  <item.icon className="h-4 w-4 text-gray-600 dark:text-gray-400" />
                </div>
                <div className="flex flex-col items-start">
                  <span className="text-sm font-medium text-gray-900 dark:text-gray-100">{item.name}</span>
                  <span className="text-xs text-gray-500 dark:text-gray-400 capitalize">{item.type}</span>
                </div>
              </CommandItem>
            ))}
          </CommandGroup>
        )}
        
        {/* Quick Actions */}
        <CommandGroup heading="Quick Actions" className="p-2 border-t border-gray-200 dark:border-gray-700">
          <CommandItem
            onSelect={() => handleNavigation("settings")}
            className="flex items-center gap-3 px-4 py-3 rounded-lg cursor-pointer hover:bg-gray-50 dark:hover:bg-gray-800 transition-colors"
          >
            <div className="h-8 w-8 rounded-lg bg-gray-100 dark:bg-gray-800 flex items-center justify-center">
              <Settings className="h-4 w-4 text-gray-600 dark:text-gray-400" />
            </div>
            <span className="text-sm font-medium">Application Settings</span>
          </CommandItem>
          <CommandItem
            onSelect={() => handleNavigation("profile")}
            className="flex items-center gap-3 px-4 py-3 rounded-lg cursor-pointer hover:bg-gray-50 dark:hover:bg-gray-800 transition-colors"
          >
            <div className="h-8 w-8 rounded-lg bg-gray-100 dark:bg-gray-800 flex items-center justify-center">
              <User className="h-4 w-4 text-gray-600 dark:text-gray-400" />
            </div>
            <span className="text-sm font-medium">Profile Settings</span>
          </CommandItem>
        </CommandGroup>
      </CommandList>
    </CommandDialog>
    </>
  )
}