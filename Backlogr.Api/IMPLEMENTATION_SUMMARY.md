# Demo seed implementation summary

This drop-in adds a temporary admin-only production demo seeder for Backlogr.

## Seeded scope
- 1 demo admin viewer account: `steeley` / `Kate`
- 6 fake themed demo users
- 12 recognizable games imported from IGDB as needed
- 14 logs
- 7 reviews
- 9 likes
- 6 comments
- 11 follows

## Important behavior
- The real username should be `steeley`, not `@steeley`.
- The UI already renders the public handle as `@steeley`.
- The endpoint is protected by existing Admin/SuperAdmin auth and also gated by `DemoSeed:Enabled`.
- The request supports optional AI search backfill after game imports.

## Files included
- `Options/DemoSeedOptions.cs`
- `DTOs/Admin/AdminDemoSeedRequestDto.cs`
- `DTOs/Admin/AdminDemoSeedResponseDto.cs`
- `Services/Interfaces/IDemoSeedService.cs`
- `Services/Implementations/DemoSeedService.cs`
- `Controllers/AdminController.cs` (updated)
- `PROGRAM_CHANGES.md`
- `APPSETTINGS_PATCH.json`
- `README_DEMO_SEED.md`
