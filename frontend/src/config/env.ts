export interface EnvironmentConfig {
  apiBaseUrl: string;
  aiServiceUrl: string;
  signalRHubUrl: string;
  lanIp: string;
  publicOrigin: string;
  defaultCampus: string;
  heartbeatIntervalMs: number;
  enableDebugLogs: boolean;
  isProduction: boolean;
}

const getEnvVar = (key: string, defaultValue: string = ''): string => {
  if (typeof window !== 'undefined' && (window as any).__ENV__ && (window as any).__ENV__[key]) {
    return (window as any).__ENV__[key];
  }
  return (import.meta.env as any)[key] || defaultValue;
};

const detectedLanIp = typeof __LAN_IP__ !== 'undefined' ? __LAN_IP__ : 'localhost';

const resolveDynamicOrigin = (): string => {
  if (typeof window === 'undefined') return `http://${detectedLanIp}:3000`;
  const { protocol, hostname, port } = window.location;
  const effectiveHost = (hostname === 'localhost' || hostname === '127.0.0.1') && detectedLanIp !== 'localhost'
    ? detectedLanIp
    : hostname;
  return `${protocol}//${effectiveHost}${port ? ':' + port : ''}`;
};

export const env: EnvironmentConfig = {
  // Empty string uses same-origin Vite proxy (/api, /hubs) which works across all LAN devices;
  // falls back to direct port 5000 if VITE_API_BASE_URL is explicitly set.
  apiBaseUrl: getEnvVar('VITE_API_BASE_URL', ''),
  aiServiceUrl: getEnvVar(
    'VITE_AI_SERVICE_URL',
    typeof window !== 'undefined' ? `${window.location.protocol}//${window.location.hostname}:8000` : 'http://localhost:8000'
  ),
  signalRHubUrl: getEnvVar('VITE_SIGNALR_HUB_URL', '/hubs/labtrack'),
  lanIp: detectedLanIp,
  publicOrigin: resolveDynamicOrigin(),
  defaultCampus: getEnvVar('VITE_DEFAULT_CAMPUS', 'DAPITAN'),
  heartbeatIntervalMs: Number(getEnvVar('VITE_HEARTBEAT_INTERVAL_MS', '30000')),
  enableDebugLogs: getEnvVar('VITE_ENABLE_DEBUG_LOGS', 'false') === 'true',
  isProduction: import.meta.env.PROD,
};