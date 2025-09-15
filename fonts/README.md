Place the required WOFF2 font files here to self-host fonts for the app.

Recommended files and names (these names are referenced by the CSS in ../css/app.custom.css):

- Inter-Regular.woff2
- Inter-Bold.woff2
- RobotoMono-Regular.woff2
- RobotoMono-Bold.woff2

How to obtain:
- Download WOFF2 files from the font provider (e.g. Google Fonts) or a font-subsetting tool. Ensure you respect font licensing.
- Place the files into this directory and commit.

Notes:
- Using relative paths (../fonts/*.woff2) keeps the assets working under GitHub Pages repository subpaths.
- After adding files, rebuild and redeploy. Hard-refresh the site (Ctrl+Shift+R) to bypass cached assets.
