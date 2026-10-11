export function SupportPage() {
  return (
    <div className="card">
      <div className="header">
        <h4 className="title">Help & Support</h4>
        <p className="category">Self-hosted ModSecurity audit log console</p>
      </div>
      <div className="content">
        <p>
          BugShield collects audit records that ModSecurity already wrote. It searches those records
          and shows what the log recorded. It does not claim that every event was blocked, and it
          does not change WAF rules.
        </p>
        <h4 className="title" style={{ marginTop: 24 }}>Where to look</h4>
        <table className="table">
          <tbody>
            <tr>
              <td>Dashboard</td>
              <td>Event counts, confirmed disruptive actions, trends, top rules / IPs / URLs</td>
            </tr>
            <tr>
              <td>Blocked Attacks</td>
              <td>Search and open a redacted event record</td>
            </tr>
            <tr>
              <td>Reports</td>
              <td>Tabular summary of the same aggregates</td>
            </tr>
            <tr>
              <td>Preferences</td>
              <td>Sites, ingestion tokens, uploads, retention, redaction, and the demo dataset</td>
            </tr>
            <tr>
              <td>Change Password</td>
              <td>User menu in the top-right</td>
            </tr>
          </tbody>
        </table>
        <p className="muted">
          Documentation lives in this repository: README, docs/architecture.md, docs/api.md, and
          docs/modsecurity-setup.md. No support mailbox is bundled with the self-hosted build.
        </p>
      </div>
    </div>
  );
}
