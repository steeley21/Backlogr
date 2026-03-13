# FRONTEND_TODO — Backlogr.Web

Last updated: 2026-03-13

This checklist covers what’s left to finish on the **frontend** and what’s needed to **integrate the API** for Backlogr’s MVP:
- Social feed (logs + reviews), follows, likes, comments
- Library logging (status + rating + metadata)
- Reviews (create/edit/delete + spoiler flag)
- AI recommendations + review assistant
- Semantic/vector search
- IGDB search + admin-only import
- Tests + deploy readiness

Source requirements: `requirements_backlogr_updated.md` and `Assignment5AndFinal.md`.

---

## 0) Current UI status (already in place)

- [x] App shell / top nav (Feed, Browse, Library, AI Picks, Log, Profile)
- [x] Pages scaffolded:
  - [x] Feed (`/`)
  - [x] Browse (`/browse`)
  - [x] Library (`/library`)
  - [x] AI Picks (`/recommend`)
  - [x] Log (`/log`)
  - [x] Profile (`/profile`)
  - [x] Game details (`/game/:id`)
- [x] Shared components (poster cards, section header, star rating, feed cards)
- [x] Tokens + base styles
- [x] Vuetify theme configured (primary = Backlogr purple)

---

## 1) Frontend architecture tasks (do before heavy API wiring)

### 1.1 Runtime config + environment
- [ ] Add `.env.example` for frontend (no secrets):
  - `NUXT_PUBLIC_API_BASE=...`
- [ ] Ensure `nuxt.config.ts` reads:
  - `runtimeConfig.public.apiBase`
- [ ] Add a single place to build URLs:
  - `services/http.ts` uses `useRuntimeConfig().public.apiBase`

### 1.2 Typed DTOs + shared models
- [ ] Create `/types/dtos/` folder (or `/types/api/`) for request/response types:
  - `auth.ts`, `games.ts`, `feed.ts`, `library.ts`, `reviews.ts`, `ai.ts`, `igdb.ts`
- [ ] Align types with backend DTOs and naming rules (TableNameId, UTC timestamps)

### 1.3 State management (Pinia)
- [ ] Add Pinia if not already installed/configured
- [ ] Stores:
  - [ ] `authStore` (token + user + role)
  - [ ] `feedStore` (feed list + paging)
  - [ ] `libraryStore` (my logs)
  - [ ] `profileStore` (me + public profile)
- [ ] Persist token to localStorage (safe window checks)

### 1.4 Route protection (middleware)
- [ ] Add `/middleware/auth.ts` (requires logged-in)
- [ ] Add `/middleware/admin.ts` (requires Admin role)
- [ ] Gate routes:
  - [ ] `/log`, `/library`, `/profile`, `/recommend` → auth
  - [ ] IGDB import route/page → admin only

---

## 2) API integration foundation (Axios client + auth)

### 2.1 Axios client
- [ ] Install Axios (if not already)
- [ ] `services/http.ts`:
  - [ ] Create axios instance with `baseURL`
  - [ ] Request interceptor: attach `Authorization: Bearer <token>` when present
  - [ ] Response interceptor: handle 401 → logout + redirect to login
  - [ ] Standard error shape helper: `getApiErrorMessage(err)`

### 2.2 Auth service + UI
Backend endpoints (draft): `POST /api/auth/register`, `POST /api/auth/login`, `GET /api/auth/me`.

- [ ] Create pages:
  - [ ] `/login` page
  - [ ] `/register` page
- [ ] `services/authService.ts`:
  - [ ] `register(dto)`
  - [ ] `login(dto)`
  - [ ] `me()`
- [ ] `authStore`:
  - [ ] `init()` on app load (rehydrate token + call `/me`)
  - [ ] `logout()` clears token + user
- [ ] UI:
  - [ ] Add “Login/Logout” state in AppTopBar
  - [ ] Show user avatar only when authenticated

---

## 3) Wire each page to MVP endpoints (placeholder → real)

> Priority order: Feed → Library/Log → Game → Browse/IGDB → Reviews → AI.

### 3.1 Feed (`GET /api/feed`)
- [ ] Replace mock `feedItems` with API call
- [ ] Add paging (cursor or page/size)
- [ ] Loading + empty + error states (skeletons)
- [ ] Interactions:
  - [ ] Like review: `POST /api/reviews/{reviewId}/like` and `DELETE .../like`
  - [ ] Comments list + create:
    - [ ] `POST /api/reviews/{reviewId}/comments`
    - [ ] Render top N comments (expand/collapse)

### 3.2 Library (`GET /api/library/me`, `POST /api/library`, `DELETE /api/library/{gameId}`)
- [ ] Replace library mock data with API
- [ ] Filtering/sorting UI (status, rating, updated)
- [ ] Update constraints:
  - [ ] 0.5 rating steps
  - [ ] rating is stored on **GameLog** (source of truth), not Review

### 3.3 Log page (`POST /api/library`)
- [ ] Convert to “search game then log” flow:
  - [ ] Choose game (from local cache search or IGDB search)
  - [ ] Status (Playing/Played/Backlog/Wishlist/Dropped)
  - [ ] Rating (0.5 increments), platform, hours, notes, dates
- [ ] If user logs a game that already exists, treat POST as upsert (backend decides)

### 3.4 Game detail (`GET /api/games/{gameId}`)
- [ ] Fetch game info from API
- [ ] Add tabs:
  - [ ] About (summary, genres, platforms)
  - [ ] Reviews (community reviews for this game)
  - [ ] Activity (recent logs)
- [ ] Add “My log” panel (current user’s status/rating if exists)

### 3.5 Browse + Search (`GET /api/games`, `GET /api/ai/semantic-search?query=...`)
- [ ] Browse should search local cache (`/api/games`) with paging
- [ ] Add “Semantic search” mode:
  - [ ] Toggle or auto-detect when query is long/natural language
  - [ ] Call `/api/ai/semantic-search?query=...` for vector results

### 3.6 IGDB search/import
Locked decisions:
- Authenticated users can search IGDB
- Admin only can import IGDB games into local cache

Backend endpoints:
- `GET /api/igdb/search?query=...`
- `POST /api/igdb/import/{igdbId}` (admin)

Frontend tasks:
- [ ] Add an “IGDB Search” UI (can live inside Browse)
- [ ] Show IGDB results (cover/title/year)
- [ ] If Admin:
  - [ ] Show “Import” button per result
  - [ ] After import, route to `/game/:id`

### 3.7 Reviews (CRUD)
Backend endpoints:
- `POST /api/reviews`
- `PUT /api/reviews/{reviewId}`
- `DELETE /api/reviews/{reviewId}`

Frontend tasks:
- [ ] Add review editor route:
  - [ ] Option A: `/game/:id/review`
  - [ ] Option B: modal from game page
- [ ] Include spoiler toggle
- [ ] Remember: Review does **not** store rating; rating comes from GameLog

### 3.8 Social (follow/unfollow)
Backend endpoints:
- `POST /api/follows/{userId}`
- `DELETE /api/follows/{userId}`

Frontend tasks:
- [ ] Public profile page route: `/u/:username` (or `/user/:id`)
- [ ] Follow button + follower counts
- [ ] Feed should reflect follows

---

## 4) AI features (core app functionality)

### 4.1 Recommendations (`POST /api/ai/recommendations`)
- [ ] Hook AI Picks page to API
- [ ] Inputs:
  - [ ] user profile + logs + ratings (+ optional review text)
- [ ] Output UI:
  - [ ] poster list + “why you might like it”
  - [ ] “save to backlog” CTA per recommendation

### 4.2 Review writing assistant (`POST /api/ai/review-assistant`)
- [ ] Add assistant panel in review editor:
  - [ ] Draft from bullets
  - [ ] Rewrite tone/clarity
  - [ ] Shorten/expand
  - [ ] Spoiler-safe summary
- [ ] Must never auto-post (only drafts)

---

## 5) Error handling + UX baseline (fast, but important)

- [ ] Standard loading state components:
  - [ ] Skeleton cards for feed
  - [ ] Skeleton posters for browse/trending
- [ ] Empty states (no feed items, no library items, no search results)
- [ ] Toast/snackbar pattern for API errors + success messages
- [ ] Form validation (required fields, rating step 0.5)
- [ ] Accessibility pass (labels, keyboard navigation)

---

## 6) Frontend tests (Vitest) — minimum set

Requirement: “Unit tests cover core functionality for the front end and back end.”

### 6.1 Service tests (axios mocked)
- [ ] `authService` (login/register/me)
- [ ] `feedService` (get feed)
- [ ] `libraryService` (get/upsert/delete)
- [ ] `reviewsService` (create/edit/delete/like/comment)
- [ ] `aiService` (recommendations, review assistant, semantic search)

### 6.2 Store tests (Pinia)
- [ ] `authStore` token persistence + logout clears
- [ ] `feedStore` loads + paging merge
- [ ] `libraryStore` upsert updates local cache

### 6.3 Component smoke tests (only key flows)
- [ ] Feed renders list items from mocked store/service
- [ ] Log form validation
- [ ] Review editor + spoiler toggle

---

## 7) Deployment readiness checklist (frontend side)

- [ ] Azure Static Web Apps config:
  - [ ] Ensure API base URL uses runtime config (no hardcoded localhost)
  - [ ] Environment variable set in SWA: `NUXT_PUBLIC_API_BASE`
- [ ] CORS and auth flows work with deployed API domain
- [ ] Ensure build succeeds in CI:
  - [ ] `npm ci`
  - [ ] `npm run build`
  - [ ] `npm test` (once Vitest added)
- [ ] Remove placeholder data once endpoints are live (or keep a dev-only mock mode)

---

## 8) API integration “contract” checklist (what frontend needs from backend)

Before fully wiring the UI, confirm backend provides:

- [ ] Auth:
  - [ ] Register/login returns token + user/roles
  - [ ] `/me` returns current user profile + roles
- [ ] Feed:
  - [ ] Unified feed DTO supports both log + review items
  - [ ] Like/comment counts and whether current user liked
- [ ] Library:
  - [ ] Upsert log by game (unique UserId+GameId)
  - [ ] Rating increments (0.5) enforced consistently
- [ ] Reviews:
  - [ ] One review per user per game (unique)
  - [ ] Spoiler flag field
- [ ] IGDB:
  - [ ] Search endpoint for authenticated users
  - [ ] Import endpoint admin-only and returns local GameId
- [ ] AI:
  - [ ] Recommendations endpoint returns list of games + explanations
  - [ ] Semantic search returns ranked list + scores
  - [ ] Review assistant returns draft text only

---

## Suggested order of work (fastest path to “demoable”)

1. Auth + token wiring + middleware
2. Feed → real data + like/comment actions
3. Library + Log upsert
4. Game detail fetch + “My log” panel
5. Browse local games + IGDB search/import (admin)
6. AI recommendations + semantic search
7. Review editor + assistant
8. Tests + polish pass
