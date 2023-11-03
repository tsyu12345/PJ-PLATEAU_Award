import uvicorn
from fastapi import FastAPI, WebSocket, WebSocketDisconnect, Query
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel
import enum
import random
import json 

app = FastAPI()
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

class ClientType(enum.Enum):
    UNITY = "unity"
    WatchOS = "watchos"

class ActivityData(BaseModel):
    user_id: str
    steps: int
class ClientData(BaseModel):
    user_id: str
    client_type: ClientType

class UserDataBase:

    __DATA_BASE: dict[str, ActivityData]

    def __init__(self):
        self.__DATA_BASE = {}

    
    def renew(self, data: ActivityData):
        self.__DATA_BASE[data.user_id] = ActivityData(
            user_id=data.user_id,
            steps=data.steps
        )

    def get_user_data(self, userid: str):
        try:
            return self.__DATA_BASE[userid]
        except KeyError:
            return None
        
data_base = UserDataBase()
unity_clients: dict[str, WebSocket] = {}
watchos_clients: dict[str, WebSocket] = {}

@app.websocket("/ws_steps/{user_id}-{client_type}")
async def receive_user_steps(websocket: WebSocket, user_id: str, client_type: str):
    """ユーザーの歩数を受け取る"""
    await websocket.accept()
    
    
    if client_type == ClientType.UNITY.value:
        unity_clients[user_id] = websocket
    elif client_type == ClientType.WatchOS.value:
        watchos_clients[user_id] = websocket
    
    print(f"connect: {user_id} of {client_type}")
    try:
        while True:
            if(client_type == ClientType.WatchOS.value):
                data = await websocket.receive_json()
                data = ActivityData(**data)
                print(f"{data.user_id} now steps : {data.steps}")
                data_base.renew(data)
                await send_data_to_unity_client(user_id, data_base.get_user_data(user_id))
            elif(client_type == ClientType.UNITY.value):
                data = await websocket.receive_json()
                print(f"unity client data : {data}")
                
    except WebSocketDisconnect:
        pass

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



if __name__ == "__main__":
    uvicorn.run(app, host="0.0.0.0", port=8000)