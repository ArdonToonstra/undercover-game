# undercover-game
Recreation of the Undercover rounds game — single-page Blazor WebAssembly client.

Quick start (PowerShell):

```powershell
cd c:\git\undercover-game\undercover-client
dotnet build
dotnet run --urls "https://localhost:5001;http://localhost:5000"
```

Open `https://localhost:5001` or `http://localhost:5000` in your browser. For static previews, serve `undercover-client/bin/Debug/net8.0/wwwroot` with any static file server.

See `.github/copilot-instructions.md` for a longer developer orientation and troubleshooting.

Deployment
- This repository includes a GitHub Actions workflow at `.github/workflows/deploy-gh-pages.yml` which builds the Blazor WebAssembly app and publishes the `wwwroot` output to the `gh-pages` branch on every push to `main`.
	- The site will be served by GitHub Pages from the `gh-pages` branch (configure repository Pages settings if needed).
