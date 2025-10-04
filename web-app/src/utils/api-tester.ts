// API Testing Utility for Deep Investigation
const API_BASE_URL = "https://multiplex-promotions-api.winitsoftware.com/api";

const AUTH_TOKEN =
  "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiQURNSU4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3VzZXJkYXRhIjoiYWRtaW4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBZG1pbiIsInBlcm1pc3Npb25zIjpbInJlYWQiLCJ3cml0ZSJdLCJleHAiOjE3NTQ1NjM1ODEsImlzcyI6Im15aXNzdWVyIn0.QXc2NiYt64mRNNYJytLrWkTiX20KgFdSDjl2kLsuR1A";

interface ApiTestResult {
  endpoint: string;
  method: string;
  status: number;
  success: boolean;
  data?: any;
  error?: string;
  responseTime: number;
}

class ApiTester {
  private getHeaders() {
    return {
      Authorization: `Bearer ${AUTH_TOKEN}`,
      "Content-Type": "application/json",
      Accept: "application/json",
    };
  }

  private async testEndpoint(
    endpoint: string,
    method: "GET" | "POST" | "PUT" | "DELETE",
    body?: any
  ): Promise<ApiTestResult> {
    const startTime = Date.now();

    try {
      const response = await fetch(`${API_BASE_URL}${endpoint}`, {
        method,
        headers: this.getHeaders(),
        body: body ? JSON.stringify(body) : undefined,
      });

      const responseTime = Date.now() - startTime;
      const data = await response.json();

      return {
        endpoint,
        method,
        status: response.status,
        success: response.ok,
        data,
        responseTime,
      };
    } catch (error) {
      const responseTime = Date.now() - startTime;
      return {
        endpoint,
        method,
        status: 0,
        success: false,
        error: error instanceof Error ? error.message : "Unknown error",
        responseTime,
      };
    }
  }

  // Standard paging request for POST endpoints
  private getDefaultPagingRequest() {
    return {
      PageNumber: 1,
      PageSize: 10,
      IsCountRequired: true,
      FilterCriterias: [],
      SortCriterias: [],
    };
  }

  async testAllSKUGroupTypeAPIs(): Promise<ApiTestResult[]> {
    console.log("🧪 Testing SKU Group Type APIs...");

    const results: ApiTestResult[] = [];

    // 1. Get all SKU Group Types
    results.push(
      await this.testEndpoint(
        "/SKUGroupType/SelectAllSKUGroupTypeDetails",
        "POST",
        this.getDefaultPagingRequest()
      )
    );

    // 2. Get SKU Group Type View
    results.push(
      await this.testEndpoint("/SKUGroupType/SelectSKUGroupTypeView", "GET")
    );

    // 3. Get SKU Attribute DDL
    results.push(
      await this.testEndpoint("/SKUGroupType/SelectSKUAttributeDDL", "GET")
    );

    return results;
  }

  async testAllSKUGroupAPIs(): Promise<ApiTestResult[]> {
    console.log("🧪 Testing SKU Group APIs...");

    const results: ApiTestResult[] = [];

    // 1. Get all SKU Groups
    results.push(
      await this.testEndpoint(
        "/SKUGroup/SelectAllSKUGroupDetails",
        "POST",
        this.getDefaultPagingRequest()
      )
    );

    // 2. Get SKU Group View
    results.push(
      await this.testEndpoint("/SKUGroup/SelectSKUGroupView", "GET")
    );

    // 3. Get SKU Group Item Views
    results.push(
      await this.testEndpoint(
        "/SKUGroup/SelectAllSKUGroupItemViews",
        "POST",
        this.getDefaultPagingRequest()
      )
    );

    return results;
  }

  async testAllSKUToGroupMappingAPIs(): Promise<ApiTestResult[]> {
    console.log("🧪 Testing SKU to Group Mapping APIs...");

    const results: ApiTestResult[] = [];

    // 1. Get all SKU to Group Mappings
    results.push(
      await this.testEndpoint(
        "/SKUToGroupMapping/SelectAllSKUToGroupMappingDetails",
        "POST",
        this.getDefaultPagingRequest()
      )
    );

    return results;
  }

  async testSKUAPIs(): Promise<ApiTestResult[]> {
    console.log("🧪 Testing SKU APIs...");

    const results: ApiTestResult[] = [];

    // 1. Get all SKUs
    results.push(
      await this.testEndpoint(
        "/SKU/SelectAllSKUDetailsWebView",
        "POST",
        this.getDefaultPagingRequest()
      )
    );

    return results;
  }

  async runFullInvestigation(): Promise<void> {
    console.log("🚀 Starting Deep API Investigation...");
    console.log("=".repeat(60));

    const allResults: ApiTestResult[] = [];

    // Test all API groups
    allResults.push(...(await this.testAllSKUGroupTypeAPIs()));
    allResults.push(...(await this.testAllSKUGroupAPIs()));
    allResults.push(...(await this.testAllSKUToGroupMappingAPIs()));
    allResults.push(...(await this.testSKUAPIs()));

    // Analyze results
    this.analyzeResults(allResults);

    // Extract data samples
    this.extractDataSamples(allResults);
  }

  private analyzeResults(results: ApiTestResult[]): void {
    console.log("\n📊 API Test Results Summary");
    console.log("=".repeat(60));

    const successful = results.filter((r) => r.success);
    const failed = results.filter((r) => !r.success);

    console.log(`✅ Successful: ${successful.length}`);
    console.log(`❌ Failed: ${failed.length}`);
    console.log(
      `📈 Total Response Time: ${results.reduce(
        (sum, r) => sum + r.responseTime,
        0
      )}ms`
    );

    console.log("\n🔍 Detailed Results:");
    results.forEach((result) => {
      const status = result.success ? "✅" : "❌";
      const timing = `${result.responseTime}ms`;
      console.log(
        `${status} ${result.method} ${result.endpoint} (${result.status}) - ${timing}`
      );

      if (result.error) {
        console.log(`   Error: ${result.error}`);
      }

      if (result.data && result.success) {
        console.log(`   Data: ${this.summarizeData(result.data)}`);
      }
    });
  }

  private summarizeData(data: any): string {
    if (!data) return "No data";
    if (data.Data) {
      if (Array.isArray(data.Data.PagedData)) {
        return `${data.Data.PagedData.length} items (Total: ${data.Data.TotalCount})`;
      }
      if (Array.isArray(data.Data)) {
        return `${data.Data.length} items`;
      }
      return "Single item";
    }
    return "Raw data";
  }

  private extractDataSamples(results: ApiTestResult[]): void {
    console.log("\n📋 Data Structure Analysis:");
    console.log("=".repeat(60));

    results.forEach((result) => {
      if (result.success && result.data && result.data.Data) {
        console.log(`\n🔍 ${result.endpoint}:`);
        if (
          result.data.Data.PagedData &&
          result.data.Data.PagedData.length > 0
        ) {
          console.log(
            "Sample item:",
            JSON.stringify(result.data.Data.PagedData[0], null, 2)
          );
        } else if (
          Array.isArray(result.data.Data) &&
          result.data.Data.length > 0
        ) {
          console.log(
            "Sample item:",
            JSON.stringify(result.data.Data[0], null, 2)
          );
        } else if (typeof result.data.Data === "object") {
          console.log(
            "Data structure:",
            JSON.stringify(result.data.Data, null, 2)
          );
        }
      }
    });
  }
}

export default ApiTester;
