"use client"

import { useState, useEffect } from "react"
import { zodResolver } from "@hookform/resolvers/zod"
import { useForm } from "react-hook-form"
import * as z from "zod"
import { Loader2, Monitor, Smartphone, Crown, Shield, Building } from "lucide-react"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Textarea } from "@/components/ui/textarea"
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
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs"
import { Badge } from "@/components/ui/badge"
import { Separator } from "@/components/ui/separator"
import { useToast } from "@/components/ui/use-toast"
import { Role, Module } from "@/types/admin.types"
import { roleService } from "@/services/admin/role.service"

const roleFormSchema = z.object({
  code: z.string().min(1, "Role code is required").max(20, "Role code must be 20 characters or less"),
  roleNameEn: z.string().min(1, "Role name is required").max(100, "Role name must be 100 characters or less"),
  description: z.string().optional(),
  isPrincipalRole: z.boolean().default(false),
  isDistributorRole: z.boolean().default(false),
  isAdmin: z.boolean().default(false),
  isWebUser: z.boolean().default(true),
  isAppUser: z.boolean().default(false),
  webMenuData: z.string().optional(),
  mobileMenuData: z.string().optional(),
  isActive: z.boolean().default(true)
})

type RoleFormData = z.infer<typeof roleFormSchema>

interface RoleFormProps {
  roleId?: string | null
  onSuccess: () => void
  onCancel: () => void
}

export function RoleForm({ roleId, onSuccess, onCancel }: RoleFormProps) {
  const { toast } = useToast()
  const [loading, setLoading] = useState(false)
  const [initialLoading, setInitialLoading] = useState(!!roleId)
  const [webModules, setWebModules] = useState<Module[]>([])
  const [mobileModules, setMobileModules] = useState<Module[]>([])
  const [selectedWebModules, setSelectedWebModules] = useState<string[]>([])
  const [selectedMobileModules, setSelectedMobileModules] = useState<string[]>([])

  const isEdit = !!roleId

  const form = useForm<RoleFormData>({
    resolver: zodResolver(roleFormSchema),
    defaultValues: {
      code: "",
      roleNameEn: "",
      description: "",
      isPrincipalRole: false,
      isDistributorRole: false,
      isAdmin: false,
      isWebUser: true,
      isAppUser: false,
      webMenuData: "",
      mobileMenuData: "",
      isActive: true
    }
  })

  // Load modules
  useEffect(() => {
    const loadModules = async () => {
      try {
        const [webMods, mobileMods] = await Promise.all([
          roleService.getAllModules('Web'),
          roleService.getAllModules('Mobile')
        ])
        setWebModules(Array.isArray(webMods) ? webMods : [])
        setMobileModules(Array.isArray(mobileMods) ? mobileMods : [])
      } catch (error) {
        console.error('Failed to load modules:', error)
        setWebModules([])
        setMobileModules([])
        toast({
          title: "Warning",
          description: "Failed to load modules. Some features may not work properly.",
          variant: "destructive",
        })
      }
    }

    loadModules()
  }, [toast])

  // Load role data for editing
  useEffect(() => {
    if (!roleId) return

    const loadRole = async () => {
      try {
        setInitialLoading(true)
        const role = await roleService.getRoleById(roleId)
        
        // Populate form with existing data
        form.reset({
          code: role.code,
          roleNameEn: role.roleNameEn,
          description: "",
          isPrincipalRole: role.isPrincipalRole,
          isDistributorRole: role.isDistributorRole,
          isAdmin: role.isAdmin,
          isWebUser: role.isWebUser,
          isAppUser: role.isAppUser,
          webMenuData: role.webMenuData || "",
          mobileMenuData: role.mobileMenuData || "",
          isActive: role.isActive
        })

        // Parse existing menu data to set selected modules
        if (role.webMenuData) {
          try {
            const webMenu = JSON.parse(role.webMenuData)
            // Extract module UIDs from menu data
            setSelectedWebModules(webMenu.modules || [])
          } catch (e) {
            console.warn('Failed to parse web menu data')
          }
        }

        if (role.mobileMenuData) {
          try {
            const mobileMenu = JSON.parse(role.mobileMenuData)
            setSelectedMobileModules(mobileMenu.modules || [])
          } catch (e) {
            console.warn('Failed to parse mobile menu data')
          }
        }
      } catch (error) {
        console.error('Failed to load role:', error)
        toast({
          title: "Error",
          description: "Failed to load role data. Please try again.",
          variant: "destructive",
        })
        onCancel()
      } finally {
        setInitialLoading(false)
      }
    }

    loadRole()
  }, [roleId, form, toast, onCancel])

  const onSubmit = async (data: RoleFormData) => {
    try {
      setLoading(true)

      // Validate role type selection
      if (!data.isPrincipalRole && !data.isDistributorRole) {
        toast({
          title: "Validation Error",
          description: "Please select at least one role type (Principal or Distributor).",
          variant: "destructive",
        })
        return
      }

      // Validate platform selection
      if (!data.isWebUser && !data.isAppUser) {
        toast({
          title: "Validation Error",
          description: "Please select at least one platform (Web or Mobile).",
          variant: "destructive",
        })
        return
      }

      // Build menu data
      const webMenuData = data.isWebUser ? JSON.stringify({
        modules: selectedWebModules
      }) : ""

      const mobileMenuData = data.isAppUser ? JSON.stringify({
        modules: selectedMobileModules
      }) : ""

      const roleData: Partial<Role> = {
        ...data,
        uid: isEdit ? roleId : `ROLE_${Date.now()}`,
        webMenuData,
        mobileMenuData,
        createdDate: isEdit ? undefined : new Date().toISOString(),
        modifiedDate: isEdit ? new Date().toISOString() : undefined
      }

      if (isEdit) {
        await roleService.updateRole(roleData as Role)
      } else {
        await roleService.createRole(roleData)
      }

      onSuccess()
    } catch (error) {
      console.error('Failed to save role:', error)
      toast({
        title: "Error",
        description: `Failed to ${isEdit ? 'update' : 'create'} role. Please try again.`,
        variant: "destructive",
      })
    } finally {
      setLoading(false)
    }
  }

  const handleWebModuleToggle = (moduleUid: string) => {
    setSelectedWebModules(prev => 
      prev.includes(moduleUid) 
        ? prev.filter(uid => uid !== moduleUid)
        : [...prev, moduleUid]
    )
  }

  const handleMobileModuleToggle = (moduleUid: string) => {
    setSelectedMobileModules(prev => 
      prev.includes(moduleUid) 
        ? prev.filter(uid => uid !== moduleUid)
        : [...prev, moduleUid]
    )
  }

  if (initialLoading) {
    return (
      <div className="flex items-center justify-center py-12">
        <Loader2 className="h-8 w-8 animate-spin" />
        <span className="ml-2">Loading role data...</span>
      </div>
    )
  }

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-6">
        <Tabs defaultValue="basic" className="w-full">
          <TabsList className="grid w-full grid-cols-3">
            <TabsTrigger value="basic">Basic Info</TabsTrigger>
            <TabsTrigger value="permissions">Type & Permissions</TabsTrigger>
            <TabsTrigger value="modules">Module Access</TabsTrigger>
          </TabsList>

          {/* Basic Information Tab */}
          <TabsContent value="basic" className="space-y-4">
            <Card>
              <CardHeader>
                <CardTitle>Basic Information</CardTitle>
                <CardDescription>
                  Essential role information and identification.
                </CardDescription>
              </CardHeader>
              <CardContent className="space-y-4">
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <FormField
                    control={form.control}
                    name="code"
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>Role Code *</FormLabel>
                        <FormControl>
                          <Input {...field} placeholder="ROLE_MANAGER" />
                        </FormControl>
                        <FormDescription>
                          Unique identifier for the role
                        </FormDescription>
                        <FormMessage />
                      </FormItem>
                    )}
                  />

                  <FormField
                    control={form.control}
                    name="roleNameEn"
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>Role Name *</FormLabel>
                        <FormControl>
                          <Input {...field} placeholder="Manager" />
                        </FormControl>
                        <FormDescription>
                          Display name for the role
                        </FormDescription>
                        <FormMessage />
                      </FormItem>
                    )}
                  />
                </div>

                <FormField
                  control={form.control}
                  name="description"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Description</FormLabel>
                      <FormControl>
                        <Textarea 
                          {...field} 
                          placeholder="Brief description of the role and its responsibilities..."
                          rows={3}
                        />
                      </FormControl>
                      <FormDescription>
                        Optional description of the role's purpose
                      </FormDescription>
                      <FormMessage />
                    </FormItem>
                  )}
                />

                <FormField
                  control={form.control}
                  name="isActive"
                  render={({ field }) => (
                    <FormItem className="flex flex-row items-center justify-between rounded-lg border p-4">
                      <div className="space-y-0.5">
                        <FormLabel className="text-base">Active Status</FormLabel>
                        <FormDescription>
                          Whether this role is currently active and can be assigned
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
          </TabsContent>

          {/* Type & Permissions Tab */}
          <TabsContent value="permissions" className="space-y-4">
            <Card>
              <CardHeader>
                <CardTitle>Role Type & Permissions</CardTitle>
                <CardDescription>
                  Configure role type, platform access, and administrative privileges.
                </CardDescription>
              </CardHeader>
              <CardContent className="space-y-6">
                {/* Role Type */}
                <div className="space-y-4">
                  <Label className="text-base font-medium">Role Type</Label>
                  <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                    <FormField
                      control={form.control}
                      name="isPrincipalRole"
                      render={({ field }) => (
                        <FormItem className="flex flex-row items-center space-x-3 space-y-0 rounded-md border p-4">
                          <FormControl>
                            <input
                              type="checkbox"
                              checked={field.value}
                              onChange={field.onChange}
                              className="h-4 w-4"
                            />
                          </FormControl>
                          <div className="space-y-1 leading-none">
                            <FormLabel className="flex items-center gap-2">
                              <Crown className="h-4 w-4 text-blue-600" />
                              Principal Role
                            </FormLabel>
                            <FormDescription>
                              High-level organizational role with broad permissions
                            </FormDescription>
                          </div>
                        </FormItem>
                      )}
                    />

                    <FormField
                      control={form.control}
                      name="isDistributorRole"
                      render={({ field }) => (
                        <FormItem className="flex flex-row items-center space-x-3 space-y-0 rounded-md border p-4">
                          <FormControl>
                            <input
                              type="checkbox"
                              checked={field.value}
                              onChange={field.onChange}
                              className="h-4 w-4"
                            />
                          </FormControl>
                          <div className="space-y-1 leading-none">
                            <FormLabel className="flex items-center gap-2">
                              <Building className="h-4 w-4 text-green-600" />
                              Distributor Role
                            </FormLabel>
                            <FormDescription>
                              Role for distributors with specific business permissions
                            </FormDescription>
                          </div>
                        </FormItem>
                      )}
                    />
                  </div>
                </div>

                <Separator />

                {/* Platform Access */}
                <div className="space-y-4">
                  <Label className="text-base font-medium">Platform Access</Label>
                  <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                    <FormField
                      control={form.control}
                      name="isWebUser"
                      render={({ field }) => (
                        <FormItem className="flex flex-row items-center space-x-3 space-y-0 rounded-md border p-4">
                          <FormControl>
                            <input
                              type="checkbox"
                              checked={field.value}
                              onChange={field.onChange}
                              className="h-4 w-4"
                            />
                          </FormControl>
                          <div className="space-y-1 leading-none">
                            <FormLabel className="flex items-center gap-2">
                              <Monitor className="h-4 w-4 text-blue-600" />
                              Web Application
                            </FormLabel>
                            <FormDescription>
                              Access to the web-based administrative interface
                            </FormDescription>
                          </div>
                        </FormItem>
                      )}
                    />

                    <FormField
                      control={form.control}
                      name="isAppUser"
                      render={({ field }) => (
                        <FormItem className="flex flex-row items-center space-x-3 space-y-0 rounded-md border p-4">
                          <FormControl>
                            <input
                              type="checkbox"
                              checked={field.value}
                              onChange={field.onChange}
                              className="h-4 w-4"
                            />
                          </FormControl>
                          <div className="space-y-1 leading-none">
                            <FormLabel className="flex items-center gap-2">
                              <Smartphone className="h-4 w-4 text-green-600" />
                              Mobile Application
                            </FormLabel>
                            <FormDescription>
                              Access to the mobile application interface
                            </FormDescription>
                          </div>
                        </FormItem>
                      )}
                    />
                  </div>
                </div>

                <Separator />

                {/* Administrative Privileges */}
                <div className="space-y-4">
                  <Label className="text-base font-medium">Administrative Privileges</Label>
                  <FormField
                    control={form.control}
                    name="isAdmin"
                    render={({ field }) => (
                      <FormItem className="flex flex-row items-center justify-between rounded-lg border p-4">
                        <div className="space-y-0.5">
                          <FormLabel className="flex items-center gap-2 text-base">
                            <Shield className="h-4 w-4 text-red-600" />
                            Administrator Privileges
                          </FormLabel>
                          <FormDescription>
                            Grants full administrative access to the system including user management, 
                            role configuration, and system settings. Use with caution.
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
                  {form.watch("isAdmin") && (
                    <div className="bg-red-50 border border-red-200 rounded-lg p-4">
                      <div className="flex items-center gap-2 text-red-800">
                        <Shield className="h-4 w-4" />
                        <span className="font-medium">Warning: Administrator Role</span>
                      </div>
                      <p className="text-red-700 text-sm mt-1">
                        This role will have complete administrative access to the system. 
                        Only assign to trusted personnel.
                      </p>
                    </div>
                  )}
                </div>
              </CardContent>
            </Card>
          </TabsContent>

          {/* Module Access Tab */}
          <TabsContent value="modules" className="space-y-4">
            <Card>
              <CardHeader>
                <CardTitle>Module Access Configuration</CardTitle>
                <CardDescription>
                  Select which modules this role can access on each platform.
                </CardDescription>
              </CardHeader>
              <CardContent className="space-y-6">
                {/* Web Modules */}
                {form.watch("isWebUser") && (
                  <div className="space-y-4">
                    <div className="flex items-center gap-2">
                      <Monitor className="h-5 w-5 text-blue-600" />
                      <Label className="text-base font-medium">Web Platform Modules</Label>
                      <Badge variant="secondary">{selectedWebModules.length} selected</Badge>
                    </div>
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
                      {(webModules || []).map(module => (
                        <div
                          key={module.uid}
                          className={`flex items-center space-x-3 rounded-md border p-3 cursor-pointer transition-colors ${
                            selectedWebModules.includes(module.uid) 
                              ? 'border-blue-500 bg-blue-50' 
                              : 'border-gray-200 hover:border-gray-300'
                          }`}
                          onClick={() => handleWebModuleToggle(module.uid)}
                        >
                          <input
                            type="checkbox"
                            checked={selectedWebModules.includes(module.uid)}
                            onChange={() => handleWebModuleToggle(module.uid)}
                            className="h-4 w-4"
                          />
                          <div className="flex-1">
                            <Label className="cursor-pointer">{module.moduleNameEn}</Label>
                          </div>
                        </div>
                      ))}
                    </div>
                  </div>
                )}

                {/* Mobile Modules */}
                {form.watch("isAppUser") && (
                  <>
                    {form.watch("isWebUser") && <Separator />}
                    <div className="space-y-4">
                      <div className="flex items-center gap-2">
                        <Smartphone className="h-5 w-5 text-green-600" />
                        <Label className="text-base font-medium">Mobile Platform Modules</Label>
                        <Badge variant="secondary">{selectedMobileModules.length} selected</Badge>
                      </div>
                      <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
                        {(mobileModules || []).map(module => (
                          <div
                            key={module.uid}
                            className={`flex items-center space-x-3 rounded-md border p-3 cursor-pointer transition-colors ${
                              selectedMobileModules.includes(module.uid) 
                                ? 'border-green-500 bg-green-50' 
                                : 'border-gray-200 hover:border-gray-300'
                            }`}
                            onClick={() => handleMobileModuleToggle(module.uid)}
                          >
                            <input
                              type="checkbox"
                              checked={selectedMobileModules.includes(module.uid)}
                              onChange={() => handleMobileModuleToggle(module.uid)}
                              className="h-4 w-4"
                            />
                            <div className="flex-1">
                              <Label className="cursor-pointer">{module.moduleNameEn}</Label>
                            </div>
                          </div>
                        ))}
                      </div>
                    </div>
                  </>
                )}

                {!form.watch("isWebUser") && !form.watch("isAppUser") && (
                  <div className="text-center py-8 text-muted-foreground">
                    <p>Please select at least one platform in the "Type & Permissions" tab to configure module access.</p>
                  </div>
                )}
              </CardContent>
            </Card>
          </TabsContent>
        </Tabs>

        {/* Form Actions */}
        <div className="flex justify-end space-x-2 pt-6 border-t">
          <Button type="button" variant="outline" onClick={onCancel} disabled={loading}>
            Cancel
          </Button>
          <Button type="submit" disabled={loading}>
            {loading && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
            {isEdit ? 'Update Role' : 'Create Role'}
          </Button>
        </div>
      </form>
    </Form>
  )
}