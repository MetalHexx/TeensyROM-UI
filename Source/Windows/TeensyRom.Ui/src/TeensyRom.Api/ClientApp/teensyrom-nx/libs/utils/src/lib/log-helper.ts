export enum LogType {
  Start = '🚀',
  Play = '▶️',
  Paused = '⏸️',
  Stop = '⏹️',
  FastForward = '⏩',
  Rewind = '⏪',
  Next = '⏭️',
  Previous = '⏮️',
  Finish = '🏁',
  Success = '✅',
  NetworkRequest = '🌐',
  Navigate = '🧭',
  Refresh = '🔄',
  Cleanup = '🧹',
  Error = '❌',
  Warning = '⚠️',
  Unknown = '❓',
  Select = '🖱️',
  Info = 'ℹ️',
  Critical = '🛑',
  Debug = '🐞',
  Midi = '🎹',
}

export function logInfo(operation: LogType, message: string, data?: unknown): void {
  if (data !== undefined) {
    console.log(`${operation} ${message}`, data);
  } else {
    console.log(`${operation} ${message}`);
  }
}

export function logError(message: string, error?: unknown): void {
  if (error !== undefined) {
    console.error(`${LogType.Error} ${message}`, error);
  } else {
    console.error(`${LogType.Error} ${message}`);
  }
}

export function logWarn(message: string): void {
  console.warn(`${LogType.Warning} ${message}`);
}
