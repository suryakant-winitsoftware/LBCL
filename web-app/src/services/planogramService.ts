import axios from "axios";
import { authService } from "@/lib/auth-service";

const API_BASE_URL =
  process.env.NEXT_PUBLIC_API_URL || "http://localhost:8000/api";

export interface PlanogramSetup {
  UID?: string; // API returns uppercase UID
  uid?: string; // Keep lowercase for compatibility
  CategoryCode: string; // API returns Pascal case
  categoryCode?: string; // Keep camel case for compatibility
  ShareOfShelfCm?: number; // API returns Pascal case
  shareOfShelfCm?: number; // Keep camel case for compatibility
  SelectionType?: string; // API returns Pascal case
  selectionType?: string; // Keep camel case for compatibility
  SelectionValue?: string; // API returns Pascal case
  selectionValue?: string; // Keep camel case for compatibility
  CreatedBy?: string; // API returns Pascal case
  createdBy?: string; // Keep camel case for compatibility
  CreatedTime?: string; // API returns Pascal case
  createdTime?: string; // Keep camel case for compatibility
  ModifiedBy?: string; // API returns Pascal case
  modifiedBy?: string; // Keep camel case for compatibility
  ModifiedTime?: string; // API returns Pascal case
  modifiedTime?: string; // Keep camel case for compatibility
}

export interface PlanogramCategory {
  categoryCode: string;
  categoryName: string;
  setupCount: number;
  categoryImage?: string;
}

export interface PlanogramSearchRequest {
  searchText?: string;
  categoryCodes?: string[];
  minShelfCm?: number;
  maxShelfCm?: number;
  selectionType?: string;
  pageNumber?: number;
  pageSize?: number;
}

export interface PlanogramExecutionHeader {
  uid?: string;
  storeUID: string;
  beatHistoryUID?: string;
  storeHistoryUID?: string;
  executionDate?: string;
  totalCategories?: number;
  completedCategories?: number;
}

export interface PlanogramExecutionDetail {
  uid?: string;
  planogramExecutionHeaderUID: string;
  planogramSetupUID: string;
  isCompliant: boolean;
  compliancePercentage?: number;
  notes?: string;
  imagePath?: string;
}

class PlanogramService {
  // Get all planogram setups with pagination
  async getAllPlanogramSetups(pageNumber: number = 1, pageSize: number = 50) {
    try {
      const response = await axios.get(
        `${API_BASE_URL}/Planogram/GetAllPlanogramSetups`,
        {
          params: { pageNumber, pageSize },
          headers: authService.getAuthHeaders()
        }
      );
      return response.data;
    } catch (error) {
      console.error("Error fetching planogram setups:", error);
      throw error;
    }
  }

  // Get planogram setup by UID
  async getPlanogramSetupByUID(uid: string) {
    try {
      const response = await axios.get(
        `${API_BASE_URL}/Planogram/GetPlanogramSetupByUID/${uid}`,
        { headers: authService.getAuthHeaders() }
      );
      return response.data;
    } catch (error) {
      console.error("Error fetching planogram setup:", error);
      throw error;
    }
  }

  // Get planogram setups by category
  async getPlanogramSetupsByCategory(categoryCode: string) {
    try {
      const response = await axios.get(
        `${API_BASE_URL}/Planogram/GetPlanogramSetupsByCategory/${categoryCode}`,
        { headers: authService.getAuthHeaders() }
      );
      return response.data;
    } catch (error) {
      console.error("Error fetching planogram setups by category:", error);
      throw error;
    }
  }

  // Create new planogram setup
  async createPlanogramSetup(setup: PlanogramSetup) {
    try {
      console.log("=== PLANOGRAM SERVICE CREATE ===");
      console.log("API URL:", `${API_BASE_URL}/Planogram/CreatePlanogramSetup`);
      console.log("Setup data being sent:", setup);
      console.log("Auth headers:", authService.getAuthHeaders());

      const response = await axios.post(
        `${API_BASE_URL}/Planogram/CreatePlanogramSetup`,
        setup,
        { headers: authService.getAuthHeaders() }
      );

      console.log("Service response status:", response.status);
      console.log("Service response data:", response.data);

      return response.data;
    } catch (error: any) {
      console.error("Error creating planogram setup:", error);
      console.error("Error response:", error.response?.data);
      console.error("Error status:", error.response?.status);
      console.error("Error message:", error.message);

      // Return more detailed error information
      if (error.response?.data) {
        throw new Error(
          error.response.data.ErrorMessage ||
            error.response.data.Message ||
            "API Error"
        );
      }
      throw error;
    }
  }

  // Update planogram setup
  async updatePlanogramSetup(setup: PlanogramSetup) {
    try {
      const response = await axios.put(
        `${API_BASE_URL}/Planogram/UpdatePlanogramSetup`,
        setup,
        { headers: authService.getAuthHeaders() }
      );
      return response.data;
    } catch (error) {
      console.error("Error updating planogram setup:", error);
      throw error;
    }
  }

  // Delete planogram setup
  async deletePlanogramSetup(uid: string) {
    try {
      const response = await axios.delete(
        `${API_BASE_URL}/Planogram/DeletePlanogramSetup/${uid}`,
        { headers: authService.getAuthHeaders() }
      );
      return response.data;
    } catch (error) {
      console.error("Error deleting planogram setup:", error);
      throw error;
    }
  }

  // Bulk create planogram setups
  async bulkCreatePlanogramSetups(setups: PlanogramSetup[]) {
    try {
      const response = await axios.post(
        `${API_BASE_URL}/Planogram/BulkCreatePlanogramSetups`,
        setups,
        { headers: authService.getAuthHeaders() }
      );
      return response.data;
    } catch (error) {
      console.error("Error bulk creating planogram setups:", error);
      throw error;
    }
  }

  // Bulk delete planogram setups
  async bulkDeletePlanogramSetups(uids: string[]) {
    try {
      const response = await axios.delete(
        `${API_BASE_URL}/Planogram/BulkDeletePlanogramSetups`,
        {
          data: uids,
          headers: authService.getAuthHeaders()
        }
      );
      return response.data;
    } catch (error) {
      console.error("Error bulk deleting planogram setups:", error);
      throw error;
    }
  }

  // Search planogram setups
  async searchPlanogramSetups(searchRequest: PlanogramSearchRequest) {
    try {
      const response = await axios.post(
        `${API_BASE_URL}/Planogram/SearchPlanogramSetups`,
        searchRequest,
        { headers: authService.getAuthHeaders() }
      );
      return response.data;
    } catch (error) {
      console.error("Error searching planogram setups:", error);
      throw error;
    }
  }

  // Get planogram categories
  async getPlanogramCategories() {
    try {
      const response = await axios.get(
        `${API_BASE_URL}/Planogram/GetPlanogramCategories`,
        { headers: authService.getAuthHeaders() }
      );
      return response.data;
    } catch (error) {
      console.error("Error fetching planogram categories:", error);
      throw error;
    }
  }

  // Get planogram recommendations
  async getPlanogramRecommendations(categoryCode: string) {
    try {
      const response = await axios.get(
        `${API_BASE_URL}/Planogram/GetPlanogramRecommendations/${categoryCode}`,
        { headers: authService.getAuthHeaders() }
      );
      return response.data;
    } catch (error) {
      console.error("Error fetching planogram recommendations:", error);
      throw error;
    }
  }

  // Create planogram execution header
  async createPlanogramExecutionHeader(header: PlanogramExecutionHeader) {
    try {
      const response = await axios.post(
        `${API_BASE_URL}/Planogram/CreatePlanogramExecutionHeader`,
        header,
        { headers: authService.getAuthHeaders() }
      );
      return response.data;
    } catch (error) {
      console.error("Error creating planogram execution header:", error);
      throw error;
    }
  }

  // Create planogram execution detail
  async createPlanogramExecutionDetail(detail: PlanogramExecutionDetail) {
    try {
      const response = await axios.post(
        `${API_BASE_URL}/Planogram/CreatePlanogramExecutionDetail`,
        detail,
        { headers: authService.getAuthHeaders() }
      );
      return response.data;
    } catch (error) {
      console.error("Error creating planogram execution detail:", error);
      throw error;
    }
  }

  // Get planogram execution details
  async getPlanogramExecutionDetails(headerUID: string) {
    try {
      const response = await axios.get(
        `${API_BASE_URL}/Planogram/GetPlanogramExecutionDetails/${headerUID}`,
        { headers: authService.getAuthHeaders() }
      );
      return response.data;
    } catch (error) {
      console.error("Error fetching planogram execution details:", error);
      throw error;
    }
  }

  // Update planogram execution status
  async updatePlanogramExecutionStatus(uid: string, isCompleted: boolean) {
    try {
      const response = await axios.put(
        `${API_BASE_URL}/Planogram/UpdatePlanogramExecutionStatus/${uid}`,
        isCompleted,
        { headers: authService.getAuthHeaders() }
      );
      return response.data;
    } catch (error) {
      console.error("Error updating planogram execution status:", error);
      throw error;
    }
  }

  // Helper to generate UUID
  private generateUID(): string {
    return "xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx".replace(
      /[xy]/g,
      function (c) {
        const r = (Math.random() * 16) | 0;
        const v = c === "x" ? r : (r & 0x3) | 0x8;
        return v.toString(16);
      }
    );
  }

  // Convert file to base64
  async fileToBase64(file: File): Promise<string> {
    return new Promise((resolve, reject) => {
      console.log(
        "Converting file to base64:",
        file.name,
        file.size,
        file.type
      );
      const reader = new FileReader();
      reader.readAsDataURL(file);
      reader.onload = () => {
        const result = reader.result as string;
        console.log(
          "FileReader result type:",
          typeof result,
          "length:",
          result?.length
        );
        // Remove data URL prefix to get pure base64
        const base64 = result.split(",")[1];
        if (!base64) {
          console.error("Failed to extract base64 from result:", result);
          reject(new Error("Failed to extract base64 data from file"));
        } else {
          console.log("Base64 conversion successful, length:", base64.length);
          resolve(base64);
        }
      };
      reader.onerror = (error) => {
        console.error("FileReader error:", error);
        reject(error);
      };
    });
  }

  // Helper to convert base64 to File object
  private async base64ToFile(
    base64: string,
    fileName: string,
    mimeType: string
  ): Promise<File> {
    // Convert base64 to blob
    const byteCharacters = atob(base64);
    const byteNumbers = new Array(byteCharacters.length);
    for (let i = 0; i < byteCharacters.length; i++) {
      byteNumbers[i] = byteCharacters.charCodeAt(i);
    }
    const byteArray = new Uint8Array(byteNumbers);
    const blob = new Blob([byteArray], { type: mimeType });

    // Create File from blob
    return new File([blob], fileName, { type: mimeType });
  }

  // File operations - Direct file upload (no base64 conversion needed)
  async uploadPlanogramImageDirect(planogramUid: string, file: File) {
    try {
      console.log("=== PLANOGRAM UPLOAD DEBUG START ===");
      console.log("planogramUid received:", planogramUid);
      console.log("planogramUid type:", typeof planogramUid);
      console.log("planogramUid is null/undefined:", planogramUid == null);
      console.log("Step 1: Uploading file directly to server...");
      console.log("File details:", file.name, file.size, file.type);

      // Step 1: Upload the actual file using FileUpload endpoint
      const formData = new FormData();
      formData.append("files", file, file.name);
      formData.append("folderPath", `Data/planogram-images/${planogramUid}`);

      console.log(
        "Making request to:",
        `${API_BASE_URL}/FileUpload/UploadFile`
      );
      console.log(
        "FormData folder path:",
        `Data/planogram-images/${planogramUid}`
      );

      // Get auth token but don't set Content-Type
      const authToken = localStorage.getItem("auth_token");
      console.log("Auth token available:", !!authToken);
      console.log("Auth token length:", authToken?.length || 0);

      const headers: any = {};
      if (authToken) {
        headers["Authorization"] = `Bearer ${authToken}`;
      }
      console.log("Request headers:", headers);

      const uploadResponse = await fetch(
        `${API_BASE_URL}/FileUpload/UploadFile`,
        {
          method: "POST",
          headers: headers,
          body: formData
        }
      );

      console.log("Upload response status:", uploadResponse.status);
      console.log("Upload response ok:", uploadResponse.ok);
      console.log(
        "Upload response headers:",
        Object.fromEntries(uploadResponse.headers.entries())
      );

      if (!uploadResponse.ok) {
        const errorText = await uploadResponse.text();
        console.error("Upload error response:", errorText);
        console.error("Upload error status:", uploadResponse.status);
        console.error("Upload error statusText:", uploadResponse.statusText);
        throw new Error(
          `File upload failed! status: ${uploadResponse.status}, response: ${errorText}`
        );
      }

      const uploadResult = await uploadResponse.json();
      console.log("File upload result:", uploadResult);

      if (uploadResult.Status !== 1) {
        console.error(
          "Upload failed with status:",
          uploadResult.Status,
          "Message:",
          uploadResult.Message
        );
        throw new Error(uploadResult.Message || "File upload failed");
      }

      // Step 2: Create file_sys record with the uploaded file path
      const uniqueUID = this.generateUID();

      // Get the employee UID from localStorage
      let empUID: string | null = null;
      try {
        const userInfoStr = localStorage.getItem("user_info");
        if (userInfoStr) {
          const userInfo = JSON.parse(userInfoStr);
          empUID = userInfo.uid || userInfo.id || userInfo.UID;
        }
      } catch (e) {
        console.error("Failed to parse user_info from localStorage:", e);
      }

      if (!empUID) {
        throw new Error(
          "User not authenticated. Please login again to upload images."
        );
      }

      // Extract the relative path from the upload result
      const relativePath =
        uploadResult.SavedImgsPath && uploadResult.SavedImgsPath.length > 0
          ? uploadResult.SavedImgsPath[0]
          : `Data/planogram-images/${planogramUid}/${file.name}`;

      console.log("=== CREATING FILESYS RECORD DEBUG ===");
      console.log("planogramUid for LinkedItemUID:", planogramUid);

      const fileSysData: any = {
        // Required BaseModel fields
        UID: uniqueUID,
        SS: 1, // Status flag, 1 for active
        CreatedBy: empUID,
        CreatedTime: new Date().toISOString(),
        ModifiedBy: empUID,
        ModifiedTime: new Date().toISOString(),

        // FileSys specific fields
        LinkedItemType: "Planogram",
        LinkedItemUID: planogramUid,
        FileSysType: "Image",
        FileType: file.type,
        FileName: file.name,
        DisplayName: `Planogram - ${file.name}`,
        FileSize: file.size,
        IsDefault: false,
        IsDirectory: false,
        RelativePath: relativePath,
        FileSysFileType: 1, // 1 for Image
        CreatedByEmpUID: empUID
      };

      console.log("Step 2: Creating file_sys record with path:", relativePath);

      const response = await fetch(`${API_BASE_URL}/FileSys/CreateFileSys`, {
        method: "POST",
        headers: {
          ...authService.getAuthHeaders(),
          "Content-Type": "application/json",
          Accept: "application/json"
        },
        body: JSON.stringify(fileSysData)
      });

      if (!response.ok) {
        let errorMessage = `HTTP error! status: ${response.status}`;
        try {
          const errorData = await response.json();
          if (errorData.ErrorMessage) {
            errorMessage = errorData.ErrorMessage;
          }
        } catch (e) {
          // Ignore JSON parse errors
        }
        throw new Error(errorMessage);
      }

      const result = await response.json();
      console.log("File system record created successfully:", result);
      return result;
    } catch (error) {
      console.error("Error uploading planogram image:", error);
      throw error;
    }
  }

  async uploadPlanogramImage(planogramSetupUid: string, file: File) {
    try {
      // Step 1: Upload the actual file using FileUpload endpoint
      const formData = new FormData();
      formData.append("files", file, file.name);
      formData.append(
        "folderPath",
        `Data/planogram-images/${planogramSetupUid}`
      );

      console.log("Step 1: Uploading file to server...");

      // Get auth token but don't set Content-Type
      const authToken = localStorage.getItem("auth_token");
      const headers: any = {};
      if (authToken) {
        headers["Authorization"] = `Bearer ${authToken}`;
      }

      const uploadResponse = await fetch(
        `${API_BASE_URL}/FileUpload/UploadFile`,
        {
          method: "POST",
          headers: headers,
          body: formData
        }
      );

      if (!uploadResponse.ok) {
        throw new Error(`File upload failed! status: ${uploadResponse.status}`);
      }

      const uploadResult = await uploadResponse.json();
      console.log("File upload result:", uploadResult);

      if (uploadResult.Status !== 1) {
        throw new Error(uploadResult.Message || "File upload failed");
      }

      // Step 2: Create file_sys record with the uploaded file path
      const uniqueUID = this.generateUID();

      // Get the employee UID from localStorage
      let empUID: string | null = null;
      try {
        const userInfoStr = localStorage.getItem("user_info");
        if (userInfoStr) {
          const userInfo = JSON.parse(userInfoStr);
          empUID = userInfo.uid || userInfo.id || userInfo.UID;
        }
      } catch (e) {
        console.error("Failed to parse user_info from localStorage:", e);
      }

      if (!empUID) {
        throw new Error(
          "User not authenticated. Please login again to upload images."
        );
      }

      // Extract the relative path from the upload result
      const relativePath =
        uploadResult.SavedImgsPath && uploadResult.SavedImgsPath.length > 0
          ? uploadResult.SavedImgsPath[0]
          : `Data/planogram-images/${planogramSetupUid}/${file.name}`;

      const fileSysData: any = {
        // Required BaseModel fields
        UID: uniqueUID,
        SS: 1, // Status flag, 1 for active
        CreatedBy: empUID,
        CreatedTime: new Date().toISOString(),
        ModifiedBy: empUID,
        ModifiedTime: new Date().toISOString(),

        // FileSys specific fields
        LinkedItemType: "Planogram",
        LinkedItemUID: planogramSetupUid,
        FileSysType: "Image",
        FileType: file.type,
        FileName: file.name,
        DisplayName: file.name,
        FileSize: file.size,
        IsDefault: false,
        IsDirectory: false,
        RelativePath: relativePath,
        FileSysFileType: 1, // 1 for Image
        CreatedByEmpUID: empUID
      };

      console.log("Step 2: Creating file_sys record with path:", relativePath);

      const response = await fetch(`${API_BASE_URL}/FileSys/CreateFileSys`, {
        method: "POST",
        headers: {
          ...authService.getAuthHeaders(),
          "Content-Type": "application/json",
          Accept: "application/json"
        },
        body: JSON.stringify(fileSysData)
      });

      if (!response.ok) {
        let errorMessage = `HTTP error! status: ${response.status}`;
        try {
          const errorData = await response.json();
          if (errorData.ErrorMessage) {
            errorMessage = errorData.ErrorMessage;
          }
        } catch (e) {
          // Ignore JSON parse errors
        }
        throw new Error(errorMessage);
      }

      const result = await response.json();
      console.log("File system record created successfully:", result);
      return result;
    } catch (error) {
      console.error("Error uploading planogram image:", error);
      throw error;
    }
  }

  async getPlanogramImages(planogramSetupUid: string) {
    console.log(`=== GET PLANOGRAM IMAGES ===`);
    console.log(`Planogram UID: ${planogramSetupUid}`);

    try {
      const requestBody = {
        PageNumber: 1,
        PageSize: 1000,
        IsCountRequired: false,
        FilterCriterias: [
          { Name: "FileSysType", Value: "Image" },
          { Name: "LinkedItemUID", Value: planogramSetupUid }
        ],
        SortCriterias: [
          { SortParameter: "IsDefault", Direction: "Desc" },
          { SortParameter: "DisplayName", Direction: "Asc" }
        ]
      };

      const response = await fetch(
        `${API_BASE_URL}/FileSys/SelectAllFileSysDetails`,
        {
          method: "POST",
          headers: {
            ...authService.getAuthHeaders(),
            "Content-Type": "application/json",
            Accept: "application/json"
          },
          body: JSON.stringify(requestBody)
        }
      );

      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }

      const result = await response.json();
      console.log(`API Response:`, result);
      console.log(`Response IsSuccess:`, result.IsSuccess);
      console.log(`Response Data length:`, result.Data?.PagedData?.length || 0);

      if (result.IsSuccess && result.Data?.PagedData) {
        console.log(`Raw images from API:`, result.Data.PagedData.slice(0, 3)); // Show first 3

        // Debug: Show all unique LinkedItemType values in the response
        const uniqueLinkedItemTypes = [
          ...new Set(
            result.Data.PagedData.map((img: any) => img.LinkedItemType)
          )
        ];
        console.log(`All LinkedItemType values found:`, uniqueLinkedItemTypes);

        // Debug: Show LinkedItemType and LinkedItemUID for first few images
        console.log(
          `First 5 images LinkedItemType details:`,
          result.Data.PagedData.slice(0, 5).map((img: any) => ({
            DisplayName: img.DisplayName,
            LinkedItemType: img.LinkedItemType,
            LinkedItemUID: img.LinkedItemUID,
            FileSysType: img.FileSysType
          }))
        );

        // Debug: Show all unique LinkedItemUIDs to see what's available
        const uniqueLinkedItemUIDs = [
          ...new Set(result.Data.PagedData.map((img: any) => img.LinkedItemUID))
        ];
        console.log(`All LinkedItemUID values found:`, uniqueLinkedItemUIDs);
        console.log(`Looking for LinkedItemUID:`, planogramSetupUid);

        // Filter client-side by LinkedItemUID only (since LinkedItemType varies in database)
        const filteredImages = result.Data.PagedData.filter(
          (img: any) => img.LinkedItemUID === planogramSetupUid
        );

        console.log(`Filtered images count:`, filteredImages.length);
        console.log(
          `Filtered images:`,
          filteredImages.map((img) => ({
            DisplayName: img.DisplayName,
            LinkedItemType: img.LinkedItemType,
            LinkedItemUID: img.LinkedItemUID
          }))
        );

        return { IsSuccess: true, Data: filteredImages };
      }
      return { IsSuccess: true, Data: [] };
    } catch (error) {
      console.error("Error getting planogram images:", error);
      throw error;
    }
  }

  async deletePlanogramImage(fileUid: string) {
    try {
      const response = await fetch(
        `${API_BASE_URL}/FileSys/DeleteFileSys?UID=${fileUid}`,
        {
          method: "DELETE",
          headers: {
            ...authService.getAuthHeaders(),
            "Content-Type": "application/json",
            Accept: "application/json"
          }
        }
      );

      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }

      const result = await response.json();
      return result;
    } catch (error) {
      console.error("Error deleting planogram image:", error);
      throw error;
    }
  }

  // Get file data as blob for display
  async getImageBlob(fileSys: any): Promise<string> {
    try {
      if (fileSys.RelativePath) {
        // If we have a relative path, construct the full URL to the image
        const baseUrl = API_BASE_URL || "http://localhost:8000/api";
        // Remove 'api' from the URL if present since static files are served from root
        const staticUrl = baseUrl.replace("/api", "");

        // Convert the relative path: Data/planogram-images/... -> /data/planogram-images/...
        let imagePath = fileSys.RelativePath;
        if (imagePath.startsWith("Data/")) {
          imagePath = imagePath.replace("Data/", "/data/");
        } else if (!imagePath.startsWith("/data/")) {
          imagePath = `/data/${imagePath}`;
        }

        return `${staticUrl}${imagePath}`;
      } else if (fileSys.TempPath) {
        // Handle temp path similarly
        const baseUrl = API_BASE_URL || "http://localhost:8000/api";
        const staticUrl = baseUrl.replace("/api", "");

        let imagePath = fileSys.TempPath;
        if (imagePath.startsWith("Data/")) {
          imagePath = imagePath.replace("Data/", "/data/");
        } else if (!imagePath.startsWith("/data/")) {
          imagePath = `/data/${imagePath}`;
        }

        return `${staticUrl}${imagePath}`;
      }

      // Return placeholder image
      return "/placeholder-product.png";
    } catch (error) {
      console.error("Error creating image URL:", error);
      return "/placeholder-product.png";
    }
  }

  getImageUrl(relativePath: string) {
    return `${API_BASE_URL.replace("/api", "")}/${relativePath}`;
  }
}

const planogramService = new PlanogramService();
export default planogramService;
