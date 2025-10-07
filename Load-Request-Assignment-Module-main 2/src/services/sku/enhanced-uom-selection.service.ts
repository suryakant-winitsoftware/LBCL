// Enhanced UOM Selection Service - Uses our fully dynamic UOM service
// Provides rich UOM data with all physical dimensions for product creation

import { fullyDynamicUOMService } from './fully-dynamic-uom.service';

export interface EnhancedUOMOption {
  // Basic identifiers
  Id: number;
  UID: string;
  Code: string;
  Name: string;
  Label: string;
  
  // UOM classification
  IsBaseUOM: boolean;
  IsOuterUOM: boolean;
  Multiplier: number;
  
  // Physical dimensions - The missing fields we discovered!
  Length?: number;
  Width?: number;
  Height?: number;
  Depth?: number;
  Weight?: number;
  GrossWeight?: number;
  Volume?: number;
  Liter?: number;
  KGM?: number;
  
  // Units for measurements
  DimensionUnit?: string;
  WeightUnit?: string;
  VolumeUnit?: string;
  GrossWeightUnit?: string;
  
  // SKU relationship
  SKUUID?: string;
  
  // Display formatting
  displayText?: string;
  categoryInfo?: string;
}

export interface UOMSelectionData {
  baseUOMs: EnhancedUOMOption[];
  outerUOMs: EnhancedUOMOption[];
  allUOMs: EnhancedUOMOption[];
  dimensionUOMs: EnhancedUOMOption[];
  weightUOMs: EnhancedUOMOption[];
  volumeUOMs: EnhancedUOMOption[];
}

class EnhancedUOMSelectionService {
  private _cachedUOMs: EnhancedUOMOption[] = [];
  private _lastCacheTime: number = 0;
  private readonly CACHE_DURATION = 300000; // 5 minutes
  
  /**
   * Get all UOM data with full physical dimension information
   */
  async getAllEnhancedUOMs(): Promise<EnhancedUOMOption[]> {
    const now = Date.now();
    
    // Return cached data if fresh
    if (this._cachedUOMs.length > 0 && (now - this._lastCacheTime) < this.CACHE_DURATION) {
      return this._cachedUOMs;
    }
    
    try {
      // Use our fully dynamic UOM service to get ALL fields
      const rawUOMData = await fullyDynamicUOMService.getAllUOMData();
      
      // Transform to enhanced format with all discovered fields
      const enhancedUOMs: EnhancedUOMOption[] = rawUOMData.map(uom => ({
        // Basic identifiers
        Id: uom.Id,
        UID: uom.UID,
        Code: uom.Code,
        Name: uom.Name,
        Label: uom.Label || uom.Name,
        
        // UOM classification
        IsBaseUOM: Boolean(uom.IsBaseUOM),
        IsOuterUOM: Boolean(uom.IsOuterUOM),
        Multiplier: Number(uom.Multiplier) || 1,
        
        // Physical dimensions - The key missing data!
        Length: uom.Length ? Number(uom.Length) : undefined,
        Width: uom.Width ? Number(uom.Width) : undefined,
        Height: uom.Height ? Number(uom.Height) : undefined,
        Depth: uom.Depth ? Number(uom.Depth) : undefined,
        Weight: uom.Weight ? Number(uom.Weight) : undefined,
        GrossWeight: uom.GrossWeight ? Number(uom.GrossWeight) : undefined,
        Volume: uom.Volume ? Number(uom.Volume) : undefined,
        Liter: uom.Liter ? Number(uom.Liter) : undefined,
        KGM: uom.KGM ? Number(uom.KGM) : undefined,
        
        // Units for measurements
        DimensionUnit: uom.DimensionUnit,
        WeightUnit: uom.WeightUnit,
        VolumeUnit: uom.VolumeUnit,
        GrossWeightUnit: uom.GrossWeightUnit,
        
        // SKU relationship
        SKUUID: uom.SKUUID,
        
        // Generate rich display text with dimensions
        displayText: this.generateDisplayText(uom),
        categoryInfo: this.generateCategoryInfo(uom)
      }));
      
      // Cache the results
      this._cachedUOMs = enhancedUOMs;
      this._lastCacheTime = now;
      
      console.log(`✅ Loaded ${enhancedUOMs.length} enhanced UOMs with full dimension data`);
      return enhancedUOMs;
      
    } catch (error) {
      console.error('Failed to load enhanced UOM data:', error);
      return [];
    }
  }
  
  /**
   * Get categorized UOM selection data for product creation
   */
  async getUOMSelectionData(): Promise<UOMSelectionData> {
    const allUOMs = await this.getAllEnhancedUOMs();
    
    const baseUOMs = allUOMs.filter(uom => uom.IsBaseUOM);
    const outerUOMs = allUOMs.filter(uom => uom.IsOuterUOM);
    
    // Categorize by physical properties
    const dimensionUOMs = allUOMs.filter(uom => 
      uom.Length || uom.Width || uom.Height || uom.Depth
    );
    
    const weightUOMs = allUOMs.filter(uom => 
      uom.Weight || uom.GrossWeight || uom.KGM
    );
    
    const volumeUOMs = allUOMs.filter(uom => 
      uom.Volume || uom.Liter
    );
    
    return {
      baseUOMs,
      outerUOMs,
      allUOMs,
      dimensionUOMs,
      weightUOMs,
      volumeUOMs
    };
  }
  
  /**
   * Generate rich display text showing UOM with dimensions
   */
  private generateDisplayText(uom: any): string {
    let display = `${uom.Code} - ${uom.Name}`;
    
    // Add dimension info if available
    const dimensions = [];
    if (uom.Length || uom.Width || uom.Height) {
      const l = uom.Length || '?';
      const w = uom.Width || '?';
      const h = uom.Height || '?';
      dimensions.push(`${l}×${w}×${h}${uom.DimensionUnit || ''}`);
    }
    
    if (uom.Weight) {
      dimensions.push(`${uom.Weight}${uom.WeightUnit || 'kg'}`);
    }
    
    if (uom.Volume) {
      dimensions.push(`${uom.Volume}${uom.VolumeUnit || 'L'}`);
    }
    
    if (dimensions.length > 0) {
      display += ` (${dimensions.join(', ')})`;
    }
    
    // Add multiplier info
    if (uom.Multiplier && uom.Multiplier !== 1) {
      display += ` [×${uom.Multiplier}]`;
    }
    
    return display;
  }
  
  /**
   * Generate category information for UOM
   */
  private generateCategoryInfo(uom: any): string {
    const categories = [];
    
    if (uom.IsBaseUOM) categories.push('Base');
    if (uom.IsOuterUOM) categories.push('Outer');
    
    if (uom.Length || uom.Width || uom.Height) {
      categories.push('Dimensional');
    }
    
    if (uom.Weight || uom.GrossWeight) {
      categories.push('Weight');
    }
    
    if (uom.Volume || uom.Liter) {
      categories.push('Volume');
    }
    
    return categories.length > 0 ? categories.join(' • ') : 'Standard';
  }
  
  /**
   * Search UOMs by text with intelligent matching
   */
  async searchUOMs(searchTerm: string): Promise<EnhancedUOMOption[]> {
    const allUOMs = await this.getAllEnhancedUOMs();
    
    if (!searchTerm || searchTerm.trim() === '') {
      return allUOMs;
    }
    
    const term = searchTerm.toLowerCase().trim();
    
    return allUOMs.filter(uom => 
      uom.Code.toLowerCase().includes(term) ||
      uom.Name.toLowerCase().includes(term) ||
      uom.Label?.toLowerCase().includes(term) ||
      uom.displayText?.toLowerCase().includes(term) ||
      uom.categoryInfo?.toLowerCase().includes(term)
    );
  }
  
  /**
   * Get UOM by ID with full dimension data
   */
  async getUOMById(id: number): Promise<EnhancedUOMOption | null> {
    const allUOMs = await this.getAllEnhancedUOMs();
    return allUOMs.find(uom => uom.Id === id) || null;
  }
  
  /**
   * Get UOM by Code with full dimension data
   */
  async getUOMByCode(code: string): Promise<EnhancedUOMOption | null> {
    const allUOMs = await this.getAllEnhancedUOMs();
    return allUOMs.find(uom => uom.Code === code) || null;
  }
  
  /**
   * Clear cache to force fresh data load
   */
  clearCache(): void {
    this._cachedUOMs = [];
    this._lastCacheTime = 0;
  }
}

export const enhancedUOMSelectionService = new EnhancedUOMSelectionService();