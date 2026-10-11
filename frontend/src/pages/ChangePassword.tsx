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
    <div className="card" style={{ maxWidth: 520 }}>
      <div className="header">
        <h4 className="title">Change Password</h4>
        <p className="category">The new password must be at least 12 characters.</p>
      </div>
      <div className="content">
        <form onSubmit={submit}>
          <div className="form-group">
            <label htmlFor="OldPassword">Current password</label>
            <input className="form-control" id="OldPassword" name="current_password" type="password" required />
          </div>
          <div className="form-group">
            <label htmlFor="NewPassword">New password</label>
            <input className="form-control" id="NewPassword" name="new_password" type="password" minLength={12} required />
          </div>
          <div className="form-group">
            <label htmlFor="ConfirmPassword">Confirm new password</label>
            <input className="form-control" id="ConfirmPassword" name="confirm_password" type="password" minLength={12} required />
          </div>
          {error && <p className="error">{error}</p>}
          {message && <p className="success-text">{message}</p>}
          <button type="submit" className="btn btn-rose">
            Change Password
          </button>
        </form>
      </div>
    </div>
  );
}
