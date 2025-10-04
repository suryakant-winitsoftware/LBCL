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
import { IStoreGroupType } from "@/types/storeGroup.types";

interface StoreGroupTypeDialogProps {
  open: boolean;
  onClose: (saved: boolean) => void;
  storeGroupType: IStoreGroupType | null;
}

interface FormData {
  UID?: string;
  Code: string;
  Name: string;
  Description?: string;
  ParentUID?: string | null;
  LevelNo?: number;
}

export default function StoreGroupTypeDialog({
  open,
  onClose,
  storeGroupType
}: StoreGroupTypeDialogProps) {
  const { toast } = useToast();
  const [saving, setSaving] = useState(false);
  const [parentTypes, setParentTypes] = useState<IStoreGroupType[]>([]);
  
  const [formData, setFormData] = useState<FormData>({
    UID: storeGroupType?.UID || "",
    Code: storeGroupType?.Code || "",
    Name: storeGroupType?.Name || "",
    Description: storeGroupType?.Description || "",
    ParentUID: storeGroupType?.ParentUID || null,
    LevelNo: storeGroupType?.LevelNo || 1
  });

  const [errors, setErrors] = useState<Record<string, string>>({});

  useEffect(() => {
    if (open) {
      loadParentTypes();
      // Reset form when dialog opens
      setFormData({
        UID: storeGroupType?.UID || "",
        Code: storeGroupType?.Code || "",
        Name: storeGroupType?.Name || "",
        Description: storeGroupType?.Description || "",
        ParentUID: storeGroupType?.ParentUID || null,
        LevelNo: storeGroupType?.LevelNo || 1
      });
      setErrors({});
    }
  }, [open, storeGroupType]);

  const loadParentTypes = async () => {
    try {
      const response = await storeGroupService.getAllStoreGroupTypes({
        PageNumber: 1,
        PageSize: 100,
        FilterCriterias: [],
        SortCriterias: [],
        IsCountRequired: false
      });
      // Filter out the current type if editing
      const types = response.PagedData.filter(t => t.UID !== storeGroupType?.UID);
      setParentTypes(types);
    } catch (error) {
      console.error("Error loading parent types:", error);
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
    
    // Check for duplicate codes (basic validation)
    if (!storeGroupType && formData.Code) {
      const existingType = parentTypes.find(t => 
        t.Code.toLowerCase() === formData.Code.toLowerCase()
      );
      if (existingType) {
        newErrors.Code = "Code already exists";
      }
    }
    
    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSave = async () => {
    if (!validateForm()) return;

    setSaving(true);
    try {
      if (storeGroupType) {
        // Update existing store group type
        const updateData: IStoreGroupType = {
          ...storeGroupType,
          ...formData,
          ModifiedTime: new Date().toISOString()
        };
        
        const response = await storeGroupService.updateStoreGroupType(updateData);
        
        if (response.IsSuccess) {
          toast({
            title: "Success",
            description: "Store group type updated successfully"
          });
          onClose(true);
        } else {
          throw new Error(response.Message || "Update failed");
        }
      } else {
        // Create new store group type
        const createData: Partial<IStoreGroupType> = {
          ...formData,
          UID: formData.UID || formData.Code, // Use code as UID if not provided
          IsActive: true,
          CreatedTime: new Date().toISOString(),
          ModifiedTime: new Date().toISOString()
        };
        
        const response = await storeGroupService.createStoreGroupType(createData);
        
        if (response.IsSuccess) {
          toast({
            title: "Success",
            description: "Store group type created successfully"
          });
          onClose(true);
        } else {
          throw new Error(response.Message || "Creation failed");
        }
      }
    } catch (error: any) {
      console.error("Error saving store group type:", error);
      toast({
        title: "Error",
        description: error.message || "Failed to save store group type",
        variant: "destructive"
      });
    } finally {
      setSaving(false);
    }
  };

  const handleFieldChange = (field: keyof FormData, value: any) => {
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

    // Auto-calculate level based on parent
    if (field === 'ParentUID' && value) {
      const parent = parentTypes.find(p => p.UID === value);
      if (parent) {
        setFormData(prev => ({
          ...prev,
          LevelNo: (parent.LevelNo || 1) + 1
        }));
      }
    } else if (field === 'ParentUID' && !value) {
      setFormData(prev => ({
        ...prev,
        LevelNo: 1
      }));
    }

    // Auto-generate UID from Code for new types
    if (field === 'Code' && !storeGroupType) {
      setFormData(prev => ({
        ...prev,
        UID: value
      }));
    }
  };

  return (
    <Dialog open={open} onOpenChange={() => onClose(false)}>
      <DialogContent className="sm:max-w-[500px]">
        <DialogHeader>
          <DialogTitle>
            {storeGroupType ? "Edit Store Group Type" : "Add New Store Group Type"}
          </DialogTitle>
          <DialogDescription>
            {storeGroupType 
              ? "Update the store group type details below"
              : "Enter the details for the new store group type"}
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
                placeholder="e.g., CHANNEL"
                className={errors.Code ? "border-red-500" : ""}
                disabled={!!storeGroupType} // Disable editing code for existing types
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
                placeholder="e.g., Channel"
                className={errors.Name ? "border-red-500" : ""}
              />
              {errors.Name && (
                <p className="text-xs text-red-500 mt-1">{errors.Name}</p>
              )}
            </div>
          </div>

          <div className="grid grid-cols-2 gap-4">
            <div>
              <Label htmlFor="parent">Parent Type</Label>
              <Select
                value={formData.ParentUID || "none"}
                onValueChange={(value) => handleFieldChange('ParentUID', value === 'none' ? null : value)}
              >
                <SelectTrigger>
                  <SelectValue placeholder="Select parent type" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="none">None (Top Level)</SelectItem>
                  {parentTypes.map(type => (
                    <SelectItem key={type.UID} value={type.UID}>
                      {type.Name} ({type.Code})
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            <div>
              <Label htmlFor="level">Level</Label>
              <Input
                id="level"
                type="number"
                value={formData.LevelNo}
                onChange={(e) => handleFieldChange('LevelNo', parseInt(e.target.value))}
                min="1"
                max="10"
                disabled // Auto-calculated based on parent
              />
              <p className="text-xs text-gray-500 mt-1">Auto-calculated from parent</p>
            </div>
          </div>

          {!storeGroupType && (
            <div>
              <Label htmlFor="uid">UID</Label>
              <Input
                id="uid"
                value={formData.UID}
                onChange={(e) => handleFieldChange('UID', e.target.value)}
                placeholder="Auto-generated from code"
                disabled // Auto-generated from code
              />
              <p className="text-xs text-gray-500 mt-1">Auto-generated from code</p>
            </div>
          )}

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
              storeGroupType ? "Update" : "Create"
            )}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}