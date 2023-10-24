"""サーバーエントリポイント"""
import uvicorn
from fastapi import FastAPI
from pydantic import BaseModel

app = FastAPI()

class ActivityData(BaseModel):
    energy: float
    distance: float
    steps: int

@app.post("/receive_data")
async def receive_data(data: ActivityData):
    # ここでデータを処理する
    print(data.energy, data.distance, data.steps)
    return {"message": "Data received successfully"}


if __name__ == "__main__":
    uvicorn.run(app, host="0.0.0.0", port=8000)