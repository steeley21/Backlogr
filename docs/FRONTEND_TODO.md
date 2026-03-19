# FRONTEND_TODO — Backlogr.Web

Last updated: 2026-03-18

This checklist reflects the current integrated frontend state after the landing-page/admin/account-management pass, the member-profile/feed-social feature pass, the new **For You / Following** feed split, and the AI/frontend wiring pass for browse semantic search and live recommendation/review-assistant flows.

> **Document location:** this file lives in the repo root `docs/` folder.

Source requirements: `requirements_backlogr_updated.md` and `Assignment5AndFinal.md`.

---

## 0) Current frontend status

### Core app state
- [x] Frontend works locally against the API
- [x] Frontend is deployed and working in Azure Static Web Apps
- [x] Frontend GitHub Actions CI/CD is configured
- [x] Production build succeeds locally (`npm run build`)
- [x] Frontend Vitest suite passes for the current covered slices (`npm test`)

### Confirmed deployed behavior
- [x] Register works in deployed environment
- [x] Login works in deployed environment
- [x] Library flow works in deployed environment
- [x] Feed flow loads in deployed environment
- [x] Frontend can reach deployed API successfully
- [x] Browse is backed by the live merged catalog/search flow

### Latest local codebase additions ready for next deploy / smoke test
- [x] Public landing page at `/`
- [x] Authenticated feed moved to `/feed`
- [x] Admin dashboard route and role-gated navigation
- [x] Admin user list/create/edit/delete flows
- [x] Profile delete-account flow
- [x] Member profile route at `/u/[username]`
- [x] Follow / unfollow UI
- [x] Review edit / delete UI from feed cards
- [x] Feed like / comment UI
- [x] Feed source tabs: **For You** and **Following**
- [x] Route-query persistence for the selected feed tab
- [x] Feed page Vitest coverage for tab behavior
- [x] Browse page semantic-search mode + dedicated search panel
- [x] AI Picks page copy updated for live recommendations
- [x] Log page review-assistant UX polish
- [x] Browse page Vitest coverage
- [x] Recommend page Vitest coverage

### Current known limitations
- [x] Member profile route is built
- [x] Follow/unfollow UI is built
- [x] Review edit/delete UI is built
- [x] Feed like/comment UI is built
- [x] Feed split UI is built locally and covered by tests
- [x] Semantic search UI is built
- [ ] Frontend test coverage is still partial beyond the current covered slices
- [ ] Member profiles are still authenticated routes rather than signed-out public pages

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
- [x] Profile/member-social types
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
- [x] Member profile route currently stays inside the authenticated app model

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
- [x] Profile menu includes a member-profile link

---

## 3) Page/API wiring status

### Landing / navigation
- [x] Public landing page at `/`
- [x] Feed moved to `/feed`
- [x] Public vs authenticated top-bar behavior updated
- [x] Profile menu links into `/u/[username]`

### Feed
- [x] Replace mock feed data with live API call
- [x] Support **For You** and **Following** feed tabs
- [x] Preserve selected feed tab in the route query
- [x] Keep `All / Reviews / Logs` as a secondary local filter
- [x] Loading / empty / error states
- [x] Like review UI wiring
- [x] Comments UI wiring
- [x] Surface like/comment counts from expanded backend feed DTO
- [x] Review edit/delete UI from feed cards
- [ ] Paging for feed

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
- [ ] True edit-existing-review flow *(backend still lacks a dedicated “my review for this game” lookup endpoint)*

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
- [x] Semantic search toggle / UI
- [x] Dedicated browse-page search input / semantic examples
- [ ] Paging

### AI Picks
- [x] Recommendations page wired to live API
- [x] Refresh flow
- [x] Take/count selection
- [x] Recommendation copy updated away from stub language
- [ ] Save-to-backlog CTA per recommendation

### Profile / member-social
- [x] Real authenticated profile page
- [x] Refresh profile flow
- [x] Delete-account section and dialog
- [x] Member profile route `/u/[username]`
- [x] Social stats / follow counts
- [x] Follow / unfollow button
- [x] Public-library tab inside member profile route
- [x] Recent-reviews display inside member profile route
- [ ] Editable profile fields
- [ ] Follower/following detail lists

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
- [x] Edit review UI from feed cards
- [x] Delete review UI from feed cards
- [ ] Dedicated review editor route/modal beyond the feed-card flow
- [ ] Spoiler handling polish on display

### Follow / profile-social
- [x] Member profile page route (`/u/:username`)
- [x] Follow button
- [x] Unfollow button
- [x] Follower/following/review/library counts
- [x] Tie follow actions into feed expectations
- [ ] Follower/following list pages or dialogs

### Feed interactions
- [x] Like review action
- [x] Unlike review action
- [x] Add comment flow
- [x] Delete own comment flow
- [x] For You / Following feed split
- [ ] Any richer comment-thread moderation/pagination beyond the lightweight inline thread

---

## 5) Search / discovery work still needed

Current search decisions:
- Browse should not visually signal whether a game was already in the local DB
- Search should prefer local results first
- Missing local results can be imported behind the scenes through the current browse flow

### Remaining frontend tasks
- [ ] Add optional browse/result polish for larger catalogs
- [ ] Decide whether any explicit “import management” UI is needed for admin/demo purposes
- [ ] Consider richer result context for semantic results (why/score chips, badges, or helper text)

---

## 6) Error handling / polish pass

- [x] Basic loading/empty/error states are present on core wired pages
- [x] Admin success/error snackbar feedback added
- [x] Admin create/edit/delete actions have safer loading/disabled states
- [x] Feed interaction feedback/snackbar path added
- [ ] Add a broader shared toast/snackbar pattern across the rest of the app
- [ ] Tighten form validation / messaging on log + auth pages
- [ ] Accessibility pass (labels, keyboard flow, aria polish)
- [ ] Mobile UX pass after next deploy verification

---

## 7) Frontend tests (Vitest)

Requirement: “Unit tests cover core functionality for the front end and back end.”

### Service tests
- [ ] `authService`
- [ ] `gameService`
- [x] `feedService`
- [ ] `libraryService`
- [x] `reviewService`
- [x] `profileService`
- [x] `followService`
- [ ] `aiService`
- [ ] `adminService`

### Store tests
- [x] `authStore` token persistence + logout clears
- [ ] Role helper visibility behavior for admin/superadmin UI

### Component / page smoke tests
- [ ] Landing page render
- [ ] Auth pages
- [x] Feed review card interactions
- [x] Feed comment thread interactions
- [x] Feed page render + tab behavior
- [ ] Browse click/import flow
- [ ] Log form validation
- [x] Recommend page render
- [x] Browse page semantic-mode render
- [x] Member profile page (`/u/[username]`)
- [ ] Admin dashboard permissions + dialog flows
- [ ] Profile delete-account flow

---

## 8) Deployment readiness checklist (frontend side)

- [ ] Rebuild and deploy frontend with the latest AI/browse updates
- [ ] Smoke test `/`, `/feed?tab=for-you`, `/feed?tab=following`, `/browse`, `/recommend`, `/log?gameId=...`, `/u/[username]`, `/admin`, and `/profile` against production API
- [ ] Verify feed tab state persists after refresh/navigation in production
- [ ] Verify semantic search mode works in production browse
- [ ] Verify recommendation page returns live recommendations in production
- [ ] Verify review-assistant actions work from the log page in production
- [ ] Verify follow/unfollow behavior in production
- [ ] Verify feed like/comment/edit/delete behavior in production
- [ ] Verify admin visibility rules for User/Admin/SuperAdmin in production
- [ ] Verify delete-account redirect/logout flow in production

---

## Next UI / UX Enhancements

### Feed polish after the tab split
- Improve the visual separation between the feed-source tabs and the local `All / Reviews / Logs` filter
- Evaluate whether a lightweight feed snapshot or feed-type count row should become interactive
- Add paging or incremental loading once the feed gets larger
- Revisit the empty-state copy after production smoke testing with real user data

### Visual polish / motion pass
- Explore adding tasteful visual enhancements inspired by **React Bits**
- Evaluate card hover states, transitions, and lightweight motion effects that fit the Backlogr design
- Prioritize subtle improvements over flashy effects so the app still feels clean and readable
- Candidate areas:
  - feed cards
  - profile header / stat cards
  - hover states on game cards
  - page section transitions
- Verify that any new animation or interaction still performs well on mobile

---

## DevOps / CI Improvements

### Speed up Static Web App workflow
- Review the Azure Static Web Apps GitHub Actions workflow for unnecessary wait time
- Investigate why the most recent frontend deploy took ~17 minutes
- Check for opportunities to improve:
  - dependency caching
  - redundant install/build steps
  - unnecessary full rebuilds
  - workflow trigger scope
- Confirm the workflow still runs required frontend tests/build checks before deploy
