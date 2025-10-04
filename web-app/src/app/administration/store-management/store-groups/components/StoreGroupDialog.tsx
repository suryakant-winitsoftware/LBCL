"use client";

import React, { useState, useEffect } from "react";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { useToast } from "@/components/ui/use-toast";
import { Loader2 } from "lucide-react";
import { storeGroupService } from "@/services/storeGroupService";
import { IStoreGroup, IStoreGroupType, StoreGroupFormData } from "@/types/storeGroup.types";

interface StoreGroupDialogProps {
  open: boolean;
  onClose: (saved: boolean) => void;
  storeGroup: IStoreGroup | null;
}

export default function StoreGroupDialog({
  open,
  onClose,
  storeGroup
}: StoreGroupDialogProps) {
  const { toast } = useToast();
  const [saving, setSaving] = useState(false);
  const [storeGroupTypes, setStoreGroupTypes] = useState<IStoreGroupType[]>([]);
  const [parentGroups, setParentGroups] = useState<IStoreGroup[]>([]);
  
  const [formData, setFormData] = useState<StoreGroupFormData>({
    UID: storeGroup?.UID || "",
    StoreGroupTypeUID: storeGroup?.StoreGroupTypeUID || "Channel",
    Code: storeGroup?.Code || "",
    Name: storeGroup?.Name || "",
    ParentUID: storeGroup?.ParentUID || null,
    ItemLevel: storeGroup?.ItemLevel || 1,
    Description: ""
  });

  const [errors, setErrors] = useState<Record<string, string>>({});

  useEffect(() => {
    loadStoreGroupTypes();
    loadParentGroups();
  }, []);

  const loadStoreGroupTypes = async () => {
    try {
      const types = await storeGroupService.getStoreGroupTypes();
      setStoreGroupTypes(types);
    } catch (error) {
      console.error("Error loading store group types:", error);
    }
  };

  const loadParentGroups = async () => {
    try {
      const response = await storeGroupService.getAllStoreGroups({
        PageNumber: 1,
        PageSize: 100,
        FilterCriterias: [],
        SortCriterias: [],  // Remove sort criteria to avoid SQL error
        IsCountRequired: false
      });
      // Filter out the current store group if editing
      const groups = response.PagedData.filter(g => g.UID !== storeGroup?.UID);
      setParentGroups(groups);
    } catch (error) {
      console.error("Error loading parent groups:", error);
    }
  };

  const validateForm = (): boolean => {
    const newErrors: Record<string, string> = {};
    
    if (!formData.Code.trim()) {
      newErrors.Code = "Code is required";
    }
    
    if (!formData.Name.trim()) {
      newErrors.Name = "Name is required";
    }
    
    if (!formData.StoreGroupTypeUID) {
      newErrors.StoreGroupTypeUID = "Type is required";
    }
    
    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSave = async () => {
    if (!validateForm()) return;

    setSaving(true);
    try {
      if (storeGroup) {
        // Update existing store group
        const updateData: IStoreGroup = {
          ...storeGroup,
          ...formData,
          ModifiedTime: new Date().toISOString()
        };
        
        const response = await storeGroupService.updateStoreGroup(updateData);
        
        if (response.IsSuccess) {
          toast({
            title: "Success",
            description: "Store group updated successfully"
          });
          onClose(true);
        } else {
          throw new Error(response.Message || "Update failed");
        }
      } else {
        // Create new store group
        const response = await storeGroupService.createStoreGroup(formData);
        
        if (response.IsSuccess) {
          // If hierarchy type is specified, insert hierarchy
          if (formData.StoreGroupTypeUID && formData.UID) {
            await storeGroupService.insertStoreGroupHierarchy(
              formData.StoreGroupTypeUID.toLowerCase(),
              formData.UID
            );
          }
          
          toast({
            title: "Success",
            description: "Store group created successfully"
          });
          onClose(true);
        } else {
          throw new Error(response.Message || "Creation failed");
        }
      }
    } catch (error: any) {
      console.error("Error saving store group:", error);
      toast({
        title: "Error",
        description: error.message || "Failed to save store group",
        variant: "destructive"
      });
    } finally {
      setSaving(false);
    }
  };

  const handleFieldChange = (field: keyof StoreGroupFormData, value: any) => {
    setFormData(prev => ({
      ...prev,
      [field]: value
    }));
    
    // Clear error for this field
    if (errors[field]) {
      setErrors(prev => {
        const newErrors = { ...prev };
        delete newErrors[field];
        return newErrors;
      });
    }

    // Auto-calculate item level based on parent
    if (field === 'ParentUID' && value) {
      const parent = parentGroups.find(p => p.UID === value);
      if (parent) {
        setFormData(prev => ({
          ...prev,
          ItemLevel: parent.ItemLevel + 1
        }));
      }
    } else if (field === 'ParentUID' && !value) {
      setFormData(prev => ({
        ...prev,
        ItemLevel: 1
      }));
    }
  };

  return (
    <Dialog open={open} onOpenChange={() => onClose(false)}>
      <DialogContent className="sm:max-w-[500px]">
        <DialogHeader>
          <DialogTitle>
            {storeGroup ? "Edit Store Group" : "Add New Store Group"}
          </DialogTitle>
          <DialogDescription>
            {storeGroup 
              ? "Update the store group details below"
              : "Enter the details for the new store group"}
          </DialogDescription>
        </DialogHeader>

        <div className="grid gap-4 py-4">
          <div className="grid grid-cols-2 gap-4">
            <div>
              <Label htmlFor="code">Code *</Label>
              <Input
                id="code"
                value={formData.Code}
                onChange={(e) => handleFieldChange('Code', e.target.value)}
                placeholder="e.g., GRP001"
                className={errors.Code ? "border-red-500" : ""}
              />
              {errors.Code && (
                <p className="text-xs text-red-500 mt-1">{errors.Code}</p>
              )}
            </div>

            <div>
              <Label htmlFor="name">Name *</Label>
              <Input
                id="name"
                value={formData.Name}
                onChange={(e) => handleFieldChange('Name', e.target.value)}
                placeholder="e.g., North Region"
                className={errors.Name ? "border-red-500" : ""}
              />
              {errors.Name && (
                <p className="text-xs text-red-500 mt-1">{errors.Name}</p>
              )}
            </div>
          </div>

          <div className="grid grid-cols-2 gap-4">
            <div>
              <Label htmlFor="type">Type *</Label>
              <Select
                value={formData.StoreGroupTypeUID}
                onValueChange={(value) => handleFieldChange('StoreGroupTypeUID', value)}
              >
                <SelectTrigger className={errors.StoreGroupTypeUID ? "border-red-500" : ""}>
                  <SelectValue placeholder="Select type" />
                </SelectTrigger>
                <SelectContent>
                  {storeGroupTypes.map(type => (
                    <SelectItem key={type.UID} value={type.UID}>
                      {type.Name}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
              {errors.StoreGroupTypeUID && (
                <p className="text-xs text-red-500 mt-1">{errors.StoreGroupTypeUID}</p>
              )}
            </div>

            <div>
              <Label htmlFor="level">Item Level</Label>
              <Input
                id="level"
                type="number"
                value={formData.ItemLevel}
                onChange={(e) => handleFieldChange('ItemLevel', parseInt(e.target.value))}
                min="1"
                max="10"
              />
            </div>
          </div>

          <div>
            <Label htmlFor="parent">Parent Group</Label>
            <Select
              value={formData.ParentUID || "none"}
              onValueChange={(value) => handleFieldChange('ParentUID', value === 'none' ? null : value)}
            >
              <SelectTrigger>
                <SelectValue placeholder="Select parent group" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="none">None (Top Level)</SelectItem>
                {parentGroups.map(group => (
                  <SelectItem key={group.UID} value={group.UID}>
                    {group.Name} ({group.Code})
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>

          <div>
            <Label htmlFor="description">Description</Label>
            <Textarea
              id="description"
              value={formData.Description}
              onChange={(e) => handleFieldChange('Description', e.target.value)}
              placeholder="Optional description..."
              rows={3}
            />
          </div>
        </div>

        <DialogFooter>
          <Button variant="outline" onClick={() => onClose(false)}>
            Cancel
          </Button>
          <Button onClick={handleSave} disabled={saving}>
            {saving ? (
              <>
                <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                Saving...
              </>
            ) : (
              storeGroup ? "Update" : "Create"
            )}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}