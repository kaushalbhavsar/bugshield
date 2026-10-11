# ModSecurity setup

This guide configures ModSecurity to write audit logs that BugShield can
ingest. It does not change WAF rules and does not enable blocking for you.

## Recommended logging

Use serial audit logging in JSON or native format:

```
SecAuditEngine RelevantOnly
SecAuditLogRelevantStatus "^(?:5|4(?!04))"
SecAuditLogType Serial
SecAuditLog /var/log/modsec/audit.log
SecAuditLogParts ABFHZ
```

JSON (ModSecurity 2.9.1+ / 3.x with YAJL):

```
SecAuditLogFormat JSON
```

Native (default):

```
SecAuditLogFormat Native
```

`RelevantOnly` still records both detections and disruptive actions. Do not
assume every written event was blocked.

## Engine mode

- `SecRuleEngine On` — matching disruptive actions can be applied.
- `SecRuleEngine DetectionOnly` — matches are logged, not blocked.

BugShield labels DetectionOnly matches as **detected**, not confirmed
disruptive.

## File permissions

The agent needs read access to the audit file and write access to its state
file:

```
/var/log/modsec/audit.log
/var/lib/bugshield-agent/state.json
```

Do not run the agent as root if a dedicated user can read the log.

## Agent

1. Create a site in the UI and an ingestion token. Copy the token immediately.
2. Write `agent.json` from `deploy/agent.example.json`.
3. Run:

```
cd backend
python -m app.agent --config /path/to/agent.json
```

The agent tails the file, handles truncation/rotation of the live path,
retries failed HTTP calls, and only advances its offset after the API
accepts the complete records.

## Upload

Operators can also upload a log file on the Sites page. The same parsers and
redaction rules apply.

## What not to do

- Do not send debug logs (`SecDebugLog`). They are not audit records.
- Do not point BugShield at production application databases.
- Do not paste live cookies or tokens into issues; stored records are already
  redacted, but tickets may still leak leftover fields.
