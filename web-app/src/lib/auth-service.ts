import {
  User,
  LoginCredentials,
  LoginResponse,
  SessionInfo,
  DeviceInfo,
  SecurityEvent,
  MFAVerification
} from "@/types/auth.types";
import { auditService } from "./audit-service";
import { AuditLogLevel } from "@/types/audit.types";
// Removed crypto-utils import as we're using direct password authentication

class AuthService {
  private readonly TOKEN_KEY = "auth_token"; // Changed to match the working token key
  private readonly REFRESH_TOKEN_KEY = "winit_refresh_token";
  private readonly USER_KEY = "user_info"; // Changed to match the working user key
  private readonly API_BASE_URL =
    process.env.NEXT_PUBLIC_API_URL || "http://localhost:8000/api";

  // Production Authentication Method
  async login(credentials: LoginCredentials): Promise<LoginResponse> {
    try {
      // Generate challenge code in the required format (yyyyMMddHHmmss)
      const challengeCode = this.generateChallengeCode();

      // For production authentication, we need to encrypt password with challenge code using SHA256
      // The backend flow is: rawPassword + challengeCode -> SHA256 hash -> compare with stored encrypted password
      const hashedPassword = await this.encryptPasswordWithChallenge(
        credentials.password,
        challengeCode
      );

      // Use WINIT production authentication format
      const requestBody = {
        UserID: credentials.loginId,
        Password: hashedPassword,
        ChallengeCode: challengeCode,
        DeviceId: credentials.deviceFingerprint || "web-client-" + Date.now()
      };

      if (process.env.NODE_ENV === "development") {
        // Authentication process started
      }

      // Use the production Auth/GetToken endpoint (the main authentication endpoint)
      const response = await fetch(`${this.API_BASE_URL}/Auth/GetToken`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Accept: "application/json"
        },
        body: JSON.stringify(requestBody)
      });

      if (process.env.NODE_ENV === "development") {
        // Authentication request sent
      }

      // Check if response has content and is JSON
      const responseText = await response.text();
      if (process.env.NODE_ENV === "development") {
        // Response received
      }

      let data;
      try {
        data = responseText ? JSON.parse(responseText) : {};
      } catch (parseError) {
        // JSON parse error occurred

        // If JSON parsing fails, it might be an HTML error page or empty response
        return {
          success: false,
          token: "",
          refreshToken: "",
          user: {} as User,
          requiresMFA: false,
          expiresIn: 0,
          error: `Server returned non-JSON response: ${response.status} ${response.statusText}`
        };
      }

      if (process.env.NODE_ENV === "development") {
        // Response parsed successfully
      }

      // Handle production WINITAPI response structure
      if (response.ok && data.IsSuccess !== false && data.isSuccess !== false) {
        // Backend now returns PascalCase fields
        const responseData = data.Data || data;
        const token = responseData?.Token;

        if (!token) {
          // No token received from backend
          return {
            success: false,
            token: "",
            refreshToken: "",
            user: {} as User,
            requiresMFA: false,
            expiresIn: 0,
            error: "Authentication successful but no token received"
          };
        }

        // Extract user data from production WINITAPI response structure (PascalCase)
        const authMaster = responseData?.AuthMaster || {};
        const empData = authMaster?.Emp || {};
        const roleData = authMaster?.Role || {};
        const jobPositionData = authMaster?.JobPosition || {};

        // Create production user object from backend response
        // Backend now returns PascalCase fields
        if (!empData.Code && !empData.LoginId && !empData.UID) {
          console.error("Invalid employee data received:", empData);
          throw new Error("Invalid employee data received from backend");
        }

        const user: User = {
          id: empData.Id || empData.UID || empData.Code,
          uid: empData.UID || empData.Code,
          loginId: empData.LoginId || credentials.loginId,
          name: empData.Name || empData.AliasName,
          email: empData.Email || empData.EmailAddress || "",
          mobile: empData.Mobile || empData.PhoneNumber || "",
          status: empData.Status || "Active",
          lastLoginTime: new Date(),
          companyUID: empData.CompanyUID || "",
          preferences: {
            theme: empData.Theme || "system",
            language: empData.Language || "en",
            notifications: {
              email: Boolean(empData.EmailNotifications),
              sms: Boolean(empData.SmsNotifications),
              push: Boolean(empData.PushNotifications)
            },
            security: {
              mfaEnabled: Boolean(empData.MfaEnabled),
              trustedDevicesOnly: Boolean(empData.TrustedDevicesOnly),
              sessionTimeout: Number(empData.SessionTimeout) || 120
            }
          },
          roles: [
            {
              id:
                roleData.Id ||
                jobPositionData.UserRoleUID ||
                credentials.loginId,
              uid:
                roleData.UID ||
                roleData.Code ||
                jobPositionData.UserRoleUID ||
                credentials.loginId,
              roleNameEn:
                roleData.RoleNameEn ||
                roleData.Name ||
                roleData.RoleName ||
                jobPositionData.UserRoleUID ||
                credentials.loginId,
              code: roleData.Code || roleData.RoleCode || credentials.loginId,
              isAdmin: credentials.loginId.toLowerCase() === "admin", // Check if admin based on loginId
              isPrincipalRole: Boolean(roleData.IsPrincipalRole),
              isDistributorRole: Boolean(roleData.IsDistributorRole),
              isWebUser: true, // This is a web login
              isAppUser: Boolean(roleData.IsAppUser),
              isDefault: Boolean(roleData.IsDefault)
            }
          ],
          currentOrganization: {
            uid: empData.OrgUID || "",
            code: empData.OrgCode || "",
            name: empData.OrgName || "",
            type: empData.OrgType || "Principal",
            isActive: empData.OrgIsActive !== false
          },
          availableOrganizations: empData.Organizations
            ? empData.Organizations.map((org: Record<string, unknown>) => ({
                uid: org.UID || "",
                code: org.Code || "",
                name: org.Name || "",
                type: org.Type || "Principal",
                isActive: org.IsActive !== false
              }))
            : [
                {
                  uid: empData.OrgUID || "",
                  code: empData.OrgCode || "",
                  name: empData.OrgName || "",
                  type: empData.OrgType || "Principal",
                  isActive: empData.OrgIsActive !== false
                }
              ]
        };

        // Store tokens and user
        this.setToken(token);
        this.setRefreshToken(token); // Using same token as refresh for now
        this.setUser(user);

        // Console log the token for debugging purposes
        console.log("🔑 User login successful!");
        console.log("📋 Login ID:", credentials.loginId);
        console.log("🎫 Auth Token:", token);
        console.log("👤 User Info:", user);

        // Log security event
        await this.logSecurityEvent({
          userId: user.id,
          eventType: "login",
          severity: "low",
          description: "Successful login",
          metadata: { loginId: credentials.loginId }
        });

        // Track login in audit trail
        await auditService.trackSecurityEvent(
          {
            eventType: "LOGIN",
            ipAddress:
              typeof window !== "undefined"
                ? window.location.hostname
                : "unknown",
            userAgent:
              typeof navigator !== "undefined"
                ? navigator.userAgent
                : "unknown",
            details: {
              loginId: credentials.loginId,
              deviceId: requestBody.DeviceId,
              timestamp: new Date().toISOString()
            }
          },
          AuditLogLevel.INFO
        );

        return {
          success: true,
          token: token,
          refreshToken: token, // Using same token as refresh for now
          user,
          requiresMFA: false,
          expiresIn: 7200 // 2 hours
        };
      } else {
        // Log failed login
        await this.logSecurityEvent({
          userId: "unknown",
          eventType: "failed_login",
          severity: "medium",
          description: "Failed production login attempt",
          metadata: {
            loginId: credentials.loginId,
            error:
              data.ErrorMessage ||
              data.Message ||
              data.error ||
              "Invalid credentials",
            statusCode: response.status
          }
        });

        // Track failed login in audit trail
        await auditService.trackSecurityEvent(
          {
            eventType: "FAILED_LOGIN",
            ipAddress:
              typeof window !== "undefined"
                ? window.location.hostname
                : "unknown",
            userAgent:
              typeof navigator !== "undefined"
                ? navigator.userAgent
                : "unknown",
            details: {
              loginId: credentials.loginId,
              error:
                data.ErrorMessage ||
                data.Message ||
                data.error ||
                "Invalid credentials",
              statusCode: response.status,
              timestamp: new Date().toISOString()
            }
          },
          AuditLogLevel.WARNING
        );

        return {
          success: false,
          token: "",
          refreshToken: "",
          user: {} as User,
          requiresMFA: false,
          expiresIn: 0,
          error:
            data.ErrorMessage ||
            data.Message ||
            data.error ||
            "Invalid credentials"
        };
      }
    } catch (error) {
      // Login error occurred

      await this.logSecurityEvent({
        userId: "unknown",
        eventType: "login_error",
        severity: "high",
        description: "Login service error",
        metadata: {
          loginId: credentials.loginId,
          error: error instanceof Error ? error.message : "Unknown error"
        }
      });

      return {
        success: false,
        token: "",
        refreshToken: "",
        user: {} as User,
        requiresMFA: false,
        expiresIn: 0,
        error: "Authentication service unavailable"
      };
    }
  }

  async logout(): Promise<void> {
    try {
      const user = this.getCurrentUser();
      const token = this.getToken();

      if (user && token) {
        // Notify backend about logout
        try {
          await fetch(`${this.API_BASE_URL}/Auth/Logout`, {
            method: "POST",
            headers: {
              "Content-Type": "application/json",
              Authorization: `Bearer ${token}`
            }
          });
        } catch {
          // Continue with local logout even if backend call fails
        }

        await this.logSecurityEvent({
          userId: user.id,
          eventType: "logout",
          severity: "low",
          description: "User logged out"
        });

        // Track logout in audit trail
        await auditService.trackSecurityEvent(
          {
            eventType: "LOGOUT",
            ipAddress:
              typeof window !== "undefined"
                ? window.location.hostname
                : "unknown",
            userAgent:
              typeof navigator !== "undefined"
                ? navigator.userAgent
                : "unknown",
            sessionId: token.substring(0, 10), // Use first 10 chars of token as session identifier
            details: {
              userId: user.uid,
              loginId: user.loginId,
              timestamp: new Date().toISOString()
            }
          },
          AuditLogLevel.INFO
        );
      }

      // Clear all stored data
      this.clearTokens();
      this.clearUser();
    } catch (error) {
      // Logout error occurred
    }
  }

  async refreshToken(): Promise<string | null> {
    try {
      const refreshToken = this.getRefreshToken();
      if (!refreshToken) return null;

      // Since RefreshToken endpoint doesn't exist in the backend,
      // we'll return null and rely on re-authentication
      // In production, the user would need to log in again when token expires
      // RefreshToken endpoint not available - user will need to re-authenticate
      return null;
    } catch (error) {
      // Token refresh error occurred
      return null;
    }
  }

  async validateSession(): Promise<boolean> {
    try {
      const token = this.getToken();
      const user = this.getCurrentUser();

      // If no token or user, session is invalid
      if (!token || !user) {
        return false;
      }

      // Basic token validation - check if it's not expired
      const tokenData = this.parseToken(token);

      if (tokenData && tokenData.exp) {
        const currentTime = Math.floor(Date.now() / 1000);

        if (tokenData.exp < currentTime) {
          // Token is expired, clear auth and return false
          this.clearTokens();
          this.clearUser();
          return false;
        }
      }

      // Token exists and appears valid
      return true;
    } catch (error) {
      return false;
    }
  }

  async resetPassword(loginId: string): Promise<boolean> {
    try {
      const response = await fetch(
        `${this.API_BASE_URL}/Auth/VerifyUserIdAndSendRandomPassword`,
        {
          method: "POST",
          headers: {
            "Content-Type": "application/json",
            Accept: "application/json"
          },
          body: JSON.stringify({ LoginId: loginId })
        }
      );

      if (response.ok) {
        const data = await response.json();
        return data.IsSuccess !== false;
      }

      return false;
    } catch (error) {
      // Password reset error occurred
      return false;
    }
  }

  async verifyMFA(verification: MFAVerification): Promise<boolean> {
    try {
      const response = await fetch(`${this.API_BASE_URL}/Auth/VerifyMFA`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Accept: "application/json",
          Authorization: `Bearer ${this.getToken()}`
        },
        body: JSON.stringify(verification)
      });

      if (response.ok) {
        const data = await response.json();
        return data.IsSuccess !== false;
      }

      return false;
    } catch (error) {
      // MFA verification error occurred
      return false;
    }
  }

  // Session Management
  async getActiveSessions(): Promise<SessionInfo[]> {
    try {
      const token = this.getToken();
      if (!token) return [];

      const response = await fetch(
        `${this.API_BASE_URL}/Auth/GetActiveSessions`,
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
        return data.Data || data.data || [];
      }

      return [];
    } catch (error) {
      // Get sessions error occurred
      return [];
    }
  }

  async revokeSession(sessionId: string): Promise<boolean> {
    try {
      const token = this.getToken();
      if (!token) return false;

      const response = await fetch(`${this.API_BASE_URL}/Auth/RevokeSession`, {
        method: "POST",
        headers: {
          Authorization: `Bearer ${token}`,
          "Content-Type": "application/json",
          Accept: "application/json"
        },
        body: JSON.stringify({ sessionId })
      });

      if (response.ok) {
        const data = await response.json();
        return data.IsSuccess !== false;
      }

      return false;
    } catch (error) {
      // Revoke session error occurred
      return false;
    }
  }

  // Device Management
  async getTrustedDevices(): Promise<DeviceInfo[]> {
    try {
      const token = this.getToken();
      if (!token) return [];

      const response = await fetch(
        `${this.API_BASE_URL}/Auth/GetTrustedDevices`,
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
        return data.Data || data.data || [];
      }

      return [];
    } catch (error) {
      // Get devices error occurred
      return [];
    }
  }

  // Security Events
  private async logSecurityEvent(
    event: Omit<SecurityEvent, "id" | "timestamp" | "ipAddress" | "userAgent">
  ): Promise<void> {
    try {
      const securityEvent: SecurityEvent = {
        ...event,
        id: `evt_${Date.now()}`,
        timestamp: new Date(),
        ipAddress: await this.getClientIP(),
        userAgent: navigator.userAgent
      };

      // Skip audit trail logging if endpoint doesn't exist
      // This is optional functionality for production environments
    } catch (error) {
      // Log security event error occurred
    }
  }

  private async getClientIP(): Promise<string> {
    try {
      // Skip backend IP detection if endpoint doesn't exist
      // Just use browser-based identification
      return this.getBrowserIP();
    } catch {
      return "unknown";
    }
  }

  private getBrowserIP(): string {
    // Use browser capabilities to detect network info without external APIs
    if (typeof window !== "undefined") {
      // Use timestamp and random for unique session identification
      const timestamp = Date.now().toString(36);
      const random = Math.random().toString(36).substring(2, 7);
      return `browser-${timestamp}-${random}`;
    }
    return "server-side";
  }

  private parseToken(
    token: string
  ): { exp: number; sub: string; iat: number; roles: string[] } | null {
    try {
      const parts = token.split(".");
      if (parts.length !== 3) throw new Error("Invalid token");

      return JSON.parse(atob(parts[1]));
    } catch (error) {
      // Parse token error occurred
      return null;
    }
  }

  // Storage Methods
  getToken(): string | null {
    if (typeof window === "undefined") return null;
    return localStorage.getItem(this.TOKEN_KEY);
  }

  private setToken(token: string): void {
    if (typeof window === "undefined") return;
    localStorage.setItem(this.TOKEN_KEY, token);
    console.log("💾 Token stored in localStorage:", token);
  }

  getRefreshToken(): string | null {
    if (typeof window === "undefined") return null;
    return localStorage.getItem(this.REFRESH_TOKEN_KEY);
  }

  private setRefreshToken(token: string): void {
    if (typeof window === "undefined") return;
    localStorage.setItem(this.REFRESH_TOKEN_KEY, token);
  }

  getCurrentUser(): User | null {
    if (typeof window === "undefined") return null;
    const userData = localStorage.getItem(this.USER_KEY);
    return userData ? JSON.parse(userData) : null;
  }

  setUser(user: User): void {
    if (typeof window === "undefined") return;
    localStorage.setItem(this.USER_KEY, JSON.stringify(user));
  }

  clearTokens(): void {
    if (typeof window === "undefined") return;
    localStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem(this.REFRESH_TOKEN_KEY);
  }

  clearUser(): void {
    if (typeof window === "undefined") return;
    localStorage.removeItem(this.USER_KEY);
  }

  // Utility Methods
  isAuthenticated(): boolean {
    return !!this.getToken() && !!this.getCurrentUser();
  }

  getAuthHeaders(): Record<string, string> {
    const token = this.getToken();
    if (!token) {
      return { "Content-Type": "application/json" };
    }
    return {
      Authorization: `Bearer ${token}`,
      "Content-Type": "application/json"
    };
  }

  /**
   * Get current username as string (for backward compatibility)
   */
  getCurrentUsername(): string {
    const user = this.getCurrentUser();
    return user?.loginId || "SYSTEM";
  }

  /**
   * Get user info from token (for backward compatibility)
   */
  getUserInfo(): User | null {
    return this.getCurrentUser();
  }

  // Production authentication helper methods
  private generateChallengeCode(): string {
    const now = new Date();
    const year = now.getFullYear();
    const month = String(now.getMonth() + 1).padStart(2, "0");
    const day = String(now.getDate()).padStart(2, "0");
    const hours = String(now.getHours()).padStart(2, "0");
    const minutes = String(now.getMinutes()).padStart(2, "0");
    const seconds = String(now.getSeconds()).padStart(2, "0");
    return `${year}${month}${day}${hours}${minutes}${seconds}`;
  }

  private async encryptPasswordWithChallenge(
    password: string,
    challengeCode: string
  ): Promise<string> {
    // This matches the backend SHACommonFunctions.EncryptPasswordWithChallenge method
    const passwordWithChallenge = password + challengeCode;
    return await this.hashPasswordWithSHA256(passwordWithChallenge);
  }

  private async hashPasswordWithSHA256(input: string): Promise<string> {
    // Use Web Crypto API for SHA256 hashing (same as backend C# implementation)
    const encoder = new TextEncoder();
    const data = encoder.encode(input);
    const hashBuffer = await crypto.subtle.digest("SHA-256", data);
    const hashArray = Array.from(new Uint8Array(hashBuffer));
    const hashBase64 = btoa(String.fromCharCode.apply(null, hashArray));
    return hashBase64;
  }
}

export const authService = new AuthService();

// Export commonly used functions for backward compatibility
export const getCurrentUser = () => authService.getCurrentUsername();
export const getAuthHeaders = () => authService.getAuthHeaders();
export const isAuthenticated = () => authService.isAuthenticated();
