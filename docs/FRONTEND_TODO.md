# FRONTEND_TODO — Backlogr.Web

Last updated: 2026-03-13

This checklist reflects the **current integrated frontend state** and what is still left before/during deployment and final polish.

> **Document location:** this file now lives in the repo root `docs/` folder.

Source requirements: `requirements_backlogr_updated.md` and `Assignment5AndFinal.md`.

---

## 0) Current local app status

### Core MVP flow now working locally
- [x] Auth pages (`/login`, `/register`)
- [x] Global auth gating for the core app
- [x] Shared Axios API client with bearer token support
- [x] Pinia auth store with token persistence
- [x] Feed page wired to live backend data
- [x] Browse page wired to local catalog search
- [x] Game detail page wired to live backend data
- [x] Library page wired to live backend data
- [x] Log page wired to save library entries
- [x] Optional review creation from the log page
- [x] Profile page wired to authenticated user data
- [x] AI Picks page wired to recommendation stub
- [x] Review assistant wired into the log page
- [x] Local fallback cover asset added for deployment safety
- [x] Production build currently succeeds locally (`npm run build`)

### Current known limitations
- [ ] Public profile pages are not built yet
- [ ] Follow/unfollow UI is not built yet
- [ ] Review edit/delete UI is not built yet
- [ ] Feed like/comment UI is not built yet
- [ ] IGDB search/import UI is not built yet
- [ ] Semantic search UI is not built yet
- [ ] Frontend tests are not written yet
- [ ] Deployment environment/config still needs to be set up

---

## 1) Frontend architecture / foundation

### Runtime config + environment
- [x] `nuxt.config.ts` reads `runtimeConfig.public.apiBase`
- [x] Shared API client created in `services/api.ts`
- [ ] Add `.env.example` for frontend:
  - `NUXT_PUBLIC_API_BASE=...`

### Typed DTOs + shared models
- [x] Auth types
- [x] Game types
- [x] Feed types
- [x] Library types
- [x] Review types
- [x] AI types
- [ ] Consider a future cleanup pass to organize DTOs into a dedicated `types/api/` or `types/dtos/` structure

### State management (Pinia)
- [x] Pinia installed/configured
- [x] `authStore` implemented
- [ ] `feedStore` *(page still loads directly from service)*
- [ ] `libraryStore`
- [ ] `profileStore`

### Route protection
- [x] Global auth middleware added
- [x] Core app routes gated behind auth
- [ ] Admin-only middleware/page flow for IGDB import UI

---

## 2) Auth + API integration foundation

### Axios client
- [x] Axios installed
- [x] Shared API client created
- [x] Bearer token request interceptor
- [x] 401 handling / redirect to login
- [ ] Extract reusable shared `getApiErrorMessage()` helper if we want less repeated page logic

### Auth service + UI
- [x] `authService.ts`
- [x] Login page
- [x] Register page
- [x] Auth store rehydration
- [x] AppTopBar login/logout state
- [x] Profile/avatar only shown when authenticated

---

## 3) Page/API wiring status

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
- [x] Browse wired to local cached game search
- [x] Query-string search support
- [x] Deployment-safe local fallback posters
- [ ] Paging
- [ ] Semantic search toggle / UI
- [ ] IGDB search surface

### AI Picks
- [x] Recommendations page wired to API stub
- [x] Refresh flow
- [x] Take/count selection
- [ ] Save-to-backlog CTA per recommendation

### Profile
- [x] Real authenticated profile page
- [x] Refresh profile flow
- [ ] Public profile route
- [ ] Social stats / follow counts
- [ ] Editable profile fields

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

## 5) IGDB + search work still needed

Locked decisions remain:
- Authenticated users can search IGDB
- Admin only can import IGDB games into the local cache

### Remaining frontend tasks
- [ ] Add IGDB search UI
- [ ] Show IGDB results in browse flow
- [ ] Add admin-only import actions
- [ ] Route imported result to local `/game/:id`
- [ ] Add semantic search UI for `GET /api/ai/semantic-search`

---

## 6) Error handling / polish pass

- [x] Basic loading/empty/error states are present on core wired pages
- [ ] Centralize shared API error formatting
- [ ] Add toast/snackbar pattern for success + failure feedback
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

### Store tests
- [ ] `authStore` token persistence + logout clears

### Component / page smoke tests
- [ ] Auth pages
- [ ] Feed render
- [ ] Log form validation
- [ ] Recommend page render

---

## 8) Deployment readiness checklist (frontend side)

### Configuration
- [x] Runtime API base is configurable
- [x] Production build succeeds locally
- [ ] Add `.env.example`
- [ ] Set `NUXT_PUBLIC_API_BASE` in Azure Static Web Apps
- [ ] Confirm deployed API URL is correct in production

### Deployment validation
- [ ] Confirm CORS works with deployed frontend domain
- [ ] Test login/register against deployed API
- [ ] Test browse/game detail/library/feed against deployed API
- [ ] Test AI recommendation + review assistant stubs against deployed API
- [ ] Verify direct route loads in production (`/game/:id`, `/profile`, `/recommend`, etc.)

### CI expectations
- [ ] `npm ci`
- [x] `npm run build`
- [ ] `npm test`

---

## 9) Suggested next order of work

### Immediate
1. Deploy `Backlogr.Api` first
2. Configure frontend `NUXT_PUBLIC_API_BASE` for the deployed API
3. Deploy `Backlogr.Web` to Azure Static Web Apps
4. Validate auth + feed + library + AI flows in production

### After deployment is stable
1. Add `.env.example`
2. Write frontend service/store tests
3. Build public profile + follow UI
4. Build review edit/delete UI
5. Add IGDB search/import UI
6. Add semantic search UI
7. Add feed like/comment UI

---

## 10) Notes

- The frontend is no longer in the “placeholder-only” stage for the main MVP path.
- The biggest remaining work is now deployment/config, social UI completion, IGDB/admin flows, and tests.
- Keep using explicit imports in project code to avoid local Nuxt/VS Code auto-import issues.
