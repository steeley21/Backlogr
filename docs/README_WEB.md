# Backlogr.Web — Frontend

Nuxt 3 + Vuetify 3 + TypeScript frontend for **Backlogr** — a Letterboxd-style social web app for video games.

This frontend is now **wired to the local Backlogr API for the core MVP loop** instead of being only a UI shell. The current local app supports authenticated browsing, viewing game details, logging games into a personal library, creating reviews, viewing a personalized feed, checking a profile, and using the current AI stub features.

> **Document location:** this file now lives in the repo root `docs/` folder.

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

The frontend is working locally against the current `Backlogr.Api` MVP surface.

### Implemented locally
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
- Live game details page wired to `GET /api/games/{gameId}`
- Live library page wired to `GET /api/library/me`
- Live log flow wired to `POST /api/library`
- Review creation from the log page wired to `POST /api/reviews`
- Profile page wired to the authenticated user state / `/api/auth/me`
- AI Picks page wired to `POST /api/ai/recommendations`
- Review assistant actions on the log page wired to `POST /api/ai/review-assistant`
- Local fallback cover asset for posters and recommendations
- Top bar log button improved so it routes through a real game-first flow

### Still not implemented in the frontend
- Public profile pages / follow UI
- Review edit/delete UI
- Feed like/comment UI wiring
- IGDB search/import UI
- Semantic search UI
- Frontend tests
- Azure Static Web Apps deployment/config wiring

---

## UI Mockups (Source of Truth)

Design artifacts live in the repo:

- `docs/ui/`
  - `flow/` (app flow diagrams)
  - `screens/` (PNG mockups)
  - `README_figma.md` (notes)

---

## What’s Implemented

### App shell
- Global layout: `layouts/default.vue`
- Top navigation: `components/app/AppTopBar.vue`
- Tokens + base styling:
  - `assets/styles/tokens.css`
  - `assets/styles/base.css`
- Local fallback poster asset:
  - `public/images/fallback-game-cover.svg`

### Auth + app state
- `stores/auth.ts`
- `middleware/auth.global.ts`
- `services/api.ts`
- `services/authService.ts`
- Pages:
  - `/login`
  - `/register`

### Pages (routes)
- **Feed:** `/` → live feed data
- **Browse:** `/browse` → live local catalog search
- **Library:** `/library` → live user library
- **AI Picks:** `/recommend` → live AI stub recommendations
- **Log a Game:** `/log` → live log + optional review creation
- **Profile:** `/profile` → live authenticated user info
- **Game Detail:** `/game/:id` → live game details

### Reusable components
- Feed cards:
  - `components/feed/FeedReviewCard.vue`
  - `components/feed/FeedLogCard.vue`
  - `components/feed/AiCalloutCard.vue`
- Game UI:
  - `components/game/GamePosterCard.vue`
- Layout helpers:
  - `components/layout/SectionHeader.vue`
- Shared:
  - `components/shared/BacklogrLogo.vue`
  - `components/shared/StarRating.vue`

> **Convention:** Use explicit imports in pages/components. Do not rely on Nuxt auto-imports for project code because the current environment has been inconsistent with them.

---

## Runtime Config / Environment

The frontend reads the API base URL from runtime config:

- `runtimeConfig.public.apiBase`

Current config in `nuxt.config.ts`:

```ts
runtimeConfig: {
  public: {
    apiBase: process.env.NUXT_PUBLIC_API_BASE || 'http://localhost:5042'
  }
}
```

### Recommended local `.env`

```env
NUXT_PUBLIC_API_BASE=http://localhost:5042
```

For Azure deployment, this value should be set in the Static Web App environment configuration and must point to the deployed API URL.

---

## Vuetify Theme Setup

Vuetify is configured via **vuetify-nuxt-module** in `nuxt.config.ts`.

Current default theme:
- dark theme
- primary color = Backlogr purple
- background/surface aligned to the current mockup styling

If theme colors change, update both:
- `assets/styles/tokens.css`
- `nuxt.config.ts` Vuetify theme colors

---

## Local Development

### Prereqs
- **Node.js** (recommended: current LTS)
- **npm**
- local `Backlogr.Api` running

### Install
```powershell
cd Backlogr.Web
npm install
```

### Run dev server
```powershell
npm run dev
```

Nuxt runs at:
- http://localhost:3000

### Required local backend
The frontend expects the API to be available at the configured `NUXT_PUBLIC_API_BASE` and currently assumes the authenticated backend surface is available.

---

## Build & Preview

### Production build
```powershell
npm run build
```

### Preview production build locally
```powershell
npm run preview
```

The production build is currently succeeding locally.

---

## Linting / Tests

### Lint
```powershell
npm run lint
```

### Tests
```powershell
npm test
```

Vitest is installed, but core frontend tests still need to be written for the current service/store/UI flows.

---

## Project Structure (high level)

```text
Backlogr.Web/
├── assets/styles/           # tokens + base styles
├── components/
│   ├── app/                 # app shell (top bar)
│   ├── feed/                # feed cards + callouts
│   ├── game/                # game poster cards, etc.
│   ├── layout/              # section headers, layout helpers
│   └── shared/              # shared UI pieces (logo, stars)
├── layouts/                 # Nuxt layouts
├── middleware/              # auth route protection
├── pages/                   # route-based pages
├── plugins/                 # Vuetify plugin
├── public/images/           # local static assets
├── services/                # API service layer
├── stores/                  # Pinia stores
├── types/                   # shared TypeScript types
└── nuxt.config.ts
```

---

## Current MVP Flow

A working local path through the app now looks like this:

1. Register or log in
2. Browse local catalog games
3. Open a game detail page
4. Log the game into the user library
5. Optionally create a review from the log page
6. See resulting activity in:
   - library
   - feed
   - profile
7. Use current AI stubs for:
   - recommendations
   - review drafting / rewriting

---

## Known Limitations / Next Steps

### Frontend work still needed
- Follow/unfollow UI
- Public user profiles
- Feed like/comment UI
- Dedicated review editing/deleting flow
- Semantic search UI
- IGDB search/import UI
- Toast/snackbar pattern for success/error feedback
- More polished validation and accessibility passes
- Vitest coverage for services, stores, and critical flows

### Deployment readiness work
- Add `.env.example`
- Configure Azure Static Web Apps environment values
- Point `NUXT_PUBLIC_API_BASE` to deployed API
- Validate CORS against deployed domains
- Finalize README / docs deployment instructions

---

## Notes

- The app is intentionally auth-gated right now to reduce complexity during deployment setup.
- The current AI and IGDB integrations are still stub-backed on the API side.
- The frontend is now beyond the placeholder/mock-data phase for the main MVP path, but several social/admin/search features are still pending.
