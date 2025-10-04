/**
 * Common date formatting utilities
 * Provides consistent date formatting across the application
 */

/**
 * Format date to "04 sep, 2025" format
 * @param dateValue - Date value to format (string, Date, or any)
 * @param fallback - Fallback string if date is invalid (default: "N/A")
 * @returns Formatted date string
 */
export const formatDateToDayMonthYear = (dateValue: any, fallback: string = "N/A"): string => {
  try {
    if (!dateValue) return fallback;
    const date = new Date(dateValue);
    if (isNaN(date.getTime())) return fallback;
    
    const day = String(date.getDate()).padStart(2, '0');
    const month = date.toLocaleDateString('en-US', { month: 'short' }).toLowerCase();
    const year = date.getFullYear();
    return `${day} ${month}, ${year}`;
  } catch (error) {
    return fallback;
  }
};

/**
 * Format date to time format "2:30 PM"
 * @param dateValue - Date value to format
 * @param fallback - Fallback string if date is invalid (default: "N/A")
 * @returns Formatted time string
 */
export const formatTime = (dateValue: any, fallback: string = "N/A"): string => {
  try {
    if (!dateValue) return fallback;
    const date = new Date(dateValue);
    if (isNaN(date.getTime())) return fallback;
    
    return date.toLocaleTimeString('en-US', { 
      hour: 'numeric', 
      minute: '2-digit',
      hour12: true 
    });
  } catch (error) {
    return fallback;
  }
};

/**
 * Format date to "04 sep, 2025 at 2:30 PM" format
 * @param dateValue - Date value to format
 * @param fallback - Fallback string if date is invalid (default: "N/A")
 * @returns Formatted datetime string
 */
export const formatDateTime = (dateValue: any, fallback: string = "N/A"): string => {
  try {
    if (!dateValue) return fallback;
    const date = new Date(dateValue);
    if (isNaN(date.getTime())) return fallback;
    
    const dateStr = formatDateToDayMonthYear(dateValue);
    const timeStr = formatTime(dateValue);
    return `${dateStr} at ${timeStr}`;
  } catch (error) {
    return fallback;
  }
};

/**
 * Get day of week name from date
 * @param dateValue - Date value to format
 * @param fallback - Fallback string if date is invalid (default: "N/A")
 * @returns Day name (e.g., "Monday", "Tuesday")
 */
export const formatDayOfWeek = (dateValue: any, fallback: string = "N/A"): string => {
  try {
    if (!dateValue) return fallback;
    const date = new Date(dateValue);
    if (isNaN(date.getTime())) return fallback;
    
    return date.toLocaleDateString('en-US', { weekday: 'long' });
  } catch (error) {
    return fallback;
  }
};