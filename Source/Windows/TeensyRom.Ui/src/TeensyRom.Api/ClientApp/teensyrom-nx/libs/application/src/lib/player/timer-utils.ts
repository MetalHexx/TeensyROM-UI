/**
 * Timer utility functions for parsing playLength metadata.
 *
 * Supports standard time formats:
 * - "MM:SS" format (e.g., "3:45" → 225000ms)
 * - "H:MM:SS" format (e.g., "1:02:30" → 3750000ms)
 *
 * Invalid formats return 0ms for graceful handling.
 */

/**
 * Parse playLength string to milliseconds.
 *
 * @param playLength - Time string in "MM:SS" or "H:MM:SS" format
 * @returns Duration in milliseconds, or 0 if invalid format
 *
 * @example
 * parsePlayLength("3:45")      // → 225000 (3 minutes 45 seconds)
 * parsePlayLength("1:02:30")   // → 3750000 (1 hour 2 minutes 30 seconds)
 * parsePlayLength("invalid")   // → 0 (invalid format)
 * parsePlayLength("")          // → 0 (empty string)
 */
export function parsePlayLength(playLength: string): number {
  // Handle empty or invalid input
  if (!playLength || typeof playLength !== 'string') {
    return 0;
  }

  // Trim whitespace
  const trimmed = playLength.trim();
  if (!trimmed) {
    return 0;
  }

  // Split by colon
  const parts = trimmed.split(':');

  // Invalid format (less than 2 parts or more than 3 parts)
  if (parts.length < 2 || parts.length > 3) {
    return 0;
  }

  try {
    // Parse parts as integers
    const numbers = parts.map((part) => {
      // Check if part contains only digits (no letters or special characters)
      if (!/^\d+$/.test(part.trim())) {
        throw new Error('Invalid number format');
      }
      const num = parseInt(part, 10);
      // Check for valid number
      if (isNaN(num) || num < 0) {
        throw new Error('Invalid number');
      }
      return num;
    });

    let totalSeconds = 0;

    if (numbers.length === 2) {
      // MM:SS format
      const [minutes, seconds] = numbers;
      totalSeconds = minutes * 60 + seconds;
    } else {
      // H:MM:SS format
      const [hours, minutes, seconds] = numbers;
      totalSeconds = hours * 3600 + minutes * 60 + seconds;
    }

    // Convert seconds to milliseconds
    return totalSeconds * 1000;
  } catch {
    // Invalid number parsing
    return 0;
  }
}
