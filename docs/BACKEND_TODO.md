# BACKEND_TODO — Backlogr.Api

Last updated: 2026-03-14

This checklist reflects the current backend state after Azure deployment, real IGDB integration, and local/prod config cleanup.

> **Document location:** this file lives in the repo root `docs/` folder.

Source requirements: `requirements_backlogr_updated.md` and `Assignment5AndFinal.md`.

---

## 0) Current backend status

### Core backend state
- [x] Backend MVP surface is implemented
- [x] Automated backend tests pass across implemented slices
- [x] API is deployed and working in Azure
- [x] Swagger works in the deployed environment
- [x] GitHub Actions CI/CD is configured for the API
- [x] Frontend is successfully using the deployed API
- [x] Local development is back on a separate LocalDB target

### Confirmed deployed behavior
- [x] Register works in deployed environment
- [x] Login works in deployed environment
- [x] Library flow works in deployed environment
- [x] Feed flow loads in deployed environment
- [x] IGDB search works in deployed environment
- [x] IGDB import works in deployed environment

### Important scope note
- [x] Keep current feature status conservative
- [x] Do not treat incomplete AI/vector-search work as complete just because deployment is working

---

## 1) Backend MVP slices

### Auth / identity
- [x] `ApplicationUser`
- [x] `ApplicationRole`
- [x] Identity + JWT auth
- [x] `POST /api/auth/register`
- [x] `POST /api/auth/login`
- [x] `GET /api/auth/me`
- [x] Role seeding for `User` and `Admin`
- [x] Development admin seeding via user secrets

### Games / catalog
- [x] `Game`
- [x] `IGameService`
- [x] `GameService`
- [x] `GamesController`
- [x] `GET /api/games`
- [x] `GET /api/games/search`
- [x] `GET /api/games/{gameId}`

### IGDB slice
- [x] `IIgdbService`
- [x] Real `IgdbService`
- [x] Twitch app token service / caching
- [x] `IgdbController`
- [x] `GET /api/igdb/search`
- [x] `POST /api/igdb/import/{igdbId}` *(authenticated)*
- [x] Import/update local `Game` rows by `IgdbId`
- [x] Merge local catalog results with IGDB fallback for browse/search

### Library / logging
- [x] `GameLog`
- [x] `LibraryStatus`
- [x] `ILibraryService`
- [x] `LibraryService`
- [x] `LibraryController`
- [x] `GET /api/library/me`
- [x] `POST /api/library`
- [x] `DELETE /api/library/{gameId}`

### Reviews
- [x] `Review`
- [x] `IReviewService`
- [x] `ReviewService`
- [x] `ReviewsController`
- [x] `POST /api/reviews`
- [x] `PUT /api/reviews/{reviewId}`
- [x] `DELETE /api/reviews/{reviewId}`

### Review interactions
- [x] `ReviewLike`
- [x] `ReviewComment`
- [x] `IReviewInteractionService`
- [x] `ReviewInteractionService`
- [x] `CommentsController`
- [x] `POST /api/reviews/{reviewId}/like`
- [x] `DELETE /api/reviews/{reviewId}/like`
- [x] `POST /api/reviews/{reviewId}/comments`
- [x] `DELETE /api/comments/{reviewCommentId}`

### Follow / feed
- [x] `Follow`
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

---

## 2) Database / schema

- [x] Initial identity migration created
- [x] Games / GameLogs migration created
- [x] GameLog alignment migration created
- [x] Reviews migration created
- [x] ReviewLike / ReviewComment migration created
- [x] Follows migration created
- [x] Local development database update works
- [x] Deployed database schema was applied successfully
- [x] Local development now targets LocalDB instead of the deployed database

### Constraints / rules
- [x] `GameLog` unique `(UserId, GameId)`
- [x] `Review` unique `(UserId, GameId)`
- [x] `Follow` unique `(FollowerId, FollowingId)`
- [x] `ReviewLike` unique `(UserId, ReviewId)`
- [x] `Game.IgdbId` unique filtered index when present
- [x] Prevent self-follow
- [x] Keep `GameLog.Rating` as the rating source of truth

---

## 3) Testing status

### Service-layer tests
- [x] Library update rules
- [x] Rating validation
- [x] Review creation/edit rules
- [x] Follow rules
- [x] Like/comment rules
- [x] Ownership / authorization rules for implemented slices
- [x] Feed aggregation rules
- [x] Game search/detail rules
- [x] IGDB search/import flow rules
- [x] AI stub behavior rules

### Controller / integration tests
- [x] Validation error status codes
- [x] Protected endpoint auth behavior
- [x] Authenticated IGDB endpoint protection
- [x] WebApplicationFactory happy-path flows for implemented slices

---

## 4) Deployment / CI-CD

### Deployment status
- [x] API deployed to Azure
- [x] Swagger verified after deployment
- [x] Production config is working
- [x] Deployed frontend can call the deployed API
- [x] API GitHub Actions workflow is working
- [x] Production IGDB secrets are configured in Azure

### Keep after deployment
- [x] Keep secrets out of tracked files
- [x] Keep Azure config values out of docs when not needed
- [x] Keep local and deployed smoke tests separate from feature-completion claims
- [x] Keep local and production databases/configuration separated

---

## 5) Remaining backend work

### Higher-priority next work
- [ ] Extract auth token generation into a dedicated service
- [ ] Add production-friendly admin/bootstrap tooling
- [ ] Add global exception handling middleware
- [ ] Add structured logging / production diagnostics hardening

### Real integration work
- [ ] Replace AI stubs with real Azure AI implementation
- [ ] Add Azure AI Search / semantic search backing
- [ ] Add embedding pipeline / vector search wiring

### Potential cleanup / polish
- [ ] Review production-friendly diagnostics strategy
- [ ] Add clearer deployment documentation/examples where safe
- [ ] Revisit any admin/demo tooling needed for the final presentation
- [ ] Clean up any optional catalog seeding/import helper scripts before final handoff

---

## 6) Notes

- Keep controllers thin.
- Keep DTOs separate from entities.
- Keep business logic in services.
- Manual DTO mapping is still fine for the current MVP.
- Do not add file upload/storage for avatars right now.
- The backend is live and real IGDB integration is in place, but AI/vector-search surfaces are still intentionally stubbed.
