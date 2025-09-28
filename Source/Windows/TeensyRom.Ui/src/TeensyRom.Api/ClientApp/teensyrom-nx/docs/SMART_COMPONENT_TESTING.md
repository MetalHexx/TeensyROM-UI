# Smart Component Testing Methodology

This document describes the standard methodology for testing Angular smart components that depend on NgRx Signal Stores or Context Services. It focuses on testing components through their public API while mocking store/service dependencies at the injection boundary.

## Overview

**Smart Components** are components that:
- Inject and depend on stores or context services
- Coordinate between multiple data sources
- Handle complex user interactions and workflows
- Manage local UI state in conjunction with global state

**Testing Strategy**:
- Test components through their public interface (inputs, outputs, DOM)
- Mock store/service dependencies using strongly typed mocks
- Focus on component behavior, not store implementation details
- Use Angular TestBed for realistic component instantiation

## Test Types

### Component-Level Testing (Primary)

**Purpose**: Test component behavior with mocked dependencies
- Exercise component inputs, outputs, and user interactions
- Mock stores/services at the injection boundary
- Assert component state, DOM changes, and event emissions
- Cover normal, error, and edge case scenarios

### Integration Testing (Secondary)

**Purpose**: Test component + real store/service integration
- Use real stores with mocked infrastructure services
- Validate complete data flow through component + store layers
- Reserve for complex workflows requiring end-to-end validation

## Mocking Strategy

### Store/Service Mocking Pattern

```typescript
// Example: ProductListComponent depends on ProductContextService

// 1. Define service interface (production code)
export interface IProductContextService {
  loadProducts(categoryId: string): void;
  getCurrentProducts(): Signal<Product[]>;
  isLoading(): Signal<boolean>;
  getError(): Signal<string | null>;
}

export const PRODUCT_CONTEXT_SERVICE = new InjectionToken<IProductContextService>('PRODUCT_CONTEXT_SERVICE');

// 2. Test setup with typed mocks
describe('ProductListComponent', () => {
  let component: ProductListComponent;
  let fixture: ComponentFixture<ProductListComponent>;
  let mockContextService: MockedObject<IProductContextService>;

  beforeEach(async () => {
    // Create strongly typed mock
    mockContextService = {
      loadProducts: vi.fn(),
      getCurrentProducts: vi.fn(() => signal([])),
      isLoading: vi.fn(() => signal(false)),
      getError: vi.fn(() => signal(null)),
    };

    await TestBed.configureTestingModule({
      imports: [ProductListComponent],
      providers: [
        { provide: PRODUCT_CONTEXT_SERVICE, useValue: mockContextService },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(ProductListComponent);
    component = fixture.componentInstance;
  });
});
```

### Signal Mock Utilities

```typescript
// Helper for creating signal mocks
function createSignalMock<T>(initialValue: T): WritableSignal<T> {
  return signal(initialValue);
}

// Setup signal mocks in beforeEach
const productsSignal = createSignalMock<Product[]>([]);
const loadingSignal = createSignalMock(false);
const errorSignal = createSignalMock<string | null>(null);

mockContextService = {
  loadProducts: vi.fn(),
  getCurrentProducts: vi.fn(() => productsSignal.asReadonly()),
  isLoading: vi.fn(() => loadingSignal.asReadonly()),
  getError: vi.fn(() => errorSignal.asReadonly()),
};
```

## Component Testing Patterns

### Input/Output Testing

```typescript
it('should emit product selection when product clicked', () => {
  const selectedProduct = { id: '1', name: 'Test Product' };
  const productSelectedSpy = vi.fn();
  component.productSelected.subscribe(productSelectedSpy);

  // Simulate user interaction
  component.onProductClick(selectedProduct);

  expect(productSelectedSpy).toHaveBeenCalledWith(selectedProduct);
});

it('should load products when category input changes', () => {
  const categoryId = 'electronics';

  component.categoryId.set(categoryId);
  fixture.detectChanges();

  expect(mockContextService.loadProducts).toHaveBeenCalledWith(categoryId);
});
```

### State-Dependent Rendering

```typescript
it('should display loading spinner when loading', () => {
  // Update mock signal
  loadingSignal.set(true);
  fixture.detectChanges();

  const loadingElement = fixture.debugElement.query(By.css('[data-testid="loading-spinner"]'));
  expect(loadingElement).toBeTruthy();
});

it('should display products when loaded', () => {
  const mockProducts = [
    { id: '1', name: 'Product 1' },
    { id: '2', name: 'Product 2' },
  ];

  // Update mock signal
  productsSignal.set(mockProducts);
  fixture.detectChanges();

  const productElements = fixture.debugElement.queryAll(By.css('[data-testid="product-item"]'));
  expect(productElements).toHaveLength(2);
});

it('should display error message when error occurs', () => {
  const errorMessage = 'Failed to load products';

  // Update mock signal
  errorSignal.set(errorMessage);
  fixture.detectChanges();

  const errorElement = fixture.debugElement.query(By.css('[data-testid="error-message"]'));
  expect(errorElement.nativeElement.textContent).toContain(errorMessage);
});
```

### User Interaction Testing

```typescript
it('should trigger product load when refresh button clicked', () => {
  component.categoryId.set('electronics');

  const refreshButton = fixture.debugElement.query(By.css('[data-testid="refresh-button"]'));
  refreshButton.nativeElement.click();

  expect(mockContextService.loadProducts).toHaveBeenCalledWith('electronics');
});

it('should select product when double-clicked', () => {
  const product = { id: '1', name: 'Test Product' };
  const productSelectedSpy = vi.fn();
  component.productSelected.subscribe(productSelectedSpy);

  const productElement = fixture.debugElement.query(By.css('[data-testid="product-item"]'));
  productElement.triggerEventHandler('dblclick', { product });

  expect(productSelectedSpy).toHaveBeenCalledWith(product);
});
```

## Behaviors to Test

Use this checklist to design comprehensive smart component tests:

### 1. Initialization
- [ ] Component initializes with correct default state
- [ ] Required services are injected properly
- [ ] Initial data loading triggered when appropriate
- [ ] Input signals properly bound to component properties

### 2. Input Handling
- [ ] Input changes trigger appropriate service calls
- [ ] Input validation and error handling
- [ ] Multiple input combinations work correctly
- [ ] Input changes update component state appropriately

### 3. Output Events
- [ ] User interactions emit correct output events
- [ ] Event payloads contain expected data
- [ ] Events fired at appropriate times
- [ ] No unwanted event emissions

### 4. State-Dependent Rendering
- [ ] Loading states display appropriate UI
- [ ] Error states show error messages
- [ ] Success states render data correctly
- [ ] Empty states handled gracefully

### 5. User Interactions
- [ ] Click handlers trigger expected actions
- [ ] Form submissions call appropriate services
- [ ] Keyboard interactions work correctly
- [ ] Touch/mobile interactions (if applicable)

### 6. Service Integration
- [ ] Service methods called with correct parameters
- [ ] Service responses handled appropriately
- [ ] Service errors displayed to user
- [ ] Service state changes update component UI

### 7. Complex Workflows
- [ ] Multi-step user workflows complete successfully
- [ ] State transitions between different UI modes
- [ ] Conditional logic based on service state
- [ ] Integration between multiple service dependencies

### 8. Edge Cases
- [ ] Null/undefined input handling
- [ ] Empty data sets handled gracefully
- [ ] Network failures and recovery
- [ ] Rapid user interactions don't cause issues

## Integration Testing Approach

For complex components requiring end-to-end validation:

```typescript
describe('ProductListComponent Integration', () => {
  let component: ProductListComponent;
  let contextService: ProductContextService;
  let mockInfrastructureService: MockedObject<IProductService>;

  beforeEach(async () => {
    mockInfrastructureService = {
      getProducts: vi.fn(),
      getProduct: vi.fn(),
    };

    await TestBed.configureTestingModule({
      imports: [ProductListComponent],
      providers: [
        ProductContextService,
        ProductStore,
        { provide: PRODUCT_SERVICE, useValue: mockInfrastructureService },
      ],
    }).compileComponents();

    contextService = TestBed.inject(ProductContextService);
    fixture = TestBed.createComponent(ProductListComponent);
    component = fixture.componentInstance;
  });

  it('should complete full product loading workflow', async () => {
    const mockProducts = [{ id: '1', name: 'Test Product' }];
    mockInfrastructureService.getProducts.mockReturnValue(of(mockProducts));

    component.categoryId.set('electronics');
    fixture.detectChanges();

    // Wait for async operations
    await fixture.whenStable();
    fixture.detectChanges();

    expect(component.getCurrentProducts()()).toEqual(mockProducts);
    expect(component.isLoading()()).toBe(false);
  });
});
```

## Do / Don't

### Do
- Test through component's public interface (inputs, outputs, DOM)
- Use strongly typed mocks for service dependencies
- Test user interactions and their effects
- Assert on DOM changes and component state
- Use TestBed for realistic component instantiation
- Test error scenarios and edge cases

### Don't
- Test store/service implementation details in component tests
- Use real HTTP calls in component unit tests
- Test internal component methods directly
- Mock Angular framework features unnecessarily
- Ignore accessibility in component tests
- Test component and store together in unit tests (use integration tests)

## Quick Checklist

- [ ] TestBed setup with mocked service dependencies
- [ ] Strongly typed service mocks
- [ ] Component initialization behavior
- [ ] Input handling and validation
- [ ] Output event emissions
- [ ] State-dependent rendering
- [ ] User interaction handling
- [ ] Service integration points
- [ ] Complex workflow scenarios
- [ ] Edge cases and error handling
- [ ] Integration tests for critical workflows

## Example Test Structure

```typescript
describe('ExampleSmartComponent', () => {
  let component: ExampleSmartComponent;
  let fixture: ComponentFixture<ExampleSmartComponent>;
  let mockContextService: MockedObject<IExampleContextService>;

  beforeEach(async () => {
    // Setup mocks and TestBed
  });

  describe('Initialization', () => {
    // Initialization tests
  });

  describe('Input Handling', () => {
    // Input processing tests
  });

  describe('User Interactions', () => {
    // User interaction tests
  });

  describe('State-Dependent Rendering', () => {
    // UI state tests
  });

  describe('Service Integration', () => {
    // Service coordination tests
  });

  describe('Error Handling', () => {
    // Error scenario tests
  });
});
```

This methodology ensures comprehensive testing of smart components while maintaining clear separation between component behavior and service implementation details.