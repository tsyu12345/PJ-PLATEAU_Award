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


            
        
