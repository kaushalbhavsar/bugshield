import { useEffect, useState } from "react";
import { api } from "../api";
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

  if (!data) return <p className="muted">{error || "Loading dashboard…"}</p>;
  const siteName = sites.find((site) => site.id === siteId)?.name || "all sites";
  const maxTrend = Math.max(1, ...data.trends.map((point) => point.events));

  return (
    <div className="row">
      <div className="col-md-12">
        <h4 className="title">
          Events on <b>{siteName}</b>
          <select className="form-control" value={siteId} onChange={(event) => setSiteId(event.target.value)} style={{ maxWidth: 240, display: "inline-block", marginLeft: 12 }}>
            <option value="">All sites</option>
            {sites.map((site) => (
              <option key={site.id} value={site.id}>
                {site.name}
              </option>
            ))}
          </select>
        </h4>
        <p className="muted">Displayed in {user.timezone}. Counts distinguish detections from confirmed disruptive actions.</p>
        {data.demo_count > 0 && (
          <div className="banner">Demo dataset is loaded ({data.demo_count} synthetic events). These records are labelled and are not production traffic.</div>
        )}
      </div>

      <div className="col-lg-3 col-md-6 col-sm-6">
        <StatCard icon="ti-stats-up" tone="warning" label="Events" value={data.event_count} footer="Recorded audit events" />
      </div>
      <div className="col-lg-3 col-md-6 col-sm-6">
        <StatCard icon="ti-receipt" tone="danger" label="Confirmed disruptive" value={data.disrupted_count} footer="Disruptive action in the log" />
      </div>
      <div className="col-lg-3 col-md-6 col-sm-6">
        <StatCard icon="ti-shield" tone="success" label="Detected" value={data.detected_count} footer="Matched without a confirmed block" />
      </div>
      <div className="col-lg-3 col-md-6 col-sm-6">
        <StatCard icon="ti-panel" tone="info" label="Observed" value={data.observed_count} footer="No matched-rule evidence" />
      </div>

      <div className="col-md-12">
        <div className="card" style={{ height: 420 }}>
          <div className="content">
            <h4 className="title">Attacks blocked in the last 7 days</h4>
            {data.trends.length === 0 ? (
              <p className="muted">No Attacks in the last 7 days</p>
            ) : (
              <div className="chart-line" aria-label="attacks blocked">
                <svg viewBox="0 0 600 280" preserveAspectRatio="none">
                  <polyline
                    fill="none"
                    stroke="#d40000"
                    strokeWidth="3"
                    points={data.trends
                      .map((point, index) => {
                        const x = (index / Math.max(1, data.trends.length - 1)) * 580 + 10;
                        const y = 260 - (point.disrupted / maxTrend) * 240;
                        return `${x},${y}`;
                      })
                      .join(" ")}
                  />
                </svg>
              </div>
            )}
          </div>
        </div>
      </div>

      <div className="col-lg-6 col-md-12">
        <div className="card" style={{ height: 420 }}>
          <div className="content">
            <h4 className="title">Attacks types</h4>
            <OutcomePie data={data} />
          </div>
        </div>
      </div>
      <div className="col-lg-6 col-md-12">
        <div className="card" style={{ height: 420 }}>
          <div className="content">
            <h4 className="title">Top 10 Target Urls</h4>
            {data.top_urls.length === 0 ? (
              <p className="muted">No Attacks in the last 7 days</p>
            ) : (
              data.top_urls.slice(0, 10).map((item) => {
                const max = Math.max(1, data.top_urls[0].count);
                return (
                  <div className="bar-row" key={item.name}>
                    <span title={item.name}>{item.name.length > 25 ? `${item.name.slice(0, 25)}…` : item.name}</span>
                    <div className="track">
                      <div className="fill" style={{ width: `${(item.count / max) * 100}%` }} />
                    </div>
                    <strong>{item.count}</strong>
                  </div>
                );
              })
            )}
          </div>
        </div>
      </div>
    </div>
  );
}

function StatCard({
  icon,
  tone,
  label,
  value,
  footer,
}: {
  icon: string;
  tone: string;
  label: string;
  value: number;
  footer: string;
}) {
  return (
    <div className="card">
      <div className="content">
        <div className="row">
          <div className="col-xs-5">
            <div className={`icon-big icon-${tone} text-center`}>
              <i className={icon} />
            </div>
          </div>
          <div className="col-xs-7">
            <div className="numbers">
              <p>{label}</p>
              {value}
            </div>
          </div>
        </div>
      </div>
      <div className="card-footer">
        <div className="stats">
          <i className="ti-reload" /> {footer}
        </div>
      </div>
    </div>
  );
}

function OutcomePie({ data }: { data: Dashboard }) {
  const slices = [
    { name: "Confirmed disruptive", value: data.disrupted_count, color: "#d40000" },
    { name: "Detected", value: data.detected_count, color: "#ff9800" },
    { name: "Observed", value: data.observed_count, color: "#9a9a9a" },
  ];
  const total = slices.reduce((sum, slice) => sum + slice.value, 0);
  if (!total) return <p className="muted">No Attacks in the last 7 days</p>;
  let angle = 0;
  const paths = slices.map((slice) => {
    const sweep = (slice.value / total) * Math.PI * 2;
    const start = angle;
    angle += sweep;
    const x1 = 80 + Math.cos(start) * 70;
    const y1 = 80 + Math.sin(start) * 70;
    const x2 = 80 + Math.cos(angle) * 70;
    const y2 = 80 + Math.sin(angle) * 70;
    const large = sweep > Math.PI ? 1 : 0;
    return { ...slice, d: `M80,80 L${x1},${y1} A70,70 0 ${large} 1 ${x2},${y2} Z` };
  });
  return (
    <div className="chart-pie">
      <svg viewBox="0 0 280 200">
        {paths.map((slice) => (
          <path key={slice.name} d={slice.d} fill={slice.color} />
        ))}
      </svg>
      {slices.map((slice) => (
        <div key={slice.name} className="muted">
          <span style={{ display: "inline-block", width: 10, height: 10, background: slice.color, marginRight: 6 }} />
          {slice.name}: {slice.value}
        </div>
      ))}
    </div>
  );
}
