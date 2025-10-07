"use client"

import { useState } from "react"
import { Button } from "@/components/ui/button"
import { Label } from "@/components/ui/label"
import { Checkbox } from "@/components/ui/checkbox"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Badge } from "@/components/ui/badge"
import { Separator } from "@/components/ui/separator"
import { Crown, Building, Shield, Monitor, Smartphone, CheckCircle, XCircle } from "lucide-react"

interface RoleFiltersProps {
  filters: {
    isPrincipalRole: boolean[]
    isDistributorRole: boolean[]
    isAdmin: boolean[]
    isWebUser: boolean[]
    isAppUser: boolean[]
    isActive: boolean[]
  }
  onFiltersChange: (filters: RoleFiltersProps['filters']) => void
  onClose: () => void
}

export function RoleFilters({ filters, onFiltersChange, onClose }: RoleFiltersProps) {
  const [localFilters, setLocalFilters] = useState(filters)

  const handleRoleTypeChange = (type: keyof typeof localFilters, value: boolean, checked: boolean) => {
    const newValues = checked
      ? [...localFilters[type], value]
      : localFilters[type].filter(v => v !== value)
    
    setLocalFilters(prev => ({ ...prev, [type]: newValues }))
  }

  const handleApplyFilters = () => {
    onFiltersChange(localFilters)
    onClose()
  }

  const handleClearFilters = () => {
    const clearedFilters = {
      isPrincipalRole: [],
      isDistributorRole: [],
      isAdmin: [],
      isWebUser: [],
      isAppUser: [],
      isActive: []
    }
    setLocalFilters(clearedFilters)
    onFiltersChange(clearedFilters)
  }

  const getTotalFilterCount = () => {
    return Object.values(localFilters).reduce((total, arr) => total + arr.length, 0)
  }

  const filterSections = [
    {
      key: 'isPrincipalRole' as const,
      title: 'Role Type - Principal',
      description: 'Filter by principal role designation',
      icon: Crown,
      iconColor: 'text-blue-600',
      options: [
        { value: true, label: 'Principal Roles', description: 'Roles with principal designation' },
        { value: false, label: 'Non-Principal Roles', description: 'Roles without principal designation' }
      ]
    },
    {
      key: 'isDistributorRole' as const,
      title: 'Role Type - Distributor',
      description: 'Filter by distributor role designation',
      icon: Building,
      iconColor: 'text-green-600',
      options: [
        { value: true, label: 'Distributor Roles', description: 'Roles with distributor designation' },
        { value: false, label: 'Non-Distributor Roles', description: 'Roles without distributor designation' }
      ]
    },
    {
      key: 'isAdmin' as const,
      title: 'Administrative Privileges',
      description: 'Filter by administrative access level',
      icon: Shield,
      iconColor: 'text-red-600',
      options: [
        { value: true, label: 'Administrator Roles', description: 'Roles with admin privileges' },
        { value: false, label: 'Regular Roles', description: 'Roles without admin privileges' }
      ]
    },
    {
      key: 'isWebUser' as const,
      title: 'Web Platform Access',
      description: 'Filter by web application access',
      icon: Monitor,
      iconColor: 'text-blue-600',
      options: [
        { value: true, label: 'Web Access', description: 'Roles with web platform access' },
        { value: false, label: 'No Web Access', description: 'Roles without web platform access' }
      ]
    },
    {
      key: 'isAppUser' as const,
      title: 'Mobile Platform Access',
      description: 'Filter by mobile application access',
      icon: Smartphone,
      iconColor: 'text-green-600',
      options: [
        { value: true, label: 'Mobile Access', description: 'Roles with mobile platform access' },
        { value: false, label: 'No Mobile Access', description: 'Roles without mobile platform access' }
      ]
    },
    {
      key: 'isActive' as const,
      title: 'Status',
      description: 'Filter by role active status',
      icon: CheckCircle,
      iconColor: 'text-emerald-600',
      options: [
        { value: true, label: 'Active Roles', description: 'Currently active and assignable roles' },
        { value: false, label: 'Inactive Roles', description: 'Deactivated or disabled roles' }
      ]
    }
  ]

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h3 className="text-lg font-semibold">Filter Roles</h3>
          <p className="text-sm text-muted-foreground">
            Refine the role list using the filters below
          </p>
        </div>
        {getTotalFilterCount() > 0 && (
          <Badge variant="secondary">
            {getTotalFilterCount()} filter{getTotalFilterCount() !== 1 ? 's' : ''} active
          </Badge>
        )}
      </div>

      {/* Filter Sections */}
      <div className="space-y-4 max-h-[60vh] overflow-y-auto">
        {filterSections.map((section, index) => {
          const Icon = section.icon
          const activeFilters = localFilters[section.key]
          
          return (
            <div key={section.key}>
              <Card>
                <CardHeader className="pb-3">
                  <CardTitle className="text-base flex items-center gap-2">
                    <Icon className={`h-4 w-4 ${section.iconColor}`} />
                    {section.title}
                    {activeFilters.length > 0 && (
                      <Badge variant="outline" className="ml-auto">
                        {activeFilters.length}
                      </Badge>
                    )}
                  </CardTitle>
                  <CardDescription>{section.description}</CardDescription>
                </CardHeader>
                <CardContent className="space-y-3">
                  {section.options.map(option => (
                    <div key={String(option.value)} className="flex items-start space-x-3">
                      <Checkbox
                        id={`${section.key}-${option.value}`}
                        checked={activeFilters.includes(option.value)}
                        onCheckedChange={(checked) => 
                          handleRoleTypeChange(section.key, option.value, checked as boolean)
                        }
                        className="mt-0.5"
                      />
                      <div className="flex-1">
                        <Label
                          htmlFor={`${section.key}-${option.value}`}
                          className="cursor-pointer font-medium text-sm"
                        >
                          {option.label}
                        </Label>
                        <p className="text-xs text-muted-foreground mt-0.5">
                          {option.description}
                        </p>
                      </div>
                    </div>
                  ))}
                </CardContent>
              </Card>
              {index < filterSections.length - 1 && <Separator />}
            </div>
          )
        })}
      </div>

      {/* Filter Summary */}
      {getTotalFilterCount() > 0 && (
        <Card className="bg-blue-50 border-blue-200">
          <CardContent className="pt-4">
            <div className="flex items-center gap-2 mb-2">
              <Badge variant="secondary" className="bg-blue-100 text-blue-800">
                Active Filters
              </Badge>
            </div>
            <div className="flex flex-wrap gap-2">
              {Object.entries(localFilters).map(([key, values]) => 
                values.map(value => {
                  const section = filterSections.find(s => s.key === key)
                  const option = section?.options.find(o => o.value === value)
                  if (!option) return null
                  
                  return (
                    <Badge key={`${key}-${value}`} variant="outline" className="text-xs">
                      {option.label}
                    </Badge>
                  )
                })
              )}
            </div>
          </CardContent>
        </Card>
      )}

      {/* Actions */}
      <div className="flex justify-between pt-4 border-t">
        <Button variant="outline" onClick={handleClearFilters}>
          Clear All Filters
        </Button>
        <div className="space-x-2">
          <Button variant="outline" onClick={onClose}>
            Cancel
          </Button>
          <Button onClick={handleApplyFilters}>
            Apply Filters ({getTotalFilterCount()})
          </Button>
        </div>
      </div>
    </div>
  )
}