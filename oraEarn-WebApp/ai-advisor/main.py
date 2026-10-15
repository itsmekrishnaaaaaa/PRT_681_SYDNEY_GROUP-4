"""
OraEarn Aura — Python microservice (FastAPI).

Called only by the .NET API. Do not expose this service to the public internet.
"""
import os

from fastapi import FastAPI, Header, HTTPException
from advisor import chat
from models import ChatRequest, ChatResponse

IS_PRODUCTION = os.getenv("AURA_ENV", "development").lower() == "production"
AURA_API_KEY = os.getenv("AURA_API_KEY", "")

app = FastAPI(
    title="OraEarn Aura",
    description="Aura — OraEarn's Python AI for savings, visa hours, and work limits.",
    version="1.0.0",
    docs_url=None if IS_PRODUCTION else "/docs",
    redoc_url=None if IS_PRODUCTION else "/redoc",
    openapi_url=None if IS_PRODUCTION else "/openapi.json",
)


@app.get("/health")
def health() -> dict[str, str]:
    """Liveness check for local runs and deployment probes; touches nothing else."""
    return {"status": "ok", "engine": "python"}


@app.post("/api/advisor/chat", response_model=ChatResponse)
def advisor_chat(
    body: ChatRequest,
    x_aura_key: str | None = Header(default=None, alias="X-Aura-Key"),
) -> ChatResponse:
    """Answer one chat message using only the context the .NET API sends with it.

    Aura never reads the database, so the reply is only as current as that context. When AURA_API_KEY
    is set, the X-Aura-Key header must match; when it is empty (the local default) the check is skipped.
    """
    if AURA_API_KEY and x_aura_key != AURA_API_KEY:
        raise HTTPException(status_code=401, detail="Invalid advisor key.")
    return chat(body.message.strip(), body.context)
