"use client"

import { useState, useEffect } from "react"
import { zodResolver } from "@hookform/resolvers/zod"
import { useForm } from "react-hook-form"
import * as z from "zod"
import { Copy, Loader2, Crown, Building, Monitor, Smartphone, Shield } from "lucide-react"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Switch } from "@/components/ui/switch"
import { 
  Form, 
  FormControl, 
  FormDescription, 
  FormField, 
  FormItem, 
  FormLabel, 
  FormMessage 
} from "@/components/ui/form"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Badge } from "@/components/ui/badge"
import { Separator } from "@/components/ui/separator"
import { useToast } from "@/components/ui/use-toast"
import { Role } from "@/types/admin.types"
import { roleService } from "@/services/admin/role.service"
import { permissionService } from "@/services/admin/permission.service"

const cloneRoleSchema = z.object({
  newRoleName: z.string().min(1, "Role name is required").max(100, "Role name must be 100 characters or less"),
  newRoleCode: z.string().min(1, "Role code is required").max(20, "Role code must be 20 characters or less"),
  copyPermissions: z.boolean().default(true),
  copyWebPermissions: z.boolean().default(true),
  copyMobilePermissions: z.boolean().default(true),
  makeActive: z.boolean().default(true)
})

type CloneRoleFormData = z.infer<typeof cloneRoleSchema>

interface RoleCloneDialogProps {
  sourceRoleId?: string | null
  onSuccess: () => void
  onCancel: () => void
}

export function RoleCloneDialog({ sourceRoleId, onSuccess, onCancel }: RoleCloneDialogProps) {
  const { toast } = useToast()
  const [loading, setLoading] = useState(false)
  const [initialLoading, setInitialLoading] = useState(!!sourceRoleId)
  const [sourceRole, setSourceRole] = useState<Role | null>(null)
  const [webPermissionCount, setWebPermissionCount] = useState(0)
  const [mobilePermissionCount, setMobilePermissionCount] = useState(0)

  const form = useForm<CloneRoleFormData>({
    resolver: zodResolver(cloneRoleSchema),
    defaultValues: {
      newRoleName: "",
      newRoleCode: "",
      copyPermissions: true,
      copyWebPermissions: true,
      copyMobilePermissions: true,
      makeActive: true
    }
  })

  // Load source role data
  useEffect(() => {
    if (!sourceRoleId) return

    const loadSourceRole = async () => {
      try {
        setInitialLoading(true)
        const role = await roleService.getRoleById(sourceRoleId)
        setSourceRole(role)

        // Set default values based on source role
        form.setValue("newRoleName", `${role.roleNameEn} (Copy)`)
        form.setValue("newRoleCode", `${role.code}_COPY`)

        // Load permission counts
        if (role.isWebUser) {
          try {
            const webPerms = await permissionService.getPermissionsByRole(
              role.uid, 
              'Web', 
              role.isPrincipalRole
            )
            setWebPermissionCount(webPerms.length)
          } catch (error) {
            console.warn('Failed to load web permissions count')
          }
        }

        if (role.isAppUser) {
          try {
            const mobilePerms = await permissionService.getPermissionsByRole(
              role.uid, 
              'Mobile', 
              role.isPrincipalRole
            )
            setMobilePermissionCount(mobilePerms.length)
          } catch (error) {
            console.warn('Failed to load mobile permissions count')
          }
        }

      } catch (error) {
        console.error('Failed to load source role:', error)
        toast({
          title: "Error",
          description: "Failed to load source role data. Please try again.",
          variant: "destructive",
        })
        onCancel()
      } finally {
        setInitialLoading(false)
      }
    }

    loadSourceRole()
  }, [sourceRoleId, form, toast, onCancel])

  const onSubmit = async (data: CloneRoleFormData) => {
    if (!sourceRole) return

    try {
      setLoading(true)

      // Validate unique role code
      const isCodeUnique = await roleService.validateRoleCode(data.newRoleCode)
      if (!isCodeUnique) {
        toast({
          title: "Validation Error",
          description: "Role code already exists. Please choose a different code.",
          variant: "destructive",
        })
        return
      }

      // Clone the role
      const newRole = await roleService.cloneRole(
        sourceRoleId!,
        data.newRoleName,
        data.newRoleCode
      )

      // Set active status
      if (!data.makeActive) {
        await roleService.updateRole({
          ...newRole,
          isActive: false
        })
      }

      // Copy permissions if requested
      if (data.copyPermissions) {
        const permissionPromises = []

        if (data.copyWebPermissions && sourceRole.isWebUser) {
          permissionPromises.push(
            permissionService.copyPermissions(sourceRoleId!, newRole.uid, 'Web')
          )
        }

        if (data.copyMobilePermissions && sourceRole.isAppUser) {
          permissionPromises.push(
            permissionService.copyPermissions(sourceRoleId!, newRole.uid, 'Mobile')
          )
        }

        await Promise.all(permissionPromises)
      }

      toast({
        title: "Success",
        description: `Role "${data.newRoleName}" has been cloned successfully.`,
      })

      onSuccess()
    } catch (error) {
      console.error('Failed to clone role:', error)
      toast({
        title: "Error",
        description: "Failed to clone role. Please try again.",
        variant: "destructive",
      })
    } finally {
      setLoading(false)
    }
  }

  if (initialLoading) {
    return (
      <div className="flex items-center justify-center py-12">
        <Loader2 className="h-8 w-8 animate-spin" />
        <span className="ml-2">Loading source role data...</span>
      </div>
    )
  }

  if (!sourceRole) {
    return (
      <div className="text-center py-8">
        <p className="text-muted-foreground">Source role not found.</p>
        <Button variant="outline" onClick={onCancel} className="mt-4">
          Cancel
        </Button>
      </div>
    )
  }

  return (
    <div className="space-y-6">
      {/* Source Role Info */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Copy className="h-5 w-5" />
            Clone Role: {sourceRole.roleNameEn}
          </CardTitle>
          <CardDescription>
            Create a new role based on the selected role's configuration and permissions.
          </CardDescription>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <Label className="text-sm font-medium">Source Role</Label>
              <div className="mt-1">
                <div className="font-medium">{sourceRole.roleNameEn}</div>
                <div className="text-sm text-muted-foreground">{sourceRole.code}</div>
              </div>
            </div>
            <div>
              <Label className="text-sm font-medium">Attributes</Label>
              <div className="flex flex-wrap gap-1 mt-1">
                {sourceRole.isPrincipalRole && (
                  <Badge variant="outline" className="text-xs">
                    <Crown className="w-3 h-3 mr-1" />
                    Principal
                  </Badge>
                )}
                {sourceRole.isDistributorRole && (
                  <Badge variant="outline" className="text-xs">
                    <Building className="w-3 h-3 mr-1" />
                    Distributor
                  </Badge>
                )}
                {sourceRole.isAdmin && (
                  <Badge variant="destructive" className="text-xs">
                    <Shield className="w-3 h-3 mr-1" />
                    Admin
                  </Badge>
                )}
                {sourceRole.isWebUser && (
                  <Badge variant="secondary" className="text-xs">
                    <Monitor className="w-3 h-3 mr-1" />
                    Web
                  </Badge>
                )}
                {sourceRole.isAppUser && (
                  <Badge variant="secondary" className="text-xs">
                    <Smartphone className="w-3 h-3 mr-1" />
                    Mobile
                  </Badge>
                )}
              </div>
            </div>
          </div>
        </CardContent>
      </Card>

      <Form {...form}>
        <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-6">
          {/* Basic Info */}
          <Card>
            <CardHeader>
              <CardTitle>New Role Information</CardTitle>
              <CardDescription>
                Specify the name and code for the new role.
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <FormField
                control={form.control}
                name="newRoleName"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>New Role Name *</FormLabel>
                    <FormControl>
                      <Input {...field} placeholder="Manager (Copy)" />
                    </FormControl>
                    <FormDescription>
                      Display name for the new role
                    </FormDescription>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <FormField
                control={form.control}
                name="newRoleCode"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>New Role Code *</FormLabel>
                    <FormControl>
                      <Input {...field} placeholder="ROLE_MANAGER_COPY" />
                    </FormControl>
                    <FormDescription>
                      Unique identifier for the new role
                    </FormDescription>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <FormField
                control={form.control}
                name="makeActive"
                render={({ field }) => (
                  <FormItem className="flex flex-row items-center justify-between rounded-lg border p-4">
                    <div className="space-y-0.5">
                      <FormLabel className="text-base">Make Role Active</FormLabel>
                      <FormDescription>
                        Whether the new role should be active and available for assignment
                      </FormDescription>
                    </div>
                    <FormControl>
                      <Switch
                        checked={field.value}
                        onCheckedChange={field.onChange}
                      />
                    </FormControl>
                  </FormItem>
                )}
              />
            </CardContent>
          </Card>

          {/* Permission Copying Options */}
          <Card>
            <CardHeader>
              <CardTitle>Permission Copying</CardTitle>
              <CardDescription>
                Choose which permissions to copy from the source role.
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <FormField
                control={form.control}
                name="copyPermissions"
                render={({ field }) => (
                  <FormItem className="flex flex-row items-center justify-between rounded-lg border p-4">
                    <div className="space-y-0.5">
                      <FormLabel className="text-base">Copy Permissions</FormLabel>
                      <FormDescription>
                        Copy all permissions from the source role to the new role
                      </FormDescription>
                    </div>
                    <FormControl>
                      <Switch
                        checked={field.value}
                        onCheckedChange={field.onChange}
                      />
                    </FormControl>
                  </FormItem>
                )}
              />

              {form.watch("copyPermissions") && (
                <div className="space-y-4 ml-4 border-l-2 border-gray-200 pl-4">
                  {sourceRole.isWebUser && (
                    <FormField
                      control={form.control}
                      name="copyWebPermissions"
                      render={({ field }) => (
                        <FormItem className="flex flex-row items-center justify-between rounded-lg border p-3">
                          <div className="space-y-0.5">
                            <FormLabel className="flex items-center gap-2">
                              <Monitor className="h-4 w-4 text-blue-600" />
                              Copy Web Permissions
                            </FormLabel>
                            <FormDescription>
                              Copy {webPermissionCount} web platform permissions
                            </FormDescription>
                          </div>
                          <FormControl>
                            <Switch
                              checked={field.value}
                              onCheckedChange={field.onChange}
                            />
                          </FormControl>
                        </FormItem>
                      )}
                    />
                  )}

                  {sourceRole.isAppUser && (
                    <FormField
                      control={form.control}
                      name="copyMobilePermissions"
                      render={({ field }) => (
                        <FormItem className="flex flex-row items-center justify-between rounded-lg border p-3">
                          <div className="space-y-0.5">
                            <FormLabel className="flex items-center gap-2">
                              <Smartphone className="h-4 w-4 text-green-600" />
                              Copy Mobile Permissions
                            </FormLabel>
                            <FormDescription>
                              Copy {mobilePermissionCount} mobile platform permissions
                            </FormDescription>
                          </div>
                          <FormControl>
                            <Switch
                              checked={field.value}
                              onCheckedChange={field.onChange}
                            />
                          </FormControl>
                        </FormItem>
                      )}
                    />
                  )}
                </div>
              )}

              {!form.watch("copyPermissions") && (
                <div className="bg-yellow-50 border border-yellow-200 rounded-lg p-4">
                  <div className="flex items-center gap-2 text-yellow-800">
                    <Copy className="h-4 w-4" />
                    <span className="font-medium">Note</span>
                  </div>
                  <p className="text-yellow-700 text-sm mt-1">
                    The new role will be created with the same basic configuration but no permissions. 
                    You'll need to configure permissions separately.
                  </p>
                </div>
              )}
            </CardContent>
          </Card>

          {/* Form Actions */}
          <div className="flex justify-end space-x-2 pt-6 border-t">
            <Button type="button" variant="outline" onClick={onCancel} disabled={loading}>
              Cancel
            </Button>
            <Button type="submit" disabled={loading}>
              {loading && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
              Clone Role
            </Button>
          </div>
        </form>
      </Form>
    </div>
  )
}