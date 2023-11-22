import json
from ..Interfaces.server_config import ServerConfig, CORSConfig

class AppConfig:
    
    

    def __init__(self, config_file: str):
        self.config_file = config_file
        self.server: ServerConfig
        self.cors: CORSConfig
        self.__read_json()


    def __read_json(self):
        with open(self.config_file, 'r') as file:
            config = json.load(file)
            self.server= ServerConfig(**config["server"])
            self.cors = CORSConfig(**config["cors"])


    def edit(self, property: str, value: str):
        with open(self.config_file, 'r') as file:
            config = json.load(file)
            #.を辿る
            for p in property.split("."):
                config = config[p]
            #値を代入する
            config = value
        with open(self.config_file, 'w') as file:
            json.dump(config, file, indent=4)
    
    