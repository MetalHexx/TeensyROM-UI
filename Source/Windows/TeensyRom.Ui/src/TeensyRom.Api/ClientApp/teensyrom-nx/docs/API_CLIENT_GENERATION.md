# API Client Generation

Simple steps to regenerate the TypeScript HTTP client from the .NET API.

## Prerequisites

- .NET 9 API configured for build-time OpenAPI generation
- Node.js and OpenAPI Generator CLI installed
- pnpm package manager

## Steps

### 1. Build the API Project

From the Angular workspace (`ClientApp/teensyrom-nx`):

```bash
dotnet build ../../TeensyRom.Api.csproj
```

This generates the OpenAPI specification file in the API's root folder.

### 2. Generate the HTTP Client

```bash
pnpm run generate:api-client
```

## Notes

- The OpenAPI spec is generated to `../../api-spec/TeensyRom.Api.json` during build
- Client generation script is in `libs/data-access/api-client/scripts/generate-client.js`
- Generated services use `*ApiService` naming convention (e.g., `DevicesApiService`)
- Output location: `libs/data-access/api-client/src/lib/`
- Build-time generation requires no running server
