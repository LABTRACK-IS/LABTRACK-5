from fastapi import APIRouter
from pydantic import BaseModel, Field
from typing import Optional, Dict, Any
from datetime import datetime
from .ai_chat import router as ai_chat_router, _build_response, _get_session
from .asset import router as asset_router
from .damage import router as damage_router
from .repair import router as repair_router
from .telemetry import router as telemetry_router
from .recommendations import router as recommendations_router
from .health import router as health_router

api_router = APIRouter()

class GeneratePromptRequest(BaseModel):
    Prompt: Optional[str] = Field(None, alias="prompt")
    SystemContext: Optional[str] = Field("", alias="systemContext")
    Parameters: Optional[Dict[str, Any]] = Field(default_factory=dict, alias="parameters")
    model_config = {"populate_by_name": True, "extra": "allow"}

@api_router.post("/generate", tags=["C# Gateway AI Bridge"])
async def generate_bridge(req: GeneratePromptRequest):
    prompt_text = req.Prompt or ""
    session = _get_session("csharp-bridge")
    text, sugg, acts = await _build_response(prompt_text, {"role": "TECHNICIAN"}, session)
    return {
        "success": True,
        "content": text,
        "errorMessage": None,
        "metadata": {
            "model": "gemini-2.5-pro",
            "suggestions": sugg,
            "actions": acts,
            "generated_at": datetime.utcnow().isoformat()
        }
    }

api_router.include_router(ai_chat_router, prefix="/ai", tags=["AI Assistance (Gemini 2.5 Pro)"])
api_router.include_router(asset_router, prefix="/assets", tags=["Laboratory Assets"])
api_router.include_router(damage_router, prefix="/damage", tags=["Damage Reports"])
api_router.include_router(repair_router, prefix="/repairs", tags=["Repair Tickets"])
api_router.include_router(telemetry_router, prefix="/telemetry", tags=["PC Telemetry"])
api_router.include_router(recommendations_router, prefix="/recommendations", tags=["Smart Recommendations"])
api_router.include_router(health_router, prefix="/health", tags=["Detailed System Health"])

__all__ = ["api_router"]
