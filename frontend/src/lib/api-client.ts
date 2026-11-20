import axios, { AxiosError, InternalAxiosRequestConfig } from 'axios';

const ConfiguredBaseUrl = (import.meta.env.VITE_API_BASE_URL || '').trim();

export const getDynamicBackendBaseUrl = (): string => {
  if (ConfiguredBaseUrl) return ConfiguredBaseUrl;
  if (typeof window !== 'undefined') {
    return '';
  }
  return 'http://localhost:5000';
};

export const getDirectBackendUrl = (): string => {
  if (ConfiguredBaseUrl) return ConfiguredBaseUrl;
  if (typeof window !== 'undefined') {
    return `${window.location.protocol}//${window.location.hostname}:5000`;
  }
  return 'http://localhost:5000';
};

export const getPythonAiBackendUrl = (): string => {
  const configuredAi = (import.meta.env.VITE_AI_SERVICE_URL || '').trim();
  if (configuredAi) return configuredAi;
  if (typeof window !== 'undefined') {
    return `${window.location.protocol}//${window.location.hostname}:8000`;
  }
  return 'http://localhost:8000';
};

const getStoredUserId = (): string => {
  try {
    const raw = sessionStorage.getItem('labtrack_user') || localStorage.getItem('labtrack_user');
    if (raw) {
      const parsed = JSON.parse(raw);
      if (parsed?.id) return String(parsed.id);
    }
  } catch {}
  return '00000000-0000-0000-0000-000000000001';
};

export const collectLiveUiUxContext = (overrideRole?: string) => {
  if (typeof window === 'undefined') return {};
  try {
    const path = window.location.pathname || '/';
    const search = window.location.search || '';
    const params = new URLSearchParams(search);

    let storedUser: any = {};
    const rawUser =
      sessionStorage.getItem('labtrack_user') ||
      localStorage.getItem('labtrack_user') ||
      localStorage.getItem('jrmsu_labtrack_user');
    if (rawUser) {
      try { storedUser = JSON.parse(rawUser); } catch {}
    }

    let pcCode =
      params.get('pc') ||
      params.get('workstation') ||
      params.get('ws') ||
      sessionStorage.getItem('labtrack_workstation') ||
      localStorage.getItem('labtrack_workstation') ||
      '';
    let labName =
      params.get('lab') ||
      params.get('laboratory') ||
      sessionStorage.getItem('labtrack_lab') ||
      localStorage.getItem('labtrack_lab') ||
      '';

    for (const store of [sessionStorage, localStorage]) {
      for (let i = 0; i < store.length; i++) {
        const k = store.key(i) || '';
        if (/session|pc|workstation|student/i.test(k)) {
          const val = store.getItem(k) || '';
          if (val.startsWith('{')) {
            try {
              const obj = JSON.parse(val);
              if (!pcCode && (obj.workstationCode || obj.workstationNumber || obj.pcName || obj.code)) {
                pcCode = String(obj.workstationCode || obj.workstationNumber || obj.pcName || obj.code);
              }
              if (!labName && (obj.labName || obj.laboratoryName || obj.roomName)) {
                labName = String(obj.labName || obj.laboratoryName || obj.roomName);
              }
            } catch {}
          }
        }
      }
    }

    const headings = Array.from(document.querySelectorAll('h1, h2, h3'))
      .map((el) => (el.textContent || '').trim().replace(/\s+/g, ' '))
      .filter((t) => t.length > 2 && !/labtrack ai assistant/i.test(t))
      .slice(0, 6);

    const activeButtons = Array.from(document.querySelectorAll('button, a'))
      .filter((el) => {
        const cls = (el.className || '').toString();
        return /bg-blue|bg-indigo|bg-emerald|bg-\[#1e5352\]|border-blue|font-black/i.test(cls);
      })
      .map((el) => (el.textContent || '').trim().replace(/\s+/g, ' '))
      .filter((t) => t.length > 2 && t.length < 40)
      .slice(0, 6);

    const visibleText = (document.body?.innerText || '').replace(/\s+/g, ' ');
    if (!pcCode) {
      const m = visibleText.match(/\b(WS-\d+|PC-\d+)\b/i);
      if (m) pcCode = m[1].toUpperCase();
    }
    if (!labName) {
      const m = visibleText.match(/\b(Main Lab|ComLab\s*\d+|IT Lab\s*\w+)\b/i);
      if (m) labName = m[1];
    }

    const detectedRole =
      overrideRole ||
      (path.startsWith('/admin')
        ? 'ADMIN'
        : path.startsWith('/instructor')
        ? 'INSTRUCTOR'
        : path.startsWith('/student')
        ? 'STUDENT'
        : storedUser?.role || 'GUEST');

    return {
      role: detectedRole,
      userName: detectedRole === 'GUEST' ? '' : (storedUser?.fullName || storedUser?.name || storedUser?.username || ''),
      userEmail: storedUser?.email || '',
      studentIdNumber: storedUser?.idNumber || storedUser?.studentNumber || '',
      campus: 'JRMSU Dapitan Main Campus',
      path,
      fullUrl: window.location.href,
      pageTitle: headings[0] || document.title || 'LabTrack System Page',
      uiHeadings: headings,
      activeUiTabs: activeButtons,
      workstationCode: detectedRole === 'GUEST' ? '' : (pcCode || 'WS-02'),
      laboratoryName: detectedRole === 'GUEST' ? '' : (labName || 'Main Lab (RM-100)'),
    };
  } catch {
    return {};
  }
};

export const apiClient = axios.create({
  baseURL: getDynamicBackendBaseUrl(),
  headers: {
    'Content-Type': 'application/json',
    Accept: 'application/json',
  },
  timeout: 25000,
});

apiClient.interceptors.request.use(
  (config: InternalAxiosRequestConfig) => {
    const token =
      sessionStorage.getItem('labtrack_token') ||
      localStorage.getItem('labtrack_token') ||
      localStorage.getItem('labtrack_access_token') ||
      localStorage.getItem('jrmsu_labtrack_auth_token');

    if (token && config.headers) {
      config.headers.Authorization = `Bearer ${token}`;
    }

    if (config.url) {
      // 1. Automatically enrich ALL AI Chat calls (Global, Admin, Instructor) with live UI/UX DOM context
      if (config.url.includes('/api/v1/ai/chat') && config.method?.toLowerCase() === 'post') {
        const body = typeof config.data === 'string' ? JSON.parse(config.data || '{}') : (config.data || {});
        let parsedCtx: any = {};
        if (body.systemContext) {
          try {
            parsedCtx = typeof body.systemContext === 'string' ? JSON.parse(body.systemContext) : body.systemContext;
          } catch {}
        }
        const liveUi = collectLiveUiUxContext(body.role || parsedCtx.role);
        const mergedCtx = { ...liveUi, ...parsedCtx, uiHeadings: liveUi.uiHeadings, activeUiTabs: liveUi.activeUiTabs };
        config.data = {
          ...body,
          prompt: body.prompt || body.message || body.query || '',
          role: mergedCtx.role || 'ADMIN',
          systemContext: JSON.stringify(mergedCtx),
        };
      }

      // 2. Notifications route normalization
      if (config.url === '/api/v1/notifications/broadcast' && config.method?.toLowerCase() === 'post') {
        config.url = '/api/v1/notifications';
        const body = typeof config.data === 'string' ? JSON.parse(config.data || '{}') : (config.data || {});
        config.data = {
          userId: body.userId || getStoredUserId(),
          title: body.title || body.subject || 'System Broadcast',
          message: body.message || body.details || body.content || 'System announcement',
          type: body.type || body.severity || 'INFO',
        };
      } else if (config.url === '/api/v1/admin/notifications') {
        config.url = '/api/v1/notifications';
      } else if (config.url === '/api/v1/notifications/mark-all-read') {
        config.url = '/api/v1/notifications/read-all';
      }

      // 3. PC Usage session delete -> end-session-release normalization
      const pcUsageDeleteMatch = config.url.match(/^\/api\/v1\/pc-usage\/([0-9a-fA-F-]{36})$/);
      if (pcUsageDeleteMatch && config.method?.toLowerCase() === 'delete') {
        config.method = 'post';
        config.url = `/api/v1/pc-usage/end-session-release/${pcUsageDeleteMatch[1]}`;
        config.data = {};
      }

      // 4. Users /me route normalization
      if (config.url === '/api/v1/users/me') {
        config.url = `/api/v1/users/${getStoredUserId()}`;
      }

      // 5. Heartbeat normalizer
      if (config.url.includes('/api/v1/pc-usage/heartbeat') && config.method?.toLowerCase() === 'post') {
        const hbBody = typeof config.data === 'string' ? JSON.parse(config.data || '{}') : (config.data || {});
        config.data = {
          sessionId: hbBody.sessionId || '00000000-0000-0000-0000-000000000001',
          qrToken: hbBody.qrToken || 'AUTO',
          workstationCode: hbBody.workstationCode || 'PC-001',
        };
      }
    }

    return config;
  },
  (error) => Promise.reject(error)
);

apiClient.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const originalRequest = error.config as InternalAxiosRequestConfig & {
      _directFallback?: boolean;
      _pyFallback?: boolean;
    };

    if (
      originalRequest &&
      !originalRequest._directFallback &&
      (!error.response || error.response.status === 502 || error.response.status === 504) &&
      !originalRequest.baseURL
    ) {
      originalRequest._directFallback = true;
      originalRequest.baseURL = getDirectBackendUrl();
      return apiClient(originalRequest);
    }

    if (
      originalRequest &&
      !originalRequest._pyFallback &&
      (originalRequest.url || '').includes('/api/v1/ai/chat')
    ) {
      originalRequest._pyFallback = true;
      try {
        const rawData = typeof originalRequest.data === 'string'
          ? JSON.parse(originalRequest.data || '{}')
          : (originalRequest.data || {});
        const pyRes = await axios.post(`${getPythonAiBackendUrl()}/api/v1/ai/chat`, {
          prompt: rawData.prompt || rawData.message || rawData.query || 'Status check',
          system_context: rawData.systemContext || rawData.system_context || JSON.stringify(collectLiveUiUxContext(rawData.role)),
          session_id: rawData.sessionId || rawData.session_id,
        });
        return {
          ...pyRes,
          data: {
            success: true,
            data: pyRes.data,
            response: pyRes.data?.response,
          },
        };
      } catch {}
    }

    return Promise.reject(error);
  }
);

export const unwrapApiData = <T = any>(res: any): T => {
  if (res?.data?.data !== undefined) return res.data.data as T;
  if (res?.data !== undefined) return res.data as T;
  return res as T;
};

export const unwrapPaginatedItems = <T = any>(res: any): T[] => {
  const payload = unwrapApiData<any>(res);
  if (Array.isArray(payload)) return payload;
  if (Array.isArray(payload?.items)) return payload.items;
  return [];
};

export default apiClient;