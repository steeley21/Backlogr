# Demo seed rollout notes

## Important username note
Do **not** store the demo username as `@steeley`.

Your login flow treats any identifier containing `@` as an email address, so the real seeded username should be:

- `steeley`

The UI will already render that as `@steeley` where appropriate.

## App Service setting
Enable the seed only when you are ready to run it in Azure:

- `DemoSeed__Enabled = true`

After the seed is done and verified, turn it back off:

- `DemoSeed__Enabled = false`

## Swagger request
Call:

- `POST /api/admin/demo/seed`

Request body:

```json
{
  "runAiBackfill": true
}
```

## Seeded demo login
- Username: `steeley`
- Password: `Backlogr123!`

## What this seeds
- 1 demo admin viewer account (`steeley` / `Kate`)
- 6 clearly fake themed users
- follows to make the Following feed non-empty
- logs, reviews, likes, and comments
- a small imported game pool from IGDB
- optional Azure AI Search backfill after import
