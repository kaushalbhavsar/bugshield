import { FormEvent, useEffect, useState } from "react";
import { api } from "../api";
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
    if (selected) setSelected(items.find((item) => item.id === selected.id) || null);
    else if (items[0]) setSelected(items[0]);
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
    await api.saveSettings({
      retention_days: Number(data.get("retention_days")),
      extra_redact_headers: String(data.get("extra_redact_headers") || "").split(",").map((item) => item.trim()).filter(Boolean),
      extra_redact_fields: String(data.get("extra_redact_fields") || "").split(",").map((item) => item.trim()).filter(Boolean),
    });
    await api.updateMe({ timezone: data.get("timezone") });
    setMessage("Preferences saved.");
    onUser();
  }

  async function createSite(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const data = new FormData(event.currentTarget);
    const site = (await api.createSite({
      name: data.get("name"),
      hostname: data.get("hostname"),
      notes: data.get("notes"),
    })) as Site;
    event.currentTarget.reset();
    await refreshSites();
    setSelected(site);
  }

  if (!settings) return <p className="muted">{error || "Loading preferences…"}</p>;

  return (
    <div>
      <form className="form-horizontal">
        <div className="col-md-6 col-md-offset-2">
          <div className="card card-wizard">
            <div className="content">
              <div className="form-group">
                <div className="col-md-4 control-label">
                  <b>Website</b>
                </div>
                <div className="col-md-6">
                  <input className="form-control" readOnly value={selected?.hostname || selected?.name || ""} />
                </div>
              </div>
              <div className="form-group">
                <div className="col-md-4 control-label">
                  <b>Email</b>
                </div>
                <div className="col-md-6">
                  <input className="form-control" readOnly value={user.email} />
                  <p className="muted">Notification email is not sent by this self-hosted successor.</p>
                </div>
              </div>
            </div>
          </div>
        </div>
      </form>

      <div className="col-md-6">
        <div className="card">
          <div className="content">
            <h4 className="title">Add a site</h4>
            <form onSubmit={createSite}>
              <div className="form-group">
                <input className="form-control" name="name" placeholder="Name" required />
              </div>
              <div className="form-group">
                <input className="form-control" name="hostname" placeholder="Hostname" />
              </div>
              <div className="form-group">
                <textarea className="form-control" name="notes" placeholder="Notes" rows={3} />
              </div>
              <button type="submit" className="btn btn-rose">
                Create site
              </button>
            </form>
          </div>
        </div>
      </div>
      <div className="col-md-6">
        <div className="card">
          <div className="content">
            <h4 className="title">Your sites</h4>
            <div className="site-list">
              {sites.map((site) => (
                <button key={site.id} type="button" className="btn btn-default" onClick={() => setSelected(site)}>
                  {site.name}
                  <div className="muted">{site.hostname || site.slug}</div>
                </button>
              ))}
            </div>
          </div>
        </div>
      </div>

      {selected && (
        <div className="col-md-12">
          <div className="card">
            <div className="content">
              <h4 className="title">{selected.name}</h4>
              <form
                className="form-inline"
                onSubmit={async (event) => {
                  event.preventDefault();
                  const data = new FormData(event.currentTarget);
                  const token = (await api.createToken(selected.id, String(data.get("label") || "default"))) as Token;
                  setNewToken(token);
                  setTokens((await api.tokens(selected.id)) as Token[]);
                }}
              >
                <input className="form-control" name="label" defaultValue="edge-agent" />
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
              <h4 className="title">Upload audit log</h4>
              <form
                onSubmit={async (event) => {
                  event.preventDefault();
                  const data = new FormData(event.currentTarget);
                  const result = await api.upload(selected.id, data.get("file") as File);
                  setMessage(JSON.stringify(result));
                }}
              >
                <input name="file" type="file" required />
                <button type="submit" className="btn btn-info">
                  Upload
                </button>
              </form>
            </div>
          </div>
        </div>
      )}

      <div className="col-md-8">
        <div className="card">
          <div className="content">
            <h4 className="title">Retention and display</h4>
            <form onSubmit={saveSettings}>
              <div className="form-group">
                <label>Display timezone</label>
                <input className="form-control" name="timezone" defaultValue={user.timezone} />
              </div>
              <div className="form-group">
                <label>Retention (days)</label>
                <input className="form-control" name="retention_days" type="number" min={1} max={3650} defaultValue={settings.retention_days} />
              </div>
              <div className="form-group">
                <label>Extra headers to redact</label>
                <input className="form-control" name="extra_redact_headers" defaultValue={settings.extra_redact_headers.join(", ")} />
              </div>
              <div className="form-group">
                <label>Extra query/body field names to redact</label>
                <input className="form-control" name="extra_redact_fields" defaultValue={settings.extra_redact_fields.join(", ")} />
              </div>
              <button type="submit" className="btn btn-default">
                Update
              </button>
            </form>
            <hr />
            <button type="button" className="btn btn-rose" onClick={async () => setMessage(JSON.stringify(await api.loadDemo()))}>
              Load demo dataset
            </button>{" "}
            <button type="button" className="btn btn-default" onClick={async () => setMessage(JSON.stringify(await api.clearDemo()))}>
              Remove demo dataset
            </button>{" "}
            <button type="button" className="btn btn-default" onClick={async () => setMessage(JSON.stringify(await api.runRetention()))}>
              Run retention now
            </button>
          </div>
        </div>
      </div>
      {message && <div className="col-md-12"><p>{message}</p></div>}
      {error && <div className="col-md-12"><p className="error">{error}</p></div>}
    </div>
  );
}
