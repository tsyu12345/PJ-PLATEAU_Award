"""_summary_
Unity側から送られてくるデータ型を定義する
"""
from pydantic import BaseModel


class PositionData(BaseModel):
    x: float
    y: float
    z: float

class UserData(BaseModel):
    user_id: str
    pos: PositionData


