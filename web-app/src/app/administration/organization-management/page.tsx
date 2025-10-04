import Link from 'next/link'
import { 
  Building2, 
  Users, 
  Network, 
  Tags, 
  DollarSign, 
  Landmark, 
  Calendar,
  ArrowRight 
} from 'lucide-react'

const modules = [
  {
    title: 'Organizations',
    description: 'Create and manage organization units',
    icon: Building2,
    href: '/administration/organization-management/organizations',
    color: 'bg-blue-500',
  },
  {
    title: 'Departments',
    description: 'Manage organizational departments',
    icon: Users,
    href: '/administration/organization-management/departments',
    color: 'bg-green-500',
  },
  {
    title: 'Operational Hierarchy',
    description: 'View and manage organization structure',
    icon: Network,
    href: '/administration/organization-management/hierarchy',
    color: 'bg-purple-500',
  },
  {
    title: 'Organizations by Type',
    description: 'View organizations grouped by types',
    icon: Tags,
    href: '/administration/organization-management/types',
    color: 'bg-orange-500',
  },
  {
    title: 'Currency Management',
    description: 'Manage currencies and exchange rates',
    icon: DollarSign,
    href: '/administration/organization-management/currency',
    color: 'bg-yellow-500',
  },
  {
    title: 'Bank Details',
    description: 'Manage bank accounts and details',
    icon: Landmark,
    href: '/administration/organization-management/bank-details',
    color: 'bg-indigo-500',
  },
  {
    title: 'Holiday Management',
    description: 'Configure holidays and working days',
    icon: Calendar,
    href: '/administration/organization-management/holidays',
    color: 'bg-red-500',
  },
]

export default function OrganizationManagementPage() {
  return (
    <div className="container mx-auto p-6">
      <div className="mb-8">
        <h1 className="text-4xl font-bold mb-2">Organization Management</h1>
        <p className="text-lg text-muted-foreground">
          Comprehensive tools to manage your organization structure and settings
        </p>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        {modules.map((module) => {
          const Icon = module.icon
          return (
            <Link
              key={module.href}
              href={module.href}
              className="group relative overflow-hidden rounded-lg border bg-card p-6 hover:shadow-lg transition-all duration-200"
            >
              <div className="flex items-start justify-between">
                <div className="flex-1">
                  <div className={`inline-flex p-3 rounded-lg ${module.color} bg-opacity-10 mb-4`}>
                    <Icon className={`h-6 w-6 ${module.color.replace('bg-', 'text-')}`} />
                  </div>
                  <h3 className="font-semibold text-lg mb-2 group-hover:text-primary transition-colors">
                    {module.title}
                  </h3>
                  <p className="text-sm text-muted-foreground">
                    {module.description}
                  </p>
                </div>
                <ArrowRight className="h-5 w-5 text-muted-foreground group-hover:text-primary transition-all group-hover:translate-x-1" />
              </div>
            </Link>
          )
        })}
      </div>

      <div className="mt-8 p-6 rounded-lg bg-muted/50">
        <h2 className="text-xl font-semibold mb-3">Quick Stats</h2>
        <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
          <div className="bg-background rounded-lg p-4">
            <p className="text-sm text-muted-foreground">Total Organizations</p>
            <p className="text-2xl font-bold">12</p>
          </div>
          <div className="bg-background rounded-lg p-4">
            <p className="text-sm text-muted-foreground">Active Departments</p>
            <p className="text-2xl font-bold">48</p>
          </div>
          <div className="bg-background rounded-lg p-4">
            <p className="text-sm text-muted-foreground">Currencies</p>
            <p className="text-2xl font-bold">3</p>
          </div>
          <div className="bg-background rounded-lg p-4">
            <p className="text-sm text-muted-foreground">Upcoming Holidays</p>
            <p className="text-2xl font-bold">5</p>
          </div>
        </div>
      </div>
    </div>
  )
}