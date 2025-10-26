import { ALERT_SELECTORS } from '../constants/selector.constants';
import {
  ALERT_ICON,
  ALERT_ICON_BY_SEVERITY,
  ALERT_SEVERITY,
  AlertIconKey,
  AlertIconValue,
  AlertSeverityKey,
  AlertSeverityValue,
} from '../constants/alert.constants';

type AlertIconParam = AlertIconKey | AlertIconValue;
type AlertSeverityParam = AlertSeverityKey | AlertSeverityValue;

/**
 * Assert that the alert container is currently visible.
 *
 * @example
 * verifyAlertVisible();
 */
export function verifyAlertVisible(): void {
  cy.get(ALERT_SELECTORS.container).should('be.visible');
}

/**
 * Assert that the alert container is not present in the DOM.
 *
 * @example
 * verifyAlertNotVisible();
 */
export function verifyAlertNotVisible(): void {
  cy.get(ALERT_SELECTORS.container).should('not.exist');
}

/**
 * Assert that the alert message contains the provided text (partial match).
 *
 * @param message Text expected within the alert message.
 *
 * @example
 * verifyAlertMessage('No devices found.');
 */
export function verifyAlertMessage(message: string): void {
  cy.get(ALERT_SELECTORS.message).should('contain.text', message);
}

/**
 * Assert that the alert message matches the provided text exactly once trimmed.
 *
 * @param message Exact text the alert message should display.
 *
 * @example
 * verifyAlertMessageExact('No devices found.');
 */
export function verifyAlertMessageExact(message: string): void {
  cy.get(ALERT_SELECTORS.message).should(($message) => {
    expect($message.text().trim()).to.eq(message);
  });
}

/**
 * Assert that the alert message does not contain the provided text.
 *
 * @param text Text that must not appear in the alert message.
 *
 * @example
 * verifyAlertMessageDoesNotContain('Stack trace');
 */
export function verifyAlertMessageDoesNotContain(text: string): void {
  cy.get(ALERT_SELECTORS.message).should(($message) => {
    expect($message.text()).not.to.include(text);
  });
}

/**
 * Assert that the rendered Material icon matches the expected name.
 *
 * @param icon Material icon name or key from {@link ALERT_ICON}.
 *
 * @example
 * verifyAlertIcon(ALERT_ICON.ERROR);
 * verifyAlertIcon('ERROR');
 */
export function verifyAlertIcon(icon: AlertIconParam): void {
  const iconName = resolveIconValue(icon);
  cy.get(ALERT_SELECTORS.icon).should('contain.text', iconName);
}

/**
 * Assert that alert severity (icon) matches the expected severity.
 *
 * @param severity Severity value or key from {@link ALERT_SEVERITY}.
 *
 * @example
 * verifyAlertSeverity(ALERT_SEVERITY.ERROR);
 * verifyAlertSeverity('ERROR');
 */
export function verifyAlertSeverity(severity: AlertSeverityParam): void {
  const severityValue = resolveSeverityValue(severity);
  verifyAlertIcon(ALERT_ICON_BY_SEVERITY[severityValue]);
}

/**
 * Click the alert dismiss button.
 *
 * @example
 * dismissAlert();
 */
export function dismissAlert(): void {
  cy.get(ALERT_SELECTORS.dismissButton).click();
}

/**
 * Wait for the alert to auto-dismiss by asserting it disappears within the timeout.
 *
 * @param timeoutMs Total time in milliseconds to wait for dismissal (default 3500ms).
 *
 * @example
 * waitForAlertAutoDismiss();
 */
export function waitForAlertAutoDismiss(timeoutMs = 3500): void {
  cy.get(ALERT_SELECTORS.container).should('exist');
  cy.get(ALERT_SELECTORS.container, { timeout: timeoutMs }).should('not.exist');
}

/**
 * Assert that the alert container has been dismissed.
 *
 * @example
 * verifyAlertDismissed();
 */
export function verifyAlertDismissed(): void {
  verifyAlertNotVisible();
}

/**
 * Resolve a severity value when provided a key or value.
 */
function resolveSeverityValue(severity: AlertSeverityParam): AlertSeverityValue {
  if (isAlertSeverityKey(severity)) {
    return ALERT_SEVERITY[severity];
  }
  return severity;
}

/**
 * Resolve an icon value when provided a key or value.
 */
function resolveIconValue(icon: AlertIconParam): AlertIconValue {
  if (isAlertIconKey(icon)) {
    return ALERT_ICON[icon];
  }
  return icon;
}

function isAlertSeverityKey(value: unknown): value is AlertSeverityKey {
  return typeof value === 'string' && Object.prototype.hasOwnProperty.call(ALERT_SEVERITY, value);
}

function isAlertIconKey(value: unknown): value is AlertIconKey {
  return typeof value === 'string' && Object.prototype.hasOwnProperty.call(ALERT_ICON, value);
}

export {
  ALERT_SEVERITY,
  ALERT_ICON,
  ALERT_ICON_BY_SEVERITY,
  AlertSeverityKey,
  AlertSeverityValue,
  AlertIconKey,
  AlertIconValue,
};
