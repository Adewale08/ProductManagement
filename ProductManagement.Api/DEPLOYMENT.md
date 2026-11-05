# Deploy StockPilot API with Supabase PostgreSQL and Render

The API uses EF Core with PostgreSQL and keeps ASP.NET Core Identity for login. Supabase Auth is not used. Keep database credentials in environment variables; never commit them.

## 1. Create the Supabase database

Create a Supabase project and copy its PostgreSQL **Session pooler** connection string from the dashboard. Use the ADO.NET/Npgsql format and port `5432`; do not use the transaction pooler for EF migrations. Keep the connection string private.

Do not commit the connection string or share it.

## 2. Create the PostgreSQL schema

In PowerShell at the repository root, set the connection string for this terminal session and apply the EF migration:

```powershell
$env:ConnectionStrings__DefaultConnection = Read-Host 'Paste the Supabase session-pooler connection string'
dotnet ef database update --project .\ProductManagement.Api --startup-project .\ProductManagement.Api --configuration Release
```

The migration creates the schema in Supabase. Run this against a new/empty Supabase project. The SQL Server database is not changed.

## 3. Copy existing SQL Server data (if needed)

Back up the SQL Server database and stop writes to it before the final transfer. The transfer utility reads the SQL Server tables and inserts their rows into the migrated Supabase tables; it preserves IDs and ASP.NET Identity records, including password hashes. It never writes to the source database.

Set both connection strings locally in PowerShell. The SQL Server source string can use Windows Integrated Authentication; the Supabase target string must be the same session-pooler connection string used above.

```powershell
$env:SQLSERVER_SOURCE_CONNECTION_STRING = 'Server=YOUR_SQL_SERVER;Database=ProductManagementDb;Integrated Security=True;TrustServerCertificate=True;'
$env:SUPABASE_CONNECTION_STRING = Read-Host 'Paste the Supabase session-pooler connection string'
dotnet run --project .\tools\SqlServerToSupabase\SqlServerToSupabase.csproj
```

The utility aborts without committing if a target table already has rows or if source and target table columns do not match. If it aborts, keep the backup and resolve the reported schema/data issue before retrying against a fresh target. Verify the copied table counts and test login before switching the frontend to the new API.

## 4. Configure Render

Set these variables in the Render API service's **Environment** settings:

| Name | Value |
| --- | --- |
| `ConnectionStrings__DefaultConnection` | Supabase session-pooler connection string |
| `FrontendOrigins` | Exact HTTPS origin of the frontend, e.g. `https://your-app.vercel.app` |

Render supplies `PORT`; the root `Dockerfile` binds the API to that port. Commit and push the code, then deploy the updated service.

## 5. Configure the frontend

Set `VITE_API_BASE_URL` in the Vercel frontend project's Environment Variables to the deployed API's HTTPS origin only (no `/swagger`, `/api`, or trailing slash), then redeploy the frontend. The frontend and API need to use HTTPS for cookie-based login.

Swagger is enabled only in Development, so a production `/swagger` 404 is expected.
