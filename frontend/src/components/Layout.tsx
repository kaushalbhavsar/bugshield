import { useEffect, useState } from "react";
import { NavLink, Outlet, useLocation, useNavigate } from "react-router-dom";
import type { User } from "../types";

const TITLES: Record<string, string> = {
  "/": "Dashboard",
  "/dashboard": "Dashboard",
  "/attacks": "Blocked Attacks",
  "/attacks/report": "Reports",
  "/skanjobs": "Vulnerability Scan",
  "/preferences": "Preferences",
  "/preferences/edit": "Preferences",
  "/support": "Help & Support",
  "/manage/change-password": "Change Password",
};

function pageTitle(pathname: string): string {
  if (TITLES[pathname]) return TITLES[pathname];
  if (pathname.startsWith("/attacks/") && pathname !== "/attacks/report") return "Attack Detail";
  return "Dashboard";
}

export function Layout({ user, onLogout }: { user: User; onLogout: () => void }) {
  const location = useLocation();
  const navigate = useNavigate();
  const [mini, setMini] = useState(false);
  const [mobileOpen, setMobileOpen] = useState(false);
  const [menuOpen, setMenuOpen] = useState(false);
  const title = pageTitle(location.pathname);

  useEffect(() => {
    document.title = `BugShield - ${title}`;
    setMobileOpen(false);
    setMenuOpen(false);
  }, [location.pathname, title]);

  return (
    <div className={`wrapper${mini ? " sidebar-mini" : ""}`}>
      <div className={`sidebar${mobileOpen ? " open" : ""}`} data-background-color="white" data-active-color="danger">
        <div className="logo">
          <a href="/dashboard" className="simple-text" onClick={(event) => { event.preventDefault(); navigate("/dashboard"); }}>
            <img src="/assets/img/logo-dark.png" width={170} title="BugShield" alt="BugShield" />
          </a>
        </div>
        <div className="sidebar-wrapper">
          <ul className="nav">
            <li className={location.pathname === "/" || location.pathname === "/dashboard" ? "active" : ""}>
              <NavLink to="/dashboard">
                <i className="ti-panel" />
                <p>Dashboard</p>
              </NavLink>
            </li>
            <li className={location.pathname.startsWith("/attacks") && location.pathname !== "/attacks/report" ? "active" : ""}>
              <NavLink to="/attacks">
                <i className="ti-shield" />
                <p>Blocked Attacks</p>
              </NavLink>
            </li>
            <li className={location.pathname === "/attacks/report" ? "active" : ""}>
              <NavLink to="/attacks/report">
                <i className="ti-clipboard" />
                <p>Reports</p>
              </NavLink>
            </li>
            <li className={location.pathname === "/skanjobs" ? "active" : ""}>
              <NavLink to="/skanjobs">
                <i className="ti-target" />
                <p>Vulnerability Scan</p>
              </NavLink>
            </li>
            <li className={location.pathname.startsWith("/preferences") ? "active" : ""}>
              <NavLink to="/preferences/edit">
                <i className="ti-settings" />
                <p>Preferences</p>
              </NavLink>
            </li>
            <li className={location.pathname === "/support" ? "active" : ""}>
              <NavLink to="/support">
                <i className="ti-help" />
                <p>Help & Support</p>
              </NavLink>
            </li>
          </ul>
        </div>
      </div>
      <div className="main-panel">
        <nav className="navbar navbar-default">
          <div className="container-fluid">
            <div className="navbar-left">
              <button
                type="button"
                className="navbar-toggle"
                aria-label="Toggle navigation"
                onClick={() => setMobileOpen((open) => !open)}
              >
                <i className="ti-menu" />
              </button>
              <button
                id="minimizeSidebar"
                type="button"
                className="btn btn-round btn-white btn-fill btn-just-icon"
                aria-label={mini ? "Expand sidebar" : "Minimize sidebar"}
                onClick={() => setMini((value) => !value)}
              >
                <i className={mini ? "ti-arrow-right" : "ti-arrow-left"} />
              </button>
              <a className="navbar-brand" href={location.pathname}>
                {title}
              </a>
            </div>
            <div className="user-menu">
              <button type="button" className="toggle" onClick={() => setMenuOpen((open) => !open)}>
                <i className="ti-user" />
                <span>{user.display_name || user.email}</span>
                <span className="caret" />
              </button>
              <div className={`dropdown-menu${menuOpen ? " open" : ""}`} role="menu">
                <a href="/manage/change-password" onClick={(event) => { event.preventDefault(); navigate("/manage/change-password"); }}>
                  Change Password
                </a>
                <button type="button" className="linkish" onClick={onLogout}>
                  Sign out
                </button>
              </div>
            </div>
          </div>
        </nav>
        <div className="content">
          <div className="container-fluid">
            <Outlet />
          </div>
        </div>
      </div>
    </div>
  );
}
