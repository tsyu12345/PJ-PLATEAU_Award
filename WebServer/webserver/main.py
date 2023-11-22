from typing import Callable

import uvicorn
from fastapi import FastAPI, WebSocket, WebSocketDisconnect, Query
from fastapi.middleware.cors import CORSMiddleware
from contextlib import asynccontextmanager
import public_ip as ip
import random

from .modules.device_motion_store import DeviceMotion
from .Interfaces.activity_interface import ActivityData
from .Interfaces.server_config import AppConfig
from .Interfaces.client_types import ClientType


event_handlers: dict[str, list[Callable]] = {}

@asynccontextmanager
async def lifespan(app: FastAPI):
    # Code to execute at startup
    callbacks = event_handlers['startup']
    for callback in callbacks:
        await callback()
    yield
    # Code to execute at shutdown

def addEventListener(event: str, callback: Callable):
    """イベントリスナーを追加する"""
    if event not in event_handlers:
        event_handlers[event] = []
    event_handlers[event].append(callback)
    
#Custom functions

def deploy_publicIP() -> None:
    #デプロイされているアドレスを取得する
    public_ip = ip.get()
    AppConfig.edit("./server_config.json", "server.host", public_ip)

#Add event handlers
addEventListener("startup", deploy_publicIP)

device = DeviceMotion() #ユーザーごとにデバイスからの入力を保持するインスタンスを生成
app = FastAPI(lifespan=lifespan) #FastAPIインスタンスを生成

# 設定ファイルを読み込む
config = AppConfig.from_json("./server_config.json")
app.add_middleware(
    CORSMiddleware,
    allow_origins=config.cors.origins,
    allow_credentials=config.cors.allow_credentials,
    allow_methods=config.cors.allow_methods,
    allow_headers=config.cors.allow_headers,
)


#Endpoints
@app.websocket("/store/strength/{user_id}-{client_type}")
async def receive_user_data(websocket: WebSocket, user_id: str, client_type: str):
    """【Device input】ユーザーデバイス入力を受け取る"""
    await websocket.accept()
    print(f'[/store/strength/{user_id}] accept : {websocket}')
    try:
        while True:
            if client_type == ClientType.UNITY.value:
                data = ActivityData(user_id=user_id, strength=device.get_strength(user_id))
                await send_data_to_Unity(websocket, data, debug=True)
            else:
                input: dict = await websocket.receive_json(mode="text")
                data = ActivityData(**input)
                device.renew(data)
    except WebSocketDisconnect:
        pass

#sub functions
async def send_data_to_Unity(client: WebSocket, data: ActivityData, debug: bool = False):
    """【Unity output】Unityクライアントにデータを送信する"""
    if debug:
        data = ActivityData(user_id="test", strength=random.uniform(0.0, 3.0))
        print(f"send_data_to_Unity: {data.strength}")

    json_str = data.model_dump_json()
    await client.send_json(json_str, mode="text")


if __name__ == "__main__":
    uvicorn.run(app, host=config.server.host, port=config.server.port)