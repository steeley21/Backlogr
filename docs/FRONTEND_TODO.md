# FRONTEND_TODO — Backlogr.Web

Last updated: 2026-03-14

This checklist reflects the current integrated frontend state after the landing-page/admin/account-management pass.

> **Document location:** this file lives in the repo root `docs/` folder.

Source requirements: `requirements_backlogr_updated.md` and `Assignment5AndFinal.md`.

---

## 0) Current frontend status

### Core app state
- [x] Frontend works locally against the API
- [x] Frontend is deployed and working in Azure Static Web Apps
- [x] Frontend GitHub Actions CI/CD is configured
- [x] Production build succeeds locally (`npm run build`)

### Confirmed deployed behavior
- [x] Register works in deployed environment
- [x] Login works in deployed environment
- [x] Library flow works in deployed environment
- [x] Feed flow loads in deployed environment
- [x] Frontend can reach deployed API successfully
- [x] Browse is backed by the live merged catalog/search flow

### Recent codebase additions ready for deploy/smoke test
- [x] Public landing page at `/`
- [x] Authenticated feed moved to `/feed`
- [x] Admin dashboard route and role-gated navigation
- [x] Admin user list/create/edit/delete flows
- [x] Profile delete-account flow

### Current known limitations
- [ ] Public profile pages are not built yet
- [ ] Follow/unfollow UI is not built yet
- [ ] Review edit/delete UI is not built yet
- [ ] Feed like/comment UI is not built yet
- [ ] Semantic search UI is not built yet
- [ ] Frontend tests are not written yet

---

## 1) Frontend architecture / foundation

### Runtime config + environment
- [x] `nuxt.config.ts` reads `runtimeConfig.public.apiBase`
- [x] Shared API client created in `services/api.ts`
- [x] `.env.example` added for frontend runtime config

### Typed DTOs + shared models
- [x] Auth types
- [x] Game types
- [x] Browse-result types for merged local/IGDB search
- [x] Feed types
- [x] Library types
- [x] Review types
- [x] AI types
- [x] Admin/account-management types
- [ ] Consider a future cleanup pass to organize DTOs into a dedicated `types/api/` or `types/dtos/` structure

### State management (Pinia)
- [x] Pinia installed/configured
- [x] `authStore` implemented
- [ ] `feedStore` *(page still loads directly from service)*
- [ ] `libraryStore`
- [ ] `profileStore`

### Route protection
- [x] Global auth middleware added
- [x] Public-route allowance for `/`, `/login`, and `/register`
- [x] Core app routes gated behind auth
- [x] Admin-only gate for `/admin`

---

## 2) Auth + API integration foundation

### Axios client
- [x] Axios installed
- [x] Shared API client created
- [x] Bearer token request interceptor
- [x] 401 handling / redirect to login
- [x] Shared `getApiErrorMessage()` helper added

### Auth service + UI
- [x] `authService.ts`
- [x] Login page
- [x] Register page
- [x] Auth store rehydration
- [x] AppTopBar login/logout state
- [x] Profile/avatar only shown when authenticated
- [x] Self-delete account flow wired to the API

---

## 3) Page/API wiring status

### Landing / navigation
- [x] Public landing page at `/`
- [x] Feed moved to `/feed`
- [x] Public vs authenticated top-bar behavior updated

### Feed
- [x] Replace mock feed data with live API call
- [x] Show current user activity + followed-user activity (backend now supports this)
- [x] Loading / empty / error states
- [ ] Paging for feed
- [ ] Like review UI wiring
- [ ] Comments UI wiring
- [ ] Surface like/comment counts if/when backend feed DTO expands

### Library
- [x] Replace mock library data with API
- [x] Status-tab filtering
- [x] Loading / empty / error states
- [ ] Sorting UI (updated, rating, etc.)
- [ ] Inline remove-from-library action

### Log page
- [x] Real log save via `POST /api/library`
- [x] Entry launched from real `gameId`
- [x] Optional review creation on save
- [x] Review assistant tools integrated
- [ ] Search/select-game flow from inside the page itself
- [ ] True edit-existing-review flow *(backend currently lacks “my review for this game” lookup)*

### Game detail
- [x] Real game detail fetch
- [x] Real metadata rendering
- [x] Real route navigation from browse/library/recommendations
- [ ] Reviews tab wiring
- [ ] Activity tab wiring
- [ ] “My log” panel

### Browse / search
- [x] Browse wired to merged local + IGDB-backed search
- [x] Query-string search support
- [x] Deployment-safe local fallback posters
- [x] Automatic import-on-click flow for non-local results
- [x] Route imported result to local `/game/:id`
- [ ] Paging
- [ ] Semantic search toggle / UI

### AI Picks
- [x] Recommendations page wired to API stub
- [x] Refresh flow
- [x] Take/count selection
- [ ] Save-to-backlog CTA per recommendation

### Profile
- [x] Real authenticated profile page
- [x] Refresh profile flow
- [x] Delete-account section and dialog
- [ ] Public profile route
- [ ] Social stats / follow counts
- [ ] Editable profile fields

### Admin
- [x] Admin dashboard route
- [x] User list wired to API
- [x] Create-user dialog wired to API
- [x] Search + role filter
- [x] `SuperAdmin` role edit dialog
- [x] Delete-user dialog
- [ ] User-list pagination
- [ ] Optional richer audit/help text if needed for final demo

---

## 4) Social features still needed

### Reviews
- [x] Create review from log page
- [ ] Dedicated review editor route/modal
- [ ] Edit review UI
- [ ] Delete review UI
- [ ] Spoiler handling polish on display

### Follow / profile-social
- [ ] Public profile page route (`/u/:username` or similar)
- [ ] Follow button
- [ ] Unfollow button
- [ ] Follower/following counts
- [ ] Tie follow actions into feed expectations

### Feed interactions
- [ ] Like review action
- [ ] Unlike review action
- [ ] Add comment flow
- [ ] Delete own comment flow

---

## 5) Search / discovery work still needed

Current search decisions:
- Browse should not visually signal whether a game was already in the local DB
- Search should prefer local results first
- Missing local results can be imported behind the scenes through the current browse flow

### Remaining frontend tasks
- [ ] Add semantic search UI for `GET /api/ai/semantic-search`
- [ ] Add optional browse/result polish for larger catalogs
- [ ] Decide whether any explicit “import management” UI is needed for admin/demo purposes

---

## 6) Error handling / polish pass

- [x] Basic loading/empty/error states are present on core wired pages
- [x] Admin success/error snackbar feedback added
- [x] Admin create/edit/delete actions have safer loading/disabled states
- [ ] Add a broader shared toast/snackbar pattern across the rest of the app
- [ ] Tighten form validation / messaging on log + auth pages
- [ ] Accessibility pass (labels, keyboard flow, aria polish)
- [ ] Mobile UX pass after deployment verification

---

## 7) Frontend tests (Vitest)

Requirement: “Unit tests cover core functionality for the front end and back end.”

### Service tests
- [ ] `authService`
- [ ] `gameService`
- [ ] `feedService`
- [ ] `libraryService`
- [ ] `reviewService`
- [ ] `aiService`
- [ ] `adminService`

### Store tests
- [ ] `authStore` token persistence + logout clears
- [ ] Role helper visibility behavior for admin/superadmin UI

### Component / page smoke tests
- [ ] Landing page render
- [ ] Auth pages
- [ ] Feed render
- [ ] Browse click/import flow
- [ ] Log form validation
- [ ] Recommend page render
- [ ] Admin dashboard permissions + dialog flows
- [ ] Profile delete-account flow

---

## 8) Deployment readiness checklist (frontend side)

- [ ] Rebuild and deploy frontend with the latest landing/admin/account-management changes
- [ ] Smoke test `/`, `/feed`, `/admin`, and `/profile` against production API
- [ ] Verify admin visibility rules for User/Admin/SuperAdmin in production
- [ ] Verify delete-account redirect/logout flow in production
