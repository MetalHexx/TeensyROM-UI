import { ResponseError, ProblemDetails } from '@teensyrom-nx/data-access/api-client';

/**
 * Extracts a user-friendly error message from an API response.
 *
 * Attempts to parse the response body as ProblemDetails and extract the title,
 * which is the user-friendly error message from the API.
 * Falls back to the generic error message if parsing fails.
 *
 * @param error - The error thrown by the API client
 * @param fallbackMessage - The fallback message to use if extraction fails
 * @returns A user-friendly error message
 */
export async function extractErrorMessage(
  error: unknown,
  fallbackMessage: string
): Promise<string> {
  // If it's a ResponseError from the OpenAPI client, try to extract ProblemDetails first
  if (error instanceof ResponseError && error.response) {
    try {
      const contentType = error.response.headers.get('content-type') || '';
      console.log('🔍 Error response content-type:', contentType);

      // Only attempt to parse JSON responses
      if (
        contentType.includes('application/json') ||
        contentType.includes('application/problem+json')
      ) {
        const bodyText = await error.response.clone().text();
        console.log('🔍 Error response body:', bodyText);

        if (bodyText) {
          const problemDetails = JSON.parse(bodyText) as ProblemDetails;
          console.log('🔍 Parsed ProblemDetails:', problemDetails);

          // ProblemDetails title is the user-friendly message
          if (problemDetails.title) {
            console.log('✅ Using ProblemDetails.title:', problemDetails.title);
            return problemDetails.title;
          }

          // Fall back to detail if title is not available
          if (problemDetails.detail) {
            console.log('✅ Using ProblemDetails.detail:', problemDetails.detail);
            return problemDetails.detail;
          }
        }
      }
    } catch (parseError) {
      // If parsing fails, we'll fall through to the fallback message
      console.error('❌ Failed to parse error response body:', parseError);
    }
  }

  // If it's a standard Error object with a message, return it
  if (error instanceof Error && error.message) {
    console.log('⚠️ Using Error.message:', error.message);
    return error.message;
  }

  // Return the fallback message as a last resort
  console.log('⚠️ Using fallback message:', fallbackMessage);
  return fallbackMessage;
}
