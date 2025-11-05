/**
 * Application Routes Constants
 *
 * Centralized definitions for all application routes.
 * Single source of truth for app navigation paths - update here, affects all tests.
 */

export const APP_ROUTES = {
  root: '/',
  devices: '/devices',
  player: '/player',
} as const;

export const ROUTE_NAMES = {
  DEVICES_VIEW: 'devices',
  PLAYER_VIEW: 'player',
  ROOT: 'root',
} as const;

/**
 * Helper function to navigate to a specific route
 * @param route The route path from APP_ROUTES
 * @returns Full route path
 */
export function getRoute(route: string): string {
  return route;
}
