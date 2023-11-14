import uvicorn
from fastapi import FastAPI, WebSocket, WebSocketDisconnect, Query
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel

from modules.user_data_store import UserDataStore
from Interfaces.activity_interface import ActivityData

app = FastAPI()

#TODO:以下の設定項目を外部ファイルから編集できる様にする
origins = [
    "http://localhost",
    "http://localhost:8080",
    "http://127.0.0.1:8000",
]

app.add_middleware(
    CORSMiddleware,
    allow_origins=origins,
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)
#####


store = UserDataStore()

@app.websocket("/strength/post/{user_id}")
async def receive_user_data(websocket: WebSocket, user_id: str, data: ActivityData):
    """ユーザーデバイス入力を受け取る"""
    await websocket.accept()
    print(f'accept websocket : {websocket}') 
    store.initialize(user_id)

    try:
        while True:
            data: ActivityData = await websocket.receive_json()
            print(f"receive data : {data}")
            store.renew(data)
            print(f"store data : {store.get_user_pos(user_id)}")

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
    uvicorn.run(app, host="0.0.0.0", port=8000)