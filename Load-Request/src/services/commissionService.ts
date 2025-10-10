/**
 * Commission Service
 * Production-ready service for commission management and calculations
 */

import { authService } from "@/lib/auth-service";
import { auditService } from "@/lib/audit-service";
import type {
  Commission,
  CommissionKPI,
  CommissionKPISlab,
  CommissionUserMapping,
  CommissionUserKPIPerformance,
  CommissionUserPayout,
  CommissionCalculationRequest,
  CommissionCalculationResponse,
  CommissionAnalytics,
  CommissionReportFilters,
  CommissionDashboard,
  CommissionApiResponse,
  CommissionPagingRequest,
  CommissionPagingResponse
} from "@/types/commission.types";

class CommissionService {
  private readonly API_BASE_URL =
    process.env.NEXT_PUBLIC_API_URL || "http://localhost:8000/api";

  private async makeRequest<T>(
    endpoint: string,
    options: RequestInit = {}
  ): Promise<CommissionApiResponse<T>> {
    const token = authService.getToken();
    if (!token) {
      throw new Error("No authentication token available");
    }

    const url = `${this.API_BASE_URL}${endpoint}`;
    const response = await fetch(url, {
      ...options,
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${token}`,
        ...options.headers
      }
    });

    if (!response.ok) {
      throw new Error(
        `Commission API error: ${response.status} ${response.statusText}`
      );
    }

    return response.json();
  }

  // Commission Configuration Management

  /**
   * Get all commissions with optional filtering
   */
  async getCommissions(
    filters?: CommissionReportFilters
  ): Promise<Commission[]> {
    const result = await this.makeRequest<Commission[]>("/Commission", {
      method: "POST",
      body: JSON.stringify(filters || {})
    });
    return result.data;
  }

  /**
   * Get commission by ID
   */
  async getCommissionById(commissionId: string): Promise<Commission> {
    const result = await this.makeRequest<Commission>(
      `/Commission/${commissionId}`
    );
    return result.data;
  }

  /**
   * Create new commission
   */
  async createCommission(
    commission: Omit<Commission, "commission_id">
  ): Promise<Commission> {
    const result = await this.makeRequest<Commission>("/Commission", {
      method: "POST",
      body: JSON.stringify(commission)
    });

    // Track audit
    await auditService.createAudit({
      linkedItemType: "Commission",
      linkedItemUID: result.data.commission_id,
      commandType: "Insert",
      newData: result.data
    });

    return result.data;
  }

  /**
   * Update commission
   */
  async updateCommission(commission: Commission): Promise<Commission> {
    const result = await this.makeRequest<Commission>(
      `/Commission/${commission.commission_id}`,
      {
        method: "PUT",
        body: JSON.stringify(commission)
      }
    );

    // Track audit
    await auditService.createAudit({
      linkedItemType: "Commission",
      linkedItemUID: commission.commission_id,
      commandType: "Update",
      newData: result.data
    });

    return result.data;
  }

  /**
   * Delete commission
   */
  async deleteCommission(commissionId: string): Promise<boolean> {
    await this.makeRequest(`/Commission/${commissionId}`, {
      method: "DELETE"
    });

    // Track audit
    await auditService.createAudit({
      linkedItemType: "Commission",
      linkedItemUID: commissionId,
      commandType: "Delete",
      newData: { commission_id: commissionId, deleted: true }
    });

    return true;
  }

  // Commission KPI Management

  /**
   * Get KPIs for a commission
   */
  async getCommissionKPIs(commissionId: string): Promise<CommissionKPI[]> {
    const result = await this.makeRequest<CommissionKPI[]>(
      `/Commission/${commissionId}/KPIs`
    );
    return result.data;
  }

  /**
   * Create commission KPI
   */
  async createCommissionKPI(
    kpi: Omit<CommissionKPI, "commission_kpi_id">
  ): Promise<CommissionKPI> {
    const result = await this.makeRequest<CommissionKPI>("/Commission/KPI", {
      method: "POST",
      body: JSON.stringify(kpi)
    });

    await auditService.createAudit({
      linkedItemType: "CommissionKPI",
      linkedItemUID: result.data.commission_kpi_id,
      commandType: "Insert",
      newData: result.data
    });

    return result.data;
  }

  /**
   * Update commission KPI
   */
  async updateCommissionKPI(kpi: CommissionKPI): Promise<CommissionKPI> {
    const result = await this.makeRequest<CommissionKPI>(
      `/Commission/KPI/${kpi.commission_kpi_id}`,
      {
        method: "PUT",
        body: JSON.stringify(kpi)
      }
    );

    await auditService.createAudit({
      linkedItemType: "CommissionKPI",
      linkedItemUID: kpi.commission_kpi_id,
      commandType: "Update",
      newData: result.data
    });

    return result.data;
  }

  // Commission Calculation

  /**
   * Process commission calculation
   */
  async processCommission(
    request: CommissionCalculationRequest
  ): Promise<CommissionCalculationResponse> {
    const result = await this.makeRequest<CommissionCalculationResponse>(
      "/Commission/ProcessCommission",
      {
        method: "POST",
        body: JSON.stringify(request)
      }
    );

    // Track audit for commission calculation
    await auditService.createAudit({
      linkedItemType: "CommissionCalculation",
      linkedItemUID: request.commission_id || "ALL",
      commandType: "Update",
      newData: {
        request,
        result: result.data
      }
    });

    return result.data;
  }

  /**
   * Get commission calculation status
   */
  async getCalculationStatus(
    calculationId: string
  ): Promise<{ status: string; progress: number; message: string }> {
    const result = await this.makeRequest<{
      status: string;
      progress: number;
      message: string;
    }>(`/Commission/CalculationStatus/${calculationId}`);
    return result.data;
  }

  // Commission Payouts

  /**
   * Get user commission payouts
   */
  async getUserPayouts(
    request: CommissionPagingRequest
  ): Promise<CommissionPagingResponse<CommissionUserPayout>> {
    const result = await this.makeRequest<
      CommissionPagingResponse<CommissionUserPayout>
    >("/Commission/UserPayouts", {
      method: "POST",
      body: JSON.stringify(request)
    });
    return result.data;
  }

  /**
   * Approve commission payout
   */
  async approvePayout(
    payoutId: string,
    approvalNotes?: string
  ): Promise<CommissionUserPayout> {
    const result = await this.makeRequest<CommissionUserPayout>(
      `/Commission/Payout/${payoutId}/Approve`,
      {
        method: "PUT",
        body: JSON.stringify({ approval_notes: approvalNotes })
      }
    );

    await auditService.createAudit({
      linkedItemType: "CommissionPayout",
      linkedItemUID: payoutId,
      commandType: "Update",
      newData: { status: "Approved", approval_notes: approvalNotes }
    });

    return result.data;
  }

  /**
   * Process payout payment
   */
  async processPayout(
    payoutId: string,
    paymentDetails?: Record<string, unknown>
  ): Promise<CommissionUserPayout> {
    const result = await this.makeRequest<CommissionUserPayout>(
      `/Commission/Payout/${payoutId}/Pay`,
      {
        method: "PUT",
        body: JSON.stringify(paymentDetails || {})
      }
    );

    await auditService.createAudit({
      linkedItemType: "CommissionPayout",
      linkedItemUID: payoutId,
      commandType: "Update",
      newData: { status: "Paid", payment_details: paymentDetails }
    });

    return result.data;
  }

  // Commission Analytics and Reporting

  /**
   * Get commission dashboard data
   */
  async getDashboardData(orgCode?: string): Promise<CommissionDashboard> {
    const result = await this.makeRequest<CommissionDashboard>(
      `/Commission/Dashboard${orgCode ? `?org_code=${orgCode}` : ""}`
    );
    return result.data;
  }

  /**
   * Get commission analytics
   */
  async getAnalytics(
    filters: CommissionReportFilters
  ): Promise<CommissionAnalytics> {
    const result = await this.makeRequest<CommissionAnalytics>(
      "/Commission/Analytics",
      {
        method: "POST",
        body: JSON.stringify(filters)
      }
    );
    return result.data;
  }

  /**
   * Export commission report
   */
  async exportReport(
    filters: CommissionReportFilters,
    format: "csv" | "excel" = "excel"
  ): Promise<Blob> {
    const token = authService.getToken();
    if (!token) {
      throw new Error("No authentication token available");
    }

    const response = await fetch(
      `${this.API_BASE_URL}/Commission/Export?format=${format}`,
      {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${token}`
        },
        body: JSON.stringify(filters)
      }
    );

    if (!response.ok) {
      throw new Error(
        `Export failed: ${response.status} ${response.statusText}`
      );
    }

    // Track export audit
    await auditService.createAudit({
      linkedItemType: "CommissionReport",
      linkedItemUID: `Export_${Date.now()}`,
      commandType: "Export",
      newData: { filters, format, exported_at: new Date() }
    });

    return response.blob();
  }

  // Commission User Performance

  /**
   * Get user KPI performance
   */
  async getUserKPIPerformance(
    userId: string,
    commissionId?: string
  ): Promise<CommissionUserKPIPerformance[]> {
    const url = `/Commission/UserPerformance/${userId}${
      commissionId ? `?commission_id=${commissionId}` : ""
    }`;
    const result = await this.makeRequest<CommissionUserKPIPerformance[]>(url);
    return result.data;
  }

  /**
   * Get commission mappings for user
   */
  async getUserCommissionMappings(
    userId: string
  ): Promise<CommissionUserMapping[]> {
    const result = await this.makeRequest<CommissionUserMapping[]>(
      `/Commission/UserMappings/${userId}`
    );
    return result.data;
  }

  // Commission Slabs Management

  /**
   * Get commission slabs for KPI
   */
  async getKPISlabs(kpiId: string): Promise<CommissionKPISlab[]> {
    const result = await this.makeRequest<CommissionKPISlab[]>(
      `/Commission/KPI/${kpiId}/Slabs`
    );
    return result.data;
  }

  /**
   * Create commission slab
   */
  async createKPISlab(
    slab: Omit<CommissionKPISlab, "commission_kpi_slab_id">
  ): Promise<CommissionKPISlab> {
    const result = await this.makeRequest<CommissionKPISlab>(
      "/Commission/KPI/Slab",
      {
        method: "POST",
        body: JSON.stringify(slab)
      }
    );

    await auditService.createAudit({
      linkedItemType: "CommissionKPISlab",
      linkedItemUID: result.data.commission_kpi_slab_id,
      commandType: "Insert",
      newData: result.data
    });

    return result.data;
  }

  // Utility Methods

  /**
   * Validate commission configuration
   */
  async validateCommissionConfig(
    commission: Partial<Commission>
  ): Promise<{ valid: boolean; errors: string[] }> {
    const result = await this.makeRequest<{ valid: boolean; errors: string[] }>(
      "/Commission/Validate",
      {
        method: "POST",
        body: JSON.stringify(commission)
      }
    );
    return result.data;
  }

  /**
   * Get commission calculation preview
   */
  async getCalculationPreview(request: CommissionCalculationRequest): Promise<{
    estimated_users: number;
    estimated_payout: number;
    preview_data: CommissionUserPayout[];
  }> {
    const result = await this.makeRequest<{
      estimated_users: number;
      estimated_payout: number;
      preview_data: CommissionUserPayout[];
    }>("/Commission/CalculationPreview", {
      method: "POST",
      body: JSON.stringify(request)
    });
    return result.data;
  }
}

// Export singleton instance
export const commissionService = new CommissionService();
