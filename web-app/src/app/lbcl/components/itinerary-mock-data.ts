// Mock data extracted from FSM_Itinerary Template2.xlsx
// This will be replaced with API calls later

export interface ItineraryEntry {
  id: number;
  type: string; // NEW: Activity Type (Meetings, Field Visit, etc.)
  focusArea: string;
  date: string;
  day: string;
  kraPlan: string;
  morningMeeting: string;
  timeFrom: string;
  timeTo: string;
  market: string;
  channel: string;
  routeNo: string;
  outletNo: string;
  accompaniedBy: string;
  nightOut: string;
  noOfDays: number;
  noOfDaysPercent?: number;
  plannedMileage: number;
  kraReview?: string;
  comments?: string;
}

// Activity TYPES (First level dropdown)
export const ACTIVITY_TYPES = [
  "Meetings",
  "Trainings",
  "Events",
  "Coaching/ Core Services Excellence (Core service audit/ TM Assessment)",
  "Observations / Night Visits",
  "Sales Planning/ Admin",
  "Leave",
];

// TYPE → Focus Area mapping (Second level dropdown - cascading)
export const TYPE_FOCUS_AREA_MAPPING: Record<string, string[]> = {
  "Meetings": [
    "MR App rollout & discussion",
    "F26 insights & planning",
    "Operation Meeting",
    "Pre DPM & Training",
    "SFA Market Visit - WINTT",
    "S & M Meeting",
    "CPM 01",
    "CPM 02",
    "Southern Cluster Meeting",
    "F26 1st Cut PPT",
    "Premium Meeting",
    "F26 Plan Session 01",
    "F26 Plan Session 02",
  ],
  "Trainings": [],
  "Events": [],
  "Coaching/ Core Services Excellence (Core service audit/ TM Assessment)": [
    "Boralasgamuwa",
    "Beruwala",
    "Galle",
    "Tangalle",
  ],
  "Observations / Night Visits": [
    "Boralasgamuwa",
    "Beruwala",
    "Galle",
    "Tangalle",
  ],
  "Sales Planning/ Admin": [
    "Admin",
  ],
  "Leave": [
    "Personal matter",
    "Sick leave",
    "Vacation",
  ],
};

export const MARKETS = [
  "NA",
  "Colombo",
  "Galle",
  "HO",
  "Negombo",
  "TBC",
  "Beruwala",
  "Boralasgamuwa",
  "Kaduwela",
  "Tangalle",
];

export const CHANNELS = [
  "NA",
  "All",
  "TOFT",
  "Modern Trade",
];

export const ROUTE_NUMBERS = [
  "NA",
  "All",
  "Route 01",
  "Route 02",
  "Route 03",
  "Route 04",
  "Route 05",
];

export const OUTLET_NUMBERS = [
  "NA",
  "All",
  "1",
  "2",
  "3",
  "4",
  "5",
  "6",
  "7",
  "8",
  "9",
  "10",
];

export const ACCOMPANIED_BY_OPTIONS = [
  "Sales & Marketing",
  "Sales Leadership Team",
  "Southern Cluster",
  "TM",
];

export const KRA_PLAN_OPTIONS = [
  "MR app roll out & discussion",
  "Annual planning insights",
  "Discussion of operational matters with Sales Leadership team",
  "Bottom up discussion & insights for improvements",
  "SFA market visit",
  "Cluster visit - Core Service Excellence Audit & TM Assessment",
  "Store observation night/day visit",
  "Administrative work and planning",
  "Other",
];

// Activity summary (from Excel Summary tab)
export const ACTIVITY_SUMMARY = {
  "Meetings & Trainings": 14,
  "Coaching/Core Services Excellence": 5,
  "Observations/Night Visit": 4,
  "Sales Planning/Admin": 2,
  "Leave": 1,
};

// Mock itinerary entries from Excel
export const MOCK_ITINERARY_DATA: ItineraryEntry[] = [
  {
    id: 1,
    type: "Meetings",
    focusArea: "MR App rollout & discussion",
    date: "2024-10-01",
    day: "Tue",
    kraPlan: "MR app roll out & discussion",
    morningMeeting: "NA",
    timeFrom: "9.00am",
    timeTo: "5.00pm",
    market: "Colombo",
    channel: "NA",
    routeNo: "NA",
    outletNo: "NA",
    accompaniedBy: "Sales & Marketing",
    nightOut: "Colombo",
    noOfDays: 1,
    plannedMileage: 55,
  },
  {
    id: 2,
    type: "Meetings",
    focusArea: "F26 insights & planning",
    date: "2024-10-02",
    day: "Wed",
    kraPlan: "Annual planning insights",
    morningMeeting: "NA",
    timeFrom: "9.00am",
    timeTo: "5.00pm",
    market: "Colombo",
    channel: "NA",
    routeNo: "NA",
    outletNo: "NA",
    accompaniedBy: "Sales & Marketing",
    nightOut: "Colombo",
    noOfDays: 1,
    plannedMileage: 55,
  },
  {
    id: 3,
    type: "Meetings",
    focusArea: "Operation Meeting",
    date: "2024-10-03",
    day: "Thu",
    kraPlan: "Discussion of operational matters with Sales Leadership team",
    morningMeeting: "NA",
    timeFrom: "9.00am",
    timeTo: "5.00pm",
    market: "TBC",
    channel: "NA",
    routeNo: "NA",
    outletNo: "NA",
    accompaniedBy: "Sales Leadership Team",
    nightOut: "TBC",
    noOfDays: 1,
    plannedMileage: 0,
  },
  {
    id: 4,
    type: "Meetings",
    focusArea: "Pre DPM & Training",
    date: "2024-10-07",
    day: "Mon",
    kraPlan: "Bottom up discussion & insights for improvements",
    morningMeeting: "NA",
    timeFrom: "9.00am",
    timeTo: "5.00pm",
    market: "Colombo",
    channel: "NA",
    routeNo: "NA",
    outletNo: "NA",
    accompaniedBy: "Sales Leadership Team",
    nightOut: "Colombo",
    noOfDays: 1,
    plannedMileage: 55,
  },
  {
    id: 5,
    type: "Meetings",
    focusArea: "SFA Market Visit - WINTT",
    date: "2024-10-09",
    day: "Wed",
    kraPlan: "SFA market visit",
    morningMeeting: "NA",
    timeFrom: "9.00am",
    timeTo: "5.00pm",
    market: "Colombo",
    channel: "NA",
    routeNo: "NA",
    outletNo: "NA",
    accompaniedBy: "Sales Leadership Team",
    nightOut: "Colombo",
    noOfDays: 1,
    plannedMileage: 55,
  },
  {
    id: 6,
    type: "Coaching/ Core Services Excellence (Core service audit/ TM Assessment)",
    focusArea: "Galle",
    date: "2024-10-10",
    day: "Thu",
    kraPlan: "Cluster visit - Core Service Excellence Audit & TM Assessment",
    morningMeeting: "NA",
    timeFrom: "7.00am",
    timeTo: "7.00pm",
    market: "Galle",
    channel: "All",
    routeNo: "All",
    outletNo: "All",
    accompaniedBy: "Southern Cluster",
    nightOut: "Galle",
    noOfDays: 1,
    plannedMileage: 280,
  },
  {
    id: 7,
    type: "Coaching/ Core Services Excellence (Core service audit/ TM Assessment)",
    focusArea: "Beruwala",
    date: "2024-10-11",
    day: "Fri",
    kraPlan: "Cluster visit - Core Service Excellence Audit & TM Assessment",
    morningMeeting: "NA",
    timeFrom: "7.00am",
    timeTo: "7.00pm",
    market: "Beruwala",
    channel: "All",
    routeNo: "All",
    outletNo: "All",
    accompaniedBy: "Southern Cluster",
    nightOut: "Beruwala",
    noOfDays: 1,
    plannedMileage: 180,
  },
  {
    id: 8,
    type: "Coaching/ Core Services Excellence (Core service audit/ TM Assessment)",
    focusArea: "Boralasgamuwa",
    date: "2024-10-14",
    day: "Mon",
    kraPlan: "Cluster visit - Core Service Excellence Audit & TM Assessment",
    morningMeeting: "NA",
    timeFrom: "7.00am",
    timeTo: "7.00pm",
    market: "Negombo",
    channel: "All",
    routeNo: "All",
    outletNo: "All",
    accompaniedBy: "TM",
    nightOut: "Negombo",
    noOfDays: 1,
    plannedMileage: 80,
  },
  {
    id: 9,
    type: "Observations / Night Visits",
    focusArea: "Boralasgamuwa",
    date: "2024-10-15",
    day: "Tue",
    kraPlan: "Store observation night/day visit",
    morningMeeting: "NA",
    timeFrom: "7.00am",
    timeTo: "7.00pm",
    market: "Boralasgamuwa",
    channel: "All",
    routeNo: "All",
    outletNo: "All",
    accompaniedBy: "TM",
    nightOut: "Boralasgamuwa",
    noOfDays: 1,
    plannedMileage: 60,
  },
  {
    id: 10,
    type: "Sales Planning/ Admin",
    focusArea: "Admin",
    date: "2024-10-16",
    day: "Wed",
    kraPlan: "Administrative work and planning",
    morningMeeting: "NA",
    timeFrom: "9.00am",
    timeTo: "5.00pm",
    market: "Colombo",
    channel: "NA",
    routeNo: "NA",
    outletNo: "NA",
    accompaniedBy: "TM",
    nightOut: "Colombo",
    noOfDays: 1,
    plannedMileage: 55,
  },
];
