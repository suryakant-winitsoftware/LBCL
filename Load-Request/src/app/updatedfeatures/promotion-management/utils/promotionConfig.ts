import { ComponentType } from 'react';
import { Tag, FileText } from 'lucide-react';

export interface PromotionFormat {
  value: string;
  label: string;
  description: string;
}

export interface PromotionLevel {
  id: string;
  name: string;
  description: string;
  icon: ComponentType<{ className?: string }>;
  formats: PromotionFormat[];
}

export const PROMOTION_LEVELS: PromotionLevel[] = [
  {
    id: "instant",
    name: "Item/Line Level",
    description: "Applied at individual item level",
    icon: Tag,
    formats: [
      {
        value: "IQFD",
        label: "Item Fixed Discount",
        description: "Fixed amount off per item"
      },
      {
        value: "IQPD",
        label: "Item Percentage Discount",
        description: "Percentage off per item"
      },
      {
        value: "IQXF",
        label: "Item Quantity X Free",
        description: "Buy X units, get Y free"
      },
      {
        value: "BQXF",
        label: "Buy Quantity X Free (FOC)",
        description: "Buy X quantity, get FOC items"
      },
      {
        value: "MPROD",
        label: "Multi-Product Configuration",
        description: "Configure multiple products with individual discount rules"
      }
    ]
  },
  {
    id: "invoice",
    name: "Invoice Level",
    description: "Applied to entire invoice",
    icon: FileText,
    formats: [
      {
        value: "BYVALUE",
        label: "By Invoice Value",
        description: "Minimum invoice value required"
      },
      {
        value: "BYQTY",
        label: "By Total Quantity",
        description: "Minimum total quantity required"
      },
      {
        value: "LINECOUNT",
        label: "By Line Count",
        description: "Minimum line items required"
      },
      {
        value: "BRANDCOUNT",
        label: "By Brand Count",
        description: "Minimum brands required"
      },
      {
        value: "ANYVALUE",
        label: "Any Value",
        description: "No minimum threshold"
      }
    ]
  }
];

// Helper functions
export const getPromotionLevel = (levelId: string): PromotionLevel | undefined => {
  return PROMOTION_LEVELS.find(level => level.id === levelId);
};

export const getPromotionFormat = (levelId: string, formatValue: string): PromotionFormat | undefined => {
  const level = getPromotionLevel(levelId);
  return level?.formats.find(format => format.value === formatValue);
};

export const getPromotionLevelName = (levelId: string): string => {
  return getPromotionLevel(levelId)?.name || '';
};

export const getPromotionFormatLabel = (levelId: string, formatValue: string): string => {
  return getPromotionFormat(levelId, formatValue)?.label || '';
};