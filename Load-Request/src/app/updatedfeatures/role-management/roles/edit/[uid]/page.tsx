"use client"

import React, { useState, useEffect, useCallback } from "react"
import { useRouter, useParams } from "next/navigation"
import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import * as z from "zod"
import { motion, AnimatePresence } from "framer-motion"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Textarea } from "@/components/ui/textarea"
import { Switch } from "@/components/ui/switch"
import { useToast } from "@/components/ui/use-toast"
import { Badge } from "@/components/ui/badge"
import { Checkbox } from "@/components/ui/checkbox"
import { Skeleton } from "@/components/ui/skeleton"
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select"
import {
  ArrowLeft,
  ArrowRight,
  CheckCircle2,
  Crown,
  Shield,
  Building,
  Monitor,
  Smartphone,
  RotateCcw,
  Save,
  AlertCircle
} from "lucide-react"
import { cn } from "@/lib/utils"
import { ModuleHierarchy, Role } from "@/types/admin.types"
import { roleService } from "@/services/admin/role.service"
import { permissionService } from "@/services/admin/permission.service"
import { rolePermissionService } from "@/services/admin/role-permission.service"

const roleFormSchema = z.object({
  code: z.string().min(1, "Role code is required").max(20, "Role code must be 20 characters or less"),
  roleNameEn: z.string().min(1, "Role name is required").max(100, "Role name must be 100 characters or less"),
  roleNameOther: z.string().optional(),
  parentRoleUid: z.string().optional(),
  orgUid: z.string().optional(),
  isPrincipalRole: z.boolean().default(false),
  isDistributorRole: z.boolean().default(false),
  isAdmin: z.boolean().default(false),
  isWebUser: z.boolean().default(true),
  isAppUser: z.boolean().default(false),
  isActive: z.boolean().default(true),
  haveWarehouse: z.boolean().default(false),
  haveVehicle: z.boolean().default(false),
  isForReportsTo: z.boolean().default(false)
})

type RoleFormData = z.infer<typeof roleFormSchema>

interface StepConfig {
  num: number
  label: string
  mobileLabel: string
}

const progressSteps: StepConfig[] = [
  { num: 1, label: "Basic Information", mobileLabel: "Basic" },
  { num: 2, label: "Permissions & Access Configuration", mobileLabel: "Configuration" }
]

export default function EditRolePage() {
  const router = useRouter()
  const params = useParams()
  const uid = params?.uid as string
  const { toast } = useToast()
  
  const [currentStep, setCurrentStep] = useState(1)
  const [loading, setLoading] = useState(false)
  const [initialLoading, setInitialLoading] = useState(true)
  const [webModules, setWebModules] = useState<ModuleHierarchy[]>([])
  const [mobileModules, setMobileModules] = useState<ModuleHierarchy[]>([])
  const [selectedWebModules, setSelectedWebModules] = useState<string[]>([])
  const [selectedMobileModules, setSelectedMobileModules] = useState<string[]>([])
  const [originalRole, setOriginalRole] = useState<Role | null>(null)
  const [availableRoles, setAvailableRoles] = useState<any[]>([])
  const [loadingRoles, setLoadingRoles] = useState(false)

  const form = useForm<RoleFormData>({
    resolver: zodResolver(roleFormSchema),
    defaultValues: {
      code: "",
      roleNameEn: "",
      description: "",
      parentRoleUid: "",
      orgUid: "WINIT",
      isPrincipalRole: false,
      isDistributorRole: false,
      isAdmin: false,
      isWebUser: true,
      isAppUser: false,
      isActive: true,
      haveWarehouse: false,
      haveVehicle: false,
      isForReportsTo: false
    }
  })

  const { register, handleSubmit, formState: { errors }, trigger, watch, setValue, reset } = form
  const formData = watch()

  // Load role data and modules - only on mount or uid change
  useEffect(() => {
    const loadData = async () => {
      if (!uid) return
      
      try {
        setInitialLoading(true)
        
        // Load role data
        const role = await roleService.getRoleById(uid)
        setOriginalRole(role)
        
        // Populate form with existing data
        // Note: Backend uses RoleNameOther for description
        reset({
          code: role.Code || "",
          roleNameEn: role.RoleNameEn || "",
          roleNameOther: role.RoleNameOther || "",
          parentRoleUid: role.ParentRoleUid || "",
          orgUid: role.OrgUid || "WINIT",
          isPrincipalRole: role.IsPrincipalRole || false,
          isDistributorRole: role.IsDistributorRole || false,
          isAdmin: role.IsAdmin || false,
          isWebUser: role.IsWebUser || false,
          isAppUser: role.IsAppUser || false,
          isActive: role.IsActive !== false,
          haveWarehouse: role.HaveWarehouse || false,
          haveVehicle: role.HaveVehicle || false,
          isForReportsTo: role.IsForReportsTo || false
        })

        // Load module hierarchy
        const [webHierarchy, mobileHierarchy] = await Promise.all([
          permissionService['getModuleHierarchy']('Web'),
          permissionService['getModuleHierarchy']('Mobile')
        ])
        
        setWebModules(Array.isArray(webHierarchy) ? webHierarchy : [])
        setMobileModules(Array.isArray(mobileHierarchy) ? mobileHierarchy : [])

        // Load existing permissions
        if (role.IsWebUser) {
          const webPermissions = await rolePermissionService.loadRolePermissions(
            role.UID,
            "Web",
            role.IsPrincipalRole || false
          )
          setSelectedWebModules(webPermissions)
        }

        if (role.IsAppUser) {
          const mobilePermissions = await rolePermissionService.loadRolePermissions(
            role.UID,
            "Mobile",
            role.IsPrincipalRole || false
          )
          setSelectedMobileModules(mobilePermissions)
        }

        // Load available parent roles
        const pagingRequest = roleService.buildRolePagingRequest(1, 100)
        const rolesResponse = await roleService.getRoles(pagingRequest)
        const activeRoles = rolesResponse.pagedData.filter(r => 
          r.IsActive && r.UID !== uid
        )
        setAvailableRoles(activeRoles)
        
      } catch (error) {
        console.error('Failed to load role data:', error)
        toast({
          title: "Error",
          description: "Failed to load role data. Please try again.",
          variant: "destructive",
        })
        router.push("/updatedfeatures/role-management/roles/manage")
      } finally {
        setInitialLoading(false)
        setLoadingRoles(false)
      }
    }

    loadData()
  }, [uid]) // Only depend on uid to avoid re-renders

  const validateStep = async (step: number): Promise<boolean> => {
    switch (step) {
      case 1:
        return await trigger(['code', 'roleNameEn'])
      case 2:
        if (!(formData.isPrincipalRole || formData.isDistributorRole)) return false
        if (!(formData.isWebUser || formData.isAppUser)) return false
        if (formData.isWebUser && selectedWebModules.length === 0) return false
        if (formData.isAppUser && selectedMobileModules.length === 0) return false
        return true
      default:
        return true
    }
  }

  const handleNext = async () => {
    const isValid = await validateStep(currentStep)
    
    if (currentStep === 2 && !isValid) {
      if (!(formData.isPrincipalRole || formData.isDistributorRole)) {
        toast({
          title: "Validation Error",
          description: "Please select at least one role type (Principal or Distributor).",
          variant: "destructive",
        })
        return
      }
      if (!(formData.isWebUser || formData.isAppUser)) {
        toast({
          title: "Validation Error",
          description: "Please select at least one platform (Web or Mobile).",
          variant: "destructive",
        })
        return
      }
      if (formData.isWebUser && selectedWebModules.length === 0) {
        toast({
          title: "Validation Error",
          description: "Please select at least one web module.",
          variant: "destructive",
        })
        return
      }
      if (formData.isAppUser && selectedMobileModules.length === 0) {
        toast({
          title: "Validation Error",
          description: "Please select at least one mobile module.",
          variant: "destructive",
        })
        return
      }
    }

    if (isValid && currentStep < progressSteps.length) {
      setCurrentStep(currentStep + 1)
    }
  }

  const handlePrevious = () => {
    if (currentStep > 1) {
      setCurrentStep(currentStep - 1)
    }
  }

  const handleReset = () => {
    if (originalRole) {
      reset({
        code: originalRole.Code || "",
        roleNameEn: originalRole.RoleNameEn || "",
        description: originalRole.Description || "",
        isPrincipalRole: originalRole.IsPrincipalRole || false,
        isDistributorRole: originalRole.IsDistributorRole || false,
        isAdmin: originalRole.IsAdmin || false,
        isWebUser: originalRole.IsWebUser || false,
        isAppUser: originalRole.IsAppUser || false,
        isActive: originalRole.IsActive !== false
      })
      setCurrentStep(1)
      // Reset module selections to original
      loadExistingModuleSelections(originalRole)
    }
  }

  const handleWebModuleToggle = (moduleUID: string) => {
    setSelectedWebModules(prev => 
      prev.includes(moduleUID) 
        ? prev.filter(uid => uid !== moduleUID)
        : [...prev, moduleUID]
    )
  }

  const handleMobileModuleToggle = (moduleUID: string) => {
    setSelectedMobileModules(prev => 
      prev.includes(moduleUID) 
        ? prev.filter(uid => uid !== moduleUID)
        : [...prev, moduleUID]
    )
  }

  const onSubmit = async (data: RoleFormData) => {
    try {
      setLoading(true)

      // Map form data to backend expected format - include all fields
      const updatedRole = {
        ...originalRole,
        UID: uid,
        Code: data.code,
        RoleNameEn: data.roleNameEn,
        RoleNameOther: data.roleNameOther || "", // Backend field for description/alias
        ParentRoleUid: data.parentRoleUid || null,
        OrgUid: data.orgUid || "WINIT",
        IsPrincipalRole: data.isPrincipalRole,
        IsDistributorRole: data.isDistributorRole,
        IsAdmin: data.isAdmin,
        IsWebUser: data.isWebUser,
        IsAppUser: data.isAppUser,
        IsActive: data.isActive,
        HaveWarehouse: data.haveWarehouse,
        HaveVehicle: data.haveVehicle,
        IsForReportsTo: data.isForReportsTo,
        ModifiedBy: "ADMIN",
        ModifiedTime: new Date().toISOString()
      }

      // Use the new service to update role with permissions
      await rolePermissionService.updateRoleWithPermissions(
        updatedRole as Role,
        selectedWebModules,
        selectedMobileModules,
        webModules,
        mobileModules
      )
      
      toast({
        title: "Success",
        description: "Role and permissions updated successfully!",
      })
      
      router.push("/updatedfeatures/role-management/roles/manage")
    } catch (error) {
      console.error('Failed to update role:', error)
      toast({
        title: "Error",
        description: "Failed to update role. Please try again.",
        variant: "destructive",
      })
    } finally {
      setLoading(false)
    }
  }

  const getStepStatus = (stepIndex: number) => {
    if (stepIndex < currentStep) return { completed: true, current: false }
    if (stepIndex === currentStep) return { completed: false, current: true }
    return { completed: false, current: false }
  }

  const stepperSteps = progressSteps.map((step, index) => ({
    ...step,
    ...getStepStatus(index)
  }))

  const renderStepContent = () => {
    switch (currentStep) {
      case 1:
        return (
          <motion.div
            initial={{ opacity: 0, x: 20 }}
            animate={{ opacity: 1, x: 0 }}
            exit={{ opacity: 0, x: -20 }}
            className="space-y-6"
          >
            <div>
              <h2 className="text-xl font-semibold text-gray-900 mb-1">Basic Information</h2>
              <p className="text-gray-600">Update role information and identification details.</p>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
              <div className="space-y-2">
                <Label htmlFor="code">Role Code *</Label>
                <Input
                  id="code"
                  placeholder="ROLE_MANAGER"
                  {...register('code')}
                  className={errors.code ? 'border-red-300' : ''}
                />
                {errors.code && (
                  <p className="text-sm text-red-600">{errors.code.message}</p>
                )}
                <p className="text-xs text-gray-500">
                  Unique identifier for the role (max 20 characters)
                </p>
              </div>

              <div className="space-y-2">
                <Label htmlFor="roleNameEn">Role Name *</Label>
                <Input
                  id="roleNameEn"
                  placeholder="Manager"
                  {...register('roleNameEn')}
                  className={errors.roleNameEn ? 'border-red-300' : ''}
                />
                {errors.roleNameEn && (
                  <p className="text-sm text-red-600">{errors.roleNameEn.message}</p>
                )}
                <p className="text-xs text-gray-500">
                  Display name for the role (max 100 characters)
                </p>
              </div>
            </div>

            <div className="space-y-2">
              <Label htmlFor="description">Description</Label>
              <Textarea
                id="roleNameOther"
                placeholder="Brief description of the role and its responsibilities..."
                rows={4}
                {...register('roleNameOther')}
              />
              <p className="text-xs text-gray-500">
                Optional description of the role's purpose and responsibilities
              </p>
            </div>

            <div className="space-y-2">
              <Label htmlFor="parentRoleUid">Parent Role</Label>
              <Select 
                value={formData.parentRoleUid || "none"} 
                onValueChange={(value) => {
                  setValue('parentRoleUid', value === "none" ? "" : value)
                }}
                disabled={loadingRoles}
              >
                <SelectTrigger id="parentRoleUid" className="w-full">
                  <SelectValue placeholder={loadingRoles ? "Loading roles..." : "Select parent role (optional)"} />
                </SelectTrigger>
                <SelectContent className="max-h-[300px] overflow-y-auto z-[9999]" position="popper" sideOffset={5}>
                  <SelectItem value="none">No Parent Role</SelectItem>
                  {availableRoles && availableRoles.length > 0 ? (
                    availableRoles.map((role) => (
                      <SelectItem key={role.UID} value={role.UID}>
                        {role.RoleNameEn} ({role.Code})
                      </SelectItem>
                    ))
                  ) : (
                    !loadingRoles && (
                      <div className="px-2 py-1.5 text-sm text-gray-500">
                        No roles available
                      </div>
                    )
                  )}
                </SelectContent>
              </Select>
              <p className="text-xs text-gray-500">
                Select a parent role to establish hierarchy (optional). 
                {loadingRoles && " Loading..."}
                {!loadingRoles && availableRoles.length === 0 && " No roles available."}
                {!loadingRoles && availableRoles.length > 0 && ` ${availableRoles.length} roles available.`}
              </p>
            </div>

            <div className="flex items-center justify-between p-4 border rounded-lg bg-gray-50">
              <div className="space-y-0.5">
                <Label className="text-base">Active Status</Label>
                <p className="text-sm text-gray-600">
                  Whether this role is currently active and can be assigned
                </p>
              </div>
              <Switch
                checked={formData.isActive}
                onCheckedChange={(checked) => setValue('isActive', !!checked)}
              />
            </div>
          </motion.div>
        )

      case 2:
        return (
          <motion.div
            initial={{ opacity: 0, x: 20 }}
            animate={{ opacity: 1, x: 0 }}
            exit={{ opacity: 0, x: -20 }}
            className="space-y-8"
          >
            <div>
              <h2 className="text-xl font-semibold text-gray-900 mb-1">Permissions & Access Configuration</h2>
              <p className="text-gray-600">Configure role type, administrative privileges, platform access, and module permissions.</p>
            </div>

            {/* Role Type Section */}
            <div className="space-y-4">
              <Label className="text-lg font-medium text-gray-900">Role Type</Label>
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div 
                  className={`flex items-center space-x-3 rounded-lg border p-4 transition-colors ${
                    formData.isPrincipalRole ? 'border-blue-500 bg-blue-50' : 'border-gray-200'
                  }`}
                >
                  <Checkbox
                    checked={formData.isPrincipalRole}
                    onCheckedChange={(checked) => setValue('isPrincipalRole', !!checked)}
                  />
                  <div className="space-y-1">
                    <Label className="flex items-center gap-2">
                      <Crown className="h-4 w-4 text-blue-600" />
                      Principal Role
                    </Label>
                    <p className="text-sm text-gray-600">
                      High-level organizational role with broad permissions
                    </p>
                  </div>
                </div>

                <div 
                  className={`flex items-center space-x-3 rounded-lg border p-4 transition-colors ${
                    formData.isDistributorRole ? 'border-green-500 bg-green-50' : 'border-gray-200'
                  }`}
                >
                  <Checkbox
                    checked={formData.isDistributorRole}
                    onCheckedChange={(checked) => setValue('isDistributorRole', !!checked)}
                  />
                  <div className="space-y-1">
                    <Label className="flex items-center gap-2">
                      <Building className="h-4 w-4 text-green-600" />
                      Distributor Role
                    </Label>
                    <p className="text-sm text-gray-600">
                      Role for distributors with specific business permissions
                    </p>
                  </div>
                </div>
              </div>
            </div>

            {/* Administrative Privileges Section */}
            <div className="space-y-4">
              <Label className="text-lg font-medium text-gray-900">Administrative Privileges</Label>
              <div 
                className={`flex items-center justify-between rounded-lg border p-4 transition-colors ${
                  formData.isAdmin ? 'border-red-500 bg-red-50' : 'border-gray-200'
                }`}
              >
                <div className="space-y-0.5">
                  <Label className="flex items-center gap-2 text-base">
                    <Shield className="h-4 w-4 text-red-600" />
                    Administrator Privileges
                  </Label>
                  <p className="text-sm text-gray-600">
                    Grants full administrative access to the system. Use with caution.
                  </p>
                </div>
                <Switch
                  checked={formData.isAdmin}
                  onCheckedChange={(checked) => setValue('isAdmin', checked)}
                />
              </div>
              
              {formData.isAdmin && (
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

            {/* Additional Configuration Section */}
            <div className="space-y-4">
              <Label className="text-lg font-medium text-gray-900">Additional Configuration</Label>
              <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                <div className="flex items-center justify-between rounded-lg border p-4">
                  <div className="space-y-0.5">
                    <Label className="text-sm">Warehouse Access</Label>
                    <p className="text-xs text-gray-600">Can manage warehouses</p>
                  </div>
                  <Switch
                    checked={formData.haveWarehouse}
                    onCheckedChange={(checked) => setValue('haveWarehouse', checked)}
                  />
                </div>

                <div className="flex items-center justify-between rounded-lg border p-4">
                  <div className="space-y-0.5">
                    <Label className="text-sm">Vehicle Access</Label>
                    <p className="text-xs text-gray-600">Can manage vehicles</p>
                  </div>
                  <Switch
                    checked={formData.haveVehicle}
                    onCheckedChange={(checked) => setValue('haveVehicle', checked)}
                  />
                </div>

                <div className="flex items-center justify-between rounded-lg border p-4">
                  <div className="space-y-0.5">
                    <Label className="text-sm">Reports To Access</Label>
                    <p className="text-xs text-gray-600">Can configure reporting structure</p>
                  </div>
                  <Switch
                    checked={formData.isForReportsTo}
                    onCheckedChange={(checked) => setValue('isForReportsTo', checked)}
                  />
                </div>
              </div>
            </div>

            {/* Platform Access Section */}
            <div className="space-y-4">
              <Label className="text-lg font-medium text-gray-900">Platform Access</Label>
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div 
                  className={`flex items-center space-x-3 rounded-lg border p-4 transition-colors ${
                    formData.isWebUser ? 'border-blue-500 bg-blue-50' : 'border-gray-200'
                  }`}
                >
                  <Checkbox
                    checked={formData.isWebUser}
                    onCheckedChange={(checked) => setValue('isWebUser', !!checked)}
                  />
                  <div className="space-y-1">
                    <Label className="flex items-center gap-2">
                      <Monitor className="h-4 w-4 text-blue-600" />
                      Web Application
                    </Label>
                    <p className="text-sm text-gray-600">
                      Access to the web-based administrative interface
                    </p>
                  </div>
                </div>

                <div 
                  className={`flex items-center space-x-3 rounded-lg border p-4 transition-colors ${
                    formData.isAppUser ? 'border-green-500 bg-green-50' : 'border-gray-200'
                  }`}
                >
                  <Checkbox
                    checked={formData.isAppUser}
                    onCheckedChange={(checked) => setValue('isAppUser', !!checked)}
                  />
                  <div className="space-y-1">
                    <Label className="flex items-center gap-2">
                      <Smartphone className="h-4 w-4 text-green-600" />
                      Mobile Application
                    </Label>
                    <p className="text-sm text-gray-600">
                      Access to the mobile application interface
                    </p>
                  </div>
                </div>
              </div>

              {(formData.isWebUser || formData.isAppUser) && (
                <div className="bg-blue-50 border border-blue-200 rounded-lg p-4">
                  <div className="flex items-center gap-2 text-blue-800">
                    <Monitor className="h-4 w-4" />
                    <span className="font-medium">Platform Access Configured</span>
                  </div>
                  <p className="text-blue-700 text-sm mt-1">
                    {formData.isWebUser && formData.isAppUser 
                      ? "This role will have access to both web and mobile platforms."
                      : formData.isWebUser 
                      ? "This role will have access to the web platform only."
                      : "This role will have access to the mobile platform only."
                    }
                  </p>
                </div>
              )}
            </div>

            {/* Module Access Section */}
            <div className="space-y-6">
              <Label className="text-lg font-medium text-gray-900">Module Access</Label>
              
              {formData.isWebUser && (
                <div className="space-y-4">
                  <div className="flex items-center gap-2">
                    <Monitor className="h-5 w-5 text-blue-600" />
                    <Label className="text-base font-medium">Web Platform Modules</Label>
                    <Badge variant="secondary">{selectedWebModules.length} selected</Badge>
                  </div>
                  <div className="space-y-4 max-h-96 overflow-y-auto border rounded-lg p-4">
                    {(webModules || []).map(module => (
                      <div key={module.UID} className="space-y-2">
                        <div className="font-medium text-sm text-gray-900 border-b pb-1">
                          {module.ModuleNameEn}
                        </div>
                        {module.children?.map(subModule => (
                          <div key={subModule.UID} className="ml-4 space-y-1">
                            <div className="font-medium text-xs text-gray-700">
                              {subModule.SubModuleNameEn}
                            </div>
                            {subModule.children?.map(page => (
                              <div
                                key={page.UID}
                                className={`ml-4 flex items-center space-x-2 p-2 rounded transition-colors ${
                                  selectedWebModules.includes(page.UID) 
                                    ? 'border border-blue-500 bg-blue-50' 
                                    : 'hover:bg-gray-50'
                                }`}
                              >
                                <Checkbox
                                  checked={selectedWebModules.includes(page.UID)}
                                  onCheckedChange={() => handleWebModuleToggle(page.UID)}
                                />
                                <div className="flex-1">
                                  <Label className="text-xs cursor-pointer">{page.SubSubModuleNameEn}</Label>
                                </div>
                              </div>
                            ))}
                          </div>
                        ))}
                      </div>
                    ))}
                  </div>
                </div>
              )}

              {formData.isAppUser && (
                <div className="space-y-4">
                  <div className="flex items-center gap-2">
                    <Smartphone className="h-5 w-5 text-green-600" />
                    <Label className="text-base font-medium">Mobile Platform Modules</Label>
                    <Badge variant="secondary">{selectedMobileModules.length} selected</Badge>
                  </div>
                  <div className="space-y-4 max-h-96 overflow-y-auto border rounded-lg p-4">
                    {(mobileModules || []).map(module => (
                      <div key={module.UID} className="space-y-2">
                        <div className="font-medium text-sm text-gray-900 border-b pb-1">
                          {module.ModuleNameEn}
                        </div>
                        {module.children?.map(subModule => (
                          <div key={subModule.UID} className="ml-4 space-y-1">
                            <div className="font-medium text-xs text-gray-700">
                              {subModule.SubModuleNameEn}
                            </div>
                            {subModule.children?.map(page => (
                              <div
                                key={page.UID}
                                className={`ml-4 flex items-center space-x-2 p-2 rounded transition-colors ${
                                  selectedMobileModules.includes(page.UID) 
                                    ? 'border border-green-500 bg-green-50' 
                                    : 'hover:bg-gray-50'
                                }`}
                              >
                                <Checkbox
                                  checked={selectedMobileModules.includes(page.UID)}
                                  onCheckedChange={() => handleMobileModuleToggle(page.UID)}
                                />
                                <div className="flex-1">
                                  <Label className="text-xs cursor-pointer">{page.SubSubModuleNameEn}</Label>
                                </div>
                              </div>
                            ))}
                          </div>
                        ))}
                      </div>
                    ))}
                  </div>
                </div>
              )}

              {!formData.isWebUser && !formData.isAppUser && (
                <div className="text-center py-8 text-gray-500 border rounded-lg bg-gray-50">
                  <p>Please select at least one platform above to configure module access.</p>
                </div>
              )}
            </div>
          </motion.div>
        )

      default:
        return null
    }
  }

  if (initialLoading) {
    return (
      <div className="min-h-screen bg-gray-50">
        <div className="px-6 py-4 border-b bg-white">
          <div className="flex items-center justify-between max-w-6xl">
            <Skeleton className="h-8 w-32" />
          </div>
        </div>
        <div className="px-6 py-4 border-b bg-gray-50">
          <div className="flex items-center justify-between max-w-6xl">
            <Skeleton className="h-12 w-full" />
          </div>
        </div>
        <div className="px-6 pb-8">
          <div className="max-w-6xl">
            <div className="bg-white p-6 space-y-6">
              <Skeleton className="h-8 w-64" />
              <Skeleton className="h-32 w-full" />
              <Skeleton className="h-32 w-full" />
            </div>
          </div>
        </div>
      </div>
    )
  }

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <div className="px-6 py-4 border-b bg-white">
        <div className="flex items-center justify-between max-w-6xl">
          <Button
            variant="ghost"
            size="sm"
            onClick={() => router.push("/updatedfeatures/role-management/roles/manage")}
            className="text-gray-600 hover:text-gray-900"
          >
            <ArrowLeft className="h-4 w-4 mr-2" />
            Cancel Edit
          </Button>
          <div className="flex items-center gap-2">
            <AlertCircle className="h-4 w-4 text-amber-500" />
            <span className="text-sm text-gray-600">Editing: {originalRole?.RoleNameEn}</span>
          </div>
        </div>
      </div>

      {/* Progress Section */}
      <div className="px-6 py-4 border-b bg-gray-50">
        <div className="flex items-center justify-between max-w-6xl">
          <div className="flex items-center flex-1">
            {progressSteps.map((step, index) => (
              <React.Fragment key={step.num}>
                <div className="flex items-center">
                  <div
                    className={cn(
                      "w-8 h-8 rounded-full flex items-center justify-center text-sm font-medium transition-all",
                      currentStep > step.num
                        ? "bg-blue-600 text-white"
                        : currentStep === step.num
                        ? "bg-blue-600 text-white ring-2 ring-blue-300 ring-offset-2"
                        : "bg-gray-200 text-gray-500"
                    )}
                  >
                    {currentStep > step.num ? (
                      <CheckCircle2 className="h-4 w-4" />
                    ) : (
                      <span>{step.num}</span>
                    )}
                  </div>
                  <span
                    className={cn(
                      "ml-2 text-sm hidden md:inline",
                      currentStep >= step.num
                        ? "text-gray-900 font-medium"
                        : "text-gray-500"
                    )}
                  >
                    {step.label}
                  </span>
                </div>
                {index < progressSteps.length - 1 && (
                  <div className="flex-1 mx-4">
                    <div className="h-0.5 bg-gray-200">
                      <div
                        className="h-full bg-blue-600 transition-all duration-300"
                        style={{
                          width: currentStep > step.num ? "100%" : "0%"
                        }}
                      />
                    </div>
                  </div>
                )}
              </React.Fragment>
            ))}
          </div>
        </div>
      </div>

      {/* Form Content */}
      <div className="px-6 pb-8">
        <div className="max-w-6xl">
          <div className="bg-white p-6">
            <form>
              <AnimatePresence mode="wait">
                {renderStepContent()}
              </AnimatePresence>

              {/* Navigation */}
              <div className="border-t pt-6 mt-8">
                <div className="flex items-center justify-between">
                  <Button
                    type="button"
                    variant="outline"
                    onClick={handlePrevious}
                    disabled={currentStep === 1}
                    className={cn("h-10", currentStep === 1 && "invisible")}
                  >
                    <ArrowLeft className="h-4 w-4 mr-2" />
                    Previous
                  </Button>

                  <div className="flex items-center gap-3">
                    <Button
                      type="button"
                      variant="ghost"
                      onClick={handleReset}
                      disabled={loading}
                      className="h-10"
                    >
                      <RotateCcw className="h-4 w-4 mr-2" />
                      Reset
                    </Button>

                    {currentStep < progressSteps.length ? (
                      <Button
                        type="button"
                        onClick={handleNext}
                        disabled={loading}
                        className="h-10"
                      >
                        Next
                        <ArrowRight className="h-4 w-4 ml-2" />
                      </Button>
                    ) : (
                      <Button
                        type="button"
                        onClick={handleSubmit(onSubmit)}
                        disabled={loading}
                        className="h-10"
                      >
                        {loading ? (
                          <>
                            <div className="mr-2 h-4 w-4 animate-spin rounded-full border-2 border-current border-t-transparent" />
                            Updating...
                          </>
                        ) : (
                          <>
                            <Save className="mr-2 h-4 w-4" />
                            Update Role
                          </>
                        )}
                      </Button>
                    )}
                  </div>
                </div>
              </div>
            </form>
          </div>
        </div>
      </div>
    </div>
  )
}