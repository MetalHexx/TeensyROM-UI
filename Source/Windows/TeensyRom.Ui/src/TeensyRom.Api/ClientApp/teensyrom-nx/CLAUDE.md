# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Architecture

This is a **hybrid .NET/Angular application** for TeensyROM device management and media playback. The backend is a .NET 9 Web API using **RadEndpoints** for minimal APIs and **MediatR** for CQRS patterns. The frontend is an **Angular 19 application** built with **Nx monorepo** architecture.

### Backend (.NET API)

- **API Layer**: `TeensyRom.Api` - Web API with RadEndpoints, SignalR hubs, and OpenAPI documentation
- **Core Layer**: `TeensyRom.Core` - Domain models, commands, queries, and business logic
- **Device Layer**: `TeensyRom.Core.Device` - Device connection and lifecycle management
- **Serial Layer**: `TeensyRom.Core.Serial` - Serial communication with TeensyROM devices
- **Storage Layer**: `TeensyRom.Core.Storage` - File system and storage operations

### Frontend (Angular/Nx)

- **Monorepo Structure**: Uses Nx workspace with domain-driven library organization
- **App Shell**: Main application in `apps/teensyrom-ui`
- **Domain Libraries**: Business logic organized by domain (`domain/device`, `domain/storage`)
- **Feature Libraries**: UI components and pages (`features/devices`, `features/player`)
- **Shared Libraries**: Reusable UI components (`ui/components`, `ui/styles`)
- **Data Access**: Generated OpenAPI client in `data-access/api-client`

## Common Development Commands

### Backend (.NET API)

```bash
# Build the API
dotnet build

# Run the API (from TeensyRom.Api directory)
dotnet run

# Run tests
dotnet test

# Generate OpenAPI spec (API must be running)
curl http://localhost:5000/openapi.json > openapi.json
```

### Frontend (Angular/Nx)

```bash
# Navigate to Angular workspace
cd ClientApp/teensyrom-nx

# Install dependencies
npm install

# Start development server
npm start
# or
npx nx run teensyrom-ui:serve

# Build for production
npx nx build teensyrom-ui

# Run tests
npx nx test teensyrom-ui

# Run linting
npm run lint
# or
npx nx lint

# Format code
npm run format

# Generate API client (see docs/API_CLIENT_GENERATION.md for details)
pnpm run generate:api-client
```

## API Client Generation

When the .NET API changes and you need to regenerate the TypeScript client:

1. **See detailed instructions**: `docs/API_CLIENT_GENERATION.md`
2. **Quick commands** from Angular workspace (`ClientApp/teensyrom-nx`):

   ```bash
   # Build API and generate OpenAPI spec (no server needed)
   dotnet build ../../TeensyRom.Api.csproj

   # Generate TypeScript client
   pnpm run generate:api-client
   ```

This uses build-time OpenAPI generation - no running server required.

## Key Technologies

### Backend Stack

- **.NET 9**: Target framework
- **RadEndpoints**: Minimal API endpoints with automatic OpenAPI generation
- **MediatR**: CQRS pattern implementation for commands and queries
- **SignalR**: Real-time communication for device logs and events
- **Scalar**: API documentation UI (replaces Swagger)

### Frontend Stack

- **Angular 19**: Latest Angular with standalone components and modern control flow
- **Nx**: Monorepo tooling and build system
- **NgRx Signals**: State management with signal-based stores
- **Angular Material**: UI component library
- **SCSS**: Styling with design tokens
- **Vitest**: Unit testing framework
- **Cypress**: End-to-end testing

## Code Organization Patterns

### Backend Endpoints

- **Location**: `Endpoints/[Domain]/[Action]/`
- **Structure**: Each endpoint has `[Action]Endpoint.cs` and `[Action]Models.cs`
- **Example**: `Endpoints/Files/GetDirectory/GetDirectoryEndpoint.cs`
- **Pattern**: RadEndpoints with explicit configuration and OpenAPI documentation

### Frontend Libraries

- **Domain Services**: `libs/domain/[domain]/services/` - API calls and business logic
- **Domain State**: `libs/domain/[domain]/state/` - NgRx Signal stores with methods
- **Feature Components**: `libs/features/[feature]/` - UI components and pages
- **Shared UI**: `libs/ui/components/` - Reusable presentational components

### Angular Component Standards

- **Use `input()` and `output()`** instead of `@Input()` and `@Output()`
- **Modern control flow**: Use `@if`, `@for`, `@switch` instead of structural directives
- **Standalone components**: Default in Angular 19, import dependencies directly
- **Signal-based state**: Prefer signals over observables where appropriate
- **Component structure**: Follow established ordering (inputs, outputs, properties, lifecycle)

## Development Workflow

### API Changes

1. Modify endpoint or models in `Endpoints/` or `Models/`
2. Update Core layer if business logic changes
3. Run API to generate new OpenAPI spec
4. Regenerate Angular client: `npm run generate:api-client`
5. Update Angular services and state as needed

### Frontend Changes

1. Work within appropriate Nx library boundaries
2. Update domain services for API interactions
3. Update signal stores for state changes
4. Create/modify feature components for UI
5. Run `npm run lint` and `npm run format` before committing

### Testing

- **Backend**: Unit tests in `*.Tests` projects, integration tests in `*.Tests.Integration`
- **Frontend**: Unit tests with Vitest (`npx nx test`), E2E tests with Cypress
- **Linting**: ESLint for TypeScript, follow established coding standards

## File Structure Context

The project is located within a larger TeensyROM solution:

- **Current API**: `TeensyRom.Api` (this directory)
- **Angular App**: `ClientApp/teensyrom-nx/` (Nx workspace)
- **Core Libraries**: Referenced via ProjectReference in `.csproj`
- **Solution Root**: Several directories up from this API project

## Important Notes

- **SignalR Hubs**: Real-time communication for device logs (`/logHub`) and events (`/deviceEventHub`)
- **CORS Configuration**: Configured for Angular dev server in `UiCors` extension
- **Rate Limiting**: Strict rate limiting applied to all endpoints
- **Asset Management**: Automatic asset unpacking on startup
- **API Documentation**: Available at `/scalar/v1` when running
- always warn about the context remaining and before starting a new task. make sure the left over context is enough to complete the task or not. if not, suggest to the user to use compact.
