/**
 * UI Component Selectors Constants
 *
 * Centralized selector definitions for all UI components used in E2E tests.
 * Single source of truth for DOM selection - update here, affects all tests.
 */

// CSS classes for styling states
export const ICON_CLASSES = {
  highlighted: 'highlight',
  normal: 'normal',
} as const;

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
  powerButtonIcon: '[data-testid="device-power-button"] mat-icon',
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

// Device toolbar and button controls
export const DEVICE_TOOLBAR_SELECTORS = {
  container: '[data-testid="device-toolbar"]',
  indexAllButton: '[data-testid="toolbar-button-index-all"] button',
  refreshButton: '[data-testid="toolbar-button-refresh-devices"] button',
  resetButton: '[data-testid="toolbar-button-reset-devices"] button',
  pingButton: '[data-testid="toolbar-button-ping-devices"] button',
  indexAllContainer: '[data-testid="toolbar-button-index-all"]',
  refreshContainer: '[data-testid="toolbar-button-refresh-devices"]',
  resetContainer: '[data-testid="toolbar-button-reset-devices"]',
  pingContainer: '[data-testid="toolbar-button-ping-devices"]',
} as const;

// Player toolbar and controls (Deep Linking E2E tests)
export const PLAYER_TOOLBAR_SELECTORS = {
  toolbar: '[data-testid="player-toolbar"]',
  previousButton: 'button[aria-label="Previous File"]',
  playPauseButton: 'button[aria-label*="Pause"], button[aria-label*="Play"]',
  stopButton: 'button[aria-label="Stop Playback"]',
  nextButton: 'button[aria-label="Next File"]',
  randomButton: '[data-testid="random-launch-button"]',
  currentFileInfo: '[data-testid="current-file-info"]',
} as const;

// Directory files listing
export const DIRECTORY_FILES_SELECTORS = {
  directoryFilesContainer: 'lib-directory-files',
  fileListItem: (filePath: string) => `[data-item-path="${filePath}"]`,
  fileListItemsByPrefix: (pathPrefix: string) => `[data-item-path^="${pathPrefix}"]`,
} as const;

// Storage index buttons (indexing feature)
export const STORAGE_INDEX_BUTTON_SELECTORS = {
  usb: getByTestId('storage-index-button-usb'),
  sd: getByTestId('storage-index-button-sd'),
  byType: (storageType: 'usb' | 'sd') =>
    storageType === 'usb'
      ? getByTestId('storage-index-button-usb')
      : getByTestId('storage-index-button-sd'),
} as const;

// Busy dialog with generic selectors (reusable for multiple operations)
export const BUSY_DIALOG_GENERIC_SELECTORS = {
  container: getByTestId('busy-dialog-container'),
  message: getByTestId('busy-dialog-message'),
  backdrop: '.cdk-overlay-backdrop',
} as const;

// Common button patterns
export const BUTTON_SELECTORS = {
  byText: (text: string) => `button:contains("${text}")`,
  closeButton: 'button[aria-label="Close"]',
  dismissButton: 'button[aria-label="Dismiss"]',
  confirmButton: 'button[aria-label="Confirm"]',
  acceptButton: 'button[aria-label="Accept"]',
} as const;

// CSS classes
export const CSS_CLASSES = {
  DIMMED: 'dimmed',
  UNAVAILABLE: 'unavailable',
  ERROR_MESSAGE: 'error-message',
} as const;

// DOM attributes
export const DOM_ATTRIBUTES = {
  ROLE_ALERT: 'alert',
  BODY_TAG: 'body',
  DATA_TESTID: 'data-testid',
} as const;

// String constants
export const CONSTANTS = {
  ERROR_TEXT: 'error',
  DEFAULT_TIMEOUT: 5000,
} as const;

// Consolidated export for bulk imports
export const UI_SELECTORS = {
  alert: ALERT_SELECTORS,
  busyDialog: BUSY_DIALOG_SELECTORS,
  busyDialogGeneric: BUSY_DIALOG_GENERIC_SELECTORS,
  deviceView: DEVICE_VIEW_SELECTORS,
  deviceCard: DEVICE_CARD_SELECTORS,
  deviceToolbar: DEVICE_TOOLBAR_SELECTORS,
  playerToolbar: PLAYER_TOOLBAR_SELECTORS,
  directoryFiles: DIRECTORY_FILES_SELECTORS,
  storageIndexButton: STORAGE_INDEX_BUTTON_SELECTORS,
  buttons: BUTTON_SELECTORS,
  icons: ICON_CLASSES,
  css: CSS_CLASSES,
  dom: DOM_ATTRIBUTES,
  constants: CONSTANTS,
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

// Dialog messages (UI text content)
export const DIALOG_MESSAGES = {
  INDEXING: 'Indexing Storage',
  INDEXING_USB: 'Indexing USB Storage',
  INDEXING_SD: 'Indexing SD Storage',
} as const;
