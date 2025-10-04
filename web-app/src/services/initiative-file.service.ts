import { getAuthHeaders } from "@/lib/auth-service";

export interface InitiativeFile {
  UID: string;
  LinkedItemType: string;
  LinkedItemUID: string;
  FileSysType: string; // "POSM", "DefaultImage", "EmailAttachment"
  FileType: string; // MIME type
  FileName: string;
  DisplayName: string;
  FileSize: number;
  RelativePath?: string;
  IsDefault: boolean;
  CreatedBy?: string;
  CreatedTime?: string;
}

export interface FileUploadResponse {
  Status: number;
  Message: string;
  SavedImgsPath?: string[];
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

class InitiativeFileService {
  private baseURL =
    process.env.NEXT_PUBLIC_API_URL || "https://multiplex-promotions-api.winitsoftware.com/api";

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

  // Convert base64 to File object
  private async base64ToFile(
    base64Data: string,
    fileName: string,
    mimeType: string
  ): Promise<File> {
    // Remove data URL prefix if present
    const base64WithoutPrefix = base64Data.replace(/^data:.*,/, "");

    // Convert base64 to blob
    const byteCharacters = atob(base64WithoutPrefix);
    const byteNumbers = new Array(byteCharacters.length);
    for (let i = 0; i < byteCharacters.length; i++) {
      byteNumbers[i] = byteCharacters.charCodeAt(i);
    }
    const byteArray = new Uint8Array(byteNumbers);
    const blob = new Blob([byteArray], { type: mimeType });

    // Convert blob to File
    return new File([blob], fileName, { type: mimeType });
  }

  // Convert File to base64
  private fileToBase64(file: File): Promise<string> {
    return new Promise((resolve, reject) => {
      const reader = new FileReader();
      reader.onload = () => {
        resolve(reader.result as string);
      };
      reader.onerror = reject;
      reader.readAsDataURL(file);
    });
  }

  // Upload file for initiative (POSM, Default Image, or Email Attachment)
  async uploadInitiativeFile(
    initiativeUID: string,
    file: File,
    fileType: "POSM" | "DefaultImage" | "EmailAttachment"
  ): Promise<any> {
    try {
      // Step 1: Upload the physical file
      const formData = new FormData();
      formData.append("files", file, file.name);

      // Organize files by type
      let subFolder = "";
      switch (fileType) {
        case "POSM":
          subFolder = "posm";
          break;
        case "DefaultImage":
          subFolder = "images";
          break;
        case "EmailAttachment":
          subFolder = "attachments";
          break;
      }

      // Don't include "Data/" prefix as the controller adds it
      formData.append(
        "folderPath",
        `initiative-files/${initiativeUID}/${subFolder}`
      );

      if (fileType === "EmailAttachment")
        console.log(
          `Uploading ${fileType} file for initiative ${initiativeUID}...`
        );

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
        const errorText = await uploadResponse.text();
        console.error("Upload failed:", errorText);
        throw new Error(`File upload failed! status: ${uploadResponse.status}`);
      }

      const uploadResult: FileUploadResponse = await uploadResponse.json();
      if (fileType === "EmailAttachment")
        console.log("File upload result:", uploadResult);

      if (uploadResult.Status !== 1) {
        throw new Error(uploadResult.Message || "File upload failed");
      }

      // Step 2: Create FileSys record
      const uniqueUID = this.generateUID();

      // Get user info
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
        throw new Error("User not authenticated. Please login again.");
      }

      // Extract the relative path - remove duplicate "Data/" if present
      let relativePath = "";
      if (uploadResult.SavedImgsPath && uploadResult.SavedImgsPath.length > 0) {
        relativePath = uploadResult.SavedImgsPath[0];
        // Remove duplicate "Data/" prefix if present
        if (relativePath.startsWith("Data/Data/")) {
          relativePath = relativePath.substring(5); // Remove first "Data/"
        }
      } else {
        relativePath = `Data/initiative-files/${initiativeUID}/${subFolder}/${file.name}`;
      }

      if (fileType === "EmailAttachment")
        console.log("Processed relative path:", relativePath);

      // Truncate file name to fit varchar(50) constraint
      const truncatedFileName =
        file.name.length > 50
          ? file.name.substring(0, 46) + file.name.slice(-4) // Keep last 4 chars for extension
          : file.name;

      const truncatedDisplayName =
        file.name.length > 50
          ? file.name.substring(0, 45) + "..." // Show ellipsis for display
          : file.name;

      const fileSysData: any = {
        UID: uniqueUID,
        SS: 1, // Active status
        CreatedBy: empUID,
        CreatedTime: new Date().toISOString(),
        ModifiedBy: empUID,
        ModifiedTime: new Date().toISOString(),

        // FileSys specific fields
        LinkedItemType: "Initiative",
        LinkedItemUID: initiativeUID,
        FileSysType: fileType,
        FileType: file.type,
        FileName: truncatedFileName, // Truncated to fit varchar(50)
        DisplayName: truncatedDisplayName, // Truncated to fit varchar(50)
        FileSize: file.size,
        IsDefault: false,
        IsDirectory: false,
        RelativePath: relativePath,
        FileSysFileType: this.getFileSysFileType(file.type),

        // Required for API
        CreatedByEmpUID: empUID,
      };

      if (fileType === "EmailAttachment")
        console.log("Creating FileSys record:", fileSysData);
      console.log(`🔍 FileSys API call for ${fileType}:`, {
        url: `${this.baseURL}/FileSys/CUDFileSys`,
        fileType: fileType,
        fileName: truncatedFileName,
        fileSysType: fileSysData.FileSysType,
        mimeType: fileSysData.FileType,
      });

      // Add timeout for FileSys creation
      const controller = new AbortController();
      const timeoutId = setTimeout(() => {
        console.error(`⏱️ Timeout triggered for ${fileType} FileSys creation`);
        controller.abort();
      }, 30000); // 30 second timeout

      let fileSysResponse;
      try {
        // Use CUDFileSys endpoint which works for Initiative files
        fileSysResponse = await fetch(`${this.baseURL}/FileSys/CUDFileSys`, {
          method: "POST",
          headers: {
            ...getAuthHeaders(),
            "Content-Type": "application/json",
            Accept: "application/json",
          },
          body: JSON.stringify(fileSysData),
          signal: controller.signal,
        });
        clearTimeout(timeoutId);
      } catch (fetchError: any) {
        clearTimeout(timeoutId);
        if (fetchError.name === "AbortError") {
          console.error(`❌ FileSys creation timed out for ${fileType}`);
          throw new Error(
            `FileSys creation timed out after 30 seconds for ${fileType}`
          );
        }
        throw fetchError;
      }

      if (fileType === "EmailAttachment")
        console.log(`📡 FileSys Response for ${fileType}:`, {
          status: fileSysResponse.status,
          statusText: fileSysResponse.statusText,
          ok: fileSysResponse.ok,
          headers: Object.fromEntries(fileSysResponse.headers.entries()),
        });

      if (!fileSysResponse.ok) {
        let errorMessage = `FileSys creation failed! status: ${fileSysResponse.status}`;
        try {
          const errorText = await fileSysResponse.text();
          console.error(
            `❌ DETAILED ERROR for ${fileType} - Status: ${fileSysResponse.status} - Response:`,
            errorText
          );

          // Try to parse as JSON for better error message
          try {
            const errorJson = JSON.parse(errorText);
            console.error(`❌ PARSED ERROR for ${fileType}:`, errorJson);
            if (errorJson.ErrorMessage) {
              errorMessage = errorJson.ErrorMessage;
            } else if (errorJson.Message) {
              errorMessage = errorJson.Message;
            } else if (errorJson.message) {
              errorMessage = errorJson.message;
            }
          } catch (jsonParseError) {
            console.error(`❌ Raw error text for ${fileType}:`, errorText);
          }
        } catch (textError) {
          console.error(
            `❌ Could not read error response for ${fileType}:`,
            textError
          );
        }
        throw new Error(errorMessage);
      }

      const fileSysResult = await fileSysResponse.json();
      if (fileType === "EmailAttachment") {
        console.log("📊 FileSys creation result:", fileSysResult);
        console.log(`📊 FileSys result details for ${fileType}:`, {
          isSuccess: fileSysResult.IsSuccess,
          statusCode: fileSysResult.StatusCode,
          data: fileSysResult.Data,
          errorMessage: fileSysResult.ErrorMessage,
        });
      }

      // Check if the API returned success
      if (fileSysResult.IsSuccess === false) {
        console.error("FileSys creation failed with response:", fileSysResult);
        throw new Error(
          fileSysResult.ErrorMessage || "FileSys creation failed"
        );
      }

      // Log success
      if (fileType === "EmailAttachment") {
        console.log(`✅ ${fileType} file saved successfully:`, {
          fileUID: uniqueUID,
          fileName: truncatedFileName,
          path: relativePath,
        });
      }

      return {
        success: true,
        fileUID: uniqueUID,
        relativePath: relativePath,
        message: `${fileType} uploaded successfully`,
      };
    } catch (error) {
      if (fileType === "EmailAttachment")
        console.error(`❌ Error uploading ${fileType}:`, error);
      throw error;
    }
  }

  // Get file type enum value based on MIME type
  private getFileSysFileType(mimeType: string): number {
    console.log("Getting FileSysFileType for MIME type:", mimeType);

    if (mimeType.startsWith("image/")) return 1; // Image
    if (mimeType.includes("pdf")) return 2; // PDF
    if (mimeType.includes("doc") || mimeType.includes("word")) return 3; // Doc
    if (mimeType.startsWith("video/")) return 4; // Video
    if (mimeType.includes("zip") || mimeType.includes("compressed")) return 3; // Treat ZIP as Doc type
    if (mimeType.includes("sheet") || mimeType.includes("excel")) return 3; // Excel as Doc

    console.log("Unknown MIME type, defaulting to Doc type (3):", mimeType);
    return 3; // Default to Doc type instead of 0 for better compatibility
  }

  // Get all files for an initiative
  async getInitiativeFiles(initiativeUID: string): Promise<InitiativeFile[]> {
    try {
      const requestBody = {
        PageNumber: 1,
        PageSize: 100,
        IsCountRequired: true,
        FilterCriterias: [
          { Name: "LinkedItemType", Value: "Initiative" },
          { Name: "LinkedItemUID", Value: initiativeUID },
        ],
        SortCriterias: [
          { SortParameter: "FileSysType", Direction: "Asc" },
          { SortParameter: "CreatedTime", Direction: "Desc" },
        ],
      };

      console.log(`🔍 Fetching files for initiative ${initiativeUID}...`);
      console.log("Request body:", JSON.stringify(requestBody, null, 2));

      const response = await fetch(
        `${this.baseURL}/FileSys/SelectAllFileSysDetails`,
        {
          method: "POST",
          headers: {
            ...getAuthHeaders(),
            "Content-Type": "application/json",
          },
          body: JSON.stringify(requestBody),
        }
      );

      console.log(`📡 FileSys response status: ${response.status}`);

      if (!response.ok) {
        const errorText = await response.text();
        console.error(`❌ FileSys fetch failed: ${response.status}`, errorText);
        throw new Error(`Failed to fetch files! status: ${response.status}`);
      }

      const result: ApiResponse<PagedResponseData<InitiativeFile>> =
        await response.json();
      console.log("📊 FileSys raw response:", result);

      if (result.IsSuccess && result.Data?.PagedData) {
        console.log(
          `✅ Found ${result.Data.PagedData.length} files for initiative ${initiativeUID}:`,
          result.Data.PagedData
        );
        return result.Data.PagedData;
      }

      console.log(
        `⚠️ No files found for initiative ${initiativeUID}. Response:`,
        result
      );
      return [];
    } catch (error) {
      console.error("Error fetching initiative files:", error);
      return [];
    }
  }

  // Get only image files for an initiative
  async getInitiativeImages(initiativeUID: string): Promise<InitiativeFile[]> {
    try {
      const requestBody = {
        PageNumber: 1,
        PageSize: 100,
        IsCountRequired: true,
        FilterCriterias: [
          { Name: "LinkedItemType", Value: "Initiative" },
          { Name: "LinkedItemUID", Value: initiativeUID },
          { Name: "FileType", Value: "image/jpeg" }, // Only get JPEG images
        ],
        SortCriterias: [
          { SortParameter: "FileSysType", Direction: "Asc" },
          { SortParameter: "CreatedTime", Direction: "Desc" },
        ],
      };

      console.log(`🖼️ Fetching images for initiative ${initiativeUID}...`);

      const response = await fetch(
        `${this.baseURL}/FileSys/SelectAllFileSysDetails`,
        {
          method: "POST",
          headers: {
            ...getAuthHeaders(),
            "Content-Type": "application/json",
          },
          body: JSON.stringify(requestBody),
        }
      );

      console.log(`📡 Initiative images response status: ${response.status}`);

      if (!response.ok) {
        const errorText = await response.text();
        console.error(
          `❌ Initiative images fetch failed: ${response.status}`,
          errorText
        );
        throw new Error(`Failed to fetch images! status: ${response.status}`);
      }

      const result: ApiResponse<PagedResponseData<InitiativeFile>> =
        await response.json();
      console.log("🖼️ Initiative images response:", result);

      if (result.IsSuccess && result.Data?.PagedData) {
        console.log(
          `✅ Found ${result.Data.PagedData.length} images for initiative ${initiativeUID}:`,
          result.Data.PagedData
        );
        return result.Data.PagedData;
      }

      console.log(
        `⚠️ No images found for initiative ${initiativeUID}. Response:`,
        result
      );
      return [];
    } catch (error) {
      console.error("Error fetching initiative images:", error);
      return [];
    }
  }

  // Delete a file
  async deleteFile(fileUID: string): Promise<boolean> {
    try {
      const response = await fetch(`${this.baseURL}/FileSys/DeleteFileSys`, {
        method: "DELETE",
        headers: {
          ...getAuthHeaders(),
          "Content-Type": "application/json",
        },
        body: JSON.stringify({ UID: fileUID }),
      });

      if (!response.ok) {
        throw new Error(`Failed to delete file! status: ${response.status}`);
      }

      const result = await response.json();
      return result.IsSuccess || false;
    } catch (error) {
      console.error("Error deleting file:", error);
      return false;
    }
  }

  // Get file URL for display
  getFileUrl(relativePath: string): string {
    if (!relativePath) return "";

    // Convert backslashes to forward slashes for web URLs
    const webPath = relativePath.replace(/\\/g, "/");

    // Remove "Data/" prefix if present since it's served from /data/
    const cleanPath = webPath.startsWith("Data/")
      ? webPath.substring(5)
      : webPath;

    return `/data/${cleanPath}`;
  }

  // Validate file before upload
  validateFile(
    file: File,
    fileType: "POSM" | "DefaultImage" | "EmailAttachment"
  ): {
    valid: boolean;
    error?: string;
  } {
    const maxSize = 10 * 1024 * 1024; // 10MB limit - backend restriction

    if (file.size > maxSize) {
      return {
        valid: false,
        error: `File size exceeds 10MB limit. Current size: ${(
          file.size /
          1024 /
          1024
        ).toFixed(2)}MB`,
      };
    }

    // Validate file types based on the field
    const allowedTypes: { [key: string]: string[] } = {
      POSM: ["image/jpeg", "image/png", "image/gif", "application/pdf"],
      DefaultImage: ["image/jpeg", "image/png", "image/gif"],
      EmailAttachment: [
        "application/pdf",
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "application/vnd.ms-excel",
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "application/zip",
        "application/x-zip-compressed",
      ],
    };

    const allowed = allowedTypes[fileType] || [];
    if (!allowed.includes(file.type)) {
      return {
        valid: false,
        error: `Invalid file type. Allowed types for ${fileType}: ${allowed.join(
          ", "
        )}`,
      };
    }

    return { valid: true };
  }

  // Format file size for display
  formatFileSize(bytes: number): string {
    if (bytes === 0) return "0 Bytes";
    const k = 1024;
    const sizes = ["Bytes", "KB", "MB", "GB"];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + " " + sizes[i];
  }
}

export const initiativeFileService = new InitiativeFileService();
