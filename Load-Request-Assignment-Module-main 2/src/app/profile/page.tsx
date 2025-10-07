"use client";

import { useState, useEffect } from "react";
import { useAuth } from "@/providers/auth-provider";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle
} from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Badge } from "@/components/ui/badge";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import {
  User,
  Mail,
  Phone,
  Building,
  Shield,
  Calendar,
  MapPin,
  Settings,
  Save
} from "lucide-react";
import { authService } from "@/lib/auth-service";
import { SkeletonLoader } from "@/components/ui/loader";

interface UserProfile {
  id: string;
  uid: string;
  loginId: string;
  name: string;
  email: string;
  mobile: string;
  status: string;
  designation?: string;
  department?: string;
  joiningDate?: string;
  reportingTo?: string;
  location?: string;
  lastLoginTime: Date;
  roles: Array<{
    id: string;
    uid: string;
    roleNameEn: string;
    code: string;
    isAdmin: boolean;
    isPrincipalRole: boolean;
    isWebUser: boolean;
    isAppUser: boolean;
  }>;
  currentOrganization?: {
    uid: string;
    code: string;
    name: string;
    type: string;
    isActive: boolean;
  };
}

// Helper function to encrypt password with challenge code using SHA256
const encryptPasswordWithChallenge = async (
  password: string,
  challengeCode: string
): Promise<string> => {
  const passwordWithChallenge = password + challengeCode;
  const encoder = new TextEncoder();
  const data = encoder.encode(passwordWithChallenge);
  const hashBuffer = await crypto.subtle.digest("SHA-256", data);
  const hashArray = Array.from(new Uint8Array(hashBuffer));
  const hashBase64 = btoa(String.fromCharCode.apply(null, hashArray));
  return hashBase64;
};

export default function ProfilePage() {
  const { user, updateUser } = useAuth();
  const [profileData, setProfileData] = useState<UserProfile | null>(null);
  const [isEditing, setIsEditing] = useState(false);
  const [isSaving, setSaving] = useState(false);
  const [showPasswordChange, setShowPasswordChange] = useState(false);
  const [passwords, setPasswords] = useState({
    current: "",
    new: "",
    confirm: ""
  });
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    fetchUserProfile();
  }, []);

  const fetchUserProfile = async () => {
    try {
      setIsLoading(true);
      setError(null);

      const token = authService.getToken();
      if (!token) {
        setError("No authentication token found");
        return;
      }

      // Get current user from auth service to retrieve UID
      const currentUser = authService.getCurrentUser();
      if (!currentUser?.uid) {
        setError("No user UID found");
        return;
      }

      // Use correct production endpoint: /api/Emp/GetEmpByUID
      const API_BASE_URL =
        process.env.NEXT_PUBLIC_API_URL || "http://localhost:8000/api";
      const response = await fetch(
        `${API_BASE_URL}/Emp/GetEmpByUID?UID=${encodeURIComponent(
          currentUser.uid
        )}`,
        {
          method: "GET",
          headers: {
            Authorization: `Bearer ${token}`,
            Accept: "application/json"
          }
        }
      );

      if (response.ok) {
        const data = await response.json();

        if (data.IsSuccess !== false && (data.Data || data.data || data)) {
          const rawProfile = data.Data || data.data || data;

          // Map the backend employee data to our UserProfile interface
          const profile: UserProfile = {
            id:
              rawProfile.Id ||
              rawProfile.id ||
              rawProfile.UID ||
              rawProfile.uid ||
              "",
            uid:
              rawProfile.UID ||
              rawProfile.uid ||
              rawProfile.Code ||
              rawProfile.LoginId ||
              "",
            loginId:
              rawProfile.LoginId || rawProfile.loginId || rawProfile.Code || "",
            name:
              rawProfile.Name ||
              rawProfile.name ||
              rawProfile.FullName ||
              rawProfile.FirstName ||
              "Unknown User",
            email:
              rawProfile.Email ||
              rawProfile.email ||
              rawProfile.EmailAddress ||
              "",
            mobile:
              rawProfile.Mobile ||
              rawProfile.mobile ||
              rawProfile.PhoneNumber ||
              rawProfile.Phone ||
              "",
            status:
              rawProfile.Status || rawProfile.status || rawProfile.IsActive
                ? "Active"
                : "Inactive",
            designation:
              rawProfile.Designation ||
              rawProfile.designation ||
              rawProfile.JobTitle ||
              "",
            department: rawProfile.Department || rawProfile.department || "",
            joiningDate:
              rawProfile.JoiningDate ||
              rawProfile.joiningDate ||
              rawProfile.DateJoined ||
              "",
            reportingTo:
              rawProfile.ReportingTo ||
              rawProfile.reportingTo ||
              rawProfile.Manager ||
              "",
            location:
              rawProfile.Location ||
              rawProfile.location ||
              rawProfile.Branch ||
              "",
            lastLoginTime: new Date(),
            roles: [], // We'll get this from current user context
            currentOrganization: undefined // We'll get this from current user context
          };

          // Add roles and organization from current user if available
          const currentUser = authService.getCurrentUser();
          if (currentUser) {
            profile.roles = currentUser.roles || [];
            profile.currentOrganization = currentUser.currentOrganization;
          }

          setProfileData(profile);
        } else {
          setError("Invalid profile data received from server");
        }
      } else {
        setError(
          `Failed to load profile: ${response.status} ${response.statusText}`
        );
      }
    } catch (error) {
      console.error("Error fetching profile:", error);
      setError("Failed to load profile data. Please check your connection.");
    } finally {
      setIsLoading(false);
    }
  };

  const handleSaveProfile = async () => {
    if (!profileData) return;

    try {
      setSaving(true);
      const token = authService.getToken();
      if (!token) return;

      // Use correct production endpoint: /api/Emp/UpdateEmp
      const API_BASE_URL =
        process.env.NEXT_PUBLIC_API_URL || "http://localhost:8000/api";
      const response = await fetch(`${API_BASE_URL}/Emp/UpdateEmp`, {
        method: "PUT",
        headers: {
          Authorization: `Bearer ${token}`,
          "Content-Type": "application/json",
          Accept: "application/json"
        },
        body: JSON.stringify({
          UID: profileData.uid,
          Name: profileData.name,
          Email: profileData.email,
          Mobile: profileData.mobile
        })
      });

      if (response.ok) {
        setIsEditing(false);
        // Update the auth context with new data
        if (user) {
          updateUser({
            ...user,
            name: profileData.name,
            email: profileData.email,
            mobile: profileData.mobile
          });
        }
      } else {
        setError("Failed to update profile");
      }
    } catch (error) {
      console.error("Error updating profile:", error);
      setError("Failed to update profile");
    } finally {
      setSaving(false);
    }
  };

  const handlePasswordChange = async () => {
    if (passwords.new !== passwords.confirm) {
      setError("New passwords do not match");
      return;
    }

    try {
      setSaving(true);
      const token = authService.getToken();
      const currentUser = authService.getCurrentUser();
      if (!token || !currentUser) return;

      // Generate challenge code in the required format (yyyyMMddHHmmss)
      const challengeCode = new Date()
        .toISOString()
        .slice(0, 19)
        .replace(/[-T:]/g, "")
        .slice(0, 14);

      // Encrypt old password with challenge code using SHA256 (same as login process)
      const hashedOldPassword = await encryptPasswordWithChallenge(
        passwords.current,
        challengeCode
      );

      // Use correct production endpoint: /api/Auth/UpdateExistingPasswordWithNewPassword
      const API_BASE_URL =
        process.env.NEXT_PUBLIC_API_URL || "http://localhost:8000/api";
      const response = await fetch(
        `${API_BASE_URL}/Auth/UpdateExistingPasswordWithNewPassword`,
        {
          method: "POST",
          headers: {
            Authorization: `Bearer ${token}`,
            "Content-Type": "application/json",
            Accept: "application/json"
          },
          body: JSON.stringify({
            UserId: currentUser.loginId,
            EmpUID: currentUser.uid,
            OldPassword: hashedOldPassword,
            NewPassword: passwords.new,
            ChallengeCode: challengeCode
          })
        }
      );

      if (response.ok) {
        setShowPasswordChange(false);
        setPasswords({ current: "", new: "", confirm: "" });
        setError(null);
      } else {
        const errorData = await response.json();
        setError(errorData.message || "Failed to change password");
      }
    } catch (error) {
      console.error("Error changing password:", error);
      setError("Failed to change password");
    } finally {
      setSaving(false);
    }
  };

  if (isLoading) {
    return (
      <div className="p-8">
        <div className="max-w-4xl mx-auto space-y-8">
          <div className="bg-white dark:bg-gray-950 rounded-lg border p-6 space-y-6">
            <div className="flex items-center gap-6">
              <SkeletonLoader className="h-20 w-20 rounded-full" />
              <div className="space-y-2">
                <SkeletonLoader className="h-6 w-48 rounded" />
                <SkeletonLoader className="h-4 w-32 rounded" />
                <SkeletonLoader className="h-4 w-40 rounded" />
              </div>
            </div>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
              {Array.from({ length: 6 }).map((_, i) => (
                <div key={i} className="space-y-2">
                  <SkeletonLoader className="h-4 w-24 rounded" />
                  <SkeletonLoader className="h-10 w-full rounded" />
                </div>
              ))}
            </div>
          </div>
        </div>
      </div>
    );
  }

  if (!profileData) {
    return (
      <div className="p-8">
        <div className="max-w-4xl mx-auto">
          <Card>
            <CardHeader>
              <CardTitle className="text-red-600">Error</CardTitle>
            </CardHeader>
            <CardContent>
              <p>{error || "Failed to load profile data"}</p>
              <Button onClick={fetchUserProfile} className="mt-4">
                Retry
              </Button>
            </CardContent>
          </Card>
        </div>
      </div>
    );
  }

  return (
    <div className="p-8">
      <div className="max-w-4xl mx-auto space-y-6">
        {/* Header */}
        <div className="flex items-center justify-between">
          <div>
            <h1 className="text-3xl font-bold text-gray-900">User Profile</h1>
            <p className="text-gray-600 mt-1">
              Manage your account information and preferences
            </p>
          </div>
          <div className="flex gap-2">
            {!isEditing ? (
              <Button onClick={() => setIsEditing(true)}>
                <Settings className="mr-2 h-4 w-4" />
                Edit Profile
              </Button>
            ) : (
              <>
                <Button variant="outline" onClick={() => setIsEditing(false)}>
                  Cancel
                </Button>
                <Button onClick={handleSaveProfile} disabled={isSaving}>
                  {isSaving ? (
                    <>
                      <SkeletonLoader className="h-4 w-4 rounded mr-2" />
                      Saving...
                    </>
                  ) : (
                    <>
                      <Save className="mr-2 h-4 w-4" />
                      Save Changes
                    </>
                  )}
                </Button>
              </>
            )}
          </div>
        </div>

        {error && (
          <Card className="border-red-200 bg-red-50">
            <CardContent className="pt-4">
              <p className="text-red-600 text-sm">{error}</p>
            </CardContent>
          </Card>
        )}

        <Tabs defaultValue="personal" className="space-y-6">
          <TabsList className="grid w-full grid-cols-4">
            <TabsTrigger value="personal">Personal Info</TabsTrigger>
            <TabsTrigger value="role">Role & Permissions</TabsTrigger>
            <TabsTrigger value="organization">Organization</TabsTrigger>
            <TabsTrigger value="security">Security</TabsTrigger>
          </TabsList>

          <TabsContent value="personal" className="space-y-6">
            {/* Profile Header */}
            <Card>
              <CardContent className="pt-6">
                <div className="flex items-center gap-6">
                  <Avatar className="h-20 w-20">
                    <AvatarFallback className="text-2xl">
                      {profileData.name
                        ? profileData.name
                            .split(" ")
                            .map((n) => n[0])
                            .join("")
                            .toUpperCase()
                        : "U"}
                    </AvatarFallback>
                  </Avatar>
                  <div className="flex-1">
                    <h2 className="text-2xl font-bold">
                      {profileData.name || "Unknown User"}
                    </h2>
                    <p className="text-gray-600">
                      {profileData.loginId || "No Login ID"}
                    </p>
                    <div className="flex items-center gap-2 mt-2">
                      <Badge
                        variant={
                          profileData.status === "Active"
                            ? "default"
                            : "secondary"
                        }
                      >
                        {profileData.status}
                      </Badge>
                      {profileData.designation && (
                        <Badge variant="outline">
                          {profileData.designation}
                        </Badge>
                      )}
                    </div>
                  </div>
                </div>
              </CardContent>
            </Card>

            {/* Personal Information */}
            <Card>
              <CardHeader>
                <CardTitle>Personal Information</CardTitle>
                <CardDescription>
                  Your personal details and contact information
                </CardDescription>
              </CardHeader>
              <CardContent className="space-y-4">
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <div className="space-y-2">
                    <Label htmlFor="name">Full Name</Label>
                    <div className="flex items-center gap-2">
                      <User className="h-4 w-4 text-gray-500" />
                      {isEditing ? (
                        <Input
                          id="name"
                          value={profileData.name || ""}
                          onChange={(e) =>
                            setProfileData({
                              ...profileData,
                              name: e.target.value
                            })
                          }
                        />
                      ) : (
                        <span className="text-sm">{profileData.name}</span>
                      )}
                    </div>
                  </div>

                  <div className="space-y-2">
                    <Label htmlFor="loginId">Login ID</Label>
                    <div className="flex items-center gap-2">
                      <Shield className="h-4 w-4 text-gray-500" />
                      <span className="text-sm text-gray-600">
                        {profileData.loginId}
                      </span>
                    </div>
                  </div>

                  <div className="space-y-2">
                    <Label htmlFor="email">Email Address</Label>
                    <div className="flex items-center gap-2">
                      <Mail className="h-4 w-4 text-gray-500" />
                      {isEditing ? (
                        <Input
                          id="email"
                          type="email"
                          value={profileData.email || ""}
                          onChange={(e) =>
                            setProfileData({
                              ...profileData,
                              email: e.target.value
                            })
                          }
                        />
                      ) : (
                        <span className="text-sm">
                          {profileData.email || "Not provided"}
                        </span>
                      )}
                    </div>
                  </div>

                  <div className="space-y-2">
                    <Label htmlFor="mobile">Mobile Number</Label>
                    <div className="flex items-center gap-2">
                      <Phone className="h-4 w-4 text-gray-500" />
                      {isEditing ? (
                        <Input
                          id="mobile"
                          value={profileData.mobile || ""}
                          onChange={(e) =>
                            setProfileData({
                              ...profileData,
                              mobile: e.target.value
                            })
                          }
                        />
                      ) : (
                        <span className="text-sm">
                          {profileData.mobile || "Not provided"}
                        </span>
                      )}
                    </div>
                  </div>

                  {profileData.department && (
                    <div className="space-y-2">
                      <Label>Department</Label>
                      <div className="flex items-center gap-2">
                        <Building className="h-4 w-4 text-gray-500" />
                        <span className="text-sm">
                          {profileData.department}
                        </span>
                      </div>
                    </div>
                  )}

                  {profileData.location && (
                    <div className="space-y-2">
                      <Label>Location</Label>
                      <div className="flex items-center gap-2">
                        <MapPin className="h-4 w-4 text-gray-500" />
                        <span className="text-sm">{profileData.location}</span>
                      </div>
                    </div>
                  )}
                </div>

                {profileData.joiningDate && (
                  <div className="space-y-2">
                    <Label>Joining Date</Label>
                    <div className="flex items-center gap-2">
                      <Calendar className="h-4 w-4 text-gray-500" />
                      <span className="text-sm">
                        {new Date(profileData.joiningDate).toLocaleDateString()}
                      </span>
                    </div>
                  </div>
                )}
              </CardContent>
            </Card>
          </TabsContent>

          <TabsContent value="role" className="space-y-6">
            <Card>
              <CardHeader>
                <CardTitle>Roles & Permissions</CardTitle>
                <CardDescription>
                  Your assigned roles and their associated permissions
                </CardDescription>
              </CardHeader>
              <CardContent>
                <div className="space-y-4">
                  {profileData.roles.map((role) => (
                    <div key={role.id} className="border rounded-lg p-4">
                      <div className="flex items-center justify-between mb-3">
                        <h4 className="font-semibold text-lg">
                          {role.roleNameEn}
                        </h4>
                        <div className="flex gap-2">
                          {role.isAdmin && (
                            <Badge className="bg-red-100 text-red-800">
                              Admin
                            </Badge>
                          )}
                          {role.isPrincipalRole && (
                            <Badge className="bg-blue-100 text-blue-800">
                              Principal
                            </Badge>
                          )}
                        </div>
                      </div>

                      <div className="grid grid-cols-2 md:grid-cols-4 gap-4 text-sm">
                        <div>
                          <span className="font-medium">Code:</span>
                          <p className="text-gray-600">{role.code}</p>
                        </div>
                        <div>
                          <span className="font-medium">UID:</span>
                          <p className="text-gray-600">{role.uid}</p>
                        </div>
                        <div>
                          <span className="font-medium">Web Access:</span>
                          <p className="text-gray-600">
                            {role.isWebUser ? "Yes" : "No"}
                          </p>
                        </div>
                        <div>
                          <span className="font-medium">Mobile Access:</span>
                          <p className="text-gray-600">
                            {role.isAppUser ? "Yes" : "No"}
                          </p>
                        </div>
                      </div>
                    </div>
                  ))}
                </div>
              </CardContent>
            </Card>
          </TabsContent>

          <TabsContent value="organization" className="space-y-6">
            {profileData.currentOrganization && (
              <Card>
                <CardHeader>
                  <CardTitle>Organization Details</CardTitle>
                  <CardDescription>
                    Your current organization and hierarchy
                  </CardDescription>
                </CardHeader>
                <CardContent>
                  <div className="space-y-4">
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                      <div className="space-y-2">
                        <Label>Organization Name</Label>
                        <div className="flex items-center gap-2">
                          <Building className="h-4 w-4 text-gray-500" />
                          <span className="text-sm font-medium">
                            {profileData.currentOrganization.name}
                          </span>
                        </div>
                      </div>

                      <div className="space-y-2">
                        <Label>Organization Code</Label>
                        <span className="text-sm text-gray-600">
                          {profileData.currentOrganization.code}
                        </span>
                      </div>

                      <div className="space-y-2">
                        <Label>Organization Type</Label>
                        <Badge variant="outline">
                          {profileData.currentOrganization.type}
                        </Badge>
                      </div>

                      <div className="space-y-2">
                        <Label>Status</Label>
                        <Badge
                          variant={
                            profileData.currentOrganization.isActive
                              ? "default"
                              : "secondary"
                          }
                        >
                          {profileData.currentOrganization.isActive
                            ? "Active"
                            : "Inactive"}
                        </Badge>
                      </div>
                    </div>

                    {profileData.reportingTo && (
                      <div className="space-y-2">
                        <Label>Reporting To</Label>
                        <div className="flex items-center gap-2">
                          <User className="h-4 w-4 text-gray-500" />
                          <span className="text-sm">
                            {profileData.reportingTo}
                          </span>
                        </div>
                      </div>
                    )}
                  </div>
                </CardContent>
              </Card>
            )}
          </TabsContent>

          <TabsContent value="security" className="space-y-6">
            <Card>
              <CardHeader>
                <CardTitle>Security Settings</CardTitle>
                <CardDescription>
                  Manage your account security and authentication settings
                </CardDescription>
              </CardHeader>
              <CardContent className="space-y-6">
                <div className="space-y-4">
                  <div className="flex items-center justify-between p-4 border rounded-lg">
                    <div>
                      <h4 className="font-medium">Password</h4>
                      <p className="text-sm text-gray-600">
                        Change your account password
                      </p>
                    </div>
                    <Button
                      variant="outline"
                      onClick={() => setShowPasswordChange(!showPasswordChange)}
                    >
                      {showPasswordChange ? "Cancel" : "Change Password"}
                    </Button>
                  </div>

                  {showPasswordChange && (
                    <Card className="border-blue-200 bg-blue-50">
                      <CardContent className="pt-4 space-y-4">
                        <div className="space-y-2">
                          <Label htmlFor="currentPassword">
                            Current Password
                          </Label>
                          <div className="relative">
                            <Input
                              id="currentPassword"
                              type="password"
                              value={passwords.current}
                              onChange={(e) =>
                                setPasswords({
                                  ...passwords,
                                  current: e.target.value
                                })
                              }
                            />
                          </div>
                        </div>

                        <div className="space-y-2">
                          <Label htmlFor="newPassword">New Password</Label>
                          <Input
                            id="newPassword"
                            type="password"
                            value={passwords.new}
                            onChange={(e) =>
                              setPasswords({
                                ...passwords,
                                new: e.target.value
                              })
                            }
                          />
                        </div>

                        <div className="space-y-2">
                          <Label htmlFor="confirmPassword">
                            Confirm New Password
                          </Label>
                          <Input
                            id="confirmPassword"
                            type="password"
                            value={passwords.confirm}
                            onChange={(e) =>
                              setPasswords({
                                ...passwords,
                                confirm: e.target.value
                              })
                            }
                          />
                        </div>

                        <Button
                          onClick={handlePasswordChange}
                          disabled={
                            isSaving ||
                            !passwords.current ||
                            !passwords.new ||
                            !passwords.confirm
                          }
                          className="w-full"
                        >
                          {isSaving ? (
                            <>
                              <SkeletonLoader className="h-4 w-4 rounded mr-2" />
                              Changing...
                            </>
                          ) : (
                            "Update Password"
                          )}
                        </Button>
                      </CardContent>
                    </Card>
                  )}

                  <div className="space-y-4">
                    <div className="flex items-center justify-between p-4 border rounded-lg">
                      <div>
                        <h4 className="font-medium">Last Login</h4>
                        <p className="text-sm text-gray-600">
                          {new Date(profileData.lastLoginTime).toLocaleString()}
                        </p>
                      </div>
                      <Calendar className="h-5 w-5 text-gray-400" />
                    </div>

                    <div className="flex items-center justify-between p-4 border rounded-lg">
                      <div>
                        <h4 className="font-medium">Account UID</h4>
                        <p className="text-sm text-gray-600 font-mono">
                          {profileData.uid}
                        </p>
                      </div>
                      <Shield className="h-5 w-5 text-gray-400" />
                    </div>
                  </div>
                </div>
              </CardContent>
            </Card>
          </TabsContent>
        </Tabs>
      </div>
    </div>
  );
}
