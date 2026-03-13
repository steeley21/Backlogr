# Backlogr TODO

## Current project state

- `Backlogr.Web` is scaffolded.
- Frontend is **not deployed yet**.
- API project has been created as **`Backlogr.Api`**.
- Test project has been created as **`Backlogr.Api.Tests`**.
- Backend implementation has **not started yet**.

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
- **.NET 8 / ASP.NET Core Web API**
- **Entity Framework Core 8**
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

## Planned first-pass entities

### Identity / profile
- `ApplicationUser`

### Catalog
- `Game`

### Library
- `GameLog`

### Reviews / social
- `Review`
- `Follow`
- `ReviewLike`
- `ReviewComment`

---

## Required constraints

- `GameLog`: unique `(UserId, GameId)`
- `Review`: unique `(UserId, GameId)`
- `Follow`: unique `(FollowerId, FollowingId)`
- `ReviewLike`: unique `(UserId, ReviewId)`
- Prevent self-follow
- Enforce ownership checks on update/delete actions

---

## Suggested solution / folder layout

```text
BACKLOGR/
├── Backlogr.Web/
├── Backlogr.Api/
├── Backlogr.Api.Tests/
├── docs/
├── Backlogr.sln
├── README.md
└── requirements_backlogr.md
```

### Inside `Backlogr.Api`

```text
Backlogr.Api/
├── Controllers/
├── Data/
│   ├── ApplicationDbContext.cs
│   ├── Configurations/
│   └── Migrations/
├── DTOs/
├── Extensions/
├── Middleware/
├── Models/
│   ├── Entities/
│   └── Enums/
├── Options/
├── Services/
│   ├── Implementations/
│   └── Interfaces/
├── Common/
├── Mapping/
└── Program.cs
```

---

## Initial setup checklist

### Solution / project setup
- [x] Create `Backlogr.Api`
- [x] Create `Backlogr.Api.Tests`
- [ ] Create / confirm `Backlogr.sln`
- [ ] Add both projects to the solution
- [ ] Add test project reference to API project

### NuGet packages to install
- [ ] `Microsoft.EntityFrameworkCore.SqlServer`
- [ ] `Microsoft.EntityFrameworkCore.Design`
- [ ] `Microsoft.EntityFrameworkCore.Tools`
- [ ] `Microsoft.AspNetCore.Identity.EntityFrameworkCore`
- [ ] `Microsoft.AspNetCore.Authentication.JwtBearer`
- [ ] `Swashbuckle.AspNetCore` *(if not already included)*
- [ ] `Microsoft.AspNetCore.Mvc.Testing` *(test project)*
- [ ] `FluentAssertions` *(test project)*

### Configuration
- [ ] Set up `appsettings.Development.json`
- [ ] Add LocalDB connection string
- [ ] Add JWT settings
- [ ] Add CORS settings for local frontend
- [ ] Initialize user secrets
- [ ] Move secrets out of committed config files

### Core backend plumbing
- [ ] Create `ApplicationUser` with Guid identity key
- [ ] Create `ApplicationDbContext`
- [ ] Configure Identity
- [ ] Configure JWT auth
- [ ] Configure authorization policies / roles
- [ ] Configure Swagger/OpenAPI
- [ ] Configure CORS for `Backlogr.Web`
- [ ] Add global exception handling strategy
- [ ] Add basic logging setup

---

## First implementation order

### Phase 1 — foundation
- [ ] Confirm solution builds cleanly
- [ ] Set up EF Core + Identity + JWT
- [ ] Add initial options/config classes
- [ ] Add first migration
- [ ] Create database locally
- [ ] Seed roles: `User`, `Admin`
- [ ] Add a way to seed an initial admin account

### Phase 2 — auth slice
- [ ] Build `AuthController`
- [ ] Implement:
  - [ ] `POST /api/auth/register`
  - [ ] `POST /api/auth/login`
  - [ ] `GET /api/auth/me`
- [ ] Create DTOs for auth flows
- [ ] Add auth service / token generation service
- [ ] Test auth endpoints

### Phase 3 — core data model
- [ ] Create entities:
  - [ ] `Game`
  - [ ] `GameLog`
  - [ ] `Review`
  - [ ] `Follow`
  - [ ] `ReviewLike`
  - [ ] `ReviewComment`
- [ ] Add EF configurations and constraints
- [ ] Add migration for domain tables

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
- [ ] Build library service
- [ ] Implement:
  - [ ] `GET /api/library/me`
  - [ ] `POST /api/library`
  - [ ] `DELETE /api/library/{gameId}`
- [ ] Enforce rating validation
- [ ] Enforce status rules and ownership

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

- [ ] API builds and runs locally
- [ ] Swagger works
- [ ] LocalDB connection works
- [ ] Identity + JWT works
- [ ] Roles exist (`User`, `Admin`)
- [ ] Register/login/me endpoints work
- [ ] Initial database migration is applied
- [ ] Core domain tables exist
- [ ] At least one auth happy-path integration test passes
- [ ] At least one library happy-path test passes

---

## Testing plan

### Service-layer tests
- [ ] Library update rules
- [ ] Rating validation
- [ ] Review creation/edit rules
- [ ] Follow rules
- [ ] Like/comment rules
- [ ] Ownership / authorization rules

### Controller / integration tests
- [ ] Validation error status codes
- [ ] Protected endpoint auth behavior
- [ ] Admin-only endpoint protection
- [ ] WebApplicationFactory happy-path flows

---

## Notes for implementation

- Keep DTOs separate from entities.
- Keep controllers thin.
- Put business rules in services.
- Manual DTO mapping is fine for MVP.
- Do not overbuild the AI layer early.
- Do not add file upload/storage for avatars right now.
- Prefer stable, testable backend slices over trying to finish every endpoint at once.

---

## Next immediate step

**Next step when work begins:**
1. Confirm the solution file and project references.
2. Install core NuGet packages.
3. Set up Identity + EF Core + LocalDB.
4. Create `ApplicationUser` and `ApplicationDbContext`.
5. Wire JWT auth and Swagger.
6. Add the first migration.
