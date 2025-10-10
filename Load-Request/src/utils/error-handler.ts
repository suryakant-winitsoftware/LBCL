// Production-ready error handler for API calls
export interface ApiError {
  message: string;
  status: number;
  details?: any;
  endpoint?: string;
  method?: string;
}

export class ErrorHandler {
  static handleApiError(error: any, context?: string): ApiError {
    console.error(`API Error${context ? ` (${context})` : ''}:`, error);
    
    // If it's already our custom error format
    if (error.status && error.message) {
      return {
        message: error.message,
        status: error.status,
        details: error.details,
        endpoint: error.endpoint,
        method: error.method,
      };
    }

    // Handle different error types
    if (error.name === 'AbortError') {
      return {
        message: 'Request timeout - please try again',
        status: 408,
      };
    }

    if (error.message && error.message.includes('Failed to fetch')) {
      return {
        message: 'Unable to connect to server. Please check if the backend is running on localhost:8000',
        status: 0,
      };
    }

    if (error.message && error.message.includes('HTTP error')) {
      const statusMatch = error.message.match(/status: (\d+)/);
      const status = statusMatch ? parseInt(statusMatch[1]) : 500;
      
      return {
        message: this.getStatusMessage(status),
        status,
        details: error.details,
      };
    }

    // Default error
    return {
      message: error.message || 'An unexpected error occurred',
      status: 500,
    };
  }

  static getStatusMessage(status: number): string {
    switch (status) {
      case 400:
        return 'Invalid request data';
      case 401:
        return 'Authentication required - please login again';
      case 403:
        return 'Access denied - insufficient permissions';
      case 404:
        return 'API endpoint not found - this feature may not be available';
      case 409: 
        return 'Data conflict - the resource may have been modified';
      case 422:
        return 'Invalid data provided';
      case 429:
        return 'Too many requests - please try again later';
      case 500:
        return 'Server error - please try again or contact support';
      case 502:
        return 'Bad gateway - server is temporarily unavailable';
      case 503:
        return 'Service unavailable - server is under maintenance';
      case 504:
        return 'Gateway timeout - server is taking too long to respond';
      default:
        return `Server returned error ${status}`;
    }
  }

  static getRetryableStatus(): number[] {
    return [408, 429, 500, 502, 503, 504];
  }

  static isRetryable(error: ApiError): boolean {
    return this.getRetryableStatus().includes(error.status);
  }

  static getUserFriendlyMessage(error: ApiError): string {
    // Provide more user-friendly messages for common scenarios
    if (error.status === 404 && error.endpoint?.includes('Journey')) {
      return 'Journey plan data is not available. This feature may not be implemented in the backend yet.';
    }

    if (error.status === 404 && error.endpoint?.includes('UserJourney')) {
      return 'User journey data is not available. The system will use alternative data sources.';
    }

    if (error.status === 500 && error.message.includes('BeatHistory')) {
      return 'Unable to load journey data. Please check if the data exists or try again later.';
    }

    return error.message;
  }
}