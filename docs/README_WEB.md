# Backlogr.Web — Frontend

Nuxt 3 + Vuetify 3 + TypeScript frontend for **Backlogr** — a Letterboxd-style social web app for video games.

This frontend is deployed in Azure Static Web Apps. The current codebase now includes the landing page, admin/account-management flows, member profile pages, feed review interactions, the new **For You / Following** feed split, and the current Vitest coverage pass.

> **Document location:** this file lives in the repo root `docs/` folder.

**Production web URL:** `https://victorious-grass-0bb8bf10f.2.azurestaticapps.net/`

---

## Tech Stack

- **Nuxt 3**
- **Vue 3**
- **Vuetify 3**
- **TypeScript**
- **Pinia**
- **Axios**
- **Material Design Icons (MDI)**
- **ESLint**
- **Vitest** for current frontend unit/component/page coverage

---

## Current Status

The frontend works locally and is deployed against the current `Backlogr.Api` MVP surface.

### Implemented now
- Public landing page at `/`
- Authentication flow with:
  - `POST /api/auth/login`
  - `POST /api/auth/register`
  - `GET /api/auth/me`
- Global auth gating for the app:
  - `/`, `/login`, and `/register` stay public
  - core app routes require authentication
  - `/admin` is role-gated for `Admin` / `SuperAdmin`
- Shared Axios API client with bearer token support and 401 handling
- Pinia auth store with token persistence + user rehydration
- Feed moved to `/feed` and wired to `GET /api/feed`
- Feed now supports two source tabs:
  - **For You** — all recent activity across the app
  - **Following** — followed-user activity plus the current user’s own activity
- Feed source tab is preserved in the route query:
  - `/feed?tab=for-you`
  - `/feed?tab=following`
- Feed review cards now support:
  - like / unlike
  - inline comment threads
  - add comment
  - delete own comment
  - edit / delete for review owners
- Member profile route at `/u/[username]` with:
  - profile header and bio
  - follow / unfollow button
  - follower / following / review / library counts
  - recent reviews
  - public library tab
- Live browse page wired to the merged game search flow:
  - `GET /api/games/search`
  - local catalog results first
  - IGDB-backed fallback results from the API when needed
- Browse click flow now:
  - routes directly to `/game/[id]` when a result already exists locally
  - imports automatically through `POST /api/igdb/import/{igdbId}` when needed
  - then routes to the normal local game detail page
- Live game detail page wired to `GET /api/games/{gameId}`
- Live library page wired to `GET /api/library/me`
- Log page wired to `POST /api/library`
- Optional review creation from the log page via `POST /api/reviews`
- Live authenticated profile page using `GET /api/auth/me`
- Self-service delete-account UI on profile wired to `POST /api/auth/delete-account`
- Admin dashboard with:
  - user list
  - create user dialog
  - search + role filter
  - `SuperAdmin` role edit flow
  - delete-user flow for allowed targets
- AI Picks page wired to `POST /api/ai/recommendations`
- Review assistant wired to `POST /api/ai/review-assistant`
- Local fallback cover asset added for deployment safety
- Production build succeeds locally
- Frontend Vitest setup is in place with current service/store/component/page tests
- GitHub Actions CI/CD is configured for frontend deployment

### Confirmed deployed behavior
- Deployed frontend loads successfully.
- Register, login, library, and feed are working in production at the same level they were working in development.
- Frontend is successfully calling the deployed API.
- The production browse catalog is populated from the live backend catalog.

### Latest codebase additions ready for next deploy / smoke test
- Feed source tabs: **For You** and **Following**
- Route-query persistence for the selected feed tab
- Feed page empty-state/copy updates for the new feed split
- Feed page Vitest coverage for source-tab behavior
- Expanded `feedService` test coverage for feed scopes

### Current known limitations
- Member profile pages are still **authenticated routes** in the current MVP, not signed-out public pages
- Semantic search UI is not built yet
- Game-detail review/activity tabs are not built yet
- Broader frontend test coverage is still incomplete beyond the current covered slices
- AI-backed features are still limited to the current backend stub behavior

---

## Routing / App Areas

### Public routes
- `/`
- `/login`
- `/register`

### Authenticated routes in current MVP
- `/feed`
- `/browse`
- `/game/[id]`
- `/library`
- `/log` *(expects `?gameId=...`)*
- `/profile`
- `/recommend`
- `/u/[username]`

### Admin-only route
- `/admin`

---

## Current Integration Notes

### Auth
- Login/register are live against the API.
- Auth state persists through the Pinia store + token storage.
- Authenticated user rehydration runs through `/api/auth/me`.
- Profile includes a self-service delete-account flow with confirmation checks.

### Feed
- Feed is live against the backend.
- The feed page now supports **For You** and **Following** source tabs.
- The selected tab is stored in the route query so refresh/back navigation keeps the same feed source.
- Review cards use backend-provided like/comment counts and liked/owner state.
- Review likes and comments are updated inline from the feed page.
- The existing local `All / Reviews / Logs` filter still applies inside both feed tabs.

### Member profiles / follows
- Member profiles load from `/api/profiles/{userName}` and `/api/profiles/{userName}/library`.
- User names are treated as unique handles for member-profile routing.
- Follow/unfollow actions call the live API and update the visible follower count optimistically.
- In the current MVP, member profiles are available inside the authenticated app rather than to signed-out visitors.

### Browse / catalog
- Browse is no longer local-only.
- Query-string search is supported.
- The page uses the merged backend browse endpoint rather than only `GET /api/games`.
- The UI does **not** visually signal whether a result was already in the local database.
- Imported results are normalized back into the standard local `gameId` route flow.

### Library / logging
- Library is loaded from live backend data.
- Logging a game creates/updates the library entry.
- Review creation is optional from the log workflow.
- Ratings continue to come from the library/log model rather than a separate review rating field.
- The log page is currently driven by `gameId` in the query string.

### Admin/account management
- The admin dashboard is role-gated from the frontend and expects the admin endpoints from the API.
- `Admin` can create standard users.
- `SuperAdmin` can create higher-privilege accounts, edit roles, and delete allowed accounts.
- Protected actions use confirmation dialogs, filtered actions, and disabled states to reduce mistakes.

### AI surfaces
- Recommendation and review-assistant pages are wired.
- These still rely on the current backend stub behavior rather than real AI integration.

---

## Local Development

### Prerequisites
- Node.js 20+
- npm
- Running `Backlogr.Api` instance

### Install dependencies
From `Backlogr.Web`:

```powershell
npm install
```

### Run in development

```powershell
npm run dev
```

### Build for production

```powershell
npm run build
```

### Run tests

```powershell
npm test
```

### Runtime config
The frontend reads its API base URL from runtime config.

Expected environment variable:

```text
NUXT_PUBLIC_API_BASE=https://backlograpi.azurewebsites.net
```

Keep this configurable per environment.

If you are running against the local API during development, point `NUXT_PUBLIC_API_BASE` at the local API URL instead.

---

## Current Frontend Structure

```text
Backlogr.Web/
├── components/
├── layouts/
├── middleware/
├── pages/
├── plugins/
├── public/
├── services/
├── stores/
├── tests/
├── types/
├── utils/
├── .env.example
├── nuxt.config.ts
├── package.json
├── tsconfig.json
└── vitest.config.ts
```

---

## Testing

Current frontend coverage includes:
- service tests for `feedService`, `profileService`, `reviewService`, and `followService`
- store tests for `authStore`
- component tests for `FeedReviewCard` and `FeedReviewCommentThread`
- page test coverage for `/feed`
- page test coverage for `/u/[username]`

This is still partial app coverage, but the current covered slices are passing.

---

## Deployment Notes

Current deployment status:
- Frontend is deployed and working in Azure Static Web Apps.
- `NUXT_PUBLIC_API_BASE` is configured to use the deployed API.
- API CORS is configured to allow the deployed frontend origin.
- GitHub Actions CI/CD is configured for the frontend.

Post-deployment validation already completed for the currently deployed build:
- frontend loads successfully in production
- API integration works from the deployed frontend
- core auth/library/feed flows load as expected in production
- browse is backed by the live catalog/search flow

Recommended smoke tests for the next deploy:
- verify `/` landing page behavior for signed-out users
- verify `/feed?tab=for-you` loads the broader feed correctly
- verify `/feed?tab=following` loads followed-user + self activity correctly
- verify the feed tab stays selected after refresh/navigation
- verify follow/unfollow behavior against the deployed API
- verify feed review like/comment/edit/delete flows against the deployed API
- verify `/u/[username]` member-profile load for an authenticated user
- verify `/admin` access control for non-admin vs admin users
- verify profile delete-account flow against the deployed API

---

## Remaining Frontend Work

Recommended next frontend work:
1. Add semantic search UI.
2. Finish game-detail review/activity wiring.
3. Expand frontend test coverage beyond the current covered slices.
4. Add broader shared snackbar/toast patterns where useful.
5. Do a final accessibility/mobile polish pass.

---

## Notes

- Keep using explicit imports in project code to avoid local Nuxt/VS Code auto-import issues.
- Do not overstate incomplete features just because deployment is live.
- The frontend now covers the core social/profile/feed MVP slice, including the new feed split, but semantic search and broader polish/testing still remain.
