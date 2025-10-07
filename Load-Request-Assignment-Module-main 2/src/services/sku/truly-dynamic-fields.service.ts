/**
 * Truly Dynamic Field Discovery Service
 * NO HARDCODING - Everything discovered from API responses
 */

export interface DynamicField {
  name: string;
  label: string;
  type: 'text' | 'number' | 'boolean' | 'select' | 'date';
  value: any;
  required: boolean;
  options?: string[];
}

class TrulyDynamicFieldsService {
  /**
   * Discover ALL fields from API response - NO HARDCODING
   */
  async discoverFieldsFromAPI(endpoint: string): Promise<DynamicField[]> {
    try {
      const token = localStorage.getItem('access_token');
      
      // Call the backend API endpoint
      const response = await fetch(`${window.location.origin}/api/${endpoint}`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': token ? `Bearer ${token}` : ''
        },
        body: JSON.stringify({
          PageNumber: 0,
          PageSize: 0,
          SortCriterias: [],
          FilterCriterias: [],
          IsCountRequired: false
        })
      });

      // Check if response is JSON
      const contentType = response.headers.get('content-type');
      if (!contentType || !contentType.includes('application/json')) {
        console.warn('Response is not JSON');
        return [];
      }

      const result = await response.json();
      
      // Debug log to see what we're getting
      console.log('Backend API Response:', result);
      
      // Extract records from PagedResponse format
      // Backend returns: { Data: { Data: [...], TotalCount: n }, Success: true }
      let records: any[] = [];
      
      if (result.Data?.Data) {
        records = result.Data.Data; // Backend PagedResponse format
      } else if (result.data?.data) {
        records = result.data.data; // Alternative format
      } else if (result.Data && Array.isArray(result.Data)) {
        records = result.Data; // Direct array
      } else if (result.data && Array.isArray(result.data)) {
        records = result.data; // Direct array
      } else if (Array.isArray(result)) {
        records = result; // Response is the array itself
      }
      
      console.log('Extracted records:', records);
      
      if (!Array.isArray(records) || records.length === 0) {
        console.warn('No records found in response. Response structure:', result);
        return [];
      }

      // Discover fields from the first record
      return this.discoverFieldsFromRecord(records[0], records);

    } catch (error) {
      console.error('Error discovering fields:', error);
      return [];
    }
  }

  /**
   * Discover fields from a record - COMPLETELY DYNAMIC
   */
  private discoverFieldsFromRecord(record: any, allRecords: any[]): DynamicField[] {
    if (!record || typeof record !== 'object') {
      return [];
    }

    const fields: DynamicField[] = [];
    
    // Discover ALL fields from the record
    Object.keys(record).forEach(fieldName => {
      // Skip system fields
      if (this.isSystemField(fieldName)) {
        return;
      }

      const value = record[fieldName];
      const field: DynamicField = {
        name: fieldName,
        label: this.generateLabel(fieldName),
        type: this.detectFieldType(fieldName, value, allRecords),
        value: this.getDefaultValue(value),
        required: this.isRequired(fieldName, allRecords),
        options: this.getOptions(fieldName, allRecords)
      };

      fields.push(field);
    });

    return fields;
  }

  /**
   * Detect field type from value - NO HARDCODING
   */
  private detectFieldType(fieldName: string, value: any, allRecords: any[]): DynamicField['type'] {
    // Check actual value type
    if (typeof value === 'boolean') {
      return 'boolean';
    }

    if (typeof value === 'number' || !isNaN(Number(value))) {
      return 'number';
    }

    // Check if it's a date
    if (this.isDate(value)) {
      return 'date';
    }

    // Check if field has limited unique values (for select)
    const uniqueValues = this.getUniqueValues(fieldName, allRecords);
    if (uniqueValues.length > 1 && uniqueValues.length <= 10) {
      return 'select';
    }

    // Default to text
    return 'text';
  }

  /**
   * Generate human-readable label from field name
   */
  private generateLabel(fieldName: string): string {
    return fieldName
      .replace(/_/g, ' ')
      .replace(/([A-Z])/g, ' $1')
      .trim()
      .split(' ')
      .map(word => word.charAt(0).toUpperCase() + word.slice(1).toLowerCase())
      .join(' ');
  }

  /**
   * Get default value based on type
   */
  private getDefaultValue(value: any): any {
    if (value === null || value === undefined) {
      return '';
    }
    return value;
  }

  /**
   * Check if field is required based on data
   */
  private isRequired(fieldName: string, allRecords: any[]): boolean {
    // Check if field has values in most records
    const filledCount = allRecords.filter(record => {
      const val = record[fieldName];
      return val !== null && val !== undefined && val !== '';
    }).length;

    return (filledCount / allRecords.length) > 0.8; // 80% threshold
  }

  /**
   * Get options for select fields
   */
  private getOptions(fieldName: string, allRecords: any[]): string[] | undefined {
    const uniqueValues = this.getUniqueValues(fieldName, allRecords);
    
    if (uniqueValues.length > 1 && uniqueValues.length <= 10) {
      return uniqueValues;
    }
    
    return undefined;
  }

  /**
   * Get unique values for a field
   */
  private getUniqueValues(fieldName: string, allRecords: any[]): string[] {
    const values = new Set<string>();
    
    allRecords.forEach(record => {
      const value = record[fieldName];
      if (value !== null && value !== undefined && value !== '') {
        values.add(String(value));
      }
    });
    
    return Array.from(values).sort();
  }

  /**
   * Check if value is a date
   */
  private isDate(value: any): boolean {
    if (!value) return false;
    const date = new Date(value);
    return date instanceof Date && !isNaN(date.getTime()) && value.includes('-');
  }

  /**
   * Check if field is system field
   */
  private isSystemField(fieldName: string): boolean {
    const systemFields = [
      'Id', 'ID',
      'UID',
      'CreatedBy', 'ModifiedBy',
      'CreatedTime', 'ModifiedTime',
      'ServerAddTime', 'ServerModifiedTime',
      'IsDeleted', 'IsActive',
      'OrgUID', 'DistributionChannelOrgUID'
    ];
    
    return systemFields.some(sf => fieldName === sf);
  }
}

export const trulyDynamicFieldsService = new TrulyDynamicFieldsService();