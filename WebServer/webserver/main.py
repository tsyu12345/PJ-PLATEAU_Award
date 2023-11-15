import uvicorn
from fastapi import FastAPI, WebSocket, WebSocketDisconnect, Query
from fastapi.middleware.cors import CORSMiddleware

from .modules.user_data_store import UserDataStore
from .modules.device_motion_store import DeviceMortion
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


store = UserDataStore() #全ユーザーの位置情報を保持するインメモリデータベース

@app.websocket("/store/strength/{user_id}")
async def receive_user_data(websocket: WebSocket, user_id: str):
    """【Device input】ユーザーデバイス入力を受け取る"""
    await websocket.accept()
    print(f'[/store/strength/{user_id}] accept : {websocket}')
    device = DeviceMortion(user_id) #ユーザーごとにデバイスからの入力を保持するインスタンスを生成
    try:
        while True:
            input: dict = await websocket.receive_json(mode="text")
            data = ActivityData(**input)
            print(f"receive data : {data}")
            device.renew(data.strength)
            print(f"mortion strength : {device.get_strength()}")
    except WebSocketDisconnect:
        pass


@app.websocket("/store/position/{user_id}")
async def receive_client_data(websocket: WebSocket, user_id: str):
    """NOTE:【開発中】Unityクライアントからのデータを受け取る"""
    await websocket.accept()
    print(f'[/store/position/{user_id}] accept : {websocket}') 
    store.initialize(user_id)
    try:
        while True:
            data: UserData = await websocket.receive_json()
            print(f"receive data : {data}")
            store.renew(data)
            print(f"mortion strength : {store.get_user_pos(user_id)}")
    except WebSocketDisconnect:
        pass



"""
async def send_data_to_unity_client(client_id: int, data: ActivityData):
    client = unity_clients.get(client_id)
    #print(f'client : {client}')
    if client:
        ##NOTE:テスト用に乱数を送信
        #data = ActivityData(user_id=client_id, steps=random.randint(0, 1))
        print(f'send data to unity client : {client_id}')
        #JSON文字列に変換して送信
        data = data.model_dump_json()
        print(f'data : {data}')
        await client.send_json(data, mode="text")
"""


if __name__ == "__main__":
    uvicorn.run(app, host=config.server.host, port=config.server.port)