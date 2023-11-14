from pydantic import BaseModel, HttpUrl
from typing import List

class ServerConfig(BaseModel):
    host: str
    port: int

class CORSConfig(BaseModel):
    origins: List[HttpUrl]
    allow_credentials: bool
    allow_methods: List[str]
    allow_headers: List[str]

class AppConfig(BaseModel):
    server: ServerConfig
    cors: CORSConfig

    @classmethod
    def from_json(cls, file_path: str):
        import json
        with open(file_path, 'r') as file:
            return cls(**json.load(file))
