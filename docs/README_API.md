# Backlogr.Api

ASP.NET Core Web API backend for **Backlogr**, a social video game tracking app inspired by Letterboxd.

> **Document location:** this file lives in the repo root `docs/` folder.

## Current Status

The backend is **deployed and working in Azure** for the current MVP. The latest local codebase now also includes the real AI/vector-search pass: OpenAI-backed review assistance, Azure AI Search vector indexing, semantic search, and recommendation generation built from the user's own logs, ratings, and review text.

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
  - `POST /api/auth/delete-account`
- Member profile endpoints:
  - `GET /api/profiles/{userName}` *(authenticated)*
  - `GET /api/profiles/{userName}/library` *(authenticated)*
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
  - `GET /api/reviews/{reviewId}/comments`
  - `POST /api/reviews/{reviewId}/comments`
  - `DELETE /api/comments/{reviewCommentId}`
- Follow/feed endpoints:
  - `POST /api/follows/{userId}`
  - `DELETE /api/follows/{userId}`
  - `GET /api/feed`
- Admin endpoints:
  - `GET /api/admin/users`
  - `POST /api/admin/users`
  - `PUT /api/admin/users/{userId}/role`
  - `DELETE /api/admin/users/{userId}`
- Temporary bootstrap endpoint for one-time `SuperAdmin` elevation when explicitly enabled:
  - `POST /api/bootstrap/superadmin`
- Real AI/vector-search endpoints:
  - `POST /api/ai/recommendations`
  - `POST /api/ai/review-assistant`
  - `GET /api/ai/semantic-search`
- OpenAI-backed review assistant + embedding generation
- Azure AI Search `games` index creation/backfill on startup for the local catalog
- Semantic search over indexed game metadata
- Recommendation generation based on the user's own logs, ratings, and review themes
- Automated tests covering services, protected routes, and authenticated endpoint flows across implemented slices

### Confirmed deployed behavior
- Swagger is available for the deployed API.
- Register, login, library, and feed flows are working in the deployed environment at the same level they were working in development.
- `GameLog.Rating` remains the source of truth for ratings.
- IGDB search and import are working against the real IGDB API in both local development and production.
- Browse/search can use the local catalog first and pull in IGDB-backed results through the backend search flow.
- The currently deployed API is still tracked conservatively as the pre-AI baseline until the latest AI/vector-search pass is redeployed and smoke tested.
- The latest local codebase successfully builds/runs with real OpenAI + Azure AI Search integration.

### Latest codebase additions ready for next deploy / smoke test
- OpenAI configuration/options added for chat + embeddings
- Azure AI Search configuration/options added for vector search
- `IEmbeddingService` + `OpenAiEmbeddingService` added
- `IAiSearchIndexService` + `AzureAiSearchIndexService` added
- `IAiSearchSyncService` + startup backfill added
- `ISemanticSearchService` now uses real Azure AI Search hybrid/vector queries
- `IRecommendationService` now builds recommendations from the user's taste profile
- `IReviewAssistantService` now calls the live OpenAI Responses API
- Feed scope support for `scope=for-you` and `scope=following` remains in place

### Not implemented yet
- Production-grade global exception handling middleware
- Structured logging / production diagnostics hardening
- Review moderation/admin content-management endpoints
- Public/admin audit trail tooling
- Production smoke testing of the latest AI/vector-search pass after deploy
- Incremental catalog re-indexing tied directly to import/update flows instead of full startup backfill

---

## Tech Stack

- **.NET 10**
- **ASP.NET Core Web API**
- **Entity Framework Core 10**
- **SQL Server / LocalDB**
- **ASP.NET Core Identity**
- **JWT Bearer auth**
- **Swashbuckle / Swagger**
- **OpenAI API** for chat + embeddings
- **Azure AI Search** for vector/hybrid search
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
│   ├── BootstrapController.cs
│   ├── CommentsController.cs
│   ├── FeedController.cs
│   ├── FollowsController.cs
│   ├── GamesController.cs
│   ├── IgdbController.cs
│   ├── LibraryController.cs
│   ├── ProfilesController.cs
│   └── ReviewsController.cs
├── Data/
│   ├── ApplicationDbContext.cs
│   ├── DevelopmentAdminSeeder.cs
│   ├── DevelopmentDataSeeder.cs
│   └── IdentityDataSeeder.cs
├── DTOs/
│   ├── AI/
│   ├── Admin/
│   ├── Auth/
│   ├── Feed/
│   ├── Games/
│   ├── Igdb/
│   ├── Library/
│   ├── Profiles/
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

## Library / Review / Social Notes

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

### Profile / feed behavior
- user names are unique through Identity normalization and are used as the member-profile route key
- member profile endpoints are authenticated in the current MVP
- public-profile library responses intentionally exclude private `GameLog.Notes`
- feed responses include social metadata needed by the frontend without a second round-trip for summary state
- feed supports two scopes:
  - `for-you` for broader recent activity
  - `following` for followed users + current user

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
  "OpenAI": {
    "ApiKey": "your-openai-api-key",
    "ChatModel": "gpt-5.4-mini",
    "EmbeddingModel": "text-embedding-3-small"
  },
  "AzureAiSearch": {
    "Endpoint": "https://your-service.search.windows.net",
    "ApiKey": "your-search-admin-key",
    "GamesIndexName": "games"
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
dotnet user-secrets set "OpenAI:ApiKey" "replace-with-your-openai-api-key"
dotnet user-secrets set "OpenAI:ChatModel" "gpt-5.4-mini"
dotnet user-secrets set "OpenAI:EmbeddingModel" "text-embedding-3-small"
dotnet user-secrets set "AzureAiSearch:Endpoint" "https://your-service.search.windows.net"
dotnet user-secrets set "AzureAiSearch:ApiKey" "replace-with-your-search-admin-key"
dotnet user-secrets set "AzureAiSearch:GamesIndexName" "games"
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
- the development admin account is elevated to `SuperAdmin` for local admin testing
- the Azure AI Search `games` index is ensured and the current local catalog is backfilled into search on startup

Current seeded test game id:

```text
11111111-1111-1111-1111-111111111111
```

This is only intended for local development/testing.

---

## Implemented Endpoints

### Auth
- `POST /api/auth/register`
- `POST /api/auth/login`
- `GET /api/auth/me`
- `POST /api/auth/delete-account`

### Admin
- `GET /api/admin/users`
- `POST /api/admin/users`
- `PUT /api/admin/users/{userId}/role`
- `DELETE /api/admin/users/{userId}`

### Bootstrap
- `POST /api/bootstrap/superadmin`

### Games
- `GET /api/games`
- `GET /api/games/search`
- `GET /api/games/{gameId}`

### IGDB
- `GET /api/igdb/search`
- `POST /api/igdb/import/{igdbId}`

### Profiles
- `GET /api/profiles/{userName}`
- `GET /api/profiles/{userName}/library`

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
- `GET /api/reviews/{reviewId}/comments`
- `POST /api/reviews/{reviewId}/comments`

### Comments
- `DELETE /api/comments/{reviewCommentId}`

### Follows
- `POST /api/follows/{userId}`
- `DELETE /api/follows/{userId}`

### Feed
- `GET /api/feed`
- optional query params:
  - `scope=for-you`
  - `scope=following`
  - `take=<n>`

### AI / vector search
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
- feed scope coverage for **For You** and **Following**
- profile service tests
- authenticated profile flow tests
- game service tests
- games controller tests
- IGDB authenticated search/import flow tests
- AI unauthorized route tests
- authenticated AI flow tests
- existing AI coverage should be expanded/updated for the new real OpenAI + Azure AI Search path
- admin integration tests for user list/create/role-edit permissions

Recommended next test additions:
- self-delete account flow tests
- admin delete-user flow tests
- `SuperAdmin` grant/delete guardrail tests for the latest account-management additions
- semantic-search service/controller tests for real vector-backed results
- recommendation service tests for exclusion of already-logged games
- review-assistant service tests for OpenAI response parsing

Run tests from `Backlogr.Api` or repo root:

```powershell
dotnet test
```

---

## Security / Config Notes

- Keep JWT keys, IGDB secrets, bootstrap secrets, and connection strings out of tracked config files.
- Keep the temporary bootstrap endpoint disabled unless you are intentionally doing a one-time controlled elevation.
- If you use the bootstrap endpoint in production, disable it again immediately after success and rotate/remove the secret.
