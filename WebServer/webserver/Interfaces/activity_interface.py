"""_summary_
デバイス側から送られてくるモーションデータ型を定義する
"""

from pydantic import BaseModel

class ActivityData(BaseModel):
    user_id: str
    strength: float