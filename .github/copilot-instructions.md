### Quick orientation — Undercover (Blazor WASM) repo (current)

Goal
- Single-page Blazor WebAssembly client for the "Undercover" rounds game. Runtime is browser-side (local pass-and-play). Intended for static deployment (GitHub Pages) but also runnable locally with `dotnet run` during development.

At-a-glance (important updates)
- The entire app is a single-page UI in `undercover-client/Pages/LocalGame.razor` (`@page "/").` That file contains page-local UI and game logic — most behavior is implemented in that Razor file and the local services.
- Word pairs now include a language code per category in `wwwroot/WordPairs.yaml` (e.g. `language: "EN"` or `language: "NL"`). Categories are grouped/filtered by language in the UI.
- YAML models use neutral pair fields `wordA`/`wordB` (mapped to `YamlWordPair.WordA` / `YamlWordPair.WordB`) and the runtime randomly assigns `WordA`/`WordB` to `WordPair.Civilian` / `WordPair.Undercover` so the YAML file doesn't contain `Civilian`/`Undercover` names.
- `Services/WordPairClientService.cs` reads `wwwroot/WordPairs.yaml` as the primary source (with a small set of hard-coded fallbacks). It attempts HTTP reads from the app base address and falls back to the local wwwroot file when necessary.

Project layout (what to open first)
- `undercover-client/Pages/LocalGame.razor` — UI entry and main logic for setup, rounds, voting, and results.
- `undercover-client/Services/WordPairClientService.cs` — loads YAML, converts YAML models into runtime `WordPairCategory` objects, and randomizes the civilian/undercover assignment.
- `undercover-client/Shared/Models/Games/WordPairModels.cs` — runtime DTOs used by the UI (`WordPair`, `WordPairCategory`). Note: `WordPairCategory` now has `Language`.
- `undercover-client/wwwroot/WordPairs.yaml` — the canonical set of categories and pairs. Add `language: "EN"` or `language: "NL"` per category.
- `undercover-client/wwwroot/css/app.custom.css` — styling and component classes.

Developer workflows (commands)
- Build (PowerShell):
  ```powershell
  cd c:\git\undercover-game\undercover-client
  dotnet build
  ```
- Run locally (PowerShell):
  ```powershell
  cd c:\git\undercover-game\undercover-client
  dotnet run --urls "https://localhost:5001;http://localhost:5000"
  ```
  The app serves static assets from `bin/Debug/net8.0/wwwroot` during a normal build/run; open the `http://localhost:5000` or `https://localhost:5001` URL in a browser.

Word pairs and YAML details
- File: `undercover-client/wwwroot/WordPairs.yaml` — top-level `categories:` list. Each category should include:
  - `name:` display name
  - `description:` (optional)
  - `language:` short code (currently `EN` or `NL`)
  - `pairs:` list of objects with `wordA:` / `wordB:` values

- Models:
  - YAML binding types: `YamlWordPairData`, `YamlWordPairCategory` (includes `Language`), `YamlWordPair` (`WordA`/`WordB`).
  - Runtime DTOs: `WordPairCategory` (Name, Description, Language, Pairs) and `WordPair` (Civilian, Undercover).

- Important behavior: the YAML uses neutral `wordA`/`wordB`. On load the service randomly assigns which side becomes the Civilian vs Undercover word at runtime. This is intentional so the same pair can be used multiple times with varied assignments.

Common runtime issues and troubleshooting
- Error: "Failed to load YAML: Property 'civilian' not found on type 'RoamingRoutes.Client.Services.YamlWordPair'."
  - Cause: something in the app attempted to deserialize YAML directly into a runtime `WordPair`-shaped type (which has `Civilian`/`Undercover`) instead of the neutral `YamlWordPair` (`wordA`/`wordB`). That happens when stale/old code is present (or a cached/old build is used).
  - Fixes to try (in order):
    1. Ensure there are no duplicate/conflicting `LocalGame.razor` files in the repo root (there was a stray `LocalGame.razor` outside `undercover-client/` previously). Remove any old root file.
    2. Perform a clean rebuild to clear old artifacts:
       ```powershell
       cd c:\git\undercover-game\undercover-client
       Remove-Item -Recurse -Force bin, obj
       dotnet build
       ```
    3. Hard-refresh the browser (or open an incognito window) so the old WASM/js assets are not cached.
    4. If you run a separate YAML test tool (like `tools/YamlReader`), update its local Yaml model types to the same shapes (it used an older model if it failed with `language` missing).

Note: Incognito / hard-refresh recommended
- Blazor WebAssembly assets (blazor.boot.json, .wasm and JS) are aggressively cached. If you see a deserialization error but your server logs show the YAML parsed correctly, open the app in an Incognito/private window or hard-refresh (Ctrl+Shift+R) to force fresh client assets. This resolved the issue when port/profile switching exposed cached WASM that expected an older YAML shape.

- Error: 404 for `undercover-client.styles.css` or other missing static files
  - Cause: a reference to a stylesheet path that doesn't exist in `wwwroot` (or a build output mismatch).
  - Fix: check `wwwroot/index.html` and `Pages/_Host` (if any) to ensure CSS files referenced exist under `wwwroot/css/`. The project ships `wwwroot/css/app.custom.css` and standard filenames under `wwwroot/css/`.

Tips for editing `WordPairs.yaml`
- When adding a category, add `language: "EN"` or `language: "NL"` on the category node.
- Keep pair entries neutral: `wordA` / `wordB`. The UI will randomize which becomes the civilian word at runtime.

Next steps and helpful checks
- If you still see the "Property 'civilian' not found" error after a clean build and cache clear, run a grep for `YamlWordPair` and `wordA`/`wordB` to ensure the code and models are consistent.
- If categories look missing in the UI, open the browser console and check for the YAML load logs (the service logs the HTTP path tried and the loaded content length). If the file loads but deserialization fails, the stack will point to which type failed to bind.

Files to reference while coding
- `Pages/LocalGame.razor`, `Services/LocalGameService.cs`, `Services/WordPairClientService.cs`, `Shared/Models/Games/*`, `wwwroot/WordPairs.yaml`, `wwwroot/css/app.custom.css`.

If anything in this file is unclear or incomplete, point to the exact file or behavior and I'll update this doc accordingly.
