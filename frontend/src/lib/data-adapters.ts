import { apiClient, unwrapApiData } from './api-client';
import {
  LabTrackApi,
  AuthApi,
  AiApi,
  LaboratoriesApi,
  WorkstationsApi,
  AssetsApi,
  PcUsageApi,
  ProblemReportsApi,
  RepairTicketsApi,
  MaintenanceApi,
  ReservationsApi,
  NotificationsApi,
  UsersApi,
  AnalyticsApi,
  FinancialsApi,
  AuditLogsApi,
  NetworkApi,
  QrApi,
  ReplacementsApi,
  CommunicationsApi,
} from '../api/services';

export const LabTrackService = {
  // Laboratories & Campuses
  getLaboratories: LaboratoriesApi.getAll,
  getLaboratoryById: LaboratoriesApi.getById,
  createLaboratory: LaboratoriesApi.create,
  updateLaboratory: LaboratoriesApi.update,
  deleteLaboratory: LaboratoriesApi.remove,
  getLaboratoryWorkstations: LaboratoriesApi.getWorkstations,

  // Workstations & Assets
  getWorkstations: WorkstationsApi.getAll,
  getWorkstationById: WorkstationsApi.getById,
  createWorkstation: WorkstationsApi.create,
  updateWorkstation: WorkstationsApi.update,
  updateWorkstationStatus: WorkstationsApi.updateStatus,
  deleteWorkstation: WorkstationsApi.remove,
  getAssets: AssetsApi.getAll,
  getAssetById: AssetsApi.getById,
  createAsset: AssetsApi.create,
  updateAsset: AssetsApi.update,
  deleteAsset: AssetsApi.remove,

  // Problem Reports & Repair Tickets
  getProblemReports: (mineOnly = false) =>
    mineOnly ? ProblemReportsApi.getMine() : ProblemReportsApi.getAll(),
  createProblemReport: ProblemReportsApi.create,
  updateProblemReport: ProblemReportsApi.update,
  updateReportStatus: ProblemReportsApi.updateStatus,
  deleteProblemReport: ProblemReportsApi.remove,
  getRepairTickets: RepairTicketsApi.getAll,
  createRepairTicket: RepairTicketsApi.create,
  updateRepairTicket: RepairTicketsApi.update,
  deleteRepairTicket: RepairTicketsApi.remove,

  // Reservations
  getReservations: ReservationsApi.getAll,
  createReservation: ReservationsApi.create,
  updateReservationStatus: ReservationsApi.updateStatus,
  cancelReservation: ReservationsApi.cancel,

  // PC Usage Sessions (Student QR Scan & Heartbeat)
  scanLogin: PcUsageApi.scanLogin,
  scanLoginAuto: PcUsageApi.scanLoginAuto,
  sendHeartbeat: PcUsageApi.heartbeat,
  endSession: PcUsageApi.endSession,
  endSessionRelease: PcUsageApi.endSessionRelease,
  getActiveSessions: PcUsageApi.getActive,
  getSessionHistory: PcUsageApi.getHistory,

  // Maintenance Schedules & Records
  getMaintenanceSchedules: MaintenanceApi.getSchedules,
  createMaintenanceSchedule: MaintenanceApi.createSchedule,
  updateMaintenanceSchedule: MaintenanceApi.updateSchedule,
  deleteMaintenanceSchedule: MaintenanceApi.removeSchedule,
  getMaintenanceRecords: MaintenanceApi.getRecords,
  createMaintenanceRecord: MaintenanceApi.createRecord,
  updateMaintenanceRecord: MaintenanceApi.updateRecord,
  deleteMaintenanceRecord: MaintenanceApi.removeRecord,

  // Notifications
  getNotifications: NotificationsApi.getAll,
  createNotification: NotificationsApi.create,
  markNotificationRead: NotificationsApi.markRead,
  markAllNotificationsRead: NotificationsApi.markAllRead,
  deleteNotification: NotificationsApi.remove,

  // Analytics, Financials & Audit Logs
  getUsageAnalytics: AnalyticsApi.getUsage,
  getAssetStatusSummary: AnalyticsApi.getAssetStatusSummary,
  getFrequentIssues: AnalyticsApi.getFrequentIssues,
  getFinancialSummary: FinancialsApi.getSummary,
  getFinancialTransactions: FinancialsApi.getTransactions,
  createFinancialTransaction: FinancialsApi.createTransaction,
  getAuditLogs: AuditLogsApi.getAll,
  deleteAuditLog: AuditLogsApi.remove,
  getChainOfCustody: AuditLogsApi.getChainOfCustody,

  // Network, QR & Replacements
  getNetworkDevices: NetworkApi.getDevices,
  getNetworkSummary: NetworkApi.getSummary,
  getSystemNetworkInfo: NetworkApi.getSystemNetworkInfo,
  getReplacements: ReplacementsApi.getAll,
  createReplacement: ReplacementsApi.create,
  updateReplacementStatus: ReplacementsApi.updateStatus,
  validateQr: QrApi.validate,
  generateQr: QrApi.generate,
  getAdminQrList: QrApi.getAdminList,

  // Users & Profile
  getUsers: UsersApi.getAll,
  createUser: UsersApi.create,
  updateUser: UsersApi.update,
  toggleUserStatus: UsersApi.toggleStatus,
  deleteUser: UsersApi.remove,
  getCurrentUser: async () => unwrapApiData(await apiClient.get('/api/v1/users/me')),
  updateCurrentUser: async (data: any) => unwrapApiData(await apiClient.put('/api/v1/users/me', data)),

  // AI Assistant (Global, Admin, Instructor)
  aiChat: AiApi.chat,
  aiChatQuery: AiApi.chatQuery,
  getAiSessions: AiApi.getSessions,
  estimateDamageCost: AiApi.estimateDamageCost,
  predictiveMaintenance: AiApi.predictiveMaintenance,
  getAiHealth: AiApi.getHealth,

  // Nested Modular Access
  api: LabTrackApi,
  auth: AuthApi,
  ai: AiApi,
  laboratories: LaboratoriesApi,
  workstations: WorkstationsApi,
  assets: AssetsApi,
  pcUsage: PcUsageApi,
  problemReports: ProblemReportsApi,
  repairTickets: RepairTicketsApi,
  maintenance: MaintenanceApi,
  reservations: ReservationsApi,
  notifications: NotificationsApi,
  users: UsersApi,
  analytics: AnalyticsApi,
  financials: FinancialsApi,
  auditLogs: AuditLogsApi,
  network: NetworkApi,
  qr: QrApi,
  replacements: ReplacementsApi,
  communications: CommunicationsApi,
};

export default LabTrackService;