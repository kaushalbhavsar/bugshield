function csrfToken(): string {
  const match = document.cookie.match(/(?:^|; )bugshield_csrf=([^;]+)/);
  return match ? decodeURIComponent(match[1]) : "";
}

async function request<T>(path: string, init: RequestInit = {}): Promise<T> {
  const headers = new Headers(init.headers);
  if (!headers.has("X-CSRF-Token")) {
    headers.set("X-CSRF-Token", csrfToken());
  }
  if (init.body && !(init.body instanceof FormData) && !headers.has("Content-Type")) {
    headers.set("Content-Type", "application/json");
  }
  const response = await fetch(path, { ...init, headers, credentials: "include" });
  if (!response.ok) {
    let detail = response.statusText;
    try {
      const payload = await response.json();
      detail = payload.detail || JSON.stringify(payload);
    } catch {
      /* keep status text */
    }
    throw new Error(typeof detail === "string" ? detail : JSON.stringify(detail));
  }
  if (response.status === 204) {
    return undefined as T;
  }
  return (await response.json()) as T;
}

export const api = {
  setupStatus: () => request<{ needs_setup: boolean }>("/api/setup/status"),
  setup: (body: object) => request("/api/setup", { method: "POST", body: JSON.stringify(body) }),
  login: (body: object) => request("/api/auth/login", { method: "POST", body: JSON.stringify(body) }),
  logout: () => request("/api/auth/logout", { method: "POST" }),
  me: () => request("/api/auth/me"),
  updateMe: (body: object) => request("/api/auth/me", { method: "PATCH", body: JSON.stringify(body) }),
  changePassword: (body: object) =>
    request("/api/auth/password", { method: "POST", body: JSON.stringify(body) }),
  sites: () => request("/api/sites"),
  createSite: (body: object) => request("/api/sites", { method: "POST", body: JSON.stringify(body) }),
  updateSite: (id: string, body: object) =>
    request(`/api/sites/${id}`, { method: "PATCH", body: JSON.stringify(body) }),
  deleteSite: (id: string) => request(`/api/sites/${id}`, { method: "DELETE" }),
  tokens: (siteId: string) => request(`/api/sites/${siteId}/tokens`),
  createToken: (siteId: string, label: string) =>
    request(`/api/sites/${siteId}/tokens`, { method: "POST", body: JSON.stringify({ label }) }),
  revokeToken: (siteId: string, tokenId: string) =>
    request(`/api/sites/${siteId}/tokens/${tokenId}`, { method: "DELETE" }),
  dashboard: (params: string) => request(`/api/dashboard${params}`),
  events: (params: string) => request(`/api/events${params}`),
  event: (id: string) => request(`/api/events/${id}`),
  upload: async (siteId: string, file: File) => {
    const body = new FormData();
    body.append("file", file);
    return request(`/api/sites/${siteId}/uploads`, { method: "POST", body });
  },
  settings: () => request("/api/settings"),
  saveSettings: (body: object) => request("/api/settings", { method: "PUT", body: JSON.stringify(body) }),
  loadDemo: () => request("/api/settings/demo", { method: "POST" }),
  clearDemo: () => request("/api/settings/demo", { method: "DELETE" }),
  runRetention: () => request("/api/settings/retention/run", { method: "POST" }),
};
