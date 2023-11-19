"""_summary_
デバイス側から送られてくるモーションデータ型を定義する
"""
from pydantic import BaseModel
from .client_types import ClientType

class ActivityData(BaseModel):
    user_id: str
    strength: float
    client_type: ClientType