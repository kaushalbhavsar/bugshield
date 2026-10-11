import { useEffect, useState } from "react";
import { api } from "../api";
import { EmptyState } from "../components/EmptyState";
import type { Dashboard, Site, User } from "../types";

export function ReportsPage({ user }: { user: User }) {
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
      .catch((err) => setError(err instanceof Error ? err.message : "report failed"));
  }, [siteId]);

  if (!data) return <p className="muted">{error || "Loading report…"}</p>;

  return (
    <div>
      <div className="page-toolbar">
        <p className="muted">
          Report of recorded audit events. Times displayed in {user.timezone}. This view does not change WAF rules.
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
      {data.event_count === 0 ? (
        <EmptyState title="No report data yet">Load events from ingest or the demo dataset in Preferences.</EmptyState>
      ) : (
        <>
          <div className="card">
            <div className="header">
              <h4 className="title">Summary</h4>
            </div>
            <div className="content">
              <table className="table">
                <tbody>
                  <tr>
                    <td>Events</td>
                    <td>{data.event_count}</td>
                  </tr>
                  <tr>
                    <td>Confirmed disruptive</td>
                    <td>{data.disrupted_count}</td>
                  </tr>
                  <tr>
                    <td>Detected</td>
                    <td>{data.detected_count}</td>
                  </tr>
                  <tr>
                    <td>Observed</td>
                    <td>{data.observed_count}</td>
                  </tr>
                  <tr>
                    <td>Demo records</td>
                    <td>{data.demo_count}</td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>
          <div className="columns even">
            <ReportTable title="Top rules" rows={data.top_rules} />
            <ReportTable title="Source IPs" rows={data.top_source_ips} />
          </div>
          <ReportTable title="Targeted URLs" rows={data.top_urls} />
        </>
      )}
    </div>
  );
}

function ReportTable({ title, rows }: { title: string; rows: Array<{ name: string; count: number }> }) {
  return (
    <div className="card">
      <div className="header">
        <h4 className="title">{title}</h4>
      </div>
      <div className="content">
        {rows.length === 0 ? (
          <p className="muted">No rows.</p>
        ) : (
          <table className="table">
            <thead>
              <tr>
                <th>Name</th>
                <th>Count</th>
              </tr>
            </thead>
            <tbody>
              {rows.map((row) => (
                <tr key={row.name}>
                  <td>{row.name}</td>
                  <td>{row.count}</td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>
    </div>
  );
}
