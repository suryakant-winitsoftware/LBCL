import { useCallback } from 'react';

export function usePromotionCode() {
  const generatePromotionCode = useCallback((name: string): string => {
    if (!name) return '';
    
    // Remove special characters and split by spaces
    const words = name
      .replace(/[^a-zA-Z0-9\s]/g, '')
      .toUpperCase()
      .split(' ')
      .filter(word => word.length > 0);
    
    let code = '';
    
    if (words.length === 1) {
      // Single word: take first 6 characters
      code = words[0].substring(0, 6);
    } else if (words.length === 2) {
      // Two words: take first 3 characters of each
      code = words[0].substring(0, 3) + words[1].substring(0, 3);
    } else {
      // Multiple words: take first 2 characters of first 3 words
      code = words
        .slice(0, 3)
        .map(word => word.substring(0, 2))
        .join('');
    }
    
    // Add random number suffix
    const randomNum = Math.floor(Math.random() * 100);
    code += randomNum.toString().padStart(2, '0');
    
    return code.toUpperCase();
  }, []);

  const validatePromotionCode = useCallback((code: string): boolean => {
    // Code should be alphanumeric, 4-20 characters
    const codeRegex = /^[A-Z0-9]{4,20}$/;
    return codeRegex.test(code);
  }, []);

  const formatPromotionCode = useCallback((code: string): string => {
    // Remove non-alphanumeric characters and convert to uppercase
    return code.replace(/[^A-Z0-9]/gi, '').toUpperCase();
  }, []);

  return {
    generatePromotionCode,
    validatePromotionCode,
    formatPromotionCode,
  };
}