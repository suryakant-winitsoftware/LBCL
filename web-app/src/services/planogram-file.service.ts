import { apiService } from "./api";

// Types matching FileSys API
export interface PlanogramFile {
  UID: string;
  LinkedItemType: string;
  LinkedItemUID: string;
  FileSysType: string;
  FileType: string;
  FileName: string;
  DisplayName: string;
  FileSize: number;
  RelativePath: string;
  CreatedByEmpUID: string;
  CreatedBy: string;
  CreatedTime: string;
  ModifiedTime: string;
  IsDefault: boolean;
}

export interface FileSysResponse {
  Data: {
    PagedData: PlanogramFile[];
    TotalCount: number;
  };
  StatusCode: number;
  IsSuccess: boolean;
}

class PlanogramFileService {
  private baseUrl = "/FileSys";

  // Get all files for a planogram
  async getPlanogramFiles(planogramUid: string): Promise<PlanogramFile[]> {
    try {
      console.log("Getting files for planogram:", planogramUid);

      const response = await apiService.post(
        `${this.baseUrl}/SelectAllFileSysDetails`,
        {
          PageNumber: 1,
          PageSize: 100,
          IsCountRequired: true,
          FilterCriterias: [
            { Name: "LinkedItemType", Value: "Planogram" },
            { Name: "LinkedItemUID", Value: planogramUid },
          ],
          SortCriterias: [],
        }
      );

      if (response.IsSuccess && response.Data?.PagedData) {
        console.log(
          `Found ${response.Data.PagedData.length} files for planogram ${planogramUid}`
        );
        return response.Data.PagedData;
      }

      return [];
    } catch (error) {
      console.error("Error fetching planogram files:", error);
      return [];
    }
  }

  // Get only image files for a planogram
  async getPlanogramImages(planogramUid: string): Promise<PlanogramFile[]> {
    try {
      console.log("Getting images for planogram:", planogramUid);

      const response = await apiService.post(
        `${this.baseUrl}/SelectAllFileSysDetails`,
        {
          PageNumber: 1,
          PageSize: 100,
          IsCountRequired: true,
          FilterCriterias: [
            { Name: "LinkedItemType", Value: "Planogram" },
            { Name: "LinkedItemUID", Value: planogramUid },
            { Name: "FileType", Value: "image/", Type: 10 }, // Type 10 = Contains
          ],
          SortCriterias: [],
        }
      );

      if (response.IsSuccess && response.Data?.PagedData) {
        console.log(
          `Found ${response.Data.PagedData.length} images for planogram ${planogramUid}`
        );
        return response.Data.PagedData;
      }

      return [];
    } catch (error) {
      console.error("Error fetching planogram images:", error);
      return [];
    }
  }

  // Get the URL for displaying a file
  getFileUrl(relativePath: string): string {
    if (!relativePath) {
      return "/placeholder-product.svg";
    }

    try {
      // Get base URL from environment or default
      const baseUrl =
        process.env.NEXT_PUBLIC_API_URL || "https://multiplex-promotions-api.winitsoftware.com/api";

      // Remove /api suffix to get the static file server base URL
      const staticUrl = baseUrl.replace("/api", "");

      // If path starts with Data/, serve it directly from backend
      if (relativePath.startsWith("Data/")) {
        // Direct URL to backend static files
        return `http://localhost:8000/${relativePath}`;
      }

      // Otherwise, construct the URL normally
      return `${staticUrl}/${relativePath}`;
    } catch (error) {
      console.error("Error generating file URL:", error);
      return "/placeholder-product.svg";
    }
  }

  // Helper function to format file size
  formatFileSize(bytes: number): string {
    if (bytes === 0) return "0 Bytes";
    const k = 1024;
    const sizes = ["Bytes", "KB", "MB", "GB"];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + " " + sizes[i];
  }

  // Check if file is an image
  isImage(fileType: string): boolean {
    return fileType && fileType.startsWith("image/");
  }

  // Delete a file
  async deleteFile(fileUid: string): Promise<boolean> {
    try {
      const response = await apiService.delete(
        `${this.baseUrl}/DeleteFileSys?UID=${fileUid}`
      );
      return response.IsSuccess;
    } catch (error) {
      console.error("Error deleting file:", error);
      return false;
    }
  }
}

export const planogramFileService = new PlanogramFileService();
