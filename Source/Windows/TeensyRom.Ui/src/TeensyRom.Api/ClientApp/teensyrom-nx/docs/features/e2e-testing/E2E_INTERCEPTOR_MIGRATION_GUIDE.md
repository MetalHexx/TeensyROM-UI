# E2E Interceptor Migration Guide

## 🎯 Purpose

This guide helps developers migrate from the old scattered interceptor patterns to the new consolidated, explicit import architecture. The migration improves code organization, maintainability, and developer experience.

---

## 📚 Background

The E2E testing infrastructure has been refactored from a scattered approach to a **consolidated per-endpoint architecture**. Previously, interceptor logic was spread across multiple files:

- `api.constants.ts` (endpoint definitions and aliases)
- Domain-specific interceptor files (e.g., `device.interceptors.ts`)
- Separate test-helpers files

**New Architecture**: Each endpoint now has its own self-contained interceptor file following a consistent 6-section structure.

---

## 🔄 Migration Overview

### Before (Old Pattern)
```typescript
// Imports scattered across multiple files
import { DEVICE_ENDPOINTS, INTERCEPT_ALIASES } from '../support/constants/api.constants';
import { interceptFindDevices, waitForFindDevices } from '../support/interceptors/device.interceptors';

// Usage in tests
cy.intercept(DEVICE_ENDPOINTS.FIND_DEVICES.pattern, fixture).as(INTERCEPT_ALIASES.FIND_DEVICES);
cy.wait(`@${INTERCEPT_ALIASES.FIND_DEVICES}`);
```

### After (New Pattern)
```typescript
// Explicit imports from individual endpoint files
import { interceptFindDevices, waitForFindDevices } from '../support/interceptors/findDevices.interceptors';

// Usage in tests
interceptFindDevices({ fixture: mockDevices });
waitForFindDevices();
```

---

## 📋 Step-by-Step Migration

### Step 1: Identify Current Usage

Audit your test files to identify current interceptor usage patterns:

```bash
# Search for old import patterns
grep -r "DEVICE_ENDPOINTS\|STORAGE_ENDPOINTS\|INTERCEPT_ALIASES" src/e2e/
grep -r "from.*device.interceptors" src/e2e/
grep -r "from.*storage.interceptors" src/e2e/
```

### Step 2: Update Import Statements

Replace old scattered imports with explicit imports:

**Device Domain:**
```typescript
// Old imports to replace:
import { DEVICE_ENDPOINTS, INTERCEPT_ALIASES } from '../support/constants/api.constants';
import { interceptFindDevices, waitForFindDevices } from '../support/interceptors/device.interceptors';

// New explicit imports:
import {
  interceptFindDevices,
  waitForFindDevices
} from '../support/interceptors/findDevices.interceptors';

import {
  interceptConnectDevice,
  waitForConnectDevice
} from '../support/interceptors/connectDevice.interceptors';

import {
  interceptDisconnectDevice,
  waitForDisconnectDevice
} from '../support/interceptors/disconnectDevice.interceptors';

import {
  interceptPingDevice,
  waitForPingDevice
} from '../support/interceptors/pingDevice.interceptors';
```

**Storage Domain:**
```typescript
// Old imports to replace:
import { interceptGetDirectory, waitForGetDirectory } from '../support/interceptors/storage.interceptors';
import { interceptSaveFavorite, waitForSaveFavorite } from '../support/interceptors/storage.interceptors';
import { interceptRemoveFavorite, waitForRemoveFavorite } from '../support/interceptors/storage.interceptors';

// New explicit imports:
import {
  interceptGetDirectory,
  waitForGetDirectory
} from '../support/interceptors/getDirectory.interceptors';

import {
  interceptSaveFavorite,
  waitForSaveFavorite
} from '../support/interceptors/saveFavorite.interceptors';

import {
  interceptRemoveFavorite,
  waitForRemoveFavorite
} from '../support/interceptors/removeFavorite.interceptors';
```

**Player Domain:**
```typescript
// Old imports to replace:
import { interceptLaunchFile, waitForLaunchFile } from '../support/interceptors/player.interceptors';
import { interceptLaunchRandom, waitForLaunchRandom } from '../support/interceptors/player.interceptors';

// New explicit imports:
import {
  interceptLaunchFile,
  waitForLaunchFile
} from '../support/interceptors/launchFile.interceptors';

import {
  interceptLaunchRandom,
  waitForLaunchRandom
} from '../support/interceptors/launchRandom.interceptors';
```

**Indexing Domain:**
```typescript
// Old imports to replace:
import { interceptIndexStorage, waitForIndexStorage } from '../support/interceptors/storage-indexing.interceptors';
import { interceptIndexAllStorage, waitForIndexAllStorage } from '../support/interceptors/storage-indexing.interceptors';

// New explicit imports:
import {
  interceptIndexStorage,
  waitForIndexStorage
} from '../support/interceptors/indexStorage.interceptors';

import {
  interceptIndexAllStorage,
  waitForIndexAllStorage
} from '../support/interceptors/indexAllStorage.interceptors';
```

### Step 3: Update Test Usage Patterns

Replace old usage patterns with new helper functions:

**Before:**
```typescript
// Old manual interceptor setup
cy.intercept(DEVICE_ENDPOINTS.FIND_DEVICES.pattern, { fixture: 'devices.json' }).as(INTERCEPT_ALIASES.FIND_DEVICES);
cy.wait(`@${INTERCEPT_ALIASES.FIND_DEVICES}`);

// Old manual error handling
cy.intercept('GET', DEVICE_ENDPOINTS.FIND_DEVICES.pattern, {
  statusCode: 500,
  body: { error: 'Internal Server Error' }
}).as(INTERCEPT_ALIASES.FIND_DEVICES);
```

**After:**
```typescript
// New helper function usage
interceptFindDevices({ fixture: mockDevices });
waitForFindDevices();

// New error handling
interceptFindDevices({ errorMode: true });
waitForFindDevices();
```

### Step 4: Update Helper Functions

Replace old helper function calls:

**Before:**
```typescript
// Old helper functions
setupDeviceDiscovery(mockDevices);
verifyDeviceDiscoveryCompleted();
```

**After:**
```typescript
// New helper functions (same names, but imported from individual files)
setupFindDevices(mockDevices);
verifyFindDevicesCompleted();
```

---

## 🏗️ New Architecture Benefits

### 1. **Explicit Dependencies**
```typescript
// Clear visibility of what endpoints each test uses
import {
  interceptFindDevices,
  waitForFindDevices,
  interceptConnectDevice,
  waitForConnectDevice
} from '../support/interceptors/findDevices.interceptors';
```

### 2. **Self-Contained Files**
Each endpoint file contains everything needed for that endpoint:
- Endpoint definition
- Interface definitions
- Interceptor function
- Wait function
- Helper functions
- Export constants

### 3. **Type Safety**
```typescript
// Better TypeScript support with proper interfaces
interceptFindDevices({
  fixture: mockDevices,  // Type: MockDeviceFixture
  errorMode: false,     // Type: boolean
  responseDelayMs: 1000 // Type: number
});
```

### 4. **Consistent Patterns**
All endpoints follow the same 6-section structure, making them predictable and easy to use.

---

## 📖 Common Migration Scenarios

### Scenario 1: Device Discovery Test

**Before:**
```typescript
import { DEVICE_ENDPOINTS, INTERCEPT_ALIASES } from '../support/constants/api.constants';
import { setupDeviceDiscovery } from '../support/interceptors/device.interceptors';

it('should discover devices', () => {
  cy.intercept(DEVICE_ENDPOINTS.FIND_DEVICES.pattern, { fixture: 'devices.json' })
    .as(INTERCEPT_ALIASES.FIND_DEVICES);

  cy.visit('/');
  cy.wait(`@${INTERCEPT_ALIASES.FIND_DEVICES}`);

  cy.get('[data-testid=device-card]').should('have.length', 3);
});
```

**After:**
```typescript
import {
  interceptFindDevices,
  waitForFindDevices,
  setupFindDevices
} from '../support/interceptors/findDevices.interceptors';

it('should discover devices', () => {
  setupFindDevices(mockDevices);

  cy.visit('/');
  waitForFindDevices();

  cy.get('[data-testid=device-card]').should('have.length', 3);
});
```

### Scenario 2: File Launching Test

**Before:**
```typescript
import { PLAYER_ENDPOINTS, INTERCEPT_ALIASES } from '../support/constants/api.constants';

it('should launch file', () => {
  cy.intercept('POST', PLAYER_ENDPOINTS.LAUNCH_FILE.pattern('device-1', 'sdcard'))
    .as(INTERCEPT_ALIASES.LAUNCH_FILE);

  cy.get('[data-testid=launch-button]').click();
  cy.wait(`@${INTERCEPT_ALIASES.LAUNCH_FILE}`);

  cy.get('[data-testid=launch-success]').should('be.visible');
});
```

**After:**
```typescript
import {
  interceptLaunchFile,
  waitForLaunchFile,
  setupLaunchFile
} from '../support/interceptors/launchFile.interceptors';

it('should launch file', () => {
  setupLaunchFile(mockFile);

  cy.get('[data-testid=launch-button]').click();
  waitForLaunchFile();

  cy.get('[data-testid=launch-success]').should('be.visible');
});
```

### Scenario 3: Error Handling Test

**Before:**
```typescript
it('should handle connection errors', () => {
  cy.intercept('POST', DEVICE_ENDPOINTS.CONNECT_DEVICE.pattern('device-1'), {
    statusCode: 500,
    body: { error: 'Connection failed' }
  }).as(INTERCEPT_ALIASES.CONNECT_DEVICE);

  cy.get('[data-testid=connect-button]').click();
  cy.wait(`@${INTERCEPT_ALIASES.CONNECT_DEVICE}`);

  cy.get('[data-testid=error-message]').should('be.visible');
});
```

**After:**
```typescript
import {
  interceptConnectDevice,
  waitForConnectDevice,
  setupConnectDeviceError
} from '../support/interceptors/connectDevice.interceptors';

it('should handle connection errors', () => {
  setupConnectDeviceError();

  cy.get('[data-testid=connect-button]').click();
  waitForConnectDevice();

  cy.get('[data-testid=error-message]').should('be.visible');
});
```

---

## 🛠️ Available Endpoint Files

### Device Domain
- `findDevices.interceptors.ts` - Device discovery
- `connectDevice.interceptors.ts` - Device connection
- `disconnectDevice.interceptors.ts` - Device disconnection
- `pingDevice.interceptors.ts` - Device health checks

### Storage Domain
- `getDirectory.interceptors.ts` - Directory browsing
- `saveFavorite.interceptors.ts` - Save favorite files
- `removeFavorite.interceptors.ts` - Remove favorite files

### Player Domain
- `launchFile.interceptors.ts` - Launch specific files
- `launchRandom.interceptors.ts` - Launch random files

### Indexing Domain
- `indexStorage.interceptors.ts` - Individual storage indexing
- `indexAllStorage.interceptors.ts` - Batch indexing operations

---

## ⚠️ Troubleshooting

### Issue: Import Errors
**Problem**: `Cannot find module '../support/interceptors/findDevices.interceptors'`

**Solution**:
1. Verify the file exists in the correct location
2. Check for typos in the import path
3. Ensure you're importing from `.interceptors.ts` files, not old domain files

### Issue: Function Not Found
**Problem**: `interceptFindDevices is not a function`

**Solution**:
1. Verify the function is exported from the interceptor file
2. Check that you're importing the correct function name
3. Look at the interceptor file to see available exports

### Issue: Type Errors
**Problem**: TypeScript errors about missing properties

**Solution**:
1. Check the interface definition in the interceptor file
2. Use the correct option object structure
3. Look at the JSDoc comments in the interceptor file for guidance

### Issue: Tests Failing After Migration
**Problem**: Tests that worked before are now failing

**Solution**:
1. Verify you're using the correct helper functions
2. Check that the wait functions are called after the expected API calls
3. Ensure fixture data matches the expected interface

---

## 📝 Best Practices

### 1. Import Only What You Need
```typescript
// Good: Import specific functions
import { interceptFindDevices, waitForFindDevices } from './findDevices.interceptors';

// Avoid: Importing everything
import * as findDevices from './findDevices.interceptors';
```

### 2. Use Helper Functions When Available
```typescript
// Good: Use provided helper functions
setupFindDevices(mockDevices);

// Avoid: Manual interceptor setup unless needed
cy.intercept(FIND_DEVICES_ENDPOINT.pattern, mockDevices).as(FIND_DEVICES_ENDPOINT.alias);
```

### 3. Leverage Type Safety
```typescript
// Good: Use proper interfaces
const options: InterceptFindDevicesOptions = {
  fixture: mockDevices,
  errorMode: false
};
interceptFindDevices(options);

// Good: Let TypeScript infer types
interceptFindDevices({ fixture: mockDevices });
```

### 4. Consistent Error Handling
```typescript
// Good: Use built-in error modes
interceptFindDevices({ errorMode: true });

// Good: Use specific error helpers
setupFindDevicesError();

// Avoid: Manual error responses unless custom behavior needed
```

### 5. Wait for Correct Operations
```typescript
// Good: Wait for specific endpoints
waitForFindDevices();
waitForConnectDevice();

// Good: Chain operations when appropriate
interceptFindDevices();
interceptConnectDevice();
waitForFindDevices();
waitForConnectDevice();
```

---

## 🔧 Advanced Usage

### Custom Interceptor Options
```typescript
import { interceptGetDirectory, waitForGetDirectory } from './getDirectory.interceptors';

interceptGetDirectory({
  filesystem: mockFilesystem,  // Custom filesystem
  path: '/games',             // Specific path
  errorMode: false,           // Success mode
  responseDelayMs: 1000,      // Custom delay
  customFiles: extraFiles     // Additional files
});
```

### Complex Workflows
```typescript
import {
  interceptFindDevices,
  waitForFindDevices,
  interceptConnectDevice,
  waitForConnectDevice,
  interceptGetDirectory,
  waitForGetDirectory,
  interceptLaunchFile,
  waitForLaunchFile
} from '../support/interceptors';

// Setup complex workflow
interceptFindDevices({ fixture: mockDevices });
interceptConnectDevice();
interceptGetDirectory({ filesystem: mockFilesystem });
interceptLaunchFile({ file: targetFile });

// Execute workflow
cy.visit('/');
waitForFindDevices();

cy.get('[data-testid=device-1]').click();
waitForConnectDevice();

cy.get('[data-testid=browse-files]').click();
waitForGetDirectory();

cy.get('[data-testid=file-to-launch]').click();
waitForLaunchFile();
```

---

## 📚 Reference Documentation

- **[E2E Testing Overview](../../../apps/teensyrom-ui-e2e/E2E_TESTS.md)** - Main E2E testing documentation
- **[Interceptor Format Documentation](./INTERCEPTOR_FORMAT.md)** - Technical format specifications
- **[Phase 1-5 Documentation](./E2E_INTERCEPTOR_REFACTOR_P*.md)** - Complete refactoring documentation
- **[Architecture Summary](./E2E_INTERCEPTOR_ARCHITECTURE.md)** - Final architecture overview

---

## 🤝 Getting Help

If you encounter issues during migration:

1. **Check the interceptor file** - Each file contains JSDoc comments and examples
2. **Look at existing migrated tests** - See how other tests have been updated
3. **Refer to the format documentation** - [INTERCEPTOR_FORMAT.md](./INTERCEPTOR_FORMAT.md)
4. **Ask for help** - The architecture team is available to assist with migration

---

## ✅ Migration Checklist

- [ ] **Identify all old import patterns** in your test files
- [ ] **Update import statements** to use explicit endpoint imports
- [ ] **Replace manual interceptor setup** with helper functions
- [ ] **Update wait patterns** to use endpoint-specific wait functions
- [ ] **Verify error handling** uses new error modes
- [ ] **Test your changes** to ensure functionality is preserved
- [ ] **Remove unused old imports** to clean up code
- [ ] **Update any documentation** that references old patterns

---

**Migration Complete!** 🎉

You've successfully migrated to the new consolidated interceptor architecture. Enjoy improved code organization, better type safety, and enhanced developer experience!