import { useEffect, useState } from "react";
import { Navigate, Route, Routes } from "react-router-dom";
import { api } from "./api";
import { Layout } from "./components/Layout";
import { DashboardPage } from "./pages/Dashboard";
import { EventDetailPage } from "./pages/EventDetail";
import { EventsPage } from "./pages/Events";
import { Login } from "./pages/Login";
import { SettingsPage } from "./pages/Settings";
import { Setup } from "./pages/Setup";
import { SitesPage } from "./pages/Sites";
import type { User } from "./types";

export function App() {
  const [needsSetup, setNeedsSetup] = useState<boolean | null>(null);
  const [user, setUser] = useState<User | null>(null);
  const [loading, setLoading] = useState(true);

  async function bootstrap() {
    setLoading(true);
    const status = await api.setupStatus();
    setNeedsSetup(status.needs_setup);
    if (!status.needs_setup) {
      try {
        setUser((await api.me()) as User);
      } catch {
        setUser(null);
      }
    } else {
      setUser(null);
    }
    setLoading(false);
  }

  useEffect(() => {
    bootstrap().catch(() => setLoading(false));
  }, []);

  if (loading || needsSetup === null) {
    return <p className="muted" style={{ padding: "2rem" }}>Starting BugShield…</p>;
  }
  if (needsSetup) {
    return <Setup onReady={bootstrap} />;
  }
  if (!user) {
    return <Login onReady={bootstrap} />;
  }

  return (
    <Routes>
      <Route
        element={
          <Layout
            user={user}
            onLogout={async () => {
              await api.logout();
              setUser(null);
            }}
          />
        }
      >
        <Route path="/" element={<DashboardPage user={user} />} />
        <Route path="/events" element={<EventsPage user={user} />} />
        <Route path="/events/:eventId" element={<EventDetailPage user={user} />} />
        <Route path="/sites" element={<SitesPage />} />
        <Route path="/settings" element={<SettingsPage user={user} onUser={bootstrap} />} />
        <Route path="*" element={<Navigate to="/" replace />} />
      </Route>
    </Routes>
  );
}
