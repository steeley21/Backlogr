# Backlogr.Web — Frontend

Nuxt 3 + Vuetify 3 + TypeScript frontend for **Backlogr** — a Letterboxd-style social web app for video games.

This frontend is deployed in Azure Static Web Apps and is currently wired to the deployed `Backlogr.Api`, including the live AI surfaces.

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
- Live game detail page wired to:
  - `GET /api/games/{gameId}`
  - `GET /api/games/{gameId}/me`
  - `GET /api/games/{gameId}/reviews`
  - `GET /api/games/{gameId}/activity`
- Game detail page now includes:
  - real metadata rendering
  - community **Reviews** tab
  - community **Activity** tab
  - dedicated **My log** panel using the current user’s game state
- Live library page wired to `GET /api/library/me`
- Log page wired to `POST /api/library`
- Optional review creation from the log page via `POST /api/reviews`
- Log page now supports loading and editing an existing log/review for a selected game
- Live authenticated profile page using `GET /api/auth/me`
- Self-service delete-account UI on profile wired to `POST /api/auth/delete-account`
- Admin dashboard with:
  - user list
  - create user dialog
  - search + role filter
  - role edit flow for `SuperAdmin`
- AI Picks page wired to `POST /api/ai/recommendations`
- Browse page supports **Standard** vs **Semantic** search modes
- Browse semantic search is wired to `GET /api/ai/semantic-search`
- Browse page includes a dedicated on-page search panel for clearer discovery UX
- Review assistant wired to `POST /api/ai/review-assistant`
- Review assistant action bar is polished for draft/rewrite/shorten/expand/spoiler-safe actions
- Local fallback cover asset added for deployment safety
- Production build succeeds locally
- Frontend Vitest setup is in place with current service/store/component/page tests
- Added current test coverage for:
  - `gameService`
  - `pages/game/[id].vue`
  - `pages/log.vue`
- GitHub Actions CI/CD is configured for frontend deployment

### Confirmed deployed behavior
- Deployed frontend loads successfully.
- Register, login, library, and feed are working in production at the same level they were working in development.
- Frontend is successfully calling the deployed API.
- The production browse catalog is populated from the live backend catalog.
- Semantic search is deployed and working through the frontend.
- AI Picks are deployed and working through the frontend.
- Review-assistant actions are deployed and working through the frontend.

### Current known limitations
- Member profile pages are still **authenticated routes** in the current MVP, not signed-out public pages
- Broader frontend test coverage is still incomplete beyond the current covered slices
- Admin delete-user UI is not currently wired in this repo snapshot
- AI relevance is currently MVP-level and should improve as the catalog/search metadata gets richer

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
- The feed page supports **For You** and **Following** source tabs.
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
- Semantic mode allows natural-language discovery against the AI search endpoint.

### Game detail / logging
- Game detail metadata loads from `/api/games/{gameId}`.
- The page loads the current user’s own game state from `/api/games/{gameId}/me`.
- The page loads community reviews from `/api/games/{gameId}/reviews`.
- The page loads mixed game activity from `/api/games/{gameId}/activity`.
- The log page is driven by `gameId` in the query string.
- Logging a game creates or updates the library entry.
- Review creation remains optional from the log workflow.
- Existing log/review state can now be loaded back into the log page for update flows.
- Ratings continue to come from the library/log model rather than a separate review rating field.

### Admin/account management
- The admin dashboard is role-gated from the frontend and expects the admin endpoints from the API.
- `Admin` can create standard users.
- `SuperAdmin` can create higher-privilege accounts and edit roles.
- The current repo snapshot does **not** include a wired delete-user action in the frontend admin flow.

### AI surfaces
- Recommendation page is wired to the live recommendation endpoint.
- Browse page exposes semantic search as a first-class UI mode.
- Log page review assistant is wired to the live review-assistant endpoint.
- Current AI behavior is intentionally MVP-level and prioritizes a stable end-to-end integration over heavy prompt tuning.

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
├── assets/
├── components/
├── layouts/
├── middleware/
├── pages/
├── plugins/
├── services/
├── stores/
├── tests/
├── types/
├── app.vue
├── nuxt.config.ts
└── package.json
```
