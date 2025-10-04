# Category Brand Competitor Mapping

This module manages the mapping between categories, brands, and competitor companies.

## Features

- **View Mappings**: Display all category-brand-competitor mappings in a paginated table
- **Filter & Search**: Filter by category, brand, and search across all fields
- **Add New Mapping**: Create new competitor mappings for category-brand combinations
- **Edit Mapping**: Update competitor assignments for existing mappings
- **Delete Mapping**: Remove mappings (soft delete)
- **Bulk Operations**: Select and delete multiple mappings at once

## API Endpoints

The module uses the following API endpoints:

1. `GET /api/CompetitorBrand/GetMappings` - Fetches all mappings with pagination and filtering
2. `POST /api/CompetitorBrand/Create` - Creates new competitor brand mapping
3. `PUT /api/CompetitorBrand/Update` - Updates existing mapping
4. `DELETE /api/CompetitorBrand/Delete/{uid}` - Soft deletes a mapping
5. `GET /api/CompetitorBrand/GetCategories` - Gets categories for dropdown
6. `GET /api/CompetitorBrand/GetBrandsByCategory/{categoryCode}` - Gets brands for selected category
7. `GET /api/CompetitorBrand/GetCompetitors` - Gets competitor list for dropdown

## File Structure

```
category-brand-competitor-mapping/
├── page.tsx                 # Main list page with table and filters
├── components/
│   └── CompetitorBrandModal.tsx  # Add/Edit modal component
└── README.md               # This documentation
```

## Service Layer

The `competitorBrandService.ts` file in `/services/` provides all API interactions:

- Pagination support
- Error handling
- TypeScript interfaces
- Bulk operations

## Access URL

The page is accessible at:
`/administration/configurations/category-brand-competitor-mapping`

## Dependencies

- React components from shadcn/ui
- Lucide React icons
- Axios for API calls
- Next.js router
- Toast notifications

## Usage

1. Navigate to the URL above
2. Use filters to narrow down results
3. Click "Add New" to create a mapping
4. Use the edit icon to modify existing mappings
5. Select multiple items and delete them in bulk
6. Use pagination controls for large datasets

## Data Flow

1. **Create**: Select category → Select brand → Select competitor → Save
2. **Edit**: Only allows changing the competitor (category/brand locked)
3. **Delete**: Soft delete (sets status to inactive)
4. **Filter**: Real-time filtering and server-side pagination