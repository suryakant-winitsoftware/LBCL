import { CreateProductWithAttributesRequest, ProductAttribute } from '@/types/product.types';

export interface ValidationError {
  field: string;
  message: string;
}

export interface ValidationResult {
  isValid: boolean;
  errors: ValidationError[];
}

/**
 * Validates product code format
 */
export function validateProductCode(code: string): ValidationResult {
  const errors: ValidationError[] = [];
  
  if (!code || code.trim().length === 0) {
    errors.push({ field: 'productCode', message: 'Product code is required' });
  } else {
    // Check length
    if (code.length < 3) {
      errors.push({ field: 'productCode', message: 'Product code must be at least 3 characters' });
    }
    if (code.length > 50) {
      errors.push({ field: 'productCode', message: 'Product code must not exceed 50 characters' });
    }
    
    // Check format (alphanumeric with underscores and hyphens)
    const codePattern = /^[A-Z0-9_-]+$/;
    if (!codePattern.test(code)) {
      errors.push({ 
        field: 'productCode', 
        message: 'Product code must contain only uppercase letters, numbers, underscores, and hyphens' 
      });
    }
    
    // Check for spaces
    if (code.includes(' ')) {
      errors.push({ field: 'productCode', message: 'Product code cannot contain spaces' });
    }
  }
  
  return {
    isValid: errors.length === 0,
    errors
  };
}

/**
 * Validates product name
 */
export function validateProductName(name: string): ValidationResult {
  const errors: ValidationError[] = [];
  
  if (!name || name.trim().length === 0) {
    errors.push({ field: 'productName', message: 'Product name is required' });
  } else {
    if (name.length < 2) {
      errors.push({ field: 'productName', message: 'Product name must be at least 2 characters' });
    }
    if (name.length > 200) {
      errors.push({ field: 'productName', message: 'Product name must not exceed 200 characters' });
    }
  }
  
  return {
    isValid: errors.length === 0,
    errors
  };
}

/**
 * Validates product attributes
 */
export function validateProductAttributes(attributes: Omit<ProductAttribute, 'ProductCode' | 'UID' | 'CreatedTime' | 'ModifiedTime'>[]): ValidationResult {
  const errors: ValidationError[] = [];
  
  if (!attributes || attributes.length === 0) {
    errors.push({ field: 'attributes', message: 'At least one attribute is required' });
    return { isValid: false, errors };
  }
  
  // Check each attribute
  attributes.forEach((attr, index) => {
    if (!attr.HierarchyType || attr.HierarchyType.trim().length === 0) {
      errors.push({ 
        field: `attributes[${index}].HierarchyType`, 
        message: `Attribute ${index + 1}: Hierarchy type is required` 
      });
    }
    
    if (!attr.HierarchyCode || attr.HierarchyCode.trim().length === 0) {
      errors.push({ 
        field: `attributes[${index}].HierarchyCode`, 
        message: `Attribute ${index + 1}: Hierarchy code is required` 
      });
    } else if (attr.HierarchyCode.length > 20) {
      errors.push({ 
        field: `attributes[${index}].HierarchyCode`, 
        message: `Attribute ${index + 1}: Hierarchy code must not exceed 20 characters` 
      });
    }
    
    if (!attr.HierarchyValue || attr.HierarchyValue.trim().length === 0) {
      errors.push({ 
        field: `attributes[${index}].HierarchyValue`, 
        message: `Attribute ${index + 1}: Hierarchy value is required` 
      });
    } else if (attr.HierarchyValue.length > 100) {
      errors.push({ 
        field: `attributes[${index}].HierarchyValue`, 
        message: `Attribute ${index + 1}: Hierarchy value must not exceed 100 characters` 
      });
    }
  });
  
  // Check for duplicate hierarchy type + code combinations
  const combinations = new Set<string>();
  const duplicates: string[] = [];
  
  attributes.forEach((attr, index) => {
    const key = `${attr.HierarchyType}:${attr.HierarchyCode}`;
    if (combinations.has(key)) {
      duplicates.push(`Attribute ${index + 1}`);
    }
    combinations.add(key);
  });
  
  if (duplicates.length > 0) {
    errors.push({ 
      field: 'attributes', 
      message: `Duplicate hierarchy type and code combination found in: ${duplicates.join(', ')}` 
    });
  }
  
  return {
    isValid: errors.length === 0,
    errors
  };
}

/**
 * Validates date range
 */
export function validateDateRange(fromDate: string, toDate: string): ValidationResult {
  const errors: ValidationError[] = [];
  
  if (!fromDate) {
    errors.push({ field: 'fromDate', message: 'From date is required' });
  }
  
  if (!toDate) {
    errors.push({ field: 'toDate', message: 'To date is required' });
  }
  
  if (fromDate && toDate) {
    const from = new Date(fromDate);
    const to = new Date(toDate);
    
    if (from > to) {
      errors.push({ field: 'dateRange', message: 'From date must be before or equal to To date' });
    }
    
    // Check if dates are valid
    if (isNaN(from.getTime())) {
      errors.push({ field: 'fromDate', message: 'Invalid from date format' });
    }
    if (isNaN(to.getTime())) {
      errors.push({ field: 'toDate', message: 'Invalid to date format' });
    }
  }
  
  return {
    isValid: errors.length === 0,
    errors
  };
}

/**
 * Validates UOM
 */
export function validateUOM(uom: string): ValidationResult {
  const errors: ValidationError[] = [];
  const validUOMs = ['EA', 'KG', 'G', 'L', 'ML', 'PCS', 'BOX', 'CASE', 'PALLET'];
  
  if (!uom || uom.trim().length === 0) {
    errors.push({ field: 'baseUOM', message: 'Base UOM is required' });
  } else if (!validUOMs.includes(uom)) {
    errors.push({ field: 'baseUOM', message: `Invalid UOM. Must be one of: ${validUOMs.join(', ')}` });
  }
  
  return {
    isValid: errors.length === 0,
    errors
  };
}

/**
 * Comprehensive validation for product creation request
 */
export function validateProductCreationRequest(request: CreateProductWithAttributesRequest): ValidationResult {
  const allErrors: ValidationError[] = [];
  
  // Validate product code
  const codeValidation = validateProductCode(request.ProductCode);
  allErrors.push(...codeValidation.errors);
  
  // Validate product name
  const nameValidation = validateProductName(request.ProductName);
  allErrors.push(...nameValidation.errors);
  
  // Validate attributes
  const attrValidation = validateProductAttributes(request.Attributes);
  allErrors.push(...attrValidation.errors);
  
  // Validate date range
  const dateValidation = validateDateRange(request.FromDate, request.ToDate);
  allErrors.push(...dateValidation.errors);
  
  // Validate UOM
  const uomValidation = validateUOM(request.BaseUOM);
  allErrors.push(...uomValidation.errors);
  
  // Validate org UID
  if (!request.OrgUID || request.OrgUID.trim().length === 0) {
    allErrors.push({ field: 'orgUID', message: 'Organization UID is required' });
  }
  
  return {
    isValid: allErrors.length === 0,
    errors: allErrors
  };
}

/**
 * Sanitizes product code to ensure it meets requirements
 */
export function sanitizeProductCode(code: string): string {
  return code
    .toUpperCase()
    .replace(/[^A-Z0-9_-]/g, '') // Remove invalid characters
    .replace(/\s+/g, '_') // Replace spaces with underscores
    .substring(0, 50); // Limit length
}

/**
 * Formats error messages for display
 */
export function formatValidationErrors(errors: ValidationError[]): string {
  if (errors.length === 0) return '';
  
  if (errors.length === 1) {
    return errors[0].message;
  }
  
  return `Multiple validation errors:\n${errors.map(e => `• ${e.message}`).join('\n')}`;
}

/**
 * Gets field-specific errors
 */
export function getFieldErrors(errors: ValidationError[], field: string): string[] {
  return errors
    .filter(e => e.field === field || e.field.startsWith(`${field}.`) || e.field.startsWith(`${field}[`))
    .map(e => e.message);
}