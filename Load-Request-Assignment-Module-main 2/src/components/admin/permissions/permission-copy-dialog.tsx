"use client"

import { useState, useCallback, useMemo } from "react"
import { Copy, Monitor, Smartphone, Crown, Building, Shield, CheckCircle2, AlertCircle } from "lucide-react"
import { Button } from "@/components/ui/button"
import { Label } from "@/components/ui/label"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Badge } from "@/components/ui/badge"
import { Separator } from "@/components/ui/separator"
import { 
  Select, 
  SelectContent, 
  SelectItem, 
  SelectTrigger, 
  SelectValue 
} from "@/components/ui/select"
import { Checkbox } from "@/components/ui/checkbox"
import { Alert, AlertDescription } from "@/components/ui/alert"
import { Role } from "@/types/admin.types"

interface PermissionCopyDialogProps {
  roles: Role[]
  sourceRoleId?: string
  platform: 'Web' | 'Mobile'
  onCopy: (sourceRoleId: string, targetRoleId: string) => void
  onCancel: () => void
}

interface CopyOption {
  key: string
  label: string
  description: string
  recommended?: boolean
}

const copyOptions: CopyOption[] = [
  {
    key: 'replace',
    label: 'Replace All Permissions',
    description: 'Replace all existing permissions with permissions from the source role',
    recommended: true
  },
  {
    key: 'merge',
    label: 'Merge Permissions',
    description: 'Add source permissions to existing permissions (union of both)'
  },
  {
    key: 'supplement',
    label: 'Supplement Missing',
    description: 'Only add permissions that don\'t exist in the target role'
  }
]

export function PermissionCopyDialog({ 
  roles, 
  sourceRoleId, 
  platform, 
  onCopy, 
  onCancel 
}: PermissionCopyDialogProps) {
  const [selectedSourceRoleId, setSelectedSourceRoleId] = useState(sourceRoleId || "")
  const [selectedTargetRoleIds, setSelectedTargetRoleIds] = useState<Set<string>>(new Set())
  const [copyMode, setCopyMode] = useState<string>('replace')
  const [confirmUnderstand, setConfirmUnderstand] = useState(false)

  // Filter roles for source selection (exclude currently selected role if any)
  const sourceRoles = useMemo(() => {
    return roles.filter(role => {
      const platformAccess = platform === 'Web' ? role.isWebUser : role.isAppUser
      return platformAccess && role.isActive
    })
  }, [roles, platform])

  // Filter roles for target selection (exclude source role)
  const targetRoles = useMemo(() => {
    return roles.filter(role => {
      const platformAccess = platform === 'Web' ? role.isWebUser : role.isAppUser
      return platformAccess && role.isActive && role.uid !== selectedSourceRoleId
    })
  }, [roles, platform, selectedSourceRoleId])

  const sourceRole = useMemo(() => {
    return roles.find(role => role.uid === selectedSourceRoleId)
  }, [roles, selectedSourceRoleId])

  const handleTargetRoleToggle = useCallback((roleId: string, selected: boolean) => {
    setSelectedTargetRoleIds(prev => {
      const newSet = new Set(prev)
      if (selected) {
        newSet.add(roleId)
      } else {
        newSet.delete(roleId)
      }
      return newSet
    })
  }, [])

  const handleSelectAllTargets = useCallback((selected: boolean) => {
    if (selected) {
      setSelectedTargetRoleIds(new Set(targetRoles.map(role => role.uid)))
    } else {
      setSelectedTargetRoleIds(new Set())
    }
  }, [targetRoles])

  const handleCopyPermissions = useCallback(() => {
    if (!selectedSourceRoleId || selectedTargetRoleIds.size === 0) return

    // For now, we'll copy to each target role individually
    // In a production system, you might want to batch this operation
    Array.from(selectedTargetRoleIds).forEach(targetRoleId => {
      onCopy(selectedSourceRoleId, targetRoleId)
    })
  }, [selectedSourceRoleId, selectedTargetRoleIds, onCopy])

  const getRoleTypeInfo = (role: Role) => {
    const types = []
    if (role.isPrincipalRole) types.push('Principal')
    if (role.isDistributorRole) types.push('Distributor')
    if (role.isAdmin) types.push('Admin')
    return types
  }

  const canProceed = selectedSourceRoleId && selectedTargetRoleIds.size > 0 && confirmUnderstand
  const allTargetsSelected = targetRoles.length > 0 && targetRoles.every(role => selectedTargetRoleIds.has(role.uid))

  return (
    <div className="space-y-6">
      {/* Header */}
      <div>
        <h3 className="text-lg font-semibold flex items-center gap-2">
          <Copy className="h-5 w-5" />
          Copy Permissions
        </h3>
        <p className="text-sm text-muted-foreground mt-1">
          Copy permissions from one role to other roles on the {platform} platform.
        </p>
      </div>

      {/* Platform Info */}
      <Alert>
        <div className="flex items-center gap-2">
          {platform === 'Web' ? <Monitor className="h-4 w-4" /> : <Smartphone className="h-4 w-4" />}
          <AlertDescription>
            Copying permissions for <strong>{platform}</strong> platform only. 
            {platform === 'Web' ? ' Mobile' : ' Web'} permissions will not be affected.
          </AlertDescription>
        </div>
      </Alert>

      {/* Source Role Selection */}
      <Card>
        <CardHeader>
          <CardTitle className="text-base">Source Role</CardTitle>
          <CardDescription>
            Select the role to copy permissions from.
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="space-y-2">
            <Label>Source Role</Label>
            <Select value={selectedSourceRoleId} onValueChange={setSelectedSourceRoleId}>
              <SelectTrigger>
                <SelectValue placeholder="Select a source role" />
              </SelectTrigger>
              <SelectContent>
                {sourceRoles.map(role => (
                  <SelectItem key={role.uid} value={role.uid}>
                    <div className="flex items-center gap-2 w-full">
                      <span>{role.roleNameEn}</span>
                      <div className="flex gap-1 ml-auto">
                        {role.isPrincipalRole && (
                          <Badge variant="outline" className="text-xs">
                            <Crown className="w-3 h-3 mr-1" />
                            P
                          </Badge>
                        )}
                        {role.isDistributorRole && (
                          <Badge variant="outline" className="text-xs">
                            <Building className="w-3 h-3 mr-1" />
                            D
                          </Badge>
                        )}
                        {role.isAdmin && (
                          <Badge variant="destructive" className="text-xs">
                            <Shield className="w-3 h-3 mr-1" />
                            A
                          </Badge>
                        )}
                      </div>
                    </div>
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>

          {sourceRole && (
            <Card className="bg-blue-50 border-blue-200">
              <CardContent className="p-4">
                <div className="space-y-2">
                  <div className="flex items-center justify-between">
                    <span className="font-medium">{sourceRole.roleNameEn}</span>
                    <Badge variant="secondary">{sourceRole.code}</Badge>
                  </div>
                  <div className="flex items-center gap-2">
                    {getRoleTypeInfo(sourceRole).map(type => (
                      <Badge key={type} variant="outline" className="text-xs">
                        {type}
                      </Badge>
                    ))}
                    <Badge variant="secondary" className="text-xs">
                      {platform} Access
                    </Badge>
                  </div>
                </div>
              </CardContent>
            </Card>
          )}
        </CardContent>
      </Card>

      {/* Copy Mode Selection */}
      <Card>
        <CardHeader>
          <CardTitle className="text-base">Copy Mode</CardTitle>
          <CardDescription>
            Choose how permissions should be copied to target roles.
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-3">
          {copyOptions.map(option => (
            <div
              key={option.key}
              className={`border rounded-lg p-3 cursor-pointer transition-colors ${
                copyMode === option.key ? 'border-blue-500 bg-blue-50' : 'border-gray-200 hover:border-gray-300'
              }`}
              onClick={() => setCopyMode(option.key)}
            >
              <div className="flex items-start space-x-3">
                <input
                  type="radio"
                  checked={copyMode === option.key}
                  onChange={() => setCopyMode(option.key)}
                  className="h-4 w-4 mt-0.5"
                />
                <div className="flex-1">
                  <div className="flex items-center gap-2">
                    <span className="font-medium text-sm">{option.label}</span>
                    {option.recommended && (
                      <Badge variant="secondary" className="text-xs">Recommended</Badge>
                    )}
                  </div>
                  <p className="text-xs text-muted-foreground mt-1">
                    {option.description}
                  </p>
                </div>
              </div>
            </div>
          ))}
        </CardContent>
      </Card>

      {/* Target Roles Selection */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center justify-between text-base">
            <span>Target Roles ({targetRoles.length} available)</span>
            {targetRoles.length > 0 && (
              <div className="flex items-center gap-2">
                <Checkbox
                  checked={allTargetsSelected}
                  onCheckedChange={handleSelectAllTargets}
                />
                <span className="text-sm text-muted-foreground">Select All</span>
              </div>
            )}
          </CardTitle>
          <CardDescription>
            Select the roles to copy permissions to.
          </CardDescription>
        </CardHeader>
        <CardContent>
          {targetRoles.length === 0 ? (
            <div className="text-center py-8 text-muted-foreground">
              <p>No target roles available for the selected source role and platform.</p>
            </div>
          ) : (
            <div className="max-h-64 overflow-y-auto space-y-2">
              {targetRoles.map(role => (
                <div
                  key={role.uid}
                  className="flex items-center space-x-3 p-3 rounded border hover:bg-gray-50"
                >
                  <Checkbox
                    checked={selectedTargetRoleIds.has(role.uid)}
                    onCheckedChange={(checked) => handleTargetRoleToggle(role.uid, checked as boolean)}
                  />
                  <div className="flex-1">
                    <div className="flex items-center justify-between">
                      <span className="font-medium text-sm">{role.roleNameEn}</span>
                      <Badge variant="outline" className="text-xs">{role.code}</Badge>
                    </div>
                    <div className="flex items-center gap-1 mt-1">
                      {getRoleTypeInfo(role).map(type => (
                        <Badge key={type} variant="outline" className="text-xs">
                          {type}
                        </Badge>
                      ))}
                    </div>
                  </div>
                </div>
              ))}
            </div>
          )}
        </CardContent>
      </Card>

      {/* Selection Summary */}
      {selectedTargetRoleIds.size > 0 && (
        <Card className="bg-green-50 border-green-200">
          <CardContent className="p-4">
            <div className="flex items-center gap-2">
              <CheckCircle2 className="h-4 w-4 text-green-600" />
              <span className="font-medium text-green-800">
                Ready to copy permissions to {selectedTargetRoleIds.size} role{selectedTargetRoleIds.size !== 1 ? 's' : ''}
              </span>
            </div>
          </CardContent>
        </Card>
      )}

      {/* Warning and Confirmation */}
      {copyMode === 'replace' && selectedTargetRoleIds.size > 0 && (
        <Alert>
          <AlertCircle className="h-4 w-4" />
          <AlertDescription>
            <strong>Warning:</strong> This will replace all existing {platform.toLowerCase()} permissions 
            for the selected target roles. This action cannot be undone.
          </AlertDescription>
        </Alert>
      )}

      {selectedTargetRoleIds.size > 0 && (
        <div className="flex items-center space-x-2">
          <Checkbox
            checked={confirmUnderstand}
            onCheckedChange={setConfirmUnderstand}
          />
          <Label className="text-sm">
            I understand the implications of this action and want to proceed.
          </Label>
        </div>
      )}

      {/* Actions */}
      <div className="flex justify-between pt-4 border-t">
        <Button variant="outline" onClick={onCancel}>
          Cancel
        </Button>
        <Button 
          onClick={handleCopyPermissions}
          disabled={!canProceed}
        >
          Copy to {selectedTargetRoleIds.size} Role{selectedTargetRoleIds.size !== 1 ? 's' : ''}
        </Button>
      </div>
    </div>
  )
}