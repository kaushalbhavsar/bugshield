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
      <div className="page-title">
        <div>
          <h1>Dashboard</h1>
          <p className="muted">
            Counts distinguish detections from confirmed disruptive actions. Times are shown in{" "}
            {user.timezone}.
          </p>
        </div>
        <select value={siteId} onChange={(event) => setSiteId(event.target.value)} style={{ maxWidth: 240 }}>
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
          Demo dataset is loaded ({data.demo_count} synthetic events). These records are labelled and
          are not production traffic.
        </div>
      )}
      <div className="grid metrics">
        <div className="card metric">
          Events
          <b>{data.event_count}</b>
        </div>
        <div className="card metric">
          Confirmed disruptive
          <b>{data.disrupted_count}</b>
        </div>
        <div className="card metric">
          Detected
          <b>{data.detected_count}</b>
        </div>
        <div className="card metric">
          Observed
          <b>{data.observed_count}</b>
        </div>
      </div>
      {empty ? (
        <EmptyState title="No audit events yet">
          Create a site, generate an ingestion token, and upload a ModSecurity audit log — or load the
          labelled demo dataset from Settings.
        </EmptyState>
      ) : (
        <div className="columns" style={{ marginTop: "1rem" }}>
          <div className="card">
            <h3>Hourly trend</h3>
            <p className="muted">Blue bars are all events. The lower marker is confirmed disruptive actions.</p>
            <div className="trend" aria-label="event trend">
              {data.trends.map((point) => (
                <i key={point.bucket} style={{ height: `${(point.events / maxTrend) * 100}%` }}>
                  <em style={{ height: `${point.events ? (point.disrupted / point.events) * 100 : 0}%` }} />
                </i>
              ))}
            </div>
          </div>
          <div className="card">
            <h3>Top rules</h3>
            <RankList items={data.top_rules} empty="No rule IDs were present in the selected window." />
          </div>
        </div>
      )}
      {!empty && (
        <div className="columns" style={{ marginTop: "1rem" }}>
          <div className="card">
            <h3>Source IPs</h3>
            <RankList items={data.top_source_ips} empty="No source IPs recorded." />
          </div>
          <div className="card">
            <h3>Targeted URLs</h3>
            <RankList items={data.top_urls} empty="No URLs recorded." />
          </div>
        </div>
      )}
      <p style={{ marginTop: "1rem" }}>
        <Link to="/events">Search events</Link>
      </p>
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
