# LabTrack v5 - Iteration 5

Iteration 5 of LabTrack. Imported from labtrack 5 on 2026-11-20.

Compared against: **labtrack 4**.

## Snapshot

- Backend controllers: **29**
- Backend C# files: **459**
- Frontend TS/TSX files: **72**

## Changes in v5 (vs labtrack 4)

### Backend

- **0** files modified
- **0** new files
- Compile Include directives cleaned (CS0101)

### Frontend

- **4** files modified
- **0** new files

## Infrastructure

- Cleaned stray Compile Include directives from csproj
- Removed bak and cleanup text artifacts
- Updated .gitignore

## Full details

See [CHANGELOG-V5.md](./CHANGELOG-V5.md) for the complete per-file diff.

## Controllers

- AiAssistant
- Analytics
- Asset
- Audit
- Auth
- ChainOfCustody
- Communication
- Financial
- IdempotencyRecords
- InstructorCommunication
- Laboratories
- Maintenance
- MaintenanceRecord
- MaintenanceRecords
- Network
- NetworkIncident
- NetworkInfo
- Notification
- PcUsage
- ProblemReport
- QrAdmin
- Qr
- QrSession
- RepairTicket
- Replacement
- ReplacementRecord
- Reservation
- Users
- Workstations

## Quick start

Backend:

    cd backend/LabTrack.CSharp/src/LabTrack.Api
    dotnet run

Frontend:

    cd frontend
    npm install
    npm run dev

Endpoints:

- Frontend: http://localhost:3000
- Swagger: http://localhost:5000/swagger
- Health: http://localhost:5000/health/live

---

Generated: 2026-11-20 21:13:18
