/**
 * Session Management Types
 * Production-ready type definitions for session tracking and management
 */

export interface Session {
  id: string
  sessionKey: string
  userUID: string
  loginId: string
  token: string
  deviceId: string
  deviceType: 'Web' | 'Mobile' | 'Desktop'
  deviceName: string
  platform: string
  browser: string
  ipAddress: string
  location?: string
  userAgent: string
  loginTime: Date
  lastActivityTime: Date
  expiryTime: Date
  isActive: boolean
  isCurrentSession: boolean
  riskLevel: 'Low' | 'Medium' | 'High'
  metadata?: Record<string, any>
}

export interface SessionActivity {
  sessionId: string
  activityType: 'page_view' | 'api_call' | 'action' | 'idle' | 'focus' | 'blur'
  timestamp: Date
  details?: Record<string, any>
}

export interface ConcurrentSession {
  userUID: string
  activeSessions: Session[]
  maxAllowed: number
  policy: 'block_new' | 'logout_oldest' | 'logout_all' | 'allow_all'
}

export interface SessionConfig {
  sessionTimeout: number // in minutes
  idleTimeout: number // in minutes
  maxConcurrentSessions: number
  allowMultipleDevices: boolean
  requireDeviceTrust: boolean
  sessionExtendOnActivity: boolean
  trackActivityDetails: boolean
}

export interface SessionValidation {
  isValid: boolean
  reason?: 'expired' | 'invalid_token' | 'user_inactive' | 'ip_mismatch' | 'device_mismatch' | 'concurrent_limit'
  session?: Session
}

export interface DeviceFingerprint {
  deviceId: string
  platform: string
  browser: string
  screenResolution: string
  timezone: string
  language: string
  colorDepth: number
  hardwareConcurrency: number
  deviceMemory?: number
  touchSupport: boolean
  webGL: string
  canvas: string
  fonts: string[]
  plugins: string[]
}

export interface SessionApiResponse<T = any> {
  Data?: T
  StatusCode?: number
  IsSuccess?: boolean
  ErrorMessage?: string
  Message?: string
}

export interface SessionCreateRequest {
  userUID: string
  loginId: string
  token: string
  deviceFingerprint: DeviceFingerprint
  ipAddress?: string
  userAgent?: string
}

export interface SessionUpdateRequest {
  sessionKey: string
  lastActivityTime?: Date
  metadata?: Record<string, any>
}

export interface SessionTerminateRequest {
  sessionKey: string
  reason: 'logout' | 'timeout' | 'admin_action' | 'security' | 'concurrent_limit'
  terminatedBy?: string
}

export interface SessionListRequest {
  userUID?: string
  activeOnly?: boolean
  pageNumber: number
  pageSize: number
  sortBy?: 'loginTime' | 'lastActivity' | 'device'
  sortDirection?: 'asc' | 'desc'
}

export interface SessionStats {
  totalSessions: number
  activeSessions: number
  uniqueUsers: number
  averageSessionDuration: number
  deviceBreakdown: Record<string, number>
  locationBreakdown: Record<string, number>
  hourlyActivity: Array<{ hour: number; count: number }>
}

// Session events for real-time tracking
export interface SessionEvent {
  type: 'session_started' | 'session_ended' | 'session_extended' | 'concurrent_login_attempt' | 'suspicious_activity'
  sessionId: string
  userUID: string
  timestamp: Date
  details?: Record<string, any>
}

// Session security alerts
export interface SessionSecurityAlert {
  id: string
  sessionId: string
  alertType: 'new_device' | 'new_location' | 'concurrent_limit' | 'suspicious_pattern' | 'rapid_requests'
  severity: 'low' | 'medium' | 'high' | 'critical'
  message: string
  timestamp: Date
  resolved: boolean
  resolvedBy?: string
  resolvedAt?: Date
}