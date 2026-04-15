# Backlogr

Backlogr is a social web app for video games inspired by Letterboxd. It lets users track what they play, build a personal library, write reviews, follow other players, and discover games through both traditional browse flows and AI-powered discovery.

The project was built as a full-stack course final and is deployed in Azure with a separate frontend, backend, database, AI integration, and CI/CD pipelines.

## Live app

- **Web:** https://victorious-grass-0bb8bf10f.2.azurestaticapps.net/
- **API:** https://backlograpi.azurewebsites.net
- **Swagger:** https://backlograpi.azurewebsites.net/swagger

## Try it out

You can either create your own account in the live app or sign in with the demo account below.

### Demo admin account

- **Username:** `admin`
- **Password:** `Horse1234!`

> Provided for class/demo convenience. Credentials should be rotated or removed after the course final.

## What Backlogr does

### Social tracking and reviews
- Register and log in
- Search and browse for games
- Add games to a personal library
- Track play status with:
  - `Backlog`
  - `Playing`
  - `Played`
  - `Wishlist`
  - `Dropped`
- Rate games in 0.5-point increments
- Write reviews with spoiler flags
- Like reviews and comment on review threads
- Follow other users and view their activity
- View member profiles with library and review activity

### Discovery and game pages
- Browse local catalog results first, with IGDB-backed fallback through the API
- Import game data into the local catalog through the app flow
- Open dedicated game pages with:
  - game details
  - community reviews
  - community activity
  - your own current log/review state
- Use two feed views:
  - **For You** — broader recent activity across the app
  - **Following** — activity from followed users plus your own

### AI-enabled features
- Get AI-powered game recommendations based on your own logs, ratings, and review themes
- Use semantic search for natural-language game discovery
- Use the AI review assistant to:
  - draft review text
  - rewrite for clarity
  - shorten or expand a review
  - generate spoiler-safe summaries

## Admin capabilities

Backlogr includes an admin dashboard for account management.

### Current admin features
- View users in an admin dashboard
- Create new accounts
- Filter/search users
- Edit roles through the admin flow
- Support role levels including `User`, `Admin`, and `SuperAdmin`
- Prevent unsafe self-service role actions through backend guardrails
- Allow users to delete their own account through a confirmation flow



## Current project status

Backlogr is deployed and working in Azure as a live MVP. The frontend is deployed through Azure Static Web Apps, the API is deployed through Azure App Service, Azure SQL is used for persistence, and the app now includes live AI recommendations, review-assistant actions, and semantic search through Azure AI Search and OpenAI-backed services.

The deployed app currently supports live register/login flows, browse/search, logging, library management, reviews, social feed interactions, follows, member profiles, admin account-management flows, and AI-assisted discovery.

## High-level architecture

1. **Backlogr.Web** is a Nuxt 3 frontend deployed to Azure Static Web Apps.
2. **Backlogr.Api** is an ASP.NET Core Web API deployed to Azure App Service.
3. **Azure SQL Database** stores users, games, logs, reviews, comments, likes, and follows.
4. **IGDB integration** supports game search/import and local catalog caching.
5. **OpenAI-backed services** power review assistance, embeddings, and recommendation generation.
6. **Azure AI Search** powers semantic/vector search over indexed game metadata.
7. **GitHub Actions** handles build, test, and deployment workflows.

## Tech stack

### Frontend
- Nuxt 3
- Vue 3
- Vuetify 3
- TypeScript
- Pinia
- Axios
- Vitest

### Backend
- .NET 10
- ASP.NET Core Web API
- Entity Framework Core 10
- ASP.NET Core Identity
- JWT authentication
- xUnit

### AI / search
- OpenAI API
- Azure AI Search

### Hosting / DevOps
- Azure Static Web Apps
- Azure App Service
- Azure SQL Database
- GitHub Actions

## Repository layout

```text
Backlogr/
├── .github/                     # GitHub Actions workflows
├── .vscode/                     # local editor settings
├── Backlogr.Api/                # ASP.NET Core Web API
├── Backlogr.Api.Tests/          # backend tests
├── Backlogr.Web/                # Nuxt frontend
├── docs/                        # project docs and supporting materials
│   ├── ui/                      # UI mockups / design assets
│   ├── BACKEND_TODO.md
│   ├── FRONTEND_TODO.md
│   ├── README_API.md
│   └── README_WEB.md
├── scripts/                     # helper scripts
├── README.md                    # root project overview
├── requirements_backlogr.md
└── requirements_backlogr_updated.md
```

## Documentation map

Start with these files:

- `README.md` — top-level project overview
- `docs/README_WEB.md` — frontend architecture, routes, integration notes, and local setup
- `docs/README_API.md` — backend architecture, endpoints, domain rules, and deployment notes
- `docs/FRONTEND_TODO.md` — frontend completion and polish checklist
- `docs/BACKEND_TODO.md` — backend completion and polish checklist
- `requirements_backlogr.md` — original requirements draft
- `requirements_backlogr_updated.md` — updated requirements and architecture plan

## Local development

### Frontend
From `Backlogr.Web`:

```powershell
npm install
npm run dev
```

### Backend
From `Backlogr.Api`:

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet restore
dotnet build
dotnet ef database update
dotnet run
```

### Frontend runtime config

Set the frontend API base URL with:

```powershell
$env:NUXT_PUBLIC_API_BASE = "http://localhost:5042"
```

Use the deployed API URL instead when pointing the frontend at production.

## Notes for reviewers

- This project was built as a class final for an AI-enabled social video game tracking application.
- The app is intentionally split into a frontend and backend to reflect a real full-stack deployment.
- Core AI functionality and vector search are included as part of the actual deployed application.
- The repo contains both implementation docs and TODO checklists so reviewers can quickly see shipped features versus remaining polish work.
