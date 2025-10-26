/**
 * Alert constants for reusable Cypress assertions.
 *
 * Provides severity values aligned with the application AlertSeverity enum
 * and the corresponding Material icon names rendered in the UI.
 */

export const ALERT_SEVERITY = {
  SUCCESS: 'success',
  ERROR: 'error',
  WARNING: 'warning',
  INFO: 'info',
} as const;

export const ALERT_ICON = {
  SUCCESS: 'check_circle',
  ERROR: 'error',
  WARNING: 'warning',
  INFO: 'info',
} as const;

export type AlertSeverityKey = keyof typeof ALERT_SEVERITY;
export type AlertSeverityValue = (typeof ALERT_SEVERITY)[AlertSeverityKey];
export type AlertIconKey = keyof typeof ALERT_ICON;
export type AlertIconValue = (typeof ALERT_ICON)[AlertIconKey];

export const ALERT_ICON_BY_SEVERITY: Record<AlertSeverityValue, AlertIconValue> = {
  [ALERT_SEVERITY.SUCCESS]: ALERT_ICON.SUCCESS,
  [ALERT_SEVERITY.ERROR]: ALERT_ICON.ERROR,
  [ALERT_SEVERITY.WARNING]: ALERT_ICON.WARNING,
  [ALERT_SEVERITY.INFO]: ALERT_ICON.INFO,
} as const;
