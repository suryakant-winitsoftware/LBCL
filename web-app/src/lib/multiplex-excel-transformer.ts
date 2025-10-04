/**
 * Multiplex-specific Excel format transformer
 * Handles the specific Excel format used by Multiplex with pivoted category data
 */

import { parseExcelFromArrayBuffer, readFileAsArrayBuffer, ParsedRow, ParseResult } from './excel-utils';

export interface MultiplexExcelRow {
  EmpNo: string;
  EmpName: string;
  CustID: string;
  CustName: string;
  YearMon: string; // Format: YYYYMM
  Cosmetics: number;
  'FMCG Non-Food': number;
  'FMCG Food': number;
}

export interface TransformedTarget {
  UserLinkedType: string;
  UserLinkedUid: string;
  CustomerLinkedType: string;
  CustomerLinkedUid: string;
  ItemLinkedItemType: string;
  ItemLinkedItemUid: string | null;
  TargetMonth: number;
  TargetYear: number;
  TargetAmount: number;
  Status: string;
  Notes: string | null;
}

/**
 * Parse and transform Multiplex Excel format to standard Target format
 */
export async function parseMultiplexExcel(file: File): Promise<ParseResult> {
  try {
    console.log('🔄 Parsing Multiplex Excel format...');
    
    const arrayBuffer = await readFileAsArrayBuffer(file);
    
    // Use basic column mapping for the Multiplex format
    const multiplexColumns = [
      { name: 'EmpNo', required: true, type: 'string' as const },
      { name: 'EmpName', required: false, type: 'string' as const },
      { name: 'CustID', required: true, type: 'string' as const },
      { name: 'CustName', required: false, type: 'string' as const },
      { name: 'YearMon', required: true, type: 'string' as const },
      { name: 'Cosmetics', required: false, type: 'number' as const },
      { name: 'FMCG Non-Food', required: false, type: 'number' as const },
      { name: 'FMCG Food', required: false, type: 'number' as const }
    ];
    
    // Parse the Excel file with Multiplex column structure
    const parseResult = parseExcelFromArrayBuffer(arrayBuffer, multiplexColumns);
    
    if (parseResult.errors.length > 0) {
      return parseResult;
    }
    
    console.log('📊 Raw Multiplex data:', parseResult.data);
    
    // Transform the data to standard Target format
    const transformedTargets: ParsedRow[] = [];
    const errors: string[] = [];
    
    parseResult.data.forEach((row, index) => {
      try {
        const rowNumber = index + 2; // Excel row number
        
        // Extract basic info
        const empNo = row['EmpNo'] as string;
        const empName = row['EmpName'] as string;
        const custId = row['CustID'] as string;
        const custName = row['CustName'] as string;
        const yearMon = row['YearMon'] as string;
        
        // Parse year and month from YYYYMM format
        if (!yearMon || yearMon.length !== 6) {
          errors.push(`Row ${rowNumber}: Invalid YearMon format "${yearMon}". Expected YYYYMM (e.g., 202509)`);
          return;
        }
        
        const year = parseInt(yearMon.substring(0, 4));
        const month = parseInt(yearMon.substring(4, 6));
        
        if (isNaN(year) || isNaN(month) || month < 1 || month > 12) {
          errors.push(`Row ${rowNumber}: Invalid year/month in "${yearMon}"`);
          return;
        }
        
        // Extract target amounts for each category
        const cosmeticsAmount = Number(row['Cosmetics']) || 0;
        const fmcgNonFoodAmount = Number(row['FMCG Non-Food']) || 0;
        const fmcgFoodAmount = Number(row['FMCG Food']) || 0;
        
        // Create separate target records for each category with non-zero amounts
        const categories = [
          { name: 'Cosmetics', amount: cosmeticsAmount },
          { name: 'FMCG Non-Food', amount: fmcgNonFoodAmount },
          { name: 'FMCG Food', amount: fmcgFoodAmount }
        ];
        
        categories.forEach(category => {
          if (category.amount > 0) {
            const transformedTarget: ParsedRow = {
              'User Type': 'Employee', // Since we have EmpNo
              'User UID': empNo,
              'Customer Type': 'Customer',
              'Customer UID': custId,
              'Category': category.name,
              'Item UID': null, // Will be properly handled as null
              'Target Month': month,
              'Target Year': year,
              'Target Amount': category.amount,
              'Achieved Amount': 0,
              'Status': 'Not Started',
              'Notes': `Employee: ${empName}, Customer: ${custName}`
            };
            
            transformedTargets.push(transformedTarget);
          }
        });
        
      } catch (error) {
        errors.push(`Row ${index + 2}: Error processing row - ${error.message}`);
      }
    });
    
    console.log('✅ Transformed targets:', transformedTargets);
    console.log(`📈 Created ${transformedTargets.length} target records from ${parseResult.data.length} source rows`);
    
    return {
      data: transformedTargets,
      errors,
      totalRows: parseResult.totalRows,
      validRows: transformedTargets.length
    };
    
  } catch (error) {
    return {
      data: [],
      errors: [`Failed to parse Multiplex Excel file: ${error.message}`],
      totalRows: 0,
      validRows: 0
    };
  }
}

/**
 * Detect if an Excel file is in Multiplex format
 */
export function isMultiplexFormat(headers: string[]): boolean {
  const requiredMultiplexHeaders = ['EmpNo', 'CustID', 'YearMon'];
  const categoryHeaders = ['Cosmetics', 'FMCG Non-Food', 'FMCG Food'];
  
  const hasRequiredHeaders = requiredMultiplexHeaders.every(header =>
    headers.some(h => h.toLowerCase().trim() === header.toLowerCase().trim())
  );
  
  const hasCategoryHeaders = categoryHeaders.some(header =>
    headers.some(h => h.toLowerCase().trim() === header.toLowerCase().trim())
  );
  
  return hasRequiredHeaders && hasCategoryHeaders;
}