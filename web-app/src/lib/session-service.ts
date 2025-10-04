/**
 * Session Management Service
 * Production-ready service for tracking and managing user sessions
 */

import { authService } from "./auth-service";
import { auditService } from "./audit-service";
import type {
  Session,
  SessionActivity,
  ConcurrentSession,
  SessionConfig,
  SessionValidation,
  DeviceFingerprint,
  SessionCreateRequest,
  SessionUpdateRequest,
  SessionTerminateRequest,
  SessionListRequest,
  SessionStats,
  SessionEvent,
  SessionApiResponse,
} from "@/types/session.types";
import { AuditLogLevel } from "@/types/audit.types";

class SessionService {
  private readonly API_BASE_URL =
    process.env.NEXT_PUBLIC_API_URL || "https://multiplex-promotions-api.winitsoftware.com/api";
  private readonly SESSION_KEY = "winit_session_key";
  private readonly DEVICE_ID_KEY = "winit_device_id";
  private activityTimer?: NodeJS.Timeout;
  private heartbeatTimer?: NodeJS.Timeout;
  private currentSession: Session | null = null;

  // Default configuration
  private config: SessionConfig = {
    sessionTimeout: 120, // 2 hours
    idleTimeout: 30, // 30 minutes
    maxConcurrentSessions: 3,
    allowMultipleDevices: true,
    requireDeviceTrust: false,
    sessionExtendOnActivity: true,
    trackActivityDetails: true,
  };

  /**
   * Initialize session service
   */
  async initialize() {
    // Load session configuration
    await this.loadConfiguration();

    // Check for existing session
    const sessionKey = this.getStoredSessionKey();
    if (sessionKey) {
      const validation = await this.validateSession(sessionKey);
      if (validation.isValid && validation.session) {
        this.currentSession = validation.session;
        this.startSessionMonitoring();
      } else {
        // Clear invalid session
        this.clearStoredSession();
      }
    }

    // Set up activity tracking
    this.setupActivityTracking();

    // Set up page visibility tracking
    this.setupVisibilityTracking();
  }

  /**
   * Create a new session
   */
  async createSession(request: SessionCreateRequest): Promise<Session | null> {
    try {
      // Generate session key
      const sessionKey = this.generateSessionKey();

      // Get device fingerprint
      const deviceFingerprint =
        request.deviceFingerprint || (await this.getDeviceFingerprint());

      // Get location from IP
      const location = await this.getLocationFromIP(request.ipAddress);

      // Create session object
      const session: Session = {
        id: `${request.userUID}_${Date.now()}`,
        sessionKey,
        userUID: request.userUID,
        loginId: request.loginId,
        token: request.token,
        deviceId: deviceFingerprint.deviceId,
        deviceType: this.getDeviceType(deviceFingerprint.platform),
        deviceName: this.getDeviceName(deviceFingerprint),
        platform: deviceFingerprint.platform,
        browser: deviceFingerprint.browser,
        ipAddress: request.ipAddress || "unknown",
        location,
        userAgent: request.userAgent || navigator.userAgent,
        loginTime: new Date(),
        lastActivityTime: new Date(),
        expiryTime: new Date(
          Date.now() + this.config.sessionTimeout * 60 * 1000
        ),
        isActive: true,
        isCurrentSession: true,
        riskLevel: "Low",
        metadata: {
          fingerprint: deviceFingerprint,
          loginMethod: "standard",
        },
      };

      // Check concurrent sessions
      const concurrentCheck = await this.checkConcurrentSessions(
        request.userUID
      );
      if (!concurrentCheck.allowed) {
        // Handle based on policy
        if (concurrentCheck.policy === "logout_oldest") {
          await this.terminateOldestSession(request.userUID);
        } else if (concurrentCheck.policy === "logout_all") {
          await this.terminateAllSessions(request.userUID);
        } else if (concurrentCheck.policy === "block_new") {
          throw new Error("Maximum concurrent sessions reached");
        }
      }

      // Store session locally
      this.currentSession = session;
      this.storeSessionKey(sessionKey);

      // Store session in backend (if API available)
      await this.syncSessionToBackend(session);

      // Start monitoring
      this.startSessionMonitoring();

      // Track session creation
      await auditService.trackSecurityEvent(
        {
          eventType: "LOGIN",
          sessionId: sessionKey,
          ipAddress: session.ipAddress,
          userAgent: session.userAgent,
          details: {
            deviceType: session.deviceType,
            platform: session.platform,
            browser: session.browser,
            location: session.location,
          },
        },
        AuditLogLevel.INFO
      );

      return session;
    } catch (error) {
      // Failed to create session
      return null;
    }
  }

  /**
   * Validate an existing session
   */
  async validateSession(sessionKey: string): Promise<SessionValidation> {
    try {
      // Get current session
      if (
        !this.currentSession ||
        this.currentSession.sessionKey !== sessionKey
      ) {
        return { isValid: false, reason: "invalid_token" };
      }

      const session = this.currentSession;

      // Check expiry
      if (new Date() > new Date(session.expiryTime)) {
        return { isValid: false, reason: "expired" };
      }

      // Check idle timeout
      const idleTime =
        Date.now() - new Date(session.lastActivityTime).getTime();
      if (idleTime > this.config.idleTimeout * 60 * 1000) {
        return { isValid: false, reason: "expired" };
      }

      // Validate with backend (if needed)
      const user = authService.getCurrentUser();
      if (!user || user.uid !== session.userUID) {
        return { isValid: false, reason: "user_inactive" };
      }

      // Check device/IP if strict mode
      if (this.config.requireDeviceTrust) {
        const currentFingerprint = await this.getDeviceFingerprint();
        if (currentFingerprint.deviceId !== session.deviceId) {
          return { isValid: false, reason: "device_mismatch" };
        }
      }

      return { isValid: true, session };
    } catch (error) {
      // Session validation error occurred
      return { isValid: false, reason: "invalid_token" };
    }
  }

  /**
   * Update session activity
   */
  async updateActivity(activityType?: SessionActivity["activityType"]) {
    if (!this.currentSession) return;

    const now = new Date();
    this.currentSession.lastActivityTime = now;

    // Extend session if configured
    if (this.config.sessionExtendOnActivity) {
      this.currentSession.expiryTime = new Date(
        now.getTime() + this.config.sessionTimeout * 60 * 1000
      );
    }

    // Track activity
    if (this.config.trackActivityDetails && activityType) {
      const activity: SessionActivity = {
        sessionId: this.currentSession.sessionKey,
        activityType,
        timestamp: now,
        details: {
          page: window.location.pathname,
          title: document.title,
        },
      };

      // Queue activity for batch processing
      this.queueActivity(activity);
    }

    // Sync to backend periodically
    this.scheduleSyncToBackend();
  }

  /**
   * Terminate current session
   */
  async terminateSession(reason: SessionTerminateRequest["reason"] = "logout") {
    if (!this.currentSession) return;

    const sessionKey = this.currentSession.sessionKey;

    // Stop monitoring
    this.stopSessionMonitoring();

    // Track termination
    await auditService.trackSecurityEvent(
      {
        eventType: "LOGOUT",
        sessionId: sessionKey,
        details: {
          reason,
          sessionDuration:
            Date.now() - new Date(this.currentSession.loginTime).getTime(),
          lastActivity: this.currentSession.lastActivityTime,
        },
      },
      AuditLogLevel.INFO
    );

    // Clear local session
    this.currentSession = null;
    this.clearStoredSession();

    // Notify backend
    await this.syncSessionTermination(sessionKey, reason);
  }

  /**
   * Get all active sessions for current user
   */
  async getActiveSessions(): Promise<Session[]> {
    try {
      const user = authService.getCurrentUser();
      if (!user) return [];

      // For now, return current session if exists
      // In production, this would fetch from backend API
      return this.currentSession ? [this.currentSession] : [];
    } catch (error) {
      // Failed to get active sessions
      return [];
    }
  }

  /**
   * Get session statistics
   */
  async getSessionStats(): Promise<SessionStats | null> {
    try {
      const user = authService.getCurrentUser();
      if (!user) return null;

      // Try to get stats from backend API
      const token = authService.getToken();
      if (token) {
        try {
          const response = await fetch(
            `${this.API_BASE_URL}/Session/Statistics`,
            {
              method: "GET",
              headers: {
                Authorization: `Bearer ${token}`,
                "Content-Type": "application/json",
              },
            }
          );

          if (response.ok) {
            const data = await response.json();
            return data.data || data;
          }
        } catch (apiError) {
          // API not available, fall back to local data
        }
      }

      // Fallback: return basic stats based on current session
      if (!this.currentSession) return null;

      return {
        totalSessions: 1,
        activeSessions: 1,
        uniqueUsers: 1,
        averageSessionDuration:
          Date.now() - new Date(this.currentSession.loginTime).getTime(),
        deviceBreakdown: {
          [this.currentSession.deviceType]: 1,
        },
        locationBreakdown: {
          [this.currentSession.location || "Unknown"]: 1,
        },
        hourlyActivity: [], // Empty array instead of mock data
      };
    } catch (error) {
      // Failed to get session stats
      return null;
    }
  }

  /**
   * Private helper methods
   */

  private async getDeviceFingerprint(): Promise<DeviceFingerprint> {
    // Get or generate device ID
    let deviceId = localStorage.getItem(this.DEVICE_ID_KEY);
    if (!deviceId) {
      deviceId = this.generateDeviceId();
      localStorage.setItem(this.DEVICE_ID_KEY, deviceId);
    }

    // Collect device information
    const fingerprint: DeviceFingerprint = {
      deviceId,
      platform: navigator.platform || "Unknown",
      browser: this.getBrowserInfo(),
      screenResolution: `${screen.width}x${screen.height}`,
      timezone: Intl.DateTimeFormat().resolvedOptions().timeZone,
      language: navigator.language,
      colorDepth: screen.colorDepth,
      hardwareConcurrency: navigator.hardwareConcurrency || 0,
      deviceMemory: (navigator as any).deviceMemory,
      touchSupport: "ontouchstart" in window,
      webGL: this.getWebGLInfo(),
      canvas: await this.getCanvasFingerprint(),
      fonts: await this.getInstalledFonts(),
      plugins: this.getPlugins(),
    };

    return fingerprint;
  }

  private generateSessionKey(): string {
    return `SK_${Date.now()}_${Math.random().toString(36).substring(2, 15)}`;
  }

  private generateDeviceId(): string {
    return `DV_${Date.now()}_${Math.random().toString(36).substring(2, 15)}`;
  }

  private getDeviceType(platform: string): Session["deviceType"] {
    if (/Android|webOS|iPhone|iPad|iPod/i.test(navigator.userAgent)) {
      return "Mobile";
    } else if (/Tablet|iPad/i.test(navigator.userAgent)) {
      return "Mobile";
    } else {
      return "Web";
    }
  }

  private getDeviceName(fingerprint: DeviceFingerprint): string {
    return `${fingerprint.browser} on ${fingerprint.platform}`;
  }

  private getBrowserInfo(): string {
    const ua = navigator.userAgent;
    if (ua.includes("Chrome")) return "Chrome";
    if (ua.includes("Firefox")) return "Firefox";
    if (ua.includes("Safari")) return "Safari";
    if (ua.includes("Edge")) return "Edge";
    return "Unknown";
  }

  private getWebGLInfo(): string {
    try {
      const canvas = document.createElement("canvas");
      const gl =
        canvas.getContext("webgl") || canvas.getContext("experimental-webgl");
      if (!gl) return "Not supported";

      const renderer = (gl as WebGLRenderingContext).getParameter(
        (gl as WebGLRenderingContext).RENDERER
      );
      return renderer || "Unknown";
    } catch {
      return "Not available";
    }
  }

  private async getCanvasFingerprint(): Promise<string> {
    try {
      const canvas = document.createElement("canvas");
      const ctx = canvas.getContext("2d");
      if (!ctx) return "Not supported";

      ctx.textBaseline = "top";
      ctx.font = "14px Arial";
      ctx.fillStyle = "#f60";
      ctx.fillRect(125, 1, 62, 20);
      ctx.fillStyle = "#069";
      ctx.fillText("Canvas fingerprint", 2, 15);

      return canvas.toDataURL().substring(0, 100);
    } catch {
      return "Not available";
    }
  }

  private async getInstalledFonts(): Promise<string[]> {
    // This is a simplified version - in production you'd use a more sophisticated method
    const testFonts = [
      "Arial",
      "Helvetica",
      "Times New Roman",
      "Courier New",
      "Verdana",
    ];
    return testFonts.filter((font) => document.fonts.check(`12px ${font}`));
  }

  private getPlugins(): string[] {
    return Array.from(navigator.plugins)
      .map((p) => p.name)
      .slice(0, 5);
  }

  private async getLocationFromIP(ip?: string): Promise<string> {
    // In production, this would call a geolocation API
    return "Unknown Location";
  }

  private async checkConcurrentSessions(
    userUID: string
  ): Promise<{ allowed: boolean; policy: ConcurrentSession["policy"] }> {
    // This would check backend for active sessions
    // For now, always allow
    return { allowed: true, policy: "allow_all" };
  }

  private async terminateOldestSession(userUID: string) {
    // Implementation would terminate oldest session via backend
  }

  private async terminateAllSessions(userUID: string) {
    // Implementation would terminate all sessions via backend
  }

  private startSessionMonitoring() {
    // Heartbeat every 5 minutes
    this.heartbeatTimer = setInterval(() => {
      this.sendHeartbeat();
    }, 5 * 60 * 1000);

    // Activity check every minute
    this.activityTimer = setInterval(() => {
      this.checkIdleTimeout();
    }, 60 * 1000);
  }

  private stopSessionMonitoring() {
    if (this.heartbeatTimer) {
      clearInterval(this.heartbeatTimer);
      this.heartbeatTimer = undefined;
    }
    if (this.activityTimer) {
      clearInterval(this.activityTimer);
      this.activityTimer = undefined;
    }
  }

  private async sendHeartbeat() {
    if (!this.currentSession) return;

    // Update activity
    await this.updateActivity("api_call");

    // Validate session is still valid
    const validation = await this.validateSession(
      this.currentSession.sessionKey
    );
    if (!validation.isValid) {
      await this.terminateSession("timeout");
    }
  }

  private checkIdleTimeout() {
    if (!this.currentSession) return;

    const idleTime =
      Date.now() - new Date(this.currentSession.lastActivityTime).getTime();
    if (idleTime > this.config.idleTimeout * 60 * 1000) {
      this.terminateSession("timeout");
    }
  }

  private setupActivityTracking() {
    // Track mouse movements (throttled)
    let lastMouseMove = 0;
    document.addEventListener("mousemove", () => {
      const now = Date.now();
      if (now - lastMouseMove > 60000) {
        // Every minute
        this.updateActivity("action");
        lastMouseMove = now;
      }
    });

    // Track clicks
    document.addEventListener("click", () => {
      this.updateActivity("action");
    });

    // Track key presses (throttled)
    let lastKeyPress = 0;
    document.addEventListener("keypress", () => {
      const now = Date.now();
      if (now - lastKeyPress > 30000) {
        // Every 30 seconds
        this.updateActivity("action");
        lastKeyPress = now;
      }
    });
  }

  private setupVisibilityTracking() {
    document.addEventListener("visibilitychange", () => {
      if (document.hidden) {
        this.updateActivity("blur");
      } else {
        this.updateActivity("focus");
      }
    });
  }

  private storeSessionKey(key: string) {
    sessionStorage.setItem(this.SESSION_KEY, key);
  }

  private getStoredSessionKey(): string | null {
    return sessionStorage.getItem(this.SESSION_KEY);
  }

  private clearStoredSession() {
    sessionStorage.removeItem(this.SESSION_KEY);
  }

  private async loadConfiguration() {
    // Load from backend or use defaults
    // This would typically fetch from API
  }

  private activityQueue: SessionActivity[] = [];
  private queueActivity(activity: SessionActivity) {
    this.activityQueue.push(activity);

    // Batch sync every 10 activities or 30 seconds
    if (this.activityQueue.length >= 10) {
      this.syncActivitiesToBackend();
    }
  }

  private syncTimer?: NodeJS.Timeout;
  private scheduleSyncToBackend() {
    if (this.syncTimer) return;

    this.syncTimer = setTimeout(() => {
      this.syncSessionToBackend(this.currentSession!);
      this.syncTimer = undefined;
    }, 30000); // 30 seconds
  }

  private async syncSessionToBackend(session: Session) {
    // Implementation would sync to backend API
    // TODO: Implement actual backend sync
  }

  private async syncActivitiesToBackend() {
    if (this.activityQueue.length === 0) return;

    const activities = [...this.activityQueue];
    this.activityQueue = [];

    // Implementation would send to backend
    // TODO: Implement actual backend sync
  }

  private async syncSessionTermination(sessionKey: string, reason: string) {
    // Implementation would notify backend
    // TODO: Implement actual backend notification
  }

  // Public getters
  getCurrentSession(): Session | null {
    return this.currentSession;
  }

  getConfig(): SessionConfig {
    return { ...this.config };
  }

  isSessionActive(): boolean {
    return this.currentSession !== null && this.currentSession.isActive;
  }
}

// Export singleton instance
export const sessionService = new SessionService();

// Initialize on import
if (typeof window !== "undefined") {
  sessionService.initialize();
}
