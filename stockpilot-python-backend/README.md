# StockPilot Python API

FastAPI implementation of StockPilot, using SQLAlchemy 2.0 and PostgreSQL on Supabase. The service is designed to use Supabase's IPv4-compatible **Session Pooler** from Render. Use the PostgreSQL connection URL supplied by Supabase through `DATABASE_URL`; SQLAlchemy uses psycopg 3 and TLS (`sslmode=require` in the example).

## Local setup

1. Create a Supabase project and copy the Session Pooler connection string from **Connect**. Use the Session Pooler host, port `5432`, and username supplied by Supabase. URL-encode special characters in the database password.
2. Copy `.env.example` to `.env`, replace every placeholder, and set `FRONTEND_ORIGINS` to the exact deployed Vercel origin and local development origin. `.env` is ignored by Git.
3. Create a Python 3.12+ virtual environment, install `requirements.txt`, then run `uvicorn app.main:app --reload` from this directory.
4. `GET /health` is the Render health check. OpenAPI docs are at `/docs`.

## API routes

| Area | Routes |
|---|---|
| Auth | `POST /api/Auth/register`, `POST /api/Auth/login`, `GET /api/Auth/me`, `POST /api/Auth/logout` |
| Products | `GET /Products`, `GET /Products/{id}`, `POST /Products`, `PUT /Products/{id}`, `DELETE /Products/{id}`, `PATCH /Products/{id}/stock` |
| Categories | `GET /Category`, `GET /Category/{id}`, `POST /Category`, `PUT /Category/{id}`, `DELETE /Category/{id}` |
| Suppliers | `GET /api/Suppliers`, `GET /api/Suppliers/{id}`, `POST /api/Suppliers`, `PUT /api/Suppliers/{id}`, `DELETE /api/Suppliers/{id}` |
| Orders | `POST /Orders`, `GET /Orders`, `GET /Orders/{id}`, `PATCH /Orders/{id}/status` |

Every catalog and order request resolves its business from the authenticated user. Request `BusinessId` values are ignored. Order creation locks the selected business products and updates stock inside one database transaction. Cancelling a pending order restores its stock.

## Authentication

The frontend sends cookies (`credentials: include`) and expects ASP.NET Identity session behavior. FastAPI uses the same auth paths with an HttpOnly signed session cookie, so the frontend does not need an auth rewrite. Production cookies use `Secure` and `SameSite=None` for separate Render and Vercel origins. A request-origin guard blocks state changes from origins outside the configured allowlist. Set a random `SESSION_SECRET` with at least 32 characters.

Passwords are hashed with Argon2. Supabase is a fresh database, so existing ASP.NET Identity password hashes are not imported or converted. Registration accepts the frontend `fullName` field; as in the C# implementation, the business name is used as the username and `fullName` is not persisted.

## Database and migration safety

`DATABASE_URL` is the only database connection setting. The Python service does not read the C# connection string and never connects to Azure SQL. SQLAlchemy models and the initial Alembic revision map the C# PostgreSQL migration's entities, columns, relationships, indexes, and uniqueness rules. Product, category, and supplier ownership is enforced by API queries and writes; client-provided ownership IDs are not used.

The initial revision is present for review but has not been applied. Do not run `alembic upgrade` until you explicitly approve creating the Supabase schema. The initial revision refuses downgrade rather than dropping tables or data.

The source has one existing schema mismatch: category request DTOs permit descriptions up to 500 characters, while the PostgreSQL column is limited to 250. This implementation follows the actual database column limit.

## Deployment

The root `render.yaml` points Render at this project and configures `/health`. In Render, set `DATABASE_URL` to the Supabase Session Pooler URI, `FRONTEND_ORIGINS` to the exact deployed Vercel origin, and keep the generated `SESSION_SECRET`. In Vercel, set `VITE_API_BASE_URL` to the Render service origin and redeploy the frontend. Render does not run migrations at deploy time.
