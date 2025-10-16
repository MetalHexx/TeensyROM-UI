# TeensyROM-UI Agent Guidelines

## Project Overview

TeensyROM-UI is a hybrid .NET/Angular application that provides a modern desktop interface for controlling Commodore 64/128 computers via the TeensyROM hardware cartridge. This project combines a .NET 9 Web API backend with an Angular 19 frontend using Nx monorepo architecture and Clean Architecture principles.

## Architecture & Technology Stack

### Backend (.NET 9 Web API)
- **Location**: `Source/Windows/TeensyRom.Ui/src/TeensyRom.Api/`
- **Framework**: .NET 9 with RadEndpoints minimal APIs
- **Patterns**: MediatR for CQRS, SignalR for real-time communication
- **Documentation**: Scalar API docs at `/scalar/v1`
- **Key Projects**:
  - `TeensyRom.Api` - Web API with endpoints
  - `TeensyRom.Core` - Core business logic and domain models
  - `TeensyRom.Core.Device` - Hardware device communication
  - `TeensyRom.Core.Serial` - Serial port handling
  - `TeensyRom.Core.Storage` - File system operations

### Frontend (Angular 19 + Nx)
- **Location**: `Source/Windows/TeensyRom.Ui/src/TeensyRom.Api/ClientApp/teensyrom-nx/`
- **Framework**: Angular 19 with standalone components
- **Architecture**: Clean Architecture with Nx workspace
- **State Management**: NgRx Signal Store
- **UI Library**: Angular Material with custom theming
- **Package Manager**: PNPM (required)

## Development Workflow Guidelines

### Prerequisites & Setup
1. **Required Tools**:
   - .NET 9 SDK (preview versions acceptable)
   - Node.js 18+ with PNPM package manager
   - Visual Studio 2022 or VS Code with C# Dev Kit

2. **Initial Setup**:
   ```bash
   # Backend setup
   cd Source/Windows/TeensyRom.Ui
   dotnet restore
   
   # Frontend setup  
   cd src/TeensyRom.Api/ClientApp/teensyrom-nx
   pnpm install
   ```

### Development Commands

#### Backend Development
```bash
# From: Source/Windows/TeensyRom.Ui/
dotnet build                                    # Build entire solution
dotnet run --project src/TeensyRom.Api        # Start API server (port 5000/5001)
dotnet test                                    # Run all .NET tests
```

#### Frontend Development
```bash
# From: src/TeensyRom.Api/ClientApp/teensyrom-nx/
pnpm start                                     # Development server (port 4200)
pnpm nx build teensyrom-ui                     # Production build
pnpm nx lint                                   # Lint all projects
pnpm nx test <project>                         # Run unit tests for specific project
pnpm run format                                # Format all files with Prettier
```

#### API Client Generation
```bash
# After backend API changes - from teensyrom-nx/
dotnet build ../../TeensyRom.Api.csproj
pnpm run generate:api-client
```

### Clean Architecture Enforcement

#### Module Boundary Rules (ESLint Enforced)
The project uses strict ESLint rules to enforce Clean Architecture:

1. **Domain Layer** (`libs/domain`): 
   - Tag: `scope:domain`
   - Dependencies: None (pure TypeScript)
   - Contains: Models, contracts, business rules

2. **Application Layer** (`libs/application`):
   - Tag: `scope:application`
   - Dependencies: Domain, shared utilities only
   - Contains: Use cases, state management

3. **Infrastructure Layer** (`libs/infrastructure`):
   - Tag: `scope:infrastructure`
   - Dependencies: Domain, shared, api-client
   - Contains: HTTP clients, SignalR, external integrations

4. **Features Layer** (`libs/features/*`):
   - Tags: `scope:features`, `feature:device|player`
   - Dependencies: Application, domain, shared UI
   - **Isolation Rule**: Features cannot import from each other

5. **App Layer** (`apps/teensyrom-ui`, `libs/app/*`):
   - Tag: `scope:app`
   - Dependencies: All layers (composition root)
   - Contains: Application shell, bootstrapping, routing

#### Architectural Violations
- ESLint will **fail the build** if dependency rules are violated
- Always run `pnpm nx lint` before committing
- Check module boundary errors in ESLint output

### Code Standards & Conventions

#### Angular Standards
- **Components**: Use Angular 19 standalone components
- **Control Flow**: Use `@if`, `@for`, `@switch` syntax (not structural directives)
- **Inputs/Outputs**: Use `input()` and `output()` functions
- **State Management**: NgRx Signal Store for reactive state
- **Styling**: SCSS modules with Angular Material theming

#### .NET Standards
- **APIs**: Use RadEndpoints for minimal API endpoints
- **Commands/Queries**: MediatR handlers with CQRS pattern
- **Real-time**: SignalR hubs for device events and logging
- **Domain Logic**: Keep pure in TeensyRom.Core projects

#### File Organization
- **Frontend**: Kebab-case files, PascalCase classes
- **Backend**: PascalCase C# conventions
- **Exports**: Use barrel exports (index.ts) for clean imports
- **Co-location**: Place specs next to implementation files

### Testing Guidelines

#### Frontend Testing
- **Unit Tests**: Vitest framework, co-located `*.spec.ts` files
- **Mocking**: Use MSW (Mock Service Worker) for HTTP mocking
- **E2E Tests**: Cypress in `apps/teensyrom-ui-e2e/`
- **Component Testing**: Angular Testing Library patterns

#### Backend Testing
- **Unit Tests**: NUnit framework in separate test projects
- **Integration Tests**: TestContainers for database/external dependencies
- **API Testing**: Test RadEndpoints with WebApplicationFactory

### Git & Commit Guidelines

#### Commit Messages (Conventional Commits)
```
feat(player): add continuous SID playback functionality
fix(device): resolve serial port connection timeout issues
refactor(api): migrate to RadEndpoints minimal APIs
docs(readme): update installation instructions
chore(deps): update Angular to version 19
```

#### Branch Strategy
- **Main Branch**: `master` (stable releases)
- **Feature Branches**: `feat/feature-name`
- **Bug Fixes**: `fix/issue-description`
- **Documentation**: `docs/topic-name`

#### Pre-commit Hooks (Husky)
- Automatic linting and formatting
- Ensure tests pass before commit
- Validate commit message format

### Build & Deployment

#### Production Build Process
```bash
# Backend build
cd Source/Windows/TeensyRom.Ui
dotnet publish src/TeensyRom.Api -c Release

# Frontend build  
cd src/TeensyRom.Api/ClientApp/teensyrom-nx
pnpm nx build teensyrom-ui --configuration=production
```

#### Release Pipeline
- GitHub Actions for automated builds
- Self-contained .NET deployment with embedded runtime
- Windows executable packaging with assets
- Manual distribution via GitHub Releases

### Domain-Specific Guidelines

#### TeensyROM Hardware Integration
- **Serial Communication**: Use `TeensyRom.Core.Serial` abstractions
- **Device Discovery**: Automatic port detection and connection management
- **File Operations**: All storage operations through `TeensyRom.Core.Storage`
- **Command Patterns**: Use MediatR commands for device interactions

#### Media Management Features
- **SID Music**: HVSC metadata integration with accurate timing
- **Games**: OneLoad64 compatibility and rapid launching
- **File Transfer**: Drag-drop and watch directory synchronization
- **Search**: Light search engine syntax with metadata querying

#### Real-time Communication
- **SignalR Hubs**: Device events, logs, and status updates
- **Frontend Subscriptions**: Use infrastructure layer services
- **Connection Management**: Automatic reconnection and error handling

### Security & Performance

#### Security Considerations
- **Serial Port Access**: May require elevated permissions
- **File System Access**: Validate all file operations
- **API Endpoints**: Rate limiting configured in backend
- **CORS Policy**: Properly configured for Angular dev server

#### Performance Guidelines
- **Large Collections**: HVSC indexing may be time-intensive
- **File Transfers**: Optimize for large file operations
- **Memory Management**: Dispose of serial port resources properly
- **Caching**: Use Nx computation caching for builds

### Troubleshooting Common Issues

#### Development Environment
- **File Locking**: Close Visual Studio during CLI builds to avoid lock conflicts
- **Port Conflicts**: Ensure TeensyROM device not connected during development
- **Module Boundaries**: Check ESLint output for architecture violations
- **API Client Sync**: Regenerate client after backend API changes

#### Runtime Issues  
- **Windows Defender**: Application requires security exception (unsigned)
- **Serial Port Access**: Administrator privileges may be needed
- **USB Performance**: Large files transfer faster via SD card removal
- **HVSC Loading**: Large metadata collections require patience during indexing

### Documentation Standards

#### Code Documentation
- **JSDoc**: Document public APIs and complex logic
- **README Files**: Each library should have usage instructions
- **Architecture Decisions**: Document major design choices
- **API Documentation**: Scalar auto-generates from code annotations

#### User Documentation
- **Feature Guides**: Document user-facing functionality
- **Installation Instructions**: Keep README.md current
- **Video Tutorials**: YouTube demonstrations for complex workflows
- **Troubleshooting**: Common issues and solutions

### Integration Points

#### External Systems
- **HVSC Database**: SID metadata and composer information
- **DeepSID Integration**: Composer images and extended metadata
- **OneLoad64**: Game screenshots and optimized loading
- **VICE Emulator**: Future integration possibilities

#### Hardware Dependencies
- **TeensyROM Cartridge**: Primary hardware interface
- **Commodore 64/128**: Target platforms
- **Storage Media**: SD cards and USB drives
- **Serial Connection**: USB micro-B cable required

## Project-Specific Notes

### Known Limitations
- **Windows Only**: Currently Windows-exclusive (future: cross-platform)
- **Code Signing**: Unsigned executable causes security warnings
- **Hardware Required**: Full functionality requires TeensyROM hardware
- **Large Collections**: HVSC indexing performance with massive libraries

### Future Roadmap
- **Cross-platform Support**: macOS and Linux compatibility
- **Auto-updater**: Seamless application updates
- **Plugin Architecture**: Extensible third-party integrations
- **Mobile Companion**: Remote control capabilities
- **Enhanced Metadata**: Expanded content information

---

## Working with This Project

When working on TeensyROM-UI, always:

1. **Follow Clean Architecture**: Respect layer boundaries and dependency rules
2. **Run Linting**: Use `pnpm nx lint` to catch architectural violations
3. **Test Thoroughly**: Both unit and integration testing required
4. **Document Changes**: Update relevant documentation for new features
5. **Consider Hardware**: Remember the physical TeensyROM integration
6. **Optimize Performance**: Large file collections impact user experience

The project balances modern development practices with the unique requirements of retro computing hardware integration. Maintain this balance when making changes or additions.