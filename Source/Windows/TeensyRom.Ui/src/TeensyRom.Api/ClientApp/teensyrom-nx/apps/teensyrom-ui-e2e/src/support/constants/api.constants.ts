/**
 * E2E API Constants - Single source of truth for API endpoints
 * Note: TeensyROM API has NO /api prefix - routes go directly to base URL
 */

export const API_CONFIG = {
  BASE_URL: 'http://localhost:5168',
} as const;

/**
 * Build a ProblemDetails error response for API interceptors
 * @param statusCode HTTP status code (e.g., 404, 500)
 * @param title User-friendly error message
 * @param detail Optional technical details
 * @returns Object ready for cy.intercept() req.reply()
 */
export function createProblemDetailsResponse(
  statusCode: number,
  title: string,
  detail?: string
) {
  return {
    statusCode,
    headers: {
      'content-type': 'application/problem+json',
    },
    body: {
      type: `https://tools.ietf.org/html/rfc9110#section-${statusCode === 404 ? '15.5.5' : statusCode === 500 ? '15.6.1' : '15.5.5'}`,
      title,
      status: statusCode,
      ...(detail && { detail }),
    },
  };
}
