import { FormEvent, useEffect, useState } from "react";
import { api } from "../api";
import { EmptyState } from "../components/EmptyState";
import type { Site, Token } from "../types";

export function SitesPage() {
  const [sites, setSites] = useState<Site[]>([]);
  const [selected, setSelected] = useState<Site | null>(null);
  const [tokens, setTokens] = useState<Token[]>([]);
  const [newToken, setNewToken] = useState<Token | null>(null);
  const [message, setMessage] = useState("");
  const [error, setError] = useState("");

  async function refresh() {
    const items = (await api.sites()) as Site[];
    setSites(items);
    if (selected) {
      const match = items.find((item) => item.id === selected.id) || null;
      setSelected(match);
    }
  }

  useEffect(() => {
    refresh().catch((err) => setError(String(err)));
  }, []);

  useEffect(() => {
    if (!selected) {
      setTokens([]);
      return;
    }
    api.tokens(selected.id).then((items) => setTokens(items as Token[])).catch((err) => setError(String(err)));
  }, [selected]);

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
    await refresh();
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

  return (
    <div>
      <h1>Sites</h1>
      <p className="muted">
        Each site has its own ingestion tokens. Tokens are hashed at rest and the secret is shown only
        once.
      </p>
      <div className="columns">
        <div className="card">
          <h3>Add a site</h3>
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
            <button type="submit">Create site</button>
          </form>
        </div>
        <div className="card">
          <h3>Your sites</h3>
          {sites.length === 0 ? (
            <EmptyState title="No sites yet">Create a site to start ingesting audit logs.</EmptyState>
          ) : (
            <div className="grid">
              {sites.map((site) => (
                <button
                  key={site.id}
                  className="secondary"
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
      {selected && (
        <div className="card" style={{ marginTop: "1rem" }}>
          <h3>{selected.name}</h3>
          <p className="muted">{selected.notes || "No notes"}</p>
          <form className="row" onSubmit={createToken}>
            <input name="label" placeholder="Token label" defaultValue="edge-agent" />
            <button type="submit">Create ingestion token</button>
          </form>
          {newToken?.token && (
            <div className="token-once" style={{ marginTop: "1rem" }}>
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
                        className="danger"
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
          <h3 style={{ marginTop: "1.4rem" }}>Upload audit log</h3>
          <form className="row" onSubmit={upload}>
            <input name="file" type="file" required />
            <button type="submit">Upload</button>
          </form>
          {message && <pre>{message}</pre>}
          {error && <p className="error">{error}</p>}
        </div>
      )}
    </div>
  );
}
