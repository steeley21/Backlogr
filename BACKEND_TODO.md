# Backlogr Backend TODO

## Current project state

- `Backlogr.Web` is scaffolded.
- Frontend is **not deployed yet**.
- API project exists as **`Backlogr.Api`**.
- Test project exists as **`Backlogr.Api.Tests`**.
- Backend implementation is **well underway**.
- Core backend slices, test coverage, and stubbed external seams are now working locally.

---

## Locked decisions

### Project structure
- Use **one API project + one test project**.
- Project names:
  - `Backlogr.Api`
  - `Backlogr.Api.Tests`
- Use the **ASP.NET Core Web API** Visual Studio template.
- Keep architecture as a **modular monolith**, not a multi-project clean architecture split.

### Backend stack
- **.NET 10 / ASP.NET Core Web API**
- **Entity Framework Core 10**
- **ASP.NET Core Identity**
- **JWT auth**
- **Role-based authorization**
- **xUnit** for backend tests
- **LocalDB** for local development
- **Azure SQL** later for deployment

### Identity / database decisions
- Use **Guid keys**.
- Follow naming rule: primary keys should be **`TableNameId`**.
- Foreign keys should also use target names like **`UserId`**, **`GameId`**, **`ReviewId`**.
- Store timestamps in **UTC**.

### Domain decisions
- **One review per user per game**.
- **`GameLog.Rating` is the single source of truth** for rating.
- Remove **`Review.Rating`** from the implementation plan.
- Avatar/profile image will be stored as **URL only** for now.

### IGDB decisions
- **Authenticated users can search IGDB**.
- **Admin only can import IGDB games into local cache**.
- Imported games are cached locally and used for app-facing game data.
- Use a **stub IGDB service first**, then replace it later.

### AI decisions
- AI is required by the final project.
- AI endpoints should be **stubbed first**.
- Semantic/vector search and recommendation work come after core app functionality.
- Current AI endpoints are **stub implementations**, not production integrations.

---

## MVP backend scope

### Core auth + profile
- Register
- Login
- Current user (`me`)
- Role support: `User`, `Admin`
- Profile fields:
  - `Username`
  - `DisplayName`
  - `FirstName` *(optional)*
  - `LastName` *(optional)*
  - `AvatarUrl` *(optional)*
  - `Bio` *(optional)*

### Games
- Local cached `Game` records
- Game list/detail endpoints
- IGDB search endpoint
- Admin-only IGDB import endpoint

### Library / logging
- Add or update one `GameLog` per user/game
- Status values:
  - `Playing`
  - `Played`
  - `Backlog`
  - `Wishlist`
  - `Dropped`
- Optional fields:
  - `Rating` (0.5 increments)
  - `Platform`
  - `Hours`
  - `StartedAt`
  - `FinishedAt`
  - `Notes`

### Reviews / social
- Create/edit/delete review
- Spoiler flag support
- Like review
- Comment on review
- Delete own comment or admin delete
- Follow/unfollow users
- Feed of logs + reviews from followed users

### AI
- Recommendations endpoint *(stub currently)*
- Review assistant endpoint *(stub currently)*
- Semantic search endpoint *(stub currently)*

---

## Implemented so far

### Foundation
- [x] API project created
- [x] Test project created
- [x] Root `.gitignore` fixed and moved to repo root
- [x] Core NuGet packages installed
- [x] Solution builds cleanly
- [x] User secrets initialized
- [x] LocalDB connection configured
- [x] Swagger/OpenAPI configured
- [x] Swagger bearer auth support configured
- [x] Swagger enum display improvements configured
- [x] CORS configured for local frontend
- [x] User secrets / JWT key kept out of repo

### Identity / auth
- [x] `ApplicationUser` with Guid key
- [x] `ApplicationRole` with Guid key
- [x] `ApplicationDbContext`
- [x] Identity configured
- [x] JWT auth configured
- [x] Roles seeded: `User`, `Admin`
- [x] `AuthController`
- [x] `POST /api/auth/register`
- [x] `POST /api/auth/login`
- [x] `GET /api/auth/me`

### Core domain model
- [x] `Game`
- [x] `GameLog`
- [x] `LibraryStatus`
- [x] `Review`
- [x] `Follow`
- [x] `ReviewLike`
- [x] `ReviewComment`

### Games / IGDB
- [x] `IGameService`
- [x] `GameService`
- [x] `GamesController`
- [x] `GET /api/games`
- [x] `GET /api/games/{gameId}`
- [x] `IIgdbService`
- [x] `StubIgdbService`
- [x] `IgdbController`
- [x] `GET /api/igdb/search`
- [x] `POST /api/igdb/import/{igdbId}` *(admin only)*

### Library slice
- [x] `ILibraryService`
- [x] `LibraryService`
- [x] `LibraryController`
- [x] `GET /api/library/me`
- [x] `POST /api/library`
- [x] `DELETE /api/library/{gameId}`
- [x] Rating validation
- [x] Ownership enforcement
- [x] Development test game seeding for Swagger/testing

### Review slice
- [x] `IReviewService`
- [x] `ReviewService`
- [x] `ReviewsController`
- [x] `POST /api/reviews`
- [x] `PUT /api/reviews/{reviewId}`
- [x] `DELETE /api/reviews/{reviewId}`

### Review interactions
- [x] `IReviewInteractionService`
- [x] `ReviewInteractionService`
- [x] `CommentsController`
- [x] `POST /api/reviews/{reviewId}/like`
- [x] `DELETE /api/reviews/{reviewId}/like`
- [x] `POST /api/reviews/{reviewId}/comments`
- [x] `DELETE /api/comments/{reviewCommentId}`

### Follow / feed
- [x] `IFollowService`
- [x] `FollowService`
- [x] `FollowsController`
- [x] `IFeedService`
- [x] `FeedService`
- [x] `FeedController`
- [x] `POST /api/follows/{userId}`
- [x] `DELETE /api/follows/{userId}`
- [x] `GET /api/feed`

### AI stubs
- [x] `IRecommendationService`
- [x] `IReviewAssistantService`
- [ ] `IEmbeddingService`
- [x] `ISemanticSearchService`
- [x] `StubRecommendationService`
- [x] `StubReviewAssistantService`
- [x] `StubSemanticSearchService`
- [x] `AiController`
- [x] `POST /api/ai/recommendations`
- [x] `POST /api/ai/review-assistant`
- [x] `GET /api/ai/semantic-search`

### Database
- [x] Initial identity migration created and applied
- [x] Game / GameLog migration created and applied
- [x] GameLog alignment migration created and applied
- [x] Review migration created and applied
- [x] ReviewLike / ReviewComment migration created and applied
- [x] Follow migration created and applied

### Testing
- [x] Auth integration tests
- [x] Library service + route + flow tests
- [x] Review service + route + flow tests
- [x] Review interaction service + route + flow tests
- [x] Follow service + route + flow tests
- [x] Feed service + route + flow tests
- [x] Game service + controller tests
- [x] IGDB auth/role/flow tests
- [x] AI stub service + route + flow tests

---

## Planned first-pass entities

### Identity / profile
- [x] `ApplicationUser`

### Catalog
- [x] `Game`

### Library
- [x] `GameLog`

### Reviews / social
- [x] `Review`
- [x] `Follow`
- [x] `ReviewLike`
- [x] `ReviewComment`

---

## Required constraints

- [x] `GameLog`: unique `(UserId, GameId)`
- [x] `Review`: unique `(UserId, GameId)`
- [x] `Follow`: unique `(FollowerId, FollowingId)`
- [x] `ReviewLike`: unique `(UserId, ReviewId)`
- [x] Prevent self-follow
- [x] Enforce ownership checks on update/delete actions for implemented slices

---

## Suggested solution / folder layout

```text
BACKLOGR/
├── .gitignore
├── BACKEND_TODO.md
├── Backlogr.Web/
├── Backlogr.Api/
├── Backlogr.Api.Tests/
├── docs/
├── README.md
└── requirements_backlogr.md
```

### Inside `Backlogr.Api`

```text
Backlogr.Api/
├── Common/
├── Controllers/
├── Data/
├── DTOs/
├── Extensions/
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

## Setup checklist

### Solution / project setup
- [x] Create `Backlogr.Api`
- [x] Create `Backlogr.Api.Tests`
- [x] Add both projects to the solution
- [x] Add test project reference to API project

### NuGet packages installed
- [x] `Microsoft.EntityFrameworkCore.SqlServer`
- [x] `Microsoft.EntityFrameworkCore.Design`
- [x] `Microsoft.EntityFrameworkCore.Tools`
- [x] `Microsoft.EntityFrameworkCore.InMemory` *(test project)*
- [x] `Microsoft.AspNetCore.Identity.EntityFrameworkCore`
- [x] `Microsoft.AspNetCore.Authentication.JwtBearer`
- [x] `Swashbuckle.AspNetCore`
- [x] `Microsoft.AspNetCore.Mvc.Testing` *(test project)*
- [x] `FluentAssertions` *(test project)*
- [x] `Moq` *(test project)*

### Configuration
- [x] Set up `appsettings.Development.json`
- [x] Add LocalDB connection string via user secrets
- [x] Add JWT settings via user secrets
- [x] Add CORS settings for local frontend
- [x] Initialize user secrets
- [x] Move secrets out of committed config files

### Core backend plumbing
- [x] Create `ApplicationUser` with Guid identity key
- [x] Create `ApplicationDbContext`
- [x] Configure Identity
- [x] Configure JWT auth
- [x] Configure authorization / role support
- [x] Configure Swagger/OpenAPI
- [x] Configure CORS for `Backlogr.Web`
- [ ] Add global exception handling strategy
- [ ] Add basic logging setup

---

## Implementation status by phase

### Phase 1 — foundation
- [x] Confirm solution builds cleanly
- [x] Set up EF Core + Identity + JWT
- [x] Add initial options/config classes
- [x] Add first migration
- [x] Create database locally
- [x] Seed roles: `User`, `Admin`
- [ ] Add a way to seed an initial admin account

### Phase 2 — auth slice
- [x] Build `AuthController`
- [x] Implement:
  - [x] `POST /api/auth/register`
  - [x] `POST /api/auth/login`
  - [x] `GET /api/auth/me`
- [x] Create DTOs for auth flows
- [ ] Add auth service / token generation service
- [x] Test auth endpoints

### Phase 3 — core data model
- [x] Create entities:
  - [x] `Game`
  - [x] `GameLog`
  - [x] `Review`
  - [x] `Follow`
  - [x] `ReviewLike`
  - [x] `ReviewComment`
- [x] Add EF configurations and constraints
- [x] Add migrations for domain tables

### Phase 4 — game/catalog slice
- [x] Build local `GamesController`
- [x] Build IGDB integration service interface
- [x] Stub IGDB service implementation first
- [x] Implement:
  - [x] `GET /api/games`
  - [x] `GET /api/games/{gameId}`
  - [x] `GET /api/igdb/search`
  - [x] `POST /api/igdb/import/{igdbId}` *(admin only)*

### Phase 5 — library slice
- [x] Build library service
- [x] Implement:
  - [x] `GET /api/library/me`
  - [x] `POST /api/library`
  - [x] `DELETE /api/library/{gameId}`
- [x] Enforce rating validation
- [x] Enforce status rules and ownership

### Phase 6 — reviews + interactions
- [x] Build reviews service
- [x] Implement:
  - [x] `POST /api/reviews`
  - [x] `PUT /api/reviews/{reviewId}`
  - [x] `DELETE /api/reviews/{reviewId}`
  - [x] `POST /api/reviews/{reviewId}/like`
  - [x] `DELETE /api/reviews/{reviewId}/like`
  - [x] `POST /api/reviews/{reviewId}/comments`
  - [x] `DELETE /api/comments/{reviewCommentId}`

### Phase 7 — follows + feed
- [x] Build follow/feed services
- [x] Implement:
  - [x] `GET /api/feed`
  - [x] `POST /api/follows/{userId}`
  - [x] `DELETE /api/follows/{userId}`

### Phase 8 — AI stubs
- [x] Add interfaces:
  - [x] `IRecommendationService`
  - [x] `IReviewAssistantService`
  - [ ] `IEmbeddingService`
  - [x] `ISemanticSearchService`
- [x] Add stubbed endpoints:
  - [x] `POST /api/ai/recommendations`
  - [x] `POST /api/ai/review-assistant`
  - [x] `GET /api/ai/semantic-search`

### Phase 9 — Azure integration later
- [ ] Replace stubs with Azure AI / Azure AI Search implementations
- [ ] Add embeddings pipeline
- [ ] Add vector search index integration
- [ ] Add recommendation logic
- [ ] Add review assistant logic

---

## Milestone status

### First backend milestone
- [x] API builds and runs locally
- [x] Swagger works
- [x] LocalDB connection works
- [x] Identity + JWT works
- [x] Roles exist (`User`, `Admin`)
- [x] Register/login/me endpoints work
- [x] Initial database migration is applied
- [x] Core domain tables for implemented slices exist
- [x] At least one auth happy-path integration test passes
- [x] At least one library happy-path test passes

> **Status:** First backend milestone is complete.

### Current backend MVP status
- [x] Core social/logging/catalog/auth endpoints are implemented locally
- [x] IGDB and AI seams are stubbed
- [x] Automated tests pass across implemented backend slices

---

## Testing plan status

### Service-layer tests
- [x] Library update rules
- [x] Rating validation
- [x] Review creation/edit rules
- [x] Follow rules
- [x] Like/comment rules
- [x] Ownership / authorization rules for implemented slices
- [x] Feed aggregation rules
- [x] Game search/detail rules
- [x] AI stub behavior rules

### Controller / integration tests
- [x] Validation error status codes
- [x] Protected endpoint auth behavior
- [x] Admin-only endpoint protection
- [x] WebApplicationFactory happy-path flows for implemented slices

---

## Notes for implementation

- Keep DTOs separate from entities.
- Keep controllers thin.
- Put business rules in services.
- Manual DTO mapping is fine for MVP.
- Do not overbuild the AI layer early.
- Do not add file upload/storage for avatars right now.
- Prefer stable, testable backend slices over trying to finish every endpoint at once.
- Current development seeding includes:
  - role seeding (`User`, `Admin`)
  - one temporary development test game for local testing

---

## Next immediate step

### Recommended next work
1. Update backend README and TODO docs to reflect the current implemented state.
2. Identify any remaining assignment deliverables around documentation/architecture/testing summaries.
3. Extract token generation into an auth service.
4. Add admin bootstrap strategy.
5. Add global exception handling middleware / strategy.
6. Add structured logging setup.

### Next major implementation phase
1. Replace IGDB stub with real IGDB API integration.
2. Replace AI stubs with Azure AI / Azure AI Search implementations.
3. Add embedding pipeline / vector search wiring.
4. Prepare Azure deployment configuration for the API.
