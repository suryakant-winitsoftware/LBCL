import { Metadata } from 'next'

export const metadata: Metadata = {
  title: 'Organization Management',
  description: 'Manage your organization structure, departments, hierarchy, and settings',
}

export default function OrganizationManagementLayout({
  children,
}: {
  children: React.ReactNode
}) {
  return (
    <div className="organization-management-module">
      {children}
    </div>
  )
}