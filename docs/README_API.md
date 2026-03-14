# Backlogr.Api

ASP.NET Core Web API backend for **Backlogr**, a social video game tracking app inspired by Letterboxd.

> **Document location:** this file lives in the repo root `docs/` folder.

## Current Status

The backend is now **deployed and working in Azure** and is serving the current `Backlogr.Web` frontend.

**Production API URL:** `https://backlograpi.azurewebsites.net`

Implemented so far:
- ASP.NET Core Web API on **.NET 10**
- Entity Framework Core + SQL Server for local and deployed environments
- ASP.NET Core Identity with **Guid** keys
- JWT authentication
- Role seeding for `User` and `Admin`
- Swagger/OpenAPI with bearer auth support
- CORS configured for local frontend development and deployed frontend access
- GitHub Actions CI/CD for API deployment
- Game catalog endpoints:
  - `GET /api/games`
  - `GET /api/games/{gameId}`
- IGDB stub endpoints:
  - `GET /api/igdb/search`
  - `POST /api/igdb/import/{igdbId}` *(admin only)*
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
- AI stub endpoints:
  - `POST /api/ai/recommendations`
  - `POST /api/ai/review-assistant`
  - `GET /api/ai/semantic-search`
- Automated tests covering services, protected routes, and authenticated endpoint flows across implemented slices

### Current deployed behavior
- Swagger is available for the deployed API.
- Register, login, library, and feed flows are working in the deployed environment at the same level they were working in development.
- Feed includes the **current user’s own activity** in addition to activity from followed users.
- `GameLog.Rating` remains the source of truth for ratings.
- IGDB and AI surfaces are still stub-backed for now.

Not implemented yet:
- Real IGDB API integration
- Real Azure AI / Azure AI Search integration
- Admin bootstrap flow
- Global exception handling middleware
- Structured logging / production diagnostics hardening

---

## Tech Stack

- **.NET 10**
- **ASP.NET Core Web API**
- **Entity Framework Core 10**
- **SQL Server**
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
│   ├── DevelopmentDataSeeder.cs
│   └── IdentityDataSeeder.cs
├── DTOs/
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

### User secrets
Initialize user secrets on the API project and keep secrets out of source control.

Required local secrets:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=BacklogrDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  },
  "Jwt": {
    "Key": "your-long-random-dev-secret",
    "Issuer": "Backlogr.Api",
    "Audience": "Backlogr.Web"
  },
  "Igdb": {
    "ClientId": "",
    "ClientSecret": ""
  }
}
```

Recommended commands from `Backlogr.Api`:

```powershell
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=(localdb)\MSSQLLocalDB;Database=BacklogrDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
dotnet user-secrets set "Jwt:Issuer" "Backlogr.Api"
dotnet user-secrets set "Jwt:Audience" "Backlogr.Web"
dotnet user-secrets set "Jwt:Key" "replace-with-a-long-random-secret"
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
- roles: `User`, `Admin`
- one temporary test game for local development/testing

Current seeded test game id:

```text
11111111-1111-1111-1111-111111111111
```

This is only intended for local development/testing until real game catalog + IGDB integration is in place.

---

## Implemented Endpoints

### Auth
- `POST /api/auth/register`
- `POST /api/auth/login`
- `GET /api/auth/me`

### Games
- `GET /api/games`
- `GET /api/games/{gameId}`

### IGDB (stub)
- `GET /api/igdb/search`
- `POST /api/igdb/import/{igdbId}` *(admin only)*

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
- IGDB auth/role/flow tests
- AI stub service tests
- AI unauthorized route tests
- authenticated AI flow tests

Run tests from `Backlogr.Api` or repo root:

```powershell
dotnet test
```

---

## Security / Config Notes

- Keep JWT keys, IGDB secrets, and connection strings out of tracked config files.
- Use **User Secrets** for local development.
- Production secrets belong in Azure configuration.
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

Deployment notes:
- Keep production connection strings and JWT settings in Azure configuration.
- Keep CORS aligned with both localhost and the deployed frontend origin.
- Point the frontend `NUXT_PUBLIC_API_BASE` to the deployed API URL.
- Re-run smoke tests after any deployment/config changes.

---

## Current Gaps / Next Steps

Recommended next backend work:
1. Extract token generation into an auth/token service.
2. Add an admin bootstrap strategy.
3. Add global exception handling middleware.
4. Add structured logging/diagnostics hardening.
5. Replace IGDB stubs with real IGDB integration.
6. Replace AI stubs with Azure AI / Azure AI Search implementations.
