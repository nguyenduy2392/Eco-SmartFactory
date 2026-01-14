declare var window: any;

export const AppConfig = {
  getApiUrl(): string {
    const baseUrl = window.__env?.baseApiUrl || 'http://183.81.32.190:8081/';
    // Đảm bảo baseUrl kết thúc bằng '/'
    const normalizedBaseUrl = baseUrl.endsWith('/') ? baseUrl : baseUrl + '/';
    return normalizedBaseUrl + 'api';
  },

  getBaseUrl(): string {
    const baseUrl = window.__env?.baseApiUrl || 'http://183.81.32.190:8081/';
    // Đảm bảo baseUrl kết thúc bằng '/'
    return baseUrl.endsWith('/') ? baseUrl.slice(0, -1) : baseUrl;
  }
};
