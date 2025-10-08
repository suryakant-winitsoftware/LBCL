# Sales Itinerary - PostgreSQL Setup Guide

## ✅ Current Status: localStorage (Working)
- Entries saved to browser localStorage
- Data persists on page refresh
- CRUD operations working locally

## 🎯 Next Step: PostgreSQL Integration

### 1. Database Schema

Create this table in PostgreSQL:

```sql
CREATE TABLE sales_itinerary (
    id SERIAL PRIMARY KEY,
    type VARCHAR(255) NOT NULL,
    focus_area VARCHAR(255) NOT NULL,
    date DATE NOT NULL,
    day VARCHAR(10),
    kra_plan TEXT NOT NULL,
    morning_meeting VARCHAR(100),
    time_from VARCHAR(20),
    time_to VARCHAR(20),
    market VARCHAR(100) NOT NULL,
    channel VARCHAR(100),
    route_no VARCHAR(50),
    outlet_no VARCHAR(50),
    accompanied_by VARCHAR(100) NOT NULL,
    night_out VARCHAR(100) NOT NULL,
    no_of_days INTEGER DEFAULT 1,
    planned_mileage INTEGER DEFAULT 0,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    created_by VARCHAR(100),
    is_active BOOLEAN DEFAULT TRUE
);

-- Index for faster queries
CREATE INDEX idx_sales_itinerary_date ON sales_itinerary(date);
CREATE INDEX idx_sales_itinerary_type ON sales_itinerary(type);
CREATE INDEX idx_sales_itinerary_active ON sales_itinerary(is_active);
```

### 2. Install Required Packages

```bash
npm install pg
npm install --save-dev @types/pg
```

### 3. Database Connection Configuration

File: `/lib/db/postgres.ts`

```typescript
import { Pool } from 'pg';

const pool = new Pool({
  host: '10.20.53.130',
  port: 5432,
  database: 'multiplexdev15072025',
  user: 'multiplex',
  password: 'multiplex',
  max: 20, // Maximum number of clients in the pool
  idleTimeoutMillis: 30000,
  connectionTimeoutMillis: 2000,
});

pool.on('error', (err) => {
  console.error('Unexpected error on idle client', err);
  process.exit(-1);
});

export default pool;
```

### 4. API Routes Structure

#### GET All Entries
File: `/app/api/itinerary/route.ts`

```typescript
import { NextRequest, NextResponse } from 'next/server';
import pool from '@/lib/db/postgres';

export async function GET(request: NextRequest) {
  try {
    const result = await pool.query(
      `SELECT * FROM sales_itinerary
       WHERE is_active = true
       ORDER BY date DESC, id DESC`
    );

    return NextResponse.json({
      success: true,
      data: result.rows,
      count: result.rowCount
    });
  } catch (error) {
    console.error('Error fetching itinerary entries:', error);
    return NextResponse.json(
      { success: false, error: 'Failed to fetch entries' },
      { status: 500 }
    );
  }
}

export async function POST(request: NextRequest) {
  try {
    const body = await request.json();

    const result = await pool.query(
      `INSERT INTO sales_itinerary (
        type, focus_area, date, day, kra_plan,
        morning_meeting, time_from, time_to,
        market, channel, route_no, outlet_no,
        accompanied_by, night_out, no_of_days, planned_mileage
      ) VALUES ($1, $2, $3, $4, $5, $6, $7, $8, $9, $10, $11, $12, $13, $14, $15, $16)
      RETURNING *`,
      [
        body.type,
        body.focusArea,
        body.date,
        body.day,
        body.kraPlan,
        body.morningMeeting || 'NA',
        body.timeFrom,
        body.timeTo,
        body.market,
        body.channel || 'NA',
        body.routeNo || 'NA',
        body.outletNo || 'NA',
        body.accompaniedBy,
        body.nightOut,
        body.noOfDays || 1,
        body.plannedMileage || 0
      ]
    );

    return NextResponse.json({
      success: true,
      data: result.rows[0]
    });
  } catch (error) {
    console.error('Error creating itinerary entry:', error);
    return NextResponse.json(
      { success: false, error: 'Failed to create entry' },
      { status: 500 }
    );
  }
}
```

#### DELETE Entry
File: `/app/api/itinerary/[id]/route.ts`

```typescript
import { NextRequest, NextResponse } from 'next/server';
import pool from '@/lib/db/postgres';

export async function DELETE(
  request: NextRequest,
  { params }: { params: { id: string } }
) {
  try {
    const id = params.id;

    // Soft delete
    const result = await pool.query(
      `UPDATE sales_itinerary
       SET is_active = false, updated_at = CURRENT_TIMESTAMP
       WHERE id = $1
       RETURNING *`,
      [id]
    );

    if (result.rowCount === 0) {
      return NextResponse.json(
        { success: false, error: 'Entry not found' },
        { status: 404 }
      );
    }

    return NextResponse.json({
      success: true,
      message: 'Entry deleted successfully'
    });
  } catch (error) {
    console.error('Error deleting itinerary entry:', error);
    return NextResponse.json(
      { success: false, error: 'Failed to delete entry' },
      { status: 500 }
    );
  }
}

export async function PUT(
  request: NextRequest,
  { params }: { params: { id: string } }
) {
  try {
    const id = params.id;
    const body = await request.json();

    const result = await pool.query(
      `UPDATE sales_itinerary
       SET type = $1, focus_area = $2, date = $3, day = $4,
           kra_plan = $5, time_from = $6, time_to = $7,
           market = $8, channel = $9, route_no = $10,
           outlet_no = $11, accompanied_by = $12, night_out = $13,
           no_of_days = $14, planned_mileage = $15,
           updated_at = CURRENT_TIMESTAMP
       WHERE id = $16 AND is_active = true
       RETURNING *`,
      [
        body.type,
        body.focusArea,
        body.date,
        body.day,
        body.kraPlan,
        body.timeFrom,
        body.timeTo,
        body.market,
        body.channel,
        body.routeNo,
        body.outletNo,
        body.accompaniedBy,
        body.nightOut,
        body.noOfDays,
        body.plannedMileage,
        id
      ]
    );

    if (result.rowCount === 0) {
      return NextResponse.json(
        { success: false, error: 'Entry not found' },
        { status: 404 }
      );
    }

    return NextResponse.json({
      success: true,
      data: result.rows[0]
    });
  } catch (error) {
    console.error('Error updating itinerary entry:', error);
    return NextResponse.json(
      { success: false, error: 'Failed to update entry' },
      { status: 500 }
    );
  }
}
```

### 5. Service Layer

File: `/services/itineraryService.ts`

```typescript
export interface ItineraryEntry {
  id: number;
  type: string;
  focusArea: string;
  date: string;
  day: string;
  kraPlan: string;
  morningMeeting?: string;
  timeFrom: string;
  timeTo: string;
  market: string;
  channel?: string;
  routeNo?: string;
  outletNo?: string;
  accompaniedBy: string;
  nightOut: string;
  noOfDays: number;
  plannedMileage: number;
}

class ItineraryService {
  private baseUrl = '/api/itinerary';

  async getAll(): Promise<ItineraryEntry[]> {
    const response = await fetch(this.baseUrl);
    const result = await response.json();

    if (!result.success) {
      throw new Error(result.error || 'Failed to fetch entries');
    }

    return result.data;
  }

  async create(entry: Omit<ItineraryEntry, 'id'>): Promise<ItineraryEntry> {
    const response = await fetch(this.baseUrl, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(entry),
    });

    const result = await response.json();

    if (!result.success) {
      throw new Error(result.error || 'Failed to create entry');
    }

    return result.data;
  }

  async delete(id: number): Promise<void> {
    const response = await fetch(`${this.baseUrl}/${id}`, {
      method: 'DELETE',
    });

    const result = await response.json();

    if (!result.success) {
      throw new Error(result.error || 'Failed to delete entry');
    }
  }

  async update(id: number, entry: Partial<ItineraryEntry>): Promise<ItineraryEntry> {
    const response = await fetch(`${this.baseUrl}/${id}`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(entry),
    });

    const result = await response.json();

    if (!result.success) {
      throw new Error(result.error || 'Failed to update entry');
    }

    return result.data;
  }
}

export const itineraryService = new ItineraryService();
```

### 6. Update Components to Use API

#### Update `add-itinerary-entry.tsx`:

Replace localStorage save with API call:

```typescript
const handleSave = async () => {
  const finalFormData = {
    type: formData.type || "",
    focusArea: formData.focusArea || "",
    date: formData.date || "",
    day: formData.day || "",
    kraPlan: formData.kraPlan === "Other" ? kraPlanOther : formData.kraPlan || "",
    morningMeeting: "NA",
    timeFrom: `${timeFromHour}:${timeFromMin} ${timeFromPeriod}`,
    timeTo: `${timeToHour}:${timeToMin} ${timeToPeriod}`,
    market: formData.market || "",
    channel: formData.channel || "NA",
    routeNo: formData.routeNo || "NA",
    outletNo: formData.outletNo || "NA",
    accompaniedBy: formData.accompaniedBy || "",
    nightOut: formData.nightOut || "",
    noOfDays: formData.noOfDays || 1,
    plannedMileage: formData.plannedMileage || 0,
  };

  try {
    await itineraryService.create(finalFormData);
    console.log("✅ Entry saved to database");
    router.push("/lbcl/sales-itinerary");
  } catch (error) {
    console.error("❌ Error saving entry:", error);
    alert("Failed to save entry. Please try again.");
  }
};
```

#### Update `sales-itinerary-template.tsx`:

Replace localStorage with API:

```typescript
useEffect(() => {
  const loadEntries = async () => {
    try {
      const data = await itineraryService.getAll();
      setEntries(data);
      console.log("✅ Loaded entries from database:", data);
    } catch (error) {
      console.error("❌ Error loading entries:", error);
      // Fallback to mock data on error
      setEntries(MOCK_ITINERARY_DATA);
    }
  };

  loadEntries();
}, []);

const handleDeleteEntry = async (id: number) => {
  try {
    await itineraryService.delete(id);
    setEntries(entries.filter(e => e.id !== id));
    console.log("🗑️ Entry deleted from database");
  } catch (error) {
    console.error("❌ Error deleting entry:", error);
    alert("Failed to delete entry. Please try again.");
  }
};
```

## 📝 Migration Steps

1. ✅ **Phase 1 (Current)**: localStorage - Working now
2. **Phase 2**: Create PostgreSQL table
3. **Phase 3**: Create API routes
4. **Phase 4**: Create service layer
5. **Phase 5**: Update components to use API
6. **Phase 6**: Test CRUD operations
7. **Phase 7**: Add authentication/authorization

## 🔧 Environment Variables

Add to `.env.local`:

```env
DB_HOST=10.20.53.130
DB_PORT=5432
DB_NAME=multiplexdev15072025
DB_USER=multiplex
DB_PASSWORD=multiplex
```

## ✅ Next.js PostgreSQL Support

**Yes!** Next.js fully supports PostgreSQL:

- ✅ Native support through API routes
- ✅ Works with `pg`, `pg-promise`, Prisma, TypeORM
- ✅ Server-side only (secure, credentials not exposed)
- ✅ Full CRUD operations
- ✅ Connection pooling for performance
- ✅ TypeScript support

## 🎯 Recommendation

**Start with localStorage** (already working), then migrate to PostgreSQL when ready. This approach:
- ✅ Allows immediate testing
- ✅ No database setup needed initially
- ✅ Easy to migrate later (just swap service implementation)
- ✅ Same CRUD operations work for both
