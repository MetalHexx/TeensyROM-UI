---
description: Validate code changes with git diff, linting, type checking, and tests
---

# Code Validation Workflow

## 1. Establish Test Baseline
Running initial test baseline to establish current state...

```bash
echo "🔍 Establishing test baseline..."
npx nx test teensyrom-ui --runInBand --coverage=false
```

## 2. Show Git Changes
Displaying what files have changed...

```bash
echo "📝 Showing git diff..."
git diff --name-only HEAD~1
git diff
```

## 3. Run Linting
Checking for linting errors...

```bash
echo "✨ Running linting..."
npx nx lint teensyrom-ui
```

## 4. TypeScript Type Checking
Validating TypeScript compilation...

```bash
echo "🔷 Running TypeScript type check..."
npx nx typecheck teensyrom-ui
```

## 5. Test Affected Projects
Re-running tests on changed code...

```bash
echo "🧪 Running tests on affected projects..."
npx nx test --affected
```

## Summary
All validations complete! Fix any errors shown above before committing your changes.