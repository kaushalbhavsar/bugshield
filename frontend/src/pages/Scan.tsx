import { useState } from "react";

export function ScanPage() {
  const [notice, setNotice] = useState("");

  function explain() {
    setNotice(
      "Vulnerability scanning is not part of this open-source successor. The original schedule and credential dialogs are shown for layout only and do not collect or store scan credentials.",
    );
  }

  return (
    <div>
      <button id="btnSchedule" type="button" className="btn btn-m btn-primary" onClick={explain}>
        <i className="ti-calendar" /> Schedule a new scan
      </button>{" "}
      <button id="btnEditCred" type="button" className="btn btn-m btn-primary" onClick={explain}>
        <i className="ti-calendar" /> Edit Credentials
      </button>
      {notice && (
        <div className="card" style={{ marginTop: 20 }}>
          <div className="content">
            <h4 className="title">Vulnerability Scan</h4>
            <p>{notice}</p>
            <p className="muted">
              This instance collects ModSecurity audit logs. It does not run remote scans or store website login credentials.
            </p>
          </div>
        </div>
      )}
      <div id="PartialViewForScan" />
    </div>
  );
}
