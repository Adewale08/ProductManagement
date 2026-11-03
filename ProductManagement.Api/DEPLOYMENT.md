# Deploy StockPilot API to MonsterASP.NET

The API targets .NET 10 and uses EF Core with SQL Server. Keep database credentials in the hosting control panel; do not commit them to this repository.

## 1. Create the website and database

Create a .NET 10 website and an MSSQL database in MonsterASP.NET. The provider documents that the website and database need to be on the same plan to use the local database connection string. Copy the database's **Local access** connection string for the hosted API.

The local SQL Express connection string is now in `appsettings.Development.json` only. Production must receive its own connection string through the hosting environment.

## 2. Set production environment variables

In the website's Environment variables settings, set:

| Name | Value |
| --- | --- |
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `ConnectionStrings__DefaultConnection` | The hosted database's Local access connection string |
| `FrontendOrigins` | The exact HTTPS origin of the StockPilot frontend, without a trailing slash |

For example, `FrontendOrigins` should be `https://app.example.com`, not a wildcard and not a URL with a path. Restart the website after changing environment variables.

## 3. Apply database migrations

For a new, empty hosted database, generate the SQL migration script from the repository root:

```powershell
dotnet tool install --global dotnet-ef --version 10.0.13
dotnet ef migrations script --idempotent --project .\ProductManagement.Api --startup-project .\ProductManagement.Api --output .\stockpilot-migrations.sql
```

Run `stockpilot-migrations.sql` against the new hosted database using SQL Server Management Studio. If the database already contains data, back it up and inspect its EF migration history and legacy records before running the script. Do not use this new-database procedure to overwrite or clean up existing records.

## 4. Publish the API

From the repository root, publish the API:

```powershell
dotnet publish .\ProductManagement.Api\ProductManagement.Api.csproj --configuration Release --output .\api-publish
Compress-Archive -Path .\api-publish\* -DestinationPath .\stockpilot-api.zip -Force
```

Upload `stockpilot-api.zip` using the provider's ZIP deployment flow and extract the contents into the website's application root (`wwwroot`). Upload the contents of the publish output, not a parent folder containing the output. The publish output contains the IIS `web.config` generated for ASP.NET Core hosting.

## 5. Connect the frontend

Copy the deployed API's HTTPS origin. In Vercel, set `VITE_API_BASE_URL` to that origin only, with no trailing slash or `/api` suffix, then redeploy the frontend.

Identity uses cookies. For dependable browser login, put the frontend and API on custom subdomains of the same domain, such as `app.example.com` and `api.example.com`. Set `FrontendOrigins` to the frontend's exact origin and `VITE_API_BASE_URL` to the API's exact origin.

The API only enables Swagger in Development, so a production `/swagger` 404 is expected. Test an API route such as `/api/Auth/me`; a `401` while signed out confirms the API route is responding and enforcing authentication.
