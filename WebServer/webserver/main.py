import uvicorn
from fastapi import FastAPI, WebSocket, WebSocketDisconnect, Query
from fastapi.middleware.cors import CORSMiddleware

from .modules.user_data_store import UserDataStore
from .modules.device_motion_store import DeviceMotion
from .Interfaces.activity_interface import ActivityData
from .Interfaces.client_data_interface import UserData
from .Interfaces.server_config import AppConfig

app = FastAPI()

# 設定ファイルを読み込む
config = AppConfig.from_json("./server_config.json")
app.add_middleware(
    CORSMiddleware,
    allow_origins=config.cors.origins,
    allow_credentials=config.cors.allow_credentials,
    allow_methods=config.cors.allow_methods,
    allow_headers=config.cors.allow_headers,
)

device = DeviceMotion() #ユーザーごとにデバイスからの入力を保持するインスタンスを生成

@app.websocket("/store/strength/{user_id}")
async def receive_user_data(websocket: WebSocket, user_id: str):
    """【Device input】ユーザーデバイス入力を受け取る"""
    await websocket.accept()
    print(f'[/store/strength/{user_id}] accept : {websocket}')
    try:
        while True:
            input: dict = await websocket.receive_json(mode="text")
            data = ActivityData(**input)
            print(f"receive data : {data}")
            device.renew(data)
            print(f"mortion strength : {data.strength}")
    except WebSocketDisconnect:
        pass


@app.websocket("/get/strength/{user_id}")
async def get_user_input(websocket: WebSocket, user_id: str):
    """【Unity output】ユーザーのモーションデータを返す"""
    await websocket.accept()
    print(f'[/get/strength/{user_id}] accept : {websocket}')
    try:
        while True:
            data = ActivityData(user_id=user_id, strength=device.get_strength(user_id))
            await send_data_to_Unity(websocket, data)
    except WebSocketDisconnect:
        pass


async def send_data_to_Unity(client: WebSocket, data: ActivityData):
    """【Unity output】Unityクライアントにデータを送信する"""
    json_str = data.model_dump_json()
    await client.send_json(json_str, mode="text")


if __name__ == "__main__":
    uvicorn.run(app, host=config.server.host, port=config.server.port)