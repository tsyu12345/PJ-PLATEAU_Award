"""サーバーエントリポイント"""
import uvicorn
from fastapi import FastAPI, WebSocket
from pydantic import BaseModel

app = FastAPI()

class ActivityData(BaseModel):
    user_id: str
    steps: int

@app.websocket("/ws_post_step")
async def receive_user_steps(websocket: WebSocket):
    """ユーザーの歩数を受け取る"""
    await websocket.accept()
    while True:
        data = await websocket.receive_json()
        print(data)

if __name__ == "__main__":
    uvicorn.run(app, host="0.0.0.0", port=8000)