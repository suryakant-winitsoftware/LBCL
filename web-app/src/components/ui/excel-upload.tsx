'use client';

import React, { useState, useCallback } from 'react';
import { Button } from '@/components/ui/button';
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from '@/components/ui/card';
import { Alert, AlertDescription, AlertTitle } from '@/components/ui/alert';
import { useToast } from '@/components/ui/use-toast';
import { 
  Upload, 
  FileSpreadsheet, 
  Download, 
  AlertCircle, 
  CheckCircle2,
  X,
  Loader2
} from 'lucide-react';

export interface ExcelUploadColumn {
  name: string;
  required?: boolean;
  description?: string;
}

export interface ExcelUploadResult {
  success: boolean;
  message: string;
  count?: number;
  errors?: string[];
}

export interface ExcelUploadProps {
  title?: string;
  description?: string;
  columns: ExcelUploadColumn[];
  onUpload: (file: File) => Promise<ExcelUploadResult>;
  onDownloadTemplate?: () => void;
  maxFileSize?: number; // in MB
  acceptedFormats?: string[];
  className?: string;
}

export function ExcelUpload({
  title = "Upload Excel File",
  description = "Import data from an Excel file",
  columns,
  onUpload,
  onDownloadTemplate,
  maxFileSize = 10,
  acceptedFormats = ['.xlsx', '.xls'],
  className = ""
}: ExcelUploadProps) {
  const { toast } = useToast();
  const [file, setFile] = useState<File | null>(null);
  const [uploading, setUploading] = useState(false);
  const [uploadResult, setUploadResult] = useState<ExcelUploadResult | null>(null);
  const [dragActive, setDragActive] = useState(false);

  const validateFile = useCallback((file: File): boolean => {
    const validTypes = [
      'application/vnd.ms-excel',
      'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
    ];

    const fileExtension = '.' + file.name.split('.').pop()?.toLowerCase();
    
    if (!validTypes.includes(file.type) && !acceptedFormats.includes(fileExtension)) {
      toast({
        title: 'Invalid File',
        description: `Please upload an Excel file (${acceptedFormats.join(', ')})`,
        variant: 'destructive',
      });
      return false;
    }

    if (file.size > maxFileSize * 1024 * 1024) {
      toast({
        title: 'File Too Large',
        description: `File size must be less than ${maxFileSize}MB`,
        variant: 'destructive',
      });
      return false;
    }

    return true;
  }, [acceptedFormats, maxFileSize, toast]);

  const handleDrag = useCallback((e: React.DragEvent) => {
    e.preventDefault();
    e.stopPropagation();
    if (e.type === 'dragenter' || e.type === 'dragover') {
      setDragActive(true);
    } else if (e.type === 'dragleave') {
      setDragActive(false);
    }
  }, []);

  const handleDrop = useCallback((e: React.DragEvent) => {
    e.preventDefault();
    e.stopPropagation();
    setDragActive(false);

    if (e.dataTransfer.files && e.dataTransfer.files[0]) {
      const droppedFile = e.dataTransfer.files[0];
      if (validateFile(droppedFile)) {
        setFile(droppedFile);
        setUploadResult(null);
      }
    }
  }, [validateFile]);

  const handleFileChange = useCallback((e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files && e.target.files[0]) {
      const selectedFile = e.target.files[0];
      if (validateFile(selectedFile)) {
        setFile(selectedFile);
        setUploadResult(null);
      }
    }
  }, [validateFile]);

  const handleUpload = async () => {
    if (!file) {
      toast({
        title: 'No File Selected',
        description: 'Please select a file to upload',
        variant: 'destructive',
      });
      return;
    }

    try {
      setUploading(true);
      setUploadResult(null);
      const result = await onUpload(file);
      setUploadResult(result);
      
      if (result.success) {
        toast({
          title: 'Success',
          description: result.message,
        });
      } else {
        toast({
          title: 'Upload Failed',
          description: result.message,
          variant: 'destructive',
        });
      }
    } catch (error: any) {
      console.error('Error uploading file:', error);
      const errorResult: ExcelUploadResult = {
        success: false,
        message: error.message || 'Failed to upload file',
        errors: [error.message || 'Unknown error occurred']
      };
      setUploadResult(errorResult);
      toast({
        title: 'Upload Failed',
        description: errorResult.message,
        variant: 'destructive',
      });
    } finally {
      setUploading(false);
    }
  };

  const removeFile = () => {
    setFile(null);
    setUploadResult(null);
  };

  const requiredColumns = columns.filter(col => col.required);
  const optionalColumns = columns.filter(col => !col.required);

  return (
    <Card className={className}>
      <CardHeader>
        <CardTitle>{title}</CardTitle>
        <CardDescription>{description}</CardDescription>
      </CardHeader>
      <CardContent className="space-y-6">
        {/* Format Requirements */}
        <Alert>
          <AlertCircle className="h-4 w-4" />
          <AlertTitle>Excel Format Requirements</AlertTitle>
          <AlertDescription>
            <ul className="list-disc list-inside mt-2 space-y-1">
              <li>File must be in {acceptedFormats.join(' or ')} format</li>
              <li>First row should contain column headers</li>
              <li>Maximum file size: {maxFileSize}MB</li>
              {requiredColumns.length > 0 && (
                <li>
                  Required columns: {requiredColumns.map(col => col.name).join(', ')}
                </li>
              )}
              {optionalColumns.length > 0 && (
                <li>
                  Optional columns: {optionalColumns.map(col => col.name).join(', ')}
                </li>
              )}
            </ul>
          </AlertDescription>
        </Alert>

        {/* Template Download */}
        {onDownloadTemplate && (
          <div className="flex items-center justify-between p-4 bg-blue-50 rounded-lg">
            <div>
              <h4 className="font-semibold">Download Template</h4>
              <p className="text-sm text-gray-600">
                Use our template to ensure your data is formatted correctly
              </p>
            </div>
            <Button onClick={onDownloadTemplate} variant="outline">
              <Download className="mr-2 h-4 w-4" />
              Download Template
            </Button>
          </div>
        )}

        {/* File Upload Area */}
        <div
          className={`border-2 border-dashed rounded-lg p-8 text-center transition-colors ${
            dragActive ? 'border-blue-500 bg-blue-50' : 'border-gray-300'
          }`}
          onDragEnter={handleDrag}
          onDragLeave={handleDrag}
          onDragOver={handleDrag}
          onDrop={handleDrop}
        >
          <FileSpreadsheet className="mx-auto h-12 w-12 text-gray-400 mb-4" />
          
          {file ? (
            <div className="space-y-2">
              <p className="text-sm font-medium">Selected File:</p>
              <p className="text-lg">{file.name}</p>
              <p className="text-sm text-gray-500">
                {(file.size / 1024).toFixed(2)} KB
              </p>
              <Button
                onClick={removeFile}
                variant="outline"
                size="sm"
                className="mt-2"
              >
                <X className="mr-2 h-4 w-4" />
                Remove File
              </Button>
            </div>
          ) : (
            <>
              <p className="text-gray-600 mb-2">
                Drag and drop your Excel file here, or click to browse
              </p>
              <input
                type="file"
                id="excel-file-upload"
                className="hidden"
                accept={acceptedFormats.join(',')}
                onChange={handleFileChange}
              />
              <label htmlFor="excel-file-upload">
                <Button variant="outline" asChild>
                  <span>Select File</span>
                </Button>
              </label>
            </>
          )}
        </div>

        {/* Upload Result */}
        {uploadResult && (
          <Alert className={uploadResult.success ? "bg-green-50 border-green-200" : "bg-red-50 border-red-200"}>
            {uploadResult.success ? (
              <CheckCircle2 className="h-4 w-4 text-green-600" />
            ) : (
              <AlertCircle className="h-4 w-4 text-red-600" />
            )}
            <AlertTitle className={uploadResult.success ? "text-green-800" : "text-red-800"}>
              {uploadResult.success ? 'Upload Complete' : 'Upload Failed'}
            </AlertTitle>
            <AlertDescription className={uploadResult.success ? "text-green-700" : "text-red-700"}>
              {uploadResult.message}
              {uploadResult.count && (
                <div className="mt-2">
                  <strong>Records Processed:</strong> {uploadResult.count}
                </div>
              )}
              {uploadResult.errors && uploadResult.errors.length > 0 && (
                <div className="mt-2">
                  <strong>Errors:</strong>
                  <ul className="list-disc list-inside ml-4 mt-1">
                    {uploadResult.errors.map((error, index) => (
                      <li key={index}>{error}</li>
                    ))}
                  </ul>
                </div>
              )}
            </AlertDescription>
          </Alert>
        )}

        {/* Action Buttons */}
        <div className="flex justify-end gap-4">
          <Button
            onClick={handleUpload}
            disabled={!file || uploading}
          >
            {uploading ? (
              <>
                <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                Uploading...
              </>
            ) : (
              <>
                <Upload className="mr-2 h-4 w-4" />
                Upload File
              </>
            )}
          </Button>
        </div>
      </CardContent>
    </Card>
  );
}