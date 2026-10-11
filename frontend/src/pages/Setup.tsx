import { FormEvent, useState } from "react";
import { api } from "../api";

export function Setup({ onReady }: { onReady: () => void }) {
  const [error, setError] = useState("");
  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const data = new FormData(event.currentTarget);
    setError("");
    try {
      await api.setup({
        email: data.get("email"),
        password: data.get("password"),
        display_name: data.get("display_name"),
        timezone: data.get("timezone") || "UTC",
      });
      onReady();
    } catch (err) {
      setError(err instanceof Error ? err.message : "setup failed");
    }
  }
  return (
    <div className="auth">
      <form className="card auth-card grid" onSubmit={submit}>
        <div>
          <h1>Create the administrator</h1>
          <p className="muted">This instance has no users yet. The first account becomes the operator.</p>
        </div>
        <label>
          Display name
          <input name="display_name" required defaultValue="Administrator" />
        </label>
        <label>
          Email
          <input name="email" type="email" required />
        </label>
        <label>
          Password (12+ characters)
          <input name="password" type="password" minLength={12} required />
        </label>
        <label>
          Timezone
          <input name="timezone" defaultValue="UTC" />
        </label>
        {error && <div className="error">{error}</div>}
        <button type="submit">Create administrator</button>
      </form>
    </div>
  );
}
