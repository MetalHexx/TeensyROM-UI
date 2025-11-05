/**
 * Enhanced timing utilities for E2E testing with better error reporting
 * Provides consistent timeout handling and debugging information across the test suite
 */

import { TIMEOUTS } from '../constants/test.constants';

/**
 * Enhanced timeout options for timing operations
 */
export interface EnhancedTimeoutOptions {
  /** Custom timeout duration in milliseconds */
  timeout?: number;
  /** Operation description for error messages */
  operation?: string;
  /** Additional context for debugging */
  context?: string;
  /** Whether to log timing information */
  logTiming?: boolean;
  /** Common causes of timeout for this operation */
  commonCauses?: string[];
  /** Debugging suggestions for this operation */
  debuggingTips?: string[];
}

/**
 * Default common causes for timeout operations
 */
const DEFAULT_COMMON_CAUSES = [
  'Network connectivity issues',
  'API endpoint not responding',
  'Operation taking longer than expected',
  'System resources are constrained',
  'Browser performance issues',
];

/**
 * Default debugging suggestions for timeout operations
 */
const DEFAULT_DEBUGGING_TIPS = [
  'Check network connectivity and API status',
  'Increase timeout duration if operation is legitimately slow',
  'Verify the operation should actually be happening',
  'Monitor browser devtools for network activity',
  'Check system resources and browser performance',
];

/**
 * Creates a timing tracker for enhanced timeout operations
 */
export class TimingTracker {
  private startTime: number;
  private operation: string;
  public timeout: number;
  private logTiming: boolean;

  constructor(options: EnhancedTimeoutOptions = {}) {
    this.startTime = Date.now();
    this.operation = options.operation || 'Unknown operation';
    this.timeout = options.timeout || TIMEOUTS.DEFAULT;
    this.logTiming = options.logTiming ?? true;

    if (this.logTiming) {
      cy.log(`⏳ Starting operation: ${this.operation}`);
      cy.log(`⏱️ Timeout: ${this.timeout}ms`);
      if (options.context) {
        cy.log(`📝 Context: ${options.context}`);
      }
    }
  }

  /**
   * Mark the operation as completed successfully
   */
  complete(additionalInfo?: string): number {
    const elapsedTime = Date.now() - this.startTime;
    if (this.logTiming) {
      cy.log(`✅ ${this.operation} completed successfully in ${elapsedTime}ms`);
      if (additionalInfo) {
        cy.log(`ℹ️ ${additionalInfo}`);
      }
    }
    return elapsedTime;
  }

  /**
   * Mark the operation as failed with enhanced error reporting
   */
  fail(error: Error | string, options: { context?: string; suggestion?: string } = {}): never {
    const elapsedTime = Date.now() - this.startTime;
    const baseMessage = typeof error === 'string' ? error : error.message;

    const enhancedMessage = [
      `❌ ${this.operation} failed after ${elapsedTime}ms (timeout: ${this.timeout}ms)`,
      `Error: ${baseMessage}`,
      options.context ? `Context: ${options.context}` : null,
      '',
      '💡 Common causes:',
      ...DEFAULT_COMMON_CAUSES.map((cause) => `  • ${cause}`),
      '',
      '🔧 Debugging suggestions:',
      ...DEFAULT_DEBUGGING_TIPS.map((tip) => `  • ${tip}`),
      options.suggestion ? `  • ${options.suggestion}` : null,
    ]
      .filter(Boolean)
      .join('\n');

    assert.fail(enhancedMessage);
  }

  /**
   * Get the elapsed time so far
   */
  getElapsedTime(): number {
    return Date.now() - this.startTime;
  }

  /**
   * Check if we're approaching timeout (80% of timeout duration)
   */
  isApproachingTimeout(): boolean {
    return this.getElapsedTime() > this.timeout * 0.8;
  }

  /**
   * Log a warning if approaching timeout
   */
  logTimeoutWarning(): void {
    if (this.isApproachingTimeout() && this.logTiming) {
      const remaining = this.timeout - this.getElapsedTime();
      cy.log(`⚠️ ${this.operation} is taking longer than expected (${remaining}ms remaining)`);
    }
  }
}

/**
 * Creates an enhanced Cypress wait with better error reporting
 * @param alias Cypress alias to wait for
 * @param options Enhanced timeout options
 */
export function enhancedWait(alias: string, options: EnhancedTimeoutOptions = {}): void {
  const tracker = new TimingTracker({
    operation: options.operation || `Waiting for @${alias}`,
    timeout: options.timeout,
    context: options.context,
    logTiming: options.logTiming,
  });

  // Log warning if approaching timeout
  const warningInterval = setInterval(() => {
    tracker.logTimeoutWarning();
  }, 1000);

  cy.wait(`@${alias}`, { timeout: tracker.timeout }).then((xhr) => {
    clearInterval(warningInterval);
    tracker.complete(
      `Request completed: ${xhr?.request?.method || 'Unknown'} ${
        xhr?.request?.url || 'Unknown URL'
      }`
    );

    // Check for error responses
    if (xhr?.response?.statusCode && xhr.response.statusCode >= 400) {
      const errorInfo = [
        `⚠️ Request completed with error status: ${xhr.response.statusCode}`,
        `Response: ${JSON.stringify(xhr.response.body)}`,
        '',
        'This might indicate:',
        '  • API endpoint returned an error',
        '  • Request parameters are invalid',
        '  • Resource not found or not accessible',
        '  • Server-side processing error',
      ].join('\n');

      cy.log(errorInfo);
    }
  });
}

/**
 * Creates an enhanced element wait with better error reporting
 * @param selector CSS selector for the element
 * @param options Enhanced timeout options
 */
export function enhancedElementWait(
  selector: string,
  options: EnhancedTimeoutOptions = {}
): Cypress.Chainable<JQuery<HTMLElement>> {
  const tracker = new TimingTracker({
    operation: options.operation || `Waiting for element: ${selector}`,
    timeout: options.timeout,
    context: options.context,
    logTiming: options.logTiming,
  });

  // Log warning if approaching timeout
  const warningInterval = setInterval(() => {
    tracker.logTimeoutWarning();
  }, 1000);

  return cy.get(selector, { timeout: tracker.timeout }).then(($element) => {
    clearInterval(warningInterval);
    tracker.complete(`Element found: ${selector}`);
    return $element;
  });
}

/**
 * Creates a retry wrapper for operations that might fail initially
 * Note: Simplified implementation to avoid TypeScript complexities with Cypress chains
 * @param operation Function to retry
 * @param options Retry configuration
 */
export function retryOperation<T>(
  operation: () => Cypress.Chainable<T>,
  options: {
    maxRetries?: number;
    retryDelay?: number;
    operationName?: string;
    onRetry?: (attempt: number, error: Error) => void;
  } = {}
): Cypress.Chainable<T> {
  const { maxRetries = 3, operationName = 'Operation' } = options;

  let attempts = 0;

  // For now, just attempt the operation once without retry logic
  // TODO: Implement proper retry mechanism that works with Cypress and TypeScript
  if (attempts > 1) {
    cy.log(`🔄 ${operationName} retry attempt ${attempts - 1}/${maxRetries}`);
  }

  attempts++;

  return operation();
}

/**
 * Utility function to format milliseconds into human-readable time
 */
export function formatDuration(ms: number): string {
  if (ms < 1000) {
    return `${ms}ms`;
  } else if (ms < 60000) {
    return `${(ms / 1000).toFixed(1)}s`;
  } else {
    const minutes = Math.floor(ms / 60000);
    const seconds = Math.floor((ms % 60000) / 1000);
    return `${minutes}m ${seconds}s`;
  }
}

/**
 * Performance tracking utility to monitor operation timing
 */
export class PerformanceTracker {
  private operations: Map<string, number[]> = new Map();

  /**
   * Record the duration of an operation
   */
  recordOperation(name: string, duration: number): void {
    if (!this.operations.has(name)) {
      this.operations.set(name, []);
    }
    const operationArray = this.operations.get(name);
    if (operationArray) {
      operationArray.push(duration);
    }
  }

  /**
   * Get statistics for an operation
   */
  getStats(name: string): { count: number; avg: number; min: number; max: number } | null {
    const operationDurations = this.operations.get(name);
    if (!operationDurations || operationDurations.length === 0) {
      return null;
    }

    return {
      count: operationDurations.length,
      avg: operationDurations.reduce((a, b) => a + b, 0) / operationDurations.length,
      min: Math.min(...operationDurations),
      max: Math.max(...operationDurations),
    };
  }

  /**
   * Log performance report for all tracked operations
   */
  logReport(): void {
    cy.log('📊 Performance Report:');

    for (const [name] of this.operations.entries()) {
      const stats = this.getStats(name);
      if (stats) {
        cy.log(`  ${name}:`);
        cy.log(`    Count: ${stats.count}`);
        cy.log(`    Average: ${formatDuration(stats.avg)}`);
        cy.log(`    Range: ${formatDuration(stats.min)} - ${formatDuration(stats.max)}`);
      }
    }
  }

  /**
   * Clear all tracked operations
   */
  clear(): void {
    this.operations.clear();
  }
}
