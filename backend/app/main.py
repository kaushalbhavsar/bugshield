from contextlib import asynccontextmanager

from fastapi import FastAPI, Request
from fastapi.middleware.cors import CORSMiddleware
from fastapi.responses import JSONResponse

from app.config import get_settings
from app.migrate import apply_migrations
from app.routers import auth, dashboard, events, health, ingest, setup, sites
from app.routers import settings as settings_router


@asynccontextmanager
async def lifespan(_: FastAPI):
    apply_migrations()
    yield


config = get_settings()
app = FastAPI(
    title=config.app_name,
    version=config.app_version,
    description=(
        "Self-hosted dashboard for ModSecurity audit logs. "
        "ModSecurity performs detection and blocking; this application collects, "
        "searches, and explains recorded events."
    ),
    lifespan=lifespan,
)
app.add_middleware(
    CORSMiddleware,
    allow_origins=config.cors_origin_list,
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)
app.include_router(health.router)
app.include_router(setup.router)
app.include_router(auth.router)
app.include_router(sites.router)
app.include_router(ingest.router)
app.include_router(events.router)
app.include_router(dashboard.router)
app.include_router(settings_router.router)


@app.exception_handler(ValueError)
async def value_error_handler(_: Request, exc: ValueError) -> JSONResponse:
    return JSONResponse(status_code=400, content={"detail": str(exc)})
