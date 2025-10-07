"use client";

import React, { useState, useEffect } from "react";
import { Card, CardHeader, CardTitle, CardContent, CardDescription } from "@/components/ui/card";
import { Calendar } from "@/components/ui/calendar";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { useToast } from "@/components/ui/use-toast";
import { ScrollArea } from "@/components/ui/scroll-area";
import { Skeleton } from "@/components/ui/skeleton";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { 
  Calendar as CalendarIcon,
  Plus,
  Edit,
  Trash2,
  Save,
  X,
  Info,
  AlertCircle,
  CheckCircle,
  Globe,
  MapPin,
  Users
} from "lucide-react";
import { holidayService, Holiday as HolidayType } from "@/services/holidayService";
import { authService } from "@/lib/auth-service";
import moment from "moment";
import { cn } from "@/lib/utils";

// Use Holiday type from service
type Holiday = HolidayType;

export default function HolidayManagementPage() {
  const { toast } = useToast();
  const [holidays, setHolidays] = useState<Holiday[]>([]);
  const [loading, setLoading] = useState(true);
  const [selectedDates, setSelectedDates] = useState<Date[]>([]);
  const [showAddDialog, setShowAddDialog] = useState(false);
  const [editingHoliday, setEditingHoliday] = useState<Holiday | null>(null);
  const [saving, setSaving] = useState(false);
  const [selectedYear, setSelectedYear] = useState(new Date().getFullYear());
  const currentUser = authService.getCurrentUser();

  // Form state
  const [formData, setFormData] = useState({
    name: "",
    date: "",
    type: "Company" as Holiday['Type'],
    isRecurring: false,
    description: "",
    applicableOrgUIDs: [] as string[],
    applicableRegions: [] as string[],
    isActive: true
  });

  useEffect(() => {
    loadHolidays();
  }, [selectedYear]);

  const loadHolidays = async () => {
    setLoading(true);
    try {
      // Fetch real holidays from API
      const fetchedHolidays = await holidayService.getHolidaysByYear(
        selectedYear,
        currentUser?.orgUID
      );
      
      // If no holidays from year API, try org-specific holidays
      if (!fetchedHolidays || fetchedHolidays.length === 0) {
        const orgHolidays = await holidayService.getHolidayByOrgUID(
          currentUser?.orgUID || ''
        );
        
        // Filter for selected year
        const yearHolidays = orgHolidays.filter(h => {
          const holidayYear = new Date(h.Date || h.HolidayDate || '').getFullYear();
          return holidayYear === selectedYear;
        });
        
        setHolidays(yearHolidays);
        
        // Set selected dates for calendar
        const holidayDates = yearHolidays
          .filter(h => h.IsActive)
          .map(h => new Date(h.Date || h.HolidayDate || ''));
        setSelectedDates(holidayDates);
      } else {
        setHolidays(fetchedHolidays);
        
        // Set selected dates for calendar
        const holidayDates = fetchedHolidays
          .filter(h => h.IsActive)
          .map(h => new Date(h.Date || h.HolidayDate || ''));
        setSelectedDates(holidayDates);
      }
    } catch (error) {
      console.error("Error loading holidays:", error);
      toast({
        title: "Error",
        description: "Failed to load holidays. Please try again.",
        variant: "destructive"
      });
      setHolidays([]);
    } finally {
      setLoading(false);
    }
  };

  const handleAddHoliday = () => {
    setFormData({
      name: "",
      date: "",
      type: "Company",
      isRecurring: false,
      description: "",
      applicableOrgUIDs: [],
      applicableRegions: [],
      isActive: true
    });
    setEditingHoliday(null);
    setShowAddDialog(true);
  };

  const handleEditHoliday = (holiday: Holiday) => {
    setFormData({
      name: holiday.Name,
      date: holiday.Date,
      type: holiday.Type,
      isRecurring: holiday.IsRecurring,
      description: holiday.Description || "",
      applicableOrgUIDs: holiday.ApplicableOrgUIDs || [],
      applicableRegions: holiday.ApplicableRegions || [],
      isActive: holiday.IsActive
    });
    setEditingHoliday(holiday);
    setShowAddDialog(true);
  };

  const handleDeleteHoliday = async (holiday: Holiday) => {
    if (!confirm(`Are you sure you want to delete "${holiday.Name}"?`)) return;

    try {
      await holidayService.deleteHoliday(holiday.UID);
      toast({
        title: "Success",
        description: `Holiday "${holiday.Name}" deleted successfully`
      });
      loadHolidays();
    } catch (error) {
      console.error("Error deleting holiday:", error);
      toast({
        title: "Error",
        description: "Failed to delete holiday",
        variant: "destructive"
      });
    }
  };

  const handleSaveHoliday = async () => {
    if (!formData.name || !formData.date) {
      toast({
        title: "Validation Error",
        description: "Please provide holiday name and date",
        variant: "destructive"
      });
      return;
    }

    setSaving(true);
    try {
      const holidayData: Partial<Holiday> = {
        Name: formData.name,
        Date: formData.date,
        Type: formData.type,
        IsRecurring: formData.isRecurring,
        Description: formData.description,
        ApplicableOrgUIDs: formData.applicableOrgUIDs.length > 0 
          ? formData.applicableOrgUIDs 
          : [currentUser?.orgUID || ''],
        ApplicableRegions: formData.applicableRegions,
        IsActive: formData.isActive,
        OrgUID: currentUser?.orgUID,
        CompanyUID: currentUser?.companyUID,
        CreatedBy: currentUser?.uid || currentUser?.email || 'SYSTEM',
        ModifiedBy: currentUser?.uid || currentUser?.email || 'SYSTEM'
      };

      if (editingHoliday) {
        // Update holiday
        await holidayService.updateHoliday({
          ...editingHoliday,
          ...holidayData,
          UID: editingHoliday.UID
        } as Holiday);
        
        toast({
          title: "Success",
          description: "Holiday updated successfully"
        });
      } else {
        // Create new holiday
        await holidayService.createHoliday(holidayData);
        
        toast({
          title: "Success",
          description: "Holiday added successfully"
        });
      }
      
      setShowAddDialog(false);
      loadHolidays();
    } catch (error) {
      console.error("Error saving holiday:", error);
      toast({
        title: "Error",
        description: "Failed to save holiday. Please try again.",
        variant: "destructive"
      });
    } finally {
      setSaving(false);
    }
  };

  const getHolidayTypeColor = (type: Holiday['Type']) => {
    switch (type) {
      case 'National':
        return 'bg-red-100 text-red-800';
      case 'Regional':
        return 'bg-blue-100 text-blue-800';
      case 'Company':
        return 'bg-green-100 text-green-800';
      case 'Optional':
        return 'bg-yellow-100 text-yellow-800';
      default:
        return 'bg-gray-100 text-gray-800';
    }
  };

  const getHolidayIcon = (type: Holiday['Type']) => {
    switch (type) {
      case 'National':
        return <Globe className="h-4 w-4" />;
      case 'Regional':
        return <MapPin className="h-4 w-4" />;
      case 'Company':
        return <Users className="h-4 w-4" />;
      default:
        return <CalendarIcon className="h-4 w-4" />;
    }
  };

  const holidaysByMonth = holidays.reduce((acc, holiday) => {
    const month = moment(holiday.Date).format('MMMM');
    if (!acc[month]) acc[month] = [];
    acc[month].push(holiday);
    return acc;
  }, {} as Record<string, Holiday[]>);

  return (
    <div className="container mx-auto p-6">
      {/* Header */}
      <div className="mb-6 flex justify-between items-center">
        <div>
          <h1 className="text-2xl font-bold">Holiday Management</h1>
          <p className="text-gray-600 mt-1">
            Manage holidays that affect journey plan generation
          </p>
        </div>
        <Button onClick={handleAddHoliday}>
          <Plus className="h-4 w-4 mr-2" />
          Add Holiday
        </Button>
      </div>

      {/* Info Alert */}
      <Card className="mb-6 bg-blue-50 border-blue-200">
        <CardContent className="p-4">
          <div className="flex items-start gap-3">
            <Info className="h-5 w-5 text-blue-600 mt-0.5" />
            <div>
              <h4 className="font-semibold text-blue-900 mb-1">How Holidays Work</h4>
              <ul className="text-sm text-blue-800 space-y-1">
                <li>• Holidays prevent journey plan generation for those dates</li>
                <li>• National holidays apply to all organizations</li>
                <li>• Regional holidays apply to specific regions</li>
                <li>• Company holidays apply to specific organizations</li>
                <li>• Optional holidays can be chosen by employees</li>
                <li>• Recurring holidays repeat every year automatically</li>
              </ul>
            </div>
          </div>
        </CardContent>
      </Card>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Calendar View */}
        <Card className="lg:col-span-1">
          <CardHeader>
            <CardTitle className="text-lg flex items-center gap-2">
              <CalendarIcon className="h-5 w-5" />
              {selectedYear} Calendar
            </CardTitle>
            <div className="flex gap-2 mt-2">
              <Button
                variant="outline"
                size="sm"
                onClick={() => setSelectedYear(selectedYear - 1)}
              >
                Previous Year
              </Button>
              <Button
                variant="outline"
                size="sm"
                onClick={() => setSelectedYear(new Date().getFullYear())}
              >
                Current Year
              </Button>
              <Button
                variant="outline"
                size="sm"
                onClick={() => setSelectedYear(selectedYear + 1)}
              >
                Next Year
              </Button>
            </div>
          </CardHeader>
          <CardContent>
            <Calendar
              mode="multiple"
              selected={selectedDates}
              className="rounded-md"
              disabled={(date) => date.getFullYear() !== selectedYear}
            />
            
            {/* Legend */}
            <div className="mt-4 space-y-2">
              <p className="text-sm font-medium text-gray-600">Holiday Types:</p>
              <div className="grid grid-cols-2 gap-2">
                {(['National', 'Regional', 'Company', 'Optional'] as const).map(type => (
                  <div key={type} className="flex items-center gap-2">
                    <div className={cn("w-3 h-3 rounded", getHolidayTypeColor(type).split(' ')[0])} />
                    <span className="text-xs">{type}</span>
                  </div>
                ))}
              </div>
            </div>
          </CardContent>
        </Card>

        {/* Holiday List */}
        <Card className="lg:col-span-2">
          <CardHeader>
            <CardTitle>Holidays for {selectedYear}</CardTitle>
            <CardDescription>
              Total: {holidays.length} holidays ({holidays.filter(h => h.IsActive).length} active)
            </CardDescription>
          </CardHeader>
          <CardContent>
            <ScrollArea className="h-[600px]">
              {loading ? (
                <div className="space-y-4">
                  {[1, 2, 3].map(i => (
                    <Skeleton key={i} className="h-20" />
                  ))}
                </div>
              ) : Object.keys(holidaysByMonth).length > 0 ? (
                <div className="space-y-6">
                  {Object.entries(holidaysByMonth).map(([month, monthHolidays]) => (
                    <div key={month}>
                      <h3 className="font-semibold text-lg mb-3">{month}</h3>
                      <div className="space-y-2">
                        {monthHolidays.map(holiday => (
                          <div
                            key={holiday.UID}
                            className={cn(
                              "p-4 border rounded-lg",
                              !holiday.IsActive && "opacity-50"
                            )}
                          >
                            <div className="flex items-start justify-between">
                              <div className="flex-1">
                                <div className="flex items-center gap-2 mb-1">
                                  {getHolidayIcon(holiday.Type)}
                                  <h4 className="font-medium">{holiday.Name}</h4>
                                  <Badge className={getHolidayTypeColor(holiday.Type)}>
                                    {holiday.Type}
                                  </Badge>
                                  {holiday.IsRecurring && (
                                    <Badge variant="outline">
                                      Recurring
                                    </Badge>
                                  )}
                                  {!holiday.IsActive && (
                                    <Badge variant="secondary">
                                      Inactive
                                    </Badge>
                                  )}
                                </div>
                                <p className="text-sm text-gray-600">
                                  {moment(holiday.Date).format("dddd, MMMM D, YYYY")}
                                </p>
                                {holiday.Description && (
                                  <p className="text-sm text-gray-500 mt-1">
                                    {holiday.Description}
                                  </p>
                                )}
                              </div>
                              <div className="flex gap-2">
                                <Button
                                  variant="ghost"
                                  size="sm"
                                  onClick={() => handleEditHoliday(holiday)}
                                >
                                  <Edit className="h-4 w-4" />
                                </Button>
                                <Button
                                  variant="ghost"
                                  size="sm"
                                  onClick={() => handleDeleteHoliday(holiday)}
                                  className="hover:text-red-600"
                                >
                                  <Trash2 className="h-4 w-4" />
                                </Button>
                              </div>
                            </div>
                          </div>
                        ))}
                      </div>
                    </div>
                  ))}
                </div>
              ) : (
                <div className="text-center py-12">
                  <CalendarIcon className="h-12 w-12 mx-auto mb-3 text-gray-300" />
                  <p className="text-gray-500">No holidays configured for {selectedYear}</p>
                  <Button
                    variant="outline"
                    className="mt-4"
                    onClick={handleAddHoliday}
                  >
                    Add First Holiday
                  </Button>
                </div>
              )}
            </ScrollArea>
          </CardContent>
        </Card>
      </div>

      {/* Add/Edit Holiday Dialog */}
      <Dialog open={showAddDialog} onOpenChange={setShowAddDialog}>
        <DialogContent className="sm:max-w-[500px]">
          <DialogHeader>
            <DialogTitle>
              {editingHoliday ? 'Edit Holiday' : 'Add New Holiday'}
            </DialogTitle>
            <DialogDescription>
              Configure holiday details that will affect journey plan generation
            </DialogDescription>
          </DialogHeader>
          
          <div className="space-y-4 py-4">
            <div>
              <Label htmlFor="name">Holiday Name</Label>
              <Input
                id="name"
                value={formData.name}
                onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                placeholder="e.g., Diwali, Christmas"
              />
            </div>

            <div>
              <Label htmlFor="date">Date</Label>
              <Input
                id="date"
                type="date"
                value={formData.date}
                onChange={(e) => setFormData({ ...formData, date: e.target.value })}
              />
            </div>

            <div>
              <Label htmlFor="type">Holiday Type</Label>
              <Select
                value={formData.type}
                onValueChange={(value: Holiday['Type']) => 
                  setFormData({ ...formData, type: value })
                }
              >
                <SelectTrigger>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="National">National</SelectItem>
                  <SelectItem value="Regional">Regional</SelectItem>
                  <SelectItem value="Company">Company</SelectItem>
                  <SelectItem value="Optional">Optional</SelectItem>
                </SelectContent>
              </Select>
            </div>

            <div className="flex items-center space-x-2">
              <input
                type="checkbox"
                id="recurring"
                checked={formData.isRecurring}
                onChange={(e) => setFormData({ ...formData, isRecurring: e.target.checked })}
                className="h-4 w-4"
              />
              <Label htmlFor="recurring">
                Recurring (repeats every year)
              </Label>
            </div>

            <div>
              <Label htmlFor="description">Description (Optional)</Label>
              <Textarea
                id="description"
                value={formData.description}
                onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                placeholder="Additional details about this holiday"
                rows={3}
              />
            </div>

            <div className="flex items-center space-x-2">
              <input
                type="checkbox"
                id="active"
                checked={formData.isActive}
                onChange={(e) => setFormData({ ...formData, isActive: e.target.checked })}
                className="h-4 w-4"
              />
              <Label htmlFor="active">
                Active (affects journey plan generation)
              </Label>
            </div>
          </div>

          <DialogFooter>
            <Button variant="outline" onClick={() => setShowAddDialog(false)}>
              Cancel
            </Button>
            <Button onClick={handleSaveHoliday} disabled={saving}>
              {saving ? (
                <>Saving...</>
              ) : (
                <>
                  <Save className="h-4 w-4 mr-2" />
                  {editingHoliday ? 'Update' : 'Add'} Holiday
                </>
              )}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* Statistics */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4 mt-6">
        <Card>
          <CardContent className="p-4">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-gray-600">Total Holidays</p>
                <p className="text-2xl font-bold">{holidays.length}</p>
              </div>
              <CalendarIcon className="h-8 w-8 text-blue-500" />
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardContent className="p-4">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-gray-600">National</p>
                <p className="text-2xl font-bold">
                  {holidays.filter(h => h.Type === 'National').length}
                </p>
              </div>
              <Globe className="h-8 w-8 text-red-500" />
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardContent className="p-4">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-gray-600">Company</p>
                <p className="text-2xl font-bold">
                  {holidays.filter(h => h.Type === 'Company').length}
                </p>
              </div>
              <Users className="h-8 w-8 text-green-500" />
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardContent className="p-4">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-gray-600">Recurring</p>
                <p className="text-2xl font-bold">
                  {holidays.filter(h => h.IsRecurring).length}
                </p>
              </div>
              <CheckCircle className="h-8 w-8 text-purple-500" />
            </div>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}