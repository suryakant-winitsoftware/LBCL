"use client";
import React, { useState, useCallback } from "react";
import {
  Upload,
  Download,
  CheckCircle2,
  AlertTriangle,
  X,
  FileText,
  Users,
  Search,
  RefreshCw,
  AlertCircle,
  Check
} from "lucide-react";
import * as XLSX from "xlsx";

interface Customer {
  UID: string;
  Code: string;
  Name: string;
}

interface ImportedCustomer {
  rowNumber: number;
  name?: string;
  code: string;
  isValid: boolean;
  matchedCustomer?: Customer;
  matchStatus: "matched" | "partial" | "not_found" | "duplicate";
  errors: string[];
}

interface CustomerExcelImportProps {
  availableCustomers: Customer[];
  onCustomersImported: (customers: Customer[]) => void;
  onClose: () => void;
  isOpen: boolean;
}

const CustomerExcelImport: React.FC<CustomerExcelImportProps> = ({
  availableCustomers,
  onCustomersImported,
  onClose,
  isOpen
}) => {
  const [importData, setImportData] = useState<ImportedCustomer[]>([]);
  const [isProcessing, setIsProcessing] = useState(false);
  const [step, setStep] = useState<"upload" | "review" | "complete">("upload");
  const [searchTerm, setSearchTerm] = useState("");

  // Reset state when component opens/closes
  React.useEffect(() => {
    if (isOpen) {
      setImportData([]);
      setStep("upload");
      setSearchTerm("");
    }
  }, [isOpen]);

  // Download template function
  const downloadTemplate = () => {
    const template = [
      {
        "Customer Code": "ABC001",
        "Customer Name (Optional)": "ABC Store"
      },
      {
        "Customer Code": "XYZ002",
        "Customer Name (Optional)": "XYZ Mart"
      }
    ];

    const ws = XLSX.utils.json_to_sheet(template);
    const wb = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(wb, ws, "Customer Template");

    // Set column widths
    ws["!cols"] = [
      { width: 15 }, // Customer Code
      { width: 25 } // Customer Name (Optional)
    ];

    XLSX.writeFile(wb, "customer_import_template.xlsx");
  };

  // Match imported customer with available customers
  const matchCustomer = (
    importedCustomer: ImportedCustomer
  ): Customer | null => {
    const { name, code } = importedCustomer;

    // Exact match by code (primary matching method)
    const exactCodeMatch = availableCustomers.find(
      (c) => c.Code.toLowerCase().trim() === code.toLowerCase().trim()
    );
    if (exactCodeMatch) return exactCodeMatch;

    // Exact match by name (if name is provided)
    if (name) {
      const exactNameMatch = availableCustomers.find(
        (c) => c.Name.toLowerCase().trim() === name.toLowerCase().trim()
      );
      if (exactNameMatch) return exactNameMatch;

      // Partial match by name (contains)
      const partialNameMatch = availableCustomers.find(
        (c) =>
          c.Name.toLowerCase().includes(name.toLowerCase()) ||
          name.toLowerCase().includes(c.Name.toLowerCase())
      );
      if (partialNameMatch) return partialNameMatch;
    }

    return null;
  };

  // Determine match status
  const getMatchStatus = (
    importedCustomer: ImportedCustomer,
    matchedCustomer: Customer | null
  ) => {
    if (!matchedCustomer) return "not_found";

    const { name, code } = importedCustomer;

    // Check for exact matches
    const exactCodeMatch =
      matchedCustomer.Code.toLowerCase().trim() === code.toLowerCase().trim();
    const exactNameMatch =
      name &&
      matchedCustomer.Name.toLowerCase().trim() === name.toLowerCase().trim();

    if (exactCodeMatch || exactNameMatch) return "matched";

    // Check for duplicates (multiple matches) - only if name is provided
    if (name) {
      const nameMatches = availableCustomers.filter(
        (c) =>
          c.Name.toLowerCase().includes(name.toLowerCase()) ||
          name.toLowerCase().includes(c.Name.toLowerCase())
      );

      if (nameMatches.length > 1) return "duplicate";
    }

    return "partial";
  };

  // Process Excel file
  const processExcelFile = useCallback(
    async (file: File) => {
      setIsProcessing(true);

      try {
        const data = await file.arrayBuffer();
        const workbook = XLSX.read(data);
        const sheetName = workbook.SheetNames[0];
        const worksheet = workbook.Sheets[sheetName];

        // Convert to JSON
        const jsonData = XLSX.utils.sheet_to_json(worksheet, { header: 1 });

        if (jsonData.length < 2) {
          throw new Error(
            "Excel file must contain at least a header row and one data row"
          );
        }

        // Get headers (first row)
        const headers = jsonData[0] as string[];

        // Find column indices
        const nameIndex = headers.findIndex(
          (h) =>
            h?.toLowerCase().includes("name") ||
            h?.toLowerCase().includes("customer")
        );
        const codeIndex = headers.findIndex(
          (h) =>
            h?.toLowerCase().includes("code") || h?.toLowerCase().includes("id")
        );

        if (codeIndex === -1) {
          throw new Error(
            'Could not find a "Customer Code" column. Please ensure your Excel file has a column with "code" or "id" in the header.'
          );
        }

        // Process data rows
        const processedData: ImportedCustomer[] = [];

        for (let i = 1; i < jsonData.length; i++) {
          const row = jsonData[i] as any[];
          const rowNumber = i + 1;

          // Skip empty rows
          if (
            !row ||
            row.every((cell) => !cell || cell.toString().trim() === "")
          ) {
            continue;
          }

          const name =
            nameIndex !== -1 ? row[nameIndex]?.toString().trim() : undefined;
          const code = row[codeIndex]?.toString().trim();

          if (!code) {
            continue; // Skip rows without codes
          }

          const importedCustomer: ImportedCustomer = {
            rowNumber,
            name,
            code,
            isValid: false,
            matchStatus: "not_found",
            errors: []
          };

          // Validate basic data
          if (code.length < 1) {
            importedCustomer.errors.push("Customer code is required");
          }
          if (name && name.length < 2) {
            importedCustomer.errors.push("Customer name too short");
          }

          // Try to match with available customers
          const matchedCustomer = matchCustomer(importedCustomer);
          importedCustomer.matchedCustomer = matchedCustomer || undefined;
          importedCustomer.matchStatus = getMatchStatus(
            importedCustomer,
            matchedCustomer
          );

          // Determine if valid
          importedCustomer.isValid =
            matchedCustomer !== null && importedCustomer.errors.length === 0;

          processedData.push(importedCustomer);
        }

        if (processedData.length === 0) {
          throw new Error("No valid customer data found in Excel file");
        }

        setImportData(processedData);
        setStep("review");
      } catch (error) {
        console.error("Error processing Excel file:", error);
        alert(
          `Error processing Excel file: ${
            error instanceof Error ? error.message : "Unknown error"
          }`
        );
      } finally {
        setIsProcessing(false);
      }
    },
    [availableCustomers]
  );

  // Handle file upload
  const handleFileUpload = (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    if (file) {
      processExcelFile(file);
    }
  };

  // Import selected customers
  const handleImport = () => {
    const validCustomers = importData
      .filter((item) => item.isValid && item.matchedCustomer)
      .map((item) => item.matchedCustomer!);

    const uniqueCustomers = Array.from(
      new Map(validCustomers.map((c) => [c.UID, c])).values()
    );

    onCustomersImported(uniqueCustomers);
    setStep("complete");
  };

  // Filter data based on search
  const filteredData = importData.filter(
    (item) =>
      item.code.toLowerCase().includes(searchTerm.toLowerCase()) ||
      (item.name &&
        item.name.toLowerCase().includes(searchTerm.toLowerCase())) ||
      item.matchedCustomer?.Name.toLowerCase().includes(
        searchTerm.toLowerCase()
      )
  );

  // Get statistics
  const stats = {
    total: importData.length,
    matched: importData.filter((item) => item.matchStatus === "matched").length,
    partial: importData.filter((item) => item.matchStatus === "partial").length,
    notFound: importData.filter((item) => item.matchStatus === "not_found")
      .length,
    duplicate: importData.filter((item) => item.matchStatus === "duplicate")
      .length,
    valid: importData.filter((item) => item.isValid).length
  };

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
      <div className="bg-white rounded-lg shadow-xl w-full max-w-6xl max-h-[90vh] overflow-hidden">
        {/* Header */}
        <div className="flex items-center justify-between p-6 border-b border-gray-200">
          <div className="flex items-center gap-3">
            <div className="p-2 bg-blue-100 rounded-lg">
              <Upload className="w-5 h-5 text-blue-600" />
            </div>
            <div>
              <h2 className="text-xl font-semibold text-gray-900">
                Import Customers from Excel
              </h2>
              <p className="text-sm text-gray-500">
                Upload an Excel file to import customer data
              </p>
            </div>
          </div>
          <button
            onClick={onClose}
            className="p-2 hover:bg-gray-100 rounded-lg transition-colors"
          >
            <X className="w-5 h-5 text-gray-500" />
          </button>
        </div>

        {/* Content */}
        <div
          className="p-6 overflow-y-auto"
          style={{ maxHeight: "calc(90vh - 140px)" }}
        >
          {step === "upload" && (
            <div className="space-y-6">
              {/* Instructions */}
              <div className="bg-blue-50 border border-blue-200 rounded-lg p-4">
                <h3 className="font-medium text-blue-900 mb-2">Instructions</h3>
                <ul className="text-sm text-blue-800 space-y-1">
                  <li>• Upload an Excel file (.xlsx) with customer data</li>
                  <li>• Column 1 (Required): "Customer Code"</li>
                  <li>• Column 2 (Optional): "Customer Name"</li>
                  <li>
                    • System will automatically match customers with existing
                    data
                  </li>
                </ul>
              </div>

              {/* Download Template */}
              <div className="text-center">
                <button
                  onClick={downloadTemplate}
                  className="inline-flex items-center gap-2 px-4 py-2 bg-green-500 text-white rounded-lg hover:bg-green-600 transition-colors"
                >
                  <Download className="w-4 h-4" />
                  Download Template
                </button>
                <p className="text-sm text-gray-500 mt-2">
                  Download a sample Excel template to get started
                </p>
              </div>

              {/* File Upload */}
              <div className="border-2 border-dashed border-gray-300 rounded-lg p-8">
                <div className="text-center">
                  <FileText className="w-12 h-12 text-gray-400 mx-auto mb-4" />
                  <div className="space-y-2">
                    <label className="block">
                      <span className="sr-only">Choose Excel file</span>
                      <input
                        type="file"
                        accept=".xlsx,.xls"
                        onChange={handleFileUpload}
                        disabled={isProcessing}
                        className="block w-full text-sm text-gray-500 file:mr-4 file:py-2 file:px-4 file:rounded-full file:border-0 file:text-sm file:font-semibold file:bg-blue-50 file:text-blue-700 hover:file:bg-blue-100"
                      />
                    </label>
                    {isProcessing && (
                      <div className="flex items-center justify-center gap-2 text-blue-600">
                        <RefreshCw className="w-4 h-4 animate-spin" />
                        Processing Excel file...
                      </div>
                    )}
                  </div>
                </div>
              </div>
            </div>
          )}

          {step === "review" && (
            <div className="space-y-6">
              {/* Statistics */}
              <div className="grid grid-cols-2 md:grid-cols-5 gap-4">
                <div className="bg-gray-50 p-3 rounded-lg">
                  <div className="text-lg font-semibold text-gray-900">
                    {stats.total}
                  </div>
                  <div className="text-sm text-gray-600">Total Rows</div>
                </div>
                <div className="bg-green-50 p-3 rounded-lg">
                  <div className="text-lg font-semibold text-green-600">
                    {stats.matched}
                  </div>
                  <div className="text-sm text-gray-600">Exact Match</div>
                </div>
                <div className="bg-yellow-50 p-3 rounded-lg">
                  <div className="text-lg font-semibold text-yellow-600">
                    {stats.partial}
                  </div>
                  <div className="text-sm text-gray-600">Partial Match</div>
                </div>
                <div className="bg-red-50 p-3 rounded-lg">
                  <div className="text-lg font-semibold text-red-600">
                    {stats.notFound}
                  </div>
                  <div className="text-sm text-gray-600">Not Found</div>
                </div>
                <div className="bg-blue-50 p-3 rounded-lg">
                  <div className="text-lg font-semibold text-blue-600">
                    {stats.valid}
                  </div>
                  <div className="text-sm text-gray-600">Ready to Import</div>
                </div>
              </div>

              {/* Search */}
              <div className="relative">
                <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 w-4 h-4 text-gray-400" />
                <input
                  type="text"
                  placeholder="Search imported customers..."
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                  className="w-full pl-9 pr-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                />
              </div>

              {/* Data Table */}
              <div className="border border-gray-200 rounded-lg overflow-hidden">
                <div className="overflow-x-auto">
                  <table className="w-full">
                    <thead className="bg-gray-50">
                      <tr>
                        <th className="px-4 py-3 text-left text-sm font-medium text-gray-900">
                          Row
                        </th>
                        <th className="px-4 py-3 text-left text-sm font-medium text-gray-900">
                          Status
                        </th>
                        <th className="px-4 py-3 text-left text-sm font-medium text-gray-900">
                          Excel Data
                        </th>
                        <th className="px-4 py-3 text-left text-sm font-medium text-gray-900">
                          Matched Customer
                        </th>
                      </tr>
                    </thead>
                    <tbody className="divide-y divide-gray-200">
                      {filteredData.map((item, index) => (
                        <tr key={index} className="hover:bg-gray-50">
                          <td className="px-4 py-3 text-sm text-gray-500">
                            {item.rowNumber}
                          </td>
                          <td className="px-4 py-3">
                            <div className="flex items-center gap-2">
                              {item.matchStatus === "matched" && (
                                <span className="flex items-center gap-1 px-2 py-1 bg-green-100 text-green-800 text-xs rounded-full">
                                  <CheckCircle2 className="w-3 h-3" />
                                  Matched
                                </span>
                              )}
                              {item.matchStatus === "partial" && (
                                <span className="flex items-center gap-1 px-2 py-1 bg-yellow-100 text-yellow-800 text-xs rounded-full">
                                  <AlertTriangle className="w-3 h-3" />
                                  Partial
                                </span>
                              )}
                              {item.matchStatus === "not_found" && (
                                <span className="flex items-center gap-1 px-2 py-1 bg-red-100 text-red-800 text-xs rounded-full">
                                  <AlertCircle className="w-3 h-3" />
                                  Not Found
                                </span>
                              )}
                              {item.matchStatus === "duplicate" && (
                                <span className="flex items-center gap-1 px-2 py-1 bg-orange-100 text-orange-800 text-xs rounded-full">
                                  <Users className="w-3 h-3" />
                                  Multiple
                                </span>
                              )}
                            </div>
                          </td>
                          <td className="px-4 py-3">
                            <div className="space-y-1">
                              <div className="font-medium text-gray-900">
                                Code: {item.code}
                              </div>
                              {item.name && (
                                <div className="text-sm text-gray-500">
                                  Name: {item.name}
                                </div>
                              )}
                              {item.errors.length > 0 && (
                                <div className="text-xs text-red-600">
                                  {item.errors.join(", ")}
                                </div>
                              )}
                            </div>
                          </td>
                          <td className="px-4 py-3">
                            {item.matchedCustomer ? (
                              <div className="space-y-1">
                                <div className="font-medium text-gray-900">
                                  {item.matchedCustomer.Name}
                                </div>
                                <div className="text-sm text-gray-500">
                                  Code: {item.matchedCustomer.Code}
                                </div>
                              </div>
                            ) : (
                              <div className="text-sm text-gray-400">
                                No match found
                              </div>
                            )}
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              </div>

              {/* Actions */}
              <div className="flex items-center justify-between pt-4 border-t border-gray-200">
                <button
                  onClick={() => setStep("upload")}
                  className="px-4 py-2 border border-gray-300 text-gray-700 rounded-lg hover:bg-gray-50 transition-colors"
                >
                  Back to Upload
                </button>
                <div className="flex items-center gap-3">
                  <div className="text-sm text-gray-600">
                    {stats.valid} customers ready to import
                  </div>
                  <button
                    onClick={handleImport}
                    disabled={stats.valid === 0}
                    className="px-4 py-2 bg-blue-500 text-white rounded-lg hover:bg-blue-600 disabled:bg-gray-300 disabled:cursor-not-allowed transition-colors"
                  >
                    Import {stats.valid} Customers
                  </button>
                </div>
              </div>
            </div>
          )}

          {step === "complete" && (
            <div className="text-center space-y-4">
              <div className="mx-auto w-16 h-16 bg-green-100 rounded-full flex items-center justify-center">
                <Check className="w-8 h-8 text-green-600" />
              </div>
              <div>
                <h3 className="text-lg font-semibold text-gray-900">
                  Import Complete!
                </h3>
                <p className="text-gray-600">
                  Successfully imported {stats.valid} customers from Excel file
                </p>
              </div>
              <button
                onClick={onClose}
                className="px-4 py-2 bg-blue-500 text-white rounded-lg hover:bg-blue-600 transition-colors"
              >
                Close
              </button>
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

export default CustomerExcelImport;
