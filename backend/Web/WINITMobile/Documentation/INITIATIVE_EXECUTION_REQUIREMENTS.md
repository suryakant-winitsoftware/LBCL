# Initiative Execution Module - Requirements & Implementation Guide

## Table of Contents
1. [Overview](#overview)
2. [Current State Analysis](#current-state-analysis)
3. [Business Requirements](#business-requirements)
4. [Technical Architecture](#technical-architecture)
5. [UI/UX Design](#uiux-design)
6. [Database Schema](#database-schema)
7. [Implementation Roadmap](#implementation-roadmap)

---

## Overview

### Definition
**Initiative Execution** in mSFA context refers to the systematic tracking and implementation of trade marketing campaigns, promotional activities, and strategic brand initiatives at the point of sale. It ensures field representatives properly execute planned marketing initiatives and captures evidence of execution for compliance tracking.

### Business Value
- **Execution Rate Tracking**: Measure % of planned initiatives completed
- **Compliance Monitoring**: Ensure quality of execution meets standards
- **ROI Measurement**: Link initiative execution to sales uplift
- **Competitive Intelligence**: Capture competitor activities during store visits
- **Real-time Visibility**: Management dashboard for execution tracking

---

## Current State Analysis

### Existing Implementation
- **Module Name**: Broadcast Initiative
- **Location**: `D:\Multiplex\backend\Web\WINITMobile\Pages\BroadcastInitiative.razor`
- **Route**: `/broadcast-initiative`
- **Status**: Implemented as standalone module, NOT integrated as Store Activity

### Current Fields Captured
```csharp
public class BroadcastInitiative : BaseModel
{
    public string Gender { get; set; }
    public string EndCustomerName { get; set; }
    public string MobileNo { get; set; }
    public string FtbRc { get; set; }  // First Time Buyer/Repeat Customer
    public string BeatHistoryUID { get; set; }
    public string RouteUID { get; set; }
    public string JobPositionUID { get; set; }
    public string EmpUID { get; set; }
    public DateTime ExecutionTime { get; set; }
    public string StoreUID { get; set; }
    public string SKUUID { get; set; }
}
```

### Current Limitations
1. Not integrated with Store Activity framework
2. Limited execution tracking fields
3. No campaign/initiative master linkage
4. Missing compliance scoring
5. No competitor activity tracking
6. Limited validation and quality metrics

---

## Business Requirements

### Functional Requirements

#### 1. Core Execution Tracking
- **Campaign Assignment**: Link execution to planned campaigns/initiatives
- **Execution Checklist**: Step-by-step tasks for field reps
- **Quality Metrics**: Visibility rating, share of shelf, placement quality
- **Evidence Capture**: Before/after photos with GPS and timestamp
- **Status Tracking**: Full/Partial/Not Executed with reasons

#### 2. Data Collection Types

| Question Type | Purpose | Example |
|--------------|---------|---------|
| Yes/No | Binary confirmation | "Is display setup complete?" |
| Single Choice | One from many | "Display Location: Entrance/Aisle/Counter" |
| Multiple Choice | Multiple selections | "POSM Materials: Poster/Wobbler/Standee" |
| Rating Scale | Quality measurement | "Visibility: 1-5 stars" |
| Numeric Input | Quantity capture | "Stock Units: 25" |
| Percentage | Proportion measurement | "Share of Shelf: 35%" |
| Text Input | Open feedback | "Customer comments" |
| Photo Capture | Visual evidence | "Display photo" |
| Dropdown | Predefined selection | "Competitor brand" |
| Conditional | Dependent questions | "If Yes, specify which..." |

#### 3. Execution Workflow
```
1. Pre-Visit
   └─> View assigned initiatives
   └─> Review execution guidelines
   └─> Check previous execution history

2. In-Store Execution
   └─> Select initiative/campaign
   └─> Complete execution checklist
   └─> Capture quality metrics
   └─> Document competitor activity
   └─> Record customer engagement
   └─> Take photo evidence

3. Post-Visit
   └─> Submit execution report
   └─> Sync data to server
   └─> Update compliance score
```

#### 4. Key Metrics to Track
- **Execution Rate**: % of assigned initiatives completed
- **Compliance Score**: Quality rating (0-100%)
- **Time to Execute**: Days from assignment to completion
- **Customer Reach**: Number of customers engaged
- **Photo Evidence**: % with proper documentation
- **Competitor Presence**: % stores with competitor activity

---

## Technical Architecture

### 1. Enhanced Data Model

```csharp
public class InitiativeExecution : BaseModel
{
    // Campaign Linkage
    public string InitiativeUID { get; set; }
    public string CampaignUID { get; set; }
    public string InitiativeType { get; set; } // Launch/Promotion/Sampling
    
    // Store Information
    public string StoreUID { get; set; }
    public string StoreHistoryUID { get; set; }
    public string BeatHistoryUID { get; set; }
    
    // Execution Details
    public DateTime ExecutionDate { get; set; }
    public string ExecutionStatus { get; set; } // Full/Partial/NotExecuted
    public string NonComplianceReason { get; set; }
    
    // Display Setup
    public bool DisplaySetup { get; set; }
    public string DisplayLocation { get; set; } // Entrance/Aisle/Counter/Endcap
    public int VisibilityScore { get; set; } // 1-5
    public bool IsEyeLevel { get; set; }
    public decimal ShareOfShelf { get; set; }
    
    // Stock Information
    public bool StockAvailable { get; set; }
    public decimal StockQuantity { get; set; }
    public string StockUnit { get; set; }
    
    // Pricing
    public bool PriceTagPlaced { get; set; }
    public decimal DisplayPrice { get; set; }
    public bool PriceCompliance { get; set; }
    
    // POSM Materials
    public bool POSMPlaced { get; set; }
    public string POSMMaterials { get; set; } // JSON array of materials used
    
    // Competitor Activity
    public bool CompetitorDisplayPresent { get; set; }
    public string CompetitorBrand { get; set; }
    public string CompetitorActivity { get; set; }
    
    // Customer Engagement
    public int SamplesDistributed { get; set; }
    public int NewCustomersEngaged { get; set; }
    public int ExistingCustomersEngaged { get; set; }
    public string CustomerFeedback { get; set; } // JSON array
    
    // Compliance
    public decimal ComplianceScore { get; set; }
    public string Notes { get; set; }
    
    // Tracking
    public string EmpUID { get; set; }
    public string JobPositionUID { get; set; }
    public string Latitude { get; set; }
    public string Longitude { get; set; }
}
```

### 2. Question Configuration Model

```csharp
public class InitiativeQuestion
{
    public string QuestionId { get; set; }
    public string SectionId { get; set; }
    public string QuestionText { get; set; }
    public QuestionType Type { get; set; }
    public bool IsRequired { get; set; }
    public int DisplayOrder { get; set; }
    public string ValidationRule { get; set; }
    public string[] Options { get; set; }
    public object DefaultValue { get; set; }
    public int? MinValue { get; set; }
    public int? MaxValue { get; set; }
    public string DependsOnQuestion { get; set; }
    public string DependsOnValue { get; set; }
    public int Weight { get; set; } // For compliance scoring
}

public enum QuestionType
{
    YesNo,
    SingleChoice,
    MultipleChoice,
    Rating,
    Numeric,
    Percentage,
    Text,
    Photo,
    Date,
    Time,
    GPS,
    Signature
}

public class InitiativeSection
{
    public string SectionId { get; set; }
    public string SectionName { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsRequired { get; set; }
    public List<InitiativeQuestion> Questions { get; set; }
}
```

### 3. Database Schema

```sql
-- Initiative Master
CREATE TABLE initiative_master (
    uid VARCHAR(50) PRIMARY KEY,
    code VARCHAR(50),
    name VARCHAR(200),
    initiative_type VARCHAR(50), -- Launch/Promotion/Sampling
    campaign_uid VARCHAR(50),
    valid_from DATE,
    valid_to DATE,
    priority VARCHAR(20),
    target_stores TEXT, -- JSON array of store UIDs
    is_active BOOLEAN,
    created_by VARCHAR(50),
    created_time TIMESTAMP,
    modified_by VARCHAR(50),
    modified_time TIMESTAMP
);

-- Initiative Questions Configuration
CREATE TABLE initiative_questions (
    uid VARCHAR(50) PRIMARY KEY,
    initiative_uid VARCHAR(50),
    section_id VARCHAR(50),
    section_name VARCHAR(100),
    question_id VARCHAR(50),
    question_text TEXT,
    question_type VARCHAR(30),
    is_required BOOLEAN,
    display_order INT,
    options TEXT, -- JSON array
    validation_rule TEXT,
    min_value INT,
    max_value INT,
    depends_on_question VARCHAR(50),
    depends_on_value VARCHAR(100),
    weight INT,
    is_active BOOLEAN
);

-- Initiative Execution Header
CREATE TABLE initiative_execution (
    uid VARCHAR(50) PRIMARY KEY,
    initiative_uid VARCHAR(50),
    campaign_uid VARCHAR(50),
    store_uid VARCHAR(50),
    store_history_uid VARCHAR(50),
    beat_history_uid VARCHAR(50),
    execution_date TIMESTAMP,
    execution_status VARCHAR(20),
    non_compliance_reason VARCHAR(200),
    compliance_score DECIMAL(5,2),
    emp_uid VARCHAR(50),
    job_position_uid VARCHAR(50),
    latitude VARCHAR(50),
    longitude VARCHAR(50),
    notes TEXT,
    ss INT,
    created_time TIMESTAMP,
    modified_time TIMESTAMP,
    server_add_time TIMESTAMP,
    server_modified_time TIMESTAMP
);

-- Initiative Execution Responses
CREATE TABLE initiative_execution_responses (
    uid VARCHAR(50) PRIMARY KEY,
    execution_uid VARCHAR(50),
    question_id VARCHAR(50),
    response_value TEXT,
    response_type VARCHAR(30),
    created_time TIMESTAMP
);

-- Initiative Photos
CREATE TABLE initiative_photos (
    uid VARCHAR(50) PRIMARY KEY,
    execution_uid VARCHAR(50),
    photo_type VARCHAR(50), -- Before/After/Shelf/Competitor
    file_path TEXT,
    latitude VARCHAR(50),
    longitude VARCHAR(50),
    captured_time TIMESTAMP,
    created_time TIMESTAMP
);
```

---

## UI/UX Design

### Screen Flow Mockup

```
┌─────────────────────────────────────────┐
│ ← Back    Initiative Execution    ⋮     │
│─────────────────────────────────────────│
│                                         │
│ [Campaign Header Card]                  │
│ ┌─────────────────────────────────────┐ │
│ │ 🎯 Summer Launch Campaign            │ │
│ │ Product: Coca Cola 500ml             │ │
│ │ Valid: Jan 15 - Feb 15, 2025        │ │
│ └─────────────────────────────────────┘ │
│                                         │
│ [Section 1: Execution Checklist]        │
│ ┌─────────────────────────────────────┐ │
│ │ ☐ Display Setup                     │ │
│ │ ☐ Stock Available                   │ │
│ │ ☐ Price Tag Placed                  │ │
│ │ ☐ POSM Materials                    │ │
│ └─────────────────────────────────────┘ │
│                                         │
│ [Section 2: Visibility & Placement]     │
│ ┌─────────────────────────────────────┐ │
│ │ Visibility Rating: ⭐⭐⭐⭐☆        │ │
│ │ Eye Level: Yes/No                   │ │
│ │ Share of Shelf: [====] 35%          │ │
│ └─────────────────────────────────────┘ │
│                                         │
│ [Section 3: Competition]                │
│ ┌─────────────────────────────────────┐ │
│ │ Competitor Present? Yes/No          │ │
│ │ If Yes: [Select Brand ▼]            │ │
│ │ [📷 Capture Photo]                  │ │
│ └─────────────────────────────────────┘ │
│                                         │
│ [Section 4: Customer Engagement]        │
│ ┌─────────────────────────────────────┐ │
│ │ Samples Given: [___]                │ │
│ │ Customer Type: New/Existing         │ │
│ │ [+ Add Feedback]                    │ │
│ └─────────────────────────────────────┘ │
│                                         │
│ [Section 5: Photo Evidence]             │
│ ┌─────────────────────────────────────┐ │
│ │ [📷Before] [📷After] [📷Shelf] [+] │ │
│ └─────────────────────────────────────┘ │
│                                         │
│ [Section 6: Status]                     │
│ ┌─────────────────────────────────────┐ │
│ │ ○ Fully Executed                    │ │
│ │ ○ Partially Executed                │ │
│ │ ○ Not Executed                      │ │
│ └─────────────────────────────────────┘ │
│                                         │
│ [Submit Button]                         │
└─────────────────────────────────────────┘
```

### Component Structure

```razor
@page "/initiative-execution/{InitiativeUID}"
@inherits WINITMobile.Pages.Base.BaseComponentBase

<InitiativeExecutionForm>
    <CampaignHeader Initiative="@SelectedInitiative" />
    
    @foreach(var section in InitiativeSections)
    {
        <FormSection Title="@section.SectionName">
            @foreach(var question in section.Questions)
            {
                <DynamicQuestion 
                    Question="@question" 
                    OnValueChanged="@HandleQuestionResponse" />
            }
        </FormSection>
    }
    
    <PhotoEvidenceSection 
        RequiredPhotos="@RequiredPhotoTypes"
        OnPhotosCaptured="@HandlePhotosCapture" />
    
    <ExecutionStatusSection 
        OnStatusSelected="@HandleStatusSelection" />
    
    <SubmitButton 
        IsValid="@IsFormValid()" 
        OnSubmit="@HandleSubmit" />
</InitiativeExecutionForm>
```

---

## Implementation Roadmap

### Phase 1: Foundation (Week 1-2)
- [ ] Create database tables for initiative execution
- [ ] Implement enhanced data models
- [ ] Create base UI components for question types
- [ ] Set up navigation and routing

### Phase 2: Core Features (Week 3-4)
- [ ] Build dynamic form generation system
- [ ] Implement question configuration
- [ ] Add photo capture with GPS/timestamp
- [ ] Create validation framework
- [ ] Implement conditional questions logic

### Phase 3: Integration (Week 5-6)
- [ ] Integrate with Store Activity framework
- [ ] Link to campaign/promotion modules
- [ ] Add offline storage capability
- [ ] Implement data sync mechanism
- [ ] Create compliance scoring algorithm

### Phase 4: Advanced Features (Week 7-8)
- [ ] Add competitor tracking module
- [ ] Implement customer feedback collection
- [ ] Create management dashboard
- [ ] Add reporting and analytics
- [ ] Implement push notifications for new initiatives

### Phase 5: Testing & Optimization (Week 9-10)
- [ ] Unit testing for all components
- [ ] Integration testing
- [ ] Performance optimization
- [ ] User acceptance testing
- [ ] Bug fixes and refinements

---

## Key Integration Points

### 1. Store Activity Integration
```csharp
// Add to store_activity table
INSERT INTO store_activity (uid, code, name, icon_path, nav_path, serial_no, is_active)
VALUES ('INIT_EXEC', 'INIT_EXEC', 'Initiative Execution', 
        '/Images/initiative_icon.png', '/initiative-execution', 8, 1);

// Add role mapping
INSERT INTO store_activity_role_mapping (uid, store_activity_uid, role_uid)
VALUES ('INIT_EXEC_ROLE', 'INIT_EXEC', 'FIELD_REP_ROLE');
```

### 2. Sales Order Linkage
- Track if initiative execution led to sales
- Link execution UID to sales orders created in same visit

### 3. Promotion Module Integration
- Pull active promotions for the store
- Link execution to promotion performance

### 4. Reporting Integration
- Real-time dashboard for management
- Compliance reports by region/team/individual
- ROI analysis linking execution to sales uplift

---

## Success Metrics

| Metric | Target | Measurement |
|--------|--------|-------------|
| Adoption Rate | >90% | % of field reps using the module |
| Data Quality | >95% | % of complete submissions |
| Photo Compliance | >85% | % with all required photos |
| Sync Success | >98% | % of successful data syncs |
| User Satisfaction | >4.0/5 | User feedback score |
| Time to Complete | <5 min | Average form completion time |

---

## Technical Considerations

### Performance
- Implement lazy loading for sections
- Cache question configurations locally
- Compress photos before upload
- Batch sync operations

### Offline Capability
- Store responses in SQLite
- Queue photos for upload
- Sync when connection available
- Handle conflict resolution

### Security
- Validate all inputs
- Sanitize text responses
- Encrypt sensitive data
- Implement role-based access

### Scalability
- Design for 10,000+ daily executions
- Optimize database queries
- Implement pagination for lists
- Use CDN for static assets

---

## Appendix

### A. Sample Question Configuration
```json
{
  "sections": [
    {
      "sectionId": "EXEC_CHECK",
      "sectionName": "Execution Checklist",
      "questions": [
        {
          "questionId": "Q001",
          "questionText": "Is the display setup complete?",
          "type": "YesNo",
          "isRequired": true,
          "weight": 20
        },
        {
          "questionId": "Q002",
          "questionText": "Display Location",
          "type": "SingleChoice",
          "options": ["Entrance", "Main Aisle", "End Cap", "Counter"],
          "dependsOn": "Q001",
          "dependsOnValue": "Yes",
          "weight": 10
        }
      ]
    }
  ]
}
```

### B. Compliance Score Calculation
```csharp
public decimal CalculateComplianceScore(InitiativeExecution execution)
{
    decimal totalWeight = 0;
    decimal achievedWeight = 0;
    
    // Check each weighted question
    foreach(var response in execution.Responses)
    {
        var question = GetQuestion(response.QuestionId);
        totalWeight += question.Weight;
        
        if(IsResponseCompliant(response, question))
        {
            achievedWeight += question.Weight;
        }
    }
    
    // Photo evidence (20% weight)
    if(execution.HasRequiredPhotos)
        achievedWeight += 20;
    totalWeight += 20;
    
    // Calculate percentage
    return (achievedWeight / totalWeight) * 100;
}
```

### C. References
- Industry best practices for trade marketing execution
- mSFA implementation guidelines
- FMCG retail execution standards
- Mobile form optimization patterns

---

## Document Control

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2025-01-20 | System Analysis | Initial documentation |

---

## Next Steps

1. Review and approve requirements with stakeholders
2. Finalize technical architecture
3. Create detailed UI mockups
4. Begin Phase 1 implementation
5. Set up project tracking and milestones

---

**END OF DOCUMENT**