# Backlogr Backend TODO

## Current project state

- `Backlogr.Web` is scaffolded.
- Frontend is **not deployed yet**.
- API project exists as **`Backlogr.Api`**.
- Test project exists as **`Backlogr.Api.Tests`**.
- Backend implementation is **actively in progress**.
- Core backend foundation, auth, library endpoints, and initial automated tests are now working locally.

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

### AI decisions
- AI features are required by the final project, but will be implemented **later**.
- AI endpoints should be **stubbed first**.
- Semantic/vector search and recommendation work come after core app functionality.

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
- Game detail endpoint
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

### AI later
- Recommendations endpoint
- Review assistant endpoint
- Semantic search endpoint

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
- [ ] `Review`
- [ ] `Follow`
- [ ] `ReviewLike`
- [ ] `ReviewComment`

### Library slice
- [x] `ILibraryService`
- [x] `LibraryService`
- [x] `LibraryController`
- [x] `GET /api/library/me`
- [x] `POST /api/library`
- [x] `DELETE /api/library/{gameId}`
- [x] Rating validation
- [x] Ownership enforcement
- [x] Development test game seeding for Swagger testing

### Database
- [x] Initial identity migration created and applied
- [x] Game / GameLog migration created and applied
- [x] GameLog alignment migration created and applied

### Testing
- [x] Library service unit tests
- [x] Protected library route unauthorized tests
- [x] Auth controller integration tests for register/login/duplicate/wrong password
- [x] Authenticated `/me` integration coverage
- [x] Authenticated library flow integration tests
- [ ] Admin-only endpoint protection tests
- [ ] Review service/controller tests
- [ ] Follow/feed tests

---

## Planned first-pass entities

### Identity / profile
- [x] `ApplicationUser`

### Catalog
- [x] `Game`

### Library
- [x] `GameLog`

### Reviews / social
- [ ] `Review`
- [ ] `Follow`
- [ ] `ReviewLike`
- [ ] `ReviewComment`

---

## Required constraints

- [x] `GameLog`: unique `(UserId, GameId)`
- [ ] `Review`: unique `(UserId, GameId)`
- [ ] `Follow`: unique `(FollowerId, FollowingId)`
- [ ] `ReviewLike`: unique `(UserId, ReviewId)`
- [ ] Prevent self-follow
- [x] Enforce ownership checks on implemented library delete/update actions

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
  - [ ] `Review`
  - [ ] `Follow`
  - [ ] `ReviewLike`
  - [ ] `ReviewComment`
- [x] Add EF configurations and constraints for implemented entities
- [x] Add migration for implemented domain tables

### Phase 4 — game/catalog slice
- [ ] Build local `GamesController`
- [ ] Build IGDB integration service interface
- [ ] Stub IGDB service implementation first
- [ ] Implement:
  - [ ] `GET /api/games`
  - [ ] `GET /api/games/{gameId}`
  - [ ] `GET /api/igdb/search`
  - [ ] `POST /api/igdb/import/{igdbId}` *(admin only)*

### Phase 5 — library slice
- [x] Build library service
- [x] Implement:
  - [x] `GET /api/library/me`
  - [x] `POST /api/library`
  - [x] `DELETE /api/library/{gameId}`
- [x] Enforce rating validation
- [x] Enforce status rules and ownership

### Phase 6 — reviews + interactions
- [ ] Build reviews service
- [ ] Implement:
  - [ ] `POST /api/reviews`
  - [ ] `PUT /api/reviews/{reviewId}`
  - [ ] `DELETE /api/reviews/{reviewId}`
  - [ ] `POST /api/reviews/{reviewId}/like`
  - [ ] `DELETE /api/reviews/{reviewId}/like`
  - [ ] `POST /api/reviews/{reviewId}/comments`
  - [ ] `DELETE /api/comments/{reviewCommentId}`

### Phase 7 — follows + feed
- [ ] Build follow/feed services
- [ ] Implement:
  - [ ] `GET /api/feed`
  - [ ] `POST /api/follows/{userId}`
  - [ ] `DELETE /api/follows/{userId}`

### Phase 8 — AI stubs
- [ ] Add interfaces:
  - [ ] `IRecommendationService`
  - [ ] `IReviewAssistantService`
  - [ ] `IEmbeddingService`
  - [ ] `ISemanticSearchService`
- [ ] Add stubbed endpoints:
  - [ ] `POST /api/ai/recommendations`
  - [ ] `POST /api/ai/review-assistant`
  - [ ] `GET /api/ai/semantic-search`

### Phase 9 — Azure integration later
- [ ] Replace stubs with Azure AI / Azure AI Search implementations
- [ ] Add embeddings pipeline
- [ ] Add vector search index integration
- [ ] Add recommendation logic
- [ ] Add review assistant logic

---

## First milestone definition

The first backend milestone is complete when all of the following are true:

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

---

## Testing plan status

### Service-layer tests
- [x] Library update rules
- [x] Rating validation
- [ ] Review creation/edit rules
- [ ] Follow rules
- [ ] Like/comment rules
- [x] Ownership / authorization rules for implemented library actions

### Controller / integration tests
- [x] Validation error status codes
- [x] Protected endpoint auth behavior
- [ ] Admin-only endpoint protection
- [x] WebApplicationFactory happy-path flows for auth/library

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
  - one temporary development test game for library endpoint testing

---

## Next immediate step

### Recommended next feature slice
1. Create `Review` entity and DTOs.
2. Add EF configuration + migration for reviews.
3. Build review service and controller.
4. Implement:
   - `POST /api/reviews`
   - `PUT /api/reviews/{reviewId}`
   - `DELETE /api/reviews/{reviewId}`
5. Add review service tests and review integration tests.

### Cleanup / technical debt after that
- Extract token generation into an auth/token service.
- Add admin bootstrap strategy.
- Add global exception handling middleware / strategy.
- Add structured logging setup.
