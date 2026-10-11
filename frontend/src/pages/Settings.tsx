import { FormEvent, useEffect, useState } from "react";
import { api } from "../api";
import type { Settings, User } from "../types";

export function SettingsPage({ user, onUser }: { user: User; onUser: () => void }) {
  const [settings, setSettings] = useState<Settings | null>(null);
  const [message, setMessage] = useState("");
  const [error, setError] = useState("");

  useEffect(() => {
    api.settings().then((payload) => setSettings(payload as Settings)).catch((err) => setError(String(err)));
  }, []);

  async function save(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!settings) return;
    const data = new FormData(event.currentTarget);
    setError("");
    await api.saveSettings({
      retention_days: Number(data.get("retention_days")),
      extra_redact_headers: String(data.get("extra_redact_headers") || "")
        .split(",")
        .map((item) => item.trim())
        .filter(Boolean),
      extra_redact_fields: String(data.get("extra_redact_fields") || "")
        .split(",")
        .map((item) => item.trim())
        .filter(Boolean),
    });
    await api.updateMe({ timezone: data.get("timezone") });
    setMessage("Settings saved.");
    onUser();
  }

  if (!settings) return <p className="muted">{error || "Loading settings…"}</p>;

  return (
    <div>
      <h1>Settings</h1>
      <form className="card grid" style={{ maxWidth: 640 }} onSubmit={save}>
        <label>
          Display timezone
          <input name="timezone" defaultValue={user.timezone} />
        </label>
        <label>
          Retention (days)
          <input name="retention_days" type="number" min={1} max={3650} defaultValue={settings.retention_days} />
        </label>
        <label>
          Extra headers to redact (comma separated)
          <input name="extra_redact_headers" defaultValue={settings.extra_redact_headers.join(", ")} />
        </label>
        <label>
          Extra query/body field names to redact
          <input name="extra_redact_fields" defaultValue={settings.extra_redact_fields.join(", ")} />
        </label>
        <button type="submit">Save settings</button>
      </form>
      <div className="card" style={{ marginTop: "1rem", maxWidth: 640 }}>
        <h3>Demo dataset</h3>
        <p className="muted">
          Load clearly labelled synthetic events. They are marked as demo data and can be removed
          without touching real ingest.
        </p>
        <div className="row">
          <button
            onClick={async () => {
              const result = await api.loadDemo();
              setMessage(`Demo loaded: ${JSON.stringify(result)}`);
            }}
          >
            Load demo dataset
          </button>
          <button
            className="secondary"
            onClick={async () => {
              const result = await api.clearDemo();
              setMessage(`Demo removed: ${JSON.stringify(result)}`);
            }}
          >
            Remove demo dataset
          </button>
          <button
            className="secondary"
            onClick={async () => {
              const result = await api.runRetention();
              setMessage(`Retention run: ${JSON.stringify(result)}`);
            }}
          >
            Run retention now
          </button>
        </div>
      </div>
      {message && <p>{message}</p>}
      {error && <p className="error">{error}</p>}
    </div>
  );
}
