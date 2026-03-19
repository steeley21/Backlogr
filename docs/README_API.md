# Backlogr.Api

ASP.NET Core Web API backend for **Backlogr**, a social video game tracking app inspired by Letterboxd.

> **Document location:** this file lives in the repo root `docs/` folder.

## Current Status

The API is **deployed and working in Azure** for the current MVP, and the current deployed stack now includes the live AI/vector-search path as part of the app experience.

**Production API URL:** `https://backlograpi.azurewebsites.net`

### Implemented now
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
  - `GET /api/games/{gameId}/me`
  - `GET /api/games/{gameId}/reviews`
  - `GET /api/games/{gameId}/activity`
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
  - `GET /api/feed?scope=for-you`
  - `GET /api/feed?scope=following`
- Admin endpoints currently exposed:
  - `GET /api/admin/users`
  - `POST /api/admin/users`
  - `PUT /api/admin/users/{userId}/role`
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
- Added dedicated game-detail read surface to support:
  - current viewer state for a game
  - game-scoped reviews
  - game-scoped activity

### Confirmed deployed behavior
- Swagger is available for the deployed API.
- Register, login, library, and feed flows are working in the deployed environment at the same level they were working in development.
- IGDB search and import are working against the real IGDB API in both local development and production.
- Browse/search can use the local catalog first and pull in IGDB-backed results through the backend search flow.
- The deployed frontend is successfully calling the deployed API.
- Semantic search is deployed and has been tested through the frontend.
- AI recommendations are deployed and have been tested through the frontend.
- The review assistant is deployed and has been tested through the frontend.
- `GameLog.Rating` remains the source of truth for ratings.

### Current scope notes
- The admin **service layer** contains delete-user logic, but a `DELETE /api/admin/users/{userId}` controller route is **not currently exposed** in this repo snapshot.
- The current AI/vector-search implementation is live and should no longer be treated as “local only” in project docs.
- AI relevance is still MVP-level and is expected to improve as the catalog/search metadata gets richer.

### Still worth improving
- Production-grade global exception handling middleware
- Structured logging / production diagnostics hardening
- Dedicated automated coverage for the **real** OpenAI + Azure AI Search path
- Incremental catalog re-indexing tied directly to import/update flows instead of startup backfill
- Review moderation/admin content-management endpoints if the final scope still needs them
- An admin delete-user controller route if that flow is still part of the final planned scope

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

### Profile / feed / game-detail behavior
- user names are unique through Identity normalization and are used as the member-profile route key
- member profile endpoints are authenticated in the current MVP
- public-profile library responses intentionally exclude private `GameLog.Notes`
- feed responses include social metadata needed by the frontend without a second round-trip for summary state
- feed supports two scopes:
  - `for-you` for broader recent activity
  - `following` for followed users + current user
- `/api/games/{gameId}/me` returns the current user’s log + review state for that game
- `/api/games/{gameId}/reviews` returns game-scoped community reviews
- `/api/games/{gameId}/activity` returns game-scoped mixed activity
