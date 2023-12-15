from typing import Callable, Any
import uvicorn
from fastapi import FastAPI, WebSocket, WebSocketDisconnect, Query
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel
from contextlib import asynccontextmanager
import socket
import json
import uuid


from .modules.device_motion_store import DeviceMotion
from .modules.ServerApp import AppConfig
from .Interfaces.activity_interface import ActivityData
from .Interfaces.client_types import ClientType
from .Interfaces.utils import PrintColor


class RegisteredData(BaseModel):
    nickname: str
    uuid: str


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
    


device = DeviceMotion() #ユーザーごとにデバイスからの入力を保持するインスタンスを生成
app = FastAPI(lifespan=lifespan) #FastAPIインスタンスを生成
config = AppConfig("server_config.json") #設定ファイルを読み込む

async def deploy_publicIP() -> None:
    """【Server output】サーバーの公開IPを設定ファイルに書き込む"""
    s = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    try:
        # このアドレスは実際には接続されません
        s.connect(('10.255.255.255', 1))
        IP = s.getsockname()[0]
    except Exception:
        IP = '127.0.0.1'
    finally:
        s.close()

    with open("PLATEAU-run Client/Assets/temp/ipconfig.txt", "w") as f:
        addless = f'{IP}:{config.server.port}'
        info = f"【!Your Server IP address!】{PrintColor.BLUE.value}{IP}:{config.server.port} {PrintColor.RESET.value}"
        print(info)
        f.write(addless)

async def deploy_user_id() -> None:
    """JSONファイルからユーザーIDをUnityに渡す"""
    with open("TestAccount/TESTUSER1.json", "r") as f:
        json_data = json.load(f)
        user_id = json_data["userId"]
        info = f"【!Your User ID!】{PrintColor.BLUE.value}{user_id}{PrintColor.RESET.value}"
        print(info)
        
    #PLATEAU-run Client/Assets/UserConfigにuserId.jsonを作成し書き込む
    #ファイルが存在しない場合は作成される
    with open(f"PLATEAU-run Client/Assets/Config/{user_id}.json", "w") as f:
        json_data = {
            "userName": None, #TODO: ユーザー名表示対応時に設定する
            "userId": user_id,
        }
        json.dump(json_data, f, indent=4)




app.add_middleware(
    CORSMiddleware,
    allow_origins=config.cors.origins,
    allow_credentials=config.cors.allow_credentials,
    allow_methods=config.cors.allow_methods,
    allow_headers=config.cors.allow_headers,
)


#Endpoints
@app.websocket("/store/strength/{client_type}")
async def receive_user_data(websocket: WebSocket, user_id: str, client_type: str):
    """【Device input】ユーザーデバイス入力を受け取るWebSocket"""
    await websocket.accept()
    print(f'[/store/strength/{user_id}] accept : {websocket}')
    try:
        while True:
            if client_type == ClientType.UNITY.value:
                data = ActivityData(user_id=user_id, strength=device.get_strength(user_id))
                #print(f'sending data to Unity: {data.strength}')
                await send_data_to_Unity(websocket, data)
            else:
                input: dict = await websocket.receive_json(mode="text")
                data = ActivityData(**input)
                device.renew(data)
                #print(f'[device Input {client_type}] {data.strength}')
    except WebSocketDisconnect:
        pass

@app.route("/register/user/{nickname}", methods=["POST"])
async def register_user(nickname: str):
    """【Server input】ユーザー情報を登録する"""
    user_id = str(uuid.uuid4())
    device.register(user_id)
    return {"nickname": nickname, "uuid": user_id}
    


#sub functions
async def send_data_to_Unity(client: WebSocket, data: ActivityData, debug: bool = False):
    """【Unity output】Unityクライアントにデータを送信する"""
    if debug:
        data = ActivityData(user_id="test", strength=1.8)

    json_str = data.model_dump_json()
    await client.send_json(json_str, mode="text")


if __name__ == "__main__":
    uvicorn.run(app, port=config.server.port)