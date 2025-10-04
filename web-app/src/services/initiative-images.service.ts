import { getAuthHeaders } from "@/lib/auth-service";

export interface InitiativeFile {
  UID: string;
  LinkedItemType: string;
  LinkedItemUID: string;
  FileSysType: string; // "POSM", "DefaultImage", "EmailAttachment", "Image"
  FileType: string; // MIME type
  FileName: string;
  DisplayName: string;
  FileSize: number;
  RelativePath?: string;
  TempPath?: string;
  IsDefault: boolean;
  IsDirectory: boolean;
  CreatedBy?: string;
  CreatedTime?: string;
  ModifiedBy?: string;
  ModifiedTime?: string;
  FileSysFileType: number;
}

export interface InitiativeWithImages {
  initiative: any;
  images: InitiativeFile[];
  defaultImage?: InitiativeFile;
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

class InitiativeImagesService {
  private baseURL =
    process.env.NEXT_PUBLIC_API_URL || "https://multiplex-promotions-api.winitsoftware.com/api";

  // Get all images for specific Initiative UIDs
  async getInitiativeImages(
    initiativeUIDs: string[]
  ): Promise<InitiativeFile[]> {
    try {
      const requestBody: PagingRequest = {
        PageNumber: 1,
        PageSize: 1000,
        IsCountRequired: false,
        FilterCriterias: [
          { Name: "LinkedItemType", Value: "Initiative" },
          { Name: "LinkedItemUID", Value: initiativeUIDs.join(",") },
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

      const result: ApiResponse<PagedResponseData<InitiativeFile>> =
        await response.json();

      if (result.IsSuccess && result.Data?.PagedData) {
        // Filter for image files (POSM, DefaultImage, or Image type)
        return result.Data.PagedData.filter(
          (file) =>
            file.LinkedItemType === "Initiative" &&
            (file.FileSysType === "POSM" ||
              file.FileSysType === "DefaultImage" ||
              file.FileSysType === "Image" ||
              (file.FileType && file.FileType.startsWith("image/")))
        );
      }
      return [];
    } catch (error) {
      console.error("Error fetching Initiative images:", error);
      throw error;
    }
  }

  // Get images for a single Initiative
  async getSingleInitiativeImages(
    initiativeUID: string
  ): Promise<InitiativeFile[]> {
    return this.getInitiativeImages([initiativeUID]);
  }

  // Get all Initiatives with their images - optimized approach
  async getInitiativesWithImages(
    pageNumber: number = 1,
    pageSize: number = 50,
    searchTerm?: string
  ): Promise<{
    initiativesWithImages: InitiativeWithImages[];
    totalCount: number;
  }> {
    try {
      console.log(
        `🔍 Loading images for initiatives - Page: ${pageNumber}, Size: ${pageSize}`
      );

      // Step 1: Get ALL image files from database (similar to SKU approach)
      const fileSysRequest: PagingRequest = {
        PageNumber: 1,
        PageSize: 1000, // Get all images
        IsCountRequired: true,
        FilterCriterias: [
          { Name: "LinkedItemType", Value: "Initiative" },
          // Filter for image files only
        ],
        SortCriterias: [],
      };

      console.log("Fetching ALL Initiative images from database...");
      const response = await fetch(
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

      if (!response.ok) {
        throw new Error(`FileSys API error! status: ${response.status}`);
      }

      const fileSysResult: ApiResponse<PagedResponseData<InitiativeFile>> =
        await response.json();

      const allFileSysRecords = fileSysResult?.Data?.PagedData || [];
      console.log("Total FileSys records from API:", allFileSysRecords.length);

      // Filter for Initiative image files
      const allInitiativeImages = allFileSysRecords.filter(
        (fileSys: any) =>
          fileSys.LinkedItemType === "Initiative" &&
          (fileSys.FileSysType === "POSM" ||
            fileSys.FileSysType === "DefaultImage" ||
            fileSys.FileSysType === "Image" ||
            (fileSys.FileType && fileSys.FileType.startsWith("image/")))
      );

      console.log("Filtered Initiative images:", allInitiativeImages.length);

      // Step 2: Group images by Initiative UID
      const imagesByInitiativeUID = new Map<string, InitiativeFile[]>();
      allInitiativeImages.forEach((image: any) => {
        const initiativeUID = image.LinkedItemUID;
        if (!imagesByInitiativeUID.has(initiativeUID)) {
          imagesByInitiativeUID.set(initiativeUID, []);
        }
        imagesByInitiativeUID.get(initiativeUID)!.push(image);
      });

      // Step 3: Create initiative objects from image data
      const initiativeUIDs = Array.from(imagesByInitiativeUID.keys());
      const initiativesWithImages: InitiativeWithImages[] = initiativeUIDs.map(
        (initiativeUID) => {
          const images = imagesByInitiativeUID.get(initiativeUID) || [];
          const defaultImage = images.find((img) => img.IsDefault) || images[0];

          // Create a minimal initiative object
          const initiative = {
            InitiativeId: parseInt(initiativeUID) || initiativeUID,
            UID: initiativeUID,
            Name: `Initiative ${initiativeUID}`,
            // Add more fields as needed
          };

          return {
            initiative,
            images,
            defaultImage,
          };
        }
      );

      // Step 4: Apply search filter if provided
      let filteredInitiatives = initiativesWithImages;
      if (searchTerm) {
        filteredInitiatives = initiativesWithImages.filter(
          (initiativeWithImages) =>
            initiativeWithImages.initiative.Name?.toLowerCase().includes(
              searchTerm.toLowerCase()
            ) ||
            initiativeWithImages.initiative.InitiativeId?.toString().includes(
              searchTerm
            )
        );
      }

      // Step 5: Apply pagination
      const startIndex = (pageNumber - 1) * pageSize;
      const paginatedInitiatives = filteredInitiatives.slice(
        startIndex,
        startIndex + pageSize
      );

      return {
        initiativesWithImages: paginatedInitiatives,
        totalCount: filteredInitiatives.length,
      };
    } catch (error) {
      console.error("Error fetching Initiatives with images:", error);
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

  // Upload image for Initiative
  async uploadInitiativeImage(request: ImageUploadRequest): Promise<any> {
    try {
      // Step 1: Upload the actual file using FileUpload endpoint
      const file = await this.base64ToFile(
        request.fileData,
        request.fileName,
        request.fileType
      );

      const formData = new FormData();
      formData.append("files", file, request.fileName);
      formData.append(
        "folderPath",
        `initiative-images/${request.linkedItemUID}`
      );

      console.log("Step 1: Uploading file to server...");

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
          body: formData,
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
          : `Data/initiative-images/${request.linkedItemUID}/${request.fileName}`;

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
        RelativePath: relativePath,
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
      console.error("Error uploading Initiative image:", error);
      throw error;
    }
  }

  // Helper to convert base64 to File object
  private async base64ToFile(
    base64: string,
    fileName: string,
    mimeType: string
  ): Promise<File> {
    const byteCharacters = atob(base64);
    const byteNumbers = new Array(byteCharacters.length);
    for (let i = 0; i < byteCharacters.length; i++) {
      byteNumbers[i] = byteCharacters.charCodeAt(i);
    }
    const byteArray = new Uint8Array(byteNumbers);
    const blob = new Blob([byteArray], { type: mimeType });

    return new File([blob], fileName, { type: mimeType });
  }

  // Delete Initiative image
  async deleteInitiativeImage(fileSysUID: string): Promise<any> {
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
      console.error("Error deleting Initiative image:", error);
      throw error;
    }
  }

  // Get file data as URL for display (same logic as initiative-file.service.ts)
  async getImageBlob(fileSys: InitiativeFile): Promise<string> {
    try {
      if (fileSys.RelativePath) {
        // If we have a relative path, construct the full URL to the image
        const baseUrl =
          process.env.NEXT_PUBLIC_API_URL || "http://localhost:8000";
        // Remove 'api' from the URL if present since static files are served from root
        const staticUrl = baseUrl.replace("/api", "");

        // Convert the relative path: Data/initiative-images/... -> /data/initiative-images/...
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
      }

      // Return placeholder image
      return "/placeholder-initiative.png";
    } catch (error) {
      console.error("Error creating image URL:", error);
      return "/placeholder-initiative.png";
    }
  }

  // Convert file to base64
  async fileToBase64(file: File): Promise<string> {
    return new Promise((resolve, reject) => {
      const reader = new FileReader();
      reader.readAsDataURL(file);
      reader.onload = () => {
        const result = reader.result as string;
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

  // Load images for a specific initiative ID (for the view page)
  async loadImagesForInitiative(initiativeId: string | number): Promise<{
    images: InitiativeFile[];
    imageUrls: Record<string, string>;
    defaultImage?: InitiativeFile;
  }> {
    try {
      console.log(`Loading images for initiative ${initiativeId}...`);

      // Get images for this specific initiative
      const images = await this.getSingleInitiativeImages(
        initiativeId.toString()
      );

      console.log(
        `Found ${images.length} images for initiative ${initiativeId}`
      );

      // Generate image URLs
      const imageUrls: Record<string, string> = {};
      for (const image of images) {
        if (!imageUrls[image.UID]) {
          imageUrls[image.UID] = await this.getImageBlob(image);
        }
      }

      // Find default image
      const defaultImage = images.find((img) => img.IsDefault) || images[0];

      console.log(`Generated URLs for ${Object.keys(imageUrls).length} images`);

      return {
        images,
        imageUrls,
        defaultImage,
      };
    } catch (error) {
      console.error(
        `Failed to load images for initiative ${initiativeId}:`,
        error
      );
      return {
        images: [],
        imageUrls: {},
      };
    }
  }
}

export const initiativeImagesService = new InitiativeImagesService();
