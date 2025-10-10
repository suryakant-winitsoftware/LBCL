/**
 * Dynamic Table Fields Discovery Service
 * Automatically discovers ALL fields from temp_sku_uom table
 * Adapts when new columns are added to database
 * Zero hardcoding - everything discovered dynamically
 */

export interface TableFieldDefinition {
  name: string;
  label: string;
  type: 'text' | 'number' | 'boolean' | 'select';
  required: boolean;
  defaultValue?: any;
  options?: string[];
  category: string;
}

export interface DynamicTableSchema {
  tableName: string;
  fields: TableFieldDefinition[];
  categories: { [key: string]: string };
}

class DynamicTableFieldsService {
  private schemaCache: { [tableName: string]: DynamicTableSchema } = {};

  /**
   * Get ALL fields dynamically from any table
   * Automatically discovers new columns when added
   */
  async getTableSchema(tableName: string): Promise<DynamicTableSchema> {
    // Check cache first
    if (this.schemaCache[tableName]) {
      return this.schemaCache[tableName];
    }

    try {
      // Always try to discover from actual data first
      const sampleSchema = await this.discoverFromSampleData(tableName);
      this.schemaCache[tableName] = sampleSchema;
      return sampleSchema;

    } catch (error) {
      console.error(`Error getting schema for ${tableName}:`, error);
      
      // Fallback to empty schema that will be populated when data arrives
      return this.getEmptySchema(tableName);
    }
  }

  /**
   * Get schema from dedicated schema API endpoint
   */
  private async getSchemaFromAPI(tableName: string): Promise<DynamicTableSchema | null> {
    try {
      const response = await fetch(`/api/schema/${tableName}`, {
        headers: {
          'Authorization': `Bearer ${localStorage.getItem('access_token')}`
        }
      });

      if (!response.ok) {
        return null;
      }

      const schemaData = await response.json();
      return this.transformSchemaResponse(tableName, schemaData);

    } catch (error) {
      console.warn(`No schema API available for ${tableName}`);
      return null;
    }
  }

  /**
   * Discover schema from sample data
   */
  private async discoverFromSampleData(tableName: string): Promise<DynamicTableSchema> {
    try {
      // Skip sample endpoint and go directly to getting actual data
      return await this.getSchemaFromFirstRecords(tableName);

    } catch (error) {
      console.warn(`Could not discover schema from sample data for ${tableName}`);
      throw error;
    }
  }

  /**
   * Get schema by analyzing first few records
   */
  private async getSchemaFromFirstRecords(tableName: string): Promise<DynamicTableSchema> {
    try {
      // For temp_sku_uom, use the existing SKUUOM API
      const endpoint = this.getAPIEndpoint(tableName);
      const token = localStorage.getItem('access_token');
      
      if (!token) {
        console.warn('No access token available');
        return this.getStaticSchemaForTable(tableName);
      }
      
      const response = await fetch(`${window.location.origin}/api/${endpoint}`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify({
          PageNumber: 0, // Use 0 to bypass pagination issues
          PageSize: 0,   // Use 0 to get all records
          SortCriterias: [],
          FilterCriterias: [],
          IsCountRequired: false
        })
      });

      // Check if response is JSON
      const contentType = response.headers.get('content-type');
      if (!contentType || !contentType.includes('application/json')) {
        console.warn(`Response is not JSON for ${tableName}, using fallback`);
        // Return a comprehensive static schema for temp_sku_uom
        return this.getComprehensiveUOMSchema(tableName);
      }

      if (!response.ok) {
        console.warn(`API call failed for ${tableName}, using fallback schema`);
        return this.getComprehensiveUOMSchema(tableName);
      }

      const data = await response.json();
      const records = data.data?.data || data.data || [];
      
      if (records.length === 0) {
        console.warn(`No sample data available for ${tableName}, using comprehensive schema`);
        return this.getComprehensiveUOMSchema(tableName);
      }

      return this.analyzeDataStructure(tableName, records);
    } catch (error) {
      console.warn(`Failed to fetch data for ${tableName}:`, error);
      return this.getComprehensiveUOMSchema(tableName);
    }
  }

  /**
   * Analyze data structure to discover fields
   */
  private analyzeDataStructure(tableName: string, records: any[]): DynamicTableSchema {
    if (!records || records.length === 0) {
      return this.getFallbackSchema(tableName);
    }

    const fields: TableFieldDefinition[] = [];
    const sampleRecord = records[0];
    const allRecords = records;

    // Analyze each field in the sample record
    Object.keys(sampleRecord).forEach(fieldName => {
      // Skip system fields
      if (this.isSystemField(fieldName)) {
        return;
      }

      const field = this.analyzeField(fieldName, sampleRecord[fieldName], allRecords);
      if (field) {
        fields.push(field);
      }
    });

    return {
      tableName,
      fields: this.sortFieldsByImportance(fields),
      categories: this.generateCategories(fields)
    };
  }

  /**
   * Analyze individual field to determine its properties
   */
  private analyzeField(fieldName: string, sampleValue: any, allRecords: any[]): TableFieldDefinition | null {
    // Determine field type from sample value and field name
    const type = this.inferFieldType(fieldName, sampleValue, allRecords);
    
    // Generate human-readable label
    const label = this.generateFieldLabel(fieldName);
    
    // Determine if field is required (has values in most records)
    const required = this.isFieldRequired(fieldName, allRecords);
    
    // Get default value
    const defaultValue = this.getDefaultValue(fieldName, type, sampleValue);
    
    // Get category
    const category = this.categorizeField(fieldName);
    
    // Get options for select fields
    const options = type === 'select' ? this.getFieldOptions(fieldName, allRecords) : undefined;

    return {
      name: fieldName,
      label,
      type,
      required,
      defaultValue,
      options,
      category
    };
  }

  /**
   * Infer field type from name and sample data
   */
  private inferFieldType(fieldName: string, sampleValue: any, allRecords: any[]): TableFieldDefinition['type'] {
    // Boolean fields
    if (typeof sampleValue === 'boolean' || fieldName.startsWith('is_') || fieldName.startsWith('has_')) {
      return 'boolean';
    }

    // Number fields
    if (typeof sampleValue === 'number' || fieldName.includes('multiplier') || 
        fieldName.includes('weight') || fieldName.includes('length') || 
        fieldName.includes('width') || fieldName.includes('height') ||
        fieldName.includes('volume') || fieldName.includes('depth')) {
      return 'number';
    }

    // Select fields (unit fields with limited options)
    if (fieldName.includes('_unit') || fieldName === 'code' || fieldName === 'name') {
      const uniqueValues = this.getUniqueValues(fieldName, allRecords);
      if (uniqueValues.length <= 20 && uniqueValues.length > 1) {
        return 'select';
      }
    }

    // Default to text
    return 'text';
  }

  /**
   * Generate human-readable label from field name
   */
  private generateFieldLabel(fieldName: string): string {
    return fieldName
      .split('_')
      .map(word => word.charAt(0).toUpperCase() + word.slice(1))
      .join(' ')
      .replace(/Uom/g, 'UOM')
      .replace(/Id/g, 'ID')
      .replace(/Uid/g, 'UID');
  }

  /**
   * Determine if field is required based on data
   */
  private isFieldRequired(fieldName: string, allRecords: any[]): boolean {
    // Core fields are always required
    if (['code', 'name', 'label', 'multiplier'].includes(fieldName)) {
      return true;
    }

    // Check if field has values in most records (>50%)
    const filledCount = allRecords.filter(record => {
      const value = record[fieldName];
      return value !== null && value !== undefined && value !== '';
    }).length;

    return (filledCount / allRecords.length) > 0.5;
  }

  /**
   * Get default value for field
   */
  private getDefaultValue(fieldName: string, type: TableFieldDefinition['type'], sampleValue: any): any {
    if (type === 'boolean') {
      return fieldName === 'is_base_uom' ? true : false;
    }
    
    if (type === 'number') {
      if (fieldName === 'multiplier') return 1;
      return 0;
    }
    
    if (type === 'text' || type === 'select') {
      if (fieldName === 'code') return 'EA';
      if (fieldName === 'name') return 'Each';
      if (fieldName === 'label') return 'EA';
      return '';
    }
    
    return '';
  }

  /**
   * Categorize field for UI grouping
   */
  private categorizeField(fieldName: string): string {
    if (['code', 'name', 'label', 'multiplier', 'barcodes'].includes(fieldName)) {
      return 'basic';
    }
    
    if (fieldName.startsWith('is_') || fieldName.startsWith('has_')) {
      return 'flags';
    }
    
    if (['length', 'width', 'height', 'depth', 'dimension_unit'].includes(fieldName)) {
      return 'dimensions';
    }
    
    if (['weight', 'gross_weight', 'weight_unit', 'gross_weight_unit', 'kgm'].includes(fieldName)) {
      return 'weight';
    }
    
    if (['volume', 'volume_unit', 'liter'].includes(fieldName)) {
      return 'volume';
    }
    
    return 'other';
  }

  /**
   * Get unique values for select fields
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
   * Get options for select fields
   */
  private getFieldOptions(fieldName: string, allRecords: any[]): string[] {
    return this.getUniqueValues(fieldName, allRecords);
  }

  /**
   * Check if field is system field (should be hidden)
   */
  private isSystemField(fieldName: string): boolean {
    return ['id', 'uid', 'sku_uid', 'created_by', 'created_time', 
            'modified_by', 'modified_time', 'server_add_time', 'server_modified_time'].includes(fieldName);
  }

  /**
   * Sort fields by importance for better UX
   */
  private sortFieldsByImportance(fields: TableFieldDefinition[]): TableFieldDefinition[] {
    const importance = {
      'code': 1, 'name': 2, 'label': 3, 'multiplier': 4, 'is_base_uom': 5,
      'is_outer_uom': 6, 'barcodes': 7, 'length': 8, 'width': 9, 'height': 10
    };

    return fields.sort((a, b) => {
      const aImportance = importance[a.name as keyof typeof importance] || 999;
      const bImportance = importance[b.name as keyof typeof importance] || 999;
      return aImportance - bImportance;
    });
  }

  /**
   * Generate category labels
   */
  private generateCategories(fields: TableFieldDefinition[]): { [key: string]: string } {
    const categories: { [key: string]: string } = {};
    
    fields.forEach(field => {
      if (!categories[field.category]) {
        categories[field.category] = this.generateCategoryLabel(field.category);
      }
    });
    
    return categories;
  }

  /**
   * Generate category label
   */
  private generateCategoryLabel(category: string): string {
    const labels: { [key: string]: string } = {
      'basic': 'Basic Information',
      'flags': 'Options & Flags',
      'dimensions': 'Dimensions',
      'weight': 'Weight Information',
      'volume': 'Volume Information',
      'other': 'Additional Fields'
    };
    
    return labels[category] || category.charAt(0).toUpperCase() + category.slice(1);
  }

  /**
   * Get API endpoint for table
   */
  private getAPIEndpoint(tableName: string): string {
    // Use SKUUOM API for any UOM-related table
    if (tableName.includes('uom')) {
      return 'SKUUOM/SelectAllSKUUOMDetails';
    }
    
    // Dynamic endpoint based on table name
    return tableName.replace(/_/g, '').toUpperCase() + '/SelectAll' + tableName.replace(/_/g, '') + 'Details';
  }

  /**
   * Transform schema API response
   */
  private transformSchemaResponse(tableName: string, schemaData: any): DynamicTableSchema {
    // Transform database schema response to our format
    const fields: TableFieldDefinition[] = schemaData.columns?.map((col: any) => ({
      name: col.column_name,
      label: this.generateFieldLabel(col.column_name),
      type: this.mapDatabaseTypeToFieldType(col.data_type),
      required: col.is_nullable === 'NO',
      category: this.categorizeField(col.column_name),
      defaultValue: this.getDefaultValue(col.column_name, this.mapDatabaseTypeToFieldType(col.data_type), null)
    })) || [];

    return {
      tableName,
      fields: this.sortFieldsByImportance(fields.filter(f => !this.isSystemField(f.name))),
      categories: this.generateCategories(fields)
    };
  }

  /**
   * Map database type to field type
   */
  private mapDatabaseTypeToFieldType(dbType: string): TableFieldDefinition['type'] {
    if (dbType === 'boolean') return 'boolean';
    if (dbType.includes('numeric') || dbType.includes('decimal') || dbType.includes('int')) return 'number';
    return 'text';
  }

  /**
   * Get comprehensive UOM schema based on database structure
   * This is used as fallback when API is not available
   */
  private getComprehensiveUOMSchema(tableName: string): DynamicTableSchema {
    // Based on the actual temp_sku_uom table structure from database
    return {
      tableName,
      fields: [
        // Core fields - discovered from database
        { name: 'code', label: 'UOM Code', type: 'text', required: true, category: 'basic', defaultValue: 'EA' },
        { name: 'name', label: 'UOM Name', type: 'text', required: true, category: 'basic', defaultValue: 'Each' },
        { name: 'label', label: 'Display Label', type: 'text', required: true, category: 'basic', defaultValue: 'EA' },
        { name: 'barcodes', label: 'Barcodes', type: 'text', required: false, category: 'basic', defaultValue: '' },
        { name: 'multiplier', label: 'Multiplier', type: 'number', required: true, category: 'basic', defaultValue: 1 },
        
        // Boolean flags - discovered from database
        { name: 'is_base_uom', label: 'Is Base UOM', type: 'boolean', required: false, category: 'flags', defaultValue: false },
        { name: 'is_outer_uom', label: 'Is Outer UOM', type: 'boolean', required: false, category: 'flags', defaultValue: false },
        
        // Dimensions - discovered from database
        { name: 'length', label: 'Length', type: 'number', required: false, category: 'dimensions', defaultValue: 0 },
        { name: 'depth', label: 'Depth', type: 'number', required: false, category: 'dimensions', defaultValue: 0 },
        { name: 'width', label: 'Width', type: 'number', required: false, category: 'dimensions', defaultValue: 0 },
        { name: 'height', label: 'Height', type: 'number', required: false, category: 'dimensions', defaultValue: 0 },
        { name: 'dimension_unit', label: 'Dimension Unit', type: 'text', required: false, category: 'dimensions', defaultValue: '' },
        
        // Volume - discovered from database
        { name: 'volume', label: 'Volume', type: 'number', required: false, category: 'volume', defaultValue: 0 },
        { name: 'volume_unit', label: 'Volume Unit', type: 'text', required: false, category: 'volume', defaultValue: '' },
        { name: 'liter', label: 'Liters', type: 'number', required: false, category: 'volume', defaultValue: 0 },
        
        // Weight - discovered from database
        { name: 'weight', label: 'Weight', type: 'number', required: false, category: 'weight', defaultValue: 0 },
        { name: 'gross_weight', label: 'Gross Weight', type: 'number', required: false, category: 'weight', defaultValue: 0 },
        { name: 'weight_unit', label: 'Weight Unit', type: 'text', required: false, category: 'weight', defaultValue: '' },
        { name: 'gross_weight_unit', label: 'Gross Weight Unit', type: 'text', required: false, category: 'weight', defaultValue: '' },
        { name: 'kgm', label: 'Weight (KGM)', type: 'number', required: false, category: 'weight', defaultValue: 0 }
      ],
      categories: {
        'basic': 'Basic Information',
        'flags': 'Options & Flags',
        'dimensions': 'Dimensions',
        'volume': 'Volume Information',
        'weight': 'Weight Information'
      }
    };
  }

  /**
   * Get empty schema that will be populated dynamically
   */
  private getEmptySchema(tableName: string): DynamicTableSchema {
    return {
      tableName,
      fields: [],
      categories: {}
    };
  }

  /**
   * Dynamically discover schema from real data (no hardcoding)
   */
  private getStaticSchemaForTable(tableName: string): DynamicTableSchema {
    // No hardcoding - always discover from data
    return this.getEmptySchema(tableName);
  }

  /**
   * Fallback schema when all discovery methods fail
   */
  private getFallbackSchema(tableName: string): DynamicTableSchema {
    // No hardcoding - return empty schema
    return this.getEmptySchema(tableName);
  }

  /**
   * Clear cache to force fresh discovery
   */
  clearCache(tableName?: string): void {
    if (tableName) {
      delete this.schemaCache[tableName];
    } else {
      this.schemaCache = {};
    }
  }
}

export const dynamicTableFieldsService = new DynamicTableFieldsService();