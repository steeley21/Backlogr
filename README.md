# Backlogr

Backlogr is a social web app for video games inspired by Letterboxd. It lets users track what they play, keep a personal library, write reviews, follow activity, and discover games through browse and recommendation flows.

## Live app

- **Web:** https://victorious-grass-0bb8bf10f.2.azurestaticapps.net/
- **API:** https://backlograpi.azurewebsites.net

## Try it out

You can either create your own account in the live app or use the demo admin account below.

### Demo admin account

- **Username:** `admin`
- **Password:** `Horse1234!`

> This is for class/demo use convenience. Will be rotated or removed after final.

## What the app does

### Core user features
- Register and log in
- Browse and search for games
- Import game data through the app's IGDB-backed catalog flow
- Add games to a personal library
- Track play status using:
  - `Playing`
  - `Played`
  - `Backlog`
  - `Wishlist`
  - `Dropped`
- Rate games in 0.5-point increments
- Write reviews with spoiler flags
- View a social-style feed of activity
- Use AI recommendation and review-assistant surfaces

### Admin features
- View users in an admin dashboard
- Create new `User` and `Admin` accounts
- Promote and demote roles
- Grant `SuperAdmin` to other users
- Delete users with guardrails
- Block unsafe actions such as self-demotion or deleting protected accounts
- Allow users to delete their own account with confirmation checks

## Current project status

Backlogr is deployed in Azure with a working frontend and backend. The current production app supports live register/login flows, browse, library, feed, and IGDB-backed catalog usage. The API is deployed separately, Swagger is available, and automated backend tests are in place. AI surfaces exist in the current MVP, but the backend recommendation/review-assistant behavior is still stub-backed rather than fully integrated with Azure AI services.

## High-level architecture

1. **Backlogr.Web** is a Nuxt 3 frontend deployed to Azure Static Web Apps.
2. **Backlogr.Api** is an ASP.NET Core Web API deployed to Azure App Service.
3. **Azure SQL** stores users, games, logs, reviews, follows, comments, and likes.
4. **IGDB integration** provides game search/import and local catalog caching.
5. **GitHub Actions** handles CI/CD for both frontend and backend.
6. **Azure AI / Azure AI Search** are planned for deeper recommendation and semantic search work, but are not fully implemented yet.

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
- Entity Framework Core
- ASP.NET Core Identity
- JWT authentication
- xUnit

### Hosting / DevOps
- Azure Static Web Apps
- Azure App Service
- Azure SQL Database
- GitHub Actions

## Repository layout

```text
Backlogr/
├── Backlogr.Web/   # Nuxt frontend
├── Backlogr.Api/   # ASP.NET Core Web API
├── docs/           # detailed frontend/backend docs and TODOs
└── .github/        # CI/CD workflows
```

## Detailed docs

For project-specific setup and deeper implementation notes, start here:

- `docs/README_WEB.md`
- `docs/README_API.md`
- `docs/FRONTEND_TODO.md`
- `docs/BACKEND_TODO.md`

## Local development at a glance

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

## Notes for reviewers

- This project is built as a class final for a social video game tracking app.
- The app is intentionally structured with a separate frontend and backend.


