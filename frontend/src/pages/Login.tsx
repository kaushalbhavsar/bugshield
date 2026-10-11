import { FormEvent, useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { api } from "../api";
import { AuthFooter } from "../components/AuthFooter";

export function Login({ onReady }: { onReady: () => void }) {
  const [error, setError] = useState("");
  const [visible, setVisible] = useState(false);

  useEffect(() => {
    document.title = "BugShield - Login to Dashboard";
    const timer = window.setTimeout(() => setVisible(true), 700);
    return () => window.clearTimeout(timer);
  }, []);

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
    <div className="wrapper wrapper-full-page">
      <div className="full-page login-page" data-color="rose">
        <div className="content">
          <div className="login-card-wrap">
            <section id="loginForm">
              <form className="form-horizontal" onSubmit={submit}>
                <div className={`card card-login${visible ? " show" : " card-hidden"}`}>
                  <div className="header text-center">
                    <img src="/assets/img/logo.png" width={250} title="BugShield" alt="BugShield" />
                  </div>
                  <div className="content">
                    {error && <div className="text-danger">{error}</div>}
                    <div className="form-group">
                      <label htmlFor="Email">Email</label>
                      <input className="form-control input-no-border" id="Email" name="email" type="text" required />
                    </div>
                    <div className="form-group">
                      <label htmlFor="Password">Password</label>
                      <input className="form-control input-no-border" id="Password" name="password" type="password" required />
                    </div>
                    <div className="text-center">
                      <input type="submit" className="btn btn-rose btn-wd btn-lg" value="Sign in" />
                      <p>
                        <br />
                        <Link to="/account/forgot-password" style={{ color: "#d40000", fontSize: "small" }}>
                          Forgot Password?
                        </Link>
                      </p>
                    </div>
                  </div>
                </div>
              </form>
            </section>
          </div>
        </div>
        <AuthFooter />
      </div>
    </div>
  );
}
