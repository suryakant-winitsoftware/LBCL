export interface User {
  id: string;
  uid: string;
  loginId: string;
  name: string;
  email: string;
  mobile: string;
  status: 'Active' | 'Inactive' | 'Suspended';
  lastLoginTime: Date;
  profilePicture?: string;
  preferences: UserPreferences;
  roles: UserRole[];
  currentOrganization?: Organization;
  availableOrganizations: Organization[];
}

export interface UserPreferences {
  theme: 'light' | 'dark' | 'system';
  language: string;
  notifications: {
    email: boolean;
    sms: boolean;
    push: boolean;
  };
  security: {
    mfaEnabled: boolean;
    trustedDevicesOnly: boolean;
    sessionTimeout: number; // in minutes
  };
}

export interface UserRole {
  id: string;
  uid: string;
  roleNameEn: string;
  code: string;
  isAdmin: boolean;
  isPrincipalRole: boolean;
  isDistributorRole: boolean;
  isWebUser: boolean;
  isAppUser: boolean;
  organizationUID?: string;
  isDefault: boolean;
}

export interface Organization {
  uid: string;
  code: string;
  name: string;
  type: string;
  parentUID?: string;
  isActive: boolean;
}

export interface LoginCredentials {
  loginId: string;
  password: string;
  rememberMe: boolean;
  deviceFingerprint?: string;
}

export interface LoginResponse {
  success: boolean;
  token: string;
  refreshToken: string;
  user: User;
  requiresMFA: boolean;
  mfaToken?: string;
  expiresIn: number;
  error?: string;
}

export interface SessionInfo {
  sessionId: string;
  userId: string;
  deviceType: 'Web' | 'Mobile' | 'Desktop';
  deviceName: string;
  ipAddress: string;
  location: string;
  userAgent: string;
  loginTime: Date;
  lastActivity: Date;
  isCurrentSession: boolean;
  status: 'Active' | 'Expired' | 'Revoked';
  riskLevel: 'Low' | 'Medium' | 'High';
}

export interface DeviceInfo {
  deviceId: string;
  deviceName: string;
  deviceType: 'Web' | 'Mobile' | 'Desktop';
  platform: string;
  browser: string;
  fingerprint: string;
  isTrusted: boolean;
  lastUsed: Date;
  registeredDate: Date;
  location: string;
}

export interface SecurityEvent {
  id: string;
  userId: string;
  eventType: 'login' | 'logout' | 'failed_login' | 'login_error' | 'password_change' | 'mfa_enabled' | 'suspicious_activity' | 'permission_denied';
  severity: 'low' | 'medium' | 'high' | 'critical';
  description: string;
  ipAddress: string;
  userAgent: string;
  timestamp: Date;
  metadata?: Record<string, unknown>;
}

export interface MFASetup {
  secret: string;
  qrCode: string;
  backupCodes: string[];
}

export interface MFAVerification {
  token: string;
  code: string;
}

export interface PasswordResetRequest {
  loginId: string;
  email: string;
}

export interface PasswordReset {
  token: string;
  newPassword: string;
  confirmPassword: string;
}

export interface AuthState {
  user: User | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  error: string | null;
  sessionInfo: SessionInfo | null;
}