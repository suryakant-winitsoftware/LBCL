import CryptoJS from 'crypto-js';
import { API_CONFIG, AUTH_ENDPOINTS, buildApiUrl } from './api-config';

class AuthService {
  private static instance: AuthService;

  private constructor() {}

  static getInstance(): AuthService {
    if (!AuthService.instance) {
      AuthService.instance = new AuthService();
    }
    return AuthService.instance;
  }

  generateChallengeCode(): string {
    const now = new Date();
    const year = now.getUTCFullYear();
    const month = String(now.getUTCMonth() + 1).padStart(2, '0');
    const day = String(now.getUTCDate()).padStart(2, '0');
    const hours = String(now.getUTCHours()).padStart(2, '0');
    const minutes = String(now.getUTCMinutes()).padStart(2, '0');
    const seconds = String(now.getUTCSeconds()).padStart(2, '0');
    
    return `${year}${month}${day}${hours}${minutes}${seconds}`;
  }

  hashPasswordWithSHA256(input: string): string {
    const hash = CryptoJS.SHA256(input);
    return CryptoJS.enc.Base64.stringify(hash);
  }

  encryptPasswordWithChallenge(password: string, challenge: string): string {
    const passwordWithChallenge = password + challenge;
    return this.hashPasswordWithSHA256(passwordWithChallenge);
  }

  generateDeviceId(): string {
    if (typeof window === 'undefined') {
      return 'server-side-device-id';
    }

    const existingDeviceId = localStorage.getItem('deviceId');
    if (existingDeviceId) {
      return existingDeviceId;
    }

    const userAgent = navigator.userAgent;
    const screenResolution = `${screen.width}x${screen.height}`;
    const timezone = Intl.DateTimeFormat().resolvedOptions().timeZone;
    const language = navigator.language;
    
    const deviceString = `${userAgent}-${screenResolution}-${timezone}-${language}-${Date.now()}`;
    const deviceId = this.hashPasswordWithSHA256(deviceString).substring(0, 32);
    
    localStorage.setItem('deviceId', deviceId);
    return deviceId;
  }

  async login(userId: string, password: string): Promise<{ success: boolean; data?: any; error?: string }> {
    try {
      const challengeCode = this.generateChallengeCode();
      const encryptedPassword = this.encryptPasswordWithChallenge(password, challengeCode);
      const deviceId = this.generateDeviceId();

      const requestBody = {
        userID: userId,
        password: encryptedPassword,
        challengeCode: challengeCode,
        deviceId: deviceId
      };

      const response = await fetch(buildApiUrl(AUTH_ENDPOINTS.getToken), {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(requestBody)
      });

      if (!response.ok) {
        const errorData = await response.text();
        throw new Error(errorData || 'Login failed');
      }

      const data = await response.json();
      
      if (typeof window !== 'undefined') {
        if (data.token || data.Token) {
          const token = data.token || data.Token;
          localStorage.setItem('authToken', token);
          localStorage.setItem('userId', userId);
          
          if (data.user || data.User) {
            localStorage.setItem('userData', JSON.stringify(data.user || data.User));
          }
        }
      }

      return {
        success: true,
        data: data
      };
    } catch (error: any) {
      console.error('Login error:', error);
      return {
        success: false,
        error: error.message || 'An error occurred during login'
      };
    }
  }

  logout(): void {
    if (typeof window !== 'undefined') {
      localStorage.removeItem('authToken');
      localStorage.removeItem('userId');
      localStorage.removeItem('userData');
    }
  }

  isAuthenticated(): boolean {
    if (typeof window === 'undefined') {
      return false;
    }
    return !!localStorage.getItem('authToken');
  }

  getToken(): string | null {
    if (typeof window === 'undefined') {
      return null;
    }
    
    // First try to get the stored auth token
    const authToken = localStorage.getItem('authToken');
    
    if (authToken && authToken !== 'null' && authToken !== 'undefined') {
      return authToken;
    }
    
    // Fallback to get token from user session
    const userData = localStorage.getItem('currentUser');
    
    if (userData) {
      try {
        const user = JSON.parse(userData);
        
        if (user.token && user.token !== 'null' && user.token !== 'undefined') {
          return user.token;
        }
      } catch (error) {
        console.error('Error parsing user data:', error);
      }
    }
    
    return null;
  }

  getAuthToken(): string | null {
    return this.getToken();
  }

  getUserData(): any | null {
    if (typeof window === 'undefined') {
      return null;
    }
    const userData = localStorage.getItem('userData');
    return userData ? JSON.parse(userData) : null;
  }

  getAuthHeaders(): Record<string, string> {
    const token = this.getToken();
    return token ? {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    } : {
      'Content-Type': 'application/json'
    };
  }
}

export const authService = AuthService.getInstance();
export default authService;