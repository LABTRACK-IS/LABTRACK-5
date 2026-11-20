## Summary

Iteration **v5** of LabTrack, imported from ``labtrack 5`` on 2026-11-20.

Compared against: **labtrack 4**.

## Change overview

| Area | Modified | Added |
|------|----------|-------|
| Backend | **0** | **0** |
| Frontend | **4** | **0** |

Full per-file detail in [CHANGELOG-V5.md](./CHANGELOG-V5.md).

## Backend - notable changes

- (no backend modifications)

### New backend files

- (none)

## Frontend - notable changes

- frontend\src\components\auth\AuthLayout.tsx (18971B)
- frontend\src\layouts\LandingLayout.tsx (12604B)
- frontend\src\pages\landing\AboutPage.tsx (31674B)
- frontend\src\pages\landing\HomePage.tsx (36081B)

### New frontend files

- (none)

## QA checklist

- [ ] dotnet build succeeds on backend
- [ ] tsc --noEmit returns 0 errors on frontend
- [ ] /health/live returns 200
- [ ] No junk files (bak, cleanup) in git tree
- [ ] Diff vs labtrack 4 is intentional
- [ ] Swagger shows all controllers

## How to verify

Backend:

    cd backend/LabTrack.CSharp/src/LabTrack.Api
    dotnet build && dotnet run

Frontend (new terminal):

    cd frontend
    npm install && npm run dev

Expected URLs:

- http://localhost:3000 - frontend
- http://localhost:5000/swagger - API docs
- http://localhost:5000/health/live - 200 OK

---

Auto-generated on 2026-11-20 21:13:18
