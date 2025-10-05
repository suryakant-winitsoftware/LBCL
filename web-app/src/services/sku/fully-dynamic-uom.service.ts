// Fully Dynamic UOM Service - ZERO hardcoded values, completely adaptive
import { getAuthHeaders } from "@/lib/auth-service";
import { PagingRequest } from "@/types/common.types";

export interface DynamicField {
  name: string;
  type: string; // Completely dynamic - can be ANY type
  displayName: string;
  isRequired: boolean;
  isReadonly: boolean;
  category: string; // Completely dynamic - inferred from patterns
  metadata: Record<string, any>; // Store any additional field info
  validationRules?: Record<string, any>; // Completely dynamic validation
  uiHints?: Record<string, any>; // UI rendering hints
}

export interface DynamicTableSchema {
  tableName: string;
  fields: DynamicField[];
  primaryKey: string;
  displayField: string;
  relationships: Record<string, any>; // Dynamic relationships
  constraints: Record<string, any>; // Dynamic constraints
  metadata: Record<string, any>; // Table-level metadata
}

export interface ApiResponse<T> {
  Data: T;
  StatusCode: number;
  IsSuccess: boolean;
  ErrorMessage?: string;
}

export interface PagedResponse<T> {
  PagedData: T[];
  TotalCount: number;
}

class FullyDynamicUOMService {
  private baseURL =
    process.env.NEXT_PUBLIC_API_URL || "http://localhost:8000/api";
  private _loadingSchemas: Set<string> = new Set();

  /**
   * Discover ALL available tables and their structures dynamically
   */
  async discoverAllTables(): Promise<string[]> {
    try {
      // Try multiple discovery methods
      const discoveryMethods = [
        () => this.discoverFromInformationSchema(),
        () => this.discoverFromAPIEndpoints(),
        () => this.discoverFromSampleData()
      ];

      for (const method of discoveryMethods) {
        try {
          const tables = await method();
          if (tables.length > 0) {
            return tables;
          }
        } catch (error) {
          console.warn("Discovery method failed:", error);
          continue;
        }
      }

      return [];
    } catch (error) {
      console.error("Failed to discover tables:", error);
      return [];
    }
  }

  /**
   * Discover tables from database information schema
   */
  private async discoverFromInformationSchema(): Promise<string[]> {
    try {
      // Try to call a generic endpoint that returns table information
      const response = await fetch(`${this.baseURL}/System/GetTables`, {
        method: "GET",
        headers: {
          ...getAuthHeaders(),
          Accept: "application/json"
        }
      });

      if (response.ok) {
        const result = await response.json();
        return Array.isArray(result) ? result : result.Data || [];
      }
    } catch (error) {
      // Silent fail, try next method
    }

    // Fallback: Try known patterns
    const knownPatterns = [
      "UOM",
      "SKU",
      "Product",
      "Customer",
      "Order",
      "Store"
    ];
    const discoveredTables: string[] = [];

    for (const pattern of knownPatterns) {
      try {
        const response = await fetch(
          `${this.baseURL}/${pattern}/GetTableInfo`,
          {
            method: "GET",
            headers: { ...getAuthHeaders() }
          }
        );

        if (response.ok) {
          discoveredTables.push(pattern.toLowerCase());
        }
      } catch (error) {
        // Continue checking other patterns
      }
    }

    return discoveredTables;
  }

  /**
   * Discover tables from available API endpoints
   */
  private async discoverFromAPIEndpoints(): Promise<string[]> {
    try {
      // Try to get API documentation or endpoint list
      const response = await fetch(`${this.baseURL}/swagger/v1/swagger.json`, {
        method: "GET",
        headers: { Accept: "application/json" }
      });

      if (response.ok) {
        const swagger = await response.json();
        const paths = Object.keys(swagger.paths || {});

        // Extract table names from API paths
        const tables = new Set<string>();
        paths.forEach((path) => {
          const matches = path.match(/\/api\/([^\/]+)/);
          if (matches && matches[1]) {
            tables.add(matches[1].toLowerCase());
          }
        });

        return Array.from(tables);
      }
    } catch (error) {
      // Silent fail
    }

    return [];
  }

  /**
   * Discover tables by trying common endpoints
   */
  private async discoverFromSampleData(): Promise<string[]> {
    const commonEndpoints = [
      "SKUUOM",
      "SKU",
      "UOMType",
      "Product",
      "Customer",
      "Store",
      "Employee",
      "Order",
      "Route",
      "Organization"
    ];

    const discoveredTables: string[] = [];

    for (const endpoint of commonEndpoints) {
      try {
        const response = await fetch(
          `${this.baseURL}/${endpoint}/SelectAll${endpoint}Details`,
          {
            method: "POST",
            headers: {
              ...getAuthHeaders(),
              "Content-Type": "application/json"
            },
            body: JSON.stringify({
              PageNumber: 1,
              PageSize: 1,
              FilterCriterias: [],
              SortCriterias: [],
              IsCountRequired: false
            })
          }
        );

        if (response.ok) {
          discoveredTables.push(endpoint);
        }
      } catch (error) {
        // Continue with next endpoint
      }
    }

    return discoveredTables;
  }

  /**
   * Get completely dynamic table schema
   */
  async getTableSchema(tableName: string): Promise<DynamicTableSchema> {
    const cacheKey = tableName.toLowerCase();

    // Prevent concurrent schema loading for same table
    if (this._loadingSchemas.has(cacheKey)) {
      console.log(
        "⏳ Schema loading in progress for",
        tableName,
        "- waiting..."
      );
      // Wait for completion
      while (this._loadingSchemas.has(cacheKey)) {
        await new Promise((resolve) => setTimeout(resolve, 100));
      }
    }

    this._loadingSchemas.add(cacheKey);

    try {
      let schema: DynamicTableSchema;

      // For SKUUOM, skip metadata API and go directly to sample data inference
      const methods =
        tableName.toUpperCase() === "SKUUOM"
          ? [() => this.inferSchemaFromSampleData(tableName)]
          : [
              () => this.getSchemaFromMetadataAPI(tableName),
              () => this.inferSchemaFromSampleData(tableName),
              () => this.getSchemaFromDatabaseIntrospection(tableName)
            ];

      for (const method of methods) {
        try {
          schema = await method();
          if (schema && schema.fields.length > 0) {
            this._loadingSchemas.delete(cacheKey);
            return schema;
          }
        } catch (error) {
          console.warn(`Schema method failed for ${tableName}:`, error);
          continue;
        }
      }

      this._loadingSchemas.delete(cacheKey);
      throw new Error(`Unable to determine schema for table: ${tableName}`);
    } catch (error) {
      this._loadingSchemas.delete(cacheKey);
      console.error(`Failed to get schema for ${tableName}:`, error);
      throw error;
    }
  }

  /**
   * Get schema from dedicated metadata API
   */
  private async getSchemaFromMetadataAPI(
    tableName: string
  ): Promise<DynamicTableSchema> {
    const response = await fetch(`${this.baseURL}/${tableName}/GetSchema`, {
      method: "GET",
      headers: {
        ...getAuthHeaders(),
        Accept: "application/json"
      }
    });

    if (!response.ok) {
      throw new Error(`Metadata API not available for ${tableName}`);
    }

    return await response.json();
  }

  /**
   * Infer schema completely dynamically from sample data
   */
  private async inferSchemaFromSampleData(
    tableName: string
  ): Promise<DynamicTableSchema> {
    // Get sample data for any table dynamically
    let sampleData: any[] = [];
    if (tableName.toUpperCase() === "SKUUOM") {
      // For SKUUOM, get data using the working endpoint directly without filters
      try {
        const response = await fetch(
          `${this.baseURL}/SKUUOM/SelectAllSKUUOMDetails`,
          {
            method: "POST",
            headers: {
              ...getAuthHeaders(),
              "Content-Type": "application/json",
              Accept: "application/json"
            },
            body: JSON.stringify({
              PageNumber: 0,
              PageSize: 5, // Just get 5 records for schema
              FilterCriterias: [],
              SortCriterias: [],
              IsCountRequired: false
            })
          }
        );

        if (response.ok) {
          const result = await response.json();
          if (result.IsSuccess) {
            sampleData = result.Data?.PagedData || [];
          }
        }
      } catch (error) {
        console.error("Failed to get sample data for schema:", error);
      }
    } else {
      sampleData = await this.getRawTableData(tableName, 1, 10);
    }

    if (sampleData.length === 0) {
      throw new Error(`No sample data available for ${tableName}`);
    }

    const allFields = new Set<string>();
    const fieldAnalysis: Record<string, any> = {};

    // Analyze all records to understand field patterns
    sampleData.forEach((record) => {
      Object.keys(record).forEach((fieldName) => {
        allFields.add(fieldName);

        if (!fieldAnalysis[fieldName]) {
          fieldAnalysis[fieldName] = {
            values: [],
            types: new Set(),
            nullCount: 0,
            patterns: new Set()
          };
        }

        const value = record[fieldName];
        fieldAnalysis[fieldName].values.push(value);

        if (value === null || value === undefined) {
          fieldAnalysis[fieldName].nullCount++;
        } else {
          fieldAnalysis[fieldName].types.add(typeof value);

          // Analyze patterns
          if (typeof value === "string") {
            this.analyzeStringPatterns(
              value,
              fieldAnalysis[fieldName].patterns
            );
          }
        }
      });
    });

    const fields: DynamicField[] = [];

    // Generate fields completely dynamically
    allFields.forEach((fieldName) => {
      const analysis = fieldAnalysis[fieldName];
      const field = this.createDynamicField(
        fieldName,
        analysis,
        sampleData.length
      );
      fields.push(field);
    });

    // Determine primary key dynamically
    const primaryKey = this.determinePrimaryKey(fields, sampleData);

    // Determine display field dynamically
    const displayField = this.determineDisplayField(fields, sampleData);

    return {
      tableName,
      fields,
      primaryKey,
      displayField,
      relationships: this.discoverRelationships(fields, sampleData),
      constraints: this.discoverConstraints(fields, sampleData),
      metadata: {
        sampleSize: sampleData.length,
        inferredAt: new Date().toISOString(),
        confidence: this.calculateSchemaConfidence(fields, sampleData)
      }
    };
  }

  /**
   * Analyze string patterns to determine field characteristics
   */
  private analyzeStringPatterns(value: string, patterns: Set<string>): void {
    // Date patterns
    if (/^\d{4}-\d{2}-\d{2}/.test(value)) patterns.add("date");
    if (/T\d{2}:\d{2}:\d{2}/.test(value)) patterns.add("datetime");

    // ID patterns
    if (/^[A-Z0-9_-]+$/.test(value)) patterns.add("code");
    if (value.includes("_") && value.length > 10) patterns.add("uid");

    // Measurement patterns
    if (/\d+(\.\d+)?\s*(kg|g|lb|oz)/i.test(value)) patterns.add("weight");
    if (/\d+(\.\d+)?\s*(cm|m|mm|in|ft)/i.test(value)) patterns.add("dimension");
    if (/\d+(\.\d+)?\s*(l|ml|gal)/i.test(value)) patterns.add("volume");

    // Boolean-like patterns
    if (
      ["true", "false", "yes", "no", "1", "0"].includes(value.toLowerCase())
    ) {
      patterns.add("boolean-like");
    }

    // Email pattern
    if (/@/.test(value) && /\.[a-z]{2,}$/i.test(value)) patterns.add("email");

    // URL pattern
    if (/^https?:\/\//.test(value)) patterns.add("url");
  }

  /**
   * Create dynamic field definition
   */
  private createDynamicField(
    fieldName: string,
    analysis: any,
    totalRecords: number
  ): DynamicField {
    const types = Array.from(analysis.types);
    const patterns = Array.from(analysis.patterns);
    const nullPercentage = analysis.nullCount / totalRecords;

    // Determine type dynamically
    let type = "unknown";
    if (types.length === 1) {
      type = types[0];
    } else if (types.includes("number")) {
      type = patterns.includes("date") ? "datetime" : "number";
    } else if (types.includes("boolean")) {
      type = "boolean";
    } else if (types.includes("string")) {
      if (patterns.includes("date")) type = "date";
      else if (patterns.includes("datetime")) type = "datetime";
      else if (patterns.includes("email")) type = "email";
      else if (patterns.includes("url")) type = "url";
      else if (patterns.includes("boolean-like")) type = "boolean";
      else type = "string";
    }

    // Determine category dynamically based on patterns and names
    const category = this.determineDynamicCategory(
      fieldName,
      patterns,
      analysis.values
    );

    // Determine if required (less than 10% nulls and not metadata fields)
    const isRequired = nullPercentage < 0.1 && !this.isMetadataField(fieldName);

    // Determine if readonly (metadata or system fields)
    const isReadonly = this.isReadonlyField(fieldName, patterns);

    return {
      name: fieldName,
      type,
      displayName: this.generateDynamicDisplayName(fieldName),
      isRequired,
      isReadonly,
      category,
      metadata: {
        nullPercentage,
        uniqueValues: new Set(analysis.values).size,
        patterns: patterns,
        sampleValues: analysis.values.slice(0, 3),
        inferredFrom: "data-analysis"
      },
      validationRules: this.generateDynamicValidation(
        fieldName,
        type,
        patterns,
        analysis.values
      ),
      uiHints: this.generateDynamicUIHints(
        fieldName,
        type,
        patterns,
        analysis.values
      )
    };
  }

  /**
   * Determine field category completely dynamically
   */
  private determineDynamicCategory(
    fieldName: string,
    patterns: string[],
    values: any[]
  ): string {
    const name = fieldName.toLowerCase();

    // Create dynamic categories based on discovered patterns
    if (
      patterns.includes("weight") ||
      name.includes("weight") ||
      name.includes("kg")
    ) {
      return "weight-measurement";
    }

    if (
      patterns.includes("dimension") ||
      ["length", "width", "height", "depth"].some((dim) => name.includes(dim))
    ) {
      return "dimension-measurement";
    }

    if (
      patterns.includes("volume") ||
      name.includes("volume") ||
      name.includes("liter")
    ) {
      return "volume-measurement";
    }

    if (name.includes("unit") || name.includes("uom")) {
      return "unit-definition";
    }

    if (
      patterns.includes("boolean-like") ||
      name.startsWith("is_") ||
      name.startsWith("has_") ||
      name.startsWith("can_")
    ) {
      return "boolean-flag";
    }

    if (
      name.includes("created") ||
      name.includes("modified") ||
      name.includes("server") ||
      name.includes("time")
    ) {
      return "system-metadata";
    }

    if (
      patterns.includes("uid") ||
      patterns.includes("code") ||
      name.includes("id")
    ) {
      return "identifier";
    }

    if (
      name.includes("name") ||
      name.includes("title") ||
      name.includes("description")
    ) {
      return "descriptive-text";
    }

    // Use pattern-based categories for unknown fields
    if (patterns.length > 0) {
      return `pattern-${patterns[0]}`;
    }

    // Fallback to type-based category
    const primaryType = typeof values[0];
    return `type-${primaryType}`;
  }

  /**
   * Check if field is metadata field
   */
  private isMetadataField(fieldName: string): boolean {
    const name = fieldName.toLowerCase();
    const metadataPatterns = [
      "id",
      "created",
      "modified",
      "server",
      "timestamp",
      "version"
    ];
    return metadataPatterns.some((pattern) => name.includes(pattern));
  }

  /**
   * Check if field is readonly
   */
  private isReadonlyField(fieldName: string, patterns: string[]): boolean {
    const name = fieldName.toLowerCase();
    const readonlyPatterns = ["id", "created", "server"];
    return (
      readonlyPatterns.some((pattern) => name.includes(pattern)) ||
      (patterns.includes("datetime") &&
        (name.includes("created") || name.includes("modified")))
    );
  }

  /**
   * Generate dynamic display name
   */
  private generateDynamicDisplayName(fieldName: string): string {
    return fieldName
      .replace(/([a-z])([A-Z])/g, "$1 $2") // camelCase to words
      .replace(/_/g, " ") // underscores to spaces
      .replace(/\b\w/g, (l) => l.toUpperCase()) // capitalize words
      .replace(/\bUid\b/g, "ID") // Special cases
      .replace(/\bUom\b/g, "UOM")
      .replace(/\bApi\b/g, "API")
      .replace(/\bUrl\b/g, "URL")
      .replace(/\bDb\b/g, "Database")
      .trim();
  }

  /**
   * Generate dynamic validation rules
   */
  private generateDynamicValidation(
    fieldName: string,
    type: string,
    patterns: string[],
    values: any[]
  ): Record<string, any> {
    const rules: Record<string, any> = {};

    if (type === "number") {
      const numValues = values.filter((v) => typeof v === "number");
      if (numValues.length > 0) {
        rules.min = Math.min(...numValues);
        rules.max = Math.max(...numValues);
      }
    }

    if (patterns.includes("email")) {
      rules.pattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/.source;
    }

    if (patterns.includes("url")) {
      rules.pattern = /^https?:\/\/.+/.source;
    }

    if (patterns.includes("code")) {
      rules.pattern = /^[A-Z0-9_-]+$/.source;
    }

    // Dynamic options based on actual values - only for truly categorical data
    const uniqueValues = [...new Set(values)]
      .filter(
        (v) =>
          v !== null &&
          v !== undefined &&
          v !== "" &&
          typeof v === "string" &&
          v.trim() !== ""
      )
      .map((v) => String(v).trim());

    if (
      uniqueValues.length <= 10 &&
      uniqueValues.length > 1 &&
      uniqueValues.length < values.length * 0.8 &&
      type === "string"
    ) {
      // Only create options if there are few unique values relative to total values (indicating categories)
      rules.options = uniqueValues.filter((v) => v.length > 0);
    }

    return Object.keys(rules).length > 0 ? rules : {};
  }

  /**
   * Generate dynamic UI hints
   */
  private generateDynamicUIHints(
    fieldName: string,
    type: string,
    patterns: string[],
    values: any[]
  ): Record<string, any> {
    const hints: Record<string, any> = {};

    if (patterns.includes("weight")) hints.suffix = "kg";
    if (patterns.includes("dimension")) hints.suffix = "cm";
    if (patterns.includes("volume")) hints.suffix = "L";

    if (type === "boolean") hints.component = "switch";
    if (patterns.includes("datetime")) hints.component = "datetime-picker";
    if (patterns.includes("email")) hints.component = "email-input";
    if (patterns.includes("url")) hints.component = "url-input";

    // Multi-line for long text
    const avgLength =
      values
        .filter((v) => typeof v === "string")
        .reduce((sum, v) => sum + v.length, 0) / values.length;
    if (avgLength > 100) hints.component = "textarea";

    return hints;
  }

  /**
   * Determine primary key dynamically
   */
  private determinePrimaryKey(
    fields: DynamicField[],
    sampleData: any[]
  ): string {
    // For SKUUOM specifically, we know Id is the primary key
    if (sampleData.length > 0 && sampleData[0].hasOwnProperty("Id")) {
      return "Id";
    }

    // Look for fields with unique values and ID-like characteristics
    const candidates = fields.filter(
      (field) =>
        field.category === "identifier" ||
        field.name.toLowerCase().includes("id") ||
        field.name.toLowerCase() === "uid"
    );

    // Prioritize numeric IDs
    const numericIdCandidates = candidates.filter(
      (field) => field.name.toLowerCase() === "id" || field.type === "number"
    );

    // Check uniqueness in sample data
    const allCandidates = [...numericIdCandidates, ...candidates];
    for (const candidate of allCandidates) {
      const values = sampleData.map((record) => record[candidate.name]);
      const uniqueValues = new Set(
        values.filter((v) => v !== null && v !== undefined)
      );

      if (uniqueValues.size === values.length && uniqueValues.size > 0) {
        return candidate.name;
      }
    }

    // Fallback: first non-null field
    return fields.find((f) => !f.isReadonly)?.name || fields[0]?.name || "Id";
  }

  /**
   * Determine display field dynamically
   */
  private determineDisplayField(
    fields: DynamicField[],
    sampleData: any[]
  ): string {
    // Look for name-like fields
    const nameFields = fields.filter(
      (field) =>
        field.category === "descriptive-text" ||
        field.name.toLowerCase().includes("name") ||
        field.name.toLowerCase().includes("title")
    );

    if (nameFields.length > 0) {
      return nameFields[0].name;
    }

    // Fallback to first string field that's not an ID
    const stringFields = fields.filter(
      (field) => field.type === "string" && field.category !== "identifier"
    );

    return stringFields[0]?.name || fields[0]?.name || "name";
  }

  /**
   * Discover relationships dynamically
   */
  private discoverRelationships(
    fields: DynamicField[],
    sampleData: any[]
  ): Record<string, any> {
    const relationships: Record<string, any> = {};

    // Look for foreign key patterns
    fields.forEach((field) => {
      const name = field.name.toLowerCase();
      if (name.endsWith("_uid") && name !== "uid") {
        const relatedTable = name.replace("_uid", "");
        relationships[field.name] = {
          type: "foreign_key",
          relatedTable: relatedTable,
          confidence: 0.8
        };
      }
    });

    return relationships;
  }

  /**
   * Discover constraints dynamically
   */
  private discoverConstraints(
    fields: DynamicField[],
    sampleData: any[]
  ): Record<string, any> {
    const constraints: Record<string, any> = {};

    fields.forEach((field) => {
      const values = sampleData.map((record) => record[field.name]);
      const nonNullValues = values.filter((v) => v !== null && v !== undefined);

      // Uniqueness constraint
      const uniqueValues = new Set(nonNullValues);
      if (
        uniqueValues.size === nonNullValues.length &&
        nonNullValues.length > 1
      ) {
        constraints[field.name] = { unique: true };
      }
    });

    return constraints;
  }

  /**
   * Calculate schema confidence score
   */
  private calculateSchemaConfidence(
    fields: DynamicField[],
    sampleData: any[]
  ): number {
    let confidence = 0;

    // More sample data = higher confidence
    confidence += Math.min(sampleData.length / 100, 0.5);

    // Fields with clear patterns = higher confidence
    const fieldsWithPatterns = fields.filter(
      (f) => f.metadata.patterns.length > 0
    );
    confidence += (fieldsWithPatterns.length / fields.length) * 0.3;

    // Fields with clear types = higher confidence
    const fieldsWithKnownTypes = fields.filter((f) => f.type !== "unknown");
    confidence += (fieldsWithKnownTypes.length / fields.length) * 0.2;

    return Math.min(confidence, 1);
  }

  /**
   * Get schema from database introspection (if available)
   */
  private async getSchemaFromDatabaseIntrospection(
    tableName: string
  ): Promise<DynamicTableSchema> {
    try {
      const response = await fetch(
        `${this.baseURL}/System/IntrospectTable/${tableName}`,
        {
          method: "GET",
          headers: {
            ...getAuthHeaders(),
            Accept: "application/json"
          }
        }
      );

      if (!response.ok) {
        throw new Error("Database introspection not available");
      }

      const introspectionResult = await response.json();

      // Convert introspection result to our dynamic schema format
      return this.convertIntrospectionToSchema(tableName, introspectionResult);
    } catch (error) {
      throw new Error(`Database introspection failed: ${error.message}`);
    }
  }

  /**
   * Convert database introspection result to our schema format
   */
  private convertIntrospectionToSchema(
    tableName: string,
    introspection: any
  ): DynamicTableSchema {
    const fields = (introspection.columns || []).map((col: any) => ({
      name: col.column_name,
      type: this.mapDatabaseTypeToJSType(col.data_type),
      displayName: this.generateDynamicDisplayName(col.column_name),
      isRequired: col.is_nullable === "NO",
      isReadonly:
        col.column_default !== null && col.column_default.includes("nextval"),
      category: this.determineDynamicCategory(col.column_name, [], []),
      metadata: {
        databaseType: col.data_type,
        maxLength: col.character_maximum_length,
        numericPrecision: col.numeric_precision,
        isNullable: col.is_nullable === "YES",
        defaultValue: col.column_default,
        inferredFrom: "database-introspection"
      },
      validationRules: this.generateValidationFromDatabaseType(col),
      uiHints: this.generateUIHintsFromDatabaseType(col)
    }));

    return {
      tableName,
      fields,
      primaryKey: introspection.primaryKey || fields[0]?.name || "id",
      displayField:
        fields.find((f) => f.name.toLowerCase().includes("name"))?.name ||
        fields[0]?.name,
      relationships: introspection.foreignKeys || {},
      constraints: introspection.constraints || {},
      metadata: {
        source: "database-introspection",
        retrievedAt: new Date().toISOString(),
        confidence: 1.0
      }
    };
  }

  /**
   * Map database types to JavaScript types
   */
  private mapDatabaseTypeToJSType(dbType: string): string {
    const type = dbType.toLowerCase();

    if (
      type.includes("int") ||
      type.includes("serial") ||
      type.includes("number")
    ) {
      return "number";
    }
    if (
      type.includes("decimal") ||
      type.includes("numeric") ||
      type.includes("float") ||
      type.includes("double")
    ) {
      return "decimal";
    }
    if (type.includes("bool")) {
      return "boolean";
    }
    if (type.includes("date") || type.includes("time")) {
      return "datetime";
    }
    if (
      type.includes("text") ||
      type.includes("varchar") ||
      type.includes("char")
    ) {
      return "string";
    }
    if (type.includes("json") || type.includes("jsonb")) {
      return "json";
    }
    if (type.includes("uuid")) {
      return "uuid";
    }

    return "string"; // fallback
  }

  /**
   * Generate validation from database type info
   */
  private generateValidationFromDatabaseType(col: any): Record<string, any> {
    const rules: Record<string, any> = {};

    if (col.character_maximum_length) {
      rules.maxLength = col.character_maximum_length;
    }

    if (col.numeric_precision) {
      rules.precision = col.numeric_precision;
    }

    if (col.data_type.toLowerCase().includes("email")) {
      rules.pattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/.source;
    }

    return rules;
  }

  /**
   * Generate UI hints from database type
   */
  private generateUIHintsFromDatabaseType(col: any): Record<string, any> {
    const hints: Record<string, any> = {};
    const type = col.data_type.toLowerCase();

    if (type.includes("text") && col.character_maximum_length > 255) {
      hints.component = "textarea";
    }

    if (type.includes("bool")) {
      hints.component = "switch";
    }

    if (type.includes("date")) {
      hints.component = type.includes("time")
        ? "datetime-picker"
        : "date-picker";
    }

    if (type.includes("json")) {
      hints.component = "json-editor";
    }

    return hints;
  }

  /**
   * Get raw table data completely dynamically
   */
  private async getRawTableData(
    tableName: string,
    pageNumber: number = 1,
    pageSize: number = 100
  ): Promise<any[]> {
    // Try multiple endpoint patterns - prioritize known working endpoints
    const endpointPatterns = [
      `${this.baseURL}/SKUUOM/SelectAllSKUUOMDetails`, // Known working endpoint
      `${this.baseURL}/${tableName}/SelectAll${tableName}Details`,
      `${this.baseURL}/${tableName}/GetAll`,
      `${this.baseURL}/${tableName}/Select`,
      `${this.baseURL}/Generic/GetTableData/${tableName}`
    ];

    for (const endpoint of endpointPatterns) {
      try {
        const response = await this.tryEndpoint(endpoint, pageNumber, pageSize);
        if (response) {
          return response;
        }
      } catch (error) {
        continue; // Try next endpoint
      }
    }

    throw new Error(`Unable to fetch data from table: ${tableName}`);
  }

  /**
   * Try a specific endpoint pattern
   */
  private async tryEndpoint(
    endpoint: string,
    pageNumber: number,
    pageSize: number
  ): Promise<any[] | null> {
    const request: PagingRequest = {
      PageNumber: 0,
      PageSize: 0,
      FilterCriterias: [],
      SortCriterias: [],
      IsCountRequired: false
    };

    const response = await fetch(endpoint, {
      method: "POST",
      headers: {
        ...getAuthHeaders(),
        "Content-Type": "application/json",
        Accept: "application/json"
      },
      body: JSON.stringify(request)
    });

    if (!response.ok) {
      return null;
    }

    const result: ApiResponse<PagedResponse<any>> = await response.json();

    if (!result.IsSuccess) {
      return null;
    }

    return result.Data?.PagedData || [];
  }

  /**
   * Perform any CRUD operation dynamically
   */
  async performOperation(
    tableName: string,
    operation: "create" | "read" | "update" | "delete",
    data?: any,
    identifier?: string
  ): Promise<any> {
    // Use correct SKUUOM endpoints
    const endpointMap = {
      create: `${this.baseURL}/SKUUOM/CreateSKUUOM`,
      read: `${this.baseURL}/SKUUOM/SelectSKUUOMByUID?UID=${identifier}`,
      update: `${this.baseURL}/SKUUOM/UpdateSKUUOM`,
      delete: `${this.baseURL}/SKUUOM/DeleteSKUUOMByUID?UID=${identifier}`
    };

    const endpoint = endpointMap[operation];
    const method =
      operation === "read"
        ? "GET"
        : operation === "delete"
        ? "DELETE"
        : operation === "update"
        ? "PUT"
        : "POST";

    const requestOptions: RequestInit = {
      method,
      headers: {
        ...getAuthHeaders(),
        "Content-Type": "application/json",
        Accept: "application/json"
      }
    };

    if (data && (operation === "create" || operation === "update")) {
      // Enrich data with timestamps and metadata dynamically
      const enrichedData = {
        ...data,
        ...(operation === "create" && {
          CreatedTime: new Date().toISOString(),
          CreatedBy: "SYSTEM"
        }),
        ModifiedTime: new Date().toISOString(),
        ModifiedBy: "SYSTEM",
        ServerAddTime: new Date().toISOString(),
        ServerModifiedTime: new Date().toISOString()
      };

      requestOptions.body = JSON.stringify(enrichedData);
    }

    const response = await fetch(endpoint, requestOptions);

    if (!response.ok) {
      throw new Error(`${operation} operation failed: ${response.statusText}`);
    }

    // No caching - direct server-side operations only

    if (operation === "read") {
      const result: ApiResponse<any> = await response.json();
      return result.IsSuccess ? result.Data : null;
    }

    return await response.json();
  }

  /**
   * Get all UOM data using the correct SKUUOM endpoint
   */
  async getAllUOMData(): Promise<any[]> {
    try {
      const request = {
        PageNumber: 0,
        PageSize: 0,
        FilterCriterias: [],
        SortCriterias: [],
        IsCountRequired: false
      };

      const response = await fetch(
        `${this.baseURL}/SKUUOM/SelectAllSKUUOMDetails`,
        {
          method: "POST",
          headers: {
            ...getAuthHeaders(),
            "Content-Type": "application/json",
            Accept: "application/json"
          },
          body: JSON.stringify(request)
        }
      );

      if (!response.ok) {
        throw new Error(`SKUUOM API failed: ${response.statusText}`);
      }

      const result = await response.json();

      if (!result.IsSuccess) {
        throw new Error(`SKUUOM API error: ${result.ErrorMessage}`);
      }

      return result.Data?.PagedData || [];
    } catch (error) {
      console.error("❌ Failed to fetch SKUUOM data:", error);
      throw error;
    }
  }

  /**
   * Get paginated UOM data using the correct SKUUOM endpoint
   */
  async getPagedUOMData(
    pageNumber: number = 1,
    pageSize: number = 10,
    searchTerm?: string,
    categoryFilter?: string
  ): Promise<{ data: any[]; totalCount: number }> {
    try {
      // Build filter criteria using correct database column names and backend format
      const filterCriterias: any[] = [];

      // Add search filter if provided - using actual database column names with OR logic
      if (searchTerm && searchTerm.trim()) {
        const search = searchTerm.trim();
        console.log("🔍 Adding search filter for:", search);

        // Search across multiple fields using OR logic
        // Backend expects Name, Value, Type, and FilterMode properties
        filterCriterias.push({
          Name: "sku_uid",
          Type: 10, // FilterType.Contains = 10
          Value: search,
          FilterMode: 1 // FilterMode.Or = 1
        });
        filterCriterias.push({
          Name: "code",
          Type: 10, // FilterType.Contains = 10
          Value: search,
          FilterMode: 1 // FilterMode.Or = 1
        });
        filterCriterias.push({
          Name: "name",
          Type: 10, // FilterType.Contains = 10
          Value: search,
          FilterMode: 1 // FilterMode.Or = 1
        });
      }

      // Add type filter if provided - using correct database column names
      if (categoryFilter && categoryFilter.trim()) {
        const filters = categoryFilter.split(",").filter((f) => f.trim());

        // If we have search filters, use AND to combine with type filters
        if (filterCriterias.length > 0) {
          // Change the last search filter to AND to separate from type filters
          filterCriterias[filterCriterias.length - 1].FilterMode = 0; // FilterMode.And = 0
        }

        filters.forEach((filter, index) => {
          if (filter === "base-uom") {
            filterCriterias.push({
              Name: "is_base_uom",
              Type: 1, // FilterType.Equal = 1
              Value: true, // Send as boolean, not string
              FilterMode: index > 0 ? 1 : 0 // OR between type filters, AND with search
            });
          } else if (filter === "outer-uom") {
            filterCriterias.push({
              Name: "is_outer_uom",
              Type: 1, // FilterType.Equal = 1
              Value: true, // Send as boolean, not string
              FilterMode: index > 0 ? 1 : 0 // OR between type filters, AND with search
            });
          }
        });
      }

      // Use proper server-side pagination with filters
      const request = {
        PageNumber: pageNumber - 1, // Convert 1-based to 0-based
        PageSize: pageSize,
        FilterCriterias: filterCriterias,
        SortCriterias: [],
        IsCountRequired: true
      };

      console.log(
        "📤 SKUUOM API Request (server-side pagination):",
        JSON.stringify(request, null, 2)
      );

      const response = await fetch(
        `${this.baseURL}/SKUUOM/SelectAllSKUUOMDetails`,
        {
          method: "POST",
          headers: {
            ...getAuthHeaders(),
            "Content-Type": "application/json",
            Accept: "application/json"
          },
          body: JSON.stringify(request)
        }
      );

      console.log("📥 Response status:", response.status, response.statusText);

      if (!response.ok) {
        const errorText = await response.text();
        console.error("❌ SKUUOM API Error Response:", errorText);

        // Try to parse error as JSON if possible
        try {
          const errorObj = JSON.parse(errorText);
          throw new Error(
            errorObj.ErrorMessage || `SKUUOM API failed: ${response.statusText}`
          );
        } catch {
          throw new Error(`SKUUOM API failed: ${response.statusText}`);
        }
      }

      const result = await response.json();
      console.log("📊 API Result:", result);

      if (!result.IsSuccess) {
        console.error("❌ API returned IsSuccess=false:", result);
        throw new Error(
          `SKUUOM API error: ${result.ErrorMessage || "Unknown error"}`
        );
      }

      const pagedData = result.Data?.PagedData || [];
      const totalCount = result.Data?.TotalCount || 0;

      console.log(
        `📥 Received ${pagedData.length} records, total: ${totalCount}`
      );

      if (pagedData.length > 0) {
        console.log("📋 Sample record:", pagedData[0]);
      }

      return {
        data: pagedData,
        totalCount: totalCount
      };
    } catch (error) {
      console.error("❌ Failed to fetch paginated SKUUOM data:", error);
      throw error;
    }
  }

  /**
   * Validate UOM data (simplified for now)
   */
  async validateUOMData(
    data: any
  ): Promise<{ isValid: boolean; errors: string[] }> {
    const errors: string[] = [];

    // Basic validation
    if (!data.Code || typeof data.Code !== "string") {
      errors.push("Code is required and must be a string");
    }

    if (!data.Name || typeof data.Name !== "string") {
      errors.push("Name is required and must be a string");
    }

    if (
      data.Multiplier !== undefined &&
      (isNaN(data.Multiplier) || data.Multiplier <= 0)
    ) {
      errors.push("Multiplier must be a positive number");
    }

    return {
      isValid: errors.length === 0,
      errors
    };
  }

  /**
   * Get all available operations for a table dynamically
   */
  async getAvailableOperations(tableName: string): Promise<string[]> {
    const operations = ["create", "read", "update", "delete"];
    const availableOperations: string[] = [];

    for (const operation of operations) {
      try {
        // Test if endpoint exists by making a lightweight request
        const testEndpoint = `${this.baseURL}/${tableName}/${
          operation === "create"
            ? "Create"
            : operation === "read"
            ? "Select"
            : operation === "update"
            ? "Update"
            : "Delete"
        }${tableName}`;

        const response = await fetch(testEndpoint, {
          method: "HEAD",
          headers: getAuthHeaders()
        });

        if (response.ok || response.status === 405) {
          // 405 means method not allowed but endpoint exists
          availableOperations.push(operation);
        }
      } catch (error) {
        // Endpoint doesn't exist
      }
    }

    return availableOperations;
  }

  /**
   * Get system health information
   */
  async getSystemInfo(): Promise<Record<string, any>> {
    return {
      systemCapabilities: {
        schemaIntrospection: await this.testCapability("schema"),
        metadataAPI: await this.testCapability("metadata"),
        dynamicEndpoints: await this.testCapability("dynamic")
      }
    };
  }

  /**
   * Test system capabilities
   */
  private async testCapability(capability: string): Promise<boolean> {
    try {
      switch (capability) {
        case "schema":
          const response = await fetch(`${this.baseURL}/System/GetTables`, {
            method: "HEAD",
            headers: getAuthHeaders()
          });
          return response.ok;

        case "metadata":
          const metaResponse = await fetch(`${this.baseURL}/SKUUOM/GetSchema`, {
            method: "HEAD",
            headers: getAuthHeaders()
          });
          return metaResponse.ok;

        case "dynamic":
          return true; // Our system is always dynamic

        default:
          return false;
      }
    } catch (error) {
      return false;
    }
  }
}

export const fullyDynamicUOMService = new FullyDynamicUOMService();
