# Approval Navigation Structure

## Complete Menu Hierarchy

```
┌─────────────────────────────────────────────────────────────────────────┐
│                          SYSTEM NAVIGATION                              │
└─────────────────────────────────────────────────────────────────────────┘

📁 Administration (SystemAdministration)
│
├─ 📂 Hierarchy Management
├─ 📂 Reference Data
├─ 📂 Franchisee Management
├─ 📂 Tax Configuration
├─ 📂 Error Management
├─ 📂 Distributor Management
├─ 📂 Organization Management
├─ 📂 Employee Management
├─ 📂 Role & Permission Management
├─ 📂 System Configuration
├─ 📂 Store Management
├─ 📂 Location Management
├─ 📂 Configurations
├─ 📂 Currency
│
└─ 📂 Approval Management ✨ NEW!
   │
   ├─ 📄 Approval Dashboard
   │   └─ Path: /administration/approvals
   │   └─ Shows: Statistics, charts, overview
   │
   ├─ 📄 Pending Approvals
   │   └─ Path: /administration/approvals?tab=pending
   │   └─ Shows: All approvals waiting for action
   │
   ├─ 📄 Approved Requests
   │   └─ Path: /administration/approvals?tab=approved
   │   └─ Shows: All approved requests
   │
   ├─ 📄 Rejected Requests
   │   └─ Path: /administration/approvals?tab=rejected
   │   └─ Shows: All rejected requests
   │
   └─ 📄 Approval History
       └─ Path: /administration/approvals?tab=all
       └─ Shows: Complete approval history
```

---

## Database Records

### Module Level (Depth 1)

```
┌──────────────────────────────────────────────────────┐
│ modules                                              │
├──────────────────────────────────────────────────────┤
│ id: 1                                                │
│ uid: SystemAdministration                            │
│ module_name_en: Administration                       │
│ platform: Web                                        │
│ show_in_menu: true                                   │
└──────────────────────────────────────────────────────┘
```

### Sub-Module Level (Depth 2)

```
┌──────────────────────────────────────────────────────┐
│ sub_modules                                          │
├──────────────────────────────────────────────────────┤
│ id: 83                                               │
│ uid: SystemAdministration_ApprovalManagement         │
│ submodule_name_en: Approval Management               │
│ relative_path: approvals                             │
│ module_uid: SystemAdministration                     │
│ serial_no: 27                                        │
│ show_in_menu: true                                   │
└──────────────────────────────────────────────────────┘
```

### Sub-Sub-Module Level (Depth 3)

```
┌──────────────────────────────────────────────────────────────────┐
│ sub_sub_modules                                                  │
├──────────────────────────────────────────────────────────────────┤
│ 1. Approval Dashboard                                            │
│    id: 332                                                       │
│    uid: SystemAdministration_ApprovalManagement_Dashboard        │
│    relative_path: approvals                                      │
│    serial_no: 1                                                  │
├──────────────────────────────────────────────────────────────────┤
│ 2. Pending Approvals                                             │
│    id: 333                                                       │
│    uid: SystemAdministration_ApprovalManagement_Pending          │
│    relative_path: approvals?tab=pending                          │
│    serial_no: 2                                                  │
├──────────────────────────────────────────────────────────────────┤
│ 3. Approved Requests                                             │
│    id: 334                                                       │
│    uid: SystemAdministration_ApprovalManagement_Approved         │
│    relative_path: approvals?tab=approved                         │
│    serial_no: 3                                                  │
├──────────────────────────────────────────────────────────────────┤
│ 4. Rejected Requests                                             │
│    id: 335                                                       │
│    uid: SystemAdministration_ApprovalManagement_Rejected         │
│    relative_path: approvals?tab=rejected                         │
│    serial_no: 4                                                  │
├──────────────────────────────────────────────────────────────────┤
│ 5. Approval History                                              │
│    id: 336                                                       │
│    uid: SystemAdministration_ApprovalManagement_History          │
│    relative_path: approvals?tab=all                              │
│    serial_no: 5                                                  │
└──────────────────────────────────────────────────────────────────┘
```

---

## How Frontend Renders This

### Step 1: Query Modules

```sql
SELECT * FROM modules
WHERE platform = 'Web'
  AND show_in_menu = true
ORDER BY serial_no;
```

**Result:** Administration, Sales Management, Distribution Management, etc.

---

### Step 2: Query Sub-Modules for Administration

```sql
SELECT * FROM sub_modules
WHERE module_uid = 'SystemAdministration'
  AND show_in_menu = true
ORDER BY serial_no;
```

**Result:** Includes "Approval Management" (serial_no: 27)

---

### Step 3: Query Sub-Sub-Modules for Approval Management

```sql
SELECT * FROM sub_sub_modules
WHERE sub_module_uid = 'SystemAdministration_ApprovalManagement'
  AND show_in_menu = true
ORDER BY serial_no;
```

**Result:** 5 items (Dashboard, Pending, Approved, Rejected, History)

---

## React/Next.js Navigation Component Example

```tsx
// Pseudo-code for navigation structure

const navigationData = {
  module: {
    id: 'SystemAdministration',
    name: 'Administration',
    icon: <SettingOutlined />
  },
  subModules: [
    // ... other sub-modules ...
    {
      id: 'SystemAdministration_ApprovalManagement',
      name: 'Approval Management',
      icon: <CheckCircleOutlined />,
      path: '/administration/approvals',
      children: [
        {
          id: 'SystemAdministration_ApprovalManagement_Dashboard',
          name: 'Approval Dashboard',
          path: '/administration/approvals',
          icon: <DashboardOutlined />
        },
        {
          id: 'SystemAdministration_ApprovalManagement_Pending',
          name: 'Pending Approvals',
          path: '/administration/approvals?tab=pending',
          icon: <ClockCircleOutlined />,
          badge: pendingCount // Show count badge
        },
        {
          id: 'SystemAdministration_ApprovalManagement_Approved',
          name: 'Approved Requests',
          path: '/administration/approvals?tab=approved',
          icon: <CheckCircleOutlined />
        },
        {
          id: 'SystemAdministration_ApprovalManagement_Rejected',
          name: 'Rejected Requests',
          path: '/administration/approvals?tab=rejected',
          icon: <CloseCircleOutlined />
        },
        {
          id: 'SystemAdministration_ApprovalManagement_History',
          name: 'Approval History',
          path: '/administration/approvals?tab=all',
          icon: <HistoryOutlined />
        }
      ]
    }
  ]
};
```

---

## URL Mappings

| Menu Item | URL | Page Component |
|-----------|-----|----------------|
| Approval Dashboard | `/administration/approvals` | `app/administration/approvals/page.tsx` |
| Pending Approvals | `/administration/approvals?tab=pending` | Same page, pending tab |
| Approved Requests | `/administration/approvals?tab=approved` | Same page, approved tab |
| Rejected Requests | `/administration/approvals?tab=rejected` | Same page, rejected tab |
| Approval History | `/administration/approvals?tab=all` | Same page, all tab |
| View Detail | `/administration/approvals/[id]` | `app/administration/approvals/[id]/page.tsx` |

---

## Permission Checking

Frontend should check permissions based on UIDs:

```typescript
const hasAccessToApprovals = checkPermission([
  'SystemAdministration_ApprovalManagement',
  'SystemAdministration_ApprovalManagement_Dashboard'
]);

const canViewPending = checkPermission(
  'SystemAdministration_ApprovalManagement_Pending'
);

const canViewHistory = checkPermission(
  'SystemAdministration_ApprovalManagement_History'
);
```

---

## Menu Item Properties

All menu items have these common properties:

| Property | Value | Description |
|----------|-------|-------------|
| **show_in_menu** | `true` | Visible in navigation |
| **is_for_distributor** | `true` | Distributors can access |
| **is_for_principal** | `true` | Principals can access |
| **ss** | `0` | Sync status |
| **created_by** | `ADMIN` | Created by admin |

---

## Visual Menu Preview

```
┌───────────────────────────────────────────────┐
│ 🏢 Multiplex System                           │
├───────────────────────────────────────────────┤
│                                               │
│ 🏠 Dashboard                                  │
│                                               │
│ 📊 Sales Management                           │
│ ├─ Customer Management                        │
│ ├─ Product Management                         │
│ └─ Price Management                           │
│                                               │
│ 🚚 Distribution Management                    │
│ ├─ Sales Orders                               │
│ └─ Credit Returns                             │
│                                               │
│ ⚙️ Administration ◀                          │
│ ├─ Organization Management                    │
│ ├─ Employee Management                        │
│ ├─ Role & Permission Management               │
│ ├─ ...                                        │
│ └─ ✅ Approval Management ✨                  │
│    ├─ 📊 Approval Dashboard                   │
│    ├─ 🕐 Pending Approvals (5) 🔴            │
│    ├─ ✅ Approved Requests                    │
│    ├─ ❌ Rejected Requests                    │
│    └─ 📜 Approval History                     │
│                                               │
└───────────────────────────────────────────────┘
```

---

## Badge/Counter Implementation

For "Pending Approvals", show count:

```tsx
<MenuItem key="pending">
  <Link to="/administration/approvals?tab=pending">
    <ClockCircleOutlined />
    Pending Approvals
    {pendingCount > 0 && (
      <Badge count={pendingCount} className="ml-2" />
    )}
  </Link>
</MenuItem>
```

Fetch count:
```typescript
const getPendingCount = async () => {
  const stats = await approvalService.getApprovalStatistics();
  return stats.totalPending;
};
```

---

## Breadcrumb Navigation

Example breadcrumbs for different pages:

```
Dashboard Page:
Home > Administration > Approval Management

Pending Tab:
Home > Administration > Approval Management > Pending Approvals

Detail Page:
Home > Administration > Approval Management > Request #123
```

---

## Active Route Highlighting

Highlight menu item based on current route:

```typescript
const isActive = (path: string) => {
  const currentPath = window.location.pathname;
  const currentSearch = window.location.search;

  if (path.includes('?')) {
    // Match with query params
    return `${currentPath}${currentSearch}` === path;
  }
  return currentPath === path;
};
```

---

## Summary

✅ **Module:** Administration (SystemAdministration)
✅ **Sub-Module:** Approval Management (serial_no: 27)
✅ **Sub-Sub-Modules:** 5 items (Dashboard, Pending, Approved, Rejected, History)
✅ **Database IDs:** 83 (sub-module), 332-336 (sub-sub-modules)
✅ **Status:** Ready for frontend integration

The navigation structure is now complete and waiting for the frontend to render it!
