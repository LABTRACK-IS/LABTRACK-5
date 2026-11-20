import apiClient, {
  unwrapApiData,
  unwrapPaginatedItems,
  getPythonAiBackendUrl,
} from '../lib/api-client';
import axios from 'axios';

export interface PaginationQuery {
  pageNumber?: number;
  pageSize?: number;
  search?: string;
  status?: string;
  campus?: string;
}

export interface AiChatPayload {
  prompt: string;
  role?: string;
  userName?: string;
  userEmail?: string;
  campusId?: string;
  path?: string;
  sessionId?: string;
  history?: Array<{ role: string; text: string }>;
}

export interface AiChatNormalizedResult {
  sessionId: string;
  response: string;
  processingTimeMs: number;
  tokensUsed: number;
  suggestions: string[];
}

export const AuthApi = {
  login: async (payload: any) => unwrapApiData(await apiClient.post('/api/v1/auth/login', payload)),
  register: async (payload: any) => unwrapApiData(await apiClient.post('/api/v1/auth/register', payload)),
  verifyPin: async (payload: any) => unwrapApiData(await apiClient.post('/api/v1/auth/verify-pin', payload)),
};

const collectLiveSystemPageContext = () => {
  if (typeof window === 'undefined') return {};
  try {
    const path = window.location.pathname || '/';
    const search = window.location.search || '';
    const params = new URLSearchParams(search);

    // 1. Detect logged-in user from storage
    let storedUser: any = {};
    const rawUser =
      sessionStorage.getItem('labtrack_user') ||
      localStorage.getItem('labtrack_user') ||
      localStorage.getItem('jrmsu_labtrack_user');
    if (rawUser) {
      try { storedUser = JSON.parse(rawUser); } catch {}
    }

    // 2. Detect active PC / Workstation session from URL, storage, or DOM
    let pcCode =
      params.get('pc') ||
      params.get('workstation') ||
      params.get('ws') ||
      sessionStorage.getItem('labtrack_workstation') ||
      localStorage.getItem('labtrack_workstation') ||
      localStorage.getItem('jrmsu_labtrack_workstation') ||
      '';
    let labName =
      params.get('lab') ||
      params.get('laboratory') ||
      sessionStorage.getItem('labtrack_lab') ||
      localStorage.getItem('labtrack_lab') ||
      '';
    let activeSessionId =
      sessionStorage.getItem('labtrack_session_id') ||
      localStorage.getItem('labtrack_session_id') ||
      localStorage.getItem('jrmsu_labtrack_session_id') ||
      '';

    // Scan all storage keys for any active student session or workstation object
    for (const store of [sessionStorage, localStorage]) {
      for (let i = 0; i < store.length; i++) {
        const k = store.key(i) || '';
        if (k.toLowerCase().includes('session') || k.toLowerCase().includes('pc') || k.toLowerCase().includes('workstation') || k.toLowerCase().includes('student')) {
          const val = store.getItem(k) || '';
          if (val.startsWith('{')) {
            try {
              const obj = JSON.parse(val);
              if (!pcCode && (obj.workstationCode || obj.workstationNumber || obj.pcName || obj.pcNumber || obj.code)) {
                pcCode = String(obj.workstationCode || obj.workstationNumber || obj.pcName || obj.pcNumber || obj.code);
              }
              if (!labName && (obj.labName || obj.laboratoryName || obj.roomName)) {
                labName = String(obj.labName || obj.laboratoryName || obj.roomName);
              }
              if (!activeSessionId && (obj.sessionId || obj.id)) {
                activeSessionId = String(obj.sessionId || obj.id);
              }
            } catch {}
          }
        }
      }
    }

    // 3. Detect exact active page heading & active navigation tab from live DOM
    const headings = Array.from(document.querySelectorAll('h1, h2'))
      .map((el) => (el.textContent || '').trim())
      .filter((t) => t.length > 2 && !t.toLowerCase().includes('labtrack ai'));
    const activeHeading = headings[0] || document.title || 'LabTrack Portal';

    // Extract visible KPI / status cards on the current page so AI knows what's on screen
    const visibleText = (document.body?.innerText || '').replace(/\s+/g, ' ');
    if (!pcCode) {
      const pcMatch = visibleText.match(/\b(WS-\d+|PC-\d+|COMLAB\s*\d+\s*-\s*PC-\d+)\b/i);
      if (pcMatch) pcCode = pcMatch[1].toUpperCase();
    }
    if (!labName) {
      const labMatch = visibleText.match(/\b(Main Lab|ComLab\s*\d+|IT Lab\s*\w+|Network Lab\s*\w+)\b/i);
      if (labMatch) labName = labMatch[1];
    }

    return {
      fullUrl: window.location.href,
      path,
      queryString: search,
      pageTitle: activeHeading,
      userFullName: storedUser?.fullName || storedUser?.name || storedUser?.username || '',
      userEmail: storedUser?.email || '',
      userRole: storedUser?.role || '',
      studentIdNumber: storedUser?.idNumber || storedUser?.studentNumber || '',
      workstationCode: pcCode,
      laboratoryName: labName,
      pcSessionId: activeSessionId,
    };
  } catch {
    return {};
  }
};

export const AiApi = {
  chat: async (payload: AiChatPayload): Promise<AiChatNormalizedResult> => {
    const livePageCtx = collectLiveSystemPageContext();
    const systemContext = JSON.stringify({
      role: payload.role || livePageCtx.userRole || 'ADMIN',
      userName: payload.userName || livePageCtx.userFullName || 'LabTrack User',
      userEmail: payload.userEmail || livePageCtx.userEmail || '',
      campus: payload.campusId || 'JRMSU Dapitan Main Campus',
      path: payload.path || livePageCtx.path || (typeof window !== 'undefined' ? window.location.pathname : '/'),
      pageTitle: livePageCtx.pageTitle || '',
      fullUrl: livePageCtx.fullUrl || '',
      workstationCode: livePageCtx.workstationCode || '',
      laboratoryName: livePageCtx.laboratoryName || '',
      studentIdNumber: livePageCtx.studentIdNumber || '',
      pcSessionId: livePageCtx.pcSessionId || '',
      history: payload.history || [],
    });

    try {
      const res = await apiClient.post('/api/v1/ai/chat', {
        prompt: payload.prompt,
        systemContext,
        sessionId: payload.sessionId,
        role: payload.role || 'ADMIN',
        campus_id: payload.campusId || 'Dapitan Main Campus',
        user_email: payload.userEmail || '',
      });
      const data = unwrapApiData<any>(res);
      const text = data?.response || res?.data?.response || '';
      if (text) {
        return {
          sessionId: data?.session_id || data?.sessionId || payload.sessionId || 'default',
          response: String(text),
          processingTimeMs: Number(data?.processingTimeMs || data?.processing_time_ms || 0),
          tokensUsed: Number(data?.tokens_used || data?.tokensUsed || Math.max(24, String(text).split(/\s+/).length)),
          suggestions: Array.isArray(data?.suggestions) ? data.suggestions : [],
        };
      }
    } catch {}

    // Direct Python FastAPI Microservice Fallback (:8000)
    const pyRes = await axios.post(`${getPythonAiBackendUrl()}/api/v1/ai/chat`, {
      prompt: payload.prompt,
      system_context: systemContext,
      session_id: payload.sessionId,
    });
    const pyData = pyRes.data || {};
    return {
      sessionId: pyData.session_id || payload.sessionId || 'default',
      response: String(pyData.response || 'LabTrack AI processed your query.'),
      processingTimeMs: Number(pyData.processing_time_ms || 0),
      tokensUsed: Number(pyData.tokens_used || 32),
      suggestions: Array.isArray(pyData.suggestions) ? pyData.suggestions : [],
    };
  },

  chatQuery: async (payload: AiChatPayload) => AiApi.chat(payload),

  getSessions: async () => {
    const res = await apiClient.get('/api/v1/ai/sessions');
    return unwrapPaginatedItems(res);
  },

  estimateDamageCost: async (payload: {
    problemReportId: string;
    issueDescription: string;
    hardwareModel: string;
  }) => unwrapApiData(await apiClient.post('/api/v1/ai/damage-estimate', payload)),

  predictiveMaintenance: async (payload: { assetId: string; systemUsageHours: number }) =>
    unwrapApiData(await apiClient.post('/api/v1/ai/predictive-maintenance', payload)),

  getHealth: async () => {
    const res = await axios.get(`${getPythonAiBackendUrl()}/api/v1/ai/health`);
    return res.data;
  },
};

export const LaboratoriesApi = {
  getAll: async (params?: PaginationQuery) => unwrapPaginatedItems(await apiClient.get('/api/v1/laboratories', { params })),
  getById: async (id: string) => unwrapApiData(await apiClient.get(`/api/v1/laboratories/${id}`)),
  create: async (payload: any) => unwrapApiData(await apiClient.post('/api/v1/laboratories', payload)),
  update: async (id: string, payload: any) => unwrapApiData(await apiClient.put(`/api/v1/laboratories/${id}`, payload)),
  remove: async (id: string) => unwrapApiData(await apiClient.delete(`/api/v1/laboratories/${id}`)),
  getWorkstations: async (labId: string) =>
    unwrapPaginatedItems(await apiClient.get(`/api/v1/laboratories/${labId}/workstations`)),
};

export const WorkstationsApi = {
  getAll: async (params?: PaginationQuery) => unwrapPaginatedItems(await apiClient.get('/api/v1/workstations', { params })),
  getById: async (id: string) => unwrapApiData(await apiClient.get(`/api/v1/workstations/${id}`)),
  create: async (payload: any) => unwrapApiData(await apiClient.post('/api/v1/workstations', payload)),
  update: async (id: string, payload: any) => unwrapApiData(await apiClient.put(`/api/v1/workstations/${id}`, payload)),
  updateStatus: async (id: string, status: string) =>
    unwrapApiData(await apiClient.patch(`/api/v1/workstations/${id}/status`, { status })),
  remove: async (id: string) => unwrapApiData(await apiClient.delete(`/api/v1/workstations/${id}`)),
  getMaintenanceStatus: async (code: string) =>
    unwrapApiData(await apiClient.get(`/api/v1/maintenance/workstation-status/${encodeURIComponent(code)}`)),
};

export const AssetsApi = {
  getAll: async (params?: PaginationQuery) => unwrapPaginatedItems(await apiClient.get('/api/v1/assets', { params })),
  getById: async (id: string) => unwrapApiData(await apiClient.get(`/api/v1/assets/${id}`)),
  create: async (payload: any) => unwrapApiData(await apiClient.post('/api/v1/assets', payload)),
  update: async (id: string, payload: any) => unwrapApiData(await apiClient.put(`/api/v1/assets/${id}`, payload)),
  remove: async (id: string) => unwrapApiData(await apiClient.delete(`/api/v1/assets/${id}`)),
};

export const PcUsageApi = {
  scanLogin: async (payload: any) => unwrapApiData(await apiClient.post('/api/v1/pc-usage/scan-login', payload)),
  scanLoginAuto: async (payload: any) => unwrapApiData(await apiClient.post('/api/v1/pc-usage/scan-login-auto', payload)),
  heartbeat: async (sessionId: string, extra?: any) =>
    unwrapApiData(await apiClient.post('/api/v1/pc-usage/heartbeat', { sessionId, ...extra })),
  endSession: async (sessionId: string) =>
    unwrapApiData(await apiClient.post(`/api/v1/pc-usage/end-session/${sessionId}`, {})),
  endSessionRelease: async (sessionId: string) =>
    unwrapApiData(await apiClient.post(`/api/v1/pc-usage/end-session-release/${sessionId}`, {})),
  getActive: async (pageNumber = 1, pageSize = 1000) =>
    unwrapPaginatedItems(await apiClient.get('/api/v1/pc-usage/active', { params: { pageNumber, pageSize } })),
  getHistory: async (pageNumber = 1, pageSize = 200) =>
    unwrapPaginatedItems(await apiClient.get('/api/v1/pc-usage/history', { params: { pageNumber, pageSize } })),
};

export const ProblemReportsApi = {
  getAll: async (params?: PaginationQuery) =>
    unwrapPaginatedItems(await apiClient.get('/api/v1/problem-reports', { params })),
  getMine: async () => unwrapPaginatedItems(await apiClient.get('/api/v1/problem-reports/mine')),
  getById: async (id: string) => unwrapApiData(await apiClient.get(`/api/v1/problem-reports/${id}`)),
  create: async (payload: any) => unwrapApiData(await apiClient.post('/api/v1/problem-reports', payload)),
  update: async (id: string, payload: any) => unwrapApiData(await apiClient.put(`/api/v1/problem-reports/${id}`, payload)),
  updateStatus: async (id: string, status: string, notes?: string) =>
    unwrapApiData(await apiClient.patch(`/api/v1/problem-reports/${id}/status`, { status, resolutionNotes: notes, notes })),
  remove: async (id: string) => unwrapApiData(await apiClient.delete(`/api/v1/problem-reports/${id}`)),
};

export const RepairTicketsApi = {
  getAll: async (params?: PaginationQuery) =>
    unwrapPaginatedItems(await apiClient.get('/api/v1/repair-tickets', { params })),
  getById: async (id: string) => unwrapApiData(await apiClient.get(`/api/v1/repair-tickets/${id}`)),
  create: async (payload: any) => unwrapApiData(await apiClient.post('/api/v1/repair-tickets', payload)),
  update: async (id: string, payload: any) => unwrapApiData(await apiClient.put(`/api/v1/repair-tickets/${id}`, payload)),
  remove: async (id: string) => unwrapApiData(await apiClient.delete(`/api/v1/repair-tickets/${id}`)),
};

export const MaintenanceApi = {
  getSchedules: async (params?: PaginationQuery) =>
    unwrapPaginatedItems(await apiClient.get('/api/v1/maintenance/schedules', { params })),
  getScheduleById: async (id: string) => unwrapApiData(await apiClient.get(`/api/v1/maintenance/schedules/${id}`)),
  createSchedule: async (payload: any) => unwrapApiData(await apiClient.post('/api/v1/maintenance/schedules', payload)),
  updateSchedule: async (id: string, payload: any) =>
    unwrapApiData(await apiClient.put(`/api/v1/maintenance/schedules/${id}`, payload)),
  removeSchedule: async (id: string) => unwrapApiData(await apiClient.delete(`/api/v1/maintenance/schedules/${id}`)),
  getRecords: async (mineOnly = false) =>
    unwrapPaginatedItems(await apiClient.get('/api/v1/maintenance/records', { params: mineOnly ? { mine: true } : undefined })),
  createRecord: async (payload: any) => unwrapApiData(await apiClient.post('/api/v1/maintenance/records', payload)),
  updateRecord: async (id: string, payload: any) =>
    unwrapApiData(await apiClient.put(`/api/v1/maintenance/records/${id}`, payload)),
  removeRecord: async (id: string) => unwrapApiData(await apiClient.delete(`/api/v1/maintenance/records/${id}`)),
};

export const ReservationsApi = {
  getAll: async (params?: PaginationQuery) =>
    unwrapPaginatedItems(await apiClient.get('/api/v1/reservations', { params })),
  getById: async (id: string) => unwrapApiData(await apiClient.get(`/api/v1/reservations/${id}`)),
  create: async (payload: any) => unwrapApiData(await apiClient.post('/api/v1/reservations', payload)),
  updateStatus: async (id: string, status: string) =>
    unwrapApiData(await apiClient.patch(`/api/v1/reservations/${id}/status`, { status })),
  cancel: async (id: string) => unwrapApiData(await apiClient.post(`/api/v1/reservations/${id}/cancel`, {})),
};

export const NotificationsApi = {
  getAll: async (params?: any) => unwrapPaginatedItems(await apiClient.get('/api/v1/notifications', { params })),
  create: async (payload: any) => unwrapApiData(await apiClient.post('/api/v1/notifications', payload)),
  markRead: async (id: string) => unwrapApiData(await apiClient.patch(`/api/v1/notifications/${id}/read`, {})),
  markAllRead: async () => unwrapApiData(await apiClient.post('/api/v1/notifications/read-all', {})),
  remove: async (id: string) => unwrapApiData(await apiClient.delete(`/api/v1/notifications/${id}`)),
};

export const UsersApi = {
  getAll: async (params?: PaginationQuery) => unwrapPaginatedItems(await apiClient.get('/api/v1/users', { params })),
  getById: async (id: string) => unwrapApiData(await apiClient.get(`/api/v1/users/${id}`)),
  create: async (payload: any) => unwrapApiData(await apiClient.post('/api/v1/users', payload)),
  update: async (id: string, payload: any) => unwrapApiData(await apiClient.put(`/api/v1/users/${id}`, payload)),
  toggleStatus: async (id: string, isActive: boolean) =>
    unwrapApiData(await apiClient.patch(`/api/v1/users/${id}/status`, { isActive })),
  remove: async (id: string) => unwrapApiData(await apiClient.delete(`/api/v1/users/${id}`)),
};

export const AnalyticsApi = {
  getUsage: async () => unwrapApiData(await apiClient.get('/api/v1/analytics/usage')),
  getAssetStatusSummary: async () => unwrapPaginatedItems(await apiClient.get('/api/v1/analytics/assets/status-summary')),
  getFrequentIssues: async () => unwrapPaginatedItems(await apiClient.get('/api/v1/analytics/issues/frequent')),
};

export const FinancialsApi = {
  getSummary: async () => unwrapApiData(await apiClient.get('/api/v1/financials/summary')),
  getTransactions: async (params?: PaginationQuery) =>
    unwrapPaginatedItems(await apiClient.get('/api/v1/financials/transactions', { params })),
  getTransactionById: async (id: string) => unwrapApiData(await apiClient.get(`/api/v1/financials/transactions/${id}`)),
  createTransaction: async (payload: any) => unwrapApiData(await apiClient.post('/api/v1/financials/transactions', payload)),
};

export const AuditLogsApi = {
  getAll: async (params?: PaginationQuery) => unwrapPaginatedItems(await apiClient.get('/api/v1/audit-logs', { params })),
  getById: async (id: string) => unwrapApiData(await apiClient.get(`/api/v1/audit-logs/${id}`)),
  remove: async (id: string) => unwrapApiData(await apiClient.delete(`/api/v1/audit-logs/${id}`)),
  getChainOfCustody: async () => unwrapPaginatedItems(await apiClient.get('/api/v1/chain-of-custody')),
};

export const NetworkApi = {
  getDevices: async (params?: PaginationQuery) =>
    unwrapPaginatedItems(await apiClient.get('/api/v1/network/devices', { params })),
  getDeviceById: async (id: string) => unwrapApiData(await apiClient.get(`/api/v1/network/devices/${id}`)),
  createDevice: async (payload: any) => unwrapApiData(await apiClient.post('/api/v1/network/devices', payload)),
  updateDevice: async (id: string, payload: any) =>
    unwrapApiData(await apiClient.put(`/api/v1/network/devices/${id}`, payload)),
  removeDevice: async (id: string) => unwrapApiData(await apiClient.delete(`/api/v1/network/devices/${id}`)),
  getSummary: async () => unwrapApiData(await apiClient.get('/api/v1/network/summary')),
  getSystemNetworkInfo: async () => unwrapApiData(await apiClient.get('/api/v1/system/network')),
  getIncidents: async () => unwrapPaginatedItems(await apiClient.get('/api/v1/network-incidents')),
  createIncident: async (payload: any) => unwrapApiData(await apiClient.post('/api/v1/network-incidents', payload)),
  resolveIncident: async (id: string, payload: any) =>
    unwrapApiData(await apiClient.patch(`/api/v1/network-incidents/${id}/resolve`, payload)),
};

export const QrApi = {
  validate: async (qrToken: string) =>
    unwrapApiData(await apiClient.get(`/api/v1/qr/validate/${encodeURIComponent(qrToken)}`)),
  generate: async (payload: any) => unwrapApiData(await apiClient.post('/api/v1/qr/generate', payload)),
  getAdminList: async () => unwrapPaginatedItems(await apiClient.get('/api/v1/admin/qr')),
  removeAdminQr: async (id: string) => unwrapApiData(await apiClient.delete(`/api/v1/admin/qr/${id}`)),
  getSessions: async () => unwrapPaginatedItems(await apiClient.get('/api/v1/qr-sessions')),
  createSession: async (payload: any) => unwrapApiData(await apiClient.post('/api/v1/qr-sessions', payload)),
};

export const ReplacementsApi = {
  getAll: async (params?: PaginationQuery) => unwrapPaginatedItems(await apiClient.get('/api/v1/replacements', { params })),
  getById: async (id: string) => unwrapApiData(await apiClient.get(`/api/v1/replacements/${id}`)),
  create: async (payload: any) => unwrapApiData(await apiClient.post('/api/v1/replacements', payload)),
  updateStatus: async (id: string, status: string) =>
    unwrapApiData(await apiClient.patch(`/api/v1/replacements/${id}/status`, { status })),
  remove: async (id: string) => unwrapApiData(await apiClient.delete(`/api/v1/replacements/${id}`)),
  getRecords: async () => unwrapPaginatedItems(await apiClient.get('/api/v1/replacement-records')),
  createRecord: async (payload: any) => unwrapApiData(await apiClient.post('/api/v1/replacement-records', payload)),
};

export const CommunicationsApi = {
  getAll: async (params?: PaginationQuery) =>
    unwrapPaginatedItems(await apiClient.get('/api/v1/communications', { params })),
  getById: async (id: string) => unwrapApiData(await apiClient.get(`/api/v1/communications/${id}`)),
  send: async (payload: any) => unwrapApiData(await apiClient.post('/api/v1/communications/send', payload)),
  resend: async (id: string) => unwrapApiData(await apiClient.post(`/api/v1/communications/${id}/resend`, {})),
  getInstructorCommunications: async () => unwrapPaginatedItems(await apiClient.get('/api/v1/instructor-communications')),
  createInstructorCommunication: async (payload: any) =>
    unwrapApiData(await apiClient.post('/api/v1/instructor-communications', payload)),
};

export const LabTrackApi = {
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

export default LabTrackApi;