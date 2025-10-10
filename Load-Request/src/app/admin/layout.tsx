import { ReactNode } from "react"
import { AdminHeader } from "@/components/admin/layout/admin-header"
import { AdminSidebar } from "@/components/admin/layout/admin-sidebar"
import { AdminGuard } from "@/components/admin/admin-guard"

interface AdminLayoutProps {
  children: ReactNode
}

export default function AdminLayout({ children }: AdminLayoutProps) {
  return (
    <AdminGuard>
      <div className="min-h-screen bg-gray-50/50">
        <AdminSidebar />
        <div className="pl-64">
          <AdminHeader />
          <main className="p-6">
            {children}
          </main>
        </div>
      </div>
    </AdminGuard>
  )
}

export const metadata = {
  title: "Admin Panel - MULTIPLEX Access Control",
  description: "Administrative panel for managing users, roles, and permissions",
}