/**
 * Cache Service for Journey Plan Creation
 * Implements in-memory caching with TTL for API responses
 */

interface CacheEntry<T> {
  data: T;
  timestamp: number;
  ttl: number;
}

class CacheService {
  private cache: Map<string, CacheEntry<any>> = new Map();
  private defaultTTL = 5 * 60 * 1000; // 5 minutes default

  /**
   * Get cached data if valid
   */
  get<T>(key: string): T | null {
    const entry = this.cache.get(key);
    if (!entry) return null;

    const now = Date.now();
    if (now - entry.timestamp > entry.ttl) {
      // Cache expired
      this.cache.delete(key);
      return null;
    }

    console.log(`[Cache HIT] ${key}`);
    return entry.data as T;
  }

  /**
   * Set cache data
   */
  set<T>(key: string, data: T, ttl?: number): void {
    console.log(`[Cache SET] ${key}`);
    this.cache.set(key, {
      data,
      timestamp: Date.now(),
      ttl: ttl || this.defaultTTL,
    });
  }

  /**
   * Clear specific cache or all
   */
  clear(key?: string): void {
    if (key) {
      this.cache.delete(key);
      console.log(`[Cache CLEAR] ${key}`);
    } else {
      this.cache.clear();
      console.log('[Cache CLEAR ALL]');
    }
  }

  /**
   * Generate cache key
   */
  generateKey(...parts: any[]): string {
    return parts.filter(Boolean).join(':');
  }

  /**
   * Check if cache exists and is valid
   */
  has(key: string): boolean {
    const data = this.get(key);
    return data !== null;
  }

  /**
   * Get or fetch data with caching
   */
  async getOrFetch<T>(
    key: string,
    fetcher: () => Promise<T>,
    ttl?: number
  ): Promise<T> {
    // Check cache first
    const cached = this.get<T>(key);
    if (cached !== null) {
      return cached;
    }

    // Fetch and cache
    console.log(`[Cache MISS] ${key} - Fetching...`);
    try {
      const data = await fetcher();
      this.set(key, data, ttl);
      return data;
    } catch (error) {
      console.error(`[Cache ERROR] Failed to fetch for ${key}:`, error);
      throw error;
    }
  }

  /**
   * Batch fetch with caching
   */
  async batchFetch<T>(
    requests: Array<{
      key: string;
      fetcher: () => Promise<T>;
      ttl?: number;
    }>
  ): Promise<T[]> {
    const promises = requests.map(req =>
      this.getOrFetch(req.key, req.fetcher, req.ttl)
    );
    return Promise.all(promises);
  }

  /**
   * Prefetch data in background
   */
  prefetch<T>(key: string, fetcher: () => Promise<T>, ttl?: number): void {
    if (!this.has(key)) {
      fetcher()
        .then(data => this.set(key, data, ttl))
        .catch(error => console.error(`[Cache PREFETCH ERROR] ${key}:`, error));
    }
  }
}

// Singleton instance
export const cacheService = new CacheService();

// Specific cache keys for journey plan
export const CACHE_KEYS = {
  routes: (orgUID: string) => `routes:${orgUID}`,
  employees: (orgUID: string) => `employees:${orgUID}`,
  vehicles: (orgUID: string) => `vehicles:${orgUID}`,
  locations: (orgUID: string) => `locations:${orgUID}`,
  customers: (routeUID: string) => `customers:${routeUID}`,
  holidays: (orgUID: string, start: string, end: string) => `holidays:${orgUID}:${start}:${end}`,
  routeDetails: (routeUID: string) => `route:${routeUID}`,
  organizations: () => 'organizations:all',
  orgTypes: () => 'orgTypes:all',
};