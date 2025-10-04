# SKU Sequencing Page - Setup Guide

## Overview
A complete drag-and-drop product sequencing system has been created at:
`/productssales/product-management/sequencing`

## Features Implemented ✅

### 1. **Drag & Drop Reordering**
- Smooth drag-and-drop interface using `@dnd-kit`
- Visual feedback during dragging
- Auto-updates serial numbers on reorder

### 2. **Sequence Type Selection**
- General (default for all users)
- Route (specific to routes)
- Customer (specific to customers)
- Easily extensible for more types

### 3. **Search & Filter**
- Search by SKU code or name
- Real-time filtering
- Shows filtered count

### 4. **Save/Reset Functionality**
- Tracks unsaved changes
- Visual indicator for unsaved changes
- Reset to last saved state
- Toast notifications for success/error

### 5. **Professional UI**
- Clean, modern design using shadcn/ui components
- Loading states
- Empty states
- Responsive layout

---

## Installation Steps

### Step 1: Install Required Dependencies

Run this command in your `web-app` directory:

```bash
cd /Users/suryakantkumar/Desktop/Multiplex/web-app

npm install @dnd-kit/core @dnd-kit/sortable @dnd-kit/utilities
```

Or if using yarn:

```bash
yarn add @dnd-kit/core @dnd-kit/sortable @dnd-kit/utilities
```

### Step 2: Verify Existing Dependencies

Make sure these are already installed (should be from shadcn/ui):
- `lucide-react` - for icons ✓
- `sonner` - for toast notifications ✓
- `@/components/ui/card` ✓
- `@/components/ui/button` ✓
- `@/components/ui/select` ✓
- `@/components/ui/input` ✓
- `@/components/ui/alert` ✓
- `@/components/ui/badge` ✓

### Step 3: Files Created

1. **Service Layer**
   - `src/services/sku/sku-sequence.service.ts`
   - Handles all API calls to backend

2. **Page Component**
   - `src/app/productssales/product-management/sequencing/page.tsx`
   - Main sequencing page with drag-and-drop

---

## How It Works

### 1. Loading Sequences

When page loads or sequence type changes:
```typescript
GET /api/SkuSequence/SelectAllSkuSequenceDetails?SeqType=General
```

Response includes:
- SKU UID
- SKU Code
- SKU Name
- Serial Number (current order)
- Sequence Type

### 2. Drag & Drop Reordering

User drags SKU from position 5 to position 2:
- Frontend updates order immediately (optimistic update)
- Serial numbers are recalculated (1, 2, 3, 4, 5...)
- "Unsaved Changes" badge appears

### 3. Saving Changes

When user clicks "Save Sequence":
```typescript
POST /api/SkuSequence/CUDSkuSequence
[
  { uid: "...", skuUID: "...", seqType: "General", serialNo: 1, actionType: "Add" },
  { uid: "...", skuUID: "...", seqType: "General", serialNo: 2, actionType: "Add" },
  // ... all sequences with updated serial numbers
]
```

Backend:
- Updates `serial_no` for each SKU in sequence
- Uses `actionType: "Add"` to update existing records

---

## Usage Guide for Users

### Basic Workflow

1. **Select Sequence Type**
   - Choose from dropdown: General, Route, or Customer
   - Products load automatically

2. **Reorder Products**
   - Click and hold the grip icon (⋮⋮)
   - Drag product to new position
   - Drop to place it

3. **Search Products**
   - Type in search box to filter
   - Search works on both code and name
   - Drag-and-drop still works on filtered list

4. **Save Changes**
   - Click "Save Sequence" button
   - Wait for success notification
   - "Unsaved Changes" badge disappears

5. **Reset Changes**
   - Click "Reset Changes" to undo
   - Returns to last saved state

---

## API Integration

### Backend APIs Used

1. **Get Sequences**
   ```
   POST /api/SkuSequence/SelectAllSkuSequenceDetails?SeqType={type}
   Body: { pageNumber, pageSize, sortCriterias, filterCriterias }
   ```

2. **Save Sequences**
   ```
   POST /api/SkuSequence/CUDSkuSequence
   Body: [{ uid, skuUID, seqType, serialNo, actionType }]
   ```

3. **Auto-Create Sequence** (for new SKUs)
   ```
   POST /api/SkuSequence/CreateGeneralSKUSequenceForSKU?BUOrgUID=...&SKUUID=...
   ```

---

## Database Impact

### When User Saves Sequence:

```sql
-- Updates serial_no for each SKU
UPDATE sku_sequence
SET serial_no = 1, modified_time = NOW()
WHERE uid = 'sku-1-uid' AND seq_type = 'General';

UPDATE sku_sequence
SET serial_no = 2, modified_time = NOW()
WHERE uid = 'sku-2-uid' AND seq_type = 'General';

-- ... and so on for all SKUs
```

### When Other Modules Query:

```sql
-- Sales Order, Purchase Order, etc. should ORDER BY serial_no
SELECT s.*, seq.serial_no
FROM sku s
LEFT JOIN sku_sequence seq ON s.uid = seq.sku_uid
    AND seq.seq_type = 'General'
ORDER BY seq.serial_no ASC;  -- Shows products in user-defined order!
```

---

## Extending the System

### Add New Sequence Types

1. Update `sequenceTypes` array in page.tsx:
```typescript
const sequenceTypes = [
  { label: 'General', value: 'General' },
  { label: 'Route', value: 'Route' },
  { label: 'Customer', value: 'Customer' },
  { label: 'Warehouse', value: 'Warehouse' },  // NEW
  { label: 'Region', value: 'Region' },        // NEW
];
```

2. That's it! Backend already supports any `seq_type` value.

### Add Route/Customer Selection

For Route or Customer specific sequences, add a second dropdown:

```typescript
{sequenceType === 'Route' && (
  <Select value={selectedRoute} onValueChange={setSelectedRoute}>
    <SelectTrigger>
      <SelectValue placeholder="Select route" />
    </SelectTrigger>
    <SelectContent>
      {routes.map(route => (
        <SelectItem key={route.uid} value={route.uid}>
          {route.name}
        </SelectItem>
      ))}
    </SelectContent>
  </Select>
)}
```

Then filter sequences by route UID when loading.

---

## Troubleshooting

### Issue: Drag & Drop Not Working
**Solution:** Make sure `@dnd-kit` packages are installed:
```bash
npm install @dnd-kit/core @dnd-kit/sortable @dnd-kit/utilities
```

### Issue: Toast Notifications Not Showing
**Solution:** Make sure `sonner` is configured in your root layout:
```tsx
import { Toaster } from 'sonner';

export default function RootLayout({ children }) {
  return (
    <html>
      <body>
        {children}
        <Toaster position="top-right" />
      </body>
    </html>
  );
}
```

### Issue: API Calls Failing
**Solution:** Check that backend API is running and accessible. Verify API base URL in `services/api.ts`

### Issue: Empty List Showing
**Solution:**
1. Check if SKUs have General sequences created
2. Run this to create sequences for existing SKUs:
   ```sql
   -- For each SKU without a sequence
   INSERT INTO sku_sequence (uid, bu_org_uid, sku_uid, seq_type, serial_no, created_by, created_time)
   SELECT gen_random_uuid(), '...', uid, 'General',
          ROW_NUMBER() OVER (ORDER BY code),
          'SYSTEM', NOW()
   FROM sku
   WHERE uid NOT IN (SELECT sku_uid FROM sku_sequence WHERE seq_type = 'General');
   ```

---

## Testing Checklist

- [ ] Install @dnd-kit dependencies
- [ ] Navigate to `/productssales/product-management/sequencing`
- [ ] Select "General" sequence type
- [ ] Verify products load
- [ ] Drag a product to new position
- [ ] Verify "Unsaved Changes" badge appears
- [ ] Click "Save Sequence"
- [ ] Verify success toast appears
- [ ] Refresh page and verify order persists
- [ ] Test search functionality
- [ ] Test "Reset Changes" button
- [ ] Switch to "Route" or "Customer" type
- [ ] Test with empty sequence type

---

## Next Steps

1. **Install Dependencies**
   ```bash
   npm install @dnd-kit/core @dnd-kit/sortable @dnd-kit/utilities
   ```

2. **Test the Page**
   - Start dev server: `npm run dev`
   - Navigate to: `http://localhost:3000/productssales/product-management/sequencing`

3. **Update Navigation Menu**
   - Add link to sequencing page in your sidebar/menu

4. **Apply Sequences to Other Modules**
   - Update Sales Order product list to ORDER BY serial_no
   - Update Purchase Order to use sequence
   - Update any product selection UI to respect sequence

---

## Support

If you encounter issues:
1. Check browser console for errors
2. Verify API responses in Network tab
3. Check backend logs for API errors
4. Ensure database has `sku_sequence` table with correct schema

---

**Created:** October 1, 2025
**Page Location:** `/productssales/product-management/sequencing`
**Service Location:** `/services/sku/sku-sequence.service.ts`
