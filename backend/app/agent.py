"""File-tailing ingestion agent.

The agent reads configured audit files, sends only complete records, persists
its byte offset after the server accepts the payload, and retries without
advancing the offset when the API is unavailable.
"""

from __future__ import annotations

import argparse
import json
import time
from dataclasses import dataclass
from pathlib import Path
from typing import Callable

import httpx

from app.parsers.detect import split_complete_records


@dataclass
class FileState:
    path: str
    inode: int = 0
    offset: int = 0


class StateStore:
    def __init__(self, path: Path) -> None:
        self.path = path
        self._files: dict[str, FileState] = {}
        self.load()

    def load(self) -> None:
        if not self.path.exists():
            return
        payload = json.loads(self.path.read_text(encoding="utf-8"))
        for item in payload.get("files", []):
            self._files[item["path"]] = FileState(
                path=item["path"],
                inode=int(item.get("inode") or 0),
                offset=int(item.get("offset") or 0),
            )

    def save(self) -> None:
        self.path.parent.mkdir(parents=True, exist_ok=True)
        payload = {
            "files": [
                {"path": item.path, "inode": item.inode, "offset": item.offset}
                for item in self._files.values()
            ]
        }
        tmp = self.path.with_suffix(self.path.suffix + ".tmp")
        tmp.write_text(json.dumps(payload, indent=2), encoding="utf-8")
        tmp.replace(self.path)

    def get(self, path: str) -> FileState:
        return self._files.setdefault(path, FileState(path=path))


class IngestSender:
    def __init__(self, api_url: str, token: str, timeout: float = 15.0) -> None:
        self.api_url = api_url.rstrip("/") + "/api/ingest"
        self.token = token
        self.timeout = timeout

    def send(self, payload: str) -> dict:
        response = httpx.post(
            self.api_url,
            content=payload.encode("utf-8"),
            headers={
                "Authorization": f"Bearer {self.token}",
                "Content-Type": "text/plain",
            },
            timeout=self.timeout,
        )
        response.raise_for_status()
        return response.json()


class FileTailer:
    def __init__(
        self,
        path: Path,
        store: StateStore,
        sender: Callable[[str], dict],
        max_attempts: int = 5,
        base_delay: float = 0.2,
    ) -> None:
        self.path = path
        self.store = store
        self.sender = sender
        self.max_attempts = max_attempts
        self.base_delay = base_delay

    def tick(self) -> int:
        if not self.path.exists():
            return 0
        stat = self.path.stat()
        state = self.store.get(str(self.path))
        if state.inode and state.inode != stat.st_ino:
            # Rotation: finish the previous inode if the file still exists under
            # a rotated name is out of scope; reset when the live path is new.
            state.inode = stat.st_ino
            state.offset = 0
        if stat.st_size < state.offset:
            state.offset = 0
        state.inode = stat.st_ino
        with self.path.open("rb") as handle:
            handle.seek(state.offset)
            chunk = handle.read()
        if not chunk:
            self.store.save()
            return 0
        text = chunk.decode("utf-8", errors="replace")
        records, leftover = split_complete_records(text)
        if not records:
            return 0
        payload = "".join(records)
        self._send_with_retry(payload)
        consumed = len(text) - len(leftover)
        state.offset += consumed
        self.store.save()
        return len(records)

    def _send_with_retry(self, payload: str) -> dict:
        last_error: Exception | None = None
        for attempt in range(self.max_attempts):
            try:
                return self.sender(payload)
            except Exception as exc:  # noqa: BLE001 - retry any transport/API failure
                last_error = exc
                time.sleep(self.base_delay * (2**attempt))
        raise RuntimeError(f"ingest failed after retries: {last_error}") from last_error


def run_agent(config_path: Path, once: bool = False, interval: float = 2.0) -> None:
    config = json.loads(config_path.read_text(encoding="utf-8"))
    store = StateStore(Path(config.get("state_path") or "agent-state.json"))
    sender = IngestSender(config["api_url"], config["site_token"])
    tailers = [
        FileTailer(Path(path), store, sender.send, int(config.get("max_attempts") or 5))
        for path in config.get("paths") or []
    ]
    while True:
        for tailer in tailers:
            tailer.tick()
        if once:
            return
        time.sleep(interval)


def main() -> None:
    parser = argparse.ArgumentParser(description="BugShield ModSecurity audit log agent")
    parser.add_argument("--config", required=True, help="Path to agent JSON configuration")
    parser.add_argument("--once", action="store_true", help="Process available bytes and exit")
    parser.add_argument("--interval", type=float, default=2.0)
    args = parser.parse_args()
    run_agent(Path(args.config), once=args.once, interval=args.interval)


if __name__ == "__main__":
    main()
