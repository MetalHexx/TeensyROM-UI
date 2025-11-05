# Repository Guidelines

## Architecture Overview

- Hybrid solution: .NET 9 Web API backend and Angular 19 Nx frontend.
- Clean Architecture layers (domain -> application -> infrastructure -> presentation) are defined and enforced via tags; see `docs/OVERVIEW_CONTEXT.md`.
- API project: `TeensyRom.Api` (RadEndpoints, MediatR, SignalR) exposes Scalar docs at `/scalar/v1`. Frontend workspace lives under `ClientApp/teensyrom-nx` (this directory).

- App: `apps/teensyrom-ui` (source in `src/`, assets in `public/`). Cypress scaffold: `apps/teensyrom-ui-e2e`.
- Libraries in `libs/`: `domain`, `application`, `features/{devices,player}`, `infrastructure`, `ui/{components,styles}`, `app/{bootstrap,navigation,shell}`, `utils`, `data-access/api-client`.
- ESLint module boundaries (see `eslint.config.mjs`) enforce `scope:*` rules; avoid cross-feature imports (`feature:device <-> feature:player`).

## Build, Test, and Development Commands

- Install: `pnpm install`.
- Serve: `pnpm start` (`nx run teensyrom-ui:serve`, port 4200).
- Build: `pnpm nx build teensyrom-ui`; static preview: `pnpm nx run teensyrom-ui:serve-static`.
- Unit tests (Vitest): `pnpm nx test <project>`.
- Lint/format: `pnpm nx lint`; `pnpm run format`.
- API client: `dotnet build ../../TeensyRom.Api.csproj` then `pnpm run generate:api-client` (details in `docs/API_CLIENT_GENERATION.md`).
- Backend: run `dotnet build`, `dotnet run`, `dotnet test` from `TeensyRom.Api`.

## Angular Standards & Style

- EditorConfig: 2 spaces, trim whitespace; Prettier: single quotes, width 100, trailing commas `es5`.
- Angular 19 conventions: standalone components, `input()`/`output()`, `@if/@for/@switch` control flow.
- Favor NgRx Signal Store, SCSS modules, and Angular Material primitives.
- Maintain clean imports through barrels; use kebab-case files and PascalCase classes.

## Testing Guidelines

- Unit: colocate `*.spec.ts`, prefer MSW for HTTP mocking (`docs/OVERVIEW_CONTEXT.md`).
- E2E: add Cypress specs under `apps/teensyrom-ui-e2e/src`.
- Backend: run `dotnet test` for API projects.

## Commit & Pull Request Guidelines

- Use Conventional Commits (`feat`, `fix`, `refactor`, `docs`, `chore`, optional scopes like `feat(player): ...`).
- PRs need intent, linked issues, UI screenshots when relevant, and breaking-change notes.
- Run `pnpm nx lint` and `pnpm nx test -w`; confirm module-boundary linting stays green.

## UI & Styling Resources

- Component catalog (selectors, inputs, animation wrappers) is in `docs/COMPONENT_LIBRARY.md`; consult before adding to `libs/ui/components`.
- Global styles, utility classes like `.dimmed`/`.glassy`, and Material overrides live in `docs/STYLE_GUIDE.md`.
- Presentation components stay logic-free and compose application stores (reinforced in `docs/OVERVIEW_CONTEXT.md`).

## Security & Configuration Tips

- Do not edit generated API clients; regenerate via the script and review `openapitools.json`.
- Keep secrets out of the repo; prefer backend configuration or environment-specific providers.

## Reference Docs

- `docs/OVERVIEW_CONTEXT.md`
- `docs/API_CLIENT_GENERATION.md`
- `docs/COMPONENT_LIBRARY.md`
- `docs/STYLE_GUIDE.md`
