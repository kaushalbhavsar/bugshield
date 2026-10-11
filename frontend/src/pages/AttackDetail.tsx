import { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import { api } from "../api";
import { formatTime, outcomeLabel } from "../time";
import type { EventDetail, User } from "../types";

export function AttackDetailPage({ user }: { user: User }) {
  const { eventId } = useParams();
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

  return (
    <div>
      <p>
        <Link to="/attacks/today">← Attacks today</Link>
      </p>
      <div className="card">
        <div className="header">
          <h4 className="title">{event.rules[0]?.message || outcomeLabel(event.outcome)}</h4>
        </div>
        <div className="content">
          <fieldset>
            <div className="form-group">
              <label className="col-sm-2 control-label">Time</label>
              <div className="col-sm-10">
                <span>{formatTime(event.occurred_at, user.timezone)}</span>
              </div>
            </div>
          </fieldset>
          <fieldset>
            <div className="form-group">
              <label className="col-sm-2 control-label">Source</label>
              <div className="col-sm-10">
                <span>{event.source_ip || "—"}</span>
              </div>
            </div>
          </fieldset>
          <fieldset>
            <div className="form-group">
              <label className="col-sm-2 control-label">Method</label>
              <div className="col-sm-10">
                <span>{event.method}</span>
              </div>
            </div>
          </fieldset>
          <fieldset>
            <div className="form-group">
              <label className="col-sm-2 control-label">Url</label>
              <div className="col-sm-10">
                <span>
                  <code>
                    {event.host}
                    {event.uri}
                  </code>
                </span>
              </div>
            </div>
          </fieldset>
          <fieldset>
            <div className="form-group">
              <label className="col-sm-2 control-label">Outcome</label>
              <div className="col-sm-10">
                <span>{outcomeLabel(event.outcome)}</span>
                <p>{event.outcome_note}</p>
              </div>
            </div>
          </fieldset>
          <h4 className="title">Redacted request headers</h4>
          <pre className="redacted">{JSON.stringify(event.request_headers_redacted, null, 2)}</pre>
          <h4 className="title">Redacted source record</h4>
          <pre className="redacted">{event.source_record_redacted}</pre>
        </div>
      </div>
    </div>
  );
}
