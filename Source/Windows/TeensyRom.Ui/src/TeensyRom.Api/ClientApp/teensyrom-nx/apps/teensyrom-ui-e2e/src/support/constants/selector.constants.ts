/**
 * UI Component Selectors Constants
 *
 * Centralized selector definitions for all UI components used in E2E tests.
 * Single source of truth for DOM selection - update here, affects all tests.
 */

// Alert/notification display
export const ALERT_SELECTORS = {
  container: '.alert-display',
  icon: '.alert-icon',
  message: '.alert-message',
  dismissButton: '.alert-display button[aria-label="Dismiss alert"]',
  messageInContainer: '.alert-display .alert-message',
  iconInContainer: '.alert-display .alert-icon',
} as const;

// Busy dialog (loading indicator)
export const BUSY_DIALOG_SELECTORS = {
  content: '.busy-dialog-content',
  backdrop: '.busy-dialog-backdrop',
  spinner: '.busy-dialog-spinner',
} as const;

// Device view page layout
export const DEVICE_VIEW_SELECTORS = {
  container: '[data-testid="device-view"]',
  deviceList: '[data-testid="device-list"]',
  emptyStateMessage: '[data-testid="empty-state-message"]',
  loadingIndicator: '.busy-dialog-content',
  refreshButton: 'button:contains("Refresh Devices")',
} as const;

// Device card and internal elements
export const DEVICE_CARD_SELECTORS = {
  card: '[data-testid="device-card"]',
  powerButton: '[data-testid="device-power-button"]',
  deviceInfo: '[data-testid="device-info"]',
  deviceStorage: '[data-testid="device-storage"]',
  idLabel: '[data-testid="device-id-label"]',
  firmwareLabel: '[data-testid="device-firmware-label"]',
  portLabel: '[data-testid="device-port-label"]',
  stateLabel: '[data-testid="device-state-label"]',
  compatibleLabel: '[data-testid="device-compatible-label"]',
  usbStorageStatus: '[data-testid="usb-storage-status"]',
  sdStorageStatus: '[data-testid="sd-storage-status"]',
} as const;

// Common button patterns
export const BUTTON_SELECTORS = {
  byText: (text: string) => `button:contains("${text}")`,
  closeButton: 'button[aria-label="Close"]',
  dismissButton: 'button[aria-label="Dismiss"]',
  confirmButton: 'button[aria-label="Confirm"]',
  acceptButton: 'button[aria-label="Accept"]',
} as const;

// Helper functions for common patterns
export function getAlertMessageSelector(): string {
  return ALERT_SELECTORS.messageInContainer;
}

export function getAlertIconSelector(): string {
  return ALERT_SELECTORS.iconInContainer;
}

export function getDeviceCardByIndexSelector(index: number): string {
  return `${DEVICE_CARD_SELECTORS.card}:eq(${index})`;
}

export function getByTestId(testId: string): string {
  return `[data-testid="${testId}"]`;
}

export function getByClass(className: string): string {
  return `.${className}`;
}

// Consolidated export for bulk imports
export const UI_SELECTORS = {
  alert: ALERT_SELECTORS,
  busyDialog: BUSY_DIALOG_SELECTORS,
  deviceView: DEVICE_VIEW_SELECTORS,
  deviceCard: DEVICE_CARD_SELECTORS,
  buttons: BUTTON_SELECTORS,
} as const;
