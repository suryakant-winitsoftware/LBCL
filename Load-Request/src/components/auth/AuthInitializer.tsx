'use client';

import { useEffect } from 'react';
import { ApiDebug } from '@/utils/api-debug';

export function AuthInitializer() {
  useEffect(() => {
    // Load API debug utility
    ApiDebug.logToConsole();
  }, []);

  return null; // This component doesn't render anything
}