/**
 * Audit Trail Integration Layer
 * Production-ready integration for all CRUD operations
 */

import { auditService } from "./audit-service";
import { withAudit } from "@/hooks/use-audit";
import type { AuditTrailEntry } from "@/types/audit.types";

/**
 * Higher-order function to automatically add audit tracking to any service
 */
export function withAuditTracking<T extends Record<string, any>>(
  service: T,
  entityType: string
): T {
  const auditedService = {} as T;

  for (const methodName in service) {
    const method = service[methodName];

    if (typeof method === "function") {
      // Determine operation type based on method name
      let commandType: AuditTrailEntry["commandType"] = "View";

      if (
        methodName.includes("create") ||
        methodName.includes("add") ||
        methodName.includes("insert")
      ) {
        commandType = "Insert";
      } else if (
        methodName.includes("update") ||
        methodName.includes("edit") ||
        methodName.includes("save")
      ) {
        commandType = "Update";
      } else if (
        methodName.includes("delete") ||
        methodName.includes("remove")
      ) {
        commandType = "Delete";
      } else if (methodName.includes("export")) {
        commandType = "Export";
      }

      // Wrap the method with audit tracking
      auditedService[methodName] = withAudit(method.bind(service), {
        entityType,
        getEntityUID: (...args: any[]) => {
          // Try to extract UID from various argument patterns
          if (args[0]?.uid || args[0]?.UID) return args[0].uid || args[0].UID;
          if (typeof args[0] === "string") return args[0];
          return `${entityType}_${Date.now()}`;
        },
        commandType,
        getData: (...args: any[]) => ({
          method: methodName,
          args: args.length === 1 ? args[0] : args,
          timestamp: new Date().toISOString(),
        }),
      }) as T[Extract<keyof T, string>];
    } else {
      // Copy non-function properties as-is
      auditedService[methodName] = method;
    }
  }

  return auditedService;
}

/**
 * Audit middleware for API calls
 */
export function createAuditMiddleware() {
  return {
    /**
     * Intercept fetch requests to add audit tracking
     */
    fetch: async (url: string, options?: RequestInit): Promise<Response> => {
      const method = options?.method || "GET";
      const isWrite = ["POST", "PUT", "PATCH", "DELETE"].includes(method);

      // Parse entity info from URL
      const urlParts = url.split("/");
      const apiIndex = urlParts.indexOf("api");
      const controller = urlParts[apiIndex + 1] || "Unknown";
      const action = urlParts[apiIndex + 2] || "Unknown";

      // Track API call start
      if (isWrite) {
        await auditService.createAudit({
          linkedItemType: controller,
          linkedItemUID: `${action}_${Date.now()}`,
          commandType:
            method === "POST"
              ? "Insert"
              : method === "DELETE"
              ? "Delete"
              : "Update",
          newData: {
            url,
            method,
            action,
            requestBody: options?.body
              ? typeof options.body === "string"
                ? JSON.parse(options.body)
                : options.body
              : null,
            timestamp: new Date().toISOString(),
          },
        });
      }

      // Execute the request
      const response = await fetch(url, options);

      // Track response
      if (isWrite && !response.ok) {
        await auditService.createAudit({
          linkedItemType: `${controller}_Error`,
          linkedItemUID: `${action}_${Date.now()}`,
          commandType: "View",
          newData: {
            url,
            method,
            action,
            status: response.status,
            statusText: response.statusText,
            error: true,
            timestamp: new Date().toISOString(),
          },
        });
      }

      return response;
    },
  };
}

/**
 * Audit context provider for React components
 */
import React, { createContext, useContext, useEffect, ReactNode } from "react";

interface AuditContextValue {
  entityType: string;
  entityUID?: string;
  setContext: (type: string, uid?: string) => void;
}

const AuditContext = createContext<AuditContextValue | null>(null);

export function AuditProvider({
  children,
  entityType: initialType = "Unknown",
}: {
  children: ReactNode;
  entityType?: string;
}) {
  const [entityType, setEntityType] = React.useState(initialType);
  const [entityUID, setEntityUID] = React.useState<string>();

  const setContext = (type: string, uid?: string) => {
    setEntityType(type);
    setEntityUID(uid);
    auditService.setContext({
      linkedItemType: type,
      linkedItemUID: uid || "",
    });
  };

  useEffect(() => {
    auditService.setContext({
      linkedItemType: entityType,
      linkedItemUID: entityUID || "",
    });

    return () => {
      auditService.clearContext();
    };
  }, [entityType, entityUID]);

  return React.createElement(
    AuditContext.Provider,
    { value: { entityType, entityUID, setContext } },
    children
  );
}

export function useAuditContext() {
  const context = useContext(AuditContext);
  if (!context) {
    throw new Error("useAuditContext must be used within AuditProvider");
  }
  return context;
}

/**
 * Production-ready audit integration helpers
 */
export const auditHelpers = {
  /**
   * Track page navigation
   */
  trackNavigation: (fromPage: string, toPage: string) => {
    return auditService.trackPageView(toPage, {
      from: fromPage,
      navigation: true,
    });
  },

  /**
   * Track form submission
   */
  trackFormSubmit: (formName: string, data: any, success: boolean) => {
    return auditService.createAudit({
      linkedItemType: "Form",
      linkedItemUID: formName,
      commandType: success ? "Insert" : "View",
      newData: {
        formName,
        success,
        fields: Object.keys(data),
        timestamp: new Date().toISOString(),
      },
    });
  },

  /**
   * Track error
   */
  trackError: (error: Error, context: string) => {
    return auditService.createAudit({
      linkedItemType: "Error",
      linkedItemUID: `${context}_${Date.now()}`,
      commandType: "View",
      newData: {
        error: error.message,
        stack: error.stack,
        context,
        timestamp: new Date().toISOString(),
      },
    });
  },

  /**
   * Track performance metric
   */
  trackPerformance: (metric: string, duration: number, metadata?: any) => {
    return auditService.createAudit({
      linkedItemType: "Performance",
      linkedItemUID: metric,
      commandType: "View",
      newData: {
        metric,
        duration,
        ...metadata,
        timestamp: new Date().toISOString(),
      },
    });
  },
};

/**
 * Batch audit operations for performance
 */
export class AuditBatch {
  private entries: Partial<AuditTrailEntry>[] = [];
  private flushTimeout?: NodeJS.Timeout;

  add(entry: Partial<AuditTrailEntry>) {
    this.entries.push(entry);

    // Auto-flush after 10 entries or 5 seconds
    if (this.entries.length >= 10) {
      this.flush();
    } else if (!this.flushTimeout) {
      this.flushTimeout = setTimeout(() => this.flush(), 5000);
    }
  }

  async flush() {
    if (this.flushTimeout) {
      clearTimeout(this.flushTimeout);
      this.flushTimeout = undefined;
    }

    const entriesToFlush = [...this.entries];
    this.entries = [];

    // Send all entries
    for (const entry of entriesToFlush) {
      await auditService.createAudit(entry);
    }
  }
}

// Export singleton batch instance
export const auditBatch = new AuditBatch();

// Auto-flush on page unload
if (typeof window !== "undefined") {
  window.addEventListener("beforeunload", () => {
    auditBatch.flush();
    auditService.flush();
  });
}
