/**
 * Crypto utilities for authentication
 */

/**
 * Generate challenge code in format: yyyyMMddHHmmss
 * @returns Challenge code string
 */
export function generateChallengeCode(): string {
  const now = new Date()
  const year = now.getFullYear()
  const month = String(now.getMonth() + 1).padStart(2, '0')
  const day = String(now.getDate()).padStart(2, '0')
  const hours = String(now.getHours()).padStart(2, '0')
  const minutes = String(now.getMinutes()).padStart(2, '0')
  const seconds = String(now.getSeconds()).padStart(2, '0')
  
  return `${year}${month}${day}${hours}${minutes}${seconds}`
}

/**
 * Hash text using SHA256
 * @param text Text to hash
 * @returns Base64 encoded hash
 */
export async function sha256Hash(text: string): Promise<string> {
  if (typeof window === 'undefined') {
    // Server-side: use Node.js crypto
    const crypto = await import('crypto')
    const hash = crypto.createHash('sha256')
    hash.update(text)
    return hash.digest('base64')
  } else {
    // Client-side: use Web Crypto API
    const encoder = new TextEncoder()
    const data = encoder.encode(text)
    const hashBuffer = await crypto.subtle.digest('SHA-256', data)
    const hashArray = Array.from(new Uint8Array(hashBuffer))
    const hashBase64 = btoa(String.fromCharCode(...hashArray))
    return hashBase64
  }
}

/**
 * Encrypt password with challenge code
 * @param password Plain text password
 * @param challengeCode Challenge code
 * @returns SHA256 hash of password + challenge
 */
export async function encryptPasswordWithChallenge(
  password: string, 
  challengeCode: string
): Promise<string> {
  const passwordWithChallenge = password + challengeCode
  return sha256Hash(passwordWithChallenge)
}

/**
 * Validate challenge code (must be within 5 minutes)
 * @param challengeCode Challenge code to validate
 * @param maxAgeMinutes Maximum age in minutes (default: 5)
 * @returns true if valid, false otherwise
 */
export function validateChallengeCode(
  challengeCode: string, 
  maxAgeMinutes: number = 5
): boolean {
  try {
    // Parse challenge code: yyyyMMddHHmmss
    const year = parseInt(challengeCode.substring(0, 4))
    const month = parseInt(challengeCode.substring(4, 6)) - 1 // 0-indexed
    const day = parseInt(challengeCode.substring(6, 8))
    const hours = parseInt(challengeCode.substring(8, 10))
    const minutes = parseInt(challengeCode.substring(10, 12))
    const seconds = parseInt(challengeCode.substring(12, 14))
    
    const challengeDate = new Date(year, month, day, hours, minutes, seconds)
    const now = new Date()
    const ageInMinutes = (now.getTime() - challengeDate.getTime()) / (1000 * 60)
    
    return ageInMinutes >= 0 && ageInMinutes <= maxAgeMinutes
  } catch {
    return false
  }
}