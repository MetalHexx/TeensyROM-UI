# Auto-Connect Polling System Plan

## Problem Context

When users start the TeensyROM UI before the .NET API is running, the application shows no available devices and provides no feedback about the connection status. Users must manually refresh or restart the application once the API becomes available.

### Current Behavior

1. User starts UI before API is running
2. Initial device discovery fails silently
3. UI shows empty device list with no indication of connection issues
4. User must manually trigger device discovery again
5. Poor user experience with no automated recovery

## Solution: Aggressive Polling System

Implement an intelligent polling system that automatically attempts to discover devices when the API becomes available.

### Core Features

#### 1. Automatic Polling

- Start aggressive polling when initial device discovery fails
- Use exponential backoff with reasonable limits
- Continue until devices are discovered or user manually stops
- Provide clear visual feedback during polling attempts

#### 2. API Health Detection

- Detect when API endpoints become available
- Differentiate between network errors and empty device responses
- Handle different failure scenarios (API down, API up but no devices, etc.)

#### 3. Device Store Integration

- Update device store automatically when devices are discovered
- Trigger appropriate UI state changes
- Maintain polling state in the store for UI feedback

#### 4. User Control

- Allow users to manually stop/start polling
- Provide manual "Retry" button for immediate attempts
- Show current polling status and next attempt countdown

### Technical Implementation

#### Components Involved

```
@teensyrom-nx/domain/device/state
├── Device Store (polling state management)
├── Auto-connect methods
└── Polling status tracking

@teensyrom-nx/domain/device/services
├── Device Service (API calls)
├── Health check utilities
└── Connection retry logic

@teensyrom-nx/features/devices
├── Device Discovery UI
├── Polling status indicators
└── Manual control buttons
```

#### Polling Strategy

```typescript
// Pseudo-implementation
interface PollingConfig {
  initialDelay: 1000; // 1 second
  maxDelay: 30000; // 30 seconds max
  backoffMultiplier: 1.5; // Exponential backoff
  maxAttempts: null; // Poll indefinitely
  aggressiveWindow: 60000; // First minute: more aggressive
}
```

#### State Management

```typescript
interface DevicePollingState {
  isPolling: boolean;
  attempt: number;
  nextRetryAt: Date | null;
  lastError: string | null;
  pollingReason: 'startup' | 'manual' | 'connection-lost';
}
```

### User Experience Flow

#### Startup Scenario

1. **App Loads** → Initial device discovery fails
2. **Auto-Polling Starts** → Shows "Searching for devices..." indicator
3. **Polling Feedback** → "Attempt 3/∞ - Next retry in 5s"
4. **API Available** → Discovers devices, updates store
5. **Success State** → Shows discovered devices, stops polling

#### Visual Feedback

- Animated "searching" indicator during polling
- Countdown timer showing next attempt
- Current attempt number
- Last error message (user-friendly)
- Manual retry button always available

### Implementation Phases

#### Phase 1: Basic Polling Infrastructure

- [ ] Add polling state to device store
- [ ] Implement basic retry logic with exponential backoff
- [ ] Add health check utilities for API detection
- [ ] Create polling start/stop methods

#### Phase 2: UI Integration

- [ ] Add polling status indicators to device discovery UI
- [ ] Implement manual control buttons (retry, stop)
- [ ] Show user-friendly error messages
- [ ] Add countdown timers and attempt counters

#### Phase 3: Smart Polling Logic

- [ ] Differentiate between API down vs no devices
- [ ] Implement aggressive vs normal polling modes
- [ ] Add connection lost detection and recovery
- [ ] Optimize polling intervals based on failure types

#### Phase 4: Advanced Features

- [ ] Remember last successful API endpoint
- [ ] Detect API version compatibility
- [ ] Handle partial failures (some endpoints work, others don't)
- [ ] Add user preferences for polling behavior

### Configuration Options

```typescript
interface AutoConnectSettings {
  enableAutoPolling: boolean;
  aggressivePollingDuration: number; // ms
  maxPollingInterval: number; // ms
  showPollingNotifications: boolean;
  autoStopAfterSuccess: boolean;
}
```

### Error Handling

#### Failure Categories

1. **Network/Connection Errors** → API completely unavailable
2. **HTTP Errors** → API available but returning errors
3. **Timeout Errors** → API slow to respond
4. **Parse Errors** → API returning malformed responses

#### Recovery Strategies

- Network errors: Aggressive polling with exponential backoff
- HTTP errors: Less aggressive polling, may indicate API issues
- Timeouts: Medium polling with timeout adjustments
- Parse errors: Minimal polling, likely indicates version mismatch

### Performance Considerations

#### Resource Management

- Use AbortController for cancelling in-flight requests
- Implement proper cleanup when component unmounts
- Avoid memory leaks from continuous polling
- Throttle UI updates during rapid polling

#### Network Efficiency

- Use lightweight health check endpoints when available
- Implement request deduplication
- Cache negative responses briefly to avoid spam
- Respect any rate limiting from the API

### Success Criteria

- [ ] UI provides clear feedback when API is unavailable
- [ ] Automatic device discovery when API becomes available
- [ ] No manual intervention required for normal startup scenarios
- [ ] Reasonable resource usage during polling
- [ ] User can control polling behavior when needed

### Future Enhancements

#### WebSocket Integration

- Replace polling with WebSocket notifications when API supports it
- Fall back to polling when WebSocket unavailable
- Real-time device connection/disconnection events

#### Service Worker Integration

- Background polling even when UI is not active
- Push notifications for device availability
- Offline capability indicators

#### Analytics Integration

- Track polling success/failure rates
- Measure time-to-first-device-discovery
- Identify common failure patterns for API improvements

---

## Notes

This is a placeholder document for future implementation. The auto-connect polling system will significantly improve the user experience when starting the UI before the API is available, providing automatic recovery and clear status feedback.

**Priority**: Medium - Improves UX but not blocking for core functionality  
**Effort**: 2-3 weeks for full implementation across all phases  
**Dependencies**: Requires stable device service and store architecture
