import * as signalR from '@microsoft/signalr';
import { env } from '@/config/env';

export const SIGNALR_ENDPOINTS = {
  LABTRACK_HUB: '/hubs/labtrack',
  AICHAT_HUB: '/hubs/aichat',
  SESSION_HUB: '/hubs/session',
  TELEMETRY_HUB: '/hubs/labtrack',
  ATTENDANCE_HUB: '/hubs/session',
  ALERT_HUB: '/hubs/labtrack',
} as const;

export type SignalRHubEndpoint = typeof SIGNALR_ENDPOINTS[keyof typeof SIGNALR_ENDPOINTS];

export const resolveSignalRHubUrl = (endpoint: string = SIGNALR_ENDPOINTS.LABTRACK_HUB): string => {
  const cleanEndpoint = endpoint.startsWith('/') ? endpoint : `/${endpoint}`;
  if (env.apiBaseUrl) {
    return `${env.apiBaseUrl.replace(/\/+$/, '')}${cleanEndpoint}`;
  }
  if (typeof window !== 'undefined') {
    return `${window.location.origin}${cleanEndpoint}`;
  }
  return `http://localhost:5000${cleanEndpoint}`;
};

export const createSignalRConnection = (
  endpoint: SignalRHubEndpoint | string = SIGNALR_ENDPOINTS.LABTRACK_HUB
): signalR.HubConnection => {
  const hubUrl = resolveSignalRHubUrl(endpoint);

  return new signalR.HubConnectionBuilder()
    .withUrl(hubUrl, {
      accessTokenFactory: () => {
        return (
          sessionStorage.getItem('labtrack_token') ||
          localStorage.getItem('labtrack_token') ||
          localStorage.getItem('jrmsu_labtrack_auth_token') ||
          ''
        );
      },
      transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.LongPolling,
      skipNegotiation: false,
    })
    .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
    .configureLogging(env.enableDebugLogs ? signalR.LogLevel.Information : signalR.LogLevel.None)
    .build();
};

export default createSignalRConnection;