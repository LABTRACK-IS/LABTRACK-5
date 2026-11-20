LabTrack Build 5
================

Build 5 landed on 2026-11-20.
Rollup compared against **labtrack 4**.

TL;DR
-----

Backend saw 0 edits and 0 additions.
Frontend saw 4 edits and 0 additions.
Project files cleaned of stray Compile Include directives.

Delta at a glance
-----------------

| Layer    | Edited | Added | Removed |
|----------|--------|-------|---------|
| Backend  | 0      | 0     | 0       |
| Frontend | 4      | 0     | 0       |

What moved in backend
---------------------

* nothing edited

What moved in frontend
----------------------

* frontend\src\components\auth\AuthLayout.tsx  [18971B vs 18590B]
* frontend\src\layouts\LandingLayout.tsx  [12604B vs 8195B]
* frontend\src\pages\landing\AboutPage.tsx  [31674B vs 21589B]
* frontend\src\pages\landing\HomePage.tsx  [36081B vs 21094B]

Runtime checks
--------------

| Check                       | Expectation |
|-----------------------------|-------------|
| dotnet build (API project)  | succeeded   |
| tsc --noEmit (frontend)     | 0 errors    |
| /health/live                | 200 OK      |
| /swagger/v1/swagger.json    | loads       |

Code surface
------------

- Controllers registered: 29
- C# source files: 459
- TS/TSX source files: 72

Rollout
-------

Fire the API:

    cd backend/LabTrack.CSharp/src/LabTrack.Api
    dotnet run

Fire the SPA:

    cd frontend
    npm install && npm run dev

Hit these:

- http://localhost:3000 (SPA)
- http://localhost:5000/swagger (API surface)
- http://localhost:5000/health/live (probe)

Notes
-----

Full per-file delta ships in CHANGELOG-V5.md.
PR review notes ship in PR-BODY.md.

Snapshot written 2026-11-20 21:16:19.
