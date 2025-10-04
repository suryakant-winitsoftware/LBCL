/**
 * Generate TOTP (Time-based One-Time Password) for force check-in validation
 * This matches the mobile app's implementation exactly
 *
 * @param secret - The encrypted_password from emp table (ASCII string)
 * @param timestamp - Current timestamp in milliseconds (Date.now())
 * @param period - Time period in seconds (default: 120 = 2 minutes)
 * @returns 6-digit OTP string
 */
export function generateTOTP(
  secret: string,
  timestamp: number = Date.now(),
  period: number = 120
): string {
  try {
    // Convert secret (ASCII string) to bytes
    const keyBytes: number[] = []
    for (let i = 0; i < secret.length; i++) {
      keyBytes.push(secret.charCodeAt(i))
    }

    // Calculate time counter (number of periods since epoch)
    let timeCounter = Math.floor(timestamp / 1000 / period)

    // Convert counter to 8-byte array (big-endian)
    const counterBytes = new Array(8).fill(0)
    for (let i = 7; i >= 0; i--) {
      counterBytes[i] = timeCounter & 0xff
      timeCounter = timeCounter >>> 8
    }

    // Simple hash calculation (same algorithm as mobile app)
    let hash = 0
    for (let i = 0; i < keyBytes.length; i++) {
      hash = ((hash << 5) - hash) + keyBytes[i] + counterBytes[i % 8]
      hash = hash & hash // Convert to 32bit integer
    }

    // Get offset from last 4 bits of hash
    const offset = hash & 0xf

    // Extract 4 bytes starting at offset and convert to 6-digit OTP
    const code = (Math.abs(hash) % 1000000).toString().padStart(6, '0')

    return code
  } catch (error) {
    console.error('[TOTP] Error generating OTP:', error)
    throw error
  }
}

/**
 * Verify TOTP with window tolerance
 * Checks current time window and adjacent windows (±1 = ±2 minutes)
 *
 * @param token - The 6-digit OTP entered by user
 * @param secret - The encrypted_password from emp table
 * @param window - Number of periods to check before/after current (default: 1)
 * @returns boolean - true if token is valid
 */
export function verifyTOTP(
  token: string,
  secret: string,
  window: number = 1
): boolean {
  try {
    if (!secret || !token) {
      return false
    }

    const now = Date.now()
    const period = 120 // 2 minutes (120 seconds)

    // Check current time window and adjacent windows
    for (let i = -window; i <= window; i++) {
      const timestamp = now + (i * period * 1000) // Convert to milliseconds
      const otp = generateTOTP(secret, timestamp, period)

      if (otp === token) {
        return true
      }
    }

    return false
  } catch (error) {
    console.error('[TOTP] Error verifying:', error)
    return false
  }
}

/**
 * Get time remaining until current OTP expires
 * @returns Number of seconds remaining
 */
export function getTimeRemaining(): number {
  const now = Date.now()
  const period = 120000 // 2 minutes in milliseconds
  const expiresAt = Math.ceil(now / period) * period
  return Math.floor((expiresAt - now) / 1000)
}

/**
 * Get OTP expiry information
 * @param secret - The encrypted_password from emp table
 * @returns Object with OTP and expiry info
 */
export function getOTPWithExpiry(secret: string) {
  const otp = generateTOTP(secret)
  const secondsRemaining = getTimeRemaining()

  return {
    otp,
    expiresIn: secondsRemaining,
    validityWindow: '6 minutes (±2 minutes from current)'
  }
}
