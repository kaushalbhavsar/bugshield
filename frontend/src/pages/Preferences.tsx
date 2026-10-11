import { FormEvent, useEffect, useState } from "react";
import { api } from "../api";
import { EmptyState } from "../components/EmptyState";
import type { Settings, Site, Token, User } from "../types";

export function PreferencesPage({ user, onUser }: { user: User; onUser: () => void }) {
  const [settings, setSettings] = useState<Settings | null>(null);
  const [sites, setSites] = useState<Site[]>([]);
  const [selected, setSelected] = useState<Site | null>(null);
  const [tokens, setTokens] = useState<Token[]>([]);
  const [newToken, setNewToken] = useState<Token | null>(null);
  const [message, setMessage] = useState("");
  const [error, setError] = useState("");

  async function refreshSites() {
    const items = (await api.sites()) as Site[];
    setSites(items);
    if (selected) {
      setSelected(items.find((item) => item.id === selected.id) || null);
    }
  }

  useEffect(() => {
    api.settings().then((payload) => setSettings(payload as Settings)).catch((err) => setError(String(err)));
    refreshSites().catch((err) => setError(String(err)));
  }, []);

  useEffect(() => {
    if (!selected) {
      setTokens([]);
      return;
    }
    api.tokens(selected.id).then((items) => setTokens(items as Token[])).catch((err) => setError(String(err)));
  }, [selected]);

  async function saveSettings(event: FormEvent<HTMLFormElement>) {
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
    setMessage("Preferences saved.");
    onUser();
  }

  async function createSite(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const data = new FormData(event.currentTarget);
    setError("");
    const site = (await api.createSite({
      name: data.get("name"),
      hostname: data.get("hostname"),
      notes: data.get("notes"),
    })) as Site;
    event.currentTarget.reset();
    await refreshSites();
    setSelected(site);
  }

  async function createToken(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!selected) return;
    const data = new FormData(event.currentTarget);
    const token = (await api.createToken(selected.id, String(data.get("label") || "default"))) as Token;
    setNewToken(token);
    setTokens((await api.tokens(selected.id)) as Token[]);
  }

  async function upload(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!selected) return;
    const data = new FormData(event.currentTarget);
    const file = data.get("file") as File;
    const result = await api.upload(selected.id, file);
    setMessage(JSON.stringify(result));
  }

  if (!settings) return <p className="muted">{error || "Loading preferences…"}</p>;

  return (
    <div>
      <div className="columns even">
        <div className="card">
          <div className="header">
            <h4 className="title">Add a site</h4>
            <p className="category">Each site has its own ingestion tokens.</p>
          </div>
          <div className="content">
            <form className="grid" onSubmit={createSite}>
              <label>
                Name
                <input name="name" required />
              </label>
              <label>
                Hostname
                <input name="hostname" placeholder="app.example.test" />
              </label>
              <label>
                Notes
                <textarea name="notes" rows={3} />
              </label>
              <button type="submit" className="btn btn-rose">
                Create site
              </button>
            </form>
          </div>
        </div>
        <div className="card">
          <div className="header">
            <h4 className="title">Your sites</h4>
          </div>
          <div className="content">
            {sites.length === 0 ? (
              <EmptyState title="No sites yet">Create a site to start ingesting audit logs.</EmptyState>
            ) : (
              <div className="grid site-list">
                {sites.map((site) => (
                  <button
                    key={site.id}
                    type="button"
                    className={`btn site-item${selected?.id === site.id ? " active" : ""}`}
                    onClick={() => {
                      setSelected(site);
                      setNewToken(null);
                      setMessage("");
                    }}
                  >
                    {site.name}
                    <div className="muted">{site.hostname || site.slug}</div>
                  </button>
                ))}
              </div>
            )}
          </div>
        </div>
      </div>

      {selected && (
        <div className="card">
          <div className="header">
            <h4 className="title">{selected.name}</h4>
            <p className="category">{selected.notes || "No notes"}</p>
          </div>
          <div className="content">
            <form className="row" onSubmit={createToken}>
              <input name="label" placeholder="Token label" defaultValue="edge-agent" className="input-bordered" />
              <button type="submit" className="btn btn-rose">
                Create ingestion token
              </button>
            </form>
            {newToken?.token && (
              <div className="token-once">
                <strong>Copy this token now.</strong> It will not be shown again.
                <pre>{newToken.token}</pre>
              </div>
            )}
            <table className="table">
              <thead>
                <tr>
                  <th>Prefix</th>
                  <th>Label</th>
                  <th>Status</th>
                  <th></th>
                </tr>
              </thead>
              <tbody>
                {tokens.map((token) => (
                  <tr key={token.id}>
                    <td>{token.token_prefix}</td>
                    <td>{token.label}</td>
                    <td>{token.revoked_at ? "revoked" : "active"}</td>
                    <td>
                      {!token.revoked_at && (
                        <button
                          type="button"
                          className="btn btn-danger"
                          onClick={async () => {
                            await api.revokeToken(selected.id, token.id);
                            setTokens((await api.tokens(selected.id)) as Token[]);
                          }}
                        >
                          Revoke
                        </button>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
            <h4 className="title" style={{ marginTop: "1.4rem" }}>
              Upload audit log
            </h4>
            <form className="row" onSubmit={upload}>
              <input name="file" type="file" required />
              <button type="submit" className="btn btn-info">
                Upload
              </button>
            </form>
          </div>
        </div>
      )}

      <div className="card" style={{ maxWidth: 640 }}>
        <div className="header">
          <h4 className="title">Retention and display</h4>
        </div>
        <div className="content">
          <form className="grid" onSubmit={saveSettings}>
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
            <button type="submit" className="btn btn-rose">
              Save preferences
            </button>
          </form>
        </div>
      </div>

      <div className="card" style={{ maxWidth: 640 }}>
        <div className="header">
          <h4 className="title">Demo dataset</h4>
          <p className="category">
            Load clearly labelled synthetic events. They are marked as demo data and can be removed
            without touching real ingest.
          </p>
        </div>
        <div className="content">
          <div className="row">
            <button
              type="button"
              className="btn btn-rose"
              onClick={async () => {
                const result = await api.loadDemo();
                setMessage(`Demo loaded: ${JSON.stringify(result)}`);
              }}
            >
              Load demo dataset
            </button>
            <button
              type="button"
              className="btn btn-white"
              onClick={async () => {
                const result = await api.clearDemo();
                setMessage(`Demo removed: ${JSON.stringify(result)}`);
              }}
            >
              Remove demo dataset
            </button>
            <button
              type="button"
              className="btn btn-white"
              onClick={async () => {
                const result = await api.runRetention();
                setMessage(`Retention run: ${JSON.stringify(result)}`);
              }}
            >
              Run retention now
            </button>
          </div>
        </div>
      </div>
      {message && <p>{message}</p>}
      {error && <p className="error">{error}</p>}
    </div>
  );
}
