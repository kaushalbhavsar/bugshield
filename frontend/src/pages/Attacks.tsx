import { useEffect, useMemo, useState } from "react";
import { Link } from "react-router-dom";
import { api } from "../api";
import { formatTime, relativeTime } from "../time";
import type { EventSummary, Site, User } from "../types";

type Page = { items: EventSummary[]; total: number; page: number; page_size: number };
type Range = "today" | "week" | "month";

function rangeParams(range: Range): string {
  const to = new Date();
  const from = new Date(to);
  if (range === "today") from.setUTCDate(from.getUTCDate() - 1);
  if (range === "week") from.setUTCDate(from.getUTCDate() - 7);
  if (range === "month") from.setUTCDate(from.getUTCDate() - 30);
  const params = new URLSearchParams();
  params.set("occurred_from", from.toISOString());
  params.set("occurred_to", to.toISOString());
  params.set("page_size", "25");
  return `?${params.toString()}`;
}

export function AttacksPage({ user, range }: { user: User; range: Range }) {
  const [sites, setSites] = useState<Site[]>([]);
  const [page, setPage] = useState<Page>({ items: [], total: 0, page: 1, page_size: 25 });
  const [siteId, setSiteId] = useState("");
  const [sort, setSort] = useState<"time" | "source" | "method" | "url" | "type">("time");
  const [error, setError] = useState("");

  useEffect(() => {
    api.sites().then((items) => setSites(items as Site[])).catch((err) => setError(String(err)));
  }, []);

  useEffect(() => {
    const params = new URLSearchParams(rangeParams(range));
    if (siteId) params.set("site_id", siteId);
    api
      .events(`?${params.toString()}`)
      .then((payload) => setPage(payload as Page))
      .catch((err) => setError(err instanceof Error ? err.message : "search failed"));
  }, [range, siteId]);

  const items = useMemo(() => {
    const copy = [...page.items];
    copy.sort((a, b) => {
      if (sort === "source") return (a.source_ip || "").localeCompare(b.source_ip || "");
      if (sort === "method") return a.method.localeCompare(b.method);
      if (sort === "url") return a.uri.localeCompare(b.uri);
      if (sort === "type") return a.outcome.localeCompare(b.outcome);
      return b.occurred_at.localeCompare(a.occurred_at);
    });
    return copy;
  }, [page.items, sort]);

  const windowLabel = range === "today" ? "24 hours" : range === "week" ? "7 days" : "30 days";
  const siteName = sites.find((site) => site.id === siteId)?.hostname || sites.find((site) => site.id === siteId)?.name || "all sites";

  return (
    <div>
      <div className="col-md-12">
        <h4 className="title">
          {page.total} attacks blocked on <b>{siteName}</b> in last <b>{windowLabel}</b>
        </h4>
        <select className="form-control" value={siteId} onChange={(event) => setSiteId(event.target.value)} style={{ maxWidth: 260 }}>
          <option value="">All sites</option>
          {sites.map((site) => (
            <option key={site.id} value={site.id}>
              {site.name}
            </option>
          ))}
        </select>
      </div>
      <div className="card">
        <div className="content">
          Sort by{" "}
          <button type="button" className="btn btn-primary btn-sm" onClick={() => setSort("time")}>
            <i className="ti ti-time" /> Time of attack
          </button>{" "}
          <button type="button" className="btn btn-success btn-sm" onClick={() => setSort("source")}>
            <i className="ti ti-world" /> Source of attack
          </button>{" "}
          <button type="button" className="btn btn-danger btn-sm" onClick={() => setSort("method")}>
            <i className="ti ti-direction" /> Request type
          </button>{" "}
          <button type="button" className="btn btn-info btn-sm" onClick={() => setSort("url")}>
            <i className="ti ti-link" /> Url attacked
          </button>{" "}
          <button type="button" className="btn btn-warning btn-sm" onClick={() => setSort("type")}>
            <i className="ti ti-bolt" /> Type of attack
          </button>
          {error && <p className="error">{error}</p>}
          <table id="datatables" className="table table-striped table-responsive table-no-bordered table-hover" style={{ width: "100%" }}>
            <thead>
              <tr>
                <th><h5>Time</h5></th>
                <th><h5>Source</h5></th>
                <th><h5>Method</h5></th>
                <th><h5>URL</h5></th>
                <th><h5>Description</h5></th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              {items.map((item) => (
                <tr key={item.id}>
                  <td style={{ width: 100 }}>
                    <span>{relativeTime(item.occurred_at)}</span>
                    <div className="muted">{formatTime(item.occurred_at, user.timezone)}</div>
                    {item.is_demo && <div className="demo-pill">DEMO</div>}
                  </td>
                  <td>
                    {item.source_ip || "—"}
                    <div className="muted">{item.host}</div>
                  </td>
                  <td>
                    <span className={`label-method ${item.method.toLowerCase()}`}>{item.method}</span>
                  </td>
                  <td className="wrappedcell">{item.uri}</td>
                  <td>
                    {item.rule_ids.join(", ") || item.outcome}
                    <div className="muted">{item.severity}</div>
                  </td>
                  <td>
                    <Link to={`/attacks/${item.id}`} className="btn btn-info btn-fab btn-fab-mini btn-fill">
                      <i className="ti ti-eye" />
                    </Link>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
          {items.length === 0 && <p className="muted">No attacks in this window.</p>}
        </div>
      </div>
    </div>
  );
}
