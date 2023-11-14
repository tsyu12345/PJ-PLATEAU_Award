from __future__ import annotations
from typing import Final as const

from ..Interfaces.client_data_interface import UserData

class UserDataStore:
    """接続中の各ユーザーのマップ上における位置情報を保持するクラス"""

    __DATA_BASE: dict[str, UserData] = {} #インメモリで保持する,

    def __init__(self):
        pass
    
    
    def initialize(self, user_id: str):
        """指定ユーザーの初期化"""
        self.__DATA_BASE[user_id] = UserData()


    def renew(self, data: UserData):
        """指定ユーザーの位置情報を更新する"""
        self.__DATA_BASE[data.user_id] = UserData(
            user_id=data.user_id,
            pos=data.pos
        )
    
    def get_user_pos(self, user_id: str):
        """指定ユーザーの位置情報を取得する"""
        try:
            return self.__DATA_BASE[user_id]
        except KeyError:
            return None
    
    def __save(self, data: UserData):
        """todo: 指定ユーザーの位置情報をローカルに保存する"""
        pass
        