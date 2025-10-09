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

// Mock itinerary entries from Excel (FSM_Itinerary Template2.xlsx)
export const MOCK_ITINERARY_DATA: ItineraryEntry[] = [
  {
    id: 1,
    type: "Coaching/ Core Services Excellence (Core service audit/ TM Assessment)",
    focusArea: "Boralasgamuwa",
    date: "2024-10-04",
    day: "Fri",
    kraPlan: "DCSLB conditional selling & operational issues",
    morningMeeting: "Yes",
    timeFrom: "8:00 AM",
    timeTo: "5:00 PM",
    market: "Boralasgamuwa",
    channel: "All",
    routeNo: "NA",
    outletNo: "10",
    accompaniedBy: "TM",
    nightOut: "Kaduwela",
    noOfDays: 1,
    plannedMileage: 55,
  },
  {
    id: 2,
    type: "Coaching/ Core Services Excellence (Core service audit/ TM Assessment)",
    focusArea: "Beruwala",
    date: "2024-10-08",
    day: "Tue",
    kraPlan: "Core service excellence & operational issues",
    morningMeeting: "Yes",
    timeFrom: "8:00 AM",
    timeTo: "5:00 PM",
    market: "Beruwala",
    channel: "All",
    routeNo: "NA",
    outletNo: "10",
    accompaniedBy: "TM",
    nightOut: "Kaduwela",
    noOfDays: 1,
    plannedMileage: 130,
  },
  {
    id: 3,
    type: "Coaching/ Core Services Excellence (Core service audit/ TM Assessment)",
    focusArea: "Galle",
    date: "2024-10-21",
    day: "Mon",
    kraPlan: "Core service & DCSLB penetration issues",
    morningMeeting: "Yes",
    timeFrom: "8:00 AM",
    timeTo: "5:00 PM",
    market: "Galle",
    channel: "All",
    routeNo: "NA",
    outletNo: "10",
    accompaniedBy: "TM",
    nightOut: "Galle",
    noOfDays: 1,
    plannedMileage: 180,
  },
  {
    id: 4,
    type: "Coaching/ Core Services Excellence (Core service audit/ TM Assessment)",
    focusArea: "Tangalle",
    date: "2024-10-30",
    day: "Wed",
    kraPlan: "Core service excellence & DCSL approaches",
    morningMeeting: "Yes",
    timeFrom: "8:00 AM",
    timeTo: "5:00 PM",
    market: "Tangalle",
    channel: "All",
    routeNo: "NA",
    outletNo: "10",
    accompaniedBy: "TM",
    nightOut: "Tangalle",
    noOfDays: 1,
    plannedMileage: 270,
  },
  {
    id: 5,
    type: "Coaching/ Core Services Excellence (Core service audit/ TM Assessment)",
    focusArea: "Tangalle",
    date: "2024-10-31",
    day: "Thu",
    kraPlan: "Core service excellence & DCSL approaches",
    morningMeeting: "Yes",
    timeFrom: "8:00 AM",
    timeTo: "5:00 PM",
    market: "Tangalle",
    channel: "All",
    routeNo: "NA",
    outletNo: "10",
    accompaniedBy: "TM",
    nightOut: "Tangalle",
    noOfDays: 1,
    plannedMileage: 270,
  },
  {
    id: 6,
    type: "Observations / Night Visits",
    focusArea: "Boralasgamuwa",
    date: "2024-10-03",
    day: "Thu",
    kraPlan: "MOFT LI visibility execusion",
    morningMeeting: "NA",
    timeFrom: "6:00 PM",
    timeTo: "10:00 PM",
    market: "Boralasgamuwa",
    channel: "NA",
    routeNo: "NA",
    outletNo: "NA",
    accompaniedBy: "TM",
    nightOut: "Boralasgamuwa",
    noOfDays: 1,
    plannedMileage: 35,
  },
  {
    id: 7,
    type: "Observations / Night Visits",
    focusArea: "Beruwala",
    date: "2024-10-08",
    day: "Tue",
    kraPlan: "MOFT execusion & consumer behaviour",
    morningMeeting: "NA",
    timeFrom: "6:00 PM",
    timeTo: "10:00 PM",
    market: "Beruwala",
    channel: "NA",
    routeNo: "NA",
    outletNo: "NA",
    accompaniedBy: "TM",
    nightOut: "Beruwala",
    noOfDays: 1,
    plannedMileage: 15,
  },
  {
    id: 8,
    type: "Observations / Night Visits",
    focusArea: "Galle",
    date: "2024-10-21",
    day: "Mon",
    kraPlan: "Observe TONT footfalls & consumer behaviour",
    morningMeeting: "NA",
    timeFrom: "6:00 PM",
    timeTo: "10:00 PM",
    market: "Galle",
    channel: "NA",
    routeNo: "NA",
    outletNo: "NA",
    accompaniedBy: "TM",
    nightOut: "Galle",
    noOfDays: 1,
    plannedMileage: 15,
  },
  {
    id: 9,
    type: "Observations / Night Visits",
    focusArea: "Tangalle",
    date: "2024-09-30",
    day: "Mon",
    kraPlan: "Observe regular channel offtake trends",
    morningMeeting: "NA",
    timeFrom: "6:00 PM",
    timeTo: "10:00 PM",
    market: "Tangalle",
    channel: "NA",
    routeNo: "NA",
    outletNo: "NA",
    accompaniedBy: "TM",
    nightOut: "Tangalle",
    noOfDays: 1,
    plannedMileage: 25,
  },
  {
    id: 10,
    type: "Sales Planning/ Admin",
    focusArea: "Admin",
    date: "2024-10-14",
    day: "Mon",
    kraPlan: "Complete for admin works & prepare for OPS meeting",
    morningMeeting: "Yes",
    timeFrom: "8:00 AM",
    timeTo: "5:00 PM",
    market: "Negombo",
    channel: "NA",
    routeNo: "NA",
    outletNo: "NA",
    accompaniedBy: "TM",
    nightOut: "Admin",
    noOfDays: 1,
    plannedMileage: 60,
  },
  {
    id: 11,
    type: "Sales Planning/ Admin",
    focusArea: "Admin",
    date: "2024-10-22",
    day: "Tue",
    kraPlan: "Preparation for cluster meeting",
    morningMeeting: "Yes",
    timeFrom: "8:00 AM",
    timeTo: "5:00 PM",
    market: "Galle",
    channel: "NA",
    routeNo: "NA",
    outletNo: "NA",
    accompaniedBy: "TM",
    nightOut: "Admin",
    noOfDays: 1,
    plannedMileage: 50,
  },
  {
    id: 12,
    type: "Sales Planning/ Admin",
    focusArea: "Personal matter",
    date: "2024-10-18",
    day: "Fri",
    kraPlan: "Personal matter",
    morningMeeting: "NA",
    timeFrom: "9:00 AM",
    timeTo: "5:00 PM",
    market: "Colombo",
    channel: "NA",
    routeNo: "NA",
    outletNo: "NA",
    accompaniedBy: "TM",
    nightOut: "Personal matter",
    noOfDays: 1,
    plannedMileage: 0,
  },
];
