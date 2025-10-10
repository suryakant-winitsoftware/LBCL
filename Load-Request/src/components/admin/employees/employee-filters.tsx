"use client"

import { useState, useEffect } from "react"
import { Button } from "@/components/ui/button"
import { Label } from "@/components/ui/label"
import { Checkbox } from "@/components/ui/checkbox"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Badge } from "@/components/ui/badge"
import { Separator } from "@/components/ui/separator"
import { Role } from "@/types/admin.types"
import { roleService } from "@/services/admin/role.service"
import { organizationService, Organization } from "@/services/admin/organization.service"

interface EmployeeFiltersProps {
  filters: {
    status: string[]
    roles: string[]
    organizations: string[]
  }
  onFiltersChange: (filters: EmployeeFiltersProps['filters']) => void
  onClose: () => void
}

export function EmployeeFilters({ filters, onFiltersChange, onClose }: EmployeeFiltersProps) {
  const [roles, setRoles] = useState<Role[]>([])
  const [organizations, setOrganizations] = useState<Organization[]>([])

  const [localFilters, setLocalFilters] = useState(filters)

  useEffect(() => {
    const loadData = async () => {
      try {
        // Load roles
        const rolesResponse = await roleService.getRoles({
          pageNumber: 0,
          pageSize: 100,
          isCountRequired: true
        })
        setRoles(rolesResponse.pagedData.filter(role => role.isActive))

        // Load organizations
        const organizations = await organizationService.getActiveOrganizations()
        setOrganizations(organizations)
      } catch (error) {
        console.error('Failed to load data:', error)
      }
    }

    loadData()
  }, [])

  const statusOptions = [
    { value: "Active", label: "Active", color: "bg-green-100 text-green-800" },
    { value: "Inactive", label: "Inactive", color: "bg-red-100 text-red-800" },
    { value: "Pending", label: "Pending", color: "bg-yellow-100 text-yellow-800" }
  ]

  const handleStatusChange = (status: string, checked: boolean) => {
    const newStatus = checked
      ? [...localFilters.status, status]
      : localFilters.status.filter(s => s !== status)
    
    setLocalFilters(prev => ({ ...prev, status: newStatus }))
  }

  const handleRoleChange = (roleId: string, checked: boolean) => {
    const newRoles = checked
      ? [...localFilters.roles, roleId]
      : localFilters.roles.filter(r => r !== roleId)
    
    setLocalFilters(prev => ({ ...prev, roles: newRoles }))
  }

  const handleOrganizationChange = (orgId: string, checked: boolean) => {
    const newOrgs = checked
      ? [...localFilters.organizations, orgId]
      : localFilters.organizations.filter(o => o !== orgId)
    
    setLocalFilters(prev => ({ ...prev, organizations: newOrgs }))
  }

  const handleApplyFilters = () => {
    onFiltersChange(localFilters)
    onClose()
  }

  const handleClearFilters = () => {
    const clearedFilters = {
      status: [],
      roles: [],
      organizations: []
    }
    setLocalFilters(clearedFilters)
    onFiltersChange(clearedFilters)
  }

  const getTotalFilterCount = () => {
    return localFilters.status.length + localFilters.roles.length + localFilters.organizations.length
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h3 className="text-lg font-semibold">Filter Employees</h3>
          <p className="text-sm text-muted-foreground">
            Refine the employee list using the filters below
          </p>
        </div>
        {getTotalFilterCount() > 0 && (
          <Badge variant="secondary">
            {getTotalFilterCount()} filter{getTotalFilterCount() !== 1 ? 's' : ''} active
          </Badge>
        )}
      </div>

      {/* Status Filter */}
      <Card>
        <CardHeader className="pb-3">
          <CardTitle className="text-base">Status</CardTitle>
          <CardDescription>Filter by employee status</CardDescription>
        </CardHeader>
        <CardContent className="space-y-3">
          {statusOptions.map(option => (
            <div key={option.value} className="flex items-center space-x-2">
              <Checkbox
                id={`status-${option.value}`}
                checked={localFilters.status.includes(option.value)}
                onCheckedChange={(checked) => 
                  handleStatusChange(option.value, checked as boolean)
                }
              />
              <Label
                htmlFor={`status-${option.value}`}
                className="flex items-center gap-2 cursor-pointer"
              >
                <Badge className={option.color} variant="secondary">
                  {option.label}
                </Badge>
              </Label>
            </div>
          ))}
        </CardContent>
      </Card>

      <Separator />

      {/* Role Filter */}
      <Card>
        <CardHeader className="pb-3">
          <CardTitle className="text-base">Roles</CardTitle>
          <CardDescription>Filter by assigned roles</CardDescription>
        </CardHeader>
        <CardContent className="space-y-3 max-h-60 overflow-y-auto">
          {roles.map(role => (
            <div key={role.uid} className="flex items-center space-x-2">
              <Checkbox
                id={`role-${role.uid}`}
                checked={localFilters.roles.includes(role.uid)}
                onCheckedChange={(checked) => 
                  handleRoleChange(role.uid, checked as boolean)
                }
              />
              <Label
                htmlFor={`role-${role.uid}`}
                className="cursor-pointer flex-1"
              >
                <div>
                  <span className="font-medium">{role.roleNameEn}</span>
                  <div className="flex gap-1 mt-1">
                    {role.isPrincipalRole && (
                      <Badge variant="outline" className="text-xs">Principal</Badge>
                    )}
                    {role.isDistributorRole && (
                      <Badge variant="outline" className="text-xs">Distributor</Badge>
                    )}
                    {role.isAdmin && (
                      <Badge variant="outline" className="text-xs">Admin</Badge>
                    )}
                  </div>
                </div>
              </Label>
            </div>
          ))}
        </CardContent>
      </Card>

      <Separator />

      {/* Organization Filter */}
      <Card>
        <CardHeader className="pb-3">
          <CardTitle className="text-base">Organizations</CardTitle>
          <CardDescription>Filter by organization assignment</CardDescription>
        </CardHeader>
        <CardContent className="space-y-3">
          {organizations.map(org => (
            <div key={org.uid} className="flex items-center space-x-2">
              <Checkbox
                id={`org-${org.uid}`}
                checked={localFilters.organizations.includes(org.uid)}
                onCheckedChange={(checked) => 
                  handleOrganizationChange(org.uid, checked as boolean)
                }
              />
              <Label
                htmlFor={`org-${org.uid}`}
                className="cursor-pointer"
              >
                {org.name}
              </Label>
            </div>
          ))}
        </CardContent>
      </Card>

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