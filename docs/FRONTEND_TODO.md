# FRONTEND_TODO — Backlogr.Web

Last updated: 2026-03-14

This checklist reflects the current integrated frontend state after deployment and the new merged browse/IGDB flow.

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

### Current known limitations
- [ ] Public profile pages are not built yet
- [ ] Follow/unfollow UI is not built yet
- [ ] Review edit/delete UI is not built yet
- [ ] Feed like/comment UI is not built yet
- [ ] Semantic search UI is not built yet
- [ ] Frontend tests are not written yet
- [ ] Dedicated admin/import management UI is not built

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
- [ ] Browse click/import flow
- [ ] Log form validation
- [ ] Recommend page render

---

## 8) Deployment readiness checklist (frontend side)

### Configuration
- [x] Runtime API base is configurable
- [x] `.env.example` exists
- [x] Production build succeeds locally
- [x] Set `NUXT_PUBLIC_API_BASE` in Azure Static Web Apps
- [x] Confirm deployed API URL is correct in production

### Deployment validation
- [x] Confirm CORS works with deployed frontend domain
- [x] Test login/register against deployed API
- [x] Test browse/game detail/library/feed against deployed API at the current MVP level
- [ ] Test AI recommendation + review assistant stubs against deployed API
- [ ] Verify direct route loads in production (`/game/:id`, `/profile`, `/recommend`, etc.)

### CI expectations
- [ ] `npm ci`
- [x] `npm run build`
- [ ] `npm test`

---

## 9) Suggested next order of work

### Immediate
1. Write frontend service/store tests.
2. Build public profile + follow UI.
3. Build review edit/delete UI.
4. Add semantic search UI.
5. Add feed like/comment UI.
6. Tighten shared API error handling and feedback.

### After that
1. Add direct-route production verification.
2. Do a final accessibility/mobile polish pass.
3. Decide whether any admin/demo-only catalog tooling is still worth adding.

---

## 10) Notes

- The frontend is no longer in the placeholder-only stage.
- Deployment is working, but deployment success does not change incomplete feature status.
- Keep using explicit imports in project code to avoid local Nuxt/VS Code auto-import issues.
