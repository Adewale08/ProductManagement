# StockPilot frontend

React, TypeScript and Vite client for the existing ASP.NET Core API. The frontend uses the API's Identity cookie and never stores credentials in browser storage.

## Run locally

1. Install Node.js LTS.
2. From this folder, run `npm install`.
3. Start the API with its HTTPS launch profile (`https://localhost:7147`).
4. Run `npm run dev` and open `http://localhost:5173`.
5. Run `npm run build` for the TypeScript check and production build.

The Vite development proxy sends `/api`, `/Products`, `/Orders`, `/Category` and `/Suppliers` to `https://localhost:7147`. It keeps browser calls same-origin during development, so the Identity cookie works without adding permissive CORS rules. `secure: false` is limited to Vite's local proxy connection to the development HTTPS certificate.

For a deployed frontend, set `VITE_API_BASE_URL` in the deployment environment to the API origin (for example, `https://api.example.com`, with no trailing slash). The API must then allow the exact frontend origin with credentials and configure cookie `SameSite`/`Secure` settings for that deployment. Do not use wildcard CORS with cookies. No API secrets belong in this frontend.

## API-backed features

- Login, registration, session check and logout use `/api/Auth/*` and the existing Identity cookie.
- Product list/create/update/delete use `/Products`. Search and page controls operate on the API's returned list; the current API returns at most 100 products and does not apply its `ProductQuery` DTO to this controller.
- Sales submit only `{ "items": [{ "productId": 1, "quantity": 2 }] }` to `/Orders`. Displayed cart amounts are estimates; the API response supplies authoritative order totals and prices.
- Orders and dashboard revenue use `/Orders`, which returns the signed-in business's records.
- Category and supplier pages explain why they are unavailable. Their current controllers authorize any signed-in user but do not scope records to a business, so exposing their data would cross business boundaries.

## Manual verification

Create two accounts with distinct business names and emails. Sign into account A, add a product, and record a sale. Sign out, sign into account B in a separate browser profile, and confirm A's product and order are absent. Repeat with B's records, then attempt to open an A order URL while signed into B; it should return not found. The backend ownership checks remain the security boundary. Registration currently accepts `fullName`, but the existing backend does not persist it.
