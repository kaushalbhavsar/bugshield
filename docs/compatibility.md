# Parser compatibility

The parsers use structured scanning (JSON objects, named regular-expression
groups, and section boundaries). They do not slice records with hardcoded
offsets.

## Supported

| Format | How it is recognized | What is extracted |
| --- | --- | --- |
| ModSecurity 3 JSON | `transaction.unique_id` / `client_ip` / structured `messages` | Request, producer, engine mode, rule IDs, severities, tags |
| ModSecurity 2 JSON | `transaction` + `audit_data.messages` as strings | Request line, engine mode, action, `[id]`, `[msg]`, `[severity]` |
| Native serial | `--boundary-A--` … `--boundary-Z--` | Section A header, B request, F status, H trailer, K matched rules |

JSON may arrive as a single object, an array, or NDJSON (one object per line).
Native logs may contain multiple entries in one file.

## Incomplete and malformed input

- A native entry without part `Z` is leftover, not an event.
- A truncated JSON object is leftover or reported as an issue.
- Unknown payloads return a useful error and accept nothing.
- A new native `A` before `Z` stops the current entry so it can complete later.

## Limits

- Concurrent audit-log *index* files (`modsec_audit.log` index lines that only
  point at per-transaction files) are not parsed. Point the agent at the
  serial file or at the individual concurrent files.
- Part `C` / request bodies are not interpreted or executed. Bodies may appear
  in the redacted source record; credential-like query fields are redacted.
- Part `K` in ModSecurity 2 is treated as a list of matched rule texts. Full
  `SecRule` source is stored as a message, not compiled.
- JSON pretty-printed across many lines is supported for uploads of a single
  object or array. The tailing agent expects NDJSON or native serial.
- Custom connector JSON that does not match v2 or v3 layouts is rejected
  rather than guessed.
- Rule explanations never add OWASP text that was not in the record.

## Fixtures

Representative samples live in `backend/tests/fixtures/`. Use them as the
compatibility baseline when changing a parser.
