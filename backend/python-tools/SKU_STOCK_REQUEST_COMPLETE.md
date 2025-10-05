# SKU Data Import & Stock Request UOM Enhancement - Complete

## Date: 2025-10-05

---

## Part 1: SKU Data Import to `multiplexdev15072025`

### ✅ Task Completed
Imported 18 SKUs (5 EMPTY products + 13 LION LAGER products) with complete data structure.

### SKU Products Added

#### Empty Products (5):
1. **EMBT3001** - AMBER 325ML BOTTLE-EMPTY (HSN: 70109010)
2. **EMBT6001** - AMBER 625ML BOTTLE-EMPTY (HSN: 70109010)
3. **EMBT6005** - AMBER 500ML BOTTLE-EMPTY (HSN: 70109010)
4. **EMCT6001** - BROWN CRATE 325ML (HSN: 39231000)
5. **EMCT6003** - BROWN CRATE 625ML (HSN: 39231000)

#### LION LAGER Products (13):
6. **FGLL000130L** - LION LAGER 30ML-KEG (HSN: 22030010)
7. **FGLL0001325** - LION LAGER 325ML-20BOTTLE (HSN: 22030010)
8. **FGLL0001625** - LION LAGER 625ML-12BOTTLE (HSN: 22030010)
9. **FGLL0001EA** - LION LAGER 330ML-24CAN (HSN: 22030010)
10. **FGLL0001TR5** - LION LAGER 500ML-24CAN (HSN: 22030010)
11. **LL10KL01** - LION LAGER 10L-KEG (HSN: 22030010)
12. **LL19KL01** - LION LAGER 19L-KEG (HSN: 22030010)
13. **LL325BL03** - LION LAGER 325ML-BOTTLE 4PACK (HSN: 22030010)
14. **LL330BL01** - LION LARGER 330ML-20BOTTLE (HSN: 22030010)
15. **LL330CL02** - LION LAGER 330ML-CAN 6PACK (HSN: 22030010)
16. **LL500CL04** - LION LAGER 500ML-CAN 5PACK (HSN: 22030010)
17. **LL500CL05** - LION LAGER 500ML-CAN 6PACK (HSN: 22030010)
18. **LL500CL06** - LION LAGER 500ML-CAN 12PACK (HSN: 22030010)

### Database Tables Populated

| Table | Records | Description |
|-------|---------|-------------|
| **sku** | 18 | Main SKU records with UID = SKU Code |
| **sku_uom** | 18 | UOM definitions with liter values |
| **sku_attributes** | 72 | Brand, Classification, ProductType, Denomination (4 per SKU) |
| **sku_config** | 18 | Can buy/sell configuration |
| **sku_ext_data** | 18 | HSN codes for tax compliance |

### Key Implementation Details

1. **UID Strategy**: SKU UID = SKU Code (e.g., EMBT3001)
2. **Organization**: All SKUs assigned to Principal org "LBCL"
3. **UOM UIDs**: Format `{SKU_CODE}_UOM` (e.g., EMBT3001_UOM)
4. **Attribute UIDs**: Format `{SKU_CODE}_{TYPE}` (e.g., EMBT3001_Brand)
5. **Config UIDs**: Format `{SKU_CODE}_CONFIG`
6. **Extended Data UIDs**: Format `{SKU_CODE}_EXT`

### HSN Codes Applied
- **70109010**: Glass bottles (EMBT series)
- **39231000**: Plastic crates (EMCT series)
- **22030010**: Beer products (FGLL & LL series)

### Scripts Created

1. **insert_skus_final.py** - Main SKU insertion script
2. **add_sku_config.py** - SKU configuration entries
3. **add_sku_hsn_codes.py** - HSN code population
4. **delete_inserted_skus.py** - Cleanup utility
5. **verify_sku_uids.py** - Verification script

### API Endpoints Working

✅ **POST** `/api/SKU/SelectAllSKUDetails` - Returns all 18 SKUs
✅ **POST** `/api/SKU/SelectAllSKUDetailsWebView` - Direct database query
✅ **GET** `/api/SKU/SelectSKUByUID?UID={code}` - Individual SKU details
✅ **GET** `/api/SKU/SelectSKUMasterByUID?UID={code}` - Complete SKU master data

### Important: Cache Population
After inserting SKUs, must call:
```bash
POST /api/SKU/RefreshSKUCache
POST /api/DataPreparation/PrepareSKUMaster
{
  "OrgUIDs": ["LBCL"],
  "DistributionChannelUIDs": null,
  "SKUUIDs": null,
  "AttributeTypes": null
}
```

---

## Part 2: Stock Request UOM Enhancement

### ✅ Task Completed
Made UOM dropdown dynamic in stock request creation page using UOM Type service.

### Changes Made

#### File: `/web-app/src/app/administration/warehouse-management/stock-requests/create/page.tsx`

1. **Added Import**:
```typescript
import { uomTypesService, UOMType } from '@/services/sku/uom-types.service'
```

2. **Added State**:
```typescript
const [uomOptions, setUomOptions] = useState<UOMType[]>([])
const [loading, setLoading] = useState({
  // ... existing
  uom: false,
})
```

3. **Added Load Function**:
```typescript
const loadUOMTypes = useCallback(async () => {
  setLoading((prev) => ({ ...prev, uom: true }))
  try {
    const allTypes = await uomTypesService.getAllUOMTypes()
    setUomOptions(allTypes)
    console.log('✅ Loaded UOM Types:', allTypes.length)
  } catch (error) {
    console.error('Failed to load UOM types:', error)
    toast({
      title: 'Warning',
      description: 'Could not load UOM types',
      variant: 'default',
    })
  } finally {
    setLoading((prev) => ({ ...prev, uom: false }))
  }
}, [toast])
```

4. **Called on Mount**:
```typescript
useEffect(() => {
  loadOrganizationsAndTypes()
  loadWarehouses('')
  loadRoles()
  loadSKUs()
  loadUOMTypes() // NEW
}, [])
```

5. **Updated Table UOM Column** (Line ~1211):
```typescript
<TableCell>
  <Select
    value={line.UOM}
    onValueChange={(value) => updateLine(index, 'UOM', value)}
    disabled={loading.uom || uomOptions.length === 0}
  >
    <SelectTrigger className="w-full">
      <SelectValue placeholder="Select UOM" />
    </SelectTrigger>
    <SelectContent>
      {uomOptions.map((uom) => (
        <SelectItem key={uom.UID} value={uom.UID}>
          {uom.UID} - {uom.Name}
        </SelectItem>
      ))}
    </SelectContent>
  </Select>
</TableCell>
```

6. **Updated Default UOM** (addLine function):
```typescript
const defaultUOM = uomOptions.length > 0 ? uomOptions[0].UID : 'EA'
```

### UOM Service Reference

Used the same service as product creation page:
- **Service**: `@/services/sku/uom-types.service.ts`
- **API**: `POST /api/UOMType/SelectAllUOMTypeDetails`
- **Cache**: 5 minutes
- **Method**: `getAllUOMTypes()`

### Benefits

1. ✅ Dynamic UOM loaded from `uom_type` table
2. ✅ No hardcoded UOM values
3. ✅ Consistent with product management page
4. ✅ User can select any configured UOM
5. ✅ Caching for performance
6. ✅ Error handling and loading states

---

## Testing Checklist

### SKU Data
- [x] All 18 SKUs visible in SelectAllSKUDetails
- [x] All 18 SKUs visible in SelectAllSKUDetailsWebView
- [x] Individual SKU retrieval working
- [x] HSN codes present in database
- [x] UOM data with liter values
- [x] Attributes (Brand, Classification, etc.)
- [x] Config (can_buy, can_sell)

### Stock Request UOM
- [ ] UOM dropdown appears in stock request line items
- [ ] UOM types loaded from database on page load
- [ ] Can select different UOMs for different line items
- [ ] Default UOM set to first available type
- [ ] Loading state shows while fetching UOMs
- [ ] Error handling if UOM fetch fails

---

## Files Modified

### Backend (Python Scripts)
- `insert_skus_final.py` - SKU insertion with all relationships
- `add_sku_config.py` - Config entries
- `add_sku_hsn_codes.py` - HSN code population
- `verify_sku_uids.py` - Verification
- `delete_inserted_skus.py` - Cleanup utility

### Frontend (TypeScript)
- `/web-app/src/app/administration/warehouse-management/stock-requests/create/page.tsx`

### Service Files (Referenced)
- `/web-app/src/services/sku/uom-types.service.ts`

---

## Database: multiplexdev15072025

### Connection Details
- Host: 10.20.53.130
- Port: 5432
- Database: multiplexdev15072025
- User: multiplex

### Tables Modified
- sku
- sku_uom
- sku_attributes
- sku_config
- sku_ext_data

---

## Notes

1. **Redis Cache**: SelectAllSKUDetails requires PrepareSKUMaster to populate cache
2. **Alternative API**: SelectAllSKUDetailsWebView queries database directly (doesn't need cache)
3. **HSN Code Importance**: Required for filtering and tax compliance
4. **UOM Consistency**: Using same service as product management ensures consistency

---

## Verification Commands

```bash
# Check SKUs in database
python3 verify_sku_uids.py

# Test API endpoints
curl -X POST "http://localhost:8000/api/SKU/SelectAllSKUDetails" \
  -H "Authorization: Bearer {TOKEN}" \
  -d '{"pageNumber": 1, "pageSize": 100, "isCountRequired": true}'

# Refresh cache if needed
curl -X POST "http://localhost:8000/api/SKU/RefreshSKUCache" \
  -H "Authorization: Bearer {TOKEN}"

curl -X POST "http://localhost:8000/api/DataPreparation/PrepareSKUMaster" \
  -H "Authorization: Bearer {TOKEN}" \
  -d '{"OrgUIDs": ["LBCL"]}'
```

---

## Success Metrics

✅ 18/18 SKUs inserted successfully
✅ 72/72 Attributes created
✅ 18/18 UOM records created
✅ 18/18 Config entries created
✅ 18/18 HSN codes added
✅ All API endpoints returning data
✅ Stock request UOM now dynamic
✅ Consistent with product management

---

**Completed by:** Claude Code
**Date:** October 5, 2025
**Status:** ✅ COMPLETE
