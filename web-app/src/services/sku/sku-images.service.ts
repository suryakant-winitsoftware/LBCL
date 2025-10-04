import { getAuthHeaders } from "@/lib/auth-service";

export interface SKUImage {
  SKUUID: string;
  FileSysUID: string;
  IsDefault: boolean;
}

export interface FileSys {
  UID: string;
  LinkedItemType: string;
  LinkedItemUID: string;
  FileSysType: string;
  FileData?: string;
  FileType: string;
  ParentFileSysUID?: string;
  IsDirectory: boolean;
  FileName: string;
  DisplayName: string;
  FileSize: number;
  RelativePath?: string;
  TempPath?: string;
  Latitude?: string;
  Longitude?: string;
  CreatedByJobPositionUID?: string;
  CreatedByEmpUID?: string;
  IsDefault: boolean;
  CreatedBy?: string;
  CreatedTime?: string;
  ModifiedBy?: string;
  ModifiedTime?: string;
  ServerAddTime?: string;
  ServerModifiedTime?: string;
  FileSysFileType: FileType;
}

export enum FileType {
  None = 0,
  Image = 1,
  Pdf = 2,
  Doc = 3,
  Video = 4,
}

export interface SKUWithImages {
  sku: any;
  images: FileSys[];
  defaultImage?: FileSys;
}

export interface ImageUploadRequest {
  linkedItemType: string;
  linkedItemUID: string;
  fileSysType: string;
  fileData: string;
  fileType: string;
  fileName: string;
  displayName: string;
  fileSize: number;
  isDefault: boolean;
}

interface PagingRequest {
  PageNumber: number;
  PageSize: number;
  IsCountRequired: boolean;
  FilterCriterias: Array<{ Name: string; Value: string }>;
  SortCriterias: Array<{ SortParameter: string; Direction: string }>;
}

export interface ApiResponse<T> {
  Data: T;
  StatusCode: number;
  IsSuccess: boolean;
  ErrorMessage?: string;
}

export interface PagedResponseData<T> {
  PagedData: T[];
  TotalCount: number;
}

class SKUImagesService {
  private baseURL =
    process.env.NEXT_PUBLIC_API_URL || "https://multiplex-promotions-api.winitsoftware.com/api";

  // Get all images for specific SKUs
  async getSKUImages(skuUIDs: string[]): Promise<FileSys[]> {
    try {
      const requestBody: PagingRequest = {
        PageNumber: 1,
        PageSize: 1000,
        IsCountRequired: false,
        FilterCriterias: [
          { Name: "LinkedItemType", Value: "SKU" },
          { Name: "FileSysType", Value: "Image" },
          { Name: "LinkedItemUID", Value: skuUIDs.join(",") },
        ],
        SortCriterias: [
          { SortParameter: "IsDefault", Direction: "Desc" },
          { SortParameter: "DisplayName", Direction: "Asc" },
        ],
      };

      const response = await fetch(
        `${this.baseURL}/FileSys/SelectAllFileSysDetails`,
        {
          method: "POST",
          headers: {
            ...getAuthHeaders(),
            "Content-Type": "application/json",
            Accept: "application/json",
          },
          body: JSON.stringify(requestBody),
        }
      );

      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }

      const result: ApiResponse<PagedResponseData<FileSys>> =
        await response.json();

      if (result.IsSuccess && result.Data?.PagedData) {
        return result.Data.PagedData;
      }
      return [];
    } catch (error) {
      console.error("Error fetching SKU images:", error);
      throw error;
    }
  }

  // Get images for a single SKU
  async getSingleSKUImages(skuUID: string): Promise<FileSys[]> {
    return this.getSKUImages([skuUID]);
  }

  // Get all SKUs with their images - hybrid approach
  async getSKUsWithImages(
    pageNumber: number = 1,
    pageSize: number = 50,
    searchTerm?: string
  ): Promise<{
    skusWithImages: SKUWithImages[];
    totalCount: number;
  }> {
    try {
      // Step 1: Get SKUs with proper pagination
      let allSKUs: any[] = [];
      let totalSKUCount = 0;
      let skuApiWorked = false;

      try {
        const skuRequest: PagingRequest = {
          PageNumber: pageNumber,
          PageSize: pageSize, // Use the actual page size for proper pagination
          IsCountRequired: true,
          FilterCriterias: searchTerm
            ? [{ Name: "skucodeandname", Value: searchTerm }]
            : [],
          SortCriterias: [], // Remove problematic sort
        };

        const skuResponse = await fetch(
          `${this.baseURL}/SKU/SelectAllSKUDetailsWebView`,
          {
            method: "POST",
            headers: {
              ...getAuthHeaders(),
              "Content-Type": "application/json",
              Accept: "application/json",
            },
            body: JSON.stringify(skuRequest),
          }
        );

        if (skuResponse.ok) {
          const skuResult: ApiResponse<PagedResponseData<any>> =
            await skuResponse.json();
          if (skuResult?.IsSuccess && skuResult?.Data?.PagedData) {
            allSKUs = skuResult.Data.PagedData;
            totalSKUCount = skuResult.Data.TotalCount || 0; // Get the actual total count from API
            skuApiWorked = true;
            console.log(
              `SKU API worked, got ${allSKUs.length} SKUs from page ${pageNumber}, total: ${totalSKUCount}`
            );
          }
        }
      } catch (skuError) {
        console.warn("SKU API failed, will use fallback approach:", skuError);
      }

      // Step 2: Get images only for the current page of SKUs
      // Build filter for current SKU UIDs to reduce data transfer
      const skuUIDsFilter =
        allSKUs.length > 0 && skuApiWorked
          ? allSKUs
              .map(
                (sku: any) => sku.SKUUID || sku.UID || sku.SKUCode || sku.Code
              )
              .filter(Boolean)
              .join(",")
          : "";

      const fileSysRequest: PagingRequest = {
        PageNumber: 1,
        PageSize: 1000, // Still need to get all images for the current page of SKUs
        IsCountRequired: true,
        FilterCriterias: skuUIDsFilter
          ? [
              { Name: "LinkedItemType", Value: "SKU" },
              { Name: "FileSysType", Value: "Image" },
              { Name: "LinkedItemUID", Value: skuUIDsFilter },
            ]
          : [],
        SortCriterias: [],
      };

      const fileSysResponse = await fetch(
        `${this.baseURL}/FileSys/SelectAllFileSysDetails`,
        {
          method: "POST",
          headers: {
            ...getAuthHeaders(),
            "Content-Type": "application/json",
            Accept: "application/json",
          },
          body: JSON.stringify(fileSysRequest),
        }
      );

      if (!fileSysResponse.ok) {
        throw new Error(`FileSys API error! status: ${fileSysResponse.status}`);
      }

      const fileSysResult: ApiResponse<PagedResponseData<FileSys>> =
        await fileSysResponse.json();
      const skuImages =
        fileSysResult?.Data?.PagedData?.filter(
          (fileSys) =>
            fileSys.LinkedItemType === "SKU" && fileSys.FileSysType === "Image"
        ) || [];

      console.log("Found SKU images:", skuImages.length);

      // Step 3: Group images by SKU UID
      const imagesBySkuUID = new Map<string, FileSys[]>();
      skuImages.forEach((image) => {
        const skuUID = image.LinkedItemUID;
        if (!imagesBySkuUID.has(skuUID)) {
          imagesBySkuUID.set(skuUID, []);
        }
        imagesBySkuUID.get(skuUID)!.push(image);
      });

      // Step 4: If SKU API worked, use real SKU data; otherwise create SKUs from image data
      let finalSKUs: any[];

      if (skuApiWorked && allSKUs.length > 0) {
        // Use real SKU data
        finalSKUs = allSKUs;
      } else {
        // Fallback: Create SKUs from existing database records and image data
        const skuUIDs = new Set<string>();

        // Add SKU UIDs from images
        skuImages.forEach((image) => skuUIDs.add(image.LinkedItemUID));

        // Add some common SKU UIDs from database (you can expand this list)
        // This ensures users can upload to SKUs even if they don't have images yet
        const commonSKUUIDs = ["ABD0007140", "SKU001", "SKU002", "SKU003"]; // Add more as needed
        commonSKUUIDs.forEach((uid) => skuUIDs.add(uid));

        finalSKUs = Array.from(skuUIDs).map((skuUID) => ({
          UID: skuUID,
          SKUUID: skuUID,
          SKUCode: skuUID,
          Code: skuUID,
          SKULongName: `Product ${skuUID}`,
          Name: `Product ${skuUID}`,
        }));
      }

      // Step 5: Combine SKUs with their images
      const skusWithImages: SKUWithImages[] = finalSKUs.map((sku: any) => {
        const skuUID =
          sku.SKUUID || sku.UID || sku.uid || sku.SKUCode || sku.Code;
        const images = imagesBySkuUID.get(skuUID) || [];
        const defaultImage = images.find((img) => img.IsDefault) || images[0];

        return {
          sku: sku,
          images,
          defaultImage,
        };
      });

      // Step 6: Apply search filter if provided (only if SKU API failed)
      let filteredSKUs = skusWithImages;
      let finalTotalCount = totalSKUCount;

      if (searchTerm && !skuApiWorked) {
        // Only filter on frontend if SKU API didn't work (otherwise it was filtered on backend)
        filteredSKUs = skusWithImages.filter(
          (skuWithImages) =>
            skuWithImages.sku.SKUCode?.toLowerCase().includes(
              searchTerm.toLowerCase()
            ) ||
            skuWithImages.sku.Name?.toLowerCase().includes(
              searchTerm.toLowerCase()
            ) ||
            skuWithImages.sku.SKULongName?.toLowerCase().includes(
              searchTerm.toLowerCase()
            )
        );
        finalTotalCount = filteredSKUs.length;
      }

      // Step 7: Don't paginate again if SKU API worked (already paginated from backend)
      let finalSKUsWithImages = filteredSKUs;
      if (!skuApiWorked) {
        // Only paginate on frontend if we're using fallback data
        const startIndex = (pageNumber - 1) * pageSize;
        finalSKUsWithImages = filteredSKUs.slice(
          startIndex,
          startIndex + pageSize
        );
        finalTotalCount = filteredSKUs.length;
      }

      return {
        skusWithImages: finalSKUsWithImages,
        totalCount: finalTotalCount || finalSKUsWithImages.length,
      };
    } catch (error) {
      console.error("Error fetching SKUs with images:", error);
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

  // Upload image for SKU - Two-step process: upload file first, then create file_sys record
  async uploadSKUImage(request: ImageUploadRequest): Promise<any> {
    try {
      // Step 1: Upload the actual file using FileUpload endpoint
      const file = await this.base64ToFile(
        request.fileData,
        request.fileName,
        request.fileType
      );

      const formData = new FormData();
      // The FileUploadController expects files in HttpContext.Request.Form.Files
      formData.append("files", file, request.fileName);
      // Save to Data directory which is already configured to serve static files
      formData.append("folderPath", `Data/sku-images/${request.linkedItemUID}`);

      console.log("Step 1: Uploading file to server...");

      // Get auth token but don't set Content-Type
      const authToken = localStorage.getItem("auth_token");
      const headers: any = {};
      if (authToken) {
        headers["Authorization"] = `Bearer ${authToken}`;
      }

      const uploadResponse = await fetch(
        `${this.baseURL}/FileUpload/UploadFile`,
        {
          method: "POST",
          headers: headers,
          // Don't set Content-Type - let browser set it with boundary for multipart/form-data
          body: formData,
        }
      );

      if (!uploadResponse.ok) {
        throw new Error(`File upload failed! status: ${uploadResponse.status}`);
      }

      const uploadResult = await uploadResponse.json();
      console.log("File upload result:", uploadResult);

      if (uploadResult.Status !== 1) {
        // ImageUploadStatus.SUCCESS = 1
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
          : `Data/sku-images/${request.linkedItemUID}/${request.fileName}`;

      const fileSysData: any = {
        // Required BaseModel fields
        UID: uniqueUID,
        SS: 1, // Status flag, 1 for active
        CreatedBy: empUID,
        CreatedTime: new Date().toISOString(),
        ModifiedBy: empUID,
        ModifiedTime: new Date().toISOString(),

        // FileSys specific fields
        LinkedItemType: request.linkedItemType,
        LinkedItemUID: request.linkedItemUID,
        FileSysType: request.fileSysType,
        FileType: request.fileType,
        FileName: request.fileName,
        DisplayName: request.displayName,
        FileSize: request.fileSize,
        IsDefault: request.isDefault,
        IsDirectory: false,
        RelativePath: relativePath, // Set the path to the uploaded file
        FileSysFileType: 1, // 1 for Image
        CreatedByEmpUID: empUID,
      };

      console.log("Step 2: Creating file_sys record with path:", relativePath);

      const response = await fetch(`${this.baseURL}/FileSys/CreateFileSys`, {
        method: "POST",
        headers: {
          ...getAuthHeaders(),
          "Content-Type": "application/json",
          Accept: "application/json",
        },
        body: JSON.stringify(fileSysData),
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
      console.error("Error uploading SKU image:", error);
      throw error;
    }
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

  // Update SKU image default status
  async updateSKUImageDefault(skuImages: SKUImage[]): Promise<any> {
    try {
      const response = await fetch(
        `${this.baseURL}/FileSys/UpdateSKUImageIsDefault`,
        {
          method: "PUT",
          headers: {
            ...getAuthHeaders(),
            "Content-Type": "application/json",
            Accept: "application/json",
          },
          body: JSON.stringify(skuImages),
        }
      );

      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }

      const result = await response.json();
      return result;
    } catch (error) {
      console.error("Error updating SKU image default status:", error);
      throw error;
    }
  }

  // Delete SKU image
  async deleteSKUImage(fileSysUID: string): Promise<any> {
    try {
      const response = await fetch(
        `${this.baseURL}/FileSys/DeleteFileSys?UID=${fileSysUID}`,
        {
          method: "DELETE",
          headers: {
            ...getAuthHeaders(),
            "Content-Type": "application/json",
            Accept: "application/json",
          },
        }
      );

      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }

      const result = await response.json();
      return result;
    } catch (error) {
      console.error("Error deleting SKU image:", error);
      throw error;
    }
  }

  // Get file data as blob for display
  async getImageBlob(fileSys: FileSys): Promise<string> {
    try {
      if (fileSys.RelativePath) {
        // If we have a relative path, construct the full URL to the image
        // The backend serves static files from /data path
        const baseUrl =
          process.env.NEXT_PUBLIC_API_URL || "http://localhost:8000";
        // Remove 'api' from the URL if present since static files are served from root
        const staticUrl = baseUrl.replace("/api", "");

        // Convert the relative path: Data/sku-images/... -> /data/sku-images/...
        let imagePath = fileSys.RelativePath;
        if (imagePath.startsWith("Data/")) {
          imagePath = imagePath.replace("Data/", "/data/");
        } else if (!imagePath.startsWith("/data/")) {
          imagePath = `/data/${imagePath}`;
        }

        return `${staticUrl}${imagePath}`;
      } else if (fileSys.TempPath) {
        // Handle temp path similarly
        const baseUrl =
          process.env.NEXT_PUBLIC_API_URL || "http://localhost:8000";
        const staticUrl = baseUrl.replace("/api", "");

        let imagePath = fileSys.TempPath;
        if (imagePath.startsWith("Data/")) {
          imagePath = imagePath.replace("Data/", "/data/");
        } else if (!imagePath.startsWith("/data/")) {
          imagePath = `/data/${imagePath}`;
        }

        return `${staticUrl}${imagePath}`;
      } else if (fileSys.FileData) {
        // Fallback: If we have base64 data (shouldn't happen with new flow), convert it to blob URL
        const byteCharacters = atob(fileSys.FileData);
        const byteNumbers = new Array(byteCharacters.length);
        for (let i = 0; i < byteCharacters.length; i++) {
          byteNumbers[i] = byteCharacters.charCodeAt(i);
        }
        const byteArray = new Uint8Array(byteNumbers);
        const blob = new Blob([byteArray], { type: fileSys.FileType });
        return URL.createObjectURL(blob);
      }

      // Return placeholder image
      return "/placeholder-product.png";
    } catch (error) {
      console.error("Error creating image URL:", error);
      return "/placeholder-product.png";
    }
  }

  // Convert file to base64
  async fileToBase64(file: File): Promise<string> {
    return new Promise((resolve, reject) => {
      const reader = new FileReader();
      reader.readAsDataURL(file);
      reader.onload = () => {
        const result = reader.result as string;
        // Remove data URL prefix to get pure base64
        // Format is typically: data:image/jpeg;base64,/9j/4AAQSkZJRg...
        const base64 = result.split(",")[1];
        if (!base64) {
          reject(new Error("Failed to extract base64 data from file"));
        } else {
          console.log("Base64 conversion successful, length:", base64.length);
          resolve(base64);
        }
      };
      reader.onerror = (error) => reject(error);
    });
  }

  // Bulk assign images to users/employees
  async assignImagesToUsers(assignments: {
    fileSysUIDs: string[];
    userUIDs: string[];
    assignmentType: "view" | "edit" | "manage";
  }): Promise<any> {
    try {
      // This would need to be implemented in the backend
      // For now, we'll use a placeholder implementation
      const response = await fetch(
        `${this.baseURL}/FileSys/AssignImagesToUsers`,
        {
          method: "POST",
          headers: {
            ...getAuthHeaders(),
            "Content-Type": "application/json",
            Accept: "application/json",
          },
          body: JSON.stringify(assignments),
        }
      );

      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }

      const result = await response.json();
      return result;
    } catch (error) {
      console.error("Error assigning images to users:", error);
      throw error;
    }
  }

  // Get images assigned to specific user
  async getUserAssignedImages(userUID: string): Promise<FileSys[]> {
    try {
      const requestBody: PagingRequest = {
        PageNumber: 1,
        PageSize: 1000,
        IsCountRequired: false,
        FilterCriterias: [
          { Name: "AssignedToUserUID", Value: userUID },
          { Name: "FileSysType", Value: "Image" },
        ],
        SortCriterias: [{ SortParameter: "DisplayName", Direction: "Asc" }],
      };

      const response = await fetch(
        `${this.baseURL}/FileSys/SelectAllFileSysDetails`,
        {
          method: "POST",
          headers: {
            ...getAuthHeaders(),
            "Content-Type": "application/json",
            Accept: "application/json",
          },
          body: JSON.stringify(requestBody),
        }
      );

      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }

      const result: ApiResponse<PagedResponseData<FileSys>> =
        await response.json();

      if (result.IsSuccess && result.Data?.PagedData) {
        return result.Data.PagedData;
      }
      return [];
    } catch (error) {
      console.error("Error fetching user assigned images:", error);
      throw error;
    }
  }
}

export const skuImagesService = new SKUImagesService();
