"use client"

import { useState, useCallback, useMemo, useEffect } from "react"
import { 
  Save, 
  Download, 
  Trash2, 
  Eye, 
  Plus, 
  Edit, 
  Check, 
  Shield, 
  Crown, 
  Building,
  Monitor,
  Smartphone,
  Copy,
  Star,
  FileText
} from "lucide-react"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Textarea } from "@/components/ui/textarea"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Badge } from "@/components/ui/badge"
import { Separator } from "@/components/ui/separator"
import { 
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog"
import { 
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
  AlertDialogTrigger,
} from "@/components/ui/alert-dialog"
import { useToast } from "@/components/ui/use-toast"
import { permissionService } from "@/services/admin/permission.service"

interface PermissionTemplate {
  uid: string
  name: string
  description?: string
  platform: 'Web' | 'Mobile'
  roleType: 'Principal' | 'Distributor' | 'Admin' | 'Mixed'
  permissions: {
    [pageUid: string]: {
      viewAccess?: boolean
      addAccess?: boolean
      editAccess?: boolean
      deleteAccess?: boolean
      approvalAccess?: boolean
      downloadAccess?: boolean
      fullAccess?: boolean
    }
  }
  createdDate: string
  isSystem: boolean
  usageCount: number
}

interface PermissionTemplatesProps {
  selectedRoleId?: string
  platform: 'Web' | 'Mobile'
  onApplyTemplate: (templateUid: string) => Promise<void>
  onClose: () => void
}

// System templates loaded from API or default fallback
const getSystemTemplates = (platform: 'Web' | 'Mobile'): PermissionTemplate[] => {
  // Default fallback templates for when API is not available
  const defaultTemplates: PermissionTemplate[] = [
    {
      uid: 'SYS_VIEWER',
      name: 'Read-Only Viewer',
      description: 'View-only access to all pages without modification capabilities',
      platform: 'Web',
      roleType: 'Mixed',
      permissions: {},
      createdDate: new Date().toISOString(),
      isSystem: true,
      usageCount: 0
    },
    {
      uid: 'SYS_MANAGER',
      name: 'Department Manager',
      description: 'Full access to departmental functions with approval capabilities',
      platform: 'Web',
      roleType: 'Principal',
      permissions: {},
      createdDate: new Date().toISOString(),
      isSystem: true,
      usageCount: 0
    },
    {
      uid: 'SYS_MOBILE_BASIC',
      name: 'Mobile Basic User',
      description: 'Essential mobile access for field operations',
      platform: 'Mobile',
      roleType: 'Mixed',
      permissions: {},
      createdDate: new Date().toISOString(),
      isSystem: true,
      usageCount: 0
    }
  ]
  
  return defaultTemplates.filter(template => template.platform === platform)
}

export function PermissionTemplates({ 
  selectedRoleId, 
  platform, 
  onApplyTemplate, 
  onClose 
}: PermissionTemplatesProps) {
  const { toast } = useToast()
  const [systemTemplates, setSystemTemplates] = useState<PermissionTemplate[]>([])
  const [customTemplates, setCustomTemplates] = useState<PermissionTemplate[]>([])
  const [loading, setLoading] = useState(true)
  const [isCreateDialogOpen, setIsCreateDialogOpen] = useState(false)
  const [newTemplateName, setNewTemplateName] = useState("")
  const [newTemplateDescription, setNewTemplateDescription] = useState("")
  const [searchQuery, setSearchQuery] = useState("")
  const [selectedTemplate, setSelectedTemplate] = useState<PermissionTemplate | null>(null)

  // Load templates on component mount
  useEffect(() => {
    const loadTemplates = async () => {
      try {
        setLoading(true)
        
        // For now, use default templates since API templates aren't fully implemented
        // In production, this would call: await permissionService.getTemplates(platform)
        setSystemTemplates(getSystemTemplates(platform))
        
        // Load custom templates from localStorage as fallback
        const savedTemplates = localStorage.getItem(`permission_templates_${platform}`)
        if (savedTemplates) {
          setCustomTemplates(JSON.parse(savedTemplates))
        }
      } catch (error) {
        console.error('Failed to load templates:', error)
        // Fallback to default templates
        setSystemTemplates(getSystemTemplates(platform))
      } finally {
        setLoading(false)
      }
    }

    loadTemplates()
  }, [platform])

  // Filter templates by platform and search query
  const filteredTemplates = useMemo(() => {
    const allTemplates = [...systemTemplates, ...customTemplates]
    return allTemplates.filter(template => {
      const matchesPlatform = template.platform === platform
      const matchesSearch = searchQuery === "" || 
        template.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
        template.description?.toLowerCase().includes(searchQuery.toLowerCase()) ||
        template.roleType.toLowerCase().includes(searchQuery.toLowerCase())
      
      return matchesPlatform && matchesSearch
    })
  }, [platform, searchQuery, customTemplates])

  const handleCreateTemplate = useCallback(async () => {
    if (!newTemplateName.trim() || !selectedRoleId) return

    try {
      // Create template using permission service
      const success = await permissionService.createPermissionTemplate(
        newTemplateName.trim(),
        selectedRoleId,
        platform
      )

      if (success) {
        const newTemplate: PermissionTemplate = {
          uid: `CUSTOM_${Date.now()}`,
          name: newTemplateName.trim(),
          description: newTemplateDescription.trim() || undefined,
          platform,
          roleType: 'Mixed', // Would be determined from actual role data
          permissions: {}, // Would contain actual permissions from current role
          createdDate: new Date().toISOString(),
          isSystem: false,
          usageCount: 0
        }

        const updatedTemplates = [...customTemplates, newTemplate]
        setCustomTemplates(updatedTemplates)
        
        // Save to localStorage as backup
        localStorage.setItem(`permission_templates_${platform}`, JSON.stringify(updatedTemplates))
        
        setNewTemplateName("")
        setNewTemplateDescription("")
        setIsCreateDialogOpen(false)

        toast({
          title: "Template Created",
          description: `Permission template "${newTemplate.name}" has been saved.`,
        })
      } else {
        throw new Error('Failed to create template')
      }
    } catch (error) {
      console.error('Template creation error:', error)
      toast({
        title: "Error",
        description: "Failed to create permission template. Please try again.",
        variant: "destructive",
      })
    }
  }, [newTemplateName, newTemplateDescription, selectedRoleId, platform, customTemplates, toast])

  const handleApplyTemplate = useCallback(async (template: PermissionTemplate) => {
    if (!selectedRoleId) return

    try {
      // Apply template using permission service
      if (template.isSystem || template.uid.startsWith('CUSTOM_')) {
        // For custom templates, apply via permission service
        const success = await permissionService.applyPermissionTemplate(
          template.name,
          selectedRoleId,
          platform
        )
        
        if (!success) {
          throw new Error('Failed to apply template')
        }
      } else {
        // For other templates, use the provided callback
        await onApplyTemplate(template.uid)
      }
      
      // Update usage count for custom templates
      if (!template.isSystem) {
        const updatedTemplates = customTemplates.map(t => 
          t.uid === template.uid 
            ? { ...t, usageCount: t.usageCount + 1 }
            : t
        )
        setCustomTemplates(updatedTemplates)
        localStorage.setItem(`permission_templates_${platform}`, JSON.stringify(updatedTemplates))
      }

      toast({
        title: "Template Applied",
        description: `Permissions from "${template.name}" have been applied successfully.`,
      })
      
      onClose()
    } catch (error) {
      console.error('Template application error:', error)
      toast({
        title: "Error",
        description: "Failed to apply permission template. Please try again.",
        variant: "destructive",
      })
    }
  }, [selectedRoleId, platform, onApplyTemplate, customTemplates, toast, onClose])

  const handleDeleteTemplate = useCallback(async (templateUid: string) => {
    try {
      const updatedTemplates = customTemplates.filter(t => t.uid !== templateUid)
      setCustomTemplates(updatedTemplates)
      
      // Update localStorage
      localStorage.setItem(`permission_templates_${platform}`, JSON.stringify(updatedTemplates))
      
      toast({
        title: "Template Deleted",
        description: "Permission template has been deleted successfully.",
      })
    } catch (error) {
      console.error('Template deletion error:', error)
      toast({
        title: "Error",
        description: "Failed to delete permission template. Please try again.",
        variant: "destructive",
      })
    }
  }, [customTemplates, platform, toast])

  const getRoleTypeColor = (roleType: string) => {
    switch (roleType) {
      case 'Principal': return 'text-blue-600'
      case 'Distributor': return 'text-green-600'
      case 'Admin': return 'text-red-600'
      default: return 'text-gray-600'
    }
  }

  const getRoleTypeIcon = (roleType: string) => {
    switch (roleType) {
      case 'Principal': return Crown
      case 'Distributor': return Building
      case 'Admin': return Shield
      default: return FileText
    }
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h3 className="text-lg font-semibold">Permission Templates</h3>
          <p className="text-sm text-muted-foreground">
            Apply pre-configured permission sets or create custom templates for {platform} platform.
          </p>
        </div>
        <div className="flex items-center gap-2">
          <Dialog open={isCreateDialogOpen} onOpenChange={setIsCreateDialogOpen}>
            <DialogTrigger asChild>
              <Button variant="outline" size="sm" disabled={!selectedRoleId}>
                <Save className="mr-2 h-4 w-4" />
                Save Current as Template
              </Button>
            </DialogTrigger>
            <DialogContent>
              <DialogHeader>
                <DialogTitle>Create Permission Template</DialogTitle>
                <DialogDescription>
                  Save the current role permissions as a reusable template.
                </DialogDescription>
              </DialogHeader>
              <div className="space-y-4 py-4">
                <div className="space-y-2">
                  <Label htmlFor="template-name">Template Name *</Label>
                  <Input
                    id="template-name"
                    value={newTemplateName}
                    onChange={(e) => setNewTemplateName(e.target.value)}
                    placeholder="e.g., Department Manager Template"
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="template-description">Description</Label>
                  <Textarea
                    id="template-description"
                    value={newTemplateDescription}
                    onChange={(e) => setNewTemplateDescription(e.target.value)}
                    placeholder="Optional description of this template..."
                    rows={3}
                  />
                </div>
              </div>
              <div className="flex justify-end space-x-2">
                <Button variant="outline" onClick={() => setIsCreateDialogOpen(false)}>
                  Cancel
                </Button>
                <Button onClick={handleCreateTemplate} disabled={!newTemplateName.trim()}>
                  Create Template
                </Button>
              </div>
            </DialogContent>
          </Dialog>
        </div>
      </div>

      {/* Search */}
      <div className="flex items-center gap-4">
        <div className="relative flex-1 max-w-sm">
          <Input
            placeholder="Search templates..."
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
          />
        </div>
        <Badge variant="outline">
          {platform === 'Web' ? <Monitor className="mr-1 h-3 w-3" /> : <Smartphone className="mr-1 h-3 w-3" />}
          {platform} Templates
        </Badge>
      </div>

      {/* Templates Grid */}
      {loading ? (
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          {Array.from({ length: 4 }).map((_, i) => (
            <Card key={i}>
              <CardHeader className="pb-2">
                <div className="h-4 bg-gray-200 rounded animate-pulse mb-2" />
                <div className="h-3 bg-gray-200 rounded animate-pulse w-3/4" />
              </CardHeader>
              <CardContent>
                <div className="h-3 bg-gray-200 rounded animate-pulse mb-2" />
                <div className="h-8 bg-gray-200 rounded animate-pulse" />
              </CardContent>
            </Card>
          ))}
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          {filteredTemplates.map(template => {
          const RoleIcon = getRoleTypeIcon(template.roleType)
          
          return (
            <Card key={template.uid} className="relative">
              <CardHeader className="pb-2">
                <div className="flex items-start justify-between">
                  <div className="flex items-center gap-2">
                    <RoleIcon className={`h-4 w-4 ${getRoleTypeColor(template.roleType)}`} />
                    <CardTitle className="text-base">{template.name}</CardTitle>
                  </div>
                  <div className="flex items-center gap-1">
                    {template.isSystem && (
                      <Badge variant="secondary" className="text-xs">
                        <Star className="mr-1 h-3 w-3" />
                        System
                      </Badge>
                    )}
                    <Badge variant="outline" className="text-xs">
                      {template.usageCount} uses
                    </Badge>
                  </div>
                </div>
                {template.description && (
                  <CardDescription className="text-sm">
                    {template.description}
                  </CardDescription>
                )}
              </CardHeader>
              
              <CardContent className="space-y-4">
                <div className="flex items-center justify-between text-sm">
                  <div className="flex items-center gap-2">
                    <Badge variant="outline" className="text-xs">
                      {template.roleType}
                    </Badge>
                    <Badge variant="outline" className="text-xs">
                      {platform === 'Web' ? <Monitor className="mr-1 h-3 w-3" /> : <Smartphone className="mr-1 h-3 w-3" />}
                      {template.platform}
                    </Badge>
                  </div>
                  <span className="text-muted-foreground text-xs">
                    {new Date(template.createdDate).toLocaleDateString()}
                  </span>
                </div>

                <div className="flex items-center justify-between pt-2 border-t">
                  <div className="flex gap-2">
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() => setSelectedTemplate(template)}
                    >
                      <Eye className="mr-2 h-4 w-4" />
                      Preview
                    </Button>
                    {!template.isSystem && (
                      <AlertDialog>
                        <AlertDialogTrigger asChild>
                          <Button variant="outline" size="sm">
                            <Trash2 className="h-4 w-4 text-red-600" />
                          </Button>
                        </AlertDialogTrigger>
                        <AlertDialogContent>
                          <AlertDialogHeader>
                            <AlertDialogTitle>Delete Template</AlertDialogTitle>
                            <AlertDialogDescription>
                              Are you sure you want to delete "{template.name}"? This action cannot be undone.
                            </AlertDialogDescription>
                          </AlertDialogHeader>
                          <AlertDialogFooter>
                            <AlertDialogCancel>Cancel</AlertDialogCancel>
                            <AlertDialogAction onClick={() => handleDeleteTemplate(template.uid)}>
                              Delete
                            </AlertDialogAction>
                          </AlertDialogFooter>
                        </AlertDialogContent>
                      </AlertDialog>
                    )}
                  </div>
                  <Button
                    size="sm"
                    onClick={() => handleApplyTemplate(template)}
                    disabled={!selectedRoleId}
                  >
                    <Download className="mr-2 h-4 w-4" />
                    Apply
                  </Button>
                </div>
              </CardContent>
            </Card>
          )
        })}
        </div>
      )}

      {!loading && filteredTemplates.length === 0 && (
        <Card>
          <CardContent className="text-center py-12">
            <FileText className="h-12 w-12 text-muted-foreground mx-auto mb-4" />
            <h3 className="text-lg font-semibold mb-2">No Templates Found</h3>
            <p className="text-muted-foreground mb-4">
              {searchQuery ? 
                "No templates match your search criteria." :
                `No permission templates available for ${platform} platform.`
              }
            </p>
            {selectedRoleId && (
              <Button onClick={() => setIsCreateDialogOpen(true)}>
                <Save className="mr-2 h-4 w-4" />
                Create Your First Template
              </Button>
            )}
          </CardContent>
        </Card>
      )}

      {/* Template Preview Dialog */}
      <Dialog open={!!selectedTemplate} onOpenChange={() => setSelectedTemplate(null)}>
        <DialogContent className="max-w-2xl">
          <DialogHeader>
            <DialogTitle>Template Preview: {selectedTemplate?.name}</DialogTitle>
            <DialogDescription>
              Preview the permissions included in this template.
            </DialogDescription>
          </DialogHeader>
          
          {selectedTemplate && (
            <div className="space-y-4">
              <div className="grid grid-cols-2 gap-4 p-4 bg-gray-50 rounded-lg">
                <div>
                  <Label className="text-xs text-muted-foreground">Role Type</Label>
                  <div className="flex items-center gap-2 mt-1">
                    {(() => {
                      const Icon = getRoleTypeIcon(selectedTemplate.roleType)
                      return <Icon className={`h-4 w-4 ${getRoleTypeColor(selectedTemplate.roleType)}`} />
                    })()}
                    <span className="font-medium">{selectedTemplate.roleType}</span>
                  </div>
                </div>
                <div>
                  <Label className="text-xs text-muted-foreground">Platform</Label>
                  <div className="flex items-center gap-2 mt-1">
                    {selectedTemplate.platform === 'Web' ? 
                      <Monitor className="h-4 w-4" /> : 
                      <Smartphone className="h-4 w-4" />
                    }
                    <span className="font-medium">{selectedTemplate.platform}</span>
                  </div>
                </div>
                <div>
                  <Label className="text-xs text-muted-foreground">Created</Label>
                  <div className="font-medium mt-1">
                    {new Date(selectedTemplate.createdDate).toLocaleDateString()}
                  </div>
                </div>
                <div>
                  <Label className="text-xs text-muted-foreground">Usage Count</Label>
                  <div className="font-medium mt-1">{selectedTemplate.usageCount} times</div>
                </div>
              </div>

              {selectedTemplate.description && (
                <div>
                  <Label className="text-sm font-medium">Description</Label>
                  <p className="text-sm text-muted-foreground mt-1">
                    {selectedTemplate.description}
                  </p>
                </div>
              )}

              <div>
                <Label className="text-sm font-medium">Permission Summary</Label>
                <div className="mt-2 p-4 border rounded-lg">
                  <p className="text-sm text-muted-foreground">
                    This template contains permission configurations for {Object.keys(selectedTemplate.permissions).length} pages.
                    {selectedTemplate.isSystem && " This is a system-provided template that follows best practices for role-based access control."}
                  </p>
                </div>
              </div>

              <div className="flex justify-end space-x-2 pt-4 border-t">
                <Button variant="outline" onClick={() => setSelectedTemplate(null)}>
                  Close
                </Button>
                <Button 
                  onClick={() => {
                    handleApplyTemplate(selectedTemplate)
                    setSelectedTemplate(null)
                  }}
                  disabled={!selectedRoleId}
                >
                  <Download className="mr-2 h-4 w-4" />
                  Apply Template
                </Button>
              </div>
            </div>
          )}
        </DialogContent>
      </Dialog>

      {/* Actions */}
      <div className="flex justify-end pt-4 border-t">
        <Button variant="outline" onClick={onClose}>
          Close
        </Button>
      </div>
    </div>
  )
}