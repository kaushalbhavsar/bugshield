from __future__ import annotations

from app.parsers.models import ParsedEvent

DISRUPTIVE_HINTS = (
    "intercepted",
    "deny",
    "drop",
    "block",
    "redirect",
    "blocked",
)
DETECTION_ONLY = {"detectiononly", "detection_only", "detection-only", "disabled"}


def classify_outcome(event: ParsedEvent) -> tuple[str, str]:
    """Return (outcome, normalized_action) using only recorded evidence.

    A confirmed disruptive action requires both a disruptive indicator and an
    engine mode that is not DetectionOnly. HTTP 403 alone is not enough.
    """
    action = (event.action or "").strip()
    action_l = action.lower()
    engine = (event.engine_mode or "unknown").strip().lower().replace(" ", "")
    if engine in {"enabled", "on"}:
        engine = "enabled"
    elif engine in DETECTION_ONLY:
        engine = "detection_only"
    else:
        engine = engine or "unknown"

    disruptive_evidence = any(hint in action_l for hint in DISRUPTIVE_HINTS)
    if not disruptive_evidence:
        for rule in event.rules:
            raw = f"{rule.action} {rule.raw}".lower()
            if any(hint in raw for hint in ("deny", "drop", "block", "redirect")):
                disruptive_evidence = True
                if not action:
                    action = rule.action or "disruptive-rule"
                break

    if engine == "detection_only":
        return "detected", action or "detect"
    if disruptive_evidence and engine == "enabled":
        return "disrupted", action or "intercepted"
    if disruptive_evidence and engine == "unknown":
        # Keep this conservative: evidence of a deny/drop exists but the
        # engine mode was not recorded, so we still mark it disrupted only
        # when the action text itself says the request was intercepted.
        if "intercepted" in action_l or action_l in {"blocked", "deny", "drop"}:
            return "disrupted", action
        return "detected", action or "unknown"
    if event.rules:
        return "detected", action or "detect"
    return "observed", action or "unknown"
