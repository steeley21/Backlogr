# Backlogr.Api

ASP.NET Core Web API backend for **Backlogr**, a social video game tracking app inspired by Letterboxd.

## Current status

The backend foundation is now working locally.

Implemented so far:
- ASP.NET Core Web API on **.NET 10**
- Entity Framework Core + LocalDB for local development
- ASP.NET Core Identity with **Guid** keys
- JWT authentication
- Role seeding for `User` and `Admin`
- Auth endpoints:
  - `POST /api/auth/register`
  - `POST /api/auth/login`
  - `GET /api/auth/me`
- Library/logging endpoints:
  - `GET /api/library/me`
  - `POST /api/library`
  - `DELETE /api/library/{gameId}`
- `Game` and `GameLog` domain models
- Swagger/OpenAPI with bearer auth support
- Automated tests for auth and library flows

Not implemented yet:
- Review/social entities and endpoints
- Game catalog endpoints
- IGDB integration
- Feed/follow features
- AI/recommendation/vector search features

---

## Tech stack

- **.NET 10**
- **ASP.NET Core Web API**
- **Entity Framework Core 10**
- **SQL Server LocalDB** for local development
- **ASP.NET Core Identity**
- **JWT Bearer auth**
- **Swashbuckle / Swagger**
- **xUnit** for tests

---

## Project structure

```text
Backlogr.Api/
├── Common/
├── Controllers/
│   ├── AuthController.cs
│   └── LibraryController.cs
├── Data/
│   ├── ApplicationDbContext.cs
│   ├── DevelopmentDataSeeder.cs
│   └── IdentityDataSeeder.cs
├── DTOs/
│   ├── Auth/
│   └── Library/
├── Extensions/
├── Migrations/
├── Models/
│   ├── Entities/
│   └── Enums/
├── Options/
├── Services/
│   ├── Implementations/
│   └── Interfaces/
├── Properties/
├── appsettings.json
├── appsettings.Development.json
└── Program.cs
```

---

## Current domain model

### Identity
- `ApplicationUser`
- `ApplicationRole`

### Catalog
- `Game`

### Library
- `GameLog`
- `LibraryStatus`

### Implemented constraints
- `GameLog` unique index on `(UserId, GameId)`
- `Game.IgdbId` unique filtered index when present

---

## Auth model

`ApplicationUser` extends `IdentityUser<Guid>` and currently includes:
- `DisplayName`
- `FirstName`
- `LastName`
- `AvatarUrl`
- `Bio`
- `CreatedAt`

Identity fields such as `UserName` and `Email` come from `IdentityUser<Guid>`.

---

## Library model

`GameLog` currently supports:
- `Status`
- `Rating`
- `Platform`
- `Hours`
- `StartedAt`
- `FinishedAt`
- `Notes`

### Supported library statuses
- `Backlog`
- `Playing`
- `Played`
- `Wishlist`
- `Dropped`

### Current validation rules
- one log per `(UserId, GameId)`
- `Rating` must be between `0.0` and `5.0`
- `Rating` must be in `0.5` increments
- `Hours` cannot be negative
- `FinishedAt` cannot be earlier than `StartedAt`
- `FinishedAt` is required when status is `Played`

---

## Local setup

### Prerequisites
- .NET 10 SDK
- Visual Studio 2022 or later
- SQL Server LocalDB

### Restore / build
From the repo root or from `Backlogr.Api`:

```powershell
dotnet restore
dotnet build
```

### User secrets
Initialize user secrets on the API project and keep secrets out of source control.

Required local secrets:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=BacklogrDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  },
  "Jwt": {
    "Key": "your-long-random-dev-secret",
    "Issuer": "Backlogr.Api",
    "Audience": "Backlogr.Web"
  },
  "Igdb": {
    "ClientId": "",
    "ClientSecret": ""
  }
}
```

Recommended commands from `Backlogr.Api`:

```powershell
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=(localdb)\MSSQLLocalDB;Database=BacklogrDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
dotnet user-secrets set "Jwt:Issuer" "Backlogr.Api"
dotnet user-secrets set "Jwt:Audience" "Backlogr.Web"
dotnet user-secrets set "Jwt:Key" "replace-with-a-long-random-secret"
```

### Database migrations
From `Backlogr.Api`:

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet ef database update
```

Current migrations:
- `InitialIdentitySetup`
- `AddGamesAndGameLogs`
- `AlignGameLogWithLibraryRequirements`

---

## Running the API

From `Backlogr.Api`:

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet run
```

Swagger should open locally based on the launch profile.

### Swagger auth
Use the `POST /api/auth/login` endpoint to get a token, then click **Authorize** in Swagger and paste the **raw JWT token only**.

Do **not** prefix it with `Bearer` in the Swagger auth dialog.

---

## Development seed behavior

In development, startup currently seeds:
- roles: `User`, `Admin`
- one temporary test game for library endpoint testing

Current seeded test game id:

```text
11111111-1111-1111-1111-111111111111
```

This is only intended for local development/testing until real game catalog endpoints and IGDB import are implemented.

---

## Implemented endpoints

### Auth
- `POST /api/auth/register`
- `POST /api/auth/login`
- `GET /api/auth/me`

### Library
- `GET /api/library/me`
- `POST /api/library`
- `DELETE /api/library/{gameId}`

---

## Testing

The backend test project is `Backlogr.Api.Tests`.

Current automated coverage includes:
- library service tests
- unauthorized library route tests
- authenticated library flow tests
- auth integration tests for:
  - register
  - login by username
  - login by email
  - duplicate email rejection
  - wrong password rejection
  - authenticated `/api/auth/me` coverage

Run tests from `Backlogr.Api` or repo root:

```powershell
dotnet test
```

---

## Security / config notes

- Keep JWT keys, IGDB secrets, and connection strings out of tracked config files.
- Use **User Secrets** for local development.
- Production secrets should move to **Azure Key Vault** later.
- Avatar handling is URL-only for now.
- No file upload/storage is implemented yet.

---

## Current gaps / next steps

Recommended next backend feature slice:
1. Add `Review` entity and migration
2. Build review DTOs, service, and controller
3. Implement:
   - `POST /api/reviews`
   - `PUT /api/reviews/{reviewId}`
   - `DELETE /api/reviews/{reviewId}`
4. Add review unit/integration tests

Technical cleanup still worth doing:
- extract token generation into an auth service
- add admin bootstrap flow
- add global exception handling
- add structured logging
- add game/catalog endpoints
