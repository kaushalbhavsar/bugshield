from pathlib import Path

import pytest

from app.agent import FileTailer, StateStore
from app.parsers.detect import split_complete_records
from tests.conftest import read_fixture


class Recorder:
    def __init__(self, fail_times: int = 0) -> None:
        self.fail_times = fail_times
        self.calls: list[str] = []

    def __call__(self, payload: str) -> dict:
        if self.fail_times > 0:
            self.fail_times -= 1
            raise RuntimeError("temporary failure")
        self.calls.append(payload)
        return {"accepted": 1}


def test_agent_retries_without_losing_position(tmp_path: Path):
    log = tmp_path / "audit.log"
    log.write_text(read_fixture("native_blocked.audit"), encoding="utf-8")
    store = StateStore(tmp_path / "state.json")
    sender = Recorder(fail_times=2)
    tailer = FileTailer(log, store, sender, max_attempts=4, base_delay=0.01)
    assert tailer.tick() == 1
    assert len(sender.calls) == 1
    assert store.get(str(log)).offset == log.stat().st_size


def test_agent_does_not_ack_partial_record(tmp_path: Path):
    log = tmp_path / "audit.log"
    log.write_text(read_fixture("native_partial.audit"), encoding="utf-8")
    store = StateStore(tmp_path / "state.json")
    sender = Recorder()
    tailer = FileTailer(log, store, sender, base_delay=0.01)
    assert tailer.tick() == 0
    assert sender.calls == []
    assert store.get(str(log)).offset == 0


def test_agent_handles_rotation_and_restart(tmp_path: Path):
    log = tmp_path / "audit.log"
    first = read_fixture("native_blocked.audit")
    log.write_text(first, encoding="utf-8")
    store = StateStore(tmp_path / "state.json")
    sender = Recorder()
    FileTailer(log, store, sender, base_delay=0.01).tick()
    assert len(sender.calls) == 1

    log.unlink()
    second = read_fixture("native_detection_only.audit")
    log.write_text(second, encoding="utf-8")
    FileTailer(log, store, sender, base_delay=0.01).tick()
    assert len(sender.calls) == 2
    assert "native-detect-005" in sender.calls[1]


def test_retry_exhaustion_does_not_advance(tmp_path: Path):
    log = tmp_path / "audit.log"
    log.write_text(read_fixture("native_blocked.audit"), encoding="utf-8")
    store = StateStore(tmp_path / "state.json")
    sender = Recorder(fail_times=5)
    tailer = FileTailer(log, store, sender, max_attempts=2, base_delay=0.01)
    with pytest.raises(RuntimeError):
        tailer.tick()
    assert store.get(str(log)).offset == 0


def test_split_helper_used_by_agent():
    records, leftover = split_complete_records(read_fixture("native_blocked.audit") + "--xx-A--\n", "native")
    assert len(records) == 1
    assert leftover.startswith("--xx-A--")
