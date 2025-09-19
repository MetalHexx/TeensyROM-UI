import { describe, it, expect, beforeEach, vi, afterEach } from 'vitest';
import { LogType, logInfo, logError, logWarn } from './log-helper';

describe('LogType enum', () => {
  it('should have all expected emoji values', () => {
    expect(LogType.Start).toBe('🚀');
    expect(LogType.Finish).toBe('🏁');
    expect(LogType.Success).toBe('✅');
    expect(LogType.NetworkRequest).toBe('🌐');
    expect(LogType.Navigate).toBe('🧭');
    expect(LogType.Refresh).toBe('🔄');
    expect(LogType.Cleanup).toBe('🧹');
    expect(LogType.Error).toBe('❌');
    expect(LogType.Warning).toBe('⚠️');
    expect(LogType.Unknown).toBe('❓');
    expect(LogType.Select).toBe('🖱️');
    expect(LogType.Info).toBe('ℹ️');
    expect(LogType.Critical).toBe('🛑');
    expect(LogType.Debug).toBe('🐞');
    expect(LogType.Midi).toBe('🎹');
  });

  it('should have exactly 15 log types', () => {
    const logTypeValues = Object.values(LogType);
    expect(logTypeValues).toHaveLength(15);
  });
});

describe('logInfo', () => {
  let consoleLogSpy: ReturnType<typeof vi.spyOn>;

  beforeEach(() => {
    consoleLogSpy = vi.spyOn(console, 'log').mockImplementation(() => {});
  });

  afterEach(() => {
    consoleLogSpy.mockRestore();
  });

  it('should log message with operation emoji when no data provided', () => {
    logInfo(LogType.Start, 'Test message');

    expect(consoleLogSpy).toHaveBeenCalledWith('🚀 Test message');
    expect(consoleLogSpy).toHaveBeenCalledTimes(1);
  });

  it('should log message with operation emoji and data when data provided', () => {
    const testData = { test: 'data' };
    logInfo(LogType.Success, 'Operation complete', testData);

    expect(consoleLogSpy).toHaveBeenCalledWith('✅ Operation complete', testData);
    expect(consoleLogSpy).toHaveBeenCalledTimes(1);
  });

  it('should handle all LogType values correctly', () => {
    logInfo(LogType.NetworkRequest, 'API call');
    logInfo(LogType.Navigate, 'Navigation');
    logInfo(LogType.Refresh, 'Refreshing');

    expect(consoleLogSpy).toHaveBeenNthCalledWith(1, '🌐 API call');
    expect(consoleLogSpy).toHaveBeenNthCalledWith(2, '🧭 Navigation');
    expect(consoleLogSpy).toHaveBeenNthCalledWith(3, '🔄 Refreshing');
  });

  it('should handle undefined data parameter correctly', () => {
    logInfo(LogType.Info, 'Test message', undefined);

    expect(consoleLogSpy).toHaveBeenCalledWith('ℹ️ Test message');
    expect(consoleLogSpy).toHaveBeenCalledTimes(1);
  });
});

describe('logError', () => {
  let consoleErrorSpy: ReturnType<typeof vi.spyOn>;

  beforeEach(() => {
    consoleErrorSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
  });

  afterEach(() => {
    consoleErrorSpy.mockRestore();
  });

  it('should log error message with error emoji when no error object provided', () => {
    logError('Something went wrong');

    expect(consoleErrorSpy).toHaveBeenCalledWith('❌ Something went wrong');
    expect(consoleErrorSpy).toHaveBeenCalledTimes(1);
  });

  it('should log error message with error emoji and error object when provided', () => {
    const testError = new Error('Test error');
    logError('Operation failed', testError);

    expect(consoleErrorSpy).toHaveBeenCalledWith('❌ Operation failed', testError);
    expect(consoleErrorSpy).toHaveBeenCalledTimes(1);
  });

  it('should handle undefined error parameter correctly', () => {
    logError('Error message', undefined);

    expect(consoleErrorSpy).toHaveBeenCalledWith('❌ Error message');
    expect(consoleErrorSpy).toHaveBeenCalledTimes(1);
  });

  it('should handle non-Error objects as error parameter', () => {
    const errorData = { code: 500, message: 'Server error' };
    logError('API error', errorData);

    expect(consoleErrorSpy).toHaveBeenCalledWith('❌ API error', errorData);
    expect(consoleErrorSpy).toHaveBeenCalledTimes(1);
  });
});

describe('logWarn', () => {
  let consoleWarnSpy: ReturnType<typeof vi.spyOn>;

  beforeEach(() => {
    consoleWarnSpy = vi.spyOn(console, 'warn').mockImplementation(() => {});
  });

  afterEach(() => {
    consoleWarnSpy.mockRestore();
  });

  it('should log warning message with warning emoji', () => {
    logWarn('This is a warning');

    expect(consoleWarnSpy).toHaveBeenCalledWith('⚠️ This is a warning');
    expect(consoleWarnSpy).toHaveBeenCalledTimes(1);
  });

  it('should handle empty warning message', () => {
    logWarn('');

    expect(consoleWarnSpy).toHaveBeenCalledWith('⚠️ ');
    expect(consoleWarnSpy).toHaveBeenCalledTimes(1);
  });

  it('should handle long warning messages', () => {
    const longMessage =
      'This is a very long warning message that should still be logged correctly with the warning emoji prefix';
    logWarn(longMessage);

    expect(consoleWarnSpy).toHaveBeenCalledWith(`⚠️ ${longMessage}`);
    expect(consoleWarnSpy).toHaveBeenCalledTimes(1);
  });
});

describe('logging functions integration', () => {
  let consoleLogSpy: ReturnType<typeof vi.spyOn>;
  let consoleErrorSpy: ReturnType<typeof vi.spyOn>;
  let consoleWarnSpy: ReturnType<typeof vi.spyOn>;

  beforeEach(() => {
    consoleLogSpy = vi.spyOn(console, 'log').mockImplementation(() => {});
    consoleErrorSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
    consoleWarnSpy = vi.spyOn(console, 'warn').mockImplementation(() => {});
  });

  afterEach(() => {
    consoleLogSpy.mockRestore();
    consoleErrorSpy.mockRestore();
    consoleWarnSpy.mockRestore();
  });

  it('should use different console methods for different log levels', () => {
    logInfo(LogType.Info, 'Info message');
    logError('Error message');
    logWarn('Warning message');

    expect(consoleLogSpy).toHaveBeenCalledWith('ℹ️ Info message');
    expect(consoleErrorSpy).toHaveBeenCalledWith('❌ Error message');
    expect(consoleWarnSpy).toHaveBeenCalledWith('⚠️ Warning message');

    expect(consoleLogSpy).toHaveBeenCalledTimes(1);
    expect(consoleErrorSpy).toHaveBeenCalledTimes(1);
    expect(consoleWarnSpy).toHaveBeenCalledTimes(1);
  });
});
