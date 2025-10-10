// JWT token utilities

interface TokenPayload {
  'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name': string;
  'http://schemas.microsoft.com/ws/2008/06/identity/claims/userdata': string;
  'http://schemas.microsoft.com/ws/2008/06/identity/claims/role': string;
  permissions: string[];
  exp: number;
  iss: string;
}

export interface UserInfo {
  username: string;
  userData: string;
  role: string;
  permissions: string[];
  expiresAt: Date;
}

/**
 * Decode a JWT token and extract user information
 * @param token - JWT token string
 * @returns UserInfo object or null if invalid
 */
export function decodeToken(token: string): UserInfo | null {
  try {
    // Split token parts
    const parts = token.split('.');
    if (parts.length !== 3) {
      console.error('Invalid token format');
      return null;
    }

    // Decode payload
    const payload = JSON.parse(atob(parts[1])) as TokenPayload;

    // Check if token is expired
    const expiresAt = new Date(payload.exp * 1000);
    if (expiresAt < new Date()) {
      console.error('Token is expired');
      return null;
    }

    return {
      username: payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'],
      userData: payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/userdata'],
      role: payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'],
      permissions: payload.permissions,
      expiresAt,
    };
  } catch (error) {
    console.error('Error decoding token:', error);
    return null;
  }
}

/**
 * Get current user from token
 * @param token - JWT token string
 * @returns Username for CreatedBy/ModifiedBy fields
 */
export function getCurrentUser(token: string): string {
  const userInfo = decodeToken(token);
  return userInfo?.username || 'SYSTEM';
}

/**
 * Check if user has specific permission
 * @param token - JWT token string
 * @param permission - Permission to check (e.g., 'read', 'write')
 * @returns Boolean indicating if user has permission
 */
export function hasPermission(token: string, permission: string): boolean {
  const userInfo = decodeToken(token);
  return userInfo?.permissions.includes(permission) || false;
}

/**
 * Check if user has specific role
 * @param token - JWT token string
 * @param role - Role to check (e.g., 'Admin', 'User')
 * @returns Boolean indicating if user has role
 */
export function hasRole(token: string, role: string): boolean {
  const userInfo = decodeToken(token);
  return userInfo?.role === role;
}