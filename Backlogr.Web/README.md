# Backlogr.Web — Frontend

Nuxt 3 + Vuetify 3 + TypeScript frontend for **Backlogr** — a Letterboxd-style social web app for video games.

This repo currently includes a **working UI shell + page scaffolding** based on our Figma mockups, with placeholder data (API wiring comes next).

---

## Tech Stack

- **Nuxt 3**
- **Vue 3**
- **Vuetify 3**
- **TypeScript**
- **Material Design Icons (MDI)**
- **ESLint**

---

## UI Mockups (Source of Truth)

Design artifacts live in the repo:

- `docs/ui/`
  - `flow/` (app flow diagrams)
  - `screens/` (PNG mockups)
  - `README_figma.md` (notes)

---

## What’s Implemented So Far

### App shell
- Global layout: `layouts/default.vue`
- Top navigation: `components/app/AppTopBar.vue`
- Tokens + base styling:
  - `assets/styles/tokens.css` (design tokens)
  - `assets/styles/base.css` (global font + base styles)

### Pages (routes)
- **Feed:** `/` → `pages/index.vue`
- **Browse:** `/browse` → `pages/browse.vue`
- **Library:** `/library` → `pages/library.vue`
- **AI Picks:** `/recommend` → `pages/recommend.vue`
- **Log a Game:** `/log` → `pages/log.vue`
- **Profile:** `/profile` → `pages/profile.vue`
- **Game Detail:** `/game/:id` → `pages/game/[id].vue`

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

> **Convention:** We explicitly import components in pages/components (no reliance on auto-import) to avoid casing/import edge cases.

---

## Vuetify Theme Setup

Vuetify theme is configured via **vuetify-nuxt-module** in `nuxt.config.ts` (primary color is Backlogr purple).

If you change theme colors, update both:
- `assets/styles/tokens.css` (design tokens)
- `nuxt.config.ts` Vuetify `theme.colors` (component theme)

---

## Local Development

### Prereqs
- **Node.js** (recommended: current LTS)
- **npm**

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

---

## Linting

```powershell
npm run lint
```

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
├── layouts/                 # Nuxt layouts (default shell)
├── pages/                   # route-based pages
├── plugins/                 # client plugins (if used)
├── services/                # API service layer (to be built)
├── types/                   # shared TypeScript types
└── docs/ui/                 # UI mockups + flow diagrams
```

---

## Next Steps (Planned)

- Connect pages to the API via `services/` (Axios)
- Add auth flow + route protection (middleware)
- Replace placeholder data with real models (games, logs, reviews, follows)
- Add tests (Vitest) once services are in place
