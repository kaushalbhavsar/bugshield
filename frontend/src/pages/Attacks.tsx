import { useEffect, useMemo, useState, type ReactNode } from "react";
import { Link } from "react-router-dom";
import { api } from "../api";
import { formatTime, outcomeLabel, relativeTime } from "../time";
import type { EventDetail, EventSummary, Site, User } from "../types";

type Page = { items: EventSummary[]; total: number; page: number; page_size: number };
type Range = "today" | "week" | "month";
type SortKey = "time" | "source" | "method" | "url" | "type";

function rangeWindow(range: Range): { from: Date; to: Date; windowLabel: string } {
  const to = new Date();
  const from = new Date(to);
  if (range === "today") {
    from.setUTCDate(from.getUTCDate() - 1);
    return { from, to, windowLabel: "24 hours" };
  }
  if (range === "week") {
    from.setUTCDate(from.getUTCDate() - 7);
    return { from, to, windowLabel: "7 days" };
  }
  from.setUTCDate(from.getUTCDate() - 30);
  return { from, to, windowLabel: "this month" };
}

function methodLabel(method: string) {
  const value = method.toUpperCase();
  if (value === "POST") return { cls: "label-danger", text: " Post" };
  if (value === "GET") return { cls: "label-success", text: " Get" };
  if (value === "HEAD") return { cls: "label-info", text: " Head" };
  if (value === "PUT") return { cls: "label-warning", text: " Put" };
  return { cls: "label-danger", text: ` ${method}` };
}

export function AttacksPage({ user, range }: { user: User; range: Range }) {
  const [sites, setSites] = useState<Site[]>([]);
  const [page, setPage] = useState<Page>({ items: [], total: 0, page: 1, page_size: 25 });
  const [pageNumber, setPageNumber] = useState(1);
  const [siteId, setSiteId] = useState("");
  const [sort, setSort] = useState<SortKey>("time");
  const [error, setError] = useState("");
  const [modalId, setModalId] = useState<string | null>(null);

  useEffect(() => {
    api.sites().then((items) => setSites(items as Site[])).catch((err) => setError(String(err)));
  }, []);

  useEffect(() => {
    setPageNumber(1);
  }, [range, siteId]);

  useEffect(() => {
    const { from, to } = rangeWindow(range);
    const params = new URLSearchParams();
    params.set("occurred_from", from.toISOString());
    params.set("occurred_to", to.toISOString());
    params.set("page", String(pageNumber));
    params.set("page_size", "25");
    if (siteId) params.set("site_id", siteId);
    api
      .events(`?${params.toString()}`)
      .then((payload) => setPage(payload as Page))
      .catch((err) => setError(err instanceof Error ? err.message : "search failed"));
  }, [range, siteId, pageNumber]);

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

  const { windowLabel } = rangeWindow(range);
  const siteName = sites.find((site) => site.id === siteId)?.hostname || sites.find((site) => site.id === siteId)?.name || "all sites";
  const pageCount = Math.max(1, Math.ceil(page.total / page.page_size));

  return (
    <div>
      <div className="col-md-12 attacks-heading">
        <h4 className="title">
          {page.total} attacks blocked on{" "}
          <b>{siteName}</b>
          {range === "month" ? (
            <>
              {" "}
              in <b>{windowLabel}</b>
            </>
          ) : (
            <>
              {" "}
              in last <b>{windowLabel}</b>
            </>
          )}
          <select className="form-control" value={siteId} onChange={(event) => setSiteId(event.target.value)}>
            <option value="">All sites</option>
            {sites.map((site) => (
              <option key={site.id} value={site.id}>
                {site.name}
              </option>
            ))}
          </select>
        </h4>
      </div>
      <div className="content">
        <div className="container-fluid">
          <div className="row">
            <div className="col-md-12">
              <div className="card">
                <div className="content">
                  {range === "today" && (
                    <div>
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
                    </div>
                  )}
                  {error && <p className="error">{error}</p>}
                  <table id="datatables" className="table table-striped table-responsive table-no-bordered table-hover" style={{ width: "100%" }}>
                    <thead>
                      <tr>
                        <SortHeader label="Time" onClick={() => setSort("time")} />
                        <SortHeader label="Source" onClick={() => setSort("source")} />
                        <SortHeader label="Method" onClick={() => setSort("method")} />
                        <SortHeader label="URL" onClick={() => setSort("url")} />
                        <SortHeader label="Description" onClick={() => setSort("type")} />
                        <th></th>
                      </tr>
                    </thead>
                    <tbody>
                      {items.map((item) => {
                        const method = methodLabel(item.method);
                        return (
                          <tr key={item.id}>
                            <td style={{ width: range === "today" ? 100 : 200 }}>
                              {range === "today" ? <span>{relativeTime(item.occurred_at)}</span> : <span>{formatTime(item.occurred_at, user.timezone)}</span>}
                              {range === "today" && <div className="muted">{formatTime(item.occurred_at, user.timezone)}</div>}
                              {item.is_demo && <div className="demo-pill">DEMO</div>}
                            </td>
                            <td style={{ width: 120 }}>
                              {item.source_ip || "—"}
                              <div className="muted">{item.host}</div>
                            </td>
                            <td style={{ width: 8 }}>
                              <span className={`label ${method.cls}`}>
                                <span className="glyphicon glyphicon-arrow-right">{method.text}</span>
                              </span>
                            </td>
                            <td style={{ width: 300 }} className="wrappedcell">
                              {item.uri}
                            </td>
                            <td className="wrappedcell">
                              {item.rule_ids.join(", ") || outcomeLabel(item.outcome)}
                              <div className="muted">{item.severity}</div>
                            </td>
                            <td style={{ width: 8 }}>
                              {range === "today" ? (
                                <button type="button" className="btn btn-info btn-fab btn-fab-mini btn-fill" onClick={() => setModalId(item.id)}>
                                  <i className="ti ti-eye" />
                                </button>
                              ) : (
                                <Link to={`/attacks/${item.id}?from=${range}`} className="btn btn-default">
                                  <i className="ti-arrow-right" />
                                </Link>
                              )}
                            </td>
                          </tr>
                        );
                      })}
                    </tbody>
                  </table>
                  {items.length === 0 && <p className="muted">No attacks in this window.</p>}
                </div>
              </div>
            </div>
          </div>
          <div className="row">
            <div className="col-lg-12">
              <div className="text-center">
                Displaying page {page.total === 0 ? 0 : page.page} of {page.total === 0 ? 0 : pageCount}
                <Pager page={page.page} pageCount={pageCount} onPage={setPageNumber} />
              </div>
            </div>
          </div>
        </div>
      </div>
      {modalId && <AttackModal eventId={modalId} user={user} onClose={() => setModalId(null)} />}
    </div>
  );
}

function SortHeader({ label, onClick }: { label: string; onClick: () => void }) {
  return (
    <th className="col1">
      <a href="#sort" onClick={(event) => { event.preventDefault(); onClick(); }}>
        <h5>
          {label} <i className="glyphicon glyphicon-sort" />
        </h5>
      </a>
    </th>
  );
}

function Pager({ page, pageCount, onPage }: { page: number; pageCount: number; onPage: (page: number) => void }) {
  if (pageCount <= 1) return null;
  const pages: Array<number | "gap"> = [];
  for (let item = 1; item <= pageCount; item += 1) {
    if (item === 1 || item === pageCount || Math.abs(item - page) <= 2) {
      pages.push(item);
    } else if (pages[pages.length - 1] !== "gap") {
      pages.push("gap");
    }
  }
  return (
    <ul className="pagination">
      <li className={page <= 1 ? "disabled" : undefined}>
        <a href="#prev" onClick={(event) => { event.preventDefault(); if (page > 1) onPage(page - 1); }}>
          «
        </a>
      </li>
      {pages.map((item, index) =>
        item === "gap" ? (
          <li key={`gap-${index}`} className="disabled">
            <span>…</span>
          </li>
        ) : (
          <li key={item} className={item === page ? "active" : undefined}>
            <a href={`#page-${item}`} onClick={(event) => { event.preventDefault(); onPage(item); }}>
              {item}
            </a>
          </li>
        ),
      )}
      <li className={page >= pageCount ? "disabled" : undefined}>
        <a href="#next" onClick={(event) => { event.preventDefault(); if (page < pageCount) onPage(page + 1); }}>
          »
        </a>
      </li>
    </ul>
  );
}

function AttackModal({ eventId, user, onClose }: { eventId: string; user: User; onClose: () => void }) {
  const [event, setEvent] = useState<EventDetail | null>(null);
  const [error, setError] = useState("");

  useEffect(() => {
    api
      .event(eventId)
      .then((payload) => setEvent(payload as EventDetail))
      .catch((err) => setError(err instanceof Error ? err.message : "not found"));
  }, [eventId]);

  const userAgent = event?.request_headers_redacted?.["User-Agent"] || event?.request_headers_redacted?.["user-agent"] || "—";

  return (
    <div>
      <div className="modal-backdrop in" onClick={onClose} />
      <div className="modal in" tabIndex={-1} role="dialog">
        <div className="modal-dialog modal-lg" role="document">
          <div className="modal-content">
            <div className="modal-header">
              <h4 className="modal-title">{event?.rules[0]?.message || (event ? outcomeLabel(event.outcome) : "Attack")}</h4>
            </div>
            <div className="modal-body" style={{ height: 300, overflow: "auto" }}>
              {error && <p className="error">{error}</p>}
              {!event && !error && <p className="muted">Loading…</p>}
              {event && (
                <div className="content">
                  <div className="col-md-12">
                    <DetailField label="Time" value={formatTime(event.occurred_at, user.timezone)} />
                    <DetailField label="Source" value={event.source_ip || "—"} />
                    <DetailField label="Method" value={event.method} />
                    <DetailField
                      label="Url"
                      value={
                        <code>
                          {event.host}
                          {event.uri}
                        </code>
                      }
                    />
                    <DetailField label="User Agent" value={userAgent} />
                    <DetailField label="Post Data" value={<pre className="prettyprint">{event.source_record_redacted}</pre>} />
                  </div>
                </div>
              )}
            </div>
            <div className="modal-footer">
              {event && (
                <Link to={`/attacks/${event.id}?from=today`} className="btn btn-default">
                  Full details
                </Link>
              )}
              <button type="button" className="btn btn-info" onClick={onClose}>
                Close
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}

function DetailField({ label, value }: { label: string; value: ReactNode }) {
  return (
    <fieldset>
      <div className="form-group">
        <label className="col-sm-2 control-label">{label}</label>
        <div className="col-sm-10">
          <span>{value}</span>
        </div>
      </div>
    </fieldset>
  );
}
