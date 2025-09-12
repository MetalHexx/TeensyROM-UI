# VS Code Plugin Recommendations

This document lists recommended VS Code extensions for developing the TeensyROM Angular Nx monorepo. The recommendations are based on the current project setup, which includes:

- **Frontend**: Angular application with Nx monorepo structure
- **Backend**: ASP.NET Core API (C#)
- **Hardware**: Teensy microcontroller integration (Arduino-based)
- **Tools**: Docker containerization, OpenAPI generation, Mermaid diagrams
- **Standards**: ESLint, Prettier, modern Angular patterns (signals, standalone components)

## Essential Extensions

### Angular Development

- **Angular.ng-template** (`angular.ng-template`)

  - Provides syntax highlighting and IntelliSense for Angular templates
  - Essential for Angular development, supports inline templates and component bindings
  - Critical for this project's Angular UI components

- **Nx Console** (`nrwl.angular-console`)
  - Official Nx extension for VS Code
  - Provides GUI for Nx commands, workspace visualization, and project management
  - Indispensable for managing the Nx monorepo structure and running tasks

### Code Quality & Formatting

- **ESLint** (`dbaeumer.vscode-eslint`)

  - Integrates ESLint for JavaScript/TypeScript linting
  - Enforces coding standards defined in `CODING_STANDARDS.md`
  - Essential for maintaining code quality in the Angular codebase

- **Prettier** (`esbenp.prettier-vscode`)
  - Code formatter for consistent styling
  - Works with the project's formatting configuration
  - Ensures all code follows the same style guidelines

### Documentation & Diagrams

- **Markdown Mermaid** (`bierner.markdown-mermaid`)

  - Renders Mermaid diagrams in Markdown files
  - Essential for viewing the state machine diagrams in `STORAGE_STATE_MACHINE.md`
  - Supports flowchart, sequence, and state diagrams used in documentation

- **Mermaid Markdown Syntax Highlighting** (`bpruitt-goddard.mermaid-markdown-syntax-highlighting`)
  - Syntax highlighting for Mermaid code in Markdown
  - Improves readability when editing diagram code
  - Complements the Mermaid rendering extension

## Backend Development (.NET/C#)

- **C# Dev Kit** (`ms-dotnettools.csdevkit`)

  - Comprehensive C# development toolkit
  - Required for the ASP.NET Core API backend
  - Includes IntelliSense, debugging, and project management

- **C#** (`ms-dotnettools.csharp`)

  - Core C# language support
  - Essential for editing the TeensyRom.Api project

- **.NET Runtime** (`ms-dotnettools.vscode-dotnet-runtime`)
  - Provides .NET runtime for development tasks
  - Required for building and running the C# backend
