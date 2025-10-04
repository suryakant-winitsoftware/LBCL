import { generateTOTP, verifyTOTP, getTimeRemaining as getTOTPTimeRemaining } from '@/utils/totp'

/**
 * OTP Service for generating and verifying time-based one-time passwords
 * Uses the employee's encrypted password as the secret key
 * Matches the mobile app implementation exactly
 */
class OTPService {
  // Configure OTP settings
  private readonly PERIOD = 120 // 2 minutes (120 seconds) time window

  /**
   * Generate OTP using the employee's encrypted password as secret
   * Uses custom TOTP algorithm that matches mobile app
   * @param encryptedPassword - The ENCRIPTED_PASSWORD from emp table (ASCII string)
   * @returns The generated 6-digit OTP
   */
  generateOTP(encryptedPassword: string): string {
    try {
      return generateTOTP(encryptedPassword, Date.now(), this.PERIOD)
    } catch (error) {
      console.error('Failed to generate OTP:', error)
      throw new Error('Failed to generate OTP')
    }
  }

  /**
   * Verify OTP with time window tolerance
   * Checks current time window and adjacent windows (±1 = ±2 minutes)
   * @param token - The 6-digit OTP token to verify
   * @param encryptedPassword - The ENCRIPTED_PASSWORD from emp table
   * @returns true if OTP is valid within the time window
   */
  verifyOTP(token: string, encryptedPassword: string): boolean {
    try {
      return verifyTOTP(token, encryptedPassword, 1)
    } catch (error) {
      console.error('Failed to verify OTP:', error)
      return false
    }
  }

  /**
   * Verify OTP with custom window tolerance
   * This allows checking if an OTP is valid within a larger time window
   * @param token - The OTP token to verify
   * @param encryptedPassword - The ENCRIPTED_PASSWORD from emp table
   * @param windowSteps - Number of periods to check before/after current (default 1 = ±2 min)
   * @returns true if OTP is valid within the extended time window
   */
  verifyOTPWithWindow(
    token: string,
    encryptedPassword: string,
    windowSteps: number = 1
  ): boolean {
    try {
      return verifyTOTP(token, encryptedPassword, windowSteps)
    } catch (error) {
      console.error('Failed to verify OTP with window:', error)
      return false
    }
  }

  /**
   * Get the remaining time until the current OTP expires
   * @returns Number of seconds remaining
   */
  getTimeRemaining(): number {
    return getTOTPTimeRemaining()
  }

  /**
   * Get the progress percentage of the current time window
   * @returns Progress as a percentage (0-100)
   */
  getTimeProgress(): number {
    const remaining = this.getTimeRemaining()
    return ((this.PERIOD - remaining) / this.PERIOD) * 100
  }
}

export const otpService = new OTPService()
