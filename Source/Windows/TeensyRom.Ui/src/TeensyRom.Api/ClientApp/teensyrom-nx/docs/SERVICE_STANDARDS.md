## Domain Services Configuration

### Goal

Provide a clean, testable boundary for domain services by exposing an interface and an InjectionToken, while wiring the concrete class at the application shell. This enables strong typing, easy mocking in unit tests, and swappable implementations if needed.

### Pattern

1. Define an interface and token in the service file (or a colocated `*.tokens.ts`).

```ts
// domain services (example)
export interface IExampleService {
  getData(id: string): Observable<ExampleModel>;
}

export const EXAMPLE_SERVICE = new InjectionToken<IExampleService>('EXAMPLE_SERVICE');

@Injectable({ providedIn: 'root' })
export class ExampleService implements IExampleService {
  constructor(private readonly api: ApiClient) {}
  getData(id: string) {
    /* ... */
  }
}

export const EXAMPLE_SERVICE_PROVIDER = {
  provide: EXAMPLE_SERVICE,
  useExisting: ExampleService,
};
```

2. Barrel export the interface, token, and provider.

```ts
// libs/domain/example/services/src/index.ts
export * from './lib/example.service';
export { IExampleService, EXAMPLE_SERVICE, EXAMPLE_SERVICE_PROVIDER } from './lib/example.service';
```

3. Wire the provider in the application shell.

```ts
// apps/app/src/app/app.config.ts
providers: [, /* other providers */ EXAMPLE_SERVICE_PROVIDER];
```

4. Inject the token at usage sites (e.g., stores) to depend on the interface rather than the concrete class.

```ts
export const ExampleStore = signalStore(
  { providedIn: 'root' },
  withState(initialState),
  withMethods((store, service: IExampleService = inject(EXAMPLE_SERVICE)) => ({
    // ... methods using service
  }))
);
```

### Testing

- In unit tests, provide the interface token with a small, strongly typed mock:

```ts
type GetDataFn = (id: string) => Observable<ExampleModel>;
let getDataMock: MockedFunction<GetDataFn>;

TestBed.configureTestingModule({
  providers: [
    { provide: EXAMPLE_SERVICE, useValue: { getData: (getDataMock = vi.fn<GetDataFn>()) } },
  ],
});
```
