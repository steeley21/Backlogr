# Backlogr.Web — Frontend

Nuxt 3 + Vuetify 3 + TypeScript frontend for **Backlogr** — a Letterboxd-style social web app for video games.

This frontend is deployed in Azure Static Web Apps and works against the current `Backlogr.Api` MVP surface. The current local codebase also includes a new public landing page plus an admin user-management dashboard that has been verified locally.

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
- Authentication flow with:
  - `POST /api/auth/login`
  - `POST /api/auth/register`
  - `GET /api/auth/me`
- Public landing page at `/`
- Global auth gating for the app:
  - `/`, `/login`, and `/register` stay public
  - core app routes require authentication
  - `/admin` is additionally role-gated for `Admin` / `SuperAdmin`
- Shared Axios API client with bearer token support and 401 handling
- Pinia auth store with token persistence + user rehydration
- Live feed page wired to `GET /api/feed`
- Feed route moved to `/feed`
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
- AI Picks page wired to `POST /api/ai/recommendations`
- Review assistant wired to `POST /api/ai/review-assistant`
- Local fallback cover asset added for deployment safety
- Admin dashboard at `/admin` for user-management workflows
- Admin dashboard polish pass including:
  - user search
  - role filter
  - create-user dialog validation
  - edit-role flow for `SuperAdmin`
  - safer loading/disabled states
  - success/error snackbars on admin actions
- Shared `getApiErrorMessage()` helper for cleaner API error handling
- Production build succeeds
- GitHub Actions CI/CD is configured for frontend deployment

### Confirmed deployed behavior
- Deployed frontend loads successfully.
- Register, login, library, and feed are working in production at the same level they were working in development.
- Frontend is successfully calling the deployed API.
- The production browse catalog is populated from the live backend catalog.

### Confirmed current local behavior
- Public landing page works and no longer forces authentication on first arrival.
- `/feed` works as the authenticated home/feed route.
- Admin navigation only appears for users with admin roles.
- Admin dashboard user list, create-user flow, and role-edit flow are working locally against the current API.

### Current known limitations
- Public profile pages are not built yet
- Follow/unfollow UI is not built yet
- Review edit/delete UI is not built yet
- Feed like/comment UI is not built yet
- Semantic search UI is not built yet
- Frontend tests are not written yet
- Admin review moderation is not built
- Dedicated import-management UI is not built; IGDB import remains intentionally hidden behind the browse flow
- AI-backed features still rely on the current backend stub behavior

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

### Admin route
- `/admin` *(requires `Admin` or `SuperAdmin`)*

---

## Current Integration Notes

### Auth
- Login/register are live against the API.
- Auth state persists through the Pinia store + token storage.
- Authenticated user rehydration runs through `/api/auth/me`.
- Frontend role checks use the authenticated user’s returned role array.

### Landing / navigation
- `/` is now the public first-impression route.
- The old authenticated home/feed route now lives at `/feed`.
- Auth redirects now land in `/feed` instead of `/`.
- Public and authenticated navigation states are separated in the top bar.

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

### Admin dashboard
- The admin page is a real frontend surface, not a placeholder.
- `Admin` users can view users and create standard users.
- `SuperAdmin` users can also create admins and edit existing `User` / `Admin` roles.
- Self-role editing and `SuperAdmin` role editing are intentionally blocked in the UI.

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

Post-deployment validation completed:
- frontend loads successfully in production
- API integration works from the deployed frontend
- core auth/library/feed flows load as expected in production
- browse is backed by the live catalog/search flow

Local-only features verified after the current deployment pass:
- public landing page
- `/feed` route split
- admin dashboard for user management
- `SuperAdmin`-only role editing UI

If these newer features are deployed later, re-run the same smoke-test checklist against production.

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
- The frontend is now beyond the placeholder-only stage, but several social and AI surfaces still need completion.
