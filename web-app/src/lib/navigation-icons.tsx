import {
  LayoutDashboard,
  Users,
  Shield,
  Settings,
  FileText,
  Package,
  ShoppingCart,
  BarChart3,
  Building2,
  MapPin,
  Truck,
  ClipboardList,
  UserCheck,
  Calendar,
  DollarSign,
  Activity,
  Lock,
  Key,
  Store,
  Target,
  TrendingUp,
  Database,
  Globe,
  Smartphone,
  Monitor,
  FolderOpen,
  LucideIcon
} from "lucide-react"

// Icon mapping for modules based on name/UID
export const getModuleIcon = (moduleNameEn: string, moduleUid?: string): LucideIcon => {
  const name = moduleNameEn.toLowerCase()
  const uid = moduleUid?.toLowerCase() || ""
  
  // Match by common patterns
  if (name.includes("dashboard") || uid.includes("dashboard")) return LayoutDashboard
  if (name.includes("system") && name.includes("admin")) return Shield
  if (name.includes("user") || name.includes("employee")) return Users
  if (name.includes("role") || name.includes("permission")) return Key
  if (name.includes("organization") || name.includes("company")) return Building2
  if (name.includes("location") || name.includes("territory")) return MapPin
  if (name.includes("product") || name.includes("inventory")) return Package
  if (name.includes("sales") || name.includes("order")) return ShoppingCart
  if (name.includes("customer") || name.includes("outlet")) return Store
  if (name.includes("report") || name.includes("analytics")) return BarChart3
  if (name.includes("delivery") || name.includes("distribution")) return Truck
  if (name.includes("visit") || name.includes("journey")) return Calendar
  if (name.includes("promotion") || name.includes("campaign")) return Target
  if (name.includes("finance") || name.includes("payment")) return DollarSign
  if (name.includes("audit") || name.includes("log")) return ClipboardList
  if (name.includes("setting") || name.includes("config")) return Settings
  if (name.includes("mobile")) return Smartphone
  if (name.includes("web")) return Monitor
  if (name.includes("security")) return Lock
  
  // Default icons
  return FolderOpen
}

// Icon mapping for sub-modules
export const getSubModuleIcon = (subModuleName: string, parentModuleName?: string): LucideIcon => {
  const name = subModuleName.toLowerCase()
  
  // Specific sub-module patterns
  if (name.includes("maintain") || name.includes("manage")) return Settings
  if (name.includes("hierarchy")) return Database
  if (name.includes("approval")) return UserCheck
  if (name.includes("report")) return FileText
  if (name.includes("monitor") || name.includes("track")) return Activity
  if (name.includes("analysis")) return TrendingUp
  if (name.includes("global") || name.includes("region")) return Globe
  
  // Inherit from parent module if no specific match
  if (parentModuleName) {
    return getModuleIcon(parentModuleName)
  }
  
  return FileText
}