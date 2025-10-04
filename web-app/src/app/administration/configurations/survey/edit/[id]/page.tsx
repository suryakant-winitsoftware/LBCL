'use client';

import React, { useState, useEffect, useCallback } from 'react';
import { useRouter, useParams } from 'next/navigation';
import { 
  Plus, 
  Minus, 
  ChevronDown, 
  ChevronUp, 
  Trash2, 
  Save, 
  ArrowLeft, 
  Loader2,
  FileText,
  Calendar,
  Hash,
  Type,
  Clock,
  Camera,
  CheckSquare,
  AlertCircle,
  ListChecks,
  CalendarDays,
  Settings2,
  ClipboardList,
  Info
} from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Textarea } from '@/components/ui/textarea';
import { Checkbox } from '@/components/ui/checkbox';
import { Switch } from '@/components/ui/switch';
import { Badge } from '@/components/ui/badge';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from '@/components/ui/dialog';
import { Label } from '@/components/ui/label';
import { toast } from 'sonner';
import { Skeleton } from '@/components/ui/skeleton';
import { cn } from '@/lib/utils';
import { surveyService } from '@/services/surveyService';
import {
  IManageSurvey,
  IManageSection,
  IManageQuestion,
  IManageOption,
  ISelectionItem,
  QUESTION_TYPES,
  MIN_DATE_TYPES,
  MAX_DATE_TYPES,
  CREATE_SURVEY_CONSTANTS
} from '@/types/survey-create.types';

export default function EditSurveyPage() {
  const router = useRouter();
  const params = useParams();
  const surveyId = params.id as string;
  
  // Main survey state
  const [manageSurvey, setManageSurvey] = useState<IManageSurvey>({
    Code: '',
    Title: '',
    Description: '',
    StartDate: '',
    EndDate: '',
    IsActive: true,
    Sections: []
  });

  // UI state
  const [activeSectionId, setActiveSectionId] = useState<string>('');
  const [isAddSectionPopup, setIsAddSectionPopup] = useState(false);
  const [newSectionName, setNewSectionName] = useState('');
  const [newSectionError, setNewSectionError] = useState('');
  const [errorMessage, setErrorMessage] = useState('');
  const [loading, setLoading] = useState(false);
  const [isPageDisabled, setIsPageDisabled] = useState(false);
  const [isLoadingData, setIsLoadingData] = useState(true);
  const [isNew, setIsNew] = useState(true); // Track if it's a new survey or existing

  // Fetch survey data on mount
  useEffect(() => {
    if (surveyId && surveyId !== 'new') {
      fetchSurveyData();
    } else {
      setIsLoadingData(false);
      setIsNew(true);
    }
  }, [surveyId]);

  // Fetch survey data
  const fetchSurveyData = async () => {
    setIsLoadingData(true);
    try {
      const surveyData = await surveyService.getSurveyByUID(surveyId);
      
      console.log('📊 Survey data fetched:', surveyData);
      
      if (surveyData) {
        setIsNew(false);
        
        // Parse the survey data - handle both string and object
        let parsedSurveyData: any = {};
        if (surveyData.SurveyData) {
          try {
            if (typeof surveyData.SurveyData === 'string') {
              parsedSurveyData = JSON.parse(surveyData.SurveyData);
            } else {
              parsedSurveyData = surveyData.SurveyData;
            }
            console.log('📋 Parsed survey data:', parsedSurveyData);
          } catch (e) {
            console.error('Error parsing survey data:', e);
          }
        }

        // Format dates for the date input
        const formatDate = (dateString: string | null | undefined) => {
          if (!dateString) return '';
          try {
            const date = new Date(dateString);
            return date.toISOString().split('T')[0];
          } catch {
            return '';
          }
        };

        // Set the survey data - check field names (might be capitalized)
        const sections = parsedSurveyData.sections || parsedSurveyData.Sections || [];
        // Ensure all sections have SectionId
        const sectionsWithIds = sections.map((section: any, index: number) => ({
          ...section,
          SectionId: section.SectionId || section.sectionId || section.section_id || `section_${index}_${Date.now()}`
        }));
        
        const surveyToSet = {
          SurveyId: surveyData.UID || surveyData.uid,
          Code: surveyData.Code || surveyData.code || '',
          Title: parsedSurveyData.title || parsedSurveyData.Title || '',
          Description: surveyData.Description || surveyData.description || parsedSurveyData.description || parsedSurveyData.Description || '',
          StartDate: formatDate(surveyData.StartDate || surveyData.startDate),
          EndDate: formatDate(surveyData.EndDate || surveyData.endDate),
          IsActive: surveyData.IsActive !== undefined ? surveyData.IsActive : (surveyData.isActive !== undefined ? surveyData.isActive : true),
          Sections: sectionsWithIds
        };
        
        console.log('🔧 Setting survey data:', surveyToSet);
        setManageSurvey(surveyToSet);

        // Check if page should be disabled (if start date is in the past)
        const startDateValue = surveyData.StartDate || surveyData.startDate;
        if (startDateValue) {
          const startDate = new Date(startDateValue);
          const today = new Date();
          today.setHours(0, 0, 0, 0);
          startDate.setHours(0, 0, 0, 0);
          // Only disable if the start date is strictly in the past (not today)
          setIsPageDisabled(startDate < today);
        }
      }
    } catch (error) {
      console.error('Error fetching survey:', error);
      toast.error('Failed to load survey data');
      router.push('/administration/store-management/surveys');
    } finally {
      setIsLoadingData(false);
    }
  };

  // Check if page should be disabled based on start date
  useEffect(() => {
    if (manageSurvey.StartDate && !isNew) {
      const startDate = new Date(manageSurvey.StartDate);
      const today = new Date();
      today.setHours(0, 0, 0, 0);
      startDate.setHours(0, 0, 0, 0);
      // Only disable if the start date is strictly in the past (not today)
      setIsPageDisabled(startDate < today);
    } else {
      setIsPageDisabled(false);
    }
  }, [manageSurvey.StartDate, isNew]);

  // Generate default question
  const generateDefaultQuestion = (questionNumber: number): IManageQuestion => ({
    Id: questionNumber.toString(),
    LabelQuestion: `Question ${questionNumber}`,
    Label: '',
    Type: '',
    IsScoreRequired: false,
    IsRequired: false,
    IsCameraVisible: false,
    IsDateRequired: false,
    IsTimeRequired: false,
    MinDate: '',
    MaxDate: '',
    MinSpecificDate: '',
    MaxSpecificDate: '',
    SeqNo: questionNumber,
    Options: [],
    Validations: { is_mandatory: false } // This will be synced with IsRequired
  });

  // Generate default option
  const generateDefaultOption = (optionNumber: number): IManageOption => ({
    Id: optionNumber.toString(),
    Label: `Option ${optionNumber}`,
    Points: optionNumber,
    SeqNo: optionNumber
  });

  // Add new section
  const handleAddSection = () => {
    setIsAddSectionPopup(true);
    setNewSectionName('');
    setNewSectionError('');
  };

  // Save new section
  const handleSaveSection = () => {
    if (!newSectionName.trim()) {
      setNewSectionError('Section Name is required.');
      return;
    }

    const newSection: IManageSection = {
      SectionId: Date.now().toString(),
      SectionTitle: newSectionName,
      SeqNo: (manageSurvey.Sections?.length || 0) + 1,
      Questions: [generateDefaultQuestion(1)]
    };

    setManageSurvey(prev => ({
      ...prev,
      Sections: [...(prev.Sections || []), newSection]
    }));

    setActiveSectionId(newSection.SectionId!);
    setIsAddSectionPopup(false);
    setNewSectionName('');
    setNewSectionError('');
  };

  // Delete section
  const handleDeleteSection = (sectionId: string) => {
    setManageSurvey(prev => ({
      ...prev,
      Sections: prev.Sections?.filter(s => s.SectionId !== sectionId) || []
    }));
    setActiveSectionId('');
  };

  // Toggle section visibility
  const toggleSection = (sectionId: string) => {
    setActiveSectionId(activeSectionId === sectionId ? '' : sectionId);
  };

  // Add question to section
  const handleAddQuestion = (sectionId: string) => {
    setManageSurvey(prev => ({
      ...prev,
      Sections: prev.Sections?.map(section => {
        if (section.SectionId === sectionId) {
          const nextQuestionNumber = (section.Questions?.length || 0) + 1;
          return {
            ...section,
            Questions: [
              ...(section.Questions || []),
              generateDefaultQuestion(nextQuestionNumber)
            ]
          };
        }
        return section;
      }) || []
    }));
  };

  // Remove question from section
  const handleRemoveQuestion = (sectionId: string, questionId: string) => {
    setManageSurvey(prev => ({
      ...prev,
      Sections: prev.Sections?.map(section => {
        if (section.SectionId === sectionId) {
          const updatedQuestions = section.Questions?.filter(q => q.Id !== questionId) || [];
          // Renumber questions
          const renumberedQuestions = updatedQuestions.map((q, index) => ({
            ...q,
            Id: (index + 1).toString(),
            LabelQuestion: `Question ${index + 1}`,
            SeqNo: index + 1
          }));
          return { ...section, Questions: renumberedQuestions };
        }
        return section;
      }) || []
    }));
  };

  // Update question
  const handleQuestionChange = (sectionId: string, questionId: string, field: string, value: any) => {
    setManageSurvey(prev => ({
      ...prev,
      Sections: prev.Sections?.map(section => {
        if (section.SectionId === sectionId) {
          return {
            ...section,
            Questions: section.Questions?.map(question => {
              if (question.Id === questionId) {
                // Sync IsRequired with is_mandatory for consistency
                if (field === 'IsRequired') {
                  return { 
                    ...question, 
                    [field]: value,
                    Validations: { ...question.Validations, is_mandatory: value }
                  };
                }
                if (field === 'Type' && value !== question.Type) {
                  // Reset options when type changes
                  const updatedQuestion = { ...question, [field]: value, Options: [] };
                  
                  // Add default option for multi-choice types
                  if ([CREATE_SURVEY_CONSTANTS.DropDownTypes.Radio, 
                       CREATE_SURVEY_CONSTANTS.DropDownTypes.Dropdown,
                       CREATE_SURVEY_CONSTANTS.DropDownTypes.MultiDropdown,
                       CREATE_SURVEY_CONSTANTS.DropDownTypes.Checkbox].includes(value)) {
                    updatedQuestion.Options = [generateDefaultOption(1)];
                  }
                  
                  return updatedQuestion;
                }
                return { ...question, [field]: value };
              }
              return question;
            }) || []
          };
        }
        return section;
      }) || []
    }));
  };

  // Add option to question
  const handleAddOption = (sectionId: string, questionId: string) => {
    setManageSurvey(prev => ({
      ...prev,
      Sections: prev.Sections?.map(section => {
        if (section.SectionId === sectionId) {
          return {
            ...section,
            Questions: section.Questions?.map(question => {
              if (question.Id === questionId) {
                const nextOptionNumber = (question.Options?.length || 0) + 1;
                return {
                  ...question,
                  Options: [
                    ...(question.Options || []),
                    generateDefaultOption(nextOptionNumber)
                  ]
                };
              }
              return question;
            }) || []
          };
        }
        return section;
      }) || []
    }));
  };

  // Remove option from question
  const handleRemoveOption = (sectionId: string, questionId: string, optionId: string) => {
    setManageSurvey(prev => ({
      ...prev,
      Sections: prev.Sections?.map(section => {
        if (section.SectionId === sectionId) {
          return {
            ...section,
            Questions: section.Questions?.map(question => {
              if (question.Id === questionId) {
                const updatedOptions = question.Options?.filter(o => o.Id !== optionId) || [];
                // Renumber options
                const renumberedOptions = updatedOptions.map((o, index) => ({
                  ...o,
                  Id: (index + 1).toString(),
                  Label: o.Label || `Option ${index + 1}`,
                  Points: index + 1,
                  SeqNo: index + 1
                }));
                return { ...question, Options: renumberedOptions };
              }
              return question;
            }) || []
          };
        }
        return section;
      }) || []
    }));
  };

  // Update option
  const handleOptionChange = (sectionId: string, questionId: string, optionId: string, field: string, value: any) => {
    setManageSurvey(prev => ({
      ...prev,
      Sections: prev.Sections?.map(section => {
        if (section.SectionId === sectionId) {
          return {
            ...section,
            Questions: section.Questions?.map(question => {
              if (question.Id === questionId) {
                return {
                  ...question,
                  Options: question.Options?.map(option => {
                    if (option.Id === optionId) {
                      return { ...option, [field]: value };
                    }
                    return option;
                  }) || []
                };
              }
              return question;
            }) || []
          };
        }
        return section;
      }) || []
    }));
  };

  // Validate survey
  const validateSurvey = (): boolean => {
    const errors: string[] = [];

    if (!manageSurvey.Code?.trim()) errors.push('Survey Code');
    if (!manageSurvey.Title?.trim()) errors.push('Page Title');
    if (!manageSurvey.StartDate) errors.push('Start Date');
    if (!manageSurvey.EndDate) errors.push('End Date');

    if (errors.length > 0) {
      setErrorMessage(`The mandatory fields are: ${errors.join(', ')}.`);
      return false;
    }

    // Validate sections and questions
    const sectionErrors: string[] = [];
    manageSurvey.Sections?.forEach((section, sectionIndex) => {
      const questionErrors: string[] = [];
      section.Questions?.forEach((question, questionIndex) => {
        const qErrors: string[] = [];
        if (!question.Label?.trim()) qErrors.push('Question');
        if (!question.Type?.trim()) qErrors.push('Type');
        
        if (qErrors.length > 0) {
          questionErrors.push(`Question ${questionIndex + 1} (${qErrors.join(', ')})`);
        }
      });
      
      if (questionErrors.length > 0) {
        sectionErrors.push(`${section.SectionTitle}: ${questionErrors.join('; ')}`);
      }
    });

    if (sectionErrors.length > 0) {
      setErrorMessage(`The following mandatory fields are missing:\n${sectionErrors.join('\n')}`);
      return false;
    }

    setErrorMessage('');
    return true;
  };

  // Save survey
  const handleSaveSurvey = async () => {
    if (!validateSurvey()) {
      return;
    }

    setLoading(true);
    try {
      // Prepare survey data for API
      const surveyData = {
        ...manageSurvey,
        UID: manageSurvey.SurveyId, // Include UID for update
        // Convert sections to match API structure
        Sections: manageSurvey.Sections?.map(section => ({
          ...section,
          Questions: section.Questions?.map(question => {
            const cleanedQuestion = { ...question };
            
            // Remove options for datetime type
            if (question.Type === CREATE_SURVEY_CONSTANTS.DropDownTypes.DateTime) {
              cleanedQuestion.Options = undefined;
            } else if (question.Type === CREATE_SURVEY_CONSTANTS.DropDownTypes.MultiDropdown) {
              // Remove unwanted properties for MultiDropdown type
              cleanedQuestion.MinDate = undefined;
              cleanedQuestion.MaxDate = undefined;
              cleanedQuestion.IsDateRequired = false;
              cleanedQuestion.IsTimeRequired = false;
            }
            
            return cleanedQuestion;
          })
        }))
      };

      await surveyService.createOrUpdateSurvey(surveyData);
      toast.success(isNew ? 'Survey created successfully' : 'Survey updated successfully');
      router.push('/administration/store-management/surveys');
    } catch (error) {
      console.error('Error saving survey:', error);
      toast.error(`Failed to ${isNew ? 'create' : 'update'} survey`);
    } finally {
      setLoading(false);
    }
  };


  // Calculate form progress helper
  const calculateFormProgress = () => {
    let totalFields = 4; // Basic fields: Code, Title, StartDate, EndDate
    let filledFields = 0;

    if (manageSurvey.Code?.trim()) filledFields++;
    if (manageSurvey.Title?.trim()) filledFields++;
    if (manageSurvey.StartDate) filledFields++;
    if (manageSurvey.EndDate) filledFields++;

    // Add section fields
    manageSurvey.Sections?.forEach(section => {
      totalFields += 1; // Section title
      if (section.SectionTitle?.trim()) filledFields++;

      section.Questions?.forEach(question => {
        totalFields += 2; // Question label and type
        if (question.Label?.trim()) filledFields++;
        if (question.Type?.trim()) filledFields++;
      });
    });

    return totalFields > 0 ? Math.round((filledFields / totalFields) * 100) : 0;
  };

  // Loading skeleton with enhanced design
  if (isLoadingData) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-gray-50 to-gray-100 dark:from-gray-900 dark:to-gray-950">
        <div className="container mx-auto p-6 space-y-6 max-w-7xl">
          <div className="bg-white dark:bg-gray-800 rounded-xl shadow-sm border border-gray-200 dark:border-gray-700 p-6">
            <div className="flex items-center gap-4">
              <Skeleton className="h-10 w-20" />
              <div>
                <Skeleton className="h-8 w-48" />
                <Skeleton className="h-4 w-64 mt-2" />
              </div>
            </div>
          </div>
          <Card className="border-gray-200 dark:border-gray-700">
            <CardHeader>
              <Skeleton className="h-6 w-32" />
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="grid grid-cols-2 gap-4">
                <Skeleton className="h-10 w-full" />
                <Skeleton className="h-10 w-full" />
              </div>
              <Skeleton className="h-20 w-full" />
              <div className="grid grid-cols-3 gap-4">
                <Skeleton className="h-10 w-full" />
                <Skeleton className="h-10 w-full" />
                <Skeleton className="h-10 w-full" />
              </div>
            </CardContent>
          </Card>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-gray-50 to-gray-100 dark:from-gray-900 dark:to-gray-950">
      <div className="container mx-auto p-6 space-y-6 max-w-7xl">
        {/* Enhanced Header with Progress */}
        <div className="bg-white dark:bg-gray-800 rounded-xl shadow-sm border border-gray-200 dark:border-gray-700 p-6">
          <div className="flex items-center justify-between mb-4">
            <div className="flex items-center gap-4">
              <Button
                variant="ghost"
                onClick={() => router.push('/administration/store-management/surveys')}
                className="flex items-center gap-2 hover:bg-gray-100 dark:hover:bg-gray-700 transition-all"
              >
                <ArrowLeft className="h-4 w-4" />
                <span className="hidden sm:inline">Back to Surveys</span>
              </Button>
              <div className="h-8 w-px bg-gray-200 dark:bg-gray-700" />
              <div>
                <div className="flex items-center gap-3">
                  <div className="p-2 bg-blue-100 dark:bg-blue-900 rounded-lg">
                    <ClipboardList className="h-5 w-5 text-blue-600 dark:text-blue-400" />
                  </div>
                  <div>
                    <h1 className="text-2xl font-bold text-gray-900 dark:text-gray-100">
                      {isNew ? 'Create New Survey' : 'Edit Survey'}
                    </h1>
                    <p className="text-sm text-gray-600 dark:text-gray-400">
                      {isNew ? 'Design and configure survey questions' : `Editing: ${manageSurvey.Code || 'Untitled Survey'}`}
                    </p>
                  </div>
                </div>
              </div>
            </div>
            {/* Progress Indicator */}
            <div className="hidden lg:flex items-center gap-3">
              <div className="text-right">
                <p className="text-xs text-gray-500 dark:text-gray-400">Form Progress</p>
                <p className="text-lg font-bold text-gray-900 dark:text-gray-100">{calculateFormProgress()}%</p>
              </div>
              <div className="relative w-32 h-2 bg-gray-200 dark:bg-gray-700 rounded-full overflow-hidden">
                <div 
                  className="absolute left-0 top-0 h-full bg-gradient-to-r from-blue-500 to-blue-600 rounded-full transition-all duration-300"
                  style={{ width: `${calculateFormProgress()}%` }}
                />
              </div>
            </div>
          </div>
        </div>

        {/* Error Message with Icon */}
        {errorMessage && (
          <div className="bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-lg p-4">
            <div className="flex items-start gap-3">
              <AlertCircle className="h-5 w-5 text-red-600 dark:text-red-400 flex-shrink-0 mt-0.5" />
              <div className="flex-1">
                <h4 className="text-sm font-semibold text-red-800 dark:text-red-300 mb-1">Validation Error</h4>
                <pre className="text-sm text-red-700 dark:text-red-400 whitespace-pre-wrap font-normal">{errorMessage}</pre>
              </div>
            </div>
          </div>
        )}

        {/* Disabled Warning with Enhanced Styling */}
        {isPageDisabled && !isNew && (
          <div className="bg-amber-50 dark:bg-amber-900/20 border border-amber-200 dark:border-amber-800 rounded-lg p-4">
            <div className="flex items-center justify-between">
              <div className="flex items-start gap-3">
                <Shield className="h-5 w-5 text-amber-600 dark:text-amber-400 flex-shrink-0 mt-0.5" />
                <div>
                  <h4 className="text-sm font-semibold text-amber-800 dark:text-amber-300 mb-1">Read-Only Mode</h4>
                  <p className="text-sm text-amber-700 dark:text-amber-400">
                    This survey has already started and is in read-only mode to preserve data integrity.
                  </p>
                </div>
              </div>
              <Button 
                variant="outline" 
                size="sm"
                onClick={() => setIsPageDisabled(false)}
                className="ml-4 text-amber-700 dark:text-amber-400 border-amber-500 dark:border-amber-600 hover:bg-amber-100 dark:hover:bg-amber-900/40"
              >
                <Settings2 className="h-4 w-4 mr-1" />
                Override
              </Button>
            </div>
          </div>
        )}

        <div className="space-y-6">
          {/* Survey Details Section with Enhanced Design */}
          <Card className="border-gray-200 dark:border-gray-700 shadow-sm hover:shadow-md transition-shadow">
            <CardHeader className="bg-gradient-to-r from-gray-50 to-gray-100 dark:from-gray-800 dark:to-gray-850 border-b border-gray-200 dark:border-gray-700">
              <div className="flex items-center justify-between">
                <div className="flex items-center gap-3">
                  <div className="p-2 bg-white dark:bg-gray-800 rounded-lg shadow-sm">
                    <FileText className="h-5 w-5 text-gray-600 dark:text-gray-400" />
                  </div>
                  <CardTitle className="text-lg font-semibold text-gray-900 dark:text-gray-100">Survey Information</CardTitle>
                </div>
                {/* Active/Inactive Status in Header */}
                <div className="flex items-center gap-2">
                  <span className="text-xs text-gray-500 dark:text-gray-400">Status:</span>
                  <Badge
                    variant={manageSurvey.IsActive ? "default" : "secondary"}
                    className={cn(
                      "text-xs",
                      manageSurvey.IsActive
                        ? "bg-green-100 text-green-700 dark:bg-green-900/30 dark:text-green-400"
                        : "bg-gray-100 text-gray-600 dark:bg-gray-800 dark:text-gray-400"
                    )}
                  >
                    {manageSurvey.IsActive ? "Active" : "Inactive"}
                  </Badge>
                  <Switch
                    checked={manageSurvey.IsActive}
                    onCheckedChange={(checked) => setManageSurvey(prev => ({ ...prev, IsActive: !!checked }))}
                    disabled={loading || isPageDisabled}
                    className="h-4 w-8"
                  />
                </div>
              </div>
            </CardHeader>
            <CardContent className="p-6 space-y-6">
              {/* First Row with Icon Labels */}
              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div className="space-y-2">
                  <Label htmlFor="surveyCode" className="flex items-center gap-2 text-sm font-medium text-gray-700 dark:text-gray-300">
                    <Hash className="h-4 w-4" />
                    Survey Code <span className="text-red-500">*</span>
                  </Label>
                  <div className="relative">
                    <Input
                      id="surveyCode"
                      placeholder="e.g., SURVEY-2024-001"
                      maxLength={50}
                      value={manageSurvey.Code || ''}
                      onChange={(e) => setManageSurvey(prev => ({ ...prev, Code: e.target.value }))}
                      disabled={loading || isPageDisabled || !isNew}
                      className="pl-3 pr-3 py-2 border-gray-300 dark:border-gray-600 focus:ring-2 focus:ring-blue-500 dark:focus:ring-blue-400 transition-all"
                    />
                    {manageSurvey.Code && (
                      <div className="absolute right-2 top-1/2 -translate-y-1/2">
                        <CheckSquare className="h-4 w-4 text-green-500" />
                      </div>
                    )}
                  </div>
                </div>
                <div className="space-y-2">
                  <Label htmlFor="pageTitle" className="flex items-center gap-2 text-sm font-medium text-gray-700 dark:text-gray-300">
                    <Type className="h-4 w-4" />
                    Survey Title <span className="text-red-500">*</span>
                  </Label>
                  <div className="relative">
                    <Input
                      id="pageTitle"
                      placeholder="e.g., Customer Satisfaction Survey"
                      maxLength={200}
                      value={manageSurvey.Title || ''}
                      onChange={(e) => setManageSurvey(prev => ({ ...prev, Title: e.target.value }))}
                      disabled={loading || isPageDisabled}
                      className="pl-3 pr-3 py-2 border-gray-300 dark:border-gray-600 focus:ring-2 focus:ring-blue-500 dark:focus:ring-blue-400 transition-all"
                    />
                    {manageSurvey.Title && (
                      <div className="absolute right-2 top-1/2 -translate-y-1/2">
                        <CheckSquare className="h-4 w-4 text-green-500" />
                      </div>
                    )}
                  </div>
                </div>
              </div>

              {/* Enhanced Description Field */}
              <div className="space-y-2">
                <Label htmlFor="description" className="flex items-center gap-2 text-sm font-medium text-gray-700 dark:text-gray-300">
                  <Info className="h-4 w-4" />
                  Survey Description
                </Label>
                <Textarea
                  id="description"
                  placeholder="Provide a brief description of the survey purpose and instructions for respondents..."
                  maxLength={500}
                  rows={4}
                  value={manageSurvey.Description || ''}
                  onChange={(e) => setManageSurvey(prev => ({ ...prev, Description: e.target.value }))}
                  disabled={loading || isPageDisabled}
                  className="resize-none border-gray-300 dark:border-gray-600 focus:ring-2 focus:ring-blue-500 dark:focus:ring-blue-400 transition-all"
                />
                <p className="text-xs text-gray-500 dark:text-gray-400 text-right">
                  {manageSurvey.Description?.length || 0}/500 characters
                </p>
              </div>

              {/* Enhanced Date Range Section */}
              <div className="bg-gray-50 dark:bg-gray-900 rounded-lg p-4 space-y-4">
                <h4 className="text-sm font-semibold text-gray-700 dark:text-gray-300 flex items-center gap-2">
                  <CalendarDays className="h-4 w-4" />
                  Survey Schedule
                </h4>
                <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                  <div className="space-y-2">
                    <Label htmlFor="startDate" className="flex items-center gap-2 text-sm font-medium text-gray-700 dark:text-gray-300">
                      <Calendar className="h-4 w-4" />
                      Start Date <span className="text-red-500">*</span>
                    </Label>
                    <Input
                      id="startDate"
                      type="date"
                      value={manageSurvey.StartDate || ''}
                      onChange={(e) => {
                        setManageSurvey(prev => ({ 
                          ...prev, 
                          StartDate: e.target.value 
                        }));
                        if (manageSurvey.EndDate && e.target.value > manageSurvey.EndDate) {
                          const startDate = new Date(e.target.value);
                          startDate.setDate(startDate.getDate() + 30);
                          setManageSurvey(prev => ({ 
                            ...prev, 
                            EndDate: startDate.toISOString().split('T')[0]
                          }));
                        }
                      }}
                      min={isNew ? new Date().toISOString().split('T')[0] : undefined}
                      disabled={loading || isPageDisabled}
                      className="h-10 border-gray-300 dark:border-gray-600 focus:ring-2 focus:ring-blue-500 dark:focus:ring-blue-400"
                    />
                    <p className="text-xs text-gray-500 dark:text-gray-400">When the survey begins</p>
                  </div>
                  
                  <div className="space-y-2">
                    <Label htmlFor="endDate" className="flex items-center gap-2 text-sm font-medium text-gray-700 dark:text-gray-300">
                      <Clock className="h-4 w-4" />
                      End Date <span className="text-red-500">*</span>
                    </Label>
                    <Input
                      id="endDate"
                      type="date"
                      value={manageSurvey.EndDate || ''}
                      onChange={(e) => setManageSurvey(prev => ({ 
                        ...prev, 
                        EndDate: e.target.value 
                      }))}
                      min={manageSurvey.StartDate ? 
                        (() => {
                          const date = new Date(manageSurvey.StartDate);
                          date.setDate(date.getDate() + 1);
                          return date.toISOString().split('T')[0];
                        })() : 
                        new Date().toISOString().split('T')[0]
                      }
                      disabled={loading || isPageDisabled}
                      className="h-10 border-gray-300 dark:border-gray-600 focus:ring-2 focus:ring-blue-500 dark:focus:ring-blue-400"
                    />
                    <p className="text-xs text-gray-500 dark:text-gray-400">When the survey expires</p>
                  </div>

                  <div className="flex items-center justify-center md:justify-start">
                    <div className="bg-white dark:bg-gray-800 rounded-lg p-4 border border-gray-200 dark:border-gray-700">
                      <div className="space-y-2">
                        <p className="text-xs font-medium text-gray-600 dark:text-gray-400 uppercase tracking-wide">Duration</p>
                        <p className="text-sm font-semibold text-gray-900 dark:text-gray-100">
                          {manageSurvey.StartDate && manageSurvey.EndDate ? 
                            (() => {
                              const start = new Date(manageSurvey.StartDate);
                              const end = new Date(manageSurvey.EndDate);
                              const diffTime = Math.abs(end.getTime() - start.getTime());
                              const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
                              return `${diffDays} days`;
                            })()
                            : 'Not set'
                          }
                        </p>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
          </CardContent>
        </Card>

          {/* Enhanced Questions Section */}
          <Card className="border-gray-200 dark:border-gray-700 shadow-sm hover:shadow-md transition-shadow">
            <CardHeader className="bg-gradient-to-r from-blue-50 to-indigo-50 dark:from-blue-900/20 dark:to-indigo-900/20 border-b border-gray-200 dark:border-gray-700">
              <div className="flex items-center justify-between">
                <div className="flex items-center gap-3">
                  <div className="p-2 bg-white dark:bg-gray-800 rounded-lg shadow-sm">
                    <ListChecks className="h-5 w-5 text-blue-600 dark:text-blue-400" />
                  </div>
                  <div>
                    <CardTitle className="text-lg font-semibold text-gray-900 dark:text-gray-100">Survey Questions</CardTitle>
                    <p className="text-sm text-gray-600 dark:text-gray-400 mt-1">
                      {manageSurvey.Sections?.length || 0} sections, {manageSurvey.Sections?.reduce((acc, s) => acc + (s.Questions?.length || 0), 0) || 0} questions
                    </p>
                  </div>
                </div>
                <Button
                  onClick={handleAddSection}
                  disabled={loading || isPageDisabled}
                  className="bg-blue-600 hover:bg-blue-700 text-white flex items-center gap-2 shadow-sm"
                >
                  <Plus className="h-4 w-4" />
                  Add Section
                </Button>
              </div>
            </CardHeader>
            <CardContent className="p-6">
              {manageSurvey.Sections && manageSurvey.Sections.length > 0 ? (
                <div className="space-y-4">
                  {manageSurvey.Sections.map((section, sectionIndex) => (
                    <div key={section.SectionId || `section-${sectionIndex}`} className="border border-gray-200 dark:border-gray-700 rounded-lg overflow-hidden shadow-sm hover:shadow-md transition-shadow">
                      {/* Enhanced Section Header */}
                      <div className="bg-gradient-to-r from-gray-50 to-gray-100 dark:from-gray-800 dark:to-gray-850 border-b border-gray-200 dark:border-gray-700">
                        <Button
                          variant="ghost"
                          onClick={() => toggleSection(section.SectionId!)}
                          className="w-full p-4 flex items-center justify-between hover:bg-gray-100/50 dark:hover:bg-gray-700/50 transition-all"
                        >
                          <div className="flex items-center gap-3">
                            <div className="flex items-center justify-center w-8 h-8 rounded-full bg-blue-100 dark:bg-blue-900/50 text-blue-600 dark:text-blue-400 font-semibold text-sm">
                              {sectionIndex + 1}
                            </div>
                            <span className="font-semibold text-gray-900 dark:text-gray-100">{section.SectionTitle}</span>
                            <span className="text-sm text-gray-500 dark:text-gray-400">({section.Questions?.length || 0} questions)</span>
                          </div>
                          <div className="flex items-center gap-2">
                            {activeSectionId === section.SectionId ? (
                              <ChevronUp className="h-5 w-5 text-gray-500" />
                            ) : (
                              <ChevronDown className="h-5 w-5 text-gray-500" />
                            )}
                          </div>
                        </Button>
                      </div>

                      {/* Enhanced Section Content */}
                      {activeSectionId === section.SectionId && (
                        <div className="p-6 bg-gray-50/50 dark:bg-gray-900/20 space-y-4">
                          <div className="flex justify-end">
                            <Button
                              variant="outline"
                              size="sm"
                              onClick={() => handleDeleteSection(section.SectionId!)}
                              disabled={loading || isPageDisabled}
                              className="text-red-600 dark:text-red-400 border-red-300 dark:border-red-700 hover:bg-red-50 dark:hover:bg-red-900/20 flex items-center gap-2"
                            >
                              <Trash2 className="h-4 w-4" />
                              Delete Section
                            </Button>
                          </div>

                        {/* Questions */}
                        {section.Questions?.map((question, questionIndex) => (
                          <QuestionComponent
                            key={question.Id}
                            question={question}
                            sectionId={section.SectionId!}
                            questionIndex={questionIndex}
                            totalQuestions={section.Questions?.length || 0}
                            onQuestionChange={handleQuestionChange}
                            onAddQuestion={() => handleAddQuestion(section.SectionId!)}
                            onRemoveQuestion={() => handleRemoveQuestion(section.SectionId!, question.Id!)}
                            onAddOption={() => handleAddOption(section.SectionId!, question.Id!)}
                            onRemoveOption={(optionId) => handleRemoveOption(section.SectionId!, question.Id!, optionId)}
                            onOptionChange={handleOptionChange}
                            disabled={loading || isPageDisabled}
                          />
                        ))}
                      </div>
                    )}
                  </div>
                ))}
              </div>
              ) : (
                <div className="text-center py-12">
                  <div className="flex flex-col items-center gap-4">
                    <div className="p-4 bg-gray-100 dark:bg-gray-800 rounded-full">
                      <ListChecks className="h-8 w-8 text-gray-400 dark:text-gray-600" />
                    </div>
                    <div>
                      <h3 className="text-lg font-medium text-gray-900 dark:text-gray-100">No sections yet</h3>
                      <p className="text-sm text-gray-500 dark:text-gray-400 mt-1">
                        Start building your survey by adding the first section
                      </p>
                    </div>
                    <Button
                      onClick={handleAddSection}
                      disabled={loading || isPageDisabled}
                      className="bg-blue-600 hover:bg-blue-700 text-white flex items-center gap-2 shadow-sm"
                    >
                      <Plus className="h-4 w-4" />
                      Add First Section
                    </Button>
                  </div>
                </div>
              )}
          </CardContent>
        </Card>

          {/* Enhanced Submit Buttons */}
          <div className="bg-white dark:bg-gray-800 rounded-xl shadow-sm border border-gray-200 dark:border-gray-700 p-6">
            <div className="flex items-center justify-between">
              <div className="text-sm text-gray-600 dark:text-gray-400">
                {manageSurvey.Sections?.length === 0 && (
                  <p className="flex items-center gap-2">
                    <AlertCircle className="h-4 w-4" />
                    Add at least one section to save the survey
                  </p>
                )}
              </div>
              <div className="flex items-center gap-3">
                <Button
                  variant="outline"
                  onClick={() => router.push('/administration/store-management/surveys')}
                  disabled={loading}
                  className="border-gray-300 dark:border-gray-600 hover:bg-gray-100 dark:hover:bg-gray-700"
                >
                  Cancel
                </Button>
                <Button
                  onClick={handleSaveSurvey}
                  disabled={loading || isPageDisabled || manageSurvey.Sections?.length === 0}
                  className="bg-gradient-to-r from-blue-600 to-indigo-600 hover:from-blue-700 hover:to-indigo-700 text-white shadow-sm flex items-center gap-2 min-w-[140px]"
                >
                  {loading ? (
                    <>
                      <Loader2 className="h-4 w-4 animate-spin" />
                      {isNew ? 'Creating...' : 'Updating...'}
                    </>
                  ) : (
                    <>
                      <Save className="h-4 w-4" />
                      {isNew ? 'Create Survey' : 'Save Changes'}
                    </>
                  )}
                </Button>
              </div>
            </div>
          </div>
        </div>

        {/* Enhanced Add Section Dialog */}
        <Dialog open={isAddSectionPopup} onOpenChange={setIsAddSectionPopup}>
          <DialogContent className="sm:max-w-md">
            <DialogHeader>
              <DialogTitle className="flex items-center gap-2">
                <ListChecks className="h-5 w-5 text-blue-600" />
                Add New Section
              </DialogTitle>
              <p className="text-sm text-gray-500 dark:text-gray-400 mt-2">
                Sections help organize related questions together
              </p>
            </DialogHeader>
            <div className="space-y-4 mt-4">
              <div className="space-y-2">
                <Label htmlFor="sectionName" className="text-sm font-medium">
                  Section Name <span className="text-red-500">*</span>
                </Label>
                <Input
                  id="sectionName"
                  value={newSectionName}
                  onChange={(e) => setNewSectionName(e.target.value)}
                  placeholder="e.g., Demographics, Satisfaction Rating"
                  className="w-full"
                  autoFocus
                />
                {newSectionError && (
                  <p className="text-red-500 text-sm flex items-center gap-1">
                    <AlertCircle className="h-3 w-3" />
                    {newSectionError}
                  </p>
                )}
              </div>
            </div>
            <DialogFooter className="mt-6">
              <Button 
                variant="outline" 
                onClick={() => setIsAddSectionPopup(false)}
                className="border-gray-300 dark:border-gray-600"
              >
                Cancel
              </Button>
              <Button 
                onClick={handleSaveSection}
                className="bg-blue-600 hover:bg-blue-700 text-white"
              >
                <Plus className="h-4 w-4 mr-1" />
                Add Section
              </Button>
            </DialogFooter>
          </DialogContent>
        </Dialog>
      </div>
    </div>
  );
}

// Question Component (reusing from create page with minor modifications)
interface QuestionComponentProps {
  question: IManageQuestion;
  sectionId: string;
  questionIndex: number;
  totalQuestions: number;
  onQuestionChange: (sectionId: string, questionId: string, field: string, value: any) => void;
  onAddQuestion: () => void;
  onRemoveQuestion: () => void;
  onAddOption: () => void;
  onRemoveOption: (optionId: string) => void;
  onOptionChange: (sectionId: string, questionId: string, optionId: string, field: string, value: any) => void;
  disabled?: boolean;
}

const QuestionComponent: React.FC<QuestionComponentProps> = ({
  question,
  sectionId,
  questionIndex,
  totalQuestions,
  onQuestionChange,
  onAddQuestion,
  onRemoveQuestion,
  onAddOption,
  onRemoveOption,
  onOptionChange,
  disabled = false
}) => {
  const isMultiChoiceType = [
    CREATE_SURVEY_CONSTANTS.DropDownTypes.Radio,
    CREATE_SURVEY_CONSTANTS.DropDownTypes.Dropdown,
    CREATE_SURVEY_CONSTANTS.DropDownTypes.MultiDropdown,
    CREATE_SURVEY_CONSTANTS.DropDownTypes.Checkbox
  ].includes(question.Type || '');

  const isDateTimeType = question.Type === CREATE_SURVEY_CONSTANTS.DropDownTypes.DateTime;


  return (
    <div className="bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-700 rounded-xl shadow-sm hover:shadow-md transition-all overflow-hidden">
      {/* Question Header */}
      <div className="bg-gradient-to-r from-blue-50 to-indigo-50 dark:from-blue-900/10 dark:to-indigo-900/10 border-b border-gray-200 dark:border-gray-700 px-6 py-4">
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-3">
            <div className="flex items-center justify-center w-8 h-8 rounded-full bg-blue-100 dark:bg-blue-900/50 text-blue-600 dark:text-blue-400 font-semibold text-sm">
              Q{questionIndex + 1}
            </div>
            <div>
              <h3 className="text-lg font-semibold text-gray-900 dark:text-gray-100">{question.LabelQuestion}</h3>
              <p className="text-sm text-gray-500 dark:text-gray-400">Configure question settings and options</p>
            </div>
          </div>
          <div className="flex items-center gap-2">
            <Button
              onClick={onAddQuestion}
              size="sm"
              variant="outline"
              disabled={disabled}
              className="bg-white hover:bg-blue-50 border-blue-200 text-blue-600 flex items-center gap-1"
            >
              <Plus className="h-3 w-3" />
              Add
            </Button>
            <Button
              onClick={onRemoveQuestion}
              size="sm"
              variant="outline"
              disabled={disabled || totalQuestions === 1}
              className="bg-white hover:bg-red-50 border-red-200 text-red-600 flex items-center gap-1"
            >
              <Minus className="h-3 w-3" />
              Remove
            </Button>
          </div>
        </div>
      </div>

      <div className="p-6 space-y-6">
        {/* Question Settings */}
        <div className="bg-gray-50 dark:bg-gray-900/30 rounded-lg p-4">
          <h4 className="text-sm font-semibold text-gray-700 dark:text-gray-300 mb-4 flex items-center gap-2">
            <Settings2 className="h-4 w-4" />
            Question Settings
          </h4>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            
            <div className="bg-white dark:bg-gray-800 rounded-lg p-3 border border-gray-200 dark:border-gray-700">
              <div className="flex items-center space-x-2">
                <Checkbox
                  id={`required-${question.Id}`}
                  checked={question.IsRequired}
                  onCheckedChange={(checked) => onQuestionChange(sectionId, question.Id!, 'IsRequired', !!checked)}
                  disabled={disabled}
                  className="data-[state=checked]:bg-red-500 data-[state=checked]:border-red-500"
                />
                <Label htmlFor={`required-${question.Id}`} className="text-sm font-medium cursor-pointer">
                  Required
                </Label>
              </div>
              <div className="flex items-center gap-1 mt-1">
                <AlertCircle className="h-3 w-3 text-red-500" />
                <span className="text-xs text-gray-500 dark:text-gray-400">Must answer</span>
              </div>
            </div>
            
            <div className="bg-white dark:bg-gray-800 rounded-lg p-3 border border-gray-200 dark:border-gray-700">
              <div className="flex items-center space-x-2">
                <Checkbox
                  id={`cameraVisible-${question.Id}`}
                  checked={question.IsCameraVisible}
                  onCheckedChange={(checked) => onQuestionChange(sectionId, question.Id!, 'IsCameraVisible', !!checked)}
                  disabled={disabled}
                  className="data-[state=checked]:bg-green-500 data-[state=checked]:border-green-500"
                />
                <Label htmlFor={`cameraVisible-${question.Id}`} className="text-sm font-medium cursor-pointer">
                  Camera Access
                </Label>
              </div>
              <div className="flex items-center gap-1 mt-1">
                <Camera className="h-3 w-3 text-green-500" />
                <span className="text-xs text-gray-500 dark:text-gray-400">Photo capture</span>
              </div>
            </div>
            
          </div>
        </div>

        {/* Question Configuration */}
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
          {/* Question Text */}
          <div className="lg:col-span-2 space-y-2">
            <Label className="text-sm font-semibold text-gray-700 dark:text-gray-300 flex items-center gap-2">
              <Type className="h-4 w-4" />
              Question Text <span className="text-red-500">*</span>
            </Label>
            <Input
              placeholder="Enter your question here..."
              value={question.Label || ''}
              onChange={(e) => onQuestionChange(sectionId, question.Id!, 'Label', e.target.value)}
              disabled={disabled}
              className="text-base py-3 border-gray-300 dark:border-gray-600 focus:ring-2 focus:ring-blue-500 dark:focus:ring-blue-400"
            />
            <p className="text-xs text-gray-500 dark:text-gray-400">This is what respondents will see</p>
          </div>
          
          {/* Question Type */}
          <div className="space-y-2">
            <Label className="text-sm font-semibold text-gray-700 dark:text-gray-300 flex items-center gap-2">
              <Settings2 className="h-4 w-4" />
              Question Type <span className="text-red-500">*</span>
            </Label>
            <Select
              value={question.Type || ''}
              onValueChange={(value) => onQuestionChange(sectionId, question.Id!, 'Type', value)}
              disabled={disabled}
            >
              <SelectTrigger className="h-12 border-gray-300 dark:border-gray-600 focus:ring-2 focus:ring-blue-500 dark:focus:ring-blue-400">
                <SelectValue placeholder="Choose type..." />
              </SelectTrigger>
              <SelectContent>
                {QUESTION_TYPES.map((type) => (
                  <SelectItem key={type.Code} value={type.Code!} className="py-3">
                    <div className="flex items-center gap-2">
                      <span className="font-medium">{type.Label}</span>
                    </div>
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            
          </div>
        </div>

        {/* Date/Time Configuration for DateTime type */}
        {isDateTimeType && (
          <div className="bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-700 rounded-lg">
            <div className="bg-gradient-to-r from-purple-50 to-violet-50 dark:from-purple-900/10 dark:to-violet-900/10 border-b border-gray-200 dark:border-gray-700 px-4 py-3">
              <div className="flex items-center gap-2">
                <CalendarDays className="h-5 w-5 text-purple-600 dark:text-purple-400" />
                <h4 className="text-lg font-semibold text-gray-900 dark:text-gray-100">Date & Time Configuration</h4>
              </div>
            </div>
            
            <div className="p-4 space-y-6">
              {/* Date/Time Options */}
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div className="bg-white dark:bg-gray-800 rounded-lg p-4 border border-gray-200 dark:border-gray-700">
                  <div className="flex items-center space-x-2">
                    <Checkbox
                      id={`dateRequired-${question.Id}`}
                      checked={question.IsDateRequired}
                      onCheckedChange={(checked) => onQuestionChange(sectionId, question.Id!, 'IsDateRequired', !!checked)}
                      disabled={disabled}
                      className="data-[state=checked]:bg-purple-500 data-[state=checked]:border-purple-500"
                    />
                    <Label htmlFor={`dateRequired-${question.Id}`} className="text-sm font-medium cursor-pointer">
                      Require Date
                    </Label>
                  </div>
                  <div className="flex items-center gap-1 mt-1">
                    <Calendar className="h-3 w-3 text-purple-500" />
                    <span className="text-xs text-gray-500 dark:text-gray-400">Date picker</span>
                  </div>
                </div>
                
                <div className="bg-white dark:bg-gray-800 rounded-lg p-4 border border-gray-200 dark:border-gray-700">
                  <div className="flex items-center space-x-2">
                    <Checkbox
                      id={`timeRequired-${question.Id}`}
                      checked={question.IsTimeRequired}
                      onCheckedChange={(checked) => onQuestionChange(sectionId, question.Id!, 'IsTimeRequired', !!checked)}
                      disabled={disabled}
                      className="data-[state=checked]:bg-purple-500 data-[state=checked]:border-purple-500"
                    />
                    <Label htmlFor={`timeRequired-${question.Id}`} className="text-sm font-medium cursor-pointer">
                      Require Time
                    </Label>
                  </div>
                  <div className="flex items-center gap-1 mt-1">
                    <Clock className="h-3 w-3 text-purple-500" />
                    <span className="text-xs text-gray-500 dark:text-gray-400">Time picker</span>
                  </div>
                </div>
              </div>

              {/* Date Range Configuration */}
              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div className="space-y-3">
                  <Label className="text-sm font-semibold text-gray-700 dark:text-gray-300 flex items-center gap-2">
                    <Calendar className="h-4 w-4" />
                    Minimum Date
                  </Label>
                  <Select
                    value={question.MinDate || ''}
                    onValueChange={(value) => onQuestionChange(sectionId, question.Id!, 'MinDate', value)}
                    disabled={disabled}
                  >
                    <SelectTrigger className="h-12 border-gray-300 dark:border-gray-600">
                      <SelectValue placeholder="Select minimum date..." />
                    </SelectTrigger>
                    <SelectContent>
                      {MIN_DATE_TYPES.map((type) => (
                        <SelectItem key={type.Code} value={type.Code!} className="py-3">
                          {type.Label}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                  {question.MinDate === CREATE_SURVEY_CONSTANTS.DateTypes.SpecificDate && (
                    <Input
                      type="date"
                      value={question.MinSpecificDate || ''}
                      onChange={(e) => onQuestionChange(sectionId, question.Id!, 'MinSpecificDate', e.target.value)}
                      disabled={disabled}
                      className="mt-2 border-gray-300 dark:border-gray-600"
                    />
                  )}
                </div>

                <div className="space-y-3">
                  <Label className="text-sm font-semibold text-gray-700 dark:text-gray-300 flex items-center gap-2">
                    <Calendar className="h-4 w-4" />
                    Maximum Date
                  </Label>
                  <Select
                    value={question.MaxDate || ''}
                    onValueChange={(value) => onQuestionChange(sectionId, question.Id!, 'MaxDate', value)}
                    disabled={disabled}
                  >
                    <SelectTrigger className="h-12 border-gray-300 dark:border-gray-600">
                      <SelectValue placeholder="Select maximum date..." />
                    </SelectTrigger>
                    <SelectContent>
                      {MAX_DATE_TYPES.map((type) => (
                        <SelectItem key={type.Code} value={type.Code!} className="py-3">
                          {type.Label}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                  {question.MaxDate === CREATE_SURVEY_CONSTANTS.DateTypes.SpecificDate && (
                    <Input
                      type="date"
                      value={question.MaxSpecificDate || ''}
                      onChange={(e) => onQuestionChange(sectionId, question.Id!, 'MaxSpecificDate', e.target.value)}
                      disabled={disabled}
                      className="mt-2 border-gray-300 dark:border-gray-600"
                    />
                  )}
                </div>
              </div>
            </div>
          </div>
        )}
      </div>

        {/* Options for Multi-Choice Types */}
        {isMultiChoiceType && (
          <div className="bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-700 rounded-lg">
            <div className="bg-gradient-to-r from-green-50 to-emerald-50 dark:from-green-900/10 dark:to-emerald-900/10 border-b border-gray-200 dark:border-gray-700 px-4 py-3">
              <div className="flex items-center justify-between">
                <div className="flex items-center gap-2">
                  <ListChecks className="h-5 w-5 text-green-600 dark:text-green-400" />
                  <h4 className="text-lg font-semibold text-gray-900 dark:text-gray-100">Answer Options</h4>
                  <Badge variant="outline" className="bg-green-100 text-green-700 border-green-300 dark:bg-green-900/30 dark:text-green-400 dark:border-green-700">
                    {question.Options?.length || 0} options
                  </Badge>
                </div>
                <Button
                  onClick={onAddOption}
                  size="sm"
                  disabled={disabled}
                  className="bg-green-600 hover:bg-green-700 text-white flex items-center gap-1"
                >
                  <Plus className="h-3 w-3" />
                  Add Option
                </Button>
              </div>
            </div>
            
            <div className="p-4 space-y-3">
              {question.Options?.map((option, optionIndex) => (
                <div key={option.Id} className="bg-gray-50 dark:bg-gray-900/30 rounded-lg p-4 border border-gray-200 dark:border-gray-700">
                  <div className="grid grid-cols-1 md:grid-cols-12 gap-4 items-center">
                    {/* Option Number */}
                    <div className="md:col-span-1 flex items-center justify-center">
                      <div className="w-8 h-8 rounded-full bg-green-100 dark:bg-green-900/50 text-green-600 dark:text-green-400 flex items-center justify-center font-semibold text-sm">
                        {optionIndex + 1}
                      </div>
                    </div>
                    
                    {/* Option Text */}
                    <div className="md:col-span-7">
                      <Label className="text-sm font-medium text-gray-700 dark:text-gray-300 mb-2 block">
                        Option {optionIndex + 1}
                      </Label>
                      <Input
                        placeholder="Enter option text..."
                        value={option.Label || ''}
                        onChange={(e) => onOptionChange(sectionId, question.Id!, option.Id!, 'Label', e.target.value)}
                        disabled={disabled}
                        className="border-gray-300 dark:border-gray-600 focus:ring-2 focus:ring-green-500 dark:focus:ring-green-400"
                      />
                    </div>


                    {/* Actions */}
                    <div className="md:col-span-2 flex gap-2 justify-end">
                      <Button
                        onClick={() => onRemoveOption(option.Id!)}
                        size="sm"
                        variant="outline"
                        disabled={disabled || question.Options?.length === 1}
                        className="text-red-600 hover:text-red-700 hover:bg-red-50 border-red-300 dark:border-red-700 dark:text-red-400 dark:hover:bg-red-900/20"
                      >
                        <Trash2 className="h-3 w-3" />
                      </Button>
                    </div>
                  </div>
                </div>
              ))}
              
              {(!question.Options || question.Options.length === 0) && (
                <div className="text-center py-8 text-gray-500 dark:text-gray-400">
                  <ListChecks className="h-8 w-8 mx-auto mb-2 opacity-50" />
                  <p className="text-sm">No options added yet</p>
                  <p className="text-xs">Click "Add Option" to create answer choices</p>
                </div>
              )}
            </div>
          </div>
        )}
    </div>
  );
};