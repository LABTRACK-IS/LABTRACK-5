Build 5 - Component Inventory
=============================

Snapshot date: 2026-11-20 21:18:08
Delta baseline: labtrack 4

This ledger captures the current component surface rather than a line-by-line diff. Where the diff vs labtrack 4 is minimal, this acts as the inventory of what the build ships.

Movement since labtrack 4
----------------------------

- Backend touched: 0 files
- Backend introduced: 0 files
- Frontend touched: 4 files
- Frontend introduced: 0 files

Frontend files touched:

* frontend\src\components\auth\AuthLayout.tsx
* frontend\src\layouts\LandingLayout.tsx
* frontend\src\pages\landing\AboutPage.tsx
* frontend\src\pages\landing\HomePage.tsx

Component surface
-----------------

Controllers (29):

* AiAssistant
* Analytics
* Asset
* Audit
* Auth
* ChainOfCustody
* Communication
* Financial
* IdempotencyRecords
* InstructorCommunication
* Laboratories
* Maintenance
* MaintenanceRecord
* MaintenanceRecords
* Network
* NetworkIncident
* NetworkInfo
* Notification
* PcUsage
* ProblemReport
* Qr
* QrAdmin
* QrSession
* RepairTicket
* Replacement
* ReplacementRecord
* Reservation
* Users
* Workstations

Application services (17):

* AiAssistant
* Analytics
* Asset
* Audit
* Auth
* Communication
* Financial
* Maintenance
* Network
* Notification
* PcUsage
* ProblemReport
* Qr
* RepairTicket
* Replacement
* Reservation
* User

Domain entities (32):

* AiChatMessage
* AiChatSession
* Asset
* AuditLog
* AuthSession
* ChainOfCustodyEvent
* Communication
* ComponentReplacement
* DamageAttribution
* FinancialExpense
* FinancialRecord
* IdempotencyKey
* InstructorCommunication
* Laboratory
* MaintenanceRecord
* MaintenanceSchedule
* NetworkDevice
* NetworkIncident
* Notification
* PcTelemetryEvent
* PcUsageLog
* ProblemReport
* QrCode
* QrSession
* RefreshToken
* RepairTicket
* ReplacementRecord
* ReplacementRequest
* Reservation
* User
* Workstation
* WorkstationSession

Repositories (17):

* AiChat
* Asset
* Audit
* AuthSession
* Communication
* Financial
* Laboratory
* Maintenance
* Network
* Notification
* PcUsage
* ProblemReport
* Qr
* RepairTicket
* Replacement
* Reservation
* User

EF migrations (10):

* 20261024094702_FixShadowColumns
* 20261024094702_FixShadowColumns.Designer
* 20261024162809_AddNetworkDevices
* 20261024162809_AddNetworkDevices.Designer
* 20261024165714_AddPcUsageLogBaseEntityColumns
* 20261024165714_AddPcUsageLogBaseEntityColumns.Designer
* 20261024172815_DropShadowFkColumns-20261025012745
* 20261024172815_DropShadowFkColumns-20261025012745.Designer
* 20261025055855_AddReplacementRecordAndInstructorCommTables
* 20261025055855_AddReplacementRecordAndInstructorCommTables.Designer

Admin UI modules (18):

* AdminDashboard
* AdminNotifications
* AiAssistant
* AnalyticsReports
* AssetsWorkstationDesk
* AuditLogs
* CampusesLabs
* FinancialReports
* InventoryTransfer
* IssueServiceDesk
* Maintenance
* NotificationsManagement
* Overview
* ProfileSettings
* QrGeneratorModal
* QrManagement
* SessionsManagement
* UserAccessManagement

Instructor UI modules (11):

* AiAssistant
* InstructorDashboard
* LabAvailability
* MyRequests
* Notifications
* Overview
* Profile
* ReportProblem
* Reservations
* StudentSessions
* UsageAnalytics

Python FastAPI routes (7):

* /ai_chat
* /asset
* /damage
* /health
* /recommendations
* /repair
* /telemetry

Housekeeping
------------

* Compile Include directives scrubbed from csproj files
* bak and cleanup artifacts excluded from working tree
* .gitignore refreshed to keep junk out of tracking

Inventory complete. 2026-11-20 21:18:08
