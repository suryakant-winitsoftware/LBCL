'use client';

import React from 'react';
import { useRouter } from 'next/navigation';
import { Button } from '@/components/ui/button';
import { ArrowLeft } from 'lucide-react';
import { ExcelUpload, ExcelUploadColumn, ExcelUploadResult } from '@/components/ui/excel-upload';
import { useToast } from '@/components/ui/use-toast';
import { parseExcelFile, generateCSVTemplate, downloadCSV, generateExcelTemplate, ColumnMapping } from '@/lib/excel-utils';
import { parseMultiplexExcel, isMultiplexFormat } from '@/lib/multiplex-excel-transformer';
import targetService from '@/services/targetService';

// Define the columns for validation
const TARGET_COLUMN_MAPPING: ColumnMapping[] = [
  {
    name: 'User Type',
    required: true,
    type: 'string',
    validator: (value) => ['Route', 'Employee', 'Customer'].includes(value as string)
  },
  {
    name: 'User Code',
    required: true,
    type: 'string'
  },
  {
    name: 'Customer Type',
    required: false,
    type: 'string',
    validator: (value) => !value || ['Customer', 'Store', 'Outlet'].includes(value as string)
  },
  {
    name: 'Customer Code',
    required: false,
    type: 'string'
  },
  {
    name: 'Category',
    required: false,
    type: 'string',
    validator: (value) => !value || ['All', 'Cosmetics', 'FMCG Food', 'FMCG Non-Food'].includes(value as string)
  },
  {
    name: 'Item UID',
    required: false,
    type: 'string'
  },
  {
    name: 'Target Month',
    required: true,
    type: 'number',
    validator: (value) => {
      const num = Number(value);
      return Number.isInteger(num) && num >= 1 && num <= 12;
    }
  },
  {
    name: 'Target Year',
    required: true,
    type: 'number',
    validator: (value) => {
      const num = Number(value);
      return Number.isInteger(num) && num >= 2020 && num <= 2030;
    }
  },
  {
    name: 'Target Amount',
    required: true,
    type: 'number',
    validator: (value) => Number(value) > 0
  },
  {
    name: 'Achieved Amount',
    required: false,
    type: 'number'
  },
  {
    name: 'Status',
    required: false,
    type: 'string',
    validator: (value) => !value || ['Not Started', 'In Progress', 'Achieved', 'Failed'].includes(value as string)
  },
  {
    name: 'Notes',
    required: false,
    type: 'string'
  }
];

// Convert to ExcelUploadColumn format for the component
const TARGET_COLUMNS: ExcelUploadColumn[] = [
  {
    name: 'User Type',
    required: true,
    description: 'Route, Employee, or Customer'
  },
  {
    name: 'User Code',
    required: true,
    description: 'Unique identifier for the user'
  },
  {
    name: 'Customer Type',
    required: false,
    description: 'Customer, Store, or Outlet'
  },
  {
    name: 'Customer Code',
    required: false,
    description: 'Unique identifier for the customer'
  },
  {
    name: 'Category',
    required: false,
    description: 'All, Cosmetics, FMCG Food, or FMCG Non-Food'
  },
  {
    name: 'Item UID',
    required: false,
    description: 'Unique identifier for specific item'
  },
  {
    name: 'Target Month',
    required: true,
    description: 'Month number (1-12)'
  },
  {
    name: 'Target Year',
    required: true,
    description: 'Year (e.g., 2024)'
  },
  {
    name: 'Target Amount',
    required: true,
    description: 'Target sales amount (numeric)'
  },
  {
    name: 'Achieved Amount',
    required: false,
    description: 'Current achieved amount (numeric)'
  },
  {
    name: 'Status',
    required: false,
    description: 'Not Started, In Progress, Achieved, or Failed'
  },
  {
    name: 'Notes',
    required: false,
    description: 'Additional notes or comments'
  }
];

export default function TargetUploadPage() {
  const router = useRouter();
  const { toast } = useToast();

  const handleUpload = async (file: File): Promise<ExcelUploadResult> => {
    try {
      console.log('📁 Uploading file:', file.name, file.type);
      
      // First try to parse with standard format to get headers, then decide
      let parseResult;
      
      try {
        // Try standard parsing first to get the actual headers
        const standardResult = await parseExcelFile(file, TARGET_COLUMN_MAPPING);
        
        // Check if the error message indicates Multiplex format
        const errorMessage = standardResult.errors.join(' ');
        const hasMultiplexHeaders = errorMessage.includes('EmpNo') && 
                                  errorMessage.includes('CustID') && 
                                  errorMessage.includes('YearMon');
        
        if (hasMultiplexHeaders && standardResult.errors.length > 0) {
          console.log('🔍 Detected Multiplex Excel format from headers - using transformer');
          console.log('🔍 Multiplex headers found:', errorMessage);
          parseResult = await parseMultiplexExcel(file);
        } else {
          console.log('🔍 Using standard Target format parsing');
          parseResult = standardResult;
        }
      } catch (detectionError) {
        console.log('🔍 Format detection failed:', detectionError);
        // Fallback to Multiplex if standard parsing completely fails
        try {
          console.log('🔍 Trying Multiplex format as fallback');
          parseResult = await parseMultiplexExcel(file);
        } catch (multiplexError) {
          console.log('🔍 Multiplex parsing also failed:', multiplexError);
          throw detectionError;
        }
      }
      
      console.log('📊 Parse result:', parseResult);
      
      if (parseResult.errors.length > 0) {
        return {
          success: false,
          message: `Found ${parseResult.errors.length} validation errors`,
          errors: parseResult.errors
        };
      }
      
      if (parseResult.data.length === 0) {
        return {
          success: false,
          message: 'No valid target data found in the file',
          errors: ['File appears to be empty or all rows have validation errors']
        };
      }
      
      // Convert parsed data to Target format
      const targets = parseResult.data.map(row => ({
        UserLinkedType: row['User Type'] as string,
        UserLinkedUid: row['User Code'] as string,
        CustomerLinkedType: row['Customer Type'] as string || null,
        CustomerLinkedUid: row['Customer Code'] as string || null,
        ItemLinkedItemType: row['Category'] as string || null,
        ItemLinkedItemUid: row['Item UID'] as string || null,
        TargetMonth: row['Target Month'] as number,
        TargetYear: row['Target Year'] as number,
        TargetAmount: row['Target Amount'] as number,
        Status: (row['Status'] as string) || 'Not Started',
        Notes: row['Notes'] as string || null
      }));
      
      console.log('🎯 Final targets to upload:', targets);
      
      // Send to backend bulk API
      const response = await targetService.bulkCreateTargets(targets);
      
      return {
        success: true,
        message: `Successfully imported ${targets.length} targets`,
        count: targets.length
      };
      
    } catch (error: any) {
      console.error('Upload error:', error);
      return {
        success: false,
        message: 'Failed to process file',
        errors: [error.message || 'Unknown error occurred']
      };
    }
  };

  const handleDownloadTemplate = () => {
    const sampleData = [
      {
        'User Type': 'Route',
        'User Code': 'ROUTE001',
        'Customer Type': 'Customer',
        'Customer Code': 'CUST001',
        'Category': 'Cosmetics',
        'Item UID': 'ITEM001',
        'Target Month': 12,
        'Target Year': 2024,
        'Target Amount': 50000,
        'Achieved Amount': 25000,
        'Status': 'In Progress',
        'Notes': 'Sample target for December'
      }
    ];
    
    console.log('📋 Generating template with columns:', TARGET_COLUMN_MAPPING.map(col => col.name));
    console.log('📋 Sample data:', sampleData);
    
    // Generate Excel template
    generateExcelTemplate(TARGET_COLUMN_MAPPING, sampleData, 'target-upload-template.xlsx');
    
    toast({
      title: 'Template Downloaded',
      description: 'Excel template downloaded with sample data. Fill it out and upload when ready.',
    });
  };

  return (
    <div className="container mx-auto py-6 max-w-4xl">
      <div className="mb-6">
        <Button
          onClick={() => router.push('/distributiondelivery/route-management/target')}
          variant="ghost"
        >
          <ArrowLeft className="mr-2 h-4 w-4" />
          Back to Targets
        </Button>
      </div>

      <ExcelUpload
        title="Upload Targets from Excel"
        description="Import multiple targets at once using an Excel file"
        columns={TARGET_COLUMNS}
        onUpload={handleUpload}
        onDownloadTemplate={handleDownloadTemplate}
        maxFileSize={10}
        acceptedFormats={['.xlsx', '.xls', '.csv']}
        className="w-full"
      />
    </div>
  );
}