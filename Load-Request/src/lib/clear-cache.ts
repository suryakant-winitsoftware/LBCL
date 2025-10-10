// Cache clearing utility for debugging
import { productionPermissionService } from "./production-permission-service"

export function clearAllCaches() {
  console.log("🧹 Clearing all caches...")
  
  // Clear production permission service caches
  productionPermissionService.clearCache()
  
  // Clear localStorage items related to permissions
  if (typeof window !== 'undefined') {
    const keysToRemove = []
    for (let i = 0; i < localStorage.length; i++) {
      const key = localStorage.key(i)
      if (key && (key.includes('permission') || key.includes('module') || key.includes('menu'))) {
        keysToRemove.push(key)
      }
    }
    keysToRemove.forEach(key => {
      console.log(`  Removing localStorage: ${key}`)
      localStorage.removeItem(key)
    })
  }
  
  // Clear sessionStorage items
  if (typeof window !== 'undefined') {
    const keysToRemove = []
    for (let i = 0; i < sessionStorage.length; i++) {
      const key = sessionStorage.key(i)
      if (key && (key.includes('permission') || key.includes('module') || key.includes('menu'))) {
        keysToRemove.push(key)
      }
    }
    keysToRemove.forEach(key => {
      console.log(`  Removing sessionStorage: ${key}`)
      sessionStorage.removeItem(key)
    })
  }
  
  console.log("✅ Caches cleared successfully")
}

// Make it available globally for debugging
if (typeof window !== 'undefined') {
  (window as any).clearAllCaches = clearAllCaches
  console.log("💡 Cache clearing function available. Run 'clearAllCaches()' in console to clear all caches.")
}