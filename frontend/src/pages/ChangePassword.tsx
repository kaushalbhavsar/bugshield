import { FormEvent, useState } from "react";
import { api } from "../api";

export function ChangePasswordPage() {
  const [error, setError] = useState("");
  const [message, setMessage] = useState("");

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const data = new FormData(event.currentTarget);
    const next = String(data.get("new_password") || "");
    const confirm = String(data.get("confirm_password") || "");
    if (next !== confirm) {
      setError("The password and confirmation password do not match.");
      setMessage("");
      return;
    }
    setError("");
    try {
      await api.changePassword({
        current_password: data.get("current_password"),
        new_password: next,
      });
      setMessage("Password changed.");
      event.currentTarget.reset();
    } catch (err) {
      setError(err instanceof Error ? err.message : "password change failed");
    }
  }

  return (
    <form className="form-horizontal" onSubmit={submit}>
      {error && <div className="text-danger">{error}</div>}
      {message && <div className="success-text">{message}</div>}
      <div className="col-md-8 col-md-offset-2">
        <div className="card card-wizard">
          <div className="content">
            <div className="form-group">
              <label className="col-md-4 control-label" htmlFor="OldPassword">
                Current password
              </label>
              <div className="col-md-6">
                <input className="form-control" id="OldPassword" name="current_password" type="password" required />
              </div>
            </div>
            <div className="form-group">
              <label className="col-md-4 control-label" htmlFor="NewPassword">
                New password
              </label>
              <div className="col-md-6">
                <input className="form-control" id="NewPassword" name="new_password" type="password" minLength={12} required />
              </div>
            </div>
            <div className="form-group">
              <label className="col-md-4 control-label" htmlFor="ConfirmPassword">
                Confirm password
              </label>
              <div className="col-md-6">
                <input className="form-control" id="ConfirmPassword" name="confirm_password" type="password" minLength={12} required />
              </div>
            </div>
            <div className="form-group">
              <div className="col-md-offset-2 col-md-10" style={{ marginLeft: "50%" }}>
                <input type="submit" value="Change password" className="btn btn-default" />
              </div>
            </div>
          </div>
        </div>
      </div>
    </form>
  );
}
