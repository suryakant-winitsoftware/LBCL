/**
 * API Response Transformer
 * 
 * This utility handles the case sensitivity issue where the backend returns
 * field names in different cases (camelCase vs PascalCase).
 * It normalizes the response to handle both cases automatically.
 */

/**
 * Transform API response to handle both uppercase and lowercase field names
 * This function creates a Proxy that intercepts property access and checks
 * for both PascalCase and camelCase versions of the property
 */
export function transformApiResponse<T = any>(data: any): T {
  if (data === null || data === undefined) {
    return data;
  }

  // Handle arrays
  if (Array.isArray(data)) {
    return data.map(item => transformApiResponse(item)) as any;
  }

  // Handle non-objects
  if (typeof data !== 'object') {
    return data;
  }

  // Create a proxy for the object that handles case-insensitive property access
  return new Proxy(data, {
    get(target, prop: string | symbol) {
      if (typeof prop === 'symbol') {
        return target[prop];
      }

      // First, try exact match
      if (prop in target) {
        const value = target[prop];
        // Recursively transform nested objects and arrays
        if (value !== null && typeof value === 'object') {
          return transformApiResponse(value);
        }
        return value;
      }

      // Try lowercase version
      const lowerProp = prop.charAt(0).toLowerCase() + prop.slice(1);
      if (lowerProp in target) {
        const value = target[lowerProp];
        if (value !== null && typeof value === 'object') {
          return transformApiResponse(value);
        }
        return value;
      }

      // Try uppercase version
      const upperProp = prop.charAt(0).toUpperCase() + prop.slice(1);
      if (upperProp in target) {
        const value = target[upperProp];
        if (value !== null && typeof value === 'object') {
          return transformApiResponse(value);
        }
        return value;
      }

      // Check for common variations
      const variations = [
        prop.toLowerCase(),
        prop.toUpperCase(),
        // Handle snake_case to camelCase
        prop.replace(/_([a-z])/g, (_, letter) => letter.toUpperCase()),
        // Handle camelCase to snake_case
        prop.replace(/[A-Z]/g, letter => `_${letter.toLowerCase()}`),
      ];

      for (const variant of variations) {
        if (variant in target) {
          const value = target[variant];
          if (value !== null && typeof value === 'object') {
            return transformApiResponse(value);
          }
          return value;
        }
      }

      // Return undefined if not found
      return undefined;
    },
    
    // Handle 'in' operator
    has(target, prop: string | symbol) {
      if (typeof prop === 'symbol') {
        return prop in target;
      }
      
      return prop in target || 
             (prop.charAt(0).toLowerCase() + prop.slice(1)) in target ||
             (prop.charAt(0).toUpperCase() + prop.slice(1)) in target;
    }
  }) as T;
}

/**
 * Transform paged response data
 * Handles both PagedData/pagedData field names
 */
export function transformPagedResponse(response: any): any {
  if (!response || !response.data) {
    return response;
  }

  const data = response.data;
  
  // Check for paged data in various formats
  const pagedData = data.PagedData || data.pagedData || data.pageddata || data.PAGEDDATA;
  const totalCount = data.TotalCount || data.totalCount || data.totalcount || data.TOTALCOUNT;
  
  if (pagedData !== undefined) {
    return {
      ...response,
      data: {
        ...data,
        PagedData: pagedData,
        pagedData: pagedData,
        TotalCount: totalCount,
        totalCount: totalCount
      }
    };
  }
  
  // For non-paged responses, just transform the data
  return {
    ...response,
    data: transformApiResponse(data)
  };
}

/**
 * Normalize field names in an object
 * This creates a new object with both PascalCase and camelCase versions of all fields
 */
export function normalizeFieldNames(obj: any): any {
  if (!obj || typeof obj !== 'object') {
    return obj;
  }

  if (Array.isArray(obj)) {
    return obj.map(normalizeFieldNames);
  }

  const normalized: any = {};
  
  for (const key in obj) {
    if (obj.hasOwnProperty(key)) {
      const value = obj[key];
      
      // Add the original key
      normalized[key] = value;
      
      // Add camelCase version
      const camelKey = key.charAt(0).toLowerCase() + key.slice(1);
      if (camelKey !== key) {
        normalized[camelKey] = value;
      }
      
      // Add PascalCase version
      const pascalKey = key.charAt(0).toUpperCase() + key.slice(1);
      if (pascalKey !== key) {
        normalized[pascalKey] = value;
      }
    }
  }
  
  return normalized;
}