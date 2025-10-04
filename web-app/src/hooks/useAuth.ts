import { useState, useEffect } from 'react';
import { decodeToken, UserInfo } from '@/utils/auth';
import { authService } from '@/lib/auth-service';

/**
 * Custom hook for authentication
 * Gets the token from authService which manages localStorage
 */
export function useAuth() {
  const [token, setToken] = useState<string | null>(null);
  
  const [userInfo, setUserInfo] = useState<UserInfo | null>(null);
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const currentToken = authService.getToken();
    setToken(currentToken);
    
    if (currentToken) {
      const info = decodeToken(currentToken);
      if (info) {
        setUserInfo(info);
        setIsAuthenticated(true);
      } else {
        setIsAuthenticated(false);
      }
    } else {
      setIsAuthenticated(false);
    }
    setIsLoading(false);
  }, []);

  return {
    token,
    userInfo,
    isAuthenticated,
    isLoading,
    currentUser: userInfo?.username || 'SYSTEM',
    hasPermission: (permission: string) => userInfo?.permissions.includes(permission) || false,
    hasRole: (role: string) => userInfo?.role === role,
  };
}

/**
 * Hook to get just the current username for CreatedBy/ModifiedBy fields
 */
export function useCurrentUser(): string {
  const { currentUser } = useAuth();
  return currentUser;
}