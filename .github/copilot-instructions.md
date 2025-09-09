### Quick orientation — Undercover (Blazor WASM) repo (updated)

Goal
- Single-page Blazor WebAssembly client for the "Undercover" rounds game. Runtime is browser-side (local pass-and-play). Intended for static deployment (GitHub Pages) but also runnable locally with `dotnet run` during development.

Snapshot (what changed)
- The UI was redesigned into a single landing page with an improved dark theme and modern CSS system.
- `Pages/LocalGame.razor` is the only route (`@page "/"`) and contains the full single-page UI + page-local logic.
- Navigation/menu components were removed (no site menu). `MainLayout.razor` was simplified to a single container wrapper for the app body.
- Styling: `wwwroot/css/app.custom.css` was overhauled (CSS variables, component classes, responsive utilities).
- Several compile-time model mismatches were fixed in the page code so the project builds and runs locally (see "Model name notes" below).

Big picture
- Project root: `undercover-client/` (net8.0 Blazor WASM).
- UI entry: `Pages/LocalGame.razor` — single landing page and main source of UX logic.
- Local runtime service: `Services/LocalGameService.cs` + `Services/ILocalGameService.cs` — in-memory game state, timers, and localStorage persistence via `IJSRuntime`.
- Word data: `Services/WordPairClientService.cs` (HTTP attempt + local fallback list). Shared DTOs and models live in `Shared/Models/Games/`.

Essential files to inspect first
- `Pages/LocalGame.razor` — new UI flow, role assignment, rounds, voting. Most UI decisions live here.
- `Services/LocalGameService.cs` — runtime state and transitions (StartGameAsync/AddPlayerAsync/AdvanceRoundAsync/Save/Load).
- `Services/WordPairClientService.cs` — category + pair fetching; local fallback pairs are embedded here.
- `Shared/Models/Games/*` — canonical DTOs (GameStateDTO, PlayerDTO, WordPairModels, GameConstants). Keep these stable.
- `wwwroot/css/app.custom.css` — design system and utility classes used across the page.
- `Program.cs` — DI registrations: `IWordPairClientService` and `ILocalGameService` are wired here.

Developer workflows (commands)
- Build: run from `undercover-client/`:
  ```powershell
  cd c:\git\undercover-game\undercover-client
  dotnet build
  ```
- Run the dev host (PowerShell):
  ```powershell
  cd c:\git\undercover-game\undercover-client
  dotnet run --urls "https://localhost:5001;http://localhost:5000"
  ```
  The app will listen on `https://localhost:5001` and `http://localhost:5000` (adjust ports if already in use).
- Preview static output: serve `undercover-client/bin/Debug/net8.0/wwwroot` with any static server for a GitHub Pages-like preview.

Model name notes (important fixes made)
- WordPair model uses properties `Civilian` and `Undercover` (not `CivilianWord`/`UndercoverWord`). `Pages/LocalGame.razor` was updated to match.
- DTO request types are `CreateGameRequestDTO` and `JoinGameRequestDTO` (not `CreateGameDTO`/`JoinGameDTO`). The page code was aligned to use `HostNickname` / `Nickname` as required by these DTOs.
- `GameStatus` enum values are `WaitingForPlayers`, `InProgress`, `Finished` (not `Starting`/`Completed`). UI logic was updated accordingly.

Integration points & important behaviors
- localStorage: `LocalGameService.SaveToLocalAsync()` and `LoadFromLocalAsync()` persist `GameStateDTO` under key `undercover_game_session` using `IJSRuntime`.
- Timer: `LocalGameService` uses `System.Timers.Timer` to drive discussion->voting transitions. `DisposeAsync` stops/disposes timer.
- Word data source: `WordPairClientService` tries HTTP endpoints and falls back to embedded default categories/pairs. To add categories, update `Shared/Models/Games/WordPairModels.cs` and the service fallback.

Current status (as of 2025-09-09)
- Build: succeeded after updating `Pages/LocalGame.razor` to match shared models. (Previously multiple compile errors due to property/DTO name mismatches; those were corrected.)
- Run: the dev host can be started with the `dotnet run` command above; example run used `https://localhost:5001` and `http://localhost:5000`.
-- Warnings: a few C# nullability warnings were present but have been addressed in the latest commit.

Troubleshooting
- Port already in use: if `dotnet run` fails with "address already in use", either stop the process using that port or run with explicit URLs:
  ```powershell
  dotnet run --urls "https://localhost:5001;http://localhost:5000"
  ```
- Browser caching / stale WASM: when iterating on Blazor WASM, browsers may cache old WASM assets. Hard-refresh or clear site data, or open the app in a private/incognito window to force fresh assets.
- HTTPS certificate warnings: self-signed dev certificates may prompt warnings. Trust the dev certificate for local development or use the HTTP URL during quick tests.

Common pitfalls and tips
- Avoid adding another `@page "/"` — only `Pages/LocalGame.razor` should define the root route.
- When changing DTOs used by the page, update both `Shared/Models/Games/` and `Pages/LocalGame.razor` — the Razor page binds directly to those shapes.
- If you change `Program.cs` DI registrations or `ILocalGameService` signatures, update `Pages/LocalGame.razor` accordingly since it calls the service directly.

How to add a new UI feature (recipe)
1. Add or update DTO in `Shared/Models/Games/` if the data shape is shared across pages/services.
2. If the data affects runtime state, add operations to `ILocalGameService` and implement them in `LocalGameService.cs` (persist via SaveToLocalAsync when appropriate).
3. Modify Razor UI in `Pages/LocalGame.razor`. Use classes from `app.custom.css`. Run `dotnet build` and then `dotnet run` to test.

Files to reference while coding
- `Pages/LocalGame.razor`, `Services/LocalGameService.cs`, `Services/ILocalGameService.cs`, `Services/WordPairClientService.cs`, `Shared/Models/Games/*`, `wwwroot/css/app.custom.css`, `Program.cs`.

If anything in this file is unclear or incomplete, mention the exact file or behavior you want expanded and I'll iterate.
