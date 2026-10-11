import { FormEvent, useState } from "react";
import { api } from "../api";

export function Login({ onReady }: { onReady: () => void }) {
  const [error, setError] = useState("");
  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const data = new FormData(event.currentTarget);
    setError("");
    try {
      await api.login({ email: data.get("email"), password: data.get("password") });
      onReady();
    } catch (err) {
      setError(err instanceof Error ? err.message : "login failed");
    }
  }
  return (
    <div className="auth">
      <form className="card auth-card grid" onSubmit={submit}>
        <div>
          <h1>Sign in to BugShield</h1>
          <p className="muted">Self-hosted ModSecurity audit log console.</p>
        </div>
        <label>
          Email
          <input name="email" type="email" required />
        </label>
        <label>
          Password
          <input name="password" type="password" required />
        </label>
        {error && <div className="error">{error}</div>}
        <button type="submit">Sign in</button>
      </form>
    </div>
  );
}
