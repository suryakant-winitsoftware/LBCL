import { apiService } from './api';

// Types
export interface FileSysFile {
  UID: string;
  LinkedItemType: string;
  LinkedItemUID: string;
  FileSysType: string;
  FileType: string;
  FileName: string;
  DisplayName: string;
  FileSize: number;
  RelativePath: string;
  Latitude?: string;
  Longitude?: string;
  CreatedByEmpUID: string;
  CreatedBy: string;
  CreatedTime: string;
  ModifiedTime: string;
  IsDefault: boolean;
}

export interface FileSysResponse {
  Data: {
    PagedData: FileSysFile[];
    TotalCount: number;
  };
  StatusCode: number;
  IsSuccess: boolean;
}

class FileSysService {
  private baseUrl = '/FileSys';

  // Get all files with pagination and filters
  async getAllFiles(filters: any = {}): Promise<FileSysResponse> {
    try {
      const response = await apiService.post(`${this.baseUrl}/SelectAllFileSysDetails`, {
        PageNumber: filters.pageNumber || 0,
        PageSize: filters.pageSize || 100,
        IsCountRequired: filters.isCountRequired || false,
        FilterCriterias: filters.filterCriterias || [],
        SortCriterias: filters.sortCriterias || []
      });
      return response;
    } catch (error) {
      console.error('Error fetching files:', error);
      return {
        Data: { PagedData: [], TotalCount: 0 },
        StatusCode: 500,
        IsSuccess: false
      };
    }
  }

  // Get files by linked item UID (e.g., user journey UID)
  async getFilesByLinkedItemUID(linkedItemUID: string, linkedItemType?: string): Promise<FileSysFile[]> {
    try {
      console.log('🔍 FileSysService: getFilesByLinkedItemUID called with:', {
        linkedItemUID,
        linkedItemType
      });

      const filterCriterias = [
        {
          Name: 'LinkedItemUID',
          Value: linkedItemUID,
          Type: 0 // Equals
        }
      ];

      // Add linked item type filter if provided
      if (linkedItemType) {
        filterCriterias.push({
          Name: 'LinkedItemType',
          Value: linkedItemType,
          Type: 0 // Equals
        });
      }

      console.log('🔍 FileSysService: Sending API request with filters:', filterCriterias);
      console.log('🔍 FileSysService: Filter details:', filterCriterias.map(f => `${f.Name} ${f.Type === 0 ? '=' : 'contains'} "${f.Value}"`));

      const response = await this.getAllFiles({
        filterCriterias,
        pageSize: 50
      });

      console.log('🔍 FileSysService: API response received:', {
        isSuccess: response.IsSuccess,
        statusCode: response.StatusCode,
        totalFiles: response.Data?.PagedData?.length || 0
      });

      const files = response.Data?.PagedData || [];
      
      if (files.length > 0) {
        console.log('🔍 FileSysService: Files found:', files.map(file => ({
          UID: file.UID,
          FileName: file.FileName,
          RelativePath: file.RelativePath,
          FileType: file.FileType,
          FileSysType: file.FileSysType,
          LinkedItemType: file.LinkedItemType,
          LinkedItemUID: file.LinkedItemUID,
          FileSize: file.FileSize,
          GPS: file.Latitude && file.Longitude ? `${file.Latitude}, ${file.Longitude}` : 'No GPS'
        })));
        
        // Debug: Check if this is actually an attendance selfie
        files.forEach(file => {
          console.log('🔍 FileSysService: File details check:', {
            FileName: file.FileName,
            LinkedItemType: file.LinkedItemType,
            FileSysType: file.FileSysType,
            RelativePath: file.RelativePath,
            isAttendanceSelfie: file.LinkedItemType === 'attendance_selfie',
            isSKUImage: file.RelativePath?.includes('sku-images')
          });
        });
      } else {
        console.log('🔍 FileSysService: No files found for linkedItemUID:', linkedItemUID);
      }

      return files;
    } catch (error) {
      console.error('❌ FileSysService: Error fetching files by linked item UID:', error);
      return [];
    }
  }

  // Get attendance photos for a user journey
  async getAttendancePhotosByJourneyUID(userJourneyUID: string): Promise<FileSysFile[]> {
    return this.getFilesByLinkedItemUID(userJourneyUID, 'attendance_selfie');
  }

  // Get all images for a user journey
  async getImagesByJourneyUID(userJourneyUID: string): Promise<FileSysFile[]> {
    try {
      const filterCriterias = [
        {
          Name: 'LinkedItemUID',
          Value: userJourneyUID,
          Type: 0 // Equals
        },
        {
          Name: 'FileType',
          Value: 'image/',
          Type: 10 // Contains (to match image/jpeg, image/png, etc.)
        }
      ];

      const response = await this.getAllFiles({
        filterCriterias,
        pageSize: 50
      });

      return response.Data?.PagedData || [];
    } catch (error) {
      console.error('Error fetching images by journey UID:', error);
      return [];
    }
  }

  // Get file by UID
  async getFileByUID(uid: string): Promise<FileSysFile | null> {
    try {
      const response = await apiService.get(`${this.baseUrl}/GetFileSysByUID?UID=${uid}`);
      return response.Data || null;
    } catch (error) {
      console.error('Error fetching file by UID:', error);
      return null;
    }
  }

  // Get file URL for display (construct the full URL)
  getFileUrl(file: FileSysFile): string {
    console.log('🔍 FileSysService: getFileUrl called for file:', {
      UID: file.UID,
      FileName: file.FileName,
      RelativePath: file.RelativePath,
      FileType: file.FileType
    });

    // Assuming files are served from a static endpoint
    // Adjust this based on your backend file serving setup
    if (file.RelativePath) {
      // Convert backslashes to forward slashes for web URLs
      const webPath = file.RelativePath.replace(/\\/g, '/');
      const fullUrl = `/api/files/${webPath}`;
      
      console.log('🔍 FileSysService: Generated file URL:', {
        originalPath: file.RelativePath,
        webPath: webPath,
        fullUrl: fullUrl
      });
      
      return fullUrl;
    }
    
    console.log('🔍 FileSysService: No RelativePath found, returning empty URL');
    return '';
  }

  // Get files by file system type
  async getFilesByType(fileSysType: string): Promise<FileSysFile[]> {
    try {
      const filterCriterias = [
        {
          Name: 'FileSysType',
          Value: fileSysType,
          Type: 0 // Equals
        }
      ];

      const response = await this.getAllFiles({
        filterCriterias,
        pageSize: 100
      });

      return response.Data?.PagedData || [];
    } catch (error) {
      console.error('Error fetching files by type:', error);
      return [];
    }
  }

  // Get all attendance selfies
  async getAllAttendancePhotos(): Promise<FileSysFile[]> {
    return this.getFilesByType('ATTENDANCE_SELFIE');
  }

  // Helper function to format file size
  formatFileSize(bytes: number): string {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  }

  // Helper function to get file icon based on file type
  getFileIcon(fileType: string): string {
    if (fileType.startsWith('image/')) return '🖼️';
    if (fileType.startsWith('video/')) return '🎥';
    if (fileType.startsWith('audio/')) return '🎵';
    if (fileType.includes('pdf')) return '📄';
    if (fileType.includes('doc')) return '📝';
    if (fileType.includes('sheet') || fileType.includes('excel')) return '📊';
    return '📁';
  }

  // Check if file is an image
  isImage(fileType: string): boolean {
    return fileType.startsWith('image/');
  }

  // Get GPS coordinates if available
  getGPSCoordinates(file: FileSysFile): { lat: number; lng: number } | null {
    if (file.Latitude && file.Longitude) {
      return {
        lat: parseFloat(file.Latitude),
        lng: parseFloat(file.Longitude)
      };
    }
    return null;
  }
}

export default new FileSysService();