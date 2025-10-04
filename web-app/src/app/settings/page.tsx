"use client";

import { useState, useEffect } from "react";
import { useAuth } from "@/providers/auth-provider";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Switch } from "@/components/ui/switch";
import { Separator } from "@/components/ui/separator";
import { Badge } from "@/components/ui/badge";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import {
  Settings as SettingsIcon,
  Bell,
  Shield,
  Globe,
  Monitor,
  Moon,
  Sun,
  Smartphone,
  Mail,
  MessageSquare,
  Lock,
  Users,
  Database,
  Activity,
  Save,
  RefreshCw,
} from "lucide-react";
import { authService } from "@/lib/auth-service";
import { SkeletonLoader } from "@/components/ui/loader";

interface SystemSettings {
  // Application Settings
  applicationName: string;
  applicationVersion: string;
  environment: string;

  // Security Settings
  sessionTimeout: number;
  passwordExpiry: number;
  maxLoginAttempts: number;
  requireMFA: boolean;
  allowRememberMe: boolean;

  // Notification Settings
  emailNotifications: boolean;
  smsNotifications: boolean;
  pushNotifications: boolean;
  maintenanceNotifications: boolean;

  // System Configuration
  defaultLanguage: string;
  defaultTheme: string;
  dateFormat: string;
  timeFormat: string;
  timezone: string;

  // Feature Flags
  auditTrailEnabled: boolean;
  sessionMonitoringEnabled: boolean;
  deviceTrackingEnabled: boolean;
  roleBasedMenus: boolean;
}

interface UserPreferences {
  theme: "light" | "dark" | "system";
  language: "en" | "ar" | "fr";
  notifications: {
    email: boolean;
    sms: boolean;
    push: boolean;
    security: boolean;
    system: boolean;
  };
  security: {
    mfaEnabled: boolean;
    trustedDevicesOnly: boolean;
    sessionTimeout: number;
    autoLogout: boolean;
  };
  display: {
    dateFormat: string;
    timeFormat: string;
    timezone: string;
    compactMode: boolean;
  };
}

export default function SettingsPage() {
  const { user, updateUser } = useAuth();
  const [systemSettings, setSystemSettings] = useState<SystemSettings | null>(
    null
  );
  const [userPreferences, setUserPreferences] =
    useState<UserPreferences | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [isSaving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [activeTab, setActiveTab] = useState("preferences");

  useEffect(() => {
    fetchSettings();
  }, []); // eslint-disable-line react-hooks/exhaustive-deps

  const fetchSettings = async () => {
    try {
      setIsLoading(true);
      setError(null);

      const token = authService.getToken();
      const currentUser = authService.getCurrentUser();
      if (!token) {
        setError("No authentication token found");
        return;
      }

      if (!currentUser?.uid) {
        setError("No user UID found");
        return;
      }

      // Use correct production endpoint: /api/Setting/SelectAllSettingDetails
      // This gets all settings as key-value pairs
      const API_BASE_URL =
        process.env.NEXT_PUBLIC_API_URL || "http://localhost:8000/api";
      const systemResponse = await fetch(
        `${API_BASE_URL}/Setting/SelectAllSettingDetails`,
        {
          method: "POST",
          headers: {
            Authorization: `Bearer ${token}`,
            "Content-Type": "application/json",
            Accept: "application/json",
          },
          body: JSON.stringify({
            PageNumber: 0,
            PageSize: 1000,
            SortCriterias: [],
            FilterCriterias: [],
            IsCountRequired: false,
          }),
        }
      );

      if (systemResponse.ok) {
        const systemData = await systemResponse.json();
        if (
          systemData.IsSuccess !== false &&
          (systemData.Data || systemData.data || systemData)
        ) {
          const settingsArray =
            systemData.Data || systemData.data || systemData;

          // Convert array of settings to key-value map
          const settingsMap: Record<string, string> = {};
          if (Array.isArray(settingsArray)) {
            settingsArray.forEach(
              (setting: { Name?: string; Value?: string }) => {
                if (setting.Name && setting.Value !== undefined) {
                  settingsMap[setting.Name] = setting.Value;
                }
              }
            );
          }

          // Map the backend setting data to our SystemSettings interface using actual backend values
          const mappedSystemSettings: SystemSettings = {
            applicationName:
              settingsMap["ApplicationName"] ||
              settingsMap["AppName"] ||
              "WINIT System",
            applicationVersion:
              settingsMap["ApplicationVersion"] ||
              settingsMap["Version"] ||
              "1.0.0",
            environment: settingsMap["Environment"] || "Production",
            sessionTimeout: Number(settingsMap["SessionTimeout"]) || 120,
            passwordExpiry: Number(settingsMap["PasswordExpiry"]) || 90,
            maxLoginAttempts: Number(settingsMap["MaxLoginAttempts"]) || 3,
            requireMFA:
              settingsMap["RequireMFA"] === "true" ||
              settingsMap["RequireMFA"] === "1",
            allowRememberMe:
              settingsMap["AllowRememberMe"] === "true" ||
              settingsMap["AllowRememberMe"] === "1",
            emailNotifications:
              settingsMap["EmailNotifications"] === "true" ||
              settingsMap["EmailNotifications"] === "1",
            smsNotifications:
              settingsMap["SmsNotifications"] === "true" ||
              settingsMap["SmsNotifications"] === "1",
            pushNotifications:
              settingsMap["PushNotifications"] === "true" ||
              settingsMap["PushNotifications"] === "1",
            maintenanceNotifications:
              settingsMap["MaintenanceNotifications"] === "true" ||
              settingsMap["MaintenanceNotifications"] === "1",
            defaultLanguage: settingsMap["DefaultLanguage"] || "en",
            defaultTheme: settingsMap["DefaultTheme"] || "system",
            dateFormat: settingsMap["DateFormat"] || "DD/MM/YYYY",
            timeFormat: settingsMap["TimeFormat"] || "24",
            timezone: settingsMap["Timezone"] || "UTC+3",
            auditTrailEnabled:
              settingsMap["AuditTrailEnabled"] === "true" ||
              settingsMap["AuditTrailEnabled"] === "1",
            sessionMonitoringEnabled:
              settingsMap["SessionMonitoringEnabled"] === "true" ||
              settingsMap["SessionMonitoringEnabled"] === "1",
            deviceTrackingEnabled:
              settingsMap["DeviceTrackingEnabled"] === "true" ||
              settingsMap["DeviceTrackingEnabled"] === "1",
            roleBasedMenus:
              settingsMap["RoleBasedMenus"] === "true" ||
              settingsMap["RoleBasedMenus"] === "1",
          };
          setSystemSettings(mappedSystemSettings);
        } else {
          setError("Invalid system settings data received from server");
        }
      } else {
        setError(
          `Failed to load system settings: ${systemResponse.status} ${systemResponse.statusText}`
        );
      }

      // For user preferences: Since there's no backend API, we need to store them locally
      // Load from localStorage or initialize from current user preferences
      const storedPreferences = localStorage.getItem(
        `user_preferences_${currentUser.uid}`
      );
      if (storedPreferences) {
        try {
          const preferences = JSON.parse(storedPreferences);
          setUserPreferences(preferences);
        } catch {
          // If parsing fails, initialize from current user data
          initializeUserPreferencesFromUser(currentUser);
        }
      } else {
        // Initialize from current user data
        initializeUserPreferencesFromUser(currentUser);
      }
    } catch (error) {
      console.error("Error fetching settings:", error);
      setError("Failed to load settings");
    } finally {
      setIsLoading(false);
    }
  };

  const initializeUserPreferencesFromUser = (currentUser: {
    preferences?: {
      theme?: string;
      language?: string;
      notifications?: {
        email?: boolean;
        sms?: boolean;
        push?: boolean;
      };
      security?: {
        mfaEnabled?: boolean;
        trustedDevicesOnly?: boolean;
        sessionTimeout?: number;
      };
    };
    uid: string;
  }) => {
    const preferences: UserPreferences = {
      theme:
        (currentUser.preferences?.theme as "light" | "dark" | "system") ||
        "system",
      language:
        (currentUser.preferences?.language as "en" | "ar" | "fr") || "en",
      notifications: {
        email: currentUser.preferences?.notifications?.email ?? true,
        sms: currentUser.preferences?.notifications?.sms ?? false,
        push: currentUser.preferences?.notifications?.push ?? true,
        security: true,
        system: true,
      },
      security: {
        mfaEnabled: currentUser.preferences?.security?.mfaEnabled ?? false,
        trustedDevicesOnly:
          currentUser.preferences?.security?.trustedDevicesOnly ?? false,
        sessionTimeout:
          currentUser.preferences?.security?.sessionTimeout || 120,
        autoLogout: true,
      },
      display: {
        dateFormat: "DD/MM/YYYY",
        timeFormat: "24",
        timezone: "UTC+3",
        compactMode: false,
      },
    };
    setUserPreferences(preferences);
  };

  const saveUserPreferences = async () => {
    if (!userPreferences) return;

    try {
      setSaving(true);
      const currentUser = authService.getCurrentUser();
      if (!currentUser) return;

      // Since there's no backend API for user preferences, store in localStorage
      localStorage.setItem(
        `user_preferences_${currentUser.uid}`,
        JSON.stringify(userPreferences)
      );

      // Update the auth context with new preferences
      if (user) {
        updateUser({
          ...user,
          preferences: {
            ...user.preferences,
            theme: userPreferences.theme,
            language: userPreferences.language,
            notifications: userPreferences.notifications,
            security: userPreferences.security,
          },
        });
      }

      setError(null);
    } catch (error) {
      console.error("Error saving preferences:", error);
      setError("Failed to save preferences");
    } finally {
      setSaving(false);
    }
  };

  if (isLoading) {
    return (
      <div className="p-8">
        <div className="max-w-4xl mx-auto space-y-8">
          <div className="space-y-2">
            <SkeletonLoader className="h-8 w-48 rounded" />
            <SkeletonLoader className="h-4 w-64 rounded" />
          </div>
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
            {Array.from({ length: 4 }).map((_, i) => (
              <div
                key={i}
                className="bg-white dark:bg-gray-950 rounded-lg border p-6 space-y-4"
              >
                <SkeletonLoader className="h-6 w-32 rounded" />
                <SkeletonLoader className="h-4 w-48 rounded" />
                <div className="space-y-3">
                  <SkeletonLoader className="h-10 w-full rounded" />
                  <SkeletonLoader className="h-10 w-full rounded" />
                </div>
              </div>
            ))}
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="p-8">
      <div className="max-w-6xl mx-auto space-y-6">
        {/* Header */}
        <div className="flex items-center justify-between">
          <div>
            <h1 className="text-3xl font-bold text-gray-900">Settings</h1>
            <p className="text-gray-600 mt-1">
              Manage your preferences and system configuration
            </p>
          </div>
          <Button onClick={fetchSettings} variant="outline">
            <RefreshCw className="mr-2 h-4 w-4" />
            Refresh
          </Button>
        </div>

        {error && (
          <Card className="border-red-200 bg-red-50">
            <CardContent className="pt-4">
              <p className="text-red-600 text-sm">{error}</p>
            </CardContent>
          </Card>
        )}

        <Tabs
          value={activeTab}
          onValueChange={setActiveTab}
          className="space-y-6"
        >
          <TabsList className="grid w-full grid-cols-4">
            <TabsTrigger value="preferences">User Preferences</TabsTrigger>
            <TabsTrigger value="notifications">Notifications</TabsTrigger>
            <TabsTrigger value="security">Security</TabsTrigger>
            <TabsTrigger value="system">System Info</TabsTrigger>
          </TabsList>

          <TabsContent value="preferences" className="space-y-6">
            {userPreferences && (
              <>
                {/* Appearance Settings */}
                <Card>
                  <CardHeader>
                    <CardTitle className="flex items-center gap-2">
                      <Monitor className="h-5 w-5" />
                      Appearance
                    </CardTitle>
                    <CardDescription>
                      Customize the look and feel of your interface
                    </CardDescription>
                  </CardHeader>
                  <CardContent className="space-y-6">
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                      <div className="space-y-2">
                        <Label htmlFor="theme">Theme</Label>
                        <Select
                          value={userPreferences.theme}
                          onValueChange={(value: "light" | "dark" | "system") =>
                            setUserPreferences({
                              ...userPreferences,
                              theme: value,
                            })
                          }
                        >
                          <SelectTrigger>
                            <SelectValue />
                          </SelectTrigger>
                          <SelectContent>
                            <SelectItem value="light">
                              <div className="flex items-center gap-2">
                                <Sun className="h-4 w-4" />
                                Light
                              </div>
                            </SelectItem>
                            <SelectItem value="dark">
                              <div className="flex items-center gap-2">
                                <Moon className="h-4 w-4" />
                                Dark
                              </div>
                            </SelectItem>
                            <SelectItem value="system">
                              <div className="flex items-center gap-2">
                                <Monitor className="h-4 w-4" />
                                System
                              </div>
                            </SelectItem>
                          </SelectContent>
                        </Select>
                      </div>

                      <div className="space-y-2">
                        <Label htmlFor="language">Language</Label>
                        <Select
                          value={userPreferences.language}
                          onValueChange={(value: "en" | "ar" | "fr") =>
                            setUserPreferences({
                              ...userPreferences,
                              language: value,
                            })
                          }
                        >
                          <SelectTrigger>
                            <SelectValue />
                          </SelectTrigger>
                          <SelectContent>
                            <SelectItem value="en">
                              <div className="flex items-center gap-2">
                                <Globe className="h-4 w-4" />
                                English
                              </div>
                            </SelectItem>
                            <SelectItem value="ar">Arabic</SelectItem>
                            <SelectItem value="fr">French</SelectItem>
                          </SelectContent>
                        </Select>
                      </div>
                    </div>

                    <div className="flex items-center justify-between">
                      <div className="space-y-1">
                        <Label>Compact Mode</Label>
                        <p className="text-sm text-gray-600">
                          Use a more compact interface layout
                        </p>
                      </div>
                      <Switch
                        checked={userPreferences.display.compactMode}
                        onCheckedChange={(checked) =>
                          setUserPreferences({
                            ...userPreferences,
                            display: {
                              ...userPreferences.display,
                              compactMode: checked,
                            },
                          })
                        }
                      />
                    </div>
                  </CardContent>
                </Card>

                {/* Display Settings */}
                <Card>
                  <CardHeader>
                    <CardTitle className="flex items-center gap-2">
                      <Activity className="h-5 w-5" />
                      Display Settings
                    </CardTitle>
                    <CardDescription>
                      Configure date, time, and regional formats
                    </CardDescription>
                  </CardHeader>
                  <CardContent className="space-y-4">
                    <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                      <div className="space-y-2">
                        <Label>Date Format</Label>
                        <Select
                          value={userPreferences.display.dateFormat}
                          onValueChange={(value) =>
                            setUserPreferences({
                              ...userPreferences,
                              display: {
                                ...userPreferences.display,
                                dateFormat: value,
                              },
                            })
                          }
                        >
                          <SelectTrigger>
                            <SelectValue />
                          </SelectTrigger>
                          <SelectContent>
                            <SelectItem value="DD/MM/YYYY">
                              DD/MM/YYYY
                            </SelectItem>
                            <SelectItem value="MM/DD/YYYY">
                              MM/DD/YYYY
                            </SelectItem>
                            <SelectItem value="YYYY-MM-DD">
                              YYYY-MM-DD
                            </SelectItem>
                          </SelectContent>
                        </Select>
                      </div>

                      <div className="space-y-2">
                        <Label>Time Format</Label>
                        <Select
                          value={userPreferences.display.timeFormat}
                          onValueChange={(value) =>
                            setUserPreferences({
                              ...userPreferences,
                              display: {
                                ...userPreferences.display,
                                timeFormat: value,
                              },
                            })
                          }
                        >
                          <SelectTrigger>
                            <SelectValue />
                          </SelectTrigger>
                          <SelectContent>
                            <SelectItem value="12">12-hour</SelectItem>
                            <SelectItem value="24">24-hour</SelectItem>
                          </SelectContent>
                        </Select>
                      </div>

                      <div className="space-y-2">
                        <Label>Timezone</Label>
                        <Select
                          value={userPreferences.display.timezone}
                          onValueChange={(value) =>
                            setUserPreferences({
                              ...userPreferences,
                              display: {
                                ...userPreferences.display,
                                timezone: value,
                              },
                            })
                          }
                        >
                          <SelectTrigger>
                            <SelectValue />
                          </SelectTrigger>
                          <SelectContent>
                            <SelectItem value="UTC">UTC</SelectItem>
                            <SelectItem value="UTC+3">UTC+3 (Gulf)</SelectItem>
                            <SelectItem value="UTC+2">UTC+2 (EET)</SelectItem>
                            <SelectItem value="UTC-5">UTC-5 (EST)</SelectItem>
                          </SelectContent>
                        </Select>
                      </div>
                    </div>
                  </CardContent>
                </Card>

                <div className="flex justify-end">
                  <Button onClick={saveUserPreferences} disabled={isSaving}>
                    {isSaving ? (
                      <>
                        <SkeletonLoader className="h-4 w-4 rounded mr-2" />
                        Saving...
                      </>
                    ) : (
                      <>
                        <Save className="mr-2 h-4 w-4" />
                        Save Preferences
                      </>
                    )}
                  </Button>
                </div>
              </>
            )}
          </TabsContent>

          <TabsContent value="notifications" className="space-y-6">
            {userPreferences && (
              <Card>
                <CardHeader>
                  <CardTitle className="flex items-center gap-2">
                    <Bell className="h-5 w-5" />
                    Notification Preferences
                  </CardTitle>
                  <CardDescription>
                    Choose how you want to receive notifications
                  </CardDescription>
                </CardHeader>
                <CardContent className="space-y-6">
                  <div className="space-y-4">
                    <div className="flex items-center justify-between">
                      <div className="flex items-center gap-3">
                        <Mail className="h-5 w-5 text-gray-500" />
                        <div>
                          <Label>Email Notifications</Label>
                          <p className="text-sm text-gray-600">
                            Receive notifications via email
                          </p>
                        </div>
                      </div>
                      <Switch
                        checked={userPreferences.notifications.email}
                        onCheckedChange={(checked) =>
                          setUserPreferences({
                            ...userPreferences,
                            notifications: {
                              ...userPreferences.notifications,
                              email: checked,
                            },
                          })
                        }
                      />
                    </div>

                    <div className="flex items-center justify-between">
                      <div className="flex items-center gap-3">
                        <MessageSquare className="h-5 w-5 text-gray-500" />
                        <div>
                          <Label>SMS Notifications</Label>
                          <p className="text-sm text-gray-600">
                            Receive notifications via SMS
                          </p>
                        </div>
                      </div>
                      <Switch
                        checked={userPreferences.notifications.sms}
                        onCheckedChange={(checked) =>
                          setUserPreferences({
                            ...userPreferences,
                            notifications: {
                              ...userPreferences.notifications,
                              sms: checked,
                            },
                          })
                        }
                      />
                    </div>

                    <div className="flex items-center justify-between">
                      <div className="flex items-center gap-3">
                        <Smartphone className="h-5 w-5 text-gray-500" />
                        <div>
                          <Label>Push Notifications</Label>
                          <p className="text-sm text-gray-600">
                            Receive push notifications in browser
                          </p>
                        </div>
                      </div>
                      <Switch
                        checked={userPreferences.notifications.push}
                        onCheckedChange={(checked) =>
                          setUserPreferences({
                            ...userPreferences,
                            notifications: {
                              ...userPreferences.notifications,
                              push: checked,
                            },
                          })
                        }
                      />
                    </div>

                    <Separator />

                    <div className="flex items-center justify-between">
                      <div className="flex items-center gap-3">
                        <Shield className="h-5 w-5 text-gray-500" />
                        <div>
                          <Label>Security Alerts</Label>
                          <p className="text-sm text-gray-600">
                            Important security-related notifications
                          </p>
                        </div>
                      </div>
                      <Switch
                        checked={userPreferences.notifications.security}
                        onCheckedChange={(checked) =>
                          setUserPreferences({
                            ...userPreferences,
                            notifications: {
                              ...userPreferences.notifications,
                              security: checked,
                            },
                          })
                        }
                      />
                    </div>

                    <div className="flex items-center justify-between">
                      <div className="flex items-center gap-3">
                        <SettingsIcon className="h-5 w-5 text-gray-500" />
                        <div>
                          <Label>System Updates</Label>
                          <p className="text-sm text-gray-600">
                            System maintenance and update notifications
                          </p>
                        </div>
                      </div>
                      <Switch
                        checked={userPreferences.notifications.system}
                        onCheckedChange={(checked) =>
                          setUserPreferences({
                            ...userPreferences,
                            notifications: {
                              ...userPreferences.notifications,
                              system: checked,
                            },
                          })
                        }
                      />
                    </div>
                  </div>

                  <div className="flex justify-end">
                    <Button onClick={saveUserPreferences} disabled={isSaving}>
                      <Save className="mr-2 h-4 w-4" />
                      {isSaving ? (
                        <>
                          <SkeletonLoader className="h-4 w-4 rounded mr-2" />
                          Saving...
                        </>
                      ) : (
                        "Save Notification Settings"
                      )}
                    </Button>
                  </div>
                </CardContent>
              </Card>
            )}
          </TabsContent>

          <TabsContent value="security" className="space-y-6">
            {userPreferences && (
              <Card>
                <CardHeader>
                  <CardTitle className="flex items-center gap-2">
                    <Lock className="h-5 w-5" />
                    Security Settings
                  </CardTitle>
                  <CardDescription>
                    Configure your security preferences and session settings
                  </CardDescription>
                </CardHeader>
                <CardContent className="space-y-6">
                  <div className="space-y-4">
                    <div className="flex items-center justify-between">
                      <div>
                        <Label>Multi-Factor Authentication</Label>
                        <p className="text-sm text-gray-600">
                          Add an extra layer of security to your account
                        </p>
                      </div>
                      <Switch
                        checked={userPreferences.security.mfaEnabled}
                        onCheckedChange={(checked) =>
                          setUserPreferences({
                            ...userPreferences,
                            security: {
                              ...userPreferences.security,
                              mfaEnabled: checked,
                            },
                          })
                        }
                      />
                    </div>

                    <div className="flex items-center justify-between">
                      <div>
                        <Label>Trusted Devices Only</Label>
                        <p className="text-sm text-gray-600">
                          Only allow login from trusted devices
                        </p>
                      </div>
                      <Switch
                        checked={userPreferences.security.trustedDevicesOnly}
                        onCheckedChange={(checked) =>
                          setUserPreferences({
                            ...userPreferences,
                            security: {
                              ...userPreferences.security,
                              trustedDevicesOnly: checked,
                            },
                          })
                        }
                      />
                    </div>

                    <div className="flex items-center justify-between">
                      <div>
                        <Label>Auto Logout</Label>
                        <p className="text-sm text-gray-600">
                          Automatically log out when inactive
                        </p>
                      </div>
                      <Switch
                        checked={userPreferences.security.autoLogout}
                        onCheckedChange={(checked) =>
                          setUserPreferences({
                            ...userPreferences,
                            security: {
                              ...userPreferences.security,
                              autoLogout: checked,
                            },
                          })
                        }
                      />
                    </div>

                    <div className="space-y-2">
                      <Label htmlFor="sessionTimeout">
                        Session Timeout (minutes)
                      </Label>
                      <Input
                        id="sessionTimeout"
                        type="number"
                        min="5"
                        max="480"
                        value={userPreferences.security.sessionTimeout}
                        onChange={(e) =>
                          setUserPreferences({
                            ...userPreferences,
                            security: {
                              ...userPreferences.security,
                              sessionTimeout: parseInt(e.target.value) || 120,
                            },
                          })
                        }
                        className="w-32"
                      />
                      <p className="text-sm text-gray-600">
                        How long before you&apos;re automatically logged out due
                        to inactivity
                      </p>
                    </div>
                  </div>

                  <div className="flex justify-end">
                    <Button onClick={saveUserPreferences} disabled={isSaving}>
                      <Save className="mr-2 h-4 w-4" />
                      {isSaving ? (
                        <>
                          <SkeletonLoader className="h-4 w-4 rounded mr-2" />
                          Saving...
                        </>
                      ) : (
                        "Save Security Settings"
                      )}
                    </Button>
                  </div>
                </CardContent>
              </Card>
            )}
          </TabsContent>

          <TabsContent value="system" className="space-y-6">
            {systemSettings && (
              <>
                <Card>
                  <CardHeader>
                    <CardTitle className="flex items-center gap-2">
                      <Database className="h-5 w-5" />
                      System Information
                    </CardTitle>
                    <CardDescription>
                      View system configuration and status (Read-only)
                    </CardDescription>
                  </CardHeader>
                  <CardContent>
                    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                      <div className="space-y-2">
                        <Label>Application Name</Label>
                        <p className="text-sm font-medium">
                          {systemSettings.applicationName}
                        </p>
                      </div>

                      <div className="space-y-2">
                        <Label>Version</Label>
                        <Badge variant="outline">
                          {systemSettings.applicationVersion}
                        </Badge>
                      </div>

                      <div className="space-y-2">
                        <Label>Environment</Label>
                        <Badge
                          variant={
                            systemSettings.environment === "Production"
                              ? "default"
                              : "secondary"
                          }
                        >
                          {systemSettings.environment}
                        </Badge>
                      </div>

                      <div className="space-y-2">
                        <Label>Session Timeout</Label>
                        <p className="text-sm">
                          {systemSettings.sessionTimeout} minutes
                        </p>
                      </div>

                      <div className="space-y-2">
                        <Label>Max Login Attempts</Label>
                        <p className="text-sm">
                          {systemSettings.maxLoginAttempts}
                        </p>
                      </div>

                      <div className="space-y-2">
                        <Label>Password Expiry</Label>
                        <p className="text-sm">
                          {systemSettings.passwordExpiry} days
                        </p>
                      </div>
                    </div>
                  </CardContent>
                </Card>

                <Card>
                  <CardHeader>
                    <CardTitle className="flex items-center gap-2">
                      <Users className="h-5 w-5" />
                      Feature Configuration
                    </CardTitle>
                  </CardHeader>
                  <CardContent>
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                      <div className="space-y-4">
                        <div className="flex items-center justify-between">
                          <Label>Audit Trail</Label>
                          <Badge
                            variant={
                              systemSettings.auditTrailEnabled
                                ? "default"
                                : "secondary"
                            }
                          >
                            {systemSettings.auditTrailEnabled
                              ? "Enabled"
                              : "Disabled"}
                          </Badge>
                        </div>

                        <div className="flex items-center justify-between">
                          <Label>Session Monitoring</Label>
                          <Badge
                            variant={
                              systemSettings.sessionMonitoringEnabled
                                ? "default"
                                : "secondary"
                            }
                          >
                            {systemSettings.sessionMonitoringEnabled
                              ? "Enabled"
                              : "Disabled"}
                          </Badge>
                        </div>

                        <div className="flex items-center justify-between">
                          <Label>Device Tracking</Label>
                          <Badge
                            variant={
                              systemSettings.deviceTrackingEnabled
                                ? "default"
                                : "secondary"
                            }
                          >
                            {systemSettings.deviceTrackingEnabled
                              ? "Enabled"
                              : "Disabled"}
                          </Badge>
                        </div>
                      </div>

                      <div className="space-y-4">
                        <div className="flex items-center justify-between">
                          <Label>Multi-Factor Authentication</Label>
                          <Badge
                            variant={
                              systemSettings.requireMFA
                                ? "default"
                                : "secondary"
                            }
                          >
                            {systemSettings.requireMFA
                              ? "Required"
                              : "Optional"}
                          </Badge>
                        </div>

                        <div className="flex items-center justify-between">
                          <Label>Role-Based Menus</Label>
                          <Badge
                            variant={
                              systemSettings.roleBasedMenus
                                ? "default"
                                : "secondary"
                            }
                          >
                            {systemSettings.roleBasedMenus
                              ? "Enabled"
                              : "Disabled"}
                          </Badge>
                        </div>

                        <div className="flex items-center justify-between">
                          <Label>Remember Me</Label>
                          <Badge
                            variant={
                              systemSettings.allowRememberMe
                                ? "default"
                                : "secondary"
                            }
                          >
                            {systemSettings.allowRememberMe
                              ? "Allowed"
                              : "Disabled"}
                          </Badge>
                        </div>
                      </div>
                    </div>
                  </CardContent>
                </Card>
              </>
            )}
          </TabsContent>
        </Tabs>
      </div>
    </div>
  );
}
