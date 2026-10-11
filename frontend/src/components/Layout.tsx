import { NavLink, Outlet } from "react-router-dom";
import type { User } from "../types";

export function Layout({ user, onLogout }: { user: User; onLogout: () => void }) {
  return (
    <div className="shell">
      <aside className="nav">
        <div className="brand">
          <div className="mark">B</div>
          <div>
            <strong>BugShield</strong>
            <small>ModSecurity event console</small>
          </div>
        </div>
        <NavLink to="/" end>
          Dashboard
        </NavLink>
        <NavLink to="/events">Events</NavLink>
        <NavLink to="/sites">Sites</NavLink>
        <NavLink to="/settings">Settings</NavLink>
        <div className="muted" style={{ marginTop: "2rem", fontSize: "0.85rem" }}>
          {user.display_name}
          <br />
          {user.email}
        </div>
        <button className="secondary" style={{ marginTop: "0.8rem", width: "100%" }} onClick={onLogout}>
          Sign out
        </button>
      </aside>
      <main className="main">
        <Outlet />
      </main>
    </div>
  );
}
