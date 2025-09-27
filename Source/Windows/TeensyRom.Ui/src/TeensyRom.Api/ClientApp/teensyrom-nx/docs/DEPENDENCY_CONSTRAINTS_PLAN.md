# Library Dependency Constraints Plan

## Overview

This plan establishes strict dependency constraints using Nx tags and ESLint rules to enforce Clean Architecture principles and prevent architectural violations.

## Current Analysis

**Good News**: You already have:
- ✅ ESLint with `@nx/enforce-module-boundaries` configured
- ✅ One library (`ui-styles`) properly tagged with `["scope:shared", "type:ui"]`
- ✅ Clean architecture folder structure

**Needs Setup**:
- 🏷️ Most libraries missing proper tags
- 📋 Dependency constraints need to be defined
- 🔒 Layer-specific access rules need enforcement

---

## Tagging Strategy

### Two-Dimensional Tagging System

#### 1. **Type Tags** (Architectural Layer)
- `type:domain` - Domain layer (contracts, models)
- `type:application` - Application layer (use cases, state management)
- `type:infrastructure` - Infrastructure layer (implementations)
- `type:data-access` - Data access layer (API clients)
- `type:feature` - Feature modules (bounded contexts)
- `type:ui` - UI components and styling
- `type:app` - App-specific modules (shell, bootstrap, navigation)
- `type:util` - Shared utilities

#### 2. **Scope Tags** (Domain/Functional Area)
- `scope:shared` - Libraries used across the entire application
- `scope:device` - Device-related functionality
- `scope:player` - Player-related functionality
- `scope:storage` - Storage-related functionality
- `scope:app` - App-specific concerns

---

## Proposed Library Tags

### Core Architecture Layers
```typescript
// Domain Layer
domain: ["type:domain", "scope:shared"]

// Application Layer  
application: ["type:application", "scope:shared"]

// Infrastructure Layer
infrastructure: ["type:infrastructure", "scope:shared"]

// Data Access Layer
api-client: ["type:data-access", "scope:shared"]

// Utilities
utils: ["type:util", "scope:shared"]
```

### Feature Modules
```typescript
// Feature Libraries
features-device: ["type:feature", "scope:device"]
player: ["type:feature", "scope:player"]
```

### UI Libraries
```typescript
// UI Components & Styling
ui-components: ["type:ui", "scope:shared"]
ui-styles: ["type:ui", "scope:shared"] // Already tagged correctly
```

### App-Specific Libraries
```typescript
// App Structure
app-shell: ["type:app", "scope:app"]
app-bootstrap: ["type:app", "scope:app"]
app-navigation: ["type:app", "scope:app"]
```

---

## Clean Architecture Dependency Rules

### Layer Access Rules (Type-Based)

1. **Domain Layer** (`type:domain`)
   - ✅ Can import: Nothing (pure business logic)
   - ❌ Cannot import: Any other layers

2. **Application Layer** (`type:application`)
   - ✅ Can import: `type:domain`, `type:util`
   - ❌ Cannot import: `type:infrastructure`, `type:data-access`, `type:feature`, `type:ui`, `type:app`

3. **Infrastructure Layer** (`type:infrastructure`)
   - ✅ Can import: `type:domain`, `type:application`, `type:data-access`, `type:util`
   - ❌ Cannot import: `type:feature`, `type:ui`, `type:app`

4. **Data Access Layer** (`type:data-access`)
   - ✅ Can import: `type:util`
   - ❌ Cannot import: `type:domain`, `type:application`, `type:infrastructure`, `type:feature`, `type:ui`, `type:app`

5. **Feature Layer** (`type:feature`)
   - ✅ Can import: `type:domain`, `type:application`, `type:infrastructure`, `type:data-access`, `type:ui`, `type:util`
   - ❌ Cannot import: `type:app`

6. **UI Layer** (`type:ui`)
   - ✅ Can import: `type:ui`, `type:util`
   - ❌ Cannot import: `type:domain`, `type:application`, `type:infrastructure`, `type:data-access`, `type:feature`, `type:app`

7. **App Layer** (`type:app`)
   - ✅ Can import: All types (top-level orchestration)

8. **Utilities** (`type:util`)
   - ✅ Can import: Nothing (pure utilities)
   - ❌ Cannot import: Any other layers

### Scope Access Rules (Domain-Based)

1. **Shared Scope** (`scope:shared`)
   - ✅ Can be imported by: Anyone
   - ✅ Can import from: Only `scope:shared`

2. **Feature Scopes** (`scope:device`, `scope:player`, `scope:storage`)
   - ✅ Can import from: Same scope + `scope:shared`
   - ❌ Cannot import from: Other feature scopes

3. **App Scope** (`scope:app`)
   - ✅ Can import from: All scopes (top-level orchestration)

---

## ESLint Dependency Constraints Configuration

```javascript
depConstraints: [
  // === CLEAN ARCHITECTURE LAYER RULES ===
  
  // 1. Domain Layer - Pure business logic (no dependencies)
  {
    sourceTag: 'type:domain',
    onlyDependOnLibsWithTags: [] // Cannot depend on anything
  },
  
  // 2. Application Layer - Use cases and state management
  {
    sourceTag: 'type:application',
    onlyDependOnLibsWithTags: ['type:domain', 'type:util']
  },
  
  // 3. Infrastructure Layer - Implementations
  {
    sourceTag: 'type:infrastructure',
    onlyDependOnLibsWithTags: [
      'type:domain', 
      'type:application', 
      'type:data-access', 
      'type:util'
    ]
  },
  
  // 4. Data Access Layer - External API clients
  {
    sourceTag: 'type:data-access',
    onlyDependOnLibsWithTags: ['type:util']
  },
  
  // 5. Feature Layer - Feature modules
  {
    sourceTag: 'type:feature',
    onlyDependOnLibsWithTags: [
      'type:domain',
      'type:application', 
      'type:infrastructure',
      'type:data-access',
      'type:ui',
      'type:util'
    ]
  },
  
  // 6. UI Layer - Components and styling
  {
    sourceTag: 'type:ui',
    onlyDependOnLibsWithTags: ['type:ui', 'type:util']
  },
  
  // 7. App Layer - Top-level orchestration (can access everything)
  {
    sourceTag: 'type:app',
    onlyDependOnLibsWithTags: ['*']
  },
  
  // 8. Utilities - Pure utilities (no dependencies)
  {
    sourceTag: 'type:util',
    onlyDependOnLibsWithTags: [] // Cannot depend on anything
  },
  
  // === SCOPE-BASED RULES ===
  
  // Shared scope can only import from shared scope
  {
    sourceTag: 'scope:shared',
    onlyDependOnLibsWithTags: ['scope:shared']
  },
  
  // Device scope can import from device and shared
  {
    sourceTag: 'scope:device',
    onlyDependOnLibsWithTags: ['scope:device', 'scope:shared']
  },
  
  // Player scope can import from player and shared  
  {
    sourceTag: 'scope:player',
    onlyDependOnLibsWithTags: ['scope:player', 'scope:shared']
  },
  
  // Storage scope can import from storage and shared
  {
    sourceTag: 'scope:storage',
    onlyDependOnLibsWithTags: ['scope:storage', 'scope:shared']
  },
  
  // App scope can import from all scopes (top-level)
  {
    sourceTag: 'scope:app',
    onlyDependOnLibsWithTags: ['*']
  }
]
```

---

## Benefits

### 🏗️ **Architectural Integrity**
- Enforces Clean Architecture layer separation
- Prevents business logic leaking into infrastructure
- Maintains dependency inversion principle

### 🔒 **Bounded Contexts** 
- Feature modules cannot depend on each other
- Shared libraries are clearly identified
- Domain isolation is maintained

### 🚀 **Maintainability**
- Clear dependency rules prevent coupling
- ESLint catches violations during development
- Self-documenting architecture through tags

### 📈 **Scalability**
- New features follow established patterns
- Easy to add new bounded contexts
- Clear guidelines for library placement

---

## Implementation Steps

1. **Add Tags to All Libraries** (15-20 minutes)
2. **Update ESLint Configuration** (10 minutes)  
3. **Test Constraints** (10 minutes)
4. **Fix Any Violations** (Variable - depends on current violations)
5. **Document Guidelines** (5 minutes)

---

## Risk Assessment

**Low Risk**:
- Tags are metadata only (non-breaking)
- ESLint rules provide immediate feedback
- Can be implemented incrementally
- Rollback is simple (remove tags and constraints)

**High Value**:
- Prevents architectural drift
- Improves code quality
- Enforces team standards
- Reduces review overhead

---

## Testing the Implementation

After setup, intentionally violate rules to test:

```typescript
// In infrastructure library - should fail
import { SomeFeature } from '@teensyrom-nx/player'; // ❌ Infrastructure cannot import features

// In domain library - should fail  
import { ApiClient } from '@teensyrom-nx/api-client'; // ❌ Domain cannot import anything

// In ui library - should fail
import { DeviceStore } from '@teensyrom-nx/application'; // ❌ UI cannot import application layer
```

Run `nx run-many --target=lint --all` to see ESLint catch violations.