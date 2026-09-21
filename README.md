# Ahmed El-Raggal Portfolio (GitHub Pages)

Static portfolio site for **A7MEDELRAGGAL.github.io**.

## Deploy on GitHub Pages

- Repository type: static files from repository root (`index.html` at root).
- No build step is required.
- In GitHub: **Settings → Pages** and publish from the repository root on `main`.

## Local CMS Admin (client-side only)

- Open `/admin.html` in the browser.
- Default local password: `admin123`.
- The admin panel is **convenience-only** and uses `localStorage` (`site_cms_v1`) in the current browser.
- It is **not secure** and does not provide server-side protection.

## Content workflow

- `index.html` renders the static portfolio and can add CMS entries from:
  1. `localStorage` key `site_cms_v1` (highest priority), or
  2. `content.js` (`window.SITE_CONTENT`) fallback data.
- From `admin.html`, export either:
  - JSON backup (`site_cms_v1.json`), or
  - `content.js` (directly usable by `index.html`).

## Custom subdomain later

When your final subdomain is ready:

1. In **Settings → Pages → Custom domain**, enter the exact subdomain (for example: `portfolio.example.com`).
2. Create a `CNAME` file in repository root containing exactly that subdomain.
3. Add a DNS `CNAME` record for the same subdomain pointing to:
   - `A7MEDELRAGGAL.github.io`
4. After DNS propagation, enable **Enforce HTTPS** in Pages settings.

> This repo intentionally does **not** include a guessed `CNAME` value until the final subdomain is confirmed.
