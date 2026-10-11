from __future__ import annotations

import time
from collections import defaultdict, deque
from threading import Lock

from fastapi import HTTPException, Request, status

from app.config import get_settings


class SlidingWindowLimiter:
    def __init__(self) -> None:
        self._hits: dict[str, deque[float]] = defaultdict(deque)
        self._lock = Lock()

    def check(self, key: str, limit: int, window_seconds: int = 60) -> None:
        now = time.time()
        cutoff = now - window_seconds
        with self._lock:
            bucket = self._hits[key]
            while bucket and bucket[0] < cutoff:
                bucket.popleft()
            if len(bucket) >= limit:
                raise HTTPException(
                    status_code=status.HTTP_429_TOO_MANY_REQUESTS,
                    detail="ingestion rate limit exceeded; retry later",
                )
            bucket.append(now)


limiter = SlidingWindowLimiter()


def ingest_rate_limit(request: Request, token_key: str) -> None:
    limit = get_settings().ingest_rate_limit_per_minute
    client = request.client.host if request.client else "unknown"
    limiter.check(f"{client}:{token_key}", limit)
