# WINIT Mobile Application - UI Design Specification

## Application Overview

**WINIT Mobile** is a comprehensive field force automation application built using .NET MAUI with Blazor components. The application serves field sales representatives, merchandisers, and distributors with tools for sales order management, customer relationship management, inventory tracking, and field operations.

### Technical Stack
- **Platform**: .NET MAUI cross-platform mobile app (Android, iOS, Windows, macOS)
- **UI Framework**: Blazor components with C# backend
- **Database**: SQLite for offline storage
- **Authentication**: Custom authentication with secure storage

---

## Core Screen Categories

### 1. Authentication & Setup

#### Login Screen (`/` - Index.razor)
**Purpose**: User authentication and app initialization

**Components Required**:
- **Logo Section**: App logo with branding
- **Login Form**:
  - Mobile number input field (numeric, max 64 chars)
  - Password field with show/hide toggle
  - Remember me checkbox
  - Login button with loading state
- **Secondary Actions**:
  - Forgot password link (opens modal)
  - Language selection button (English/Arabic)
- **Footer**: 
  - "Powered by" branding
  - Version info display (version, release date, build type)

**Design Notes**:
- Clean, minimal design with focus on ease of entry
- Loading states for login process
- Offline connectivity indicator
- Support for RTL languages (Arabic)

---

### 2. Dashboard & Navigation

#### Main Dashboard (`/DashBoard`)
**Purpose**: Central hub showing performance metrics and quick access to key functions

**Layout Sections**:

**Header Area**:
- Coverage statistics (Total Assigned Stores, Total Covered Stores)
- Date and route information

**Tasks Menu**:
- Grid layout of task cards with icons:
  - **Check-in/Check-out**: Attendance management
  - **My Store(s)**: Customer list access
  - **Brand Training**: Product training modules
  - **Customer Interaction**: Feedback and broadcast tools
  - **Product Sampling**: Sample distribution tracking
  - **Escalation Matrix**: Support contacts
  - **ROTA**: Schedule calendar

**Performance Widgets** (Optional - currently hidden):
- Sales performance charts
- Call coverage metrics
- Attendance summary
- Leaderboard displays

**Footer**:
- Start Day/Continue button (prominent CTA)

**Design Requirements**:
- Card-based layout for tasks
- Consistent iconography
- Easy thumb navigation
- Quick access to frequently used functions

---

### 3. Customer Management

#### Journey Plan (`/JourneyPlan`)
**Purpose**: Daily/weekly route planning and customer visit management

**Components**:
- **Date Selector**: 
  - Horizontal scrollable date picker
  - Shows day of week and date number
- **Route Header**:
  - Route name and selected date
  - Toggle between map and list view
- **Customer List/Map Toggle**:
  - List view: Customer cards with visit status
  - Map view: Geographic customer locations
- **Filtering Tabs**: Planned vs Unplanned customers

**Popups/Modals**:
- Unplanned customers selection
- Store organization details
- Store credit information

#### Customer Details (Multiple Forms)
**Purpose**: Comprehensive customer information management

**Form Categories**:
- **Basic Information**: Name, contact details, addresses
- **Business Details**: Business type, employee count, operational info
- **Financial Information**: Banking details, credit limits
- **Address Management**: Bill-to and Ship-to addresses
- **Documentation**: Required business documents and agreements

**Design Pattern**:
- Multi-step form wizard
- Progress indicators
- Field validation with clear error messaging
- Photo upload capabilities for documents

---

### 4. Sales Operations

#### Sales Order Creation (`/createsalesorder`)
**Purpose**: Product selection and order creation interface

**Key Components**:

**Search & Filter Bar**:
- Debounced search input (500ms delay)
- Barcode scanner integration
- Grid/List view toggle button
- Advanced filter options

**Product Display**:
- **List View**: Detailed product rows with pricing, stock
- **Grid View**: Card-based product tiles with images
- Product categories with filtering
- Real-time search results

**Order Management**:
- Quantity selectors with validation
- UOM (Unit of Measure) dropdowns
- Promotion handling and display
- Price override capabilities (with passcode protection)
- Clone product functionality

**Footer Section**:
- Running totals (Line Count, Quantity, Net Amount)
- Tax calculations
- Available credit limit display
- Place order button

**Modals/Popups**:
- Product promotion details
- Price edit confirmation
- Order placement confirmation
- Discount application

---

### 5. Collection & Payments

#### Payment Collection (`/collectpayment`, `/Payment`)
**Purpose**: Customer payment processing and collection management

**Components**:
- **Customer Selection**: Search and select customer
- **Payment Details**:
  - Amount input with currency display
  - Payment mode selection (Cash, Check, Digital)
  - Reference number entry
  - Date selection
- **Multi-Currency Support**: Currency conversion interface
- **Payment Summary**: Transaction details and confirmations

#### Cash Collection Management (`/maintaincashcollection`)
- Deposit request creation
- Collection tracking
- Bank deposit management

---

### 6. Inventory & Stock Management

#### Stock Operations
**Components Needed**:

**Van Stock View** (`/ViewVanStock`):
- Current inventory display
- Stock levels with low-stock alerts
- Product search and filtering
- Transfer capabilities

**Stock Audit** (`/stockaudit`):
- Physical count entry forms
- Variance reporting
- Photo documentation
- Saleable vs non-saleable stock

**Warehouse Stock** (`/MaintainWareHouseStock`):
- Warehouse inventory levels
- Stock requests and transfers
- Load/Unload request management

---

### 7. Field Activities

#### Store Check (`/storecheck`)
**Purpose**: Store compliance and audit functionality

**Features**:
- Checklist-based audit forms
- Photo capture for compliance issues
- Share of shelf measurements
- Competitor analysis integration

#### Planogram (`/planogram`)
**Purpose**: Visual merchandising compliance

**Components**:
- Product placement verification
- Photo comparison tools
- Compliance scoring
- Corrective action logging

#### Competitor Capture (`/capturecompetitor`)
**Purpose**: Market intelligence gathering

**Features**:
- Competitor product information
- Pricing data entry
- Photo documentation
- Market share analysis

---

### 8. Administrative Functions

#### Reports & Analytics
- **Dashboard Reports**: Performance metrics and KPIs
- **Charts Integration**: ECharts component for data visualization
- **Export Capabilities**: PDF and email sharing

#### Service Management
- **Call Registration**: Customer service request logging
- **Service Status**: Tracking and follow-up management
- **Escalation Matrix**: Contact information and procedures

---

## UI Design Requirements

### Visual Design System

**Color Palette**:
- Primary Blue: `#1B478A`
- Secondary Orange: `#FF9728` 
- Success Green: `#3AC3A9`
- Warning Red: `#E74F48`
- Neutral Grays: Various shades for backgrounds and text

**Typography**:
- Primary Font: OpenSans-Regular.ttf
- Hierarchy: H1-H6 headings with consistent sizing
- Body text: 14-16px for mobile readability

**Iconography**:
- Custom business-specific icon set
- Consistent sizing (16px, 24px, 32px)
- SVG format for scalability

### Layout Patterns

**Grid System**:
- 12-column responsive grid
- Consistent spacing (8px, 16px, 24px)
- Mobile-first approach

**Navigation**:
- **Bottom Tab Bar**: Primary navigation
- **Top Action Bar**: Context-sensitive actions
- **Hamburger Menu**: Secondary navigation and settings
- **Modal Overlays**: Forms and detailed views

### Component Library

**Form Controls**:
- **WinitTextBox**: Debounced text input with validation
- **Dropdown Selectors**: Single and multi-select
- **Date/Time Pickers**: Calendar integration
- **Toggle Switches**: Boolean controls
- **Sliders**: Range inputs

**Data Display**:
- **Data Tables**: Sortable and filterable
- **Card Components**: Information display
- **Progress Indicators**: Loading and completion states
- **Charts**: Integration with ECharts library

**Interactive Elements**:
- **Buttons**: Primary, secondary, and text variants
- **Floating Action Buttons**: Quick actions
- **Swipe Gestures**: List item actions
- **Pull-to-Refresh**: Data synchronization

### Mobile-Specific Features

**Touch Interactions**:
- Minimum 44px touch targets
- Swipe gestures for navigation
- Long-press for context menus
- Haptic feedback for confirmations

**Camera Integration**:
- Photo capture for documentation
- Barcode/QR code scanning
- Image compression and storage
- Gallery access for existing photos

**Offline Capabilities**:
- Local SQLite database storage
- Sync indicators and conflict resolution
- Offline form completion
- Data queue management

### Accessibility Requirements

**WCAG Compliance**:
- Minimum 4.5:1 color contrast ratios
- Screen reader compatibility
- Keyboard navigation support
- Text scaling support

**Internationalization**:
- RTL language support (Arabic)
- Dynamic text translation
- Cultural date/number formatting
- Icon universality

### Performance Considerations

**Optimization**:
- Lazy loading for large lists
- Image optimization and caching
- Debounced search inputs
- Virtual scrolling for large datasets

**Network Handling**:
- Retry mechanisms for failed requests
- Background sync capabilities
- Bandwidth-conscious media loading
- Connection status indicators

---

## Technical Implementation Notes

### State Management
- Component-level state for UI interactions
- Service layer for business logic
- Local storage for offline data persistence

### Security
- Passcode protection for sensitive operations
- Secure storage for authentication tokens
- Data encryption for sensitive information
- Session timeout handling

### Testing Requirements
- Unit tests for business logic
- UI automation tests for critical paths
- Device testing across platforms
- Performance testing under various network conditions

---

## Screen Priority Matrix

### Phase 1 (Core Functionality)
1. Login/Authentication
2. Dashboard
3. Journey Plan
4. Sales Order Creation
5. Customer List/Details

### Phase 2 (Extended Features)
1. Payment Collection
2. Stock Management
3. Reports & Analytics

### Phase 3 (Advanced Features)
1. Store Check/Planogram
2. Service Management
3. Advanced Analytics

---

This specification provides the foundation for creating a comprehensive, user-friendly mobile application that meets the needs of field sales teams while maintaining high standards for usability, performance, and accessibility.