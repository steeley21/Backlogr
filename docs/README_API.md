# Backlogr.Api

ASP.NET Core Web API backend for **Backlogr**, a social video game tracking app inspired by Letterboxd.

> **Document location:** this file lives in the repo root `docs/` folder.

## Current Status

The backend is deployed and working in Azure for the current MVP feature set, and the current local codebase now also includes admin user-management endpoints plus `SuperAdmin` support that have been verified locally with passing automated tests.

**Production API URL:** `https://backlograpi.azurewebsites.net`

Implemented so far:
- ASP.NET Core Web API on **.NET 10**
- Entity Framework Core + SQL Server for deployed environments and **LocalDB** for local development
- ASP.NET Core Identity with **Guid** keys
- JWT authentication
- Role seeding for `User`, `Admin`, and `SuperAdmin`
- Swagger/OpenAPI with bearer auth support
- CORS configured for local frontend development and deployed frontend access
- GitHub Actions CI/CD for API deployment
- Game catalog endpoints:
  - `GET /api/games`
  - `GET /api/games/search`
  - `GET /api/games/{gameId}`
- Real IGDB-backed catalog endpoints:
  - `GET /api/igdb/search` *(authenticated)*
  - `POST /api/igdb/import/{igdbId}` *(authenticated)*
- Auth endpoints:
  - `POST /api/auth/register`
  - `POST /api/auth/login`
  - `GET /api/auth/me`
- Library/logging endpoints:
  - `GET /api/library/me`
  - `POST /api/library`
  - `DELETE /api/library/{gameId}`
- Review endpoints:
  - `POST /api/reviews`
  - `PUT /api/reviews/{reviewId}`
  - `DELETE /api/reviews/{reviewId}`
  - `POST /api/reviews/{reviewId}/like`
  - `DELETE /api/reviews/{reviewId}/like`
  - `POST /api/reviews/{reviewId}/comments`
  - `DELETE /api/comments/{reviewCommentId}`
- Follow/feed endpoints:
  - `POST /api/follows/{userId}`
  - `DELETE /api/follows/{userId}`
  - `GET /api/feed`
- Admin user-management endpoints:
  - `GET /api/admin/users`
  - `POST /api/admin/users`
  - `PUT /api/admin/users/{userId}/role`
- AI stub endpoints:
  - `POST /api/ai/recommendations`
  - `POST /api/ai/review-assistant`
  - `GET /api/ai/semantic-search`
- Automated tests covering services, protected routes, authenticated endpoint flows, and admin permission rules across implemented slices

### Current deployed behavior
- Swagger is available for the deployed API.
- Register, login, library, and feed flows are working in the deployed environment at the same level they were working in development.
- Feed includes the **current user’s own activity** in addition to activity from followed users.
- `GameLog.Rating` remains the source of truth for ratings.
- IGDB search and import are working against the real IGDB API in both local development and production.
- Browse/search can use the local catalog first and pull in IGDB-backed results through the backend search flow.
- AI surfaces are still stub-backed for now.

### Current locally verified behavior
- `SuperAdmin` role seeding works in development.
- Admin user-management endpoints are working locally.
- `Admin` can create `User` accounts.
- `SuperAdmin` can create `User` and `Admin` accounts.
- `SuperAdmin` can change existing `User` / `Admin` roles.
- Self-role editing is intentionally blocked.
- `SuperAdmin` role editing through the admin endpoint is intentionally blocked.
- Automated tests for the admin endpoints and `SuperAdmin` permission paths are passing locally.

Not implemented yet:
- Real Azure AI / Azure AI Search integration
- Production-grade global exception handling middleware
- Structured logging / production diagnostics hardening
- Production/admin bootstrap strategy beyond current development seeding
- Admin review moderation endpoints/UI

---

## Tech Stack

- **.NET 10**
- **ASP.NET Core Web API**
- **Entity Framework Core 10**
- **SQL Server / LocalDB**
- **ASP.NET Core Identity**
- **JWT Bearer auth**
- **Swashbuckle / Swagger**
- **xUnit** for tests

---

## Project Structure

```text
Backlogr.Api/
├── Common/
├── Controllers/
│   ├── AdminController.cs
│   ├── AiController.cs
│   ├── AuthController.cs
│   ├── CommentsController.cs
│   ├── FeedController.cs
│   ├── FollowsController.cs
│   ├── GamesController.cs
│   ├── IgdbController.cs
│   ├── LibraryController.cs
│   └── ReviewsController.cs
├── Data/
│   ├── ApplicationDbContext.cs
│   ├── DevelopmentAdminSeeder.cs
│   ├── DevelopmentDataSeeder.cs
│   └── IdentityDataSeeder.cs
├── DTOs/
│   ├── Admin/
│   ├── AI/
│   ├── Auth/
│   ├── Feed/
│   ├── Games/
│   ├── Igdb/
│   ├── Library/
│   └── Reviews/
├── Extensions/
├── Migrations/
├── Models/
│   ├── Entities/
│   └── Enums/
├── Options/
├── Services/
│   ├── Implementations/
│   └── Interfaces/
├── Properties/
├── appsettings.json
├── appsettings.Development.json
└── Program.cs
```

---

## Current Domain Model

### Identity
- `ApplicationUser`
- `ApplicationRole`
- role names currently include `User`, `Admin`, and `SuperAdmin`

### Catalog
- `Game`

### Library
- `GameLog`
- `LibraryStatus`

### Reviews / interactions
- `Review`
- `ReviewLike`
- `ReviewComment`

### Social
- `Follow`

### Implemented constraints
- `GameLog` unique index on `(UserId, GameId)`
- `Review` unique index on `(UserId, GameId)`
- `Follow` unique index on `(FollowerId, FollowingId)`
- `ReviewLike` unique index on `(UserId, ReviewId)`
- `Game.IgdbId` unique filtered index when present

---

## Auth Model

`ApplicationUser` extends `IdentityUser<Guid>` and currently includes:
- `DisplayName`
- `FirstName`
- `LastName`
- `AvatarUrl`
- `Bio`
- `CreatedAt`

Identity fields such as `UserName` and `Email` come from `IdentityUser<Guid>`.

---

## Library / Review Model Notes

`GameLog` currently supports:
- `Status`
- `Rating`
- `Platform`
- `Hours`
- `StartedAt`
- `FinishedAt`
- `Notes`

### Supported library statuses
- `Backlog`
- `Playing`
- `Played`
- `Wishlist`
- `Dropped`

### Current validation rules
- one log per `(UserId, GameId)`
- `Rating` must be between `0.0` and `5.0`
- `Rating` must be in `0.5` increments
- `Hours` cannot be negative
- `FinishedAt` cannot be earlier than `StartedAt`
- `FinishedAt` is required when status is `Played`

### Review rules
- one review per `(UserId, GameId)`
- `Review` stores text + spoiler flag only
- `GameLog.Rating` is the source of truth for rating
- review text is required
- review text max length is `4000`

### Review interaction rules
- likes are idempotent
- one like per `(UserId, ReviewId)`
- comment text is required
- comment text max length is `2000`
- comment delete is owner-or-admin
- review delete cascades to likes/comments

### Follow rules
- no self-follow
- duplicate follow is treated as a no-op
- unfollow missing relationship is treated as a no-op

### Admin user-management rules
- `GET /api/admin/users` requires `Admin` or `SuperAdmin`
- `POST /api/admin/users` requires `Admin` or `SuperAdmin`
- `PUT /api/admin/users/{userId}/role` requires `SuperAdmin`
- `Admin` can create `User`
- `SuperAdmin` can create `User` and `Admin`
- the role-edit endpoint only supports changing between `User` and `Admin`
- self-role editing is blocked
- editing a `SuperAdmin` account through the admin endpoint is blocked

---

## Local Setup

### Prerequisites
- .NET 10 SDK
- Visual Studio 2022 or later
- SQL Server LocalDB

### Restore / build
From the repo root or from `Backlogr.Api`:

```powershell
dotnet restore
dotnet build
```

### Local configuration model
Use **User Secrets** for local development only.
Use **Azure App Service configuration** for production only.

Do **not** point local development at the deployed Azure SQL database.

### User secrets
Initialize user secrets on the API project and keep secrets out of source control.

Recommended local secrets shape:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=BacklogrDev;Trusted_Connection=True;MultipleActiveResultSets=True;TrustServerCertificate=True"
  },
  "Jwt": {
    "Key": "replace-with-a-long-random-dev-secret",
    "Issuer": "Backlogr.Api",
    "Audience": "Backlogr.Web"
  },
  "Igdb": {
    "ClientId": "your-igdb-client-id",
    "ClientSecret": "your-igdb-client-secret"
  },
  "SeedAdmin": {
    "Email": "admin@backlogr.local",
    "UserName": "admin",
    "Password": "AdminPass123",
    "DisplayName": "Backlogr Admin"
  }
}
```

If you prefer CLI commands from `Backlogr.Api`:

```powershell
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=(localdb)\MSSQLLocalDB;Database=BacklogrDev;Trusted_Connection=True;MultipleActiveResultSets=True;TrustServerCertificate=True"
dotnet user-secrets set "Jwt:Issuer" "Backlogr.Api"
dotnet user-secrets set "Jwt:Audience" "Backlogr.Web"
dotnet user-secrets set "Jwt:Key" "replace-with-a-long-random-dev-secret"
dotnet user-secrets set "Igdb:ClientId" "replace-with-your-igdb-client-id"
dotnet user-secrets set "Igdb:ClientSecret" "replace-with-your-igdb-client-secret"
```

### Database migrations
From `Backlogr.Api`:

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet ef database update
```

Current migrations:
- `InitialIdentitySetup`
- `AddGamesAndGameLogs`
- `AlignGameLogWithLibraryRequirements`
- `AddReviews`
- `AddReviewLikesAndComments`
- `AddFollows`

---

## Running the API

From `Backlogr.Api`:

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet run
```

Swagger should open locally based on the launch profile.

### Swagger auth
Use the `POST /api/auth/login` endpoint to get a token, then click **Authorize** in Swagger and paste the **raw JWT token only**.

Do **not** prefix it with `Bearer` in the Swagger auth dialog.

---

## Development Seed Behavior

In development, startup currently seeds:
- roles: `User`, `Admin`, `SuperAdmin`
- one temporary test game for local development/testing
- an optional development admin account when `SeedAdmin` values are present in user secrets

Current seeded test game id:

```text
11111111-1111-1111-1111-111111111111
```

Current local development expectation:
- the seeded development admin can be elevated to `SuperAdmin` for local admin testing
- this is intended for development/demo use and is not a production bootstrap strategy

This is only intended for local development/testing.

---

## Implemented Endpoints

### Auth
- `POST /api/auth/register`
- `POST /api/auth/login`
- `GET /api/auth/me`

### Games
- `GET /api/games`
- `GET /api/games/search`
- `GET /api/games/{gameId}`

### IGDB
- `GET /api/igdb/search`
- `POST /api/igdb/import/{igdbId}`

### Library
- `GET /api/library/me`
- `POST /api/library`
- `DELETE /api/library/{gameId}`

### Reviews
- `POST /api/reviews`
- `PUT /api/reviews/{reviewId}`
- `DELETE /api/reviews/{reviewId}`
- `POST /api/reviews/{reviewId}/like`
- `DELETE /api/reviews/{reviewId}/like`
- `POST /api/reviews/{reviewId}/comments`

### Comments
- `DELETE /api/comments/{reviewCommentId}`

### Follows
- `POST /api/follows/{userId}`
- `DELETE /api/follows/{userId}`

### Feed
- `GET /api/feed`

### Admin
- `GET /api/admin/users`
- `POST /api/admin/users`
- `PUT /api/admin/users/{userId}/role`

### AI (stub)
- `POST /api/ai/recommendations`
- `POST /api/ai/review-assistant`
- `GET /api/ai/semantic-search`

---

## Testing

The backend test project is `Backlogr.Api.Tests`.

Current automated coverage includes:
- auth integration tests
- library service tests
- library unauthorized route tests
- authenticated library flow tests
- review service tests
- review unauthorized route tests
- authenticated review flow tests
- review interaction service tests
- review interaction unauthorized route tests
- authenticated review interaction flow tests
- follow service tests
- follow unauthorized route tests
- authenticated follow flow tests
- feed service tests
- feed unauthorized route tests
- authenticated feed flow tests
- game service tests
- games controller tests
- IGDB authenticated search/import flow tests
- AI stub service tests
- AI unauthorized route tests
- authenticated AI flow tests
- admin unauthorized route tests
- admin authenticated flow tests
- `SuperAdmin` permission-path tests for create/edit-role flows

All current backend tests are passing locally.

Run tests from `Backlogr.Api` or repo root:

```powershell
dotnet test
```

---

## Security / Config Notes

- Keep JWT keys, IGDB secrets, and connection strings out of tracked config files.
- Use **User Secrets** for local development only.
- Use **Azure App Service configuration** for production only.
- Keep local and production databases separate.
- Avatar handling is URL-only for now.
- No file upload/storage is implemented yet.
- Test authentication uses a header-driven fake auth handler only in the test host.

---

## Deployment Notes

Current deployment status:
- API is deployed and working in Azure App Service.
- Swagger is working in the deployed environment.
- GitHub Actions CI/CD is configured for the API.
- Frontend-to-API integration is working against the deployed API.
- Production IGDB credentials are configured in Azure and the live API can perform IGDB search/import.

Deployment notes:
- Keep production connection strings, JWT settings, and IGDB settings in Azure configuration.
- Keep CORS aligned with both localhost and the deployed frontend origin.
- Point the frontend `NUXT_PUBLIC_API_BASE` to the deployed API URL.
- Re-run smoke tests after any deployment/config changes.

Local-only features verified after the current deployment pass:
- `SuperAdmin` role support
- admin user-management endpoints
- role-edit restrictions for self/protected accounts
- admin endpoint automated tests

Before or immediately after deploying the newer admin changes, re-run this smoke-test checklist against production:
- `POST /api/auth/login`
- `GET /api/auth/me`
- `GET /api/admin/users` as `Admin` or `SuperAdmin`
- `POST /api/admin/users` as `Admin` creating `User`
- `POST /api/admin/users` as `SuperAdmin` creating `Admin`
- `PUT /api/admin/users/{userId}/role` as `SuperAdmin`
- confirm self-role editing stays blocked
- confirm editing a `SuperAdmin` through the endpoint stays blocked

---

## Current Gaps / Next Steps

Recommended next backend work:
1. Deploy and smoke test the admin user-management endpoints in production.
2. Extract auth token generation into a dedicated service.
3. Add production-friendly admin/bootstrap tooling.
4. Add global exception handling middleware.
5. Add structured logging/diagnostics hardening.
6. Replace AI stubs with Azure AI / Azure AI Search implementations.
7. Add embeddings/vector search wiring.
