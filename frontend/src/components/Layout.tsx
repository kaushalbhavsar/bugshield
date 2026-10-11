import { useEffect, useState } from "react";
import { NavLink, Outlet, useLocation, useNavigate } from "react-router-dom";
import type { User } from "../types";

function pageTitle(pathname: string): string {
  if (pathname.startsWith("/attacks/today")) return "Attacks blocked today";
  if (pathname.startsWith("/attacks/week")) return "Attacks blocked in last week";
  if (pathname.startsWith("/attacks/month")) return "Attacks blocked in last month";
  if (pathname.startsWith("/attacks/")) return "Attack detail";
  if (pathname.startsWith("/preferences")) return "Update email address to receive notification";
  if (pathname.startsWith("/manage")) return "Change Password";
  if (pathname.startsWith("/support")) return "Help & Support";
  return "Dashboard";
}

export function Layout({ user, onLogout }: { user: User; onLogout: () => void }) {
  const location = useLocation();
  const navigate = useNavigate();
  const [mini, setMini] = useState(false);
  const [mobileOpen, setMobileOpen] = useState(false);
  const [attacksOpen, setAttacksOpen] = useState(location.pathname.startsWith("/attacks"));
  const title = pageTitle(location.pathname);

  useEffect(() => {
    document.title = `BUGSKAN - ${title}`;
    document.body.classList.toggle("sidebar-mini", mini);
    setMobileOpen(false);
    if (location.pathname.startsWith("/attacks")) setAttacksOpen(true);
  }, [location.pathname, title, mini]);

  return (
    <div className="wrapper">
      <div className={`sidebar${mobileOpen ? " open" : ""}`} data-background-color="white" data-active-color="danger">
        <div className="logo">
          <a href="/dashboard" className="simple-text" onClick={(event) => { event.preventDefault(); navigate("/dashboard"); }}>
            <img src="/assets/img/logo.png" width={170} alt="BUGSKAN" />
          </a>
        </div>
        <div className="logo logo-mini">
          <a href="/dashboard" className="simple-text" onClick={(event) => { event.preventDefault(); navigate("/dashboard"); }}>
            <img src="/assets/img/logo-small.png" width={40} alt="BUGSKAN" />
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
            <li className={location.pathname.startsWith("/attacks") ? "active" : ""}>
              <a
                href="#charts"
                className={attacksOpen ? "" : "collapsed"}
                aria-expanded={attacksOpen}
                onClick={(event) => {
                  event.preventDefault();
                  setAttacksOpen((open) => !open);
                }}
              >
                <i className="ti-shield" />
                <p>
                  Blocked Attacks
                  <b className="caret" />
                </p>
              </a>
              <ul className={`collapse-nav${attacksOpen ? " open" : ""}`}>
                <li className={location.pathname === "/attacks/today" ? "active" : ""}>
                  <NavLink to="/attacks/today">Attacks today</NavLink>
                </li>
                <li className={location.pathname === "/attacks/week" ? "active" : ""}>
                  <NavLink to="/attacks/week">Attacks this week</NavLink>
                </li>
                <li className={location.pathname === "/attacks/month" ? "active" : ""}>
                  <NavLink to="/attacks/month">Attacks this month</NavLink>
                </li>
              </ul>
            </li>
            <li className={location.pathname.startsWith("/preferences") ? "active" : ""}>
              <NavLink to="/preferences/edit">
                <i className="ti-settings" />
                <p>Preferences</p>
              </NavLink>
            </li>
            <li className={location.pathname.startsWith("/manage") ? "active" : ""}>
              <NavLink to="/manage/change-password">
                <i className="ti-user" />
                <p>Manage account</p>
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
            <div className="col-lg-8">
              <div className="navbar-minimize">
                <button
                  id="minimizeSidebar"
                  type="button"
                  className="btn btn-round btn-white btn-fill btn-just-icon"
                  onClick={() => setMini((value) => !value)}
                >
                  <i className={mini ? "ti-arrow-right" : "ti-arrow-left"} />
                </button>
              </div>
              <div className="navbar-header">
                <button type="button" className="navbar-toggle" onClick={() => setMobileOpen((open) => !open)}>
                  <span className="sr-only">Toggle navigation</span>
                  <span className="icon-bar" />
                  <span className="icon-bar" />
                  <span className="icon-bar" />
                </button>
                <a className="navbar-brand" href={location.pathname}>
                  {title}
                </a>
              </div>
            </div>
            <div className="col-lg-4 navbar-user" style={{ paddingTop: 20 }}>
              <a href="/manage/change-password" title="Manage" style={{ color: "#000000" }} onClick={(event) => { event.preventDefault(); navigate("/manage/change-password"); }}>
                {user.email}
              </a>
              <a className="signout" href="#logout" onClick={(event) => { event.preventDefault(); onLogout(); }}>
                [ <b><i>Sign Out</i></b> ]
              </a>
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
