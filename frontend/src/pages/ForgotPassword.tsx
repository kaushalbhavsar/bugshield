import { FormEvent, useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { AuthFooter } from "../components/AuthFooter";

export function ForgotPassword() {
  const [message, setMessage] = useState("");
  const [visible, setVisible] = useState(false);

  useEffect(() => {
    document.title = "BugShield - Login to Dashboard";
    const timer = window.setTimeout(() => setVisible(true), 700);
    return () => window.clearTimeout(timer);
  }, []);

  function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setMessage(
      "This self-hosted instance does not send reset email. Sign in and use Change Password, or reset the administrator password from the server.",
    );
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
                    <img src="/assets/img/logo.png" width={250} alt="BugShield" />
                  </div>
                  <div className="content">
                    <div className="form-group">
                      <label htmlFor="Email">Email</label>
                      <input
                        className="form-control input-no-border"
                        id="Email"
                        name="email"
                        placeholder="Your email ID"
                        type="text"
                        required
                      />
                    </div>
                    {message && <div className="text-danger" style={{ textAlign: "center", marginBottom: 12 }}>{message}</div>}
                    <div className="text-center">
                      <input type="submit" className="btn btn-rose btn-wd btn-lg" value="Submit" />
                      <p>
                        <br />
                        <Link to="/" style={{ color: "#d40000" }}>
                          Back to Sign in
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
