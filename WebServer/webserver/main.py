from __future__ import annotations
from typing import Final as const
import uvicorn
from fastapi import FastAPI
from pydantic import BaseModel

app = FastAPI()

class ActivityData(BaseModel):
    userid: str
    steps: int

class StepDataBase:
    #NOTE:ユーザーの歩数を統括管理するクラス。サンプルとして作成、後ほどDB化するか？
    __DATABASE: dict[str, ActivityData]

    def __init__(self):
        self.__DATABASE = {}
    
    def renew(self, data: ActivityData):
        self.__DATABASE[data.userid] = data

    def get_step(self, userid: str) -> int:
        try:
            return self.__DATABASE[userid].steps
        except KeyError:
            raise KeyError(f'userid: {userid} is not found in database')
        

data_base: const = StepDataBase()

@app.post("/step_data")
async def receive_data(data: ActivityData):
    
    print(data)
    data_base.renew(data)
    return {"message": "Data received successfully"}

@app.get("/step_data")
async def get_activity_data(userid: str):

    print(f'fetching step data of {userid}')

    try:
        steps: int = data_base.get_step(userid)
    except KeyError:
        return {
            "userid": userid,
            "steps": None,
            "error": "userid is not found in database"
        }
    else:
        return {
            "userid": userid,
            "steps": steps,
            "error": None
        }


if __name__ == "__main__":
    uvicorn.run(app, host="0.0.0.0", port=8000)