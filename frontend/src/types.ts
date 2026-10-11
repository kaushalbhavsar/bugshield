export type User = {
  id: string;
  email: string;
  display_name: string;
  timezone: string;
};

export type Site = {
  id: string;
  name: string;
  slug: string;
  hostname: string;
  notes: string;
  created_at: string;
};

export type Token = {
  id: string;
  label: string;
  token_prefix: string;
  created_at: string;
  last_used_at: string | null;
  revoked_at: string | null;
  token?: string;
  warning?: string;
};

export type EventSummary = {
  id: string;
  site_id: string;
  occurred_at: string;
  source_ip: string | null;
  method: string;
  uri: string;
  host: string;
  status_code: number | null;
  action: string;
  outcome: string;
  severity: string;
  rule_ids: string[];
  is_demo: boolean;
};

export type EventDetail = EventSummary & {
  unique_id: string;
  dest_ip: string | null;
  dest_port: number | null;
  source_port: number | null;
  engine_mode: string;
  producer: string;
  log_format: string;
  ingested_at: string;
  request_headers_redacted: Record<string, string>;
  messages: unknown[];
  rules: Array<{
    rule_id: string;
    message: string;
    action: string;
    severity: string;
    tags: string[];
    file: string;
    line_number: number | null;
  }>;
  source_record_redacted: string;
  outcome_note: string;
};

export type Dashboard = {
  event_count: number;
  disrupted_count: number;
  detected_count: number;
  observed_count: number;
  demo_count: number;
  trends: Array<{ bucket: string; events: number; disrupted: number }>;
  top_rules: Array<{ name: string; count: number }>;
  top_source_ips: Array<{ name: string; count: number }>;
  top_urls: Array<{ name: string; count: number }>;
};

export type Settings = {
  retention_days: number;
  extra_redact_headers: string[];
  extra_redact_fields: string[];
};
