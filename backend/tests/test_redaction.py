from app.redaction import REDACTED, redact_event_payload


def test_headers_query_and_source_are_redacted():
    headers, uri, source = redact_event_payload(
        {
            "Host": "shop.example.test",
            "Cookie": "session=abc",
            "Authorization": "Bearer secret",
            "X-Custom-Token": "keep-or-not",
        },
        "/login?user=ada&password=hunter2",
        '{"request":{"uri":"/login?password=hunter2","headers":{"Authorization":"Bearer secret","Cookie":"x"}}}',
        extra_headers=["X-Custom-Token"],
        extra_fields=["password"],
    )
    assert headers["Cookie"] == REDACTED
    assert headers["Authorization"] == REDACTED
    assert headers["X-Custom-Token"] == REDACTED
    assert headers["Host"] == "shop.example.test"
    assert "password=[REDACTED]" in uri
    assert "user=ada" in uri
    assert "Bearer secret" not in source
    assert "hunter2" not in source
    assert REDACTED in source
