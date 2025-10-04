import { authService } from "@/lib/auth-service";

// API Debug utility to help identify available endpoints
export class ApiDebug {
  private static baseUrl =
    process.env.NEXT_PUBLIC_API_URL || "https://multiplex-promotions-api.winitsoftware.com/api";

  static async testEndpoint(
    endpoint: string,
    method: "GET" | "POST" = "POST",
    body?: any
  ) {
    try {
      const url = `${this.baseUrl}${
        endpoint.startsWith("/") ? "" : "/"
      }${endpoint}`;
      console.log(`🔍 Testing: ${method} ${url}`);

      const token = authService.getToken();
      if (!token) {
        console.log(
          "❌ No authentication token available. Please login first."
        );
        return {
          success: false,
          error: "No authentication token",
          status: 401,
        };
      }

      const config: RequestInit = {
        method,
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${token}`,
        },
      };

      if (body && method !== "GET") {
        config.body = JSON.stringify(body);
      }

      const response = await fetch(url, config);

      console.log(`📊 Response: ${response.status} ${response.statusText}`);

      if (response.ok) {
        try {
          const data = await response.json();
          console.log("✅ Success:", data);
          return { success: true, data };
        } catch (e) {
          console.log("✅ Success (no JSON)");
          return { success: true, data: null };
        }
      } else {
        try {
          const errorData = await response.text();
          console.log("❌ Error:", errorData);
          return { success: false, error: errorData, status: response.status };
        } catch (e) {
          console.log("❌ Error (no body)");
          return {
            success: false,
            error: "No error body",
            status: response.status,
          };
        }
      }
    } catch (error) {
      console.log("🚨 Network Error:", error);
      return {
        success: false,
        error: error instanceof Error ? error.message : "Unknown error",
        status: 0,
      };
    }
  }

  static async testCommonEndpoints() {
    console.log("🧪 Testing common API endpoints...");

    const testCases = [
      // Organization endpoints (working)
      {
        endpoint: "/Org/GetOrgDetails",
        body: {
          pageNumber: 0,
          pageSize: 10,
          isCountRequired: true,
          sortCriterias: [],
          filterCriterias: [],
        },
      },

      // Try different BeatHistory endpoint variations
      {
        endpoint: "/BeatHistory/SelectAllBeatHistory",
        body: {
          pageNumber: 0,
          pageSize: 10,
          isCountRequired: true,
          sortCriterias: [],
          filterCriterias: [],
        },
      },
      {
        endpoint: "/BeatHistory/GetBeatHistory",
        body: {
          pageNumber: 0,
          pageSize: 10,
          isCountRequired: true,
          sortCriterias: [],
          filterCriterias: [],
        },
      },
      {
        endpoint: "/BeatHistory/SelectAll",
        body: {
          pageNumber: 0,
          pageSize: 10,
          isCountRequired: true,
          sortCriterias: [],
          filterCriterias: [],
        },
      },
      {
        endpoint: "/BeatHistory/GetAllBeatHistory",
        body: {
          pageNumber: 0,
          pageSize: 10,
          isCountRequired: true,
          sortCriterias: [],
          filterCriterias: [],
        },
      },

      // Try UserJourney variations
      {
        endpoint: "/UserJourney/GetUserJourneyGridDetails",
        body: {
          pageNumber: 0,
          pageSize: 10,
          isCountRequired: true,
          sortCriterias: [],
          filterCriterias: [],
        },
      },
      {
        endpoint: "/UserJourney/SelectAll",
        body: {
          pageNumber: 0,
          pageSize: 10,
          isCountRequired: true,
          sortCriterias: [],
          filterCriterias: [],
        },
      },
      {
        endpoint: "/UserJourney/GetAll",
        body: {
          pageNumber: 0,
          pageSize: 10,
          isCountRequired: true,
          sortCriterias: [],
          filterCriterias: [],
        },
      },

      // Try JourneyPlan variations
      {
        endpoint: "/JourneyPlan/SelectAll",
        body: {
          pageNumber: 0,
          pageSize: 10,
          isCountRequired: true,
          sortCriterias: [],
          filterCriterias: [],
        },
      },
      {
        endpoint: "/JourneyPlan/GetAll",
        body: {
          pageNumber: 0,
          pageSize: 10,
          isCountRequired: true,
          sortCriterias: [],
          filterCriterias: [],
        },
      },
      {
        endpoint: "/JourneyPlan/GetJourneyPlans",
        body: {
          pageNumber: 0,
          pageSize: 10,
          isCountRequired: true,
          sortCriterias: [],
          filterCriterias: [],
        },
      },

      // Try Route-based journey endpoints
      {
        endpoint: "/Route/GetRouteJourneys",
        body: {
          pageNumber: 0,
          pageSize: 10,
          isCountRequired: true,
          sortCriterias: [],
          filterCriterias: [],
        },
      },
      {
        endpoint: "/Route/SelectAllRouteDetails",
        body: {
          pageNumber: 0,
          pageSize: 10,
          isCountRequired: true,
          sortCriterias: [],
          filterCriterias: [],
        },
      },

      // Dropdown endpoints
      { endpoint: "/Dropdown/GetRouteDropDown?orgUID=Farmley", body: {} },
      {
        endpoint:
          "/Dropdown/GetEmpDropDown?orgUID=Farmley&getDataByLoginId=false",
        body: {},
      },
      {
        endpoint: "/Dropdown/GetCustomerDropDown?franchiseeOrgUID=Farmley",
        body: {},
      },

      // Store endpoint
      {
        endpoint: "/Store/SelectAllStore",
        body: {
          pageNumber: 0,
          pageSize: 10,
          isCountRequired: true,
          sortCriterias: [],
          filterCriterias: [],
        },
      },

      // Try Employee-based endpoints
      {
        endpoint: "/Employee/GetEmployeeJourneys",
        body: {
          pageNumber: 0,
          pageSize: 10,
          isCountRequired: true,
          sortCriterias: [],
          filterCriterias: [],
        },
      },
      {
        endpoint: "/Employee/SelectAllEmployeeDetails",
        body: {
          pageNumber: 0,
          pageSize: 10,
          isCountRequired: true,
          sortCriterias: [],
          filterCriterias: [],
        },
      },
    ];

    for (const testCase of testCases) {
      await this.testEndpoint(testCase.endpoint, "POST", testCase.body);
      await new Promise((resolve) => setTimeout(resolve, 100)); // Small delay between requests
    }
  }

  static logToConsole() {
    if (typeof window !== "undefined") {
      // API Debug utility loaded - available as window.ApiDebug
      (window as any).ApiDebug = this;
    }
  }
}

// Auto-load in development
if (typeof window !== "undefined" && process.env.NODE_ENV === "development") {
  ApiDebug.logToConsole();
}
