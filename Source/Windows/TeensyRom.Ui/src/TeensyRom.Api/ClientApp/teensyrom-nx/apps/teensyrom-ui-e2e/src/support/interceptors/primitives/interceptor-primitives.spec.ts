/**
 * Unit Tests for Interceptor Primitives
 *
 * Tests the core primitive functions to ensure they work correctly
 * and maintain expected behavior patterns.
 */

import { describe, it, beforeEach, afterEach, expect, vi } from 'vitest';
import {
  interceptSuccess,
  interceptError,
  interceptSequence,
  interceptEmpty,
  interceptNetworkError,
  type EndpointDefinition,
  type StatusCode,
  type CypressRequest
} from './interceptor-primitives';

// Mock Cypress global functions
interface CypressChainable {
  intercept(
    method: string,
    url: string,
    handler?: (req: unknown) => void
  ): CypressChainable;
  wait(alias: string): CypressChainable;
}

declare global {
  const cy: CypressChainable;
}

// Test endpoint definitions for consistent testing
const TEST_ENDPOINT: EndpointDefinition = {
  method: 'GET',
  pattern: '/api/test',
  alias: '@testEndpoint'
};

const ERROR_ENDPOINT: EndpointDefinition = {
  method: 'POST',
  pattern: '/api/error',
  alias: '@errorEndpoint'
};

const SEQUENCE_ENDPOINT: EndpointDefinition = {
  method: 'GET',
  pattern: '/api/sequence',
  alias: '@sequenceEndpoint'
};

// Mock data for testing
const mockSuccessData = { id: 1, name: 'Test Data', status: 'active' };
const mockSequenceData = [
  { step: 1, status: 'started' },
  { step: 2, status: 'progress', progress: 50 },
  { step: 3, status: 'completed' }
];

// Spy on cy.intercept calls
interface InterceptCall {
  method: string;
  pattern: string;
  handler: (req: CypressRequest) => void;
  alias?: string;
}

interface InterceptSpy {
  calls: InterceptCall[];
  implementation: (method: string, pattern: string, handler?: (req: CypressRequest) => void) => {
    as: (alias: string) => void;
  };
}

interface CySpy {
  intercept: (method: string, pattern: string, handler?: (req: CypressRequest) => void) => {
    as: (alias: string) => void;
  };
}

let interceptSpy: InterceptSpy;
let cySpy: CySpy;

describe('Interceptor Primitives', () => {
  beforeEach(() => {
    // Reset spy before each test
    interceptSpy = {
      calls: [],
      implementation: vi.fn().mockImplementation((method: string, pattern: string, handler?: (req: CypressRequest) => void) => {
        interceptSpy.calls.push({ method, pattern, handler });
        return {
          as: vi.fn().mockImplementation((alias: string) => {
            interceptSpy.calls[interceptSpy.calls.length - 1].alias = alias;
          })
        };
      })
    };

    // Mock cy.intercept
    cySpy = {
      intercept: interceptSpy.implementation
    };

    // Set up global cy mock
    (global as Record<string, unknown>).cy = cySpy;
  });

  afterEach(() => {
    // Clean up after each test
    vi.restoreAllMocks();
    (global as Record<string, unknown>).cy = undefined;
  });

  describe('interceptSuccess', () => {
    it('should create a basic success interceptor', () => {
      interceptSuccess(TEST_ENDPOINT);

      expect(interceptSpy.calls).to.have.length(1);
      expect(interceptSpy.calls[0].method).to.equal('GET');
      expect(interceptSpy.calls[0].pattern).to.equal('/api/test');
      expect(interceptSpy.calls[0].alias).to.equal('@testEndpoint');
      expect(interceptSpy.calls[0].handler).to.be.a('function');
    });

    it('should handle custom data', () => {
      interceptSuccess(TEST_ENDPOINT, mockSuccessData);

      const call = interceptSpy.calls[0];
      expect(call.handler).to.be.a('function');

      // Simulate request and verify response structure
      const mockRequest: CypressRequest = {
        reply: (response) => {
          expect(response.statusCode).to.equal(200);
          expect(response.headers['content-type']).to.equal('application/json');
          expect(response.body).to.deep.equal(mockSuccessData);
          expect(response.delay).to.equal(0);
        }
      };

      call.handler(mockRequest);
    });

    it('should handle custom delay', () => {
      const delay = 1500;
      interceptSuccess(TEST_ENDPOINT, mockSuccessData, delay);

      const mockRequest: CypressRequest = {
        reply: (response) => {
          expect(response.delay).to.equal(delay);
        }
      };

      interceptSpy.calls[0].handler(mockRequest);
    });

    it('should handle custom headers', () => {
      const customHeaders = { 'x-custom-header': 'test-value' };
      interceptSuccess(TEST_ENDPOINT, mockSuccessData, 0, customHeaders);

      const mockRequest: CypressRequest = {
        reply: (response) => {
          expect(response.headers['x-custom-header']).to.equal('test-value');
          expect(response.headers['cache-control']).to.equal('no-cache');
        }
      };

      interceptSpy.calls[0].handler(mockRequest);
    });

    it('should use empty object as default data', () => {
      interceptSuccess(TEST_ENDPOINT);

      const mockRequest: CypressRequest = {
        reply: (response) => {
          expect(response.body).to.deep.equal({});
        }
      };

      interceptSpy.calls[0].handler(mockRequest);
    });
  });

  describe('interceptError', () => {
    it('should create a basic error interceptor with default values', () => {
      interceptError(ERROR_ENDPOINT);

      expect(interceptSpy.calls).to.have.length(1);
      expect(interceptSpy.calls[0].method).to.equal('POST');
      expect(interceptSpy.calls[0].pattern).to.equal('/api/error');
      expect(interceptSpy.calls[0].alias).to.equal('@errorEndpoint');
    });

    it('should handle custom status code', () => {
      const statusCode = 404;
      interceptError(ERROR_ENDPOINT, statusCode);

      const mockRequest: CypressRequest = {
        reply: (response) => {
          expect(response.statusCode).to.equal(statusCode);
          expect(response.headers['content-type']).to.equal('application/problem+json');
          expect(response.body.status).to.equal(statusCode);
          expect(response.body.title).to.equal('Not Found');
          expect(response.body.type).to.include('rfc9110#section-11.4.1');
        }
      };

      interceptSpy.calls[0].handler(mockRequest);
    });

    it('should handle custom message', () => {
      const customMessage = 'Custom error message';
      interceptError(ERROR_ENDPOINT, 400, customMessage);

      const mockRequest: CypressRequest = {
        reply: (response) => {
          expect(response.body.detail).to.equal(customMessage);
        }
      };

      interceptSpy.calls[0].handler(mockRequest);
    });

    it('should use default message when custom message not provided', () => {
      interceptError(ERROR_ENDPOINT, 500);

      const mockRequest: CypressRequest = {
        reply: (response) => {
          expect(response.body.detail).to.equal('Internal Server Error');
        }
      };

      interceptSpy.calls[0].handler(mockRequest);
    });

    it('should handle delay in error responses', () => {
      const delay = 2000;
      interceptError(ERROR_ENDPOINT, 503, 'Service unavailable', delay);

      const mockRequest: CypressRequest = {
        reply: (response) => {
          expect(response.delay).to.equal(delay);
          expect(response.statusCode).to.equal(503);
        }
      };

      interceptSpy.calls[0].handler(mockRequest);
    });
  });

  describe('interceptSequence', () => {
    it('should create a sequence interceptor with responses', () => {
      interceptSequence(SEQUENCE_ENDPOINT, mockSequenceData);

      expect(interceptSpy.calls).to.have.length(1);
      expect(interceptSpy.calls[0].method).to.equal('GET');
      expect(interceptSpy.calls[0].pattern).to.equal('/api/sequence');
      expect(interceptSpy.calls[0].alias).to.equal('@sequenceEndpoint');
    });

    it('should cycle through responses sequentially', () => {
      interceptSequence(SEQUENCE_ENDPOINT, mockSequenceData);

      const handler = interceptSpy.calls[0].handler;
      const responses = [];

      // Simulate multiple requests
      for (let i = 0; i < 5; i++) {
        const mockRequest = {
          reply: (response) => {
            responses.push(response.body);
          }
        };
        handler(mockRequest);
      }

      expect(responses[0]).to.deep.equal(mockSequenceData[0]);
      expect(responses[1]).to.deep.equal(mockSequenceData[1]);
      expect(responses[2]).to.deep.equal(mockSequenceData[2]);
      expect(responses[3]).to.deep.equal(mockSequenceData[0]); // Should cycle back
      expect(responses[4]).to.deep.equal(mockSequenceData[1]);
    });

    it('should handle mixed success/error responses in sequence', () => {
      const mixedResponses = [
        { step: 1, status: 'started' },
        { statusCode: 500, message: 'Temporary failure' },
        { step: 3, status: 'completed' }
      ];

      interceptSequence(SEQUENCE_ENDPOINT, mixedResponses);

      const handler = interceptSpy.calls[0].handler;
      const responses = [];

      for (let i = 0; i < 3; i++) {
        const mockRequest = {
          reply: (response) => {
            responses.push({
              statusCode: response.statusCode,
              body: response.body
            });
          }
        };
        handler(mockRequest);
      }

      expect(responses[0].statusCode).to.equal(200);
      expect(responses[1].statusCode).to.equal(500);
      expect(responses[2].statusCode).to.equal(200);
    });

    it('should handle delay in sequence responses', () => {
      const delay = 750;
      interceptSequence(SEQUENCE_ENDPOINT, mockSequenceData, delay);

      const mockRequest: CypressRequest = {
        reply: (response) => {
          expect(response.delay).to.equal(delay);
        }
      };

      interceptSpy.calls[0].handler(mockRequest);
    });

    it('should handle empty response array with warning', () => {
      const consoleSpy = vi.spyOn(console, 'warn').mockImplementation(vi.fn());

      interceptSequence(SEQUENCE_ENDPOINT, []);

      expect(consoleSpy).toHaveBeenCalled();

      const mockRequest: CypressRequest = {
        reply: (response) => {
          expect(response.body).to.deep.equal({});
        }
      };

      interceptSpy.calls[0].handler(mockRequest);

      consoleSpy.mockRestore();
    });

    it('should handle undefined responses array', () => {
      const consoleSpy = vi.spyOn(console, 'warn').mockImplementation(vi.fn());

      interceptSequence(SEQUENCE_ENDPOINT, undefined);

      expect(consoleSpy).toHaveBeenCalled();

      const mockRequest: CypressRequest = {
        reply: (response) => {
          expect(response.body).to.deep.equal({});
        }
      };

      interceptSpy.calls[0].handler(mockRequest);

      consoleSpy.mockRestore();
    });
  });

  describe('interceptEmpty', () => {
    it('should create an empty response interceptor', () => {
      interceptEmpty(TEST_ENDPOINT);

      const mockRequest: CypressRequest = {
        reply: (response) => {
          expect(response.statusCode).to.equal(200);
          expect(response.body).to.deep.equal({});
        }
      };

      interceptSpy.calls[0].handler(mockRequest);
    });

    it('should handle delay in empty responses', () => {
      const delay = 500;
      interceptEmpty(TEST_ENDPOINT, delay);

      const mockRequest: CypressRequest = {
        reply: (response) => {
          expect(response.delay).to.equal(delay);
          expect(response.body).to.deep.equal({});
        }
      };

      interceptSpy.calls[0].handler(mockRequest);
    });
  });

  describe('interceptNetworkError', () => {
    it('should create a network error interceptor', () => {
      interceptNetworkError(TEST_ENDPOINT);

      expect(interceptSpy.calls).to.have.length(1);
      expect(interceptSpy.calls[0].handler).to.deep.equal({ forceNetworkError: true });
    });
  });

  describe('Helper Functions', () => {
    it('should handle all supported status codes in error responses', () => {
      const statusCodes: StatusCode[] = [200, 201, 400, 401, 403, 404, 500, 502, 503];

      statusCodes.forEach(statusCode => {
        interceptError(TEST_ENDPOINT, statusCode);

        const mockRequest = {
          reply: (response) => {
            if (statusCode >= 400) {
              expect(response.body.status).to.equal(statusCode);
              expect(response.body.title).to.be.a('string');
              expect(response.body.type).to.include('rfc9110#section-');
            }
          }
        };

        interceptSpy.calls[interceptSpy.calls.length - 1].handler(mockRequest);
      });
    });
  });

  describe('Type Safety', () => {
    it('should enforce proper endpoint structure', () => {
      // These should compile without errors
      const validEndpoint: EndpointDefinition = {
        method: 'GET',
        pattern: '/api/valid',
        alias: '@validAlias'
      };

      expect(() => interceptSuccess(validEndpoint)).to.not.throw();
    });

    it('should handle various data types', () => {
      const testDataTypes = [
        { string: 'test' },
        [1, 2, 3],
        { nested: { object: 'value' } },
        null,
        undefined
      ];

      testDataTypes.forEach(data => {
        expect(() => interceptSuccess(TEST_ENDPOINT, data)).to.not.throw();
      });
    });
  });
});