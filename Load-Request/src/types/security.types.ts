export interface SecuritySettings {
  passwordPolicy: PasswordPolicy;
  sessionSecurity: SessionSecurity;
  mfaSettings: MFASettings;
  deviceManagement: DeviceManagement;
  accessControl: AccessControlSettings;
}

export interface PasswordPolicy {
  minLength: number;
  requireUppercase: boolean;
  requireLowercase: boolean;
  requireNumbers: boolean;
  requireSpecialChars: boolean;
  maxAge: number; // days
  preventReuse: number; // last N passwords
  lockoutThreshold: number; // failed attempts
  lockoutDuration: number; // minutes
}

export interface SessionSecurity {
  maxConcurrentSessions: number;
  sessionTimeout: number; // minutes
  extendOnActivity: boolean;
  forceLogoutOnSuspicious: boolean;
  requireReauthForSensitive: boolean;
}

export interface MFASettings {
  enforced: boolean;
  allowedMethods: MFAMethod[];
  gracePeriod: number; // days
  backupCodesCount: number;
}

export type MFAMethod = 'totp' | 'sms' | 'email' | 'hardware_key';

export interface DeviceManagement {
  requireTrustedDevices: boolean;
  autoTrustDuration: number; // days
  maxTrustedDevices: number;
  deviceFingerprintingEnabled: boolean;
}

export interface AccessControlSettings {
  ipWhitelisting: IPWhitelist[];
  timeBasedAccess: TimeBasedAccess[];
  locationBasedAccess: boolean;
  geoBlocking: GeoBlockingRule[];
}

export interface IPWhitelist {
  id: string;
  ipAddress: string;
  cidr?: string;
  description: string;
  isActive: boolean;
  createdAt: Date;
  expiresAt?: Date;
}

export interface TimeBasedAccess {
  id: string;
  userId?: string;
  roleUID?: string;
  startTime: string; // HH:MM format
  endTime: string;   // HH:MM format
  daysOfWeek: number[]; // 0-6 (Sunday-Saturday)
  timezone: string;
  isActive: boolean;
}

export interface GeoBlockingRule {
  id: string;
  countryCode: string;
  region?: string;
  action: 'allow' | 'block';
  priority: number;
  isActive: boolean;
}

export interface SecurityAnalytics {
  failedLogins: FailedLoginStat[];
  suspiciousActivities: SuspiciousActivity[];
  deviceAnalytics: DeviceAnalytics;
  accessPatterns: AccessPattern[];
  securityScore: SecurityScore;
}

export interface FailedLoginStat {
  date: string;
  count: number;
  uniqueIPs: number;
  topIPs: { ip: string; count: number }[];
}

export interface SuspiciousActivity {
  id: string;
  userId: string;
  type: 'unusual_location' | 'unusual_device' | 'unusual_time' | 'brute_force' | 'privilege_escalation';
  description: string;
  riskLevel: 'low' | 'medium' | 'high' | 'critical';
  timestamp: Date;
  metadata: Record<string, unknown>;
  status: 'open' | 'investigating' | 'resolved' | 'false_positive';
}

export interface DeviceAnalytics {
  totalDevices: number;
  trustedDevices: number;
  deviceTypes: { type: string; count: number }[];
  operatingSystems: { os: string; count: number }[];
  browsers: { browser: string; count: number }[];
}

export interface AccessPattern {
  userId: string;
  userName: string;
  avgLoginTime: string;
  mostActiveHours: number[];
  commonLocations: string[];
  deviceConsistency: number; // 0-1 score
  riskScore: number; // 0-100
}

export interface SecurityScore {
  overall: number; // 0-100
  factors: {
    passwordStrength: number;
    mfaAdoption: number;
    deviceHygiene: number;
    accessPatterns: number;
    complianceLevel: number;
  };
  recommendations: SecurityRecommendation[];
}

export interface SecurityRecommendation {
  id: string;
  type: 'password' | 'mfa' | 'device' | 'access' | 'compliance';
  severity: 'low' | 'medium' | 'high' | 'critical';
  title: string;
  description: string;
  actionRequired: string;
  estimatedImpact: number; // score improvement
}

export interface AuditLog {
  id: string;
  userId: string;
  userName: string;
  action: string;
  resource: string;
  resourceId?: string;
  result: 'success' | 'failure' | 'denied';
  timestamp: Date;
  ipAddress: string;
  userAgent: string;
  sessionId: string;
  metadata?: Record<string, unknown>;
}

export interface ComplianceReport {
  id: string;
  reportType: 'daily' | 'weekly' | 'monthly' | 'quarterly';
  generatedAt: Date;
  period: {
    start: Date;
    end: Date;
  };
  metrics: {
    totalUsers: number;
    activeUsers: number;
    failedLogins: number;
    successfulLogins: number;
    mfaAdoption: number;
    passwordCompliance: number;
    securityIncidents: number;
  };
  violations: ComplianceViolation[];
}

export interface ComplianceViolation {
  id: string;
  type: string;
  severity: 'low' | 'medium' | 'high' | 'critical';
  description: string;
  userId?: string;
  timestamp: Date;
  status: 'open' | 'resolved' | 'acknowledged';
  remediation?: string;
}