import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { api } from "../api";
import { EmptyState } from "../components/EmptyState";
import type { Dashboard, Site, User } from "../types";

export function DashboardPage({ user }: { user: User }) {
  const [data, setData] = useState<Dashboard | null>(null);
  const [sites, setSites] = useState<Site[]>([]);
  const [siteId, setSiteId] = useState("");
  const [error, setError] = useState("");

  useEffect(() => {
    api.sites().then((items) => setSites(items as Site[])).catch((err) => setError(String(err)));
  }, []);

  useEffect(() => {
    const params = siteId ? `?site_id=${siteId}` : "";
    api
      .dashboard(params)
      .then((payload) => setData(payload as Dashboard))
      .catch((err) => setError(err instanceof Error ? err.message : "dashboard failed"));
  }, [siteId]);

  if (!data) {
    return <p className="muted">{error || "Loading dashboard…"}</p>;
  }

  const maxTrend = Math.max(1, ...data.trends.map((point) => point.events));
  const empty = data.event_count === 0;

  return (
    <div>
      <div className="page-toolbar">
        <p className="muted">
          Counts distinguish detections from confirmed disruptive actions. Times are shown in {user.timezone}.
        </p>
        <select className="input-bordered" value={siteId} onChange={(event) => setSiteId(event.target.value)} style={{ maxWidth: 240 }}>
          <option value="">All sites</option>
          {sites.map((site) => (
            <option key={site.id} value={site.id}>
              {site.name}
            </option>
          ))}
        </select>
      </div>
      {data.demo_count > 0 && (
        <div className="banner">
          Demo dataset is loaded ({data.demo_count} synthetic events). These records are labelled and are not production traffic.
        </div>
      )}
      <div className="stats">
        <Stat icon="ti-clipboard" tone="warning" label="Events" value={data.event_count} />
        <Stat icon="ti-shield" tone="danger" label="Confirmed disruptive" value={data.disrupted_count} />
        <Stat icon="ti-target" tone="rose" label="Detected" value={data.detected_count} />
        <Stat icon="ti-panel" tone="info" label="Observed" value={data.observed_count} />
      </div>
      {empty ? (
        <EmptyState title="No audit events yet">
          Create a site from Preferences, generate an ingestion token, and upload a ModSecurity audit log — or load the labelled demo dataset.
        </EmptyState>
      ) : (
        <div className="columns">
          <div className="card">
            <div className="header">
              <h4 className="title">Hourly trend</h4>
              <p className="category">All events versus confirmed disruptive actions</p>
            </div>
            <div className="content">
              <div className="trend" aria-label="event trend">
                {data.trends.map((point) => (
                  <div key={point.bucket} className="col" style={{ height: `${(point.events / maxTrend) * 100}%` }} title={point.bucket}>
                    <span className="disrupted" style={{ height: `${point.events ? (point.disrupted / point.events) * 100 : 0}%` }} />
                  </div>
                ))}
              </div>
              <div className="trend-legend">
                <span><i className="swatch events" /> Events</span>
                <span><i className="swatch blocks" /> Confirmed disruptive</span>
              </div>
            </div>
          </div>
          <div className="card">
            <div className="header">
              <h4 className="title">Top rules</h4>
            </div>
            <div className="content">
              <RankList items={data.top_rules} empty="No rule IDs were present in the selected window." />
            </div>
          </div>
        </div>
      )}
      {!empty && (
        <div className="columns even">
          <div className="card">
            <div className="header">
              <h4 className="title">Source IPs</h4>
            </div>
            <div className="content">
              <RankList items={data.top_source_ips} empty="No source IPs recorded." />
            </div>
          </div>
          <div className="card">
            <div className="header">
              <h4 className="title">Targeted URLs</h4>
            </div>
            <div className="content">
              <RankList items={data.top_urls} empty="No URLs recorded." />
            </div>
          </div>
        </div>
      )}
      <p>
        <Link to="/attacks">Search blocked attacks</Link>
      </p>
    </div>
  );
}

function Stat({
  icon,
  tone,
  label,
  value,
}: {
  icon: string;
  tone: string;
  label: string;
  value: number;
}) {
  return (
    <div className="card stat-card">
      <div className={`stat-icon ${tone}`}>
        <i className={icon} />
      </div>
      <div className="card-content">
        <p className="category">{label}</p>
        <h3 className="title">{value}</h3>
      </div>
    </div>
  );
}

function RankList({
  items,
  empty,
}: {
  items: Array<{ name: string; count: number }>;
  empty: string;
}) {
  const max = Math.max(1, ...items.map((item) => item.count));
  if (!items.length) return <p className="muted">{empty}</p>;
  return (
    <div className="bars">
      {items.map((item) => (
        <div key={item.name}>
          <div className="bar">
            <span>{item.name}</span>
            <strong>{item.count}</strong>
          </div>
          <div className="bar-track">
            <div className="bar-fill" style={{ width: `${(item.count / max) * 100}%` }} />
          </div>
        </div>
      ))}
    </div>
  );
}
