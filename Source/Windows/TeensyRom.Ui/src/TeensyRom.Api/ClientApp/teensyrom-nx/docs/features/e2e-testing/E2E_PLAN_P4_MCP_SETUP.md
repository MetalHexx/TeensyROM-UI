# Chrome DevTools MCP Server Setup Guide

## 📋 Overview

This guide walks through setting up the Chrome DevTools Model Context Protocol (MCP) server to enable AI-assisted debugging of Cypress E2E test timing issues. The MCP server allows Claude/AI assistants to directly interact with Chrome DevTools during test execution.

**Purpose**: Debug why Cypress interceptors aren't catching API calls made during Angular app bootstrap.

**What This Enables**:

- Real-time Network tab inspection during test execution
- Console log monitoring during Cypress tests
- DOM state verification at specific test breakpoints
- Source code debugging during E2E test runs

---

## 🎯 Prerequisites

### Required Tools

- **Node.js 18+** - Runtime for the MCP server
- **pnpm** - Package manager (already used in this project)
- **Chrome/Chromium** - Browser with DevTools Protocol support
- **VS Code with Copilot** - IDE with MCP integration
- **Cypress** - Already configured in this project

### Repository Context

- Project: TeensyROM-UI
- Location: `C:\dev\src\TeensyROM-UI\Source\Windows\TeensyRom.Ui\src\TeensyRom.Api\ClientApp\teensyrom-nx`
- Current Branch: `core-refactor2`
- Test Suite: Device Discovery E2E Tests (`apps/teensyrom-ui-e2e/src/e2e/devices/device-discovery.cy.ts`)

---

## 🚀 Installation Steps

### Step 1: Install Chrome DevTools MCP Server

**Option A: Global Installation (Recommended for Cross-Project Use)**

```powershell
# Install globally via npm
npm install -g @modelcontextprotocol/server-chrome-devtools

# Verify installation
chrome-devtools-mcp --version
```

**Option B: Project-Local Installation**

```powershell
# Navigate to project root
cd C:\dev\src\TeensyROM-UI\Source\Windows\TeensyRom.Ui\src\TeensyRom.Api\ClientApp\teensyrom-nx

# Install as dev dependency
pnpm add -D @modelcontextprotocol/server-chrome-devtools

# Verify installation
pnpm exec chrome-devtools-mcp --version
```

**Expected Output**:

```
@modelcontextprotocol/server-chrome-devtools v1.x.x
```

---

### Step 2: Configure VS Code MCP Settings

The MCP server needs to be registered in VS Code's Copilot configuration.

**Location**: `%APPDATA%\Code\User\settings.json` (Windows)  
**Alternative**: VS Code Settings UI → Extensions → GitHub Copilot → MCP Servers

**Add Configuration**:

```json
{
  "github.copilot.chat.mcp.enabled": true,
  "github.copilot.chat.mcp.servers": {
    "chrome-devtools": {
      "command": "chrome-devtools-mcp",
      "args": [],
      "env": {}
    }
  }
}
```

**If Installed Locally** (Option B):

```json
{
  "github.copilot.chat.mcp.enabled": true,
  "github.copilot.chat.mcp.servers": {
    "chrome-devtools": {
      "command": "pnpm",
      "args": ["exec", "chrome-devtools-mcp"],
      "cwd": "C:/dev/src/TeensyROM-UI/Source/Windows/TeensyRom.Ui/src/TeensyRom.Api/ClientApp/teensyrom-nx",
      "env": {}
    }
  }
}
```

**Save and Reload VS Code**:

```
Ctrl+Shift+P → "Developer: Reload Window"
```

---

### Step 3: Launch Chrome with Remote Debugging

The MCP server connects to Chrome via the DevTools Protocol, which requires Chrome to be launched with remote debugging enabled.

**Command**:

```powershell
# Close all existing Chrome instances first!
# Then launch with debugging port

# Windows (Standard Installation)
& "C:\Program Files\Google\Chrome\Application\chrome.exe" --remote-debugging-port=9222 --user-data-dir="C:\temp\chrome-debug-profile"

# Windows (User Installation)
& "$env:LOCALAPPDATA\Google\Chrome\Application\chrome.exe" --remote-debugging-port=9222 --user-data-dir="C:\temp\chrome-debug-profile"
```

**Flags Explained**:

- `--remote-debugging-port=9222` - Opens DevTools Protocol on port 9222 (standard port)
- `--user-data-dir="C:\temp\chrome-debug-profile"` - Uses separate profile to avoid conflicts with your regular Chrome session

**Verification**:

1. Chrome should open with a yellow warning banner: "Chrome is being controlled by automated test software"
2. Navigate to `http://localhost:9222/json` in the debug Chrome instance
3. You should see JSON output listing inspectable pages

**Expected Output at `localhost:9222/json`**:

```json
[
  {
    "description": "",
    "devtoolsFrontendUrl": "/devtools/inspector.html?ws=localhost:9222/devtools/page/...",
    "id": "...",
    "title": "about:blank",
    "type": "page",
    "url": "about:blank",
    "webSocketDebuggerUrl": "ws://localhost:9222/devtools/page/..."
  }
]
```

---

### Step 4: Connect MCP Server to Chrome

**In VS Code Copilot Chat**:

```
@chrome connect to localhost:9222
```

**Expected Response**:

```
✅ Connected to Chrome DevTools at localhost:9222
📊 Found 1 inspectable page(s)
```

**If Connection Fails**:

- Verify Chrome is running with `--remote-debugging-port=9222`
- Check that port 9222 is not blocked by firewall
- Ensure no other process is using port 9222
- Try navigating to `localhost:9222/json` in browser to confirm port is open

---

### Step 5: Run Cypress Tests with Debugging

Now we need to run the Cypress tests so the MCP server can inspect the application during execution.

**Option A: Headed Mode (Recommended for Debugging)**

```powershell
# Open Cypress Test Runner
pnpm nx e2e teensyrom-ui-e2e --spec="**/device-discovery.cy.ts" --headed --browser=chrome

# Or use the Cypress GUI
pnpm nx e2e teensyrom-ui-e2e:open-cypress
```

**Option B: Headless Mode (For CI/Automated Testing)**

```powershell
# Run tests headless but with Chrome
pnpm nx e2e teensyrom-ui-e2e --spec="**/device-discovery.cy.ts" --headless --browser=chrome
```

**Why Chrome?**  
The MCP server requires Chrome/Chromium. Cypress defaults to Electron, which doesn't support remote debugging the same way.

---

### Step 6: Inspect Test Execution with MCP

Once tests are running, use VS Code Copilot Chat to query the MCP server:

**Example Commands**:

```
@chrome list tabs
```

Lists all open tabs/pages in the debug Chrome instance.

```
@chrome navigate to http://localhost:4200/devices
```

Navigates to the device view in the test app.

```
@chrome get network requests
```

Shows all network requests made by the page (critical for debugging interceptor issues).

```
@chrome get console logs
```

Retrieves console output from the application.

```
@chrome execute script: localStorage.getItem('devices')
```

Executes JavaScript in the page context to inspect state.

```
@chrome take screenshot
```

Captures a screenshot of the current page state.

---

## 🔍 Debugging the Interceptor Timing Issue

### Investigation Workflow

**Goal**: Determine why `cy.wait('@findDevices')` times out with "No request ever occurred"

**Step 1: Observe the Actual API Call**

1. **Run test in headed mode**:

   ```powershell
   pnpm nx e2e teensyrom-ui-e2e --spec="**/device-discovery.cy.ts" --headed --browser=chrome
   ```

2. **Let test navigate to `/devices` route**

3. **Query MCP server for network requests**:

   ```
   @chrome get network requests
   ```

4. **Look for**:
   - Exact URL of the device discovery call
   - HTTP method (GET/POST)
   - Query parameters (if any)
   - Response status code
   - Timing (when it fired relative to page load)

**Expected Insights**:

- If URL is `/api/devices` (no wildcard), our interceptor pattern `/api/devices*` should match
- If URL has query params like `/api/devices?refresh=true`, we need to adjust the pattern
- If no API call appears, the store may be caching devices

**Step 2: Verify Interceptor Registration Timing**

1. **Add console logging to the test**:

   ```typescript
   beforeEach(() => {
     cy.log('Setting up interceptor');
     interceptFindDevices({ fixture: singleDevice });
     cy.log('Interceptor registered');
     navigateToDeviceView();
     cy.log('Navigated to device view');
     waitForDeviceDiscovery();
   });
   ```

2. **Query MCP for console logs**:

   ```
   @chrome get console logs
   ```

3. **Look for the sequence**:
   - "Setting up interceptor"
   - "Interceptor registered"
   - "Navigated to device view"
   - API call logs (if any)

**Expected Insights**:

- If API call happens before "Interceptor registered", we have a timing issue
- If API call doesn't appear at all, the bootstrap may be skipping it due to cached state

**Step 3: Inspect DeviceStore State**

1. **Query store state via console**:

   ```
   @chrome execute script: window.__teensyromDebug?.deviceStore?.devices()
   ```

2. **Check if devices are cached**:
   ```
   @chrome execute script: {
     const store = window.__teensyromDebug?.deviceStore;
     return {
       devices: store?.devices?.(),
       hasInitialised: store?.hasInitialised?.(),
       isLoading: store?.isLoading?.()
     };
   }
   ```

**Expected Insights**:

- If `hasInitialised` is `true` on page load, bootstrap may skip API call
- If `devices` array is not empty, store is persisting state across tests

**Step 4: Monitor Bootstrap Service**

1. **Add logging to AppBootstrapService** (temporary):

   ```typescript
   async init(): Promise<void> {
     console.log('[Bootstrap] init() called');
     return new Promise((resolve) => {
       // ... effect setup ...
       console.log('[Bootstrap] calling findDevices()');
       this.deviceStore.findDevices();
       console.log('[Bootstrap] findDevices() called');
     });
   }
   ```

2. **Run test and query logs**:
   ```
   @chrome get console logs
   ```

**Expected Insights**:

- Confirms whether `findDevices()` is actually being called
- Shows timing relative to interceptor setup

---

## 🧪 Test Scenarios to Investigate

### Scenario 1: Inline Interceptor (Known to Pass)

**Test**:

```typescript
it('should show loading indicator', () => {
  cy.intercept('GET', '/api/devices*', (req) => {
    req.reply({
      delay: 500,
      statusCode: 200,
      body: { devices: singleDevice.devices, message: 'Found 1 device(s)' },
    });
  }).as('findDevices');

  navigateToDeviceView();
  verifyLoadingState();
});
```

**MCP Investigation**:

```
@chrome get network requests
```

**Look For**:

- Does the interceptor catch the request?
- What is the exact URL?
- What is the timing between intercept setup and request?

### Scenario 2: beforeEach Interceptor (Known to Fail)

**Test**:

```typescript
beforeEach(() => {
  interceptFindDevices({ fixture: singleDevice });
  navigateToDeviceView();
  waitForDeviceDiscovery();
});

it('should display single device', () => {
  verifyDeviceCount(1);
});
```

**MCP Investigation**:

```
@chrome get network requests
@chrome get console logs
```

**Look For**:

- Does the request happen before interceptor is ready?
- Is there a race condition between `beforeEach` and `cy.visit()`?

---

## 📊 Expected Outcomes

### If URL Pattern Mismatch

**Finding**: Actual API call is `/api/devices` (no wildcard needed)

**Solution**:

```typescript
// Update interceptor
cy.intercept('GET', '/api/devices', (req) => { ... }).as('findDevices');
```

### If Timing Issue

**Finding**: API call happens before interceptor registers

**Solution**:

```typescript
// Move interceptor inline
it('should display single device', () => {
  interceptFindDevices({ fixture: singleDevice });
  navigateToDeviceView();
  waitForDeviceDiscovery();
  verifyDeviceCount(1);
});
```

### If State Caching Issue

**Finding**: `hasInitialised` is true, preventing API call

**Solution**:

```typescript
// Force store reset
export function navigateToDeviceView(): Cypress.Chainable<Cypress.AUTWindow> {
  return cy.visit('/devices', {
    onBeforeLoad: (win) => {
      win.localStorage.clear();
      win.sessionStorage.clear();
      // Force store reset via window object
      if (win['__teensyromDebug']?.deviceStore) {
        win['__teensyromDebug'].deviceStore.resetAllDevices();
      }
    },
  });
}
```

---

## 🛠️ Troubleshooting

### MCP Server Won't Connect

**Issue**: `@chrome connect to localhost:9222` fails

**Solutions**:

1. Verify Chrome is running with `--remote-debugging-port=9222`
2. Check `localhost:9222/json` in browser
3. Restart Chrome with debug flags
4. Ensure no firewall blocking port 9222

### Cypress Tests Won't Run in Chrome

**Issue**: `pnpm nx e2e teensyrom-ui-e2e --browser=chrome` fails

**Solutions**:

1. Install Chrome if not present: https://www.google.com/chrome/
2. Verify Cypress detects Chrome: `pnpm exec cypress info`
3. Try `--browser=chromium` if Chrome not detected

### Network Requests Not Appearing

**Issue**: `@chrome get network requests` returns empty

**Solutions**:

1. Ensure page is loaded before querying
2. Navigate to page first: `@chrome navigate to http://localhost:4200/devices`
3. Wait for page load, then query network

### Console Logs Not Appearing

**Issue**: `@chrome get console logs` returns empty

**Solutions**:

1. Ensure DevTools is open (in headed mode)
2. Enable "Preserve log" in Network tab
3. Add explicit `console.log()` statements to application code

---

## 📝 Documentation for Demonstration

### Talking Points

**1. The Problem**:

- 12 out of 39 E2E tests are timing out
- Cypress can't find the API request: "No request ever occurred"
- Angular app bootstrap calls `findDevices()` on startup
- Interceptor pattern isn't catching the request

**2. Traditional Debugging Limitations**:

- Can't see real-time network traffic during test execution
- Console logs lost between test runs
- No visibility into application state during Cypress tests
- Guessing at timing issues without hard data

**3. MCP Solution**:

- AI assistant connects directly to Chrome DevTools
- Real-time network request inspection
- Console log monitoring during test execution
- Execute JavaScript in page context to inspect state
- Screenshot capture at any point in test

**4. Investigation Flow**:

```
Run Cypress test (headed mode)
    ↓
@chrome get network requests  ← See exact API call
    ↓
@chrome get console logs      ← See timing of events
    ↓
@chrome execute script        ← Inspect store state
    ↓
Identify root cause (URL mismatch, timing, caching)
    ↓
Apply fix and re-test
```

### Live Demo Script

**Step 1**: Show the failing test

```powershell
pnpm nx e2e teensyrom-ui-e2e --spec="**/device-discovery.cy.ts" --headed --browser=chrome
```

**Step 2**: Connect MCP

```
@chrome connect to localhost:9222
```

**Step 3**: Show network requests

```
@chrome get network requests
```

Point out: "Here's the exact API call being made - notice the URL pattern"

**Step 4**: Show console logs

```
@chrome get console logs
```

Point out: "See when the bootstrap service fires relative to test setup"

**Step 5**: Inspect state

```
@chrome execute script: window.__teensyromDebug?.deviceStore?.hasInitialised?.()
```

Point out: "The store state shows why the API call isn't happening"

**Step 6**: Show the fix
Update the interceptor pattern based on findings, re-run test, show it passing.

---

## 🎓 Learning Resources

- **Chrome DevTools MCP**: https://github.com/ChromeDevTools/chrome-devtools-mcp/
- **Chrome DevTools Protocol**: https://chromedevtools.github.io/devtools-protocol/
- **Model Context Protocol Spec**: https://modelcontextprotocol.io/
- **Cypress Intercept Guide**: https://docs.cypress.io/api/commands/intercept

---

## ✅ Success Checklist

- [ ] MCP server installed (`chrome-devtools-mcp --version` works)
- [ ] VS Code configured with MCP settings
- [ ] Chrome launched with `--remote-debugging-port=9222`
- [ ] MCP server connected to Chrome (`@chrome connect`)
- [ ] Cypress tests running in headed Chrome mode
- [ ] Can query network requests via `@chrome get network requests`
- [ ] Can query console logs via `@chrome get console logs`
- [ ] Can execute JavaScript via `@chrome execute script`
- [ ] Identified root cause of interceptor timing issue
- [ ] Applied fix and verified tests pass

---

## 📅 Project Context

**Date**: October 19, 2025  
**Project**: TeensyROM-UI E2E Testing (Phase 4)  
**Issue**: Device Discovery Test Timing Failures  
**Document**: E2E_PLAN_P4_TIMING_ISSUES.md  
**Next Steps**: Use MCP to debug interceptor pattern mismatch or timing race condition

---

## 🙏 Acknowledgments

This debugging approach leverages the Model Context Protocol (MCP) to enable AI-assisted development. The Chrome DevTools MCP server provides programmatic access to Chrome DevTools Protocol, allowing Claude/Copilot to inspect live browser state during test execution.

**Key Innovation**: Instead of manually inspecting DevTools during test runs, the AI assistant can directly query browser state and correlate it with test execution flow, dramatically speeding up debugging.

---

**Ready to Debug!** 🚀

Once set up, proceed to E2E_PLAN_P4_TIMING_ISSUES.md "Next Steps" section and use the MCP server to investigate the interceptor timing issue.
