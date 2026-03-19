# BACKEND_TODO — Backlogr.Api

Last updated: 2026-03-18

This checklist reflects the current backend state after Azure deployment, real IGDB integration, admin/user-management, self-service account deletion, the member-profile/feed-social expansion, the **For You / Following** feed scope split, the deployed AI/vector-search integration, and the game-detail read-surface pass.

> **Document location:** this file lives in the repo root `docs/` folder.

Source requirements: `requirements_backlogr_updated.md` and `Assignment5AndFinal.md`.

---

## 0) Current backend status

### Core backend state
- [x] Backend MVP surface is implemented
- [x] Automated backend tests pass across the current covered slices
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
- [x] Semantic search works in deployed environment
- [x] AI recommendations work in deployed environment
- [x] Review-assistant flow works in deployed environment

### Recently completed backend additions
- [x] `SuperAdmin` role added to the backend role model
- [x] Admin user list/create/role-edit endpoints added
- [x] Self-service account deletion endpoint added
- [x] Shared user-deletion cleanup path added for dependent records
- [x] Authenticated member profile endpoint added
- [x] Authenticated member public-library endpoint added
- [x] Review comment-read endpoint added for inline comment threads
- [x] Feed DTO expanded with social-state fields needed by the frontend
- [x] Feed scope support added for **For You** vs **Following**
- [x] Feed tests updated to cover both scopes
- [x] OpenAI configuration/options added for chat + embeddings
- [x] Azure AI Search configuration/options added for vector search
- [x] AI search index creation/backfill added on startup
- [x] Real semantic search service wired to Azure AI Search
- [x] Real recommendation service wired to the user's own logs/ratings/reviews
- [x] Real review-assistant service wired to OpenAI
- [x] Game-detail viewer-state endpoint added
- [x] Game-detail reviews endpoint added
- [x] Game-detail activity endpoint added
- [x] Backend coverage added for the new game-detail read surface

### Important scope notes
- [x] Treat the live AI/vector-search path as deployed and working
- [x] Keep current feature status conservative where wiring is still incomplete
- [ ] Admin delete-user logic is not exposed through a controller route in the current repo snapshot

---

## 1) Backend MVP slices

### Auth / identity
- [x] `ApplicationUser`
- [x] `ApplicationRole`
- [x] Identity + JWT auth
- [x] `POST /api/auth/register`
- [x] `POST /api/auth/login`
- [x] `GET /api/auth/me`
- [x] `POST /api/auth/delete-account`
- [x] Role seeding for `User`, `Admin`, and `SuperAdmin`
- [x] Development admin seeding via user secrets

### Admin / account management
- [x] `GET /api/admin/users`
- [x] `POST /api/admin/users`
- [x] `PUT /api/admin/users/{userId}/role`
- [x] Admin can create `User`
- [x] `SuperAdmin` can create `Admin`
- [x] `SuperAdmin` can grant `SuperAdmin`
- [x] Self-role edit is blocked
- [x] Deleting your own account is handled through the self-delete auth flow instead
- [x] Self-delete blocks removing the last remaining `SuperAdmin`
- [x] Temporary bootstrap endpoint exists for one-time elevation when explicitly enabled
- [ ] `DELETE /api/admin/users/{userId}` controller route
- [ ] Admin dashboard-backed delete-user endpoint flow

### Games / catalog
- [x] `Game`
- [x] `IGameService`
- [x] `GameService`
- [x] `GamesController`
- [x] `GET /api/games`
- [x] `GET /api/games/search`
- [x] `GET /api/games/{gameId}`
- [x] `GET /api/games/{gameId}/me`
- [x] `GET /api/games/{gameId}/reviews`
- [x] `GET /api/games/{gameId}/activity`

### IGDB slice
- [x] `IIgdbService`
- [x] Real `IgdbService`
- [x] Twitch app token service / caching
- [x] `IgdbController`
- [x] `GET /api/igdb/search`
- [x] `POST /api/igdb/import/{igdbId}` *(authenticated)*
- [x] Import/update local `Game` rows by `IgdbId`
- [x] Merge local catalog results with IGDB fallback for browse/search

### Profiles / member-social
- [x] `IProfileService`
- [x] `ProfileService`
- [x] `ProfilesController`
- [x] `GET /api/profiles/{userName}` *(authenticated)*
- [x] `GET /api/profiles/{userName}/library` *(authenticated)*
- [x] Username-backed member profile lookup
- [x] Follow state / counts returned in member profile payload
- [x] Public library payload excludes private notes

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
- [x] `GET /api/reviews/{reviewId}/comments`
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
- [x] `GET /api/feed?scope=for-you`
- [x] `GET /api/feed?scope=following`
- [x] **For You** scope returns broader recent activity
- [x] **Following** scope returns followed-user activity plus current-user activity
- [x] Feed includes avatar, counts, liked-state, and owner-state fields
- [x] Invalid feed scopes return `400 Bad Request`

### AI / vector search
- [x] `IRecommendationService`
- [x] `IReviewAssistantService`
- [x] `IEmbeddingService`
- [x] `IAiSearchIndexService`
- [x] `IAiSearchSyncService`
- [x] `ISemanticSearchService`
- [x] `OpenAiEmbeddingService`
- [x] `AzureAiSearchIndexService`
- [x] `AiSearchSyncService`
- [x] `AzureRecommendationService`
- [x] `OpenAiReviewAssistantService`
- [x] `AzureSemanticSearchService`
- [x] `AiController`
- [x] `POST /api/ai/recommendations`
- [x] `POST /api/ai/review-assistant`
- [x] `GET /api/ai/semantic-search`
- [x] Azure AI Search `games` index/backfill path

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
- [x] User deletion cleans up dependent follow/comment/like data before account removal

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
- [x] Feed scope rules for **For You** and **Following**
- [x] Member profile query rules
- [x] Game search/detail rules
- [x] Game-detail viewer-state/read rules
- [x] IGDB search/import flow rules
- [ ] Dedicated automated coverage for the real OpenAI/Azure AI Search services

### Controller / integration tests
- [x] Validation error status codes
- [x] Protected endpoint auth behavior
- [x] Authenticated IGDB endpoint protection
- [x] WebApplicationFactory happy-path flows for implemented slices
- [x] Admin user list/create/role-edit permission tests
- [x] Member profile endpoint auth/flow tests
- [x] Review comment-read auth/flow tests
- [x] Feed integration assertions for both feed scopes
- [x] Feed integration assertions for expanded social-state fields
- [x] Game-detail endpoint integration coverage
- [x] AI endpoint auth + happy-path flow tests with fake AI services

### Next backend tests to add
- [ ] Self-delete account integration tests
- [ ] `SuperAdmin` grant/delete guardrail tests for the latest account-management additions
- [ ] Real semantic-search service/result-mapping tests
- [ ] Real recommendation service tests for already-logged-game exclusion
- [ ] Real review-assistant response parsing tests

---

## 4) Deployment / CI-CD

### Deployment status
- [x] API deployed to Azure
- [x] Swagger verified after deployment
- [x] Production config is working
- [x] Deployed frontend can call the deployed API
- [x] API GitHub Actions workflow is working
- [x] Production IGDB secrets are configured in Azure
- [x] Production OpenAI configuration is working
- [x] Production Azure AI Search configuration is working
- [x] Semantic search is verified in production
- [x] Recommendations are verified in production
- [x] Review assistant is verified in production

### Keep after deployment
- [x] Keep secrets out of tracked files
- [x] Keep Azure config values out of docs when not needed
- [x] Keep local and production databases/configuration separated

### Additional smoke tests worth re-running
- [ ] Verify `GET /api/admin/users` behavior in production for Admin/SuperAdmin
- [ ] Verify `POST /api/admin/users` respects Admin vs `SuperAdmin` role limits
- [ ] Verify `PUT /api/admin/users/{userId}/role` works only for `SuperAdmin`
- [ ] Verify `POST /api/auth/delete-account` with both failure and success cases in production
- [ ] Verify `GET /api/profiles/{userName}` for a signed-in user in production
- [ ] Verify `GET /api/profiles/{userName}/library` returns expected public-library data in production
- [ ] Verify `GET /api/reviews/{reviewId}/comments` supports the deployed frontend comment-thread flow
- [ ] Verify `GET /api/feed?scope=for-you` returns the broader feed in production
- [ ] Verify `GET /api/feed?scope=following` returns followed-user + self activity in production
- [ ] Verify `GET /api/games/{gameId}/me` in production
- [ ] Verify `GET /api/games/{gameId}/reviews` in production
- [ ] Verify `GET /api/games/{gameId}/activity` in production
- [ ] Verify invalid `scope` values return `400 Bad Request` in production

---

## 5) Remaining backend work

### Higher-priority next work
- [ ] Extract auth token generation into a dedicated service
- [ ] Add global exception handling middleware
- [ ] Add structured logging / production diagnostics hardening
- [ ] Decide whether the temporary bootstrap controller should remain in the final delivered codebase or be removed after production setup is stable

### Integration / architecture follow-up
- [ ] Add dedicated automated coverage for the real AI/vector-search services
- [ ] Consider incremental catalog re-indexing tied to import/update flows
- [ ] Decide whether admin delete-user should be exposed as a real controller route for the final scope

### Potential cleanup / polish
- [ ] Review production-friendly diagnostics strategy
- [ ] Add clearer deployment documentation/examples where safe
- [ ] Revisit any admin/demo tooling needed for the final presentation
- [ ] Clean up any optional catalog seeding/import helper scripts before final handoff
- [ ] Revisit feed paging/filtering strategy once production usage grows

---

## 6) Notes

- Keep controllers thin.
- Keep DTOs separate from entities.
- Keep business logic in services.
- Manual DTO mapping is still fine for the current MVP.
- Do not add file upload/storage for avatars right now.
- The backend is live, real IGDB integration is in place, and the AI/vector-search path is now part of the deployed app state.
