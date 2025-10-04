/**
 * Survey Creation Types
 * Extended type definitions for Survey creation functionality
 */

export interface IManageSurvey {
  SurveyId?: string;
  Code?: string;
  Title?: string;
  Description?: string;
  StartDate?: string;
  EndDate?: string;
  IsActive?: boolean;
  Sections?: IManageSection[];
}

export interface IManageSection {
  SectionId?: string;
  SectionTitle?: string;
  SeqNo?: number;
  Questions?: IManageQuestion[];
}

export interface IManageQuestion {
  Id?: string;
  LabelQuestion?: string;
  IsScoreRequired?: boolean;
  IsDateRequired?: boolean;
  IsTimeRequired?: boolean;
  MinDate?: string;
  MinSpecificDate?: string;
  MaxDate?: string;
  MaxSpecificDate?: string;
  IsRequired?: boolean;
  IsCameraVisible?: boolean;
  Type?: string;
  Label?: string;
  SeqNo?: number;
  Options?: IManageOption[];
  Validations?: IValidations;
}

export interface IManageOption {
  Id?: string;
  Label?: string;
  Points?: number;
  Score?: number;
  SeqNo?: number;
}

export interface IValidations {
  is_mandatory?: boolean;
}

export interface ISelectionItem {
  UID?: string;
  Code?: string;
  Label?: string;
  IsSelected?: boolean;
}

export const CREATE_SURVEY_CONSTANTS = {
  DropDownTypes: {
    Radio: 'radio',
    Dropdown: 'dropdown',
    MultiDropdown: 'multidropdown',
    Checkbox: 'checkbox',
    DateTime: 'datetime',
    TextBox: 'textbox',
    TextArea: 'textarea'
  },
  DateTypes: {
    Today: 'today',
    Yesterday: 'yesterday',
    Tomorrow: 'tomorrow',
    CurrentMonth: 'current_month',
    CurrentYear: 'current_year',
    SpecificDate: 'specific_date',
    None: 'none'
  }
} as const;

export interface CalendarWrappedData {
  Id: string;
  SelectedValue: string;
}

export interface DropDownEvent {
  SelectionItems?: ISelectionItem[];
}

export const QUESTION_TYPES: ISelectionItem[] = [
  { Code: CREATE_SURVEY_CONSTANTS.DropDownTypes.Radio, Label: 'Radio Button', UID: '1' },
  { Code: CREATE_SURVEY_CONSTANTS.DropDownTypes.Dropdown, Label: 'Dropdown', UID: '2' },
  { Code: CREATE_SURVEY_CONSTANTS.DropDownTypes.MultiDropdown, Label: 'Multi Dropdown', UID: '3' },
  { Code: CREATE_SURVEY_CONSTANTS.DropDownTypes.Checkbox, Label: 'Checkbox', UID: '4' },
  { Code: CREATE_SURVEY_CONSTANTS.DropDownTypes.DateTime, Label: 'Date Time', UID: '5' },
  { Code: CREATE_SURVEY_CONSTANTS.DropDownTypes.TextBox, Label: 'Text Box', UID: '6' },
  { Code: CREATE_SURVEY_CONSTANTS.DropDownTypes.TextArea, Label: 'Text Area', UID: '7' }
];

export const MIN_DATE_TYPES: ISelectionItem[] = [
  { Code: CREATE_SURVEY_CONSTANTS.DateTypes.Today, Label: 'Today', UID: '1' },
  { Code: CREATE_SURVEY_CONSTANTS.DateTypes.Yesterday, Label: 'Yesterday', UID: '2' },
  { Code: CREATE_SURVEY_CONSTANTS.DateTypes.CurrentMonth, Label: 'Current Month', UID: '3' },
  { Code: CREATE_SURVEY_CONSTANTS.DateTypes.CurrentYear, Label: 'Current Year', UID: '4' },
  { Code: CREATE_SURVEY_CONSTANTS.DateTypes.SpecificDate, Label: 'Specific Date', UID: '5' },
  { Code: CREATE_SURVEY_CONSTANTS.DateTypes.None, Label: 'None', UID: '6' }
];

export const MAX_DATE_TYPES: ISelectionItem[] = [
  { Code: CREATE_SURVEY_CONSTANTS.DateTypes.Today, Label: 'Today', UID: '1' },
  { Code: CREATE_SURVEY_CONSTANTS.DateTypes.Tomorrow, Label: 'Tomorrow', UID: '2' },
  { Code: CREATE_SURVEY_CONSTANTS.DateTypes.CurrentMonth, Label: 'Current Month', UID: '3' },
  { Code: CREATE_SURVEY_CONSTANTS.DateTypes.CurrentYear, Label: 'Current Year', UID: '4' },
  { Code: CREATE_SURVEY_CONSTANTS.DateTypes.SpecificDate, Label: 'Specific Date', UID: '5' },
  { Code: CREATE_SURVEY_CONSTANTS.DateTypes.None, Label: 'None', UID: '6' }
];