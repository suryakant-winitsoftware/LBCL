'use client';

import React, { useState, useCallback, useRef } from 'react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Alert, AlertDescription } from '@/components/ui/alert';
import { Download, Upload, FileSpreadsheet, CheckCircle, AlertCircle, Loader2, X } from 'lucide-react';
import * as XLSX from 'xlsx';
import { apiService } from '@/services/api';
import { routeService } from '@/services/routeService';
import { authService } from '@/lib/auth-service';
import { useToast } from '@/components/ui/use-toast';
import { employeeService } from '@/services/admin/employee.service';

interface ExcelRouteData {
  ROUTE_CODE: string;
  ROUTE_USER_ID: string;
  START_DATE: string;
  END_DATE: string;
  CUSTOMER_CODE: string;
  CUSTOMER_FREQUENCY: string;
  WEEK_NO: number | string;
  DAY: number | string; // Day number: 1-7 for weekly (Mon-Sun), 1-31 for monthly
}

interface EmployeeDetails {
  UID: string;
  Code: string;
  Name: string;
  OrgUID: string;
  RoleUID: string;
  JobPositionUID: string;
  Organization?: {
    UID: string;
    Name: string;
  };
  Role?: {
    UID: string;
    Name: string;
  };
}

interface CustomerDetails {
  UID: string;
  Code: string;
  Name: string;
  IsActive?: boolean;
}

interface ProcessedRouteData {
  routeCode: string;
  routeName: string;
  employeeUID: string;
  employeeName: string;
  orgUID: string;
  roleUID: string;
  startDate: string;
  endDate: string;
  customers: Array<{
    customerCode: string;
    customerUID?: string; // UID from validation
    customerName?: string; // Name from validation
    frequency: string;
    weekNo: number | string;
    day: number; // Converted to number for processing
    dayName?: string; // Optional day name for display
  }>;
  skippedCustomers?: number; // Track how many customers were skipped
  totalCustomersInExcel?: number; // Track original count
}

interface RouteExcelImportProps {
  selectedFrequency?: string; // Current frequency selection from parent
  onImportSuccess?: (routes: ProcessedRouteData[]) => void;
  onClose?: () => void;
}

interface FailedRecord {
  ROUTE_CODE: string;
  ROUTE_USER_ID: string;
  START_DATE: string;
  END_DATE: string;
  CUSTOMER_CODE: string;
  CUSTOMER_FREQUENCY: string;
  WEEK_NO: number | string;
  DAY: number | string;
  ERROR_REASON: string;
  ERROR_TYPE: 'EMPLOYEE_NOT_FOUND' | 'CUSTOMER_NOT_FOUND' | 'INACTIVE_CUSTOMER' | 'MISSING_ORG_ROLE';
}

export default function RouteExcelImport({ selectedFrequency, onImportSuccess, onClose }: RouteExcelImportProps) {
  const [file, setFile] = useState<File | null>(null);
  const [isProcessing, setIsProcessing] = useState(false);
  const [uploadStatus, setUploadStatus] = useState<'idle' | 'processing' | 'success' | 'error'>('idle');
  const [processedData, setProcessedData] = useState<ProcessedRouteData[]>([]);
  const [errors, setErrors] = useState<string[]>([]);
  const [employeeCache, setEmployeeCache] = useState<Map<string, EmployeeDetails>>(new Map());
  const [customerCache, setCustomerCache] = useState<Map<string, CustomerDetails>>(new Map());
  const [validationMessage, setValidationMessage] = useState<string>('');
  const [failedRecords, setFailedRecords] = useState<FailedRecord[]>([]);
  const [successfulRecords, setSuccessfulRecords] = useState<any[]>([]);
  const [skipValidation, setSkipValidation] = useState(false); // TEST MODE
  const [routesCreated, setRoutesCreated] = useState(false); // Track if routes were successfully created
  const { toast } = useToast();
  
  // Cache for all employees using ref (similar to employee-grid.tsx)
  const allEmployeesCache = useRef<any[] | null>(null);

  // Download Excel template
  const downloadTemplate = () => {
    // Generate current date and one year from now
    const today = new Date();
    const nextYear = new Date();
    nextYear.setFullYear(today.getFullYear() + 1);
    
    // Format dates as DD/MM/YYYY
    const formatDate = (date: Date) => {
      const day = date.getDate().toString().padStart(2, '0');
      const month = (date.getMonth() + 1).toString().padStart(2, '0');
      const year = date.getFullYear();
      return `${day}/${month}/${year}`;
    };
    
    const startDate = formatDate(today);
    const endDate = formatDate(nextYear);
    
    const templateData = [
      {
        ROUTE_CODE: 'RT-DEL-001',
        ROUTE_USER_ID: 'U1001',
        START_DATE: startDate,
        END_DATE: endDate,
        CUSTOMER_CODE: 'CUST1001',
        CUSTOMER_FREQUENCY: 'WEEKLY',
        WEEK_NO: 1,
        DAY: 1  // Monday (1-7 for weekly)
      },
      {
        ROUTE_CODE: 'RT-DEL-001',
        ROUTE_USER_ID: 'U1001',
        START_DATE: startDate,
        END_DATE: endDate,
        CUSTOMER_CODE: 'CUST1002',
        CUSTOMER_FREQUENCY: 'WEEKLY',
        WEEK_NO: 1,
        DAY: 3  // Wednesday
      },
      {
        ROUTE_CODE: 'RT-DEL-002',
        ROUTE_USER_ID: 'U1001',
        START_DATE: startDate,
        END_DATE: endDate,
        CUSTOMER_CODE: 'CUST2001',
        CUSTOMER_FREQUENCY: 'MONTHLY',
        WEEK_NO: 'NA',
        DAY: 15  // 15th of each month (1-31 for monthly)
      },
      {
        ROUTE_CODE: 'RT-DEL-002',
        ROUTE_USER_ID: 'U1001',
        START_DATE: startDate,
        END_DATE: endDate,
        CUSTOMER_CODE: 'CUST2002',
        CUSTOMER_FREQUENCY: 'MONTHLY',
        WEEK_NO: 'NA',
        DAY: 28  // 28th of each month
      }
    ];

    const ws = XLSX.utils.json_to_sheet(templateData);
    const wb = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(wb, ws, 'Route Template');
    
    // Add column widths for better readability
    const cols = [
      { wch: 15 }, // ROUTE_CODE
      { wch: 15 }, // ROUTE_USER_ID
      { wch: 12 }, // START_DATE
      { wch: 12 }, // END_DATE
      { wch: 15 }, // CUSTOMER_CODE
      { wch: 20 }, // CUSTOMER_FREQUENCY
      { wch: 10 }, // WEEK_NO
      { wch: 10 }  // DAY
    ];
    ws['!cols'] = cols;

    XLSX.writeFile(wb, 'route_import_template.xlsx');
    toast({
      title: 'Template Downloaded',
      description: 'Excel template has been downloaded successfully.',
    });
  };

  // Export successful records
  const exportSuccessRecords = () => {
    if (successfulRecords.length === 0) {
      toast({
        title: 'No Data',
        description: 'No successful records to export.',
        variant: 'destructive',
      });
      return;
    }

    const ws = XLSX.utils.json_to_sheet(successfulRecords);
    const wb = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(wb, ws, 'Successful Records');
    
    // Add column widths
    const cols = [
      { wch: 15 }, { wch: 15 }, { wch: 12 }, { wch: 12 }, 
      { wch: 15 }, { wch: 20 }, { wch: 10 }, { wch: 10 },
      { wch: 20 }, { wch: 15 }, { wch: 25 }, { wch: 20 }, { wch: 10 }
    ];
    ws['!cols'] = cols;

    const fileName = `route_import_success_${new Date().toISOString().split('T')[0]}.xlsx`;
    XLSX.writeFile(wb, fileName);
    
    toast({
      title: 'Export Successful',
      description: `${successfulRecords.length} successful records exported.`,
    });
  };

  // Export failed records
  const exportFailedRecords = () => {
    if (failedRecords.length === 0) {
      toast({
        title: 'No Data',
        description: 'No failed records to export.',
        variant: 'destructive',
      });
      return;
    }

    const ws = XLSX.utils.json_to_sheet(failedRecords);
    const wb = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(wb, ws, 'Failed Records');
    
    // Add column widths
    const cols = [
      { wch: 15 }, { wch: 15 }, { wch: 12 }, { wch: 12 }, 
      { wch: 15 }, { wch: 20 }, { wch: 10 }, { wch: 10 },
      { wch: 40 }, { wch: 20 }
    ];
    ws['!cols'] = cols;

    const fileName = `route_import_failures_${new Date().toISOString().split('T')[0]}.xlsx`;
    XLSX.writeFile(wb, fileName);
    
    toast({
      title: 'Export Successful',
      description: `${failedRecords.length} failed records exported.`,
    });
  };

  // Export available customers/stores list
  const exportAvailableCustomers = async () => {
    try {
      
      // Use the correct API structure from store-linking.service.ts
      const searchPayload = {
        PageNumber: 1,
        PageSize: -1, // Load all customers without pagination limit
        FilterCriterias: [], // NO FILTERS to avoid SQL bug
        SortCriterias: [{ SortParameter: "Name", Direction: "Asc" }],
        IsCountRequired: true
      };

      const response = await apiService.post('/Store/SelectAllStore', searchPayload);
      console.log('Customer API response:', response);
      
      let stores = [];
      if (response.data) {
        if (response.data.PagedData && Array.isArray(response.data.PagedData)) {
          stores = response.data.PagedData;
        } else if (response.data.Data?.PagedData && Array.isArray(response.data.Data.PagedData)) {
          stores = response.data.Data.PagedData;
        } else if (response.data.Data && Array.isArray(response.data.Data)) {
          stores = response.data.Data;
        } else if (response.data.pagedData && Array.isArray(response.data.pagedData)) {
          stores = response.data.pagedData;
        }
      }
      
      if (stores.length === 0) {
        toast({
          title: 'No Data',
          description: 'No customers/stores found in system.',
          variant: 'destructive',
        });
        return;
      }

      // Prepare customer data for export
      const customerData = stores.map((store: any) => ({
        Customer_Code: store.Code || store.code,
        Customer_Name: store.Name || store.name || store.StoreName,
        Store_UID: store.UID || store.uid,
        Is_Active: store.IsActive !== false ? 'Yes' : 'No',
        Address: store.Address1 || store.address || '',
        Phone: store.Phone || store.phone || '',
        Email: store.Email || store.email || ''
      }));

      const ws = XLSX.utils.json_to_sheet(customerData);
      const wb = XLSX.utils.book_new();
      XLSX.utils.book_append_sheet(wb, ws, 'Available Customers');
      
      // Add column widths
      const cols = [
        { wch: 15 }, { wch: 30 }, { wch: 15 }, { wch: 10 }, 
        { wch: 30 }, { wch: 15 }, { wch: 25 }
      ];
      ws['!cols'] = cols;

      const fileName = `available_customers_${new Date().toISOString().split('T')[0]}.xlsx`;
      XLSX.writeFile(wb, fileName);
      
      toast({
        title: 'Export Successful',
        description: `${customerData.length} available customers exported. Use these codes in your Excel file.`,
      });
    } catch (error) {
      console.error('Error exporting customers:', error);
      toast({
        title: 'Export Failed',
        description: 'Failed to export customer list.',
        variant: 'destructive',
      });
    }
  };

  // Export available employees list
  const exportAvailableEmployees = async () => {
    try {
      // Load employees if not already cached
      if (!allEmployeesCache.current) {
        const pagingRequest = employeeService.buildPagingRequest(1, 100, undefined, undefined, undefined, undefined);
        const response = await employeeService.getEmployees(pagingRequest);
        if (response && response.pagedData) {
          allEmployeesCache.current = response.pagedData;
        }
      }
      
      if (!allEmployeesCache.current || allEmployeesCache.current.length === 0) {
        toast({
          title: 'No Data',
          description: 'No employees found in system.',
          variant: 'destructive',
        });
        return;
      }

      // Prepare employee data for export
      const employeeData = allEmployeesCache.current.map((emp: any) => ({
        Employee_Code: emp.Code || emp.code,
        Employee_Name: emp.Name || emp.name,
        Login_ID: emp.LoginId || emp.loginId,
        Email: emp.Email || emp.email,
        Status: emp.Status || emp.status,
        Org_UID: emp.OrgUID || emp.orgUID,
        Role_UID: emp.RoleUID || emp.roleUID
      }));

      const ws = XLSX.utils.json_to_sheet(employeeData);
      const wb = XLSX.utils.book_new();
      XLSX.utils.book_append_sheet(wb, ws, 'Available Employees');
      
      // Add column widths
      const cols = [
        { wch: 15 }, { wch: 25 }, { wch: 15 }, { wch: 25 }, 
        { wch: 10 }, { wch: 15 }, { wch: 15 }
      ];
      ws['!cols'] = cols;

      const fileName = `available_employees_${new Date().toISOString().split('T')[0]}.xlsx`;
      XLSX.writeFile(wb, fileName);
      
      toast({
        title: 'Export Successful',
        description: `${employeeData.length} available employees exported. Use these codes in your Excel file.`,
      });
    } catch (error) {
      console.error('Error exporting employees:', error);
      toast({
        title: 'Export Failed',
        description: 'Failed to export employee list.',
        variant: 'destructive',
      });
    }
  };

  // Export combined report
  const exportCombinedReport = () => {
    if (successfulRecords.length === 0 && failedRecords.length === 0) {
      toast({
        title: 'No Data',
        description: 'No data to export.',
        variant: 'destructive',
      });
      return;
    }

    const wb = XLSX.utils.book_new();
    
    // Add summary sheet
    const summaryData = [
      { Metric: 'Total Records Processed', Count: successfulRecords.length + failedRecords.length },
      { Metric: 'Successful Records', Count: successfulRecords.length },
      { Metric: 'Failed Records', Count: failedRecords.length },
      { Metric: 'Success Rate', Count: `${Math.round((successfulRecords.length / (successfulRecords.length + failedRecords.length)) * 100)}%` },
      { Metric: 'Routes Created', Count: processedData.length },
      { Metric: 'Export Date', Count: new Date().toLocaleString() }
    ];
    
    const summaryWs = XLSX.utils.json_to_sheet(summaryData);
    XLSX.utils.book_append_sheet(wb, summaryWs, 'Summary');
    
    // Add successful records sheet
    if (successfulRecords.length > 0) {
      const successWs = XLSX.utils.json_to_sheet(successfulRecords);
      XLSX.utils.book_append_sheet(wb, successWs, 'Successful Records');
    }
    
    // Add failed records sheet
    if (failedRecords.length > 0) {
      const failedWs = XLSX.utils.json_to_sheet(failedRecords);
      XLSX.utils.book_append_sheet(wb, failedWs, 'Failed Records');
    }

    const fileName = `route_import_report_${new Date().toISOString().split('T')[0]}.xlsx`;
    XLSX.writeFile(wb, fileName);
    
    toast({
      title: 'Report Exported',
      description: 'Complete import report with summary, success, and failure data exported.',
    });
  };

  // Fetch employee details including org and role (using same approach as employee-grid.tsx)
  const fetchEmployeeDetails = async (employeeCode: string): Promise<EmployeeDetails | null> => {
    
    // Check individual cache first
    if (employeeCache.has(employeeCode)) {
      const cachedEmployee = employeeCache.get(employeeCode)!;
      console.log(`✅ Found employee in cache: ${employeeCode}`, cachedEmployee);
      console.log(`🔍 Cached employee details:`, {
        UID: cachedEmployee.UID,
        Code: cachedEmployee.Code,
        Name: cachedEmployee.Name,
        OrgUID: cachedEmployee.OrgUID,
        RoleUID: cachedEmployee.RoleUID,
        hasOrgUID: !!cachedEmployee.OrgUID,
        hasRoleUID: !!cachedEmployee.RoleUID
      });
      return cachedEmployee;
    }

    try {
      // Load all employees if not cached (same approach as employee-grid.tsx)
      if (!allEmployeesCache.current) {
        console.log('Loading all employees using same pattern as employee-grid.tsx...');
        
        // Use the exact same pattern as employee-grid.tsx line 82-91
        const pagingRequest = employeeService.buildPagingRequest(
          1,
          100, // Start with just 100 employees
          undefined, // No search
          undefined, // No filters
          undefined,
          undefined
        );

        console.log('Loading all employees with payload:', pagingRequest);
        
        const response = await employeeService.getEmployees(pagingRequest);
        console.log('All employees response:', response);
        
        if (response && response.pagedData) {
          allEmployeesCache.current = response.pagedData;
          console.log('Cached', response.pagedData.length, 'employees');
          console.log('Available employee codes:', response.pagedData.map((emp: any) => emp.Code || emp.code));
        } else {
          console.warn('No employee data in response');
          allEmployeesCache.current = [];
        }
      }
      
      // Find employee by code from cached data
      if (allEmployeesCache.current) {
        const employee = allEmployeesCache.current.find((emp: any) => 
          emp.Code === employeeCode || emp.code === employeeCode
        );
        
        if (employee) {
          console.log(`✅ Employee found in basic list:`, employee);
          console.log(`🔍 Raw employee fields available:`, Object.keys(employee));
          
          // The basic employee list doesn't have JobPosition data
          // Need to fetch full employee details using getEmployeeById
          console.log(`🌐 Fetching full employee details for ${employeeCode}...`);
          
          try {
            const fullEmployeeData = await employeeService.getEmployeeById(employee.UID);
            console.log(`✅ Full employee data received:`, fullEmployeeData);
            console.log(`🔍 JobPosition data:`, fullEmployeeData?.JobPosition);
            
            const employeeDetails: EmployeeDetails = {
              UID: employee.UID || employee.uid,
              Code: employee.Code || employee.code || employeeCode,
              Name: employee.Name || employee.name || 'Unknown',
              OrgUID: fullEmployeeData?.JobPosition?.OrgUID || fullEmployeeData?.JobPosition?.OrganizationUID || fullEmployeeData?.OrgUID,
              RoleUID: fullEmployeeData?.JobPosition?.UserRoleUID || fullEmployeeData?.RoleUID,
              JobPositionUID: fullEmployeeData?.JobPosition?.UID || employee.JobPositionUID
            };
            
            console.log(`🔍 Employee details extracted:`, {
              UID: employeeDetails.UID,
              Code: employeeDetails.Code,
              Name: employeeDetails.Name,
              OrgUID: employeeDetails.OrgUID,
              RoleUID: employeeDetails.RoleUID,
              hasOrgUID: !!employeeDetails.OrgUID,
              hasRoleUID: !!employeeDetails.RoleUID
            });
            
            // Cache the result for quick access
            employeeCache.set(employeeCode, employeeDetails);
            return employeeDetails;
          } catch (detailError) {
            console.error(`❌ Failed to fetch full employee details for ${employeeCode}:`, detailError);
            
            // Fallback to basic data (will likely fail org/role validation)
            const employeeDetails: EmployeeDetails = {
              UID: employee.UID || employee.uid,
              Code: employee.Code || employee.code || employeeCode,
              Name: employee.Name || employee.name || 'Unknown',
              OrgUID: employee.OrgUID || employee.orgUID,
              RoleUID: employee.RoleUID || employee.roleUID,
              JobPositionUID: employee.JobPositionUID || employee.jobPositionUID
            };
            
            employeeCache.set(employeeCode, employeeDetails);
            return employeeDetails;
          }
        }
      }
      
      console.log(`⚠️ No employee found with code: ${employeeCode}`);
      return null;
    } catch (error) {
      console.error(`❌ Error fetching employee ${employeeCode}:`, error);
      return null;
    }
  };

  // Cache for all customers (similar to employee cache)
  const allCustomersCache = useRef<any[] | null>(null);

  // Fetch and validate customer details (using same approach as store service)
  const fetchCustomerDetails = async (customerCode: string): Promise<CustomerDetails | null> => {
    console.log(`🔍 Fetching customer details for: ${customerCode}`);
    console.log(`📋 allCustomersCache status: ${allCustomersCache.current ? `${allCustomersCache.current.length} customers` : 'null'}`);
    
    // Check individual cache first
    if (customerCache.has(customerCode)) {
      console.log(`✅ Found customer in cache: ${customerCode}`);
      return customerCache.get(customerCode)!;
    }

    try {
      // Load ALL customers using pagination
      if (!allCustomersCache.current || allCustomersCache.current.length === 0) {
        console.log('🚀 Loading ALL customers from database using pagination...');
        
        let allStores: any[] = [];
        let pageNumber = 1;
        const pageSize = 10000; // Large page size but not -1
        let hasMorePages = true;
        
        while (hasMorePages) {
          console.log(`📋 Loading customers page ${pageNumber} (size: ${pageSize})...`);
          
          const searchPayload = {
            PageNumber: pageNumber,
            PageSize: pageSize,
            FilterCriterias: [], // NO FILTERS to avoid SQL bug
            SortCriterias: [{ SortParameter: "Name", Direction: "Asc" }],
            IsCountRequired: true
          };
          
          try {
            const response = await apiService.post('/Store/SelectAllStore', searchPayload);
            
            // Extract stores from the response
            let stores = [];
            if (response && typeof response === 'object') {
              if (response.PagedData !== undefined) {
                stores = response.PagedData;
              } else if (response.Data && response.Data.PagedData !== undefined) {
                stores = response.Data.PagedData;
              } else if (response.data && response.data.PagedData !== undefined) {
                stores = response.data.PagedData;
              } else if (response.pagedData !== undefined) {
                stores = response.pagedData;
              } else if (response.data && response.data.pagedData !== undefined) {
                stores = response.data.pagedData;
              }
            }
            
            if (stores && stores.length > 0) {
              allStores = allStores.concat(stores);
              console.log(`✅ Loaded ${stores.length} customers from page ${pageNumber} (Total: ${allStores.length})`);
              
              // Check for more pages
              const totalRecords = response?.Data?.TotalRecords || 
                                 response?.data?.TotalRecords || 
                                 response?.TotalRecords || 
                                 response?.totalRecords || 0;
              
              if (totalRecords > 0) {
                hasMorePages = allStores.length < totalRecords;
              } else {
                // If we got less than pageSize, we've reached the end
                hasMorePages = stores.length === pageSize;
              }
              
              pageNumber++;
            } else {
              console.log(`📄 No more customers found`);
              hasMorePages = false;
            }
            
            // Safety check
            if (pageNumber > 50) {
              console.warn('⚠️ Stopped at page 50 for safety');
              break;
            }
          } catch (pageError: any) {
            console.error(`❌ Error loading page ${pageNumber}:`, pageError);
            
            // If we get a 400 error, show a user-friendly message
            if (pageError.status === 400) {
              console.error('⚠️ API Error: The server rejected the request. This might be a configuration issue.');
              toast({
                title: 'Failed to Load Customers',
                description: 'Could not load store/customer data from the server. Please contact support if this persists.',
                variant: 'destructive',
              });
            }
            
            hasMorePages = false;
          }
        }
        
        allCustomersCache.current = allStores;
        console.log(`🎯 Total customers cached: ${allStores.length}`);
        
        // More detailed logging to debug customer codes
        const customerCodes = allStores.map((store: any) => store.Code || store.code);
        console.log('All customer codes (first 50):', customerCodes.slice(0, 50));
        console.log('Customer codes data types:', customerCodes.slice(0, 10).map(code => `${code} (${typeof code})`));
        
        // Check if the specific missing codes exist
        const missingCodes = ['1000', '1001', '1002', '1016', '1017', '1018'];
        missingCodes.forEach(code => {
          const found = allStores.find((store: any) => 
            store.Code === code || store.code === code || 
            store.Code === parseInt(code) || store.code === parseInt(code) ||
            store.Code?.toString() === code || store.code?.toString() === code
          );
          console.log(`🔍 Customer ${code} search result:`, found ? `FOUND: ${found.Code || found.code}` : 'NOT FOUND');
        });
      }
      
      // Find customer by code from cached data - try multiple variations
      if (allCustomersCache.current) {
        // Normalize the customer code for comparison
        const normalizedCustomerCode = String(customerCode).trim();
        
        const store = allCustomersCache.current.find((s: any) => {
          const storeCode = s.Code || s.code;
          if (!storeCode) return false;
          
          const normalizedStoreCode = String(storeCode).trim();
          
          // Try multiple matching strategies
          const matches = 
            // Exact string match (case-sensitive)
            normalizedStoreCode === normalizedCustomerCode ||
            // Case-insensitive match
            normalizedStoreCode.toLowerCase() === normalizedCustomerCode.toLowerCase() ||
            // Numeric comparison if both are numeric
            (!isNaN(Number(normalizedStoreCode)) && !isNaN(Number(normalizedCustomerCode)) && 
             Number(normalizedStoreCode) === Number(normalizedCustomerCode));
          
          if (matches) {
            console.log(`✅ Found matching customer: Excel[${customerCode}] === DB[${storeCode}]`);
          }
          
          return matches;
        });
        
        if (store) {
          console.log(`✅ Customer found in cache:`, store);
          
          const customer: CustomerDetails = {
            UID: store.UID || store.uid,
            Code: store.Code || store.code || customerCode,
            Name: store.Name || store.name || store.StoreName || 'Unknown',
            IsActive: store.IsActive !== false // Default to true if not specified
          };

          // Cache the result for quick access
          customerCache.set(customerCode, customer);
          return customer;
        }
      }
      
      console.log(`⚠️ No customer/store found with code: ${customerCode}`);
      return null;
    } catch (error) {
      console.error(`❌ Error fetching customer ${customerCode}:`, error);
      return null;
    }
  };

  // Process Excel file
  const processExcelFile = async (file: File) => {
    return new Promise<ExcelRouteData[]>((resolve, reject) => {
      const reader = new FileReader();
      
      reader.onload = async (e) => {
        try {
          const data = new Uint8Array(e.target?.result as ArrayBuffer);
          const workbook = XLSX.read(data, { type: 'array' });
          const sheetName = workbook.SheetNames[0];
          const worksheet = workbook.Sheets[sheetName];
          const jsonData = XLSX.utils.sheet_to_json<ExcelRouteData>(worksheet);
          
          // Validate required columns
          if (jsonData.length === 0) {
            throw new Error('Excel file is empty');
          }
          
          const requiredColumns = ['ROUTE_CODE', 'ROUTE_USER_ID', 'START_DATE', 'END_DATE', 'CUSTOMER_CODE', 'CUSTOMER_FREQUENCY', 'WEEK_NO', 'DAY'];
          const firstRow = jsonData[0];
          const missingColumns = requiredColumns.filter(col => !(col in firstRow));
          
          if (missingColumns.length > 0) {
            throw new Error(`Missing required columns: ${missingColumns.join(', ')}`);
          }
          
          resolve(jsonData);
        } catch (error) {
          reject(error);
        }
      };
      
      reader.onerror = () => reject(new Error('Failed to read file'));
      reader.readAsArrayBuffer(file);
    });
  };

  // Helper function to convert day number to day name
  const getDayName = (dayNumber: number, frequency: string): string => {
    if (frequency.toUpperCase() === 'WEEKLY') {
      const weekDays = ['Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday', 'Sunday'];
      return weekDays[dayNumber - 1] || `Day ${dayNumber}`;
    }
    return `Day ${dayNumber}`; // For monthly, just return the day number
  };

  // Group and process route data
  const processRouteData = async (excelData: ExcelRouteData[]): Promise<ProcessedRouteData[]> => {
    const routeMap = new Map<string, ProcessedRouteData>();
    const localErrors: string[] = [];
    const routeCustomerCounts = new Map<string, number>(); // Track total customers per route
    const warnings: string[] = [];
    const localFailedRecords: FailedRecord[] = [];
    const localSuccessfulRecords: any[] = [];
    const failedEmployeeRoutes = new Set<string>(); // Track routes with failed employees
    
    // First pass: count total customers per route
    for (const row of excelData) {
      const routeKey = `${row.ROUTE_CODE}_${row.ROUTE_USER_ID}`;
      routeCustomerCounts.set(routeKey, (routeCustomerCounts.get(routeKey) || 0) + 1);
    }
    
    // Show validation progress
    console.log(`🚀 Starting validation of ${excelData.length} rows`);
    console.log(`📊 Unique routes to create: ${routeCustomerCounts.size}`);
    console.log(`🗂️ Route customer counts:`, Object.fromEntries(routeCustomerCounts));
    
    setValidationMessage('Validating employees and customers...');
    
    for (const row of excelData) {
      const routeKey = `${row.ROUTE_CODE}_${row.ROUTE_USER_ID}`;
      console.log(`🔄 Processing row: ${JSON.stringify(row)}`);
      
      if (!routeMap.has(routeKey)) {
        console.log(`📝 Creating new route for: ${routeKey}`);
        // Fetch employee details
        const employee = await fetchEmployeeDetails(row.ROUTE_USER_ID);
        
        if (!employee) {
          const customerCount = routeCustomerCounts.get(routeKey);
          warnings.push(`⚠️ EMPLOYEE NOT FOUND: "${row.ROUTE_USER_ID}" - route skipped`);
          failedEmployeeRoutes.add(routeKey);
          
          // Add all rows for this failed employee to failed records
          const allRowsForThisRoute = excelData.filter(r => 
            `${r.ROUTE_CODE}_${r.ROUTE_USER_ID}` === routeKey
          );
          
          allRowsForThisRoute.forEach(failedRow => {
            localFailedRecords.push({
              ...failedRow,
              ERROR_REASON: `Employee "${row.ROUTE_USER_ID}" not found. Create this employee first.`,
              ERROR_TYPE: 'EMPLOYEE_NOT_FOUND',
              ROUTE_START_DATE: row.START_DATE,
              ROUTE_END_DATE: row.END_DATE
            });
          });
          
          continue; // Skip this route, but continue with others
        }
        
        if (!employee.OrgUID || !employee.RoleUID) {
          warnings.push(`⚠️ INCOMPLETE EMPLOYEE: "${row.ROUTE_USER_ID}" missing org/role - route skipped`);
          failedEmployeeRoutes.add(routeKey);
          
          // Add all rows for this failed employee to failed records
          const allRowsForThisRoute = excelData.filter(r => 
            `${r.ROUTE_CODE}_${r.ROUTE_USER_ID}` === routeKey
          );
          
          allRowsForThisRoute.forEach(failedRow => {
            localFailedRecords.push({
              ...failedRow,
              ERROR_REASON: `Employee "${row.ROUTE_USER_ID}" missing organization or role.`,
              ERROR_TYPE: 'MISSING_ORG_ROLE',
              ROUTE_START_DATE: row.START_DATE,
              ROUTE_END_DATE: row.END_DATE
            });
          });
          
          continue; // Skip this route, but continue with others
        }
        
        // Generate route name as username_route
        const routeName = `${employee.Name}_${row.ROUTE_CODE}`;
        
        console.log(`✅ Creating route in map for ${routeKey}:`, {
          routeCode: row.ROUTE_CODE,
          routeName: routeName,
          employeeUID: employee.UID,
          employeeName: employee.Name,
          orgUID: employee.OrgUID,
          roleUID: employee.RoleUID,
          hasOrgUID: !!employee.OrgUID,
          hasRoleUID: !!employee.RoleUID
        });
        
        // Helper function to convert Excel date (number) to YYYY-MM-DD format
        const convertExcelDate = (excelDate: any): string => {
          if (typeof excelDate === 'number') {
            // Excel date serial number - convert to JavaScript Date
            const date = new Date((excelDate - 25569) * 86400 * 1000);
            return date.toISOString().split('T')[0]; // YYYY-MM-DD format
          } else if (typeof excelDate === 'string') {
            // Already a string - try to parse and format
            const date = new Date(excelDate);
            if (!isNaN(date.getTime())) {
              return date.toISOString().split('T')[0];
            }
          }
          // Fallback to current date if parsing fails
          return new Date().toISOString().split('T')[0];
        };

        const startDateFormatted = convertExcelDate(row.START_DATE);
        const endDateFormatted = convertExcelDate(row.END_DATE);
        
        console.log(`📅 Date conversion - START_DATE: ${row.START_DATE} -> ${startDateFormatted}, END_DATE: ${row.END_DATE} -> ${endDateFormatted}`);

        routeMap.set(routeKey, {
          routeCode: row.ROUTE_CODE,
          routeName: routeName,
          employeeUID: employee.UID,
          employeeName: employee.Name,
          orgUID: employee.OrgUID,
          roleUID: employee.RoleUID,
          startDate: startDateFormatted,
          endDate: endDateFormatted,
          customers: [],
          skippedCustomers: 0,
          totalCustomersInExcel: routeCustomerCounts.get(routeKey) || 0
        });
        
        console.log(`✅ Route added to map. Total routes in map: ${routeMap.size}`);
      }
      
      // Skip if route was not created due to employee issues
      if (!routeMap.has(routeKey)) {
        console.log(`❌ Route ${routeKey} not in map. Available routes:`, Array.from(routeMap.keys()));
        // Skip this row as the failed records were already added above
        continue;
      }
      
      console.log(`✅ Route ${routeKey} found in map, proceeding to customer validation`);
      
      // Validate customer exists
      const route = routeMap.get(routeKey)!;
      console.log(`🔍 Fetching customer details for: ${row.CUSTOMER_CODE}`);
      const customer = await fetchCustomerDetails(row.CUSTOMER_CODE);
      
      if (!customer) {
        console.warn(`⚠️ Customer ${row.CUSTOMER_CODE} not found`);
        warnings.push(`⚠️ STORE NOT FOUND: "${row.CUSTOMER_CODE}" does not exist in the system`);
        warnings.push(`   → Route "${row.ROUTE_CODE}" will be created WITHOUT this store`);
        warnings.push(`   → Store can be added later after creating it in the system`);
        route.skippedCustomers = (route.skippedCustomers || 0) + 1;
        localFailedRecords.push({
          ...row,
          ERROR_REASON: `Store "${row.CUSTOMER_CODE}" not found. Create this store first, then add to route.`,
          ERROR_TYPE: 'CUSTOMER_NOT_FOUND',
          ROUTE_START_DATE: route.startDate,
          ROUTE_END_DATE: route.endDate
        });
        continue; // Skip this customer
      }
      
      if (!customer.IsActive) {
        console.warn(`⚠️ Customer ${row.CUSTOMER_CODE} is inactive`);
        warnings.push(`⚠️ INACTIVE STORE: "${row.CUSTOMER_CODE}" (${customer.Name}) is marked as inactive`);
        warnings.push(`   → Route "${row.ROUTE_CODE}" will be created WITHOUT this store`);
        warnings.push(`   → Activate the store first, then add to route`);
        route.skippedCustomers = (route.skippedCustomers || 0) + 1;
        localFailedRecords.push({
          ...row,
          ERROR_REASON: `Store "${row.CUSTOMER_CODE}" is inactive. Activate the store first, then add to route.`,
          ERROR_TYPE: 'INACTIVE_CUSTOMER',
          ROUTE_START_DATE: route.startDate,
          ROUTE_END_DATE: route.endDate
        });
        continue; // Skip inactive customers
      }
      
      console.log(`✅ Customer ${row.CUSTOMER_CODE} found:`, customer.Name);

      // Convert day to number and validate
      let dayNumber = typeof row.DAY === 'string' ? parseInt(row.DAY, 10) : row.DAY;
      
      // Validate day based on frequency
      const frequency = row.CUSTOMER_FREQUENCY.toUpperCase();
      if (frequency === 'WEEKLY' && (dayNumber < 1 || dayNumber > 7)) {
        localErrors.push(`Invalid day number ${dayNumber} for weekly frequency (must be 1-7) for customer ${row.CUSTOMER_CODE}`);
        dayNumber = 1; // Default to Monday
      } else if (frequency === 'MONTHLY' && (dayNumber < 1 || dayNumber > 31)) {
        localErrors.push(`Invalid day number ${dayNumber} for monthly frequency (must be 1-31) for customer ${row.CUSTOMER_CODE}`);
        dayNumber = 1; // Default to 1st
      }
      
      // Add customer to route with validated details
      route.customers.push({
        customerCode: row.CUSTOMER_CODE,
        customerUID: customer.UID,
        customerName: customer.Name,
        frequency: row.CUSTOMER_FREQUENCY,
        weekNo: row.WEEK_NO === 'NA' ? 0 : (typeof row.WEEK_NO === 'string' ? parseInt(row.WEEK_NO, 10) : row.WEEK_NO),
        day: dayNumber,
        dayName: getDayName(dayNumber, row.CUSTOMER_FREQUENCY)
      });
      
      // Add to successful records
      localSuccessfulRecords.push({
        ...row,
        CUSTOMER_NAME: customer.Name,
        CUSTOMER_UID: customer.UID,
        ROUTE_NAME: route.routeName,
        EMPLOYEE_NAME: route.employeeName,
        ROUTE_START_DATE: route.startDate,
        ROUTE_END_DATE: route.endDate,
        STATUS: 'SUCCESS'
      });
    }
    
    // Simple validation summary
    const allIssues = [];
    const validRoutes = Array.from(routeMap.values()).filter(route => route.customers.length > 0);
    const skippedEmployees = Array.from(failedEmployeeRoutes).map(key => key.split('_')[1]);
    const totalCustomersProcessed = localSuccessfulRecords.length;
    const totalCustomersFailed = localFailedRecords.length;
    
    // Show what will be processed
    if (validRoutes.length > 0) {
      allIssues.push(`✅ Processing ${validRoutes.length} route(s) with ${totalCustomersProcessed} store(s)`);
      
      // Show what will be skipped
      if (skippedEmployees.length > 0) {
        allIssues.push(`⚠️ Skipping ${skippedEmployees.length} route(s) - employees not found`);
      }
      if (totalCustomersFailed > 0) {
        const skippedStores = totalCustomersFailed - Array.from(failedEmployeeRoutes).reduce((sum, key) => {
          return sum + (routeCustomerCounts.get(key) || 0);
        }, 0);
        if (skippedStores > 0) {
          allIssues.push(`⚠️ Skipping ${skippedStores} store(s) - not found or inactive`);
        }
      }
    } else {
      allIssues.push('❌ No routes can be created - check your data');
    }
    
    // Filter out routes with no valid customers, but keep routes even if they have no customers for reporting
    const allRoutes = Array.from(routeMap.values());
    const routesWithCustomers = allRoutes.filter(route => route.customers.length > 0);
    
    console.log(`📋 Total routes created: ${allRoutes.length}`);
    console.log(`📋 Routes with valid customers: ${routesWithCustomers.length}`);
    console.log(`📋 Routes without customers: ${allRoutes.length - routesWithCustomers.length}`);
    
    // If we have routes but no customers, it means customer validation failed
    if (allRoutes.length > 0 && routesWithCustomers.length === 0) {
      allIssues.push('');
      allIssues.push('🔍 CUSTOMER VALIDATION ISSUE:');
      allIssues.push('All customer codes were not found in the system.');
      allIssues.push('This usually means:');
      allIssues.push('1. Customer codes in Excel don\'t exist in your database');
      allIssues.push('2. Customer codes should be text (like "CUST001") not numbers (like 1000)');
      allIssues.push('💡 Check your customer/store master data for correct codes');
    }
    
    // Only set validation errors if we're not going to show route creation results later
    setErrors(allIssues);
    setValidationMessage('');
    
    // Store failed and successful records for export
    setFailedRecords(localFailedRecords);
    setSuccessfulRecords(localSuccessfulRecords);
    
    return routesWithCustomers;
  };

  // Create routes using the same API structure as manual creation

  const createRoutesFromImport = async (routes: ProcessedRouteData[]) => {
    try {
      const currentUser = authService.getCurrentUser();
      const currentUserUID = currentUser?.uid || "SYSTEM";
      const createdRoutes: string[] = [];
      const failedRoutes: string[] = [];
      const routeCreationFailures: any[] = [];

      setValidationMessage('Creating routes in system...');

    for (const route of routes) {
      try {
        // Use route code as UID to ensure they match
        const routeUID = route.routeCode;
        const scheduleUID = `${route.routeCode}_SCHEDULE`;

        // Helper function to generate config UID that matches existing master data
        const getConfigUID = (frequency: string, weekNumber: number | string, dayNumber: number) => {
          let weekStr = "NA";
          let finalDayNumber = dayNumber;
          let finalScheduleType = frequency.toLowerCase();
          
          if (frequency.toUpperCase() === 'WEEKLY') {
            weekStr = weekNumber ? `W${weekNumber}` : "W1";
          } else if (frequency.toUpperCase() === 'MONTHLY') {
            finalDayNumber = dayNumber; // Keep the day number as-is for monthly
          }
          
          // Match the existing DB format: schedule_type_weekStr_dayNumber
          return `${finalScheduleType}_${weekStr}_${finalDayNumber}`;
        };

        // Create RouteScheduleCustomerMappings
        const routeScheduleCustomerMappings = route.customers.map((customer, index) => {
          const configUID = getConfigUID(
            customer.frequency,
            customer.weekNo,
            customer.day
          );

          return {
            UID: routeService.generateScheduleUID(),
            RouteScheduleUID: scheduleUID,
            RouteScheduleConfigUID: configUID,
            CustomerUID: customer.customerUID,
            SeqNo: index + 1,
            StartTime: "00:00:00", // Default times
            EndTime: "00:00:00",
            IsDeleted: false,
            CreatedBy: currentUserUID,
            CreatedTime: undefined,
            ModifiedBy: currentUserUID,
            ModifiedTime: undefined,
            ServerAddTime: undefined,
            ServerModifiedTime: undefined
          };
        });

        // Create RouteCustomersList for basic customer info
        const routeCustomersList = route.customers.map((customer, index) => ({
          UID: routeService.generateScheduleUID(),
          RouteUID: routeUID,
          StoreUID: customer.customerUID,
          SeqNo: index + 1,
          VisitTime: "00:00:00", // Use string instead of null
          VisitDuration: 30, // Default duration
          EndTime: "00:00:00", // Use string instead of null
          IsDeleted: false,
          IsBillToCustomer: true, // Set all customers as billing customers for now
          TravelTime: 15, // Default travel time
          ActionType: "Add", // Use "Add" instead of "CREATE" to match manual creation
          Frequency: customer.frequency, // Add frequency field
          CreatedBy: currentUserUID,
          CreatedTime: undefined,
          ModifiedBy: currentUserUID,
          ModifiedTime: undefined,
          ServerAddTime: undefined,
          ServerModifiedTime: undefined,
        }));

        // Determine schedule type based on customer frequencies
        const frequencies = route.customers.map(c => c.frequency.toLowerCase());
        const uniqueFrequencies = [...new Set(frequencies)];
        const scheduleType = uniqueFrequencies.length === 1 ? uniqueFrequencies[0] : "multiple";

        const routeMasterData = {
          Route: {
            UID: routeUID,
            CompanyUID: route.orgUID,
            Code: route.routeCode,
            Name: route.routeName,
            OrgUID: route.orgUID,
            WHOrgUID: undefined, // Send undefined instead of null
            RoleUID: route.roleUID,
            JobPositionUID: route.employeeUID, // Use employee UID as JobPositionUID (same as manual creation)
            VehicleUID: undefined, // Send undefined instead of null
            LocationUID: undefined, // Ensure undefined is sent
            IsActive: true,
            Status: "Active",
            ValidFrom: route.startDate,
            ValidUpto: route.endDate,
            VisitTime: undefined, // Send undefined instead of null
            EndTime: undefined, // Send undefined instead of null
            VisitDuration: 30,
            TravelTime: 15,
            TotalCustomers: route.customers.length,
            IsCustomerWithTime: false,
            PrintStanding: false,
            PrintForward: false,
            PrintTopup: false,
            PrintOrderSummary: false,
            AutoFreezeJP: false,
            AddToRun: false,
            AutoFreezeRunTime: undefined, // Send undefined instead of empty string
            IsChangeApplied: false, // Backend expects this for new routes
            User: currentUserUID, // Backend required field
            CreatedBy: currentUserUID,
            CreatedTime: undefined,
            ModifiedBy: currentUserUID,
            ModifiedTime: undefined,
            ServerAddTime: undefined,
            ServerModifiedTime: undefined,
          },
          RouteSchedule: {
            UID: scheduleUID,
            CompanyUID: route.orgUID,
            RouteUID: routeUID,
            Name: route.routeName + " Schedule",
            Type: scheduleType,
            StartDate: route.startDate,
            Status: 1,
            FromDate: route.startDate,
            ToDate: route.endDate,
            StartTime: "00:00:00",
            EndTime: "23:59:59",
            VisitDurationInMinutes: 30,
            TravelTimeInMinutes: 15,
            NextBeatDate: route.startDate,
            LastBeatDate: route.startDate,
            AllowMultipleBeatsPerDay: false,
            PlannedDays: JSON.stringify([]),
            SS: null,
            CreatedBy: currentUserUID,
            ModifiedBy: currentUserUID,
          },
          RouteScheduleConfigs: [], // Using existing master configs
          RouteScheduleCustomerMappings: routeScheduleCustomerMappings,
          RouteCustomersList: routeCustomersList,
          RouteUserList: [
            {
              UID: `${routeUID}_${route.employeeUID}`,
              RouteUID: routeUID,
              JobPositionUID: route.employeeUID,
              FromDate: route.startDate,
              ToDate: route.endDate,
              IsActive: true,
              CreatedBy: currentUserUID,
              CreatedTime: undefined,
              ModifiedBy: currentUserUID,
              ModifiedTime: undefined,
            }
          ],
        };

        // Debug logging to match manual creation
        console.log('🔍 CRITICAL DEBUG - Excel Import Data being sent to API:');
        console.log('  - Route UID:', routeMasterData.Route.UID);
        console.log('  - Route Code:', routeMasterData.Route.Code);
        console.log('  - Route Name:', routeMasterData.Route.Name);
        console.log('  - CompanyUID:', routeMasterData.Route.CompanyUID);
        console.log('  - OrgUID:', routeMasterData.Route.OrgUID);
        console.log('  - RoleUID:', routeMasterData.Route.RoleUID);
        console.log('  - JobPositionUID:', routeMasterData.Route.JobPositionUID);
        console.log('  - RouteScheduleCustomerMappings count:', routeMasterData.RouteScheduleCustomerMappings?.length || 0);
        console.log('  - RouteCustomersList count:', routeMasterData.RouteCustomersList?.length || 0);
        console.log('  - RouteUserList count:', routeMasterData.RouteUserList?.length || 0);
        
        if (routeMasterData.RouteScheduleCustomerMappings?.length > 0) {
          console.log('  - Sample mapping:', routeMasterData.RouteScheduleCustomerMappings[0]);
        }
        if (routeMasterData.RouteCustomersList?.length > 0) {
          console.log('  - Sample customer:', routeMasterData.RouteCustomersList[0]);
        }
        if (routeMasterData.RouteUserList?.length > 0) {
          console.log('  - Sample user:', routeMasterData.RouteUserList[0]);
        }
        
        console.log('📦 Full Route Master Data being sent:', JSON.stringify(routeMasterData, null, 2));

        // Create route using the same API as manual creation
        try {
          const response: any = await apiService.post("/Route/CreateRouteMaster", routeMasterData);

          if (response?.IsSuccess || response?.Data > 0) {
            createdRoutes.push(route.routeName);
            console.log(`✅ Successfully created route: ${route.routeName}`);
          } else {
            failedRoutes.push(route.routeName);
            console.error(`❌ Failed to create route: ${route.routeName}`, response);
            console.error('API Response details:', {
              status: response?.status,
              data: response?.Data,
              message: response?.Message,
              errors: response?.Errors
            });
          }
        } catch (apiError: any) {
          console.error(`❌ API Error creating route ${route.routeName}:`, {
            message: apiError.message,
            status: apiError.status,
            response: apiError.response,
            data: apiError.data
          });
          throw apiError;
        }

      } catch (error: any) {
        console.error(`❌ Error creating route ${route.routeName}:`, error);
        
        // Provide user-friendly error messages
        let userFriendlyMessage = '';
        let isDuplicate = false;
        
        if (error.message && (
          error.message.includes('duplicate') || 
          error.message.includes('already exists') ||
          error.message.includes('constraint') ||
          error.message.includes('Error outside transaction') ||
          error.message.includes('Unique constraint violation')
        ) || error.status === 409) {
          isDuplicate = true;
          userFriendlyMessage = `Route code "${route.routeCode}" already exists in the system. Use a different route code.`;
          console.log(`⚠️ ${userFriendlyMessage}`);
        } else if (error.message && error.message.includes('Object reference not set')) {
          userFriendlyMessage = `Route "${route.routeCode}" has invalid data or missing required fields`;
        } else if (error.status === 500) {
          userFriendlyMessage = `Route "${route.routeCode}" failed due to server error`;
        } else if (error.status === 400) {
          userFriendlyMessage = `Route "${route.routeCode}" has invalid data`;
        } else {
          userFriendlyMessage = `Route "${route.routeCode}" failed to create`;
        }
        
        // Add to failed records with user-friendly error
        routeCreationFailures.push({
          ROUTE_CODE: route.routeCode,
          ROUTE_NAME: route.routeName,
          EMPLOYEE_CODE: route.employeeUID,
          EMPLOYEE_NAME: route.employeeName,
          CUSTOMER_COUNT: route.customers.length,
          ERROR_REASON: userFriendlyMessage,
          ERROR_TYPE: isDuplicate ? 'DUPLICATE_ROUTE' : 'ROUTE_CREATION_FAILED'
        });
        
        failedRoutes.push(route.routeName);
      }
    }

    setValidationMessage('');

    // Show final results with user-friendly messages
    const duplicateFailures = routeCreationFailures.filter(f => f.ERROR_TYPE === 'DUPLICATE_ROUTE');
    const otherFailures = routeCreationFailures.filter(f => f.ERROR_TYPE !== 'DUPLICATE_ROUTE');
    
    // Build a clear creation results message
    const creationResults = [];
    
    if (createdRoutes.length > 0) {
      creationResults.push(`✅ ROUTES CREATED SUCCESSFULLY: ${createdRoutes.length}`);
      createdRoutes.forEach(route => {
        creationResults.push(`   • ${route}`);
      });
    }
    
    if (failedRoutes.length > 0) {
      creationResults.push('');
      creationResults.push(`❌ ROUTES FAILED TO CREATE: ${failedRoutes.length}`);
      
      // Group failures by type
      if (duplicateFailures.length > 0) {
        creationResults.push(`   • ${duplicateFailures.length} route(s) already exist (duplicate route codes)`);
        duplicateFailures.forEach(f => {
          creationResults.push(`     - ${f.ROUTE_CODE}: Already exists in system`);
        });
      }
      
      if (otherFailures.length > 0) {
        creationResults.push(`   • ${otherFailures.length} route(s) failed for other reasons`);
        otherFailures.forEach(f => {
          creationResults.push(`     - ${f.ROUTE_CODE}: ${f.ERROR_REASON}`);
        });
      }
    }
    
    // Show creation results (including duplicate failures) and clear validation errors
    if (creationResults.length > 0) {
      setErrors([...creationResults]); // This will replace validation errors
    }
    
    // Only show toast for failures - success will be shown in persistent display
    if (createdRoutes.length === 0 && failedRoutes.length > 0) {
      let description = `All ${failedRoutes.length} routes failed to create.`;
      if (duplicateFailures.length === failedRoutes.length) {
        description = `All routes already exist. Use different route codes.`;
      } else if (duplicateFailures.length > 0) {
        description = `${duplicateFailures.length} duplicates, ${otherFailures.length} other failures.`;
      }
      toast({
        title: '❌ Route Creation Failed',
        description: description,
        variant: 'destructive',
      });
    } else if (createdRoutes.length > 0 && failedRoutes.length > 0) {
      // Show brief toast for partial success, full details in persistent display
      toast({
        title: '⚠️ Partial Success',
        description: `${createdRoutes.length} created, ${failedRoutes.length} failed. See details below.`,
        variant: 'destructive',
      });
    }
    // Note: No toast for full success - user will see the persistent success display

    // Add failed creation records to global failed records
    if (routeCreationFailures.length > 0) {
      setFailedRecords(prev => [...prev, ...routeCreationFailures]);
    }

      return { created: createdRoutes, failed: failedRoutes };
    } catch (error) {
      // Handle any unexpected errors in route creation process
      console.error('Unexpected error in route creation:', error);
      setValidationMessage('');
      
      toast({
        title: 'Route Creation Error',
        description: 'An unexpected error occurred during route creation. Please try again.',
        variant: 'destructive',
      });
      
      return { created: [], failed: routes.map(r => r.routeName) };
    }
  };

  // Handle file selection
  const handleFileSelect = (event: React.ChangeEvent<HTMLInputElement>) => {
    const selectedFile = event.target.files?.[0];
    if (selectedFile) {
      if (selectedFile.type !== 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' && 
          selectedFile.type !== 'application/vnd.ms-excel') {
        toast({
          title: 'Invalid File',
          description: 'Please select a valid Excel file (.xlsx or .xls)',
          variant: 'destructive',
        });
        return;
      }
      setFile(selectedFile);
      setUploadStatus('idle');
      setErrors([]);
    }
  };

  // Handle import
  const handleImport = async () => {
    // Clear caches to get fresh data for debugging
    employeeCache.clear();
    customerCache.clear();
    allEmployeesCache.current = null;
    allCustomersCache.current = null;
    
    console.log('🧹 Cleared all caches for fresh data');
    
    if (!file) {
      toast({
        title: 'No File Selected',
        description: 'Please select an Excel file to import',
        variant: 'destructive',
      });
      return;
    }

    setIsProcessing(true);
    setUploadStatus('processing');
    setErrors([]);

    try {
      // Process Excel file
      const excelData = await processExcelFile(file);
      console.log('Excel data extracted:', excelData);

      // Process and validate route data
      const routes = await processRouteData(excelData);
      console.log('Processed routes:', routes);

      if (routes.length === 0) {
        throw new Error('No valid routes found in the Excel file');
      }

      setProcessedData(routes);
      // Don't set success status yet - wait for route creation to complete
      
      // Create routes in the system using the same API as manual creation
      try {
        const creationResults = await createRoutesFromImport(routes);
        
        // Only set success status after route creation completes successfully
        if (creationResults.created.length > 0) {
          setUploadStatus('success');
          setRoutesCreated(true); // Mark that routes were created successfully
        } else {
          // All routes failed - keep status as processing (not success)
          setUploadStatus('processing');
        }
        
        // DON'T call parent callback immediately - let user see results and export data first
        // Parent callback might close dialog or reset state
        // if (onImportSuccess && creationResults.created.length > 0) {
        //   onImportSuccess(routes);
        // }
      } catch (routeCreationError) {
        // Route creation errors are already handled in createRoutesFromImport
        // This catch prevents technical errors from showing to user
        console.log('Route creation completed with some failures - user already notified');
        setUploadStatus('processing'); // Don't show success if there were errors
      }
    } catch (error) {
      console.error('Import error:', error);
      setUploadStatus('error');
      setErrors([error instanceof Error ? error.message : 'Failed to import Excel file']);
      toast({
        title: 'Import Failed',
        description: error instanceof Error ? error.message : 'Failed to process Excel file',
        variant: 'destructive',
      });
    } finally {
      setIsProcessing(false);
    }
  };

  return (
    <Card className="w-full">
      <CardHeader>
        <CardTitle className="flex items-center gap-2">
          <FileSpreadsheet className="h-5 w-5" />
          Import Routes from Excel
        </CardTitle>
        {selectedFrequency && (
          <CardDescription className="mt-2">
            <Alert className="bg-blue-50 border-blue-200">
              <AlertCircle className="h-4 w-4 text-blue-600" />
              <AlertDescription className="text-blue-800">
                Importing for <strong>{selectedFrequency.charAt(0).toUpperCase() + selectedFrequency.slice(1)}</strong> frequency. 
                Only customers with matching frequency will be imported.
              </AlertDescription>
            </Alert>
          </CardDescription>
        )}
      </CardHeader>
      <CardContent className="space-y-4">
        {/* Download Template and Employee List Buttons */}
        <div className="space-y-3">
          <div className="flex justify-between items-center">
            <div className="flex gap-2">
              <Button
                variant="outline"
                size="sm"
                onClick={downloadTemplate}
                className="flex items-center gap-2"
              >
                <Download className="h-4 w-4" />
                Download Template
              </Button>
            </div>
          </div>
        </div>

        {/* File Upload */}
        <div className="space-y-3">
          <div className="relative">
            <input
              type="file"
              accept=".xlsx,.xls"
              onChange={handleFileSelect}
              disabled={isProcessing}
              className="hidden"
              id="excel-file-input"
            />
            <label
              htmlFor="excel-file-input"
              className={`
                flex flex-col items-center justify-center w-full h-32 
                border-2 border-dashed rounded-lg cursor-pointer 
                transition-colors duration-200
                ${isProcessing 
                  ? 'bg-gray-50 border-gray-200 cursor-not-allowed' 
                  : 'bg-gray-50 border-gray-300 hover:bg-gray-100 hover:border-gray-400'
                }
              `}
            >
              <div className="flex flex-col items-center justify-center pt-5 pb-6">
                {file ? (
                  <>
                    <CheckCircle className="w-10 h-10 mb-3 text-green-500" />
                    <p className="mb-1 text-sm font-medium text-gray-700">
                      {file.name}
                    </p>
                    <p className="text-xs text-gray-500">
                      {(file.size / 1024).toFixed(2)} KB • Click to change file
                    </p>
                  </>
                ) : (
                  <>
                    <FileSpreadsheet className="w-10 h-10 mb-3 text-gray-400" />
                    <p className="mb-1 text-sm text-gray-700">
                      <span className="font-semibold">Click to upload</span> or drag and drop
                    </p>
                    <p className="text-xs text-gray-500">
                      Excel files only (.xlsx, .xls)
                    </p>
                  </>
                )}
              </div>
            </label>
          </div>
          
          {file && (
            <div className="flex items-center justify-between p-3 bg-blue-50 rounded-lg border border-blue-200">
              <div className="flex items-center gap-3">
                <FileSpreadsheet className="h-5 w-5 text-blue-600" />
                <div>
                  <p className="text-sm font-medium text-gray-900">{file.name}</p>
                  <p className="text-xs text-gray-600">Ready to import</p>
                </div>
              </div>
              <Button
                type="button"
                variant="ghost"
                size="sm"
                onClick={() => {
                  setFile(null);
                  setUploadStatus('idle');
                  setErrors([]);
                  // Reset the file input
                  const input = document.getElementById('excel-file-input') as HTMLInputElement;
                  if (input) input.value = '';
                }}
                className="text-gray-500 hover:text-red-600"
              >
                <X className="h-4 w-4" />
              </Button>
            </div>
          )}
        </div>

        {/* Validation Progress */}
        {isProcessing && validationMessage && (
          <Alert>
            <Loader2 className="h-4 w-4 animate-spin" />
            <AlertDescription>{validationMessage}</AlertDescription>
          </Alert>
        )}

        {/* Error Display - Show validation errors OR route creation failures (not both) */}
        {errors.length > 0 && (
          <div className="space-y-3">
            {/* Check if this is route creation results or validation errors */}
            {errors.some(error => error.includes('ROUTES CREATED SUCCESSFULLY') || error.includes('ROUTES FAILED TO CREATE')) ? (
              // Route Creation Results Display
              <Alert className={errors.some(error => error.includes('ROUTES CREATED SUCCESSFULLY')) ? 
                "border-blue-200 bg-blue-50" : "border-red-200 bg-red-50"}>
                <AlertCircle className="h-4 w-4" />
                <AlertDescription>
                  <div className="space-y-1">
                    <p className="font-medium">📊 ROUTE CREATION RESULTS:</p>
                    <div className="pl-4">
                      {errors.map((error, index) => (
                        <div key={index}>{error}</div>
                      ))}
                    </div>
                  </div>
                </AlertDescription>
              </Alert>
            ) : (
              // Validation Errors Display
              <Alert variant="destructive">
                <AlertCircle className="h-4 w-4" />
                <AlertDescription>
                  <div className="space-y-1">
                    {errors.map((error, index) => (
                      <div key={index}>{error}</div>
                    ))}
                  </div>
                </AlertDescription>
              </Alert>
            )}
            
            {/* Don't show export options in validation errors - only in success display */}
            
          </div>
        )}

        {/* Success Display - Always show when routes were created successfully */}
        {routesCreated && processedData.length > 0 && (
          <div className="space-y-3">
            <Alert className="border-green-200 bg-green-50">
              <div className="flex items-start justify-between">
                <div className="flex items-start gap-2">
                  <CheckCircle className="h-4 w-4 text-green-600 mt-1" />
                  <AlertDescription>
                    <div className="space-y-1">
                      <p className="font-medium">✅ ROUTES CREATED SUCCESSFULLY!</p>
                  <div className="space-y-1">
                    <p>📊 IMPORT RESULTS:</p>
                    <p>✅ Routes created: {processedData.length}</p>
                    <p>✅ Stores assigned: {processedData.reduce((sum, route) => sum + route.customers.length, 0)}</p>
                    
                    {failedRecords.length > 0 && (
                      <>
                        <p>⚠️ Items skipped: {failedRecords.length}</p>
                        <div className="ml-4 text-sm">
                          {Array.from(new Set(failedRecords.filter(r => r.ERROR_TYPE === 'CUSTOMER_NOT_FOUND').map(r => r.CUSTOMER_CODE))).length > 0 && (
                            <p>• {Array.from(new Set(failedRecords.filter(r => r.ERROR_TYPE === 'CUSTOMER_NOT_FOUND').map(r => r.CUSTOMER_CODE))).length} store(s) not found</p>
                          )}
                          {Array.from(new Set(failedRecords.filter(r => r.ERROR_TYPE === 'INACTIVE_CUSTOMER').map(r => r.CUSTOMER_CODE))).length > 0 && (
                            <p>• {Array.from(new Set(failedRecords.filter(r => r.ERROR_TYPE === 'INACTIVE_CUSTOMER').map(r => r.CUSTOMER_CODE))).length} inactive store(s)</p>
                          )}
                        </div>
                      </>
                    )}
                    
                    <div className="space-y-1 mt-3">
                      <p className="font-medium">📋 CREATED ROUTES:</p>
                      {processedData.map((route, index) => (
                        <div key={index} className="ml-4">
                          <div>✓ {route.routeName} ({route.customers.length} stores)</div>
                          <div className="text-xs text-gray-600 ml-4">
                            📅 {route.startDate} to {route.endDate}
                          </div>
                        </div>
                      ))}
                    </div>
                  </div>
                </div>
              </AlertDescription>
                </div>
                <Button
                  variant="ghost"
                  size="sm"
                  onClick={() => {
                    setRoutesCreated(false);
                    setUploadStatus('idle');
                  }}
                  className="text-gray-500 hover:text-gray-700"
                  title="Close success message"
                >
                  <X className="h-4 w-4" />
                </Button>
              </div>
            </Alert>
            
            {/* Export Options - Same style as failure display */}
            <div className="space-y-2">
              <p className="text-sm font-medium text-gray-700">📥 Export Options:</p>
              <div className="flex flex-wrap gap-2">
                <Button
                  type="button"
                  variant="outline"
                  size="sm"
                  onClick={exportSuccessRecords}
                  className="flex items-center gap-2 text-green-600 border-green-300 hover:bg-green-50"
                >
                  <CheckCircle className="h-4 w-4" />
                  Export Success ({successfulRecords.length})
                </Button>
                {failedRecords.length > 0 && (
                  <Button
                    type="button"
                    variant="outline"
                    size="sm"
                    onClick={exportFailedRecords}
                    className="flex items-center gap-2 text-orange-600 border-orange-300 hover:bg-orange-50"
                  >
                    <Download className="h-4 w-4" />
                    Export Skipped ({failedRecords.length})
                  </Button>
                )}
                <Button
                  type="button"
                  variant="outline"
                  size="sm"
                  onClick={exportCombinedReport}
                  className="flex items-center gap-2"
                >
                  <Download className="h-4 w-4" />
                  Export Complete Report
                </Button>
              </div>
            </div>
          </div>
        )}

        {/* Action Buttons */}
        <div className="flex gap-2 justify-end">
          {onClose && (
            <Button
              variant="outline"
              onClick={onClose}
              disabled={isProcessing}
            >
              Cancel
            </Button>
          )}
          <Button
            onClick={handleImport}
            disabled={!file || isProcessing}
            className="flex items-center gap-2"
          >
            {isProcessing ? (
              <>
                <Loader2 className="h-4 w-4 animate-spin" />
                Processing...
              </>
            ) : (
              <>
                <Upload className="h-4 w-4" />
                Import Routes
              </>
            )}
          </Button>
        </div>
      </CardContent>
    </Card>
  );
}