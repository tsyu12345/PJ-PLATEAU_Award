from pydantic import BaseModel, HttpUrl
from typing import List
import json

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
        with open(file_path, 'r') as file:
            config = json.load(file)
            return cls(**config)
    
    
    @classmethod
    def edit(cls, file_path: str, property_name:str, value: str):
        with open(file_path, 'r') as file:
            config = json.load(file)
            #config[property_name] = value
        with open(file_path, 'w') as file:
            #.を考慮して辿る
            target = config
            for key in property_name.split("."):
                target = target[key]
            target = value
            json.dump(config, file, indent=4)
            
        
