import { useEffect, useState } from "react";
import { Navigate, Route, Routes, useParams } from "react-router-dom";
import { api } from "./api";
import { Layout } from "./components/Layout";
import { AttackDetailPage } from "./pages/AttackDetail";
import { AttacksPage } from "./pages/Attacks";
import { ChangePasswordPage } from "./pages/ChangePassword";
import { DashboardPage } from "./pages/Dashboard";
import { ForgotPassword } from "./pages/ForgotPassword";
import { Login } from "./pages/Login";
import { PreferencesPage } from "./pages/Preferences";
import { Setup } from "./pages/Setup";
import { SupportPage } from "./pages/Support";
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
    return <p className="boot">Starting BugShield…</p>;
  }
  if (needsSetup) {
    return <Setup onReady={bootstrap} />;
  }
  if (!user) {
    return (
      <Routes>
        <Route path="/account/forgot-password" element={<ForgotPassword />} />
        <Route path="*" element={<Login onReady={bootstrap} />} />
      </Routes>
    );
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
        <Route path="/dashboard" element={<DashboardPage user={user} />} />
        <Route path="/attacks" element={<Navigate to="/attacks/today" replace />} />
        <Route path="/attacks/today" element={<AttacksPage user={user} range="today" />} />
        <Route path="/attacks/week" element={<AttacksPage user={user} range="week" />} />
        <Route path="/attacks/month" element={<AttacksPage user={user} range="month" />} />
        <Route path="/attacks/:eventId" element={<AttackDetailPage user={user} />} />
        <Route path="/preferences" element={<PreferencesPage user={user} onUser={bootstrap} />} />
        <Route path="/preferences/edit" element={<PreferencesPage user={user} onUser={bootstrap} />} />
        <Route path="/support" element={<SupportPage />} />
        <Route path="/home/support" element={<SupportPage />} />
        <Route path="/manage/change-password" element={<ChangePasswordPage />} />
        <Route path="/events" element={<Navigate to="/attacks/today" replace />} />
        <Route path="/events/:eventId" element={<EventRedirect />} />
        <Route path="/sites" element={<Navigate to="/preferences/edit" replace />} />
        <Route path="/settings" element={<Navigate to="/preferences/edit" replace />} />
        <Route path="*" element={<Navigate to="/dashboard" replace />} />
      </Route>
    </Routes>
  );
}

function EventRedirect() {
  const { eventId } = useParams();
  return <Navigate to={`/attacks/${eventId}`} replace />;
}
