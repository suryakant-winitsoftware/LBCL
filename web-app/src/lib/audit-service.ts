/**
 * Audit Trail Service
 * Production-ready service for tracking all user actions and system events
 */

import { authService } from "./auth-service";
import {
  type AuditTrailEntry,
  type AuditTrailPagingRequest,
  type AuditTrailPagingResponse,
  type AuditTrailApiResponse,
  type AuditContext,
  type SecurityAuditEvent,
  AuditLogLevel,
  type ChangeLog,
} from "@/types/audit.types";

class AuditService {
  private readonly API_BASE_URL =
    process.env.NEXT_PUBLIC_API_URL || "http://localhost:8000/api";
  // Use main API instead of separate microservice to avoid CORS issues
  private readonly AUDIT_API_URL = this.API_BASE_URL;
  private auditQueue: AuditTrailEntry[] = [];
  private isProcessingQueue = false;
  private context: AuditContext | null = null;

  /**
   * Set audit context for automatic tracking
   */
  setContext(context: AuditContext | null) {
    this.context = context;
  }

  /**
   * Create an audit trail entry
   */
  async createAudit(entry: Partial<AuditTrailEntry>): Promise<boolean> {
    try {
      // Temporarily disable audit calls to focus on sidebar issue
      if (process.env.NODE_ENV === "development") {
        // Audit call disabled in dev mode
        return true;
      }

      const user = authService.getCurrentUser();
      if (!user) {
        // No user context available
        return false;
      }

      const token = authService.getToken();
      if (!token) {
        // No auth token available
        return false;
      }

      // Build complete audit entry matching backend model
      const auditEntry: AuditTrailEntry = {
        // Required fields
        linkedItemType:
          entry.linkedItemType || this.context?.linkedItemType || "",
        linkedItemUID: entry.linkedItemUID || this.context?.linkedItemUID || "",
        commandType: entry.commandType || "Insert",
        commandDate: entry.commandDate || new Date(),
        empUID: entry.empUID || user.uid,
        empName: entry.empName || user.name,

        // Optional fields
        docNo: entry.docNo || this.context?.docNo,
        jobPositionUID: user.roles?.[0]?.uid,
        newData: entry.newData || {},
        originalDataId: entry.originalDataId,
        hasChanges: entry.hasChanges || false,
        changeData: entry.changeData || [],

        // Note: Do not include non-existent fields like createdBy, createdTime, companyUID
      };

      // Try direct API call first
      try {
        const response = await fetch(
          `${this.AUDIT_API_URL}/AuditTrail/PublishAuditTrail`,
          {
            method: "POST",
            headers: {
              "Content-Type": "application/json",
              Authorization: `Bearer ${token}`,
            },
            body: JSON.stringify(auditEntry),
          }
        );

        if (response.ok) {
          return true;
        } else {
          // Fall back to queue
          this.queueAudit(auditEntry);
          return true;
        }
      } catch (apiError) {
        // Queue for batch processing
        this.queueAudit(auditEntry);
        return true;
      }
    } catch (error) {
      // Failed to create audit trail
      return false;
    }
  }

  /**
   * Queue audit entries for batch processing
   */
  private queueAudit(entry: AuditTrailEntry) {
    this.auditQueue.push(entry);

    // Process queue if not already processing
    if (!this.isProcessingQueue && this.auditQueue.length >= 10) {
      this.processQueue();
    }

    // Set timeout to process queue after 30 seconds regardless
    setTimeout(() => {
      if (this.auditQueue.length > 0 && !this.isProcessingQueue) {
        this.processQueue();
      }
    }, 30000);
  }

  /**
   * Process queued audit entries
   */
  private async processQueue() {
    if (this.isProcessingQueue || this.auditQueue.length === 0) return;

    this.isProcessingQueue = true;
    const entriesToProcess = [...this.auditQueue];
    this.auditQueue = [];

    try {
      const token = authService.getToken();
      if (!token) return;

      // Try multiple audit endpoints for compatibility
      const auditEndpoints = [
        `/AuditTrail/CreateAuditTrail`,
        `/AuditTrail/PublishAuditTrail`,
        `/Audit/CreateAudit`,
        `/Security/LogEvent`,
      ];

      for (const entry of entriesToProcess) {
        let success = false;

        for (const endpoint of auditEndpoints) {
          try {
            const response = await fetch(`${this.AUDIT_API_URL}${endpoint}`, {
              method: "POST",
              headers: {
                "Content-Type": "application/json",
                Authorization: `Bearer ${token}`,
              },
              body: JSON.stringify(entry),
            });

            if (response.ok) {
              success = true;
              break;
            }
          } catch (error) {
            // Continue to next endpoint
            continue;
          }
        }

        if (!success) {
          // Re-queue failed entries
          this.auditQueue.push(entry);
        }
      }
    } finally {
      this.isProcessingQueue = false;
    }
  }

  /**
   * Track a security event
   */
  async trackSecurityEvent(
    event: SecurityAuditEvent,
    level: AuditLogLevel = AuditLogLevel.INFO
  ): Promise<boolean> {
    return this.createAudit({
      linkedItemType: "SecurityEvent",
      linkedItemUID: `${event.eventType}_${Date.now()}`,
      commandType: event.eventType.includes("LOGIN")
        ? "Login"
        : event.eventType === "LOGOUT"
        ? "Logout"
        : "Update",
      newData: {
        eventType: event.eventType,
        level,
        ipAddress: event.ipAddress,
        userAgent: event.userAgent,
        sessionId: event.sessionId,
        timestamp: new Date().toISOString(),
        ...event.details,
      },
      hasChanges: false,
    });
  }

  /**
   * Track a CRUD operation with change detection
   */
  async trackCrudOperation<T extends Record<string, any>>(
    operation: "Insert" | "Update" | "Delete",
    entityType: string,
    entityUID: string,
    newData: T,
    oldData?: T
  ): Promise<boolean> {
    const changes: ChangeLog[] = [];

    // Calculate changes for updates
    if (operation === "Update" && oldData) {
      for (const key in newData) {
        if (oldData[key] !== newData[key]) {
          changes.push({
            field: key,
            oldValue: oldData[key],
            newValue: newData[key],
          });
        }
      }
    }

    return this.createAudit({
      linkedItemType: entityType,
      linkedItemUID: entityUID,
      commandType: operation,
      newData,
      hasChanges: changes.length > 0,
      changeData: changes.length > 0 ? changes : undefined,
      originalDataId: oldData?.uid,
    });
  }

  /**
   * Get audit trail for a specific entity
   */
  async getAuditTrail(
    linkedItemType: string,
    linkedItemUID: string
  ): Promise<AuditTrailEntry[]> {
    try {
      const token = authService.getToken();
      if (!token) return [];

      const response = await fetch(
        `${this.AUDIT_API_URL}/AuditTrail/${linkedItemType}/${linkedItemUID}`,
        {
          headers: {
            Authorization: `Bearer ${token}`,
          },
        }
      );

      if (!response.ok) {
        // Failed to fetch audit trail
        return [];
      }

      const data = await response.json();
      return Array.isArray(data) ? data : [];
    } catch (error) {
      // Error fetching audit trail
      return [];
    }
  }

  /**
   * Get paginated audit trail with filtering
   */
  async getAuditTrailPaged(
    request: AuditTrailPagingRequest
  ): Promise<AuditTrailPagingResponse> {
    try {
      const token = authService.getToken();
      if (!token) {
        return {
          data: [],
          pageNumber: request.pageNumber,
          pageSize: request.pageSize,
          totalCount: 0,
        };
      }

      const response = await fetch(
        `${this.AUDIT_API_URL}/AuditTrail/GetAuditTrailsAsyncByPaging`,
        {
          method: "POST",
          headers: {
            "Content-Type": "application/json",
            Authorization: `Bearer ${token}`,
          },
          body: JSON.stringify(request),
        }
      );

      if (!response.ok) {
        // Failed to fetch paged audit trail
        return {
          data: [],
          pageNumber: request.pageNumber,
          pageSize: request.pageSize,
          totalCount: 0,
        };
      }

      const result: AuditTrailApiResponse<AuditTrailPagingResponse> =
        await response.json();

      if (result.IsSuccess && result.Data) {
        return result.Data;
      }

      return {
        data: [],
        pageNumber: request.pageNumber,
        pageSize: request.pageSize,
        totalCount: 0,
      };
    } catch (error) {
      // Error fetching paged audit trail
      return {
        data: [],
        pageNumber: request.pageNumber,
        pageSize: request.pageSize,
        totalCount: 0,
      };
    }
  }

  /**
   * Helper to track page views
   */
  async trackPageView(pageName: string, metadata?: Record<string, any>) {
    return this.createAudit({
      linkedItemType: "PageView",
      linkedItemUID: pageName,
      commandType: "View",
      newData: {
        page: pageName,
        timestamp: new Date().toISOString(),
        ...metadata,
      },
    });
  }

  /**
   * Helper to track exports
   */
  async trackExport(
    entityType: string,
    format: string,
    recordCount: number,
    filters?: any
  ) {
    return this.createAudit({
      linkedItemType: entityType,
      linkedItemUID: `Export_${Date.now()}`,
      commandType: "Export",
      newData: {
        format,
        recordCount,
        filters,
        timestamp: new Date().toISOString(),
      },
    });
  }

  /**
   * Clear context
   */
  clearContext() {
    this.context = null;
  }

  /**
   * Flush any pending audit entries
   */
  async flush() {
    if (this.auditQueue.length > 0) {
      await this.processQueue();
    }
  }
}

// Export singleton instance
export const auditService = new AuditService();

// Export audit helpers
export const auditHelpers = {
  /**
   * Create audit context for a component
   */
  createContext: (type: string, uid: string, docNo?: string): AuditContext => ({
    linkedItemType: type,
    linkedItemUID: uid,
    docNo,
  }),

  /**
   * Format audit entry for display
   */
  formatAuditEntry: (entry: AuditTrailEntry): string => {
    const action = entry.commandType;
    const user = entry.empName;
    const date = new Date(entry.commandDate).toLocaleString();
    return `${action} by ${user} on ${date}`;
  },
};
