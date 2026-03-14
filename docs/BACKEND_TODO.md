# BACKEND_TODO — Backlogr.Api

Last updated: 2026-03-13

This checklist reflects the current backend state after local MVP completion and live Azure deployment.

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

### Confirmed deployed behavior
- [x] Register works in deployed environment
- [x] Login works in deployed environment
- [x] Library flow works in deployed environment
- [x] Feed flow loads in deployed environment

### Important scope note
- [x] Keep current feature status conservative
- [x] Do not treat incomplete dev features as complete just because deployment is working

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

### Games / catalog
- [x] `Game`
- [x] `IGameService`
- [x] `GameService`
- [x] `GamesController`
- [x] `GET /api/games`
- [x] `GET /api/games/{gameId}`

### IGDB stub slice
- [x] `IIgdbService`
- [x] `StubIgdbService`
- [x] `IgdbController`
- [x] `GET /api/igdb/search`
- [x] `POST /api/igdb/import/{igdbId}` *(admin only)*

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

### Constraints / rules
- [x] `GameLog` unique `(UserId, GameId)`
- [x] `Review` unique `(UserId, GameId)`
- [x] `Follow` unique `(FollowerId, FollowingId)`
- [x] `ReviewLike` unique `(UserId, ReviewId)`
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
- [x] AI stub behavior rules

### Controller / integration tests
- [x] Validation error status codes
- [x] Protected endpoint auth behavior
- [x] Admin-only endpoint protection
- [x] WebApplicationFactory happy-path flows for implemented slices

---

## 4) Deployment / CI-CD

### Deployment status
- [x] API deployed to Azure
- [x] Swagger verified after deployment
- [x] Production config is working
- [x] Deployed frontend can call the deployed API
- [x] API GitHub Actions workflow is working

### Keep after deployment
- [x] Keep secrets out of tracked files
- [x] Keep Azure config values out of docs when not needed
- [x] Keep local and deployed smoke tests separate from feature-completion claims

---

## 5) Remaining backend work

### Higher-priority next work
- [ ] Extract token generation into an auth/token service
- [ ] Add admin bootstrap strategy
- [ ] Add global exception handling middleware
- [ ] Add structured logging / production diagnostics hardening

### Real integration work
- [ ] Replace IGDB stub with real IGDB API integration
- [ ] Replace AI stubs with real Azure AI implementation
- [ ] Add Azure AI Search / semantic search backing
- [ ] Add embedding pipeline / vector search wiring

### Potential cleanup / polish
- [ ] Review production-friendly diagnostics strategy
- [ ] Add clearer deployment documentation/examples where safe
- [ ] Revisit any admin tooling needed for final presentation/demo

---

## 6) Notes

- Keep controllers thin.
- Keep DTOs separate from entities.
- Keep business logic in services.
- Manual DTO mapping is still fine for the current MVP.
- Do not add file upload/storage for avatars right now.
- The backend is now live, but several integrations are still intentionally stubbed.
