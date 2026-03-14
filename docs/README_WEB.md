# Backlogr.Web — Frontend

Nuxt 3 + Vuetify 3 + TypeScript frontend for **Backlogr** — a Letterboxd-style social web app for video games.

This frontend is now **deployed and working in Azure Static Web Apps** and is wired to the deployed `Backlogr.Api` for the current MVP loop.

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

The frontend works locally and is now deployed against the current `Backlogr.Api` MVP surface.

### Implemented now
- Authentication flow with:
  - `POST /api/auth/login`
  - `POST /api/auth/register`
  - `GET /api/auth/me`
- Global auth gating for the app:
  - `/login` and `/register` stay public
  - core app routes require authentication
- Shared Axios API client with bearer token support and 401 handling
- Pinia auth store with token persistence + user rehydration
- Live feed page wired to `GET /api/feed`
- Live browse page wired to `GET /api/games`
- Live game detail page wired to `GET /api/games/{gameId}`
- Live library page wired to `GET /api/library/me`
- Log page wired to `POST /api/library`
- Optional review creation from the log page via `POST /api/reviews`
- Live authenticated profile page using `GET /api/auth/me`
- AI Picks page wired to `POST /api/ai/recommendations`
- Review assistant wired to `POST /api/ai/review-assistant`
- Local fallback cover asset added for deployment safety
- Production build succeeds
- GitHub Actions CI/CD is configured for frontend deployment

### Confirmed deployed behavior
- Deployed frontend loads successfully.
- Register, login, library, and feed are working in production at the same level they were working in development.
- Frontend is successfully calling the deployed API.

### Current known limitations
- Public profile pages are not built yet
- Follow/unfollow UI is not built yet
- Review edit/delete UI is not built yet
- Feed like/comment UI is not built yet
- IGDB search/import UI is not built yet
- Semantic search UI is not built yet
- Frontend tests are not written yet
- Several backend-connected features are intentionally still limited to the current MVP/stub level

---

## Routing / App Areas

### Public routes
- `/login`
- `/register`

### Authenticated routes in current MVP
- `/`
- `/browse`
- `/game/[id]`
- `/library`
- `/log/[gameId]`
- `/profile`
- `/recommend`

---

## Current Integration Notes

### Auth
- Login/register are live against the API.
- Auth state persists through the Pinia store + token storage.
- Authenticated user rehydration runs through `/api/auth/me`.

### Feed
- Feed is live against the backend.
- Backend now includes the current user’s own activity along with followed-user activity.

### Library / logging
- Library is loaded from live backend data.
- Logging a game creates/updates the library entry.
- Review creation is optional from the log workflow.
- Ratings continue to come from the library/log model rather than a separate review rating field.

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

---

## Remaining Frontend Work

Recommended next frontend work:
1. Add `.env.example`.
2. Write frontend service/store/component tests.
3. Build public profile pages.
4. Build follow/unfollow UI.
5. Build review edit/delete UI.
6. Add feed like/comment UI.
7. Add IGDB search/import UI.
8. Add semantic search UI.
9. Do a final accessibility/mobile polish pass.

---

## Notes

- Keep using explicit imports in project code to avoid local Nuxt/VS Code auto-import issues.
- Do not overstate incomplete features just because deployment is now live.
- The main remaining work is feature completion, test coverage, and replacing current stubs with real integrations.
