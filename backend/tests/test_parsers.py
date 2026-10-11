from app.outcomes import classify_outcome
from app.parsers import parse_payload, split_complete_records
from tests.conftest import read_fixture


def test_json_v3_blocked_and_redactable_fields():
    result = parse_payload(read_fixture("json_v3_blocked.json"))
    assert result.detected_format == "json"
    assert len(result.events) == 1
    event = result.events[0]
    assert event.unique_id == "json-v3-blocked-001"
    assert event.method == "GET"
    assert "password=supersecret" in event.uri
    assert event.request_headers["Authorization"] == "Bearer secret-token"
    assert event.rules[0].rule_id == "942100"
    assert event.severity == "critical"
    outcome, _ = classify_outcome(event)
    assert outcome == "disrupted"


def test_json_v3_detection_only_is_not_a_block():
    result = parse_payload(read_fixture("json_v3_detection_only.json"))
    event = result.events[0]
    outcome, _ = classify_outcome(event)
    assert outcome == "detected"
    assert event.rules[0].rule_id == "941100"


def test_json_v2_message_fields_are_parsed():
    result = parse_payload(read_fixture("json_v2_blocked.json"))
    event = result.events[0]
    assert event.log_format == "json-v2"
    assert event.rules[0].rule_id == "942190"
    assert event.rules[0].message.startswith("SQL Injection")
    assert classify_outcome(event)[0] == "disrupted"


def test_native_blocked_and_detection_only():
    blocked = parse_payload(read_fixture("native_blocked.log")).events[0]
    detected = parse_payload(read_fixture("native_detection_only.log")).events[0]
    assert blocked.unique_id == "native-blocked-004"
    assert blocked.rules[0].rule_id == "930120"
    assert blocked.host == "blog.example.test"
    assert classify_outcome(blocked)[0] == "disrupted"
    assert classify_outcome(detected)[0] == "detected"


def test_partial_native_entry_is_not_emitted():
    result = parse_payload(read_fixture("native_partial.log"))
    assert result.events == []
    assert result.incomplete_tail is True


def test_malformed_json_is_reported():
    result = parse_payload(read_fixture("malformed.json"))
    assert result.events == []
    assert result.incomplete_tail is True or result.issues


def test_complete_then_partial_native_split():
    complete = read_fixture("native_blocked.log")
    partial = read_fixture("native_partial.log")
    records, leftover = split_complete_records(complete + partial, "native")
    assert len(records) == 1
    assert "native-partial-006" in leftover
    assert "native-blocked-004" not in leftover


def test_observed_event_without_rules():
    event = parse_payload(read_fixture("json_v3_observed.json")).events[0]
    assert classify_outcome(event)[0] == "observed"
