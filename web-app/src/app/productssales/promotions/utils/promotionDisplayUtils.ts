// Promotion Display Utilities
// Provides human-readable names for promotion types, levels, and formats

export const PROMOTION_LEVEL_DISPLAY: Record<string, string> = {
  ITEM_LEVEL: "Item/Line Level",
  INVOICE_LEVEL: "Invoice Level",
  instant: "Item/Line Level",
  invoice: "Invoice Level",
  LINE_LEVEL: "Item/Line Level",
  UNKNOWN: "General",
};

export const PROMOTION_FORMAT_DISPLAY: Record<string, string> = {
  IQFD: "Item Fixed Discount",
  IQPD: "Item Percentage Discount",
  IQXF: "Item Quantity X Free",
  BQXF: "Buy Quantity X Free (FOC)",
  MPROD: "Multi-Product Configuration",
  BYVALUE: "By Invoice Value",
  BYQTY: "By Total Quantity",
  LINECOUNT: "By Line Count",
  BRANDCOUNT: "By Brand Count",
  ANYVALUE: "Any Value",
  UNKNOWN: "General Promotion",
};

export const PROMOTION_TYPE_DISPLAY: Record<string, string> = {
  DISCOUNT: "Discount",
  BOGO: "Buy One Get One",
  PERCENTAGE: "Percentage Off",
  CASHBACK: "Cashback",
  SEASONAL: "Seasonal",
  LOYALTY: "Loyalty Reward",
  BUNDLE: "Bundle Deal",
  UNKNOWN: "General",
};

/**
 * Get human-readable promotion type from various promotion data sources
 */
export function getPromotionDisplayType(promotion: any): string {
  // Try both uppercase and lowercase field names
  
  // Try format/Format first (most specific)
  const format = promotion.Format || promotion.format;
  if (format && PROMOTION_FORMAT_DISPLAY[format]) {
    return PROMOTION_FORMAT_DISPLAY[format];
  }

  // Try PromoFormat/promoFormat
  const promoFormat = promotion.PromoFormat || promotion.promoFormat;
  if (promoFormat && PROMOTION_FORMAT_DISPLAY[promoFormat]) {
    return PROMOTION_FORMAT_DISPLAY[promoFormat];
  }

  // Try level/Level
  const level = promotion.Level || promotion.level;
  if (level && PROMOTION_LEVEL_DISPLAY[level]) {
    return PROMOTION_LEVEL_DISPLAY[level];
  }

  // Try Type/type field
  const type = promotion.Type || promotion.type;
  if (type && PROMOTION_TYPE_DISPLAY[type]) {
    return PROMOTION_TYPE_DISPLAY[type];
  }

  // Check if it's a specific format code in name or description
  const name = promotion.Name || promotion.name;
  const description = promotion.Description || promotion.description || promotion.remarks;
  
  const formatFromName = Object.keys(PROMOTION_FORMAT_DISPLAY).find(
    (key) =>
      name?.toUpperCase().includes(key) ||
      description?.toUpperCase().includes(key)
  );

  if (formatFromName) {
    return PROMOTION_FORMAT_DISPLAY[formatFromName];
  }

  // Default fallback - show type and format if available
  if (type || promoFormat) {
    return `${type || ''} ${promoFormat || ''}`.trim();
  }

  return "General Promotion";
}

/**
 * Get promotion level display name
 */
export function getPromotionLevelDisplay(promotion: any): string {
  if (promotion.Level && PROMOTION_LEVEL_DISPLAY[promotion.Level]) {
    return PROMOTION_LEVEL_DISPLAY[promotion.Level];
  }

  if (promotion.Type && PROMOTION_LEVEL_DISPLAY[promotion.Type]) {
    return PROMOTION_LEVEL_DISPLAY[promotion.Type];
  }

  return "General";
}

/**
 * Get promotion format display name
 */
export function getPromotionFormatDisplay(promotion: any): string {
  if (promotion.Format && PROMOTION_FORMAT_DISPLAY[promotion.Format]) {
    return PROMOTION_FORMAT_DISPLAY[promotion.Format];
  }

  if (
    promotion.PromoFormat &&
    PROMOTION_FORMAT_DISPLAY[promotion.PromoFormat]
  ) {
    return PROMOTION_FORMAT_DISPLAY[promotion.PromoFormat];
  }

  return "General";
}

/**
 * Generate a descriptive text for the promotion
 */
export function getPromotionDescription(promotion: any): string {
  const type = getPromotionDisplayType(promotion);
  const level = getPromotionLevelDisplay(promotion);
  
  if (promotion.Description) {
    return promotion.Description;
  }

  if (promotion.Remarks) {
    return promotion.Remarks;
  }

  return `${type} promotion at ${level.toLowerCase()}`;
}

/**
 * Group promotions by their display type
 */
export function groupPromotionsByType(promotions: any[]): Record<string, any[]> {
  return promotions.reduce((groups, promotion) => {
    const type = getPromotionDisplayType(promotion);
    if (!groups[type]) {
      groups[type] = [];
    }
    groups[type].push(promotion);
    return groups;
  }, {} as Record<string, any[]>);
}

/**
 * Get promotion status display information
 */
export function getPromotionStatus(promotion: any): {
  status: 'active' | 'inactive' | 'scheduled' | 'expired';
  label: string;
  color: 'green' | 'red' | 'blue' | 'gray';
} {
  // Handle null/undefined promotion
  if (!promotion) {
    return { status: 'inactive', label: 'Unknown', color: 'gray' };
  }

  const now = new Date();
  const validFromDate = promotion.ValidFrom || promotion.validFrom;
  const validUptoDate = promotion.ValidUpto || promotion.validUpto;
  
  // Handle missing dates
  if (!validFromDate || !validUptoDate) {
    return { status: 'inactive', label: 'No Dates', color: 'gray' };
  }

  const validFrom = new Date(validFromDate);
  const validUpto = new Date(validUptoDate);
  const isActive = promotion.IsActive !== undefined ? promotion.IsActive : promotion.isActive;

  if (isActive === false) {
    return { status: 'inactive', label: 'Inactive', color: 'gray' };
  }

  if (now < validFrom) {
    return { status: 'scheduled', label: 'Scheduled', color: 'blue' };
  }

  if (now > validUpto) {
    return { status: 'expired', label: 'Expired', color: 'red' };
  }

  return { status: 'active', label: 'Active', color: 'green' };
}