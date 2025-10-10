"use client"

import { useState, useEffect } from "react"
import { usePathname, useRouter } from "next/navigation"
import {
  Sidebar,
  SidebarContent,
  SidebarGroup,
  SidebarGroupContent,
  SidebarGroupLabel,
  SidebarHeader,
  SidebarMenu,
  SidebarMenuButton,
  SidebarMenuItem,
  SidebarMenuSub,
  SidebarMenuSubButton,
  SidebarMenuSubItem,
  useSidebar,
} from "@/components/ui/sidebar"
import { SkeletonLoader } from "@/components/ui/loader"
import {
  Collapsible,
  CollapsibleContent,
  CollapsibleTrigger,
} from "@/components/ui/collapsible"
import {
  Tooltip,
  TooltipContent,
  TooltipTrigger,
} from "@/components/ui/tooltip"
import {
  ChevronRight,
  Home,
  Truck,
  UserCheck,
} from "lucide-react"
import Image from "next/image"
import { usePermissions } from "@/providers/permission-provider"
import { SubModule, SubSubModule } from "@/types/permission.types"
import { getModuleIcon, getSubModuleIcon } from "@/lib/navigation-icons"
import { cn } from "@/lib/utils"

interface ProfessionalSidebarProps {
  className?: string
}

export function ProfessionalSidebar({ className }: ProfessionalSidebarProps) {
  const pathname = usePathname()
  const router = useRouter()
  const { menuHierarchy, isLoading } = usePermissions()
  const { state, setOpen } = useSidebar()
  const [openModules, setOpenModules] = useState<Set<string>>(new Set())
  const [openSubModules, setOpenSubModules] = useState<Set<string>>(new Set())
  const isCollapsed = state === "collapsed"
  
  // Auto-open active modules based on current path
  useEffect(() => {
    if (pathname && pathname !== '/' && pathname !== '/dashboard' && menuHierarchy.length > 0) {
      const newOpenModules = new Set<string>()
      const newOpenSubModules = new Set<string>()
      
      menuHierarchy.forEach((module) => {
        const moduleChildren = ('children' in module && Array.isArray(module.children)) ? module.children as SubModule[] : undefined
        if (moduleChildren) {
          moduleChildren.forEach((subModule) => {
            const subModuleChildren = ('children' in subModule && Array.isArray(subModule.children)) ? subModule.children as SubSubModule[] : undefined
            if (subModuleChildren) {
              const hasActivePage = subModuleChildren.some((page) => {
                // Check both the original relative path and hierarchical path
                const hierarchicalPath = buildHierarchicalPath(module, subModule, page)
                return (page.relativePath && pathname.includes(page.relativePath)) ||
                       pathname === hierarchicalPath ||
                       pathname.startsWith(hierarchicalPath + '/')
              })
              if (hasActivePage) {
                newOpenModules.add(module.uid)
                newOpenSubModules.add(subModule.uid)
              }
            }
          })
        }
      })
      
      if (newOpenModules.size > 0) {
        setOpenModules(newOpenModules)
      }
      if (newOpenSubModules.size > 0) {
        setOpenSubModules(newOpenSubModules)
      }
    }
  }, [pathname, menuHierarchy])
  
  const toggleModule = (moduleUid: string) => {
    const newOpenModules = new Set(openModules)
    if (newOpenModules.has(moduleUid)) {
      newOpenModules.delete(moduleUid)
    } else {
      newOpenModules.add(moduleUid)
    }
    setOpenModules(newOpenModules)
  }
  
  const toggleSubModule = (subModuleUid: string) => {
    const newOpenSubModules = new Set(openSubModules)
    if (newOpenSubModules.has(subModuleUid)) {
      newOpenSubModules.delete(subModuleUid)
    } else {
      newOpenSubModules.add(subModuleUid)
    }
    setOpenSubModules(newOpenSubModules)
  }
  
  const buildHierarchicalPath = (module: any, subModule?: any, page?: any): string => {
    // Build hierarchical path: /module/submodule/page
    const parts: string[] = []
    
    if (module) {
      // Use module relativePath if available, otherwise use moduleNameEn converted to URL-friendly format
      const modulePart = module.relativePath || module.moduleNameEn.toLowerCase().replace(/\s+/g, '').replace(/[^a-z0-9]/g, '')
      parts.push(modulePart)
    }
    
    if (subModule) {
      // Use subModule relativePath if available, otherwise use submoduleNameEn converted to URL-friendly format
      const subModulePart = subModule.relativePath || subModule.submoduleNameEn.toLowerCase().replace(/\s+/g, '').replace(/[^a-z0-9]/g, '')
      parts.push(subModulePart)
    }
    
    if (page) {
      // Use page relativePath directly (this is the final page)
      const pagePart = page.relativePath || page.subSubModuleNameEn.toLowerCase().replace(/\s+/g, '').replace(/[^a-z0-9]/g, '')
      parts.push(pagePart)
    }
    
    return '/' + parts.join('/')
  }

  const handleNavigation = (path: string, hierarchicalPath?: string) => {
    // If the original path is already a full path (starts with /), use it directly
    // Otherwise, use hierarchical path if provided
    const targetPath = path.startsWith('/') ? path : (hierarchicalPath || path)
    
    // Validate path before navigation
    if (!targetPath || targetPath.includes('loadrequestemplate') || targetPath.includes('LoadTemplate')) {
      router.push('/dashboard')
      return
    }
    
    // Auto-expand sidebar when clicking in collapsed mode
    if (isCollapsed) {
      setOpen(true)
    }
    
    // Ensure path starts with /
    const safePath = targetPath.startsWith('/') ? targetPath : `/${targetPath}`
    
    router.push(safePath)
  }
  
  return (
    <Sidebar 
      variant="sidebar"
      collapsible="icon" 
      className={cn(
        "transition-all duration-300 border-r border-gray-200/40 dark:border-gray-800/60",
        "bg-white/95 backdrop-blur-sm dark:bg-gray-900",
        "shadow-sm",
        className
      )}
    >
      <SidebarHeader className="border-b border-gray-200/40 dark:border-gray-800/60 bg-white dark:bg-gray-900">
        <div className={cn(
          "transition-all duration-300",
          isCollapsed ? "p-3" : "px-4 py-5"
        )}>
          <div className={cn(
            "flex items-center justify-center"
          )}>
            <Image
              src="/winitlogo.png"
              alt="WINIT Logo"
              width={isCollapsed ? 32 : 140}
              height={isCollapsed ? 32 : 45}
              className={cn(
                "object-contain transition-all duration-300",
                isCollapsed ? "w-8 h-8" : "w-auto h-11"
              )}
              priority
            />
          </div>
        </div>
      </SidebarHeader>
        
      <SidebarContent className="flex-1 overflow-y-auto overflow-x-hidden bg-white dark:bg-gray-900">
        {/* Dashboard */}
        <SidebarGroup className={cn(isCollapsed ? "px-2 pt-3 pb-2" : "px-3 pt-3 pb-2")}>
          <SidebarGroupContent>
            <SidebarMenu className={cn(isCollapsed && "space-y-2")}>
              <SidebarMenuItem>
                <SidebarMenuButton 
                  isActive={pathname === "/dashboard"}
                  onClick={() => handleNavigation("dashboard")}
                  tooltip="Dashboard"
                  className={cn(
                    "font-medium transition-all duration-200 rounded-lg group",
                    isCollapsed 
                      ? "h-11 w-11 p-0 flex items-center justify-center mx-auto" 
                      : "h-10 px-3",
                    "hover:bg-gray-100 dark:hover:bg-gray-800",
                    "data-[active=true]:bg-blue-50 data-[active=true]:text-blue-600",
                    "dark:data-[active=true]:bg-blue-950/30 dark:data-[active=true]:text-blue-400"
                  )}
                >
                  <Home className="h-4 w-4 flex-shrink-0" />
                  {!isCollapsed && <span className="truncate">Dashboard</span>}
                </SidebarMenuButton>
              </SidebarMenuItem>
            </SidebarMenu>
          </SidebarGroupContent>
        </SidebarGroup>
          
          <div className="mx-4 h-[0.5px] bg-gradient-to-r from-transparent via-gray-200/60 to-transparent dark:via-gray-700/60 mb-2" />
          
          {/* Load Management - Manual Entry */}
          <SidebarGroup className={cn(isCollapsed ? "px-2" : "px-3")}>
            <SidebarGroupContent>
              <SidebarMenu className={cn(isCollapsed ? "space-y-2" : "space-y-1")}>
                <SidebarMenuItem>
                  {isCollapsed ? (
                    <Tooltip>
                      <TooltipTrigger asChild>
                        <SidebarMenuButton
                          onClick={() => {
                            setOpen(true)
                            setTimeout(() => handleNavigation("/load-management"), 150)
                          }}
                          isActive={pathname.startsWith("/load-management")}
                          className={cn(
                            "font-medium transition-all duration-200 rounded-lg group",
                            "h-11 w-11 p-0 flex items-center justify-center mx-auto",
                            "hover:bg-gray-100 dark:hover:bg-gray-800",
                            pathname.startsWith("/load-management") && "bg-blue-50 text-blue-600 dark:bg-blue-950/30 dark:text-blue-400"
                          )}
                        >
                          <Truck className="h-4 w-4 flex-shrink-0" />
                        </SidebarMenuButton>
                      </TooltipTrigger>
                      <TooltipContent side="right">
                        <p>Load Management</p>
                      </TooltipContent>
                    </Tooltip>
                  ) : (
                    <Collapsible
                      open={pathname.startsWith("/load-management")}
                      onOpenChange={() => {}}
                    >
                      <CollapsibleTrigger asChild>
                        <SidebarMenuButton 
                          onClick={() => handleNavigation("/load-management")}
                          className={cn(
                            "font-medium transition-all duration-200 rounded-lg group",
                            "h-10",
                            "hover:bg-gray-100 dark:hover:bg-gray-800",
                            pathname.startsWith("/load-management") && "bg-blue-50 text-blue-600 dark:bg-blue-950/30 dark:text-blue-400"
                          )}
                        >
                          <Truck className="h-4 w-4 flex-shrink-0" />
                          <span className="flex-1 truncate">Load Management</span>
                          <ChevronRight className={cn(
                            "h-3 w-3 transition-transform flex-shrink-0",
                            pathname.startsWith("/load-management") && "rotate-90"
                          )} />
                        </SidebarMenuButton>
                      </CollapsibleTrigger>
                      <CollapsibleContent className="mt-1">
                        <SidebarMenuSub className="ml-3 pl-3 relative space-y-1 before:absolute before:left-0 before:top-0 before:bottom-0 before:w-[1px] before:bg-gradient-to-b before:from-gray-200/20 before:via-gray-200/50 before:to-gray-200/20 dark:before:from-gray-700/20 dark:before:via-gray-700/50 dark:before:to-gray-700/20">
                          <SidebarMenuSubItem className="mb-0.5">
                            <SidebarMenuSubButton
                              onClick={() => handleNavigation("/load-management/lsr")}
                              isActive={pathname.startsWith("/load-management/lsr")}
                              className={cn(
                                "h-9 text-sm transition-all duration-200 rounded-md",
                                "hover:bg-gray-100 dark:hover:bg-gray-800/50",
                                "data-[active=true]:bg-blue-50 data-[active=true]:text-blue-600",
                                "data-[active=true]:font-medium",
                                "dark:data-[active=true]:bg-blue-950/30 dark:data-[active=true]:text-blue-400"
                              )}
                            >
                              <Truck className="h-3.5 w-3.5 flex-shrink-0" />
                              <span className="truncate">LSR</span>
                            </SidebarMenuSubButton>
                          </SidebarMenuSubItem>
                          <SidebarMenuSubItem className="mb-0.5">
                            <SidebarMenuSubButton
                              onClick={() => handleNavigation("/load-management/logistics-approval")}
                              isActive={pathname.startsWith("/load-management/logistics-approval")}
                              className={cn(
                                "h-9 text-sm transition-all duration-200 rounded-md",
                                "hover:bg-gray-100 dark:hover:bg-gray-800/50",
                                "data-[active=true]:bg-blue-50 data-[active=true]:text-blue-600",
                                "data-[active=true]:font-medium",
                                "dark:data-[active=true]:bg-blue-950/30 dark:data-[active=true]:text-blue-400"
                              )}
                            >
                              <UserCheck className="h-3.5 w-3.5 flex-shrink-0" />
                              <span className="truncate">Logistics Approval Agent</span>
                            </SidebarMenuSubButton>
                          </SidebarMenuSubItem>
                        </SidebarMenuSub>
                      </CollapsibleContent>
                    </Collapsible>
                  )}
                </SidebarMenuItem>
              </SidebarMenu>
            </SidebarGroupContent>
          </SidebarGroup>

          <div className="mx-4 h-[0.5px] bg-gradient-to-r from-transparent via-gray-200/60 to-transparent dark:via-gray-700/60 mb-2" />
          
          {/* Modules */}
          {isLoading ? (
            <SidebarGroup className={cn(isCollapsed ? "px-2" : "px-3")}>
              <SidebarGroupLabel className={cn(
                "text-[11px] font-semibold text-gray-400 uppercase tracking-wider mb-2",
                isCollapsed ? "px-0 text-center" : "px-3"
              )}>
                {!isCollapsed && "LOADING..."}
              </SidebarGroupLabel>
              <SidebarGroupContent>
                <SidebarMenu className={cn(isCollapsed ? "space-y-2" : "space-y-1")}>
                  {/* Module skeleton loaders */}
                  {Array.from({ length: 4 }).map((_, index) => (
                    <SidebarMenuItem key={`module-skeleton-${index}`}>
                      <div className={cn(
                        "transition-all duration-200 rounded-lg",
                        isCollapsed 
                          ? "h-11 w-11 mx-auto" 
                          : "h-10 px-3 flex items-center gap-3"
                      )}>
                        {isCollapsed ? (
                          <SkeletonLoader className="h-11 w-11 rounded-lg" />
                        ) : (
                          <>
                            <SkeletonLoader className="h-4 w-4 rounded flex-shrink-0" />
                            <SkeletonLoader className="h-4 flex-1 rounded" />
                          </>
                        )}
                      </div>
                    </SidebarMenuItem>
                  ))}
                </SidebarMenu>
              </SidebarGroupContent>
            </SidebarGroup>
          ) : menuHierarchy.length === 0 ? (
            <SidebarGroup className={cn(isCollapsed ? "px-2" : "px-3")}>
              <SidebarGroupLabel className={cn(
                "text-[11px] font-semibold text-gray-400 uppercase tracking-wider mb-2",
                isCollapsed ? "px-0 text-center" : "px-3"
              )}>
                {!isCollapsed && "LOADING MODULES..."}
              </SidebarGroupLabel>
              <SidebarGroupContent>
                <SidebarMenu className={cn(isCollapsed ? "space-y-2" : "space-y-1")}>
                  {/* Module skeleton loaders */}
                  {Array.from({ length: 4 }).map((_, index) => (
                    <SidebarMenuItem key={`empty-skeleton-${index}`}>
                      <div className={cn(
                        "transition-all duration-200 rounded-lg",
                        isCollapsed 
                          ? "h-11 w-11 mx-auto" 
                          : "h-10 px-3 flex items-center gap-3"
                      )}>
                        {isCollapsed ? (
                          <SkeletonLoader className="h-11 w-11 rounded-lg" />
                        ) : (
                          <>
                            <SkeletonLoader className="h-4 w-4 rounded flex-shrink-0" />
                            <SkeletonLoader className="h-4 flex-1 rounded" />
                          </>
                        )}
                      </div>
                    </SidebarMenuItem>
                  ))}
                </SidebarMenu>
              </SidebarGroupContent>
            </SidebarGroup>
          ) : (
            <SidebarGroup className={cn(isCollapsed ? "px-2" : "px-3")}>
              <SidebarGroupLabel className={cn(
                "text-[11px] font-semibold text-gray-400 uppercase tracking-wider mb-2",
                isCollapsed ? "px-0 text-center" : "px-3"
              )}>
                {!isCollapsed && "MODULES"}
              </SidebarGroupLabel>
              <SidebarGroupContent>
                <SidebarMenu className={cn(isCollapsed ? "space-y-2" : "space-y-1")}>
                  {menuHierarchy.map((module) => {
                    const Icon = getModuleIcon(module.moduleNameEn, module.uid)
                    const moduleChildren = ('children' in module && Array.isArray(module.children)) ? module.children as SubModule[] : undefined
                    const isModuleOpen = openModules.has(module.uid)
                    
                    // Check if any child is active
                    const hasActiveChild = moduleChildren?.some(subModule => {
                      const subModuleChildren = ('children' in subModule && Array.isArray(subModule.children)) ? subModule.children : []
                      return subModuleChildren.some(page => {
                        const hierarchicalPath = buildHierarchicalPath(module, subModule, page)
                        return pathname.includes(page.relativePath) || 
                               pathname === hierarchicalPath ||
                               pathname.startsWith(hierarchicalPath + '/')
                      })
                    }) || false
                    
                    if (!moduleChildren || moduleChildren.length === 0) {
                      const modulePath = module.relativePath || `module/${module.uid}`
                      const isActive = pathname === `/${modulePath}` || pathname.includes(modulePath)
                      return (
                        <SidebarMenuItem key={module.uid}>
                          <SidebarMenuButton
                            onClick={() => handleNavigation(modulePath)}
                            isActive={isActive}
                            tooltip={module.moduleNameEn}
                            className={cn(
                              "font-medium transition-all duration-200 rounded-lg group",
                              isCollapsed 
                                ? "h-11 w-11 p-0 flex items-center justify-center mx-auto" 
                                : "h-10 px-3",
                              "hover:bg-gray-100 dark:hover:bg-gray-800",
                              "data-[active=true]:bg-blue-50 data-[active=true]:text-blue-600",
                              "dark:data-[active=true]:bg-blue-950/30 dark:data-[active=true]:text-blue-400"
                            )}
                          >
                            <Icon className="h-4 w-4 flex-shrink-0" />
                            {!isCollapsed && <span className="truncate">{module.moduleNameEn}</span>}
                          </SidebarMenuButton>
                        </SidebarMenuItem>
                      )
                    }
                    
                    return (
                      <SidebarMenuItem key={module.uid}>
                        {isCollapsed ? (
                          <Tooltip>
                            <TooltipTrigger asChild>
                              <SidebarMenuButton
                                onClick={() => {
                                  setOpen(true)
                                  setTimeout(() => toggleModule(module.uid), 150)
                                }}
                                isActive={hasActiveChild}
                                className={cn(
                                  "font-medium transition-all duration-200 rounded-lg group",
                                  "h-11 w-11 p-0 flex items-center justify-center mx-auto",
                                  "hover:bg-gray-100 dark:hover:bg-gray-800",
                                  hasActiveChild && "bg-blue-50 text-blue-600 dark:bg-blue-950/30 dark:text-blue-400"
                                )}
                              >
                                <Icon className="h-4 w-4 flex-shrink-0" />
                              </SidebarMenuButton>
                            </TooltipTrigger>
                            <TooltipContent side="right">
                              <p>{module.moduleNameEn}</p>
                            </TooltipContent>
                          </Tooltip>
                        ) : (
                          <Collapsible
                            open={isModuleOpen}
                            onOpenChange={() => toggleModule(module.uid)}
                          >
                            <CollapsibleTrigger asChild>
                              <SidebarMenuButton 
                                className={cn(
                                  "font-medium transition-all duration-200 rounded-lg group",
                                  "h-10",
                                  "hover:bg-gray-100 dark:hover:bg-gray-800",
                                  hasActiveChild && "bg-blue-50 text-blue-600 dark:bg-blue-950/30 dark:text-blue-400"
                                )}
                              >
                                <Icon className="h-4 w-4 flex-shrink-0" />
                                <span className="flex-1 truncate">{module.moduleNameEn}</span>
                                <ChevronRight className={cn(
                                  "h-3 w-3 transition-transform flex-shrink-0",
                                  isModuleOpen && "rotate-90"
                                )} />
                              </SidebarMenuButton>
                            </CollapsibleTrigger>
                            <CollapsibleContent className="mt-1">
                              <SidebarMenuSub className="ml-3 pl-3 relative space-y-1 before:absolute before:left-0 before:top-0 before:bottom-0 before:w-[1px] before:bg-gradient-to-b before:from-gray-200/20 before:via-gray-200/50 before:to-gray-200/20 dark:before:from-gray-700/20 dark:before:via-gray-700/50 dark:before:to-gray-700/20">
                              {moduleChildren.map((subModule) => {
                                const SubIcon = getSubModuleIcon(subModule.submoduleNameEn, module.moduleNameEn)
                                const subModuleChildren = ('children' in subModule && Array.isArray(subModule.children)) ? subModule.children as SubSubModule[] : undefined
                                const isSubModuleOpen = openSubModules.has(subModule.uid)
                                
                                // Check if any child is active
                                const hasActiveSubChild = subModuleChildren?.some(page => {
                                  const hierarchicalPath = buildHierarchicalPath(module, subModule, page)
                                  return pathname.includes(page.relativePath) || 
                                         pathname === hierarchicalPath ||
                                         pathname.startsWith(hierarchicalPath + '/')
                                }) || false
                                
                                if (!subModuleChildren || subModuleChildren.length === 0) {
                                  const subModulePath = subModule.relativePath || `submodule/${subModule.uid}`
                                  const isActive = pathname === `/${subModulePath}` || pathname.includes(subModulePath)
                                  return (
                                    <SidebarMenuSubItem key={subModule.uid} className="mb-0.5">
                                      <SidebarMenuSubButton
                                        onClick={() => handleNavigation(subModulePath)}
                                        isActive={isActive}
                                        className={cn(
                                          "h-9 text-sm transition-all duration-200 rounded-md",
                                          "hover:bg-gray-100 dark:hover:bg-gray-800/50",
                                          "data-[active=true]:bg-blue-50 data-[active=true]:text-blue-600",
                                          "data-[active=true]:font-medium",
                                          "dark:data-[active=true]:bg-blue-950/30 dark:data-[active=true]:text-blue-400"
                                        )}
                                      >
                                        <SubIcon className="h-3.5 w-3.5 flex-shrink-0" />
                                        <span className="truncate">{subModule.submoduleNameEn}</span>
                                      </SidebarMenuSubButton>
                                    </SidebarMenuSubItem>
                                  )
                                }
                                
                                return (
                                  <SidebarMenuSubItem key={subModule.uid} className="mb-0.5">
                                    <Collapsible
                                      open={isSubModuleOpen}
                                      onOpenChange={() => toggleSubModule(subModule.uid)}
                                    >
                                      <CollapsibleTrigger asChild>
                                        <SidebarMenuSubButton
                                          className={cn(
                                            "h-9 text-sm transition-all duration-200 rounded-md",
                                            "hover:bg-gray-100 dark:hover:bg-gray-800/50",
                                            hasActiveSubChild && "bg-blue-50 text-blue-600 font-medium dark:bg-blue-950/30 dark:text-blue-400"
                                          )}
                                        >
                                          <SubIcon className="h-3.5 w-3.5 flex-shrink-0" />
                                          <span className="flex-1 truncate">{subModule.submoduleNameEn}</span>
                                          <ChevronRight className={cn(
                                            "h-3 w-3 transition-transform flex-shrink-0",
                                            isSubModuleOpen && "rotate-90"
                                          )} />
                                        </SidebarMenuSubButton>
                                      </CollapsibleTrigger>
                                      <CollapsibleContent className="mt-1">
                                        <SidebarMenuSub className="ml-3 pl-3 relative space-y-0.5 before:absolute before:left-0 before:top-2 before:bottom-2 before:w-[1px] before:bg-gray-200/30 dark:before:bg-gray-700/30">
                                          {subModuleChildren.map((page) => {
                                            // Build hierarchical path for this page
                                            const hierarchicalPath = buildHierarchicalPath(module, subModule, page)
                                            const originalPath = page.relativePath
                                            const isActive = pathname.includes(page.relativePath) || 
                                                            pathname === hierarchicalPath ||
                                                            pathname.startsWith(hierarchicalPath + '/')
                                            
                                            return (
                                              <SidebarMenuSubItem key={page.uid} className="mb-0.5">
                                                <SidebarMenuSubButton
                                                  size="sm"
                                                  isActive={isActive}
                                                  onClick={() => handleNavigation(originalPath, hierarchicalPath)}
                                                  className={cn(
                                                    "h-7 text-xs transition-all duration-200 pl-3 rounded-md",
                                                    "hover:bg-gray-100/70 dark:hover:bg-gray-800/30",
                                                    "data-[active=true]:text-blue-600 data-[active=true]:font-medium",
                                                    "dark:data-[active=true]:text-blue-400"
                                                  )}
                                                >
                                                  <span className="truncate w-full">{page.subSubModuleNameEn}</span>
                                                </SidebarMenuSubButton>
                                              </SidebarMenuSubItem>
                                            )
                                          })}
                                        </SidebarMenuSub>
                                      </CollapsibleContent>
                                    </Collapsible>
                                  </SidebarMenuSubItem>
                                )
                              })}
                            </SidebarMenuSub>
                            </CollapsibleContent>
                          </Collapsible>
                        )}
                      </SidebarMenuItem>
                    )
                  })}
                </SidebarMenu>
              </SidebarGroupContent>
          </SidebarGroup>
        )}
      </SidebarContent>
    </Sidebar>
  )
}