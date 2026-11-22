LabTrack - Architecture Snapshot (Build 5)

This build is a state capture taken 2026-11-20.
Baseline diff target: labtrack 4.

The codebase below is the current production-ready state. When diff against baseline is minimal, this document focuses on the architecture itself rather than the delta.

Delta note
----------

Compared to labtrack 4:

- Backend files modified: 0
- Backend files added: 0
- Frontend files modified: 4
- Frontend files added: 0

This is a **near-identical build** to labtrack 4. The architecture below describes the current codebase.

Backend architecture
--------------------

Layers: Api / Application / Domain / Infrastructure
Language: C# (.NET 8)
Files: 459 source files

### Controllers (29)

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
- Qr
- QrAdmin
- QrSession
- RepairTicket
- Replacement
- ReplacementRecord
- Reservation
- Users
- Workstations

### Application services (17)

- AiAssistant
- Analytics
- Asset
- Audit
- Auth
- Communication
- Financial
- Maintenance
- Network
- Notification
- PcUsage
- ProblemReport
- Qr
- RepairTicket
- Replacement
- Reservation
- User

### Domain entities (32)

- AiChatMessage
- AiChatSession
- Asset
- AuditLog
- AuthSession
- ChainOfCustodyEvent
- Communication
- ComponentReplacement
- DamageAttribution
- FinancialExpense
- FinancialRecord
- IdempotencyKey
- InstructorCommunication
- Laboratory
- MaintenanceRecord
- MaintenanceSchedule
- NetworkDevice
- NetworkIncident
- Notification
- PcTelemetryEvent
- PcUsageLog
- ProblemReport
- QrCode
- QrSession
- RefreshToken
- RepairTicket
- ReplacementRecord
- ReplacementRequest
- Reservation
- User
- Workstation
- WorkstationSession

### Repositories (17)

- AiChat
- Asset
- Audit
- AuthSession
- Communication
- Financial
- Laboratory
- Maintenance
- Network
- Notification
- PcUsage
- ProblemReport
- Qr
- RepairTicket
- Replacement
- Reservation
- User

### EF Core migrations (10)

- 20261024094702_FixShadowColumns
- 20261024094702_FixShadowColumns.Designer
- 20261024162809_AddNetworkDevices
- 20261024162809_AddNetworkDevices.Designer
- 20261024165714_AddPcUsageLogBaseEntityColumns
- 20261024165714_AddPcUsageLogBaseEntityColumns.Designer
- 20261024172815_DropShadowFkColumns-20261025012745
- 20261024172815_DropShadowFkColumns-20261025012745.Designer
- 20261025055855_AddReplacementRecordAndInstructorCommTables
- 20261025055855_AddReplacementRecordAndInstructorCommTables.Designer

Frontend architecture
---------------------

Stack: React 18 + Vite + TypeScript
Files: 72 source files

### Admin views (18)

- AdminDashboard
- AdminNotifications
- AiAssistant
- AnalyticsReports
- AssetsWorkstationDesk
- AuditLogs
- CampusesLabs
- FinancialReports
- InventoryTransfer
- IssueServiceDesk
- Maintenance
- NotificationsManagement
- Overview
- ProfileSettings
- QrGeneratorModal
- QrManagement
- SessionsManagement
- UserAccessManagement

### Instructor views (11)

- AiAssistant
- InstructorDashboard
- LabAvailability
- MyRequests
- Notifications
- Overview
- Profile
- ReportProblem
- Reservations
- StudentSessions
- UsageAnalytics

### Custom hooks (3)

- useNotificationPolling
- usePcSession
- useRealtime

Python microservice
------------------

Framework: FastAPI
Files: 0 Python files

### API routes (7)

- /ai_chat
- /asset
- /damage
- /health
- /recommendations
- /repair
- /telemetry

How to run
----------

API:

    cd backend/LabTrack.CSharp/src/LabTrack.Api
    dotnet run

SPA:

    cd frontend
    npm install && npm run dev

Python AI:

    cd backend/LabTrack.Python
    uvicorn app.main:app --reload

Surfaces:

- SPA        http://localhost:3000
- Swagger    http://localhost:5000/swagger
- Health     http://localhost:5000/health/live

Snapshot created 2026-11-20 21:18:08.
