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
 * Click the alert dismiss button with enhanced error reporting.
 *
 * @example
 * dismissAlert();
 */
export function dismissAlert(): void {
  const startTime = Date.now();
  cy.log('⏳ Looking for alert to dismiss');

  // Find the first visible alert and dismiss it
  cy.get(ALERT_SELECTORS.container, { timeout: 10000 })
    .should('be.visible')
    .then(($alerts) => {
      if ($alerts.length === 0) {
        const elapsedTime = Date.now() - startTime;
        const errorMsg = [
          `❌ No alerts found to dismiss after ${elapsedTime}ms`,
          `💡 Common causes:`,
          `  • No alert was triggered by the application`,
          `  • Alert auto-dismissed before we could interact with it`,
          `  • Alert selector is incorrect or has changed`,
          `  • Alert is in a different DOM state`,
          `🔧 Debugging suggestions:`,
          `  • Verify an alert should be displayed at this point`,
          `  • Check if alerts auto-dismiss quickly`,
          `  • Use dismissAlertWithDebug() to see DOM structure`,
          `  • Wait for alert to appear first: verifyAlertVisible()`,
        ].join('\n');

        assert.fail(errorMsg);
      }

      cy.log(`✅ Found ${$alerts.length} alert(s) to dismiss`);
    });

  cy.get(ALERT_SELECTORS.container, { timeout: 10000 }).should('be.visible').first();

  cy.get(ALERT_SELECTORS.container)
    .first()
    .within(() => {
      // Find the dismiss button with explicit visibility and state checks
      // This uses the exact selector that matches the DOM structure from alert-display.component.html
      cy.get('button[aria-label="Dismiss alert"]', { timeout: 5000 })
        .should('be.visible')
        .and('not.be.disabled')
        .and('be.enabled')
        .then(() => {
          cy.log('✅ Found dismiss button, attempting to click');
        });

      cy.get('button[aria-label="Dismiss alert"]').click();

      cy.then(() => {
        const elapsedTime = Date.now() - startTime;
        cy.log(`✅ Alert dismissed successfully in ${elapsedTime}ms`);
      });
    });
}

/**
 * Debug version of dismissAlert that logs DOM structure for troubleshooting.
 * Use this only when the regular dismissAlert() function fails.
 *
 * @example
 * dismissAlertWithDebug();
 */
export function dismissAlertWithDebug(): void {
  cy.get(ALERT_SELECTORS.container)
    .should('be.visible')
    .first()
    .within(() => {
      // Debug: log the alert DOM structure to understand what's happening
      cy.get('.alert-display').then(($alert) => {
        cy.log('Alert DOM HTML:', $alert.html());
        cy.log('Alert DOM text:', $alert.text());

        // Log all buttons found within the alert
        cy.get('button').then(($buttons) => {
          cy.log(`Found ${$buttons.length} button(s) in alert`);
          $buttons.each((index, button) => {
            cy.log(`Button ${index}:`, button.outerHTML);
          });
        });

        // Now try to dismiss
        cy.get('button[aria-label="Dismiss alert"]')
          .should('be.visible')
          .and('not.be.disabled')
          .and('be.enabled')
          .click();
      });
    });
}

/**
 * Clear all alerts by clicking all dismiss buttons.
 * Useful for test cleanup to ensure a clean state.
 *
 * @example
 * clearAllAlerts();
 */
export function clearAllAlerts(): void {
  cy.get('body').then(($body) => {
    if ($body.find(ALERT_SELECTORS.dismissButton).length > 0) {
      cy.get(ALERT_SELECTORS.dismissButton).each(($button) => {
        cy.wrap($button).should('be.visible').and('not.be.disabled').and('be.enabled').click();
      });
    }
  });
}

/**
 * Wait for the alert to auto-dismiss by asserting it disappears within the timeout.
 *
 * @param timeoutMs Total time in milliseconds to wait for dismissal (default 3500ms).
 *
 * @example
 * waitForAlertAutoDismiss();
 */
export function waitForAlertAutoDismiss(timeoutMs = 5500): void {
  cy.get(ALERT_SELECTORS.container).should('exist');
  cy.get(ALERT_SELECTORS.container, { timeout: timeoutMs }).should('not.exist');
}

/**
 * Assert that the alert container has been dismissed with enhanced error reporting.
 *
 * @example
 * verifyAlertDismissed();
 */
export function verifyAlertDismissed(): void {
  const startTime = Date.now();
  const timeout = 6000;
  cy.log(`⏳ Waiting for alert to be dismissed (timeout: ${timeout}ms)`);

  // Wait for the alert to be removed from DOM with a longer timeout
  // This accounts for manual dismissal which might need more time
  cy.get(ALERT_SELECTORS.container, { timeout })
    .should('not.exist')
    .then(() => {
      const elapsedTime = Date.now() - startTime;
      cy.log(`✅ Alert dismissed successfully in ${elapsedTime}ms`);
    });
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
