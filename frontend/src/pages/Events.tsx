import { FormEvent, useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { api } from "../api";
import { EmptyState } from "../components/EmptyState";
import { formatTime, outcomeLabel } from "../time";
import type { EventSummary, Site, User } from "../types";

type Page = { items: EventSummary[]; total: number; page: number; page_size: number };

export function EventsPage({ user }: { user: User }) {
  const [sites, setSites] = useState<Site[]>([]);
  const [page, setPage] = useState<Page>({ items: [], total: 0, page: 1, page_size: 25 });
  const [query, setQuery] = useState("");
  const [error, setError] = useState("");

  useEffect(() => {
    api.sites().then((items) => setSites(items as Site[])).catch((err) => setError(String(err)));
  }, []);

  function search(event?: FormEvent<HTMLFormElement>, nextPage = 1) {
    event?.preventDefault();
    const form = event?.currentTarget ?? (document.getElementById("filters") as HTMLFormElement);
    const data = new FormData(form);
    const params = new URLSearchParams();
    for (const [key, value] of data.entries()) {
      if (String(value)) params.set(key, String(value));
    }
    params.set("page", String(nextPage));
    setQuery(`?${params.toString()}`);
  }

  useEffect(() => {
    api
      .events(query)
      .then((payload) => setPage(payload as Page))
      .catch((err) => setError(err instanceof Error ? err.message : "search failed"));
  }, [query]);

  return (
    <div>
      <h1>Events</h1>
      <p className="muted">
        Filter recorded audit events. Confirmed disruptive actions are labelled separately from
        detections. Display timezone: {user.timezone}.
      </p>
      <form id="filters" className="filters" onSubmit={(event) => search(event, 1)}>
        <label>
          Site
          <select name="site_id" defaultValue="">
            <option value="">All sites</option>
            {sites.map((site) => (
              <option key={site.id} value={site.id}>
                {site.name}
              </option>
            ))}
          </select>
        </label>
        <label>
          From (UTC)
          <input name="occurred_from" type="datetime-local" />
        </label>
        <label>
          To (UTC)
          <input name="occurred_to" type="datetime-local" />
        </label>
        <label>
          Source IP
          <input name="source_ip" placeholder="203.0.113.10" />
        </label>
        <label>
          URL contains
          <input name="url" />
        </label>
        <label>
          Rule ID
          <input name="rule_id" placeholder="942100" />
        </label>
        <label>
          Severity
          <select name="severity" defaultValue="">
            <option value="">Any</option>
            <option value="critical">critical</option>
            <option value="error">error</option>
            <option value="warning">warning</option>
            <option value="notice">notice</option>
          </select>
        </label>
        <label>
          Outcome
          <select name="outcome" defaultValue="">
            <option value="">Any</option>
            <option value="disrupted">Confirmed disruptive</option>
            <option value="detected">Detected</option>
            <option value="observed">Observed</option>
          </select>
        </label>
        <div style={{ alignSelf: "end" }}>
          <button type="submit">Search</button>
        </div>
      </form>
      {error && <p className="error">{error}</p>}
      {page.items.length === 0 ? (
        <EmptyState title="No events match these filters">
          Broaden the time range or load the labelled demo dataset from Settings.
        </EmptyState>
      ) : (
        <div className="card">
          <table className="table">
            <thead>
              <tr>
                <th>When</th>
                <th>Outcome</th>
                <th>Source</th>
                <th>Request</th>
                <th>Rules</th>
              </tr>
            </thead>
            <tbody>
              {page.items.map((item) => (
                <tr key={item.id}>
                  <td>
                    {formatTime(item.occurred_at, user.timezone)}
                    {item.is_demo && (
                      <>
                        <br />
                        <span className="pill demo">Demo</span>
                      </>
                    )}
                  </td>
                  <td>
                    <span className={`pill ${item.outcome}`}>{outcomeLabel(item.outcome)}</span>
                    <div className="muted">{item.severity}</div>
                  </td>
                  <td>{item.source_ip || "—"}</td>
                  <td>
                    <Link to={`/events/${item.id}`}>
                      {item.method} {item.uri}
                    </Link>
                    <div className="muted">{item.host}</div>
                  </td>
                  <td>{item.rule_ids.join(", ") || "—"}</td>
                </tr>
              ))}
            </tbody>
          </table>
          <div className="row" style={{ marginTop: "1rem" }}>
            <button
              className="secondary"
              disabled={page.page <= 1}
              onClick={() => search(undefined, page.page - 1)}
            >
              Previous
            </button>
            <span className="muted">
              Page {page.page} of {Math.max(1, Math.ceil(page.total / page.page_size))} ({page.total} events)
            </span>
            <button
              className="secondary"
              disabled={page.page * page.page_size >= page.total}
              onClick={() => search(undefined, page.page + 1)}
            >
              Next
            </button>
          </div>
        </div>
      )}
    </div>
  );
}
