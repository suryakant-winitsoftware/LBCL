/**
 * Audit Trail Hooks
 * Production-ready React hooks for audit trail functionality
 */

import { useCallback, useEffect, useRef } from 'react'
import { useMutation, useQuery } from '@tanstack/react-query'
import { auditService } from '@/lib/audit-service'
import type {
  AuditTrailEntry,
  AuditTrailPagingRequest,
  UseAuditOptions,
  AuditContext
} from '@/types/audit.types'

/**
 * Hook to automatically track CRUD operations
 */
export function useAuditTracker(options?: UseAuditOptions) {
  const contextRef = useRef<AuditContext | null>(null)

  useEffect(() => {
    if (options?.context) {
      contextRef.current = options.context
      auditService.setContext(options.context)
    }

    return () => {
      auditService.clearContext()
    }
  }, [options?.context])

  const trackCreate = useCallback(
    async <T extends Record<string, any>>(
      entityType: string,
      entityUID: string,
      data: T
    ) => {
      if (options?.enabled === false) return

      await auditService.trackCrudOperation(
        'Insert',
        entityType,
        entityUID,
        data
      )
    },
    [options?.enabled]
  )

  const trackUpdate = useCallback(
    async <T extends Record<string, any>>(
      entityType: string,
      entityUID: string,
      newData: T,
      oldData?: T
    ) => {
      if (options?.enabled === false) return

      await auditService.trackCrudOperation(
        'Update',
        entityType,
        entityUID,
        newData,
        oldData
      )
    },
    [options?.enabled]
  )

  const trackDelete = useCallback(
    async <T extends Record<string, any>>(
      entityType: string,
      entityUID: string,
      data: T
    ) => {
      if (options?.enabled === false) return

      await auditService.trackCrudOperation(
        'Delete',
        entityType,
        entityUID,
        data
      )
    },
    [options?.enabled]
  )

  const trackView = useCallback(
    async (pageName: string, metadata?: Record<string, any>) => {
      if (options?.enabled === false) return

      await auditService.trackPageView(pageName, metadata)
    },
    [options?.enabled]
  )

  const trackExport = useCallback(
    async (
      entityType: string,
      format: string,
      recordCount: number,
      filters?: any
    ) => {
      if (options?.enabled === false) return

      await auditService.trackExport(entityType, format, recordCount, filters)
    },
    [options?.enabled]
  )

  return {
    trackCreate,
    trackUpdate,
    trackDelete,
    trackView,
    trackExport
  }
}

/**
 * Hook to fetch audit trail for an entity
 */
export function useAuditTrail(
  linkedItemType: string,
  linkedItemUID: string,
  options?: {
    enabled?: boolean
    refetchInterval?: number
  }
) {
  return useQuery({
    queryKey: ['auditTrail', linkedItemType, linkedItemUID],
    queryFn: () => auditService.getAuditTrail(linkedItemType, linkedItemUID),
    enabled: options?.enabled !== false && !!linkedItemType && !!linkedItemUID,
    refetchInterval: options?.refetchInterval,
    staleTime: 30000 // 30 seconds
  })
}

/**
 * Hook for paginated audit trail with filtering
 */
export function useAuditTrailPaged(
  request: AuditTrailPagingRequest,
  options?: {
    enabled?: boolean
  }
) {
  return useQuery({
    queryKey: ['auditTrailPaged', request],
    queryFn: () => auditService.getAuditTrailPaged(request),
    enabled: options?.enabled !== false,
    staleTime: 30000
  })
}

/**
 * Hook to manually create audit entries
 */
export function useCreateAudit() {
  return useMutation({
    mutationFn: (entry: Partial<AuditTrailEntry>) => 
      auditService.createAudit(entry),
    onError: (error) => {
      console.error('Failed to create audit entry:', error)
    }
  })
}

/**
 * Hook to track component lifecycle for auditing
 */
export function useAuditedComponent(
  componentName: string,
  metadata?: Record<string, any>
) {
  useEffect(() => {
    // Track component mount
    auditService.trackPageView(componentName, {
      event: 'mount',
      ...metadata
    })

    // Track component unmount
    return () => {
      auditService.trackPageView(componentName, {
        event: 'unmount',
        ...metadata
      })
    }
  }, [componentName]) // Intentionally not including metadata to avoid re-runs
}

/**
 * Higher-order function to wrap API calls with audit tracking
 */
export function withAudit<TArgs extends any[], TReturn>(
  fn: (...args: TArgs) => Promise<TReturn>,
  auditConfig: {
    entityType: string
    getEntityUID: (...args: TArgs) => string
    commandType: AuditTrailEntry['commandType']
    getData?: (...args: TArgs) => Record<string, any>
  }
): (...args: TArgs) => Promise<TReturn> {
  return async (...args: TArgs) => {
    try {
      // Execute the original function
      const result = await fn(...args)

      // Track success
      await auditService.createAudit({
        linkedItemType: auditConfig.entityType,
        linkedItemUID: auditConfig.getEntityUID(...args),
        commandType: auditConfig.commandType,
        newData: auditConfig.getData ? auditConfig.getData(...args) : { args },
        hasChanges: true
      })

      return result
    } catch (error) {
      // Track failure
      await auditService.createAudit({
        linkedItemType: auditConfig.entityType,
        linkedItemUID: auditConfig.getEntityUID(...args),
        commandType: auditConfig.commandType,
        newData: {
          error: error instanceof Error ? error.message : 'Unknown error',
          args: auditConfig.getData ? auditConfig.getData(...args) : args
        },
        hasChanges: false
      })

      throw error
    }
  }
}