import { FormEvent, useEffect, useState } from "react";
import { api } from "../api";

export function Setup({ onReady }: { onReady: () => void }) {
  const [error, setError] = useState("");

  useEffect(() => {
    document.title = "BugShield - Create a new account";
  }, []);

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const data = new FormData(event.currentTarget);
    const password = String(data.get("password") || "");
    const confirm = String(data.get("confirm_password") || "");
    if (password !== confirm) {
      setError("The password and confirmation password do not match.");
      return;
    }
    setError("");
    try {
      await api.setup({
        email: data.get("email"),
        password,
        display_name: data.get("display_name") || "Administrator",
        timezone: data.get("timezone") || "UTC",
      });
      onReady();
    } catch (err) {
      setError(err instanceof Error ? err.message : "setup failed");
    }
  }

  return (
    <div className="register-page">
      <div className="header-text">
        <img src="/assets/img/logo-dark.png" width={300} alt="BugShield" />
        <h4>Hack proof your website INSTANTLY.</h4>
        <hr />
      </div>
      <div className="register-grid">
        <div>
          <div className="media">
            <div className="icon icon-danger">
              <i className="ti-shield" />
            </div>
            <div className="media-body">
              <h5>Realtime Protection</h5>
              Stop hackers before they reach your website.
            </div>
          </div>
          <div className="media">
            <div className="icon icon-warning">
              <i className="ti-search" />
            </div>
            <div className="media-body">
              <h5>Security Scanning</h5>
              Discover security weaknesses in your website
            </div>
          </div>
          <div className="media">
            <div className="icon icon-info">
              <i className="ti-bolt" />
            </div>
            <div className="media-body">
              <h5>Instant Activation</h5>
              Get secure instantly. No installation required!
            </div>
          </div>
        </div>
        <form className="form-horizontal" onSubmit={submit}>
          <div className="card card-plain">
            <div className="content">
              <div className="form-group">
                <input className="form-control" name="display_name" placeholder="Display name" defaultValue="Administrator" required />
              </div>
              <div className="form-group">
                <input className="form-control" id="Email" name="email" placeholder="Your email" type="text" required />
              </div>
              <div className="form-group">
                <input className="form-control" id="Password" name="password" placeholder="Password (12+ characters)" type="password" minLength={12} required />
              </div>
              <div className="form-group">
                <input className="form-control" id="ConfirmPassword" name="confirm_password" placeholder="Confirm password" type="password" minLength={12} required />
              </div>
              <div className="form-group">
                <input className="form-control" name="timezone" placeholder="Timezone" defaultValue="UTC" />
              </div>
              {error && <div className="text-danger">{error}</div>}
              <div className="footer text-center">
                <input id="btnSubmit" type="submit" className="btn btn-fill btn-danger btn-wd" value="Sign up *" />
                <p className="muted" style={{ marginTop: 16 }}>
                  * By creating an account, you are agreeing to our Terms & Conditions
                </p>
                <p className="muted">This first account becomes the administrator for this self-hosted instance.</p>
              </div>
            </div>
          </div>
        </form>
      </div>
    </div>
  );
}
