/**
 * Excel/CSV parsing utilities for frontend
 * Handles Excel files without backend dependencies
 */

import * as XLSX from 'xlsx';

export interface ParsedRow {
  [key: string]: string | number | null;
}

export interface ParseResult {
  data: ParsedRow[];
  errors: string[];
  totalRows: number;
  validRows: number;
}

export interface ColumnMapping {
  name: string;
  required?: boolean;
  type?: 'string' | 'number' | 'date';
  validator?: (value: any) => boolean;
  aliases?: string[]; // Alternative column names that can be mapped to this column
}

/**
 * Parse CSV content into structured data
 */
export function parseCSV(csvContent: string, columns: ColumnMapping[]): ParseResult {
  const lines = csvContent.split('\n').map(line => line.trim()).filter(line => line);
  const errors: string[] = [];
  const data: ParsedRow[] = [];
  
  if (lines.length === 0) {
    return { data: [], errors: ['File is empty'], totalRows: 0, validRows: 0 };
  }

  // Parse header row
  const headers = lines[0].split(',').map(h => h.trim().replace(/"/g, ''));
  
  // Validate headers against expected columns
  const requiredColumns = columns.filter(col => col.required);
  const missingColumns = requiredColumns.filter(col => 
    !headers.some(header => header.toLowerCase() === col.name.toLowerCase())
  );
  
  if (missingColumns.length > 0) {
    errors.push(`Missing required columns: ${missingColumns.map(col => col.name).join(', ')}`);
  }

  // Parse data rows
  for (let i = 1; i < lines.length; i++) {
    const rowNumber = i + 1;
    const values = parseCSVRow(lines[i]);
    
    if (values.length === 0) continue; // Skip empty rows
    
    const rowData: ParsedRow = {};
    let hasValidData = false;
    let rowErrors: string[] = [];
    
    headers.forEach((header, index) => {
      const value = values[index]?.trim().replace(/"/g, '') || '';
      const column = columns.find(col => col.name.toLowerCase() === header.toLowerCase());
      
      if (column) {
        // Type conversion and validation
        let processedValue: string | number | null = value || null;
        
        if (value && column.type === 'number') {
          const numValue = parseFloat(value);
          if (isNaN(numValue)) {
            rowErrors.push(`Row ${rowNumber}: Invalid number in column "${header}": ${value}`);
          } else {
            processedValue = numValue;
          }
        }
        
        // Required field validation
        if (column.required && (!value || value === '')) {
          rowErrors.push(`Row ${rowNumber}: Missing required field "${header}"`);
        }
        
        // Custom validation
        if (value && column.validator && !column.validator(processedValue)) {
          rowErrors.push(`Row ${rowNumber}: Invalid value in column "${header}": ${value}`);
        }
        
        rowData[column.name] = processedValue;
        if (processedValue !== null && processedValue !== '') {
          hasValidData = true;
        }
      }
    });
    
    if (hasValidData) {
      if (rowErrors.length === 0) {
        data.push(rowData);
      } else {
        errors.push(...rowErrors);
      }
    }
  }
  
  return {
    data,
    errors,
    totalRows: lines.length - 1, // Exclude header
    validRows: data.length
  };
}

/**
 * Parse a single CSV row, handling quoted values with commas
 */
function parseCSVRow(row: string): string[] {
  const values: string[] = [];
  let current = '';
  let inQuotes = false;
  
  for (let i = 0; i < row.length; i++) {
    const char = row[i];
    
    if (char === '"') {
      inQuotes = !inQuotes;
    } else if (char === ',' && !inQuotes) {
      values.push(current);
      current = '';
    } else {
      current += char;
    }
  }
  
  values.push(current); // Add the last value
  return values;
}

/**
 * Read file content as text (for CSV files)
 */
export function readFileAsText(file: File): Promise<string> {
  return new Promise((resolve, reject) => {
    const reader = new FileReader();
    reader.onload = (e) => {
      const content = e.target?.result as string;
      resolve(content);
    };
    reader.onerror = () => reject(new Error('Failed to read file'));
    reader.readAsText(file);
  });
}

/**
 * Read file content as ArrayBuffer (for Excel files)
 */
export function readFileAsArrayBuffer(file: File): Promise<ArrayBuffer> {
  return new Promise((resolve, reject) => {
    const reader = new FileReader();
    reader.onload = (e) => {
      const arrayBuffer = e.target?.result as ArrayBuffer;
      resolve(arrayBuffer);
    };
    reader.onerror = () => reject(new Error('Failed to read file'));
    reader.readAsArrayBuffer(file);
  });
}

/**
 * Parse Excel file from ArrayBuffer using SheetJS
 */
export function parseExcelFromArrayBuffer(arrayBuffer: ArrayBuffer, columns: ColumnMapping[]): ParseResult {
  try {
    // Read the workbook
    const workbook = XLSX.read(arrayBuffer, { type: 'array' });
    
    // Get the first worksheet
    const sheetName = workbook.SheetNames[0];
    if (!sheetName) {
      return {
        data: [],
        errors: ['Excel file contains no worksheets'],
        totalRows: 0,
        validRows: 0
      };
    }
    
    const worksheet = workbook.Sheets[sheetName];
    
    // Convert to JSON with header row
    const jsonData = XLSX.utils.sheet_to_json(worksheet, { 
      header: 1,
      defval: '',
      raw: false, // This ensures we get string values
      blankrows: false // Skip completely blank rows
    }) as any[][];
    
    console.log('🔍 Raw Excel Data:', jsonData);
    
    if (jsonData.length === 0) {
      return {
        data: [],
        errors: ['Excel worksheet is empty'],
        totalRows: 0,
        validRows: 0
      };
    }
    
    // Find the first non-empty row as headers
    let headerRowIndex = -1;
    let headers: string[] = [];
    
    for (let i = 0; i < jsonData.length; i++) {
      const row = jsonData[i];
      if (row && row.length > 0 && row.some(cell => cell && cell.toString().trim() !== '')) {
        headers = row.map(cell => (cell || '').toString().trim());
        headerRowIndex = i;
        break;
      }
    }
    
    if (headerRowIndex === -1) {
      return {
        data: [],
        errors: ['No valid header row found in Excel file'],
        totalRows: 0,
        validRows: 0
      };
    }
    
    console.log('🔍 Header row found at index:', headerRowIndex);
    console.log('🔍 Headers:', headers);
    
    // Extract data rows (skip header and any rows before it)
    const dataRows = jsonData.slice(headerRowIndex + 1);
    
    return parseExcelData(headers, dataRows, columns);
    
  } catch (error) {
    return {
      data: [],
      errors: [`Failed to parse Excel file: ${error.message}`],
      totalRows: 0,
      validRows: 0
    };
  }
}

/**
 * Parse Excel data rows with validation
 */
function parseExcelData(headers: string[], dataRows: any[][], columns: ColumnMapping[]): ParseResult {
  const errors: string[] = [];
  const data: ParsedRow[] = [];
  
  // Clean and normalize headers
  const cleanHeaders = headers.map(h => (h || '').toString().trim()).filter(h => h !== '');
  
  console.log('🔍 Excel Headers Found:', cleanHeaders);
  console.log('🔍 Expected Columns:', columns.map(col => col.name));
  
  // Validate headers against expected columns
  const requiredColumns = columns.filter(col => col.required);
  const missingColumns = requiredColumns.filter(col => 
    !cleanHeaders.some(header => header.toLowerCase().trim() === col.name.toLowerCase().trim())
  );
  
  if (missingColumns.length > 0) {
    errors.push(`Missing required columns: ${missingColumns.map(col => col.name).join(', ')}`);
    errors.push(`Found headers in your file: [${cleanHeaders.join(', ')}]`);
    errors.push(`Expected headers: [${columns.map(col => col.name).join(', ')}]`);
    
    // Suggest close matches
    const suggestions: string[] = [];
    missingColumns.forEach(missingCol => {
      const closeMatch = cleanHeaders.find(header => 
        header.toLowerCase().includes(missingCol.name.toLowerCase()) ||
        missingCol.name.toLowerCase().includes(header.toLowerCase())
      );
      if (closeMatch) {
        suggestions.push(`"${closeMatch}" might be "${missingCol.name}"`);
      }
    });
    
    if (suggestions.length > 0) {
      errors.push(`Possible matches: ${suggestions.join(', ')}`);
    }
    
    errors.push('Please ensure your Excel file has the exact column headers as shown in the template.');
  }
  
  // Parse data rows
  for (let i = 0; i < dataRows.length; i++) {
    const rowNumber = i + 2; // +2 because Excel rows start at 1 and we skip header
    const values = dataRows[i];
    
    if (!values || values.length === 0) continue; // Skip empty rows
    
    const rowData: ParsedRow = {};
    let hasValidData = false;
    let rowErrors: string[] = [];
    
    cleanHeaders.forEach((header, index) => {
      const value = values[index]?.toString().trim() || '';
      const column = columns.find(col => col.name.toLowerCase().trim() === header.toLowerCase().trim());
      
      if (column) {
        // Type conversion and validation
        let processedValue: string | number | null = value || null;
        
        if (value && column.type === 'number') {
          const numValue = parseFloat(value);
          if (isNaN(numValue)) {
            rowErrors.push(`Row ${rowNumber}: Invalid number in column "${header}": ${value}`);
          } else {
            processedValue = numValue;
          }
        }
        
        // Required field validation
        if (column.required && (!value || value === '')) {
          rowErrors.push(`Row ${rowNumber}: Missing required field "${header}"`);
        }
        
        // Custom validation
        if (value && column.validator && !column.validator(processedValue)) {
          rowErrors.push(`Row ${rowNumber}: Invalid value in column "${header}": ${value}`);
        }
        
        rowData[column.name] = processedValue;
        if (processedValue !== null && processedValue !== '') {
          hasValidData = true;
        }
      }
    });
    
    if (hasValidData) {
      if (rowErrors.length === 0) {
        data.push(rowData);
      } else {
        errors.push(...rowErrors);
      }
    }
  }
  
  return {
    data,
    errors,
    totalRows: dataRows.length,
    validRows: data.length
  };
}

/**
 * Parse Excel (.xlsx/.xls) or CSV files using SheetJS
 */
export async function parseExcelFile(file: File, columns: ColumnMapping[]): Promise<ParseResult> {
  const fileExtension = file.name.split('.').pop()?.toLowerCase();
  
  // For CSV files, use direct parsing
  if (fileExtension === 'csv') {
    try {
      const content = await readFileAsText(file);
      return parseCSV(content, columns);
    } catch (error) {
      return {
        data: [],
        errors: [`Failed to parse CSV file: ${error.message}`],
        totalRows: 0,
        validRows: 0
      };
    }
  }
  
  // For Excel files, use SheetJS
  if (fileExtension === 'xlsx' || fileExtension === 'xls') {
    try {
      const arrayBuffer = await readFileAsArrayBuffer(file);
      return parseExcelFromArrayBuffer(arrayBuffer, columns);
    } catch (error) {
      return {
        data: [],
        errors: [`Failed to parse Excel file: ${error.message}`],
        totalRows: 0,
        validRows: 0
      };
    }
  }
  
  return {
    data: [],
    errors: [`Unsupported file format: ${fileExtension}. Please use .xlsx, .xls, or .csv files.`],
    totalRows: 0,
    validRows: 0
  };
}

/**
 * Generate CSV template content
 */
export function generateCSVTemplate(columns: ColumnMapping[], sampleData?: any[]): string {
  const headers = columns.map(col => col.name).join(',');
  
  if (!sampleData || sampleData.length === 0) {
    return headers;
  }
  
  const dataRows = sampleData.map(row => 
    columns.map(col => {
      const value = row[col.name] || '';
      // Wrap in quotes if contains comma
      return typeof value === 'string' && value.includes(',') 
        ? `"${value}"` 
        : value;
    }).join(',')
  );
  
  return [headers, ...dataRows].join('\n');
}

/**
 * Download content as CSV file
 */
export function downloadCSV(content: string, filename: string): void {
  const blob = new Blob([content], { type: 'text/csv;charset=utf-8;' });
  const link = document.createElement('a');
  const url = URL.createObjectURL(blob);
  
  link.setAttribute('href', url);
  link.setAttribute('download', filename);
  link.style.visibility = 'hidden';
  
  document.body.appendChild(link);
  link.click();
  document.body.removeChild(link);
  
  URL.revokeObjectURL(url);
}

/**
 * Generate and download Excel template file
 */
export function generateExcelTemplate(columns: ColumnMapping[], sampleData?: any[], filename: string = 'template.xlsx'): void {
  try {
    // Create a new workbook
    const workbook = XLSX.utils.book_new();
    
    // Prepare data for worksheet
    const headers = columns.map(col => col.name);
    const data = [headers];
    
    console.log('📊 Excel Template Headers:', headers);
    
    // Add sample data if provided
    if (sampleData && sampleData.length > 0) {
      sampleData.forEach(row => {
        const rowData = columns.map(col => row[col.name] || '');
        data.push(rowData);
      });
    }
    
    console.log('📊 Excel Template Data:', data);
    
    // Create worksheet
    const worksheet = XLSX.utils.aoa_to_sheet(data);
    
    // Set column widths
    const columnWidths = columns.map(col => ({ wch: Math.max(col.name.length + 2, 15) }));
    worksheet['!cols'] = columnWidths;
    
    // Style header row (if possible)
    const range = XLSX.utils.decode_range(worksheet['!ref'] || 'A1');
    for (let col = range.s.c; col <= range.e.c; col++) {
      const cellAddress = XLSX.utils.encode_cell({ r: 0, c: col });
      const cell = worksheet[cellAddress];
      if (cell) {
        cell.s = {
          font: { bold: true },
          fill: { fgColor: { rgb: 'CCCCCC' } }
        };
      }
    }
    
    // Add worksheet to workbook
    XLSX.utils.book_append_sheet(workbook, worksheet, 'Template');
    
    // Generate Excel file buffer
    const excelBuffer = XLSX.write(workbook, { bookType: 'xlsx', type: 'array' });
    
    // Create blob and download
    const blob = new Blob([excelBuffer], { 
      type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' 
    });
    
    const link = document.createElement('a');
    const url = URL.createObjectURL(blob);
    
    link.setAttribute('href', url);
    link.setAttribute('download', filename);
    link.style.visibility = 'hidden';
    
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    
    URL.revokeObjectURL(url);
    
  } catch (error) {
    console.error('Failed to generate Excel template:', error);
    // Fallback to CSV if Excel generation fails
    const csvContent = generateCSVTemplate(columns, sampleData);
    downloadCSV(csvContent, filename.replace('.xlsx', '.csv'));
  }
}