import { FormEvent, useEffect, useState } from "react";
import { api } from "../api";

export function Setup({ onReady }: { onReady: () => void }) {
  const [error, setError] = useState("");

  useEffect(() => {
    document.title = "BUGSKAN - Create a new account";
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
    <div className="wrapper wrapper-full-page">
      <div className="register-page">
        <div className="content">
          <div className="container">
            <div className="row">
              <div className="col-md-8 col-md-offset-2">
                <div className="header-text">
                  <img src="/assets/img/logo.png" width={300} style={{ width: 300 }} alt="BUGSKAN" />
                  <h4>Hack proof your website INSTANTLY.</h4>
                  <hr />
                </div>
              </div>
              <div className="col-md-4 col-md-offset-2">
                <div className="media">
                  <div className="media-left">
                    <div className="icon icon-danger">
                      <i className="ti ti-shield" />
                    </div>
                  </div>
                  <div className="media-body">
                    <h5>Realtime Protection</h5>
                    Stop hackers before they reach your website.
                  </div>
                </div>
                <div className="media">
                  <div className="media-left">
                    <div className="icon icon-warning">
                      <i className="ti-search" />
                    </div>
                  </div>
                  <div className="media-body">
                    <h5>Security Scanning</h5>
                    Discover security weaknesses in your website
                  </div>
                </div>
                <div className="media">
                  <div className="media-left">
                    <div className="icon icon-info">
                      <i className="ti-bolt" />
                    </div>
                  </div>
                  <div className="media-body">
                    <h5>Instant Activation</h5>
                    Get secure instantly. No installation required!
                  </div>
                </div>
              </div>
              <div className="col-md-4">
                <form className="form-horizontal" onSubmit={submit}>
                  <div className="card card-plain">
                    <div className="content">
                      <div className="form-group">
                        <div className="col-md-10">
                          <input className="form-control" name="display_name" placeholder="Display name" defaultValue="Administrator" required />
                        </div>
                      </div>
                      <div className="form-group">
                        <div className="col-md-10">
                          <input className="form-control" id="Email" name="email" placeholder="Your email" type="text" required />
                        </div>
                      </div>
                      <div className="form-group">
                        <div className="col-md-10">
                          <input className="form-control" name="password" placeholder="Password (12+ characters)" type="password" minLength={12} required />
                        </div>
                      </div>
                      <div className="form-group">
                        <div className="col-md-10">
                          <input className="form-control" name="confirm_password" placeholder="Confirm password" type="password" minLength={12} required />
                        </div>
                      </div>
                      <div className="form-group">
                        <div className="col-md-10">
                          <input className="form-control" name="timezone" placeholder="Timezone" defaultValue="UTC" />
                        </div>
                      </div>
                      {error && <div className="text-danger">{error}</div>}
                      <div className="footer text-center">
                        <input type="submit" className="btn btn-fill btn-danger btn-wd" value="Sign up *" />
                        <br />
                        <br />
                        <div className="media-body">
                          <center>* By creating an account, you are agreeing to our Terms & Conditions</center>
                        </div>
                      </div>
                    </div>
                  </div>
                </form>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
