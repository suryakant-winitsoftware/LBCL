export const API_CONFIG = {
  BASE_URL: process.env.NEXT_PUBLIC_API_URL || "http://localhost:8000/api",
  AUDIT_URL:
    process.env.NEXT_PUBLIC_AUDIT_API_URL || "http://localhost:5065/api",
  TIMEOUT: 30000,
};

export const APP_CONFIG = {
  NAME: process.env.NEXT_PUBLIC_APP_NAME || "WINIT Access Control System",
  VERSION: process.env.NEXT_PUBLIC_APP_VERSION || "1.0.0",
  SESSION_TIMEOUT: Number(process.env.NEXT_PUBLIC_SESSION_TIMEOUT) || 120,
  CHALLENGE_TIMEOUT: Number(process.env.NEXT_PUBLIC_CHALLENGE_TIMEOUT) || 5,
};
