# Backlogr.Web — Frontend

Nuxt 3 + Vuetify 3 + TypeScript frontend for **Backlogr** — a Letterboxd-style social web app for video games.

This frontend is deployed in Azure Static Web Apps, and the current codebase now also includes the new landing-page, admin, and account-management work that is ready for deployment and smoke testing with the latest API.

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
- **Vitest** *(installed; tests still need to be added)*

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
- Production build succeeds
- GitHub Actions CI/CD is configured for frontend deployment

### Confirmed deployed behavior
- Deployed frontend loads successfully.
- Register, login, library, and feed are working in production at the same level they were working in development.
- Frontend is successfully calling the deployed API.
- The production browse catalog is populated from the live backend catalog.

### Latest codebase additions ready for deploy/smoke test
- Public landing page at `/`
- Authenticated feed moved to `/feed`
- Admin dashboard and role-based admin navigation
- Admin account-management flows for list/create/edit/delete
- Profile delete-account flow

### Current known limitations
- Public profile pages are not built yet
- Follow/unfollow UI is not built yet
- Review edit/delete UI is not built yet
- Feed like/comment UI is not built yet
- Semantic search UI is not built yet
- Frontend tests are not written yet
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
- `/log/[gameId]`
- `/profile`
- `/recommend`

### Admin-only route
- `/admin`

---

## Current Integration Notes

### Auth
- Login/register are live against the API.
- Auth state persists through the Pinia store + token storage.
- Authenticated user rehydration runs through `/api/auth/me`.
- Profile now includes a self-service delete-account flow with confirmation checks.

### Feed
- Feed is live against the backend.
- Backend includes the current user’s own activity along with followed-user activity.

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

### Admin/account management
- The admin dashboard is role-gated from the frontend and expects the new admin endpoints from the API.
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
├── app/
│   ├── components/
│   ├── layouts/
│   ├── middleware/
│   ├── pages/
│   ├── plugins/
│   ├── services/
│   ├── stores/
│   ├── types/
│   └── utils/
├── public/
├── .env.example
├── nuxt.config.ts
├── package.json
└── tsconfig.json
```

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
- verify `/feed` redirect/login flow
- verify `/admin` access control for non-admin vs admin users
- verify admin create/edit/delete flows against the deployed API
- verify profile delete-account flow against the deployed API

---

## Remaining Frontend Work

Recommended next frontend work:
1. Write frontend service/store/component tests.
2. Build public profile pages.
3. Build follow/unfollow UI.
4. Build review edit/delete UI.
5. Add feed like/comment UI.
6. Add semantic search UI.
7. Do a final accessibility/mobile polish pass.

---

## Notes

- Keep using explicit imports in project code to avoid local Nuxt/VS Code auto-import issues.
- Do not overstate incomplete features just because deployment is live.
- The frontend is beyond the placeholder-only stage, but several social and AI surfaces still need completion.
