import { useEffect, useState, type ReactNode } from "react";
import { Link, useParams, useSearchParams } from "react-router-dom";
import { api } from "../api";
import { formatTime, outcomeLabel } from "../time";
import type { EventDetail, User } from "../types";

export function AttackDetailPage({ user }: { user: User }) {
  const { eventId } = useParams();
  const [params] = useSearchParams();
  const from = params.get("from");
  const back =
    from === "week" ? "/attacks/week" : from === "month" ? "/attacks/month" : "/attacks/today";
  const [event, setEvent] = useState<EventDetail | null>(null);
  const [error, setError] = useState("");

  useEffect(() => {
    if (!eventId) return;
    api
      .event(eventId)
      .then((payload) => setEvent(payload as EventDetail))
      .catch((err) => setError(err instanceof Error ? err.message : "not found"));
  }, [eventId]);

  if (error) return <p className="error">{error}</p>;
  if (!event) return <p className="muted">Loading event…</p>;

  const userAgent = event.request_headers_redacted?.["User-Agent"] || event.request_headers_redacted?.["user-agent"] || "—";

  return (
    <div>
      <div className="row">
        <div className="col-md-12">
          <div className="card" style={{ minHeight: 300 }}>
            <div className="content">
              <div className="col-md-12">
                <table>
                  <tbody>
                    <DetailRow label="Time" value={formatTime(event.occurred_at, user.timezone)} />
                    <DetailRow label="Source IP" value={event.source_ip || "—"} />
                    <DetailRow label="Method" value={event.method} />
                    <DetailRow label="Code" value={event.status_code ?? "—"} />
                    <DetailRow
                      label="Url"
                      value={
                        <code>
                          {event.host}
                          {event.dest_port ? `:${event.dest_port}` : ""}
                          {event.uri}
                        </code>
                      }
                    />
                    <DetailRow label="User Agent" value={userAgent} />
                    <DetailRow label="Post Data" value={<pre className="prettyprint">{event.source_record_redacted}</pre>} />
                  </tbody>
                </table>
              </div>
            </div>
          </div>
        </div>
      </div>
      <p>
        <Link to={back} className="btn btn-transparent">
          <i className="glyphicon glyphicon-backward" /> Back to List
        </Link>
      </p>
      <div className="card">
        <div className="content">
          <h4 className="title">{event.rules[0]?.message || outcomeLabel(event.outcome)}</h4>
          <p>{event.outcome_note}</p>
          <h4 className="title">Redacted request headers</h4>
          <pre className="redacted">{JSON.stringify(event.request_headers_redacted, null, 2)}</pre>
        </div>
      </div>
    </div>
  );
}

function DetailRow({ label, value }: { label: string; value: ReactNode }) {
  return (
    <>
      <tr>
        <td width="30%">
          <b>{label}</b>
        </td>
        <td>{value}</td>
      </tr>
      <tr style={{ height: 10 }}>
        <td colSpan={3}></td>
      </tr>
    </>
  );
}
