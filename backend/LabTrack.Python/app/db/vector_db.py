import logging
import json
from typing import Any, Dict, List, Optional
from sqlalchemy import text
from sqlalchemy.ext.asyncio import AsyncEngine, create_async_engine
from app.config import settings

logger = logging.getLogger("labtrack.vector_db")
_vector_engine: AsyncEngine | None = None

def init_vector_db() -> dict:
    global _vector_engine
    try:
        _vector_engine = create_async_engine(settings.database_url, echo=False, pool_pre_ping=True)
        logger.info("Vector DB engine created for main database %s @ %s:%s", settings.DB_NAME, settings.DB_HOST, settings.DB_PORT)
        return {"status": "connected", "uri": settings.database_url}
    except Exception as e:
        logger.error(f"Vector DB init failed: {e}")
        return {"status": "disconnected", "error": str(e)}

async def ensure_vector_extension() -> bool:
    global _vector_engine
    if _vector_engine is None:
        init_vector_db()
    if _vector_engine is None:
        return False
    try:
        async with _vector_engine.begin() as conn:
            await conn.execute(text("CREATE EXTENSION IF NOT EXISTS vector"))
            await conn.execute(text("CREATE SCHEMA IF NOT EXISTS labtrack_schema"))
            await conn.execute(text("""
                CREATE TABLE IF NOT EXISTS labtrack_schema.ai_chat_sessions (
                    session_id VARCHAR(128) PRIMARY KEY,
                    user_id VARCHAR(128),
                    role VARCHAR(64) DEFAULT 'ADMIN',
                    history_json TEXT DEFAULT '[]',
                    last_active TIMESTAMPTZ DEFAULT NOW()
                )
            """))
            await conn.execute(text("""
                CREATE TABLE IF NOT EXISTS labtrack_schema.telemetry_logs (
                    id SERIAL PRIMARY KEY,
                    workstation_id VARCHAR(128) NOT NULL,
                    cpu_pct DOUBLE PRECISION DEFAULT 0.0,
                    memory_pct DOUBLE PRECISION DEFAULT 0.0,
                    temperature_celsius DOUBLE PRECISION DEFAULT 45.0,
                    status VARCHAR(64) DEFAULT 'Normal',
                    payload_json TEXT DEFAULT '{}',
                    received_at TIMESTAMPTZ DEFAULT NOW()
                )
            """))
            await conn.execute(text("""
                CREATE TABLE IF NOT EXISTS labtrack_schema.ai_vector_memory (
                    id SERIAL PRIMARY KEY,
                    topic VARCHAR(128),
                    content TEXT NOT NULL,
                    embedding vector(8),
                    created_at TIMESTAMPTZ DEFAULT NOW()
                )
            """))
        return True
    except Exception as e:
        logger.warning(f"Could not initialize vector/persistent tables: {e}")
        return False

def text_to_vector8(content: str) -> str:
    vals = [0.0] * 8
    for idx, ch in enumerate(content.lower()[:256]):
        vals[idx % 8] += (ord(ch) % 97) / 100.0
    norm = sum(v * v for v in vals) ** 0.5 or 1.0
    vec = [round(v / norm, 4) for v in vals]
    return "[" + ",".join(str(x) for x in vec) + "]"

async def store_vector_memory(topic: str, content: str) -> None:
    if _vector_engine is None:
        return
    try:
        vec_str = text_to_vector8(content)
        async with _vector_engine.begin() as conn:
            await conn.execute(
                text("INSERT INTO labtrack_schema.ai_vector_memory (topic, content, embedding) VALUES (:t, :c, CAST(:v AS vector))"),
                {"t": topic[:120], "c": content, "v": vec_str}
            )
    except Exception as e:
        logger.debug(f"Vector memory store note: {e}")

async def search_vector_memory(query: str, limit: int = 3) -> List[Dict[str, Any]]:
    if _vector_engine is None:
        return []
    try:
        vec_str = text_to_vector8(query)
        async with _vector_engine.connect() as conn:
            rows = (await conn.execute(
                text("SELECT topic, content, (embedding <-> CAST(:v AS vector)) AS distance FROM labtrack_schema.ai_vector_memory ORDER BY embedding <-> CAST(:v AS vector) LIMIT :lim"),
                {"v": vec_str, "lim": limit}
            )).fetchall()
            return [{"topic": r[0], "content": r[1], "distance": float(r[2] or 0.0)} for r in rows]
    except Exception:
        return []

async def check_vector_db_health() -> dict:
    if _vector_engine is None:
        init_vector_db()
    if _vector_engine is None:
        return {"status": "disconnected", "pgvector_version": "not_installed"}
    try:
        await ensure_vector_extension()
        async with _vector_engine.connect() as conn:
            row = (await conn.execute(text("SELECT version(), current_database()"))).fetchone()
            vec_row = (await conn.execute(text("SELECT extversion FROM pg_extension WHERE extname = 'vector'"))).fetchone()
            return {
                "status": "connected",
                "database": row[1] if row else None,
                "server_version": (row[0] or "").split(",")[0] if row else None,
                "pgvector_version": vec_row[0] if vec_row else "not_installed",
            }
    except Exception as e:
        logger.error(f"Vector DB health check failed: {e}")
        return {"status": "disconnected", "pgvector_version": "not_installed", "error": str(e)}

def get_vector_engine() -> AsyncEngine | None:
    return _vector_engine
