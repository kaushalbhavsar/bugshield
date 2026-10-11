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
        <Link to="/attacks">← Blocked Attacks</Link>
      </p>
      <div className="card">
        <div className="header">
          <h4 className="title">
            {event.method} {event.uri}
          </h4>
          <p className="category">
            {formatTime(event.occurred_at, user.timezone)} · stored in UTC · displayed in {user.timezone}
          </p>
        </div>
        <div className="content">
          <div className="row">
            <span className={`pill ${event.outcome}`}>{outcomeLabel(event.outcome)}</span>
            {event.is_demo && <span className="pill demo">Demo dataset</span>}
          </div>
        </div>
      </div>
      <div className="card">
        <div className="header">
          <h4 className="title">What the log recorded</h4>
        </div>
        <div className="content">
          <p>{event.outcome_note}</p>
          <p className="muted">
            Explanations are limited to fields present in the audit record. BugShield does not invent
            remediation advice or contact URLs found in logs.
          </p>
        </div>
      </div>
      <div className="columns even">
        <div className="card">
          <div className="header">
            <h4 className="title">Matched rules</h4>
          </div>
          <div className="content">
            {event.rules.length === 0 ? (
              <p className="muted">No rule match details were present in this record.</p>
            ) : (
              event.rules.map((rule, index) => (
                <div key={`${rule.rule_id}-${index}`} style={{ marginBottom: "1rem" }}>
                  <strong>{rule.rule_id || "unidentified rule"}</strong>
                  <div>{rule.message || "No message was recorded for this match."}</div>
                  <div className="muted">
                    severity {rule.severity || "unknown"}
                    {rule.file ? ` · ${rule.file}` : ""}
                    {rule.line_number ? `:${rule.line_number}` : ""}
                    {rule.tags.length ? ` · ${rule.tags.join(", ")}` : ""}
                  </div>
                </div>
              ))
            )}
          </div>
        </div>
        <div className="card">
          <div className="header">
            <h4 className="title">Transaction</h4>
          </div>
          <div className="content">
            <p>Action: {event.action || "unknown"}</p>
            <p>Engine mode: {event.engine_mode}</p>
            <p>Status: {event.status_code ?? "—"}</p>
            <p>
              Source: {event.source_ip || "—"}
              {event.source_port ? `:${event.source_port}` : ""}
            </p>
            <p>Host: {event.host || "—"}</p>
            <p>Producer: {event.producer || "—"}</p>
            <p>Format: {event.log_format}</p>
          </div>
        </div>
      </div>
      <div className="card">
        <div className="header">
          <h4 className="title">Redacted request headers</h4>
        </div>
        <div className="content">
          <pre>{JSON.stringify(event.request_headers_redacted, null, 2)}</pre>
        </div>
      </div>
      <div className="card">
        <div className="header">
          <h4 className="title">Redacted source record</h4>
        </div>
        <div className="content">
          <pre>{event.source_record_redacted}</pre>
        </div>
      </div>
    </div>
  );
}
