# Rule: Nx Library Move/Rename/Path Change Consistency

Whenever you move, rename, or change the path alias of a library in an Nx workspace, follow this checklist to ensure all references and dependencies are updated consistently:

---

## 1. Update the Path Alias in `tsconfig.base.json`

- Ensure the alias matches the new location and intended import path.
- Remove any obsolete aliases for the old location.

## 2. Update the Project Name in `project.json` (if renaming)

- If the project name changes, update the `"name"` field in the library's `project.json`.

## 3. Update All References in the Codebase

- **Imports:**
  - Search for all import statements using the old alias and update them to the new alias.
- **Implicit Dependencies:**
  - Update any `implicitDependencies` in `project.json` or `nx.json` that reference the old project name or alias.
- **Other Nx Configs:**
  - Update any other Nx configuration files (e.g., `nx.json`, `workspace.json`) that reference the old project name or alias.

## 4. Automated Search & Replace

- Use a global search (e.g., `grep`, `ripgrep`, or your IDE's search) for the old alias and project name.
- Replace all occurrences with the new alias or project name.

## 5. Test the Workspace

- Run `nx graph` or `nx affected:graph` to ensure the project graph is valid.
- Run `nx build`, `nx test`, and `nx lint` for the affected projects to ensure everything works.

## 6. Document the Change (Optional but Recommended)

- If working in a team, document the change in a commit message or a migration note.

---

## Example Workflow

Suppose you move `libs/domain/devices/services` to `libs/services/device` and want the alias to be `@teensyrom-nx/services/device`:

- Update `tsconfig.base.json`:
  ```json
  "@teensyrom-nx/services/device": ["libs/services/device/src/index.ts"]
  ```
- Update all imports:
  ```diff
  -import { DeviceService } from '@teensyrom-nx/domain/devices/services';
  +import { DeviceService } from '@teensyrom-nx/services/device';
  ```
- Update `implicitDependencies` and any Nx config references.
- Test the workspace.
