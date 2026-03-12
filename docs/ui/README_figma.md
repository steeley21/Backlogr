# Backlogr

A social platform for video games, inspired by Letterboxd's cinematic design language. Track your gaming library, write reviews, discover new titles, and follow what friends are playing.

This document describes the UI mockup reference. The actual app is implemented with Nuxt 3 + Vuetify 3 + TypeScript.

**Figma Link:**
https://www.figma.com/make/ArdlXONxOk0TY943fjTpG6/Backlogr-social-site?fullscreen=1&t=2du3dvGnDn8Qx8ED-1


Built with **React 18**, **React Router 7**, **Tailwind CSS v4**, and **Lucide icons**.

---

## Table of Contents

- [Screens](#screens)
- [Shared Components](#shared-components)
- [Data Layer](#data-layer)
- [Routing](#routing)
- [Design Tokens](#design-tokens)
  - [Colors](#colors)
  - [Typography](#typography)
  - [Spacing Scale](#spacing-scale)
  - [Border Radii](#border-radii)
  - [Shadows](#shadows)
- [Project Structure](#project-structure)
- [Tech Stack](#tech-stack)
- [Next Steps](#next-steps)

---

## Screens

### 1. Feed (`/`)

The landing page and social hub. Displays a hero banner for a featured game, a filterable activity feed (All / Reviews / Logs), a trending games sidebar, an AI recommendations call-to-action, and a popular members list.

**File:** `/src/app/pages/home.tsx`

### 2. Browse (`/browse`)

Full game catalog with search, genre/platform filters, sorting (Popular / Rating / Newest / Title), and grid/list view toggle. Accepts a `?q=` query param from the navbar search bar.

**File:** `/src/app/pages/browse.tsx`

### 3. Game Detail (`/game/:gameId`)

Detailed game page with cover art backdrop, metadata (release date, genres, platforms), aggregate rating, status picker (Playing/Played/Backlog/Wishlist/Dropped), personal star rating, and tabbed content for Reviews and Stats/distribution.

**File:** `/src/app/pages/game-detail.tsx`

### 4. Library (`/library`)

Personal game collection with status tab filters and grid/list views. Each game card shows its status badge and personal rating. Links to game detail pages.

**File:** `/src/app/pages/library.tsx`

### 5. Log / Review (`/log`)

Form to log a game with status, star rating (half-star precision), platform, hours played, start/finish dates, and a freeform review with AI writing assistant and spoiler toggle. Accepts `?game=` param for pre-selection.

**File:** `/src/app/pages/log-game.tsx`

### 6. Profile (`/profile`, `/user/:userId`)

User profile with avatar, bio, stats (games logged, reviews, followers), favorite games row, and tabbed content for Games / Reviews / Activity.

**File:** `/src/app/pages/profile.tsx`

### 7. AI Picks (`/recommend`)

AI-powered recommendation engine. Generates personalized game suggestions with match-score percentages, reasoning text, and thumbs up/down feedback. Includes a refresh button for new picks.

**File:** `/src/app/pages/recommend.tsx`

### 8. Site Flow Diagram (`/flow`)

Interactive SVG-based navigation map of the entire app. Nodes represent pages, edges show navigation flows (primary, secondary, global). Click to inspect, drag to pan, zoom controls, and exportable as PNG or SVG.

**File:** `/src/app/pages/flow-diagram.tsx`

### 9. 404 Not Found (`/*`)

Fallback page for unmatched routes.

**File:** `/src/app/pages/not-found.tsx`

---

## Shared Components

All reusable components live in `/src/app/components/`.

| Component | File | Description |
|---|---|---|
| **Layout** | `layout.tsx` | Root shell with `<Navbar />`, `<Outlet />` for page content, and a footer with copyright and links. |
| **Navbar** | `navbar.tsx` | Sticky top nav with logo, search bar, page links (Feed, Browse, Library, AI Picks), `+ Log` button, notification bell, and profile avatar. Responsive with a mobile hamburger menu. |
| **GameCard** | `game-card.tsx` | Game cover thumbnail with hover border effect and optional star rating. Three sizes: `sm` (100px), `md` (140px), `lg` (180px). Links to `/game/:id`. |
| **StarRating** | `star-rating.tsx` | Renders 1-5 stars with half-star precision. Supports static display and interactive mode with `onChange` callback. Uses Lucide `Star` / `StarHalf` icons. |
| **ReviewCard** | `review-card.tsx` | Review display with user avatar, star rating, review text, spoiler reveal toggle, like button with count, and comment count. Links to user profile and game page. |
| **FeedItem** | `feed-item.tsx` | Activity feed entry. Renders either a review (via `ReviewCard`) or a status log with time-ago formatting. Color-coded status labels. |
| **StatusBadge** | `status-badge.tsx` | Pill-shaped badge for game status. Color-coded: Playing (purple), Played (blue), Backlog (orange), Wishlist (violet), Dropped (red). Two sizes: `sm` / `md`. |

---

## Data Layer

**File:** `/src/app/data/mock-data.ts`

All data is client-side mock data. No API calls are made in the current prototype.

### Types

```ts
GameStatus = 'Playing' | 'Played' | 'Backlog' | 'Wishlist' | 'Dropped'

Game       { id, igdbId, title, summary, coverUrl, releaseDate, genres[], platforms[], rating, ratingCount, reviewCount }
User       { id, username, displayName, avatarUrl, bio, followersCount, followingCount, gamesLogged, reviewsWritten }
GameLog    { id, userId, gameId, status, rating?, platform, hoursPlayed?, startedAt?, finishedAt?, notes, updatedAt }
Review     { id, userId, gameId, rating, text, hasSpoilers, likesCount, commentsCount, createdAt, liked }
FeedItem   { id, type: 'log'|'review', userId, gameId, timestamp, review?, log? }
```

### Dataset Size

- **12 games** with Unsplash placeholder covers
- **4 users** (including `CURRENT_USER`)
- **Reviews and game logs** linking users to games
- **Feed items** for the activity feed

### Helper Functions

- `getGame(id)` / `getUser(id)` - Lookup by ID
- `getGameReviews(gameId)` - All reviews for a game
- `getUserReviews(userId)` - All reviews by a user
- `getUserLogs(userId)` - All logs by a user

---

## Routing

React Router v7 in Data mode. All routes are children of the `Layout` component.

```
/                   Feed (Home)
/browse             Browse catalog
/game/:gameId       Game detail
/library            Personal library
/log                Log / review a game
/profile            Current user profile
/user/:userId       Other user profile
/recommend          AI recommendations
/flow               Site flow diagram
*                   404 Not Found
```

**File:** `/src/app/routes.ts`

---

## Design Tokens

All design tokens are defined as CSS custom properties in `/src/styles/theme.css` and mapped to Tailwind v4's `@theme inline` directive.

### Colors

The palette follows a dark cinematic aesthetic with neon purple accents.

| Token | Hex | Usage |
|---|---|---|
| `--background` | `#14181c` | Page background |
| `--foreground` | `#d8d8d8` | Primary text |
| `--card` | `#1c2228` | Card / panel background |
| `--card-foreground` | `#d8d8d8` | Card text |
| `--primary` | `#a855f7` | Accent buttons, links, active states, logo |
| `--primary-foreground` | `#ffffff` | Text on primary backgrounds |
| `--secondary` | `#2c3440` | Secondary backgrounds (inputs, tags) |
| `--secondary-foreground` | `#d8d8d8` | Text on secondary backgrounds |
| `--muted` | `#2c3440` | Muted backgrounds |
| `--muted-foreground` | `#99aabb` | Subdued text, labels, timestamps |
| `--accent` | `#a855f7` | Same as primary (unified accent) |
| `--accent-foreground` | `#ffffff` | Text on accent backgrounds |
| `--destructive` | `#e05050` | Error states, delete actions |
| `--destructive-foreground` | `#ffffff` | Text on destructive backgrounds |
| `--border` | `rgba(255,255,255,0.08)` | Subtle dividers and borders |
| `--ring` | `#a855f7` | Focus ring color |
| `--sidebar` | `#0d1117` | Sidebar / deep background |
| `--input-background` | `#2c3440` | Form input fill |

#### Status Badge Colors

| Status | Color | Hex |
|---|---|---|
| Playing | Purple | `#a855f7` |
| Played | Blue | `#40bcf4` |
| Backlog | Orange | `#ee7000` |
| Wishlist | Violet | `#a700e0` |
| Dropped | Red | `#e05050` |

#### Extended Palette (used in components)

| Context | Hex | Where |
|---|---|---|
| Navbar hover | `#a855f7` | Active link border, logo |
| Muted text | `#99aabb` | Descriptions, timestamps |
| Dim text | `#556677` / `#567` | Footer, very subdued labels |
| Card hover border | `#a855f7` | GameCard hover state |
| Star filled | `#a855f7` | StarRating filled stars |
| Star empty | `#2c3440` | StarRating empty stars |

### Typography

| Property | Value |
|---|---|
| **Font family** | `Inter` (Google Fonts), fallback `sans-serif` |
| **Font import** | `Inter:wght@300;400;500;600;700` |
| **Base font size** | `16px` (`--font-size`) |
| **h1** | `var(--text-2xl)` / weight `500` / line-height `1.5` |
| **h2** | `var(--text-xl)` / weight `500` / line-height `1.5` |
| **h3** | `var(--text-lg)` / weight `500` / line-height `1.5` |
| **h4** | `var(--text-base)` / weight `500` / line-height `1.5` |
| **body / p** | `var(--text-base)` / weight `400` / line-height `1.5` |
| **button** | `var(--text-base)` / weight `500` / line-height `1.5` |
| **input** | `var(--text-base)` / weight `400` / line-height `1.5` |
| **label** | `var(--text-base)` / weight `500` / line-height `1.5` |

#### Font Sizes Used Across Components

| Context | Size | Notes |
|---|---|---|
| Logo (`BACKLOGR`) | `1.1rem` | Weight 700, letter-spacing `0.15em` |
| Nav links | `0.85rem` | Weight 500 |
| Section headings | `1.1rem` | Weight 700 |
| Card titles | `0.8rem` - `0.85rem` | Weight 600 |
| Body / descriptions | `0.8rem` - `0.85rem` | Weight 400, line-height 1.5-1.6 |
| Badges / pills | `0.7rem` | Weight 600 |
| Metadata / timestamps | `0.7rem` - `0.75rem` | Muted color |
| Fine print / captions | `0.6rem` - `0.65rem` | Dim text |

#### Font Weights

| Weight | CSS Value | Usage |
|---|---|---|
| Light | `300` | Rarely used |
| Regular | `400` | Body text, descriptions, inputs |
| Medium | `500` | Nav links, buttons, labels, headings |
| Semibold | `600` | Card titles, badges, section labels |
| Bold | `700` | Logo, page headings, stat numbers |

### Spacing Scale

The app uses a consistent spacing system based on a **4px base unit**, applied via Tailwind's spacing utilities.

| Tailwind Class | Value | Usage |
|---|---|---|
| `gap-1` / `p-1` | `4px` | Tight icon spacing, zoom control gaps |
| `gap-1.5` / `p-1.5` | `6px` | Compact list item spacing |
| `gap-2` / `p-2` | `8px` | Icon-to-text gaps, small padding |
| `p-2.5` / `px-2.5` | `10px` | Badge horizontal padding, stat card padding |
| `gap-3` / `p-3` | `12px` | Card internal padding, menu item spacing |
| `gap-4` / `p-4` / `px-4` | `16px` | Section padding, page horizontal padding, card gaps |
| `py-6` / `gap-6` | `24px` | Page vertical padding, section gaps |
| `mb-8` | `32px` | Hero-to-content spacing, section bottom margins |
| `p-10` / `gap-10` | `40px` | Hero internal padding (desktop) |
| `mt-12` | `48px` | Footer top margin |

#### Common Spacing Patterns

- **Page container:** `max-w-6xl mx-auto px-4 py-6`
- **Card padding:** `p-3` or `p-4`
- **Section gaps:** `gap-4` or `gap-6`
- **Inline icon + text:** `gap-1.5` or `gap-2`
- **Stacked lists:** `space-y-3` or `space-y-4`

### Border Radii

Defined via the `--radius` token (`0.625rem` = `10px`) with computed variants.

| Token | Computed Value | Tailwind Class | Usage |
|---|---|---|---|
| `--radius-sm` | `6px` | `rounded-sm` | Small pills, inner elements |
| `--radius-md` | `8px` | `rounded-md` | Buttons, inputs, nav links, badges |
| `--radius-lg` | `10px` | `rounded-lg` | Cards, panels, dropdowns, game covers |
| `--radius-xl` | `14px` | `rounded-xl` | Page sections, hero banners, modals |
| `rounded-full` | `9999px` | `rounded-full` | Avatars, status badges, dot indicators |

#### Radii Used in Components

| Component | Radius | Notes |
|---|---|---|
| GameCard cover | `rounded-lg` | `8px` with overflow hidden |
| Status badges | `rounded-full` | Pill shape |
| Navbar | — | Full-width, no border radius |
| Cards / panels | `rounded-xl` | `14px`, main content containers |
| Buttons | `rounded-lg` | `10px` |
| Inputs | `rounded-lg` | `10px` |
| Avatars | `rounded-full` | Circular |

### Shadows

The app relies on a **low-shadow, high-contrast** approach. Depth is communicated through background color layering rather than heavy box shadows.

| Pattern | Implementation | Usage |
|---|---|---|
| **No shadow** | Default for most elements | Cards, buttons, badges |
| **Subtle border** | `border border-white/5` or `border-white/10` | Card outlines, dividers |
| **Backdrop blur** | `bg-[#14181c]/95 backdrop-blur-md` | Sticky navbar |
| **Gradient overlay** | `bg-gradient-to-r from-[#14181c] via-[#14181c]/70 to-transparent` | Hero banner text readability |
| **Hover glow** | `border-[#a855f7]` on hover | GameCard, interactive elements |
| **SVG drop shadow** | `<feDropShadow dx="0" dy="2" stdDeviation="4" floodOpacity="0.4">` | Flow diagram nodes |
| **SVG glow** | `<feGaussianBlur stdDeviation="3">` + merge | Flow diagram highlighted edges |

#### Depth Layers (via background color)

```
#0d1117  ----  Deepest   (sidebar, flow diagram canvas)
#14181c  ----  Base      (page background)
#1c2228  ----  Elevated  (cards, panels, navbar, popover)
#2c3440  ----  Surface   (inputs, tags, secondary backgrounds)
#ffffff08 ---  Border    (dividers, card outlines)
#ffffff0d ---  Hover     (hover states on transparent elements)
```

---

## Project Structure

```
src/
  app/
    App.tsx                 # Root component with RouterProvider
    routes.ts               # React Router config (all routes)
    components/
      layout.tsx            # Page shell (Navbar + Outlet + Footer)
      navbar.tsx            # Top navigation bar
      game-card.tsx         # Game cover thumbnail
      star-rating.tsx       # Star rating (static + interactive)
      review-card.tsx       # Review display card
      feed-item.tsx         # Activity feed entry
      status-badge.tsx      # Game status pill badge
      figma/
        ImageWithFallback.tsx   # Protected - img with fallback
      ui/                   # Radix-based UI primitives (shadcn)
    data/
      mock-data.ts          # All mock data (games, users, reviews, logs, feed)
    pages/
      home.tsx              # Feed / landing page
      browse.tsx            # Game catalog with search + filters
      game-detail.tsx       # Game detail page
      library.tsx           # Personal game library
      log-game.tsx          # Log / review form
      profile.tsx           # User profile
      recommend.tsx         # AI recommendations
      flow-diagram.tsx      # Interactive site flow diagram
      not-found.tsx         # 404 page
  styles/
    fonts.css               # Google Fonts import (Inter)
    theme.css               # All CSS custom properties / design tokens
```

---

## Tech Stack

| Layer | Technology | Version |
|---|---|---|
| Framework | React | 18.3.1 |
| Routing | React Router | 7.13.0 |
| Styling | Tailwind CSS | 4.1.12 |
| Icons | Lucide React | 0.487.0 |
| Build | Vite | 6.3.5 |
| UI Primitives | Radix UI | Various |
| Font | Inter | 300-700 via Google Fonts |

---

## Next Steps

- **Supabase integration** - Auth, user accounts, persistent game logs/reviews, social follows
- **IGDB API** - Real game catalog with cover art, metadata, and search
- **Vector search** - Natural language game discovery ("games like Hades but with a story")
- **Wishlist badge color** - The current violet (`#a700e0`) is close to the neon purple accent (`#a855f7`); consider differentiating (e.g., teal or gold)
- **Responsive polish** - Fine-tune mobile breakpoints on game detail and log form
- **Performance** - Lazy load page components, image optimization, skeleton loading states
