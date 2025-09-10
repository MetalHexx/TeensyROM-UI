# Domain Standards

## Overview

This document establishes standards for domain library organization and documentation in the TeensyROM Angular application.

---

## Domain Overview Documentation

### Requirement

**Standard**: Create a `[DOMAIN-NAME]_DOMAIN.md` file for each domain library to provide lightweight context

**Purpose**: Enable efficient understanding without reading implementation files

**Location**: `libs/domain/[domain]/[DOMAIN-NAME]_DOMAIN.md`

**Naming**: Use UPPER_CASE with domain name (e.g., `STORAGE_DOMAIN.md`, `DEVICE_DOMAIN.md`)

### Format

```markdown
# Domain Name Overview

## Purpose

Brief description of domain responsibility

## Architecture

- Services: What they do (not how)
- State: What it manages (not implementation)

## Key Classes/Methods

- Class/Method names with purpose (no implementation details)

## Key Concepts

- Important patterns or design decisions
```

### Requirements

- Focus on **what** not **how**
- Include class/method names and purposes
- Mention key architectural patterns (Signal Store, flat state, etc.)
- Keep concise for AI context efficiency
- Update when domain structure changes

### Benefits

- **Faster context building** - Quick understanding without file exploration
- **Better human onboarding** - Clear entry point for new developers
- **Reduced token usage** - Efficient AI context loading
- **Documentation consistency** - Standard format across domains

### Example

See [`libs/domain/storage/STORAGE_DOMAIN.md`](../libs/domain/storage/STORAGE_DOMAIN.md) for a complete implementation following this standard.

---

## Domain Library Structure

### Standard Organization

```
libs/domain/[domain]/
├── [DOMAIN-NAME]_DOMAIN.md     # Required overview documentation (e.g., STORAGE_DOMAIN.md)
├── services/                   # Domain services and models
│   ├── src/lib/
│   │   ├── *.service.ts       # API communication
│   │   ├── *.mapper.ts        # API to domain mapping
│   │   └── *.models.ts        # Domain types and enums
│   └── src/index.ts           # Public API exports
└── state/                     # State management (if needed)
    ├── src/lib/
    │   ├── *-store.ts         # NgRx Signal Store
    │   ├── *.util.ts          # Store utilities
    │   └── methods/           # Store methods (one per file)
    └── src/index.ts           # Public API exports
```

### Naming Conventions

- Domain folders use singular names (`storage`, `device`, `user`)
- Service files end with `.service.ts`
- Mapper files end with `.mapper.ts`
- Model files end with `.models.ts`
- Store files end with `-store.ts`

---

## Related Documentation

- **State Standards**: [`STATE_STANDARDS.md`](./STATE_STANDARDS.md) - NgRx Signal Store patterns
- **Coding Standards**: [`CODING_STANDARDS.md`](./CODING_STANDARDS.md) - General coding conventions
- **Nx Library Standards**: [`NX_LIBRARY_STANDARDS.md`](./NX_LIBRARY_STANDARDS.md) - Library creation patterns
